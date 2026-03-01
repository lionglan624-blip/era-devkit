"""
Unit tests for ac_ops parser layer functions.

Tests: split_pipe_row, parse_ac_table, classify_sections,
       scan_ac_references, build_renumber_map
"""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from ac_ops import (
    split_pipe_row,
    parse_ac_table,
    classify_sections,
    scan_ac_references,
    build_renumber_map,
    SectionType,
)


# =============================================================================
# split_pipe_row
# =============================================================================


def test_split_pipe_row_basic():
    """Basic table row splitting."""
    result = split_pipe_row("| 1 | desc | code |")
    assert result == ["", "1", "desc", "code", ""]


def test_split_pipe_row_backtick_pipe():
    """Pipes inside backticks are not split."""
    result = split_pipe_row("| 1 | `a|b` | code |")
    assert result[2] == "`a|b`"


def test_split_pipe_row_escaped_pipe():
    """Backslash-escaped pipes are treated as content."""
    result = split_pipe_row("| 1 | a\\|b | code |")
    assert result[2] == "a|b"


def test_split_pipe_row_quoted_pipe():
    """Pipes inside double quotes are not split."""
    result = split_pipe_row('| 1 | "a|b" | code |')
    assert result[2] == '"a|b"'


def test_split_pipe_row_empty_cells():
    """Empty cells."""
    result = split_pipe_row("|  |  |  |")
    assert result == ["", "", "", "", ""]


# =============================================================================
# parse_ac_table
# =============================================================================


def test_parse_ac_table_basic():
    """Parse a minimal AC table."""
    lines = [
        "### AC Definition Table\n",
        "\n",
        "| AC# | Description | Type | Method | Matcher | Expected | Status |\n",
        "|:---:|-------------|------|--------|---------|----------|:------:|\n",
        "| 1 | First AC | file | Glob(x) | exists | - | [ ] |\n",
        "| 2 | Second AC | code | Grep(y) | matches | `pattern` | [ ] |\n",
        "\n",
    ]
    rows = parse_ac_table(lines)
    assert len(rows) == 2
    assert rows[0].number == 1
    assert rows[0].description == "First AC"
    assert rows[0].line_index == 4
    assert rows[1].number == 2


def test_parse_ac_table_skip_non_digit():
    """Skip rows where AC# column is not a digit."""
    lines = [
        "| AC# | Description | Type | Method | Matcher | Expected | Status |\n",
        "|:---:|-------------|------|--------|---------|----------|:------:|\n",
        "| R1 | Reserved | - | - | - | - | [ ] |\n",
        "| 1 | Real AC | file | Glob(x) | exists | - | [ ] |\n",
        "\n",
    ]
    rows = parse_ac_table(lines)
    assert len(rows) == 1
    assert rows[0].number == 1


# =============================================================================
# classify_sections
# =============================================================================


def test_classify_sections_basic():
    """Classify sections in a feature file structure."""
    lines = [
        "## Acceptance Criteria\n",           # 0
        "\n",                                   # 1
        "### Philosophy Derivation\n",         # 2
        "\n",                                   # 3
        "| claim | AC#1 |\n",                  # 4
        "\n",                                   # 5
        "### AC Definition Table\n",           # 6
        "\n",                                   # 7
        "| AC# | Description |\n",             # 8
        "| 1 | test |\n",                      # 9
        "\n",                                   # 10
        "### AC Details\n",                    # 11
        "\n",                                   # 12
        "**AC#1: test**\n",                    # 13
        "\n",                                   # 14
        "### Goal Coverage Verification\n",    # 15
        "\n",                                   # 16
        "| 1 | AC#1 |\n",                      # 17
        "\n",                                   # 18
        "## Tasks\n",                           # 19
        "\n",                                   # 20
        "| Task# | AC# | Description |\n",     # 21
        "| 1 | 1 | do stuff |\n",              # 22
        "\n",                                   # 23
        "## Technical Design\n",               # 24
        "\n",                                   # 25
        "### Approach\n",                      # 26
        "\n",                                   # 27
        "All 1 ACs blah AC#1\n",               # 28
        "\n",                                   # 29
        "### AC Coverage\n",                   # 30
        "\n",                                   # 31
        "| AC# | How |\n",                     # 32
        "| 1 | do thing |\n",                  # 33
        "\n",                                   # 34
        "## Implementation Contract\n",        # 35
        "\n",                                   # 36
        "### Success Criteria\n",              # 37
        "\n",                                   # 38
        "All 1 ACs pass: AC#1\n",              # 39
        "\n",                                   # 40
        "## Review Notes\n",                   # 41
        "\n",                                   # 42
        "- [fix] AC6 count\n",                 # 43
        "\n",                                   # 44
        "## Execution Log\n",                  # 45
        "\n",                                   # 46
        "| Event | AC#1 updated |\n",          # 47
    ]
    section_map = classify_sections(lines)
    # Philosophy Derivation
    assert section_map[4] == SectionType.PHILOSOPHY_DERIVATION
    # AC Definition Table
    assert section_map[9] == SectionType.AC_DEFINITION_TABLE
    # AC Details
    assert section_map[13] == SectionType.AC_DETAILS
    # Goal Coverage
    assert section_map[17] == SectionType.GOAL_COVERAGE
    # Tasks
    assert section_map[22] == SectionType.TASKS_TABLE
    # Tech Design Prose (Approach heading)
    assert section_map[28] == SectionType.TECH_DESIGN_PROSE
    # Tech Design AC Coverage
    assert section_map[33] == SectionType.TECH_DESIGN_AC_COV
    # Success Criteria
    assert section_map[39] == SectionType.SUCCESS_CRITERIA
    # Review Notes
    assert section_map[43] == SectionType.REVIEW_NOTES
    # Execution Log
    assert section_map[47] == SectionType.EXECUTION_LOG


def test_classify_html_comment():
    """HTML comments are always OTHER."""
    lines = [
        "## Technical Design\n",
        "<!-- this is a comment -->\n",
        "some prose\n",
    ]
    section_map = classify_sections(lines)
    assert section_map[1] == SectionType.OTHER
    assert section_map[2] == SectionType.TECH_DESIGN_PROSE


# =============================================================================
# scan_ac_references
# =============================================================================


def test_scan_ac_references_prefixed():
    """Scan prefixed AC references."""
    lines = [
        "## Acceptance Criteria\n",
        "### Philosophy Derivation\n",
        "| claim | AC#1, AC#2 |\n",
    ]
    section_map = classify_sections(lines)
    refs = scan_ac_references(lines, section_map)
    assert len(refs) == 2
    assert refs[0].ac_number == 1
    assert refs[1].ac_number == 2


def test_scan_ac_references_tasks_bare():
    """Scan bare AC numbers in Tasks table."""
    lines = [
        "## Tasks\n",
        "| Task# | AC# | Description | Tag | Status |\n",
        "|:-----:|:---:|-------------|:---:|:------:|\n",
        "| 1 | 2, 3 | Create AC#5 stuff | | [ ] |\n",
    ]
    section_map = classify_sections(lines)
    refs = scan_ac_references(lines, section_map)
    bare_refs = [r for r in refs if r.format == "bare"]
    prefixed_refs = [r for r in refs if r.format == "prefixed"]
    assert len(bare_refs) == 2
    assert set(r.ac_number for r in bare_refs) == {2, 3}
    assert len(prefixed_refs) == 1
    assert prefixed_refs[0].ac_number == 5


def test_scan_ac_references_range():
    """Scan range references in Success Criteria."""
    lines = [
        "## Implementation Contract\n",
        "### Success Criteria\n",
        "AC#1-3 pass\n",
    ]
    section_map = classify_sections(lines)
    refs = scan_ac_references(lines, section_map)
    assert len(refs) == 3
    assert set(r.ac_number for r in refs) == {1, 2, 3}


def test_scan_ac_references_informal():
    """Scan informal AC references in Review Notes."""
    lines = [
        "## Review Notes\n",
        "- [fix] AC6 count changed\n",
    ]
    section_map = classify_sections(lines)
    refs = scan_ac_references(lines, section_map)
    assert len(refs) == 1
    assert refs[0].ac_number == 6
    assert refs[0].format == "informal"


# =============================================================================
# build_renumber_map
# =============================================================================


def test_build_renumber_map_with_gaps():
    """Build map that closes gaps."""
    rmap = build_renumber_map([1, 2, 4, 5])
    assert rmap == {1: 1, 2: 2, 4: 3, 5: 4}


def test_build_renumber_map_no_gaps():
    """No gaps means identity map."""
    rmap = build_renumber_map([1, 2, 3])
    assert rmap == {1: 1, 2: 2, 3: 3}


def test_build_renumber_map_single():
    """Single number."""
    rmap = build_renumber_map([5])
    assert rmap == {5: 1}
