# .claude/hooks/pre-tdd-protection.ps1
# Claude Code PreToolUse hook - stdin経由でJSONを受信
#
# TDD PRINCIPLE: Test files created in Phase 3 (RED) are immutable during Phase 4 (GREEN).
# Implementation MUST conform to tests, NOT the other way around.
# If tests fail, fix the IMPLEMENTATION. If test definition is incorrect, escalate to user for manual correction.
# Violating this breaks the TDD contract and invalidates verification.

# stdinからJSON読み取り
$inputJson = [Console]::In.ReadToEnd()
if (-not $inputJson) { exit 0 }

try {
    $data = $inputJson | ConvertFrom-Json -ErrorAction Stop
} catch {
    Write-Error "[BLOCKED] Failed to parse JSON input: $_"
    exit 2
}
if (-not $data) {
    Write-Error "[BLOCKED] Empty JSON input"
    exit 2
}

$path = $data.tool_input.file_path
if (-not $path) { exit 0 }

# Check if path is a TDD-protected test file (devkit repo)
# tools/*Tests/ - Tool unit tests (ErbParser.Tests, ErbToYaml.Tests, etc.)
# test/ac/ - AC scenario definitions (JSON only, not .md)
$isProtectedTest = $false

if ($path -match '(src[/\\]tools[/\\]dotnet[/\\]\w+\.Tests)[/\\].*\.cs' -and $path -notmatch 'obj[/\\]') {
    $isProtectedTest = $true
} elseif ($path -match 'test[/\\]ac[/\\].*\.json') {
    $isProtectedTest = $true
}

# Non-test files → Allow
if (-not $isProtectedTest) { exit 0 }

# New test file (Phase 3 RED) → Allow
if (-not (Test-Path $path)) {
    Write-Host "[Hook] New test file allowed (Phase 3 RED): $path"
    exit 0
}

# Existing test file (Phase 4 GREEN) → Block with TDD error
Write-Error "[BLOCKED] Cannot modify existing test file: $path`nTDD PRINCIPLE: Fix implementation, not the test. If test definition is incorrect, escalate to user for manual correction."
exit 2
