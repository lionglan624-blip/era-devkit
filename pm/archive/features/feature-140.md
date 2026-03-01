# Feature 140: PostToolUse Hook - BOM + ビルド + strict検証

## Status: [DONE]

## Type: infra

## Execution State

**Current Agent**: initializer (haiku)
**Phase**: START
**Timestamp**: 2025-12-20 22:52 JST
**Next Agent**: implementer (opus)

## Background

### Problem
- ERBファイル編集時にBOMが欠落し、ビルドエラーが発生することがある（Feature 085で問題発生）
- ドキュメントに「BOMを付けろ」と書いても、エージェントが読み飛ばすことがある
- 壊れたコードを書いてもすぐに気づかない
- 実行時エラー（変数未定義、関数不一致）も早期発見したい

### Goal
- ERB/ERH編集後に自動でBOM付加＆ビルド検証＆strict検証を実行
- ドキュメント依存からコード強制へ転換（Anthropic推奨原則#1）

### Context
- [anthropic-recommended-transition.md](designs/anthropic-recommended-transition.md) Phase A
- Anthropic推奨: ルール管理をHooks（コード）で強制

### Design Rationale

**Hooks向きの条件**:
| 基準 | BOM | ビルド | strict |
|------|:---:|:------:|:------:|
| 毎回実行すべき | ✅ | ✅ | ✅ |
| 結果が単純 | ✅ | ✅ | ✅ |
| 記録不要 | ✅ | ✅ | ✅ |
| 軽量 | ✅ (~0s) | ✅ (~1s) | ✅ (~4s) |

**合計実行時間**: 約5秒（許容範囲）

**Hooksにしない処理**:
| 処理 | 理由 |
|------|------|
| 回帰テスト | タイミング選択が必要、時間増加 |
| ACチェック | 結果解釈+feature.md記録が必要 |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | Hook設定ファイル存在 | file | exists | `.claude/settings.json` | [x] |
| 2 | PostToolUse Hook登録 | file | contains | `"PostToolUse"` | [x] |
| 3 | Hookスクリプト存在 | file | exists | `.claude/hooks/post-erb-write.ps1` | [x] |
| 4 | ERB編集後BOM付加動作 | output | contains | `"[Hook] BOM"` | [x] |
| 5 | ERB編集後ビルド検証動作 | output | contains | `"[Hook] Build"` | [x] |
| 6 | ERB編集後strict検証動作 | output | contains | `"[Hook] Strict"` | [x] |

### AC Details

#### AC1: Hook設定ファイル存在

**Test Command**:
```bash
test -f .claude/settings.json && echo "exists"
```

**Expected Output**: `exists`

#### AC2: PostToolUse Hook登録

**Test Command**:
```bash
grep "PostToolUse" .claude/settings.json
```

**Expected Output**: Contains `"PostToolUse"`

#### AC3: Hookスクリプト存在

**Test Command**:
```bash
test -f .claude/hooks/post-erb-write.ps1 && echo "exists"
```

**Expected Output**: `exists`

#### AC4-6: ERB編集後の各検証動作

**Test Command**: 手動テスト - ERBファイルを編集してHookが発火することを確認

**Expected Output**:
- `[Hook] BOM added:` または `[Hook] BOM exists:`
- `[Hook] Build OK` または `[Hook] Build FAILED`
- `[Hook] Strict OK` または `[Hook] Strict FAILED`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | `.claude/settings.json` ファイル作成＆検証 | [x] PASS |
| 2 | 2 | `.claude/settings.json` にPostToolUse Hook設定追加 | [x] PASS |
| 3 | 3 | `.claude/hooks/post-erb-write.ps1` 作成 | [x] PASS |
| 4 | 4 | テスト: BOM付加動作確認 | [x] PASS |
| 5 | 5 | テスト: ビルド検証動作確認 | [x] PASS |
| 6 | 6 | テスト: strict検証動作確認 | [x] PASS ✓ |

---

## Implementation Details

### settings.json

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "powershell -NoProfile -File .claude/hooks/post-erb-write.ps1"
          }
        ]
      }
    ]
  }
}
```

### post-erb-write.ps1

```powershell
# .claude/hooks/post-erb-write.ps1
# ERBファイル書き込み後の自動処理
# 実行時間: BOM (~0s) + Build (~1s) + Strict (~4s) = ~5s

$path = $env:CLAUDE_FILE_PATH
if (-not $path) { exit 0 }
if ($path -notmatch '\.(erb|erh)$') { exit 0 }

$projectDir = $env:CLAUDE_PROJECT_DIR
if (-not $projectDir) { $projectDir = (Get-Location).Path }

# 1. BOM自動付加
$bytes = [System.IO.File]::ReadAllBytes($path)
if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
    Write-Host "[Hook] BOM exists: $path" -ForegroundColor Gray
} else {
    $content = [System.IO.File]::ReadAllText($path)
    [System.IO.File]::WriteAllText($path, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "[Hook] BOM added: $path" -ForegroundColor Green
}

# 2. ビルド検証 (~1秒)
Push-Location "$projectDir\Game"
$buildResult = dotnet build ../uEmuera/uEmuera.Headless.csproj --verbosity quiet 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[Hook] Build FAILED" -ForegroundColor Red
    Write-Host $buildResult -ForegroundColor Red
    Pop-Location
    exit 0  # 情報提供のみ、ブロックはしない
}
Write-Host "[Hook] Build OK" -ForegroundColor Green

# 3. Strict検証 (~4秒)
$strictResult = dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . --strict --exit-on-load 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[Hook] Strict FAILED" -ForegroundColor Red
    Write-Host $strictResult -ForegroundColor Red
} else {
    Write-Host "[Hook] Strict OK" -ForegroundColor Green
}

Pop-Location
exit 0
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-20 09:03 | START | implementer | Task 1 | - |
| 2025-12-20 09:03 | END | implementer | Task 1 | SUCCESS (<1min) |
| 2025-12-20 09:04 | UNIT_TEST | unit-tester | Task 1 verification | PASS (3/3 checks) |
| 2025-12-20 09:06 | START | implementer | Task 2+3 | - |
| 2025-12-20 09:07 | END | implementer | Task 2+3 | SUCCESS (1min) |
| 2025-12-20 10:55 | UNIT_TEST | unit-tester | Task 2 verification | PASS (5/5 checks) |
| 2025-12-20 22:56 | UNIT_TEST | unit-tester | Task 3 BOM hook test | PASS (All 3 ACs verified) |
| 2025-12-20 23:00 | UNIT_TEST | unit-tester | Task 4 build validation test | PASS (Build OK verified) |
| 2025-12-20 09:16 | UNIT_TEST | unit-tester | Task 5 strict validation test | PASS (Strict OK verified) |
| 2025-12-20 23:12 | END | finalizer | Feature 140 | DONE (complete) |

---

## Discovered Issues

| Issue | Type | Priority |
|-------|------|----------|
| | | |

---

## Links

- [anthropic-recommended-transition.md](designs/anthropic-recommended-transition.md)
- [hooks-reference.md](reference/hooks-reference.md)
