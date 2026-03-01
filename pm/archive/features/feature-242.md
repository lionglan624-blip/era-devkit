# Feature 242: COM_65 騎乗位 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem
COM_65 (騎乗位) lacks Phase 8d quality dialogue for all characters.

### Goal
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8d: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character
- File: KOJO_K{N}_挿入.ERB (COM 60-72 range)

---

## Acceptance Criteria

| AC# | Char | Type | Method | Matcher | Expected | MockRand | Status |
|:---:|------|------|--------|---------|----------|:--------:|:------:|
| 1 | K1美鈴 | output | --unit | contains | "{auto}" | [0] | [x] |
| 2 | K2小悪魔 | output | --unit | contains | "{auto}" | [0] | [x] |
| 3 | K3パチュリー | output | --unit | contains | "{auto}" | [0] | [x] |
| 4 | K4咲夜 | output | --unit | contains | "{auto}" | [0] | [x] |
| 5 | K5レミリア | output | --unit | contains | "{auto}" | [0] | [x] |
| 6 | K6フラン | output | --unit | contains | "{auto}" | [0] | [x] |
| 7 | K7子悪魔 | output | --unit | contains | "{auto}" | [0] | [x] |
| 8 | K8チルノ | output | --unit | contains | "{auto}" | [0] | [x] |
| 9 | K9大妖精 | output | --unit | contains | "{auto}" | [0] | [x] |
| 10 | K10魔理沙 | output | --unit | contains | "{auto}" | [0] | [x] |
| 11 | Build | build | - | succeeds | - | - | [x] |
| 12 | Regression | output | - | contains | "passed (100%)" | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_65 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 00:00 | Initialization | initializer | Status [PROPOSED]→[WIP] | Ready for Phase 2 |
| 2025-12-28 | Phase 4 | kojo-writer ×10 | K1-K10 実装 | OK:10/10 |
| 2025-12-28 | Phase 5 | kojo_test_gen.py | テスト生成 | 7/10 auto, 3/10 manual |
| 2025-12-28 | Phase 6 | - | Regression + AC | PASS:184/184 |
| 2025-12-28 | Phase 7 | feature-reviewer | Post-review | READY |

---

## Issues Found

| # | 事象 | 原因 | 対処方針 |
|:-:|------|------|----------|
| 1 | K2/K4/K9 が口挿入.ERB に書かれた | Opus が explorer 結果を prompt にハードコード | kojo-writer が SSOT 参照で自己決定 |
| 2 | kojo-writer が間違った File を使用 | prompt に File 指定あり、SSOT より優先した | dispatch を最小化 (`{ID} K{N}` のみ) |
| 3 | kojo-writer.md に prompt 解釈なし | Input セクションは読むファイルのみ記載 | Dispatch Format セクション追加 |
| 4 | kojo_test_gen.py が K2/K4/K9 失敗 | 口挿入.ERB を検索対象外 | K2/K4/K9 を挿入.ERB に移動で解決 |
| 5 | do.md に kojo dispatch 例なし | 具体例がなく Opus が自由に書いた | 最小限の例を追加 |
| 6 | 全 COM にスタブが残存 | F190 統合が不完全 | 全スタブを正規ファイルに統合 |

**対処 Feature**: [feature-260.md](feature-260.md)

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
