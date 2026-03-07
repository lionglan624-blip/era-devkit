# Modification Notice / 改変に関する表示

本プロジェクトは複数のソースコードを改変・統合した派生著作物です。

---

## 1. uEmuera (Apache License 2.0)

**原著作物**: uEmuera - Unity port of Emuera
**著作権**: Copyright 2018 xerysherry (https://github.com/xerysherry/uEmuera)
**ライセンス**: Apache License 2.0 (`engine/LICENSE`)

uEmuera は Emuera (MinorShift) の Unity 移植版であり、Emuera のコードを内包しています。
本プロジェクトは uEmuera をフォークし改変しています（Emuera を独立して取り込んではいません）。

**改変内容 (Section 4(b) notice)**:
xerysherry による最終コミット (cb66a45) 以降、`engine/Assets/Scripts/` 配下の
約356ファイルに対し 27,260行の追加・3,803行の削除を行っています。主な改変:

- **Headless モード追加**: テスト用ヘッドレス実行基盤 (`Emuera/Headless/`, ~32ファイル)
- **コマンドリファクタリング**: モジュラーコマンドアーキテクチャ (`GameProc/Commands/`, ~40ファイル)
- **サービスレイヤー追加**: DI、エラーハンドリング、ダッシュボード連携 (`Services/`, ~30ファイル)
- **GlobalStatic リファクタリング**: テスタビリティ向上のための中央状態管理改修
- **ビルド環境**: .NET 10 / C# 14、Unity 6000.3.1f1 へ移行
- **既存ファイル改変**: Config, Variable, Parser, GameProcess, UI 等 ~40ファイル

完全な変更履歴は `engine/` サブモジュールの git log で確認できます。

### 1.1 Emuera (zlib-style License) — uEmuera 経由で波及

**原著作物**: Emuera - Emulator of Eramaker
**著作権**: Copyright (C) 2008-2013 MinorShift, 妊）|д゜)の中の人
**ライセンス**: zlib-style (`docs/game/legacy/license@Emuera.txt`)

Emuera のコードは uEmuera に内包されており、本プロジェクトの engine/ にも含まれています。
上記 Section 1 の改変は Emuera 由来コードにも及んでいます。

---

## 2. era紅魔館protoNTR (zlib-style License)

**原著作物**: era紅魔館protoNTR Ver.0.039
**著作権**: Copyright (C) 2008-2015 MinorShift, 妊）|дﾟ)の中の人
**ライセンス**: zlib-style (`docs/game/legacy/license.txt`)
**原典保存**: `Game/archive/original-source/`

### 2.1 システム ERB

ゲーム機構を担うスクリプト群 (COMMON.ERB, SYSTEM.ERB, NTR.ERB, INFO.ERB, DIM.ERH 等)。

**改変内容**:
- NTR.ERB をモノリスから14ファイルのモジュール構成へ分割 (`Game/ERB/NTR/`)
- イベント処理、ギフト、移動、戦闘等の各種 ERB ファイルを改変 (~86ファイル)
- CSV データを YAML 形式へ移行 (`Game/data/`, ~190 YAML ファイル)

### 2.2 口上 (kojo) コンテンツ

キャラクター別対話ファイル (`Game/ERB/口上/`)。

**原典** (Ver.0.039): ERB 55ファイル → **現行**: ERB 117ファイル + YAML ~1128ファイル

#### 出自分類

| 分類 | ERBファイル数 | 説明 |
|------|:---:|------|
| **NEW（新規作成）** | 86 | 原典に存在しない。本プロジェクトで新規に書いた口上 |
| **MODIFIED（既存改変）** | 27 | 原典の NTR口上, お持ち帰り, WC系口上 等を改変 |
| **SAME（未変更）** | 14 | SexHara休憩中口上, NTR口上_野外調教 等。原典のまま |
| REMOVED（原典のみ） | 24 | 対あなた口上, _NTR拡張 等。分割・再構成により消滅 |

**分類方法**: 原典 (`Game/archive/original-source/`) と現行 (`Game/ERB/口上/`) の
ファイル存在比較および内容比較による。YAML ファイルは全て本プロジェクトの新規成果物。

**出力レベルの等価性検証**: `tools/KojoComparer` に `--origin` フラグを追加することで、
原典ERBと現行YAMLのゲーム出力を直接比較し、改変の有無を出力レベルで機械的に判定できる。
原典ERBと現行ERBは同一の関数命名規則 (`@KOJO_MESSAGE_COM_K{N}_{comId}`) を使用しており、
ErbEvaluator のクロスファイル検索によりファイル構成の差異（モノリス→分割）を吸収できる。

**作者について**:
各口上の原作者は era 界隈の匿名投稿文化により個別の特定が困難です。
統合パッチの一覧は `docs/game/legacy/外部パッチ2.txt` を参照してください。

---

## 3. Era.Core (移植コード)

`Era.Core/` は本プロジェクトの新規ライブラリですが、以下の移植コードを含みます:

| 移植元 | 性質 | Era.Core 内の対象 | 波及ライセンス |
|--------|------|------------------|---------------|
| engine/ (uEmuera 内 Emuera コード) | エンジン | `Functions/`, `Expressions/`, `Variables/` | zlib + Apache 2.0 |
| Game/ERB/ (システム ERB) | システム | `Common/`, `State/`, `Training/`, `Orchestration/` | zlib |

各移植ファイルにはソースコメントで移植元が記載されています。

---

## 4. 表示不要のコード

以下は本プロジェクトの完全新規コードであり、上記ライセンスの改変表示義務の対象外です:

- `src/tools/` — 各種開発ツール (ErbParser, KojoComparer, YamlValidator 等)
- `src/tools/dotnet/*Tests/` — テストコード (engine.Tests, Era.Core.Tests は各リポに移動済み)
- `pm/`, `.claude/` — 開発ドキュメント・ワークフロー定義

---

## ライセンスファイル一覧

| コンポーネント | ライセンスファイル |
|---------------|-------------------|
| uEmuera | `engine/LICENSE` (Apache 2.0) |
| Emuera | `docs/game/legacy/license@Emuera.txt` (zlib-style) |
| era紅魔館protoNTR | `docs/game/legacy/license.txt` (zlib-style) |

---

これはオリジナルの Emuera / uEmuera / era紅魔館protoNTR ではありません。
問題を原作者に報告しないでください。
