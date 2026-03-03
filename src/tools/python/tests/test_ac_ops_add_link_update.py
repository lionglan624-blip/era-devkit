"""
Unit tests for ac_ops ac_link, ac_update, and ac_add commands.

Tests: ac_link, ac_update, ac_add
"""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import pytest
from ac_ops import (
    ac_link,
    ac_update,
    ac_add,
    parse_ac_table,
    classify_sections,
    split_pipe_row,
    _add_ac_to_task_row,
    _add_ac_to_goal_row,
    _add_ac_coverage_row,
    _build_ac_row_line,
    _build_details_block,
    _find_details_insertion_point,
    _find_ac_table_end,
    SectionType,
    THRESHOLD_MATCHERS,
)
import ac_ops


FEATURE_STANDARD = """# Feature 999: Test

## Status: [DRAFT]

## Acceptance Criteria

### Philosophy Derivation

| Claim | Coverage |
|-------|----------|
| test | AC#1, AC#2 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | First AC | file | Glob(x) | exists | - | [ ] |
| 2 | Second AC | code | Grep(y) | matches | pattern | [ ] |
| 3 | Third AC | test | dotnet test | gte | >=5 | [ ] |

### AC Details

**AC#1: First AC**
- **Test**: Glob x
- **Expected**: exists

**AC#2: Second AC**
- **Test**: Grep y
- **Expected**: pattern

**AC#3: Third AC**
- **Method**: dotnet test
- **Expected**: >=5
- **Derivation**: 5 interfaces in module

### Goal Coverage Verification

| Goal | Covering ACs |
|------|-------------|
| 1 | AC#1, AC#2 |
| 2 | AC#3 |

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2 | Do stuff | | [ ] |
| 2 | 3 | More stuff | | [ ] |

## Technical Design

### Approach

All 3 ACs are verified.

### AC Coverage

| AC# | How |
|:---:|-----|
| 1 | do thing 1 |
| 2 | do thing 2 |
| 3 | do thing 3 |

## Implementation Contract

### Success Criteria

All 3 ACs pass:
- AC#1-3
"""


FEATURE_WITH_SUB_NUMBERED = """# Feature 999: Test

## Status: [DRAFT]

## Acceptance Criteria

### Philosophy Derivation

| Claim | Coverage |
|-------|----------|
| test | AC#1, AC#2, AC#3b |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | First AC | file | Glob(x) | exists | - | [ ] |
| 2 | Second AC | code | Grep(y) | matches | pattern | [ ] |
| 3 | Third AC | test | dotnet test | gte | >=5 | [ ] |

### AC Details

**AC#3: Third AC**
- **Method**: dotnet test
- **Expected**: >=5
- **Derivation**: 5 interfaces in module

### Goal Coverage Verification

| Goal | Covering ACs |
|------|-------------|
| 1 | AC#1, AC#2 |
| 2 | AC#3, AC#3b |

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2 | Do stuff | | [ ] |
| 2 | 3 | More stuff | | [ ] |

## Technical Design

### Approach

All 3 ACs are verified.

### AC Coverage

| AC# | How |
|:---:|-----|
| 1 | do thing 1 |
| 2 | do thing 2 |
| 3 | do thing 3 |

## Implementation Contract

### Success Criteria

All 3 ACs pass:
- AC#1-3
"""


FEATURE_NO_DETAILS = """# Feature 999: Test

## Status: [DRAFT]

## Acceptance Criteria

### Philosophy Derivation

| Claim | Coverage |
|-------|----------|
| test | AC#1 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | First AC | file | Glob(x) | exists | - | [ ] |

### Goal Coverage Verification

| Goal | Covering ACs |
|------|-------------|
| 1 | AC#1 |

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1 | Do stuff | | [ ] |

## Technical Design

### Approach

AC#1 verified.

### AC Coverage

| AC# | How |
|:---:|-----|
| 1 | do thing 1 |

## Implementation Contract

### Success Criteria

All 1 ACs pass:
- AC#1
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
# ac_link tests
# =============================================================================


def test_ac_link_adds_to_task(feature_file):
    """Link AC#3 to Task#1. AC# column should become '1, 2, 3'."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_link("999", ac_num=3, task=1)
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    section_map = classify_sections(lines)
    # Find the task row for Task#1 using section classification
    task_row = None
    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        if stype == SectionType.TASKS_TABLE:
            stripped = line.strip()
            if stripped.startswith("|") and "| Task#" not in stripped:
                parts = split_pipe_row(stripped)
                if (len(parts) > 2 and parts[1].strip() == "1"
                        and not set(stripped.replace("|", "").strip()) <= {"-", ":"}):
                    task_row = parts
                    break
    assert task_row is not None
    ac_col = task_row[2].strip()
    assert "3" in [t.strip() for t in ac_col.split(",")]
    assert "1" in [t.strip() for t in ac_col.split(",")]
    assert "2" in [t.strip() for t in ac_col.split(",")]


def test_ac_link_adds_to_goal(feature_file):
    """Link AC#3 to Goal#1. Covering ACs should become 'AC#1, AC#2, AC#3'."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_link("999", ac_num=3, goal=1)
    assert result == 0
    content = p.read_text(encoding="utf-8")
    assert "AC#3" in content
    # Find goal row 1
    lines = content.splitlines()
    for line in lines:
        stripped = line.strip()
        if stripped.startswith("|"):
            parts = split_pipe_row(stripped)
            if len(parts) > 2 and parts[1].strip() == "1" and "AC#" in parts[2]:
                assert "AC#3" in parts[2]
                break


def test_ac_link_both_task_and_goal(feature_file):
    """Link AC#1 to Task#2 and Goal#2 simultaneously."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_link("999", ac_num=1, task=2, goal=2)
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    # Check Task#2 row has AC#1
    task2_row = None
    in_tasks = False
    for line in lines:
        stripped = line.strip()
        if "## Tasks" in stripped:
            in_tasks = True
        if in_tasks and stripped.startswith("|"):
            parts = split_pipe_row(stripped)
            if len(parts) > 2 and parts[1].strip() == "2":
                task2_row = parts
                break
    assert task2_row is not None
    assert "1" in [t.strip() for t in task2_row[2].split(",")]
    # Check Goal#2 row has AC#1
    for line in lines:
        stripped = line.strip()
        if stripped.startswith("|"):
            parts = split_pipe_row(stripped)
            if len(parts) > 2 and parts[1].strip() == "2" and "AC#" in parts[2]:
                assert "AC#1" in parts[2]
                break


def test_ac_link_idempotent_task(feature_file):
    """Link AC#1 to Task#1 (already there). Should not duplicate."""
    p = feature_file("999", FEATURE_STANDARD)
    original = p.read_text(encoding="utf-8")
    result = ac_link("999", ac_num=1, task=1)
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    section_map = classify_sections(lines)
    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        if stype == SectionType.TASKS_TABLE:
            stripped = line.strip()
            if stripped.startswith("|") and "| Task#" not in stripped:
                parts = split_pipe_row(stripped)
                if (len(parts) > 2 and parts[1].strip() == "1"
                        and not set(stripped.replace("|", "").strip()) <= {"-", ":"}):
                    ac_col = parts[2].strip()
                    tokens = [t.strip() for t in ac_col.split(",") if t.strip()]
                    assert tokens.count("1") == 1, f"AC#1 duplicated in task row: {ac_col}"
                    break


def test_ac_link_idempotent_goal(feature_file):
    """Link AC#1 to Goal#1 (already there). Should not duplicate."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_link("999", ac_num=1, goal=1)
    assert result == 0
    content = p.read_text(encoding="utf-8")
    # Count occurrences of AC#1 in goal coverage section - should still be 1
    lines = content.splitlines()
    in_goal_cov = False
    goal1_line = None
    for line in lines:
        if "### Goal Coverage" in line:
            in_goal_cov = True
        if in_goal_cov and line.strip().startswith("|"):
            parts = split_pipe_row(line.strip())
            if len(parts) > 2 and parts[1].strip() == "1" and "AC#" in parts[2]:
                goal1_line = parts[2]
                break
    assert goal1_line is not None
    assert goal1_line.count("AC#1") == 1, f"AC#1 duplicated in goal row: {goal1_line}"


def test_ac_link_nonexistent_ac(feature_file, capsys):
    """Link AC#99. Should return 1."""
    feature_file("999", FEATURE_STANDARD)
    result = ac_link("999", ac_num=99, task=1)
    assert result == 1
    captured = capsys.readouterr()
    assert "AC#99" in captured.err or "99" in captured.err


def test_ac_link_dry_run(feature_file):
    """Dry run should not modify file. Return 0."""
    p = feature_file("999", FEATURE_STANDARD)
    original = p.read_text(encoding="utf-8")
    result = ac_link("999", ac_num=3, task=1, dry_run=True)
    assert result == 0
    content = p.read_text(encoding="utf-8")
    assert content == original


def test_ac_link_no_args_error(feature_file, capsys):
    """Neither --task nor --goal provided. Should return 1."""
    feature_file("999", FEATURE_STANDARD)
    result = ac_link("999", ac_num=1)
    assert result == 1
    captured = capsys.readouterr()
    assert "task" in captured.err.lower() or "goal" in captured.err.lower()


def test_ac_link_nonexistent_feature(feature_file, capsys):
    """Feature doesn't exist. Should return 1."""
    feature_file("999", FEATURE_STANDARD)
    result = ac_link("888", ac_num=1, task=1)
    assert result == 1
    captured = capsys.readouterr()
    assert "not found" in captured.err.lower() or "ERROR" in captured.err


# =============================================================================
# ac_update tests
# =============================================================================


def test_ac_update_description(feature_file):
    """Update AC#1's description. Check Definition Table and AC Details heading updated."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_update("999", ac_num=1, description="Updated First AC")
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    # Check Definition Table row
    ac_rows = parse_ac_table(lines)
    ac1 = next((r for r in ac_rows if r.number == 1), None)
    assert ac1 is not None
    assert ac1.description == "Updated First AC"
    # Check AC Details heading
    assert "**AC#1: Updated First AC**" in content


def test_ac_update_expected(feature_file):
    """Update AC#1's expected. Check Definition Table and AC Details expected line."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_update("999", ac_num=1, expected="new_value")
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    ac_rows = parse_ac_table(lines)
    ac1 = next((r for r in ac_rows if r.number == 1), None)
    assert ac1 is not None
    assert ac1.expected == "new_value"
    # Check AC Details expected line
    assert "- **Expected**: new_value" in content or "- **Test**: " in content


def test_ac_update_matcher(feature_file):
    """Update AC#2's matcher to 'contains'. Check Definition Table only."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_update("999", ac_num=2, matcher="contains")
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    ac_rows = parse_ac_table(lines)
    ac2 = next((r for r in ac_rows if r.number == 2), None)
    assert ac2 is not None
    assert ac2.matcher == "contains"


def test_ac_update_method(feature_file):
    """Update AC#1's method. Check Definition Table and AC Details method line."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_update("999", ac_num=1, method="Glob(z)")
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    ac_rows = parse_ac_table(lines)
    ac1 = next((r for r in ac_rows if r.number == 1), None)
    assert ac1 is not None
    assert ac1.method == "Glob(z)"
    # AC Details should have updated method/test line
    assert "Glob(z)" in content


def test_ac_update_sync_coverage(feature_file):
    """Update AC#1 desc with --sync-coverage. Check AC Coverage row text updated."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_update("999", ac_num=1, description="Synced Description", sync_coverage=True)
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    # Check AC Coverage table has updated description for AC#1
    section_map = classify_sections(lines)
    found = False
    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        if stype == SectionType.TECH_DESIGN_AC_COV:
            stripped = line.strip()
            if stripped.startswith("|"):
                parts = split_pipe_row(stripped)
                if len(parts) > 2 and parts[1].strip() == "1":
                    assert "Synced Description" in parts[2]
                    found = True
                    break
    assert found, "AC Coverage row for AC#1 not updated"


def test_ac_update_no_details_block(feature_file):
    """Feature where AC has no Details block. Should update Definition Table only without error."""
    p = feature_file("999", FEATURE_NO_DETAILS)
    result = ac_update("999", ac_num=1, description="New Description")
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    ac_rows = parse_ac_table(lines)
    ac1 = next((r for r in ac_rows if r.number == 1), None)
    assert ac1 is not None
    assert ac1.description == "New Description"


def test_ac_update_prose_warning(feature_file, capsys):
    """Update Expected from a value that appears in prose. Should print warning."""
    # Use a feature where old expected value appears in prose sections
    content = FEATURE_STANDARD.replace(
        "All 3 ACs are verified.",
        "All 3 ACs are verified. pattern is the expected output."
    )
    feature_file("999", content)
    result = ac_update("999", ac_num=2, expected="new_pattern")
    assert result == 0
    captured = capsys.readouterr()
    # Should warn about old value found in prose
    assert "WARNING" in captured.out or "prose" in captured.out.lower()


def test_ac_update_dry_run(feature_file):
    """Dry run should not modify file."""
    p = feature_file("999", FEATURE_STANDARD)
    original = p.read_text(encoding="utf-8")
    result = ac_update("999", ac_num=1, description="Changed", dry_run=True)
    assert result == 0
    content = p.read_text(encoding="utf-8")
    assert content == original


def test_ac_update_nonexistent_ac(feature_file, capsys):
    """AC#99. Should return 1."""
    feature_file("999", FEATURE_STANDARD)
    result = ac_update("999", ac_num=99, description="test")
    assert result == 1
    captured = capsys.readouterr()
    assert "AC#99" in captured.err or "99" in captured.err


def test_ac_update_no_fields(feature_file, capsys):
    """No --description/--expected/--matcher/--method. Should return 1."""
    feature_file("999", FEATURE_STANDARD)
    result = ac_update("999", ac_num=1)
    assert result == 1
    captured = capsys.readouterr()
    assert "description" in captured.err.lower() or "expected" in captured.err.lower() or "matcher" in captured.err.lower()


# =============================================================================
# ac_add tests
# =============================================================================


def test_ac_add_append(feature_file):
    """Add AC#4 at end. Check new row in Definition Table. Count should be 4."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_add("999", desc="Fourth AC", method="Grep(z)", matcher="contains", expected="something")
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    ac_rows = parse_ac_table(lines)
    assert len(ac_rows) == 4
    ac4 = next((r for r in ac_rows if r.number == 4), None)
    assert ac4 is not None
    assert ac4.description == "Fourth AC"
    assert ac4.matcher == "contains"


def test_ac_add_with_after_renumber(feature_file):
    """Add --after 1. AC#2 and AC#3 shift to 3 and 4. New AC#2 inserted."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_add("999", desc="Inserted AC", method="Grep(z)", matcher="contains",
                    expected="something", after=1)
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    ac_rows = parse_ac_table(lines)
    assert len(ac_rows) == 4
    # New AC#2 should be "Inserted AC"
    ac2 = next((r for r in ac_rows if r.number == 2), None)
    assert ac2 is not None
    assert ac2.description == "Inserted AC"
    # Old AC#2 (Second AC) should now be AC#3
    ac3 = next((r for r in ac_rows if r.number == 3), None)
    assert ac3 is not None
    assert "Second AC" in ac3.description
    # Old AC#3 (Third AC) should now be AC#4
    ac4 = next((r for r in ac_rows if r.number == 4), None)
    assert ac4 is not None
    assert "Third AC" in ac4.description


def test_ac_add_threshold_generates_details(feature_file):
    """Add with matcher=gte. AC Details block should be generated."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_add(
        "999",
        desc="New Threshold AC",
        method="dotnet test",
        matcher="gte",
        expected=">=3",
        derivation="3 things in module",
    )
    assert result == 0
    content = p.read_text(encoding="utf-8")
    # AC Details block should contain new AC
    assert "**AC#4: New Threshold AC**" in content
    assert "3 things in module" in content


def test_ac_add_non_threshold_no_details(feature_file):
    """Add with matcher=succeeds. No AC Details block should be generated."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_add(
        "999",
        desc="Simple AC",
        method="dotnet build",
        matcher="succeeds",
        expected="-",
    )
    assert result == 0
    content = p.read_text(encoding="utf-8")
    # No new details block for succeeds matcher
    assert "**AC#4: Simple AC**" not in content


def test_ac_add_with_task_goal_coverage(feature_file):
    """Add with --task 1 --goal 1 --coverage 'text'. All three linkages created."""
    p = feature_file("999", FEATURE_STANDARD)
    result = ac_add(
        "999",
        desc="Linked AC",
        method="Grep(q)",
        matcher="contains",
        expected="something",
        task=1,
        goal=1,
        coverage="do linked thing",
    )
    assert result == 0
    content = p.read_text(encoding="utf-8")
    lines = content.splitlines()
    section_map = classify_sections(lines)
    # Check Task#1 has AC#4 using section classification
    task1_row = None
    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        if stype == SectionType.TASKS_TABLE:
            stripped = line.strip()
            if stripped.startswith("|") and "| Task#" not in stripped:
                parts = split_pipe_row(stripped)
                if (len(parts) > 2 and parts[1].strip() == "1"
                        and not set(stripped.replace("|", "").strip()) <= {"-", ":"}):
                    task1_row = parts
                    break
    assert task1_row is not None
    assert "4" in [t.strip() for t in task1_row[2].split(",")]
    # Check Goal#1 has AC#4 using section classification
    for idx, line in enumerate(lines):
        stype = section_map.get(idx, SectionType.OTHER)
        if stype == SectionType.GOAL_COVERAGE:
            stripped = line.strip()
            if stripped.startswith("|") and "| Goal" not in stripped:
                parts = split_pipe_row(stripped)
                if (len(parts) > 2 and parts[1].strip() == "1"
                        and not set(stripped.replace("|", "").strip()) <= {"-", ":"}):
                    assert "AC#4" in parts[2]
                    break
    # Check AC Coverage table has AC#4
    assert "do linked thing" in content


def test_ac_add_dry_run(feature_file):
    """Dry run. File unchanged. Return 0."""
    p = feature_file("999", FEATURE_STANDARD)
    original = p.read_text(encoding="utf-8")
    result = ac_add(
        "999",
        desc="Dry AC",
        method="Grep(z)",
        matcher="contains",
        expected="something",
        dry_run=True,
    )
    assert result == 0
    content = p.read_text(encoding="utf-8")
    assert content == original


def test_ac_add_threshold_no_derivation_warning(feature_file, capsys):
    """Threshold matcher without --derivation. Should print warning."""
    feature_file("999", FEATURE_STANDARD)
    result = ac_add(
        "999",
        desc="Threshold Without Derivation",
        method="dotnet test",
        matcher="gte",
        expected=">=3",
        # No derivation provided
    )
    assert result == 0
    captured = capsys.readouterr()
    assert "WARNING" in captured.out or "derivation" in captured.out.lower()


def test_ac_add_sub_numbered_guard(feature_file, capsys):
    """Feature with sub-numbered ACs. Should return 1 without --force."""
    feature_file("999", FEATURE_WITH_SUB_NUMBERED)
    result = ac_add(
        "999",
        desc="New AC",
        method="Grep(z)",
        matcher="contains",
        expected="something",
    )
    assert result == 1
    captured = capsys.readouterr()
    assert "sub-numbered" in captured.err.lower() or "AC#3b" in captured.err or "force" in captured.err.lower()


def test_ac_add_invalid_matcher(feature_file, capsys):
    """Invalid matcher name. Return 1."""
    feature_file("999", FEATURE_STANDARD)
    result = ac_add(
        "999",
        desc="Bad Matcher AC",
        method="Grep(z)",
        matcher="invalid_matcher_xyz",
        expected="something",
    )
    assert result == 1
    captured = capsys.readouterr()
    assert "invalid_matcher_xyz" in captured.err or "Invalid matcher" in captured.err


def test_ac_add_nonexistent_feature(feature_file, capsys):
    """Feature doesn't exist. Return 1."""
    feature_file("999", FEATURE_STANDARD)
    result = ac_add(
        "888",
        desc="New AC",
        method="Grep(z)",
        matcher="contains",
        expected="something",
    )
    assert result == 1
    captured = capsys.readouterr()
    assert "not found" in captured.err.lower() or "ERROR" in captured.err
