# Feature 255: Integrate --progress into Workflow Commands

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
kojo-mapper は SSOT として Phase 8 Summary を完全代替すべき（F254 Philosophy 継承）。

F254 で `--progress` オプションを追加したが、これは **可視化ツール** に過ぎない。真の SSOT 代替には、ワークフローコマンド（/next, /kojo-init）が `--progress` を参照して **実装済み COM をスキップ** する必要がある。

### Problem (Current Issue)
- `/next` は `--coverage` のみ参照（カテゴリ別関数数）→ COM 番号別進捗を見ていない
- `/kojo-init` は content-roadmap.md の COM 定義順のみ参照 → 実装済み COM をスキップしない
- 結果: 既に実装済みの COM に対して Feature を作成してしまう可能性

### Goal (What to Achieve)
1. `/kojo-init` が `--progress` を参照し、Done の COM をスキップ
2. `/next` が `--progress` を参照し、未実装 COM を優先提案
3. Progress Tracking セクションに `--progress` コマンド例を追加

### Session Context
- **Origin**: F254 完了時のレビューで Philosophy 未達成を指摘
- **Dependencies**: F254 (--progress オプション実装済み)
- **Related**: F251 (Phase 8 Summary 削除)

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | /kojo-init が --progress 参照 | code | contains | "kojo_mapper.py" AND "--progress" | [x] |
| 2 | /kojo-init が Done COM をスキップ | output | not_contains | "COM_0 " | [x] |
| 3 | /next が --progress 参照 | code | contains | "kojo_mapper.py" AND "--progress" | [x] |
| 4 | index-features.md に --progress 例追加 | file | contains | "--progress" | [x] |

### AC Details

**AC1**: `.claude/commands/kojo-init.md` が `--progress` を実行するよう指示を含むこと

**AC2**: 0系 COM が全て Done の場合、`/kojo-init` の出力に `COM_0` が含まれないこと
```bash
# 現在 0-11 は 100% Done なので、COM_0～COM_11 は提案されないはず
```

**AC3**: `.claude/commands/next.md` が `--progress` を参照するよう指示を含むこと

**AC4**: `Game/agents/index-features.md` の Progress Tracking セクションに `--progress` コマンド例を追加

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | kojo-init.md に --progress 参照ロジック追加 | [○] |
| 2 | 3 | next.md に --progress 参照ロジック追加 | [○] |
| 3 | 4 | index-features.md Progress Tracking 更新 | [○] |

---

## Technical Notes

### kojo-init.md 変更案

Step 1 に追加:
```markdown
### Step 1.5: Check Implemented COMs

Run `python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --progress`

Parse output to extract Done COM numbers:
- If COM is Done (at least 1 character implemented) → Skip
- Only propose Remaining COMs
```

### next.md 変更案

Priority 2 Features セクションに追加:
```markdown
- Run `python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --progress`
- Identify COM ranges with Remaining > 0
- Prioritize COMs in ranges with lowest completion rate
```

### index-features.md 変更案

Progress Tracking セクション:
```markdown
## Progress Tracking

**Kojo Coverage**: Run `python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --coverage`
**COM Progress**: Run `python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --progress`
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 | Feature Completed | finalizer | Updated status to [DONE], all ACs verified [x], all Tasks [○] | READY_TO_COMMIT |

---

## Links

- [feature-254.md](feature-254.md) - --progress オプション実装
- [feature-251.md](feature-251.md) - Phase 8 Summary 削除
- [next.md](../../.claude/commands/next.md)
- [kojo-init.md](../../.claude/commands/kojo-init.md)
