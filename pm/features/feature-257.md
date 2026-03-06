# Feature 257: kojo-mapper --progress Phase別キャラ別COM進捗

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
kojo-mapper は SSOT として Phase 8 Summary を完全代替すべき（F254 Philosophy 継承）。

### Problem (Current Issue)
F254 で追加した `--progress` は範囲別サマリーのみ出力:
```
| Range | Category | Done | Remaining | Progress |
| 60-72 | 挿入系 | 12 | 1 | 92% |
```

これでは `/kojo-init` が「どの COM を作るか」判断できない:
- COM_64 が未実装と分からない
- K1-K10 のどのキャラが未実装か分からない
- Phase (C2/C3/C6) ごとの達成基準が考慮されていない

### Goal (What to Achieve)
`--progress` を Phase 別・キャラ別の達成判定に拡張し、`/kojo-init` が正確に未完了 COM を選択できるようにする。

### Session Context
- **Origin**: F254/F255 完了後のレビューで、--progress 出力が /kojo-init の要件を満たしていないことを指摘 (範囲別サマリーのみで COM 別・キャラ別の未実装情報がない)
- **Dependencies**: F254 (--progress 実装済み), F255 (ワークフロー統合済み)
- **Lesson**: F254→F255でツール出力と消費者(kojo-init)の要件ギャップが判明。出力設計時に消費者ユースケースを先に定義すべき。

---

## Design

### Architecture Flow

```
┌─────────────────────────────────────────────────────────────┐
│ /kojo-init 実行                                              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ Step 1: kojo_mapper.py --progress                            │
│                                                              │
│ 1. PHASE_REQUIREMENTS から current_phase (C2) 取得          │
│ 2. Phase 要件取得: branch_type="TALENT_4", patterns=1       │
│ 3. 各 COM × 各キャラ (K1-K10) をスキャン                     │
│ 4. Phase 要件達成判定:                                       │
│    ○ = func.branch_type == "TALENT_4"                       │
│    - = 未達成                                                │
│ 5. Done < 10 の COM を出力                                   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ 出力例                                                       │
│                                                              │
│ === Incomplete COMs (Phase: C2) ===                         │
│ Requirements: TALENT_4 (4-branch)                           │
│                                                              │
│ | COM | K1 | K2 | ... | K10 | Done  |                       │
│ |:---:|:--:|:--:|:---:|:---:|:-----:|                       │
│ | 64  | ○  | -  | ... | -   | 1/10  |                       │
│ | 90  | -  | -  | ... | -   | 0/10  |                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ Step 2: /kojo-init が COM 選択                               │
│                                                              │
│ - Done < 10 の COM から優先度順に選択                        │
│ - feature-{ID}.md 作成                                       │
└─────────────────────────────────────────────────────────────┘
```

### PHASE_REQUIREMENTS (kojo_mapper.py 内定義)

```python
# kojo_mapper.py に追加
PHASE_REQUIREMENTS = {
    "C2": {"branch_type": "TALENT_4", "patterns": 1},
    "C3": {"branch_type": "TALENT_4", "patterns": 4},
    "C6": {"branch_type": "FAV_9", "patterns": 9},
}
CURRENT_PHASE = "C2"  # Phase 移行時に /plan で更新
```

**Design Decision**: F254 の COM_RANGES 同様、Phase 要件を kojo_mapper.py にハードコード。外部ファイル (YAML) や agent を不要とし、ツールを self-contained に維持。

**Phase Mapping**: Phase 8d = C2 = TALENT_4 分岐要件。content-roadmap.md の "1 pattern" を "TALENT_4 分岐あり" と解釈。

**Scope Note**: 本 Feature は C2 (TALENT_4) のみ実装。C6 (FAV_9) 対応は Phase 移行時に別 Feature で対応。

**FAV_9 Placeholder**: C6 の `branch_type: FAV_9` は現在の kojo_mapper.py に存在しないプレースホルダー。実装時には `NTR_9` (既存の `NTR_{depth}` 命名規則) または新規 FAV_9 検出ロジックを追加。本 Feature では C2 のみ使用するため影響なし。

**Phase Transition**: CURRENT_PHASE は kojo_mapper.py 内で手動更新。Phase 移行タイミングは content-roadmap.md の Phase 定義に基づき、プロジェクトメンテナが `/plan` 実行時に判断。更新方法: kojo_mapper.py の `CURRENT_PHASE = "C2"` を次の Phase（例: "C3"）に変更。

**patterns フィールド**: `patterns` は C3 以降で使用 (has_variation 判定)。C2 では `branch_type` のみで判定し、`patterns` は参照されない。

### 判定ロジック

**実装対象**: `calculate_com_progress()` を拡張して、COM×Character ごとに Phase 要件を判定する。

**現在の実装** (F254): COM function の存在チェックのみ (関数名パターンマッチ)。Done = 少なくとも1つ実装あり (全キャラの追跡なし)。
**拡張後** (F257): branch_type による Phase 要件達成判定。Done = Phase 要件を満たすキャラ数 (0-10)。
**Semantics Change**: F254 の Done (any implementation) → F257 の Done (per-character count)。

**Output Format Transition**: F257 --progress は F254 の範囲別サマリーに加えて、Incomplete COMs セクションを追加出力する。F254 の出力形式は維持し、新しい COM×Character マトリクスを追加。

**実装方法**:
1. `PHASE_REQUIREMENTS` と `CURRENT_PHASE` から Phase 要件を取得
2. `calculate_com_progress()` をリファクタ:
   - 各 ERB ファイルに対して `parse_erb_file()` を呼び出し
   - KojoFunction オブジェクト (branch_type 含む) を取得
   - COM 番号とキャラ ID (K1-K10) でグループ化
3. 各 KojoFunction の `branch_type` を取得
4. `is_phase_complete()` で Phase 要件判定
5. ○ / - を決定

```python
def is_phase_complete(func: KojoFunction, phase_req: dict) -> bool:
    """
    Phase 要件を満たしているか判定

    Note: calculate_com_progress() 内で COM×Character ごとに呼び出される。
    func は parse_kojo_file() で解析された KojoFunction オブジェクト。
    """
    required_branch = phase_req.get("branch_type", "TALENT_4")
    required_patterns = phase_req.get("patterns", 1)

    # C2: TALENT_4 分岐があればOK
    if required_branch == "TALENT_4":
        if func.branch_type != "TALENT_4":
            return False

    # C3: さらに4パターン (has_variation) が必要
    if required_patterns >= 4:
        if not func.has_variation:
            return False

    return True
```

**COM×Character 抽出方法**:
1. **COM 番号・キャラ ID 抽出**: 関数名パターン `@KOJO_MESSAGE_COM_K{N}_{COM}` から両方を同時に抽出
   - **Pattern**: `@KOJO_MESSAGE_COM_K(?:U|(\d+))(?:_(\d+))?` (既存 KOJO_PATTERNS['COM'] と同等)
   - 例: `@KOJO_MESSAGE_COM_K7_60` → K7, COM_60
   - 例: `@KOJO_MESSAGE_COM_KU_64` → KU, COM_64
   - KU は全キャラに適用されるため特別処理（下記の KU 処理セクション参照）
2. **グループ化**: 抽出した (COM_num, char_id) タプルで KojoFunction をグループ化
   - データ構造: `Dict[Tuple[int, str], List[KojoFunction]]`
   - 例: `{(60, "K7"): [func1, func2], (64, "KU"): [func3]}`

**判定フロー**:
1. `calculate_com_progress()` が各 ERB ファイルを `parse_erb_file()` で解析
2. 返された KojoFunction オブジェクトから (COM_num, char_id) を抽出してグループ化
3. 各 (COM, Character) グループに対して: `any(is_phase_complete(func, phase_req) for func in group)`
4. 結果を ○ / - で出力（UTF-8 ターミナル前提、下記 Note 参照）

**集約ルール**: 同一 COM×Character に複数の function がある場合、ANY が Phase 要件を満たせば ○ とする。
（例: COM_64 に K1 美鈴の function が 2 つあり、1 つが TALENT_4 なら ○）

**KU (Universal) 処理**: KU function は全 K1-K10 に適用される。
- KU が TALENT_4 なら、K1-K10 全カラムに ○ を表示（10 カラム全て ○）
- KU + 個別キャラ function がある場合も ANY ルール適用
- 出力例: `| 64 | ○ | ○ | ○ | ○ | ○ | ○ | ○ | ○ | ○ | ○ | 10/10 |` (KU のみで全キャラ達成)
- **Semantics**: KU-complete (10/10) の COM は Incomplete リストから除外される。これは意図的な動作：KU が全キャラをカバーしているため、個別キャラ実装は不要と判断される。/kojo-init はこれらの COM をスキップして次の未完了 COM を選択する。
- **KU Not Meeting Phase**: KU が Phase 要件を満たさない場合（例: TALENT_4 要件に対して KU が NONE 分岐）、KU は無視されて K1-K10 の個別関数が評価される。KU のみで個別 Kx 関数がない場合、全キャラが - 表示 (0/10)。

**Migration Notes** (F254 → F257):
- **現在の実装**: `calculate_com_progress()` は `implemented_coms: Set[int]` を使用 (COM 単位のみ追跡、関数名のみ解析)
- **F257 要件**: COM × Character マトリクス (例: `Dict[int, Dict[str, bool]]`) への変更が必要
  - **Key Types**: outer key = COM 番号 (int: 0-311), inner key = キャラ ID (str: 'K1'-'K10' or 'KU'), value = Phase 達成 (bool)
- **互換性**: 既存の範囲別サマリー出力は維持し、新しい Incomplete COMs セクションを追加
- **実装変更**: 関数名パターンマッチ → parse_erb_file() による完全解析
  1. `directory.rglob('*.ERB')` で ERB ファイルを列挙
  2. 各ファイルに対して `parse_erb_file()` を呼び出し `List[KojoFunction]` を取得
  3. 各 KojoFunction から: func.name で COM#/char_id を抽出、func.branch_type で Phase 要件を判定
  4. 結果を `Dict[int, Dict[str, bool]]` に集約

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | --progress が current_phase を表示 | output | bash | contains | "Phase: C2" | [x] |
| 2 | --progress がキャラ別カラム出力 | output | bash | contains | "K1" | [x] |
| 3a | Incomplete リストに未完了 COM が含まれる | output | bash | matches | `[0-9]/10` | [x] |
| 3b | 完了 COM (10/10) が Incomplete リストから除外 | output | bash | not_contains | "10/10" | [x] |
| 4 | kojo-init.md が --progress を参照 | code | Grep | contains | "--progress" | [x] |

### AC Details

**Note (Type: infra)**: Method 列は検証ツール (grep/bash) を示す。erb/engine features の --unit/--inject とは異なる。

**AC1**: `--progress` 出力に現在の Phase が表示されること
- Test: `python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --progress`
- Expected output contains:
```
=== Incomplete COMs (Phase: C2) ===
Requirements: TALENT_4 (4-branch)
```

**AC2**: 各 COM のキャラ別達成状況が表示されること
- Test: `python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --progress`
- Expected output contains:
```
| COM | K1 | K2 | K3 | K4 | K5 | K6 | K7 | K8 | K9 | K10 | Done |
```

**AC3a**: Incomplete リストに未完了 COM が含まれること
- Note: `matches` で Done 列に `/10` パターン（例: `0/10`, `5/10`）が存在することを検証。これにより、Incomplete COMs セクションが正しく出力されることを確認。

**AC3b**: 完了 COM (10/10) が Incomplete リストから除外されること
- Method: `bash (grep -A extraction)` - Incomplete COMs セクションのみを抽出して検証
- Note: `not_contains` で "10/10" が Incomplete COMs セクションに存在しないことを検証。Done < 10 フィルタリングが正しく機能していることを確認。
- **Scope**: テスト対象は `=== Incomplete COMs` 以降のセクションのみ。F254 範囲サマリー（100% 完了範囲を含む可能性あり）は対象外。

**AC4**: `/kojo-init` が `--progress` 出力を参照して COM を選択すること
- Note: 本 AC は kojo-init.md に `--progress` への参照が存在することを検証。
- **Additive Output**: F257 の出力形式は F254 の範囲別サマリーに COM×Character マトリクスを追加する形式。既存の kojo-init.md パース処理（範囲サマリーの Done 列参照）は継続利用可能。新しいマトリクス形式のパース実装は kojo-init.md の将来拡張として scope 外。
- **Consumer Use Case**:
  - **現在 (kojo-init.md)**: 範囲サマリーの Done 列で "Done < COM_COUNT" の範囲を特定 → 任意の COM を選択。F257 マトリクスは参照しない。
  - **将来 (scope 外)**: マトリクスをパースして "Done < 10" の COM を直接選択可能。
  - **人間向け**: マトリクス出力は開発者がキャラ別進捗を視覚的に確認するために有用（/kojo-init automation なしでも価値あり）。

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo_mapper.py に PHASE_REQUIREMENTS/CURRENT_PHASE 定数を追加し、--progress で current_phase を表示 | [x] |
| 2 | 2 | kojo_mapper.py --progress でキャラ別カラム出力 (K1-K10)。calculate_com_progress() を Set[int] から Dict[int, Dict[str, bool]] に変更 | [x] |
| 3a | 3a | Incomplete リストに /10 パターンで未完了 COM を出力 | [x] |
| 3b | 3b | Incomplete リストから 10/10 完了 COM を除外（フィルタリング） | [x] |
| 4 | 4 | kojo-init.md に --progress 参照を維持（出力形式は additive、既存パースは継続利用可能） | [x] |

---

## Technical Notes

### 期待出力例

```
=== COM Progress Report ===

| Range | Category | COM Count | Done | Remaining | Progress |
|-------|----------|:---------:|:----:|:---------:|:--------:|
| 0-11 | 愛撫系 | 12 | 12 | 0 | 100% |
| 60-72 | 挿入系 | 13 | 12 | 1 | 92% |
| 90-99 | 受け身系 | 10 | 0 | 10 | 0% |
| **Total** | | **52** | **41** | **11** | **78%** |

=== Incomplete COMs (Phase: C2) ===
Requirements: TALENT_4 (4-branch)

| COM | K1 | K2 | K3 | K4 | K5 | K6 | K7 | K8 | K9 | K10 | Done |
|:---:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:---:|:----:|
| 64 | ○ | - | - | - | - | - | - | - | - | - | 1/10 |
| 90 | - | - | - | - | - | - | - | - | - | - | 0/10 |
| 91 | - | - | - | - | - | - | - | - | - | - | 0/10 |
...
```

**Note**: Name 列は省略。COM 番号と対応するアクションは content-roadmap.md 参照。

**UTF-8 Requirement**: 出力の ○/- シンボルは非 ASCII 文字。UTF-8 対応ターミナルで実行すること。パイプ出力時はエンコーディングに注意。
- **Windows**: PowerShell または Git Bash 推奨。cmd.exe では `chcp 65001` を先に実行すること。

**Done セマンティクスの違い (F254 vs F257)**:
- **F254 範囲サマリー Done**: 実装が存在する COM 数（関数名パターンマッチのみ、内容は検証しない）
- **F257 Incomplete COMs Done**: Phase 要件（TALENT_4 分岐）を満たすキャラ数（0-10）
- **意図**: 既存実装が TALENT_4 分岐を持たない場合、F254 では Done > 0 だが F257 では 0/10 と表示される。これは意図的な動作：/kojo-init はこれらの COM を Phase C2 品質にアップグレードすべきと判断する。

### K1-K10 キャラクター対応表

K1-K10 マッピングは関数名パターン `@KOJO_MESSAGE_COM_K{N}_{COM}` から導出（SSOT）。フォルダ構造 `Game/ERB/口上/` は参照用。
**抽出パターン**: 関数名から `K(\d+)` または `KU` を抽出。
**フォールバック**: パターンマッチ失敗時は該当関数をスキップ（警告ログ出力）。

| ID | Folder | Character |
|:--:|--------|-----------|
| K1 | 1_美鈴 | 美鈴 |
| K2 | 2_小悪魔 | 小悪魔 |
| K3 | 3_パチュリー | パチュリー |
| K4 | 4_咲夜 | 咲夜 |
| K5 | 5_レミリア | レミリア |
| K6 | 6_フラン | フランドール |
| K7 | 7_子悪魔 | 子悪魔 |
| K8 | 8_チルノ | チルノ |
| K9 | 9_大妖精 | 大妖精 |
| K10 | 10_魔理沙 | 魔理沙 |

Note: Skill(kojo-writing) も同様の定義を持つが、フォルダ構造が実装の SSOT。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 10:44 | START | implementer | All Tasks | - |
| 2025-12-28 10:48 | END | implementer | All Tasks | SUCCESS |

---

## Links

- [feature-254.md](feature-254.md) - --progress オプション実装
- [feature-255.md](feature-255.md) - ワークフロー統合
- [feature-258.md](feature-258.md) - /plan Phase 移行判定 (後続 Feature)
- [content-roadmap.md](content-roadmap.md) - Phase 定義元
- [kojo-init.md](../../.claude/commands/kojo-init.md)
- [next.md](../../.claude/commands/next.md) - --progress 参照元
