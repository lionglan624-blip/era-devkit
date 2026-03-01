# Feature 292: eraTW 分岐構造全 COM 比較分析

## Status: [DONE]

## Type: infra

## Background

### Philosophy
eraTW の分岐構造を理解し、紅魔館 protoNTR の kojo 品質を eraTW レベルに引き上げる

### Problem
現在の kojo 実装は TALENT 4段階分岐のみ。eraTW には以下の追加分岐がある:
- 射精直後 (TCVAR:104)
- 射精中 (NOWEX:MASTER:11) + 精飲経験分岐
- 初めて (FIRSTTIME)
- 時姦 (FLAG:時間停止)
- 屈服度 (MARK:不埒刻印)

これらの分岐が各 COM でどのように実装されているか、網羅的な把握ができていない。

### Goal
1. eraTW 霊夢の全 COM における分岐構造を抽出・整理
2. 現行実装との差分を明確化
3. 追加すべき分岐パターンを roadmap に反映

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | eraTW 霊夢 全 COM 分岐構造レポート作成 | file | Glob | exists | Game/agents/reference/eratw-branch-analysis.md | [x] |
| 2 | 差分表セクション存在 | code | Grep(eratw-branch-analysis.md) | contains | "## 差分表" | [x] |
| 3 | roadmap 更新提案セクション存在 | code | Grep(eratw-branch-analysis.md) | contains | "## Roadmap 更新提案" | [x] |

### AC Details

**AC1**: `Glob(Game/agents/reference/eratw-branch-analysis.md)` → file exists
**AC2**: `Grep("## 差分表", Game/agents/reference/eratw-branch-analysis.md)` → matches
**AC3**: `Grep("## Roadmap 更新提案", Game/agents/reference/eratw-branch-analysis.md)` → matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | eraTW 霊夢の全 COM から全分岐条件を抽出・分類しレポート作成 | [x] |
| 2 | 2 | 現行 TALENT 分岐との差分表作成 | [x] |
| 3 | 3 | content-roadmap.md への追加提案セクション作成 | [x] |

---

## Analysis Scope

### 対象 COM 範囲
- eraTW 霊夢に存在する全 COM（COMF ファイル全件）
- 優先順位: 口挿入系 (COM_80-85) > 訓練系 > その他

### 抽出対象の分岐条件
| 条件 | 説明 | 優先度 |
|------|------|:------:|
| NOWEX:MASTER:* | 射精状態 | 高 |
| TCVAR:104 | 射精直後 | 高 |
| TCVAR:精飲経験 | 精飲経験値 | 中 |
| FIRSTTIME | 初回 | 中 |
| MARK:* | 刻印状態 | 中 |
| ABL:* | 能力値分岐 | 低 (※8eと重複注意) |
| その他 | 調査で発見次第追加 | - |

※ FLAG:時間停止 は実装予定なし、対象外

---

## Roadmap 追加時の重複回避

**既存計画との重複を避けること。以下は既に roadmap/設計に含まれている:**

| 計画済み | Phase | 参照 | 状態 |
|----------|:-----:|------|:----:|
| ABL/TALENT 分岐 (感度、経験値) | 8e (C3) | F189 (PROPOSED), F216/F217 (DONE) | 基盤済 |
| FAV_* (NTR段階) 分岐 | 8h (C6) | content-roadmap.md | 計画中 |
| Event 口上 (eraTW形式) | 8g (C5) | content-roadmap.md | 計画中 |
| v0.7 監査修正 (E1主客/E2分岐) | v0.7 | v0.7-audit-fix.md | 計画中 |

**F292 の進め方:**

1. eraTW 全 COM の分岐構造を徹底的に調査
2. 調査結果から分岐パターンを抽出・分類
3. 既存計画と照合し、未計画の分岐パターンを特定
4. 追加すべき分岐を決定し、roadmap に反映

**既知の未計画パターン (例 - 調査後に確定):**
- `NOWEX:MASTER:*` (射精状態)
- `TCVAR:104` (射精直後)
- `TCVAR:精飲経験` (精飲経験)
- `FIRSTTIME` (初回)

※ 上記は COM_81 で確認された例。他 COM の調査で追加パターンが見つかる可能性あり。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 11:53 | START | implementer | Task 1 | - |
| 2026-01-01 11:53 | ANALYZE | implementer | Created Python analysis script | - |
| 2026-01-01 11:53 | EXTRACT | implementer | Analyzed eraTW M_KOJO_K1_コマンド.ERB | 133 COMs detected |
| 2026-01-01 11:56 | CREATE | implementer | Created eratw-branch-analysis.md | SUCCESS |
| 2026-01-01 11:56 | END | implementer | Task 1-3 | SUCCESS |

---

## Links
- [index-features.md](index-features.md)
- [content-roadmap.md](content-roadmap.md)
- eraTW: C:\Users\siihe\OneDrive\同人ゲーム\eraTW4.920
