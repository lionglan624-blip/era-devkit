# Feature 258: /plan Phase 移行判定と CURRENT_PHASE 自動更新

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Phase 移行�E /plan の責務。kojo-mapper の CURRENT_PHASE は /plan 実行時に自動更新されるべき、E

### Problem (Current Issue)
F257 で PHASE_REQUIREMENTS と CURRENT_PHASE めEkojo_mapper.py に追加したが、Phase 移行時の更新は手動:
- feature-257.md Design セクションに、Eplan 実行時に判断」と記輁E
- しかぁE/plan には Phase 判定�E更新ロジチE��がなぁE
- CURRENT_PHASE の手動更新が忁E��で、更新忘れのリスクがあめE

### Goal (What to Achieve)
/plan 実行時に:
1. --progress 出力から現在の Phase 達�E状況を取征E
2. content-roadmap.md の Phase 定義と比輁E
3. Phase 移行が適刁E��場合、CURRENT_PHASE を�E動更新�E�また�E提案！E

### Session Context
- **Origin**: F257 実裁E���Eレビューで Phase 移行手頁E�E不備を指摁E
- **Dependencies**: F257 (PHASE_REQUIREMENTS/CURRENT_PHASE 実裁E��み)
- **Constraint**: 自動更新 vs 提案�Eみ (ユーザー確誁E の選択が忁E��E

---

## Design

### Architecture Flow

```
┌─────────────────────────────────────────────────────────────━E
━E/plan 実衁E                                                  ━E
└─────────────────────────────────────────────────────────────━E
                              ━E
                              ▼
┌─────────────────────────────────────────────────────────────━E
━EStep 1: 現在の Phase 達�E状況を取征E                         ━E
━E                                                             ━E
━Epython src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E \    ━E
━E  --progress                                                 ━E
━E                                                             ━E
━E出力解极E                                                    ━E
━E- "=== Incomplete COMs (Phase: X) ===" セクションを解极E    ━E
━E- チE�Eブル行数をカウンチE(Incomplete COM 数)                ━E
━E- Done 列を雁E��E(完亁E��ターン数)                            ━E
└─────────────────────────────────────────────────────────────━E
                              ━E
                              ▼
┌─────────────────────────────────────────────────────────────━E
━EStep 2: Phase 完亁E��宁E                                      ━E
━E                                                             ━E
━Eif Incomplete COMs == 0:                                     ━E
━E    Phase 完亁EↁE次の Phase への移行を提桁E                  ━E
━Eelse:                                                        ━E
━E    Phase 継綁EↁE残りタスクを表示                            ━E
└─────────────────────────────────────────────────────────────━E
                              ━E
                              ▼
┌─────────────────────────────────────────────────────────────━E
━EStep 3: CURRENT_PHASE 更新提桁E(Phase 完亁E��のみ)           ━E
━E                                                             ━E
━EPhase 完亁E��の出劁E                                          ━E
━E"Phase {X} 完亁E��次 Phase に移行する場吁E                   ━E
━E Edit(kojo_mapper.py, CURRENT_PHASE = \"{X}\" ↁE\"{Y}\")"   ━E
━E                                                             ━E
━EↁEユーザーが手動で Edit コマンド実衁Eor /plan 再実衁E       ━E
━EↁEplan.md Phase 6 パターン踏襲 (Report ↁEUser Action)       ━E
└─────────────────────────────────────────────────────────────━E
```

### Error Handling

| Condition | Action |
|-----------|--------|
| --progress 実行失敁E| エラーをユーザーに報告、Phase 移行提案をスキチE�E |
| --progress 出力が空 | エラーをユーザーに報告、Phase 移行提案をスキチE�E |
| PHASE_REQUIREMENTS 未定義 Phase | エラーをユーザーに報告、手動対応を俁E�� |

### Phase 定義 (SSOT: kojo_mapper.py)

```python
PHASE_REQUIREMENTS = {
    "C2": {"branch_type": "TALENT_4", "patterns": 1},
    "C3": {"branch_type": "TALENT_4", "patterns": 4},
    "C6": {"branch_type": "FAV_9", "patterns": 9},
}
```

### Phase 遷移頁E��E

| Current | Next | 条件 |
|:-------:|:----:|------|
| C2 | C3 | Incomplete COMs (C2 基溁E == 0 |
| C3 | C6 | Incomplete COMs (C3 基溁E == 0 |
| C6 | - | 最絁EPhase |

**Scope Note**:
- PHASE_REQUIREMENTS (C2/C3/C6) は branch_type ベ�Eスの完亁E��準を持つ Phase のみ定義
- C0/C1 は歴史皁EPhase (v0.4/v0.5 完亁E、CURRENT_PHASE は C2 以上を想宁E
- C4/C5/C7/C8 は branch_type 以外�E基溁E(特定イベント完亁E��E のため、本 Feature のスコープ夁E
- 別 Feature で PHASE_REQUIREMENTS 拡張時に対忁E

### 実裁E��所

| File | Change |
|------|--------|
| `.claude/commands/plan.md` | Phase 判定�E更新ロジチE��追加 |
| `src/tools/kojo-mapper/kojo_mapper.py` | (変更なし、F257 で実裁E��み) |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | /plan ぁE--progress を実行して Phase 状況を取征E| file | Grep | contains | "--progress" | [x] |
| 2 | /plan ぁEPhase 完亁E��に移行提案を出劁E| file | Grep | contains | "Phase.*完亁E | [x] |
| 3 | /plan ぁECURRENT_PHASE 更新手頁E��提示 | file | Grep | contains | "CURRENT_PHASE" | [x] |

### AC Details

**AC1**: /plan 実行時に kojo_mapper.py --progress を呼び出すこと
- plan.md に --progress 参�Eがあること

**AC2**: Phase 完亁E��定結果をユーザーに表示すること
- plan.md に "Phase.*完亁E パターンの出力ロジチE��があること
- Incomplete COMs == 0 の場吁E "Phase {X} 完亁E + 更新手頁E��提示
- Incomplete COMs > 0 の場吁E "Phase {X}: {N} COMs 残り"

**AC3**: Phase 移行時の CURRENT_PHASE 更新手頁E��提示すること
- plan.md に Edit(CURRENT_PHASE = ...) パターン記載があること
- 実際の更新はユーザーが手動実衁E(plan.md Phase 6 パターン踏襲)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | plan.md に --progress 実行スチE��プを追加 | [x] |
| 2 | 2 | plan.md に Phase 完亁E��定と移行提案ロジチE��を追加 | [x] |
| 3 | 3 | plan.md に CURRENT_PHASE 更新手頁E��示ロジチE��を追加 | [x] |

---

## Technical Notes

### 期征E��れる /plan 出力侁E

**Note**: 出力形式�E --progress の Range Summary チE�Eブルを基に生�E、E

**Phase 継続時**:
```
=== Phase Status (from --progress) ===
| Range | Total | Done | Remaining | Progress |
|-------|:-----:|:----:|:---------:|:--------:|
| Total | 52    | 27   | 25        | 52%      |

Phase C2 継続中。残り 25 COMs を完亁E��てください、E
```

**Phase 完亁E��**:
```
=== Phase Status (from --progress) ===
| Range | Total | Done | Remaining | Progress |
|-------|:-----:|:----:|:---------:|:--------:|
| Total | 52    | 52   | 0         | 100%     |

Phase C2 完亁E��次 Phase に移行する場吁E
  Edit(kojo_mapper.py, CURRENT_PHASE = "C2" ↁE"C3")
```

### CURRENT_PHASE 更新方況E

```bash
# sed による置揁E(Git Bash)
sed -i 's/CURRENT_PHASE = "C2"/CURRENT_PHASE = "C3"/' src/tools/kojo-mapper/kojo_mapper.py

# また�E Edit チE�Eル
Edit(file_path="src/tools/kojo-mapper/kojo_mapper.py",
     old_string='CURRENT_PHASE = "C2"',
     new_string='CURRENT_PHASE = "C3"')
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 | Phase 1 | initializer | Feature validation | READY |
| 2025-12-28 | Phase 2 | explorer | Investigation | READY |
| 2025-12-28 | Phase 4 | implementer | Added Phase 7 to plan.md | SUCCESS |
| 2025-12-28 | Phase 6 | - | AC verification (static) | PASS:3/3 |
| 2025-12-28 | Phase 7 | feature-reviewer | Post-review (mode: post) | READY |
| 2025-12-28 | Finalization | finalizer | Status update to [DONE] | SUCCESS |

---

## Links

- [feature-257.md](feature-257.md) - PHASE_REQUIREMENTS/CURRENT_PHASE 実裁E
- [plan.md](../../../archive/claude_legacy_20251230/commands/plan.md) - /plan コマンド定義
- [content-roadmap.md](../content-roadmap.md) - Phase 定義允E
