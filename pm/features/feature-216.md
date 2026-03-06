# Feature 216: Kojo ABL/TALENT/EXP Branch Function Implementation

**Status**: DONE
**Type**: erb
**Priority**: Medium
**Version**: v0.7+
**Depends**: F215

---

## Background

### Philosophy (思想・上位目標)

口上は単なるTALENT分岐（思慕/恋慕/恋人）だけでなく、キャラクターの感覚・感度・中毒・経験値を反映した多層的な分岐を持つべき。システムが蓄積する変数は口上にも反映されるべきである。

### Problem (現状の問題)

1. F215 で分岐候補マトリクスと関数設計を確定したが、実行可能コードがない
2. 各口上ファイルで個別に分岐ロジックを書くと重複・不整合が発生する
3. 分岐閾値の変更時に全ファイルを修正する必要がある

### Goal (このFeatureで達成すること)

1. GET_ABL_BRANCH / GET_TALENT_BRANCH / GET_EXP_BRANCH 関数を COMMON_KOJO.ERB に実装する
2. 境界値テストで正確性を保証する
3. 口上ファイルから呼び出し可能な共通関数として提供する

---

## Technical Details

### 分岐判定関数の実装

```erb
;============================================
; ABL/TALENT/EXP 分岐判定関数
;============================================

@GET_ABL_BRANCH(ARG, ARG:1)
;ARG = TARGET番号, ARG:1 = ABL番号
;RESULT = 0:低 / 1:中 / 2:高
SIF ABL:ARG:ARG:1 >= 3
    RETURN 2
SIF ABL:ARG:ARG:1 >= 1
    RETURN 1
RETURN 0

@GET_TALENT_BRANCH(ARG, ARG:1)
;ARG = TARGET番号, ARG:1 = TALENT番号
;RESULT = 0:通常 / 1:敏感
SIF TALENT:ARG:ARG:1 == 1
    RETURN 1
RETURN 0

@GET_EXP_BRANCH(ARG, ARG:1)
;ARG = TARGET番号, ARG:1 = EXP番号
;RESULT = 0:未経験 / 1:経験少 / 2:経験豊富
SIF EXP:ARG:ARG:1 >= 100
    RETURN 2
SIF EXP:ARG:ARG:1 >= 10
    RETURN 1
RETURN 0
```

### 境界値

| 関数 | 入力 | 期待出力 |
|------|------|----------|
| GET_ABL_BRANCH | ABL=-1 | 0 (低) |
| GET_ABL_BRANCH | ABL=0 | 0 (低) |
| GET_ABL_BRANCH | ABL=1 | 1 (中) |
| GET_ABL_BRANCH | ABL=2 | 1 (中) |
| GET_ABL_BRANCH | ABL=3 | 2 (高) |
| GET_TALENT_BRANCH | TALENT=-1 | 0 (通常) |
| GET_TALENT_BRANCH | TALENT=0 | 0 (通常) |
| GET_TALENT_BRANCH | TALENT=1 | 1 (敏感) |
| GET_EXP_BRANCH | EXP=-1 | 0 (未経験) |
| GET_EXP_BRANCH | EXP=0 | 0 (未経験) |
| GET_EXP_BRANCH | EXP=9 | 0 (未経験) |
| GET_EXP_BRANCH | EXP=10 | 1 (経験少) |
| GET_EXP_BRANCH | EXP=99 | 1 (経験少) |
| GET_EXP_BRANCH | EXP=100 | 2 (経験豊富) |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | GET_ABL_BRANCH 関数存在 | code | grep | contains | "@GET_ABL_BRANCH" | [x] |
| 2 | GET_TALENT_BRANCH 関数存在 | code | grep | contains | "@GET_TALENT_BRANCH" | [x] |
| 3 | GET_EXP_BRANCH 関数存在 | code | grep | contains | "@GET_EXP_BRANCH" | [x] |
| 4 | ErbLinter 構文エラーなし | build | lint | succeeds | - | [x] |
| 5 | ABL境界値: 0→0, 1→1, 2→1, 3→2 | output | --unit | equals | "0\n1\n1\n2" | [x] |
| 6 | TALENT境界値: 0→0, 1→1 | output | --unit | equals | "0\n1" | [x] |
| 7 | EXP境界値: 0→0, 9→0, 10→1, 99→1, 100→2 | output | --unit | equals | "0\n0\n1\n1\n2" | [x] |
| 8 | 負値入力時のデフォルト動作 (ABL→TALENT→EXP順) | output | --unit | equals | "0\n0\n0" | [x] |

### AC Details

**AC1 Test**: `grep "@GET_ABL_BRANCH" Game/ERB/COMMON_KOJO.ERB`
**Expected**: Function definition found

**AC2 Test**: `grep "@GET_TALENT_BRANCH" Game/ERB/COMMON_KOJO.ERB`
**Expected**: Function definition found

**AC3 Test**: `grep "@GET_EXP_BRANCH" Game/ERB/COMMON_KOJO.ERB`
**Expected**: Function definition found

**AC4 Test**: `dotnet run --project tools\ErbLinter\ErbLinter.csproj -- Game\ERB\COMMON_KOJO.ERB`
**Expected**: Exit code 0

**AC5 Test**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit "tests/ac/erb/feature-216/ac5-abl-boundary.json"`
**Expected**: GET_ABL_BRANCH returns 0 for ABL=0, 1 for ABL=1, 1 for ABL=2, 2 for ABL=3 (output: "0\n1\n1\n2")

**AC6 Test**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit "tests/ac/erb/feature-216/ac6-talent-boundary.json"`
**Expected**: GET_TALENT_BRANCH returns 0 for TALENT=0, 1 for TALENT=1 (output: "0\n1")

**AC7 Test**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit "tests/ac/erb/feature-216/ac7-exp-boundary.json"`
**Expected**: GET_EXP_BRANCH returns 0 for EXP=0, 0 for EXP=9, 1 for EXP=10, 1 for EXP=99, 2 for EXP=100 (output: "0\n0\n1\n1\n2")

**AC8 Test**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit "tests/ac/erb/feature-216/ac8-negative-value.json"`
**Expected**: All functions return 0 when input value is negative. Order: ABL(-1)→0, TALENT(-1)→0, EXP(-1)→0 (output: "0\n0\n0")

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | 1 | GET_ABL_BRANCH 関数を実装 | Game/ERB/COMMON_KOJO.ERB | [x] |
| 2 | 2 | GET_TALENT_BRANCH 関数を実装 | Game/ERB/COMMON_KOJO.ERB | [x] |
| 3 | 3 | GET_EXP_BRANCH 関数を実装 | Game/ERB/COMMON_KOJO.ERB | [x] |
| 4 | 4 | ErbLinter で構文検証 | Game/ERB/COMMON_KOJO.ERB | [x] |
| 5 | 5 | ABL 境界値テスト作成・実行 | tests/ac/erb/feature-216/ | [x] |
| 6 | 6 | TALENT 境界値テスト作成・実行 | tests/ac/erb/feature-216/ | [x] |
| 7 | 7 | EXP 境界値テスト作成・実行 | tests/ac/erb/feature-216/ | [x] |
| 8 | 8 | 負値入力テスト作成・実行 | tests/ac/erb/feature-216/ | [x] |

---

## Review Notes

<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2025-12-25**: F215 から分離。erb タイプとして Pos+Neg（境界値テスト）を含む。旧F217からリナンバー。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-26 | Initialization | initializer | Status PROPOSED→WIP | Ready for Phase 2 |
| 2025-12-26 | Investigation | explorer | Codebase analysis | READY |
| 2025-12-26 | Test Creation | implementer | Created 4 AC test files | RED state confirmed |
| 2025-12-26 | Implementation | implementer | Added functions to COMMON_KOJO.ERB | SUCCESS |
| 2025-12-26 | Debug Fix | debugger | Moved from templates/ to ERB/, fixed syntax | SUCCESS |
| 2025-12-26 | AC Verification | ac-tester | All 8 ACs verified | 14/14 PASS |
| 2025-12-26 | Regression | regression-tester | Full suite | 24/24 PASS |
| 2025-12-26 | Completion | feature-reviewer | Post-review, status WIP→DONE | READY |

---

## Links

- [feature-215.md](feature-215.md) - 分岐候補マトリクス調査（前提）
- [m-orgasm-system.md](designs/m-orgasm-system.md) - v1.8 M絶頂システム
- [v0.8-kojo-80-90.md](designs/v0.8-kojo-80-90.md) - 80-90番台口上計画

---

## Notes

- 本Featureは「分岐判定関数の実装」がスコープ
- F215 の調査結果に基づいて実装
- 口上テキスト作成は各COM Feature で実施（roadmap で管理）
