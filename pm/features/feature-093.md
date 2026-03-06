# Feature 093: eraTW参照による口上品質向上試験 (COM_310 美鈴)

## Status: [DONE]

## Type: kojo

## Background

### Problem

現在のkojo実装は「4-8行」のガイドラインを満たしているが、最低限の4行で終わることが多い。情緒描写やバリエーションが不足している。

### Goal

eraTW霊夢の口上を「構造・分量の参考」としてkojo-writerに提供し、品質向上効果を検証する。

### Context

**試験対象**: 美鈴(K1) COM_310（撫でる）

**現状** (紅魔館 美鈴 COM_310):
```erb
@KOJO_MESSAGE_COM_K1_310_1
IF ABL:親密 > 5 && TALENT:恋慕
	PRINTFORML 「も、もう…そんなところ触って…」
	PRINTFORML 「%CALLNAME:MASTER%ったら、えっちなんだから…」
	PRINTFORMW 「…嫌じゃないけど、恥ずかしいわよ。」
; 2-3行のみ、バリエーションなし、情緒描写なし
```

**参考** (eraTW 霊夢 COM_310):
```erb
IF TALENT:恋人
	PRINTDATA
		DATALIST
			DATAFORM 「あっ……やぁ……」
			DATAFORM 霊夢はされるがままに、%CALLNAME:MASTER%に尻をなでられている。
			DATAFORM 「……%CALLNAME:MASTER%、手付き……いやらしいわ……」
			DATAFORM 霊夢は熱い息を吐きながらも、抵抗をしなかった。
		ENDLIST
		; 複数DATALIST、5-6行、情緒描写あり
```

**試験方法**:
1. kojo-writer実行前に eraTW霊夢の同COM関数を読む
2. 「構造・分量の参考。口調はコピーしない」と明示してkojo-writerに渡す
3. 出力品質を比較評価

---

## Reference: eraTW霊夢 COM_310

**ファイル**: `C:\Users\siihe\OneDrive\同人ゲーム\eraTW4.920\ERB\口上・メッセージ関連\個人口上\001 Reimu [霊夢]\霊夢\M_KOJO_K1_コマンド.ERB`
**行番号**: 1083-1143

```erb
@M_KOJO_MESSAGE_COM_K1_310_1
IF LOCAL
	IF FLAG:時間停止
		PRINTFORML
		PRINTFORMW %CALLNAME:MASTER%は霊夢の尻をなでている。
		PRINTFORMW その形はなめらかで、丸い。
		PRINTFORMW 張りのある柔らかさと体温の温かさが手に心地いい。
		PRINTFORMW %CALLNAME:MASTER%はしばらくの間、その感触を堪能した。
	ELSEIF TALENT:恋人
		PRINTDATA
			DATALIST
				DATAFORM
				DATAFORM 「あっ……やぁ……」
				DATAFORM 霊夢はされるがままに、%CALLNAME:MASTER%に尻をなでられている。
				DATAFORM 「……%CALLNAME:MASTER%、手付き……いやらしいわ……」
				DATAFORM 霊夢は熱い息を吐きながらも、抵抗をしなかった。
			ENDLIST
			DATALIST
				DATAFORM
				DATAFORM 「んっ……くぅ……」
				DATAFORM 霊夢は紅い顔で恥情に耐えている。
				DATAFORM 「……ね、%CALLNAME:MASTER%……こういうのは、服を、脱いでから……」
				DATAFORM 上気した顔で、霊夢はそう訴えてきた。
				DATAFORM それを意に介さず、%CALLNAME:MASTER%は霊夢の尻を撫で続けた。
			ENDLIST
		ENDDATA
	ELSEIF TALENT:恋慕
		PRINTDATA
			DATALIST
				DATAFORM
				DATAFORM 「んっ……やっ……」
				DATAFORM 霊夢はされるがままでいる。
				DATAFORM 「やぁ……ちょっと……そろそろ、やめ……」
				DATAFORM そういいながらも、霊夢は%CALLNAME:MASTER%の手を止めには来なかった。
			ENDLIST
			DATALIST
				DATAFORM
				DATAFORM 「んぅ……やぁ……」
				DATAFORM 霊夢は紅い顔で%CALLNAME:MASTER%に尻を撫でられている。
				DATAFORM 「んっ……服が、しわになるわ……」
				DATAFORM 霊夢は%CALLNAME:MASTER%の手をそっと止めた。
			ENDLIST
		ENDDATA
	ELSE
		PRINTFORML
		PRINTFORMW 「……っ……くぅっ……」
		PRINTFORMW 霊夢は顔を真っ赤にして、されるがままになっている。
	ENDIF
ENDIF
```

**品質ポイント**:
- 各TALENT分岐で複数DATALIST（2パターン）
- 各DATALIST 4-6行
- セリフ + 動作描写 + 心理描写の組み合わせ
- 低関係性でも最低3行

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | ビルド成功 | build | succeeds | - | [x] |
| 2 | 恋人分岐で複数DATALIST | code | contains | "DATALIST" | [x] |
| 3 | 各DATALIST 4行以上 | manual | review | 4+ lines | [x] |
| 4 | 情緒描写あり | manual | review | 動作/心理描写 | [x] |
| 5 | 美鈴の口調維持 | manual | review | のんびり混在敬語 | [x] |

### AC Details

#### AC1: ビルド成功

**Test Command**:
```bash
cd Game && dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- .
```

#### AC2: 恋人分岐で複数DATALIST

**Test Command**:
```bash
grep -n "DATALIST" Game/ERB/口上/1_美鈴/KOJO_K1_会話親密.ERB
```

**Expected**: Multiple instances of DATALIST within the COM_310 恋人 branch.

#### AC3-5: 手動レビュー

kojo-writer出力を以下の基準で評価:

| 基準 | 現状 | 目標 |
|------|------|------|
| DATALIST数 | 0 | 2+ |
| 分岐あたり行数 | 2-3 | 4-6 |
| 情緒描写 | なし | あり |
| キャラ性 | - | 美鈴らしさ維持 |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | ビルド成功確認 | [x] |
| 2 | 2 | kojo-writerで複数DATALISTを実装（eraTW参考） | [x] |
| 3 | 3 | 各DATALIST行数が4行以上か評価 | [x] |
| 4 | 4 | 情緒描写（動作/心理）が含まれているか評価 | [x] |
| 5 | 5 | 美鈴の口調（のんびり混在敬語）が維持されているか評価 | [x] |

---

## Implementation Approach

### kojo-writer プロンプト追加内容

```
## 参考: eraTW霊夢 COM_310

以下は構造・分量の参考です。口調はコピーしないでください。

[eraTW霊夢のコードを挿入]

**参考ポイント**:
- 各TALENT分岐で複数DATALIST
- 各DATALIST 4-6行
- セリフ + 動作描写 + 心理描写
- 低関係性でも情緒的反応
```

---

## Execution State

**Phase**: Implementation (kojo-writer)
**Current Task**: Task 2 - kojo-writer実装（複数DATALIST + eraTW参考）
**Assigned**: [Pending]
**Dispatch Model**: opus (kojo-writer)

### Summary

Feature 093 - eraTW参照による口上品質向上試験. Feature already in [WIP] status. Objective: Test whether providing eraTW reference structure to kojo-writer improves COM_310 (Meiling) dialogue quality beyond basic 4-line minimum. Current phase: Ready for kojo-writer dispatch.

### Status

- **Previous**: [WIP] (already in progress)
- **Current**: [WIP] (initialization confirms readiness)
- **Next Action**: Dispatch to kojo-writer with eraTW Reimu COM_310 reference to generate improved version with multiple DATALIST patterns and emotional/psychological depth

### Key Context

- **Target**: KOJO_K1_会話親密.ERB - COM_310 (撫でる)
- **Reference**: eraTW霊夢 COM_310 with multiple DATALIST patterns (4-6 lines each)
- **Goal**: Validate eraTW-reference approach for quality improvement
- **Constraints**: Maintain Meiling's character (のんびり混在敬語), don't copy Reimu's tone

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-17 | initializer | Feature 093 initialization | READY |
| 2025-12-17 | unit-tester | Task 1: Build verification | PASS |
| 2025-12-17 | kojo-writer | Task 2: COM_310 恋人分岐に複数DATALIST実装 | SUCCESS |
| 2025-12-17 | ac-tester | AC3: Verify 各DATALIST 4行以上 | PASS |
| 2025-12-17 | ac-tester | AC4: Verify emotional/psychological descriptions (心理描写) | PASS |
| 2025-12-17 | regression-tester | Full regression test suite | PASS |
| 2025-12-17 | finalizer | Feature completion - Status update to [DONE] | COMPLETE |

---

## Discovered Issues

| Issue | Type | Priority |
|-------|------|----------|
| | | |

---

## Links

- [kojo-reference.md](reference/kojo-reference.md)
- [KOJO_K1_会話親密.ERB](../ERB/口上/1_美鈴/KOJO_K1_会話親密.ERB) - 改善対象
- eraTW参照: `C:\Users\siihe\OneDrive\同人ゲーム\eraTW4.920\ERB\口上・メッセージ関連\個人口上\001 Reimu [霊夢]\霊夢\M_KOJO_K1_コマンド.ERB`
