# Feature 190: COM_60 Duplicate Cleanup

## Status: [DONE]

## Type: erb

## Background

### Problem
COM_60（正常位 = **膣挿入**）の口上が `_挿入.ERB` と `_口挿入.ERB` の**両方**に重複して存在している。

**正しい分類:**
| 範囲 | カテゴリ | ファイル |
|------|----------|----------|
| 60-72 | 挿入系（膣/アナル） | `_挿入.ERB` |
| 80-85 | 手技系（フェラ/パイズリ） | `_口挿入.ERB` |

**現状（実際に確認済み）:**
| キャラ | `_挿入.ERB` | `_口挿入.ERB` | 状態 |
|--------|:-----------:|:-------------:|------|
| K1 | ✓ | - | OK |
| K2 | ✓ | ✓ | **重複** |
| K3 | ✓ | - | OK |
| K4 | ✓ | ✓ | **重複** |
| K5 | ✓ | ✓ | **重複** |
| K6 | ✓ | - | OK |
| K7 | ✓ | ✓ | **重複** |
| K8 | ✓ | ✓ | **重複** |
| K9 | ✓ | - | OK |
| K10 | ✓ | ✓ | **重複** |
| KU | - | ✓ | **要移動** |

**根本原因:**
kojo-writer への指示に COM→ファイルカテゴリ のマッピングがなかった。

### Goal
1. `_口挿入.ERB` から COM_60 口上を削除し、重複を解消する
2. kojo-writer.md にマッピング表を追加し、再発を防止する

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | _口挿入.ERBにCOM_60なし | code | not_contains | "@KOJO_MESSAGE_COM_K.*_60" | [x] |
| 2 | _挿入.ERBにCOM_60あり（11ファイル） | output | equals | "11" | [x] |
| 3 | KU COM_60を_挿入.ERBに移動 | output | contains | "KOJO_KU_挿入.ERB" | [x] |
| 4 | kojo-writer.mdにマッピング表追加 | code | contains | "60-72" | [x] |
| 5 | ビルド成功 | build | succeeds | - | [x] |

**AC1 Method**: `grep -l "@KOJO_MESSAGE_COM_K.*_60" Game/ERB/口上/*/*_口挿入.ERB || echo "No matches"`
**AC2 Method**: `grep -l "@KOJO_MESSAGE_COM_K.*_60" Game/ERB/口上/*/*_挿入.ERB | wc -l`
**AC3 Method**: `grep -l "@KOJO_MESSAGE_COM_KU_60" Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB`
**AC4 Method**: `grep "60-72" .claude/agents/kojo-writer.md`
**AC5 Method**: `dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit`

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | 1 | K2,K4,K5,K7,K8,K10 _口挿入.ERBからCOM_60削除 | `*_口挿入.ERB` | [x] |
| 2 | 2 | _挿入.ERBにCOM_60が11ファイル存在することを確認 | `*_挿入.ERB` | [x] |
| 3 | 3 | KU COM_60を_挿入.ERBに移動 | `KOJO_KU_挿入.ERB` | [x] |
| 4 | 4 | kojo-writer.mdにCOM→ファイルマッピング表追加 | `.claude/agents/kojo-writer.md` | [x] |
| 5 | 5 | テスト再実行・修正（K4重複関数削除） | - | [x] |

---

## Root Cause Fix

kojo-writer.md に COM→ファイルマッピング表を追加:

```markdown
## File Category Mapping

| COM範囲 | カテゴリ | ファイル名 |
|---------|----------|-----------|
| 0-11 | 愛撫系 | `_愛撫.ERB` |
| 40-48 | 道具系 | `_道具.ERB` |
| 60-72 | 挿入系 | `_挿入.ERB` |
| 80-85 | 手技系 | `_口挿入.ERB` |
```

---

## Review Notes

- 2024-12-24: feature-reviewer により事実誤認を修正
  - 旧: 「配置エラー」→ 新: 「重複」
  - AC/Taskを現状に合わせて修正

---

## Execution Log

| Date | Task | Agent | Result |
|------|------|-------|--------|
| 2025-12-24 | Phase 1-2 | initializer, explorer | READY |
| 2025-12-24 | Phase 3 | - | TDD RED confirmed |
| 2025-12-24 | Phase 4 | implementer | SUCCESS |
| 2025-12-24 | Phase 6 | - | Build PASS |
| 2025-12-24 | Phase 7 | - | AC 1-5 PASS |
| 2025-12-24 | Phase 8 | regression-tester | K4重複検出 |
| 2025-12-24 | Debug | debugger | K4 CHK_CANCEL_COM60 削除 |
| 2025-12-24 | Phase 9 | - | Consistency OK |

**Notes:**
- AC2: KU移動により10→11ファイルに変更（正しい動作）
- PRE-EXISTING: K4/K5/K10 _挿入.ERBにエンコーディング警告あり（Feature 190対象外）

---

## Links

- [feature-186.md](feature-186.md)
- [com-map.md](reference/com-map.md)
