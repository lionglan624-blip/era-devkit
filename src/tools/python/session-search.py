#!/usr/bin/env python3
"""
Claude Code Session JSONL Search Tool

Searches session log files stored by Claude Code for patterns across
tool calls, messages, and results.

Usage:
    python tools/session-search.py "feature-797"
    python tools/session-search.py "feature-797" --tool Write,Edit
    python tools/session-search.py "feature-797" --after 2026-02-18
    python tools/session-search.py "feature-797" --verbose

Exit codes:
    0 = Found results
    1 = No results or error
"""

import argparse
import json
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Optional


# ── helpers ──────────────────────────────────────────────────────────────────

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
    # Python < 3.11: strptime doesn't handle trailing 'Z'
    try:
        return datetime.fromisoformat(ts.replace("Z", "+00:00"))
    except (ValueError, AttributeError):
        return None


# ── match extraction ──────────────────────────────────────────────────────────

def _truncate(text: str, width: int = 100) -> str:
    """Truncate string to width, appending ellipsis if needed."""
    text = text.replace("\n", " ").replace("\r", "")
    if len(text) <= width:
        return text
    return text[:width - 3] + "..."


def _snippet_around(text: str, pattern: str, radius: int = 50) -> str:
    """Return a snippet of text centered on the first occurrence of pattern."""
    idx = text.find(pattern)
    if idx == -1:
        return _truncate(text, radius * 2)
    start = max(0, idx - radius)
    end = min(len(text), idx + len(pattern) + radius)
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
        return f"Edit - {Path(path).name}: '{old}' → '{new}'"

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

    if name == "Task":
        desc = inp.get("description", inp.get("subagent_type", ""))
        return f"Task - {_truncate(desc, 80)}"

    if name == "Skill":
        return f"Skill({inp.get('skill', '')})"

    # Generic fallback: show first relevant input field containing pattern
    inp_str = json.dumps(inp, ensure_ascii=False)
    return f"{name} - {_snippet_around(inp_str, pattern, 50)}"


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


def _extract_match_summary(obj: dict, pattern: str, tool_filter: Optional[set]) -> Optional[str]:
    """
    Return a human-readable summary string for a matched JSON line,
    or None if the line doesn't satisfy the tool_filter constraint.
    """
    record_type = obj.get("type", "")
    message = obj.get("message", {})
    content = message.get("content", []) if isinstance(message, dict) else []

    # assistant messages: look for tool_use blocks
    if record_type == "assistant" and isinstance(content, list):
        for block in content:
            if not isinstance(block, dict):
                continue
            if block.get("type") == "tool_use":
                tool_name = block.get("name", "")
                if tool_filter and tool_name not in tool_filter:
                    continue
                inp_str = json.dumps(block.get("input", {}), ensure_ascii=False)
                # Check pattern in tool name or its input
                if pattern in tool_name or pattern in inp_str:
                    return _extract_tool_use_summary(block, pattern)
            elif block.get("type") == "text":
                if tool_filter:
                    continue  # text blocks don't match tool filter
                text = block.get("text", "")
                if pattern in text:
                    return f"text - {_snippet_around(text, pattern)}"

    # user messages: look for tool_result blocks
    elif record_type == "user" and isinstance(content, list):
        for block in content:
            if not isinstance(block, dict):
                continue
            if block.get("type") == "tool_result":
                if tool_filter:
                    # tool_result blocks don't carry tool names directly;
                    # skip if a tool filter is active
                    continue
                raw = _content_to_text(block.get("content", ""))
                if pattern in raw:
                    return f"tool_result - \"{_snippet_around(raw, pattern)}\""
            elif block.get("type") == "text":
                if tool_filter:
                    continue
                text = block.get("text", "")
                if pattern in text:
                    return f"text - {_snippet_around(text, pattern)}"

    # user messages with plain string content
    elif record_type == "user" and isinstance(content, str):
        if tool_filter:
            return None
        if pattern in content:
            return f"user - {_snippet_around(content, pattern)}"

    # For other types (progress, system, file-history-snapshot, queue-operation)
    # just surface the raw JSON snippet if it matches and no tool filter
    elif not tool_filter:
        raw = json.dumps(obj, ensure_ascii=False)
        if pattern in raw:
            return f"{record_type} - {_snippet_around(raw, pattern)}"

    return None


# ── session scanning ──────────────────────────────────────────────────────────

def scan_file(
    jsonl_path: Path,
    pattern: str,
    tool_filter: Optional[set],
    type_filter: Optional[str],
    after: Optional[datetime],
    before: Optional[datetime],
    verbose: bool,
) -> Optional[dict]:
    """
    Scan a single JSONL file.

    Returns a session dict or None if no matches.
    """
    matches = []          # [(line_number, summary)]
    timestamps = []       # datetime objects seen in this file
    total_lines = 0

    try:
        with open(jsonl_path, encoding="utf-8", errors="replace") as f:
            for lineno, raw in enumerate(f, start=1):
                total_lines = lineno

                # Fast path: skip lines that cannot possibly match
                if pattern not in raw:
                    continue

                # Parse only matching lines
                try:
                    obj = json.loads(raw)
                except json.JSONDecodeError:
                    continue

                # Collect timestamps from every parsed line (for date-range display)
                ts_str = obj.get("timestamp", "")
                ts = parse_iso(ts_str)
                if ts:
                    timestamps.append(ts)

                # Apply type filter
                record_type = obj.get("type", "")
                if type_filter and record_type != type_filter:
                    continue

                # Apply date filters based on line timestamp
                if ts:
                    if after and ts < after:
                        continue
                    if before and ts >= before:
                        continue

                summary = _extract_match_summary(obj, pattern, tool_filter)
                if summary is not None:
                    matches.append((lineno, summary))

    except OSError as exc:
        print(f"WARN: Cannot read {jsonl_path.name}: {exc}", file=sys.stderr)
        return None

    if not matches:
        return None

    session_id = jsonl_path.stem
    earliest = min(timestamps) if timestamps else None
    latest = max(timestamps) if timestamps else None

    return {
        "session_id": session_id,
        "earliest": earliest,
        "latest": latest,
        "total_lines": total_lines,
        "matches": matches,
    }


# ── output formatting ─────────────────────────────────────────────────────────

def format_dt(dt: Optional[datetime]) -> str:
    """Format datetime for display as 'YYYY-MM-DD HH:MM'."""
    if dt is None:
        return "unknown"
    # Show in local-ish UTC display
    return dt.strftime("%Y-%m-%d %H:%M")


def print_results(
    pattern: str,
    sessions: list[dict],
    verbose: bool,
) -> None:
    """Print formatted search results to stdout."""
    total_matches = sum(len(s["matches"]) for s in sessions)

    print(f'=== Session Search: "{pattern}" ===')
    print()

    for idx, session in enumerate(sessions, start=1):
        sid = session["session_id"]
        earliest = format_dt(session["earliest"])
        latest = format_dt(session["latest"])
        lines = session["total_lines"]
        match_count = len(session["matches"])

        print(f"[{idx}] {sid}")
        print(f"    Date: {earliest} ~ {latest}  |  Lines: {lines}")
        print(f"    Matches: {match_count}")

        # Show up to 5 matches by default, all in verbose mode
        display_matches = session["matches"] if verbose else session["matches"][:5]
        for lineno, summary in display_matches:
            print(f"      L{lineno:<5} {summary}")
        if not verbose and len(session["matches"]) > 5:
            remaining = len(session["matches"]) - 5
            print(f"      ... ({remaining} more, use --verbose to show all)")
        print()

    print(f"Found {len(sessions)} sessions ({total_matches} total matches)")


# ── main ──────────────────────────────────────────────────────────────────────

def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Search Claude Code session JSONL files.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python tools/session-search.py "feature-797"
  python tools/session-search.py "feature-797" --tool Write,Edit
  python tools/session-search.py "feature-797" --after 2026-02-18
  python tools/session-search.py "feature-797" --verbose
        """,
    )
    parser.add_argument(
        "pattern",
        help="Search pattern (plain text, matched against raw JSON line)",
    )
    parser.add_argument(
        "--tool",
        metavar="NAMES",
        help="Comma-separated tool names to filter (e.g. Write,Edit,Bash)",
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
        help="Record type filter (user, assistant, progress, etc.)",
    )
    parser.add_argument(
        "--limit",
        type=int,
        default=10,
        metavar="N",
        help="Max sessions to display (default: 10)",
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Show all matches per session (default: first 5)",
    )
    parser.add_argument(
        "--ccs-dir",
        metavar="PATH",
        help="Override JSONL directory (default: auto-detect from ~/.ccs/instances/*/projects/)",
    )
    return parser


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()

    pattern = args.pattern
    tool_filter: Optional[set] = (
        {t.strip() for t in args.tool.split(",")} if args.tool else None
    )
    type_filter: Optional[str] = args.record_type

    # Locate JSONL directories
    ccs_dirs = find_ccs_dirs(args.ccs_dir)

    # Collect all JSONL files, deduplicating by session ID
    # (same session can appear in multiple provider dirs; keep largest file)
    seen: dict[str, Path] = {}
    for d in ccs_dirs:
        for p in d.glob("*.jsonl"):
            sid = p.stem
            if sid not in seen or p.stat().st_size > seen[sid].stat().st_size:
                seen[sid] = p
    jsonl_files = list(seen.values())

    if not jsonl_files:
        print("ERROR: No JSONL files found.", file=sys.stderr)
        return 1

    total_files = len(jsonl_files)
    sessions: list[dict] = []

    for jsonl_path in jsonl_files:
        result = scan_file(
            jsonl_path,
            pattern,
            tool_filter,
            type_filter,
            args.after,
            args.before,
            args.verbose,
        )
        if result:
            sessions.append(result)

    if not sessions:
        print(f'No matches found for "{pattern}" in {total_files} files.', file=sys.stderr)
        return 1

    # Sort by latest timestamp descending (most recent session first)
    sessions.sort(key=lambda s: s["latest"] or datetime.min.replace(tzinfo=timezone.utc), reverse=True)

    # Apply limit
    limited = sessions[: args.limit]

    print_results(pattern, limited, args.verbose)

    if len(sessions) > args.limit:
        print(
            f"(Showing {args.limit} of {len(sessions)} matching sessions. Use --limit to show more.)",
            file=sys.stderr,
        )

    return 0


if __name__ == "__main__":
    sys.exit(main())
