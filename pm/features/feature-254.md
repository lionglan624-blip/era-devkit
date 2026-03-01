# Feature 254: kojo-mapper COM Progress Report

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
kojo-mapper は SSOT として Phase 8 Summary を完�E代替すべき、E251 で手動チE�Eブルを削除したが、旧チE�Eブルが持ってぁE��「COM 番号別 Done/Remaining」情報は kojo-mapper --coverage では出力されなぁE��E

### Problem (Current Issue)
- kojo-mapper --coverage は「カチE��リ別関数数」を出力！EOM, NTR_EVENT, DAILY等！E
- 旧 Phase 8 Summary は「COM 番号篁E��別 Done/Remaining」を表示してぁE��
- 現状では COM 番号篁E��別の進捗を一目で確認する手段がなぁE

**NOTE**: --progress は --coverage とは異なる新しい出力モードとして実裁E��る、E

### Goal (What to Achieve)
1. kojo-mapper に --progress オプションを追加
2. COM 番号別の Done/Remaining を表示
3. content-roadmap.md の COM 定義と照合して進捗率を算�E

### Session Context
- **Origin**: F251 完亁E���E課題として発甁E
- **Dependencies**: F251 (Phase 8 Summary 削除済み)
- **Related**: F166/F167 (kojo-mapper 14カチE��リ)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | --progress オプション追加 | output | bash | contains | "--progress" | [x] |
| 2 | COM 篁E��別進捗表示 | output | bash | contains | "Done" | [x] |
| 3 | COM 篁E��別進捗表示 | output | bash | contains | "Remaining" | [x] |
| 4 | 0-11 愛撫系カウンチE| output | bash | matches | "0-11.*愛撫系.*12" | [x] |
| 5 | 60-72 挿入系カウンチE| output | bash | matches | "60-72.*挿入系.*13" | [x] |
| 6 | 合計行表示 | output | bash | contains | "Total" | [x] |
| 7 | 進捗率パ�Eセント表示 | output | bash | matches | "\\d+%" | [x] |
| 8 | ヘルプに --progress 説明追加 | output | bash | contains | "COM progress" | [x] |
| 9 | 出力�EチE��ー確誁E| output | bash | contains | "COM Progress Report" | [x] |

### AC Details

**NOTE**: Type: infra のため、Method は bash コマンド実行を示す！Eesting SKILL の ERB 用メソチE��ではなぁE��、E

**AC1**: kojo-mapper.py に --progress オプションを追加
```bash
python src/tools/kojo-mapper/kojo_mapper.py --help
```
出力に `--progress` が含まれること

**AC2-AC3**: --progress 実行時に Done/Remaining カラムが表示されること
```bash
python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --progress
```

**AC4**: 0-11 愛撫系の COM 数が正確にカウントされること�E�E2個！E

**AC5**: 60-72 挿入系の COM 数が正確にカウントされること

**AC6**: 合計行が表示されること

**AC7**: 吁E��に進捗率�E�パーセント）が表示されること

**AC8**: --help に --progress の説明が追加されてぁE��こと

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,8 | kojo_mapper.py に --progress 引数を追加�E�Ergparse�E�E| [x] |
| 2 | 2,3,9 | COM 篁E��別の Done/Remaining 計算ロジチE��実裁E| [x] |
| 3 | 4,5 | COM 篁E��とカチE��リをハードコードし進捗計算ロジチE��実裁E| [x] |
| 4 | 6 | 合計行�E出力実裁E| [x] |
| 5 | 7 | 進捗率パ�Eセント計算と表示 | [x] |

<!-- AC:Task consolidation rationale:
- Task 1 covers AC1+AC8: argparse addition includes help text in single operation
- Task 2 covers AC2+AC3+AC9: Done/Remaining headers and report header displayed together
- Task 3 covers AC4+AC5: same range-category mapping logic for all ranges
-->

---

## Technical Notes

### 出力フォーマット桁E

**NOTE**: --coverage は plain text 形式だが、E-progress は Markdown チE�Eブル形式を採用する。理由: 進捗レポ�Eト�E index-features.md などへの貼り付けを想定し、視認性を優先する、E

```
=== COM Progress Report ===

| Range | Category | COM Count | Done | Remaining | Progress |
|-------|----------|:---------:|:----:|:---------:|:--------:|
| 0-11 | 愛撫系 | 12 | 12 | 0 | 100% |
| 20-21 | 会話親寁E�� | 2 | 2 | 0 | 100% |
| 40-48 | 道�E系 | 9 | 1 | 8 | 11% |
| 60-72 | 挿入系 | 13 | 4 | 9 | 31% |
| 80-85 | 奉仕系 | 6 | 0 | 6 | 0% |
| 90-99 | 受け身系 | 10 | 0 | 10 | 0% |
| **Total** | | **52** | **19** | **33** | **37%** |
```

### COM 定義ソース

COM 篁E��とカチE��リ名�E kojo_mapper.py 冁E��ハ�Eドコードする、E

**COM Range-to-Category Mapping (SSOT for hardcoded values)**:

| Range | 系表訁E| Category | Count |
|-------|-------|----------|:-----:|
| 0-11 | 0系 | 愛撫系 | 12 |
| 20-21 | 20系 | 会話親寁E�� | 2 |
| 40-48 | 40系 | 道�E系 | 9 |
| 60-72 | 60系 | 挿入系 | 13 |
| 80-85 | 80系 | 奉仕系 | 6 |
| 90-99 | 90系 | 受け身系 | 10 |

**Scope**: 本 Feature は v0.6/v0.8 スコープ！E2 COM�E�を対象とする。�E 150 COM の対応�E封E��の拡張として計画、E

**NOTE**: 出力�E Range 形式！E-11�E�を使用。ドキュメント！Eontent-roadmap.md 等）�E系表記！E系�E�を使用、E

参�Eソース:
- content-roadmap.md (v0.6 scope: 0系/20系/40系/60系)
- designs/v0.8-kojo-80-90.md (80-90系)

### Done 判定ロジチE��

既存�E `KOJO_PATTERNS['COM']` パターン�E�E@KOJO_MESSAGE_COM_K(?:U|(\d+))(?:_(\d+))?`�E�を再利用する、E

1. `Game/ERB/口丁E` 配下�E吁E��ャラクターフォルダをスキャン
2. 既存�E COM パターンでマッチした関数から COM 番号を抽出
3. COM 番号を上訁ERange マッピングに照吁E
4. ぁE��れかの K1-K10 また�E KU に該当関数が存在すれば Done
   - KU (共通口丁E の関数は全キャラクター共有としてカウンチE
   - **Done 定義**: 少なくとめEキャラクター�E�また�E KU�E�に実裁E��あれば Done。�E10キャラ完亁E�E厳寁E��追跡は封E��の拡張で検討、E

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 06:50 | Feature Completed | finalizer | Mark [DONE], All ACs [x], All Tasks [x] | READY_TO_COMMIT |

---

## Links

- [feature-251.md](feature-251.md) - Phase 8 Summary 削除
- [feature-166.md](feature-166.md) - kojo-mapper 14カチE��リ
- [feature-167.md](feature-167.md) - kojo-mapper 14カチE��リ検証
- [content-roadmap.md](../content-roadmap.md) - COM 定義
- [kojo-mapper](../../src/tools/kojo-mapper/)
