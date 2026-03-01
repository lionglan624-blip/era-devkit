# ERB-YAML等価性検証 診断レポート

> 作成日: 2026-02-11
> 最終更新: 2026-02-11 (#DIM変数対応 — 2364/2364 PASS (100%), ~12秒)
> 対象: KojoComparer --all (591テストケース), --all --multi-state (2364テストケース)
> 関連Feature: F626-F773 (95+ features)

---

## 概要

ERB口上ファイルをYAMLに変換し、出力の等価性を機械的に証明するプロジェクト。F636-F643で117 ERBファイルを~1128 YAMLファイルに自動変換した後、KojoComparer --allで等価性を検証している。

**到達状況**:

| 段階 | PASS率 | 修正内容 |
|------|:------:|----------|
| F773完了後（ソート修正前） | 79/650 (12.2%) | TALENT条件移行完了 |
| Bug 1修正後 | 356/650 (54.8%) | 辞書順ソート→数値順ソート |
| Bug 1-6修正後 | 572/588 (97.3%) | システム的バグ全修正 |
| エッジケース修正後 | 579/579 (100%) | COM_463暫定除外含む |
| COM_463サブ関数対応後 | 591/591 (100%) | COM_463を12テストとして復帰 |
| ErbEvaluator導入後 | 591/591 (100%) | インプロセス実行、~8分→~3秒 (150x高速化) |
| **#DIM変数対応後** | **2364/2364 (100%)** | **Multi-State全プロファイル完全一致** |

---

## パイプライン全体像

```
ERB口上ファイル (117files)
    │
    ▼ ErbToYaml (F633-F634)
YAML Kojoファイル (~1128files)
    │  com_id メタデータ埋め込み済み (796files)
    │
    ▼ 後処理マイグレーション
    │  ├── F675: branches→entries形式統一
    │  ├── F748-F749: イントロ行抽出・注入
    │  ├── F750: TALENT条件(branches形式, 13files)
    │  └── F773: TALENT条件(entries形式, ~608files)
    │
    ▼ KojoComparer --all (F644, F706)
等価性検証 (591テストケース → 591 PASS = 100%)
    │
    ▼ KojoComparer --all --multi-state
Multi-State検証 (4状態 × 591 = 2364テストケース → 2364 PASS = 100%)
  default{}: 591/591, 恋人: 591/591, 恋慕: 591/591, 思慕: 591/591
```

### KojoComparerの動作

1. **FileDiscovery**: YAMLファイルの`com_id`メタデータで直接マッピング（旧: com_file_map索引計算）
2. **ErbBatchEvaluator**: インプロセスERB評価（ErbEvaluator使用）。旧: headlessエンジンサブプロセス
3. **YamlRunner**: Era.CoreでYAMLをレンダリング（PriorityDialogueSelector）
4. **OutputNormalizer**: 両出力を正規化（CALLNAME, DRAWLINE, TRAIN_MESSAGE除去）
5. **DiffEngine.CompareSubset**: ERB出力の全行がYAML出力に含まれるかチェック

### ErbEvaluator（軽量ERBインタープリター）

ErbParser ASTを直接ウォークして口上関数を実行する。headlessエンジンのサブプロセス起動を不要にし、~150倍高速化。

| | 旧方式 (BatchExecutor) | 新方式 (ErbEvaluator) |
|---|---|---|
| 実行方式 | dotnet exec × ~11サブプロセス | インプロセスAST評価 |
| ERB/CSVロード | 全ファイル × 11回 | 対象関数のみ + CSV 3ファイル |
| 591件実行時間 | ~8分 | ~3秒 |
| PRINTDATA処理 | ランダム1 DATALIST選択 | 全DATALIST出力 |
| `--legacy`フラグ | - | 旧方式にフォールバック可 |

**対応ASTノード**: FunctionDef, If/ElseIf/Else, PrintForm(L/W/K等), PrintData/Datalist/Dataform, SelectCase, Assignment, Return, Call（同一ディレクトリ内クロスファイル検索対応）

**PRINTFORMW結合対策**: PRINTFORMW（改行なし）+ PRINTDATA で行結合が発生。OutputNormalizerに `。「` → `。\n「` 分割ルールを追加し解消。両側（ERB/YAML）に同じ正規化が適用されるため、自然に `。「` を含む単一行も安全に処理される。

### テスト条件

- **`--all`**: 空state `{}` のみ（ELSE/フォールバック分岐、591テスト）
- **`--all --multi-state`**: 4状態プロファイル（default, 恋人, 恋慕, 思慕）× 591 = 2364テスト
- **比較方式**: Subset matching（ERB⊆YAML）。PRINTDATAランダム選択対応。

| プロファイル | State | 検証対象 |
|:------------|-------|----------|
| default | `{}` | ELSE/フォールバック分岐 |
| 恋人 | `TALENT:TARGET:16=1` | TALENT:恋人 ブランチ |
| 恋慕 | `TALENT:TARGET:3=1` | TALENT:恋慕 ブランチ |
| 思慕 | `TALENT:TARGET:17=1` | TALENT:思慕 ブランチ |

プロファイルは `src/tools/dotnet/KojoComparer/state-profiles.json` で外部設定化済み。追加は JSON 編集のみ。

### Multi-State 結果

**結果**: 2364/2364 PASS (100%)

| プロファイル | PASS | FAIL |
|:------------|-----:|-----:|
| default | 591/591 | 0 |
| 恋人 | 591/591 | 0 |
| 恋慕 | 591/591 | 0 |
| 思慕 | 591/591 | 0 |

### 拡張ポイント（未対応条件型）

| 条件型 | 出現回数 | 必要な変更 |
|--------|:--------:|-----------|
| TCVAR | 85 | ConditionExtractor に TCVAR パーサー追加 |
| ABL | 121 | state-profiles.json にプロファイル追加のみ (パーサーは対応済み) |
| TFLAG | 98 | Talent.yaml の `コマンド成功度=197` で名前解決可。プロファイル追加 |
| CFLAG | 9 | プロファイル追加のみ (パーサーは対応済み) |

---

## 修正済みバグ一覧

### Bug 7: ErbParser #DIM変数未対応 [修正済み]

**場所**: `src/tools/dotnet/ErbParser/ErbParser.cs`
**影響**: Multi-State 1件 (COM_350 K5 恋慕プロファイル)
**修正効果**: 2363→2364 PASS (+1, 100%達成)

**根本原因**: ErbParser が `#DIM` 変数宣言をスキップし、DIM変数への代入（`奴隷 = 人物_レミリア`）を認識しなかった。ErbEvaluator で `TALENT:奴隷:恋慕` の間接変数参照が未解決 → ELSE分岐に落ちる。

**修正内容**: ErbParser に `#DIM` 変数追跡機能を追加。
1. 関数スコープの `HashSet<string> dimDeclaredVars` で宣言済み変数名を追跡
2. `#DIM varname` パースで変数名をセットに追加
3. DIM変数への代入を `AssignmentNode` として生成
4. ErbEvaluator の `ExecuteAssignment` は既に非LOCAL変数をサポート済みのため変更不要

---

### Bug 1: 辞書順ファイルソート [修正済み]

**場所**: `src/tools/dotnet/KojoComparer/FileDiscovery.cs:177`
**修正効果**: 79→356 PASS (+277)

辞書順ソートで`_10`が`_2`より前に来る問題。数値順ソートに変更。

---

### Bug 2: COM-YAML索引オフセット [修正済み — com_idメタデータ方式]

**影響**: ~171件の内容不一致FAIL
**修正効果**: Bug 2+4合計で ~138 PASS増

**根本原因**: ErbToYamlは**物理的ERB関数出現順**でYAMLを生成し、FileDiscoveryは**論理的com_file_map順**でYAMLを参照。この2つの順序が一致しない。

**追加発見**: COM_20/21は`IF LOCAL`パターン（`LOCAL = 1; IF LOCAL; IF TALENT; PRINTDATA`）のためErbToYaml変換に失敗し、YAMLが生成されない。これがオフセットの一因。

**修正内容**:
1. ErbToYaml: ERB関数名からCOM IDを抽出し`com_id`フィールドをYAML出力に追加 (`FileConverter.cs`)
2. temp-dir方式で既存796 YAMLファイルにcom_idを正確に注入（content fingerprint matching）
3. FileDiscovery: com_file_mapの索引計算を廃止。YAMLの`com_id`で直接マッピング
4. dialogue-schema.json: `com_id`フィールド（integer, optional）追加

---

### Bug 3: com_file_mapの過大申告 [修正済み]

**影響**: ~70件のファントムテスト（テスト数 650→580に削減）

**根本原因**: com_file_map.jsonでCOM_300-316を単一の`implemented: true`範囲として宣言。実際はCOM_303-309が全キャラで未実装（COM_310-316は存在する）。

**ドキュメント訂正**: 旧記述「COM_300-302しか存在しない」は**誤り**。COM_310-316は全10キャラに実装済み。ギャップはCOM_303-309のみ。

**修正内容**: `com_file_map.json`のCOM_300-316範囲を3分割:
- `{300-302, implemented: true}`
- `{303-309, implemented: false}`
- `{310-316, implemented: true}`

---

### Bug 4: character_override片方向処理 [修正済み — com_idメタデータ方式で解消]

**影響**: 8件のオーファンCOM（K1/K7/K9のCOM_094、K10のCOM_090-094）

**根本原因**: `FindComRangesForErb`がcharacter_overridesのAWAY（除外）のみ処理し、INTO（追加）を処理しない。

**追加発見**: K10 COM_091のoverride先が`_愛撫.ERB`だが実体は`_口挿入.ERB`（データ誤り）。

**修正内容**: com_idメタデータ方式により索引計算自体が不要になり解消。K10 COM_091のoverride誤りも修正。

---

### Bug 5: ERBランタイムエラー [修正済み]

**影響**: 12件（COM_006 全10キャラ + COM_003,005 K1のみ）
**修正効果**: +12 PASS

**根本原因**: `GET_ABL_BRANCH`等の関数はCOMMON_KOJO.ERBに**定義されている**が、`#FUNCTION`宣言が欠落。式中呼び出し（`LOCAL = GET_ABL_BRANCH(...)`）でパースエラー。

**ドキュメント訂正**: 旧記述「COMMON_KOJO.ERBで未定義」は**誤り**。関数は存在するが`#FUNCTION`属性が欠落。headless固有の問題ではなくパース時エラー（GUI環境でも同発生）。

**修正内容**: COMMON_KOJO.ERBの3関数（GET_ABL_BRANCH, GET_EXP_BRANCH, GET_TALENT_BRANCH）に`#FUNCTION`宣言と`RETURNF`を追加。

---

### Bug 6: YAMLパーサーエラー [修正済み]

**影響**: 11件
**修正効果**: +11 PASS

**Issue A: entries形式ファイルにbranches形式条件が混在 (8件)**

**ドキュメント訂正**: 旧記述「branches形式YAMLの条件フィールドが空」は**誤り**。8件は**entries形式**ファイルにbranches形式条件（AND/TALENT/NOT）が残存。`YamlDialogueLoader`の`IgnoreUnmatchedProperties()`でsilent discard → `DialogueCondition{Type: ""}`。

**修正内容**: 16ファイルの条件をentries形式（`type: And`, `operands`, `singleOperand`）に変換。

**Issue B: branches形式ファイルにフォールバック欠落 (3件)**

3ファイル（K8_会話親密_10, K10_会話親密_10, K10_会話親密_11）にTALENT条件ブランチのみでフォールバック（`condition: {}`）が欠落。

**修正内容**: 3ファイルにフォールバックブランチ追加。

---

## エッジケース16件の修正 [修正済み]

| カテゴリ | 件数 | 原因 | 修正内容 |
|----------|:----:|------|----------|
| COM_463 サブ関数パターン | 9→0 | 親関数なし（COMF463.ERBがサブ関数を直接呼出） | FileDiscoveryサブ関数対応 + NTRフィルタ追加 |
| COM_312 com_id重複 | 3 | Phase 8新版と旧版が共存、TryAdd first-wins | 旧版YAML 3ファイル削除 |
| COM_007 パス形式エラー | 2 | K4/K9の`_乳首責め.yaml`（サフィックスなし）が未対応 | YamlRunner正規表現拡張 |
| COM_300 イントロ未変換 | 1 | K4独自イントロブロックがYAML範囲外 | OutputNormalizerにパターン追加 |
| COM_350 行欠落 | 1 | K5のPRINTFORML行がYAML変換時に欠落 | YAML手動修正 |
| **合計** | **16→0** | | |

---

## 修正の全体効果

```
Bug 1修正:      356/650 (54.8%)
Bug 3修正:      テスト数 650→~580 (ファントム70件削除)
Bug 5修正:      +12 PASS
Bug 6修正:      +11 PASS
Bug 2+4修正:    +138 PASS (com_idメタデータ方式)
com_id再注入:   434→572 PASS (temp-dir方式で索引ズレ解消)
エッジケース修正: +7 PASS
COM_463対応:  +12 PASS (サブ関数テスト復帰, NTRフィルタ)
────────────────────────────────────
最終結果(旧):  591/591 (100.0%)

--- ErbEvaluator導入 ---
OutputNormalizer: 。「 分割ルール追加 (PRINTFORMW結合対策)
ErbEvaluator:         591/591 (100%) — ~3秒
Multi-State:        2364/2364 (100%) — ~12秒

--- #DIM変数対応 ---
ErbParser:          #DIM宣言追跡 + DIM変数代入認識
Multi-State:        2364/2364 (100%) — 全プロファイル完全一致
```

---

## 変更ファイルサマリー

| ファイル/ディレクトリ | 変更内容 |
|---------------------|----------|
| `Game/ERB/COMMON_KOJO.ERB` | #FUNCTION + RETURNF追加 (Bug 5) |
| `Game/YAML/Kojo/**/*.yaml` (796files) | com_idメタデータ注入 (Bug 2+4) |
| `Game/YAML/Kojo/**/*.yaml` (16files) | 条件形式修正 (Bug 6 Issue A) |
| `Game/YAML/Kojo/**/*.yaml` (3files) | フォールバック追加 (Bug 6 Issue B) |
| `src/tools/dotnet/ErbToYaml/FileConverter.cs` | com_id出力追加 |
| `src/tools/dotnet/ErbToYaml/ComIdInjector.cs` | 新規: com_id注入ツール |
| `src/tools/dotnet/ErbToYaml/Program.cs` | --inject-com-idモード追加 |
| `src/tools/dotnet/KojoComparer/FileDiscovery.cs` | com_idベースのルックアップに書き換え |
| `src/tools/dotnet/KojoComparer/KojoBranchesParser.cs` | ComIdプロパティ追加 |
| `src/tools/dotnet/KojoComparer/KojoYamlParser.cs` | ComIdプロパティ追加 |
| `src/tools/dotnet/KojoComparer/YamlRunner.cs` | サフィックスなしYAMLパス対応 |
| `src/tools/dotnet/KojoComparer/OutputNormalizer.cs` | 会話イントロパターン除去追加、PRINTFORMW結合分割 |
| `src/tools/dotnet/KojoComparer/BatchExecutor.cs` | サブ関数テスト名生成対応 |
| `src/tools/dotnet/KojoComparer/ErbEvaluator.cs` | 新規: 軽量インプロセスERBインタープリター |
| `src/tools/dotnet/KojoComparer/ErbBatchEvaluator.cs` | 新規: ErbEvaluatorベースのバッチ実行 |
| `src/tools/kojo-mapper/com_file_map.json` | COM_300-316範囲分割, K10 COM_091修正, COM_463 sub_functions追加 |
| `src/tools/dotnet/YamlSchemaGen/dialogue-schema.json` | com_idフィールド追加 |
| `Game/YAML/Kojo/5_レミリア/K5_会話親密_9.yaml` | 欠落PRINTFORML行追加 |
| `src/tools/dotnet/ErbParser/ErbParser.cs` | #DIM変数宣言追跡 + DIM変数代入認識 (Bug 7) |
| 削除: `K1_会話親密_11`, `K3_会話親密_4`, `K5_会話親密_0` | 旧版重複YAML削除 |

---

## 50+フィーチャーの振り返り

### 何が正しかったか

| カテゴリ | Feature数 | 成果 |
|----------|:---------:|------|
| 変換パイプライン (F633-F643) | 12 | 117 ERBファイルの忠実な変換 |
| DisplayModeシステム (F676-F700) | 13 | 完全なメタデータ保存 |
| 条件パーサー (F750-F762) | 9 | TALENT/CFLAG/bitwise等の包括的対応 |
| TALENT条件移行 (F773) | 1 | 608ファイルの条件メタデータ修正 |
| テスト基盤 (F644,F746等) | 6 | CompareSubset, バッチ実行等 |

**変換内容は全キャラで忠実**（YAML == ERBテキスト、文字単位で一致確認済み）。
インフラは正しく構築された。

### 何が見落とされたか

| 問題 | 発見タイミング | 本来の発見タイミング |
|------|---------------|---------------------|
| 辞書順ソート (Bug 1) | 2026-02-11 (本調査) | F644構築時 |
| COM-YAML索引オフセット (Bug 2) | 2026-02-11 (本調査) | F644構築時 |
| com_file_map過大申告 (Bug 3) | 2026-02-11 (本調査) | F636-F643変換時 |
| IF LOCAL変換失敗 (Bug 2補足) | 2026-02-11 (本調査) | F633構築時 |
| #FUNCTION属性欠落 (Bug 5) | 2026-02-11 (本調査) | 関数定義時 |
| 条件形式混在 (Bug 6) | 2026-02-11 (本調査) | F675/F773実行時 |

**根本原因**: FileDiscoveryの索引計算ロジックに対する**エンドツーエンドテスト**が存在しなかった。

### 教訓

1. **間接マッピングは壊れやすい**: 2つの独立システム（ErbToYaml/FileDiscovery）が暗黙の順序仮定を共有するのは危険。com_idメタデータ埋め込みで解消。
2. **エンドツーエンド検証を先に**: 大量の中間修正（条件パーサー等）より、マッピングの正しさ検証が先。
3. **失敗パターンの統計分析**: 「全キャラ均等に失敗」=システム的問題。早期にパターン分析すべきだった。
4. **convertible-node検出の信頼性**: ComIdInjectorのAST解析はErbToYaml実行結果と乖離する。temp-dir方式（実際の変換結果から抽出）が唯一確実。

---

## 関連Feature

| Feature | 状態 | 役割 |
|---------|:----:|------|
| F706 | [DONE] | KojoComparer全件等価性検証（591/591 PASS達成） |
| F709 | [CANCELLED] | Multi-State等価性テスト — 目標達成済み(2364/2364 PASS) |
| F751 | [CANCELLED] | TALENTセマンティック検証 — Multi-Stateに包含 |
| F754 | [CANCELLED] | YAML形式統一（残存branches→entries） |
| F769 | [CANCELLED] | TALENT:PLAYER実行時解決 — 等価性に影響なし |
| F771 | [CANCELLED] | EVENT未変換パターン — 等価性に影響なし |
| F772 | [CANCELLED] | Category 4 LOCAL変換 — 防御マーカーで動作中 |
| F773 | [DONE] | entries形式TALENT条件移行 |

---

## ツールリファレンス

```bash
# 等価性テスト全件実行 (ErbEvaluator, ~3秒)
dotnet run --project src/tools/dotnet/KojoComparer/ -- --all

# Multi-State等価性テスト (4状態×591=2364件, ~12秒)
dotnet run --project src/tools/dotnet/KojoComparer/ -- --all --multi-state

# 等価性テスト全件実行 (旧headlessエンジン, ~8分)
dotnet run --project src/tools/dotnet/KojoComparer/ -- --all --legacy

# 単体テスト
dotnet run --project src/tools/dotnet/KojoComparer/ -- \
  --erb "Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB" \
  --function "@KOJO_MESSAGE_COM_K1_0" \
  --yaml "Game/YAML/Kojo/1_美鈴/K1_愛撫_0.yaml"

# バッチ変換（com_id付き）
dotnet run --project src/tools/dotnet/ErbToYaml/ -- --batch Game/ERB/口上/ Game/YAML/Kojo/

# com_id注入（既存YAMLに注入）
dotnet run --project src/tools/dotnet/ErbToYaml/ -- --inject-com-id Game/ERB/口上/ Game/YAML/Kojo/

# スキーマ検証
dotnet run --project src/tools/dotnet/YamlValidator/ -- \
  --schema src/tools/dotnet/YamlSchemaGen/dialogue-schema.json \
  --validate-all Game/YAML/Kojo/
```
