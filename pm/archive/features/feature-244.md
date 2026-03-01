# Feature 244: COM_67 対面座位 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem (Current Issue)
COM_67 (対面座位) lacks Phase 8d quality dialogue for all characters.

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
| 1 | K1美鈴 COM_67 口上出力 | output | --unit tests/ac/kojo/feature-244/test-244-K1.json | contains | DATALIST output | [x] |
| 2 | K2小悪魔 COM_67 口上出力 | output | --unit tests/ac/kojo/feature-244/test-244-K2.json | contains | DATALIST output | [x] |
| 3 | K3パチュリー COM_67 口上出力 | output | --unit tests/ac/kojo/feature-244/test-244-K3.json | contains | DATALIST output | [x] |
| 4 | K4咲夜 COM_67 口上出力 | output | --unit tests/ac/kojo/feature-244/test-244-K4.json | contains | DATALIST output | [x] |
| 5 | K5レミリア COM_67 口上出力 | output | --unit tests/ac/kojo/feature-244/test-244-K5.json | contains | DATALIST output | [x] |
| 6 | K6フラン COM_67 口上出力 | output | --unit tests/ac/kojo/feature-244/test-244-K6.json | contains | DATALIST output | [x] |
| 7 | K7子悪魔 COM_67 口上出力 | output | --unit tests/ac/kojo/feature-244/test-244-K7.json | contains | DATALIST output | [x] |
| 8 | K8チルノ COM_67 口上出力 | output | --unit tests/ac/kojo/feature-244/test-244-K8.json | contains | DATALIST output | [x] |
| 9 | K9大妖精 COM_67 口上出力 | output | --unit tests/ac/kojo/feature-244/test-244-K9.json | contains | DATALIST output | [x] |
| 10 | K10魔理沙 COM_67 口上出力 | output | --unit tests/ac/kojo/feature-244/test-244-K10.json | contains | DATALIST output | [x] |
| 11 | Build succeeds | build | - | succeeds | - | [x] |
| 12 | Regression tests pass | output | --flow | contains | "passed (100%)" | [x] |

### AC Details

**AC 1-10 Test**: `dotnet run ... --unit "tests/ac/kojo/feature-244/"`
**Expected**: Each character outputs COM_67 dialogue with 4 TALENT branches × 4 patterns (160 tests total)

**AC 11 Test**: `dotnet build`
**Expected**: Build succeeds without errors

**AC 12 Test**: `dotnet run ... --flow`
**Expected**: All regression scenarios pass (100%)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | K1美鈴 COM_67 口上作成 (4分岐×4パターン) | [x] |
| 2 | 2 | K2小悪魔 COM_67 口上作成 (4分岐×4パターン) | [x] |
| 3 | 3 | K3パチュリー COM_67 口上作成 (4分岐×4パターン) | [x] |
| 4 | 4 | K4咲夜 COM_67 口上作成 (4分岐×4パターン) | [x] |
| 5 | 5 | K5レミリア COM_67 口上作成 (4分岐×4パターン) | [x] |
| 6 | 6 | K6フラン COM_67 口上作成 (4分岐×4パターン) | [x] |
| 7 | 7 | K7子悪魔 COM_67 口上作成 (4分岐×4パターン) | [x] |
| 8 | 8 | K8チルノ COM_67 口上作成 (4分岐×4パターン) | [x] |
| 9 | 9 | K9大妖精 COM_67 口上作成 (4分岐×4パターン) | [x] |
| 10 | 10 | K10魔理沙 COM_67 口上作成 (4分岐×4パターン) | [x] |
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
| 1 | eratw-reader | haiku | COM_67 | eraTW reference cache |
| 2 | kojo-writer | opus | K1-K10, COM_67, reference | KOJO_K{N}_挿入.ERB updates |
| 3 | - | - | dotnet build | Build verification |
| 4 | regression-tester | haiku | --flow | Regression results |
| 5 | ac-tester | haiku | feature-244.json | AC verification |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

- **2025-12-31**: ⚠️ **配置誤り警告** - F242/F243 で kojo-writer が COM 60-72 を誤って `口挿入.ERB` に配置した前例あり。実装前に `python tools/erb-duplicate-check.py` で既存関数の配置を確認し、必ず `KOJO_K{N}_挿入.ERB` に書くこと。口挿入.ERB は COM 80-203 専用。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-31 | Phase 1 | initializer | Status update | [WIP] |
| 2025-12-31 | Phase 2 | explorer | Investigation | READY |
| 2025-12-31 | Phase 4 | eratw-reader | COM_67 reference | OK:cached |
| 2025-12-31 | Phase 4 | kojo-writer x10 | K1-K10 implementation | OK:10/10 |
| 2025-12-31 | Phase 5 | - | Test generation | OK:160 tests |
| 2025-12-31 | Phase 6 | - | Build | OK:0 errors |
| 2025-12-31 | Phase 6 | regression-tester | Regression | OK:24/24 |
| 2025-12-31 | Phase 6 | ac-tester | AC verification | OK:160/160 |
| 2025-12-31 | Phase 7 | feature-reviewer | Post-review | NEEDS_REVISION→Fixed |

---

## Issues Found (スコープ外)

| Issue | 原因 | 回避策 |
|-------|------|--------|
| kojo-writer がステータスファイル未出力 | kojo-writer.md にステータス出力手順なし | grep で ERB 直接確認 |
| AC Expected が `{auto}` プレースホルダー | kojo-init テンプレート不備 | 手動で正しいパスに修正 |
| Task 1-9 ステータス未更新 | kojo-writer が feature-244.md を更新しない | 手動で [x] に更新 |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
