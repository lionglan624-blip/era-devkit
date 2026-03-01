# FC/FL Error Taxonomy

Classification system for FL review issues to enable data-driven FC improvements.

## Purpose

1. Categorize FL review issues for pattern identification
2. Enable feedback loop from FL/RUN failures to FC improvements
3. Track error frequencies to prioritize prevention efforts

---

## Category 1: AC Definition Errors

Issues in AC table definition that cause FL/RUN failures.

| Code | Name | Description | Prevention |
|------|------|-------------|------------|
| AC-001 | Vague Expected | Expected value not specific/measurable | Run baseline command during FC |
| AC-002 | Wrong Matcher | Matcher type inappropriate for verification | Follow testing SKILL Type Mapping |
| AC-003 | Placeholder Without [I] | Task has placeholder Expected but no [I] tag | Use [I] tag for uncertain outcomes |
| AC-004 | Weak Contains | `contains` with generic word (high false positive) | Use `matches` with specific regex |
| AC-005 | Missing AC | Goal/Philosophy item has no corresponding AC | Goal Coverage Verification |
| AC-006 | Untestable AC | AC cannot be verified with available tools | Feasibility check during FC |

**Examples**:
- AC-001: `Expected: "improvement"` → Should be `Expected: ">= 95%"`
- AC-002: `Type: test` for pytest → Should be `Type: exit_code`
- AC-004: `contains "NTR"` → Should be `matches "TALENT:NTR"`

---

## Category 2: Constraint Errors

Issues where technical constraints are missed or ignored.

| Code | Name | Description | Prevention |
|------|------|-------------|------------|
| CON-001 | Constraint Not Extracted | tech-investigator missed constraint | Mandatory constraint extraction step |
| CON-002 | Constraint Not Propagated | ac-designer didn't read constraints | Constraint reading step in ac-designer |
| CON-003 | Constraint Violated | AC/Task violates known constraint | FL constraint validation |
| CON-004 | Stale Constraint | Constraint changed since extraction | FL constraint re-verification |

**Examples**:
- CON-001: Feature assumes API exists but doesn't verify
- CON-002: AC expects output format that constraint prohibits

---

## Category 3: Dependency Errors

Issues with feature dependencies.

| Code | Name | Description | Prevention |
|------|------|-------------|------------|
| DEP-001 | Missing Predecessor | Predecessor feature not listed | Related Feature Search |
| DEP-002 | Wrong Status | Predecessor status outdated | FL Phase 0 status sync |
| DEP-003 | Circular Dependency | Features depend on each other | Dependency graph check |
| DEP-004 | Hidden Dependency | Runtime dependency discovered during RUN | Baseline measurement |

**Examples**:
- DEP-001: F750 depends on F749 but not listed
- DEP-002: Predecessor shows [WIP] but is actually [DONE]

---

## Category 4: Scope Errors

Issues with feature scope definition.

| Code | Name | Description | Prevention |
|------|------|-------------|------------|
| SCP-001 | Scope Creep | Feature expands beyond original scope | Scope Discipline enforcement |
| SCP-002 | Title Mismatch | Title doesn't match actual scope | Philosophy-to-Title verification |
| SCP-003 | Overlapping Scope | Multiple features address same problem | Related Feature Search |
| SCP-004 | Out-of-Scope Fix | Implementation fixes issues beyond scope | Track in Mandatory Handoffs |

**Examples**:
- SCP-001: "Add logging" feature also refactors error handling
- SCP-002: Title says "Duplication Elimination" but scope is "Dead Code Removal"

---

## Category 5: Format/Structure Errors

Issues with feature file format.

| Code | Name | Description | Prevention |
|------|------|-------------|------------|
| FMT-001 | Missing Section | Required section not present | feature-validator check |
| FMT-002 | Malformed Table | Table columns don't match template | FL Phase 1 validation |
| FMT-003 | Broken Link | Referenced file doesn't exist | FL Phase 0/6 reference check |
| FMT-004 | Status Typo | Status has typo (e.g., [PRPOOSED]) | Trivial fix fast-path |

**Examples**:
- FMT-001: No Risks section in feature file
- FMT-003: Links to feature-999.md which doesn't exist

---

## Category 6: Task Errors

Issues with Task table definition.

| Code | Name | Description | Prevention |
|------|------|-------------|------------|
| TSK-001 | Orphan Task | Task has no corresponding AC | AC:Task alignment check |
| TSK-002 | Uncovered AC | AC has no implementing Task | AC:Task alignment check |
| TSK-003 | Missing [I] Tag | Investigation task lacks [I] tag | Investigation Tag Validation |
| TSK-004 | Task Too Broad | Single task covers >5 ACs | Task granularity warning |

**Examples**:
- TSK-001: Task#5 exists but no AC references Task#5
- TSK-003: Task "Investigate X" has placeholder Expected but no [I]

---

## Category 7: Investigation Errors

Issues from insufficient investigation.

| Code | Name | Description | Prevention |
|------|------|-------------|------------|
| INV-001 | Desk-Only Investigation | No actual command execution | Baseline measurement |
| INV-002 | Shallow Root Cause | 5 Whys not completed | Root Cause Analysis validation |
| INV-003 | Factual Error | Technical claim doesn't match codebase | Grep/Read verification |
| INV-004 | Stale Information | Codebase changed since investigation | Re-run baseline before /run |

**Examples**:
- INV-001: AC Expected based on reading code, not running it
- INV-003: Claims "LoadCsvData() exists" but method doesn't exist

---

## Category 8: PRE-EXISTING Issues

Issues that existed before feature but discovered during RUN.

| Code | Name | Description | Prevention |
|------|------|-------------|------------|
| PRE-001 | Infrastructure Bug | Engine/tool bug discovered at scale | Pilot test before batch |
| PRE-002 | Migration Gap | Prior migration incomplete | Migration completeness AC |
| PRE-003 | Test Environment | Test setup missing prerequisite | Environment validation |
| PRE-004 | Hidden Debt | Undocumented technical debt | Codebase exploration |

**Examples**:
- PRE-001: ProcessLevelParallelRunner fails with 650 tests (F706)
- PRE-002: CSV removed but YAML data not created (F727)

---

## Usage

### During FL Review

When categorizing an issue in Review Notes:

```markdown
## Review Notes
- [resolved-applied] Phase1 iter1: [AC-001] AC#3 Expected too vague → Added concrete value
- [resolved-applied] Phase2 iter2: [CON-002] Constraint not read by ac-designer → Added constraint reference
```

### For Metrics Collection

Track frequency by category to identify systemic issues:

| Category | Count (Last 10 Features) | Trend |
|----------|:------------------------:|:-----:|
| AC-001 | 12 | ↑ |
| CON-001 | 8 | → |
| INV-001 | 15 | ↓ |

### For FC Improvement

High-frequency categories indicate FC process gaps:

| High Frequency | FC Improvement |
|----------------|----------------|
| AC-001, INV-001 | Add baseline measurement |
| CON-001, CON-002 | Add constraint handoff |
| TSK-003 | Improve [I] tag detection |
| FMT-004 | Add trivial fix fast-path |

---

## Revision History

| Date | Change |
|------|--------|
| 2026-02-05 | Initial taxonomy created |
