# Anthropic推奨アプローチへの転換設計

## Status: DRAFT

## 概要

現状の「完全定義→従わせる」設計から、Anthropic推奨の「最小ルール + コード強制 + エージェント判断」への転換を検討する。

---

## 1. Hooks 完全検討

### 1.1 全イベント一覧

| # | イベント | タイミング | マッチャー | 用途 |
|:-:|----------|-----------|-----------|------|
| 1 | **PreToolUse** | ツール実行前 | ツール名 | ブロック/修正 |
| 2 | **PermissionRequest** | 権限ダイアログ表示時 | ツール名 | 権限制御 |
| 3 | **PostToolUse** | ツール実行後 | ツール名 | 検証/変換 |
| 4 | **Notification** | 通知時 | 通知タイプ | 通知カスタマイズ |
| 5 | **UserPromptSubmit** | プロンプト送信時 | なし | コンテキスト追加 |
| 6 | **Stop** | メインエージェント完了時 | なし | 完了処理 |
| 7 | **SubagentStop** | サブエージェント完了時 | なし | サブエージェント完了処理 |
| 8 | **PreCompact** | コンパクト前 | manual/auto | コンパクト前処理 |
| 9 | **SessionStart** | セッション開始時 | startup/resume/clear/compact | 初期化 |
| 10 | **SessionEnd** | セッション終了時 | exit/clear/logout/other | 終了処理 |

### 1.2 マッチャー

| 種類 | 例 | 説明 |
|------|-----|------|
| 完全一致 | `Write` | 大文字小文字区別 |
| OR条件 | `Edit\|Write` | 複数ツール |
| 正規表現 | `Notebook.*` | パターン |
| ワイルドカード | `*` | 全ツール |
| 引数パターン | `Bash(npm test*)` | コマンド引数 |
| MCP | `mcp__memory__.*` | MCPツール |

### 1.3 Exit Code

| Code | 意味 | 動作 |
|:----:|------|------|
| 0 | 成功 | 続行、stdoutは詳細モードで表示 |
| 2 | ブロック | 停止、stderrをClaude/ユーザーに表示 |
| 他 | 非ブロックエラー | 続行、stderrは詳細モードで表示 |

### 1.4 JSON出力による制御

```json
{
  "continue": true,
  "stopReason": "string",
  "suppressOutput": true,
  "systemMessage": "string",
  "hookSpecificOutput": {
    "hookEventName": "PreToolUse",
    "permissionDecision": "allow|deny|ask",
    "updatedInput": { "field": "modified value" }
  }
}
```

---

## 2. このプロジェクトで必要なHooks

### 2.1 各イベント別検討

| イベント | 必要性 | 理由 |
|----------|:------:|------|
| **PostToolUse (ERB)** | **必須** | BOM自動付加（Feature 085で問題発生） |
| **PostToolUse (ERB)** | **推奨** | ビルド検証（壊れたコード早期発見） |
| PreToolUse | 不要 | 保護すべきファイルがない |
| PermissionRequest | 不要 | 問題発生していない |
| Notification | 不要 | 問題発生していない |
| UserPromptSubmit | 不要 | CLAUDE.mdで対応済み |
| **Stop** | **現状維持** | ビープ音 |
| SubagentStop | 検討 | 長時間タスク完了通知（優先度低） |
| PreCompact | 不要 | 自動コンパクトは必要機能 |
| SessionStart | 検討 | プロジェクト情報注入（優先度低） |
| SessionEnd | 不要 | 問題発生していない |

### 2.2 テスト関連Hooks

| 候補 | 判断 | 理由 |
|------|:----:|------|
| PostToolUse Bash (テスト後) | **不要** | subagentが結果報告する設計 |
| PreToolUse Bash (テスト前) | **不要** | 問題発生していない |
| SubagentStop (テスト完了) | **不要** | status fileポーリングで対応済み |

**結論**: テスト関連Hooksは不要。subagent設計で対応済み。

### 2.3 最終設計

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
    ],
    "Stop": [
      {
        "matcher": "",
        "hooks": [
          {
            "type": "command",
            "command": "powershell -NoProfile -Command \"[Console]::Beep(800, 200)\""
          }
        ]
      }
    ]
  }
}
```

### 2.4 post-erb-write.ps1

```powershell
# .claude/hooks/post-erb-write.ps1
# ERBファイル書き込み後の自動処理

$path = $env:CLAUDE_FILE_PATH
if (-not $path) { exit 0 }
if ($path -notmatch '\.(erb|erh)$') { exit 0 }

# 1. BOM自動付加
$bytes = [System.IO.File]::ReadAllBytes($path)
if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
    # BOM exists
} else {
    $content = [System.IO.File]::ReadAllText($path)
    [System.IO.File]::WriteAllText($path, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "[Hook] BOM added: $path" -ForegroundColor Green
}

# 2. ビルド検証
$projectDir = $env:CLAUDE_PROJECT_DIR
if ($projectDir) {
    Push-Location "$projectDir\Game"
    $result = dotnet build ../uEmuera/uEmuera.Headless.csproj --verbosity quiet 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[Hook] BUILD FAILED after editing: $path" -ForegroundColor Red
        # exit 2 でブロックはしない（情報提供のみ）
    }
    Pop-Location
}
```

---

## 3. Sessions 検討

### 3.1 現状分析

| パターン | 現状実装 | Sessions活用 |
|---------|---------|-------------|
| eratw-reader → kojo-writer×10 | ファイルキャッシュ | 不要（1:Nはファイルが正解） |
| implementer → ac-tester | 独立実行 | 可能だが分離が設計意図 |
| context compaction後の再開 | feature.md読み直し | resume可能 |

### 3.2 Anthropic推奨との整合性

sessions-reference.md の分析:

> 1:N並列パターン（eratw-reader → kojo-writer×10）はファイルキャッシュが正解。Sessionsは1:1シーケンシャル向き。

**現状のファイルベース通信はAnthropicのベストプラクティスに合致している。**

### 3.3 結論

| 項目 | 判断 |
|------|------|
| 設計変更 | **不要** |
| 理由 | 現状がベストプラクティス |
| 追加検討 | なし |

---

## 4. ドキュメント削減

### 4.1 現状の問題

| 問題 | 具体例 | 影響 |
|------|--------|------|
| **重複** | imple.md (678行) ≒ kojo.md (611行) の80% | 変更時の不整合リスク |
| **過剰定義** | Step 1, 2, 3... の手順列挙 | エージェント判断を奪う |
| **DRY違反** | 同じルールが複数箇所に | 保守コスト増 |

### 4.2 Anthropic推奨原則

1. **最小ルール**: 手順ではなく判断基準のみ
2. **エージェント判断**: How はエージェントに任せる
3. **コード強制**: 重要ルールはHooksで
4. **単一ソース**: 重複排除

### 4.3 統合・削減計画

| 対象 | 現状 | 目標 | アクション |
|------|-----:|-----:|-----------|
| imple.md | 678 | ~200 | 手順削除、判断基準のみ |
| kojo.md | 611 | 0 | imple.mdに統合（Type: kojo分岐） |
| subagent-strategy.md | 377 | ~50 | CLAUDE.mdに原則のみ統合 |
| kojo-writer.md | 421 | ~100 | 品質基準のみ |
| ac-tester.md | 432 | ~100 | 入出力とマッチャーのみ |
| 他agent.md | 各~100 | 各~50 | 入出力と判断基準のみ |

### 4.4 削減原則

**残すもの**:
- 入力: 何を受け取るか
- 出力: 何を返すか
- 判断基準: どう判断するか
- 例: 1つだけ

**削除するもの**:
- Step列挙（エージェント判断に任せる）
- 重複内容（1箇所に集約）
- CRITICAL警告（Hooksで強制）
- 複数の例（1つで十分）

### 4.5 統合後の構造

```
CLAUDE.md (~150行)
├── Project Overview
├── Quick Start
├── Subagent Strategy (原則のみ)
├── Slash Commands
└── Key Documents

.claude/commands/
├── imple.md (~200行) ← kojo.md統合
├── next.md (~100行)
├── commit.md (~80行)
└── ...

.claude/agents/
├── kojo-writer.md (~100行)
├── ac-tester.md (~100行)
├── implementer.md (~80行)
└── ... (各50-100行)

.claude/hooks/
└── post-erb-write.ps1
```

---

## 5. 実装計画

### Phase A: Hooks実装（低リスク）

1. `.claude/hooks/post-erb-write.ps1` 作成
2. `.claude/settings.json` 更新
3. テスト: ERBファイル編集後にBOM付加・ビルド検証が動作確認

### Phase B: ドキュメント統合（中リスク）

1. kojo.md → imple.md 統合
2. subagent-strategy.md → CLAUDE.md 統合
3. 各agent.md 削減
4. テスト: /imple が正常動作確認

### Phase C: 検証

1. Feature 1件を /imple で実行
2. 問題なければ完了

---

## 6. リスクと対策

| リスク | 対策 |
|--------|------|
| Hooks失敗でワークフロー停止 | exit 0 で非ブロック、情報提供のみ |
| ドキュメント削減で情報不足 | 段階的削減、問題発生時に追記 |
| 統合で重要情報が欠落 | 統合前にdiffレビュー |

---

## 7. 参考資料

- [Claude Code Hooks - 公式ドキュメント](https://code.claude.com/docs/en/hooks)
- [Hooks Guide](https://code.claude.com/docs/en/hooks-guide)
- [hooks-reference.md](../reference/hooks-reference.md) - プロジェクト内参照
- [sessions-reference.md](../reference/sessions-reference.md) - プロジェクト内参照

---

## 議論ログ

| 日付 | 内容 |
|------|------|
| 2025-12-20 | 初期作成、全Hooksイベント検討、Sessions/ドキュメント分析 |
