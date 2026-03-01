# Feature 144: Reference MD転換（最小化）

## Status: [DONE]

## Type: infra

## Background

### Problem
- Game/agents/reference/ 配下が合計4,191行
- testing-reference.md (1,246行) が最大
- 重複内容、過剰な例が多い

### Goal
- 各reference.md を本質的な情報のみに削減
- 4,191行 → ~1,000行

### Context
- [anthropic-recommended-transition.md](designs/anthropic-recommended-transition.md) Phase B
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
| 6 | ビルド成功 | build | succeeds | `-` | [x] |

### AC Details

#### AC1-4: 各reference.md行数

**Test Command**:
```bash
wc -l Game/agents/reference/{file}.md | awk '{print $1}'
```

#### AC5: 全reference.md合計行数

**Test Command**:
```bash
wc -l Game/agents/reference/*.md | tail -1 | awk '{print $1}'
```

**Expected Output**: ≤ 1200

#### AC6: ビルド成功

**Test Command**:
```bash
dotnet build uEmuera/uEmuera.sln
```

**Expected Output**: Exit code 0 (build succeeds)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | testing-reference.md を最小化（1246→300行） | [O] |
| 2 | 2 | kojo-reference.md を最小化（628→150行） | [O] |
| 3 | 3 | engine-reference.md を最小化（551→150行） | [O] |
| 4 | 4 | feature-template.md を最小化（377→100行） | [O] |
| 5 | 5 | 残り7ファイル (1811→528, subagent-strategy.md削除) | [O] |
| 6 | 6 | ビルド確認 | [O] |

---

## Target Files

| # | File | 現状 | 目標 | 削減率 |
|:-:|------|-----:|-----:|:------:|
| 1 | testing-reference.md | 1,246 | 300 | 76% |
| 2 | kojo-reference.md | 628 | 150 | 76% |
| 3 | engine-reference.md | 551 | 150 | 73% |
| 4 | feature-template.md | 377 | 100 | 73% |
| 5 | subagent-strategy.md | 376 | 0 | 100% (F146で統合) |
| 6 | kojo-canon-lines.md | 328 | 100 | 70% |
| 7 | erb-reference.md | 254 | 80 | 69% |
| 8 | ntr-system-map.md | 159 | 80 | 50% |
| 9 | hooks-reference.md | 158 | 80 | 49% |
| 10 | sessions-reference.md | 106 | 60 | 43% |
| **合計** | | **4,183** | **~1,100** | **74%** |

---

## Transformation Rules

### 削除対象

1. **重複説明**: 他の場所で説明済みの内容
2. **複数の例**: 1つに削減
3. **Step列挙**: 判断基準のみに
4. **歴史的経緯**: 現在の仕様のみ
5. **トラブルシューティング詳細**: 主要なもののみ

### 残す対象

1. **データ構造**: JSON/ERB構文
2. **API/コマンド**: 使用方法
3. **判断基準**: いつ何を使うか
4. **例**: 1つだけ

---

## Priority Order

1. **testing-reference.md** (1,246行): 最大、最優先
2. **kojo-reference.md** (628行): 高頻度参照
3. **engine-reference.md** (551行):
4. **feature-template.md** (377行):
5. 残り

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-20 | Initialization | initializer | Status PROPOSED → WIP | Ready for implementation |
| 2025-12-20 | Implementation | implementer | Minimized all reference.md files | 4,613 → 1,145 lines (75% reduction) |
| 2025-12-20 | Verification | ac-tester | All AC/Task verification complete | All 6 AC PASS, All 6 Task [O] |
| 2025-12-20 | Finalization | finalizer | Status WIP → DONE | Ready for commit |

---

## Discovered Issues

| Issue | Type | Priority |
|-------|------|----------|
| | | |

---

## Links

- [anthropic-recommended-transition.md](designs/anthropic-recommended-transition.md)
- [reference/](reference/)
