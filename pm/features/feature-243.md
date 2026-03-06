# Feature 243: COM_66 騎乗位アナル 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem (Current Issue)
COM_66 (騎乗位アナル) lacks Phase 8d quality dialogue for all characters.

### Goal (What to Achieve)
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8d: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character
- File: KOJO_K{N}_挿入.ERB (COM 60-72 range)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K1美鈴 COM_66 口上出力 | output | --unit | contains | "{auto}" | [x] |
| 2 | K2小悪魔 COM_66 口上出力 | output | --unit | contains | "{auto}" | [x] |
| 3 | K3パチュリー COM_66 口上出力 | output | --unit | contains | "{auto}" | [x] |
| 4 | K4咲夜 COM_66 口上出力 | output | --unit | contains | "{auto}" | [x] |
| 5 | K5レミリア COM_66 口上出力 | output | --unit | contains | "{auto}" | [x] |
| 6 | K6フラン COM_66 口上出力 | output | --unit | contains | "{auto}" | [x] |
| 7 | K7子悪魔 COM_66 口上出力 | output | --unit | contains | "{auto}" | [x] |
| 8 | K8チルノ COM_66 口上出力 | output | --unit | contains | "{auto}" | [x] |
| 9 | K9大妖精 COM_66 口上出力 | output | --unit | contains | "{auto}" | [x] |
| 10 | K10魔理沙 COM_66 口上出力 | output | --unit | contains | "{auto}" | [x] |
| 11 | Build succeeds | build | - | succeeds | - | [x] |
| 12 | Regression tests pass | output | --flow | contains | "passed (100%)" | [x] |

### AC Details

**AC 1-10 Test**: `dotnet run ... --unit "tests/ac/feature-243.json"`
**Expected**: Each character outputs COM_66 dialogue with 4 TALENT branches × 4 patterns

**AC 11 Test**: `dotnet build`
**Expected**: Build succeeds without errors

**AC 12 Test**: `dotnet run ... --flow`
**Expected**: All regression scenarios pass (100%)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | K1美鈴 COM_66 口上作成 (4分岐×4パターン) | [x] |
| 2 | 2 | K2小悪魔 COM_66 口上作成 (4分岐×4パターン) | [x] |
| 3 | 3 | K3パチュリー COM_66 口上作成 (4分岐×4パターン) | [x] |
| 4 | 4 | K4咲夜 COM_66 口上作成 (4分岐×4パターン) | [x] |
| 5 | 5 | K5レミリア COM_66 口上作成 (4分岐×4パターン) | [x] |
| 6 | 6 | K6フラン COM_66 口上作成 (4分岐×4パターン) | [x] |
| 7 | 7 | K7子悪魔 COM_66 口上作成 (4分岐×4パターン) | [x] |
| 8 | 8 | K8チルノ COM_66 口上作成 (4分岐×4パターン) | [x] |
| 9 | 9 | K9大妖精 COM_66 口上作成 (4分岐×4パターン) | [x] |
| 10 | 10 | K10魔理沙 COM_66 口上作成 (4分岐×4パターン) | [x] |
| 11 | 11 | ビルド確認 | [x] |
| 12 | 12 | 回帰テスト | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | eratw-reader | haiku | COM_66 | eraTW reference cache |
| 2 | kojo-writer | opus | K1-K10, COM_66, reference | KOJO_K{N}_挿入.ERB updates |
| 3 | - | - | dotnet build | Build verification |
| 4 | regression-tester | haiku | --flow | Regression results |
| 5 | ac-tester | haiku | feature-243.json | AC verification |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-31 | Init | initializer | Feature 243 initialized | READY |
| 2025-12-31 | Explore | explorer | Investigation completed | READY |
| 2025-12-31 | Cache | eratw-reader | COM_66 reference cached | OK |
| 2025-12-31 | Impl | kojo-writer×10 | K1-K10 COM_66 dialogue | OK |
| 2025-12-31 | Fix | debugger | K4 COM_65/66 moved to 挿入.ERB | FIXED |
| 2025-12-31 | Test | kojo_test_gen.py | 160 tests generated | OK |
| 2025-12-31 | Build | - | dotnet build | PASS |
| 2025-12-31 | Regr | regression-tester | 24/24 scenarios | PASS |
| 2025-12-31 | AC | ac-tester | 160/160 tests | PASS |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
