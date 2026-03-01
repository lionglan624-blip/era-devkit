# .claude/hooks/pre-bash-ac.ps1
# Claude Code PreToolUse hook for Bash - stdin経由でJSONを受信
#
# TDD PRINCIPLE: AC test files are IMMUTABLE acceptance criteria.
# Implementation MUST conform to tests, NOT the other way around.
# If tests fail, fix the IMPLEMENTATION or TEST GENERATOR, never the test file.
# Violating this breaks the TDD contract and invalidates verification.

# stdinからJSON読み取り
$inputJson = [Console]::In.ReadToEnd()
if (-not $inputJson) { exit 0 }

$data = $inputJson | ConvertFrom-Json -ErrorAction SilentlyContinue
if (-not $data) { exit 0 }

$command = $data.tool_input.command
if (-not $command) { exit 0 }

# ac/ への破壊的操作をブロック
# 対象: ファイル操作コマンドがACパスを直接ターゲットにしている場合のみ
# 保護対象: test/ac/, tools/*Tests/
$testPathPattern = '(test[/\\]ac|src[/\\]tools[/\\]dotnet[/\\]\w+\.Tests)[/\\]'

# パターン1: sed -i でACファイルを直接編集
if ($command -match "sed\s+-i.*$testPathPattern") {
    Write-Error "[BLOCKED] Cannot modify AC files via sed: $command"
    Write-Error "TDD PRINCIPLE: Fix implementation or test generator, not the test file."
    exit 2
}

# パターン2: rm/mv/cp でACファイルを操作
if ($command -match "(rm|del|move|mv)\s+.*$testPathPattern") {
    Write-Error "[BLOCKED] Cannot delete/move AC files: $command"
    Write-Error "TDD PRINCIPLE: Fix implementation or test generator, not the test file."
    exit 2
}

# パターン3: リダイレクト先がACパス
if ($command -match ">\s*[^&]*$testPathPattern") {
    Write-Error "[BLOCKED] Cannot redirect to AC files: $command"
    Write-Error "TDD PRINCIPLE: Fix implementation or test generator, not the test file."
    exit 2
}

# パターン4: rmdir でACディレクトリを削除
if ($command -match "rmdir\s+.*$testPathPattern") {
    Write-Error "[BLOCKED] Cannot delete AC directories via rmdir: $command"
    Write-Error "TDD PRINCIPLE: Fix implementation or test generator, not the test file."
    exit 2
}

# パターン5: rm -rf でACパスを削除
if ($command -match "rm\s+-rf\s+.*$testPathPattern") {
    Write-Error "[BLOCKED] Cannot delete AC files via rm -rf: $command"
    Write-Error "TDD PRINCIPLE: Fix implementation or test generator, not the test file."
    exit 2
}

# git checkout/restore/rm によるテストファイル操作をブロック
if ($command -match "git\s+(checkout|restore|rm).*$testPathPattern") {
    Write-Error "[BLOCKED] Git restore on ac files: $command"
    Write-Error "TDD PRINCIPLE: Fix implementation or test generator, not the test file."
    exit 2
}

# ===== Windows Path Guard: Git Bash では Unix パス必須 =====
# C:\ を含むコマンドをブロックし、Unix 形式パスを提案する
# bash が backslash を消費するため C:\Era → C:Era になり必ず失敗する

if ($command -match '([A-Za-z]):\\') {
    $driveLetter = $Matches[1].ToLower()
    # 変換例を生成: C:\Era\foo → /c/Era/foo
    $suggested = $command -creplace '([A-Za-z]):\\', '/$1/'
    $suggested = $suggested -replace '\\', '/'
    # ドライブレターを小文字に
    $suggested = [regex]::Replace($suggested, '/([A-Z])/', { param($m) '/' + $m.Groups[1].Value.ToLower() + '/' })
    Write-Error "[BLOCKED] Windows path in Bash command."
    Write-Error "  Before: $command"
    Write-Error "  After:  $suggested"
    Write-Error "Hint: Bash cwd is project root. Prefer relative paths: cd tools && ..."
    exit 2
}

# ===== Git Safety: 破壊的コマンドブロック (deny バグ対策の二重チェック) =====

# git restore (ワークツリー変更の破棄)
if ($command -match "^\s*git\s+restore\b") {
    Write-Error "[BLOCKED] git restore destroys uncommitted changes: $command"
    Write-Error "Use 'git diff' to check impact first. Commit or stash before discarding."
    exit 2
}

# git reset --hard (コミット履歴+ワークツリーの巻き戻し)
if ($command -match "^\s*git\s+reset\s+--hard\b") {
    Write-Error "[BLOCKED] git reset --hard destroys uncommitted changes: $command"
    Write-Error "Use 'git stash' to save changes before resetting."
    exit 2
}

# git clean (未追跡ファイルの削除)
if ($command -match "^\s*git\s+clean\b") {
    Write-Error "[BLOCKED] git clean deletes untracked files irreversibly: $command"
    Write-Error "Use 'git clean -n' (dry run) to preview first."
    exit 2
}

# git checkout . / git checkout -- . / git checkout HEAD -- . (全ファイル巻き戻し)
if ($command -match "^\s*git\s+checkout\s+(--\s+)?\.(\s|$)") {
    Write-Error "[BLOCKED] git checkout . destroys all uncommitted changes: $command"
    exit 2
}
if ($command -match "^\s*git\s+checkout\s+HEAD\s+--\s+\.(\s|$)") {
    Write-Error "[BLOCKED] git checkout HEAD -- . destroys all uncommitted changes: $command"
    exit 2
}

# git push --force / -f (リモート履歴破壊)
if ($command -match "^\s*git\s+push\s+.*(-f|--force)\b") {
    Write-Error "[BLOCKED] Force push destroys remote history: $command"
    Write-Error "Use --force-with-lease for safer force push if absolutely needed."
    exit 2
}

# git branch -D (ブランチ強制削除)
if ($command -match "^\s*git\s+branch\s+-D\b") {
    Write-Error "[BLOCKED] git branch -D force-deletes branch without merge check: $command"
    Write-Error "Use 'git branch -d' (lowercase) for safe deletion."
    exit 2
}

# git stash drop / clear (スタッシュの破棄)
if ($command -match "^\s*git\s+stash\s+(drop|clear)\b") {
    Write-Error "[BLOCKED] git stash drop/clear destroys stashed changes: $command"
    exit 2
}

# rm -rf (ディレクトリ一括削除)
if ($command -match "^\s*rm\s+-rf\b") {
    Write-Error "[BLOCKED] rm -rf deletes files irreversibly: $command"
    Write-Error "Use 'ls' to verify target first, or move to trash instead."
    exit 2
}

# rm (ファイル削除) - ただし rm 単体は意図的な場合もあるのでパス制限なし
# git rm は通常の操作なので除外

exit 0
