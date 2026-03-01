# Feature 257: kojo-mapper --progress Phase別キャラ別COM進捁E

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
kojo-mapper は SSOT として Phase 8 Summary を完�E代替すべき！E254 Philosophy 継承�E�、E

### Problem (Current Issue)
F254 で追加した `--progress` は篁E��別サマリーのみ出劁E
```
| Range | Category | Done | Remaining | Progress |
| 60-72 | 挿入系 | 12 | 1 | 92% |
```

これでは `/kojo-init` が「どの COM を作るか」判断できなぁE
- COM_64 が未実裁E��刁E��らなぁE
- K1-K10 のどのキャラが未実裁E��刁E��らなぁE
- Phase (C2/C3/C6) ごとの達�E基準が老E�EされてぁE��ぁE

### Goal (What to Achieve)
`--progress` めEPhase 別・キャラ別の達�E判定に拡張し、`/kojo-init` が正確に未完亁ECOM を選択できるようにする、E

### Session Context
- **Origin**: F254/F255 完亁E���Eレビューで、E-progress 出力が /kojo-init の要件を満たしてぁE��ぁE��とを指摁E(篁E��別サマリーのみで COM 別・キャラ別の未実裁E��報がなぁE
- **Dependencies**: F254 (--progress 実裁E��み), F255 (ワークフロー統合済み)
- **Lesson**: F254→F255でチE�Eル出力と消費老Ekojo-init)の要件ギャチE�Eが判明。�E力設計時に消費老E��ースケースを�Eに定義すべき、E

---

## Design

### Architecture Flow

```
┌─────────────────────────────────────────────────────────────━E
━E/kojo-init 実衁E                                             ━E
└─────────────────────────────────────────────────────────────━E
                              ━E
                              ▼
┌─────────────────────────────────────────────────────────────━E
━EStep 1: kojo_mapper.py --progress                            ━E
━E                                                             ━E
━E1. PHASE_REQUIREMENTS から current_phase (C2) 取征E         ━E
━E2. Phase 要件取征E branch_type="TALENT_4", patterns=1       ━E
━E3. 吁ECOM ÁE吁E��ャラ (K1-K10) をスキャン                     ━E
━E4. Phase 要件達�E判宁E                                       ━E
━E   ◁E= func.branch_type == "TALENT_4"                       ━E
━E   - = 未達�E                                                ━E
━E5. Done < 10 の COM を�E劁E                                  ━E
└─────────────────────────────────────────────────────────────━E
                              ━E
                              ▼
┌─────────────────────────────────────────────────────────────━E
━E出力侁E                                                      ━E
━E                                                             ━E
━E=== Incomplete COMs (Phase: C2) ===                         ━E
━ERequirements: TALENT_4 (4-branch)                           ━E
━E                                                             ━E
━E| COM | K1 | K2 | ... | K10 | Done  |                       ━E
━E|:---:|:--:|:--:|:---:|:---:|:-----:|                       ━E
━E| 64  | ◁E | -  | ... | -   | 1/10  |                       ━E
━E| 90  | -  | -  | ... | -   | 0/10  |                       ━E
└─────────────────────────────────────────────────────────────━E
                              ━E
                              ▼
┌─────────────────────────────────────────────────────────────━E
━EStep 2: /kojo-init ぁECOM 選抁E                              ━E
━E                                                             ━E
━E- Done < 10 の COM から優先度頁E��選抁E                       ━E
━E- feature-{ID}.md 作�E                                       ━E
└─────────────────────────────────────────────────────────────━E
```

### PHASE_REQUIREMENTS (kojo_mapper.py 冁E��義)

```python
# kojo_mapper.py に追加
PHASE_REQUIREMENTS = {
    "C2": {"branch_type": "TALENT_4", "patterns": 1},
    "C3": {"branch_type": "TALENT_4", "patterns": 4},
    "C6": {"branch_type": "FAV_9", "patterns": 9},
}
CURRENT_PHASE = "C2"  # Phase 移行時に /plan で更新
```

**Design Decision**: F254 の COM_RANGES 同様、Phase 要件めEkojo_mapper.py にハ�Eドコード。外部ファイル (YAML) めEagent を不要とし、ツールめEself-contained に維持、E

**Phase Mapping**: Phase 8d = C2 = TALENT_4 刁E��要件。content-roadmap.md の "1 pattern" めE"TALENT_4 刁E��あめE と解釈、E

**Scope Note**: 本 Feature は C2 (TALENT_4) のみ実裁E��E6 (FAV_9) 対応�E Phase 移行時に別 Feature で対応、E

**FAV_9 Placeholder**: C6 の `branch_type: FAV_9` は現在の kojo_mapper.py に存在しなぁE�Eレースホルダー。実裁E��には `NTR_9` (既存�E `NTR_{depth}` 命名規則) また�E新要EFAV_9 検�EロジチE��を追加。本 Feature では C2 のみ使用するため影響なし、E

**Phase Transition**: CURRENT_PHASE は kojo_mapper.py 冁E��手動更新。Phase 移行タイミングは content-roadmap.md の Phase 定義に基づき、�EロジェクトメンチE��ぁE`/plan` 実行時に判断。更新方況E kojo_mapper.py の `CURRENT_PHASE = "C2"` を次の Phase�E�侁E "C3"�E�に変更、E

**patterns フィールチE*: `patterns` は C3 以降で使用 (has_variation 判宁E、E2 では `branch_type` のみで判定し、`patterns` は参�EされなぁE��E

### 判定ロジチE��

**実裁E��象**: `calculate_com_progress()` を拡張して、COM×Character ごとに Phase 要件を判定する、E

**現在の実裁E* (F254): COM function の存在チェチE��のみ (関数名パターンマッチE、Eone = 少なくとめEつ実裁E��めE(全キャラの追跡なぁE、E
**拡張征E* (F257): branch_type による Phase 要件達�E判定、Eone = Phase 要件を満たすキャラ数 (0-10)、E
**Semantics Change**: F254 の Done (any implementation) ↁEF257 の Done (per-character count)、E

**Output Format Transition**: F257 --progress は F254 の篁E��別サマリーに加えて、Incomplete COMs セクションを追加出力する、E254 の出力形式�E維持し、新しい COM×Character マトリクスを追加、E

**実裁E��況E*:
1. `PHASE_REQUIREMENTS` と `CURRENT_PHASE` から Phase 要件を取征E
2. `calculate_com_progress()` をリファクタ:
   - 吁EERB ファイルに対して `parse_erb_file()` を呼び出ぁE
   - KojoFunction オブジェクチE(branch_type 含む) を取征E
   - COM 番号とキャラ ID (K1-K10) でグループ化
3. 吁EKojoFunction の `branch_type` を取征E
4. `is_phase_complete()` で Phase 要件判宁E
5. ◁E/ - を決宁E

```python
def is_phase_complete(func: KojoFunction, phase_req: dict) -> bool:
    """
    Phase 要件を満たしてぁE��か判宁E

    Note: calculate_com_progress() 冁E�� COM×Character ごとに呼び出される、E
    func は parse_kojo_file() で解析された KojoFunction オブジェクト、E
    """
    required_branch = phase_req.get("branch_type", "TALENT_4")
    required_patterns = phase_req.get("patterns", 1)

    # C2: TALENT_4 刁E��があればOK
    if required_branch == "TALENT_4":
        if func.branch_type != "TALENT_4":
            return False

    # C3: さらに4パターン (has_variation) が忁E��E
    if required_patterns >= 4:
        if not func.has_variation:
            return False

    return True
```

**COM×Character 抽出方況E*:
1. **COM 番号・キャラ ID 抽出**: 関数名パターン `@KOJO_MESSAGE_COM_K{N}_{COM}` から両方を同時に抽出
   - **Pattern**: `@KOJO_MESSAGE_COM_K(?:U|(\d+))(?:_(\d+))?` (既孁EKOJO_PATTERNS['COM'] と同筁E
   - 侁E `@KOJO_MESSAGE_COM_K7_60` ↁEK7, COM_60
   - 侁E `@KOJO_MESSAGE_COM_KU_64` ↁEKU, COM_64
   - KU は全キャラに適用されるため特別処琁E��下記�E KU 処琁E��クション参�E�E�E
2. **グループ化**: 抽出した (COM_num, char_id) タプルで KojoFunction をグループ化
   - チE�Eタ構造: `Dict[Tuple[int, str], List[KojoFunction]]`
   - 侁E `{(60, "K7"): [func1, func2], (64, "KU"): [func3]}`

**判定フロー**:
1. `calculate_com_progress()` が各 ERB ファイルめE`parse_erb_file()` で解极E
2. 返された KojoFunction オブジェクトかめE(COM_num, char_id) を抽出してグループ化
3. 吁E(COM, Character) グループに対して: `any(is_phase_complete(func, phase_req) for func in group)`
4. 結果めE◁E/ - で出力！ETF-8 ターミナル前提、下訁ENote 参�E�E�E

**雁E��E��ール**: 同一 COM×Character に褁E��の function がある場合、ANY ぁEPhase 要件を満たせば ◁Eとする、E
�E�侁E COM_64 に K1 美鈴の function ぁE2 つあり、E つぁETALENT_4 なめE○！E

**KU (Universal) 処琁E*: KU function は全 K1-K10 に適用される、E
- KU ぁETALENT_4 なら、K1-K10 全カラムに ◁Eを表示�E�E0 カラム全て ○！E
- KU + 個別キャラ function がある場合も ANY ルール適用
- 出力侁E `| 64 | ◁E| ◁E| ◁E| ◁E| ◁E| ◁E| ◁E| ◁E| ◁E| ◁E| 10/10 |` (KU のみで全キャラ達�E)
- **Semantics**: KU-complete (10/10) の COM は Incomplete リストから除外される。これ�E意図皁E��動作：KU が�Eキャラをカバ�EしてぁE��ため、個別キャラ実裁E�E不要と判断される、Ekojo-init はこれら�E COM をスキチE�Eして次の未完亁ECOM を選択する、E
- **KU Not Meeting Phase**: KU ぁEPhase 要件を満たさなぁE��合（侁E TALENT_4 要件に対して KU ぁENONE 刁E��）、KU は無視されて K1-K10 の個別関数が評価される、EU のみで個別 Kx 関数がなぁE��合、�EキャラぁE- 表示 (0/10)、E

**Migration Notes** (F254 ↁEF257):
- **現在の実裁E*: `calculate_com_progress()` は `implemented_coms: Set[int]` を使用 (COM 単位�Eみ追跡、E��数名�Eみ解极E
- **F257 要件**: COM ÁECharacter マトリクス (侁E `Dict[int, Dict[str, bool]]`) への変更が忁E��E
  - **Key Types**: outer key = COM 番号 (int: 0-311), inner key = キャラ ID (str: 'K1'-'K10' or 'KU'), value = Phase 達�E (bool)
- **互換性**: 既存�E篁E��別サマリー出力�E維持し、新しい Incomplete COMs セクションを追加
- **実裁E��更**: 関数名パターンマッチEↁEparse_erb_file() による完�E解极E
  1. `directory.rglob('*.ERB')` で ERB ファイルを�E持E
  2. 吁E��ァイルに対して `parse_erb_file()` を呼び出ぁE`List[KojoFunction]` を取征E
  3. 吁EKojoFunction から: func.name で COM#/char_id を抽出、func.branch_type で Phase 要件を判宁E
  4. 結果めE`Dict[int, Dict[str, bool]]` に雁E��E

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | --progress ぁEcurrent_phase を表示 | output | bash | contains | "Phase: C2" | [x] |
| 2 | --progress がキャラ別カラム出劁E| output | bash | contains | "K1" | [x] |
| 3a | Incomplete リストに未完亁ECOM が含まれる | output | bash | matches | `[0-9]/10` | [x] |
| 3b | 完亁ECOM (10/10) ぁEIncomplete リストから除夁E| output | bash | not_contains | "10/10" | [x] |
| 4 | kojo-init.md ぁE--progress を参照 | code | Grep | contains | "--progress" | [x] |

### AC Details

**Note (Type: infra)**: Method 列�E検証チE�Eル (grep/bash) を示す。erb/engine features の --unit/--inject とは異なる、E

**AC1**: `--progress` 出力に現在の Phase が表示されること
- Test: `python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --progress`
- Expected output contains:
```
=== Incomplete COMs (Phase: C2) ===
Requirements: TALENT_4 (4-branch)
```

**AC2**: 吁ECOM のキャラ別達�E状況が表示されること
- Test: `python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --progress`
- Expected output contains:
```
| COM | K1 | K2 | K3 | K4 | K5 | K6 | K7 | K8 | K9 | K10 | Done |
```

**AC3a**: Incomplete リストに未完亁ECOM が含まれること
- Note: `matches` で Done 列に `/10` パターン�E�侁E `0/10`, `5/10`�E�が存在することを検証。これにより、Incomplete COMs セクションが正しく出力されることを確認、E

**AC3b**: 完亁ECOM (10/10) ぁEIncomplete リストから除外されること
- Method: `bash (grep -A extraction)` - Incomplete COMs セクションのみを抽出して検証
- Note: `not_contains` で "10/10" ぁEIncomplete COMs セクションに存在しなぁE��とを検証、Eone < 10 フィルタリングが正しく機�EしてぁE��ことを確認、E
- **Scope**: チE��ト対象は `=== Incomplete COMs` 以降�Eセクションのみ、E254 篁E��サマリー�E�E00% 完亁E��E��を含む可能性あり�E��E対象外、E

**AC4**: `/kojo-init` ぁE`--progress` 出力を参�Eして COM を選択すること
- Note: 本 AC は kojo-init.md に `--progress` への参�Eが存在することを検証、E
- **Additive Output**: F257 の出力形式�E F254 の篁E��別サマリーに COM×Character マトリクスを追加する形式。既存�E kojo-init.md パ�Eス処琁E��篁E��サマリーの Done 列参照�E��E継続利用可能。新しいマトリクス形式�Eパ�Eス実裁E�E kojo-init.md の封E��拡張として scope 外、E
- **Consumer Use Case**:
  - **現在 (kojo-init.md)**: 篁E��サマリーの Done 列で "Done < COM_COUNT" の篁E��を特宁EↁE任意�E COM を選択、E257 マトリクスは参�EしなぁE��E
  - **封E�� (scope 夁E**: マトリクスをパースして "Done < 10" の COM を直接選択可能、E
  - **人間向ぁE*: マトリクス出力�E開発老E��キャラ別進捗を視覚的に確認するために有用�E�Ekojo-init automation なしでも価値あり�E�、E

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo_mapper.py に PHASE_REQUIREMENTS/CURRENT_PHASE 定数を追加し、E-progress で current_phase を表示 | [x] |
| 2 | 2 | kojo_mapper.py --progress でキャラ別カラム出劁E(K1-K10)。calculate_com_progress() めESet[int] から Dict[int, Dict[str, bool]] に変更 | [x] |
| 3a | 3a | Incomplete リストに /10 パターンで未完亁ECOM を�E劁E| [x] |
| 3b | 3b | Incomplete リストかめE10/10 完亁ECOM を除外（フィルタリング�E�E| [x] |
| 4 | 4 | kojo-init.md に --progress 参�Eを維持E���E力形式�E additive、既存パースは継続利用可能�E�E| [x] |

---

## Technical Notes

### 期征E�E力侁E

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
| 64 | ◁E| - | - | - | - | - | - | - | - | - | 1/10 |
| 90 | - | - | - | - | - | - | - | - | - | - | 0/10 |
| 91 | - | - | - | - | - | - | - | - | - | - | 0/10 |
...
```

**Note**: Name 列�E省略、EOM 番号と対応するアクションは content-roadmap.md 参�E、E

**UTF-8 Requirement**: 出力�E ◁E- シンボルは靁EASCII 斁E��。UTF-8 対応ターミナルで実行すること。パイプ�E力時はエンコーチE��ングに注意、E
- **Windows**: PowerShell また�E Git Bash 推奨。cmd.exe では `chcp 65001` を�Eに実行すること、E

**Done セマンチE��クスの違い (F254 vs F257)**:
- **F254 篁E��サマリー Done**: 実裁E��存在する COM 数�E�関数名パターンマッチ�Eみ、�E容は検証しなぁE��E
- **F257 Incomplete COMs Done**: Phase 要件�E�EALENT_4 刁E��）を満たすキャラ数�E�E-10�E�E
- **意図**: 既存実裁E�� TALENT_4 刁E��を持たなぁE��合、F254 では Done > 0 だぁEF257 では 0/10 と表示される。これ�E意図皁E��動作！Ekojo-init はこれら�E COM めEPhase C2 品質にアチE�Eグレードすべきと判断する、E

### K1-K10 キャラクター対応表

K1-K10 マッピングは関数名パターン `@KOJO_MESSAGE_COM_K{N}_{COM}` から導�E�E�ESOT�E�。フォルダ構造 `Game/ERB/口丁E` は参�E用、E
**抽出パターン**: 関数名かめE`K(\d+)` また�E `KU` を抽出、E
**フォールバック**: パターンマッチ失敗時は該当関数をスキチE�E�E�警告ログ出力）、E

| ID | Folder | Character |
|:--:|--------|-----------|
| K1 | 1_美鈴 | 美鈴 |
| K2 | 2_小悪魁E| 小悪魁E|
| K3 | 3_パチュリー | パチュリー |
| K4 | 4_咲夁E| 咲夁E|
| K5 | 5_レミリア | レミリア |
| K6 | 6_フラン | フランド�Eル |
| K7 | 7_子悪魁E| 子悪魁E|
| K8 | 8_チルチE| チルチE|
| K9 | 9_大妖精 | 大妖精 |
| K10 | 10_魔理沁E| 魔理沁E|

Note: Skill(kojo-writing) も同様�E定義を持つが、フォルダ構造が実裁E�E SSOT、E

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 10:44 | START | implementer | All Tasks | - |
| 2025-12-28 10:48 | END | implementer | All Tasks | SUCCESS |

---

## Links

- [feature-254.md](feature-254.md) - --progress オプション実裁E
- [feature-255.md](feature-255.md) - ワークフロー統吁E
- [feature-258.md](feature-258.md) - /plan Phase 移行判宁E(後綁EFeature)
- [content-roadmap.md](../content-roadmap.md) - Phase 定義允E
- [kojo-init.md](../../../archive/claude_legacy_20251230/commands/kojo-init.md)
- [next.md](../../../archive/claude_legacy_20251230/commands/next.md) - --progress 参�E允E
