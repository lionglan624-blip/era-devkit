# Feature 587: ac-static-verifier Expected Column Quote Stripping

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

AC verification tools should handle Expected column patterns correctly to provide reliable test results. Quote handling standardization ensures patterns match their intended content rather than literal quotation marks.

### Problem (Current Issue)

ac-static-verifier.py reads Expected column from AC Definition Table but does not strip surrounding quotes. When Expected contains quoted patterns like `"persist_pending Usage Guidance"`, the tool searches for literal quotes in target files, causing false FAIL results.

Root cause: line 96 (approximate) reads `expected=parts[6]` without quote stripping, line 137 (approximate) uses `pattern=ac.expected` directly.

### Goal (What to Achieve)

Strip surrounding quotes from Expected column in ac-static-verifier.py parsing logic so patterns like `"text"` become `text` before grep execution.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Quote stripping implementation exists | code | Grep(tools/ac-static-verifier.py) | contains | ".strip('\"')" | [x] |
| 2 | Feature 582 code ACs pass | build | python tools/ac-static-verifier.py --feature 582 --ac-type code | succeeds | - | [x] |

### AC Details

**AC#1**: Quote stripping implementation exists
- Test: `grep -n ".strip('\"')" tools/ac-static-verifier.py`
- Expected: Contains quote stripping implementation for double quotes

**AC#2**: Feature 582 code ACs pass
- Test: `python tools/ac-static-verifier.py --feature 582 --ac-type code`
- Expected: All Feature 582 code ACs return PASS status

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add quote stripping logic to Expected column parsing | [x] |
| 2 | 2 | Verify Feature 582 code ACs pass with fixed tool | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Target Files

| File | Change | Line Context |
|------|--------|--------------|
| tools/ac-static-verifier.py | Add quote stripping | Line 96 `expected=parts[6]` |

### Implementation Steps

1. **Parse Expected column with quote stripping** (Line 96 context, approximate)
   - Modify `expected = parts[6]` to `expected = parts[6].strip('"')`
   - Preserve existing parsing logic structure
   - This affects ALL AC types (code, build, file) since they share ACDefinition parsing
   - No changes needed in individual verify methods (they use the parsed value)
   - Target: Expected columns with double quotes (standard markdown convention)

2. **Verify pattern application** (Line 137 context, approximate)
   - Confirm `pattern = ac.expected` uses the stripped value
   - No additional changes needed if step 1 correctly modifies Expected assignment

3. **Test with Feature 582**
   - Run `python tools/ac-static-verifier.py --feature 582 --ac-type code`
   - Verify PASS results for quoted patterns

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F582 | [DONE] | FL workflow persist_pending guidance (test target) |
| Related | F584 | [PROPOSED] | Testing SKILL.md Method column format (AC verification context) |

---

## Review Notes
- [deferred] AC#2 Expected column escaping issue deferred to feature resolution during implementation
- [applied] Phase1-Uncertain iter5: AC#5 inconsistency resolved by aligning Description with Expected
- [applied] Phase1-Uncertain iter6: Task#5/AC#5 verification vs implementation purpose resolved by removing redundant ACs
- [applied] Phase1-Uncertain iter9: Step 2 verification clarity already addressed in Implementation Contract
<!-- Optional: Add review feedback here. /next command shows this section. -->

---

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| - | - | - | - |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-21 | Phase 1 | initializer | Status [REVIEWED] → [WIP] | OK |
| 2026-01-21 | Phase 2 | explorer | Investigate quote stripping | READY |
| 2026-01-21 | Phase 4 | implementer | Task 1 implementation | SUCCESS |
| 2026-01-21 | Phase 6 | verifier | AC#1 grep pattern | PASS |
| 2026-01-21 | Phase 6 | verifier | AC#2 F582 code ACs | PASS (4/4) |

## Links

- [feature-582.md](feature-582.md) - FL workflow persist_pending guidance
- [feature-584.md](feature-584.md) - Testing SKILL.md Method column format
- [index-features.md](index-features.md)