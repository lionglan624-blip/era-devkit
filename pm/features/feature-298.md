# Feature 298: do.md --check-stub ワークフロー統合

## Status: [DONE]

## Type: infra

## Background

### Philosophy
/do ワークフローが erb-duplicate-check.py の --check-stub オプションを活用し、スタブと実装済みを正確に区別して適切な処理を行う。

### Problem
F288 で K4, K9 が `OK: 1 definition` を返したため「実装済み」と誤判断する可能性があった。F294 で --check-stub オプションを追加したが、do.md ワークフローはまだこれを使用していない。

### Goal
do.md Phase 4.0 Pre-Implementation Check を更新し、--check-stub を使用した STUB/IMPLEMENTED 判定ロジックを追加する。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md に --check-stub 使用記述 | code | Grep | contains | "--check-stub" | [x] |
| 2 | STUB 検出時の処理記述 | code | Grep | matches | "STUB.*Remove" | [x] |
| 3 | IMPLEMENTED 検出時の処理記述 | code | Grep | matches | "IMPLEMENTED.*Skip" | [x] |
| 4 | DUPLICATE 検出時の処理記述 | code | Grep | matches | "DUPLICATE.*STOP" | [x] |

### AC Details

**AC#1 Test**: `grep "--check-stub" .claude/commands/do.md`

**AC#2 Test**: `grep -E "STUB.*Remove" .claude/commands/do.md`
**Expected**: STUB result row with Remove/削除 action

**AC#3 Test**: `grep -E "IMPLEMENTED.*Skip" .claude/commands/do.md`
**Expected**: IMPLEMENTED result row with Skip action

**AC#4 Test**: `grep -E "DUPLICATE.*STOP" .claude/commands/do.md`
**Expected**: DUPLICATE result row with STOP action

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-4 | do.md Phase 4.0 Pre-Implementation Check を --check-stub 使用に更新 | [○] |

---

## Implementation Notes

### 現行 (do.md Phase 4.0)

```markdown
**Duplicate Check**: For each target character K{N}:
python tools/erb-duplicate-check.py --function "KOJO_MESSAGE_COM_K{N}_{COM}" --path "Game/ERB/口上"

| Result | Action |
|--------|--------|
| OK | Continue to eraTW Cache |
| DUPLICATE | Remove stub first, then continue |
```

### 変更案

```markdown
**Stub Check**: For each target character K{N}:
python tools/erb-duplicate-check.py --check-stub --function "KOJO_MESSAGE_COM_K{N}_{COM}" --path "Game/ERB/口上"

| Result | Action |
|--------|--------|
| NOT_FOUND | Continue to eraTW Cache (new implementation) |
| STUB | Remove stub first (see procedure below), then continue |
| IMPLEMENTED | **Skip** - already implemented, do not overwrite |
| DUPLICATE | **STOP** → Report to user (multiple definitions found) |
```

**Note**: --check-stub は単一定義時に STUB/IMPLEMENTED を判定。複数定義時は --check-stub の有無に関わらず DUPLICATE を返す。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 | Phase 4 | implementer | Update do.md Phase 4.0 | SUCCESS |
| 2026-01-01 | Phase 6 | - | AC verification | 4/4 PASS |
| 2026-01-01 | Phase 7 | feature-reviewer | Post-review | READY |
| 2026-01-01 | Phase 9 | finalizer | Finalize | DONE |

---

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-294.md](feature-294.md)
