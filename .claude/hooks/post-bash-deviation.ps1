# .claude/hooks/post-bash-deviation.ps1
# PostToolUse hook for Bash - exit ≠ 0を検出し、.tmp/deviation-log.txtに追記
#
# Purpose: DEVIATIONの記録漏れを防ぐ。Phase 8で機械的に比較可能。

$inputJson = [Console]::In.ReadToEnd()
if (-not $inputJson) { exit 0 }

$data = $inputJson | ConvertFrom-Json -ErrorAction SilentlyContinue
if (-not $data) { exit 0 }

# exit codeを取得（複数の形式に対応）
$exitCode = $null
if (-not $data.tool_result) { exit 0 }
if ($data.tool_result.PSObject.Properties['exit_code']) {
    $exitCode = $data.tool_result.exit_code
} elseif ($data.tool_result.PSObject.Properties['exitCode']) {
    $exitCode = $data.tool_result.exitCode
} elseif ($data.tool_result.PSObject.Properties['code']) {
    $exitCode = $data.tool_result.code
}

if ($null -eq $exitCode) { exit 0 }

if ($exitCode -ne 0) {
    $projectDir = $env:CLAUDE_PROJECT_DIR
    if (-not $projectDir) { $projectDir = (Get-Location).Path }

    $tmpDir = Join-Path $projectDir ".tmp"
    if (-not (Test-Path $tmpDir)) {
        New-Item -ItemType Directory -Path $tmpDir -Force | Out-Null
    }

    $logFile = Join-Path $tmpDir "deviation-log.txt"
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $command = $data.tool_input.command

    # コマンドが長い場合は切り詰め
    if ($command.Length -gt 100) {
        $command = $command.Substring(0, 100) + "..."
    }

    $logEntry = "$timestamp | exit $exitCode | $command"
    Add-Content -Path $logFile -Value $logEntry -Encoding UTF8
}

exit 0
