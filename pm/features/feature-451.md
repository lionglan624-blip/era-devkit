# Feature 451: do.md Gate Improvement

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: infra

## Created: 2026-01-11

---

## Summary

Add explicit gate verification to do.md Step 8.4 report format. Define abnormality clearly and require concrete tracking for all non-zero counts.

**Origin**: Discovered during F448 xUnit v3 Migration.

---

## Background

### Philosophy (Mid-term Vision)

**Explicit Normality Gate** - do.md Step 8.4 is the single source of truth for execution quality. Zero counts (Error:0 Retry:0 Rerun:0 Caution:0) define normal execution. All deviations are abnormal regardless of scope, and must be tracked to concrete destinations (Phase Task or Feature ID) to prevent issue loss.

### Problem (Current Issue)

1. do.md has "正常の定義" section but it's buried and not visible in reports
2. In-scope vs out-of-scope distinction can mislead that out-of-scope issues are acceptable
3. "未定" or "別Feature候補" without concrete tracking destination leads to lost issues
4. No explicit gate format in report template

### Goal (What to Achieve)

1. **Explicit Gate Section** in Step 8.4 report showing counts (NEW format: Error/Retry/Rerun/Caution, separate from existing 正常の定義)
2. **Clear Abnormality Definition** visible in report section
3. **Mandatory Tracking Rule** for all non-zero items

**Note**: Introduces new Gate terminology (Error/Retry/Rerun/Caution) alongside existing 正常の定義 format. The new Gate section provides explicit visibility in reports.

**Terminology Mapping**:
- `Error`: debugger intervention, build failures, test failures (NEW)
- `Retry`: Same as existing `retry` (implementation retry)
- `Rerun`: Same as existing `rerun` (test rerun)
- `Caution`: Warnings, non-blocking issues (replaces/extends `fix`)

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| `.claude/commands/do.md` | Step 8.4 report template update | All /do executions use new format |
| `.claude/commands/do.md` | Abnormality definition section | Opus follows new tracking rule |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Gate section header exists | file | Grep | contains | "\\*\\*Gate\\*\\*:" | [x] |
| 2 | Gate format shows Error count | file | Grep | matches | "Error:\\d+" | [x] |
| 3 | Gate format shows Retry count | file | Grep | matches | "Retry:\\d+" | [x] |
| 4 | Gate format shows Rerun count | file | Grep | matches | "Rerun:\\d+" | [x] |
| 5 | Gate format shows Caution count | file | Grep | matches | "Caution:\\d+" | [x] |
| 6 | Normal baseline documented | file | Grep | matches | "Error:0.*Retry:0.*Rerun:0.*Caution:0" | [x] |
| 7 | Abnormality definition present | file | Grep | matches | "異常.*スコープ.*関係なく" | [x] |
| 8 | Tracking requirement present | file | Grep | matches | "Phase.*Task.*Feature" | [x] |
| 9 | Links in do.md valid | file | reference-checker | succeeds | - | [x] |

### AC Details

**AC#1**: Gate section header
- Test: Grep pattern="\\*\\*Gate\\*\\*:" path=".claude/commands/do.md"
- Expected: Step 8.4 report template has "**Gate**:" header

**AC#2-5**: Gate format counts
- Test: Grep for each count type in do.md
- Expected: Template shows `Error:N Retry:N Rerun:N Caution:N` format

**AC#6**: Normal baseline documented
- Test: Grep pattern="Error:0.*Retry:0.*Rerun:0.*Caution:0" path=".claude/commands/do.md"
- Expected: Full zero baseline explicitly documented as normal (Philosophy: all four counts)

**AC#7**: Abnormality definition
- Test: Grep pattern="異常.*スコープ.*関係なく" path=".claude/commands/do.md"
- Expected: Clear statement that deviations are abnormal regardless of scope
- Implementation text: "異常はスコープに関係なく追跡が必要" (or similar phrasing)

**AC#8**: Tracking requirement
- Test: Grep pattern="Phase.*Task.*Feature" path=".claude/commands/do.md"
- Expected: Mandate that non-zero items have concrete destination

**AC#9**: Link validation
- Test: reference-checker on do.md
- Expected: All internal links resolve correctly

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-6 | Add Gate section with normal baseline to do.md Step 8.4 report template | [x] |
| 2 | 7 | Add abnormality definition to do.md | [x] |
| 3 | 8 | Add tracking requirement to do.md | [x] |
| 4 | 9 | Verify all links valid | [x] |

<!-- Task 1 batches AC#1-6: same edit operation (Gate section template with normal baseline) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Rollback Plan

If issues arise after implementation:
1. Revert do.md changes: `git checkout -- .claude/commands/do.md`
2. Notify user of rollback
3. Create follow-up feature for investigation

### Implementation Steps

1. Read current do.md Step 8.4 section
2. Add Gate section format to report template
3. Add abnormality definition (Japanese)
4. Add tracking requirement with examples
5. Run reference-checker to validate links

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Origin | F448 | Discovered during xUnit v3 Migration |

---

## Links

- [feature-448.md](feature-448.md) - Origin (xUnit v3 Migration)
- [do.md](../../.claude/commands/do.md) - Target file

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-11 FL iter1**: [resolved] Phase2-Validate - AC#8 pattern: Pattern acceptable, AC Details provides context guidance
- **2026-01-11 FL iter2**: [resolved] Phase2-Validate - Task comment: Current comment sufficient for rationale
- **2026-01-11 FL iter3**: [resolved] Phase2-Validate - AC#7 Japanese pattern: AC Details "(or similar phrasing)" provides flexibility
- **2026-01-11 FL iter4**: [resolved] Phase3-Maintainability - Goal#3: Format existence implies mandatory rule
- **2026-01-11 FL iter5**: [resolved] Phase2-Validate - Task batching: Accepted for infra type, comment documents rationale

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 | create | do | Created from F448 残課題 | PROPOSED |
| 2026-01-11 | revise | do | Applied feature-quality INFRA.md checklist | REVISED |
| 2026-01-11 18:06 | START | implementer | Task 1 (AC#1-6) | - |
| 2026-01-11 18:06 | END | implementer | Task 1 (AC#1-6) | SUCCESS |
| 2026-01-11 18:06 | START | implementer | Task 2 (AC#7) | - |
| 2026-01-11 18:06 | END | implementer | Task 2 (AC#7) | SUCCESS |
| 2026-01-11 18:06 | START | implementer | Task 3 (AC#8) | - |
| 2026-01-11 18:06 | END | implementer | Task 3 (AC#8) | SUCCESS |
| 2026-01-11 18:07 | START | reference-checker | Task 4 (AC#9) | - |
| 2026-01-11 18:07 | END | reference-checker | Task 4 (AC#9) | SUCCESS |
| 2026-01-11 18:08 | START | ac-tester | AC verification | - |
| 2026-01-11 18:08 | END | ac-tester | AC verification | PASS:9/9 |
