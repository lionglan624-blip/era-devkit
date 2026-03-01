# Feature 271: COM 60-72 誤配置修正（F261補完）

## Status: [DONE]

## Type: erb

## Background

### Philosophy (Mid-term Vision)

F261 で検出された COM 60-72 の誤配置のうち、F262 で修正漏れとなった関数を移動する。
口上ファイル配置ルールの一貫性を確保し、今後の開発での混乱を防ぐ。

### Problem (Current Issue)

F261 Phase ② で以下の誤配置が検出されたが、F262 では一部のみ修正された：

**F261 検出内容** (lines 833-838):
| キャラ | 誤配置COM | 対処 |
|--------|:---------:|------|
| K2 | 65-71 | 移動 |
| K4 | 65-71 | 移動 |
| K9 | 65-71 | 移動 |
| KU | 61-71 | 移動 |

**注記**: 実際の調査では K4 COM 65-66 は既に `_挿入.ERB` に配置済み。F261 検出時点と現状に差異あり。

**F262 実施内容**:
- K2 COM 65-67 のみ移動
- K4, K9, KU は未対応

**結果**: F261 → F262 の Phase ③ 抽出で大部分が漏れた。

**注記**: Current State セクションは F262 実施後の状態を反映。K2 は残り 68-71 のみ誤配置。

### Goal (What to Achieve)

COM 60-72（挿入系）を全て `_挿入.ERB` に配置し、`_口挿入.ERB` から除去する。

---

## Current State (2025-12-31 調査)

### 誤配置関数一覧

| キャラ | 誤配置COM | 関数名 | 関数数 |
|--------|-----------|--------|:------:|
| K2 小悪魔 | 68, 69, 70, 71 | @KOJO_MESSAGE_COM_K2_{68,69,70,71}[_1] | 8 |
| K4 咲夜 | 67, 68, 69, 70, 71 | @KOJO_MESSAGE_COM_K4_{67,68,69,70,71}[_1] | 10 |
| K9 大妖精 | 65, 67, 68, 69, 70, 71 | @KOJO_MESSAGE_COM_K9_{65,67,68,69,70,71}[_1] | 12 |
| KU 汎用 | 61-71 | @KOJO_MESSAGE_COM_KU_{61-71}[_1] | 22 |
| **合計** | | | **52** |

### 正しく配置済みのキャラ

| キャラ | _挿入.ERB の COM | 状態 |
|--------|------------------|:----:|
| K1 美鈴 | 60-66 | ✅ |
| K2 小悪魔 | 60-67 | ⚠️ 68-71 が口挿入に誤配置 |
| K3 パチュリー | 60-66 | ✅ |
| K4 咲夜 | 60-66 | ⚠️ 67-71 が口挿入に誤配置 |
| K5 レミリア | 60-66 | ✅ |
| K6 フラン | 60-66 | ✅ |
| K7 子悪魔 | 60-66 | ✅ |
| K8 チルノ | 60-66 | ✅ |
| K9 大妖精 | 60-64, 66 | ⚠️ 65, 67-71 が口挿入に誤配置 |
| K10 魔理沙 | 60-66 | ✅ |
| KU 汎用 | 60 | ⚠️ 61-71 が口挿入に誤配置 |

---

## Acceptance Criteria

### K2 小悪魔 (COM 68-71)
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K2 COM 68 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/2_小悪魔/KOJO_K2_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K2_68 | [x] |
| 2 | K2 COM 68 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/2_小悪魔/KOJO_K2_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K2_68 | [x] |
| 3 | K2 COM 69 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/2_小悪魔/KOJO_K2_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K2_69 | [x] |
| 4 | K2 COM 69 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/2_小悪魔/KOJO_K2_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K2_69 | [x] |
| 5 | K2 COM 70 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/2_小悪魔/KOJO_K2_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K2_70 | [x] |
| 6 | K2 COM 70 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/2_小悪魔/KOJO_K2_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K2_70 | [x] |
| 7 | K2 COM 71 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/2_小悪魔/KOJO_K2_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K2_71 | [x] |
| 8 | K2 COM 71 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/2_小悪魔/KOJO_K2_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K2_71 | [x] |

### K4 咲夜 (COM 67-71)
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 9 | K4 COM 67 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/4_咲夜/KOJO_K4_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K4_67 | [x] |
| 10 | K4 COM 67 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/4_咲夜/KOJO_K4_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K4_67 | [x] |
| 11 | K4 COM 68 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/4_咲夜/KOJO_K4_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K4_68 | [x] |
| 12 | K4 COM 68 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/4_咲夜/KOJO_K4_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K4_68 | [x] |
| 13 | K4 COM 69 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/4_咲夜/KOJO_K4_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K4_69 | [x] |
| 14 | K4 COM 69 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/4_咲夜/KOJO_K4_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K4_69 | [x] |
| 15 | K4 COM 70 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/4_咲夜/KOJO_K4_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K4_70 | [x] |
| 16 | K4 COM 70 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/4_咲夜/KOJO_K4_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K4_70 | [x] |
| 17 | K4 COM 71 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/4_咲夜/KOJO_K4_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K4_71 | [x] |
| 18 | K4 COM 71 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/4_咲夜/KOJO_K4_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K4_71 | [x] |

### K9 大妖精 (COM 65, 67-71)
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 19 | K9 COM 65 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K9_65 | [x] |
| 20 | K9 COM 65 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K9_65 | [x] |
| 21 | K9 COM 67 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K9_67 | [x] |
| 22 | K9 COM 67 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K9_67 | [x] |
| 23 | K9 COM 68 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K9_68 | [x] |
| 24 | K9 COM 68 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K9_68 | [x] |
| 25 | K9 COM 69 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K9_69 | [x] |
| 26 | K9 COM 69 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K9_69 | [x] |
| 27 | K9 COM 70 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K9_70 | [x] |
| 28 | K9 COM 70 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K9_70 | [x] |
| 29 | K9 COM 71 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_挿入.ERB) | contains | @KOJO_MESSAGE_COM_K9_71 | [x] |
| 30 | K9 COM 71 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/9_大妖精/KOJO_K9_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_K9_71 | [x] |

### KU 汎用 (COM 61-71)
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 31 | KU COM 61 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB) | contains | @KOJO_MESSAGE_COM_KU_61 | [x] |
| 32 | KU COM 61 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_KU_61 | [x] |
| 33 | KU COM 62 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB) | contains | @KOJO_MESSAGE_COM_KU_62 | [x] |
| 34 | KU COM 62 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_KU_62 | [x] |
| 35 | KU COM 63 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB) | contains | @KOJO_MESSAGE_COM_KU_63 | [x] |
| 36 | KU COM 63 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_KU_63 | [x] |
| 37 | KU COM 64 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB) | contains | @KOJO_MESSAGE_COM_KU_64 | [x] |
| 38 | KU COM 64 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_KU_64 | [x] |
| 39 | KU COM 65 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB) | contains | @KOJO_MESSAGE_COM_KU_65 | [x] |
| 40 | KU COM 65 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_KU_65 | [x] |
| 41 | KU COM 66 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB) | contains | @KOJO_MESSAGE_COM_KU_66 | [x] |
| 42 | KU COM 66 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_KU_66 | [x] |
| 43 | KU COM 67 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB) | contains | @KOJO_MESSAGE_COM_KU_67 | [x] |
| 44 | KU COM 67 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_KU_67 | [x] |
| 45 | KU COM 68 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB) | contains | @KOJO_MESSAGE_COM_KU_68 | [x] |
| 46 | KU COM 68 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_KU_68 | [x] |
| 47 | KU COM 69 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB) | contains | @KOJO_MESSAGE_COM_KU_69 | [x] |
| 48 | KU COM 69 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_KU_69 | [x] |
| 49 | KU COM 70 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB) | contains | @KOJO_MESSAGE_COM_KU_70 | [x] |
| 50 | KU COM 70 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_KU_70 | [x] |
| 51 | KU COM 71 が挿入.ERB に存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB) | contains | @KOJO_MESSAGE_COM_KU_71 | [x] |
| 52 | KU COM 71 が口挿入.ERB に非存在 | code | Grep(Game/ERB/口上/U_汎用/KOJO_KU_口挿入.ERB) | not_contains | @KOJO_MESSAGE_COM_KU_71 | [x] |

### Build & Test
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 53 | ビルド成功 | build | dotnet build | succeeds | - | [x] |
| 54 | 回帰テスト PASS | test | --flow tests/regression/ | succeeds | 24/24 scenarios | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-8 | K2 COM 68-71 (8関数) を口挿入.ERB → 挿入.ERB に移動 | [x] |
| 2 | 9-18 | K4 COM 67-71 (10関数) を口挿入.ERB → 挿入.ERB に移動 | [x] |
| 3 | 19-30 | K9 COM 65,67-71 (12関数) を口挿入.ERB → 挿入.ERB に移動 | [x] |
| 4 | 31-52 | KU COM 61-71 (22関数) を口挿入.ERB → 挿入.ERB に移動 | [x] |
| 5 | 53 | ビルド確認 | [x] |
| 6 | 54 | 回帰テスト実行 | [x] |

---

## Implementation Notes

### 移動方法

1. `_口挿入.ERB` から該当関数をカット
2. `_挿入.ERB` の末尾（または適切な COM 順序位置）にペースト
3. 重複がないことを確認

### 配置ルール (SKILL.md より)

| COM Range | Category | File |
|-----------|----------|------|
| 60-72 | 挿入系 (膣/アナル) | `KOJO_K{N}_挿入.ERB` |
| 80-203 | 非挿入系 | `KOJO_K{N}_口挿入.ERB` |

---

## Links

- [feature-261.md](features/feature-261.md) - 全ERB完全調査（本 Feature の根拠）
- [feature-262.md](features/feature-262.md) - 口上品質修正（部分修正済み）
- [kojo-writing SKILL](.claude/skills/kojo-writing/SKILL.md) - 配置ルール SSOT

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-31 | create | opus | F261補完として作成 | - |
| 2025-12-31 | implement | implementer | K2,K4,K9,KU COM移動 (52関数) | SUCCESS |
| 2025-12-31 | verify | opus | AC 54/54 PASS, Regression 24/24 PASS | SUCCESS |
| 2025-12-31 | finalize | opus | Status [DONE] | SUCCESS |
