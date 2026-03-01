# stop-ratelimit-resume.ps1
# Stop hook: Detects 429 rate limit and auto-resumes in a new terminal tab
# with a different CCS account.
#
# Trigger: Claude Code Stop event
# Input: JSON via stdin (session_id, transcript_path, last_assistant_message, stop_hook_active, cwd)
# Output: exit 0 (allow stop). New WT tab opened if 429 detected.

$ErrorActionPreference = 'SilentlyContinue'

# --- 1. Read stdin JSON ---
$jsonInput = [Console]::In.ReadToEnd()
if (-not $jsonInput) { exit 0 }
$data = $jsonInput | ConvertFrom-Json
if (-not $data) { exit 0 }

# --- 2. Prevent infinite loop ---
if ($data.stop_hook_active -eq $true) { exit 0 }

# --- 3. Detect rate limit ---
$rateLimitPattern = 'hit your limit|rate_limit_error|exceed your (organization|account).?s? rate limit'
$isRateLimited = $false

# 3a. Check last_assistant_message
if ($data.last_assistant_message -match $rateLimitPattern) {
    $isRateLimited = $true
}

# 3b. Fallback: check transcript tail
if (-not $isRateLimited -and $data.transcript_path -and (Test-Path $data.transcript_path)) {
    try {
        $tail = Get-Content $data.transcript_path -Tail 20 -Raw -ErrorAction Stop
        if ($tail -match $rateLimitPattern) {
            $isRateLimited = $true
        }
    } catch {}
}

# Not rate limited -> normal exit
if (-not $isRateLimited) { exit 0 }

# --- 4. Determine current account ---
$currentProfile = 'unknown'
if ($env:CLAUDE_CONFIG_DIR) {
    $currentProfile = Split-Path $env:CLAUDE_CONFIG_DIR -Leaf
}

# --- 5. Pick best target account via cs-pick.js ---
$csPickScript = Join-Path $env:USERPROFILE '.local\bin\cs-pick.js'
$configYaml = Join-Path $env:USERPROFILE '.ccs\config.yaml'
$cacheJson = Join-Path $data.cwd '.tmp\dashboard\ratelimit-cache.json'

$target = $null
$usage = '?'
try {
    $pickOutput = & node $csPickScript $configYaml $cacheJson 2>$null
    if ($pickOutput) {
        $parts = $pickOutput -split '\s+'
        if ($parts.Length -ge 2) {
            $target = $parts[1]
            if ($parts.Length -ge 3) { $usage = $parts[2] }
        }
    }
} catch {}

# Fallback: round-robin if cs-pick.js failed
if (-not $target -or $target -eq $currentProfile) {
    $allProfiles = @('google', 'apple', 'proton') | Where-Object { $_ -ne $currentProfile }
    $target = $allProfiles | Select-Object -First 1
}

if (-not $target) {
    Write-Host "[429-Resume] No alternative account available."
    exit 0
}

# --- 6. Write lock file ---
$lockDir = Join-Path $data.cwd '.tmp\dashboard'
if (-not (Test-Path $lockDir)) {
    New-Item -Path $lockDir -ItemType Directory -Force | Out-Null
}
$lockFile = Join-Path $lockDir 'cs-switch.lock'
$now = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
$lockData = @{
    lockedBy  = 'stop-hook'
    profile   = $target
    sessionId = $data.session_id
    timestamp = $now
    expiresAt = $now + 60000
} | ConvertTo-Json -Compress
[IO.File]::WriteAllText($lockFile, $lockData)

# --- 7. Switch default profile ---
& ccs auth default $target 2>$null | Out-Null

# --- 8. Open new Windows Terminal tab ---
$sessionId = $data.session_id
$launchCmd = "claude_with_proxy --resume $sessionId `"Continue from where you left off.`""

try {
    $wtArgs = "-w 0 new-tab -d `"$($data.cwd)`" -- cmd /k $launchCmd"
    Start-Process wt.exe -ArgumentList $wtArgs
} catch {
    # Fallback: if wt.exe not available, just notify
    Write-Host "[429-Resume] Failed to open new tab: $_"
    Write-Host "[429-Resume] Run manually: cd $($data.cwd) && $launchCmd"
}

# --- 9. Notify user in current tab ---
Write-Host ""
Write-Host "=== 429 AUTO-RESUME ==="
Write-Host "  $currentProfile -> $target (usage: $usage%)"
Write-Host "  Session: $sessionId"
Write-Host "  New tab opened with --resume"
Write-Host "========================"

exit 0
