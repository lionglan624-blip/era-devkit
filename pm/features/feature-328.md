# Feature 328: COM_99 騎乗位する 口上 (Phase 8d)

## Status: [CANCELLED]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem
COM_99 (騎乗位する) lacks Phase 8d quality dialogue for all characters.

### Goal
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8d: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character
- COM特性: TCVAR:116 = TARGET (相手が行為者), MASTERが騎乗位で挿入する

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K1美鈴 COM_99 口上出力 | output | --unit tests/ac/kojo/feature-328/test-328-K1.json | contains | DATALIST output | [ ] |
| 2 | K2小悪魔 COM_99 口上出力 | output | --unit tests/ac/kojo/feature-328/test-328-K2.json | contains | DATALIST output | [ ] |
| 3 | K3パチュリー COM_99 口上出力 | output | --unit tests/ac/kojo/feature-328/test-328-K3.json | contains | DATALIST output | [ ] |
| 4 | K4咲夜 COM_99 口上出力 | output | --unit tests/ac/kojo/feature-328/test-328-K4.json | contains | DATALIST output | [ ] |
| 5 | K5レミリア COM_99 口上出力 | output | --unit tests/ac/kojo/feature-328/test-328-K5.json | contains | DATALIST output | [ ] |
| 6 | K6フラン COM_99 口上出力 | output | --unit tests/ac/kojo/feature-328/test-328-K6.json | contains | DATALIST output | [ ] |
| 7 | K7子悪魔 COM_99 口上出力 | output | --unit tests/ac/kojo/feature-328/test-328-K7.json | contains | DATALIST output | [ ] |
| 8 | K8チルノ COM_99 口上出力 | output | --unit tests/ac/kojo/feature-328/test-328-K8.json | contains | DATALIST output | [ ] |
| 9 | K9大妖精 COM_99 口上出力 | output | --unit tests/ac/kojo/feature-328/test-328-K9.json | contains | DATALIST output | [ ] |
| 10 | K10魔理沙 COM_99 口上出力 | output | --unit tests/ac/kojo/feature-328/test-328-K10.json | contains | DATALIST output | [ ] |
| 11 | Build succeeds | build | - | succeeds | - | [ ] |
| 12 | Regression tests pass | output | --flow tests/regression/ | contains | "passed (100%)" | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_99 口上作成 (4分岐×4パターン) | [ ] |
| 2 | 11 | ビルド確認 | [ ] |
| 3 | 12 | 回帰テスト | [ ] |
| 4 | 1-10 | AC検証 | [ ] |

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
