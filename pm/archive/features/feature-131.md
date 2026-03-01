# Feature 131: ERB重複関数警告修正

## Status: [DONE]

## Execution State

- **Current**: Completed
- **Assignment**: Finalized
- **Last Updated**: 2025-12-19
- **Notes**: All 7 Tasks complete, all 7 ACs passed (100%)

## Type: erb

## Background

### Problem
kojo-writerが愛撫.ERBと個別COM用ERB（乳首責め.ERB等）の両方に同じ関数を書くことで、重複定義警告が発生。

### Current Warnings (6件)
```
K2_愛撫.ERB:1332 → @KOJO_MESSAGE_COM_K2_7 (also in K2_乳首責め.ERB)
K2_愛撫.ERB:1338 → @KOJO_MESSAGE_COM_K2_7_1 (also in K2_乳首責め.ERB)
K4_愛撫.ERB:1378 → @KOJO_MESSAGE_COM_K4_7 (also in K4_乳首責め.ERB)
K4_愛撫.ERB:1384 → @KOJO_MESSAGE_COM_K4_7_1 (also in K4_乳首責め.ERB)
K9_愛撫.ERB:1417 → @KOJO_MESSAGE_COM_K9_7 (also in K9_乳首責め.ERB)
K9_愛撫.ERB:1423 → @KOJO_MESSAGE_COM_K9_7_1 (also in K9_乳首責め.ERB)
```

### Goal
重複関数を削除し、警告ゼロを達成。

### Root Cause
- kojo-writerがCOM_7を愛撫.ERBに追記する設計と、個別ファイル作成の設計が混在
- 一部キャラ(K2,K4,K9)で両方に書き込まれた

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | 重複関数の所在確認 | code | contains | "@KOJO_MESSAGE_COM_K2_7" | [x] |
| 2 | 重複警告ゼロ | output | not_contains | "[WARNING]" | [x] |
| 3 | COM_7動作確認 K2 | output | contains | "乳首ばかり" | [x] |
| 4 | COM_7動作確認 K4 | output | contains | "弱いって" | [x] |
| 5 | COM_7動作確認 K9 | output | contains | "そこ、弱いの" | [x] |
| 6 | Build成功 | build | succeeds | - | [x] |
| 7 | Regression PASS | output | contains | "passed (100%)" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | 重複関数の所在確認 (愛撫.ERB vs 個別.ERB) | [x] |
| 2 | 2 | 愛撫.ERBから重複関数を削除 | [x] |
| 3 | 3 | K2個別ファイルの関数が正常動作することを確認 | [x] |
| 4 | 4 | K4個別ファイルの関数が正常動作することを確認 | [x] |
| 5 | 5 | K9個別ファイルの関数が正常動作することを確認 | [x] |
| 6 | 6 | ビルド確認 | [x] |
| 7 | 7 | 回帰テスト実行 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2025-12-19 11:29 | START | implementer | Task 2 | - |
| 2025-12-19 11:30 | END | implementer | Task 2 | SUCCESS (1min) |
| 2025-12-19 11:40 | START | finalizer | Feature 131 | - |
| 2025-12-19 11:40 | END | finalizer | Feature 131 | DONE (completed) |

---

## Design Decision

| 項目 | 決定 |
|------|------|
| 残す場所 | 個別ファイル (乳首責め.ERB等) |
| 削除する場所 | 愛撫.ERB |
| 理由 | 個別ファイルの方が新規作成・品質が高い |

---

## Notes

- 優先度: Low (機能に影響なし、警告のみ)
- 発見: Feature 127 実装時の regression-tester
- kojo-writer指示の改善も検討 (将来の重複防止)
