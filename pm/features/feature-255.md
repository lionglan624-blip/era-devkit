# Feature 255: Integrate --progress into Workflow Commands



## Status: [DONE]



## Type: infra



## Background



### Philosophy (Mid-term Vision)

kojo-mapper は SSOT として Phase 8 Summary を完�E代替すべき！E254 Philosophy 継承�E�、E


F254 で `--progress` オプションを追加したが、これ�E **可視化チE�Eル** に過ぎなぁE��真の SSOT 代替には、ワークフローコマンド！Enext, /kojo-init�E�が `--progress` を参照して **実裁E��み COM をスキチE�E** する忁E��がある、E


### Problem (Current Issue)

- `/next` は `--coverage` のみ参�E�E�カチE��リ別関数数�E��E COM 番号別進捗を見てぁE��ぁE
- `/kojo-init` は content-roadmap.md の COM 定義頁E�Eみ参�E ↁE実裁E��み COM をスキチE�EしなぁE
- 結果: 既に実裁E��みの COM に対して Feature を作�EしてしまぁE��能性



### Goal (What to Achieve)

1. `/kojo-init` ぁE`--progress` を参照し、Done の COM をスキチE�E

2. `/next` ぁE`--progress` を参照し、未実裁ECOM を優先提桁E
3. Progress Tracking セクションに `--progress` コマンド例を追加



### Session Context

- **Origin**: F254 完亁E��のレビューで Philosophy 未達�Eを指摁E
- **Dependencies**: F254 (--progress オプション実裁E��み)

- **Related**: F251 (Phase 8 Summary 削除)



---



## Acceptance Criteria



| AC# | Description | Type | Matcher | Expected | Status |

|:---:|-------------|------|---------|----------|:------:|

| 1 | /kojo-init ぁE--progress 参�E | code | contains | "kojo_mapper.py" AND "--progress" | [x] |

| 2 | /kojo-init ぁEDone COM をスキチE�E | output | not_contains | "COM_0 " | [x] |

| 3 | /next ぁE--progress 参�E | code | contains | "kojo_mapper.py" AND "--progress" | [x] |

| 4 | index-features.md に --progress 例追加 | file | contains | "--progress" | [x] |



### AC Details



**AC1**: `.claude/commands/kojo-init.md` ぁE`--progress` を実行するよぁE��示を含むこと



**AC2**: 0系 COM が�Eて Done の場合、`/kojo-init` の出力に `COM_0` が含まれなぁE��と

```bash

# 現在 0-11 は 100% Done なので、COM_0�E�COM_11 は提案されなぁE�EぁE
```



**AC3**: `.claude/commands/next.md` ぁE`--progress` を参照するよう持E��を含むこと



**AC4**: `pm/index-features.md` の Progress Tracking セクションに `--progress` コマンド例を追加



---



## Tasks



| Task# | AC# | Description | Status |

|:-----:|:---:|-------------|:------:|

| 1 | 1,2 | kojo-init.md に --progress 参�EロジチE��追加 | [○] |

| 2 | 3 | next.md に --progress 参�EロジチE��追加 | [○] |

| 3 | 4 | index-features.md Progress Tracking 更新 | [○] |



---



## Technical Notes



### kojo-init.md 変更桁E


Step 1 に追加:

```markdown

### Step 1.5: Check Implemented COMs



Run `python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --progress`



Parse output to extract Done COM numbers:

- If COM is Done (at least 1 character implemented) ↁESkip

- Only propose Remaining COMs

```



### next.md 変更桁E


Priority 2 Features セクションに追加:

```markdown

- Run `python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --progress`

- Identify COM ranges with Remaining > 0

- Prioritize COMs in ranges with lowest completion rate

```



### index-features.md 変更桁E


Progress Tracking セクション:

```markdown

## Progress Tracking



**Kojo Coverage**: Run `python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --coverage`

**COM Progress**: Run `python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --progress`

```



---



## Execution Log



| Timestamp | Event | Agent | Action | Result |

|-----------|:-----:|-------|--------|--------|

| 2025-12-28 | Feature Completed | finalizer | Updated status to [DONE], all ACs verified [x], all Tasks [○] | READY_TO_COMMIT |



---



## Links



- [feature-254.md](feature-254.md) - --progress オプション実裁E
- [feature-251.md](feature-251.md) - Phase 8 Summary 削除

- [next.md](../../../archive/claude_legacy_20251230/commands/next.md)

- [kojo-init.md](../../../archive/claude_legacy_20251230/commands/kojo-init.md)

