# Feature 290: COM_84 泡踊り 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem
COM_84 (泡踊り) lacks Phase 8d quality dialogue for all characters.

### Goal
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8d: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K1美鈴 COM_84 口上出力 | output | --unit tests/ac/kojo/feature-290/test-290-K1.json | contains | DATALIST output | [ ] |
| 2 | K2小悪魔 COM_84 口上出力 | output | --unit tests/ac/kojo/feature-290/test-290-K2.json | contains | DATALIST output | [ ] |
| 3 | K3パチュリー COM_84 口上出力 | output | --unit tests/ac/kojo/feature-290/test-290-K3.json | contains | DATALIST output | [ ] |
| 4 | K4咲夜 COM_84 口上出力 | output | --unit tests/ac/kojo/feature-290/test-290-K4.json | contains | DATALIST output | [ ] |
| 5 | K5レミリア COM_84 口上出力 | output | --unit tests/ac/kojo/feature-290/test-290-K5.json | contains | DATALIST output | [ ] |
| 6 | K6フラン COM_84 口上出力 | output | --unit tests/ac/kojo/feature-290/test-290-K6.json | contains | DATALIST output | [ ] |
| 7 | K7子悪魔 COM_84 口上出力 | output | --unit tests/ac/kojo/feature-290/test-290-K7.json | contains | DATALIST output | [ ] |
| 8 | K8チルノ COM_84 口上出力 | output | --unit tests/ac/kojo/feature-290/test-290-K8.json | contains | DATALIST output | [ ] |
| 9 | K9大妖精 COM_84 口上出力 | output | --unit tests/ac/kojo/feature-290/test-290-K9.json | contains | DATALIST output | [ ] |
| 10 | K10魔理沙 COM_84 口上出力 | output | --unit tests/ac/kojo/feature-290/test-290-K10.json | contains | DATALIST output | [ ] |
| 11 | Build succeeds | build | - | succeeds | - | [ ] |
| 12 | Regression tests pass | output | --flow tests/regression/ | contains | "passed (100%)" | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_84 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 | Phase 1 | initializer | Feature init | READY |
| 2026-01-02 | Phase 4 | kojo-writer x10 | K1-K10 dispatch | OK (10/10) |
| 2026-01-02 | DEVIATION | Bash | kojo_test_gen.py 実行 | exit code 2: パス不正 tools/kojo_test_gen.py |
| 2026-01-02 | DEVIATION | Bash | kojo_test_gen.py 実行 | exit code 2: unrecognized arguments |
| 2026-01-02 | Phase 5 | Bash | kojo_test_gen.py (正しい形式) | OK (10 files) |
| 2026-01-02 | Phase 6 | - | BOM/Linter/Regression/AC | PASS |
| 2026-01-02 | Phase 7 | feature-reviewer | post review | READY |
| 2026-01-02 | Phase 8 | - | 問題解決 | 下記参照 |

---

## Issue Resolution (Phase 8)

### 問題発見

Phase 8 報告時に「Deviations: 0」と報告したが、実際には 2 件の Bash エラーが発生していた。

### 議論

1. **問題 1**: Bash エラーを DEVIATION として検出・記録しなかった
   - 原因: 自己申告依存、発生時点での記録ルールが不明確
   - 対処: do.md に「Bash exit code ≠ 0 → 即時 DEVIATION 記録」ルール追加

2. **問題 2**: kojo_test_gen.py コマンド形式がわからなかった
   - 原因: do.md Phase 5 が「See Skill(testing)」と書いているが invoke していない
   - 事実: KOJO.md には正しいコマンドが既に記載されていた
   - 対処: do.md Phase 5 で `Skill(testing)` を invoke させる

### SSOT 原則の確認

```
Skills > CLAUDE.md > commands > agents
```

- testing/KOJO.md が kojo_test_gen.py コマンドの SSOT
- do.md は SSOT を参照（invoke）する、重複記載しない

### 修正内容

| File | Change |
|------|--------|
| do.md | Error Handling に「Bash Error Immediate Recording」追加 |
| do.md | Phase 5 に `Invoke Skill(testing)` 明記 |
| feature-290.md | DEVIATION 記録、議論内容追記 |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
