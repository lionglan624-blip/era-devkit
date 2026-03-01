# Feature 103: COM_311 抱き付く 口上 (Phase 8d)

## Status: [DONE]

**Completed**: 2025-12-18

## Type: kojo

## Background

- **Original problem**: Phase 8d で300番台COMを順次8d品質で作成
- **Considered alternatives**:
  - COM 302 8d改修 - 8c完了済みで後回し可能
  - COM 311 抱き付く - 未着手COMを優先
- **Key decisions**: Phase 8d進捗表の順序に従いCOM 311を選択
- **Constraints**: K1-K10全キャラ、TALENT 4段階分岐、各4-8行

## Overview

COM 311「抱き付く」の口上を全キャラ(K1-K10)で作成。Phase 8d品質基準に従い、感情描写・情景描写を含む4-8行の口上を実装する。

## Goals

1. COM_311 抱き付く口上を全キャラ(K1-K10)で実装
2. TALENT 4段階分岐（恋人/恋慕/思慕/なし）対応
3. 8d品質基準（感情描写・情景描写）達成
4. 全テスト合格

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | MockRand | Status |
|:---:|-------------|------|---------|----------|:--------:|:------:|
| 1 | K1美鈴 COM_311口上出力 | output | contains | "温かいわね" | - | [x] |
| 2 | K2小悪魔 COM_311口上出力 | output | contains | "温かいですわ" | [0] | [x] |
| 3 | K3パチュリー COM_311口上出力 | output | contains | "温かいわね" | - | [x] |
| 4 | K4咲夜 COM_311口上出力 | output | contains | "温かいわ" | - | [x] |
| 5 | K5レミリア COM_311口上出力 | output | contains | "温かいわね" | - | [x] |
| 6 | K6フラン COM_311口上出力 | output | contains | "あったかい" | - | [x] |
| 7 | K7子悪魔 COM_311口上出力 | output | contains | "あったかい" | - | [x] |
| 8 | K8チルノ COM_311口上出力 | output | contains | "幸せすぎて溶けちゃいそう" | - | [x] |
| 9 | K9大妖精 COM_311口上出力 | output | contains | "あったかい" | [0] | [x] |
| 10 | K10魔理沙 COM_311口上出力 | output | contains | "本当に幸せだよ" | - | [x] |
| 11 | ビルド成功 | build | succeeds | - | - | [x] |
| 12 | 回帰テスト成功 | output | contains | "passed (100%)" | - | [x] |

**Note**: AC2, AC9 use `mock_rand` for deterministic DATALIST testing. See `tests/scenario-103-ac*.json`.

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | K1美鈴 COM_311口上を8d品質に更新 | [x] |
| 2 | 2 | K2小悪魔 COM_311口上を8d品質に更新 | [x] |
| 3 | 3 | K3パチュリー COM_311口上作成（新規実装） | [x] |
| 4 | 4 | K4咲夜 COM_311口上を8d品質に更新 | [x] |
| 5 | 5 | K5レミリア COM_311口上作成（新規実装） | [x] |
| 6 | 6 | K6フラン COM_311口上作成（新規実装） | [x] |
| 7 | 7 | K7子悪魔 COM_311口上作成（新規実装） | [x] |
| 8 | 8 | K8チルノ COM_311口上を8d品質に更新 | [x] |
| 9 | 9 | K9大妖精 COM_311口上を8d品質に更新 | [x] |
| 10 | 10 | K10魔理沙 COM_311口上を8d品質に更新 | [x] |
| 11 | 11 | ビルド確認 | [x] |
| 12 | 12 | 回帰テスト実行 | [x] |

## Execution State

**Initialized**: 2025-12-18
**Agent**: initializer
**Ready for**: /imple

## Execution Log

### Task 10: K10魔理沙 COM_311口上を8d品質に更新

**Implementation**:
- **Agent**: kojo-writer
- **Date**: 2025-12-18
- **Status**: SUCCESS
- **File**: Game/ERB/口上/10_魔理沙/KOJO_K10_会話親密.ERB
- **Action**: Rewrote KOJO_MESSAGE_COM_K10_311_1
- **AC10 requirement**: "本当に幸せだよ" appears in ALL success variations (恋人/恋慕/思慕 branches)

**Key Lines**:
  - 恋人: 「こうやってると、本当に幸せだよ…」
  - 恋人: 「…へへ、あったかいな。本当に幸せだよ」
  - 恋慕: 「…こうしてると、本当に幸せだよ…なんてな」
  - 恋慕: 「…あったかいな。こうしてると本当に幸せだよ」
  - 思慕: 「…あったかいな…こうしてると、本当に幸せだよ…かも」
  - 思慕: 「…あったかいな。こうしてると本当に幸せだよ…なんてな」

### AC10 Verification (ac-tester)

**Agent**: ac-tester
**Date**: 2025-12-18
**AC Number**: 10
**Description**: K10魔理沙 COM_311口上出力
**Type**: output
**Matcher**: contains
**Expected**: "本当に幸せだよ"

**Test Execution**:
- **Command**: --unit "KOJO_MESSAGE_COM_K10_311_1" --char 10 --set "TALENT:TARGET:16=1"
- **Duration**: 1.41s
- **Status**: OK

**Output**:
```
「――ぎゅってして、あなた」
魔理沙が珍しく素直にお願いしてきた。
あなたは魔理沙を優しく抱きしめた。
「…へへ、あったかいな。本当に幸せだよ」
魔理沙はあなたの心臓の音を聞くように、胸に耳を当てている。
「死ぬまで一緒にいてくれよな。…約束だぜ」
金髪の魔法使いは、照れ隠しのように帽子を深く被った。
```

**Verification**:
- **Matcher**: contains("本当に幸せだよ")
- **Result**: TRUE
- **Match Found**: Line 4: "「…へへ、あったかいな。本当に幸せだよ」"
- **AC10 Status**: PASS
