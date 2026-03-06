# Feature 507: Mandatory Handoff Tracking System

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

**"Write and forget" must be impossible.** Every deferred task must have a concrete tracking destination before a Feature can be completed. The workflow itself must enforce this - not just documentation guidelines.

CLAUDE.md Deferred Task Protocol defines 3 options for tracking deferred work. The system must make it impossible to defer without choosing one.

The "負債の意図的受け入れ" sections in architecture reviews created a 4th tracking mechanism outside Deferred Task Protocol. Abolishing these ensures all deferred work uses the standard 3 options.

### Problem (Current Issue)

Two mechanisms allow "write and forget":

1. **"負債の意図的受け入れ" sections** in architecture-review-15.md
   - F506 investigation identified 3 occurrences
   - Creates a 4th mechanism parallel to Deferred Task Protocol
   - Risk: Agents write here and forget

2. **"残課題" section** in feature-template.md
   - Name suggests "leftover tasks" without urgency
   - Table structure doesn't enforce tracking destination
   - Risk: Tasks written but never tracked

**User Decision**: Option 1 (Abolish) chosen over F506's Option 2 recommendation because:
- Simpler enforcement: No section = can't write there
- Deferred Task Protocol's 3 options are sufficient
- Reduces "write and forget" surface area

### Goal (What to Achieve)

1. Abolish "負債の意図的受け入れ" sections (no replacement)
2. Rename "残課題" to "引継ぎ先指定" with mandatory tracking columns
3. Add FL validation to detect empty tracking destinations
4. Add /do completion check to ensure all handoffs are tracked

---

## Impact Analysis

### Affected Documents
| File | Change |
|------|--------|
| architecture-review-15.md | Remove 3 "負債の意図的受け入れ" sections |
| full-csharp-architecture.md | Add clarification that Known Technical Debt follows Deferred Task Protocol |
| feature-template.md | Rename "残課題" to "引継ぎ先指定", update table structure |
| fl.md | Add Phase 8: Handoff Validation |
| do.md | Add handoff confirmation to completion phase |

### Breaking Changes
- Agents can no longer use "負債の意図的受け入れ" as a tracking mechanism
- "残課題" section name changes to "引継ぎ先指定"
- FL will FAIL if handoff destinations are empty/TBD
- /do will not complete if handoffs lack valid tracking IDs

### Migration Path
- TD-P14-001 already tracked in full-csharp-architecture.md + F501
- Existing features with "残課題" sections: No migration needed (section name change is cosmetic)
- New features must use new format

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 1-4 H2 section removed | file | Grep(Game/agents/designs/architecture-review-15.md) | not_contains | "## 負債の意図的受け入れ:" | [x] |
| 2 | Phase 5-8 H3 section removed | file | Grep(Game/agents/designs/architecture-review-15.md) | not_contains | "### 負債の意図的受け入れ:" | [x] |
| 3 | Phase 9-12 content removed | file | Grep(Game/agents/designs/architecture-review-15.md) | not_contains | "Phase 9-12 Technical Debt" | [x] |
| 4 | full-csharp clarification added | file | Grep(Game/agents/designs/full-csharp-architecture.md) | contains | "Deferred Task Protocol" | [x] |
| 5 | Template section renamed | file | Grep(Game/agents/reference/feature-template.md) | contains | "引継ぎ先指定" | [x] |
| 6 | Template has tracking columns | file | Grep(Game/agents/reference/feature-template.md) | contains | "追跡先ID" | [x] |
| 7 | FL has handoff validation | file | Grep(.claude/commands/fl.md) | contains | "Handoff Validation" | [x] |
| 8 | do.md has handoff check | file | Grep(.claude/commands/do.md) | contains | "引継ぎ先" | [x] |
| 9 | No "Accepted" debt pattern | file | Grep(Game/agents/designs/architecture-review-15.md) | not_contains | "Acceptance Rationale" | [x] |
| 10 | TD-P14-001 still tracked | file | Grep(Game/agents/designs/full-csharp-architecture.md) | contains | "TD-P14-001" | [x] |
| 11 | F501 reference preserved | file | Grep(Game/agents/designs/architecture-review-15.md) | contains | "F501" | [x] |
| 12 | All links valid | file | reference-checker | succeeds | - | [x] |

### AC Details

**AC#1-3**: Remove all 3 "負債の意図的受け入れ" section occurrences from architecture-review-15.md
- AC#1 covers both H2 sections: Phase 1-4 (line 967) AND Phase 9-12 (line 2620) since both use '## 負債の意図的受け入れ:'
- AC#2 covers H3 section: Phase 5-8 (line 1991) which uses '### 負債の意図的受け入れ:'
- AC#3 verifies Phase 9-12 specific content 'Phase 9-12 Technical Debt' is also removed (belt-and-suspenders check)

**AC#4**: Add clarification to full-csharp-architecture.md Known Technical Debt section

**AC#5-6**: Update feature-template.md:
- Rename section from "残課題 (Deferred Tasks)" to "引継ぎ先指定 (Mandatory Handoffs)"
- Add "追跡先" and "追跡先ID" columns to table

**AC#7**: Add handoff validation phase to fl.md:
- Detect empty tracking destinations
- Detect "TBD", "未定", "後で決める" patterns
- FAIL if found

**AC#8**: Add handoff confirmation to do.md completion phase:
- Before marking [DONE], verify all handoffs have valid tracking IDs
- Create missing Features if needed

**AC#9-11**: Verify existing tracking is preserved after changes

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Remove Phase 1-4 "負債の意図的受け入れ" section | [x] |
| 2 | 2 | Remove Phase 5-8 "負債の意図的受け入れ" section | [x] |
| 3 | 3 | Remove Phase 9-12 "負債の意図的受け入れ" section | [x] |
| 4 | 4 | Add Deferred Task Protocol clarification to full-csharp-architecture.md | [x] |
| 5 | 5 | Rename section from 残課題 to 引継ぎ先指定 in feature-template.md | [x] |
| 6 | 6 | Add 追跡先 and 追跡先ID columns to feature-template.md table | [x] |
| 7 | 7 | Add Handoff Validation section to fl.md | [x] |
| 8 | 8 | Add handoff check (Step 8.2.5) to do.md Phase 8 | [x] |

<!-- AC#9-12 are verification-only (Grep/reference-checker). No implementation task needed.
     Tasks 1-3 constraint: During removal, preserve F501 references (AC#11), TD-P14-001 migration info (AC#10).
     Ensure no 'Acceptance Rationale' patterns remain after section removal (AC#9). -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Part 1: Remove "負債の意図的受け入れ" Sections

**Note**: Line numbers are approximate. Use Grep to locate actual sections.

1. **Phase 1-4** (Grep: `## 負債の意図的受け入れ:` near Phase 1-4 summary):
   Remove entire section including header and content.

2. **Phase 5-8** (Grep: `### 負債の意図的受け入れ:` near Phase 5-8 summary):
   Remove entire section including header and content.

3. **Phase 9-12** (Grep: `Phase 9-12 Technical Debt` near Phase 9-12 summary):
   Remove entire section including header and content.

### Part 2: Add Clarification to full-csharp-architecture.md

Add before "Known Technical Debt" table:
```markdown
**Tracking Policy**: Technical debt follows CLAUDE.md Deferred Task Protocol:
- Option A: Create Feature immediately
- Option B: Add to existing Feature's Tasks
- Option C: Add to architecture.md Phase Tasks

This section documents WHAT the debt is. Resolution tracking uses the 3 options above.
```

### Part 3: Update feature-template.md

Replace:
```markdown
## 残課題 (Deferred Tasks)
<!-- MANDATORY when deferring tasks. Delete section if none. -->
<!-- Per CLAUDE.md Deferred Task Protocol: 引継ぎ先を明記しないと漏れる -->

| Task | Reason | Target Phase | Target Feature |
|------|--------|:------------:|:--------------:|
| {deferred task description} | {why deferred} | Phase N | F{ID} or TBD |
```

With:
```markdown
## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題なし → セクション削除。課題あり → 全行に追跡先必須 -->
<!-- 空欄・TBD・未定 禁止。FL が FAIL する -->

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| {課題内容} | {なぜ今やらないか} | Feature / Task / Phase | F{ID} / Phase N |

<!-- Note: "TBD" removed from example per CLAUDE.md TBD Prohibition -->
<!-- 追跡先の選択肢 (CLAUDE.md Deferred Task Protocol):
- Feature: 新規 Feature を作成 → 追跡先ID = F{ID}
- Task: 既存 Feature の Tasks に追加 → 追跡先ID = F{ID}#T{N}
- Phase: architecture.md Phase Tasks に追加 → 追跡先ID = Phase {N}
-->
```

### Part 4: Add Handoff Validation to fl.md

**Location**: Two changes required:
1. Add Phase 8 pseudocode within the WHILE loop (Section 4), after Phase 7 (Final Reference Check) and before BREAK
2. Update TodoWrite initialization at Section 1: Add `{content: "Phase 8: Handoff Validation", status: "pending", activeForm: "Running Handoff Validation"}` between Phase 7 and Post-loop entries

Note: Documentation sections (## 8. Report, ## 9. Status Update, ## 10. Philosophy Gate) do NOT need renumbering as they are post-loop sections, separate from Phase numbers.

Add Phase 8 pseudocode within WHILE loop (after Phase 7):

```markdown
# ============================================================
# TodoWrite: Phase 8 開始
# ============================================================
TodoWrite: "Phase 8: Handoff Validation" → status: "in_progress"

# Phase 8: Handoff Validation (Features Only)
IF target_type == "feature":
    # Check 引継ぎ先指定 section
    handoff_section = Grep("引継ぎ先指定", target_path)

    IF handoff_section exists AND not "セクション削除":
        # Extract 引継ぎ先指定 table rows, validate 追跡先ID column
        handoff_table = parse_table(handoff_section)
        FOR row in handoff_table:
            # Check for empty/TBD patterns
            IF row.追跡先ID is empty OR row.追跡先ID in ["TBD", "未定", "後で決める"]:
                persist_pending({
                    severity: "critical",
                    location: "引継ぎ先指定 section",
                    issue: "Empty or TBD tracking destination: {row.追跡先ID}",
                    fix: "Specify concrete tracking destination (Feature ID, Task ID, or Phase number)"
                }, iteration, "Phase8-Handoff")
            # Validate Feature tracking IDs exist
            ELIF row.追跡先ID starts with "F":
                IF NOT exists("Game/agents/feature-{ID}.md"):
                    persist_pending({
                        severity: "critical",
                        location: "引継ぎ先指定 section",
                        issue: "Referenced feature does not exist: {row.追跡先ID}",
                        fix: "Create the referenced feature or update tracking ID"
                    }, iteration, "Phase8-Handoff")
            # Phase references don't need existence validation

# ============================================================
# TodoWrite: Phase 8 完了
# ============================================================
TodoWrite: "Phase 8: Handoff Validation" → status: "completed"
```

### Part 5: Add Handoff Check to do.md

**Location**: Insert Step 8.2.5 between Step 8.2 (Check for Problems) and Step 8.3 (Problem Resolution). Handoff validation runs before problem resolution routing, ensuring invalid handoffs block completion regardless of other issues.

Add to Phase 8, after Step 8.2 routing table and before Step 8.3 (Problem Resolution):

```markdown
### Step 8.2.5: 引継ぎ先確定

IF "引継ぎ先指定" section exists AND has rows:
    FOR each row in handoff_table:
        IF 追跡先ID is empty OR contains "TBD":
            STOP → Ask user: "引継ぎ先が未指定です: {課題}"
            Options:
            A) Create new Feature now
            B) Add to existing Feature's Tasks
            C) Add to Phase Tasks

        IF 追跡先 == "Feature" AND Feature does not exist:
            Create feature-{ID}.md with minimal spec
            Update 追跡先ID in current feature

    Verify all 追跡先ID are valid before proceeding
```

### Rollback Plan

If issues arise:
1. `git revert` the commit
2. Report to user
3. Create follow-up feature for fix

---

## Review Notes
<!-- Optional: Add review feedback here. -->
- **2026-01-15 FL iter0-3**: Previous review notes archived. Scope expanded per user decision.
- **2026-01-15**: User chose Option 1 (Abolish) over F506's Option 2 recommendation. Rationale: Simpler enforcement, reduces "write and forget" surface area.
- **2026-01-15**: Scope expanded to include "残課題" → "引継ぎ先指定" rename and workflow enforcement.
- **2026-01-15 FL iter2**: [resolved] Phase2-Validate - Scope mismatch: F507 targets only architecture-review-15.md but '負債の意図的受け入れ' sections exist in other design docs. User decision: Option B - Create F508 to track remaining 5 files (testability-assessment-15.md, folder-structure-15.md, naming-conventions-15.md, test-strategy.md).
- **2026-01-15 FL iter10**: MAX_ITERATIONS reached. 25+ auto-fixes applied. CRITICAL scope issue resolved via user decision. Minor issues (AC#3 redundancy, Part 4 TodoWrite position) remain but are non-blocking.

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-15 16:53 | START | implementer | Tasks 1-8 | - |
| 2026-01-15 16:53 | END | implementer | Tasks 1-8 | SUCCESS |
| 2026-01-15 16:54 | START | ac-tester | AC#1-12 | - |
| 2026-01-15 16:54 | END | ac-tester | AC#1-12 | PASS:12/12 |
| 2026-01-15 16:55 | START | feature-reviewer | post | - |
| 2026-01-15 16:55 | END | feature-reviewer | post | READY |
| 2026-01-15 16:55 | START | feature-reviewer | doc-check | - |
| 2026-01-15 16:55 | DEVIATION | - | doc-check | NEEDS_REVISION: 6 section name refs |
| 2026-01-15 16:56 | END | feature-reviewer | doc-check (retry) | READY |

## Links

**Context**:
- [F506](feature-506.md) - Investigation feature (recommended Option 2; user chose Option 1 override)
- [CLAUDE.md](../../CLAUDE.md) - Deferred Task Protocol (SSOT)
- [architecture-review-15.md](designs/architecture-review-15.md) - Target for section removal
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Target for clarification
- [feature-template.md](reference/feature-template.md) - Target for section rename
- [fl.md](../../.claude/commands/fl.md) - Target for handoff validation
- [do.md](../../.claude/commands/do.md) - Target for completion check

**Related Features**:
- [F501](feature-501.md) - Architecture Refactoring (resolves TD-P14-001)
- [F508](feature-508.md) - Remove '負債の意図的受け入れ' from remaining design docs (follow-up)
