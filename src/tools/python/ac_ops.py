#!/usr/bin/env python3
"""
AC Manipulation Operations

Provides parser, transform, and command layers for manipulating
Acceptance Criteria in feature-{ID}.md files.

Used by feature-status.py subcommands: ac-check, ac-renumber, ac-insert, ac-delete, ac-fix, ac-link, ac-update, ac-add.
"""

import dataclasses
import re
import shutil
import sys
from enum import Enum, auto
from pathlib import Path

# --- Constants ---
REPO_ROOT = Path(__file__).resolve().parent.parent.parent.parent
PLANNING_DIR = REPO_ROOT / "pm"
AGENTS_DIR = PLANNING_DIR / "features"

VALID_MATCHERS = frozenset({
    "equals", "contains", "not_contains", "matches", "not_matches",
    "succeeds", "fails", "gt", "gte", "lt", "lte",
    "count_equals", "exists", "not_exists",
})
THRESHOLD_MATCHERS = frozenset({"gte", "gt", "lt", "lte", "count_equals"})

_MATCHER_FIX_MAP = {
    "count_gte": "gte",
    "count_gt": "gt",
    "count_lt": "lt",
    "count_lte": "lte",
}


def feature_path(fid: str) -> Path:
    return AGENTS_DIR / f"feature-{fid}.md"


# =============================================================================
# Parser Layer
# =============================================================================


def split_pipe_row(line: str) -> list[str]:
    """
    Split a markdown pipe-table row into cell strings, respecting quoted regions.
    Handles in_quotes (double-quote) and in_backticks state machines.
    Escaped pipes (odd backslashes before |) are included as content.
    Returns a list of stripped cell contents.
    """
    parts = []
    current_part = ""
    in_quotes = False
    in_backticks = False
    i = 0
    while i < len(line):
        char = line[i]
        if char == '"' and not in_backticks:
            # Count consecutive backslashes before the quote
            num_backslashes = 0
            j = i - 1
            while j >= 0 and line[j] == '\\':
                num_backslashes += 1
                j -= 1
            # Even number of backslashes means quote is NOT escaped
            if num_backslashes % 2 == 0:
                in_quotes = not in_quotes
            current_part += char
        elif char == '`' and not in_quotes:
            in_backticks = not in_backticks
            current_part += char
        elif char == '|' and not in_quotes and not in_backticks:
            # Check if pipe is backslash-escaped
            num_backslashes = 0
            j = i - 1
            while j >= 0 and line[j] == '\\':
                num_backslashes += 1
                j -= 1
            if num_backslashes % 2 == 0:
                # Not escaped - split here
                parts.append(current_part.strip())
                current_part = ""
            else:
                # Escaped pipe - include as content (without the escaping backslash)
                current_part = current_part[:-1] + char
        else:
            current_part += char
        i += 1
    # Add final part (always append to preserve trailing empty string after last |)
    parts.append(current_part.strip())
    return parts


@dataclasses.dataclass
class ACRow:
    number: int
    description: str
    type_: str
    method: str
    matcher: str
    expected: str
    status: str
    line_index: int  # 0-based line index in file


def parse_ac_table(lines: list[str]) -> list[ACRow]:
    """
    Find the AC Definition Table by looking for '| AC# |' header.
    Skip the separator line. Parse each subsequent row. Stop on non-table line.
    Returns list of ACRow objects.
    """
    rows = []
    in_ac_table = False
    skip_separator = False

    for idx, line in enumerate(lines):
        stripped = line.strip()

        if not in_ac_table:
            if "| AC# |" in stripped:
                in_ac_table = True
                skip_separator = True
            continue

        if skip_separator:
            skip_separator = False
            continue

        # End of table
        if not stripped.startswith("|"):
            break
        if not stripped.strip("|").strip():
            break

        parts = split_pipe_row(stripped)
        # Expected: ['', AC#, Description, Type, Method, Matcher, Expected, Status, '']
        # parts[0] = '' (before first |), parts[1] = AC#, parts[2..7] = columns, parts[-1] = ''
        if len(parts) < 8:
            continue

        ac_num_str = parts[1].strip()
        if not ac_num_str.isdigit():
            continue

        try:
            ac_number = int(ac_num_str)
            description = parts[2].strip() if len(parts) > 2 else ""
            type_ = parts[3].strip() if len(parts) > 3 else ""
            method = parts[4].strip() if len(parts) > 4 else ""
            matcher = parts[5].strip() if len(parts) > 5 else ""
            expected = parts[6].strip() if len(parts) > 6 else ""
            status = parts[7].strip() if len(parts) > 7 else ""
            rows.append(ACRow(
                number=ac_number,
                description=description,
                type_=type_,
                method=method,
                matcher=matcher,
                expected=expected,
                status=status,
                line_index=idx,
            ))
        except (ValueError, IndexError):
            continue

    return rows


class SectionType(Enum):
    PHILOSOPHY_DERIVATION = auto()   # AC#N, AC#M format in Philosophy Derivation table
    AC_DEFINITION_TABLE = auto()     # bare N (column 1) in AC Definition Table
    AC_DETAILS = auto()              # **AC#N: desc** format in AC Details blocks
    GOAL_COVERAGE = auto()           # AC#N, AC#M format in Goal Coverage table
    TASKS_TABLE = auto()             # bare N (col 2) + AC#N in Description (col 3)
    TECH_DESIGN_AC_COV = auto()      # bare N (column 1) in Technical Design AC Coverage
    TECH_DESIGN_PROSE = auto()       # AC#N inline in Approach/Key Decisions/Upstream Issues/Interfaces
    SUCCESS_CRITERIA = auto()        # AC#N-M range format
    REVIEW_NOTES = auto()            # AC#N / ACN informal
    EXECUTION_LOG = auto()           # Historical record: AC#N refs updated, "N ACs" NOT updated
    OTHER = auto()                   # Never modified


def classify_sections(lines: list[str]) -> dict[int, SectionType]:
    """
    Maps 0-based line indices to SectionType.
    Tracks current H2 and H3 headings to determine section context.
    HTML comment lines are always classified as OTHER.
    """
    section_map = {}
    current_h2 = ""
    current_h3 = ""
    in_html_comment = False

    for idx, line in enumerate(lines):
        stripped = line.strip()

        # HTML comment detection (multi-line)
        if in_html_comment:
            section_map[idx] = SectionType.OTHER
            if "-->" in stripped:
                in_html_comment = False
            continue

        if stripped.startswith("<!--"):
            section_map[idx] = SectionType.OTHER
            if "-->" not in stripped:
                in_html_comment = True
            continue

        # Heading detection
        if stripped.startswith("## "):
            current_h2 = stripped[3:].strip()
            current_h3 = ""
            section_map[idx] = SectionType.OTHER
            continue

        if stripped.startswith("### "):
            current_h3 = stripped[4:].strip()
            section_map[idx] = SectionType.OTHER
            continue

        # Classify based on current headings
        section_type = _classify_line(stripped, current_h2, current_h3)
        section_map[idx] = section_type

    return section_map


def _classify_line(stripped: str, current_h2: str, current_h3: str) -> SectionType:
    """Helper to determine SectionType for a single line based on heading context."""
    h2 = current_h2.lower()
    h3 = current_h3.lower()

    if "acceptance criteria" in h2:
        if "philosophy derivation" in h3:
            return SectionType.PHILOSOPHY_DERIVATION
        if "ac definition table" in h3:
            return SectionType.AC_DEFINITION_TABLE
        if "ac details" in h3:
            return SectionType.AC_DETAILS
        if "goal coverage" in h3:
            return SectionType.GOAL_COVERAGE
        return SectionType.OTHER

    if h2.startswith("tasks"):
        return SectionType.TASKS_TABLE

    if "technical design" in h2:
        if "ac coverage" in h3:
            return SectionType.TECH_DESIGN_AC_COV
        # Approach, Key Decisions, Upstream Issues, Interfaces, or no H3 yet
        return SectionType.TECH_DESIGN_PROSE

    if "implementation contract" in h2:
        if "success criteria" in h3:
            return SectionType.SUCCESS_CRITERIA
        return SectionType.OTHER

    if "review notes" in h2:
        return SectionType.REVIEW_NOTES

    if "execution log" in h2:
        return SectionType.EXECUTION_LOG

    return SectionType.OTHER


@dataclasses.dataclass
class ACReference:
    line_index: int
    ac_number: int
    section_type: SectionType
    format: str   # "prefixed" (AC#N), "bare" (N), "range" (AC#N-M), "informal" (ACN)
    context: str  # the line text for reporting


def scan_ac_references(lines: list[str], section_map: dict[int, SectionType]) -> list[ACReference]:
    """
    Scan all lines for AC references based on section type.
    Returns all found references as ACReference objects.
    """
    refs = []

    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)

        if stype == SectionType.OTHER:
            continue

        if stype == SectionType.PHILOSOPHY_DERIVATION:
            for m in re.finditer(r'AC#(\d+)', line):
                refs.append(ACReference(
                    line_index=idx,
                    ac_number=int(m.group(1)),
                    section_type=stype,
                    format="prefixed",
                    context=line.rstrip(),
                ))

        elif stype == SectionType.AC_DEFINITION_TABLE:
            stripped = line.strip()
            if stripped.startswith("|") and "| AC# |" not in stripped:
                parts = split_pipe_row(stripped)
                if len(parts) > 1:
                    num_str = parts[1].strip()
                    if num_str.isdigit():
                        refs.append(ACReference(
                            line_index=idx,
                            ac_number=int(num_str),
                            section_type=stype,
                            format="bare",
                            context=line.rstrip(),
                        ))

        elif stype == SectionType.AC_DETAILS:
            for m in re.finditer(r'\*\*AC#(\d+):', line):
                refs.append(ACReference(
                    line_index=idx,
                    ac_number=int(m.group(1)),
                    section_type=stype,
                    format="prefixed",
                    context=line.rstrip(),
                ))

        elif stype == SectionType.GOAL_COVERAGE:
            for m in re.finditer(r'AC#(\d+)', line):
                refs.append(ACReference(
                    line_index=idx,
                    ac_number=int(m.group(1)),
                    section_type=stype,
                    format="prefixed",
                    context=line.rstrip(),
                ))

        elif stype == SectionType.TASKS_TABLE:
            stripped = line.strip()
            if stripped.startswith("|"):
                parts = split_pipe_row(stripped)
                # Column 2 (index 2): AC# references (bare, comma-separated)
                if len(parts) > 2:
                    ac_col = parts[2].strip()
                    if ac_col and ac_col != "AC#":
                        for token in ac_col.split(","):
                            token = token.strip()
                            if token.isdigit():
                                refs.append(ACReference(
                                    line_index=idx,
                                    ac_number=int(token),
                                    section_type=stype,
                                    format="bare",
                                    context=line.rstrip(),
                                ))
                # Column 3 (index 3): Description - may contain AC#N references
                if len(parts) > 3:
                    desc_col = parts[3]
                    for m in re.finditer(r'AC#(\d+)', desc_col):
                        refs.append(ACReference(
                            line_index=idx,
                            ac_number=int(m.group(1)),
                            section_type=stype,
                            format="prefixed",
                            context=line.rstrip(),
                        ))

        elif stype == SectionType.TECH_DESIGN_AC_COV:
            stripped = line.strip()
            if stripped.startswith("|"):
                parts = split_pipe_row(stripped)
                if len(parts) > 1:
                    num_str = parts[1].strip()
                    if num_str.isdigit():
                        refs.append(ACReference(
                            line_index=idx,
                            ac_number=int(num_str),
                            section_type=stype,
                            format="bare",
                            context=line.rstrip(),
                        ))

        elif stype == SectionType.TECH_DESIGN_PROSE:
            for m in re.finditer(r'AC#(\d+)', line):
                refs.append(ACReference(
                    line_index=idx,
                    ac_number=int(m.group(1)),
                    section_type=stype,
                    format="prefixed",
                    context=line.rstrip(),
                ))

        elif stype == SectionType.SUCCESS_CRITERIA:
            # Find ranges AC#N-M and individual AC#N refs
            for m in re.finditer(r'AC#(\d+)(?:-(\d+))?', line):
                start_num = int(m.group(1))
                if m.group(2) is not None:
                    end_num = int(m.group(2))
                    # Add all numbers in range
                    for n in range(start_num, end_num + 1):
                        refs.append(ACReference(
                            line_index=idx,
                            ac_number=n,
                            section_type=stype,
                            format="range",
                            context=line.rstrip(),
                        ))
                else:
                    refs.append(ACReference(
                        line_index=idx,
                        ac_number=start_num,
                        section_type=stype,
                        format="prefixed",
                        context=line.rstrip(),
                    ))

        elif stype in (SectionType.REVIEW_NOTES, SectionType.EXECUTION_LOG):
            # Match both AC#N and ACN (informal)
            for m in re.finditer(r'AC#?(\d+)', line):
                full_match = m.group(0)
                fmt = "informal" if not full_match.startswith("AC#") else "prefixed"
                refs.append(ACReference(
                    line_index=idx,
                    ac_number=int(m.group(1)),
                    section_type=stype,
                    format=fmt,
                    context=line.rstrip(),
                ))

    return refs


# =============================================================================
# Transform Layer
# =============================================================================


def build_renumber_map(old_numbers: list[int]) -> dict[int, int]:
    """
    Given a sorted list of current AC numbers (possibly with gaps),
    returns mapping from old to new sequential numbers.
    Example: [1,2,4,5] -> {1:1, 2:2, 4:3, 5:4}
    """
    sorted_nums = sorted(set(old_numbers))
    return {old: (new + 1) for new, old in enumerate(sorted_nums)}


def _find_unescaped_pipes(line: str) -> list[int]:
    """Return positions of unescaped pipe characters, respecting backtick/quote regions."""
    positions = []
    in_quotes = False
    in_backticks = False
    i = 0
    while i < len(line):
        char = line[i]
        if char == '"' and not in_backticks:
            num_backslashes = 0
            j = i - 1
            while j >= 0 and line[j] == '\\':
                num_backslashes += 1
                j -= 1
            if num_backslashes % 2 == 0:
                in_quotes = not in_quotes
        elif char == '`' and not in_quotes:
            in_backticks = not in_backticks
        elif char == '|' and not in_quotes and not in_backticks:
            # Check if escaped (odd number of backslashes before |)
            num_backslashes = 0
            j = i - 1
            while j >= 0 and line[j] == '\\':
                num_backslashes += 1
                j -= 1
            if num_backslashes % 2 == 0:
                positions.append(i)
        i += 1
    return positions


def _rebuild_pipe_row(original_line: str, parts: list[str], modified_parts: list[str]) -> str:
    """
    Rebuild a pipe row by replacing only changed cells using column positions.
    Uses pipe positions for direct positional replacement instead of string search.
    Preserves original line structure, escaping, and alignment.
    """
    if parts == modified_parts:
        return original_line

    # Strip trailing newline for processing, restore later
    trailing = ""
    line = original_line
    if line.endswith("\n"):
        trailing = "\n"
        line = line[:-1]

    pipe_positions = _find_unescaped_pipes(line)

    # We need at least 2 pipes to have any cells
    if len(pipe_positions) < 2:
        return original_line

    # Each cell[i] in parts corresponds to the segment between pipe_positions[i] and pipe_positions[i+1]
    # parts[0] is before first pipe (leading empty), parts[1] is between pipe 0 and pipe 1, etc.
    # parts[0] = content before first pipe or between pipe[0] and pipe[1]
    # For "| 1 | desc |", pipes are at 0, 4, 11
    # parts = ['', '1', 'desc', '']  (split_pipe_row strips content)
    # Segment between pipe[0]+1 and pipe[1] = " 1 " (cell index 1 in parts)
    # Segment between pipe[1]+1 and pipe[2] = " desc " (cell index 2 in parts)

    result_chars = list(line)
    # Process cells in reverse order so positions remain valid after replacement
    # Cell i corresponds to segment between pipe_positions[i-1]+1 and pipe_positions[i]
    # But parts includes leading empty (index 0 = before first pipe)
    # So parts[1] = between pipe[0] and pipe[1], parts[2] = between pipe[1] and pipe[2], etc.

    for cell_idx in range(min(len(parts), len(modified_parts)) - 1, 0, -1):
        if parts[cell_idx] == modified_parts[cell_idx]:
            continue

        # cell_idx in parts corresponds to segment between pipe_positions[cell_idx-1] and pipe_positions[cell_idx]
        pipe_left_idx = cell_idx - 1
        pipe_right_idx = cell_idx

        if pipe_left_idx >= len(pipe_positions) or pipe_right_idx >= len(pipe_positions):
            continue

        left_pipe = pipe_positions[pipe_left_idx]
        right_pipe = pipe_positions[pipe_right_idx]

        # The segment is line[left_pipe+1 : right_pipe]
        segment = line[left_pipe + 1 : right_pipe]
        old_stripped = segment.strip()
        new_cell = modified_parts[cell_idx]

        # Preserve padding: replace only the stripped content within the segment
        # Find where the stripped content starts and ends within the segment
        if old_stripped:
            content_start = segment.index(old_stripped)
            content_end = content_start + len(old_stripped)
            leading_space = segment[:content_start]
            trailing_space = segment[content_end:]
        else:
            # Empty cell - place new content centered with at least one space each side
            leading_space = " "
            trailing_space = " "

        new_segment = leading_space + new_cell + trailing_space
        # Replace in result_chars
        result_chars[left_pipe + 1 : right_pipe] = list(new_segment)

        # Recalculate pipe_positions since we may have shifted things
        # Adjustment for subsequent (earlier) replacements
        length_diff = len(new_segment) - (right_pipe - left_pipe - 1)
        for j in range(pipe_right_idx, len(pipe_positions)):
            pipe_positions[j] += length_diff

        # Update line to reflect current result_chars for subsequent iterations
        line = "".join(result_chars)

    return "".join(result_chars) + trailing


def _remap_number(n: int, rmap: dict[int, int]) -> int:
    """Return remapped number, or original if not in map."""
    return rmap.get(n, n)


def apply_renumber_map(
    lines: list[str],
    section_map: dict[int, SectionType],
    rmap: dict[int, int],
    update_counts: bool = True,
) -> list[str]:
    """
    Apply renumber map to all lines. Per-section update logic applied.
    Returns updated lines list.
    """
    result = list(lines)
    max_old = max(rmap.keys()) if rmap else 0
    new_count = len(rmap)

    # Regex patterns
    ac_prefixed_re = re.compile(r'AC#(\d+)(?![a-z\d])')
    ac_informal_re = re.compile(r'AC#?(\d+)(?![a-z\d])')
    ac_details_re = re.compile(r'(\*\*AC#)(\d+)(:)')
    ac_range_re = re.compile(r'AC#(\d+)(?:-(\d+))?(?![a-z\d])')
    all_n_acs_re = re.compile(r'(?i)((?:all\s+|All\s+))(\d+)(\s+ACs?)')

    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)

        if stype == SectionType.OTHER:
            continue

        if stype == SectionType.AC_DEFINITION_TABLE:
            stripped = line.strip()
            if not stripped.startswith("|") or "| AC# |" in stripped:
                continue
            parts = split_pipe_row(stripped)
            if len(parts) <= 1:
                continue
            num_str = parts[1].strip()
            if not num_str.isdigit():
                continue
            old_num = int(num_str)
            if old_num in rmap:
                new_num = rmap[old_num]
                new_parts = list(parts)
                new_parts[1] = str(new_num)
                result[idx] = _rebuild_pipe_row(line, parts, new_parts)

        elif stype == SectionType.AC_DETAILS:
            def replace_ac_details(m: re.Match) -> str:
                num = int(m.group(2))
                new_num = _remap_number(num, rmap)
                return m.group(1) + str(new_num) + m.group(3)
            result[idx] = ac_details_re.sub(replace_ac_details, line)

        elif stype in (SectionType.PHILOSOPHY_DERIVATION, SectionType.GOAL_COVERAGE):
            def replace_prefixed(m: re.Match) -> str:
                num = int(m.group(1))
                new_num = _remap_number(num, rmap)
                return f"AC#{new_num}"
            result[idx] = ac_prefixed_re.sub(replace_prefixed, line)

        elif stype == SectionType.TECH_DESIGN_PROSE:
            def replace_tech_prose(m: re.Match) -> str:
                num = int(m.group(1))
                new_num = _remap_number(num, rmap)
                return f"AC#{new_num}"
            new_line = ac_prefixed_re.sub(replace_tech_prose, line)
            # Update "All N ACs" count if requested
            if update_counts:
                def replace_all_n_acs(m: re.Match) -> str:
                    count_val = int(m.group(2))
                    if count_val == max_old:
                        return m.group(1) + str(new_count) + m.group(3)
                    return m.group(0)
                new_line = all_n_acs_re.sub(replace_all_n_acs, new_line)
            result[idx] = new_line

        elif stype == SectionType.TASKS_TABLE:
            stripped = line.strip()
            if not stripped.startswith("|"):
                continue
            parts = split_pipe_row(stripped)
            new_parts = list(parts)

            # Column 2 (index 2): bare numbers (comma-separated)
            if len(new_parts) > 2:
                ac_col = new_parts[2].strip()
                if ac_col and ac_col not in ("AC#", ""):
                    tokens = [t.strip() for t in ac_col.split(",")]
                    new_tokens = []
                    for token in tokens:
                        if token.isdigit():
                            old_num = int(token)
                            new_tokens.append(str(_remap_number(old_num, rmap)))
                        else:
                            new_tokens.append(token)
                    new_parts[2] = ", ".join(new_tokens)

            # Column 3 (index 3): description with AC#N refs
            if len(new_parts) > 3:
                def replace_task_desc(m: re.Match) -> str:
                    num = int(m.group(1))
                    new_num = _remap_number(num, rmap)
                    return f"AC#{new_num}"
                new_parts[3] = ac_prefixed_re.sub(replace_task_desc, new_parts[3])

            result[idx] = _rebuild_pipe_row(line, parts, new_parts)

        elif stype == SectionType.TECH_DESIGN_AC_COV:
            stripped = line.strip()
            if not stripped.startswith("|"):
                continue
            parts = split_pipe_row(stripped)
            if len(parts) <= 1:
                continue
            num_str = parts[1].strip()
            if not num_str.isdigit():
                continue
            old_num = int(num_str)
            if old_num in rmap:
                new_num = rmap[old_num]
                new_parts = list(parts)
                new_parts[1] = str(new_num)
                result[idx] = _rebuild_pipe_row(line, parts, new_parts)

        elif stype == SectionType.SUCCESS_CRITERIA:
            def replace_range(m: re.Match) -> str:
                start_num = int(m.group(1))
                new_start = _remap_number(start_num, rmap)
                if m.group(2) is not None:
                    end_num = int(m.group(2))
                    new_end = _remap_number(end_num, rmap)
                    return f"AC#{new_start}-{new_end}"
                return f"AC#{new_start}"
            new_line = ac_range_re.sub(replace_range, line)
            if update_counts:
                def replace_all_n_success(m: re.Match) -> str:
                    count_val = int(m.group(2))
                    if count_val == max_old:
                        return m.group(1) + str(new_count) + m.group(3)
                    return m.group(0)
                new_line = all_n_acs_re.sub(replace_all_n_success, new_line)
            result[idx] = new_line

        elif stype == SectionType.REVIEW_NOTES:
            def replace_review(m: re.Match) -> str:
                full = m.group(0)
                num = int(m.group(1))
                new_num = _remap_number(num, rmap)
                if full.startswith("AC#"):
                    return f"AC#{new_num}"
                else:
                    return f"AC{new_num}"
            result[idx] = ac_informal_re.sub(replace_review, line)

        elif stype == SectionType.EXECUTION_LOG:
            # Update AC#N and ACN refs but NOT "N ACs" count patterns
            def replace_exec_log(m: re.Match) -> str:
                full = m.group(0)
                num = int(m.group(1))
                new_num = _remap_number(num, rmap)
                if full.startswith("AC#"):
                    return f"AC#{new_num}"
                else:
                    return f"AC{new_num}"
            result[idx] = ac_informal_re.sub(replace_exec_log, line)

    return result


def residual_scan(
    lines: list[str],
    section_map: dict[int, SectionType],
    rmap: dict[int, int],
) -> list[str]:
    """
    After applying renumber map, scan for remaining old AC numbers not updated.
    Returns list of warning strings.
    """
    warnings = []
    # Build set of numbers that were actually remapped (key != value)
    remapped_old = {old for old, new in rmap.items() if old != new}
    if not remapped_old:
        return warnings

    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        for old_num in remapped_old:
            # Search for AC#N occurrences
            pattern = re.compile(rf'AC#{old_num}(?![a-z\d])')
            if pattern.search(line):
                warning = f"WARNING: Line {idx + 1}: old AC#{old_num} still present"
                if stype == SectionType.OTHER:
                    warning += " (in OTHER section - manual review needed)"
                warnings.append(warning)

    return warnings


# =============================================================================
# Transform Helpers (ac-add / ac-link / ac-update)
# =============================================================================


def _find_ac_table_end(lines: list[str], section_map: dict[int, SectionType]) -> int:
    """Return 0-based line index of the last data row in the AC Definition Table."""
    last_idx = -1
    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        if stype == SectionType.AC_DEFINITION_TABLE:
            stripped = line.strip()
            if stripped.startswith("|") and "| AC# |" not in stripped:
                parts = split_pipe_row(stripped)
                if len(parts) > 1 and parts[1].strip().isdigit():
                    last_idx = idx
    return last_idx


def _add_ac_to_task_row(
    lines: list[str],
    section_map: dict[int, SectionType],
    task_num: int,
    ac_num: int,
) -> list[str]:
    """Add ac_num to the AC# column of Task# task_num. Idempotent."""
    result = list(lines)
    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        if stype != SectionType.TASKS_TABLE:
            continue
        stripped = line.strip()
        if not stripped.startswith("|") or "| Task# |" in stripped:
            continue
        # Skip separator rows
        if set(stripped.replace("|", "").strip()) <= {"-", ":"}:
            continue
        parts = split_pipe_row(stripped)
        # parts layout: ['', Task#, AC#, Description, Tag, Status, '']
        if len(parts) < 3:
            continue
        task_str = parts[1].strip()
        if not task_str.isdigit() or int(task_str) != task_num:
            continue
        # Found the task row — check AC# column (index 2)
        ac_col = parts[2].strip()
        existing = [t.strip() for t in ac_col.split(",") if t.strip()] if ac_col else []
        if str(ac_num) in existing:
            return result  # Already present — idempotent
        existing.append(str(ac_num))
        # Detect separator style from original
        sep = ", " if ", " in parts[2] else ","
        if not ac_col:
            sep = ", "  # default for empty column
        new_parts = list(parts)
        new_parts[2] = sep.join(existing)
        result[idx] = _rebuild_pipe_row(line, parts, new_parts)
        break
    return result


def _add_ac_to_goal_row(
    lines: list[str],
    section_map: dict[int, SectionType],
    goal_num: int,
    ac_num: int,
) -> list[str]:
    """Add AC#ac_num to the Covering AC(s) column of Goal# goal_num. Idempotent."""
    result = list(lines)
    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        if stype != SectionType.GOAL_COVERAGE:
            continue
        stripped = line.strip()
        if not stripped.startswith("|"):
            continue
        # Skip header and separator rows
        if "| Goal" in stripped:
            continue
        if set(stripped.replace("|", "").strip()) <= {"-", ":"}:
            continue
        parts = split_pipe_row(stripped)
        if len(parts) < 3:
            continue
        goal_str = parts[1].strip()
        if not goal_str.isdigit() or int(goal_str) != goal_num:
            continue
        # Found goal row — check Covering ACs column (index 2)
        ac_col = parts[2].strip()
        ac_ref = f"AC#{ac_num}"
        if ac_ref in ac_col:
            return result  # Already present — idempotent
        if ac_col:
            new_val = f"{ac_col}, {ac_ref}"
        else:
            new_val = ac_ref
        new_parts = list(parts)
        new_parts[2] = new_val
        result[idx] = _rebuild_pipe_row(line, parts, new_parts)
        break
    return result


def _add_ac_coverage_row(
    lines: list[str],
    section_map: dict[int, SectionType],
    ac_num: int,
    text: str,
) -> list[str]:
    """Append a row to the Technical Design AC Coverage table."""
    result = list(lines)
    # Find the last row in the AC Coverage table
    last_cov_idx = -1
    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        if stype == SectionType.TECH_DESIGN_AC_COV:
            stripped = line.strip()
            if stripped.startswith("|") and not set(stripped.replace("|", "").strip()) <= {"-", ":"}:
                last_cov_idx = idx
    if last_cov_idx == -1:
        return result  # No AC Coverage table found — skip silently
    new_row = f"| {ac_num} | {text} |\n"
    result.insert(last_cov_idx + 1, new_row)
    return result


def _build_ac_row_line(
    ac_num: int,
    desc: str,
    type_: str,
    method: str,
    matcher: str,
    expected: str,
) -> str:
    """Build a Definition Table row string."""
    return f"| {ac_num} | {desc} | {type_} | {method} | {matcher} | {expected} | [ ] |\n"


def _build_details_block(
    ac_num: int,
    desc: str,
    method: str,
    expected: str,
    derivation: str | None = None,
    rationale: str | None = None,
) -> list[str]:
    """Build AC Details block lines (for threshold matchers)."""
    block = [
        f"\n**AC#{ac_num}: {desc}**\n",
        f"- **Method**: {method}\n",
        f"- **Expected**: {expected}\n",
    ]
    if derivation:
        block.append(f"- **Derivation**: {derivation}\n")
    if rationale:
        block.append(f"- **Rationale**: {rationale}\n")
    return block


def _find_details_insertion_point(lines: list[str], section_map: dict[int, SectionType]) -> int:
    """Return line index for inserting new AC Details (end of AC Details section)."""
    last_details_idx = -1
    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        if stype == SectionType.AC_DETAILS:
            last_details_idx = idx
    if last_details_idx == -1:
        return -1
    # Move past the last non-empty line in the AC Details section
    # Insert after last_details_idx + any trailing blank lines
    insert_at = last_details_idx + 1
    while insert_at < len(lines) and lines[insert_at].strip() == "":
        stype = section_map.get(insert_at, SectionType.OTHER)
        # If next section begins, stop
        if stype != SectionType.AC_DETAILS and stype != SectionType.OTHER:
            break
        insert_at += 1
    return insert_at


# =============================================================================
# Command Layer
# =============================================================================


def _backup_path(path: Path) -> Path:
    """Return backup path for a feature file."""
    return path.with_suffix('.md.bak')


def _create_backup(path: Path) -> Path:
    """Create backup of feature file. Returns backup path."""
    backup = _backup_path(path)
    shutil.copy2(path, backup)
    return backup


def _remove_backup(path: Path) -> None:
    """Remove backup file if it exists."""
    backup = _backup_path(path)
    if backup.exists():
        backup.unlink()


def _restore_backup(path: Path) -> None:
    """Restore from backup and clean up."""
    backup = _backup_path(path)
    if backup.exists():
        shutil.copy2(backup, path)
        backup.unlink()


def _detect_sub_numbered(ac_rows: list[ACRow]) -> bool:
    """Detect sub-numbered ACs like 3b, 3c."""
    for row in ac_rows:
        if re.search(r'\d+[a-z]', str(row.number)):
            return True
    return False


def _detect_sub_numbered_in_lines(lines: list[str]) -> bool:
    """Scan lines for sub-numbered AC patterns like AC#3b."""
    for line in lines:
        if re.search(r'AC#\d+[a-z]', line):
            return True
    return False


_CHECK_ID_PATTERNS: dict[str, list[str]] = {
    "N1": ["Invalid matcher '"],
    "N3": ["Regex has trailing pipe", "Regex has unbalanced parentheses"],
    "N4": ["looks like an Expected value", "looks like a Matcher name"],
    "N7": ["Not assigned to any Task"],
    "N9": ["Goal Coverage row has no AC# reference"],
    "N10": ["50 hard limit"],
    "N11": ["30 soft limit"],
    "N12": ["matcher but AC Details lacks derivation"],
}


def _issue_skipped(issue: str, skip: frozenset[str]) -> bool:
    """Return True if issue matches any skipped check ID."""
    for check_id in skip:
        for pattern in _CHECK_ID_PATTERNS.get(check_id, []):
            if pattern in issue:
                return True
    return False


def ac_check(fid: str, fix: bool = False, dry_run: bool = False,
             skip: frozenset[str] = frozenset(),
             warn_count: bool = False) -> list[str]:
    """
    Read feature file. Parse AC table. Classify sections. Scan references.
    Check for various consistency issues. Print and return issue strings.
    Returns exit code 0 if no issues, 1 if issues found.

    skip: frozenset of check IDs to suppress (e.g. {"N12"}).
    warn_count: enable N10/N11 AC count limit checks (off by default).
    """
    path = feature_path(fid)
    if not path.exists():
        print(f"ERROR: Feature file not found: {path}", file=sys.stderr)
        return ["Feature file not found"]

    with open(path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    ac_rows = parse_ac_table(lines)
    section_map = classify_sections(lines)
    refs = scan_ac_references(lines, section_map)

    issues = []

    if not ac_rows:
        issues.append("No AC Definition Table found.")
        for issue in issues:
            print(issue)
        return issues

    defined_numbers = {row.number for row in ac_rows}

    # 1. Definition vs Details mismatch
    details_numbers = set()
    for ref in refs:
        if ref.section_type == SectionType.AC_DETAILS:
            details_numbers.add(ref.ac_number)

    threshold_ac_numbers = {row.number for row in ac_rows if row.matcher in THRESHOLD_MATCHERS}
    in_table_not_details = threshold_ac_numbers - details_numbers
    in_details_not_table = details_numbers - defined_numbers

    for num in sorted(in_table_not_details):
        issues.append(f"AC#{num}: Uses threshold matcher but no AC Details block found (Derivation required).")
    for num in sorted(in_details_not_table):
        issues.append(f"AC#{num}: In AC Details but not in Definition Table.")

    # 2. Tasks referencing non-existent ACs
    tasks_numbers = set()
    for ref in refs:
        if ref.section_type == SectionType.TASKS_TABLE:
            tasks_numbers.add(ref.ac_number)

    for num in sorted(tasks_numbers - defined_numbers):
        issues.append(f"AC#{num}: Referenced in Tasks table but not in Definition Table.")

    # 3. Goal Coverage missing ACs
    goal_numbers = set()
    for ref in refs:
        if ref.section_type == SectionType.GOAL_COVERAGE:
            goal_numbers.add(ref.ac_number)

    if goal_numbers:
        missing_from_goal = defined_numbers - goal_numbers
        for num in sorted(missing_from_goal):
            issues.append(f"AC#{num}: In Definition Table but not referenced in Goal Coverage.")

    # 4. Numbering gaps (skip if sub-numbered ACs exist — gaps are expected)
    has_sub_numbered = _detect_sub_numbered_in_lines(lines)
    sorted_nums = sorted(defined_numbers)
    if sorted_nums and not has_sub_numbered:
        expected = list(range(1, sorted_nums[-1] + 1))
        gaps = [n for n in expected if n not in defined_numbers]
        if gaps:
            issues.append(f"Numbering gaps detected: {gaps}")

    # 5. "All N ACs" count mismatch (skip Review Notes and Success Criteria — historical records)
    actual_count = len(defined_numbers)
    all_n_acs_re = re.compile(r'(?i)all\s+(\d+)\s+ACs?')
    _current_section = SectionType.OTHER
    _skip_sections = {SectionType.REVIEW_NOTES, SectionType.SUCCESS_CRITERIA}
    for idx, line in enumerate(lines):
        if idx in section_map:
            _current_section = section_map[idx]
        if _current_section in _skip_sections:
            continue
        for m in all_n_acs_re.finditer(line):
            stated_count = int(m.group(1))
            if stated_count != actual_count:
                issues.append(
                    f"Line {idx + 1}: 'All {stated_count} ACs' but actual AC count is {actual_count}."
                )

    # 6. Tech Design AC Coverage count mismatch
    tech_cov_numbers = set()
    for ref in refs:
        if ref.section_type == SectionType.TECH_DESIGN_AC_COV:
            tech_cov_numbers.add(ref.ac_number)
    if tech_cov_numbers and tech_cov_numbers != defined_numbers:
        extra = tech_cov_numbers - defined_numbers
        missing = defined_numbers - tech_cov_numbers
        if extra:
            issues.append(f"Tech Design AC Coverage references non-existent ACs: {sorted(extra)}")
        if missing:
            issues.append(f"Tech Design AC Coverage missing ACs: {sorted(missing)}")

    # 7. Sub-numbered ACs (informational only — not an issue for ac-check)
    if has_sub_numbered:
        print("Info: Sub-numbered ACs detected (e.g., AC#26a). Numbering gap check skipped.")

    # N1. Invalid matcher names
    for row in ac_rows:
        if row.matcher and row.matcher not in VALID_MATCHERS:
            fix_suggestion = _MATCHER_FIX_MAP.get(row.matcher)
            if fix_suggestion:
                issues.append(f"AC#{row.number}: Invalid matcher '{row.matcher}' (suggest: '{fix_suggestion}').")
            else:
                issues.append(f"AC#{row.number}: Invalid matcher '{row.matcher}'.")

    # N3. Regex syntax errors (unbalanced parens, trailing pipe)
    for row in ac_rows:
        if row.matcher in ("matches", "not_matches") and row.expected and row.expected != "-":
            pattern = row.expected.strip("`")
            # Trailing unescaped pipe
            if pattern.endswith("|") or pattern.endswith("\\|"):
                issues.append(f"AC#{row.number}: Regex has trailing pipe: '{row.expected}'.")
            # Unbalanced parentheses (count unescaped parens, skip char classes)
            depth = 0
            in_char_class = False
            for ci, ch in enumerate(pattern):
                escaped = ci > 0 and pattern[ci - 1] == '\\'
                if escaped:
                    continue
                if ch == '[':
                    in_char_class = True
                elif ch == ']' and in_char_class:
                    in_char_class = False
                elif not in_char_class:
                    if ch == '(':
                        depth += 1
                    elif ch == ')':
                        depth -= 1
            if depth != 0:
                issues.append(f"AC#{row.number}: Regex has unbalanced parentheses: '{row.expected}'.")

    # N4. Column swap detection (Matcher column has typical Expected values)
    _expected_like = re.compile(r'^(`.*`|\d+|-.*)$')
    _method_like = re.compile(r'^(Grep|Glob|dotnet|pytest|python)', re.IGNORECASE)
    for row in ac_rows:
        # Matcher looks like an Expected value (backtick-wrapped or bare number)
        if row.matcher and _expected_like.match(row.matcher):
            issues.append(f"AC#{row.number}: Matcher column '{row.matcher}' looks like an Expected value (possible column swap).")
        # Method looks like a matcher name
        if row.method and row.method.lower() in VALID_MATCHERS:
            issues.append(f"AC#{row.number}: Method column '{row.method}' looks like a Matcher name (possible column swap).")

    # N7. ACs not assigned to any Task
    task_ac_numbers = set()
    for ref in refs:
        if ref.section_type == SectionType.TASKS_TABLE:
            task_ac_numbers.add(ref.ac_number)
    if task_ac_numbers:  # Only check if Tasks table has AC refs at all
        unassigned = defined_numbers - task_ac_numbers
        for num in sorted(unassigned):
            issues.append(f"AC#{num}: Not assigned to any Task.")

    # N9. Goal Coverage rows without AC# references
    in_goal_cov = False
    goal_cov_header_seen = False
    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        stripped = line.strip()
        if stype == SectionType.GOAL_COVERAGE:
            if not in_goal_cov:
                in_goal_cov = True
                goal_cov_header_seen = False
                continue
            # Skip empty lines
            if not stripped:
                continue
            # Skip table header row (first pipe row)
            if stripped.startswith("|") and not goal_cov_header_seen:
                goal_cov_header_seen = True
                continue
            # Skip separator lines
            if stripped.startswith("|") and set(stripped.replace("|", "").strip()) <= {"-", ":"}:
                continue
            if stripped.startswith("|") and "AC#" not in stripped:
                issues.append(f"Line {idx + 1}: Goal Coverage row has no AC# reference.")
        else:
            in_goal_cov = False
            goal_cov_header_seen = False

    # N10/N11. AC count limits (opt-in via --warn-count)
    if warn_count:
        if actual_count > 50:
            issues.append(f"AC count is {actual_count} (> 50 hard limit). Feature MUST be split.")
        elif actual_count > 30:
            issues.append(f"AC count is {actual_count} (> 30 soft limit). Consider splitting.")

    # N12. Threshold matcher without derivation in AC Details
    threshold_ac_numbers = {row.number for row in ac_rows if row.matcher in THRESHOLD_MATCHERS}
    if threshold_ac_numbers:
        # Scan AC Details for derivation notes
        details_re = re.compile(r'^\*\*AC#(\d+):')
        derivation_keywords = re.compile(
            r'\d+\s+(?:\S+\s+)*(?:'
            r'functions?|interfaces?|methods?|files?|items?|parameters?|entries|classes|types|'
            r'constants?|registrations?|occurrences?|calls?|signatures?|references?|paths?|'
            r'names?|assertions?|rows?|patterns?|checks?|members?|properties?|fields?|'
            r'tests?|values?|lines?|definitions?|declarations?|statements?|sections?|'
            r'variables?|commands?|imports?|exports?|modules?|components?|elements?|'
            r'attributes?|dependencies?|configurations?|operations?|handlers?|validators?|'
            r'matchers?|conditions?'
            r')', re.IGNORECASE)
        count_gte_pattern = re.compile(r'Count\s*>=\s*\d+\s*\(', re.IGNORECASE)
        explicit_derivation = re.compile(r'^\-\s*\*\*(?:Rationale|Derivation)\*\*:', re.IGNORECASE)
        current_ac_detail = None
        has_derivation = set()
        for line in lines:
            stripped = line.strip()
            m = details_re.match(stripped)
            if m:
                current_ac_detail = int(m.group(1))
                continue
            if current_ac_detail in threshold_ac_numbers:
                if (derivation_keywords.search(stripped)
                        or count_gte_pattern.search(stripped)
                        or explicit_derivation.match(stripped)):
                    has_derivation.add(current_ac_detail)
        missing_derivation = threshold_ac_numbers - has_derivation
        for num in sorted(missing_derivation):
            matcher_name = next(row.matcher for row in ac_rows if row.number == num)
            issues.append(f"AC#{num}: Uses '{matcher_name}' matcher but AC Details lacks derivation.")

    # Filter skipped checks
    if skip:
        issues = [i for i in issues if not _issue_skipped(i, skip)]

    if not fix:
        if issues:
            for issue in issues:
                print(issue)
            return issues
        else:
            print(f"ac-check: No issues found in feature-{fid}.md ({actual_count} ACs).")
            return []

    # --fix mode: attempt auto-corrections
    fixed = []
    remaining = []

    # Classify issues as fixable or not
    fixable_matchers = {}  # ac_num -> new_matcher
    has_gaps = False
    has_stale_counts = []  # (line_idx_1based, stated, actual)

    for issue in issues:
        # N1: Invalid matcher with suggestion
        m = re.match(r"AC#(\d+): Invalid matcher '(\w+)' \(suggest: '(\w+)'\)\.", issue)
        if m:
            fixable_matchers[int(m.group(1))] = m.group(3)
            continue
        # #5: Numbering gaps
        if "Numbering gaps detected" in issue:
            has_gaps = True
            continue
        # #6: Stale "All N ACs" count
        m2 = re.match(r"Line (\d+): 'All (\d+) ACs' but actual AC count is (\d+)\.", issue)
        if m2:
            has_stale_counts.append((int(m2.group(1)), int(m2.group(2)), int(m2.group(3))))
            continue
        remaining.append(issue)

    applied = 0

    if fixable_matchers or has_stale_counts or has_gaps:
        if dry_run:
            # Report what would be fixed
            for ac_num, new_m in sorted(fixable_matchers.items()):
                old_m = next((r.matcher for r in ac_rows if r.number == ac_num), "?")
                print(f"  [DRY-RUN] AC#{ac_num}: Invalid matcher '{old_m}' → '{new_m}'")
                applied += 1
            for line_num, stated, actual in has_stale_counts:
                print(f"  [DRY-RUN] Line {line_num}: 'All {stated} ACs' → 'All {actual} ACs'")
                applied += 1
            if has_gaps:
                print(f"  [DRY-RUN] Numbering gaps would be closed via renumber.")
                applied += 1
        else:
            backup = _create_backup(path)
            try:
                # Re-read for mutation
                with open(path, "r", encoding="utf-8") as f:
                    fix_lines = f.readlines()

                # Fix 1: Matchers (N1) — via ac_fix calls for each
                for ac_num, new_matcher in sorted(fixable_matchers.items()):
                    old_row = next((r for r in ac_rows if r.number == ac_num), None)
                    if old_row:
                        # Direct line replacement in fix_lines
                        fix_section_map = classify_sections(fix_lines)
                        for idx, line in enumerate(fix_lines):
                            stype = fix_section_map.get(idx, SectionType.OTHER)
                            if stype == SectionType.AC_DEFINITION_TABLE:
                                stripped = line.strip()
                                if stripped.startswith("|") and "| AC# |" not in stripped:
                                    parts = split_pipe_row(stripped)
                                    if len(parts) > 5 and parts[1].strip() == str(ac_num):
                                        new_parts = list(parts)
                                        new_parts[5] = new_matcher
                                        fix_lines[idx] = _rebuild_pipe_row(line, parts, new_parts)
                                        fixed.append(f"[FIXED] AC#{ac_num}: Invalid matcher '{old_row.matcher}' → '{new_matcher}'")
                                        applied += 1
                                        break

                # Fix 2: Stale counts (#6)
                all_n_acs_re = re.compile(r'(?i)(all\s+)\d+(\s+ACs?)')
                fix_section_map = classify_sections(fix_lines)
                _skip_sections = {SectionType.REVIEW_NOTES, SectionType.SUCCESS_CRITERIA}
                _fix_current_section = SectionType.OTHER
                for idx, line in enumerate(fix_lines):
                    if idx in fix_section_map:
                        _fix_current_section = fix_section_map[idx]
                    if _fix_current_section in _skip_sections:
                        continue
                    for m in all_n_acs_re.finditer(line):
                        # Check full match for stale count
                        full_match = re.search(r'(?i)all\s+(\d+)\s+ACs?', line)
                        if full_match:
                            stated = int(full_match.group(1))
                            if stated != actual_count:
                                fix_lines[idx] = re.sub(
                                    r'(?i)(all\s+)\d+(\s+ACs?)',
                                    rf'\g<1>{actual_count}\g<2>',
                                    fix_lines[idx],
                                )
                                fixed.append(f"[FIXED] Line {idx + 1}: 'All {stated} ACs' → 'All {actual_count} ACs'")
                                applied += 1
                                break  # one fix per line

                # Write intermediate
                with open(path, "w", encoding="utf-8") as f:
                    f.writelines(fix_lines)

                # Fix 3: Renumber gaps (#5) — last because AC# change
                if has_gaps:
                    result = ac_renumber(fid)
                    if result == 0:
                        fixed.append("[FIXED] Numbering gaps closed via renumber.")
                        applied += 1

                _remove_backup(path)
            except Exception as e:
                print(f"ERROR: Exception during fix: {e}. Restoring backup.", file=sys.stderr)
                _restore_backup(path)
                return issues  # Return original issues on failure

    # Post-fix re-check (verify fixes didn't introduce new issues)
    if applied > 0 and not dry_run:
        recheck_issues = ac_check(fid, fix=False)
        remaining = recheck_issues

    # Report
    total = len(issues)
    fixed_count = applied
    remaining_count = len(remaining)
    print(f"ac-check: {total} issues found, {fixed_count} auto-fixed, {remaining_count} remaining.")
    for f_msg in fixed:
        print(f"  {f_msg}")
    for r_msg in remaining:
        print(f"  {r_msg}")

    # --fix exit semantics: return [] (exit 0) if all fixable issues were fixed,
    # even when non-fixable issues remain (they are informational for --fix callers).
    # This prevents PHASE-2 from treating non-fixable WARN as a failure.
    if fixed_count > 0 and not dry_run:
        return []  # fix succeeded — remaining are informational
    return remaining


def ac_renumber(fid: str, dry_run: bool = False, force: bool = False) -> int:
    """
    Read file. Parse AC table. Check for gaps.
    If gaps exist, build renumber map and apply it.
    Returns 0 on success, 1 on error.
    """
    path = feature_path(fid)
    if not path.exists():
        print(f"ERROR: Feature file not found: {path}", file=sys.stderr)
        return 1

    with open(path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    ac_rows = parse_ac_table(lines)
    if not ac_rows:
        print("ERROR: No AC Definition Table found.", file=sys.stderr)
        return 1

    # Sub-numbered check
    if not force and _detect_sub_numbered_in_lines(lines):
        print(
            "ERROR: Sub-numbered ACs detected (e.g., AC#3b). "
            "Cannot safely renumber. Use --force to override.",
            file=sys.stderr,
        )
        return 1

    defined_numbers = sorted({row.number for row in ac_rows})

    # Check for gaps
    expected = list(range(1, defined_numbers[-1] + 1))
    gaps = [n for n in expected if n not in defined_numbers]

    if not gaps:
        print(f"ac-renumber: No gaps found in feature-{fid}.md. Nothing to do.")
        return 0

    print(f"ac-renumber: Gaps detected at: {gaps}")
    rmap = build_renumber_map(defined_numbers)
    changes = {old: new for old, new in rmap.items() if old != new}
    print(f"ac-renumber: Renumber map (changed only): {changes}")

    if dry_run:
        print("ac-renumber: [DRY RUN] No changes written.")
        return 0

    backup = _create_backup(path)
    try:
        section_map = classify_sections(lines)
        new_lines = apply_renumber_map(lines, section_map, rmap, update_counts=True)

        # Pre-write validation
        validation_rows = parse_ac_table(new_lines)
        if not validation_rows:
            print("ERROR: Renumber would destroy AC Definition Table. Aborting.", file=sys.stderr)
            _remove_backup(path)
            return 1
        if len(validation_rows) != len(ac_rows):
            print(
                f"ERROR: Renumber would change AC count from {len(ac_rows)} to "
                f"{len(validation_rows)}. Aborting.",
                file=sys.stderr,
            )
            _remove_backup(path)
            return 1

        warnings = residual_scan(new_lines, section_map, rmap)
        for w in warnings:
            print(w)

        with open(path, "w", encoding="utf-8") as f:
            f.writelines(new_lines)

        _remove_backup(path)
        print(f"ac-renumber: Wrote updated feature-{fid}.md ({len(changes)} AC numbers changed).")
        return 0
    except Exception as e:
        print(f"ERROR: Exception during renumber: {e}. Restoring backup.", file=sys.stderr)
        _restore_backup(path)
        return 1


def ac_insert(fid: str, after: int, dry_run: bool = False, force: bool = False) -> int:
    """
    Insert a placeholder for a new AC after position `after`.
    Shifts all AC numbers > after by +1.
    Does NOT add table rows or Details blocks (caller's responsibility).
    Returns 0 on success, 1 on error.
    """
    path = feature_path(fid)
    if not path.exists():
        print(f"ERROR: Feature file not found: {path}", file=sys.stderr)
        return 1

    with open(path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    ac_rows = parse_ac_table(lines)
    if not ac_rows:
        print("ERROR: No AC Definition Table found.", file=sys.stderr)
        return 1

    # Sub-numbered check
    if not force and _detect_sub_numbered_in_lines(lines):
        print(
            "ERROR: Sub-numbered ACs detected (e.g., AC#3b). "
            "Cannot safely insert. Use --force to override.",
            file=sys.stderr,
        )
        return 1

    defined_numbers = {row.number for row in ac_rows}

    if after not in defined_numbers and after != 0:
        print(f"ERROR: AC#{after} not found in Definition Table.", file=sys.stderr)
        return 1

    # Build shift map: AC > after gets +1
    all_nums = sorted(defined_numbers)
    new_ac_num = after + 1
    rmap = {}
    for num in all_nums:
        if num > after:
            rmap[num] = num + 1
        else:
            rmap[num] = num

    changes = {old: new for old, new in rmap.items() if old != new}
    print(f"ac-insert: Inserting new AC#{new_ac_num} after AC#{after}.")
    print(f"ac-insert: Shifting ACs: {changes}")

    if dry_run:
        print("ac-insert: [DRY RUN] No changes written.")
        return 0

    backup = _create_backup(path)
    try:
        section_map = classify_sections(lines)
        new_lines = apply_renumber_map(lines, section_map, rmap, update_counts=True)

        # Pre-write validation
        validation_rows = parse_ac_table(new_lines)
        if not validation_rows:
            print("ERROR: Insert would destroy AC Definition Table. Aborting.", file=sys.stderr)
            _remove_backup(path)
            return 1

        with open(path, "w", encoding="utf-8") as f:
            f.writelines(new_lines)

        _remove_backup(path)
        print(
            f"ac-insert: Wrote updated feature-{fid}.md. "
            f"New AC slot is AC#{new_ac_num} (add table row and Details block manually)."
        )
        return 0
    except Exception as e:
        print(f"ERROR: Exception during insert: {e}. Restoring backup.", file=sys.stderr)
        _restore_backup(path)
        return 1


def ac_delete(fid: str, ac_num: int, dry_run: bool = False, force: bool = False) -> int:
    """
    Delete an AC from the feature file.
    Removes the Definition Table row, the AC Details block, and all references.
    Renumbers remaining ACs to close gap.
    Returns 0 on success, 1 on error.
    """
    path = feature_path(fid)
    if not path.exists():
        print(f"ERROR: Feature file not found: {path}", file=sys.stderr)
        return 1

    with open(path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    ac_rows = parse_ac_table(lines)
    if not ac_rows:
        print("ERROR: No AC Definition Table found.", file=sys.stderr)
        return 1

    # Sub-numbered check
    if not force and _detect_sub_numbered_in_lines(lines):
        print(
            "ERROR: Sub-numbered ACs detected (e.g., AC#3b). "
            "Cannot safely delete. Use --force to override.",
            file=sys.stderr,
        )
        return 1

    defined_numbers = {row.number for row in ac_rows}
    if ac_num not in defined_numbers:
        print(f"ERROR: AC#{ac_num} not found in Definition Table.", file=sys.stderr)
        return 1

    print(f"ac-delete: Removing AC#{ac_num} from feature-{fid}.md.")

    if dry_run:
        print("ac-delete: [DRY RUN] No changes written.")
        return 0

    backup = _create_backup(path)
    try:
        section_map = classify_sections(lines)

        # Step 1: Remove Definition Table row for ac_num
        new_lines = []
        for idx, line in enumerate(lines):
            stype = section_map.get(idx, SectionType.OTHER)
            if stype == SectionType.AC_DEFINITION_TABLE:
                stripped = line.strip()
                if stripped.startswith("|") and "| AC# |" not in stripped:
                    parts = split_pipe_row(stripped)
                    if len(parts) > 1 and parts[1].strip() == str(ac_num):
                        # Skip this row (delete it)
                        continue
            new_lines.append(line)

        # Step 2: Remove AC Details block for ac_num
        # Find `**AC#N:` heading, remove until next `**AC#` or section end
        details_start = None
        details_end = None
        detail_heading_re = re.compile(r'^\*\*AC#(\d+):')

        for idx, line in enumerate(new_lines):
            m = detail_heading_re.match(line.strip())
            if m:
                num = int(m.group(1))
                if num == ac_num:
                    details_start = idx
                elif details_start is not None and details_end is None:
                    details_end = idx
                    break

        if details_start is not None:
            if details_end is None:
                # Remove from details_start to end of section
                # Find next H3 or H2 heading
                for idx in range(details_start + 1, len(new_lines)):
                    if new_lines[idx].strip().startswith("###") or new_lines[idx].strip().startswith("## "):
                        details_end = idx
                        break
                if details_end is None:
                    details_end = len(new_lines)

            del new_lines[details_start:details_end]

        # Step 3: Remove references from Goal Coverage and Tasks for ac_num
        # (We'll just remove tokens from those cells)
        temp_section_map = classify_sections(new_lines)
        final_lines = []
        for idx, line in enumerate(new_lines):
            stype = temp_section_map.get(idx, SectionType.OTHER)
            if stype == SectionType.GOAL_COVERAGE:
                # Remove AC#<ac_num> references
                line = re.sub(rf',?\s*AC#{ac_num}(?![a-z\d])', '', line)
                line = re.sub(rf'AC#{ac_num}(?![a-z\d])\s*,?\s*', '', line)
            elif stype == SectionType.TASKS_TABLE:
                stripped = line.strip()
                if stripped.startswith("|"):
                    parts = split_pipe_row(stripped)
                    if len(parts) > 2:
                        tokens = [t.strip() for t in parts[2].split(",")]
                        tokens = [t for t in tokens if t != str(ac_num)]
                        parts[2] = ", ".join(tokens)
                    if len(parts) > 3:
                        parts[3] = re.sub(rf'AC#{ac_num}(?![a-z\d])', '', parts[3])
                    line = _rebuild_pipe_row(line, parts, parts)
            final_lines.append(line)

        # Step 4: Renumber remaining ACs to close gap
        remaining_nums = sorted(defined_numbers - {ac_num})
        rmap = build_renumber_map(remaining_nums)
        final_section_map = classify_sections(final_lines)
        final_lines = apply_renumber_map(final_lines, final_section_map, rmap, update_counts=True)

        # Add validation before write
        validation_rows = parse_ac_table(final_lines)
        if not validation_rows and len(defined_numbers) > 1:
            print("ERROR: Delete would destroy AC Definition Table. Aborting.", file=sys.stderr)
            _remove_backup(path)
            return 1

        with open(path, "w", encoding="utf-8") as f:
            f.writelines(final_lines)

        _remove_backup(path)
        print(f"ac-delete: Removed AC#{ac_num} and renumbered remaining {len(remaining_nums)} ACs.")
        return 0
    except Exception as e:
        print(f"ERROR: Exception during delete: {e}. Restoring backup.", file=sys.stderr)
        _restore_backup(path)
        return 1


def ac_fix(
    fid: str,
    ac_num: int,
    expected: str | None = None,
    description: str | None = None,
    dry_run: bool = False,
) -> int:
    """
    Fix AC metadata: update Expected and/or Description columns.
    Reports (but does not auto-replace) prose occurrences of old values.
    Returns 0 on success, 1 on error.
    """
    path = feature_path(fid)
    if not path.exists():
        print(f"ERROR: Feature file not found: {path}", file=sys.stderr)
        return 1

    with open(path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    ac_rows = parse_ac_table(lines)
    if not ac_rows:
        print("ERROR: No AC Definition Table found.", file=sys.stderr)
        return 1

    target_row = None
    for row in ac_rows:
        if row.number == ac_num:
            target_row = row
            break

    if target_row is None:
        print(f"ERROR: AC#{ac_num} not found in Definition Table.", file=sys.stderr)
        return 1

    if expected is None and description is None:
        print("ERROR: Provide --expected and/or --description to update.", file=sys.stderr)
        return 1

    old_expected = target_row.expected
    old_description = target_row.description

    print(f"ac-fix: Updating AC#{ac_num} in feature-{fid}.md.")
    if expected is not None:
        print(f"  Expected: '{old_expected}' -> '{expected}'")
    if description is not None:
        print(f"  Description: '{old_description}' -> '{description}'")

    if dry_run:
        print("ac-fix: [DRY RUN] No changes written.")
        return 0

    backup = _create_backup(path)
    try:
        section_map = classify_sections(lines)
        new_lines = list(lines)

        # Update Definition Table row
        for idx, line in enumerate(lines):
            stype = section_map.get(idx, SectionType.OTHER)
            if stype == SectionType.AC_DEFINITION_TABLE:
                stripped = line.strip()
                if stripped.startswith("|") and "| AC# |" not in stripped:
                    parts = split_pipe_row(stripped)
                    if len(parts) > 1 and parts[1].strip() == str(ac_num):
                        new_parts = list(parts)
                        # Column layout: ['', AC#, Description, Type, Method, Matcher, Expected, Status, '']
                        if description is not None and len(new_parts) > 2:
                            new_parts[2] = description
                        if expected is not None and len(new_parts) > 6:
                            new_parts[6] = expected
                        new_lines[idx] = _rebuild_pipe_row(line, parts, new_parts)
                        break

        # Update AC Details block
        details_heading_re = re.compile(rf'^\*\*AC#{ac_num}:')
        in_details_block = False
        expected_line_re = re.compile(r'^-\s+\*\*Expected\*\*:')
        heading_line_re = re.compile(r'^\*\*AC#\d+:')

        for idx, line in enumerate(new_lines):
            stripped = line.strip()
            if details_heading_re.match(stripped):
                in_details_block = True
                if description is not None:
                    # Replace heading: **AC#N: old description** -> **AC#N: new description**
                    new_line = re.sub(
                        rf'(\*\*AC#{ac_num}:\s*)(.+?)(\*\*)',
                        rf'\g<1>{description}\g<3>',
                        line,
                    )
                    new_lines[idx] = new_line
                continue

            if in_details_block:
                if heading_line_re.match(stripped) or stripped.startswith("## ") or stripped.startswith("### "):
                    in_details_block = False
                    break
                if expected is not None and expected_line_re.match(stripped):
                    # Replace the Expected line
                    new_lines[idx] = re.sub(
                        r'(-\s+\*\*Expected\*\*:\s*)(.+)',
                        rf'\g<1>{expected}',
                        line,
                    )

        # Report prose occurrences of old values (don't auto-replace)
        prose_warnings = []
        for idx, line in enumerate(lines):
            stype = section_map.get(idx, SectionType.OTHER)
            if stype in (SectionType.TECH_DESIGN_PROSE, SectionType.REVIEW_NOTES, SectionType.SUCCESS_CRITERIA):
                if old_expected and old_expected != "-" and old_expected in line:
                    prose_warnings.append(
                        f"  Line {idx + 1}: old Expected value found in prose (manual review): {line.rstrip()}"
                    )
                if old_description and old_description in line and description is not None:
                    prose_warnings.append(
                        f"  Line {idx + 1}: old Description found in prose (manual review): {line.rstrip()}"
                    )

        # Pre-write validation
        validation_rows = parse_ac_table(new_lines)
        if not validation_rows:
            print("ERROR: Fix would destroy AC Definition Table. Aborting.", file=sys.stderr)
            _remove_backup(path)
            return 1

        with open(path, "w", encoding="utf-8") as f:
            f.writelines(new_lines)

        _remove_backup(path)
        print(f"ac-fix: Updated AC#{ac_num} successfully.")
        if prose_warnings:
            print("ac-fix: WARNING - old values found in prose (not auto-replaced):")
            for w in prose_warnings:
                print(w)

        return 0
    except Exception as e:
        print(f"ERROR: Exception during fix: {e}. Restoring backup.", file=sys.stderr)
        _restore_backup(path)
        return 1


def ac_link(
    fid: str,
    ac_num: int,
    task: int | None = None,
    goal: int | None = None,
    dry_run: bool = False,
) -> int:
    """
    Link an existing AC to a Task and/or Goal row.
    Idempotent: skips if already linked.
    Returns 0 on success, 1 on error.
    """
    if task is None and goal is None:
        print("ERROR: Provide --task and/or --goal.", file=sys.stderr)
        return 1

    path = feature_path(fid)
    if not path.exists():
        print(f"ERROR: Feature file not found: {path}", file=sys.stderr)
        return 1

    with open(path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    ac_rows = parse_ac_table(lines)
    if not ac_rows:
        print("ERROR: No AC Definition Table found.", file=sys.stderr)
        return 1

    defined_numbers = {row.number for row in ac_rows}
    if ac_num not in defined_numbers:
        print(f"ERROR: AC#{ac_num} not found in Definition Table.", file=sys.stderr)
        return 1

    section_map = classify_sections(lines)
    result = list(lines)
    actions = []

    if task is not None:
        new_result = _add_ac_to_task_row(result, section_map, task, ac_num)
        if new_result != result:
            actions.append(f"Linked AC#{ac_num} to Task#{task}")
        else:
            actions.append(f"AC#{ac_num} already linked to Task#{task} (skipped)")
        result = new_result
        # Reclassify after modification
        section_map = classify_sections(result)

    if goal is not None:
        new_result = _add_ac_to_goal_row(result, section_map, goal, ac_num)
        if new_result != result:
            actions.append(f"Linked AC#{ac_num} to Goal#{goal}")
        else:
            actions.append(f"AC#{ac_num} already linked to Goal#{goal} (skipped)")
        result = new_result

    for a in actions:
        print(f"ac-link: {a}")

    if dry_run:
        print("ac-link: [DRY RUN] No changes written.")
        return 0

    if result == lines:
        print("ac-link: No changes needed.")
        return 0

    backup = _create_backup(path)
    try:
        with open(path, "w", encoding="utf-8") as f:
            f.writelines(result)
        _remove_backup(path)
        print(f"ac-link: Wrote updated feature-{fid}.md.")
        return 0
    except Exception as e:
        print(f"ERROR: Exception during link: {e}. Restoring backup.", file=sys.stderr)
        _restore_backup(path)
        return 1


def ac_update(
    fid: str,
    ac_num: int,
    description: str | None = None,
    expected: str | None = None,
    matcher: str | None = None,
    method: str | None = None,
    sync_coverage: bool = False,
    dry_run: bool = False,
) -> int:
    """
    Update AC metadata: description, expected, matcher, method.
    Extended version of ac_fix with matcher/method support and --sync-coverage.
    Returns 0 on success, 1 on error.
    """
    path = feature_path(fid)
    if not path.exists():
        print(f"ERROR: Feature file not found: {path}", file=sys.stderr)
        return 1

    if description is None and expected is None and matcher is None and method is None:
        print("ERROR: Provide at least one of --description, --expected, --matcher, --method.", file=sys.stderr)
        return 1

    with open(path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    ac_rows = parse_ac_table(lines)
    if not ac_rows:
        print("ERROR: No AC Definition Table found.", file=sys.stderr)
        return 1

    target_row = None
    for row in ac_rows:
        if row.number == ac_num:
            target_row = row
            break

    if target_row is None:
        print(f"ERROR: AC#{ac_num} not found in Definition Table.", file=sys.stderr)
        return 1

    old_desc = target_row.description
    old_expected = target_row.expected
    old_matcher = target_row.matcher
    old_method = target_row.method

    changes = []
    if description is not None and description != old_desc:
        changes.append(f"  Description: '{old_desc}' -> '{description}'")
    if expected is not None and expected != old_expected:
        changes.append(f"  Expected: '{old_expected}' -> '{expected}'")
    if matcher is not None and matcher != old_matcher:
        changes.append(f"  Matcher: '{old_matcher}' -> '{matcher}'")
    if method is not None and method != old_method:
        changes.append(f"  Method: '{old_method}' -> '{method}'")

    if not changes:
        print(f"ac-update: AC#{ac_num} already has the specified values. Nothing to do.")
        return 0

    print(f"ac-update: Updating AC#{ac_num} in feature-{fid}.md.")
    for c in changes:
        print(c)

    if dry_run:
        print("ac-update: [DRY RUN] No changes written.")
        return 0

    backup = _create_backup(path)
    try:
        section_map = classify_sections(lines)
        new_lines = list(lines)

        # Update Definition Table row
        for idx, line in enumerate(lines):
            stype = section_map.get(idx, SectionType.OTHER)
            if stype == SectionType.AC_DEFINITION_TABLE:
                stripped = line.strip()
                if stripped.startswith("|") and "| AC# |" not in stripped:
                    parts = split_pipe_row(stripped)
                    if len(parts) > 1 and parts[1].strip() == str(ac_num):
                        new_parts = list(parts)
                        # Column layout: ['', AC#, Description, Type, Method, Matcher, Expected, Status, '']
                        if description is not None and len(new_parts) > 2:
                            new_parts[2] = description
                        if method is not None and len(new_parts) > 4:
                            new_parts[4] = method
                        if matcher is not None and len(new_parts) > 5:
                            new_parts[5] = matcher
                        if expected is not None and len(new_parts) > 6:
                            new_parts[6] = expected
                        new_lines[idx] = _rebuild_pipe_row(line, parts, new_parts)
                        break

        # Update AC Details block heading + Expected line
        details_heading_re = re.compile(rf'^\*\*AC#{ac_num}:')
        in_details_block = False
        heading_line_re = re.compile(r'^\*\*AC#\d+:')
        expected_line_re = re.compile(r'^-\s+\*\*Expected\*\*:')
        method_line_re = re.compile(r'^-\s+\*\*(?:Method|Test)\*\*:')

        for idx, line in enumerate(new_lines):
            stripped = line.strip()
            if details_heading_re.match(stripped):
                in_details_block = True
                if description is not None:
                    new_lines[idx] = re.sub(
                        rf'(\*\*AC#{ac_num}:\s*)(.+?)(\*\*)',
                        rf'\g<1>{description}\g<3>',
                        line,
                    )
                continue
            if in_details_block:
                if heading_line_re.match(stripped) or stripped.startswith("## ") or stripped.startswith("### "):
                    in_details_block = False
                    break
                if expected is not None and expected_line_re.match(stripped):
                    new_lines[idx] = re.sub(
                        r'(-\s+\*\*Expected\*\*:\s*)(.+)',
                        rf'\g<1>{expected}',
                        line,
                    )
                if method is not None and method_line_re.match(stripped):
                    new_lines[idx] = re.sub(
                        r'(-\s+\*\*(?:Method|Test)\*\*:\s*)(.+)',
                        rf'\g<1>{method}',
                        line,
                    )

        # --sync-coverage: update AC Coverage table
        if sync_coverage and description is not None:
            cov_section_map = classify_sections(new_lines)
            for idx, line in enumerate(new_lines):
                stype = cov_section_map.get(idx, SectionType.OTHER)
                if stype == SectionType.TECH_DESIGN_AC_COV:
                    stripped = line.strip()
                    if stripped.startswith("|"):
                        parts = split_pipe_row(stripped)
                        if len(parts) > 1 and parts[1].strip() == str(ac_num):
                            new_parts = list(parts)
                            if len(new_parts) > 2:
                                new_parts[2] = description
                            new_lines[idx] = _rebuild_pipe_row(line, parts, new_parts)
                            break

        # Report prose occurrences (don't auto-replace)
        prose_warnings = []
        for idx, line in enumerate(lines):
            stype = section_map.get(idx, SectionType.OTHER)
            if stype in (SectionType.TECH_DESIGN_PROSE, SectionType.REVIEW_NOTES, SectionType.SUCCESS_CRITERIA):
                if old_expected and old_expected != "-" and expected is not None and old_expected in line:
                    prose_warnings.append(
                        f"  Line {idx + 1}: old Expected value found in prose (manual review): {line.rstrip()}"
                    )
                if old_desc and description is not None and old_desc in line:
                    prose_warnings.append(
                        f"  Line {idx + 1}: old Description found in prose (manual review): {line.rstrip()}"
                    )

        # Pre-write validation
        validation_rows = parse_ac_table(new_lines)
        if not validation_rows:
            print("ERROR: Update would destroy AC Definition Table. Aborting.", file=sys.stderr)
            _remove_backup(path)
            return 1

        with open(path, "w", encoding="utf-8") as f:
            f.writelines(new_lines)

        _remove_backup(path)
        print(f"ac-update: Updated AC#{ac_num} successfully.")
        if prose_warnings:
            print("ac-update: WARNING - old values found in prose (not auto-replaced):")
            for w in prose_warnings:
                print(w)

        return 0
    except Exception as e:
        print(f"ERROR: Exception during update: {e}. Restoring backup.", file=sys.stderr)
        _restore_backup(path)
        return 1


def ac_add(
    fid: str,
    desc: str,
    method: str,
    matcher: str,
    expected: str,
    type_: str = "test",
    after: int | None = None,
    task: int | None = None,
    goal: int | None = None,
    coverage: str | None = None,
    derivation: str | None = None,
    rationale: str | None = None,
    dry_run: bool = False,
    force: bool = False,
) -> int:
    """
    Add a new AC to the feature file.
    Creates Definition Table row, optional Details block (for threshold matchers),
    and links to Task/Goal/AC Coverage if specified.
    Returns 0 on success, 1 on error.
    """
    path = feature_path(fid)
    if not path.exists():
        print(f"ERROR: Feature file not found: {path}", file=sys.stderr)
        return 1

    if matcher not in VALID_MATCHERS:
        print(f"ERROR: Invalid matcher '{matcher}'. Valid: {sorted(VALID_MATCHERS)}", file=sys.stderr)
        return 1

    with open(path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    ac_rows = parse_ac_table(lines)
    if not ac_rows:
        print("ERROR: No AC Definition Table found.", file=sys.stderr)
        return 1

    # Sub-numbered guard
    if not force and _detect_sub_numbered_in_lines(lines):
        print(
            "ERROR: Sub-numbered ACs detected (e.g., AC#3b). "
            "Cannot safely add. Use --force to override.",
            file=sys.stderr,
        )
        return 1

    defined_numbers = sorted({row.number for row in ac_rows})
    section_map = classify_sections(lines)

    # Determine new AC number and apply renumber if --after
    new_lines = list(lines)
    if after is not None:
        if after not in set(defined_numbers) and after != 0:
            print(f"ERROR: AC#{after} not found in Definition Table.", file=sys.stderr)
            return 1
        # Shift ACs > after by +1
        rmap = {}
        for num in defined_numbers:
            if num > after:
                rmap[num] = num + 1
            else:
                rmap[num] = num
        changes = {old: new for old, new in rmap.items() if old != new}
        if changes:
            new_lines = apply_renumber_map(new_lines, section_map, rmap, update_counts=False)
            section_map = classify_sections(new_lines)
        new_ac_num = after + 1
    else:
        new_ac_num = max(defined_numbers) + 1

    # Find insertion point in Definition Table
    table_end = _find_ac_table_end(new_lines, section_map)
    if table_end == -1:
        print("ERROR: Could not find AC Definition Table end.", file=sys.stderr)
        return 1

    # If --after, insert after the shifted AC position; else append at table end
    if after is not None:
        # Find the row for `after` (which was NOT shifted)
        insert_after_idx = table_end  # fallback
        for idx, line in enumerate(new_lines):
            stype = section_map.get(idx, SectionType.OTHER)
            if stype == SectionType.AC_DEFINITION_TABLE:
                stripped = line.strip()
                if stripped.startswith("|") and "| AC# |" not in stripped:
                    parts = split_pipe_row(stripped)
                    if len(parts) > 1 and parts[1].strip() == str(after):
                        insert_after_idx = idx
                        break
    else:
        insert_after_idx = table_end

    new_row = _build_ac_row_line(new_ac_num, desc, type_, method, matcher, expected)

    print(f"ac-add: Adding AC#{new_ac_num} to feature-{fid}.md.")

    if matcher in THRESHOLD_MATCHERS:
        if not derivation:
            print("ac-add: WARNING - Threshold matcher without --derivation. AC Details will lack derivation.")
        print(f"ac-add: Will generate AC Details block (threshold matcher '{matcher}').")

    if dry_run:
        print(f"ac-add: [DRY RUN] Would insert: {new_row.rstrip()}")
        if task is not None:
            print(f"ac-add: [DRY RUN] Would link to Task#{task}")
        if goal is not None:
            print(f"ac-add: [DRY RUN] Would link to Goal#{goal}")
        if coverage is not None:
            print(f"ac-add: [DRY RUN] Would add AC Coverage row")
        print("ac-add: [DRY RUN] No changes written.")
        return 0

    backup = _create_backup(path)
    try:
        # Insert new row into Definition Table
        new_lines.insert(insert_after_idx + 1, new_row)
        # Reclassify after insertion
        section_map = classify_sections(new_lines)

        # Add AC Details block if threshold matcher
        if matcher in THRESHOLD_MATCHERS:
            details_block = _build_details_block(
                new_ac_num, desc, method, expected, derivation, rationale
            )
            insert_pt = _find_details_insertion_point(new_lines, section_map)
            if insert_pt != -1:
                for i, detail_line in enumerate(details_block):
                    new_lines.insert(insert_pt + i, detail_line)
                section_map = classify_sections(new_lines)

        # Link to Task
        if task is not None:
            new_lines = _add_ac_to_task_row(new_lines, section_map, task, new_ac_num)
            section_map = classify_sections(new_lines)

        # Link to Goal
        if goal is not None:
            new_lines = _add_ac_to_goal_row(new_lines, section_map, goal, new_ac_num)
            section_map = classify_sections(new_lines)

        # Add AC Coverage row
        if coverage is not None:
            new_lines = _add_ac_coverage_row(new_lines, section_map, new_ac_num, coverage)

        # Pre-write validation
        validation_rows = parse_ac_table(new_lines)
        if not validation_rows:
            print("ERROR: Add would destroy AC Definition Table. Aborting.", file=sys.stderr)
            _remove_backup(path)
            return 1
        if len(validation_rows) != len(ac_rows) + 1:
            print(
                f"ERROR: Expected {len(ac_rows) + 1} ACs after add, got {len(validation_rows)}. Aborting.",
                file=sys.stderr,
            )
            _remove_backup(path)
            return 1

        with open(path, "w", encoding="utf-8") as f:
            f.writelines(new_lines)

        _remove_backup(path)
        print(f"ac-add: Wrote updated feature-{fid}.md (AC#{new_ac_num} added, total {len(validation_rows)} ACs).")
        return 0
    except Exception as e:
        print(f"ERROR: Exception during add: {e}. Restoring backup.", file=sys.stderr)
        _restore_backup(path)
        return 1


# =============================================================================
# CLI entry point (when run directly for testing)
# =============================================================================


def main() -> int:
    import argparse

    parser = argparse.ArgumentParser(
        description="AC (Acceptance Criteria) manipulation for feature-{ID}.md files.",
        epilog=(
            "examples:\n"
            "  python ac_ops.py ac-check 808                Check F808 AC consistency\n"
            "  python ac_ops.py ac-check 808 --fix          Auto-fix deterministic issues\n"
            "  python ac_ops.py ac-check 808 --fix --dry-run  Preview fixes without writing\n"
            "  python ac_ops.py ac-check 808 --warn-count   Include AC count limit checks\n"
            "  python ac_ops.py ac-check 808 --skip N12     Suppress gte derivation check\n"
            "  python ac_ops.py ac-renumber 808             Close numbering gaps in F808\n"
            "  python ac_ops.py ac-renumber 808 --dry-run   Preview without writing\n"
            "  python ac_ops.py ac-insert 808 --after 5     Insert slot after AC#5\n"
            "  python ac_ops.py ac-delete 808 --ac 3        Delete AC#3 and renumber\n"
            "  python ac_ops.py ac-fix 808 --ac 3 --expected '`pattern`'\n"
            "  python ac_ops.py ac-link 813 --ac 25 --task 5 --goal 1\n"
            "  python ac_ops.py ac-update 813 --ac 7 --description 'New desc' --expected '>=3'\n"
            "  python ac_ops.py ac-add 813 --description 'test' --method 'echo' --matcher succeeds --expected '-'\n"
            "\n"
            "also callable via: python tools/feature-status.py ac-check 808"
        ),
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    subparsers = parser.add_subparsers(dest="command", required=True)

    # ac-check
    p_check = subparsers.add_parser(
        "ac-check",
        help="Check AC consistency (gaps, orphan refs, count mismatches)",
        description=(
            "Validate AC Definition Table, AC Details, Goal Coverage, Tasks,\n"
            "and Tech Design AC Coverage for consistency issues.\n"
            "\n"
            "Checks: definition/details mismatch, orphan task refs, goal coverage gaps,\n"
            "numbering gaps, stale 'All N ACs' counts, tech design coverage mismatch,\n"
            "invalid matchers (N1), regex errors (N3), column swaps (N4),\n"
            "unassigned ACs (N7), goal coverage refs (N9), gte derivation (N12).\n"
            "AC count limits (N10/N11) are opt-in via --warn-count.\n"
            "\n"
            "With --fix: auto-correct invalid matchers, stale counts, and numbering gaps.\n"
            "Non-fixable issues (N3, N4, N7, N9, N10, N11, N12) are printed but do not\n"
            "affect the exit code when --fix is used.\n"
            "\n"
            "Exit code:\n"
            "  Without --fix: 0 = no issues, 1 = issues found.\n"
            "  With --fix:    0 = fixable issues corrected (or none found),\n"
            "                 1 = nothing was fixable and issues remain."
        ),
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    p_check.add_argument("fid", metavar="FID", help="Feature ID (e.g. 808)")
    p_check.add_argument("--fix", action="store_true",
                          help="Auto-fix deterministic issues (invalid matchers, gaps, stale counts)")
    p_check.add_argument("--dry-run", action="store_true",
                          help="With --fix: show what would be fixed without writing")
    p_check.add_argument("--skip", default="",
                          help="Comma-separated check IDs to suppress (e.g. N12)")
    p_check.add_argument("--warn-count", action="store_true",
                          help="Enable AC count limit checks (N10: >50, N11: >30). Off by default.")

    # ac-renumber
    p_renumber = subparsers.add_parser(
        "ac-renumber",
        help="Close numbering gaps (e.g. 1,2,5 -> 1,2,3)",
        description=(
            "Detect gaps in AC numbering and renumber to make them contiguous.\n"
            "Updates all cross-references: Philosophy Derivation, AC Details,\n"
            "Goal Coverage, Tasks, Tech Design, Success Criteria, etc.\n"
            "Creates .md.bak backup (auto-cleaned on success)."
        ),
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    p_renumber.add_argument("fid", metavar="FID", help="Feature ID (e.g. 808)")
    p_renumber.add_argument("--dry-run", action="store_true",
                            help="Show renumber map without writing changes")
    p_renumber.add_argument("--force", action="store_true",
                            help="Allow renumber even with sub-numbered ACs (e.g. AC#3b)")

    # ac-insert
    p_insert = subparsers.add_parser(
        "ac-insert",
        help="Insert AC slot and shift subsequent numbers",
        description=(
            "Insert a new AC number after the given position.\n"
            "Shifts all AC# > --after by +1. Does NOT add a table row\n"
            "or Details block -- add those manually after insertion."
        ),
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    p_insert.add_argument("fid", metavar="FID", help="Feature ID (e.g. 808)")
    p_insert.add_argument("--after", type=int, required=True,
                          help="Insert new AC after this AC# (0 = before all)")
    p_insert.add_argument("--dry-run", action="store_true",
                          help="Show shift map without writing changes")
    p_insert.add_argument("--force", action="store_true",
                          help="Allow insert even with sub-numbered ACs")

    # ac-delete
    p_delete = subparsers.add_parser(
        "ac-delete",
        help="Delete AC row + details block, then renumber",
        description=(
            "Remove the specified AC from the Definition Table and its\n"
            "AC Details block. Cleans references from Goal Coverage and\n"
            "Tasks, then renumbers remaining ACs to close the gap."
        ),
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    p_delete.add_argument("fid", metavar="FID", help="Feature ID (e.g. 808)")
    p_delete.add_argument("--ac", type=int, required=True, dest="ac_num",
                          help="AC# to delete")
    p_delete.add_argument("--dry-run", action="store_true",
                          help="Show what would be deleted without writing")
    p_delete.add_argument("--force", action="store_true",
                          help="Allow delete even with sub-numbered ACs")

    # ac-fix
    p_fix = subparsers.add_parser(
        "ac-fix",
        help="Update Expected/Description columns for one AC",
        description=(
            "Surgically update the Expected and/or Description columns\n"
            "of a single AC in the Definition Table and AC Details.\n"
            "Reports (but does not auto-replace) old values in prose sections."
        ),
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    p_fix.add_argument("fid", metavar="FID", help="Feature ID (e.g. 808)")
    p_fix.add_argument("--ac", type=int, required=True, dest="ac_num",
                       help="AC# to update")
    p_fix.add_argument("--expected", default=None,
                       help="New Expected column value (e.g. '`pattern`', 'gte 5')")
    p_fix.add_argument("--description", default=None,
                       help="New Description column value")
    p_fix.add_argument("--dry-run", action="store_true",
                       help="Show changes without writing")

    # ac-link
    p_link = subparsers.add_parser(
        "ac-link",
        help="Link existing AC to Task and/or Goal rows",
        description=(
            "Add AC# to a Task's AC# column and/or a Goal's Covering AC(s) column.\n"
            "Idempotent: skips if the link already exists."
        ),
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    p_link.add_argument("fid", metavar="FID", help="Feature ID (e.g. 813)")
    p_link.add_argument("--ac", type=int, required=True, dest="ac_num",
                        help="AC# to link")
    p_link.add_argument("--task", type=int, default=None,
                        help="Task# to add AC reference to")
    p_link.add_argument("--goal", type=int, default=None,
                        help="Goal# to add AC reference to")
    p_link.add_argument("--dry-run", action="store_true",
                        help="Show changes without writing")

    # ac-update
    p_update = subparsers.add_parser(
        "ac-update",
        help="Update AC metadata (description, expected, matcher, method)",
        description=(
            "Update one or more columns of an AC in the Definition Table and AC Details.\n"
            "Extended version of ac-fix with matcher/method support.\n"
            "With --sync-coverage: also update Tech Design AC Coverage row."
        ),
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    p_update.add_argument("fid", metavar="FID", help="Feature ID (e.g. 813)")
    p_update.add_argument("--ac", type=int, required=True, dest="ac_num",
                          help="AC# to update")
    p_update.add_argument("--description", default=None,
                          help="New Description column value")
    p_update.add_argument("--expected", default=None,
                          help="New Expected column value")
    p_update.add_argument("--matcher", default=None,
                          help="New Matcher column value")
    p_update.add_argument("--method", default=None,
                          help="New Method column value")
    p_update.add_argument("--sync-coverage", action="store_true",
                          help="Also update Tech Design AC Coverage row")
    p_update.add_argument("--dry-run", action="store_true",
                          help="Show changes without writing")

    # ac-add
    p_add = subparsers.add_parser(
        "ac-add",
        help="Add new AC with full linkage (table row + details + task/goal/coverage)",
        description=(
            "Add a new AC to the feature file. Creates the Definition Table row,\n"
            "optionally generates AC Details block (for threshold matchers),\n"
            "and links to Task/Goal/AC Coverage in one command.\n"
            "Use --after N to insert after AC#N (shifts subsequent ACs by +1)."
        ),
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    p_add.add_argument("fid", metavar="FID", help="Feature ID (e.g. 813)")
    p_add.add_argument("--description", required=True,
                       help="AC description text")
    p_add.add_argument("--type", default="test", dest="type_",
                       help="AC type (default: test)")
    p_add.add_argument("--method", required=True,
                       help="Test method (e.g. 'Grep ...', 'dotnet test ...')")
    p_add.add_argument("--matcher", required=True,
                       help=f"Matcher name: {sorted(VALID_MATCHERS)}")
    p_add.add_argument("--expected", required=True,
                       help="Expected value")
    p_add.add_argument("--after", type=int, default=None,
                       help="Insert after this AC# (shifts subsequent). Omit to append at end.")
    p_add.add_argument("--task", type=int, default=None,
                       help="Task# to link the new AC to")
    p_add.add_argument("--goal", type=int, default=None,
                       help="Goal# to link the new AC to")
    p_add.add_argument("--coverage", default=None,
                       help="Text for Tech Design AC Coverage row")
    p_add.add_argument("--derivation", default=None,
                       help="Derivation text for AC Details (threshold matchers)")
    p_add.add_argument("--rationale", default=None,
                       help="Rationale text for AC Details (threshold matchers)")
    p_add.add_argument("--dry-run", action="store_true",
                       help="Show changes without writing")
    p_add.add_argument("--force", action="store_true",
                       help="Allow add even with sub-numbered ACs")

    args = parser.parse_args()

    if args.command == "ac-check":
        skip = frozenset(s.strip() for s in args.skip.split(",") if s.strip()) if args.skip else frozenset()
        issues = ac_check(args.fid, fix=args.fix, dry_run=args.dry_run,
                          skip=skip, warn_count=args.warn_count)
        return 1 if issues else 0
    elif args.command == "ac-renumber":
        return ac_renumber(args.fid, dry_run=args.dry_run, force=args.force)
    elif args.command == "ac-insert":
        return ac_insert(args.fid, after=args.after, dry_run=args.dry_run, force=args.force)
    elif args.command == "ac-delete":
        return ac_delete(args.fid, ac_num=args.ac_num, dry_run=args.dry_run, force=args.force)
    elif args.command == "ac-fix":
        return ac_fix(
            args.fid,
            ac_num=args.ac_num,
            expected=args.expected,
            description=args.description,
            dry_run=args.dry_run,
        )
    elif args.command == "ac-link":
        return ac_link(
            args.fid,
            ac_num=args.ac_num,
            task=args.task,
            goal=args.goal,
            dry_run=args.dry_run,
        )
    elif args.command == "ac-update":
        return ac_update(
            args.fid,
            ac_num=args.ac_num,
            description=args.description,
            expected=args.expected,
            matcher=args.matcher,
            method=args.method,
            sync_coverage=args.sync_coverage,
            dry_run=args.dry_run,
        )
    elif args.command == "ac-add":
        return ac_add(
            args.fid,
            desc=args.description,
            method=args.method,
            matcher=args.matcher,
            expected=args.expected,
            type_=args.type_,
            after=args.after,
            task=args.task,
            goal=args.goal,
            coverage=args.coverage,
            derivation=args.derivation,
            rationale=args.rationale,
            dry_run=args.dry_run,
            force=args.force,
        )
    return 0


if __name__ == "__main__":
    sys.exit(main())
