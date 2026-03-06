# Feature 526: Add infra AC verification guidance to PHASE-6.md

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

## Background

### Philosophy (Mid-term Vision)
Progressive Disclosure workflow should provide clear guidance for each feature type. Each Phase skill should reference the appropriate verification tools based on AC types (output → --unit, file → ac-static-verifier.py).

### Problem (Current Issue)
PHASE-6.md AC verification section only covers Grep/Glob and dotnet test. For `infra` type features with `file` type ACs, the testing SKILL documents `ac-static-verifier.py` but PHASE-6.md doesn't reference it. This causes manual Glob/Grep execution instead of using the standardized tool.

### Goal (What to Achieve)
Add `ac-static-verifier.py` reference to PHASE-6.md Step 6.3 for file/code/build type ACs, providing clear guidance for infra type verification.

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| .claude/skills/run-workflow/PHASE-6.md | Add ac-static-verifier reference | infra features get clearer verification path |

**Scope Note**: Only PHASE-6 (Verification phase) is updated. Other phases (PHASE-2: spec, PHASE-7: commit) do not perform AC verification, so no update needed. testing SKILL is SSOT for tool documentation; PHASE-6.md references tool for discoverability within run-workflow context.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ac-static-verifier referenced | file | Grep(.claude/skills/run-workflow/PHASE-6.md) | contains | "ac-static-verifier" | [x] |
| 2 | file type AC mentioned | file | Grep(.claude/skills/run-workflow/PHASE-6.md) | contains | "file" | [x] |
| 3 | infra type mentioned | file | Grep(.claude/skills/run-workflow/PHASE-6.md) | contains | "infra" | [x] |
| 4 | Example command with args | file | Grep(.claude/skills/run-workflow/PHASE-6.md) | contains | "python tools/ac-static-verifier.py --feature" | [x] |

### AC Details

**AC#1**: Tool reference added
- Test: `Grep "ac-static-verifier"` in PHASE-6.md
- Verifies: The static verifier tool is documented

**AC#2**: File type AC mentioned
- Test: `Grep "file"` in PHASE-6.md
- Verifies: file type ACs get explicit guidance

**AC#3**: Type-specific guidance
- Test: `Grep "infra"` in PHASE-6.md
- Verifies: infra type gets explicit mention in verification

**AC#4**: Actionable command with arguments
- Test: `Grep "python tools/ac-static-verifier.py --feature"` in PHASE-6.md
- Verifies: Executable command example with --feature argument documented (ensures full syntax hint)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-4 | Add ac-static-verifier.py guidance to Step 6.3 for file/code/build type ACs | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Update PHASE-6.md | Updated file |
| 2 | ac-tester | haiku | AC verification | All ACs pass |

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback

---

## Links

- [index-features.md](index-features.md)
- [feature-525.md](feature-525.md) - Predecessor (run-workflow review source)
- [PHASE-6.md](../../.claude/skills/run-workflow/PHASE-6.md) - Target file
- [testing SKILL](../../.claude/skills/testing/SKILL.md) - ac-static-verifier reference

---

## Dependencies

| Type | Feature | Relationship |
|------|---------|--------------|
| Reference | F525 | Historical context - review feedback source (already [DONE]) |

---

## Review Notes

- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - AC#4 Expected pattern: Validator confirmed testing SKILL SSOT specifies exact format 'python tools/ac-static-verifier.py'. AC#4 correctly verifies SSOT compliance.
- **2026-01-18 FL iter2**: [resolved] Phase2-Validate - Method column format: Path corrected to '.claude/skills/run-workflow/PHASE-6.md' for proper ac-static-verifier.py resolution.
- **2026-01-18 FL iter2**: [resolved] AC count 4/8-15: Minimal documentation change (single section addition) justifies reduced AC count. All critical verification aspects covered. Tradeoff: Additional ACs would over-specify a simple text addition. 4 ACs verify tool name, type context (file + infra), and command example - sufficient for single-section edit.
- **2026-01-18 FL iter3**: [resolved] Phase3-Maintainability - AC#4 Expected: Updated to require '--feature' argument in command example. This ensures PHASE-6.md documents actionable syntax, not just tool name. testing SKILL remains SSOT for complete documentation.

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| (none at creation time) | - | - | - |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-18 14:42 | START | implementer | Task 1 | - |
| 2026-01-18 14:42 | END | implementer | Task 1 | SUCCESS |
