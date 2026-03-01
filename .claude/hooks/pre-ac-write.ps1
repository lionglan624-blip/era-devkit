# .claude/hooks/pre-ac-write.ps1
# Claude Code PreToolUse hook - stdin経由でJSONを受信
#
# TDD PRINCIPLE: AC test files are IMMUTABLE acceptance criteria.
# Implementation MUST conform to tests, NOT the other way around.
# If tests fail, fix the IMPLEMENTATION or TEST GENERATOR, never the test file.
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

# No longer protecting test paths - see F568 for TDD protection
exit 0
