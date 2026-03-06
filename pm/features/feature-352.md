# Feature 352: Phase 16 Kojo Conversion Planning

## Status: [DONE]

## Type: research

## Created: 2026-01-05

---

## Summary

Plan Phase 16 (Kojo Conversion) from full-csharp-architecture.md. Define batch conversion strategy and per-character migration features.

> **Terminology**: F344's "Phase 2: Core Migration" = full-csharp-architecture.md **Phase 16: Kojo Conversion**

---

## Background

### Philosophy (Mid-term Vision)

**Evidence-Based Planning**: Phase 2 planning should incorporate learnings from Phase 1 pilot. Actual conversion metrics, edge cases encountered, and tooling feedback inform realistic Phase 2 scope.

### Problem (Current Issue)

F344 outlined Phase 16 (called "Phase 2" in F344) conceptually:
- Task 2.1: Batch Conversion Tool (3-4 days)
- Task 2.2: Character-by-Character Migration (4-5 weeks)
- Task 2.3: NTR System Integration (1 week)

Phase 1 pilot (F351) will reveal:
- Actual conversion success rate
- Edge cases requiring manual fixes
- Tooling gaps

**Limitations discovered in F351**:
- ErbParser does not support `PRINTDATA...ENDDATA` structure
- F351 test code uses regex workaround
- Parser extension needed for Phase 16

Planning without Phase 1 data risks inaccurate estimates.

### Goal (What to Achieve)

Based on F351 pilot results:
1. Analyze Phase 1 pilot results
2. Refine Phase 2 task breakdown
3. Create Phase 16 sub-features (F354-F357)
4. Define implementation dependencies (Phase 2 Test Infrastructure required)

### Manual Correction Workflow (To Be Detailed)

Phase 1 pilot will reveal conversion success rate. Expect ~70-80% auto-convertible, ~20-30% requiring manual work.

**Workflow elements to define**:
| Element | Description |
|---------|-------------|
| **Conversion Status Tracking** | auto/manual/failed per file |
| **Manual Correction Queue** | Task list for failed conversions |
| **LLM-Assisted Review** | LLM drafts → human confirms |
| **Staged Rollout** | K1 complete → verify → K2... |
| **Fallback Strategy** | Unconvertible COMs remain ERB temporarily |

**Tracking file**: `Game/agents/migration-status.md` (created in Task 4)

```markdown
| Character | COM | Status | Notes |
|-----------|-----|:------:|-------|
| K1 | COM_0 | auto | Pilot complete |
| K1 | COM_1 | manual | Complex PRINTFORM |
| K1 | COM_2 | failed | Needs redesign |
```

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 1 analysis documented | file | Grep | contains | "Phase 1 Pilot Results" in full-csharp-architecture.md | [x] |
| 2a | F354 PRINTDATA Parser defined | doc | Read | contains | F354 definition in Phase 16 section | [x] |
| 2b | F355 Batch Conversion Tool defined | doc | Read | contains | F355 definition in Phase 16 section | [x] |
| 2c | F356 Character Migration defined | doc | Read | contains | F356 definition in Phase 16 section | [x] |
| 2d | F357 NTR Integration defined | doc | Read | contains | F357 definition in Phase 16 section | [x] |
| 3 | Initialization Tasks documented | doc | Read | contains | "Phase 16 Initialization Tasks" in full-csharp-architecture.md | [x] |
| 4 | migration-status.md template documented | doc | Read | contains | Template in full-csharp-architecture.md | [x] |

> **Note**: AC#2-4 の成果物は `full-csharp-architecture.md` Phase 16 セクションに統合。
> Feature ファイル作成は Phase 16 Initialization Tasks として Phase 16 開始時に実行。

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Document Phase 1 Pilot Results in full-csharp-architecture.md Phase 16 section | [x] |
| 2 | 2a-2d | Define F354-F357 in full-csharp-architecture.md Phase 16 section | [x] |
| 3 | 3 | Document Phase 16 Initialization Tasks in full-csharp-architecture.md | [x] |
| 4 | 4 | Document migration-status.md template in full-csharp-architecture.md | [x] |

> **Implementation Deferred**: Feature ファイル作成・index 更新は Phase 16 Initialization Tasks として Phase 16 開始時に実行。

---

## Output Features

F352 creates the following Phase 16 feature specifications:

> **IMPORTANT**: F354-F357 implementation requires F358 (Phase 2 Test Infrastructure) for KojoComparer verification.
> These features should be BLOCKED until F358 is complete.

| Feature | Type | Scope | Blocks | Reference |
|---------|------|-------|:------:|-----------|
| F354 | engine | PRINTDATA Parser Extension | F355 | F351 limitation |
| F355 | engine | Batch Conversion Tool | - | F344 Task 2.1 |
| F356 | infra | Character Migration Framework | - | F344 Task 2.2 |
| F357 | erb | NTR System Integration | - | F344 Task 2.3 |

**Implementation Dependency Chain**:
```
F358 (Phase 2: Test Infrastructure)
  └── KojoComparer tool
        ↓
F354-F357 (Phase 16: Kojo Conversion)
```

**Note**: F356 and F357 are alternatives; at least one must be created.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F351 | Phase 1 pilot results (DONE) |
| Predecessor | F344 | Phase 16 conceptual design source (DONE) |
| Predecessor | F358 | Phase 2: Test Infrastructure must complete first |
| Predecessor | Phase 3-11 | full-csharp-architecture.md dependency chain |
| Successor | F354-F357 | Phase 16 implementation features |

---

## Links

- [feature-351.md](feature-351.md) - Phase 1 Pilot (predecessor)
- [feature-344.md](feature-344.md) - Codebase Analysis (Phase 16 outline)
- [feature-345.md](feature-345.md) - Phase 1 Feature Breakdown
- [feature-353.md](feature-353.md) - CFLAG/Function Condition Extractor (Phase 1)
- [feature-358.md](feature-358.md) - Phase 2 Test Infrastructure (F354-F357 blocker)
- [designs/full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 16 definition
- [designs/codebase-analysis-report.md](designs/codebase-analysis-report.md) - Phase 16 tasks reference

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | - | Created as Phase 2 trigger from F345 | BLOCKED |
| 2026-01-05 | status | - | FL: F351 DONE, unblock F352 | PROPOSED |
| 2026-01-05 | status | - | Phase順に従い Phase 2-11 完了まで待機 | BLOCKED |
| 2026-01-09 | END | - | Planning integrated into full-csharp-architecture.md Phase 16 | [DONE] |
