---
description: Sync dependency statuses in feature files
---

# /sync-deps Command

Inter-Feature health check: dependency sync, status consistency, staleness, scope overlap, redundancy detection, **call chain dependency discovery**, **phase gate validation**, **mandatory handoff integrity**, **feature reference accuracy**, and **interface dependency discovery**.

**Language**: Thinking in English, respond to user in Japanese.

---

## Purpose

Feature間の依存関係を包括的にチェックし、自動修正可能なものは即修正、判断が必要なものはレポートとしてユーザーに提示する。

10のチェックを順番に実行する:

| Check | 内容 | 自動修正 |
|:-----:|------|:--------:|
| 1 | 依存ステータス同期 | Yes |
| 2 | ヘッダーステータス整合性 | 条件付きYes |
| 3 | 陳腐化検出 (ナラティブドリフト) | No (レポート) |
| 4 | スコープ重複検出 | No (レポート) |
| 5 | 他featureで完了済みチェック | No (レポート) |
| **6** | **コールチェーン依存発見** | **Yes** |
| **7** | **フェーズゲート検証** | **Yes** |
| **8** | **Mandatory Handoff 整合性** | **8a: Yes, 8b: No (レポート)** |
| **9** | **Feature参照正確性** | **No (レポート)** |
| **10** | **インターフェース依存発見** | **No (レポート)** |

---

## Phase 1: Setup

### Step 1.1: Read index-features.md

Read `pm/index-features.md` and extract:

1. **Active Features** - All features with their current statuses
2. **Recently Completed** - All features marked as completed (these are [DONE])
3. **Cancelled** - Features marked as [CANCELLED]
4. **Phase Groups** - Which features belong to the same Phase section (e.g., "Phase 20: Equipment & Shop Systems")

Build a **status map**:
```
{
  "F764": "[DONE]",
  "F765": "[DONE]",
  "F763": "[PROPOSED]",
  ...
}
```

Build a **phase map** (NEW):
```
{
  "Phase 20": ["F774", "F775", "F776", "F777", "F778", "F779", "F780", "F781", "F782", "F783"],
  ...
}
```

### Step 1.2: Read All Feature Files (parallel)

For each feature in Active Features, read `pm/features/feature-{ID}.md` in parallel.

Also read Recently Completed feature files (needed for Checks 4-5).

Extract from each active file:
- **Header Status** (e.g., `## Status: [PROPOSED]`)
- **Type** (e.g., `## Type: erb`)
- **Dependencies table** - Type, Feature, Status columns
- **Related Features table** (if exists) - Feature, Status columns
- **Background** section (Philosophy, Problem, Goal)
- **Technical Constraints** section
- **AC Design Constraints** section
- **AC Definition Table** (descriptions only)
- **Technical Design** section (file paths, interfaces)
- **Feature title** (from `# Feature {ID}: {Title}`)
- **File assignments** (extracted from title and Background — see Step 1.4)
- **Summary** section (interface keyword matching for Check 10)

Extract from each completed file:
- **Background > Goal** section
- **AC Definition Table** (descriptions)
- **Technical Design > Approach** section
- **Files Involved** (if present in Deviation Context)
- **Mandatory Handoffs table** (if exists) - Issue, Destination, Destination ID columns
- **Feature title** (from `# Feature {ID}: {Title}`)

### Step 1.3: Structural Integrity Check

Before proceeding, detect:

- **Orphan files**: `feature-{ID}.md` exists but not in index → report `[ORPHAN]`
- **Missing files**: Listed in index but file doesn't exist → report `[ERROR]`
- **Circular deps**: A→B→A predecessor chains → report `[ERROR]`
- **Stale Next Feature Number**: Extract max numeric ID from `Glob("pm/features/feature-*.md")`. Parse `Next Feature number: {N}` from index-features.md. If `max_id >= N`: report `[ERROR]` and auto-fix to `max_id + 1`.

Use Glob `pm/features/feature-*.md` to find all feature files and compare with index.

### Step 1.4: Build File→Feature Map (NEW — required for Check 6)

For each active `erb` or `engine` type feature, extract the ERB/CS files it owns.

**Extraction sources** (in priority order):

1. **Feature title**: Parse filenames from title
   - `"Shop Core (SHOP.ERB + SHOP2.ERB)"` → `["SHOP.ERB", "SHOP2.ERB"]`
   - `"Body Settings UI (体設定.ERB lines 350-943)"` → `["体設定.ERB"]` with line range `350-943`
2. **Background > Problem**: Look for file references with line counts
3. **Technical Design > Approach**: File paths mentioned

Build a **file→feature map**:
```
{
  "SHOP.ERB": { "feature": "F774", "lines": null },
  "SHOP2.ERB": { "feature": "F774", "lines": null },
  "SHOP_COLLECTION.ERB": { "feature": "F775", "lines": null },
  "体設定.ERB:6-348": { "feature": "F778", "lines": [6, 348] },
  "体設定.ERB:350-943": { "feature": "F779", "lines": [350, 943] },
  ...
}
```

**Line range handling**: When multiple features own different line ranges of the same file, map function definitions to features by checking which line range contains the function's `@` definition line.

### Step 1.5: Build Feature Title Map (NEW — required for Check 9)

For each active and completed feature, extract the title from `# Feature {ID}: {Title}` header.

For archived features referenced in active features (detected by `archive/feature-{ID}.md` link patterns), also read their titles.

Build a **title map**:
```
{
  "F421": "IFunctionRegistry / Function Call Mechanism",
  "F477": "CommandProcessor",
  "F647": "Phase 20 Planning",
  "F774": "Shop Core (SHOP.ERB + SHOP2.ERB)",
  ...
}
```

---

## Phase 2: Check 1 — Dependency Status Sync

Compare each active feature file's Dependencies/Related Features table statuses against the status map.

### Detection

**Dependencies Table**:
```markdown
| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F764 | [WIP] | ← status map says [DONE] |
```

**Related Features Table**:
```markdown
| Feature | Status | Relationship |
|---------|--------|--------------|
| F764 | [WIP] | Predecessor | ← status map says [DONE] |
```

### Auto-fix

For each mismatch:
- Use Edit tool to update the Status field in the table row
- Record: `[FIXED] feature-{ID}.md: {table}: F{dep} {old} → {new}`

---

## Phase 3: Check 2 — Header Status Consistency

Compare feature file `## Status: [X]` against index-features.md status.

### Auto-fix Rules

| Index Status | File Status | Action |
|:------------:|:-----------:|--------|
| [DONE] | any other | Auto-fix file header → [DONE] |
| [CANCELLED] | any other | Auto-fix file header → [CANCELLED] |
| [WIP] | [DRAFT]/[PROPOSED]/[REVIEWED] | Auto-fix file header (natural progression) |
| [REVIEWED] | [DRAFT]/[PROPOSED] | Auto-fix file header (natural progression) |
| [PROPOSED] | [DRAFT] | Auto-fix file header (natural progression) |

### Ask User

Any mismatch NOT covered above (e.g., regression like index=[PROPOSED] but file=[REVIEWED]):
- Collect all ambiguous cases
- Present to user at the end of the report as a batch question

---

## Phase 4: Check 3 — Staleness Detection (Narrative Drift)

Detect references to features in narrative sections that have become stale.

### Dispatch: Haiku Subagent

```
Task(subagent_type: "general-purpose", model: "haiku", prompt: "...")
```

**Subagent prompt must include**:

1. The status map (feature ID → current status)
2. For each active feature file: the Background, Technical Constraints, and AC Design Constraints sections
3. Instructions:

```
For each active feature, scan the following sections for references to other features (F{ID} pattern):
- Background (Philosophy, Problem, Goal)
- Technical Constraints
- AC Design Constraints

For each reference found:
1. Look up the referenced feature's current status in the status map
2. Check if the surrounding text implies a status that contradicts reality

Staleness patterns to detect:
- "blocked by F{ID}" or "depends on F{ID}" or "waiting for F{ID}" where F{ID} is [DONE]
- "F{ID} does not yet support" or "F{ID} lacks" where F{ID} is [DONE] (may have added it)
- "F{ID} will provide" or "F{ID} is planned" where F{ID} is [DONE] (already provided)
- "F{ID} is in progress" or "F{ID} [WIP]" where F{ID} is [DONE]

Output JSON:
{
  "findings": [
    {
      "feature": "F763",
      "section": "Background > Problem",
      "quote": "blocked by lack of EVENT function support (F764)",
      "referenced_feature": "F764",
      "referenced_status": "[DONE]",
      "issue": "Text implies F764 is blocking, but F764 is [DONE]",
      "recommendation": "Update to reflect F764 completion"
    }
  ]
}

If no staleness found: {"findings": []}
```

### Report

For each finding: `[WARN] feature-{ID}.md: {section} references F{ref} as {implication} but F{ref} is {actual_status}`

---

## Phase 5: Check 4 — Scope Overlap Detection

Detect duplicate or overlapping work across active features.

### Dispatch: Haiku Subagent

```
Task(subagent_type: "general-purpose", model: "haiku", prompt: "...")
```

**Subagent prompt must include**:

1. For each active feature: AC Definition Table (descriptions) + Technical Design (file paths, interfaces)
2. Instructions:

```
Compare AC descriptions and Technical Design across all active features to detect scope overlap.

Overlap patterns to detect:
- Two features both claim to "Create {same file}" or "Add {same handler/class}"
- Two features modify the same file for the same purpose
- Two features define the same interface or data structure
- Two features have AC descriptions that are semantically equivalent

Do NOT flag:
- Features that reference the same file but for different purposes
- Features where one is Predecessor of the other (intentional dependency)
- Test files (multiple features can test the same area)

Output JSON:
{
  "overlaps": [
    {
      "feature_a": "F768",
      "feature_b": "F751",
      "type": "file_creation | handler_addition | interface_definition | semantic_duplicate",
      "detail_a": "AC#13: Verify TargetKeywords accessible from KojoComparer",
      "detail_b": "AC#5: Update TargetKeywords mapping",
      "common_element": "TargetKeywords",
      "recommendation": "Clarify scope boundary between F768 and F751"
    }
  ]
}

If no overlaps: {"overlaps": []}
```

### Report

For each overlap: `[WARN] F{A} / F{B}: overlap on "{common_element}" — {recommendation}`

---

## Phase 6: Check 5 — Already-Completed-Elsewhere

Detect active features whose goals may already be accomplished by completed features.

### Dispatch: Haiku Subagent

```
Task(subagent_type: "general-purpose", model: "haiku", prompt: "...")
```

**Subagent prompt must include**:

1. For each active feature: Background > Goal + AC Definition Table descriptions
2. For each recently completed feature: Background > Goal + AC descriptions + Technical Design > Approach
3. Instructions:

```
Compare each ACTIVE feature's Goal and AC descriptions against COMPLETED features' accomplishments.

Detection patterns:
- Active Goal describes work that a completed feature's Goal+ACs already cover
- Active AC describes creating/adding something that a completed feature already created/added
- Active feature's Problem statement describes an issue that a completed feature already resolved

Do NOT flag:
- Partial overlap where the active feature extends beyond what was completed
- Features that intentionally build on completed work (predecessor relationship)
- Different aspects of the same area (e.g., parsing vs validation of the same construct)

Output JSON:
{
  "redundancies": [
    {
      "active_feature": "F771",
      "completed_feature": "F764",
      "active_goal": "Add unconditional EVENT dialogue conversion support",
      "completed_achievement": "EVENT Function Conversion Pipeline — converts EVENT function bodies",
      "overlap_degree": "full | substantial | partial",
      "recommendation": "Cancel F771 (fully covered by F764)" | "Revise F771 scope to focus on {uncovered aspect}"
    }
  ]
}

If no redundancies: {"redundancies": []}
```

### Report

For each redundancy:
- `full`: `[WARN] F{active} may be fully redundant with F{done} [DONE] — consider cancelling`
- `substantial`: `[WARN] F{active} substantially overlaps F{done} [DONE] — revise scope`
- `partial`: `[INFO] F{active} partially overlaps F{done} [DONE] — verify remaining scope`

---

## Phase 7: Check 6 — Call Chain Dependency Discovery

**Purpose**: Detect undeclared blocking dependencies between sibling features by analyzing CALL/JUMP references in ERB/CS source files. This catches the case where Feature A's code calls a function defined in Feature B's scope, but the dependency is declared as `Related` instead of `Predecessor`.

**Applies to**: `erb` and `engine` type features only. Skip for `kojo`, `infra`, `research`.

### Step 7.1: Identify Sibling Groups

From the phase map (Step 1.1), group features by Phase. Only analyze groups with 2+ erb/engine features.

Example:
```
Phase 20 erb/engine siblings: [F774, F775, F776, F777, F778, F779, F780, F781]
(F782=infra, F783=research → excluded from call chain analysis)
```

### Step 7.2: Scan Source Files for Cross-Feature Calls

For each Phase group, use Grep to scan ERB files for call references.

**Search patterns** (ERB):
```
CALL {function}
CALLFORM {function}
JUMP {function}
JUMPFORM {function}
GOTO {function}
TRYCALL {function}
TRYJUMP {function}
TRYGOTO {function}
TRYCALLFORM {function}
TRYJUMPFORM {function}
```

**Search patterns** (C#):
```
.{MethodName}(
new {ClassName}(
```

**Procedure**:

For each erb/engine feature F_A in the group:
1. Get F_A's owned files from file→feature map (Step 1.4)
2. For each owned file, use Grep to find all CALL/JUMP/GOTO statements
3. For each call target:
   a. Determine which feature owns the target function/file
   b. If target belongs to a sibling feature F_B:
      - Record: `F_A calls F_B` with evidence (file:line → target function)

**Function→Feature resolution for same-file splits** (e.g., 体設定.ERB):
- Use Grep to find `@{function_name}` definition line in the shared file
- Check which feature's line range contains that definition
- If definition at line 743 and F779 owns lines 350-943 → function belongs to F779

### Step 7.3: Compare Against Declared Dependencies

For each discovered call chain `F_A calls F_B`:
1. Check F_A's Dependencies table for F_B
2. Classification:

| Declared Type | Call Direction | Verdict |
|:-------------:|:-------------:|---------|
| Predecessor | A calls B's function | OK (correct) |
| Successor | B calls A's function | OK (correct) |
| Related | A calls B's function | **UNDECLARED** → should be Predecessor |
| Related | B calls A's function | **UNDECLARED** → should be Successor |
| (not listed) | A calls B's function | **MISSING** → add as Predecessor |
| (not listed) | B calls A's function | **MISSING** → add as Successor |

### Step 7.4: Auto-fix

**For UNDECLARED (Related → Predecessor/Successor)**:
- Edit the Dependencies table row: change Type from `Related` to `Predecessor` or `Successor`
- Append call evidence to Description: `" — {file}:{line} calls @{function} (F{B} scope)"`
- Record: `[FIXED] feature-{A}.md: F{B} Related → Predecessor (call chain: {file}:{line} → @{function})`

**For MISSING**:
- Add new row to Dependencies table with Type, Feature, Status, and Description including call evidence
- Record: `[ADDED] feature-{A}.md: F{B} as Predecessor (call chain: {file}:{line} → @{function})`

**Update index-features.md**:
- For each new or promoted Predecessor, update the "Depends On" column
- Use **bold** formatting for non-[DONE] dependencies per index conventions

### Step 7.5: Report

```
Check 6: Call Chain Discovery
──────────────────────────────
  Phase 20 siblings: F774, F775, F776, F777, F778, F779, F780, F781

  Call Chain Map:
    F774 (SHOP.ERB)
      → CALL SHOW_COLLECTION → F775 (SHOP_COLLECTION.ERB)
      → CALL ITEM_BUY → F776 (SHOP_ITEM.ERB)
    F780 (体設定.ERB:944-1426)
      → CALL 体詳細整頓 → F779 (体設定.ERB:350-943, line 743)

  Dependency Actions:
    [FIXED] feature-775.md: F774 Related → Predecessor
    [FIXED] feature-776.md: F774 Related → Predecessor
    [FIXED] feature-780.md: F779 Related → Predecessor
    [FIXED] index-features.md: F775 Depends On: - → F774
    [FIXED] index-features.md: F776 Depends On: - → F774
    [FIXED] index-features.md: F780 Depends On: - → F779

  Bidirectional updates:
    [FIXED] feature-774.md: F775 Related → Successor
    [FIXED] feature-774.md: F776 Related → Successor
    [FIXED] feature-779.md: F780 Related → Successor

  Summary: N call chains found, M dependency fixes applied
```

### Step 7.6: Code Duplication Advisory (informational)

While scanning for call chains, also detect **copy-paste patterns**:
- Search for comments containing "コピペ", "copy", "copied from", "同じ処理" in the scanned files
- If found, report as INFO with affected features:

```
  Code Duplication Advisory:
    [INFO] SHOP.ERB:168 comment: "SHOP_ITEM.ERBからコピペ＆改変"
      → F774 and F776 share futanari logic — consider shared utility during C# migration
```

---

## Phase 8: Check 7 — Phase Gate Validation

**Purpose**: Validate that Phase-level meta-features (Post-Phase Review, Phase Planning) have correct and complete predecessor chains. These features have structural dependency requirements that can be validated mechanically.

### Step 8.1: Identify Phase Gate Features

From each Phase group, classify features by Type:

| Type | Role | Gate Requirement |
|------|------|-----------------|
| `infra` with "Review" in title | Post-Phase Review | Must have ALL `erb`/`engine`/`kojo` features in same Phase as Predecessor |
| `research` with "Planning" in title | Phase Planning | Must have Post-Phase Review in same Phase as Predecessor |

### Step 8.2: Validate Post-Phase Review Predecessors

For each Review feature:
1. Get all implementation features in the same Phase (Type = erb, engine, kojo)
2. Check the Review feature's Dependencies table
3. For each implementation feature:

| In Dependencies? | Type | Verdict |
|:-:|:---:|---------|
| Yes | Predecessor | OK |
| Yes | Related/Successor | **WRONG TYPE** → should be Predecessor |
| No | - | **MISSING** → add as Predecessor |

### Step 8.3: Validate Planning Feature Predecessors

For each Planning feature:
1. Find the Post-Phase Review feature in the same Phase
2. Check the Planning feature's Dependencies table for the Review feature

| In Dependencies? | Type | Verdict |
|:-:|:---:|---------|
| Yes | Predecessor | OK |
| Yes | Related | **WRONG TYPE** → should be Predecessor |
| No | - | **MISSING** → add as Predecessor |

**Note**: Planning features do NOT need direct Predecessor on implementation features — the transitive chain through the Review feature is sufficient and preferred (avoids redundancy).

### Step 8.4: Auto-fix

**For WRONG TYPE**:
- Edit Dependencies table row: change Type to `Predecessor`
- Record: `[FIXED] feature-{ID}.md: F{dep} {old_type} → Predecessor (phase gate requirement)`

**For MISSING**:
- Add new Predecessor row to Dependencies table
- Record: `[ADDED] feature-{ID}.md: F{dep} as Predecessor (phase gate: {Review|Planning} requires all {implementation|review} features)`

**Update index-features.md**:
- Update "Depends On" column for affected features

**Bidirectional**: When adding Predecessor to a Review feature, also add Successor to the implementation feature's Dependencies table (if not already present).

### Step 8.5: Report

```
Check 7: Phase Gate Validation
────────────────────────────────
  Phase 20:
    Review: F782 (Post-Phase Review Phase 20)
    Planning: F783 (Phase 21 Planning)
    Implementation: F774, F775, F776, F777, F778, F779, F780, F781

    F782 Predecessor check:
      F774: OK (Predecessor)
      F775: [FIXED] Missing → added as Predecessor
      F776: [FIXED] Related → Predecessor
      ...

    F783 Predecessor check:
      F782: OK (Predecessor)

  Summary: N gate issues found, M auto-fixed
```

---

## Phase 9: Check 8 — Mandatory Handoff Integrity

**Purpose**: Validate that Mandatory Handoff destinations correctly track their origin feature, and that destination feature scope covers handoff items.

**Applies to**: Completed features with `## Mandatory Handoffs` tables.

### Step 9.1: Extract Handoff Pairs

For each completed feature with a Mandatory Handoffs table:
1. Parse each row where `Destination = "Feature"` and `Destination ID = F{X}`
2. Skip rows where `Destination = "Phase"` (unresolved handoffs to future phases)
3. Build a handoff map:

```
{
  "F774": [
    { "destination": "F788", "issue": "IConsoleOutput API extension" },
    { "destination": "F789", "issue": "IVariableStore string extension (SAVESTR)" },
    { "destination": "F790", "issue": "Engine built-in variable read interface" },
    ...
  ]
}
```

### Step 9.2: Check 8a — Predecessor Linkage (auto-fix)

For each handoff pair `(origin F{A}, destination F{X})`:
1. Read F{X}'s Dependencies table
2. Check if F{A} appears as Predecessor

| In Dependencies? | Type | Verdict |
|:-:|:---:|---------|
| Yes | Predecessor | OK |
| Yes | Related/Successor | **WRONG TYPE** → should be Predecessor |
| No | - | **MISSING** → add as Predecessor |

**Auto-fix for WRONG TYPE**:
- Edit Dependencies table row: change Type to `Predecessor`
- Append to Description: `" (Mandatory Handoff origin)"`
- Record: `[FIXED] feature-{X}.md: F{A} {old_type} → Predecessor (Mandatory Handoff)`

**Auto-fix for MISSING**:
- Add new row: `| Predecessor | F{A} | {status} | {origin title} (Mandatory Handoff origin) |`
- Record: `[ADDED] feature-{X}.md: F{A} as Predecessor (Mandatory Handoff)`

**Update index-features.md**:
- For new/promoted Predecessor, update "Depends On" column
- Use **bold** for non-[DONE] dependencies

### Step 9.3: Check 8b — Scope Coverage (report)

For each handoff pair `(origin F{A}, destination F{X})`:
1. Extract keywords from Issue column: identifiers matching `[A-Z][A-Za-z_]+` pattern, split on commas and parenthetical boundaries
2. Read F{X}'s Summary and Goal sections
3. Report keywords absent from destination scope

**Example**: Issue = `"Engine built-in variable read interface (MONEY, CHARANUM, MASTER, ..., RESULT)"` → keywords = `[MONEY, CHARANUM, ..., RESULT]`. Check each against F{X}'s Summary+Goal.

### Step 9.4: Report

```
Check 8: Mandatory Handoff Integrity
──────────────────────────────────────
  F774 → Handoff Destinations:

  8a: Predecessor Linkage
    F774 → F788: OK (Predecessor exists)
    F774 → F789: [ADDED] as Predecessor
    F774 → F791: [FIXED] Related → Predecessor

  8b: Scope Coverage
    F774 → F790: [WARN] Keyword "RESULT" not in F790 Summary/Goal
      Recommendation: Verify F790 scope includes RESULT

  Summary: N handoff pairs, M predecessor fixes, P scope warnings
```

---

## Phase 10: Check 9 — Feature Reference Accuracy

**Purpose**: Detect incorrect Feature ID parenthetical references in narrative text (e.g., "IFunctionRegistry (F477)" when the correct feature is F421).

### Step 10.1: Scan and Extract

Scan all active feature files' narrative sections (Background, Summary, Technical Constraints, AC Design Constraints) for patterns matching:

```
{descriptive text} (F{ID})
```

Regex: `(\w[\w\s/]*?)\s*\(F(\d+)\)`

Collect all `(description, F{ID}, source_file, section)` tuples.

### Step 10.2: Dispatch — Haiku Subagent

**Subagent prompt must include**:

1. The feature title map (Step 1.5)
2. All extracted `(description, F{ID})` pairs with source locations
3. Instructions:

```
For each (description, F{ID}) pair:
1. Look up F{ID}'s actual title in the title map
2. Determine if the description accurately refers to F{ID}'s scope
3. Check if the description actually matches a DIFFERENT feature's title better

Mismatch patterns:
- Description matches another feature's title better than F{ID}'s title
- Description describes a concept/interface NOT mentioned in F{ID}'s title
- Description is the exact title of a different feature

Do NOT flag:
- Shortened versions of the correct title (e.g., "Shop Core" for "Shop Core (SHOP.ERB + SHOP2.ERB)")
- Generic references like "parent planning feature (F647)"
- Dependency Type descriptions in Dependencies tables (free-form)
- References where F{ID} title is unavailable (archived/deleted)

Output JSON:
{
  "mismatches": [
    {
      "feature": "F791",
      "section": "Background > Problem",
      "text": "IFunctionRegistry (F477)",
      "referenced_id": "F477",
      "referenced_title": "CommandProcessor",
      "likely_correct_id": "F421",
      "likely_correct_title": "IFunctionRegistry / Function Call Mechanism",
      "recommendation": "Change (F477) to (F421)"
    }
  ]
}

If no mismatches: {"mismatches": []}
```

### Step 10.3: Report

For each mismatch:

```
  [WARN] feature-{ID}.md: {section}
    Text: "{description} (F{referenced})"
    F{referenced} actual title: "{title}"
    Likely correct: F{correct_id} "{correct_title}"
    Recommendation: Change (F{referenced}) to (F{correct_id})
```

```
Check 9: Feature Reference Accuracy
─────────────────────────────────────
  [WARN] feature-791.md: Background > Problem
    Text: "IFunctionRegistry (F477)"
    F477 actual title: "CommandProcessor"
    Likely correct: F421 "IFunctionRegistry / Function Call Mechanism"

  Summary: N references scanned, M mismatches found
```

---

## Phase 11: Check 10 — Interface Dependency Discovery

**Purpose**: Detect missing dependency links between engine-type features that extend/create interfaces and erb/engine-type features that consume those interfaces.

**Applies to**: Active features with Type = `engine` (providers) and Type = `erb` or `engine` (consumers).

### Step 11.1: Dispatch — Haiku Subagent

**Subagent prompt must include**:

1. For each active feature: Type, Summary, Goal, Dependencies table
2. Instructions:

```
Identify interface dependency relationships between features.

Step 1: Find Interface Providers
For each engine-type feature, extract interfaces it creates/extends from Summary and Goal.
Look for patterns: "extend {InterfaceName}", "add methods to {InterfaceName}", "create {InterfaceName}", "{InterfaceName} interface"
Interface name pattern: I[A-Z][a-zA-Z]+ (e.g., IConsoleOutput, IVariableStore, IGameState)

Step 2: Find Interface Consumers
For each erb/engine-type feature, scan Summary, Goal, Background for mentions of the same interface names found in Step 1.

Step 3: Cross-reference
For each (Provider F_P, Consumer F_C, Interface) triple:
- Check if F_C's Dependencies table already lists F_P (any Type)
- If not listed: MISSING dependency

Do NOT flag:
- Features that already list the provider in Dependencies (any Type)
- Features that are Predecessor/Successor of each other
- infra/research type features
- Self-references (provider = consumer)

Output JSON:
{
  "interface_gaps": [
    {
      "provider": "F788",
      "consumer": "F775",
      "interface": "IConsoleOutput",
      "provider_action": "extend",
      "consumer_mention": "Background > Problem: 'IConsoleOutput lacks DrawLine'",
      "recommendation": "Add F788 as Related to F775"
    }
  ]
}

If no gaps: {"interface_gaps": []}
```

### Step 11.2: Report

```
Check 10: Interface Dependency Discovery
──────────────────────────────────────────
  Interface Providers:
    F788: IConsoleOutput (extend)
    F789: IVariableStore (extend)
    F790: IEngineVariables (create)

  Missing Links:
    [WARN] F775: mentions IConsoleOutput but no link to F788
      Recommendation: Add F788 as Related to F775

  Summary: N interface relationships, M missing links
```

---

## Phase 12: Summary Report

Output a structured summary to the user.

```
/sync-deps Report — Inter-Feature Health Check
════════════════════════════════════════════════

Structural Integrity
────────────────────
  [ORPHAN] feature-999.md: Not in index-features.md
  [ERROR] F888: Listed in index but file missing

Check 1: Dependency Status Sync
────────────────────────────────
  [FIXED] feature-763.md: Dependencies: F764 [WIP] → [DONE]
  [FIXED] feature-768.md: Related Features: F767 [PROPOSED] → [DONE]
  Summary: N files, M rows fixed

Check 2: Header Status Consistency
───────────────────────────────────
  [FIXED] feature-706.md: Header [WIP] → [DONE] (index=[DONE])
  Summary: N auto-fixed, M user-resolved

Check 3: Staleness Detection
─────────────────────────────
  [WARN] feature-763.md
    Section: Background > Problem
    Quote: "blocked by lack of EVENT function support (F764)"
    Issue: F764 is [DONE]
    Recommendation: Update narrative to reflect F764 completion

  Summary: N warnings

Check 4: Scope Overlap
───────────────────────
  [WARN] F768 / F751: overlap on "TargetKeywords"
    F768: "Verify TargetKeywords accessible from KojoComparer"
    F751: "Update TargetKeywords mapping"
    Recommendation: Clarify scope boundary

  Summary: N overlaps detected

Check 5: Already-Completed
──────────────────────────
  [WARN] F771: Goal overlaps with F764 [DONE]
    Active: "Add unconditional EVENT dialogue conversion support"
    Done: "EVENT Function Conversion Pipeline"
    Overlap: substantial
    Recommendation: Revise F771 scope

  Summary: N redundancies detected

Check 6: Call Chain Discovery
──────────────────────────────
  Phase 20 siblings: F774, F775, F776, F777, F778, F779, F780, F781

  Call Chain Map:
    F774 → F775 (CALL SHOW_COLLECTION)
    F774 → F776 (CALL ITEM_BUY)
    F780 → F779 (CALL 体詳細整頓)

  [FIXED] feature-775.md: F774 Related → Predecessor
  [FIXED] feature-776.md: F774 Related → Predecessor
  [FIXED] feature-780.md: F779 Related → Predecessor

  Code Duplication Advisory:
    [INFO] SHOP.ERB:168 — F774/F776 share futanari logic

  Summary: N call chains, M fixes

Check 7: Phase Gate Validation
────────────────────────────────
  Phase 20: Review=F782, Planning=F783, Impl=[F774-F781]
  All gates: OK

  Summary: N gate issues, M fixes

Check 8: Mandatory Handoff Integrity
──────────────────────────────────────
  8a: Predecessor Linkage
    [FIXED] feature-778.md: F774 missing → added as Predecessor
  8b: Scope Coverage
    [WARN] F774 → F790: "RESULT" not in F790 scope
  Summary: N handoff pairs, M predecessor fixes, P scope warnings

Check 9: Feature Reference Accuracy
─────────────────────────────────────
  [WARN] feature-791.md: "IFunctionRegistry (F477)" → likely F421
  Summary: N references, M mismatches

Check 10: Interface Dependency Discovery
──────────────────────────────────────────
  [WARN] F775: missing link to F788 (IConsoleOutput)
  Summary: N interface relationships, M missing links

════════════════════════════════════════════════
Overall: X files scanned | Y auto-fixed | Z warnings
════════════════════════════════════════════════
```

### User Decision Batch

If Check 2 has ambiguous cases, present them as a batch after the report:

```
以下のステータス不一致は自動判定できません。正しいステータスを選んでください:

1. F647: File=[DRAFT], Index=[REVIEWED]
   → [REVIEWED] に修正 / [DRAFT] のまま (indexを修正)

2. F703: File=[REVIEWED], Index=[PROPOSED]
   → [PROPOSED] に修正 / [REVIEWED] のまま (indexを修正)
```

Use AskUserQuestion for the batch, then apply chosen fixes.

---

## Notes

- Checks 1-2: Modify feature files directly (auto-fix)
- Checks 3-5: Report only (no file modifications)
- **Check 6: Auto-fix (promotes Related→Predecessor, adds missing dependencies, updates index)**
- **Check 7: Auto-fix (adds missing phase gate predecessors, updates index)**
- **Check 8: Orchestrator (8a auto-fix + 8b report)**
- **Checks 9-10: Subagent (report only), run sequentially after Checks 3-5**
- Subagents (Checks 3-5, 9-10) run sequentially to avoid context explosion
- **Checks 6-8 run in the orchestrator** (Grep-based, no subagent needed)
- If no active features exist, report "No active features to check" and exit
- Run after completing features, before starting new Phases, or periodically
- **Recommended**: Run after batch feature creation (e.g., after a research/planning feature creates multiple DRAFTs) to catch undeclared sibling dependencies early
- **Recommended**: Run after completing features with Mandatory Handoffs to verify destination feature integrity
