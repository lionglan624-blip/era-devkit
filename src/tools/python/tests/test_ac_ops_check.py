"""
Unit tests for ac_ops ac_check command.

Tests: ac_check with clean file, gap detection, missing Details,
       count mismatch, file-not-found error, invalid matchers (N1),
       regex errors (N3), column swaps (N4), unassigned ACs (N7),
       goal coverage refs (N9), AC count limits (N10/N11),
       gte derivation (N12), and --fix auto-correction.
"""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import pytest
import ac_ops
from ac_ops import ac_check


MINIMAL_FEATURE = """# Feature 999: Test Feature

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

### AC Details

**AC#1: First AC**
- **Test**: Glob x
- **Expected**: exists

**AC#2: Second AC**
- **Test**: Grep y
- **Expected**: pattern

### Goal Coverage Verification

| Goal | Covering ACs |
|------|-------------|
| 1 | AC#1, AC#2 |

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2 | Do stuff | | [ ] |

## Technical Design

### Approach

All 2 ACs are satisfied.

### AC Coverage

| AC# | How |
|:---:|-----|
| 1 | do thing 1 |
| 2 | do thing 2 |

## Implementation Contract

### Success Criteria

All 2 ACs pass:
- AC#1-2

## Review Notes

## Execution Log
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


def test_ac_check_no_issues(feature_file, capsys):
    """No issues in a clean feature file."""
    feature_file("999", MINIMAL_FEATURE)
    issues = ac_check("999")
    assert issues == []
    captured = capsys.readouterr()
    assert "No issues found" in captured.out


def test_ac_check_gap(feature_file, capsys):
    """Detect numbering gap."""
    content = MINIMAL_FEATURE.replace("| 2 |", "| 3 |").replace("AC#2", "AC#3")
    feature_file("999", content)
    issues = ac_check("999")
    assert any("gap" in i.lower() for i in issues)


def test_ac_check_missing_details(feature_file, capsys):
    """Detect AC in table but missing Details block."""
    lines = MINIMAL_FEATURE.split("\n")
    # Find the AC#2 table row and insert AC#3 after it
    for i, line in enumerate(lines):
        if "| 2 | Second AC" in line:
            lines.insert(i + 1, "| 3 | Third AC | code | Grep(z) | gte | 5 | [ ] |")
            break
    content = "\n".join(lines)
    feature_file("999", content)
    issues = ac_check("999")
    assert any("AC#3" in i and "threshold matcher" in i for i in issues)


def test_nonthreshold_no_details_ok(feature_file, capsys):
    """Non-threshold matchers (matches/exists) need no AC Details block."""
    lines = MINIMAL_FEATURE.split("\n")
    # Add AC#3 with non-threshold matcher but no Details block
    for i, line in enumerate(lines):
        if "| 2 | Second AC" in line:
            lines.insert(i + 1, "| 3 | Third AC | file | Glob(z) | exists | - | [ ] |")
            break
    # Add AC#3 to Goal Coverage
    for i, line in enumerate(lines):
        if "| 1 | AC#1, AC#2 |" in line:
            lines[i] = "| 1 | AC#1, AC#2, AC#3 |"
            break
    # Add to Tasks
    for i, line in enumerate(lines):
        if "| 1 | 1, 2 | Do stuff" in line:
            lines[i] = "| 1 | 1, 2, 3 | Do stuff | | [ ] |"
            break
    # Update counts
    content = "\n".join(lines).replace("All 2 ACs", "All 3 ACs")
    # Add AC#3 to Tech Design
    content = content.replace("| 2 | do thing 2 |", "| 2 | do thing 2 |\n| 3 | do thing 3 |")
    # Add AC#3 to Success Criteria
    content = content.replace("AC#1-2", "AC#1-3")
    feature_file("999", content)
    issues = ac_check("999")
    # AC#3 uses exists (non-threshold) → no missing-details error
    assert not any("AC#3" in i and "threshold" in i.lower() for i in issues)


def test_ac_check_count_mismatch(feature_file, capsys):
    """Detect 'All N ACs' count mismatch."""
    content = MINIMAL_FEATURE.replace("All 2 ACs", "All 5 ACs")
    feature_file("999", content)
    issues = ac_check("999")
    assert any("5" in i and "2" in i for i in issues)


def test_ac_check_file_not_found(capsys):
    """Error for non-existent feature file."""
    issues = ac_check("99999")
    assert len(issues) == 1
    assert "not found" in issues[0].lower()


# --- N1: Invalid matcher names ---

def test_n1_invalid_matcher_with_suggestion(feature_file, capsys):
    """N1: Invalid matcher with fix suggestion from _MATCHER_FIX_MAP."""
    content = MINIMAL_FEATURE.replace("| exists |", "| count_gte |")
    feature_file("999", content)
    issues = ac_check("999")
    assert any("Invalid matcher 'count_gte'" in i and "suggest: 'gte'" in i for i in issues)


def test_n1_invalid_matcher_no_suggestion(feature_file, capsys):
    """N1: Invalid matcher with no known fix."""
    content = MINIMAL_FEATURE.replace("| exists |", "| foobar |")
    feature_file("999", content)
    issues = ac_check("999")
    assert any("Invalid matcher 'foobar'" in i for i in issues)


def test_n1_valid_matchers_pass(feature_file, capsys):
    """N1: All valid matchers should not trigger."""
    feature_file("999", MINIMAL_FEATURE)
    issues = ac_check("999")
    assert not any("Invalid matcher" in i for i in issues)


# --- N3: Regex syntax errors ---

def test_n3_trailing_pipe(feature_file, capsys):
    """N3: Regex with trailing pipe."""
    content = MINIMAL_FEATURE.replace("| matches | pattern |", "| matches | `foo|bar|` |")
    feature_file("999", content)
    issues = ac_check("999")
    assert any("trailing pipe" in i.lower() for i in issues)


def test_n3_unbalanced_parens(feature_file, capsys):
    """N3: Regex with unbalanced parentheses."""
    content = MINIMAL_FEATURE.replace("| matches | pattern |", "| matches | `(foo` |")
    feature_file("999", content)
    issues = ac_check("999")
    assert any("unbalanced parentheses" in i.lower() for i in issues)


def test_n3_char_class_paren_not_false_positive(feature_file, capsys):
    """N3: Paren inside character class [^)]+ should not trigger false positive."""
    content = MINIMAL_FEATURE.replace(
        "| matches | pattern |",
        r"| matches | `\.NtrRevelation\([^,]+,\s*[^)]+\)` |"
    )
    feature_file("999", content)
    issues = ac_check("999")
    assert not any("unbalanced parentheses" in i.lower() for i in issues)


def test_n3_real_unbalanced_with_char_class(feature_file, capsys):
    """N3: Real unbalanced paren is still caught even with char classes present."""
    content = MINIMAL_FEATURE.replace(
        "| matches | pattern |",
        "| matches | `([^)]+` |"
    )
    feature_file("999", content)
    issues = ac_check("999")
    assert any("unbalanced parentheses" in i.lower() for i in issues)


# --- N4: Column swap detection ---

def test_n4_matcher_looks_like_expected(feature_file, capsys):
    """N4: Matcher column has backtick-wrapped value (probable column swap)."""
    content = MINIMAL_FEATURE.replace("| exists | - |", "| `pattern` | exists |")
    feature_file("999", content)
    issues = ac_check("999")
    assert any("looks like an Expected value" in i for i in issues)


def test_n4_method_looks_like_matcher(feature_file, capsys):
    """N4: Method column has a valid matcher name (probable column swap)."""
    content = MINIMAL_FEATURE.replace("| Grep(y) | matches |", "| matches | Grep(y) |")
    feature_file("999", content)
    issues = ac_check("999")
    assert any("looks like a Matcher name" in i for i in issues)


# --- N7: ACs not assigned to any Task ---

def test_n7_unassigned_ac(feature_file, capsys):
    """N7: AC exists in table but not referenced by any Task."""
    # Add AC#3 to table and details but NOT to Tasks
    lines = MINIMAL_FEATURE.split("\n")
    for i, line in enumerate(lines):
        if "| 2 | Second AC" in line:
            lines.insert(i + 1, "| 3 | Third AC | file | Glob(z) | exists | - | [ ] |")
            break
    # Add Details for AC#3
    for i, line in enumerate(lines):
        if "**AC#2: Second AC**" in line:
            idx = i + 3  # after AC#2's Expected line
            lines.insert(idx, "")
            lines.insert(idx + 1, "**AC#3: Third AC**")
            lines.insert(idx + 2, "- **Test**: Glob z")
            lines.insert(idx + 3, "- **Expected**: exists")
            break
    # Add AC#3 to Goal Coverage
    for i, line in enumerate(lines):
        if "| 1 | AC#1, AC#2 |" in line:
            lines[i] = "| 1 | AC#1, AC#2, AC#3 |"
            break
    # Update counts
    content = "\n".join(lines).replace("All 2 ACs", "All 3 ACs")
    # Add AC#3 to Tech Design AC Coverage
    for i, line in enumerate(content.split("\n")):
        if "| 2 | do thing 2 |" in line:
            parts = content.split("\n")
            parts.insert(i + 1, "| 3 | do thing 3 |")
            content = "\n".join(parts)
            break
    feature_file("999", content)
    issues = ac_check("999")
    assert any("AC#3" in i and "Not assigned to any Task" in i for i in issues)


# --- N9: Goal Coverage rows without AC# ---

FEATURE_GOAL_NO_AC = MINIMAL_FEATURE.replace(
    "| 1 | AC#1, AC#2 |",
    "| 1 | covers something |"
)


def test_n9_goal_coverage_no_ac_ref(feature_file, capsys):
    """N9: Goal Coverage row has no AC# reference."""
    feature_file("999", FEATURE_GOAL_NO_AC)
    issues = ac_check("999")
    assert any("Goal Coverage row has no AC# reference" in i for i in issues)


# --- N10/N11: AC count limits ---

def _make_many_acs(count):
    """Build feature content with `count` ACs."""
    table_rows = []
    details = []
    goal_acs = []
    task_acs = []
    tech_rows = []
    philo_acs = []
    for n in range(1, count + 1):
        table_rows.append(f"| {n} | AC {n} | file | Glob(x{n}) | exists | - | [ ] |")
        details.append(f"**AC#{n}: AC {n}**\n- **Test**: Glob x{n}\n- **Expected**: exists\n")
        goal_acs.append(f"AC#{n}")
        task_acs.append(str(n))
        tech_rows.append(f"| {n} | thing {n} |")
        philo_acs.append(f"AC#{n}")

    return f"""# Feature 999: Test Feature

## Status: [DRAFT]

## Acceptance Criteria

### Philosophy Derivation

| Claim | Coverage |
|-------|----------|
| test | {", ".join(philo_acs)} |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
{chr(10).join(table_rows)}

### AC Details

{chr(10).join(details)}

### Goal Coverage Verification

| Goal | Covering ACs |
|------|-------------|
| 1 | {", ".join(goal_acs)} |

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | {", ".join(task_acs)} | Do stuff | | [ ] |

## Technical Design

### Approach

All {count} ACs are satisfied.

### AC Coverage

| AC# | How |
|:---:|-----|
{chr(10).join(tech_rows)}

## Implementation Contract

### Success Criteria

All {count} ACs pass.

## Review Notes

## Execution Log
"""


def test_n11_soft_limit(feature_file, capsys):
    """N11: AC count > 30 triggers soft limit warning (opt-in)."""
    content = _make_many_acs(31)
    feature_file("999", content)
    issues = ac_check("999", warn_count=True)
    assert any("31" in i and "30 soft limit" in i for i in issues)


def test_n10_hard_limit(feature_file, capsys):
    """N10: AC count > 50 triggers hard limit error (opt-in)."""
    content = _make_many_acs(51)
    feature_file("999", content)
    issues = ac_check("999", warn_count=True)
    assert any("51" in i and "50 hard limit" in i for i in issues)


def test_n10_n11_off_by_default(feature_file, capsys):
    """N10/N11 are off by default."""
    content = _make_many_acs(51)
    feature_file("999", content)
    issues = ac_check("999")  # no warn_count
    assert not any("limit" in i.lower() for i in issues)


def test_n10_n11_under_limit(feature_file, capsys):
    """No AC count issue when under 30 even with warn_count."""
    feature_file("999", MINIMAL_FEATURE)
    issues = ac_check("999", warn_count=True)
    assert not any("limit" in i.lower() for i in issues)


# --- N12: gte without derivation ---

FEATURE_GTE_NO_DERIVATION = """# Feature 999: Test Feature

## Status: [DRAFT]

## Acceptance Criteria

### Philosophy Derivation

| Claim | Coverage |
|-------|----------|
| test | AC#1 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Count check | code | Grep(x) | gte | 5 | [ ] |

### AC Details

**AC#1: Count check**
- **Test**: Grep x
- **Expected**: 5

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

All 1 ACs are satisfied.

### AC Coverage

| AC# | How |
|:---:|-----|
| 1 | do thing 1 |

## Implementation Contract

### Success Criteria

All 1 ACs pass.

## Review Notes

## Execution Log
"""


def test_n12_gte_no_derivation(feature_file, capsys):
    """N12: gte matcher without derivation in AC Details."""
    feature_file("999", FEATURE_GTE_NO_DERIVATION)
    issues = ac_check("999")
    assert any("gte" in i and "derivation" in i.lower() for i in issues)


FEATURE_GTE_WITH_DERIVATION = FEATURE_GTE_NO_DERIVATION.replace(
    "- **Expected**: 5",
    "- **Expected**: 5\n- **Rationale**: 5 interfaces in constructor"
)


def test_n12_gte_with_derivation(feature_file, capsys):
    """N12: gte matcher with derivation passes."""
    feature_file("999", FEATURE_GTE_WITH_DERIVATION)
    issues = ac_check("999")
    assert not any("derivation" in i.lower() for i in issues)


FEATURE_GTE_ADJECTIVE_DERIVATION = FEATURE_GTE_NO_DERIVATION.replace(
    "- **Expected**: 5",
    "- **Expected**: 5\n- **Rationale**: 5 constructor-injected interfaces present in file"
)


def test_n12_derivation_with_adjective(feature_file, capsys):
    """N12: Derivation with adjective between count and noun passes."""
    feature_file("999", FEATURE_GTE_ADJECTIVE_DERIVATION)
    issues = ac_check("999")
    assert not any("derivation" in i.lower() for i in issues)


FEATURE_GTE_COUNT_PATTERN = FEATURE_GTE_NO_DERIVATION.replace(
    "- **Expected**: 5",
    "- **Expected**: 5\n- **Rationale**: Count >= 5 (funcA, funcB, funcC, funcD, funcE)"
)


def test_n12_derivation_count_gte_pattern(feature_file, capsys):
    """N12: Count >= N (list) pattern passes."""
    feature_file("999", FEATURE_GTE_COUNT_PATTERN)
    issues = ac_check("999")
    assert not any("derivation" in i.lower() for i in issues)


FEATURE_GTE_EXTENDED_NOUN = FEATURE_GTE_NO_DERIVATION.replace(
    "- **Expected**: 5",
    "- **Expected**: 5\n- **Rationale**: 5 constants defined for flags"
)


def test_n12_derivation_extended_nouns(feature_file, capsys):
    """N12: Extended noun vocabulary (constants) passes."""
    feature_file("999", FEATURE_GTE_EXTENDED_NOUN)
    issues = ac_check("999")
    assert not any("derivation" in i.lower() for i in issues)


FEATURE_GTE_EXPLICIT_RATIONALE = FEATURE_GTE_NO_DERIVATION.replace(
    "- **Expected**: 5",
    "- **Expected**: 5\n- **Rationale**: Pairwise with AC#83. Covers visitor FLAG resets."
)


def test_n12_derivation_explicit_rationale(feature_file, capsys):
    """N12: Explicit **Rationale**: marker passes regardless of content."""
    feature_file("999", FEATURE_GTE_EXPLICIT_RATIONALE)
    issues = ac_check("999")
    assert not any("derivation" in i.lower() for i in issues)


FEATURE_GT_NO_DERIVATION = FEATURE_GTE_NO_DERIVATION.replace(
    "| gte | 5 |",
    "| gt | 5 |"
)


def test_n12_gt_no_derivation(feature_file, capsys):
    """N12: gt matcher without derivation in AC Details triggers error."""
    feature_file("999", FEATURE_GT_NO_DERIVATION)
    issues = ac_check("999")
    assert any("gt'" in i and "derivation" in i.lower() for i in issues)


# --- --fix mode tests ---

def test_fix_invalid_matcher(feature_file, capsys):
    """--fix corrects count_gte → gte in the AC table."""
    content = MINIMAL_FEATURE.replace("| exists |", "| count_gte |")
    path = feature_file("999", content)
    issues = ac_check("999", fix=True)
    # After fix, no invalid matcher issue should remain
    assert not any("Invalid matcher 'count_gte'" in i for i in issues)
    # Verify file was updated
    text = path.read_text(encoding="utf-8")
    assert "count_gte" not in text


def test_fix_stale_count(feature_file, capsys):
    """--fix corrects 'All 5 ACs' → 'All 2 ACs'."""
    content = MINIMAL_FEATURE.replace("All 2 ACs", "All 5 ACs")
    path = feature_file("999", content)
    issues = ac_check("999", fix=True)
    assert not any("All 5 ACs" in i for i in issues)
    text = path.read_text(encoding="utf-8")
    # Approach section should be fixed
    assert "All 2 ACs" in text


def test_fix_dry_run(feature_file, capsys):
    """--fix --dry-run reports but does not modify file."""
    content = MINIMAL_FEATURE.replace("| exists |", "| count_gte |")
    path = feature_file("999", content)
    issues = ac_check("999", fix=True, dry_run=True)
    captured = capsys.readouterr()
    assert "DRY-RUN" in captured.out
    # File should NOT be modified
    text = path.read_text(encoding="utf-8")
    assert "count_gte" in text


def test_fix_numbering_gap(feature_file, capsys):
    """--fix closes numbering gaps via renumber."""
    content = MINIMAL_FEATURE.replace("| 2 |", "| 3 |").replace("AC#2", "AC#3")
    path = feature_file("999", content)
    issues = ac_check("999", fix=True)
    text = path.read_text(encoding="utf-8")
    # After renumber, should have AC#1 and AC#2 (not AC#3)
    assert "AC#2" in text
    # No gap issues should remain
    assert not any("gap" in i.lower() for i in issues)


# --- --skip tests ---

def test_skip_suppresses_check(feature_file, capsys):
    """--skip N12 suppresses gte derivation warnings."""
    feature_file("999", FEATURE_GTE_NO_DERIVATION)
    issues = ac_check("999", skip=frozenset({"N12"}))
    assert not any("derivation" in i.lower() for i in issues)


def test_skip_does_not_affect_other_checks(feature_file, capsys):
    """--skip N12 does not suppress unrelated checks."""
    content = FEATURE_GTE_NO_DERIVATION.replace("| gte |", "| count_gte |")
    feature_file("999", content)
    issues = ac_check("999", skip=frozenset({"N12"}))
    assert any("Invalid matcher" in i for i in issues)
