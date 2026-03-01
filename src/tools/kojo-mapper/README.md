# kojo-mapper

ERB口上（dialogue）のカバレッジ分析ツール。

## 概要

口上ファイルを解析し、以下のメトリクスを可視化する:

- 関数数（キャラクター別、シーンタイプ別）
- 関係性分岐タイプ（TALENT_4, TALENT_3, ABL, NTR等）
- AC準拠スコア（Acceptance Criteria準拠率）
- 好感度カバレッジ（NTR_CHK_FAVORABLY分岐）
- 特殊条件分岐（処女、公衆便所、恋慕等）

## ファイル構成

```
kojo-mapper/
├── kojo_mapper.py      # 単一キャラクター解析
├── kojo_dashboard.py   # 全キャラクターダッシュボード生成
├── kojo-dashboard.html # 生成されたダッシュボード
└── kojo-map-*.md       # キャラクター別詳細レポート
```

## 使用方法

### 単一キャラクター解析

```bash
cd tools/kojo-mapper
python kojo_mapper.py <character_dir> [output_dir]

# 例: 美鈴の解析
python kojo_mapper.py ../../Game/ERB/口上/1_美鈴 .
```

**出力**: `kojo-map-{キャラ名}.md`

### 全キャラクターダッシュボード

```bash
cd tools/kojo-mapper
python kojo_dashboard.py
```

**出力**: `kojo-dashboard.html`（Chart.jsによるインタラクティブグラフ）

### スラッシュコマンド

```
/kojo-map
```

Claude Code から実行可能。自動で `kojo_dashboard.py` を実行。

## 出力メトリクス

### AC準拠スコア

| 分岐タイプ | 重み | 説明 |
|-----------|:----:|------|
| TALENT_4 | 100% | 恋人/恋慕/思慕/ELSE の4段階分岐 |
| TALENT_3 / ABL_3+ | 75% | 3段階分岐 |
| TALENT_1 / ABL_1-2 | 25% | 1-2段階分岐 |
| NONE | 0% | 分岐なし |

**計算式**: `(TALENT_4 × 100 + TALENT_3 × 75 + その他 × 25) / 総関数数`

### シーンタイプ

ファイル名から自動検出:

| ファイルパターン | シーンタイプ |
|-----------------|-------------|
| `NTR口上_お持ち帰り` | お持ち帰り |
| `NTR口上_野外調教` | 野外調教 |
| `NTR口上` | NTR基本 |
| `WC系口上.ERB` | WC系 |
| `対あなた口上.ERB` | 対あなた |
| `_会話親密` | 会話親密 |
| `_愛撫` | 愛撫 |
| `_口挿入` | 口挿入 |
| `_日常` | 日常 |
| `_EVENT` | EVENT |
| `KOJO_K` | 基本口上 |

### 分岐検出

以下のパターンを検出:

- **TALENT分岐**: `TALENT:恋人`, `TALENT:恋慕`, `TALENT:思慕`
- **ABL分岐**: `ABL:親密 <= N`
- **NTR分岐**: `NTR_CHK_FAVORABLY(_, FAV_*)`
- **バリエーション**: `PRINTDATA/DATALIST`, `SELECTCASE RAND:`, `IF RAND:`

### 口上行数計測

| 計測対象 | 説明 |
|---------|------|
| PRINTFORM行 | `PRINTFORM`, `PRINTFORML`, `PRINTFORMW` |
| DATAFORM行 | PRINTDATAブロック内のDATAFORM |
| 分岐あたり行数 | (口上行数) / (IF/ELSEIF/ELSEブロック数) |

**目標**: 4行以上/分岐（kojo-reference.md準拠）

## Markdownレポート例

```markdown
# 美鈴 口上マップ

総関数数: **289**

## AC準拠スコア

├── 4段階分岐 (TALENT_4):   15 (5%)
├── 3段階分岐 (TALENT/ABL):  25 (9%)
├── 1-2段階分岐:            50 (17%)
├── 分岐なし:              199 (69%)
├── 平均口上行数:          2.5行/分岐 (目標4+行)
└── ELSE分岐あり:           40 (14%)

**AC準拠率（推定）**: ~15%

## シーンタイプ別

| シーン | 関数数 | 好感度カバー |
|--------|--------|--------------|
| 基本口上 | 150 | 3/8 (37%) |
| NTR基本 | 80 | 6/8 (75%) |
...
```

## ダッシュボード機能

`kojo-dashboard.html` には以下のチャートを含む:

1. **総関数数比較** - 棒グラフ
2. **AC準拠スコア** - 色分け棒グラフ（緑/黄/赤）
3. **関係性分岐タイプ** - 積み上げ棒グラフ
4. **バリエーション・ELSE分岐** - 棒グラフ
5. **平均口上行数** - 棒グラフ（目標ライン付き）
6. **シーンタイプ別分布** - 積み上げ/レーダー/円グラフ
7. **特殊条件分岐** - 積み上げ/レーダー/円グラフ
8. **サマリーテーブル** - 全メトリクス一覧

## 依存関係

- Python 3.10+
- 外部ライブラリ不要（標準ライブラリのみ）
- ダッシュボード表示: モダンブラウザ + CDN経由Chart.js

## 関連ドキュメント

- [kojo-reference.md](../../docs/reference/kojo-reference.md) - 口上システム統合リファレンス
- [com-map.md](../../docs/reference/com-map.md) - COMマップ（コマンド一覧）

## 更新履歴

- 2025-12-14: Feature 055 - 分岐あたりPRINTFORM計算に変更
- 2025-12-14: COMカテゴリファイル対応追加
- 2025-12-13: Feature 051 - AC準拠度測定機能追加
- 2025-12-10: 初期作成
