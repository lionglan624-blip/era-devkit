# Feature 238: /kojo-init Template Modernization

## Status: [DONE]

## Type: infra

## Keywords (検索用)

`kojo-init`, `template`, `AC`, `SSOT`, `testing SKILL`, `feature-template`

---

## Background

### Philosophy (Mid-term Vision)

全スラッシュコマンドが生成するファイルは SSOT (Single Source of Truth) に準拠した形式を持つ。これにより、生成された Feature ファイルが他のワークフロー（/do, /fl, ac-tester 等）とシームレスに連携する。

### Problem (Current Issue)

F223 運用見直し後のレビューで、/kojo-init コマンドが生成する Feature テンプレートに複数の SSOT 違反・形式不整合が発見された。

| # | 問題 | 深刻度 | 現状 |
|---|------|:------:|------|
| 1 | AC テンプレートに Method 列がない | 高 | testing SKILL / feature-template.md 違反 |
| 2 | Philosophy セクションがない | 中 | feature-template.md 違反 |
| 3 | COM Name Lookup Table が SSOT 違反 (10 COM のみ、Skill と重複) | 中 | Skill(kojo-writing) が SSOT |
| 4 | Execution Log 形式が古い (Timestamp 欠落) | 低 | feature-template.md 違反 |
| 5 | Links に kojo-writing SKILL がない | 低 | SSOT 参照不足 |

### Goal (What to Achieve)

/kojo-init が生成する Feature テンプレートを以下に準拠させる:
1. testing SKILL の AC Definition Format
2. feature-template.md の構造
3. index-features.md の実際のテーブル形式

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | AC テンプレートに Method 列あり | file | Grep | contains | "Char \| Type \| Method \| Matcher" | [x] |
| 2 | Philosophy セクションあり | file | Grep | contains | "### Philosophy" | [x] |
| 3 | COM Lookup Table 削除 | file | Grep | not_contains | "COM Name Lookup Table" | [x] |
| 4 | Execution Log に Timestamp 列あり | file | Grep | contains | "Timestamp \| Event \| Agent" | [x] |
| 5 | Links に kojo-writing SKILL あり | file | Grep | contains | "kojo-writing/SKILL.md" | [x] |
| 6 | ビルド成功 | build | dotnet | succeeds | - | [x] |

### AC Details

**AC1**: kojo-init.md 内の Feature テンプレート AC テーブルヘッダに Method 列が含まれる（kojo-specific format 維持）
**Test**: `Grep("Char | Type | Method | Matcher", ".claude/commands/kojo-init.md")`
**Expected**: 1+ matches (Method 列が Type と Matcher の間に存在、Char 列も維持)

**AC2**: Feature テンプレートに Philosophy セクションが含まれる
**Test**: `Grep("### Philosophy", ".claude/commands/kojo-init.md")`
**Expected**: 1+ matches (セクションヘッダ存在確認。subtitle "(Mid-term Vision)" は任意だが推奨)

**AC3**: COM Name Lookup Table セクションが削除されている
**Test**: `Grep("COM Name Lookup Table", ".claude/commands/kojo-init.md")`
**Expected**: No matches (string should not exist in file)

**AC4**: Execution Log テンプレートに Timestamp 列が含まれる（5列形式）
**Test**: `Grep("Timestamp | Event | Agent", ".claude/commands/kojo-init.md")`
**Expected**: 1+ matches (Timestamp と Event 列が存在)

**AC5**: Links セクションに kojo-writing SKILL へのリンクが含まれる
**Test**: `Grep("kojo-writing/SKILL.md", ".claude/commands/kojo-init.md")`
**Expected**: 1+ matches (部分パスマッチ - 相対パス形式は柔軟に許容)

**AC6**: エンジンビルドが成功する
**Test**: `dotnet build engine/uEmuera.Headless.csproj`
**Expected**: Exit code 0

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add Method column to AC template header | [○] |
| 2 | 2 | Add Philosophy section to Feature template | [○] |
| 3 | 3 | Delete COM Name Lookup Table section | [○] |
| 4 | 4 | Add Timestamp column to Execution Log template | [○] |
| 5 | 5 | Add kojo-writing SKILL link to Links section | [○] |
| 6 | 6 | Verify build succeeds | [○] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | INIT | initializer | Status [PROPOSED]→[WIP] | OK |
| 2025-12-27 | COMPLETE | finalizer | All 6 ACs verified, Status [WIP]→[DONE] | OK |

---

## Dependencies

None

---

## Links

- [kojo-init.md](../../.claude/commands/kojo-init.md) - Target file
- [feature-template.md](reference/feature-template.md) - Template reference
- [testing SKILL](../../.claude/skills/testing/SKILL.md) - AC format SSOT
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md) - COM reference SSOT

---

## Notes

### Out of Scope

以下は本 Feature のスコープ外:

1. **Task 構造の AC:Task 1:1 違反** - kojo Type は特例として許容（F194 等で同構造が使用済み）
2. **AC#12 Regression Expected 形式** - 実運用で問題が出たら別途対応
3. **design-first ロジックの詳細化** - F233 派生 Feature で対応予定

### Implementation Notes

**AC テンプレート変更箇所** (L95):

Before:
```markdown
| AC# | Char | Type | Matcher | Expected | MockRand | Status |
```

After:
```markdown
| AC# | Char | Type | Method | Matcher | Expected | MockRand | Status |
|:---:|------|------|--------|---------|----------|:--------:|:------:|
| 1 | K1美鈴 | output | --unit | contains | "{auto}" | [0] | [ ] |
```

**Note**: kojo-specific AC format。標準形式（testing SKILL）は Description 列を使用し MockRand を持たない。kojo テンプレートは Char（キャラクター別）と MockRand（乱数固定用）列を追加拡張。Method 列は kojo output type では `--unit` を使用。

**Philosophy セクション追加** (Background の最初のサブセクションとして挿入、Problem の前に配置):

```markdown
## Background

### Philosophy (Mid-term Vision)     ← 新規追加
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem                          ← 既存（変更なし）
COM_{NUM} ({name}) lacks Phase 8d quality dialogue for all characters.

### Goal                             ← 既存（変更なし）
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context                          ← 既存（変更なし）
- Phase 8d: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character
```

**Note**: Philosophy のみ新規挿入。既存の Problem/Goal/Context は変更しない。Philosophy は policy statement（SSOT 準拠）で、Phase 8d factual content は Context に維持。

**COM Name Lookup Table 削除** (L150-164):
- 10-COM テーブルを完全削除
- Step 1 の `Skill(kojo-writing)` 参照が SSOT として機能

**Note**: index-features.md の Type 列追加は見送り（/next と整合維持）。

**Execution Log 形式**: 5列形式（Timestamp, Event, Agent, Action, Result）は feature-template.md に準拠。既存 kojo features（F194 等）は 4列形式だが、新規生成分から 5列形式を適用。
