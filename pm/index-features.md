# Feature Index

Short-term execution management. See [content-roadmap.md](content-roadmap.md) for long-term planning.

**Reference Maps**: [ntr-system-map.md](reference/ntr-system-map.md) | [kojo-mapper](../../src/tools/kojo-mapper/)

---

## 📝 Editing Guidelines

| Section | Content | Update Timing |
|---------|---------|---------------|
| **Active Features** | WIP / PROPOSED only | On new addition, status change |
| **Recently Completed** | Latest 6 DONE / CANCELLED | On completion, move to history after 6 |

**Rules**:
1. **DONE/CANCELLED → Remove from Active immediately** → Move to Recently Completed
2. **Recently Completed exceeds 6** → Move oldest to [index-features-history.md](index-features-history.md) (no move log here)
3. **Next Feature number** → Listed at file end, increment after use
4. **Feature 作成前** → `.claude/reference/agent-registry.md` 参照

---

## Table Legend

| Column | Meaning |
|--------|---------|
| **Status** | Current state: [DRAFT], [PROPOSED], [REVIEWED], [WIP], [BLOCKED], [DONE], [CANCELLED] |
| **Depends On** | Features THIS feature requires (predecessors). Must be [DONE] before this can start |

**Depends On formatting**:
- `F123` = Dependency is [DONE] (satisfied)
- `**F123**` = Dependency is NOT [DONE] (blocker, this feature cannot proceed)

**Direction**: `A depends on B` means A needs B to complete first. Arrow: `B → A` (B blocks A).

---

## Checkpoints

| Date | Commit | Description |
|------|--------|-------------|
| 2026-01-05 | `3cf1761` | リファクタリング開始点 (Phase 1 Migration) |

---

## Active Features

### Phase 19: Kojo Conversion (ERB→YAML)

**Quality Validation** (F644-F645):
| ID | Status | Name | Depends On | Links |
|:---|:------:|:-----|:-----------|:------|

**Transition** (F646-F647):
| ID | Status | Name | Depends On | Links |
|:---|:------:|:-----|:-----------|:------|

### Phase 20: Equipment & Shop Systems

| ID | Status | Name | Depends On | Links |
|:---|:------:|:-----|:-----------|:------|

### Phase 21: Counter System

| ID | Status | Name | Depends On | Links |
|:---|:------:|:-----|:-----------|:------|

### Phase 22: State Systems

| ID | Status | Name | Depends On | Links |
|:---|:------:|:-----|:-----------|:------|
| F827 | [DRAFT] | Phase 23 Planning | F826 | [feature-827.md](feature-827.md) |
| F844 | [DRAFT] | _UNESCAPE_RULES catch-all regex refactoring | - | [feature-844.md](feature-844.md) |
| F845 | [DRAFT] | Full AC pattern catalog scan across all features | - | [feature-845.md](feature-845.md) |
| F846 | [DRAFT] | feature-status.py Active→Recently Completed migration gap | - | [feature-846.md](feature-846.md) |

### Other

| ID | Status | Name | Depends On | Links |
|:---|:------:|:-----|:-----------|:------|
**Dashboard Recovery**:
| ID | Status | Name | Depends On | Links |
|:---|:------:|:-----|:-----------|:------|

> **実行順序**: (All features cancelled)
---

## Recently Completed

<!-- Overflow → history に移動。移動ログはここに書かない -->

| ID | Status | Name | Links |
|:---|:------:|:-----|:------|
| 842 | ✅ | ac-static-verifier Pattern Parsing Enhancements | [feature-842.md](feature-842.md) |
| 843 | ✅ | Latent GlobalStatic Collection Test Isolation Issues | [feature-843.md](feature-843.md) |
| 828 | ✅ | Date Initialization Migration (@日付初期設定) | [feature-828.md](feature-828.md) |
| 841 | ✅ | Build CWD Cross-Repo Resolution in ac-static-verifier | [feature-841.md](feature-841.md) |
| 840 | ✅ | Engine Test Isolation Failures (GlobalStatic Shared State) | [feature-840.md](feature-840.md) |
| 835 | ✅ | IEngineVariables Abstract Method Stubs — Real VariableData Delegation | [feature-835.md](feature-835.md) |
| 838 | ✅ | Test Infrastructure Fixes — Cross-Repo Verifier and Engine Test Isolation | [feature-838.md](feature-838.md) |

---

## Status Legend

| Status | Description | Criteria |
|--------|-------------|----------|
| `[DRAFT]` | Background のみ、AC/Tasks 未生成 | オーケストレーターがスタブ作成 |
| `[PROPOSED]` | 新規作成、FL未完了 | `/next`で作成直後、またはFL中断後 |
| `[REVIEWED]` | FL完了、実装可能 | `/fl`がゼロ問題で完了 |
| `[WIP]` | 実装中 | `/run`実行中 |
| `[BLOCKED]` | Predecessor/Blocker待ち | Dependencies表のType=Predecessor/Blockerが[DONE]でない |
| `[DONE]` | 完了 | `/run`完了 → Recently Completedへ移動 |
| `[CANCELLED]` | 中止 | 不要と判断 → Recently Completedへ移動 |

**Important**: [BLOCKED]はPredecessor/Blocker依存のみ。Related/Successor依存は対象外。

### Status Transition Rules

| From | To | Trigger | Note |
|------|-----|---------|------|
| [DRAFT] | [PROPOSED] | `/fc` 完了 | AC/Tasks 生成済み |
| [PROPOSED] | [BLOCKED] | `/fl` Phase 0でPredecessorが[DONE]でない | FL中断、実装不可 |
| [PROPOSED] | [REVIEWED] | `/fl`がゼロ問題で完了 | 実装可能 |
| [REVIEWED] | [WIP] | `/run`開始 | 実装中 |
| [WIP] | [DONE] | `/run`完了 | 完了 |
| [BLOCKED] | [PROPOSED] | `/fl`で全Predecessorが[DONE]になった | FLを再開可能 |

**Blocking Logic** (Phase 0 Dependency Gate):
```
FOR each row in Dependencies WHERE Type = "Predecessor":
    IF Predecessor.Status ≠ [DONE]:
        Current feature → [BLOCKED]
        STOP FL (do not proceed to Phase 1)
```

### Dependency Types (SSOT)

| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| `Predecessor` | F{ID} → This | **BLOCKING** | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| `Successor` | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| `Related` | - | None | Reference only. No blocking, no status effect. |

**Usage Notes**:
- Use `Predecessor` when this feature requires another feature to be completed first
- Use `Successor` to document that another feature depends on this one (for tracking)
- Use `Related` for cross-references without dependency relationship

---

## Current Phase: 8d

**Current**: Full COM coverage + quality improvement (4-8 lines + emotion/scene description)

| Phase | Status | Description |
|-------|:------:|-------------|
| 8a | ✅ | Technical Foundation (054-079) |
| 8b | ✅ | Infrastructure (080-082) |
| 8c | ✅ | Content Layer 1 - Basic quality 4 lines (085-093) |
| **8d** | 🔄 | **Full COM coverage + quality improvement (4-8 lines)** |
| 8e | ⏳ | Variation expansion (4 patterns) |
| 8e-mod | ✅ | Modular modifier introduction (Feature 154) |
| 8e-ext | ⏳ | Extended branching (ABL/TALENT - Feature 189) |
| 8f | ⏳ | First Experience (初回体験口上) |
| 8g | ⏳ | Event kojo (eraTW compliant) |
| 8h | ⏳ | NTR-specific kojo (comparison/aftermath/3P/exposure) |
| 8i | ⏳ | Location/situation kojo (outdoor/location branching) |
| 8k | ⏳ | Special situation kojo (WC/SexHara/bathing) |

---

## Progress Tracking

**Kojo Coverage**: Run `python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --coverage`

**COM Progress**: Run `python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --progress`

See [kojo-mapper](../../tools/kojo-mapper/) for detailed coverage analysis.

---

## Links

- [index-features-history.md](index-features-history.md) - Full history
- [content-roadmap.md](content-roadmap.md) - Long-term content planning
- [reference/](reference/) - Technical reference

---

**Next Feature number**: 847
