# Feature 106: COM_312 キスする 口上 (Phase 8d)

## Status: [DONE]
## Type: kojo

## Background
- **COM**: COM_312「キスする」
- **Quality**: Phase 8d (TALENT 4段階、4-8行、感情・情景描写)
- **Scope**: K1-K10 全キャラ

## Acceptance Criteria

| AC# | Char | Type | Matcher | Expected | MockRand | Status |
|:---:|------|------|---------|----------|:--------:|:------:|
| 1 | K1美鈴 | output | contains | "もっと、して" | [0] | [x] |
| 2 | K2小悪魔 | output | contains | "悪魔のキスは甘いでしょう？" | [0] | [x] |
| 3 | K3パチュリー | output | contains | "キスの魔法というものがあるのを知っているかしら" | [0] | [x] |
| 4 | K4咲夜 | output | contains | "瀟洒なメイドらしからぬ顔をしているわね" | [0] | [x] |
| 5 | K5レミリア | output | contains | "永遠を感じなさい" | [0] | [x] |
| 6 | K6フラン | output | contains | "壊したくならないのさ" | [0] | [x] |
| 7 | K7子悪魔 | output | contains | "とけちゃいそう" | [0] | [x] |
| 8 | K8チルノ | output | contains | "あたいのキス、最強でしょ？" | [0] | [x] |
| 9 | K9大妖精 | output | contains | "大好きだよ" | [0] | [x] |
| 10 | K10魔理沙 | output | contains | "最高だぜ" | [0] | [x] |
| 11 | Build | build | succeeds | - | - | [x] |
| 12 | Regression | output | contains | "passed (100%)" | - | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | K1美鈴 COM_312 口上作成 | [x] |
| 2 | 2 | K2小悪魔 COM_312 口上作成 | [x] |
| 3 | 3 | K3パチュリー COM_312 口上作成 | [x] |
| 4 | 4 | K4咲夜 COM_312 口上作成 | [x] |
| 5 | 5 | K5レミリア COM_312 口上作成 | [x] |
| 6 | 6 | K6フラン COM_312 口上作成 | [x] |
| 7 | 7 | K7子悪魔 COM_312 口上作成 | [x] |
| 8 | 8 | K8チルノ COM_312 口上作成 | [x] |
| 9 | 9 | K9大妖精 COM_312 口上作成 | [x] |
| 10 | 10 | K10魔理沙 COM_312 口上作成 | [x] |
| 11 | 11 | ビルド確認 | [x] |
| 12 | 12 | 回帰テスト | [x] |

## Execution State

| Key | Value |
|-----|-------|
| Phase | 5 (Complete) |
| Current Task | DONE |
| Last Agent | ac-tester x10 (parallel) |

## Execution Log

### Phase 1: Setup - STARTED

**Timestamp**: 2025-12-18
**Actions**:
- Created feature-106.md
- Selected COM_312 (キスする) as next 8d target
- eraTW霊夢 COM_312 reference pre-read complete

### eraTW霊夢 COM_312 参照 (8d品質基準)

```erb
;312,キスする
;==================================================
@M_KOJO_MESSAGE_COM_K1_312
CALL M_KOJO_MESSAGE_COM_K1_312_1
RETURN RESULT

@M_KOJO_MESSAGE_COM_K1_312_1
;-------------------------------------------------
;記入チェック（=0, 非表示、1, 表示）
LOCAL = 1
;-------------------------------------------------
IF LOCAL
	IF FLAG:時間停止
		PRINTFORML
		PRINTFORMW %CALLNAME:MASTER%は霊夢の唇に口付けた。
		PRINTFORMW 時が止まっていても、感触は変わらない。
		PRINTFORMW %CALLNAME:MASTER%は好きに舌も使い霊夢の唇を貪るように楽しんだ。
	ELSEIF TALENT:恋人
		IF CFLAG:TARGET:睡眠
			PRINTFORML
			PRINTFORMW 「すぅ……すぅ……」
			PRINTFORMW %CALLNAME:MASTER%は眠っている霊夢と口付けを交わした。
			PRINTFORMW 「ん……んぅ……」
			PRINTFORMW 霊夢の顔が若干紅くなった。
		ELSE
			PRINTDATA
				DATALIST
					DATAFORM
					DATAFORM 「ん……ちゅる……ぢゅる……」
					DATAFORM 「れろ……ん……ぷはっ」
					DATAFORM 霊夢は唇を%CALLNAME:MASTER%の唇から離した。
					DATAFORM 霊夢は蕩けた顔になっている。
					DATAFORM 「あー……これ、とろけちゃいそう……」
					DATAFORM 霊夢は呆けながら、焦点の微妙に定まっていない目で遠くを見つめている。
				ENDLIST
				DATALIST
					DATAFORM
					DATAFORM 「ん、む……ちゅ、れろ……」
					DATAFORM %CALLNAME:MASTER%と霊夢は熱く口付け合っている。
					DATAFORM しばらく互いの唾液を味わってから、二人は唇を離した。
					DATAFORM 「はぁっ、はぁっ、はぁぁっ……ん、もっと、しましょ……？」
					DATAFORM 霊夢は続きを催促してきた。
				ENDLIST
			ENDDATA
			PRINTFORMW
			RETURN 1
		ENDIF

	ELSEIF TALENT:恋慕
		IF CFLAG:TARGET:睡眠
			PRINTFORML
			PRINTFORMW 「すぅ……すぅ……」
			PRINTFORMW %CALLNAME:MASTER%は眠っている霊夢と口付けを交わした。
			PRINTFORMW 「ん……んぅ……」
			PRINTFORMW 霊夢の顔が、若干紅くなった。
		ELSE
			PRINTDATA
				DATALIST
					DATAFORM
					DATAFORM 「ん……んちゅ……れろ」
					DATAFORM 「ちゅぷ……んむ、はぁぁ……」
					DATAFORM 霊夢は長い息を吐いた。
					DATAFORM 「あー……これ、あつい……」
					DATAFORM 霊夢は顔を上気させてぼーっとしている。
				ENDLIST
				DATALIST
					DATAFORM
					DATAFORM 「ちゅ、んむ、ちゅる……んっ」
					DATAFORM 「ちゅぅ、う、むぅっ……ぷあ……」
					DATAFORM 「……あつい……溶けちゃいそう、%CALLNAME:MASTER%……」
					DATAFORM 霊夢は熱に浮かされたようにそう言ってきた。
				ENDLIST
				DATALIST
					DATAFORM
					DATAFORM 「ちゅ、ん、ちゅ……む、んむ……」
					DATAFORM 霊夢と%CALLNAME:MASTER%は熱く深く口付け合っている。
					DATAFORM 「は、んむ、ちゅる、ちゅ、ぷ、はぁっ……」
					DATAFORM 「はぁっ、はぁっ、%CALLNAME:MASTER%、もっと、もっとぉ」
					DATAFORM 霊夢は完全に熱に浮かされた表情でせがんできた。
				ENDLIST
			ENDDATA
			PRINTFORMW
		ENDIF
	ELSEIF TALENT:思慕
		IF CFLAG:TARGET:睡眠
			PRINTFORML
			PRINTFORMW 「すぅ……すぅ……」
			PRINTFORMW %CALLNAME:MASTER%は眠っている霊夢と口付けを交わした。
			PRINTFORMW 「ん……んぅ……」
			PRINTFORMW 霊夢の顔が若干紅くなった。
		ELSE
			PRINTDATA
				DATALIST
					DATAFORM
					DATAFORM 「ぴちゅ……れろ……ん……」
					DATAFORM 「ちゅぷ……れろ……ぷはぁ」
					DATAFORM 霊夢は顔を少し紅くして%CALLNAME:MASTER%の顔を見つめている。
				ENDLIST
				DATALIST
					DATAFORM
					DATAFORM 「ん、む……ちゅ、る……」
					DATAFORM 霊夢と%CALLNAME:MASTER%は控えめに口付けを交わしている。
					DATAFORM 「んっ……ぷ、は……」
					DATAFORM 少し舌を挿し込むと、霊夢は唇を離してしまった。
				ENDLIST
			ENDDATA
			PRINTFORMW
		ENDIF
	ELSE
		IF CFLAG:TARGET:睡眠
			PRINTFORML
			PRINTFORMW 「すぅ……すぅ……」
			PRINTFORMW %CALLNAME:MASTER%は眠っている霊夢と口付けを交わした。
			PRINTFORMW 「ん……んぅ……」
			PRINTFORMW 霊夢は少しうなった。
		ELSE
			PRINTDATA
				DATALIST
					DATAFORM
					DATAFORM 「んっ！？　……ん、むっ、は、やめ、やめなさい！」
					DATAFORM %CALLNAME:MASTER%が舌を挿し入れると、霊夢は唇を離してしまった。
				ENDLIST
				DATALIST
					DATAFORM
					DATAFORM 「んっ、む……ふ、む……」
					DATAFORM 霊夢は%CALLNAME:MASTER%と控えめな口付けを交わしている。
					DATAFORM その唇は固く閉ざされ、舌が入る余地は無かった……
				ENDLIST
			ENDDATA
			PRINTFORMW
		ENDIF
	ENDIF
ENDIF
RETURN 1
```

**品質ポイント**:
- TALENT 4段階分岐（恋人/恋慕/思慕/なし）
- 各分岐に PRINTDATA + 複数 DATALIST
- 情景描写あり（「霊夢は蕩けた顔になっている」等）
- 4-8行の文量
- **注意**: CFLAG:TARGET:睡眠 は当プロジェクトでは使用しない

### Phase 2: Implementation - COMPLETED

**Timestamp**: 2025-12-18
**Method**: 10 kojo-writers dispatched in parallel
**Result**: All 10 characters verified - COM_312 already at 8d quality

| Char | Key Phrase | Status |
|------|-----------|--------|
| K1美鈴 | "門番の仕事中だけど……もう一回だけ、いいかな" | ✓ verified |
| K2小悪魔 | "悪魔のキスは甘いでしょう？" | ✓ verified |
| K3パチュリー | "キスの魔法というものがあるのを知っているかしら" | ✓ verified |
| K4咲夜 | "瀟洒なメイドらしからぬ顔をしているわね" | ✓ verified |
| K5レミリア | "永遠を感じなさい" | ✓ verified |
| K6フラン | "壊したくならないのさ" | ✓ verified |
| K7子悪魔 | "とけちゃいそう" | ✓ verified |
| K8チルノ | "あたいのキス、最強でしょ？" | ✓ verified |
| K9大妖精 | "大好きだよ" | ✓ verified |
| K10魔理沙 | "最高だぜ" | ✓ verified |

### Phase 4: Verification - COMPLETED

**Timestamp**: 2025-12-18

**Build (AC11)**:
- Command: `dotnet build uEmuera/uEmuera.Headless.csproj`
- Exit Code: 0
- Result: PASS (23 warnings, 0 errors)

**Regression (AC12)**:
- Command: `--inject "tests/core/*.json" --parallel 10`
- Result: 21/21 passed (100%)

**AC1-AC10 (COM_312 kojo tests)**:
- Command: `--unit tests/scenario-106-com312.json --parallel 10`
- Result: 10/10 passed (100%)

| AC | Char | Result |
|----|------|--------|
| 1 | K1美鈴 | PASS |
| 2 | K2小悪魔 | PASS |
| 3 | K3パチュリー | PASS |
| 4 | K4咲夜 | PASS |
| 5 | K5レミリア | PASS |
| 6 | K6フラン | PASS |
| 7 | K7子悪魔 | PASS |
| 8 | K8チルノ | PASS |
| 9 | K9大妖精 | PASS |
| 10 | K10魔理沙 | PASS |

### Phase 5: Completion

**Timestamp**: 2025-12-18
**Status**: Feature DONE
