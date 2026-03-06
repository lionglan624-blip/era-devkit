# Feature 264: SKILL 構造統一 - 保守性と可読性の向上

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)

> **すべての SKILL は統一された構造を持ち、学習コストと保守コストを最小化する。**

F263 で Procedure/Quality セクションを追加したが、kojo-writing との構造差が残存。
長期的な保守性のため、SKILL 間の構造を統一する。

### Problem (Current Issue)

F200-250 の教訓: 部分的改善は将来の技術的負債になる。

| 観点 | kojo-writing | erb-syntax | engine-dev |
|------|:------------:|:----------:|:----------:|
| Quality 構造 | Required/Recommended/NG | Flat table | Flat table |
| Constraints | あり | なし | なし |

※ F263 で erb-syntax/engine-dev に Procedure/Quality セクション追加済み (ただし Flat table 形式)

**影響**:
- SKILL 間で学習パターンが異なる
- 「Constraints がない = 制約がない」と誤解されるリスク
- 新 SKILL 追加時の参照モデルが曖昧

### Goal (What to Achieve)

1. **Quality 階層化**: erb-syntax/engine-dev を Required/NG 構造に統一
2. **Constraints 明示化**: 各 SKILL に明示的な禁止事項セクション追加
3. **構造テンプレート定義**: 新 SKILL 作成時の参照モデル確立

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | erb-syntax Quality に Required 階層 | code | Grep | contains | "### Required Items" | [x] |
| 2 | erb-syntax Quality に NG 階層 | code | Grep | contains | "### NG Items" | [x] |
| 3 | erb-syntax に Constraints セクション | code | Grep | contains | "## Constraints" | [x] |
| 4 | engine-dev Quality に Required 階層 | code | Grep | contains | "### Required Items" | [x] |
| 5 | engine-dev Quality に NG 階層 | code | Grep | contains | "### NG Items" | [x] |
| 6 | engine-dev に Constraints セクション | code | Grep | contains | "## Constraints" | [x] |
| 7 | erb-syntax RETURN Rules セクション削除 | code | Grep | not_contains | "**RETURN Rules**" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,7 | erb-syntax SKILL 構造統一 | [x] |
| 2 | 4,5,6 | engine-dev SKILL 構造統一 | [x] |

<!-- AC:Task Mapping - Same-file changes grouped for efficiency (F263 precedent) -->

---

## Design Notes

### Quality 階層構造 (kojo-writing 参照)

```markdown
## Quality

### Required Items

- [ ] 条件1
- [ ] 条件2

### Recommended Items

- [ ] 推奨1

### NG Items

| Situation | NG Expression |
|-----------|---------------|
| 状況1 | 禁止パターン1 |
```

### erb-syntax Constraints (必須)

以下は必須で含める。Implementer は追加可能。

| Constraint | Rationale |
|------------|-----------|
| Bare RETURN 禁止 | 既存 RETURN Rules の内容を移動 (Task 1 で元セクション削除もカバー) |
| 未宣言変数使用禁止 | ランタイムエラー防止 |
| GOTO 濫用禁止 | 可読性確保 |

### engine-dev Constraints (必須)

以下は必須で含める。Implementer は追加可能。

| Constraint | Rationale |
|------------|-----------|
| GlobalStatic 直接変更禁止 (テスト外) | DI パターン維持 |
| 例外フロー制御禁止 | パフォーマンスと可読性 |
| インターフェース変更時の全実装更新必須 | 型安全性 |

---

## Review Notes

- F263 Post-Review で発見された構造不統一の解決
- F200-250 の「やりきらなかった」パターンの回避

## 残課題

- **testing SKILL の Quality 階層化**: Philosophy「すべての SKILL」に含まれるが、testing は「テスト手法」を記述する SKILL で性質が異なる。Required/Recommended/NG 構造の適用可否を別 Feature で検討

**→ Phase 28 Task 6 として追跡**: [full-csharp-architecture.md](designs/full-csharp-architecture.md) Phase 28: Documentation

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-29 | init | initializer | Status: PROPOSED → WIP | READY:264:infra |
| 2025-12-29 | impl | implementer | Task 1: erb-syntax 構造統一 | SUCCESS |
| 2025-12-29 | impl | implementer | Task 2: engine-dev 構造統一 | SUCCESS |
| 2025-12-29 | verify | static | AC 1-7 Grep verification | PASS:7/7 |
| 2025-12-29 | review | feature-reviewer | Post-review | READY |

---

## Links

- [feature-263.md](feature-263.md) - 前提 Feature (Procedure/Quality 追加)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md) - 構造参照モデル
