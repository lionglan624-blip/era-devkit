# Feature 102: kojo-test TALENT変数設定機能

## Status: [CANCELLED]

**Cancellation Reason**: 既存の `--set "TALENT:TARGET:16=1"` で TALENT 分岐テストが可能と判明。
testing-reference.md Section 4.3 に TALENT Index Reference を追加して解決。

## Type: engine

## Background

### Problem

kojo-testモードでTALENT変数（恋人/恋慕/思慕/なし）を設定できないため、TALENT分岐のある口上をテストできない。

Feature 099 (COM_310) のAC検証で発覚:
- AC2, AC3, AC4, AC5 が BLOCKED
- 理由: kojo-testがTALENT変数を設定せずに実行するため、常にELSE分岐（関係性なし）が実行される
- TALENT:恋人 分岐の期待文字列が出力されず、AC検証不可

### Goal

kojo-testモードでTALENT変数を設定してからkojo関数を実行できるようにする。

### Context

現状のkojo-test:
```bash
dotnet run ... --unit "KOJO_MESSAGE_COM_K1_310" --char 1
```

期待する機能:
```bash
dotnet run ... --unit "KOJO_MESSAGE_COM_K1_310" --char 1 --talent 恋人
# または
dotnet run ... --unit "KOJO_MESSAGE_COM_K1_310" --char 1 --set "TALENT:恋人=1"
```

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | TALENT:恋人設定でkojo実行 | output | contains | "もっと、触ってもいいのよ……？" | [ ] |
| 2 | TALENT:恋慕設定でkojo実行 | output | contains | "えっちなんだから……" | [ ] |
| 3 | TALENT未設定でELSE実行 | output | contains | "門番として、あんまりなれなれしくされると困るんですけど" | [ ] |
| 4 | ビルド成功 | build | succeeds | - | [ ] |
| 5 | testing-reference.md更新 | code | contains | "--talent" | [ ] |

### AC Details

#### AC1: TALENT:恋人設定でkojo実行

**Test Command**:
```bash
cd Game
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --unit "KOJO_MESSAGE_COM_K1_310" --char 1 --talent 恋人
```

**Expected Output**:
TALENT:恋人分岐のセリフが出力される（例: "もっと、触ってもいいのよ……？"）

#### AC2: TALENT:恋慕設定でkojo実行

**Test Command**:
```bash
cd Game
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --unit "KOJO_MESSAGE_COM_K1_310" --char 1 --talent 恋慕
```

**Expected Output**:
TALENT:恋慕分岐のセリフが出力される（例: "えっちなんだから……"）

#### AC3: TALENT未設定でELSE実行

**Test Command**:
```bash
cd Game
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --unit "KOJO_MESSAGE_COM_K1_310" --char 1
```

**Expected Output**:
ELSE分岐（関係性なし）のセリフが出力される（例: "門番として、あんまりなれなれしくされると困るんですけど"）

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | --talent フラグ実装: TALENT:恋人設定機能 | [ ] |
| 2 | 2 | --talent フラグ実装: TALENT:恋慕設定機能 | [ ] |
| 3 | 3 | --talent フラグ実装: TALENT未設定時ELSE分岐動作確認 | [ ] |
| 4 | 4 | ビルド成功確認 | [ ] |
| 5 | 5 | testing-reference.md更新 | [ ] |

---

## Design Notes

### Option 1: --talent フラグ

```bash
--unit "func" --char N --talent {恋人|恋慕|思慕|なし}
```

Pros: シンプル、よく使うケースに最適化
Cons: TALENT以外の変数設定には使えない

### Option 2: 汎用 --set フラグ

```bash
--unit "func" --char N --set "TALENT:TARGET:恋人=1"
```

Pros: 汎用的、任意の変数設定可能、**既に実装済み**
Cons: 記述が長い

**Note**: `--set "TALENT:TARGET:恋人=1"` は既に動作する。StateInjector.SetTalent()が存在。

### Recommendation

Option 1 (--talent) を推奨。kojo口上テストでは99%がTALENT分岐のテストなので、専用フラグが便利。

### TALENT Index Reference (CSV/Talent.csv)

| TALENT名 | CSV Index | 説明 |
|----------|-----------|------|
| 恋慕 | 3 | 愛情に似た感情を抱いている状態 |
| 恋人 | 16 | 最高の親愛度。恋人関係 |
| 思慕 | 17 | 好意的な感情を抱いている状態 |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-18 | ac-validator (sonnet) | AC検証・具体値埋め込み | AC1-3に具体的期待値設定、AC5をcode型に修正 |

---

## Discovered Issues

| Issue | Type | Priority |
|-------|------|----------|
| | | |

---

## Links

- [Feature 099](feature-099.md) - この問題が発覚したFeature
- [testing-reference.md](reference/testing-reference.md) - テストコマンドドキュメント
