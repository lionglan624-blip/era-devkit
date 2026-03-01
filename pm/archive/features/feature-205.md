# Feature 205: Log Verification & Consistency Check

## Status: [DONE]

## Type: infra + engine

## Background

### Problem

F186 実装中に発覚した問題: サブエージェントの報告を鵜呑みにして、Opus が検証を怠った。
F202/F204 で「盲目的に信頼しない」文言を追加したが、**機械的な検証の仕組みがない**。

**現状の問題**:
1. サブエージェントが虚偽報告しても検出できない
2. Consistency チェック (Phase 9) の定義が間違っている（AC vs Regression は無意味）
3. ログは `Game/logs/` に出力されているが、Opus が参照していない
4. ログディレクトリ構造が整理されていない（本番 vs デバッグが混在）

### Root Cause

- F203 Log Collection Audit: 「サブエージェントがログを参照していない」
- F204 Problem #6: 「本番ログ収集の仕組みなし → 結果報告の信頼性なし」
- Phase 9 の定義ミス: AC と Regression は別テストなので突き合わせは無意味
- imple.md Phase 8 の「疑わしい場合は自分で実行」は古い設計

### Goal

1. ログディレクトリ構造を整理（`logs/prod/` vs `logs/debug/`）
2. `verify-logs.py` でログから実際の結果を抽出（**全テストスイートを確認し問題を抽出しきる**）
3. imple.md Phase 8 を「verify-logs.py で毎回確認」に書き換え
4. Phase 9 でサブエージェント報告と verify-logs.py 結果を照合
5. Phase 10 で verify 結果を含めてユーザーに承認を要求

### F204 との整合性

| F204 AC | 状態 | 本 Feature との関係 |
|---------|:----:|---------------------|
| E2: ac-tester に Skill tool 追加 | ✅ 完了 | tools に Skill 追加済み (line 5) |
| - | - | **本 Feature**: `Skill(testing)` 参照**指示**を強化 |

**確認結果**: F204 で Skill tool は追加済み。本 Feature では「参照指示の強化」のみ実施。

---

## Log Structure (After)

```
logs/
├── prod/                    # 本番テスト結果（verify-logs.py 対象）
│   ├── ac/kojo/feature-{N}/ # AC テスト結果
│   ├── regression/          # Regression テスト結果
│   └── ac/engine/           # C# Unit Test 結果 (.trx)
└── debug/                   # デバッグ用（verify 対象外）
    ├── failed/              # FAIL 履歴（タイムスタンプ付き）
    │   ├── ac/
    │   └── regression/
    └── scratch/             # 一時的なデバッグ実行
```

| パス | 形式 | Pass Check |
|------|------|------------|
| `logs/prod/ac/**/*-result.json` | JSON | `summary.failed == 0` |
| `logs/prod/regression/*-result.json` | JSON | `passed == true` |
| `logs/prod/ac/engine/*.trx` | TRX (XML) | `outcome="Passed"` for all tests |

### Log Output Commands

| Test | Command |
|------|---------|
| C# Unit | `dotnet test engine.Tests/ --logger "trx;LogFileName=test-result.trx" --results-directory Game/logs/prod/ac/engine` |
| AC (kojo) | `--unit tests/ac/kojo/feature-{N}/` → 自動で `logs/prod/ac/` に出力 |
| Regression | `--flow tests/regression/` → 自動で `logs/prod/regression/` に出力 |

---

## Acceptance Criteria

### Part A: verify-logs.py 作成

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| A1 | verify-logs.py 存在 | file | exists | tools/verify-logs.py | [x] |
| A2 | logs/prod/ac JSON検証 (PASS時) | exit_code | equals | 0 | [x] |
| A3 | logs/prod/regression JSON検証 (PASS時) | exit_code | equals | 0 | [x] |
| A4 | logs/prod/engine TRX検証 (PASS時) | exit_code | equals | 0 | [x] |
| A5 | FAIL検出時 exit 1 | exit_code | equals | 1 | [x] |
| A6 | サマリー出力形式 | output | contains | "=== Log Verification ===" | [x] |
| A7 | ac-tester 互換出力形式 | output | contains | "OK:" | [x] |

**設計原則**: verify-logs.py の出力形式は ac-tester の報告形式（`OK:{passed}/{total}`）と統一し、Phase 9 での照合を容易にする。

**全テストスイート確認**: verify-logs.py は logs/prod/ 配下の**全ログファイル**を検証し、1件でも FAIL があれば exit 1 で問題を抽出しきる。

### Part B: 運用フロー更新

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| B1 | imple.md Phase 8 書き換え（古いセクション削除） | file | contains | "verify-logs.py" | [x] |
| B2 | imple.md Phase 9 書き換え | file | contains | "サブエージェント報告と照合" | [x] |
| B3 | imple.md Phase 10 に verify 結果含む | file | contains | "Log Verification" | [x] |
| B4 | skills/testing/SKILL.md にログ形式追加 | file | contains | "logs/prod/engine" | [x] |
| B5 | skills/testing/SKILL.md に --logger trx 追加 | file | contains | "--logger trx" | [x] |
| B6 | ac-tester.md に Skill(testing) 参照指示強化 | file | contains | "Skill(testing)" | [x] |

**Note B6**: F204 で Skill tool は追加済み。本 Feature では**参照指示**を強化する。

### Part C: エンジン修正（ログディレクトリ整理）

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| C1 | TestPathUtils.cs: logs/ → logs/prod/ | code | contains | "logs/prod/" | [x] |
| C2 | TestPathUtils.cs コメント明確化 | code | contains | "logs/prod/ac" | [x] |
| C3 | 既存ログファイル移動 | file | exists | logs/prod/ac/ | [x] |

### Part D: ビルド確認

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| D1 | dotnet build 成功 | build | succeeds | - | [x] |
| D2 | dotnet test 成功 | test | succeeds | - | [x] |
| D3 | Python syntax OK | exit_code | equals | 0 | [x] |

---

## Tasks

### Phase 0: 事前準備（ディレクトリ整備）

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 0.1 | C3 | logs/prod/ ディレクトリ構造作成 | Game/logs/prod/{ac,regression,engine} | [O] |
| 0.2 | C3 | 既存ログファイル移動（存在する場合のみ） | logs/{ac,regression,engine} → logs/prod/ | [O] |

**実行方法**:
```powershell
cd Game
# ディレクトリ作成
New-Item -ItemType Directory -Force -Path logs/prod/ac, logs/prod/regression, logs/prod/engine

# 既存ファイル移動（存在する場合のみ）
if (Test-Path logs/ac) { Move-Item logs/ac/* logs/prod/ac/ -Force }
if (Test-Path logs/regression) { Move-Item logs/regression/* logs/prod/regression/ -Force }
if (Test-Path logs/engine) { Move-Item logs/engine/* logs/prod/engine/ -Force }

# 古いディレクトリ削除（空の場合）
Remove-Item logs/ac, logs/regression, logs/engine -ErrorAction SilentlyContinue
```

### Phase 1: エンジン修正

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1.1 | C1 | TestPathUtils.cs: logs/ → logs/prod/ 変更 | engine/.../TestPathUtils.cs | [O] |
| 1.2 | C2 | TestPathUtils.cs コメント明確化 | engine/.../TestPathUtils.cs | [O] |

### Phase 2: verify-logs.py 作成

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 2.1 | A1 | verify-logs.py 基本構造作成 | tools/verify-logs.py | [O] |
| 2.2 | A2 | AC JSON 検証ロジック実装 | tools/verify-logs.py | [O] |
| 2.3 | A3 | Regression JSON 検証ロジック実装 | tools/verify-logs.py | [O] |
| 2.4 | A4 | TRX (XML) 検証ロジック実装 | tools/verify-logs.py | [O] |
| 2.5 | A5 | FAIL 検出時の exit 1 実装 | tools/verify-logs.py | [O] |
| 2.6 | A6,A7 | サマリー出力形式（ac-tester 互換） | tools/verify-logs.py | [O] |

### Phase 3: ドキュメント更新

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 3.1 | B1-B3 | imple.md Phase 8/9/10 書き換え | .claude/commands/imple.md | [O] |
| 3.2 | B4-B5 | skills/testing/SKILL.md 更新 | .claude/skills/testing/SKILL.md | [O] |
| 3.3 | B6 | ac-tester.md に Skill(testing) 参照指示強化 | .claude/agents/ac-tester.md | [O] |

### Phase 4: 検証

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 4.1 | D1 | dotnet build 成功 | - | [O] |
| 4.2 | D2 | dotnet test 成功（全テストスイート PASS 必須） | - | [O] |
| 4.3 | D3 | Python syntax OK | - | [O] |
| 4.4 | A2-A5 | verify-logs.py 動作確認（ポジ/ネガ） | - | [O] |

---

## Technical Details

### Task 1: TestPathUtils.cs 修正

**変更内容**: `logs/` → `logs/prod/` に変更

```csharp
// DeriveLogPath() 修正
// Before: logPath = logPath.Replace("tests/", "logs/");
// After:  logPath = logPath.Replace("tests/", "logs/prod/");

/// <summary>
/// テストファイルパスからログ出力パスを自動決定
/// tests/ac/kojo/test.json → logs/prod/ac/kojo/test-result.json
/// tests/regression/... → logs/prod/regression/...
/// (カレントディレクトリ = Game/)
/// </summary>
public static string DeriveLogPath(string testPath)
{
    testPath = Path.GetFullPath(testPath);

    // tests/ → logs/prod/ に変換
    var logPath = testPath.Replace("tests" + Path.DirectorySeparatorChar,
                                    "logs" + Path.DirectorySeparatorChar + "prod" + Path.DirectorySeparatorChar);
    logPath = logPath.Replace("tests/", "logs/prod/");

    // ... 残りは同じ
}
```

**DeriveFailedLogPath()**: 変更不要（既に `logs/debug/failed/`）

### Task 2: 既存ログファイル移動

```powershell
cd Game
# 既存ディレクトリを prod/ 配下に移動
mkdir -p logs/prod
Move-Item logs/ac logs/prod/ac
Move-Item logs/regression logs/prod/regression
Move-Item logs/engine logs/prod/engine
# debug/ はそのまま
```

### Task 3: verify-logs.py

**入力**: `Game/logs/prod/` 配下のログファイル
**出力**: サマリー + exit code（ac-tester 互換形式）

```python
# tools/verify-logs.py
# Usage: python tools/verify-logs.py [--dir Game/logs/prod]

# 処理:
# 1. logs/prod/ac/**/*-result.json を Glob
#    - 各ファイルの summary.failed == 0 を確認
#    - 全テストケースをカウント（問題を抽出しきる）
# 2. logs/prod/regression/*-result.json を Glob
#    - 各ファイルの passed == true を確認
# 3. logs/prod/ac/engine/*.trx を Glob
#    - XML パースして outcome="Passed" を確認
#    - 全テストケースをカウント

# 出力形式 (PASS) - ac-tester 互換:
# === Log Verification ===
# AC:         OK:160/160
# Regression: OK:24/24
# Engine:     OK:88/88
# -------------------------
# Result:     OK:272/272

# 出力形式 (FAIL) - ac-tester 互換:
# === Log Verification ===
# AC:         ERR:3|160
#   FAIL: logs/prod/ac/kojo/feature-186/test-186-K1-result.json
#   FAIL: logs/prod/ac/kojo/feature-186/test-186-K2-result.json
#   FAIL: logs/prod/ac/kojo/feature-186/test-186-K3-result.json
# Regression: OK:24/24
# Engine:     OK:88/88
# -------------------------
# Result:     ERR:3|272

# 照合例 (Phase 9):
# ac-tester報告: "OK:160/160"
# verify結果:    "AC: OK:160/160"
# → 数値一致で自動照合可能

# Exit code:
# 0 = ALL PASS (OK:N/N 形式)
# 1 = Any FAIL (ERR:N|M 形式)
```

**設計ポイント**:
1. **ac-tester 互換形式**: `OK:{passed}/{total}` / `ERR:{failed}|{total}`
2. **全テストスイート確認**: 全ログファイルを検証し、1件でも FAIL があれば exit 1
3. **照合容易性**: 数値部分を抽出すれば自動照合可能（厳密な文字列一致不要）

### Task 4: imple.md Phase 8/9/10 書き換え

**Phase 8 (書き換え)** - 「疑わしい場合は自分で実行」セクション削除:

```markdown
## Phase 8: Regression

**Dispatch regression-tester, then VERIFY with verify-logs.py.**

Dispatch: "Read .claude/agents/regression-tester.md. Feature {ID}."

### Log Verification (MANDATORY)

**毎回 verify-logs.py を実行してログを検証する。**

```bash
python tools/verify-logs.py --dir Game/logs/prod
```

| Result | Action |
|--------|--------|
| ALL PASS | Phase 9 |
| Any FAIL | debugger |

**合格基準**: `24/24 passed` (Regression Tests)
```

**Phase 9 (書き換え)**:

```markdown
## Phase 9: Consistency

**verify-logs.py 結果とサブエージェント報告を照合**

1. Phase 7 (ac-tester) の報告と verify-logs.py の AC 結果を比較
2. Phase 8 (regression-tester) の報告と verify-logs.py の Regression 結果を比較
3. 不一致があれば BLOCK

### 照合方法

**形式統一**: verify-logs.py と ac-tester は同じ `OK:{passed}/{total}` 形式を使用。

| サブエージェント報告 | verify-logs.py 結果 | 判定 |
|---------------------|--------------------:|:----:|
| `OK:160/160` | `AC: OK:160/160` | ✅ 一致 |
| `OK:24/24` | `Regression: OK:24/24` | ✅ 一致 |
| `OK:512/512` | `AC: OK:160/160` | ❌ 不一致（件数相違） |
| `OK:24/24` | `Regression: ERR:3|24` | ❌ 不一致（虚偽報告疑い） |

**照合ポイント**: 数値部分（passed/total）が一致すれば OK。
- 全て通っていることが自明なら PASS
- 件数不一致は要確認（テスト実行漏れの可能性）

**不一致時**: サブエージェントの虚偽報告の可能性。ログを確認して正しい結果を採用。
```

**Phase 10 (追記)**:

```markdown
## Phase 10: Completion

Report (Japanese):
\`\`\`
=== Feature {ID} 実装完了 ===
Type/Status/Tasks/ACs/Docs/Warnings

**Log Verification** (verify-logs.py):
AC:         OK:{N}/{M}
Regression: OK:{N}/{M}
Engine:     OK:{N}/{M}
Result:     OK:{total_passed}/{total}

Finalize と Commit? (y/n)
\`\`\`
```

### Task 5: skills/testing/SKILL.md 更新

`Log Directory Structure` セクションを追加・拡張:

```markdown
## Log Directory Structure

```
logs/
├── prod/                    # 本番テスト結果（verify-logs.py 対象）
│   ├── ac/kojo/feature-{N}/ # AC テスト結果
│   ├── regression/          # Regression テスト結果
│   └── engine/              # C# Unit Test 結果 (.trx)
└── debug/                   # デバッグ用（verify 対象外）
    ├── failed/              # FAIL 履歴
    └── scratch/             # 一時的なデバッグ実行
```

### Log Output Commands

| Test | Command |
|------|---------|
| C# Unit | `dotnet test engine.Tests/ --logger "trx;LogFileName=test-result.trx" --results-directory Game/logs/prod/ac/engine` |
| AC (kojo) | `--unit tests/ac/kojo/feature-{N}/` |
| Regression | `--flow tests/regression/` |

### Log Verification

```bash
python tools/verify-logs.py --dir Game/logs/prod
```

| Path | Format | Pass Check |
|------|--------|------------|
| `logs/prod/ac/engine/*.trx` | XML | `outcome="Passed"` for all tests |
| `logs/prod/ac/**/*-result.json` | JSON | `summary.failed == 0` |
| `logs/prod/regression/*-result.json` | JSON | `passed == true` |
```

### Task 6: ac-tester.md に Skill(testing) 参照指示強化

**前提**: F204 で Skill tool は追加済み（tools: Bash, Read, Glob, **Skill**）

**追記内容**:
```markdown
## Skill Reference

**MUST**: テスト実行前に `Skill(testing)` を参照してコマンドとログ形式を確認。

- engine Type の場合: `--logger trx` オプションでログ出力
- ログは `logs/prod/` に自動出力される
- 報告形式: `OK:{passed}/{total}` または `ERR:{failed}|{total}`
```

**変更理由**: Skill tool は既にあるが、いつ・何を参照すべきかの指示がない。

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-24 | - | Feature 作成 (F204 スコープ漏れとして) | - |
| 2025-12-24 | - | Feature 再設計 (レビュー結果反映) | - |
| 2025-12-24 | initializer | Initialize Feature 205 | READY |
| 2025-12-24 | implementer | Phase 0: Directory setup | SUCCESS |
| 2025-12-24 | implementer | Task 1.1-1.2: TestPathUtils.cs修正 | SUCCESS |
| 2025-12-24 | implementer | Task 2.1-2.6: verify-logs.py作成 | SUCCESS |
| 2025-12-24 | implementer | Task 3.1-3.3: ドキュメント更新 | SUCCESS |
| 2025-12-24 | - | BOM encoding fix (utf-8-sig) | SUCCESS |
| 2025-12-24 | ac-tester | AC Verification | OK:13/13 |
| 2025-12-24 | regression-tester | Regression Tests | OK:24/24 |
| 2025-12-24 | - | verify-logs.py照合 | OK:928/928 |
| 2025-12-24 | - | **追加修正**: regression-tester.md スコープ整理 | SUCCESS |

### 追加修正: regression-tester.md スコープ整理

**問題発見**: Phase 9 照合時に、regression-tester が「テストスイート全体」を報告していたことが判明。

| カテゴリ | 本来の担当 | 旧 regression-tester |
|----------|-----------|:-------------------:|
| Engine Unit Tests | AC検証 (engine type) | ❌ 重複 |
| Strict Check | Hook 自動化 | ❌ 重複 |
| Kojo AC Tests | AC検証 (kojo type) | ❌ 重複 |
| **Regression Tests** | **regression-tester** | ✅ 正しい |

**修正内容**:
1. regression-tester.md を `tests/regression/` (24件) のみに限定
2. Scope セクション追加で担当外を明記
3. 出力形式を `Regression: OK:24/24` に統一
4. verify-logs.py との照合を明記

---

## Links

- [feature-204.md](feature-204.md) - 発見元
- [feature-203.md](feature-203.md) - Log Collection Audit
- [imple.md](../../.claude/commands/imple.md) - 統合先
- [TestPathUtils.cs](../../engine/Assets/Scripts/Emuera/Headless/TestPathUtils.cs) - エンジン修正対象
- [ac-tester.md](../../.claude/agents/ac-tester.md) - Skill 参照追加対象
- [skills/testing/SKILL.md](../../.claude/skills/testing/SKILL.md) - ログ形式追加対象
