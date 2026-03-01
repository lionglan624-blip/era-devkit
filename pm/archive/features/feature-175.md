# Feature 175: Test Workflow Refactoring (TDD Alignment)

## Status: [CANCELLED] → Feature 202 に統合

## Type: infra

## Background

### Problem

Feature 160-165 でテストインフラ整備完了したが、/imple ワークフローに問題あり:

| 問題 | 現状 | 理想 |
|------|------|------|
| Phase順序 | Regression → AC | AC → Regression (TDD的) |
| Debug後 | 次Phaseへ進む | **ループして再検証** |
| Smoke重複 | Hook + Agent両方 | ACに統合 |
| Complete根拠 | Agent判定のみ | **ログ確認** |

### Goal

1. TDD に沿ったワークフローに変更
2. Smoke を AC に統合 (重複削除)
3. Debug 後のループ処理を追加
4. logs/ の結果を Complete 根拠に

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Target File | Status |
|:---:|-------------|------|---------|----------|-------------|:------:|
| 1 | Hook Smoke削除 | code | not_contains | "# 4. Smoke Test" | .claude/hooks/post-code-write.ps1 | [ ] |
| 2 | smoke-tester.md 削除 | file | not_exists | - | .claude/agents/smoke-tester.md | [ ] |
| 3 | imple.md smoke-tester記述削除 | code | not_contains | "smoke-tester" | .claude/commands/imple.md | [ ] |
| 4 | Phase 6 AC Verify記述 | code | contains | "## Phase 6: AC Verify" | .claude/commands/imple.md | [ ] |
| 5 | Phase 7 Regression記述 | code | contains | "## Phase 7: Regression" | .claude/commands/imple.md | [ ] |
| 6 | Debug→Phase 6 ループ記述 | code | contains | "→ Debug → Phase 6" | .claude/commands/imple.md | [ ] |
| 7 | AC logs/出力先記述 | code | contains | "logs/ac/feature-{ID}/" | .claude/commands/imple.md | [ ] |
| 8 | ac-tester CRASH Status追加 | code | contains | "| CRASH |" | .claude/agents/ac-tester.md | [ ] |
| 9 | 旧Hook削除確認 | file | not_exists | - | .claude/hooks/post-erb-write.ps1 | [ ] |
| 10 | 新Hook存在確認 | file | exists | - | .claude/hooks/post-code-write.ps1 | [ ] |
| 11 | Hook内C#パターンマッチ | code | contains | "engine[/\\\\].*\\\\.cs" | .claude/hooks/post-code-write.ps1 | [ ] |
| 12 | Template kojo/contains基準 | code | contains | "kojo" | Game/agents/reference/feature-template.md | [ ] |
| 13 | Template contains理由記述 | code | contains | "行単位" | Game/agents/reference/feature-template.md | [ ] |
| 14 | settings.json hook更新 | code | contains | "post-code-write" | .claude/settings.json | [ ] |
| 15 | ac-tester exists Matcher | code | contains | "exists" | .claude/agents/ac-tester.md | [ ] |

---

## Tasks

| Task# | AC# | Description | Depends | Status |
|:-----:|:---:|-------------|:-------:|:------:|
| 1 | 9 | 旧Hook削除 (post-erb-write.ps1) | - | [ ] |
| 2 | 10 | 新Hook作成 (post-code-write.ps1) | 1 | [ ] |
| 3 | 14 | settings.json の hook パス更新 | 2 | [ ] |
| 4 | 1 | post-code-write.ps1 から Smoke セクション削除 | 2 | [ ] |
| 5 | 11 | post-code-write.ps1 に C# ビルド対応追加 | 2 | [ ] |
| 6 | 2 | smoke-tester.md 削除 | - | [ ] |
| 7 | 3 | imple.md から smoke-tester 記述削除 | - | [ ] |
| 8 | 4 | imple.md Phase番号連番化 + Phase 5 AC Verify | - | [ ] |
| 9 | 5 | imple.md Phase 6 Regression | 8 | [ ] |
| 10 | 6 | imple.md Debug後ループ記述追加 | 8 | [ ] |
| 11 | 7 | imple.md Log Verification Phase追加 | 8 | [ ] |
| 12 | 8 | ac-tester.md に CRASH判定追加 | - | [ ] |
| 13 | 15 | ac-tester.md に exists/not_exists Matcher追加 | - | [ ] |
| 14 | 12 | feature-template.md に Matcher使い分け基準(kojo)追加 | - | [ ] |
| 15 | 13 | feature-template.md に Matcher使い分け基準(contains)追加 | 14 | [ ] |

<!-- AC:Task 1:1 Rule: Each AC maps to exactly one Task -->

---

## Design

### 新 /imple フロー

```
Phase 1: Initialize
    ↓
Phase 2: Investigation
    ↓
Phase 3: Test Creation (RED確認)
    ↓
Phase 4: Implementation
    │ 毎Edit: BOM + Build + Strict (Hook)
    ↓
Phase 5: AC Verify (ac-tester)
    │ → logs/ac/feature-{ID}/ に結果出力
    ├─ CRASH → Debug → Phase 5 へループ (max 3)
    │   └─ 3回超過 → STOP + User escalate
    ├─ FAIL → Debug → Phase 5 へループ (max 3)
    │   └─ 3回超過 → STOP + User escalate
    └─ PASS ↓
    ↓
Phase 6: Regression (regression-tester)
    │ → logs/regression/ に結果出力
    ├─ FAIL → Debug → Phase 6 へループ (max 3)
    │   └─ 3回超過 → STOP + User escalate
    └─ PASS ↓
    ↓
Phase 7: Log Verification & Report
    │ logs/ 確認 → 全PASS根拠でレポート出力
    ↓
Phase 8: User Approval
    │ ユーザー承認待ち (y/n)
    ├─ NO → STOP
    └─ YES ↓
    ↓
Phase 9: Finalize
    │ Status [DONE], AC [x] 更新
    ↓
Phase 10: Commit
```

### 削除するもの

| 対象 | ファイル | 理由 |
|------|----------|------|
| Hook Smoke | post-erb-write.ps1 Section 4 | ACに統合 |
| smoke-tester agent | smoke-tester.md | ACに統合 |

### 変更するもの

| 対象 | 変更前 | 変更後 | 理由 |
|------|--------|--------|------|
| Hook名 | post-erb-write.ps1 | post-code-write.ps1 | C#対応追加 |

### Hook ビルド対象 (post-erb-write.ps1 → post-code-write.ps1)

| ファイル種類 | ビルド対象 | テスト |
|-------------|-----------|--------|
| *.erb/*.erh | Headless | --strict-warnings |
| engine/*.cs | Headless + Tests | dotnet test |

```powershell
# C# (engine/) の場合
if ($path -match 'engine[/\\].*\.cs$') {
    # Build Headless
    dotnet build $headlessProj --verbosity quiet
    # Run C# unit tests
    dotnet test $testsProj --verbosity quiet
}
```

### ac-tester 変更

```markdown
## Output

| Status | Format |
|--------|--------|
| PASS | `OK:AC{N}` |
| FAIL | `ERR:AC{N}:{matcher}:{expected}:{actual}` |
| CRASH | `CRASH:AC{N}:{error}` |  ← 新規追加
| BLOCKED | `BLOCKED:AC{N}:{reason}` |

## Judgment

| Condition | Status |
|-----------|--------|
| exit ≠ 0 | CRASH |
| exit = 0 + Matcher FAIL | FAIL |
| exit = 0 + Matcher PASS | PASS |
```

### Matcher使い分け基準

| Feature Type | Matcher | 理由 |
|--------------|---------|------|
| kojo | `contains` × 複数行 | 口上は行単位で複数テスト、順序・空白変動あり |
| erb/engine | `equals` | コード検証は厳密一致が理想 |
| infra (MD) | `contains` | 文書は空白・改行変動あり |
| file existence | `exists`/`not_exists` | ファイル存在確認 |

**kojo例**: 1ACに複数contains
```markdown
| 1 | 美鈴思慕 | output | contains | "最近一緒にいると" |
|   |          |        | contains | "ドキドキする" |
```

**erb例**: 厳密一致
```markdown
| 1 | 変数設定 | variable | equals | "12345" |
```

### Complete 判定基準

```
Phase 7 (Log Verification) で確認:

1. logs/ac/feature-{ID}/ に全ACの PASS ログが存在
2. logs/regression/ に最新実行の PASS ログが存在
3. Build 成功 (exit 0)
4. Strict 警告なし (exit 0)

→ 全条件満たせばレポート出力、User approval待ち
```

---

## Links

- [feature-161.md](feature-161.md) - Test Folder Structure
- [feature-163.md](feature-163.md) - AC/Regression Protection Hooks
- [feature-174.md](feature-174.md) - Test CLI Unification

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 | CREATE | orchestrator | Feature 175 作成 | PROPOSED |
| 2025-12-21 | REDESIGN | orchestrator | TDD Alignment に再設計 | PROPOSED |
| 2025-12-21 | REVIEW | orchestrator | AC/Task 1:1準拠、Matcher明確化 | PROPOSED |
| 2025-12-21 | UPDATE | orchestrator | Phase連番化、Matcher基準追加 | PROPOSED |
| 2025-12-21 | REVIEW | opus | AC表にMethod列追加、Target File明記、Task 11-12追加 | PROPOSED |
| 2025-12-21 | UPDATE | opus | AC分割(2,8,10)、Task依存順序追加、Phase 7-10分離（承認→Finalize→Commit） | PROPOSED |
| 2025-12-21 | REFACTOR | opus | AC連番化(15件)、AC表にTarget File列統合、Method列削除 | PROPOSED |
| 2025-12-21 | ALIGN | ac-task-aligner | Task分割(9,10→2タスク / 12,13→2タスク)、Task連番化(15件)、依存順序最適化 | FIXED |
| 2025-12-21 | VALIDATE | ac-validator | AC Expected値具体化、全15件TDD Ready確認 | VALIDATED |
| 2025-12-21 | INIT | initializer | Status → [WIP], Feature initialized for implementation | READY |
