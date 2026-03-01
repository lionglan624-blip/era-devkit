---
description: Initialize up to 5 kojo feature.md files for batch workflow
---

**Language**: Thinking in English, respond to user in Japanese.

---

Create kojo feature.md files for upcoming COM implementations (max 5 PROPOSED at a time).

## Arguments

**ARGUMENTS**: `$ARGUMENTS`

| Format | Example | Behavior |
|--------|---------|----------|
| (empty) | `/kojo-init` | Auto-select from content-roadmap COM Series Order (Phase 8d) |
| COMs | `/kojo-init 60 61 65` | Create features for specified COMs (Phase 8d) |
| --phase | `/kojo-init --phase 8l` | Auto-select using specified Phase |
| --phase + COMs | `/kojo-init --phase 8j 80 81 82` | Create features for specified COMs using specified Phase |

---

## Execution Flow

### Step 0: Load Quality Reference

**MANDATORY**: Read `Skill(feature-quality)` kojo guide before creating features.

Reference: [.claude/skills/feature-quality/kojo.md](../.claude/skills/feature-quality/kojo.md)

Key requirements from kojo quality guide:
- 1 COM = 1 Feature granularity
- All 4 TALENT branches must have ACs
- eraTW reference must be documented
- Line count requirement (4-8 lines per branch)

### Step 1: Check Existing PROPOSED Count

Read `pm/index-features.md` Active Features table:
- Count rows with `[PROPOSED]` status AND Type `kojo`
- Calculate: `slots_available = 5 - existing_proposed_count`

**If slots_available <= 0**:
```
=== /kojo-init ===

Already 5+ PROPOSED kojo Features exist.

Current PROPOSED:
| ID | Name |
|----|------|
| ... | ... |

Please implement existing Features first:
  /run {ID}
```
→ Exit (do not create any features)

### Step 2: Determine Target COMs and Feature IDs

**If arguments provided**: Use specified COM numbers (capped at slots_available), auto-increment Feature IDs

**If no arguments**:
- Reference `Skill(kojo-writing)` for COM → File Placement (SSOT)
- Reference `pm/content-roadmap.md` for COM Series Order (300s → 0s → 20s → 40s → ... → 600s)
- Read `pm/index-features.md` Active Features (Content section)
- Find COMs without existing kojo Feature
- Select first `slots_available` unfinished COMs (following content-roadmap COM Series Order)
- Use auto-increment Feature IDs from "Next Feature Number"

**COM→File mapping**: [`src/tools/kojo-mapper/com_file_map.json`](../../src/tools/kojo-mapper/com_file_map.json) is SSOT. Reference this JSON when determining file placement during Feature creation.

### Step 2.5: Phase Detection

**Detection Logic**:
1. Check if arguments contain `--phase {PHASE_ID}` flag
2. If `--phase` found: Use specified Phase ID (e.g., `8j`, `8l`)
3. If `--phase` NOT found: Use Phase `8d` (default)

**Phase-specific Template Selection**:

| Phase | Template | Feature Naming Pattern |
|:-----:|----------|------------------------|
| 8d | Current template (TALENT_4 quality) | COM_{NUM} {name} Dialogue (Phase 8d) |
| 8j | Ejaculation state template | COM_{NUM} Ejaculation State Dialogue (Phase 8j) |
| 8l | FIRSTTIME template | COM_{NUM} First Execution Dialogue (Phase 8l) |

**Template Variables**: Use `{PHASE_ID}` and `{PHASE_TITLE}` in templates below, which get substituted based on detected phase:
- Phase 8d: `{PHASE_ID}` = "8d", `{PHASE_TITLE}` = "{name} Dialogue"
- Phase 8j: `{PHASE_ID}` = "8j", `{PHASE_TITLE}` = "Ejaculation State Dialogue"
- Phase 8l: `{PHASE_ID}` = "8l", `{PHASE_TITLE}` = "First Execution Dialogue"

**Backwards Compatibility**:
- `/kojo-init` → Phase 8d (existing behavior preserved)
- `/kojo-init 80 81 82` → Phase 8d (existing behavior preserved)
- `/kojo-init --phase 8l 80 81 82` → Phase 8l (new functionality)

### Step 2.6: Filter Out Implemented COMs

Before creating features, check which COMs already have implementations:

1. Run `python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --progress`
2. Parse output to identify Done COMs:
   - Any COM with `Done > 0` (at least 1 character implemented) is considered "implemented"
3. Filter these Done COMs out from the target COM list
4. Only propose features for Remaining COMs (Done = 0)

### Step 3: Create Feature Files (up to slots_available)

For each COM, create `pm/features/feature-{ID}.md`:

**Note**: Replace `{PHASE_ID}` and `{PHASE_TITLE}` with values from Phase Detection (Step 2.5).

```markdown
# Feature {ID}: COM_{NUM} {PHASE_TITLE} (Phase {PHASE_ID})

## Status: [PROPOSED]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature files follow SSOT and integrate with other workflows (/run, /fl, ac-tester)

### Problem
COM_{NUM} ({name}) lacks Phase {PHASE_ID} quality dialogue for all characters.

### Goal
Create {PHASE_ID} quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase {PHASE_ID}: Full COM coverage + quality improvements
- Quality reference: eraTW Reimu
- Structure: 4 branches × 4 patterns per character

---

## Implementation Contract

### eraTW Reference

**Source**: Extract using eratw-reader agent before implementation
**COM File**: `ERB\口上\COM{NUM}.ERB` (eraTW path)

### Quality Requirements (from feature-quality/kojo.md)

- [ ] All 4 TALENT branches (恋人/恋慕/思慕/なし) implemented
- [ ] 4-8 lines per branch
- [ ] Emotion/scene description comments included
- [ ] Character voice consistency verified

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K1 Meiling COM_{NUM} dialogue output | output | --unit tests/ac/kojo/feature-{ID}/test-{ID}-K1.json | contains | DATALIST output | [ ] |
| 2 | K2 Koakuma COM_{NUM} dialogue output | output | --unit tests/ac/kojo/feature-{ID}/test-{ID}-K2.json | contains | DATALIST output | [ ] |
| 3 | K3 Patchouli COM_{NUM} dialogue output | output | --unit tests/ac/kojo/feature-{ID}/test-{ID}-K3.json | contains | DATALIST output | [ ] |
| 4 | K4 Sakuya COM_{NUM} dialogue output | output | --unit tests/ac/kojo/feature-{ID}/test-{ID}-K4.json | contains | DATALIST output | [ ] |
| 5 | K5 Remilia COM_{NUM} dialogue output | output | --unit tests/ac/kojo/feature-{ID}/test-{ID}-K5.json | contains | DATALIST output | [ ] |
| 6 | K6 Flandre COM_{NUM} dialogue output | output | --unit tests/ac/kojo/feature-{ID}/test-{ID}-K6.json | contains | DATALIST output | [ ] |
| 7 | K7 Koakuma COM_{NUM} dialogue output | output | --unit tests/ac/kojo/feature-{ID}/test-{ID}-K7.json | contains | DATALIST output | [ ] |
| 8 | K8 Cirno COM_{NUM} dialogue output | output | --unit tests/ac/kojo/feature-{ID}/test-{ID}-K8.json | contains | DATALIST output | [ ] |
| 9 | K9 Daiyousei COM_{NUM} dialogue output | output | --unit tests/ac/kojo/feature-{ID}/test-{ID}-K9.json | contains | DATALIST output | [ ] |
| 10 | K10 Marisa COM_{NUM} dialogue output | output | --unit tests/ac/kojo/feature-{ID}/test-{ID}-K10.json | contains | DATALIST output | [ ] |
| 11 | Build succeeds | build | - | succeeds | - | [ ] |
| 12 | Regression tests pass | output | --unit tests/regression/ | contains | "passed (100%)" | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | Create K1-K10 COM_{NUM} dialogue (4 branches×4 patterns) | [ ] |
| 2 | 11 | Build verification | [ ] |
| 3 | 12 | Regression tests | [ ] |
| 4 | 1-10 | AC verification | [ ] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| | | | | |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
- [feature-quality/kojo.md](../../.claude/skills/feature-quality/kojo.md)
```

### Step 4: Update Index

Update `pm/index-features.md`:

1. **Active Features table** - Add rows for each created feature:
```markdown
| {ID} | [PROPOSED] | COM_{NUM} {PHASE_TITLE} | [feature-{ID}.md](feature-{ID}.md) |
```
(Where `{PHASE_TITLE}` is from Step 2.5 Phase Detection)

2. **Next Feature Number** - Update to current + (number of features created)

---

## Output Format

**Normal case** (created some features):
```
=== /kojo-init Complete ===

Created Features: {N} (Existing PROPOSED: {M} → Total: {N+M})

| ID | COM | Name | Status |
|----|-----|------|--------|
| 133 | 8 | Secret Garden Display | [PROPOSED] |
| 134 | 9 | Masturbation | [PROPOSED] |

Next Feature Number: 135

To implement:
  /run {ID}       Example: /run 133
```

**Limit reached** (5+ already exist):
```
=== /kojo-init ===

Already 5 PROPOSED kojo Features exist.

Current PROPOSED:
| ID | Name |
|----|------|
| 128 | COM_8 Secret Garden Display Dialogue |
| 129 | COM_9 Masturbation Dialogue |
| ... | ... |

Please implement existing Features first:
  /run {ID}
```

---

## Notes

- **Max 5 PROPOSED kojo features** - prevents queue overflow
- This command ONLY creates feature.md files (no implementation)
- AC Method includes test file path (generated by kojo_test_gen.py in Phase 5)
- Multiple sessions can work on different COMs in parallel
- To implement: `/run {ID}`
