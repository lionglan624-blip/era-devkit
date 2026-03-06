# Feature 288: COM_82 パイズリ 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem
COM_82 (パイズリ) lacks Phase 8d quality dialogue for all characters.

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
| 1 | K1美鈴 COM_82 口上出力 | output | --unit tests/ac/kojo/feature-288/test-288-K1.json | contains | DATALIST output | [x] |
| 2 | K2小悪魔 COM_82 口上出力 | output | --unit tests/ac/kojo/feature-288/test-288-K2.json | contains | DATALIST output | [x] |
| 3 | K3パチュリー COM_82 口上出力 | output | --unit tests/ac/kojo/feature-288/test-288-K3.json | contains | DATALIST output | [x] |
| 4 | K4咲夜 COM_82 口上出力 | output | --unit tests/ac/kojo/feature-288/test-288-K4.json | contains | DATALIST output | [x] |
| 5 | K5レミリア COM_82 口上出力 | output | --unit tests/ac/kojo/feature-288/test-288-K5.json | contains | DATALIST output | [x] |
| 6 | K6フラン COM_82 口上出力 | output | --unit tests/ac/kojo/feature-288/test-288-K6.json | contains | DATALIST output | [x] |
| 7 | K7子悪魔 COM_82 口上出力 | output | --unit tests/ac/kojo/feature-288/test-288-K7.json | contains | DATALIST output | [x] |
| 8 | K8チルノ COM_82 口上出力 | output | --unit tests/ac/kojo/feature-288/test-288-K8.json | contains | DATALIST output | [x] |
| 9 | K9大妖精 COM_82 口上出力 | output | --unit tests/ac/kojo/feature-288/test-288-K9.json | contains | DATALIST output | [x] |
| 10 | K10魔理沙 COM_82 口上出力 | output | --unit tests/ac/kojo/feature-288/test-288-K10.json | contains | DATALIST output | [x] |
| 11 | Build succeeds | build | - | succeeds | - | [x] |
| 12 | Regression tests pass | output | --flow tests/regression/ | contains | "passed (100%)" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_82 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| | | | | |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
