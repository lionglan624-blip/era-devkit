# Feature 175: Test Workflow Refactoring (TDD Alignment)

## Status: [CANCELLED] ↁEFeature 202 に統吁E

## Type: infra

## Background

### Problem

Feature 160-165 でチE��トインフラ整備完亁E��たが、Eimple ワークフローに問題あめE

| 問顁E| 現状 | 琁E�� |
|------|------|------|
| Phase頁E��E| Regression ↁEAC | AC ↁERegression (TDD皁E |
| Debug征E| 次Phaseへ進む | **ループして再検証** |
| Smoke重褁E| Hook + Agent両方 | ACに統吁E|
| Complete根拠 | Agent判定�Eみ | **ログ確誁E* |

### Goal

1. TDD に沿ったワークフローに変更
2. Smoke めEAC に統吁E(重褁E��除)
3. Debug 後�Eループ�E琁E��追加
4. logs/ の結果めEComplete 根拠に

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Target File | Status |
|:---:|-------------|------|---------|----------|-------------|:------:|
| 1 | Hook Smoke削除 | code | not_contains | "# 4. Smoke Test" | .claude/hooks/post-code-write.ps1 | [ ] |
| 2 | smoke-tester.md 削除 | file | not_exists | - | .claude/agents/smoke-tester.md | [ ] |
| 3 | imple.md smoke-tester記述削除 | code | not_contains | "smoke-tester" | .claude/commands/imple.md | [ ] |
| 4 | Phase 6 AC Verify記述 | code | contains | "## Phase 6: AC Verify" | .claude/commands/imple.md | [ ] |
| 5 | Phase 7 Regression記述 | code | contains | "## Phase 7: Regression" | .claude/commands/imple.md | [ ] |
| 6 | Debug→Phase 6 ループ記述 | code | contains | "ↁEDebug ↁEPhase 6" | .claude/commands/imple.md | [ ] |
| 7 | AC logs/出力�E記述 | code | contains | "logs/ac/feature-{ID}/" | .claude/commands/imple.md | [ ] |
| 8 | ac-tester CRASH Status追加 | code | contains | "| CRASH |" | .claude/agents/ac-tester.md | [ ] |
| 9 | 旧Hook削除確誁E| file | not_exists | - | .claude/hooks/post-erb-write.ps1 | [ ] |
| 10 | 新Hook存在確誁E| file | exists | - | .claude/hooks/post-code-write.ps1 | [ ] |
| 11 | Hook冁E#パターンマッチE| code | contains | "engine[/\\\\].*\\\\.cs" | .claude/hooks/post-code-write.ps1 | [ ] |
| 12 | Template kojo/contains基溁E| code | contains | "kojo" | pm/reference/feature-template.md | [ ] |
| 13 | Template contains琁E��記述 | code | contains | "行単佁E | pm/reference/feature-template.md | [ ] |
| 14 | settings.json hook更新 | code | contains | "post-code-write" | .claude/settings.json | [ ] |
| 15 | ac-tester exists Matcher | code | contains | "exists" | .claude/agents/ac-tester.md | [ ] |

---

## Tasks

| Task# | AC# | Description | Depends | Status |
|:-----:|:---:|-------------|:-------:|:------:|
| 1 | 9 | 旧Hook削除 (post-erb-write.ps1) | - | [ ] |
| 2 | 10 | 新Hook作�E (post-code-write.ps1) | 1 | [ ] |
| 3 | 14 | settings.json の hook パス更新 | 2 | [ ] |
| 4 | 1 | post-code-write.ps1 から Smoke セクション削除 | 2 | [ ] |
| 5 | 11 | post-code-write.ps1 に C# ビルド対応追加 | 2 | [ ] |
| 6 | 2 | smoke-tester.md 削除 | - | [ ] |
| 7 | 3 | imple.md から smoke-tester 記述削除 | - | [ ] |
| 8 | 4 | imple.md Phase番号連番匁E+ Phase 5 AC Verify | - | [ ] |
| 9 | 5 | imple.md Phase 6 Regression | 8 | [ ] |
| 10 | 6 | imple.md Debug後ループ記述追加 | 8 | [ ] |
| 11 | 7 | imple.md Log Verification Phase追加 | 8 | [ ] |
| 12 | 8 | ac-tester.md に CRASH判定追加 | - | [ ] |
| 13 | 15 | ac-tester.md に exists/not_exists Matcher追加 | - | [ ] |
| 14 | 12 | feature-template.md に Matcher使ぁE�Eけ基溁Ekojo)追加 | - | [ ] |
| 15 | 13 | feature-template.md に Matcher使ぁE�Eけ基溁Econtains)追加 | 14 | [ ] |

<!-- AC:Task 1:1 Rule: Each AC maps to exactly one Task -->

---

## Design

### 新 /imple フロー

```
Phase 1: Initialize
    ↁE
Phase 2: Investigation
    ↁE
Phase 3: Test Creation (RED確誁E
    ↁE
Phase 4: Implementation
    ━E毎Edit: BOM + Build + Strict (Hook)
    ↁE
Phase 5: AC Verify (ac-tester)
    ━EↁElogs/ac/feature-{ID}/ に結果出劁E
    ├─ CRASH ↁEDebug ↁEPhase 5 へルーチE(max 3)
    ━E  └─ 3回趁E�� ↁESTOP + User escalate
    ├─ FAIL ↁEDebug ↁEPhase 5 へルーチE(max 3)
    ━E  └─ 3回趁E�� ↁESTOP + User escalate
    └─ PASS ↁE
    ↁE
Phase 6: Regression (regression-tester)
    ━EↁElogs/regression/ に結果出劁E
    ├─ FAIL ↁEDebug ↁEPhase 6 へルーチE(max 3)
    ━E  └─ 3回趁E�� ↁESTOP + User escalate
    └─ PASS ↁE
    ↁE
Phase 7: Log Verification & Report
    ━Elogs/ 確誁EↁE全PASS根拠でレポ�Eト�E劁E
    ↁE
Phase 8: User Approval
    ━Eユーザー承認征E�� (y/n)
    ├─ NO ↁESTOP
    └─ YES ↁE
    ↁE
Phase 9: Finalize
    ━EStatus [DONE], AC [x] 更新
    ↁE
Phase 10: Commit
```

### 削除するも�E

| 対象 | ファイル | 琁E�� |
|------|----------|------|
| Hook Smoke | post-erb-write.ps1 Section 4 | ACに統吁E|
| smoke-tester agent | smoke-tester.md | ACに統吁E|

### 変更するも�E

| 対象 | 変更剁E| 変更征E| 琁E�� |
|------|--------|--------|------|
| Hook吁E| post-erb-write.ps1 | post-code-write.ps1 | C#対応追加 |

### Hook ビルド対象 (post-erb-write.ps1 ↁEpost-code-write.ps1)

| ファイル種顁E| ビルド対象 | チE��チE|
|-------------|-----------|--------|
| *.erb/*.erh | Headless | --strict-warnings |
| engine/*.cs | Headless + Tests | dotnet test |

```powershell
# C# (engine/) の場吁E
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
| CRASH | `CRASH:AC{N}:{error}` |  ↁE新規追加
| BLOCKED | `BLOCKED:AC{N}:{reason}` |

## Judgment

| Condition | Status |
|-----------|--------|
| exit ≠ 0 | CRASH |
| exit = 0 + Matcher FAIL | FAIL |
| exit = 0 + Matcher PASS | PASS |
```

### Matcher使ぁE�Eけ基溁E

| Feature Type | Matcher | 琁E�� |
|--------------|---------|------|
| kojo | `contains` ÁE褁E��衁E| 口上�E行単位で褁E��チE��ト、E��E���E空白変動あり |
| erb/engine | `equals` | コード検証は厳寁E��致が理想 |
| infra (MD) | `contains` | 斁E��は空白・改行変動あり |
| file existence | `exists`/`not_exists` | ファイル存在確誁E|

**kojo侁E*: 1ACに褁E��contains
```markdown
| 1 | 美鈴思�E | output | contains | "最近一緒にぁE��と" |
|   |          |        | contains | "ドキドキする" |
```

**erb侁E*: 厳寁E��致
```markdown
| 1 | 変数設宁E| variable | equals | "12345" |
```

### Complete 判定基溁E

```
Phase 7 (Log Verification) で確誁E

1. logs/ac/feature-{ID}/ に全ACの PASS ログが存在
2. logs/regression/ に最新実行�E PASS ログが存在
3. Build 成功 (exit 0)
4. Strict 警告なぁE(exit 0)

ↁE全条件満たせばレポ�Eト�E力、User approval征E��
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
| 2025-12-21 | CREATE | orchestrator | Feature 175 作�E | PROPOSED |
| 2025-12-21 | REDESIGN | orchestrator | TDD Alignment に再設訁E| PROPOSED |
| 2025-12-21 | REVIEW | orchestrator | AC/Task 1:1準拠、Matcher明確匁E| PROPOSED |
| 2025-12-21 | UPDATE | orchestrator | Phase連番化、Matcher基準追加 | PROPOSED |
| 2025-12-21 | REVIEW | opus | AC表にMethod列追加、Target File明記、Task 11-12追加 | PROPOSED |
| 2025-12-21 | UPDATE | opus | AC刁E��(2,8,10)、Task依存頁E��追加、Phase 7-10刁E���E�承認�EFinalize→Commit�E�E| PROPOSED |
| 2025-12-21 | REFACTOR | opus | AC連番匁E15件)、AC表にTarget File列統合、Method列削除 | PROPOSED |
| 2025-12-21 | ALIGN | ac-task-aligner | Task刁E��(9,10ↁEタスク / 12,13ↁEタスク)、Task連番匁E15件)、依存頁E��最適匁E| FIXED |
| 2025-12-21 | VALIDATE | ac-validator | AC Expected値具体化、�E15件TDD Ready確誁E| VALIDATED |
| 2025-12-21 | INIT | initializer | Status ↁE[WIP], Feature initialized for implementation | READY |
