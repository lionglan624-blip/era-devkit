# Feature 240: /kojo-init Workflow Simplification

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
ワークフローは単純で予測可能であるべき。kojo と non-kojo で明確に分離し、各コマンドの責務を明確化する。

### Problem
`/kojo-init` の動作が複雑で予測しづらい:
1. **design-first ロジックが `/next` と重複**: APPROVED design の確認が両コマンドに存在
2. **Phase 8 Summary が実態と乖離**: 手動更新が必要で不整合が発生
3. **COM Series Order の参照先が分散**: content-roadmap, index-features, kojo-writing SKILL に分散

### Goal
1. `/kojo-init` から design-first ロジックを削除
2. content-roadmap の COM Series Order を唯一の参照元とする

**Note**: Phase 8 Summary 削除は別 Feature で実装予定（kojo-mapper に COM カテゴリ別進捗機能を追加後）

### Session Context
- **Original problem**: `/kojo-init` 実行時にどの COM が選ばれるか予測できない
- **Considered alternatives**:
  - ❌ `/kojo-init` を `/next` に統合 - `/next` の複雑化を招く
  - ✅ `/kojo-init` を単純化し content-roadmap のみ参照 - 予測可能性向上
- **Key decisions**: kojo に design は不要、COM 順序は content-roadmap で十分

---

## Overview

### Before
```
/kojo-init
  ├─ Check designs/README.md for APPROVED kojo design
  │   └─ If found → Use design-defined Feature IDs
  └─ Fallback → content-roadmap COM Series Order
```

### After
```
/kojo-init
  └─ content-roadmap COM Series Order のみ参照
```

### Workflow Clarification
```
[kojo 以外]
/plan → designs/*.md (APPROVED) → /next → feature-{ID}.md → /do

[kojo]
/kojo-init → feature-{ID}.md (PROPOSED) → /do
```

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | kojo-init.md から designs/README.md 参照を削除 | code | Grep | not_contains | "designs/README.md" | [x] |
| 2 | kojo-init.md から APPROVED kojo design ロジックを削除 | code | Grep | not_contains | "APPROVED kojo design" | [x] |

### AC Details

**AC1 Test**: `Grep("designs/README.md", ".claude/commands/kojo-init.md")` → expect 0 matches
**AC2 Test**: `Grep("APPROVED kojo design", ".claude/commands/kojo-init.md")` → expect 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo-init.md から designs/README.md 参照を削除 | [x] |
| 2 | 2 | kojo-init.md から APPROVED kojo design ロジックを削除 | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | Initialize | initializer | Feature 240 validation | READY |
| 2025-12-27 | Investigate | explorer | Read kojo-init.md | READY |
| 2025-12-27 | Implement | implementer | Remove design-first logic | SUCCESS |
| 2025-12-27 | Verify | ac-tester | AC1: designs/README.md | PASS (0 matches) |
| 2025-12-27 | Verify | ac-tester | AC2: APPROVED kojo design | PASS (0 matches) |
| 2025-12-27 | Post-Review | feature-reviewer | Mode: post | READY |
| 2025-12-27 | Finalize | finalizer | Mark [DONE], update index | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- [kojo-init.md](../../.claude/commands/kojo-init.md)
- [next.md](../../.claude/commands/next.md)
- [content-roadmap.md](content-roadmap.md)
