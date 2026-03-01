# Feature 338: 口上セマンティクス不整合修正

## Status: [DONE]

**Previously Blocked by**: [Feature 339](feature-339.md) - com-auditor JSON 出力修正 (DONE)

## Type: kojo

## Background

### Philosophy

口上は COMF*.ERB のゲームメカニクスと整合した内容であるべき。

### Problem

F336 監査 (com-auditor opus) で **40件** の口上がセマンティクス反転エラーを持つことが判明。

### Goal

不整合口上を kojo-writer 再dispatch で修正する。

### Context

- 発見元: [feature-336.md](feature-336.md) (全口上セマンティクス監査)
- 監査レポート: [kojo-semantics-audit.md](reference/kojo-semantics-audit.md)
- JSON出力: `Game/logs/audit/com-*.json`

---

## Fix Summary

| COM | Command | INCONSISTENT | PASS例 |
|:---:|---------|:------------:|--------|
| 11 | 乳首吸わせ | 9 | K7 |
| 90 | アナル愛撫される | 6 | K2,K5,K6,K10 |
| 92 | Ａ正常位される | 7 | K3,K6,K7 |
| 93 | Ａ後背位される | 7 | K3,K6,K7 |
| 94 | Ａ騎乗位する | 1 | K1-K5,K7-K10 |
| 96 | クンニされる | 1 | K1-K2,K4-K10 |
| 97 | 正常位させる | 9 | K1 |
| **Total** | - | **40** | - |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | COM 11 INCONSISTENT=0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 2 | COM 90 INCONSISTENT=0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 3 | COM 92 INCONSISTENT=0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 4 | COM 93 INCONSISTENT=0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 5 | COM 94 INCONSISTENT=0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 6 | COM 96 INCONSISTENT=0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 7 | COM 97 INCONSISTENT=0 | file | Grep | contains | "inconsistent": 0 | [x] |
| 8 | Build succeeds | build | dotnet build | succeeds | - | [x] |
| 9 | Regression tests pass | output | --flow | contains | "passed" | [x] |

### AC Details

**Verification Flow**:
1. Task 1-7: kojo-writer により各 COM の不整合 kojo を修正
2. Task 8: com-auditor 再 dispatch で Game/logs/audit/com-{N}.json を更新
3. AC1-7: 各 JSON ファイルで `"inconsistent": 0` を Grep 確認
4. AC8-9: ビルド・回帰テスト

**Target Files**:
- AC1: `Grep("\"inconsistent\": 0", "Game/logs/audit/com-11.json")`
- AC2: `Grep("\"inconsistent\": 0", "Game/logs/audit/com-90.json")`
- AC3: `Grep("\"inconsistent\": 0", "Game/logs/audit/com-92.json")`
- AC4: `Grep("\"inconsistent\": 0", "Game/logs/audit/com-93.json")`
- AC5: `Grep("\"inconsistent\": 0", "Game/logs/audit/com-94.json")`
- AC6: `Grep("\"inconsistent\": 0", "Game/logs/audit/com-96.json")`
- AC7: `Grep("\"inconsistent\": 0", "Game/logs/audit/com-97.json")`

<!-- AC:Task 1:1 - 7 COMs verification + 2 build/test = 9 AC -->

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | COM 11 kojo修正 + com-auditor 再監査 | [x] |
| 2 | 2 | COM 90 kojo修正 + com-auditor 再監査 | [x] |
| 3 | 3 | COM 92 kojo修正 + com-auditor 再監査 | [x] |
| 4 | 4 | COM 93 kojo修正 + com-auditor 再監査 | [x] |
| 5 | 5 | COM 94 kojo修正 + com-auditor 再監査 | [x] |
| 6 | 6 | COM 96 kojo修正 + com-auditor 再監査 | [x] |
| 7 | 7 | COM 97 kojo修正 + com-auditor 再監査 | [x] |
| 8 | 8 | ビルド確認 | [x] |
| 9 | 9 | 回帰テスト | [x] |

<!-- Task 1-7: kojo-writer dispatch後、com-auditor再監査でJSON更新 -->

### Detailed Fix Items (40 total)

**COM 11 (乳首吸わせ)** - TARGET sucks PLAYER's nipple:
K1(美鈴), K2(小悪魔), K3(パチュリー), K4(咲夜), K5(レミリア), K6(フランドール), K8(チルノ), K9(大妖精), K10(魔理沙)

**COM 90 (アナル愛撫される)** - TARGET caresses PLAYER's anus:
K1(美鈴), K3(パチュリー), K4(咲夜), K7(妖精メイド), K8(チルノ), K9(大妖精)

**COM 92 (Ａ正常位される)** - TARGET inserts into PLAYER's anus:
K1(美鈴), K2(小悪魔), K4(咲夜), K5(レミリア), K8(チルノ), K9(大妖精), K10(魔理沙)

**COM 93 (Ａ後背位される)** - TARGET inserts into PLAYER's anus (doggy):
K1(美鈴), K2(小悪魔), K4(咲夜), K5(レミリア), K8(チルノ), K9(大妖精), K10(魔理沙)

**COM 94 (Ａ騎乗位する)** - TARGET receives PLAYER's insertion (cowgirl):
K6(フランドール)

**COM 96 (クンニされる)** - TARGET performs cunnilingus on PLAYER:
K3(パチュリー)

**COM 97 (正常位させる)** - TARGET inserts into PLAYER:
K2(小悪魔), K3(パチュリー), K4(咲夜), K5(レミリア), K6(フランドール), K7(妖精メイド), K8(チルノ), K9(大妖精), K10(魔理沙)

---

## Semantics Reference

| COM | Correct Semantics | Incorrect (現状) |
|:---:|-------------------|------------------|
| 11 | TARGET が PLAYER の乳首を吸う | PLAYER が TARGET の乳首を吸う |
| 90 | TARGET が PLAYER のアナルを愛撫 | PLAYER が TARGET のアナルを愛撫 |
| 92 | TARGET が PLAYER のアナルに挿入 | PLAYER が TARGET のアナルに挿入 |
| 93 | TARGET が PLAYER のアナルに挿入 | PLAYER が TARGET のアナルに挿入 |
| 94 | TARGET が PLAYER を受け入れる（騎乗） | TARGET が PLAYER に挿入 |
| 96 | TARGET が PLAYER にクンニ | PLAYER が TARGET にクンニ |
| 97 | TARGET が PLAYER に挿入 | TARGET が PLAYER を受け入れる |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 | DEVIATION | eratw-reader | COM 96 extract | ERR:section_not_found |
| 2026-01-04 | DEVIATION | eratw-reader | COM 97 extract | ERR:section_not_found |
| 2026-01-04 | - | eratw-reader | COM 11,90,92,93,94 extract | OK:cached |
| 2026-01-04 | - | kojo-writer | COM 11 K1-K6,K8-K10 dispatch | in_progress |
| 2026-01-04 | - | kojo-writer | All 40 kojo dispatches | completed |
| 2026-01-04 | DEVIATION | com-auditor | AC verification | FAIL: inconsistent>0 for all COMs |
| 2026-01-04 | DEVIATION | Manual check | K1 COM 11 | PASS: correct semantics in ERB file |
| 2026-01-04 | DEVIATION | com-auditor | JSON output | Unreliable: JSON does not match actual ERB content |
| 2026-01-04 | BLOCKED | - | F339 created | Waiting for com-auditor fix |
| 2026-01-04 | - | com-auditor | F339 complete, re-audit | All 7 COMs: inconsistent=0 |
| 2026-01-04 | - | - | Build + Regression | PASS: 0 errors, 24/24 tests |
| 2026-01-04 | - | feature-reviewer | Post-review | Task/AC status updated |

---

## Links

- [index-features.md](index-features.md)
- [feature-336.md](feature-336.md) - 発見元Feature
- [kojo-semantics-audit.md](reference/kojo-semantics-audit.md) - 監査レポート
