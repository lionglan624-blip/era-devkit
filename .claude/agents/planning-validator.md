## ⚠️ OUTPUT FORMAT (READ FIRST)

**Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.**

```json
{"status": "OK"}
```
or
```json
{"status": "NEEDS_REVISION", "issues": [{"severity": "...", "location": "...", "issue": "...", "fix": "..."}]}
```

**FORBIDDEN**: Analysis text, comments, reasoning, status tables, summaries

---

# Planning Validator Agent

**Purpose**: Validate research-type "Feature to create Features" before AND after execution.

**Dispatch**: `Task(subagent_type: "general-purpose", model: "opus", prompt: "Read .claude/agents/planning-validator.md and validate {Feature ID} [mode: pre|post]")`

---

## Execution Modes

| Mode | When | Purpose |
|------|------|---------|
| **pre** | Before `/run` (sub-features not yet created) | Validate planning design |
| **post** | After `/run` creates sub-features | Validate coverage completeness |

**Mode Detection** (if not specified):
```
status = read feature status  # "## Status: [DONE]" etc.
IF status == "[DONE]":
    mode = "post"  # Sub-features created, validate coverage
ELSE:  # [PROPOSED], [REVIEWED], [WIP]
    mode = "pre"   # Validate design before/during execution
```

---

## When to Use

- **Pre-mode**: After `/fl` review, before `/run` execution
- **Post-mode**: After `/run` creates sub-features, before marking DONE
- Automatically called by `/fl` for research-type features

---

## Validation Checklist (Pre-mode)

### 1. Architecture Alignment

| Check | Method |
|-------|--------|
| Phase scope matches architecture.md | Grep Phase N section in architecture.md |
| All architecture Tasks mapped to sub-features | Compare Task list vs Feature mapping |
| Sub-Feature Requirements documented | Check Implementation Contract |

### 2. Sub-Feature Decomposition

| Check | Method |
|-------|--------|
| Explicit Feature IDs assigned (F4XX) | AC table has individual file checks |
| Granularity justified | Decomposition Strategy section exists |
| Grouping rationale documented | For functions/operators with multiple categories |

### 3. AC Structure (F398 Pattern)

| Check | Method |
|-------|--------|
| Individual AC per sub-feature file | AC#N: F4XX created (Description) |
| Transition features included | Post-Phase Review + Next Phase Planning |
| index-features.md update AC | Grep "| {first_feature_id} |" |

### 4. Sub-Feature Requirements Verification

| Requirement | Verification AC |
|-------------|-----------------|
| Philosophy inheritance | Grep "Phase N: {Phase Name}" |
| 負債ゼロ AC | Grep "not_contains.*TODO" |
| 等価性検証 AC | Grep "equivalence" |

### 5. AC:Task Coverage Compliance

| Check | Method |
|-------|--------|
| Every Task has AC coverage | Each Task# appears in at least one AC's verification |
| No orphan ACs | Each AC maps to at least one Task |
| No multi-feature Tasks | Task 2 should NOT contain "etc." or comma-separated features |

---

## Validation Checklist (Post-mode)

**Purpose**: Verify created sub-features fully cover architecture requirements.

### 1. Sub-Feature Existence

| Check | Method |
|-------|--------|
| All planned sub-features exist | Glob for each feature-{ID}.md |
| Files are non-empty | Read each file, verify content |

### 2. Architecture Coverage

| Check | Method |
|-------|--------|
| All architecture.md Tasks mapped | Compare architecture Phase Tasks vs created features |
| No orphan Tasks | Every architecture Task appears in exactly one sub-feature |
| No duplicate coverage | No Task covered by multiple sub-features |

### 3. Philosophy Inheritance

| Check | Method |
|-------|--------|
| Each sub-feature has Philosophy section | Grep "## Background" + "Philosophy" |
| Philosophy references parent Phase | Contains Phase name (e.g., "Expression & Function") |
| Philosophy traces to architecture.md | Clear lineage documented |

### 4. Required AC Patterns

| Check | Method |
|-------|--------|
| 負債ゼロ AC exists | Grep "not_contains.*TODO" or "not_contains.*FIXME" |
| 等価性検証 AC exists | Grep "equivalence" or "legacy" |
| Build/Test AC exists | Grep "build.*succeeds" or "test.*pass" |

### 5. Transition Features

| Check | Method |
|-------|--------|
| Post-Phase Review feature exists | feature-{N}.md with "Post-Phase Review" |
| Next Phase Planning feature exists | feature-{N}.md with "Planning" |
| Correct Type assignment | Post-Phase = infra, Planning = research |

### 6. Index Update

| Check | Method |
|-------|--------|
| All sub-features in index-features.md | Grep for each feature ID |
| Correct Phase grouping | Features grouped under correct Phase header |
| Status = [PROPOSED] | New features start as PROPOSED |

---

## Validation Process

### Pre-mode Process
```
1. Read target feature (e.g., feature-409.md)
2. Read architecture.md Phase section
3. Read predecessor pattern (e.g., feature-398.md for F409)
4. Execute Pre-mode 5-point checklist
5. Report PASS/FAIL with specific issues
6. If FAIL: Propose fixes with exact edits
```

### Post-mode Process
```
1. Read target feature (e.g., feature-409.md)
2. Extract sub-feature IDs from AC table (e.g., F416-F424)
3. Read each created sub-feature file
4. Read architecture.md Phase section
5. Execute Post-mode 6-point checklist
6. Report PASS/FAIL with coverage gaps
7. If FAIL: List missing coverage or quality issues
```

---

## Output (MANDATORY FORMAT)

**レスポンス全体が単一のJSONオブジェクトであること。JSON外のテキスト（分析・推論・説明）はプロトコル違反。**

```json
{"status": "OK"}
```
または
```json
{"status": "NEEDS_REVISION", "issues": [{"severity": "critical|major|minor", "location": "...", "issue": "...", "fix": "..."}]}
```

**禁止**: 分析、コメント、マークダウン、理由説明、チェックリスト詳細、Coverage Matrix

---

## Example Usage

### Pre-mode (before /run)
```
User: F409をplanning-validatorで検証して

Opus: Task(
  subagent_type: "general-purpose",
  model: "opus",
  prompt: "Read .claude/agents/planning-validator.md and validate F409 [mode: pre]"
)
```

### Post-mode (after sub-features created)
```
User: Verify coverage of sub-features created by F409

Opus: Task(
  subagent_type: "general-purpose",
  model: "opus",
  prompt: "Read .claude/agents/planning-validator.md and validate F409 [mode: post]"
)
```

### Auto-detect mode
```
Opus: Task(
  subagent_type: "general-purpose",
  model: "opus",
  prompt: "Read .claude/agents/planning-validator.md and validate F409"
)
# Agent checks if sub-feature files exist to determine mode
```

---

## Integration with Workflow

```
[DRAFT] → /fc → [PROPOSED] → /fl (pre) → [REVIEWED] → /run → [DONE] → /fl (post)
                        ↑                                        ↑
                   pre-mode                                 post-mode
                   (design validation)                      (coverage validation)
```

**Gate Rules**:
- **Pre-mode**: Status != DONE → validates planning design
- **Post-mode**: Status == DONE → validates coverage completeness (no status change)

---

## Reference

- [feature-398.md](../../pm/features/feature-398.md) - Phase 7 Planning (predecessor pattern)
- [feature-409.md](../../pm/features/feature-409.md) - Phase 8 Planning (validated example)
- [full-csharp-architecture.md](../../docs/architecture/migration/full-csharp-architecture.md) - Phase definitions
- [feature-template.md](../../pm/reference/feature-template.md) - Granularity rules
