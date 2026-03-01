"""
Unit tests for ac_ops renumber, insert, and delete commands.

Tests: apply_renumber_map, ac_renumber, ac_insert, ac_delete, residual_scan
"""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import pytest
from ac_ops import (
    ac_renumber,
    ac_insert,
    ac_delete,
    apply_renumber_map,
    classify_sections,
    build_renumber_map,
    residual_scan,
    split_pipe_row,
    _rebuild_pipe_row,
    _find_unescaped_pipes,
    SectionType,
)
import ac_ops


FEATURE_WITH_GAP = """# Feature 999: Test

## Status: [DRAFT]

## Acceptance Criteria

### Philosophy Derivation

| Claim | Coverage |
|-------|----------|
| test | AC#1, AC#3 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | First AC | file | Glob(x) | exists | - | [ ] |
| 3 | Third AC | code | Grep(y) | matches | pattern | [ ] |

### AC Details

**AC#1: First AC**
- **Test**: Glob x
- **Expected**: exists

**AC#3: Third AC**
- **Test**: Grep y
- **Expected**: pattern

### Goal Coverage Verification

| Goal | Covering ACs |
|------|-------------|
| 1 | AC#1, AC#3 |

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 3 | Do stuff | | [ ] |

## Technical Design

### Approach

All 3 ACs are satisfied.

### AC Coverage

| AC# | How |
|:---:|-----|
| 1 | do thing 1 |
| 3 | do thing 3 |

## Implementation Contract

### Success Criteria

All 3 ACs pass:
- AC#1, AC#3

## Review Notes

- [fix] AC3 count

## Execution Log

| Event | AC#3 updated |
"""


@pytest.fixture
def feature_file(tmp_path, monkeypatch):
    """Create a temp feature file and point ac_ops.AGENTS_DIR to tmp_path."""
    agents = tmp_path / "Game" / "agents"
    agents.mkdir(parents=True)
    monkeypatch.setattr(ac_ops, "AGENTS_DIR", agents)

    def write_feature(fid, content):
        p = agents / f"feature-{fid}.md"
        p.write_text(content, encoding="utf-8")
        return p

    return write_feature


# =============================================================================
# apply_renumber_map
# =============================================================================


def test_apply_renumber_map_basic():
    """Renumber AC#3 to AC#2."""
    lines = [
        "## Acceptance Criteria\n",
        "### Philosophy Derivation\n",
        "| test | AC#1, AC#3 |\n",
        "### AC Definition Table\n",
        "| AC# | Description | Type | Method | Matcher | Expected | Status |\n",
        "|:---:|-------------|------|--------|---------|----------|:------:|\n",
        "| 1 | First | file | Glob(x) | exists | - | [ ] |\n",
        "| 3 | Third | code | Grep(y) | matches | pat | [ ] |\n",
        "### AC Details\n",
        "**AC#1: First**\n",
        "**AC#3: Third**\n",
    ]
    section_map = classify_sections(lines)
    rmap = {1: 1, 3: 2}
    result = apply_renumber_map(lines, section_map, rmap)
    # Check Philosophy Derivation
    assert "AC#2" in result[2]
    assert "AC#3" not in result[2]
    # Check Definition Table
    assert "| 2 |" in result[7]
    # Check AC Details
    assert "**AC#2:" in result[10]


def test_apply_renumber_negative_lookahead():
    """AC#1 must not match inside AC#10."""
    lines = [
        "## Acceptance Criteria\n",
        "### Philosophy Derivation\n",
        "| test | AC#1, AC#10 |\n",
    ]
    section_map = classify_sections(lines)
    rmap = {1: 1, 10: 9}
    result = apply_renumber_map(lines, section_map, rmap)
    assert "AC#1," in result[2]
    assert "AC#9" in result[2]
    assert "AC#10" not in result[2]


# =============================================================================
# ac_renumber
# =============================================================================


def test_ac_renumber_closes_gap(feature_file):
    """ac_renumber should close gaps."""
    p = feature_file("999", FEATURE_WITH_GAP)
    result = ac_renumber("999")
    assert result == 0
    content = p.read_text(encoding="utf-8")
    assert "AC#2" in content
    # AC#3 should not appear before Review Notes (it was the gap number, now gone)
    assert "AC#3" not in content.split("## Review Notes")[0]


def test_ac_renumber_no_gap(feature_file, capsys):
    """ac_renumber with no gaps does nothing."""
    content = (
        FEATURE_WITH_GAP
        .replace("| 3 |", "| 2 |")
        .replace("AC#3", "AC#2")
        .replace("All 3", "All 2")
    )
    feature_file("999", content)
    result = ac_renumber("999")
    assert result == 0
    captured = capsys.readouterr()
    assert "Nothing to do" in captured.out


def test_ac_renumber_dry_run(feature_file, capsys):
    """ac_renumber --dry-run doesn't modify file."""
    p = feature_file("999", FEATURE_WITH_GAP)
    result = ac_renumber("999", dry_run=True)
    assert result == 0
    # File should be unchanged
    content = p.read_text(encoding="utf-8")
    assert "AC#3" in content
    captured = capsys.readouterr()
    assert "DRY RUN" in captured.out


# =============================================================================
# ac_insert
# =============================================================================


def test_ac_insert_shifts(feature_file):
    """ac_insert shifts AC numbers after insertion point."""
    # Use the no-gap version (ACs 1 and 2)
    content = (
        FEATURE_WITH_GAP
        .replace("| 3 |", "| 2 |")
        .replace("AC#3", "AC#2")
        .replace("All 3", "All 2")
    )
    p = feature_file("999", content)
    result = ac_insert("999", after=1)
    assert result == 0
    new_content = p.read_text(encoding="utf-8")
    # Old AC#2 should have become AC#3
    assert "AC#3" in new_content


# =============================================================================
# ac_delete
# =============================================================================


def test_ac_delete_removes_and_renumbers(feature_file):
    """ac_delete removes AC and renumbers."""
    content = (
        FEATURE_WITH_GAP
        .replace("| 3 |", "| 2 |")
        .replace("AC#3", "AC#2")
        .replace("All 3", "All 2")
    )
    p = feature_file("999", content)
    result = ac_delete("999", ac_num=1)
    assert result == 0
    new_content = p.read_text(encoding="utf-8")
    # Old AC#2 should now be AC#1
    assert "**AC#1:" in new_content


# =============================================================================
# residual_scan
# =============================================================================


def test_residual_scan_finds_stale():
    """residual_scan detects remaining old references."""
    lines = [
        "## Background\n",
        "This mentions AC#5 which should not be here.\n",
    ]
    section_map = {0: SectionType.OTHER, 1: SectionType.OTHER}
    rmap = {5: 3}
    warnings = residual_scan(lines, section_map, rmap)
    assert len(warnings) >= 1
    assert "AC#5" in warnings[0]


# =============================================================================
# _rebuild_pipe_row (column-positional safety)
# =============================================================================


def test_rebuild_pipe_row_no_substring_corruption():
    """Renumber must not corrupt long alternation patterns in Expected column."""
    line = "| 1 | DI check | code | Grep(Foo.cs) | gte | `IVariableStore\\|IVirginityManager\\|IRandomProvider` = 13 | [ ] |\n"
    parts = split_pipe_row(line.strip())
    modified = list(parts)
    modified[1] = "2"  # Renumber 1 -> 2
    result = _rebuild_pipe_row(line, parts, modified)
    assert "| 2 |" in result
    assert "IVariableStore" in result  # Not corrupted
    assert "IRandomProvider" in result  # Not corrupted
    assert "= 13" in result  # Count not changed


def test_rebuild_pipe_row_preserves_padding():
    """Rebuilt row preserves original padding."""
    line = "| 1 | First AC | file | Glob(x) | exists | - | [ ] |\n"
    parts = split_pipe_row(line.strip())
    modified = list(parts)
    modified[1] = "2"
    result = _rebuild_pipe_row(line, parts, modified)
    assert result == "| 2 | First AC | file | Glob(x) | exists | - | [ ] |\n"


def test_find_unescaped_pipes_basic():
    """Basic pipe detection."""
    positions = _find_unescaped_pipes("| 1 | desc | code |")
    assert positions == [0, 4, 11, 18]


def test_find_unescaped_pipes_backtick():
    """Pipes in backticks are not counted."""
    positions = _find_unescaped_pipes("| 1 | `a|b` | code |")
    assert len(positions) == 4  # Only the 4 structural pipes


def test_find_unescaped_pipes_escaped():
    """Escaped pipes are not counted."""
    positions = _find_unescaped_pipes("| 1 | a\\|b | code |")
    assert len(positions) == 4  # backslash-pipe is not structural


def test_rebuild_pipe_row_no_change():
    """No-op when parts are identical."""
    line = "| 1 | desc | code |\n"
    parts = split_pipe_row(line.strip())
    result = _rebuild_pipe_row(line, parts, list(parts))
    assert result == line


def test_ac_renumber_creates_no_leftover_backup(feature_file):
    """ac_renumber should not leave .bak files behind on success."""
    p = feature_file("999", FEATURE_WITH_GAP)
    result = ac_renumber("999")
    assert result == 0
    bak = p.with_suffix('.md.bak')
    assert not bak.exists(), "Backup file should be cleaned up on success"
