# Feature 208: シナリオ期待値修正 (sc-002, sc-004)

## Status: [DONE]

## Type: infra

## Depends: [207, 209]

## Background

### Problem

F207 で回帰テストの var_equals 検証を実装したが、sc-002 と sc-004 の期待値設定に問題が発見された:

| シナリオ | 元の期待 | 問題 |
|----------|----------|------|
| sc-002 | TALENT:1:18=1 (恋慕獲得) | TALENT:18 は誤り。恋慕は TALENT:3。また EXP:奉仕快楽経験>=30 が未設定 |
| sc-004 | TALENT:1:101=1 (NTR陥落) | TALENT:101 は誤り。NTR は TALENT:6。また EVENTTURNEND が実行されない |

### F200-205 で定められた禁止事項

**回帰テストシナリオは Hook による編集禁止**。シナリオ修正は別 Feature として立てる必要がある。

### Goal

1. sc-002, sc-004 のシナリオ設定を正しく修正
2. StateInjector の EXP 対応（必要な場合）
3. EVENTTURNEND が実行される input シーケンスの作成（必要な場合）

---

## Acceptance Criteria

### Part A: シナリオ修正

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| A1 | sc-002: var_equals で TALENT:1:3 を検証 | file | contains | `"TALENT:1:3"` | [x] |
| A2 | sc-002: 恋慕獲得メッセージ出力 | output | contains | "[恋慕]を得た" | [x] |
| A3 | sc-004: var_equals で TALENT:1:6 を検証 | file | contains | `"TALENT:1:6"` | [x] |
| A4 | sc-004: NTR陥落メッセージ出力 | output | contains | "[NTR]を得た" | [x] |

### Part B: StateInjector 拡張

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| B1 | EXP 変数注入対応 | code | contains | `case "EXP":` | [x] |
| B2 | EXP 注入テスト | output | contains | `[Inject] EXP` | [x] |

### Part C: input シーケンス修正

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| C1 | input-sc-002: ターン終了→昇格判定実行 | output | contains | "[思慕]を失い" | [x] |
| C2 | input-sc-004: ターン終了→NTR判定実行 | output | contains | "寝取られてしまった" | [x] |

### Part D: ビルド・テスト

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| D1 | dotnet build 成功 | build | succeeds | - | [x] |
| D2 | dotnet test 成功 | exit_code | equals | 0 | [x] |
| D3 | 24/24 PASS (var_equals 込み) | output | contains | "24/24 passed" | [x] |

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | A1 | sc-002: characters に EXP:40=30 追加 | scenario-sc-002-*.json | [O] |
| 2 | A1 | sc-002: expect.var_equals に TALENT:1:3=1 設定 | scenario-sc-002-*.json | [O] |
| 3 | A2 | sc-002: expect.output_contains に恋慕メッセージ追加 | scenario-sc-002-*.json | [O] |
| 4 | A3 | sc-004: expect.var_equals に TALENT:1:6=1 設定 | scenario-sc-004-*.json | [O] |
| 5 | A4 | sc-004: expect.output_contains にNTRメッセージ追加 | scenario-sc-004-*.json | [O] |
| 6 | B1 | StateInjector: EXP 変数注入対応 | StateInjector.cs | [O] |
| 7 | B2 | StateInjector: EXP 注入テスト検証 | - | [O] |
| 8 | C1 | input-sc-002: ターン終了コマンド追加 | input-sc-002-*.txt | [O] |
| 9 | C2 | input-sc-004: ターン終了コマンド追加 | input-sc-004-*.txt | [O] |
| 10 | D1 | dotnet build 実行 | - | [O] |
| 11 | D2 | dotnet test 実行 | - | [O] |
| 12 | D3 | テスト結果確認 (24/24 PASS) | - | [O] |

---

## Technical Details

### 恋慕昇格条件 (EVENTTURNEND.ERB L156)

```erb
IF CFLAG:奴隷:好感度 > 閾値 && EXP:奴隷:奉仕快楽経験 >= 30 && ABL:奴隷:従順 >= 3 && !TALENT:奴隷:恋慕
```

**現在の sc-002 設定**:
- `CFLAG:2` (好感度) = 1500: ✓ (閾値 1000 超過)
- `TALENT:17` (思慕) = 1: ✓ (前提条件)
- `ABL:10` (従順) = 4: ✓ (3 以上)
- `EXP:40` (奉仕快楽経験): **未設定 ← 必要**

**解決策**:
1. scenario-sc-002 に `"EXP:40": 30` を追加
2. input-sc-002 でターン終了 (EVENTTURNEND 実行) を追加
3. expect.var_equals で `TALENT:1:3` (恋慕) = 1 を検証

### NTR陥落条件 (EVENTTURNEND.ERB L274-275)

```erb
GET_NTR = CFLAG:奴隷:好感度 < 1000 && CFLAG:奴隷:屈服度 > CFLAG:奴隷:好感度 && CFLAG:奴隷:屈服度 > 2000
```

**現在の sc-004 設定**:
- `CFLAG:2` (好感度) = 500: ✓ (1000 未満)
- `CFLAG:21` (屈服度) = 2500: ✓ (2000 超過、好感度超過)
- `TALENT:6` (NTR) = 0: ✓ (未取得)

**問題**: CHK_NTR_CHANGE は EVENTTURNEND 内で呼ばれるため、ターン終了まで実行する必要がある。

**解決策**:
1. input-sc-004 でターン終了 (EVENTTURNEND 実行) を追加
2. expect.var_equals で `TALENT:1:6` (NTR) = 1 を検証

### 変数番号対応 (CSV 定義から確認済)

**TALENT**:
| 名前 | 番号 | CSV 定義 |
|------|:----:|----------|
| 恋慕 | 3 | Talent.csv L7 |
| NTR | 6 | Talent.csv L10 |
| 思慕 | 17 | Talent.csv L16 |
| 親愛 | 148 | Talent.csv L158 |

**ABL**:
| 名前 | 番号 | CSV 定義 |
|------|:----:|----------|
| 従順 | 10 | ABL.csv L9 |

**EXP**:
| 名前 | 番号 | CSV 定義 |
|------|:----:|----------|
| 奉仕快楽経験 | 40 | exp.csv L42 |

**CFLAG**:
| 名前 | 番号 | 備考 |
|------|:----:|------|
| 好感度 | 2 | - |
| 屈服度 | 21 | - |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-25 | - | F207 から分離して作成 | PROPOSED |
| 2025-12-25 | feature-reviewer | Pre-implementation review | NEEDS_REVISION |
| 2025-12-25 | opus | AC C1/C2 を output 検証に修正、A2/A4 を output 検証に変更 | REVISED |
| 2025-12-25 | opus | CSV 確認: TALENT 番号検証、EXP/ABL 番号確認 | VERIFIED |
| 2025-12-25 | opus | Tasks 修正: EXP:40 追加タスク分離、Task 番号調整 | REVISED |
| 2025-12-25 | initializer | Status updated to WIP, dependency F207 verified DONE | READY |
| 2025-12-25 | implementer | Scenario files updated, StateInjector EXP support added | SUCCESS |
| 2025-12-25 | debugger | COM888 not triggering EVENTTURNEND in --flow mode | BLOCKED |
| 2025-12-25 | opus | F209 (Flow Mode State Fix) 作成、本Featureの依存に追加 | BLOCKED→F209待ち |
| 2025-12-25 | opus | F209-211完了、全AC PASS確認 | DONE |

---

## Links

- [feature-207.md](feature-207.md) - Flow Test Verification Framework
- [feature-209.md](feature-209.md) - Flow Mode State Transition Fix (ブロッカー)
- [EVENTTURNEND.ERB](../../ERB/EVENTTURNEND.ERB) - 昇格/陥落ロジック
- [StateInjector.cs](../../engine/Assets/Scripts/Emuera/Headless/StateInjector.cs)
- [Talent.csv](../../CSV/Talent.csv)
