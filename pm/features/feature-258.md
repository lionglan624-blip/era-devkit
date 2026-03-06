# Feature 258: /plan Phase 移行判定と CURRENT_PHASE 自動更新

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Phase 移行は /plan の責務。kojo-mapper の CURRENT_PHASE は /plan 実行時に自動更新されるべき。

### Problem (Current Issue)
F257 で PHASE_REQUIREMENTS と CURRENT_PHASE を kojo_mapper.py に追加したが、Phase 移行時の更新は手動:
- feature-257.md Design セクションに「/plan 実行時に判断」と記載
- しかし /plan には Phase 判定・更新ロジックがない
- CURRENT_PHASE の手動更新が必要で、更新忘れのリスクがある

### Goal (What to Achieve)
/plan 実行時に:
1. --progress 出力から現在の Phase 達成状況を取得
2. content-roadmap.md の Phase 定義と比較
3. Phase 移行が適切な場合、CURRENT_PHASE を自動更新（または提案）

### Session Context
- **Origin**: F257 実装後のレビューで Phase 移行手順の不備を指摘
- **Dependencies**: F257 (PHASE_REQUIREMENTS/CURRENT_PHASE 実装済み)
- **Constraint**: 自動更新 vs 提案のみ (ユーザー確認) の選択が必要

---

## Design

### Architecture Flow

```
┌─────────────────────────────────────────────────────────────┐
│ /plan 実行                                                   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ Step 1: 現在の Phase 達成状況を取得                          │
│                                                              │
│ python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" \    │
│   --progress                                                 │
│                                                              │
│ 出力解析:                                                    │
│ - "=== Incomplete COMs (Phase: X) ===" セクションを解析     │
│ - テーブル行数をカウント (Incomplete COM 数)                │
│ - Done 列を集計 (完了パターン数)                            │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ Step 2: Phase 完了判定                                       │
│                                                              │
│ if Incomplete COMs == 0:                                     │
│     Phase 完了 → 次の Phase への移行を提案                   │
│ else:                                                        │
│     Phase 継続 → 残りタスクを表示                            │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ Step 3: CURRENT_PHASE 更新提案 (Phase 完了時のみ)           │
│                                                              │
│ Phase 完了時の出力:                                          │
│ "Phase {X} 完了。次 Phase に移行する場合:                   │
│  Edit(kojo_mapper.py, CURRENT_PHASE = \"{X}\" → \"{Y}\")"   │
│                                                              │
│ → ユーザーが手動で Edit コマンド実行 or /plan 再実行        │
│ → plan.md Phase 6 パターン踏襲 (Report → User Action)       │
└─────────────────────────────────────────────────────────────┘
```

### Error Handling

| Condition | Action |
|-----------|--------|
| --progress 実行失敗 | エラーをユーザーに報告、Phase 移行提案をスキップ |
| --progress 出力が空 | エラーをユーザーに報告、Phase 移行提案をスキップ |
| PHASE_REQUIREMENTS 未定義 Phase | エラーをユーザーに報告、手動対応を促す |

### Phase 定義 (SSOT: kojo_mapper.py)

```python
PHASE_REQUIREMENTS = {
    "C2": {"branch_type": "TALENT_4", "patterns": 1},
    "C3": {"branch_type": "TALENT_4", "patterns": 4},
    "C6": {"branch_type": "FAV_9", "patterns": 9},
}
```

### Phase 遷移順序

| Current | Next | 条件 |
|:-------:|:----:|------|
| C2 | C3 | Incomplete COMs (C2 基準) == 0 |
| C3 | C6 | Incomplete COMs (C3 基準) == 0 |
| C6 | - | 最終 Phase |

**Scope Note**:
- PHASE_REQUIREMENTS (C2/C3/C6) は branch_type ベースの完了基準を持つ Phase のみ定義
- C0/C1 は歴史的 Phase (v0.4/v0.5 完了)、CURRENT_PHASE は C2 以上を想定
- C4/C5/C7/C8 は branch_type 以外の基準 (特定イベント完了等) のため、本 Feature のスコープ外
- 別 Feature で PHASE_REQUIREMENTS 拡張時に対応

### 実装場所

| File | Change |
|------|--------|
| `.claude/commands/plan.md` | Phase 判定・更新ロジック追加 |
| `tools/kojo-mapper/kojo_mapper.py` | (変更なし、F257 で実装済み) |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | /plan が --progress を実行して Phase 状況を取得 | file | Grep | contains | "--progress" | [x] |
| 2 | /plan が Phase 完了時に移行提案を出力 | file | Grep | contains | "Phase.*完了" | [x] |
| 3 | /plan が CURRENT_PHASE 更新手順を提示 | file | Grep | contains | "CURRENT_PHASE" | [x] |

### AC Details

**AC1**: /plan 実行時に kojo_mapper.py --progress を呼び出すこと
- plan.md に --progress 参照があること

**AC2**: Phase 完了判定結果をユーザーに表示すること
- plan.md に "Phase.*完了" パターンの出力ロジックがあること
- Incomplete COMs == 0 の場合: "Phase {X} 完了" + 更新手順を提示
- Incomplete COMs > 0 の場合: "Phase {X}: {N} COMs 残り"

**AC3**: Phase 移行時の CURRENT_PHASE 更新手順を提示すること
- plan.md に Edit(CURRENT_PHASE = ...) パターン記載があること
- 実際の更新はユーザーが手動実行 (plan.md Phase 6 パターン踏襲)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | plan.md に --progress 実行ステップを追加 | [x] |
| 2 | 2 | plan.md に Phase 完了判定と移行提案ロジックを追加 | [x] |
| 3 | 3 | plan.md に CURRENT_PHASE 更新手順提示ロジックを追加 | [x] |

---

## Technical Notes

### 期待される /plan 出力例

**Note**: 出力形式は --progress の Range Summary テーブルを基に生成。

**Phase 継続時**:
```
=== Phase Status (from --progress) ===
| Range | Total | Done | Remaining | Progress |
|-------|:-----:|:----:|:---------:|:--------:|
| Total | 52    | 27   | 25        | 52%      |

Phase C2 継続中。残り 25 COMs を完了してください。
```

**Phase 完了時**:
```
=== Phase Status (from --progress) ===
| Range | Total | Done | Remaining | Progress |
|-------|:-----:|:----:|:---------:|:--------:|
| Total | 52    | 52   | 0         | 100%     |

Phase C2 完了。次 Phase に移行する場合:
  Edit(kojo_mapper.py, CURRENT_PHASE = "C2" → "C3")
```

### CURRENT_PHASE 更新方法

```bash
# sed による置換 (Git Bash)
sed -i 's/CURRENT_PHASE = "C2"/CURRENT_PHASE = "C3"/' tools/kojo-mapper/kojo_mapper.py

# または Edit ツール
Edit(file_path="tools/kojo-mapper/kojo_mapper.py",
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

- [feature-257.md](feature-257.md) - PHASE_REQUIREMENTS/CURRENT_PHASE 実装
- [plan.md](../../.claude/commands/plan.md) - /plan コマンド定義
- [content-roadmap.md](content-roadmap.md) - Phase 定義元
