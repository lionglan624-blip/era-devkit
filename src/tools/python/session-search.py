#!/usr/bin/env python3
"""
Claude Code Session JSONL Search Tool

Searches session log files stored by Claude Code for patterns across
tool calls, messages, and results. Run with --help for full usage and examples.

Quick examples:
    python session-search.py "feature-797" --tool Write,Edit
    python session-search.py --session be309 --summary
    python session-search.py --session be309 --timeline
    python session-search.py --session be309 --autopsy
    python session-search.py --session be309 --trace a396 --tool Bash
    python session-search.py --list --after 2026-03-01

Exit codes:
    0 = Success (matches found, or no matches without --strict)
    1 = Error (invalid arguments, file not found, I/O error)
    2 = No matches (only with --strict flag)
"""

import argparse
import collections
import io
import json
import os
import re
import sys
from dataclasses import dataclass, field
from datetime import datetime, timedelta, timezone
from pathlib import Path
from typing import Optional

# Force UTF-8 output on Windows (avoids cp932 UnicodeEncodeError)
if sys.stdout.encoding != "utf-8":
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
if sys.stderr.encoding != "utf-8":
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8", errors="replace")


# -- helpers ------------------------------------------------------------------

def find_ccs_dirs(override: Optional[str] = None) -> list[Path]:
    """Locate CCS project directories containing JSONL session files."""
    if override:
        p = Path(override)
        if not p.is_dir():
            print(f"ERROR: --ccs-dir path not found: {p}", file=sys.stderr)
            sys.exit(1)
        return [p]

    base = Path.home() / ".ccs" / "instances"
    if not base.is_dir():
        print(f"ERROR: CCS base directory not found: {base}", file=sys.stderr)
        sys.exit(1)

    candidates = list(base.glob("*/projects/*/"))
    if not candidates:
        print(f"ERROR: No project directories found under {base}", file=sys.stderr)
        sys.exit(1)

    # Prefer directory whose name matches the current working directory
    cwd_name = Path.cwd().name
    cwd_slug = cwd_name.replace("\\", "-").replace("/", "-").replace(":", "-")
    matched = [c for c in candidates if cwd_slug in c.name or cwd_name in c.name]
    return matched if matched else candidates


def parse_date(value: str) -> datetime:
    """Parse YYYY-MM-DD into an aware UTC datetime (midnight)."""
    try:
        dt = datetime.strptime(value, "%Y-%m-%d")
        return dt.replace(tzinfo=timezone.utc)
    except ValueError:
        raise argparse.ArgumentTypeError(f"Invalid date format '{value}'. Use YYYY-MM-DD.")


def parse_iso(ts: str) -> Optional[datetime]:
    """Parse ISO 8601 timestamp to aware datetime. Returns None on failure."""
    if not ts:
        return None
    try:
        return datetime.fromisoformat(ts.replace("Z", "+00:00"))
    except (ValueError, AttributeError):
        return None


def _to_local(dt: datetime) -> datetime:
    """Convert aware datetime to local time for display."""
    return dt.astimezone() if dt.tzinfo else dt


def format_dt(dt: Optional[datetime]) -> str:
    """Format datetime for display as 'YYYY-MM-DD HH:MM' in local time."""
    if dt is None:
        return "unknown"
    return _to_local(dt).strftime("%Y-%m-%d %H:%M")


def format_time(dt: Optional[datetime]) -> str:
    """Format datetime as HH:MM:SS in local time for timeline."""
    if dt is None:
        return "??:??:??"
    return _to_local(dt).strftime("%H:%M:%S")


# -- match extraction ---------------------------------------------------------

# Module-level display width override (set by --width/--no-truncate flags).
# None means "not set by user, use per-call default".
_display_width: int | None = None


def _truncate(text: str, width: int = 100) -> str:
    """Truncate string to width, appending ellipsis if needed."""
    text = text.replace("\n", " ").replace("\r", "")
    effective = width if _display_width is None else _display_width
    if effective == 0:
        return text
    if len(text) <= effective:
        return text
    return text[:effective - 3] + "..."


def _snippet_around(
    text: str,
    pattern: str,
    radius: int = 50,
    ignore_case: bool = False,
    compiled_re: Optional[re.Pattern] = None,
) -> str:
    """Return a snippet of text centered on the first occurrence of pattern."""
    if _display_width == 0:
        return text.replace("\n", " ").replace("\r", "")
    if _display_width is not None:
        radius = max(radius, _display_width // 2)
    if compiled_re:
        m = compiled_re.search(text)
        if m:
            idx = m.start()
            match_len = m.end() - m.start()
        else:
            return _truncate(text, radius * 2)
    elif pattern:
        search_text = text.lower() if ignore_case else text
        search_pattern = pattern.lower() if ignore_case else pattern
        idx = search_text.find(search_pattern)
        if idx == -1:
            return _truncate(text, radius * 2)
        match_len = len(pattern)
    else:
        return _truncate(text, radius * 2)
    start = max(0, idx - radius)
    end = min(len(text), idx + match_len + radius)
    snippet = text[start:end].replace("\n", " ").replace("\r", "")
    prefix = "..." if start > 0 else ""
    suffix = "..." if end < len(text) else ""
    return prefix + snippet + suffix


def _extract_tool_use_summary(block: dict, pattern: str) -> str:
    """Build a human-readable summary for a tool_use block."""
    name = block.get("name", "?")
    inp = block.get("input", {})

    if name in ("Read", "Write"):
        path = inp.get("file_path", inp.get("path", ""))
        return f"{name} - {_truncate(path, 120)}"

    if name == "Edit":
        path = inp.get("file_path", "")
        old = _truncate(inp.get("old_string", ""), 40)
        new = _truncate(inp.get("new_string", ""), 40)
        filename = Path(path).name if path else "?"
        return f"Edit - {filename}: '{old}' -> '{new}'"

    if name == "Bash":
        cmd = inp.get("command", "")
        desc = inp.get("description", "")
        if desc:
            return f"Bash - {_truncate(desc, 80)}"
        return f"Bash - {_truncate(cmd, 80)}"

    if name == "Glob":
        return f"Glob - {inp.get('pattern', '')} in {inp.get('path', '.')}"

    if name == "Grep":
        return f"Grep - pattern='{inp.get('pattern', '')}' path={inp.get('path', '.')}"

    if name == "Agent":
        subagent_type = inp.get("subagent_type", "unknown")
        model = inp.get("model", "")
        desc = inp.get("description", "")
        if model:
            return f"Agent({subagent_type}) [{model}] - \"{_truncate(desc, 80)}\""
        return f"Agent({subagent_type}) - \"{_truncate(desc, 80)}\""

    # Fallback alias for older sessions that used "Task" instead of "Agent"
    if name == "Task":
        subagent_type = inp.get("subagent_type", "")
        model = inp.get("model", "")
        desc = inp.get("description", subagent_type)
        if model:
            return f"Task({subagent_type}) [{model}] - \"{_truncate(desc, 80)}\""
        return f"Task - {_truncate(desc, 80)}"

    if name == "Skill":
        return f"Skill({inp.get('skill', '')})"

    # Generic fallback: show first relevant input field containing pattern
    inp_str = json.dumps(inp, ensure_ascii=False)
    return f"{name} - {_snippet_around(inp_str, pattern, 50)}"


@dataclass
class MatchEntry:
    """A single search match with optional surrounding context."""
    lineno: int
    summary: str
    ctx_before: list[tuple[int, str]] = field(default_factory=list)
    ctx_after: list[tuple[int, str]] = field(default_factory=list)


def _extract_first_user_text(content) -> Optional[str]:
    """Extract first meaningful user text from message content.

    Handles plain string content, list-of-blocks content,
    and command-message XML tags. Returns None if no meaningful text found.
    """
    raw = None
    if isinstance(content, str) and content.strip():
        raw = content.strip()
    elif isinstance(content, list):
        for block in content:
            if isinstance(block, dict) and block.get("type") == "text":
                text = block.get("text", "").strip()
                if text:
                    raw = text
                    break
    if raw is None:
        return None
    cmd_match = re.search(
        r"<command-name>(/\S+)</command-name>\s*<command-args>(.*?)</command-args>", raw,
    )
    if cmd_match:
        return f"{cmd_match.group(1)} {cmd_match.group(2)}".strip()
    if not raw.startswith("<"):
        return _truncate(raw, 100)
    return None


def _content_to_text(content) -> str:
    """Flatten tool_result content (str or list of blocks) to plain text."""
    if isinstance(content, str):
        return content
    if isinstance(content, list):
        parts = []
        for block in content:
            if isinstance(block, dict):
                parts.append(block.get("text", block.get("content", "")))
            else:
                parts.append(str(block))
        return " ".join(parts)
    return str(content)


# Virtual content types that filter at the block level, not the record level
_VIRTUAL_CONTENT_TYPES = {"text", "tool_use", "tool_result", "user_text"}


def _extract_match_summary(
    obj: dict,
    pattern: str,
    tool_filter: Optional[set],
    tool_id_map: dict[str, str],
    no_results: bool,
    ignore_case: bool = False,
    compiled_re: Optional[re.Pattern] = None,
    content_type_filter: Optional[str] = None,
) -> Optional[str]:
    """
    Return a human-readable summary string for a matched JSON line,
    or None if the line doesn't satisfy constraints.

    tool_id_map: maps tool_use_id -> tool_name for tool_result annotation.
    no_results: if True, suppress tool_result matches entirely.
    ignore_case: if True, perform case-insensitive pattern matching.
    compiled_re: if set, use regex matching instead of substring.
    content_type_filter: if set (text/tool_use/tool_result), only match that block type.
    """
    scan_all = not pattern
    record_type = obj.get("type", "")
    message = obj.get("message", {})
    content = message.get("content", []) if isinstance(message, dict) else []

    def _match(haystack: str) -> bool:
        if not pattern:
            return True
        if compiled_re:
            return bool(compiled_re.search(haystack))
        if ignore_case:
            return pattern in haystack.lower()
        return pattern in haystack

    # Content type filter: skip non-matching block types
    # user_text: only user record text blocks (excludes tool_result from user records)
    is_user_text = content_type_filter == "user_text"
    want_text = content_type_filter is None or content_type_filter == "text" or is_user_text
    want_tool_use = content_type_filter is None or content_type_filter == "tool_use"
    want_tool_result = content_type_filter is None or content_type_filter == "tool_result"

    # user_text filter: skip assistant records entirely
    if is_user_text and record_type != "user":
        return None

    # assistant messages: look for tool_use blocks
    if record_type == "assistant" and isinstance(content, list):
        for block in content:
            if not isinstance(block, dict):
                continue
            if block.get("type") == "tool_use" and want_tool_use:
                tool_name = block.get("name", "")
                if tool_filter and tool_name not in tool_filter:
                    continue
                inp_str = json.dumps(block.get("input", {}), ensure_ascii=False)
                if scan_all or _match(tool_name) or _match(inp_str):
                    return _extract_tool_use_summary(block, pattern)
            elif block.get("type") == "text" and want_text:
                if tool_filter:
                    continue
                text = block.get("text", "")
                if scan_all and content_type_filter == "text":
                    # In text-only mode, show all text blocks
                    if text.strip():
                        return f"text - {_truncate(text, 100)}"
                    continue
                if scan_all:
                    continue
                if _match(text):
                    return f"text - {_snippet_around(text, pattern, ignore_case=ignore_case, compiled_re=compiled_re)}"

    # user messages: look for tool_result and text blocks
    elif record_type == "user" and isinstance(content, list):
        for block in content:
            if not isinstance(block, dict):
                continue
            if block.get("type") == "tool_result" and want_tool_result:
                if tool_filter or no_results:
                    continue
                if scan_all and content_type_filter != "tool_result":
                    continue
                raw = _content_to_text(block.get("content", ""))
                if scan_all and content_type_filter == "tool_result":
                    use_id = block.get("tool_use_id", "")
                    origin = tool_id_map.get(use_id, "")
                    prefix = f"result({origin})" if origin else "tool_result"
                    return f"{prefix} - \"{_truncate(raw, 100)}\""
                if _match(raw):
                    # Annotate with originating tool name
                    use_id = block.get("tool_use_id", "")
                    origin = tool_id_map.get(use_id, "")
                    prefix = f"result({origin})" if origin else "tool_result"
                    return f"{prefix} - \"{_snippet_around(raw, pattern, ignore_case=ignore_case, compiled_re=compiled_re)}\""
            elif block.get("type") == "text" and want_text:
                if tool_filter:
                    continue
                text = block.get("text", "")
                if scan_all and content_type_filter in ("text", "user_text"):
                    if text.strip():
                        return f"user_text - {_truncate(text, 100)}"
                    continue
                if scan_all and not tool_filter:
                    # Include user text blocks in scan-all mode (task prompts)
                    if text.strip():
                        return f"user_text - {_truncate(text, 100)}"
                    continue
                if _match(text):
                    return f"text - {_snippet_around(text, pattern, ignore_case=ignore_case, compiled_re=compiled_re)}"

    # user messages with plain string content
    elif record_type == "user" and isinstance(content, str):
        if not want_text:
            return None
        if tool_filter:
            return None
        if scan_all and not tool_filter:
            if content.strip():
                return f"user_text - {_truncate(content, 100)}"
            return None
        if _match(content):
            return f"user - {_snippet_around(content, pattern, ignore_case=ignore_case, compiled_re=compiled_re)}"

    # For other types (system, file-history-snapshot, queue-operation)
    # Note: 'progress' is excluded before reaching here (see scan_file)
    elif not tool_filter and not scan_all and content_type_filter is None:
        raw = json.dumps(obj, ensure_ascii=False)
        if _match(raw):
            return f"{record_type} - {_snippet_around(raw, pattern, ignore_case=ignore_case, compiled_re=compiled_re)}"

    return None


def _record_brief(obj: dict, tool_id_map: dict[str, str]) -> str:
    """Return a brief one-line summary of any JSONL record for context display."""
    record_type = obj.get("type", "")
    message = obj.get("message", {})
    content = message.get("content", []) if isinstance(message, dict) else []

    if record_type == "assistant" and isinstance(content, list):
        parts = []
        for block in content:
            if not isinstance(block, dict):
                continue
            if block.get("type") == "tool_use":
                parts.append(_extract_tool_use_summary(block, ""))
            elif block.get("type") == "text":
                text = block.get("text", "").strip()
                if text:
                    parts.append(f"text: {_truncate(text, 60)}")
        return " | ".join(parts) if parts else "[assistant]"

    if record_type == "user":
        if isinstance(content, str):
            return f"user: {_truncate(content, 80)}" if content.strip() else "[user]"
        if isinstance(content, list):
            parts = []
            for block in content:
                if not isinstance(block, dict):
                    continue
                if block.get("type") == "tool_result":
                    use_id = block.get("tool_use_id", "")
                    origin = tool_id_map.get(use_id, "?")
                    parts.append(f"result({origin})")
                elif block.get("type") == "text":
                    text = block.get("text", "").strip()
                    if text and not text.startswith("<"):
                        parts.append(f"user: {_truncate(text, 60)}")
            return " | ".join(parts) if parts else "[user]"

    return f"[{record_type}]"


# -- tool_use_id tracking -----------------------------------------------------

def _collect_tool_ids(obj: dict, tool_id_map: dict[str, str]) -> None:
    """Extract tool_use_id -> tool_name mappings from assistant messages."""
    if obj.get("type") != "assistant":
        return
    message = obj.get("message", {})
    content = message.get("content", []) if isinstance(message, dict) else []
    if not isinstance(content, list):
        return
    for block in content:
        if isinstance(block, dict) and block.get("type") == "tool_use":
            use_id = block.get("id", "")
            tool_name = block.get("name", "")
            if use_id and tool_name:
                tool_id_map[use_id] = tool_name


# -- line display -------------------------------------------------------------

def parse_line_spec(spec: str) -> tuple[int, int]:
    """Parse a line spec like '99' or '97-104' into (start, end) inclusive."""
    spec = spec.strip()
    if "-" in spec:
        parts = spec.split("-", 1)
        start = int(parts[0])
        end = int(parts[1])
        if start < 1 or end < start:
            raise ValueError(f"Invalid line range: {spec}")
        return (start, end)
    n = int(spec)
    if n < 1:
        raise ValueError(f"Invalid line number: {spec}")
    return (n, n)


def display_lines(
    jsonl_path: Path,
    start: int,
    end: int,
    raw: bool,
) -> int:
    """
    Display specific JSONL lines from a session file.

    Returns 0 on success, 1 on error.
    """
    tool_id_map: dict[str, str] = {}
    displayed = 0

    try:
        with open(jsonl_path, encoding="utf-8", errors="replace") as f:
            for lineno, line in enumerate(f, start=1):
                try:
                    obj = json.loads(line)
                except json.JSONDecodeError:
                    if start <= lineno <= end:
                        print(f"L{lineno:<5} [invalid JSON]")
                        displayed += 1
                    continue

                # Accumulate tool_id_map from all lines up to and including target
                _collect_tool_ids(obj, tool_id_map)

                if lineno < start:
                    continue
                if lineno > end:
                    break

                displayed += 1
                if raw:
                    print(json.dumps(obj, indent=2, ensure_ascii=True))
                    if lineno < end:
                        print("---")
                else:
                    ts = parse_iso(obj.get("timestamp", ""))
                    ts_str = format_dt(ts)
                    record_type = obj.get("type", "")
                    brief = _record_brief(obj, tool_id_map)
                    print(f"L{lineno:<5} [{record_type}] {ts_str} {brief}")

    except OSError as exc:
        print(f"ERROR: Cannot read {jsonl_path.name}: {exc}", file=sys.stderr)
        return 1

    if displayed == 0:
        print(f"No lines found in range {start}-{end} (file has fewer lines).")

    return 0


# -- session scanning ---------------------------------------------------------

def scan_file(
    jsonl_path: Path,
    pattern: str,
    tool_filter: Optional[set],
    type_filter: Optional[str],
    after: Optional[datetime],
    before: Optional[datetime],
    include_progress: bool = False,
    no_results: bool = False,
    ignore_case: bool = False,
    compiled_re: Optional[re.Pattern] = None,
    context: int = 0,
) -> Optional[dict]:
    """
    Scan a single JSONL file.

    Returns a session dict or None if no matches.
    When context > 0, match tuples include (lineno, summary, ctx_before, ctx_after).
    """
    scan_all = not pattern
    matches: list[MatchEntry] = []
    timestamps = []       # datetime objects seen in this file
    total_lines = 0
    tool_id_map: dict[str, str] = {}  # tool_use_id -> tool_name

    # Normalize pattern for case-insensitive fast path
    pattern_lower = pattern.lower() if ignore_case and pattern else pattern

    # Context tracking: collect all record summaries for building context windows
    all_records: Optional[list[tuple[int, str]]] = [] if context > 0 else None
    match_record_indices: Optional[list[int]] = [] if context > 0 else None

    try:
        with open(jsonl_path, encoding="utf-8", errors="replace") as f:
            for lineno, raw in enumerate(f, start=1):
                total_lines = lineno

                try:
                    obj = json.loads(raw)
                except json.JSONDecodeError:
                    continue

                # Always collect tool_use_id mappings (needed for tool_result annotation)
                _collect_tool_ids(obj, tool_id_map)

                # Collect timestamps
                ts_str = obj.get("timestamp", "")
                ts = parse_iso(ts_str)
                if ts:
                    timestamps.append(ts)

                # Exclude progress records by default
                record_type = obj.get("type", "")
                if record_type == "progress" and not include_progress:
                    continue

                # Apply type filter (record-level only; virtual types handled below)
                content_type_filter: Optional[str] = None
                if type_filter:
                    if type_filter in _VIRTUAL_CONTENT_TYPES:
                        # Virtual type: don't filter records, filter content blocks instead
                        content_type_filter = type_filter
                    elif record_type != type_filter:
                        continue

                # For context mode, collect brief summary for every record
                if context > 0:
                    brief = _record_brief(obj, tool_id_map)
                    all_records.append((lineno, brief))

                # Fast path: skip lines that cannot possibly match
                if not scan_all:
                    if compiled_re:
                        if not compiled_re.search(raw):
                            continue
                    elif ignore_case:
                        if pattern_lower not in raw.lower():
                            continue
                    elif pattern not in raw:
                        continue

                # Apply date filters
                if ts:
                    if after and ts < after:
                        continue
                    if before and ts >= before:
                        continue

                summary = _extract_match_summary(
                    obj, pattern_lower if ignore_case else pattern,
                    tool_filter, tool_id_map, no_results, ignore_case,
                    compiled_re, content_type_filter,
                )
                if summary is not None:
                    if context > 0:
                        # Track index and update with detailed match summary
                        idx = len(all_records) - 1
                        match_record_indices.append(idx)
                        all_records[idx] = (lineno, summary)
                    matches.append(MatchEntry(lineno, summary))

    except OSError as exc:
        print(f"WARN: Cannot read {jsonl_path.name}: {exc}", file=sys.stderr)
        return None

    if not matches:
        return None

    session_id = jsonl_path.stem

    # Detect parent_id from directory structure for subagent files
    parent_id = None
    if jsonl_path.parent.name == "subagents":
        parent_id = jsonl_path.parent.parent.name

    earliest = min(timestamps) if timestamps else None
    latest = max(timestamps) if timestamps else None

    # Build context-enriched matches
    if context > 0:
        context_matches: list[MatchEntry] = []
        for mi in match_record_indices:
            lineno, summary = all_records[mi]
            before_start = max(0, mi - context)
            after_end = min(len(all_records), mi + context + 1)
            ctx_before = [all_records[j] for j in range(before_start, mi)]
            ctx_after = [all_records[j] for j in range(mi + 1, after_end)]
            context_matches.append(MatchEntry(lineno, summary, ctx_before, ctx_after))
        final_matches = context_matches
    else:
        final_matches = matches

    result = {
        "session_id": session_id,
        "earliest": earliest,
        "latest": latest,
        "total_lines": total_lines,
        "matches": final_matches,
    }
    if parent_id:
        result["parent_id"] = parent_id
    return result


# -- timeline scanning --------------------------------------------------------

# Tools shown in timeline (significant actions only)
_TIMELINE_TOOLS = {"Agent", "Task", "Read", "Write", "Edit", "Bash", "Glob", "Grep", "Skill"}


def scan_timeline(
    jsonl_path: Path,
    after: Optional[datetime],
    before: Optional[datetime],
    tool_filter: Optional[set] = None,
    type_filter: Optional[str] = None,
) -> Optional[dict]:
    """
    Scan a JSONL file and produce a chronological timeline of significant events.

    Returns a session dict with 'events' instead of 'matches', or None.
    tool_filter: if provided, only show tool_use events for those tools.
    type_filter: if provided, only include records matching that type.
    """
    # Determine which tools to show in timeline
    effective_tools = tool_filter if tool_filter else _TIMELINE_TOOLS

    events = []       # [(timestamp_str, summary)]
    timestamps = []
    total_lines = 0

    try:
        with open(jsonl_path, encoding="utf-8", errors="replace") as f:
            for lineno, raw in enumerate(f, start=1):
                total_lines = lineno

                try:
                    obj = json.loads(raw)
                except json.JSONDecodeError:
                    continue

                record_type = obj.get("type", "")
                ts_str = obj.get("timestamp", "")
                ts = parse_iso(ts_str)
                if ts:
                    timestamps.append(ts)

                # Date filters
                if ts:
                    if after and ts < after:
                        continue
                    if before and ts >= before:
                        continue

                # Apply type filter (record-level; virtual types filter content blocks)
                ct_filter: Optional[str] = None
                if type_filter:
                    if type_filter in _VIRTUAL_CONTENT_TYPES:
                        ct_filter = type_filter
                    elif record_type != type_filter:
                        continue

                # Skip progress, queue-operation, system
                if record_type not in ("assistant", "user"):
                    continue

                message = obj.get("message", {})
                content = message.get("content", []) if isinstance(message, dict) else []

                want_text = ct_filter is None or ct_filter == "text"
                want_tool_use = ct_filter is None or ct_filter == "tool_use"

                if record_type == "assistant" and isinstance(content, list):
                    for block in content:
                        if not isinstance(block, dict):
                            continue
                        if block.get("type") == "tool_use" and want_tool_use:
                            tool_name = block.get("name", "")
                            if tool_name in effective_tools:
                                summary = _extract_tool_use_summary(block, "")
                                events.append((format_time(ts), summary))
                        elif block.get("type") == "text" and want_text:
                            text = block.get("text", "").strip()
                            if text and len(text) > 5:
                                events.append((format_time(ts), f"text: {_truncate(text, 90)}"))

                elif record_type == "user" and want_text:
                    if isinstance(content, str):
                        text = content.strip()
                        if text:
                            events.append((format_time(ts), f"user: {_truncate(text, 90)}"))
                    elif isinstance(content, list):
                        for block in content:
                            if not isinstance(block, dict):
                                continue
                            if block.get("type") == "text":
                                text = block.get("text", "").strip()
                                if text and not text.startswith("<"):
                                    events.append((format_time(ts), f"user: {_truncate(text, 90)}"))

    except OSError as exc:
        print(f"WARN: Cannot read {jsonl_path.name}: {exc}", file=sys.stderr)
        return None

    if not events:
        return None

    session_id = jsonl_path.stem
    parent_id = None
    if jsonl_path.parent.name == "subagents":
        parent_id = jsonl_path.parent.parent.name

    earliest = min(timestamps) if timestamps else None
    latest = max(timestamps) if timestamps else None

    result = {
        "session_id": session_id,
        "earliest": earliest,
        "latest": latest,
        "total_lines": total_lines,
        "events": events,
    }
    if parent_id:
        result["parent_id"] = parent_id
    return result


def scan_subagent_summary(jsonl_path: Path) -> Optional[dict]:
    """
    Scan a subagent JSONL file and produce a brief summary.

    Returns dict with tool counts and file list, or None.
    """
    tools: dict[str, int] = collections.Counter()
    files: set[str] = set()
    total_lines = 0
    timestamps: list[datetime] = []

    try:
        with open(jsonl_path, encoding="utf-8", errors="replace") as f:
            for lineno, raw in enumerate(f, start=1):
                total_lines = lineno
                try:
                    obj = json.loads(raw)
                except json.JSONDecodeError:
                    continue

                ts = parse_iso(obj.get("timestamp", ""))
                if ts:
                    timestamps.append(ts)

                if obj.get("type") != "assistant":
                    continue
                message = obj.get("message", {})
                content = message.get("content", []) if isinstance(message, dict) else []
                if not isinstance(content, list):
                    continue
                for block in content:
                    if isinstance(block, dict) and block.get("type") == "tool_use":
                        name = block.get("name", "?")
                        tools[name] += 1
                        inp = block.get("input", {})
                        fp = inp.get("file_path", "") or inp.get("path", "")
                        if fp:
                            files.add(os.path.basename(fp))

    except OSError:
        return None

    if not tools:
        return None

    return {
        "session_id": jsonl_path.stem,
        "total_lines": total_lines,
        "tools": dict(tools.most_common()),
        "files": sorted(files)[:10],
        "earliest": min(timestamps) if timestamps else None,
        "latest": max(timestamps) if timestamps else None,
    }


# -- summary scanning ---------------------------------------------------------

def scan_summary(jsonl_path: Path) -> Optional[dict]:
    """
    Scan a JSONL file and produce a session summary.

    Returns a summary dict or None on error.
    """
    first_user_text: Optional[str] = None
    timestamps: list[datetime] = []
    tool_counts: dict[str, int] = collections.Counter()
    errors: list[str] = []
    last_tokens: Optional[dict] = None
    total_lines = 0

    try:
        with open(jsonl_path, encoding="utf-8", errors="replace") as f:
            for lineno, raw in enumerate(f, start=1):
                total_lines = lineno
                try:
                    obj = json.loads(raw)
                except json.JSONDecodeError:
                    continue

                ts = parse_iso(obj.get("timestamp", ""))
                if ts:
                    timestamps.append(ts)

                record_type = obj.get("type", "")
                message = obj.get("message", {})
                content = message.get("content", []) if isinstance(message, dict) else []

                # First user text
                if first_user_text is None and record_type == "user":
                    first_user_text = _extract_first_user_text(content)

                # Tool counts
                if record_type == "assistant" and isinstance(content, list):
                    for block in content:
                        if isinstance(block, dict) and block.get("type") == "tool_use":
                            tool_name = block.get("name", "?")
                            tool_counts[tool_name] += 1

                # Error detection (from tool results with is_error)
                if record_type == "user" and isinstance(content, list):
                    for block in content:
                        if isinstance(block, dict) and block.get("type") == "tool_result":
                            if block.get("is_error"):
                                err_text = _content_to_text(block.get("content", ""))
                                errors.append(_truncate(err_text, 80))

                # Token usage (from assistant messages)
                if record_type == "assistant":
                    usage = message.get("usage", {}) if isinstance(message, dict) else {}
                    if usage:
                        last_tokens = usage

    except OSError as exc:
        print(f"ERROR: Cannot read {jsonl_path.name}: {exc}", file=sys.stderr)
        return None

    earliest = min(timestamps) if timestamps else None
    latest = max(timestamps) if timestamps else None

    return {
        "session_id": jsonl_path.stem,
        "first_user_text": first_user_text or "(no user text)",
        "earliest": earliest,
        "latest": latest,
        "total_lines": total_lines,
        "tool_counts": dict(collections.Counter(tool_counts).most_common()),
        "errors": errors,
        "last_tokens": last_tokens,
    }


def print_summary(summary: dict) -> None:
    """Print a formatted session summary."""
    sid = summary["session_id"]
    earliest = format_dt(summary["earliest"])
    latest = format_dt(summary["latest"])

    # Duration
    duration_str = "?"
    if summary["earliest"] and summary["latest"]:
        delta = summary["latest"] - summary["earliest"]
        minutes = delta.total_seconds() / 60
        duration_str = f"{minutes:.1f} min"

    print(f"Session {sid}")
    print(f"  Period: {earliest} ~ {latest}")
    print(f"  First: {summary['first_user_text']}")
    print(f"  Duration: {duration_str} | Lines: {summary['total_lines']}")

    # Tools
    if summary["tool_counts"]:
        tools_str = ", ".join(f"{k} {v}" for k, v in summary["tool_counts"].items())
        print(f"  Tools: {tools_str}")

    # Errors
    error_count = len(summary["errors"])
    if error_count > 0:
        sample = summary["errors"][0]
        if error_count == 1:
            print(f'  Errors: 1 ("{sample}")')
        else:
            print(f'  Errors: {error_count} (first: "{sample}")')
    else:
        print("  Errors: 0")

    # Tokens
    tokens = summary["last_tokens"]
    if tokens:
        input_t = tokens.get("input_tokens", 0)
        output_t = tokens.get("output_tokens", 0)
        cache_read = tokens.get("cache_read_input_tokens", 0)
        cache_create = tokens.get("cache_creation_input_tokens", 0)

        def _fmt_tokens(n: int) -> str:
            if n >= 1000:
                return f"{n / 1000:.0f}K"
            return str(n)

        parts = [f"input={_fmt_tokens(input_t)}"]
        if cache_read:
            parts.append(f"cached={_fmt_tokens(cache_read)}")
        if cache_create:
            parts.append(f"cache_create={_fmt_tokens(cache_create)}")
        parts.append(f"output={_fmt_tokens(output_t)}")
        print(f"  Last tokens: {', '.join(parts)}")


def print_session_list(summaries: list[dict], verbose: bool) -> None:
    """Print a compact session list table."""
    print("=== Recent Sessions ===")
    print()
    for idx, s in enumerate(summaries, 1):
        sid = s["session_id"]
        earliest_dt = s["earliest"]
        latest_dt = s["latest"]
        lines = s["total_lines"]
        first = s["first_user_text"]

        # Date range: compact "YYYY-MM-DD HH:MM~HH:MM"
        if earliest_dt and latest_dt:
            date_str = format_dt(earliest_dt)
            end_time = _to_local(latest_dt).strftime("%H:%M")
            date_range = f"{date_str}~{end_time}"
        else:
            date_range = "unknown"

        # Duration
        duration_str = "?"
        if earliest_dt and latest_dt:
            delta = latest_dt - earliest_dt
            minutes = int(delta.total_seconds() / 60)
            if minutes >= 60:
                duration_str = f"{minutes // 60}h{minutes % 60:02d}m"
            else:
                duration_str = f"{minutes}m"

        # Truncate first message for non-verbose
        display_first = first if verbose else _truncate(first, 50)

        print(f"  {idx:>2}  {sid}  {date_range}  {duration_str:>6}  {lines:>4}L  {display_first}")

        if verbose:
            tools = s.get("tool_counts", {})
            if tools:
                top = list(tools.items())[:5]
                tools_str = " ".join(f"{k}:{v}" for k, v in top)
                print(f"      Tools: {tools_str}")

    print()
    print(f"{len(summaries)} sessions found")


# -- output formatting --------------------------------------------------------

def _print_match_block(matches: list, indent: str, verbose: bool, max_default: int = 5) -> None:
    """Print match entries with optional context rendering."""
    display = matches if verbose else matches[:max_default]
    for entry in display:
        if entry.ctx_before or entry.ctx_after:
            for cl, cs in entry.ctx_before:
                print(f"{indent}L{cl:<5} {cs}")
            marker = indent[:-4] + ">>> " if len(indent) >= 4 else ">>> "
            print(f"{marker}L{entry.lineno:<5} {entry.summary}")
            for cl, cs in entry.ctx_after:
                print(f"{indent}L{cl:<5} {cs}")
            print(f"{indent}---")
        else:
            print(f"{indent}L{entry.lineno:<5} {entry.summary}")
    if not verbose and len(matches) > max_default:
        remaining = len(matches) - max_default
        print(f"{indent}... ({remaining} more, use --verbose to show all)")


def print_results(
    pattern: str,
    sessions: list[dict],
    verbose: bool,
) -> None:
    """Print formatted search results to stdout."""
    parent_sessions = [s for s in sessions if "parent_id" not in s]
    subagent_sessions = [s for s in sessions if "parent_id" in s]

    subagent_by_parent: dict[str, list[dict]] = {}
    for sub in subagent_sessions:
        pid = sub["parent_id"]
        subagent_by_parent.setdefault(pid, []).append(sub)

    parent_ids_in_results = {s["session_id"] for s in parent_sessions}
    orphan_subagents = [
        s for s in subagent_sessions
        if s["parent_id"] not in parent_ids_in_results
    ]

    total_matches = sum(len(s["matches"]) for s in sessions)

    header = f'=== Session Search: "{pattern}" ===' if pattern else "=== Session Search: (all) ==="
    print(header)
    print()

    idx = 0
    for session in parent_sessions:
        idx += 1
        sid = session["session_id"]
        earliest = format_dt(session["earliest"])
        latest = format_dt(session["latest"])
        lines = session["total_lines"]
        match_count = len(session["matches"])

        print(f"[{idx}] {sid}")
        print(f"    Date: {earliest} ~ {latest}  |  Lines: {lines}")
        print(f"    Matches: {match_count}")

        _print_match_block(session["matches"], "      ", verbose, 5)

        children = subagent_by_parent.get(sid, [])
        for child in children:
            child_sid = child["session_id"]
            child_earliest = format_dt(child["earliest"])
            child_latest = format_dt(child["latest"])
            child_lines = child["total_lines"]
            child_match_count = len(child["matches"])

            print(f"    [{idx}.sub] {child_sid} (subagent of {sid[:12]}...)")
            print(f"        Date: {child_earliest} ~ {child_latest}  |  Lines: {child_lines}")
            print(f"        Matches: {child_match_count}")

            _print_match_block(child["matches"], "          ", verbose, 5)

        print()

    for session in orphan_subagents:
        idx += 1
        sid = session["session_id"]
        pid = session["parent_id"]
        earliest = format_dt(session["earliest"])
        latest = format_dt(session["latest"])
        lines = session["total_lines"]
        match_count = len(session["matches"])

        print(f"[{idx}] {sid} (subagent of {pid[:12]}...)")
        print(f"    Date: {earliest} ~ {latest}  |  Lines: {lines}")
        print(f"    Matches: {match_count}")

        _print_match_block(session["matches"], "      ", verbose, 5)
        print()

    print(f"Found {len(sessions)} sessions ({total_matches} total matches)")


def print_timeline(
    sessions: list[dict],
    subagent_summaries: dict[str, list[dict]],
    verbose: bool,
) -> None:
    """Print chronological timeline for sessions."""
    parent_sessions = [s for s in sessions if "parent_id" not in s]
    subagent_sessions = [s for s in sessions if "parent_id" in s]

    subagent_by_parent: dict[str, list[dict]] = {}
    for sub in subagent_sessions:
        pid = sub["parent_id"]
        subagent_by_parent.setdefault(pid, []).append(sub)

    total_events = sum(len(s.get("events", [])) for s in sessions)

    print("=== Session Timeline ===")
    print()

    idx = 0
    for session in parent_sessions:
        idx += 1
        sid = session["session_id"]
        earliest = format_dt(session["earliest"])
        latest = format_dt(session["latest"])
        lines = session["total_lines"]
        events = session.get("events", [])

        print(f"[{idx}] {sid}")
        print(f"    Date: {earliest} ~ {latest}  |  Lines: {lines}  |  Events: {len(events)}")
        print()

        display_events = events if verbose else events[:20]
        for ts_str, summary in display_events:
            print(f"    {ts_str}  {summary}")
        if not verbose and len(events) > 20:
            remaining = len(events) - 20
            print(f"    ... ({remaining} more, use --verbose to show all)")

        # Print subagent timelines
        children = subagent_by_parent.get(sid, [])
        if children:
            print()
            print(f"    --- Subagents ({len(children)}) ---")
            for child in children:
                child_sid = child["session_id"]
                child_events = child.get("events", [])
                child_earliest = format_dt(child["earliest"])
                child_latest = format_dt(child["latest"])
                print(f"    [{idx}.sub] {child_sid}")
                print(f"        Date: {child_earliest} ~ {child_latest}  |  Events: {len(child_events)}")
                child_display = child_events if verbose else child_events[:10]
                for ts_str, summary in child_display:
                    print(f"        {ts_str}  {summary}")
                if not verbose and len(child_events) > 10:
                    remaining = len(child_events) - 10
                    print(f"        ... ({remaining} more)")

        # Print subagent summaries (for subagents with no timeline events)
        summaries = subagent_summaries.get(sid, [])
        displayed_subs = {c["session_id"] for c in children}
        remaining_summaries = [s for s in summaries if s["session_id"] not in displayed_subs]
        if remaining_summaries:
            if not children:
                print()
                print(f"    --- Subagents ({len(remaining_summaries)}) ---")
            for sa in remaining_summaries:
                sa_sid = sa["session_id"]
                tools_str = ", ".join(f"{k}:{v}" for k, v in sa["tools"].items())
                files_str = ", ".join(sa["files"][:5])
                if len(sa["files"]) > 5:
                    files_str += f" (+{len(sa['files']) - 5})"
                print(f"    [{idx}.sub] {sa_sid}  |  Lines: {sa['total_lines']}")
                print(f"        Tools: {tools_str}")
                if files_str:
                    print(f"        Files: {files_str}")

        print()

    print(f"Found {len(parent_sessions)} sessions ({total_events} total events)")


def print_counts(
    pattern: str,
    sessions: list[dict],
) -> None:
    """Print only match counts per session (compact survey mode)."""
    header = f'=== Session Search: "{pattern}" ===' if pattern else "=== Session Search: (all) ==="
    print(header)
    print()

    total = 0
    for session in sessions:
        sid = session["session_id"]
        earliest = format_dt(session["earliest"])
        match_count = len(session["matches"])
        total += match_count
        parent_tag = ""
        if "parent_id" in session:
            parent_tag = f" (sub of {session['parent_id'][:8]}...)"
        print(f"  {match_count:>4}  {sid[:20]}...  {earliest}{parent_tag}")

    print()
    print(f"Total: {total} matches in {len(sessions)} sessions")


# -- autopsy -----------------------------------------------------------------

def scan_autopsy(jsonl_path: Path) -> Optional[dict]:
    """Single-pass scan collecting termination diagnostics for a session."""
    session_id = jsonl_path.stem
    timestamps: list[datetime] = []
    last_assistant_line: Optional[int] = None
    last_assistant_stop_reason: Optional[str] = None
    pending_tools: dict[str, dict] = {}  # tool_use_id -> {name, line, summary}
    api_errors: list[dict] = []  # {line, text}
    token_trajectory: list[dict] = []  # {line, input, output}
    last_usage: Optional[dict] = None
    assistant_count = 0
    user_count = 0
    last_record_type: Optional[str] = None
    last_record_line = 0
    first_user_text: Optional[str] = None
    total_lines = 0

    try:
        with open(jsonl_path, encoding="utf-8", errors="replace") as f:
            for lineno, raw in enumerate(f, start=1):
                total_lines = lineno
                try:
                    obj = json.loads(raw)
                except json.JSONDecodeError:
                    continue

                ts = parse_iso(obj.get("timestamp", ""))
                if ts:
                    timestamps.append(ts)

                record_type = obj.get("type", "")
                if record_type in ("assistant", "user"):
                    last_record_type = record_type
                    last_record_line = lineno

                message = obj.get("message", {})
                content = message.get("content", []) if isinstance(message, dict) else []

                if record_type == "assistant":
                    assistant_count += 1
                    last_assistant_line = lineno
                    stop_reason = message.get("stop_reason", "") if isinstance(message, dict) else ""
                    if stop_reason:
                        last_assistant_stop_reason = stop_reason

                    # Token usage
                    usage = message.get("usage", {}) if isinstance(message, dict) else {}
                    if usage:
                        last_usage = usage
                        # Sample every 10th assistant message
                        if assistant_count % 10 == 0:
                            inp = usage.get("input_tokens", 0) + usage.get("cache_read_input_tokens", 0)
                            out = usage.get("output_tokens", 0)
                            token_trajectory.append({"line": lineno, "input": inp, "output": out})

                    # Track tool_use blocks
                    if isinstance(content, list):
                        for block in content:
                            if isinstance(block, dict) and block.get("type") == "tool_use":
                                use_id = block.get("id", "")
                                if use_id:
                                    pending_tools[use_id] = {
                                        "name": block.get("name", "?"),
                                        "line": lineno,
                                        "summary": _extract_tool_use_summary(block, ""),
                                    }

                elif record_type == "user":
                    user_count += 1

                    # First user text
                    if first_user_text is None:
                        first_user_text = _extract_first_user_text(content)

                    # Remove resolved tool results
                    if isinstance(content, list):
                        for block in content:
                            if isinstance(block, dict) and block.get("type") == "tool_result":
                                use_id = block.get("tool_use_id", "")
                                pending_tools.pop(use_id, None)

                    # Detect API errors in tool_result content
                    if isinstance(content, list):
                        for block in content:
                            if isinstance(block, dict) and block.get("type") == "tool_result":
                                if block.get("is_error"):
                                    err_text = _content_to_text(block.get("content", "")).lower()
                                    if any(kw in err_text for kw in ("overloaded", "rate_limit", "429")):
                                        api_errors.append({"line": lineno, "text": err_text[:200]})

                # Also check for error-type records (API errors outside tool_result)
                if record_type == "error" or (isinstance(message, dict) and message.get("error")):
                    err_raw = json.dumps(obj, ensure_ascii=False).lower()
                    if any(kw in err_raw for kw in ("overloaded", "rate_limit", "429")):
                        api_errors.append({"line": lineno, "text": _truncate(err_raw, 200)})

    except OSError as exc:
        print(f"ERROR: Cannot read {jsonl_path.name}: {exc}", file=sys.stderr)
        return None

    earliest = min(timestamps) if timestamps else None
    latest = max(timestamps) if timestamps else None

    return {
        "session_id": session_id,
        "total_lines": total_lines,
        "earliest": earliest,
        "latest": latest,
        "last_assistant_line": last_assistant_line,
        "last_assistant_stop_reason": last_assistant_stop_reason,
        "pending_tools": pending_tools,
        "api_errors": api_errors,
        "token_trajectory": token_trajectory,
        "last_usage": last_usage,
        "assistant_count": assistant_count,
        "user_count": user_count,
        "last_record_type": last_record_type,
        "last_record_line": last_record_line,
        "first_user_text": first_user_text or "(no user text)",
    }


def scan_subagent_autopsy(
    ccs_dirs: list[Path], session_id: str
) -> list[dict]:
    """Scan subagent JSONLs for completion/interruption state."""
    # Collect and deduplicate subagent files (keep largest)
    seen: dict[str, tuple[Path, int]] = {}
    for d in ccs_dirs:
        for p in d.glob(f"{session_id}*/subagents/*.jsonl"):
            parent_id = p.parent.parent.name
            dedup_key = f"{parent_id}/{p.stem}"
            size = p.stat().st_size
            if dedup_key not in seen or size > seen[dedup_key][1]:
                seen[dedup_key] = (p, size)

    results: list[dict] = []
    for _key, (p, _size) in seen.items():
        info = _scan_one_subagent(p)
        if info:
            results.append(info)

    results.sort(key=lambda x: x.get("earliest") or datetime.min.replace(tzinfo=timezone.utc))
    return results


def _scan_one_subagent(jsonl_path: Path) -> Optional[dict]:
    """Scan a single subagent JSONL for autopsy data."""
    timestamps: list[datetime] = []
    total_lines = 0
    last_stop_reason: Optional[str] = None
    pending_tools: dict[str, dict] = {}
    spawn_summary: Optional[str] = None

    try:
        with open(jsonl_path, encoding="utf-8", errors="replace") as f:
            for lineno, raw in enumerate(f, start=1):
                total_lines = lineno
                try:
                    obj = json.loads(raw)
                except json.JSONDecodeError:
                    continue

                ts = parse_iso(obj.get("timestamp", ""))
                if ts:
                    timestamps.append(ts)

                record_type = obj.get("type", "")
                message = obj.get("message", {})
                content = message.get("content", []) if isinstance(message, dict) else []

                # Get spawn description from first user message
                if spawn_summary is None and record_type == "user":
                    spawn_summary = _extract_first_user_text(content)

                if record_type == "assistant":
                    stop_reason = message.get("stop_reason", "") if isinstance(message, dict) else ""
                    if stop_reason:
                        last_stop_reason = stop_reason

                    if isinstance(content, list):
                        for block in content:
                            if isinstance(block, dict) and block.get("type") == "tool_use":
                                use_id = block.get("id", "")
                                if use_id:
                                    pending_tools[use_id] = {
                                        "name": block.get("name", "?"),
                                        "line": lineno,
                                        "summary": _extract_tool_use_summary(block, ""),
                                    }

                elif record_type == "user" and isinstance(content, list):
                    for block in content:
                        if isinstance(block, dict) and block.get("type") == "tool_result":
                            pending_tools.pop(block.get("tool_use_id", ""), None)

    except OSError:
        return None

    if total_lines == 0:
        return None

    earliest = min(timestamps) if timestamps else None
    latest = max(timestamps) if timestamps else None
    completed = last_stop_reason == "end_turn" and not pending_tools

    return {
        "subagent_id": jsonl_path.stem,
        "total_lines": total_lines,
        "earliest": earliest,
        "latest": latest,
        "last_stop_reason": last_stop_reason,
        "completed": completed,
        "pending_tools": pending_tools,
        "spawn_summary": spawn_summary or "(unknown)",
    }


def correlate_pm2_events(
    session_end_dt: datetime, window_minutes: int = 2
) -> list[dict]:
    """Find PM2 events within a time window of the session end."""
    try:
        import importlib.util
        pm2_diag_path = Path(__file__).parent / "pm2_diag.py"
        if not pm2_diag_path.exists():
            return []
        spec = importlib.util.spec_from_file_location("pm2_diag", pm2_diag_path)
        pm2_mod = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(pm2_mod)

        log_path = pm2_mod.get_default_log_path()
        if not log_path.exists():
            return []

        events = pm2_mod.parse_pm2_log(log_path)
        if not events:
            return []

        # Convert session_end_dt (UTC) to local time for comparison with PM2 timestamps
        local_offset = datetime.now().astimezone().utcoffset() or timedelta(0)
        session_end_local = session_end_dt.replace(tzinfo=None) + local_offset

        window = timedelta(minutes=window_minutes)
        start = session_end_local - window
        end = session_end_local + window

        # Noise patterns to skip (PM2 internal issues, not app events)
        noise_patterns = {"spawn wmic", "pidusage"}

        matched = []
        for ev in events:
            ev_ts = ev.get("timestamp")
            if ev_ts is None:
                continue
            if start <= ev_ts <= end:
                msg = ev.get("action") or ev.get("message", "")
                # Skip noisy PM2 internal errors
                if any(noise in msg for noise in noise_patterns):
                    continue
                decoded = ""
                if ev.get("exit_code") is not None:
                    decoded = pm2_mod.decode_exit_code(ev["exit_code"])
                matched.append({
                    "timestamp": ev_ts,
                    "app": ev.get("app", "?"),
                    "message": msg,
                    "exit_code": ev.get("exit_code"),
                    "decoded": decoded,
                })
        return matched
    except Exception:
        return []


def find_concurrent_sessions(
    ccs_dirs: list[Path],
    earliest: datetime,
    latest: datetime,
    exclude_id: str,
) -> list[dict]:
    """Find sessions that overlapped in time with the target session."""
    all_files = _collect_jsonl_files(ccs_dirs, None, False)
    # Pre-filter by mtime: files last modified before the target session
    # started can't overlap (1-hour margin for clock skew)
    cutoff_ts = earliest.timestamp() - 3600
    all_files = [p for p in all_files if p.stat().st_mtime >= cutoff_ts]
    concurrent: list[dict] = []

    for jsonl_path in all_files:
        sid = jsonl_path.stem
        if sid == exclude_id:
            continue
        summary = scan_summary(jsonl_path)
        if summary is None:
            continue
        s_earliest = summary.get("earliest")
        s_latest = summary.get("latest")
        if s_earliest is None or s_latest is None:
            continue
        # Check time overlap: A.start <= B.end AND A.end >= B.start
        if s_earliest <= latest and s_latest >= earliest:
            concurrent.append({
                "session_id": sid,
                "earliest": s_earliest,
                "latest": s_latest,
                "first_user_text": summary.get("first_user_text", "(unknown)"),
            })

    concurrent.sort(key=lambda x: x["earliest"])
    return concurrent


def classify_termination(
    autopsy_data: dict, subagent_data: list[dict]
) -> tuple[str, str]:
    """Classify why a session ended. Returns (verdict_code, explanation)."""
    api_errors = autopsy_data.get("api_errors", [])
    pending = autopsy_data.get("pending_tools", {})
    stop_reason = autopsy_data.get("last_assistant_stop_reason")
    last_usage = autopsy_data.get("last_usage") or {}
    last_type = autopsy_data.get("last_record_type")

    # 1. API_OVERLOADED
    for err in api_errors:
        if "overloaded" in err.get("text", ""):
            return ("API_OVERLOADED", "API returned overloaded error")

    # 2. RATE_LIMITED
    for err in api_errors:
        text = err.get("text", "")
        if "429" in text or "rate_limit" in text:
            return ("RATE_LIMITED", "API returned rate limit error")

    # 3. CONTEXT_LIMIT
    input_tokens = last_usage.get("input_tokens", 0) + last_usage.get("cache_read_input_tokens", 0)
    if input_tokens > 180000:
        return ("CONTEXT_LIMIT", f"Input tokens ({input_tokens:,}) exceeded 180K limit")

    # 4. NORMAL
    if stop_reason == "end_turn" and not pending:
        return ("NORMAL", "Session completed normally (end_turn, no pending tools)")

    # 5. INTERRUPTED_SUBAGENT
    pending_agents = [v for v in pending.values() if v["name"] in ("Agent", "Task")]
    if pending_agents:
        names = ", ".join(v["summary"] for v in pending_agents)
        return ("INTERRUPTED_SUBAGENT", f"Session interrupted with pending: {_truncate(names, 120)}")

    # 6. INTERRUPTED_TOOL
    if pending:
        names = ", ".join(v["name"] for v in pending.values())
        return ("INTERRUPTED_TOOL", f"Session interrupted with pending tools: {names}")

    # 7. USER_INTERRUPTED
    if stop_reason == "stop_sequence":
        return ("USER_INTERRUPTED", "User sent stop signal (stop_sequence)")

    # 8. NO_RESPONSE
    if last_type == "user":
        return ("NO_RESPONSE", "Last record is user message (no API response received)")

    # 9. UNKNOWN
    return ("UNKNOWN", f"stop_reason={stop_reason}, pending={len(pending)}, last_type={last_type}")


def print_autopsy(
    autopsy_data: dict,
    subagent_data: list[dict],
    pm2_events: list[dict],
    concurrent: list[dict],
    verdict_code: str,
    verdict_text: str,
) -> None:
    """Print structured autopsy report."""
    sid = autopsy_data["session_id"]

    def _fmt_tokens(n: int) -> str:
        if n >= 1000:
            return f"{n / 1000:.0f}K"
        return str(n)

    # Header
    print(f"=== Session Autopsy: {sid} ===")
    print(f"VERDICT: {verdict_code} -- {verdict_text}")
    print()

    # Session Overview
    earliest = autopsy_data["earliest"]
    latest = autopsy_data["latest"]
    duration_str = "?"
    if earliest and latest:
        delta = latest - earliest
        minutes = int(delta.total_seconds() / 60)
        duration_str = f"{minutes}min"

    e_str = format_time(earliest) if earliest else "?"
    l_str = format_time(latest) if latest else "?"
    print("--- Session Overview ---")
    print(f"  Period: {e_str}~{l_str} ({duration_str})  "
          f"Lines: {autopsy_data['total_lines']}  "
          f"Messages: {autopsy_data['assistant_count']} asst, {autopsy_data['user_count']} user")
    if autopsy_data.get("first_user_text"):
        print(f"  First: {autopsy_data['first_user_text']}")
    print()

    # Termination State
    print("--- Termination State ---")
    last_line = autopsy_data.get("last_assistant_line") or "?"
    stop = autopsy_data.get("last_assistant_stop_reason") or "?"
    print(f"  Last assistant (L{last_line}): stop_reason={stop}")

    pending = autopsy_data.get("pending_tools", {})
    if pending:
        print(f"  Pending tools ({len(pending)}):")
        for uid, info in pending.items():
            print(f"    L{info['line']} {info['summary']}  [NO RESULT]")
    else:
        print("  Pending tools: (none)")
    print()

    # Subagent State
    if subagent_data:
        in_flight = [s for s in subagent_data if not s["completed"]]
        print(f"--- Subagent State ---")
        print(f"  {len(subagent_data)} total, {len(in_flight)} in-flight:")
        for sa in subagent_data:
            status = "DONE" if sa["completed"] else "IN-FLIGHT"
            sa_earliest = format_time(sa["earliest"]) if sa["earliest"] else "?"
            sa_latest = format_time(sa["latest"]) if sa["latest"] else "?"
            pending_info = ""
            if sa["pending_tools"]:
                pnames = ", ".join(v["name"] for v in sa["pending_tools"].values())
                pending_info = f"  pending: {pnames}"
            print(f"    {sa['subagent_id'][:16]} ({status}) "
                  f"{sa_earliest}~{sa_latest} {sa['total_lines']}L  "
                  f"last: {sa.get('last_stop_reason', '?')}{pending_info}")
    else:
        print("--- Subagent State ---")
        print("  (no subagents)")
    print()

    # Token Usage
    print("--- Token Usage ---")
    usage = autopsy_data.get("last_usage")
    if usage:
        inp = usage.get("input_tokens", 0) + usage.get("cache_read_input_tokens", 0)
        out = usage.get("output_tokens", 0)
        print(f"  Final: input={_fmt_tokens(inp)}, output={_fmt_tokens(out)}")
    else:
        print("  Final: (no usage data)")

    trajectory = autopsy_data.get("token_trajectory", [])
    if trajectory:
        samples = [_fmt_tokens(t["input"]) for t in trajectory]
        print(f"  Trajectory: {' -> '.join(samples)}")
    print()

    # API Errors
    api_errors = autopsy_data.get("api_errors", [])
    if api_errors:
        print(f"--- API Errors ({len(api_errors)}) ---")
        for err in api_errors[:5]:
            print(f"  L{err['line']}: {_truncate(err['text'], 120)}")
        if len(api_errors) > 5:
            print(f"  ... ({len(api_errors) - 5} more)")
        print()

    # Temporal Context
    print("--- Temporal Context ---")
    if pm2_events:
        print(f"  PM2 events ({len(pm2_events)}):")
        for ev in pm2_events[:5]:
            ts_str = ev["timestamp"].strftime("%H:%M:%S") if ev["timestamp"] else "?"
            decoded = f" ({ev['decoded']})" if ev.get("decoded") else ""
            print(f"    {ts_str} [{ev['app']}] {_truncate(ev['message'], 80)}{decoded}")
    else:
        print("  PM2: (none within window)")

    if concurrent:
        print(f"  Concurrent sessions ({len(concurrent)}):")
        for cs in concurrent[:5]:
            cs_e = format_time(cs["earliest"]) if cs["earliest"] else "?"
            cs_l = format_time(cs["latest"]) if cs["latest"] else "?"
            print(f"    {cs['session_id'][:16]} ({cs_e}~{cs_l}) {_truncate(cs['first_user_text'], 50)}")
    else:
        print("  Concurrent: (none)")


# -- trace mode --------------------------------------------------------------

def _list_trace_subagents(ccs_dirs: list[Path], session_id: str) -> None:
    """List all subagents for a session with metadata for --trace * mode."""
    results = scan_subagent_autopsy(ccs_dirs, session_id)
    short_id = session_id[:8]
    print(f"\n=== Subagents: {short_id} ===\n")

    if not results:
        print("(no subagents)")
        return

    print(f"  {'#':>3}  {'ID':8}  {'Time':19}  {'Lines':>5}  Description")
    for idx, info in enumerate(results, start=1):
        agent_id = info["subagent_id"]
        # Strip "agent-" prefix, show first 8 chars
        short = agent_id.replace("agent-", "")[:8]
        t0 = format_time(info.get("earliest"))
        t1 = format_time(info.get("latest"))
        lines = info.get("total_lines", 0)
        desc = _truncate(info.get("spawn_summary", ""), 60)
        print(f"  {idx:3}  {short:8}  {t0}~{t1}  {lines:5}  {desc}")

    print(f"\n{len(results)} subagents")
    # Hint: show last subagent's short ID as example
    last_short = results[-1]["subagent_id"].replace("agent-", "")[:8]
    print(f"\nTrace: --session {short_id} --trace {last_short}")


def _resolve_trace_file(
    ccs_dirs: list[Path],
    session_prefix: str,
    trace_prefix: str,
    parser: argparse.ArgumentParser,
) -> Path:
    """Resolve trace target to a single JSONL path.

    trace_prefix == "." means the parent session itself.
    Otherwise glob for subagent files matching the prefix.
    """
    # Resolve parent session first
    jsonl_files = _resolve_session_files(ccs_dirs, session_prefix, parser)
    if len(jsonl_files) > 1:
        print(f'ERROR: Multiple sessions match "{session_prefix}". Be more specific.', file=sys.stderr)
        for p in jsonl_files:
            print(f"  {p.stem}", file=sys.stderr)
        sys.exit(1)
    parent_path = jsonl_files[0]

    if trace_prefix == ".":
        return parent_path

    # Find subagent files
    session_dir = parent_path.parent / parent_path.stem
    subagent_dir = session_dir / "subagents"
    if not subagent_dir.is_dir():
        print(f"ERROR: No subagents directory found for session {parent_path.stem}", file=sys.stderr)
        sys.exit(1)

    matches = [p for p in subagent_dir.glob("agent-*.jsonl") if trace_prefix in p.stem]
    if not matches:
        available = sorted(p.stem for p in subagent_dir.glob("agent-*.jsonl"))
        print(f'ERROR: No subagent matching "{trace_prefix}". Available:', file=sys.stderr)
        for name in available:
            print(f"  {name}", file=sys.stderr)
        sys.exit(1)
    if len(matches) > 1:
        print(f'ERROR: Multiple subagents match "{trace_prefix}":', file=sys.stderr)
        for p in sorted(matches, key=lambda x: x.stem):
            print(f"  {p.stem}", file=sys.stderr)
        sys.exit(1)

    return matches[0]


def scan_trace(
    jsonl_path: Path,
    tool_filter: Optional[set],
    pattern: str,
    ignore_case: bool,
    compiled_re: Optional[re.Pattern],
) -> Optional[list[dict]]:
    """Scan JSONL and pair tool_use with tool_result into trace entries."""
    pending: dict[str, dict] = {}  # tool_use_id -> partial entry
    entries: list[dict] = []

    try:
        with open(jsonl_path, encoding="utf-8", errors="replace") as f:
            for lineno, raw in enumerate(f, start=1):
                try:
                    obj = json.loads(raw)
                except json.JSONDecodeError:
                    continue

                record_type = obj.get("type", "")
                ts = obj.get("timestamp", "")
                message = obj.get("message", {})
                content = message.get("content", []) if isinstance(message, dict) else []
                if not isinstance(content, list):
                    continue

                if record_type == "assistant":
                    for block in content:
                        if isinstance(block, dict) and block.get("type") == "tool_use":
                            use_id = block.get("id", "")
                            tool_name = block.get("name", "")
                            if not use_id:
                                continue
                            if tool_filter and tool_name not in tool_filter:
                                continue
                            pending[use_id] = {
                                "lineno_use": lineno,
                                "tool_name": tool_name,
                                "tool_id": use_id,
                                "input": block.get("input", {}),
                                "timestamp_use": ts,
                            }
                        elif isinstance(block, dict) and block.get("type") == "text":
                            text_content = block.get("text", "")
                            if not text_content.strip():
                                continue
                            if tool_filter and "[text]" not in tool_filter:
                                continue
                            # Pattern filter
                            if pattern or compiled_re:
                                haystack = text_content
                                if compiled_re:
                                    if not compiled_re.search(haystack):
                                        continue
                                elif ignore_case:
                                    if pattern not in haystack.lower():
                                        continue
                                else:
                                    if pattern not in haystack:
                                        continue
                            text_entry = {
                                "lineno_use": lineno,
                                "lineno_result": lineno,
                                "tool_name": "[text]",
                                "tool_id": f"text-{lineno}-{len(entries)}",
                                "input": {},
                                "output": text_content,
                                "is_error": False,
                                "timestamp_use": ts,
                                "timestamp_result": ts,
                            }
                            entries.append(text_entry)

                elif record_type == "user":
                    for block in content:
                        if isinstance(block, dict) and block.get("type") == "tool_result":
                            use_id = block.get("tool_use_id", "")
                            if use_id not in pending:
                                continue
                            entry = pending.pop(use_id)
                            result_content = _content_to_text(block.get("content", ""))
                            entry["lineno_result"] = lineno
                            entry["output"] = result_content
                            entry["is_error"] = block.get("is_error", False)
                            entry["timestamp_result"] = ts

                            # Pattern filter
                            if pattern or compiled_re:
                                haystack = json.dumps(entry["input"], ensure_ascii=False) + "\n" + result_content
                                if compiled_re:
                                    if not compiled_re.search(haystack):
                                        continue
                                elif ignore_case:
                                    if pattern not in haystack.lower():
                                        continue
                                else:
                                    if pattern not in haystack:
                                        continue

                            entries.append(entry)

    except OSError as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return None

    return entries


def print_trace(entries: list[dict], jsonl_path: Path, verbose: bool) -> None:
    """Pretty-print trace entries."""
    label = jsonl_path.stem
    print(f"\n=== Tool Trace: {label} ===\n")

    if not entries:
        print("(no matching tool calls)")
        return

    tool_counts: dict[str, int] = collections.Counter()
    max_output_lines = 0 if verbose else 5

    for idx, entry in enumerate(entries, start=1):
        name = entry["tool_name"]
        tool_counts[name] += 1
        ts_use = parse_iso(entry.get("timestamp_use", ""))
        ts_str = format_time(ts_use)
        lr = entry.get("lineno_result", "?")

        print(f"--- [{idx}] {name} (L{entry['lineno_use']} -> L{lr}) {ts_str} ---")

        inp = entry.get("input", {})
        # Tool-specific input formatting
        if name == "[text]":
            # Text block: display content directly, no input section
            output = entry.get("output", "")
            lines = output.split("\n") if output else []
            total = len(lines)
            if max_output_lines and total > max_output_lines:
                print(f"  TEXT ({total} lines, showing {max_output_lines}):")
                for line in lines[:max_output_lines]:
                    print(f"    {line}")
                print(f"    ... ({total - max_output_lines} more lines)")
            elif lines:
                suffix = f" ({total} line{'s' if total != 1 else ''})"
                print(f"  TEXT{suffix}:")
                for line in lines:
                    print(f"    {line}")
            else:
                print("  TEXT: (empty)")
            print()
            continue
        elif name == "Bash":
            cmd = inp.get("command", "")
            desc = inp.get("description", "")
            print(f"  $ {cmd}")
            if desc:
                print(f"  (desc: {desc})")
        elif name in ("Read", "Write"):
            print(f"  file_path: {inp.get('file_path', inp.get('path', '?'))}")
        elif name == "Edit":
            path = inp.get("file_path", "?")
            old = _truncate(inp.get("old_string", ""), 60)
            new = _truncate(inp.get("new_string", ""), 60)
            print(f"  file_path: {path}")
            print(f"  old: {old}")
            print(f"  new: {new}")
        elif name in ("Grep", "Glob"):
            print(f"  pattern: {inp.get('pattern', '?')}")
            if inp.get("path"):
                print(f"  path: {inp['path']}")
        elif name == "Agent":
            stype = inp.get("subagent_type", "?")
            desc = inp.get("description", "")
            model = inp.get("model", "")
            label_parts = [f"subagent_type: {stype}"]
            if model:
                label_parts.append(f"model: {model}")
            print(f"  {', '.join(label_parts)}")
            if desc:
                print(f"  desc: {_truncate(desc, 120)}")
        elif name == "Skill":
            print(f"  skill: {inp.get('skill', '?')}")
        else:
            inp_str = json.dumps(inp, ensure_ascii=False)
            print(f"  input: {_truncate(inp_str, 120)}")

        # Output
        output = entry.get("output", "")
        is_error = entry.get("is_error", False)
        lines = output.split("\n") if output else []
        total = len(lines)

        error_tag = " [ERROR]" if is_error else ""
        if max_output_lines and total > max_output_lines:
            print(f"  OUTPUT{error_tag} ({total} lines, showing {max_output_lines}):")
            for line in lines[:max_output_lines]:
                print(f"    {line}")
            print(f"    ... ({total - max_output_lines} more lines)")
        elif lines:
            suffix = f" ({total} line{'s' if total != 1 else ''})"
            print(f"  OUTPUT{error_tag}{suffix}:")
            for line in lines:
                print(f"    {line}")
        else:
            print(f"  OUTPUT{error_tag}: (empty)")

        print()

    # Summary footer
    count_parts = ", ".join(f"{n} {name}" for name, n in tool_counts.most_common())
    print(f"{len(entries)} tool calls total ({count_parts})")

    # Duration
    first_ts = parse_iso(entries[0].get("timestamp_use", ""))
    last_entry = entries[-1]
    last_ts = parse_iso(last_entry.get("timestamp_result", "") or last_entry.get("timestamp_use", ""))
    if first_ts and last_ts:
        delta = int((last_ts - first_ts).total_seconds())
        print(f"Time: {format_time(first_ts)} ~ {format_time(last_ts)} ({delta}s)")


# -- main --------------------------------------------------------------------

def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Search Claude Code session JSONL files.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Basic search
  python session-search.py "feature-797"
  python session-search.py "feature-797" --tool Write,Edit
  python session-search.py "feature-797" --after 2026-02-18
  python session-search.py -r "feature-8[01]\\d" --count
  python session-search.py "POST-LOOP" --or "batch_mode" --or "context_pct"
  python session-search.py "nonexistent" --strict  # exit code 2 if no match

  # Session inspection
  python session-search.py --list
  python session-search.py --list --after 2026-03-01 --limit 10
  python session-search.py --session be309 --summary
  python session-search.py --session be309 --timeline
  python session-search.py --session be309 --line 99
  python session-search.py --session be309 --line 97-104 --raw
  python session-search.py --session be309 "Context at" -C 2
  python session-search.py --session be309 "issue" --type text
  python session-search.py "keyword" --type user_text  # user text only (no tool_result)
  python session-search.py --session be309 --type tool_use
  python session-search.py --session be309 --autopsy

  # Subagent investigation
  python session-search.py --session be309 --agents              # list Agent calls + subagent summaries
  python session-search.py "feature-806" --tool Agent --subagents # search inside subagent JSONLs too
  python session-search.py "feature-806" --timeline --subagents   # timeline including subagents
  python session-search.py --session be309 --trace "*"            # list all subagents with IDs
  python session-search.py --session be309 --trace a396           # full tool trace of one subagent
  python session-search.py --session be309 --trace a396 --tool Bash  # trace filtered to Bash only
  python session-search.py --session be309 --trace . --tool Agent    # trace parent session's Agent calls
  python session-search.py --session be309 --trace . --tool Agent --verbose  # full subagent output
  python session-search.py --session be309 "pattern" --no-truncate           # no text truncation
        """,
    )
    parser.add_argument(
        "pattern",
        nargs="?",
        default="",
        help="Search pattern (plain text, matched against raw JSON line). "
             "Optional when --session or --timeline is used.",
    )
    parser.add_argument(
        "--tool",
        metavar="NAMES",
        help="Comma-separated tool names to filter (e.g. Write,Edit,Bash)",
    )
    parser.add_argument(
        "--session",
        metavar="ID",
        help="Filter by session ID prefix match (e.g. --session be309)",
    )
    parser.add_argument(
        "--subagents",
        action="store_true",
        help="Include subagent JSONL files in the search",
    )
    parser.add_argument(
        "--agents",
        action="store_true",
        help="Shorthand for --tool Agent --subagents",
    )
    parser.add_argument(
        "--timeline",
        action="store_true",
        help="Show chronological timeline of significant events",
    )
    parser.add_argument(
        "--no-results",
        action="store_true",
        help="Suppress tool_result matches (reduce noise)",
    )
    parser.add_argument(
        "--include-progress",
        action="store_true",
        help="Include progress records (excluded by default)",
    )
    parser.add_argument(
        "--after",
        type=parse_date,
        metavar="DATE",
        help="Only sessions with timestamps after this date (YYYY-MM-DD)",
    )
    parser.add_argument(
        "--before",
        type=parse_date,
        metavar="DATE",
        help="Only sessions with timestamps before this date (YYYY-MM-DD)",
    )
    parser.add_argument(
        "--type",
        dest="record_type",
        metavar="TYPE",
        help="Filter by record type (user, assistant, progress) or "
             "content block type (text, tool_use, tool_result, user_text). "
             "user_text = user text blocks only (excludes tool_result)",
    )
    parser.add_argument(
        "--limit",
        type=int,
        default=None,
        metavar="N",
        help="Max sessions to display (default: 25, or 3 for --timeline without --session)",
    )
    parser.add_argument(
        "-i", "--ignore-case",
        action="store_true",
        help="Case-insensitive pattern matching",
    )
    parser.add_argument(
        "-r", "--regex",
        action="store_true",
        help="Treat pattern as a regular expression",
    )
    parser.add_argument(
        "--count",
        action="store_true",
        help="Show only match counts per session (no details)",
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Show all matches/events per session (default: first 5/20)",
    )
    parser.add_argument(
        "--ccs-dir",
        metavar="PATH",
        help="Override JSONL directory (default: auto-detect from ~/.ccs/instances/*/projects/)",
    )
    parser.add_argument(
        "--strict",
        action="store_true",
        help="Exit with code 2 when no matches found (default: exit 0)",
    )
    parser.add_argument(
        "--or",
        dest="or_patterns",
        action="append",
        default=[],
        metavar="PATTERN",
        help="Additional search pattern(s) to OR-combine with the main pattern (repeatable)",
    )
    parser.add_argument(
        "--line",
        metavar="SPEC",
        help="Display specific JSONL line(s): N or N-M (requires --session)",
    )
    parser.add_argument(
        "--raw",
        action="store_true",
        help="Output raw JSON (use with --line)",
    )
    parser.add_argument(
        "--summary",
        action="store_true",
        help="Show session summary: first message, duration, tool counts, errors, tokens (requires --session)",
    )
    parser.add_argument(
        "--list",
        action="store_true",
        help="List recent sessions with summary info (no pattern required). Compatible with --after, --before, --limit.",
    )
    parser.add_argument(
        "-C", "--context",
        type=int,
        default=0,
        metavar="N",
        help="Show N records before and after each match (like grep -C)",
    )
    parser.add_argument(
        "--autopsy",
        action="store_true",
        help="Diagnose why a session ended (requires --session). "
             "Shows verdict, termination state, subagent status, token usage, "
             "and correlated PM2/debug log events.",
    )
    parser.add_argument(
        "--trace",
        metavar="PREFIX",
        help='Trace tool calls in a subagent by ID prefix. '
             'Special values: "." = parent session, "*" = list all subagents. '
             "Requires --session.",
    )
    parser.add_argument(
        "--width",
        type=int,
        default=None,
        metavar="N",
        help="Display width for truncation (default: 100, 0=unlimited). "
             "Overrides per-field truncation widths.",
    )
    parser.add_argument(
        "--no-truncate",
        action="store_true",
        help="Disable text truncation (alias for --width 0)",
    )
    return parser


def _collect_jsonl_files(
    ccs_dirs: list[Path],
    session_prefix: Optional[str],
    include_subagents: bool,
) -> list[Path]:
    """Collect and deduplicate JSONL files from CCS directories."""
    # Parent sessions
    seen: dict[str, tuple[Path, int]] = {}
    for d in ccs_dirs:
        for p in d.glob("*.jsonl"):
            sid = p.stem
            size = p.stat().st_size
            if sid not in seen or size > seen[sid][1]:
                seen[sid] = (p, size)
    jsonl_files = [path for path, _ in seen.values()]

    if session_prefix:
        jsonl_files = [p for p in jsonl_files if p.stem.startswith(session_prefix)]

    # Subagent files
    if include_subagents:
        seen_sub: dict[str, tuple[Path, int]] = {}
        for d in ccs_dirs:
            for p in d.glob("*/subagents/*.jsonl"):
                parent_id = p.parent.parent.name
                if session_prefix and not parent_id.startswith(session_prefix):
                    continue
                dedup_key = f"{parent_id}/{p.stem}"
                size = p.stat().st_size
                if dedup_key not in seen_sub or size > seen_sub[dedup_key][1]:
                    seen_sub[dedup_key] = (p, size)
        jsonl_files.extend(path for path, _ in seen_sub.values())

    return jsonl_files


def _collect_subagent_summaries(
    ccs_dirs: list[Path],
    session_prefix: Optional[str],
) -> dict[str, list[dict]]:
    """Collect subagent summaries grouped by parent session ID."""
    result: dict[str, list[dict]] = {}
    seen_sub: dict[str, tuple[Path, int]] = {}

    for d in ccs_dirs:
        for p in d.glob("*/subagents/*.jsonl"):
            parent_id = p.parent.parent.name
            if session_prefix and not parent_id.startswith(session_prefix):
                continue
            dedup_key = f"{parent_id}/{p.stem}"
            size = p.stat().st_size
            if dedup_key not in seen_sub or size > seen_sub[dedup_key][1]:
                seen_sub[dedup_key] = (p, size)

    for dedup_key, (p, _) in seen_sub.items():
        parent_id = p.parent.parent.name
        summary = scan_subagent_summary(p)
        if summary:
            result.setdefault(parent_id, []).append(summary)

    return result


def _resolve_session_files(
    ccs_dirs: list[Path],
    session_prefix: str,
    parser: argparse.ArgumentParser,
) -> list[Path]:
    """Resolve session prefix to JSONL files, erroring if ambiguous or missing."""
    jsonl_files = _collect_jsonl_files(ccs_dirs, session_prefix, False)
    if not jsonl_files:
        print(f'ERROR: No session found matching "{session_prefix}".', file=sys.stderr)
        sys.exit(1)
    return jsonl_files


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()

    # Apply --width / --no-truncate global display width
    global _display_width
    if args.no_truncate:
        _display_width = 0
    elif args.width is not None:
        _display_width = args.width

    # --agents is shorthand for --tool Agent --subagents
    if args.agents:
        args.subagents = True
        if not args.tool:
            args.tool = "Agent"

    # Validate --raw requires --line
    if args.raw and args.line is None:
        parser.error("--raw requires --line")

    pattern = args.pattern

    # Resolve effective --limit (sentinel-based default)
    if args.limit is not None:
        effective_limit = args.limit
    elif args.timeline and not args.session:
        effective_limit = 3
    else:
        effective_limit = 25

    tool_filter: Optional[set] = (
        {t.strip() for t in args.tool.split(",")} if args.tool else None
    )
    type_filter: Optional[str] = args.record_type
    ignore_case: bool = args.ignore_case
    use_regex: bool = args.regex

    # Compile regex pattern (with OR pattern combination)
    compiled_re: Optional[re.Pattern] = None
    or_patterns: list[str] = args.or_patterns
    if or_patterns:
        if not pattern:
            parser.error("--or requires a main pattern")
        all_patterns = [pattern] + or_patterns
        if use_regex:
            combined = "|".join(f"(?:{p})" for p in all_patterns)
        else:
            combined = "|".join(re.escape(p) for p in all_patterns)
        flags = re.IGNORECASE if ignore_case else 0
        try:
            compiled_re = re.compile(combined, flags)
        except re.error as exc:
            parser.error(f"invalid combined pattern: {exc}")
    elif use_regex and pattern:
        flags = re.IGNORECASE if ignore_case else 0
        try:
            compiled_re = re.compile(pattern, flags)
        except re.error as exc:
            parser.error(f"invalid regex: {exc}")
    elif ignore_case and pattern:
        # Normalize pattern for case-insensitive substring matching
        pattern = pattern.lower()

    # Display label (preserves original case, includes --or patterns)
    if or_patterns:
        display_pattern = " | ".join([args.pattern] + or_patterns)
    else:
        display_pattern = args.pattern

    ccs_dirs = find_ccs_dirs(args.ccs_dir)

    # --line mode (early branch)
    if args.line is not None:
        if not args.session:
            parser.error("--line requires --session")
        jsonl_files = _resolve_session_files(ccs_dirs, args.session, parser)
        if len(jsonl_files) > 1:
            print(f'ERROR: Multiple sessions match "{args.session}". Be more specific.', file=sys.stderr)
            for p in jsonl_files:
                print(f"  {p.stem}", file=sys.stderr)
            return 1
        try:
            start, end = parse_line_spec(args.line)
        except ValueError as exc:
            parser.error(str(exc))
        return display_lines(jsonl_files[0], start, end, args.raw)

    # --summary mode (early branch)
    if args.summary:
        if not args.session:
            parser.error("--summary requires --session")
        jsonl_files = _resolve_session_files(ccs_dirs, args.session, parser)
        if len(jsonl_files) > 1:
            print(f'ERROR: Multiple sessions match "{args.session}". Be more specific.', file=sys.stderr)
            for p in jsonl_files:
                print(f"  {p.stem}", file=sys.stderr)
            return 1
        summary = scan_summary(jsonl_files[0])
        if summary is None:
            return 1
        print_summary(summary)
        return 0

    # --list mode (early branch)
    if args.list:
        all_files = _collect_jsonl_files(ccs_dirs, args.session, False)
        if not all_files:
            print("No sessions found.", file=sys.stderr)
            return 1

        # Pre-sort by mtime (newest first) to limit scan when no date filters
        limit = args.limit if args.limit is not None else 20
        if not args.after and not args.before:
            all_files.sort(key=lambda p: p.stat().st_mtime, reverse=True)
            all_files = all_files[:limit * 2]  # margin for None summaries

        summaries = []
        for jsonl_path in all_files:
            summary = scan_summary(jsonl_path)
            if summary is None:
                continue
            # Apply date filters
            if args.after and summary["latest"] and summary["latest"] < args.after:
                continue
            if args.before and summary["earliest"] and summary["earliest"] >= args.before:
                continue
            summaries.append(summary)

        # Sort by latest timestamp (newest first)
        summaries.sort(
            key=lambda s: s["latest"] or datetime.min.replace(tzinfo=timezone.utc),
            reverse=True,
        )
        summaries = summaries[:limit]

        print_session_list(summaries, args.verbose)
        return 0

    # --autopsy mode (early branch)
    if args.autopsy:
        if not args.session:
            parser.error("--autopsy requires --session")
        jsonl_files = _resolve_session_files(ccs_dirs, args.session, parser)
        if len(jsonl_files) > 1:
            print(f'ERROR: Multiple sessions match "{args.session}". Be more specific.', file=sys.stderr)
            for p in jsonl_files:
                print(f"  {p.stem}", file=sys.stderr)
            return 1
        autopsy_data = scan_autopsy(jsonl_files[0])
        if autopsy_data is None:
            return 1
        subagent_data = scan_subagent_autopsy(ccs_dirs, autopsy_data["session_id"])
        verdict_code, verdict_text = classify_termination(autopsy_data, subagent_data)
        pm2_events: list[dict] = []
        if autopsy_data["latest"]:
            pm2_events = correlate_pm2_events(autopsy_data["latest"])
        concurrent: list[dict] = []
        if autopsy_data["earliest"] and autopsy_data["latest"]:
            concurrent = find_concurrent_sessions(
                ccs_dirs, autopsy_data["earliest"], autopsy_data["latest"],
                autopsy_data["session_id"],
            )
        print_autopsy(autopsy_data, subagent_data, pm2_events, concurrent, verdict_code, verdict_text)
        return 0

    # --trace mode (early branch)
    if args.trace is not None:
        if not args.session:
            parser.error("--trace requires --session")
        if args.trace == "*":
            jsonl_files = _resolve_session_files(ccs_dirs, args.session, parser)
            if len(jsonl_files) > 1:
                print(f'ERROR: Multiple sessions match "{args.session}". Be more specific.', file=sys.stderr)
                for p in jsonl_files:
                    print(f"  {p.stem}", file=sys.stderr)
                return 1
            _list_trace_subagents(ccs_dirs, jsonl_files[0].stem)
            return 0
        trace_path = _resolve_trace_file(ccs_dirs, args.session, args.trace, parser)
        trace_entries = scan_trace(trace_path, tool_filter, pattern, ignore_case, compiled_re)
        if trace_entries is None:
            return 1
        print_trace(trace_entries, trace_path, args.verbose)
        return 0

    # Validate: pattern is required unless --session or --timeline is specified
    if not pattern and not args.session and not args.timeline and not args.list:
        parser.error("pattern is required unless --session or --timeline is specified")

    # Timeline mode
    if args.timeline:
        if args.session:
            # Existing behavior: direct timeline scan for specific session
            jsonl_files = _collect_jsonl_files(ccs_dirs, args.session, args.subagents)
            if not jsonl_files:
                print("ERROR: No JSONL files found.", file=sys.stderr)
                return 1

            if len(jsonl_files) > 20:
                print(f"Scanning {len(jsonl_files)} files...", file=sys.stderr)

            sessions: list[dict] = []
            for jsonl_path in jsonl_files:
                result = scan_timeline(
                    jsonl_path, args.after, args.before,
                    tool_filter=tool_filter, type_filter=type_filter,
                )
                if result:
                    sessions.append(result)

            if not sessions:
                print(f'No timeline events found for session prefix "{args.session}".')
                return 2 if args.strict else 0

            sessions.sort(key=lambda s: s["earliest"] or datetime.min.replace(tzinfo=timezone.utc), reverse=True)
            limited = sessions[:effective_limit]

            # Collect subagent summaries for non-matching subagents
            subagent_summaries = _collect_subagent_summaries(ccs_dirs, args.session)
            print_timeline(limited, subagent_summaries, args.verbose)

            return 0
        else:
            # New: 2-pass - find matching sessions, then timeline them
            if not pattern:
                parser.error("--timeline without --session requires a search pattern")

            all_files = _collect_jsonl_files(ccs_dirs, None, args.subagents)
            if not all_files:
                print("ERROR: No JSONL files found.", file=sys.stderr)
                return 1

            if len(all_files) > 20:
                print(f"Scanning {len(all_files)} files...", file=sys.stderr)

            # Pass 1: find sessions with matches
            matched_files: list[tuple[Path, dict]] = []
            for jsonl_path in all_files:
                result = scan_file(
                    jsonl_path, pattern, tool_filter, type_filter,
                    args.after, args.before,
                    include_progress=args.include_progress,
                    no_results=args.no_results,
                    ignore_case=ignore_case,
                    compiled_re=compiled_re,
                )
                if result:
                    matched_files.append((jsonl_path, result))

            if not matched_files:
                print(f'No sessions matching "{display_pattern}" found.')
                return 2 if args.strict else 0

            # Sort by latest timestamp, limit
            matched_files.sort(
                key=lambda x: x[1].get("latest") or datetime.min.replace(tzinfo=timezone.utc),
                reverse=True,
            )
            matched_files = matched_files[:effective_limit]

            # Pass 2: generate timelines for matched sessions
            sessions = []
            for jsonl_path, _ in matched_files:
                result = scan_timeline(
                    jsonl_path, args.after, args.before,
                    tool_filter=tool_filter, type_filter=type_filter,
                )
                if result:
                    sessions.append(result)

            if not sessions:
                print(f'No timeline events found for matched sessions.')
                return 2 if args.strict else 0

            sessions.sort(key=lambda s: s["earliest"] or datetime.min.replace(tzinfo=timezone.utc), reverse=True)
            print_timeline(sessions, {}, args.verbose)

            return 0

    # Normal search mode
    jsonl_files = _collect_jsonl_files(ccs_dirs, args.session, args.subagents)
    if not jsonl_files:
        print("ERROR: No JSONL files found.", file=sys.stderr)
        return 1

    if len(jsonl_files) > 20:
        print(f"Scanning {len(jsonl_files)} files...", file=sys.stderr)

    total_files = len(jsonl_files)
    sessions = []
    context_n = args.context if not args.count else 0

    for jsonl_path in jsonl_files:
        result = scan_file(
            jsonl_path,
            pattern,
            tool_filter,
            type_filter,
            args.after,
            args.before,
            include_progress=args.include_progress,
            no_results=args.no_results,
            ignore_case=ignore_case,
            compiled_re=compiled_re,
            context=context_n,
        )
        if result:
            sessions.append(result)

    if not sessions:
        label = f'"{display_pattern}"' if display_pattern else "(all)"
        print(f'No matches found for {label} in {total_files} files.')
        return 2 if args.strict else 0

    sessions.sort(key=lambda s: s["latest"] or datetime.min.replace(tzinfo=timezone.utc), reverse=True)
    limited = sessions[:effective_limit]

    if args.count:
        print_counts(display_pattern, limited)
    else:
        print_results(display_pattern, limited, args.verbose)

    if len(sessions) > effective_limit:
        print(
            f"(Showing {effective_limit} of {len(sessions)} matching sessions. Use --limit to show more.)",
            file=sys.stderr,
        )

    return 0


if __name__ == "__main__":
    sys.exit(main())
