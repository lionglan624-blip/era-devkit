# Feature 163: AC/Regression Protection Hooks

## Status: [DONE]

## Type: erb

## Background

Test Infrastructure Reorganization の第4段階。
AC/regressionテストファイルをClaude Codeによる改ざんから保護するHookを実装。

### Problem (解決対象)

| 問題 | 具体例 |
|------|--------|
| テスト内容の改変 | ACを満たすためにテスト自体を書き換え |
| テスト方法の変更 | flow→unitに勝手に変更 |
| デバッグテストで上書き | 本番テストがデバッグ出力で消える |

## Dependencies

- Feature 162 (Migration) - 保護対象ファイルが新パスに存在

## Protection Matrix

| 対象 | 新規作成 | 編集/削除 | 保護方法 |
|------|:--------:|:---------:|----------|
| `tests/ac/**` | ✅ | ❌ | Hook |
| `tests/regression/**` | ✅ | ❌ | Hook |
| `tests/debug/**` | ✅ | ✅ | なし |

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | pre-ac-write.ps1 新規ファイル許可 | exit_code | equals | 0 | [x] |
| 2 | pre-ac-write.ps1 既存編集ブロック | exit_code | equals | 2 | [x] |
| 3 | pre-ac-write.ps1 debug/編集許可 | exit_code | equals | 0 | [x] |
| 4 | pre-bash-ac.ps1 rm ブロック | exit_code | equals | 2 | [x] |
| 5 | pre-bash-ac.ps1 git restore ブロック | exit_code | equals | 2 | [x] |
| 6 | settings.json hook登録完了 | code | contains | "pre-ac-write" | [x] |

### AC Test Method

> **Note**: AC 1-5 は手動検証。Hookの動作をHook内からテストできない。

| AC# | 検証方法 | 手順 |
|:---:|----------|------|
| 1 | 手動 | `Write tests/ac/erb/feature-163/new-file.json` → exit 0確認 |
| 2 | 手動 | `Edit tests/ac/erb/feature-163/new-file.json` → exit 2確認 |
| 3 | 手動 | `Edit tests/debug/test.json` → exit 0確認 |
| 4 | 手動 | `Bash rm tests/ac/erb/feature-163/new-file.json` → exit 2確認 |
| 5 | 手動 | `Bash git restore tests/ac/erb/feature-163/` → exit 2確認 |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | pre-ac-write.ps1 作成 | [x] |
| 2 | 4,5 | pre-bash-ac.ps1 作成 | [x] |
| 3 | 6 | settings.json hook登録 | [x] |

## Design

### Hook: pre-ac-write.ps1

```powershell
# .claude/hooks/pre-ac-write.ps1
# Claude Code PreToolUse hook - stdin経由でJSONを受信

# stdinからJSON読み取り
$inputJson = [Console]::In.ReadToEnd()
if (-not $inputJson) { exit 0 }

$data = $inputJson | ConvertFrom-Json -ErrorAction SilentlyContinue
if (-not $data) { exit 0 }

$path = $data.tool_input.file_path
if (-not $path) { exit 0 }

# ac/ または regression/ 以外は無視
if ($path -notmatch 'tests[/\\](ac|regression)[/\\]') { exit 0 }

# 新規作成 → 許可
if (-not (Test-Path $path)) {
    Write-Host "[Hook] New AC/regression file allowed: $path"
    exit 0
}

# 既存ファイル → ブロック (exit 2)
Write-Error "[BLOCKED] Cannot modify existing AC/regression file: $path"
exit 2
```

### Hook: pre-bash-ac.ps1

```powershell
# .claude/hooks/pre-bash-ac.ps1
# Claude Code PreToolUse hook for Bash - stdin経由でJSONを受信

# stdinからJSON読み取り
$inputJson = [Console]::In.ReadToEnd()
if (-not $inputJson) { exit 0 }

$data = $inputJson | ConvertFrom-Json -ErrorAction SilentlyContinue
if (-not $data) { exit 0 }

$command = $data.tool_input.command
if (-not $command) { exit 0 }

# ac/ または regression/ への破壊操作をブロック
if ($command -match '(rm|del|move|mv|cp|copy|sed\s+-i|cat\s*>|echo\s*>|>\s*).*tests[/\\](ac|regression)[/\\]') {
    Write-Error "[BLOCKED] Destructive operation on ac/regression: $command"
    exit 2
}

# git checkout/restore/rm によるテストファイル操作をブロック
if ($command -match 'git\s+(checkout|restore|rm).*tests[/\\](ac|regression)[/\\]') {
    Write-Error "[BLOCKED] Git restore on ac/regression files: $command"
    exit 2
}

exit 0
```

### settings.json Update

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Write|Edit",
        "hooks": [{
          "type": "command",
          "command": "powershell -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/pre-ac-write.ps1"
        }]
      },
      {
        "matcher": "Bash",
        "hooks": [{
          "type": "command",
          "command": "powershell -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/pre-bash-ac.ps1"
        }]
      }
    ]
  }
}
```

## Claude Code Hooks Reference

| 項目 | 仕様 |
|------|------|
| 入力方法 | **stdin経由のJSON** |
| ブロック用exit code | **exit 2** |
| 許可用exit code | exit 0 |

### Exit Code動作

| Code | 動作 |
|:----:|------|
| 0 | 許可 - stdoutをユーザーに表示 (verbose時) |
| 2 | **ブロック** - stderrをClaudeにフィードバック |
| その他 | 非ブロックエラー - 実行継続 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 | START | initializer | Feature init | READY |
| 2025-12-21 | START | explorer | Investigation | READY |
| 2025-12-21 | START | implementer | Task 1-3 | SUCCESS |
| 2025-12-21 | START | regression-tester | Regression | ALL_PASS |
| 2025-12-21 | AC | ac-tester | AC 6 | PASS |
| 2025-12-21 | MANUAL | - | AC 1-5 | PASS |

---

## Manual Verification Results (2025-12-21)

### Test 1: Write/Edit Protection

| 操作 | 結果 | 期待 |
|------|------|------|
| Write tests/ac/.../new-file.json | ✅ 許可 | 許可 |
| Edit tests/ac/.../new-file.json | ✅ ブロック | ブロック |
| Write tests/debug/test.json | ✅ 許可 | 許可 |

### Test 2: Bash Protection

| 操作 | 結果 | 期待 |
|------|------|------|
| rm tests/ac/... | ✅ ブロック | ブロック |
| git restore tests/ac/... | ✅ ブロック | ブロック |
| git rm tests/ac/... | ✅ ブロック | ブロック |

### Test 3: Git Operations (User-initiated)

| 操作 | 結果 | 備考 |
|------|------|------|
| git add tests/ac/... (新規) | ✅ 許可 | 新規ファイル追加 |
| git commit (新規ファイル含む) | ✅ 許可 | コミット可能 |
| git add tests/ac/... (削除) | ✅ 許可 | ユーザー手動削除後 |
| git commit (削除含む) | ✅ 許可 | ユーザー意図的削除 |

### Design Decision

ユーザーが手動削除 → git add → git commit は**許可**する。
- 理由: ブロックすると永久に削除できなくなる
- Claude経由の `rm` と `git restore` がブロックされていれば実務的に問題なし

---

## Links

- [feature-162.md](feature-162.md) - Migration (依存元)
- [feature-165.md](feature-165.md) - Documentation Update (依存先)
- [Hooks Reference - Claude Docs](https://docs.claude.com/en/docs/claude-code/hooks)
