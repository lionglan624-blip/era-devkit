# Feature 706 調査レポート

## 日付: 2026-02-04

---

## KojoComparer ツールの使い方

### バッチモード（全650ケース）

```bash
cd "C:\Era\erakoumakanNTR"
dotnet run --project tools/KojoComparer -- --all
```

### 単一ケーステスト

```bash
dotnet run --project tools/KojoComparer -- \
  --erb "Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB" \
  --function "@KOJO_MESSAGE_COM_K1_0_1" \
  --yaml "Game/YAML/Kojo/1_美鈴/K1_愛撫_0.yaml" \
  --talent "TALENT:TARGET:恋慕=1"
```

### 結果の見方

```
=== SUMMARY ===
12/650 PASS

650 FAILURES:
FAIL: COM_301 (10)
ERB line 1 not found in YAML:
  "魔理沙は..."
```

- `COM_301 (10)`: COM ID 301, キャラクター ID 10 (魔理沙)
- `ERB line N not found in YAML`: ERB の N 行目が YAML に存在しない

### デバッグ用コマンド

```bash
# ERB 関数を直接実行
dotnet run --project engine/uEmuera.Headless.csproj -- Game \
  --unit-function "@KOJO_MESSAGE_COM_K10_301" --character 10

# YAML ファイルの確認
cat "Game/YAML/Kojo/10_魔理沙/K10_会話親密_1.yaml"
```

---

## 現在の結果

**43/650 PASS** (6.6% 成功率) - 2026-02-05更新

### 進捗
- 12/650 → 43/650 (OutputNormalizerにキャラ名追加で31件改善)

---

## 発見した問題と修正

### 1. GenerateFunctionName バグ (修正済み)

**問題**: COM_301 → `@KOJO_MESSAGE_COM_K10_3_1` と生成されていた

**正しい関数名**: `@KOJO_MESSAGE_COM_K10_301`

**修正**: FileDiscovery.cs
```csharp
// Before: @KOJO_MESSAGE_COM_K{characterId}_{hundreds}_{remainder}
// After:  @KOJO_MESSAGE_COM_K{characterId}_{comId}
```

### 2. CALLNAME TARGET 正規化 (修正済み)

**問題**:
- ERB: `魔理沙` → `<CALLNAME:CHAR>`
- YAML: `%CALLNAME:TARGET%` → `<CALLNAME:TARGET>`

**修正**: OutputNormalizer.cs で TARGET も CHAR に正規化

### 3. TRAIN_MESSAGE/DRAWLINE 定型文 (修正済み)

**問題**: ERB 出力に以下の定型文が含まれ、YAML には存在しない
- `----------------------------------------------` (DRAWLINE)
- `XXXとつながったまま...を丹念に愛撫した…`
- `XXXはくわえ込んだペニスを締め付けながら...`

**修正**: OutputNormalizer.cs で正規表現でこれらを除去

### 4. KojoBranchesParser 分岐選択 (修正済み)

**問題**:
- ERB は ELSE 分岐 (state なし → 最後の分岐)
- YAML は FirstOrDefault で最初の分岐を選択

**修正**: LastOrDefault で最後の空条件分岐を選択

---

## 残りの問題

### パターン 1: 空出力 (COM_303-309)

ERB から空文字列 `""` が返される。原因:
- 該当 COM が未実装？
- 条件分岐で何も出力しないパス？

### パターン 2: 内容の部分的不一致

一部の行は一致するが、一部は不一致。例:
```
ERB: "魔理沙は特に興味なさそうに、<CALLNAME:MASTER>と話をしている。"
→ YAML に存在しない
```

考えられる原因:
- ERB と YAML の内容が異なる (変換時のバグ？手動編集？)
- YAML の分岐選択が間違っている
- 正規化パターンの漏れ

### パターン 3: YAML ファイルの欠落

一部の COM に対応する YAML ファイルが存在しない可能性

---

## 構造的な発見

### ErbToYaml 変換の仕組み

1. 1つの ERB ファイルから複数の YAML ファイルが生成される
2. YAML ファイルのインデックス (_0, _1, _2...) は変換順序であり、COM ID ではない
3. 例: `KOJO_K10_会話親密.ERB` → `K10_会話親密_0.yaml` (COM_300), `K10_会話親密_1.yaml` (COM_301), ...

### FileDiscovery のマッピング

```
COM_300 → sequenceIndex=0 → K10_会話親密_0.yaml
COM_301 → sequenceIndex=1 → K10_会話親密_1.yaml
```

このマッピングが正しければ、内容は一致するはず。

---

## 次のアクション候補

1. ~~詳細ログ追加~~ → 完了
2. ~~サンプル検証~~ → 完了
3. ~~YAML 内容確認~~ → **F748で対応** (ErbToYaml Intro Line Extraction)
4. **空出力の調査**: COM_303-309 がなぜ空出力を返すか調査

---

## 最新調査結果 (2026-02-05)

### 根本原因: ErbToYaml Intro Line Gap

**発見**: 607/650の失敗は、ErbToYaml変換時に「導入文」(PRINTFORM[WL] before PRINTDATA)がYAMLに含まれていないことが原因。

**ERB構造**:
```erb
ELSE
    PRINTFORMW %CALLNAME:TARGET%は特に興味なさそうに...  ← YAMLに無い
    PRINTDATA
        DATALIST  ← YAMLにはここのみ
```

**対応**: F748 [DRAFT] を作成。ErbToYamlの拡張が必要。

### AC状況

| AC | 状態 | 備考 |
|:--:|:----:|------|
| AC1-2 | ✅ | ビルド成功 |
| AC3a | ✅ | 22/22 pass、3 skipped (F678 gap) |
| AC3b | ✅ | YamlRunner tests pass |
| AC4 | ✅ | Skip=3（F678 displayMode gap） |
| AC5 | ⏳ | バッチ完了するが exit 1 (43/650) - F748待ち |
| AC6 | ✅ | Discovered 650 test cases |
| AC7 | ⏳ | 43/650 PASS - F748待ち |
| AC8-26 | ✅ | 全て完了 |

---

## 具体例: COM_301 (魔理沙) の詳細分析

### GenerateFunctionName バグの発見経緯

**症状**: ERB 出力が期待と全く異なる内容だった

```
期待 (COM_301 = お茶を淹れる):
  「おっ、%CALLNAME:MASTER%がお茶を淹れてくれたのか？」

実際の出力:
  「っ……！ お前、何考えてるんだ……」
```

**原因調査**:
1. GenerateFunctionName を確認
2. COM_301 → `@KOJO_MESSAGE_COM_K10_3_1` を生成していた
3. 正しくは `@KOJO_MESSAGE_COM_K10_301`

**バグのロジック**:
```csharp
// Before (バグ)
var hundreds = comId / 100;  // 301 / 100 = 3
var remainder = comId % 100; // 301 % 100 = 1
return $"@KOJO_MESSAGE_COM_K{characterId}_{hundreds}_{remainder}";
// → @KOJO_MESSAGE_COM_K10_3_1 (存在しない関数)

// After (修正)
return $"@KOJO_MESSAGE_COM_K{characterId}_{comId}";
// → @KOJO_MESSAGE_COM_K10_301 (正しい関数)
```

### ERB と YAML の内容対応確認

**ERB ファイル**: `Game/ERB/口上/10_魔理沙/KOJO_K10_会話親密.ERB`
- Line 8: `@KOJO_MESSAGE_COM_K10_300` (普通の会話)
- Line 212: `@KOJO_MESSAGE_COM_K10_301` (お茶を淹れる)

**YAML ファイル**:
- `K10_会話親密_0.yaml` = COM_300 の内容
- `K10_会話親密_1.yaml` = COM_301 の内容

**検証**: ERB line 224 と YAML line 5 を比較
```
ERB:  「――おっ、%CALLNAME:MASTER%がお茶を淹れてくれたのか？　嬉しいぜ」
YAML: 「――おっ、%CALLNAME:MASTER%がお茶を淹れてくれたのか？　嬉しいぜ」
```
→ **完全一致** (変換は正しく行われている)

### PASS しているケースの特徴

12/650 PASS のケースを分析すると:
- 正規化後に ERB の全行が YAML に存在する
- TRAIN_MESSAGE 定型文が正しく除去されている
- CALLNAME パターンが正しく正規化されている

### FAIL しているケースのパターン

| パターン | 例 | 原因 |
|---------|-----|------|
| 空出力 | COM_303-309 | ERB 関数が何も出力しない (未実装 or 条件不成立) |
| 部分不一致 | COM_300, 301 | 一部行は一致、一部は不一致 |
| 全行不一致 | COM_302 | 分岐内容が完全に異なる |

### 空出力の原因推測

COM_303-309 が空出力を返す理由:
1. **未実装 COM**: ERB に該当関数が存在しない
2. **条件不成立**: CFLAG チェック等で早期 RETURN
3. **TALENT 分岐漏れ**: 4分岐全てに該当しない (ありえないはず)

確認方法:
```bash
grep -n "@KOJO_MESSAGE_COM_K10_303" Game/ERB/口上/10_魔理沙/*.ERB
```

---

## 代替調査方針: 分岐0（恋人分岐）のテスト

現在の方針は「最後の分岐（なし/ELSE）」をテストしているが、「最初の分岐（恋人）」をテストする方針も考えられる。

### 現在の方針（LastOrDefault = なし分岐）

| 側 | 選択 | 内容 |
|----|------|------|
| ERB | ELSE分岐 (state なし) | 低好感度/警戒 |
| YAML | LastOrDefault (condition: {}) | 最後の分岐 |

**問題**: YAML の最後の分岐が ERB の ELSE 分岐と内容一致しない場合がある

### 代替方針（FirstOrDefault = 恋人分岐）

| 側 | 選択 | 内容 |
|----|------|------|
| ERB | IF TALENT:恋人 分岐 (TALENT:恋人=1) | 高好感度/甘い |
| YAML | FirstOrDefault (condition: {}) | 最初の分岐 |

**利点**:
- ErbToYaml 変換時、最初に処理されるのは恋人分岐なので、インデックス一致しやすい
- YAML ファイルの最初の分岐は恋人内容のはず

**実装変更**:
1. KojoBranchesParser: `LastOrDefault` → `FirstOrDefault` に戻す
2. FileDiscovery.GetRepresentativeState(): `TALENT:TARGET:恋人=1` を設定
3. ただし Talent.csv に「恋人」が未定義なので、`TALENT:TARGET:恋慕=1` などで代用

### どちらの方針が良いか

| 方針 | Pros | Cons |
|------|------|------|
| なし分岐 (Last) | TALENT 設定不要 | 分岐順序がインデックスと逆 |
| 恋人分岐 (First) | インデックス一致しやすい | TALENT 設定が必要、未定義問題 |

**推奨**: 分岐0（恋人/First）方針を試す価値あり。ErbToYaml の変換順序と一致するため。

---

## 重要な内部知識

### com_file_map.json の構造

場所: `tools/kojo-mapper/com_file_map.json`

```json
{
  "ranges": [
    {"start": 0, "end": 6, "file": "_愛撫.ERB", "implemented": true},
    {"start": 300, "end": 316, "file": "_会話親密.ERB", "implemented": true},
    ...
  ],
  "character_overrides": {
    "K1": { "94": "_挿入.ERB" }  // キャラ固有の上書き
  }
}
```

### Talent.csv の状況

```csv
3,恋慕,;愛情に似た感情を抱いている状態。
148,親愛,;確かな愛情を抱いている状態
```

**注意**: `恋人` と `思慕` は ERB で使用されているが CSV に未定義。Engine は未定義の場合 0 として評価。

### FileDiscovery のマッピングロジック

```
COM_301 (キャラ10)
  ↓ com_file_map.json 参照
range = {start:300, end:316, file:"_会話親密.ERB"}
  ↓ category 抽出
category = "会話親密"
  ↓ YAML ファイル検索
pattern = "K10_会話親密_*.yaml"
  ↓ sequenceIndex 計算
implementedComs = [300, 301, 302, ...]
sequenceIndex = indexOf(301) = 1
  ↓ 結果
yamlFile = yamlFiles[1] = "K10_会話親密_1.yaml"
```

### ErbToYaml の変換順序

1. ERB ファイルをパース
2. 全 PRINTDATA/DATALIST ブロックを順番に処理
3. 各ブロックに `_0`, `_1`, `_2`... のインデックス付与
4. 1 ERB → 複数 YAML (例: 12 YAML from 1 ERB)

**重要**: インデックスは COM ID ではなく、ERB 内のブロック出現順序

### OutputNormalizer の正規化パターン

| パターン | 処理 |
|---------|------|
| CRLF | → LF |
| `^DATAFORM\s*` | 除去 |
| `^-{20,}$` | 除去 (DRAWLINE) |
| `XXXとつながったまま...愛撫した…` | 除去 (TRAIN_MESSAGE) |
| `XXXはくわえ込んだペニスを...` | 除去 (TRAIN_MESSAGE) |
| `[COLOR 0xXXX]` | 除去 |
| 全角スペース | → 半角 |
| `%CALLNAME:MASTER%` | → `<CALLNAME:MASTER>` |
| `%CALLNAME:TARGET%` | → `<CALLNAME:CHAR>` |
| `%CALLNAME:人物_X%` | → `<CALLNAME:CHAR>` |
| `<CALLNAME:...>` | 同様に正規化 |
| 既知キャラ名 (美鈴等) | → `<CALLNAME:CHAR>` |
| 空行 | 除去 |

### KojoBranchesParser の制限

1. **条件評価をしない** - `condition: {talent: 恋慕}` などを無視
2. **context パラメータを使用しない** - TALENT 値を受け取っても分岐選択に使わない
3. **FirstOrDefault / LastOrDefault のみ** - 空条件の分岐を機械的に選択
4. **テンプレート置換なし** - `%CALLNAME:X%` をそのまま返す (OutputNormalizer で後処理)

---

## ファイル変更履歴

| ファイル | 変更内容 |
|---------|---------|
| tools/KojoComparer/FileDiscovery.cs | GenerateFunctionName 修正, GetRepresentativeState 空辞書に変更 |
| tools/KojoComparer/OutputNormalizer.cs | TARGET→CHAR 正規化, TRAIN_MESSAGE/DRAWLINE 除去 |
| tools/KojoComparer/KojoBranchesParser.cs | FirstOrDefault → LastOrDefault |

---

## 参考情報

- ERB と YAML の内容は ErbToYaml ツールで変換されたもので、基本的には一致するはず
- Phase 19 (Kojo Conversion) で F636-F643 にて変換が実施された
- 等価性テストは F644 で設計、F706 で完全検証を目指している
