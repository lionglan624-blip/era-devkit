# Feature 254: kojo-mapper COM Progress Report

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
kojo-mapper は SSOT として Phase 8 Summary を完全代替すべき。F251 で手動テーブルを削除したが、旧テーブルが持っていた「COM 番号別 Done/Remaining」情報は kojo-mapper --coverage では出力されない。

### Problem (Current Issue)
- kojo-mapper --coverage は「カテゴリ別関数数」を出力（COM, NTR_EVENT, DAILY等）
- 旧 Phase 8 Summary は「COM 番号範囲別 Done/Remaining」を表示していた
- 現状では COM 番号範囲別の進捗を一目で確認する手段がない

**NOTE**: --progress は --coverage とは異なる新しい出力モードとして実装する。

### Goal (What to Achieve)
1. kojo-mapper に --progress オプションを追加
2. COM 番号別の Done/Remaining を表示
3. content-roadmap.md の COM 定義と照合して進捗率を算出

### Session Context
- **Origin**: F251 完了後の課題として発生
- **Dependencies**: F251 (Phase 8 Summary 削除済み)
- **Related**: F166/F167 (kojo-mapper 14カテゴリ)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | --progress オプション追加 | output | bash | contains | "--progress" | [x] |
| 2 | COM 範囲別進捗表示 | output | bash | contains | "Done" | [x] |
| 3 | COM 範囲別進捗表示 | output | bash | contains | "Remaining" | [x] |
| 4 | 0-11 愛撫系カウント | output | bash | matches | "0-11.*愛撫系.*12" | [x] |
| 5 | 60-72 挿入系カウント | output | bash | matches | "60-72.*挿入系.*13" | [x] |
| 6 | 合計行表示 | output | bash | contains | "Total" | [x] |
| 7 | 進捗率パーセント表示 | output | bash | matches | "\\d+%" | [x] |
| 8 | ヘルプに --progress 説明追加 | output | bash | contains | "COM progress" | [x] |
| 9 | 出力ヘッダー確認 | output | bash | contains | "COM Progress Report" | [x] |

### AC Details

**NOTE**: Type: infra のため、Method は bash コマンド実行を示す（testing SKILL の ERB 用メソッドではない）。

**AC1**: kojo-mapper.py に --progress オプションを追加
```bash
python tools/kojo-mapper/kojo_mapper.py --help
```
出力に `--progress` が含まれること

**AC2-AC3**: --progress 実行時に Done/Remaining カラムが表示されること
```bash
python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --progress
```

**AC4**: 0-11 愛撫系の COM 数が正確にカウントされること（12個）

**AC5**: 60-72 挿入系の COM 数が正確にカウントされること

**AC6**: 合計行が表示されること

**AC7**: 各行に進捗率（パーセント）が表示されること

**AC8**: --help に --progress の説明が追加されていること

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,8 | kojo_mapper.py に --progress 引数を追加（argparse） | [x] |
| 2 | 2,3,9 | COM 範囲別の Done/Remaining 計算ロジック実装 | [x] |
| 3 | 4,5 | COM 範囲とカテゴリをハードコードし進捗計算ロジック実装 | [x] |
| 4 | 6 | 合計行の出力実装 | [x] |
| 5 | 7 | 進捗率パーセント計算と表示 | [x] |

<!-- AC:Task consolidation rationale:
- Task 1 covers AC1+AC8: argparse addition includes help text in single operation
- Task 2 covers AC2+AC3+AC9: Done/Remaining headers and report header displayed together
- Task 3 covers AC4+AC5: same range-category mapping logic for all ranges
-->

---

## Technical Notes

### 出力フォーマット案

**NOTE**: --coverage は plain text 形式だが、--progress は Markdown テーブル形式を採用する。理由: 進捗レポートは index-features.md などへの貼り付けを想定し、視認性を優先する。

```
=== COM Progress Report ===

| Range | Category | COM Count | Done | Remaining | Progress |
|-------|----------|:---------:|:----:|:---------:|:--------:|
| 0-11 | 愛撫系 | 12 | 12 | 0 | 100% |
| 20-21 | 会話親密系 | 2 | 2 | 0 | 100% |
| 40-48 | 道具系 | 9 | 1 | 8 | 11% |
| 60-72 | 挿入系 | 13 | 4 | 9 | 31% |
| 80-85 | 奉仕系 | 6 | 0 | 6 | 0% |
| 90-99 | 受け身系 | 10 | 0 | 10 | 0% |
| **Total** | | **52** | **19** | **33** | **37%** |
```

### COM 定義ソース

COM 範囲とカテゴリ名は kojo_mapper.py 内にハードコードする。

**COM Range-to-Category Mapping (SSOT for hardcoded values)**:

| Range | 系表記 | Category | Count |
|-------|-------|----------|:-----:|
| 0-11 | 0系 | 愛撫系 | 12 |
| 20-21 | 20系 | 会話親密系 | 2 |
| 40-48 | 40系 | 道具系 | 9 |
| 60-72 | 60系 | 挿入系 | 13 |
| 80-85 | 80系 | 奉仕系 | 6 |
| 90-99 | 90系 | 受け身系 | 10 |

**Scope**: 本 Feature は v0.6/v0.8 スコープ（52 COM）を対象とする。全 150 COM の対応は将来の拡張として計画。

**NOTE**: 出力は Range 形式（0-11）を使用。ドキュメント（content-roadmap.md 等）は系表記（0系）を使用。

参照ソース:
- content-roadmap.md (v0.6 scope: 0系/20系/40系/60系)
- designs/v0.8-kojo-80-90.md (80-90系)

### Done 判定ロジック

既存の `KOJO_PATTERNS['COM']` パターン（`@KOJO_MESSAGE_COM_K(?:U|(\d+))(?:_(\d+))?`）を再利用する。

1. `Game/ERB/口上/` 配下の各キャラクターフォルダをスキャン
2. 既存の COM パターンでマッチした関数から COM 番号を抽出
3. COM 番号を上記 Range マッピングに照合
4. いずれかの K1-K10 または KU に該当関数が存在すれば Done
   - KU (共通口上) の関数は全キャラクター共有としてカウント
   - **Done 定義**: 少なくとも1キャラクター（または KU）に実装があれば Done。全10キャラ完了の厳密な追跡は将来の拡張で検討。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 06:50 | Feature Completed | finalizer | Mark [DONE], All ACs [x], All Tasks [x] | READY_TO_COMMIT |

---

## Links

- [feature-251.md](feature-251.md) - Phase 8 Summary 削除
- [feature-166.md](feature-166.md) - kojo-mapper 14カテゴリ
- [feature-167.md](feature-167.md) - kojo-mapper 14カテゴリ検証
- [content-roadmap.md](content-roadmap.md) - COM 定義
- [kojo-mapper](../../tools/kojo-mapper/)
