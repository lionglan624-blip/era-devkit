# .claude/hooks/post-code-write.ps1
# PostToolUse hook for devkit repo - build + test for dotnet tools

$logFile = "$env:CLAUDE_PROJECT_DIR\.claude\hooks\post-hook.log"
Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Hook started"

$inputJson = [Console]::In.ReadToEnd()
Add-Content -Path $logFile -Value "inputJson length: $($inputJson.Length)"
if (-not $inputJson) {
    Add-Content -Path $logFile -Value "No input, exit 0"
    exit 0
}

# Extract file_path via regex to avoid ConvertFrom-Json failures
# on large tool_input.old_string containing escape sequences (pre-tdd-protection.ps1 pattern)
if ($inputJson -match '"file_path"\s*:\s*"([^"\\]*(?:\\.[^"\\]*)*)"') {
    $path = $Matches[1] -replace '\\/', '/' -replace '\\\\', '\'
} else {
    Add-Content -Path $logFile -Value "No file_path match, exit 0"
    exit 0
}
Add-Content -Path $logFile -Value "file_path: $path"
if (-not $path) {
    Add-Content -Path $logFile -Value "No path, exit 0"
    exit 0
}

# Only C# tool files
$isCS = $path -match 'src[/\\]tools[/\\]dotnet[/\\]\w+[/\\].*\.cs$'
if (-not $isCS) { exit 0 }

$projectDir = $env:CLAUDE_PROJECT_DIR
if (-not $projectDir) { exit 0 }

# Determine test project from file path
$testsProj = $null
if ($path -match 'src[/\\]tools[/\\]dotnet[/\\](\w+)[/\\]') {
    $toolName = $Matches[1]
    if ($toolName -match '\.Tests$') {
        $testsProj = Join-Path $projectDir "src" "tools" "dotnet" "$toolName"
    } else {
        $testsProj = Join-Path $projectDir "src" "tools" "dotnet" "$toolName.Tests"
    }
    if (-not (Test-Path $testsProj)) { $testsProj = $null }
}

$hasError = $false

# 1. Build the tool/test project
$buildTarget = if ($testsProj) { $testsProj } else { $path | Split-Path -Parent }
try {
    $wslBuildTarget = ($buildTarget -replace '\\', '/' -replace '^([A-Z]):', { "/mnt/$($_.Groups[1].Value.ToLower())" })
    $buildResult = wsl -- bash -c "/home/siihe/.dotnet/dotnet build '$wslBuildTarget' --verbosity quiet" 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "[Hook] Build FAILED"
        $buildResult | ForEach-Object { Write-Error "  $_" }
        $hasError = $true
    }
} catch {
    Write-Error "[Hook] Build exception: $_"
    $hasError = $true
}

# 2. Run tests (skip if build failed or no test project)
if (-not $hasError -and $testsProj) {
    try {
        $wslTestsProj = ($testsProj -replace '\\', '/' -replace '^([A-Z]):', { "/mnt/$($_.Groups[1].Value.ToLower())" })
        $testResult = wsl -- bash -c "/home/siihe/.dotnet/dotnet test '$wslTestsProj' --verbosity quiet --blame-hang-timeout 10s" 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error "[Hook] Tests FAILED"
            $testResult | ForEach-Object { Write-Error "  $_" }
            $hasError = $true
        }
    } catch {
        Write-Error "[Hook] Test exception: $_"
        $hasError = $true
    }
}

if ($hasError) { exit 2 }
exit 0
