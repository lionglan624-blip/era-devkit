# Feature 144: Reference MD転換（最小化�E�E

## Status: [DONE]

## Type: infra

## Background

### Problem
- pm/reference/ 配下が合訁E,191衁E
- testing-reference.md (1,246衁E が最大
- 重褁E�E容、E��剰な例が多い

### Goal
- 各reference.md を本質皁E��惁E��のみに削渁E
- 4,191衁EↁE~1,000衁E

### Context
- [anthropic-recommended-transition.md](../designs/anthropic-recommended-transition.md) Phase B
- Anthropic推奨原則#4: 最小ルール + エージェント判断

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | testing-reference.md行数 | output | lte | `300` | [x] |
| 2 | kojo-reference.md行数 | output | lte | `150` | [x] |
| 3 | engine-reference.md行数 | output | lte | `150` | [x] |
| 4 | feature-template.md行数 | output | lte | `100` | [x] |
| 5 | 全reference.md合計行数 | output | lte | `1200` | [x] |
| 6 | ビルド�E劁E| build | succeeds | `-` | [x] |

### AC Details

#### AC1-4: 各reference.md行数

**Test Command**:
```bash
wc -l pm/reference/{file}.md | awk '{print $1}'
```

#### AC5: 全reference.md合計行数

**Test Command**:
```bash
wc -l pm/reference/*.md | tail -1 | awk '{print $1}'
```

**Expected Output**: ≤ 1200

#### AC6: ビルド�E劁E

**Test Command**:
```bash
dotnet build uEmuera/uEmuera.sln
```

**Expected Output**: Exit code 0 (build succeeds)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | testing-reference.md を最小化�E�E246ↁE00行！E| [O] |
| 2 | 2 | kojo-reference.md を最小化�E�E28ↁE50行！E| [O] |
| 3 | 3 | engine-reference.md を最小化�E�E51ↁE50行！E| [O] |
| 4 | 4 | feature-template.md を最小化�E�E77ↁE00行！E| [O] |
| 5 | 5 | 残り7ファイル (1811ↁE28, subagent-strategy.md削除) | [O] |
| 6 | 6 | ビルド確誁E| [O] |

---

## Target Files

| # | File | 現状 | 目樁E| 削減率 |
|:-:|------|-----:|-----:|:------:|
| 1 | testing-reference.md | 1,246 | 300 | 76% |
| 2 | kojo-reference.md | 628 | 150 | 76% |
| 3 | engine-reference.md | 551 | 150 | 73% |
| 4 | feature-template.md | 377 | 100 | 73% |
| 5 | subagent-strategy.md | 376 | 0 | 100% (F146で統吁E |
| 6 | kojo-canon-lines.md | 328 | 100 | 70% |
| 7 | erb-reference.md | 254 | 80 | 69% |
| 8 | ntr-system-map.md | 159 | 80 | 50% |
| 9 | hooks-reference.md | 158 | 80 | 49% |
| 10 | sessions-reference.md | 106 | 60 | 43% |
| **合訁E* | | **4,183** | **~1,100** | **74%** |

---

## Transformation Rules

### 削除対象

1. **重褁E��昁E*: 他�E場所で説明済みの冁E��
2. **褁E��の侁E*: 1つに削渁E
3. **Step列挙**: 判断基準�Eみに
4. **歴史皁E��緯**: 現在の仕様�Eみ
5. **トラブルシューチE��ング詳細**: 主要なも�Eのみ

### 残す対象

1. **チE�Eタ構造**: JSON/ERB構文
2. **API/コマンチE*: 使用方況E
3. **判断基溁E*: ぁE��何を使ぁE��
4. **侁E*: 1つだぁE

---

## Priority Order

1. **testing-reference.md** (1,246衁E: 最大、最優允E
2. **kojo-reference.md** (628衁E: 高頻度参�E
3. **engine-reference.md** (551衁E:
4. **feature-template.md** (377衁E:
5. 残り

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-20 | Initialization | initializer | Status PROPOSED ↁEWIP | Ready for implementation |
| 2025-12-20 | Implementation | implementer | Minimized all reference.md files | 4,613 ↁE1,145 lines (75% reduction) |
| 2025-12-20 | Verification | ac-tester | All AC/Task verification complete | All 6 AC PASS, All 6 Task [O] |
| 2025-12-20 | Finalization | finalizer | Status WIP ↁEDONE | Ready for commit |

---

## Discovered Issues

| Issue | Type | Priority |
|-------|------|----------|
| | | |

---

## Links

- [anthropic-recommended-transition.md](../designs/anthropic-recommended-transition.md)
- [reference/](../reference/)
