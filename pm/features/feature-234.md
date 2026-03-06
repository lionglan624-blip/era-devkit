# Feature 234: Plan Command Phase Restructure

## Status: [DONE]

## Type: infra

## Keywords (検索用)

`plan`, `phase`, `subagent`, `error-handling`, `recovery`, `todowrite`, `approval-gate`, `infra`

---

## Background

### Philosophy (Mid-term Vision)

全スラッシュコマンドが一貫した構造パターンを持つ: 明確な Phase 定義、明示的な subagent 契約、エラーハンドリング、Recovery Procedures。これにより予測可能な実行と保守性の向上を実現する。

### Problem (Current Issue)

/plan コマンドに以下の構造的欠陥が存在 ([F233 Issue Inventory](feature-233.md) より):

| ID | 問題 | 現状 | リスク |
|:--:|------|------|--------|
| PL1 | Phase/Step 混在 | Phase 1-3 と Step 1-4 が混在 | 実行順序不明確 |
| PL2 | Subagent 戻り値未定義 | goal-setter 等の成功/失敗条件が不明 | エラー時の対応不明 |
| PL3 | Error Handling なし | エラー時の対応セクションなし | 失敗時に継続リスク |
| PL4 | Recovery Procedures なし | 中断からの復帰手順なし | 再開不可 |
| PL5 | TodoWrite 統合なし | 進捗追跡の仕組みなし | 進捗不可視 |
| PL6 | Approval Gate 暗黙的 | DRAFT→APPROVED の承認プロセス不明確 | 未承認で進行リスク |
| PL7 | Feature Granularity Guide 内容重複 | plan.md と feature-template.md で同内容重複 | 保守性低下 |

### Goal (What to Achieve)

/plan コマンドを /do と同等の構造品質に引き上げる:
1. Phase/Step を明確に分離し、実行順序を明確化
2. Subagent 戻り値を定義（成功/失敗条件）
3. Error Handling セクションを追加
4. Recovery Procedures を定義
5. TodoWrite 統合を必須化
6. Approval Gate を明示化
7. Feature Granularity Guide を SSOT 参照に変更（plan.md の重複内容を削除し feature-template.md への参照に置換）

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 定義が明確 (4以上) | code | Grep | gte | 4 | [x] |
| 2 | Subagent 戻り値定義あり | code | Grep | contains | "\| Returns \|" | [x] |
| 3 | Error Handling セクションあり | code | Grep | contains | "Error Handling" | [x] |
| 4 | Recovery Procedures あり | code | Grep | contains | "Recovery" | [x] |
| 5 | TodoWrite 統合必須 | code | Grep | contains | "TodoWrite" | [x] |
| 6 | Approval Gate 明示 | code | Grep | contains | "Approval" | [x] |
| 7 | Granularity Guide SSOT参照 | code | Grep | contains | "feature-template.md" | [x] |
| 8 | ビルド成功 | build | dotnet | succeeds | - | [x] |

### AC Details

- AC#1: `Grep pattern="^## Phase [0-9]" path=".claude/commands/plan.md" output_mode="count"` で 4 以上 (現plan.mdは3 Phase、再構成後は4 Phase以上)
- AC#2: `Grep "| Returns |" plan.md` で subagent 戻り値テーブル形式が存在
- AC#3: `Grep "## Error Handling" plan.md` でセクションが存在
- AC#4: `Grep "Recovery" plan.md` で Recovery Procedures が存在
- AC#5: `Grep "TodoWrite" plan.md` で TodoWrite 統合が記述
- AC#6: `Grep "Approval" plan.md` で承認ゲートが明示
- AC#7: `Grep "feature-template.md" plan.md` で SSOT 参照が存在
- AC#8: `dotnet build` 成功

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Restructure plan.md Phase/Step hierarchy to match do.md pattern | [x] |
| 2 | 2 | Add subagent return value definitions | [x] |
| 3 | 3 | Add Error Handling section | [x] |
| 4 | 4 | Add Recovery Procedures section | [x] |
| 5 | 5 | Add TodoWrite integration requirement | [x] |
| 6 | 6 | Add explicit Approval Gate process | [x] |
| 7 | 7 | Replace Granularity Guide with SSOT reference | [x] |
| 8 | 8 | Verify build succeeds | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | Completion Finalization | finalizer | Mark DONE, all tasks/ACs [x] | READY_TO_COMMIT |

---

## Dependencies

- Parent: F233 (/plan-/next Workflow Audit)

---

## Links

- [F233](feature-233.md) - Parent audit feature
- [/plan command](../../.claude/commands/plan.md)
- [/do command](../../.claude/commands/do.md) - Reference for structure pattern
