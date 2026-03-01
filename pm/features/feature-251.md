# Feature 251: Phase 8 Summary Automation

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
index-features.md の Phase 8 Summary は kojo-mapper から自動生成されるべき。手動更新は SSOT 違反、E

### Problem (Current Issue)
- F240 で「Phase 8 Summary 削除は別 Feature で実裁E��定！Eojo-mapper に COM カチE��リ別進捗機�Eを追加後）」と延朁E
- F166/F167 で kojo-mapper に 14 カチE��リ検�E機�Eは実裁E��み
- しかぁEPhase 8 Summary の自動化/削除 Feature は未作�E
- index-features.md の Phase 8 Summary は手動更新のまま�E�ESOT 違反継続中�E�E

### Goal (What to Achieve)
1. index-features.md から手動 Phase 8 Summary を削除
2. kojo-mapper --coverage 出力を SSOT として使用
3. `/plan`、`/next`、`/do` に kojo-mapper --coverage への参�Eを追加

### Session Context
- **Origin**: F240 延期事頁E�E正弁EFeature 匁E
- **Dependencies**: F166 (14カチE��リ検�E), F167 (検証完亁E
- **Related**: F252 (COM-Kojo Consistency Audit - CANCELLED)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 8 Summary セクション削除 | file | Grep | not_contains | "## Phase 8 Summary" | [x] |
| 2 | kojo-mapper 参�Eリンク追加 | file | Grep | contains | "src/tools/kojo-mapper/kojo_mapper.py" | [x] |
| 3 | content-roadmap.md 進捗リンク更新 | file | Grep | contains | "tools/kojo-mapper" | [x] |
| 4 | content-roadmap.md 古ぁE��ンク削除 | file | Grep | not_contains | "#phase-8-サマリー" | [x] |
| 5 | kojo-mapper --coverage 実行�E劁E| output | bash | contains | "=== Kojo Coverage Report ===" | [x] |
| 6 | Current Phase セクション保�E | file | Grep | contains | "## Current Phase: 8d" | [x] |
| 7 | Links セクション保�E | file | Grep | contains | "## Links" | [x] |
| 8 | /plan に kojo-mapper --coverage 追加 | file | Grep | contains | "--coverage" | [x] |
| 9 | /next に kojo-mapper --coverage 追加 | file | Grep | contains | "--coverage" | [x] |
| 10 | /do に kojo-mapper --coverage 追加 | file | Grep | contains | "--coverage" | [x] |

### AC Details

**AC1**: index-features.md から `## Phase 8 Summary` セクション�E�テーブル含む�E�を削除

**AC2**: index-features.md に kojo-mapper 実行コマンドへの参�Eを追加
```markdown
**Progress**: `python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --coverage`
```

**AC3**: content-roadmap.md の "Progress tracking" リンクを更新

**AC4**: content-roadmap.md から古ぁE`#phase-8-サマリー` アンカーリンクを削除

**AC5**: kojo-mapper --coverage が正常動作すること
```bash
python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --coverage
```
出力に `=== Kojo Coverage Report ===` が含まれること

**AC6**: index-features.md の `## Current Phase: 8d` セクションが保�EされてぁE��こと

**AC7**: index-features.md の `## Links` セクションが保�EされてぁE��こと

**AC8**: `/plan` コマンドが kojo-mapper --coverage を実行して進捗を確認すること
- Target: `.claude/commands/plan.md`

**AC9**: `/next` コマンドが kojo-mapper --coverage を実行して進捗を確認すること
- Target: `.claude/commands/next.md`

**AC10**: `/do` コマンドが kojo-mapper --coverage を実行して進捗を確認すること
- Target: `.claude/commands/do.md`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | index-features.md から ## Phase 8 Summary セクション�E�テーブル含む�E�を削除 | [○] |
| 2 | 2 | index-features.md の ## Current Phase セクション後に Progress Tracking セクション挿入 | [○] |
| 3 | 3 | content-roadmap.md line 234 の Progress tracking リンクめEsrc/tools/kojo-mapper/ に変更 | [○] |
| 4 | 4 | content-roadmap.md line 234 から #phase-8-サマリー アンカーを削除 | [○] |
| 5 | 5 | kojo-mapper --coverage 実行して出力を確誁E| [○] |
| 6 | 6 | index-features.md に ## Current Phase: 8d が残ってぁE��ことを確誁E| [○] |
| 7 | 7 | index-features.md に ## Links が残ってぁE��ことを確誁E| [○] |
| 8 | 8 | plan.md Phase 1.1 (Read Current State) セクションに kojo-mapper --coverage 実行スチE��プ追加 | [○] |
| 9 | 9 | next.md Priority 2 セクションの Phase 8 サマリー参�EめEkojo-mapper に置揁E| [○] |
| 10 | 10 | do.md Phase 8: Report & Approval (Step 8.2) に kojo-mapper --coverage 進捗確認追加 | [○] |

---

## Technical Notes

### Before (index-features.md)

```markdown
## Phase 8 Summary

| Range | Category | COM Count | Done | Remaining |
|-------|----------|:---------:|:----:|:---------:|
| 0-11 | 愛撫系 | 12 | 12 | 0 |
| 20-21 | キス系 | 2 | 2 | 0 |
| 40-48 | 道�E系 | 9 | 9 | 0 |
| 60-72 | 挿入系 | 13 | 4 | 9 |  ↁE手動更新忁E��E
...
```

### After (index-features.md)

```markdown
## Progress Tracking

**Kojo Coverage**: Run `python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --coverage`

See [kojo-mapper](../../src/tools/kojo-mapper/) for detailed coverage analysis.
```

### Before (content-roadmap.md line 234)

```markdown
**Progress tracking**: [index-features.md](index-features.md#phase-8-サマリー)
```

### After (content-roadmap.md line 234)

```markdown
**Progress tracking**: See [kojo-mapper](../../src/tools/kojo-mapper/) for coverage analysis
```

### kojo-mapper --coverage 出力侁E

```
=== Kojo Coverage Report ===

Category Summary:
Total: 16 categories

GLOBAL: COM=1408, COUNTER=409, DAILY=38, EVENT=75, ...

K1: COM=239, COUNTER=71, DAILY=38, EVENT=20, ...
K2: COM=349, COUNTER=133, DAILY=38, EVENT=22, ...
...

=== KU Integration Verification ===
Uncategorized: 0
Overlaps: 0 (first-match-wins prevents overlaps)
4966 total functions = 4906 categorized + 60 utility + 0 uncategorized
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 | Finalization | finalizer | Mark all ACs [x], all Tasks [○], status [DONE] | READY_TO_COMMIT |

---

## Links

- [index-features.md](../index-features.md)
- [feature-240.md](feature-240.md) - 延期�E
- [feature-166.md](feature-166.md) - kojo-mapper 14カチE��リ
- [feature-167.md](feature-167.md) - 検証完亁E
- [kojo-mapper](../../src/tools/kojo-mapper/)
