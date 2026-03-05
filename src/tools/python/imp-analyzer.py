#!/usr/bin/env python3
"""
imp-analyzer.py - Feature Lifecycle Analysis Tool

Analyzes past Claude Code session JSONL files for a feature's FC/FL/RUN
lifecycle and generates a structured improvement report.

Usage:
    python src/tools/python/imp-analyzer.py 813
    python src/tools/python/imp-analyzer.py 813 --verbose
    python src/tools/python/imp-analyzer.py 813 --json
    python src/tools/python/imp-analyzer.py 813 --max-sessions 10
    python src/tools/python/imp-analyzer.py 813 --ccs-dir /path/to/ccs

Exit codes:
    0 = Success
    1 = Error (invalid arguments, session not found, I/O error)
"""

import argparse
import collections
import dataclasses
import io
import json
import re
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Optional

# Force UTF-8 output on Windows (avoids cp932 UnicodeEncodeError)
if sys.stdout.encoding != "utf-8":
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
if sys.stderr.encoding != "utf-8":
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8", errors="replace")

# --- Constants ---
REPO_ROOT = Path(__file__).resolve().parent.parent.parent.parent
FEATURES_DIR = REPO_ROOT / "pm" / "features"

# Path prefixes to strip during normalization
_PATH_PREFIXES = [
    "C:/Era/devkit/",
    "/c/Era/devkit/",
    "/mnt/c/Era/devkit/",
    "C:\\Era\\devkit\\",
]

# FC phase → prevention rule mapping
FC_PREVENTION_MAP = {
    "ac-gap": ("Phase 4 (ac-designer)", "AC:Task カバレッジ検証の強化"),
    "stale-reference": ("Phase 6 (quality-fixer)", "クロスリファレンス検証"),
    "template-compliance": ("Phase 6 (quality-fixer)", "テンプレートチェックリスト強化"),
    "deferred-obligation": ("Phase 5 (wbs-generator)", "Mandatory Handoffs自動生成"),
    "scope-reduction": ("Phase 4/5", "孤立Task/AC検出"),
    "language-policy": ("Phase 6 (quality-fixer)", "言語ポリシー自動チェック"),
    "structural-reorganization": ("Phase 4 (ac-designer)", "セクション配置検証"),
    "content-correction": ("Phase 4/5", "ソースとの数値照合"),
    "other": ("(該当なし)", "パターン分析要"),
}


# =============================================================================
# Data Classes
# =============================================================================


@dataclasses.dataclass
class SessionInfo:
    """Metadata about a discovered session file."""
    path: Path
    session_type: str  # "fc", "fl", "run"
    start_time: Optional[datetime] = None
    end_time: Optional[datetime] = None
    total_lines: int = 0

    @property
    def duration_minutes(self) -> float:
        if self.start_time and self.end_time:
            delta = self.end_time - self.start_time
            return delta.total_seconds() / 60.0
        return 0.0


@dataclasses.dataclass
class SessionStats:
    """Analysis results for a single session."""
    session_info: SessionInfo
    tool_counts: dict = dataclasses.field(default_factory=dict)
    file_reads: dict = dataclasses.field(default_factory=dict)
    file_writes: dict = dataclasses.field(default_factory=dict)
    grep_patterns: dict = dataclasses.field(default_factory=dict)
    error_count: int = 0
    errors: list = dataclasses.field(default_factory=list)
    agent_dispatches: list = dataclasses.field(default_factory=list)
    bash_failures: int = 0
    exploration_sequences: int = 0
    bash_commands: list = dataclasses.field(default_factory=list)
    phase_transitions: list = dataclasses.field(default_factory=list)

    @property
    def total_tool_calls(self) -> int:
        return sum(self.tool_counts.values())


@dataclasses.dataclass
class PhaseTransition:
    """A detected phase transition from TaskUpdate patterns."""
    phase: str             # e.g., "Phase 2", "Phase 7"
    timestamp: Optional[datetime] = None
    iteration: Optional[int] = None  # FL iteration if applicable
    duration_minutes: float = 0.0    # Duration from previous transition


@dataclasses.dataclass
class FLFix:
    """A single FL fix entry parsed from Review Notes."""
    tag: str           # "fix", "pending", "resolved-applied", etc.
    phase: str         # "Phase2-Review", "Phase1-RefCheck", etc.
    iteration: int
    location: str
    description: str
    category: str      # classified category


# =============================================================================
# Session Discovery
# =============================================================================


def find_ccs_dirs(override: Optional[str] = None) -> list:
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

    cwd_name = Path.cwd().name
    cwd_slug = cwd_name.replace("\\", "-").replace("/", "-").replace(":", "-")
    matched = [c for c in candidates if cwd_slug in c.name or cwd_name in c.name]
    return matched if matched else candidates


def _parse_iso(ts: str) -> Optional[datetime]:
    """Parse ISO 8601 timestamp to aware datetime. Returns None on failure."""
    if not ts:
        return None
    try:
        return datetime.fromisoformat(ts.replace("Z", "+00:00"))
    except (ValueError, AttributeError):
        return None


def _detect_session_type(jsonl_path: Path, feature_id: str) -> Optional[str]:
    """
    Detect what type of session this is (fc/fl/run) by scanning first 30 lines.
    Returns "fc", "fl", "run", or None if not related to this feature.
    """
    patterns = {
        "fc": [
            f"<command-name>/fc</command-name>",
            f"<command-args>{feature_id}</command-args>",
            f"/fc {feature_id}",
            f"/fc\n{feature_id}",
        ],
        "fl": [
            f"<command-name>/fl</command-name>",
            f"/fl {feature_id}",
        ],
        "run": [
            f"<command-name>/run</command-name>",
            f"/run {feature_id}",
        ],
    }

    # Also look for generic feature references
    feature_ref_patterns = [
        f"feature-{feature_id}",
        f"F{feature_id}",
        f"feature_{feature_id}",
    ]

    found_types: set = set()
    found_feature_ref = False

    try:
        with open(jsonl_path, encoding="utf-8", errors="replace") as f:
            for i, line in enumerate(f):
                if i >= 50:
                    break
                try:
                    obj = json.loads(line)
                except json.JSONDecodeError:
                    continue

                # Serialize to string for pattern matching
                line_str = json.dumps(obj, ensure_ascii=False)

                for session_type, type_patterns in patterns.items():
                    for pat in type_patterns:
                        if pat in line_str:
                            found_types.add(session_type)

                for ref in feature_ref_patterns:
                    if ref in line_str:
                        found_feature_ref = True
    except OSError:
        return None

    # Return the most specific match
    if found_types:
        # Prefer order: fc > fl > run
        for t in ("fc", "fl", "run"):
            if t in found_types:
                return t

    return None


def find_feature_sessions(
    feature_id: str,
    ccs_dirs: list,
    max_sessions: int = 5,
) -> dict:
    """
    Scan all JSONL files to find sessions related to a feature ID.

    Returns: {"fc": [SessionInfo], "fl": [SessionInfo], "run": [SessionInfo]}
    """
    results: dict = {"fc": [], "fl": [], "run": []}

    for ccs_dir in ccs_dirs:
        jsonl_files = sorted(ccs_dir.glob("*.jsonl"), key=lambda p: p.stat().st_mtime)

        for jsonl_path in jsonl_files:
            session_type = _detect_session_type(jsonl_path, feature_id)
            if session_type is None:
                continue

            if len(results[session_type]) >= max_sessions:
                continue

            # Get timestamps for duration
            start_time = None
            end_time = None
            total_lines = 0

            try:
                with open(jsonl_path, encoding="utf-8", errors="replace") as f:
                    for line in f:
                        total_lines += 1
                        try:
                            obj = json.loads(line)
                        except json.JSONDecodeError:
                            continue
                        ts = _parse_iso(obj.get("timestamp", ""))
                        if ts:
                            if start_time is None:
                                start_time = ts
                            end_time = ts
            except OSError:
                continue

            info = SessionInfo(
                path=jsonl_path,
                session_type=session_type,
                start_time=start_time,
                end_time=end_time,
                total_lines=total_lines,
            )
            results[session_type].append(info)

    return results


# =============================================================================
# Path Normalization
# =============================================================================


def normalize_path(path_str: str) -> str:
    """Normalize file paths by stripping common prefixes and converting separators."""
    if not path_str:
        return path_str
    normalized = path_str.replace("\\", "/")
    for prefix in _PATH_PREFIXES:
        prefix_normalized = prefix.replace("\\", "/")
        if normalized.startswith(prefix_normalized):
            return normalized[len(prefix_normalized):]
    return normalized


# =============================================================================
# SessionAnalyzer - Single-pass JSONL analysis
# =============================================================================


class SessionAnalyzer:
    """Analyze a single session JSONL file in a single pass."""

    def __init__(self):
        self.tool_counts: dict = collections.Counter()
        self.file_reads: dict = collections.Counter()
        self.file_writes: dict = collections.Counter()
        self.grep_patterns: dict = collections.Counter()
        self.error_count: int = 0
        self.errors: list = []
        self.agent_dispatches: list = []
        self.bash_failures: int = 0
        self.exploration_sequences: int = 0
        self.bash_commands: list = []

        self.phase_transitions: list = []

        # Internal tracking
        self._tool_id_map: dict = {}         # tool_use_id -> tool_name
        self._tool_use_id_is_error: dict = {}  # tool_use_id -> True if agent error
        self._consecutive_explore: int = 0   # running count of Grep/Glob calls
        self._last_non_explore_was_write = False
        self._last_transition_ts: Optional[datetime] = None

    def analyze(self, jsonl_path: Path) -> SessionStats:
        """Stream through JSONL and collect stats. Returns SessionStats."""
        start_time = None
        end_time = None
        total_lines = 0

        try:
            with open(jsonl_path, encoding="utf-8", errors="replace") as f:
                for line in f:
                    total_lines += 1
                    try:
                        obj = json.loads(line)
                    except json.JSONDecodeError:
                        continue

                    # Track timestamps
                    ts = _parse_iso(obj.get("timestamp", ""))
                    if ts:
                        if start_time is None:
                            start_time = ts
                        end_time = ts

                    record_type = obj.get("type", "")
                    if record_type == "assistant":
                        self._process_assistant(obj)
                    elif record_type == "user":
                        self._process_user(obj)

        except OSError as exc:
            print(f"WARNING: Cannot read {jsonl_path.name}: {exc}", file=sys.stderr)

        info = SessionInfo(
            path=jsonl_path,
            session_type="",
            start_time=start_time,
            end_time=end_time,
            total_lines=total_lines,
        )

        return SessionStats(
            session_info=info,
            tool_counts=dict(self.tool_counts),
            file_reads=dict(self.file_reads),
            file_writes=dict(self.file_writes),
            grep_patterns=dict(self.grep_patterns),
            error_count=self.error_count,
            errors=list(self.errors),
            agent_dispatches=list(self.agent_dispatches),
            bash_failures=self.bash_failures,
            exploration_sequences=self.exploration_sequences,
            bash_commands=list(self.bash_commands),
            phase_transitions=list(self.phase_transitions),
        )

    def _process_assistant(self, obj: dict) -> None:
        """Process an assistant message block."""
        message = obj.get("message", {})
        content = message.get("content", []) if isinstance(message, dict) else []
        if not isinstance(content, list):
            return

        for block in content:
            if not isinstance(block, dict):
                continue
            if block.get("type") != "tool_use":
                continue

            tool_name = block.get("name", "")
            tool_id = block.get("id", "")
            inp = block.get("input", {}) or {}

            # Track tool_use_id -> tool_name for result correlation
            if tool_id and tool_name:
                self._tool_id_map[tool_id] = tool_name

            self.tool_counts[tool_name] += 1

            # Track exploration sequences
            if tool_name in ("Grep", "Glob"):
                self._consecutive_explore += 1
                if self._consecutive_explore >= 5:
                    # Count the start of each new sequence reaching 5
                    if self._consecutive_explore == 5:
                        self.exploration_sequences += 1
            else:
                self._consecutive_explore = 0

            # Tool-specific extraction
            if tool_name == "Read":
                path = inp.get("file_path", "")
                if path:
                    self.file_reads[normalize_path(path)] += 1

            elif tool_name in ("Write", "Edit"):
                path = inp.get("file_path", "")
                if path:
                    self.file_writes[normalize_path(path)] += 1

            elif tool_name == "Grep":
                pattern = inp.get("pattern", "")
                path = inp.get("path", "")
                if pattern:
                    key = f"{pattern}|{normalize_path(path)}" if path else pattern
                    self.grep_patterns[key] += 1

            elif tool_name in ("Agent", "Task"):
                dispatch = {
                    "subagent_type": inp.get("subagent_type", ""),
                    "model": inp.get("model", ""),
                    "description": inp.get("description", "")[:120],
                    "tool_id": tool_id,
                }
                self.agent_dispatches.append(dispatch)

            elif tool_name == "Bash":
                cmd = inp.get("command", "")
                if cmd:
                    self.bash_commands.append(cmd)

            elif tool_name == "TaskUpdate":
                subject = inp.get("subject", "")
                self._detect_phase_transition(subject, obj)

    # Patterns for phase transitions
    _FL_PHASE_RE = re.compile(r"Iteration (\d+)/\d+: Phase (\d+)")
    _RUN_PHASE_RE = re.compile(r"Phase (\d+)")

    def _detect_phase_transition(self, subject: str, obj: dict) -> None:
        """Detect phase transitions from TaskUpdate subject patterns."""
        ts = _parse_iso(obj.get("timestamp", ""))

        # FL pattern: "Iteration N/10: Phase X"
        m = self._FL_PHASE_RE.search(subject)
        if m:
            iteration = int(m.group(1))
            phase = f"Phase {m.group(2)}"
            duration = 0.0
            if self._last_transition_ts and ts:
                duration = (ts - self._last_transition_ts).total_seconds() / 60.0
            self.phase_transitions.append(PhaseTransition(
                phase=phase, timestamp=ts, iteration=iteration,
                duration_minutes=round(duration, 1),
            ))
            if ts:
                self._last_transition_ts = ts
            return

        # /run pattern: "Phase N: ..." (without "Iteration")
        m = self._RUN_PHASE_RE.search(subject)
        if m and "Iteration" not in subject:
            phase = f"Phase {m.group(1)}"
            duration = 0.0
            if self._last_transition_ts and ts:
                duration = (ts - self._last_transition_ts).total_seconds() / 60.0
            self.phase_transitions.append(PhaseTransition(
                phase=phase, timestamp=ts, iteration=None,
                duration_minutes=round(duration, 1),
            ))
            if ts:
                self._last_transition_ts = ts

    def _process_user(self, obj: dict) -> None:
        """Process a user message block (tool results)."""
        message = obj.get("message", {})
        content = message.get("content", []) if isinstance(message, dict) else []
        if not isinstance(content, list):
            return

        for block in content:
            if not isinstance(block, dict):
                continue
            if block.get("type") != "tool_result":
                continue

            is_error = block.get("is_error", False)
            tool_use_id = block.get("tool_use_id", "")
            tool_name = self._tool_id_map.get(tool_use_id, "")

            if is_error:
                self.error_count += 1
                # Extract error summary
                raw_content = block.get("content", "")
                if isinstance(raw_content, list):
                    text_parts = [
                        b.get("text", "") for b in raw_content
                        if isinstance(b, dict)
                    ]
                    error_text = " ".join(text_parts)
                else:
                    error_text = str(raw_content)

                if len(self.errors) < 10:
                    summary = f"[{tool_name}] {error_text[:100]}" if tool_name else error_text[:100]
                    self.errors.append(summary)

                # Track bash failures specifically
                if tool_name == "Bash":
                    self.bash_failures += 1


def analyze_sessions(sessions: list, verbose: bool = False) -> list:
    """Analyze all sessions in the list and return list of SessionStats."""
    stats_list = []
    for session_info in sessions:
        analyzer = SessionAnalyzer()
        stats = analyzer.analyze(session_info.path)
        # Copy timing/type from session_info
        stats.session_info.start_time = session_info.start_time
        stats.session_info.end_time = session_info.end_time
        stats.session_info.session_type = session_info.session_type
        stats.session_info.total_lines = session_info.total_lines
        stats_list.append(stats)
    return stats_list


def merge_stats(stats_list: list) -> SessionStats:
    """Merge multiple SessionStats into one aggregate."""
    merged = SessionStats(
        session_info=SessionInfo(path=Path("."), session_type=""),
    )
    for s in stats_list:
        for tool, count in s.tool_counts.items():
            merged.tool_counts[tool] = merged.tool_counts.get(tool, 0) + count
        for path, count in s.file_reads.items():
            merged.file_reads[path] = merged.file_reads.get(path, 0) + count
        for path, count in s.file_writes.items():
            merged.file_writes[path] = merged.file_writes.get(path, 0) + count
        for pat, count in s.grep_patterns.items():
            merged.grep_patterns[pat] = merged.grep_patterns.get(pat, 0) + count
        merged.error_count += s.error_count
        merged.errors.extend(s.errors[:3])  # Sample from each session
        merged.agent_dispatches.extend(s.agent_dispatches)
        merged.bash_failures += s.bash_failures
        merged.exploration_sequences += s.exploration_sequences
        merged.bash_commands.extend(s.bash_commands)
    return merged


# =============================================================================
# FLFixParser
# =============================================================================

# Pattern: - [tag] PhaseX-Type iterN: Location | Description
_FL_FIX_PATTERN = re.compile(
    r"^\s*-\s+\[([^\]]+)\]\s+(Phase\d+-\w+)\s+iter(\d+):\s*([^|]+?)\s*\|\s*(.+?)\s*$",
    re.IGNORECASE,
)


def classify_fl_fix(description: str) -> str:
    """Classify an FL fix description into a category."""
    desc = description.lower()

    if re.search(r"added ac#|ac.*coverage", desc):
        return "ac-gap"
    if re.search(r"stale|aligned|fixed stale", desc):
        return "stale-reference"
    if re.search(r"missing.*section|missing.*table|template", desc):
        return "template-compliance"
    if re.search(r"tbd|destination|mandatory handoff", desc):
        return "deferred-obligation"
    if re.search(r"removed", desc):
        return "scope-reduction"
    if re.search(r"translated", desc):
        return "language-policy"
    if re.search(r"moved|reorganiz", desc):
        return "structural-reorganization"
    if re.search(r"updated|revised|fixed", desc):
        return "content-correction"
    return "other"


def parse_fl_fixes(feature_id: str) -> list:
    """
    Parse the Review Notes section from pm/features/feature-{ID}.md.
    Returns list of FLFix objects.
    """
    feature_path = FEATURES_DIR / f"feature-{feature_id}.md"
    if not feature_path.exists():
        return []

    fixes = []
    in_review_notes = False

    try:
        with open(feature_path, encoding="utf-8", errors="replace") as f:
            for line in f:
                stripped = line.rstrip()

                # Detect section start/end
                if re.match(r"^##\s+Review Notes", stripped):
                    in_review_notes = True
                    continue
                if in_review_notes and re.match(r"^##\s+", stripped):
                    break  # End of section

                if not in_review_notes:
                    continue

                m = _FL_FIX_PATTERN.match(stripped)
                if m:
                    tag = m.group(1).strip()
                    phase = m.group(2).strip()
                    iteration = int(m.group(3))
                    location = m.group(4).strip()
                    description = m.group(5).strip()
                    category = classify_fl_fix(description)
                    fixes.append(FLFix(
                        tag=tag,
                        phase=phase,
                        iteration=iteration,
                        location=location,
                        description=description,
                        category=category,
                    ))
    except OSError:
        pass

    return fixes


# =============================================================================
# TediumDetector
# =============================================================================


@dataclasses.dataclass
class TediumIssue:
    issue_type: str
    severity: str   # "high" or "medium"
    details: str
    count: int


def detect_tedium(
    stats_by_type: dict,
    all_stats_list: list,
) -> list:
    """
    Detect tedious patterns across sessions.
    stats_by_type: {"fc": [SessionStats], "fl": [...], "run": [...]}
    all_stats_list: flat list of all SessionStats
    Returns list of TediumIssue sorted by severity.
    """
    issues = []

    # Per-session checks
    for stats in all_stats_list:
        session_label = f"{stats.session_info.session_type}:{stats.session_info.path.stem[:8]}"

        # Same file read > 3 times in single session
        for path, count in stats.file_reads.items():
            if count > 3:
                issues.append(TediumIssue(
                    issue_type="repeated-file-read",
                    severity="high",
                    details=f"{path} ({session_label})",
                    count=count,
                ))

        # Same grep pattern > 2 times in single session
        for pattern, count in stats.grep_patterns.items():
            if count > 2:
                issues.append(TediumIssue(
                    issue_type="repeated-grep",
                    severity="medium",
                    details=f"`{pattern[:60]}` ({session_label})",
                    count=count,
                ))

        # Bash failures
        if stats.bash_failures > 0:
            issues.append(TediumIssue(
                issue_type="bash-failures",
                severity="medium",
                details=f"{session_label}: {stats.bash_failures} failed Bash calls",
                count=stats.bash_failures,
            ))

        # Agent errors
        agent_errors = sum(
            1 for d in stats.agent_dispatches
            if d.get("had_error", False)
        )
        if agent_errors > 0:
            issues.append(TediumIssue(
                issue_type="agent-errors",
                severity="medium",
                details=f"{session_label}: {agent_errors} agent errors",
                count=agent_errors,
            ))

        # Exploration sequences
        if stats.exploration_sequences > 0:
            issues.append(TediumIssue(
                issue_type="exploration-sequences",
                severity="medium",
                details=f"{session_label}: {stats.exploration_sequences} long Grep/Glob sequences",
                count=stats.exploration_sequences,
            ))

    # Cross-session: same file read > 10 times total
    total_reads: dict = collections.Counter()
    for stats in all_stats_list:
        for path, count in stats.file_reads.items():
            total_reads[path] += count

    for path, total in total_reads.items():
        if total > 10:
            issues.append(TediumIssue(
                issue_type="cross-session-repeated-read",
                severity="high",
                details=f"{path} (across all sessions)",
                count=total,
            ))

    # Sort: high first, then by count descending
    severity_order = {"high": 0, "medium": 1}
    issues.sort(key=lambda x: (severity_order.get(x.severity, 2), -x.count))

    return issues


# =============================================================================
# Tool Usage Audit
# =============================================================================


# CLI tool patterns: tool_name -> (bash_pattern, description)
CLI_TOOLS = {
    "feature-status.py deps": "feature-status.py deps",
    "feature-status.py set": "feature-status.py set",
    "feature-status.py sync": "feature-status.py sync",
    "ac_ops.py ac-check": "ac_ops.py ac-check",
    "ac_ops.py ac-renumber": "ac_ops.py ac-renumber",
    "ac_ops.py ac-insert": "ac_ops.py ac-insert",
    "session-search.py": "session-search.py",
}


@dataclasses.dataclass
class ToolUsageIssue:
    tool: str
    session: str
    manual_calls: int
    suggestion: str


def detect_tool_usage(all_stats_list: list) -> list:
    """
    Detect CLI tools that were NOT used when manual alternatives were observed.
    Returns list of ToolUsageIssue.
    """
    issues = []

    for stats in all_stats_list:
        session_label = f"{stats.session_info.session_type}:{stats.session_info.path.stem[:8]}"
        all_bash = " ".join(stats.bash_commands)

        # Status manual grep (feature-status.py deps/query not used)
        status_greps = sum(
            1 for p in stats.grep_patterns
            if "Status:" in p and "feature-" in p
        )
        if status_greps >= 3 and "feature-status.py" not in all_bash:
            issues.append(ToolUsageIssue(
                tool="feature-status.py deps/query",
                session=session_label,
                manual_calls=status_greps,
                suggestion="Status確認にfeature-status.py deps/queryを使用",
            ))

        # AC manual edit (ac_ops.py not used)
        ac_edits = sum(
            1 for p in stats.file_writes
            if "feature-" in p and stats.file_writes[p] > 2
        )
        ac_greps = sum(
            1 for p in stats.grep_patterns
            if "AC#" in p or "ac_" in p.lower()
        )
        if (ac_edits >= 2 or ac_greps >= 3) and "ac_ops.py" not in all_bash:
            manual_count = ac_edits + ac_greps
            issues.append(ToolUsageIssue(
                tool="ac_ops.py",
                session=session_label,
                manual_calls=manual_count,
                suggestion="AC操作にac_ops.pyを使用 (ac-check, ac-renumber, ac-insert)",
            ))

        # JSONL manual parsing (session-search.py not used)
        jsonl_greps = sum(
            1 for p in stats.grep_patterns
            if ".jsonl" in p
        )
        jsonl_bash = sum(
            1 for cmd in stats.bash_commands
            if "jsonl" in cmd.lower() and "session-search" not in cmd
        )
        if (jsonl_greps >= 2 or jsonl_bash >= 1) and "session-search.py" not in all_bash:
            issues.append(ToolUsageIssue(
                tool="session-search.py",
                session=session_label,
                manual_calls=jsonl_greps + jsonl_bash,
                suggestion="JSONL調査にsession-search.pyを使用",
            ))

    return issues


# =============================================================================
# ReportGenerator
# =============================================================================


def _dur_str(sessions: list) -> str:
    """Format total duration as minutes."""
    total = sum(s.duration_minutes for s in sessions)
    return f"{total:.0f}"


def _session_type_totals(stats_list: list) -> tuple:
    """Return (session_count, duration_min_str, total_tool_calls)."""
    n = len(stats_list)
    total_dur = sum(s.session_info.duration_minutes for s in stats_list)
    total_calls = sum(s.total_tool_calls for s in stats_list)
    return n, f"{total_dur:.0f}", total_calls


def generate_markdown_report(
    feature_id: str,
    session_map: dict,
    all_stats: dict,
    fl_fixes: list,
    tedium: list,
    verbose: bool = False,
) -> str:
    """Generate the structured markdown improvement report."""
    lines = []

    lines.append(f"# /imp Analysis: F{feature_id}")
    lines.append("")

    # --- Session Summary ---
    lines.append("## Session Summary")
    lines.append("| Type | Sessions | Duration (min) | Tool Calls |")
    lines.append("|------|:--------:|:--------------:|:----------:|")

    for stype in ("fc", "fl", "run"):
        stats_list = all_stats.get(stype, [])
        sessions = session_map.get(stype, [])
        if not stats_list:
            lines.append(f"| {stype.upper()} | 0 | - | - |")
            continue
        n, dur, calls = _session_type_totals(stats_list)
        lines.append(f"| {stype.upper()} | {n} | {dur} | {calls} |")

    lines.append("")

    # --- FC Prevention Analysis ---
    lines.append("## 1. FC Prevention Analysis (FC予防分析)")

    if not fl_fixes:
        lines.append("_No FL fix data found in feature file._")
        lines.append("")
    else:
        # Group by category
        category_fixes: dict = collections.defaultdict(list)
        for fix in fl_fixes:
            category_fixes[fix.category].append(fix)

        lines.append("| Category | FL Fixes | FC Phase | Prevention Rule |")
        lines.append("|----------|:--------:|----------|-----------------|")

        for category, fixes in sorted(category_fixes.items(), key=lambda x: -len(x[1])):
            fc_phase, rule = FC_PREVENTION_MAP.get(category, ("(該当なし)", "パターン分析要"))
            count = len(fixes)
            lines.append(f"| {category} | {count} | {fc_phase} | {rule} |")

        lines.append("")
        lines.append("**Top examples per category:**")
        lines.append("")

        for category, fixes in sorted(category_fixes.items(), key=lambda x: -len(x[1])):
            lines.append(f"**{category}** ({len(fixes)} fixes):")
            for fix in fixes[:3]:
                tag_str = f"[{fix.tag}]"
                lines.append(f"- {tag_str} {fix.phase} iter{fix.iteration}: {fix.location} | {fix.description}")
            lines.append("")

    # --- Tool/Script Optimization ---
    lines.append("## 2. Tool/Script Optimization (ツール最適化)")

    # Merge all stats
    all_flat = []
    for stype in ("fc", "fl", "run"):
        all_flat.extend(all_stats.get(stype, []))

    if not all_flat:
        lines.append("_No session data found._")
        lines.append("")
    else:
        merged = merge_stats(all_flat)

        # Top 10 tools
        lines.append("**Top tools by call count:**")
        lines.append("")
        lines.append("| Tool | Calls | % of Total |")
        lines.append("|------|:-----:|:----------:|")

        total_calls = merged.total_tool_calls or 1
        top_tools = sorted(merged.tool_counts.items(), key=lambda x: -x[1])[:10]
        for tool, count in top_tools:
            pct = count / total_calls * 100
            lines.append(f"| {tool} | {count} | {pct:.1f}% |")

        lines.append("")

        # Top 5 most-read files
        if merged.file_reads:
            lines.append("**Top 5 most-read files:**")
            lines.append("")
            lines.append("| File | Read Count |")
            lines.append("|------|:----------:|")
            top_reads = sorted(merged.file_reads.items(), key=lambda x: -x[1])[:5]
            for path, count in top_reads:
                lines.append(f"| `{path}` | {count} |")
            lines.append("")

        # Top 5 repeated grep patterns
        if merged.grep_patterns:
            lines.append("**Top 5 repeated grep patterns:**")
            lines.append("")
            lines.append("| Pattern | Count |")
            lines.append("|---------|:-----:|")
            top_greps = sorted(merged.grep_patterns.items(), key=lambda x: -x[1])[:5]
            for pat, count in top_greps:
                display_pat = pat[:70] + "..." if len(pat) > 70 else pat
                lines.append(f"| `{display_pat}` | {count} |")
            lines.append("")

    # --- Workflow Improvements ---
    lines.append("## 3. Workflow Improvements (ワークフロー改善)")

    if fl_fixes:
        # FL iteration analysis
        iterations_by_phase: dict = collections.defaultdict(set)
        fix_count_by_phase: dict = collections.Counter()

        for fix in fl_fixes:
            iterations_by_phase[fix.phase].add(fix.iteration)
            fix_count_by_phase[fix.phase] += 1

        total_iterations = max(
            (max(iters) for iters in iterations_by_phase.values()),
            default=0
        )

        lines.append("| Metric | Value | Observation |")
        lines.append("|--------|:-----:|-------------|")
        lines.append(f"| Total FL iterations | {total_iterations} | {'High iteration count - consider pre-FL checks' if total_iterations > 3 else 'Normal range'} |")
        lines.append(f"| Total FL fixes | {len(fl_fixes)} | {'Many fixes - FC quality improvement recommended' if len(fl_fixes) > 10 else 'Moderate'} |")
        lines.append(f"| Phases with fixes | {len(fix_count_by_phase)} | {', '.join(sorted(fix_count_by_phase.keys()))} |")

        # Phase distribution
        if fix_count_by_phase:
            top_phase = fix_count_by_phase.most_common(1)[0]
            lines.append(f"| Most fixes in phase | {top_phase[0]} | {top_phase[1]} fixes |")

        lines.append("")

        # Pending fixes
        pending = [f for f in fl_fixes if f.tag == "pending"]
        if pending:
            lines.append(f"**Pending fixes ({len(pending)}):**")
            for fix in pending[:5]:
                lines.append(f"- [{fix.tag}] {fix.phase} iter{fix.iteration}: {fix.location} | {fix.description}")
            lines.append("")
    else:
        lines.append("_No FL fix data available for workflow analysis._")
        lines.append("")

    # --- Phase Timing Analysis ---
    lines.append("## 4. Phase Timing Analysis (Phase間タイミング)")

    # Collect all phase transitions across sessions
    all_transitions = []
    for stype in ("fc", "fl", "run"):
        for stats in all_stats.get(stype, []):
            if stats.phase_transitions:
                session_label = f"{stats.session_info.session_type}:{stats.session_info.path.stem[:8]}"
                for pt in stats.phase_transitions:
                    all_transitions.append((session_label, pt))

    if not all_transitions:
        lines.append("_No phase transitions detected in session data._")
        lines.append("")
    else:
        lines.append("| Session | Phase | Iteration | Duration (min) |")
        lines.append("|---------|-------|:---------:|:--------------:|")
        for session_label, pt in all_transitions:
            iter_str = str(pt.iteration) if pt.iteration is not None else "-"
            dur_str = f"{pt.duration_minutes:.1f}" if pt.duration_minutes > 0 else "-"
            lines.append(f"| {session_label} | {pt.phase} | {iter_str} | {dur_str} |")
        lines.append("")

        # Phase duration summary (average per phase)
        phase_durations: dict = collections.defaultdict(list)
        for _, pt in all_transitions:
            if pt.duration_minutes > 0:
                phase_durations[pt.phase].append(pt.duration_minutes)

        if phase_durations:
            lines.append("**Average duration per phase:**")
            lines.append("")
            lines.append("| Phase | Avg (min) | Max (min) | Count |")
            lines.append("|-------|:---------:|:---------:|:-----:|")
            for phase in sorted(phase_durations.keys(), key=lambda x: int(x.split()[-1]) if x.split()[-1].isdigit() else 99):
                durations = phase_durations[phase]
                avg = sum(durations) / len(durations)
                mx = max(durations)
                lines.append(f"| {phase} | {avg:.1f} | {mx:.1f} | {len(durations)} |")
            lines.append("")

    # --- Tedium Reduction ---
    lines.append("## 5. Tedium Reduction (手間削減)")

    if not tedium:
        lines.append("_No significant tedium patterns detected._")
        lines.append("")
    else:
        lines.append("| Type | Severity | Details | Count |")
        lines.append("|------|:--------:|---------|:-----:|")
        for issue in tedium[:15]:  # Cap at 15 rows
            sev_display = issue.severity.upper()
            lines.append(f"| {issue.issue_type} | {sev_display} | {issue.details[:60]} | {issue.count} |")
        lines.append("")

    # --- Tool Usage Audit ---
    lines.append("## 6. Tool Usage Audit (CLIツール活用監査)")
    lines.append("")

    # Compute tool usage summary
    all_flat_for_audit = []
    for stype in ("fc", "fl", "run"):
        all_flat_for_audit.extend(all_stats.get(stype, []))

    tool_usage_issues = detect_tool_usage(all_flat_for_audit)

    # Build used-tools set from all bash commands
    all_bash_cmds = []
    for stats in all_flat_for_audit:
        all_bash_cmds.extend(stats.bash_commands)
    all_bash_str = " ".join(all_bash_cmds)

    lines.append("| Tool | Used | Manual Alternative Detected | Sessions | Suggestion |")
    lines.append("|------|:----:|:---------------------------:|:--------:|------------|")

    # Group issues by tool
    issue_by_tool: dict = {}
    for issue in tool_usage_issues:
        if issue.tool not in issue_by_tool:
            issue_by_tool[issue.tool] = []
        issue_by_tool[issue.tool].append(issue)

    for tool_name, bash_pattern in CLI_TOOLS.items():
        used = bash_pattern in all_bash_str
        tool_issues = issue_by_tool.get(tool_name, [])
        if tool_issues:
            sessions_str = ", ".join(i.session for i in tool_issues)
            total_manual = sum(i.manual_calls for i in tool_issues)
            suggestion = tool_issues[0].suggestion
            lines.append(f"| {tool_name} | {'Yes' if used else 'No'} | {total_manual} calls | {sessions_str} | {suggestion} |")
        else:
            lines.append(f"| {tool_name} | {'Yes' if used else '-'} | - | - | - |")

    lines.append("")

    # --- Verbose per-session breakdown ---
    if verbose:
        lines.append("## Appendix: Per-Session Breakdown")
        lines.append("")
        for stype in ("fc", "fl", "run"):
            stats_list = all_stats.get(stype, [])
            if not stats_list:
                continue
            lines.append(f"### {stype.upper()} Sessions")
            lines.append("")
            for i, stats in enumerate(stats_list, 1):
                info = stats.session_info
                dur = f"{info.duration_minutes:.1f} min"
                lines.append(f"**Session {i}**: `{info.path.stem}` | {dur} | {stats.total_tool_calls} calls")
                if stats.agent_dispatches:
                    lines.append(f"  - Agent dispatches: {len(stats.agent_dispatches)}")
                    for d in stats.agent_dispatches[:3]:
                        lines.append(f"    - [{d.get('subagent_type', '?')}] {d.get('description', '')[:60]}")
                lines.append("")

    return "\n".join(lines)


# =============================================================================
# JSON Output
# =============================================================================


def generate_json_output(
    feature_id: str,
    session_map: dict,
    all_stats: dict,
    fl_fixes: list,
    tedium: list,
) -> str:
    """Generate JSON output for programmatic consumption."""

    def stats_to_dict(stats: SessionStats) -> dict:
        return {
            "path": str(stats.session_info.path),
            "duration_minutes": round(stats.session_info.duration_minutes, 1),
            "total_tool_calls": stats.total_tool_calls,
            "tool_counts": stats.tool_counts,
            "top_files_read": dict(sorted(stats.file_reads.items(), key=lambda x: -x[1])[:10]),
            "top_files_written": dict(sorted(stats.file_writes.items(), key=lambda x: -x[1])[:5]),
            "error_count": stats.error_count,
            "bash_failures": stats.bash_failures,
            "exploration_sequences": stats.exploration_sequences,
            "agent_dispatches": len(stats.agent_dispatches),
            "phase_transitions": [
                {
                    "phase": pt.phase,
                    "iteration": pt.iteration,
                    "duration_minutes": pt.duration_minutes,
                }
                for pt in stats.phase_transitions
            ],
        }

    result = {
        "feature_id": feature_id,
        "sessions": {
            stype: [stats_to_dict(s) for s in stats_list]
            for stype, stats_list in all_stats.items()
        },
        "fl_fixes": [
            {
                "tag": f.tag,
                "phase": f.phase,
                "iteration": f.iteration,
                "location": f.location,
                "description": f.description,
                "category": f.category,
            }
            for f in fl_fixes
        ],
        "fl_fix_summary": dict(
            collections.Counter(f.category for f in fl_fixes)
        ),
        "tedium_issues": [
            {
                "type": t.issue_type,
                "severity": t.severity,
                "details": t.details,
                "count": t.count,
            }
            for t in tedium
        ],
        "tool_usage_audit": [
            {
                "tool": t.tool,
                "session": t.session,
                "manual_calls": t.manual_calls,
                "suggestion": t.suggestion,
            }
            for t in detect_tool_usage(
                [s for stype in all_stats.values() for s in stype]
            )
        ],
    }

    return json.dumps(result, indent=2, ensure_ascii=False)


# =============================================================================
# CLI
# =============================================================================


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Analyze feature FC/FL/RUN session lifecycle and generate improvement report.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python src/tools/python/imp-analyzer.py 813
  python src/tools/python/imp-analyzer.py 813 --verbose
  python src/tools/python/imp-analyzer.py 813 --json
  python src/tools/python/imp-analyzer.py 813 --max-sessions 10
        """,
    )
    parser.add_argument(
        "feature_id",
        help="Feature ID (numeric, e.g., 813)",
    )
    parser.add_argument(
        "--json",
        action="store_true",
        help="Output as JSON instead of markdown",
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Include per-session breakdowns in the report",
    )
    parser.add_argument(
        "--max-sessions",
        type=int,
        default=5,
        metavar="N",
        help="Max sessions per type to analyze (default: 5)",
    )
    parser.add_argument(
        "--ccs-dir",
        metavar="PATH",
        help="Override CCS directory path",
    )
    return parser


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()

    feature_id = args.feature_id.strip().lstrip("fF")  # Accept "813" or "F813"
    if not feature_id.isdigit():
        print(f"ERROR: feature_id must be numeric, got: {args.feature_id}", file=sys.stderr)
        return 1

    # 1. Find CCS dirs
    ccs_dirs = find_ccs_dirs(args.ccs_dir)

    # 2. Find sessions for this feature
    print(f"Scanning sessions for F{feature_id}...", file=sys.stderr)
    session_map = find_feature_sessions(feature_id, ccs_dirs, max_sessions=args.max_sessions)

    total_sessions = sum(len(v) for v in session_map.values())
    if total_sessions == 0:
        print(f"WARNING: No sessions found for feature {feature_id}.", file=sys.stderr)
        print("Try --ccs-dir to specify the CCS directory manually.", file=sys.stderr)

    print(
        f"Found: FC={len(session_map['fc'])} FL={len(session_map['fl'])} RUN={len(session_map['run'])} sessions",
        file=sys.stderr,
    )

    # 3. Analyze each session
    all_stats: dict = {}
    for stype in ("fc", "fl", "run"):
        all_stats[stype] = analyze_sessions(session_map[stype], verbose=args.verbose)

    # 4. Parse FL fixes
    print(f"Parsing FL fixes from feature-{feature_id}.md...", file=sys.stderr)
    fl_fixes = parse_fl_fixes(feature_id)
    print(f"Found {len(fl_fixes)} FL fix entries.", file=sys.stderr)

    # 5. Detect tedium
    all_flat = []
    for stype in ("fc", "fl", "run"):
        all_flat.extend(all_stats[stype])

    tedium = detect_tedium(all_stats, all_flat)

    # 6. Generate output
    if args.json:
        output = generate_json_output(
            feature_id=feature_id,
            session_map=session_map,
            all_stats=all_stats,
            fl_fixes=fl_fixes,
            tedium=tedium,
        )
    else:
        output = generate_markdown_report(
            feature_id=feature_id,
            session_map=session_map,
            all_stats=all_stats,
            fl_fixes=fl_fixes,
            tedium=tedium,
            verbose=args.verbose,
        )

    print(output)
    return 0


if __name__ == "__main__":
    sys.exit(main())
