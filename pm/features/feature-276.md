# Feature 276: ErbLinter Phase 6 統合修正

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
静的解析ツールは作成するだけでなく、ワークフローに統合して自動実行されなければ価値がない。
発見は早いほど修正コストが低い。

### Problem (Current Issue)
- F272 で ErbLinter を統合したが、do.md の記載と実際の動作に乖離がある
- do.md Phase 6 PostToolUse Hooks セクションに ErbLinter が記載されている（line 397）
- do.md は「Automatic via PostToolUse Hooks」と記載しているが、これは Claude Code 組み込みの hooks 機能ではなく、エージェントの期待動作を記述したドキュメント
- 実際に自動実行されるのは pre-commit hook のみ（commit 時 = 発見が遅い）
- /do 実行時に Phase 6 で linter が明示的に実行される手順がない

### Goal (What to Achieve)
1. do.md Phase 6 で ErbLinter を明示的に実行するよう修正
2. Phase 8 Report に linter 結果を含める
3. pre-commit は最終安全網として維持（F272 で実装済み、本 Feature では変更しない）

**Note**: PostToolUse Hooks リストから削除し、Phase 6 の明示的ステップとして追加。これにより per-file（自動）から per-phase（明示的）に変更。pre-commit が safety net として残る。

### Context
- F272 は pre-commit 統合を完了（AC2 PASS）、ただし /do での自動実行は未実装
- F245 実行時に /do 中の linter 実行漏れが発覚

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1a | do.md Phase 6 に Manual Linter Check セクション追加 | code | contains | "### Manual Linter Check" | [x] |
| 1b | Manual Linter Check に ErbLinter コマンド記載 | code | contains | "-l warning Game/ERB/" | [x] |
| 2 | do.md Phase 8 Report に linter summary 追加 | code | contains | "**Linter** (ErbLinter):" | [x] |
| 3 | do.md PostToolUse Hooks リストから ErbLinter を Manual Linter Check へ移動 | code | not_contains | "ErbLinter static analysis" | [x] |

### AC Details

**AC1a/1b**: Phase 6 に「### Manual Linter Check」サブセクションを追加
- 位置: do.md の Hook Result テーブル後、FAIL Response Flow セクション前
- 実行フロー: PostToolUse Hooks（自動検証）完了後に 1 回実行
- CWD: repository root（/do は常に repo root から実行）
```bash
dotnet run --project tools/ErbLinter/ErbLinter.csproj -- -l warning Game/ERB/
```

**AC2**: Phase 8 Report テンプレートの **Log Verification** セクション後に追加
```
**Linter** (ErbLinter):
Errors:   {N}
Warnings: {N}
```

**AC3**: PostToolUse Hooks リスト（line 390-398付近）から ErbLinter を「Manual Linter Check」サブセクション（AC1）へ移動。

**Note**: AC2 は Linter セクションヘッダの存在を検証。完全なフォーマット（Errors/Warnings 行）はコードレビューで確認。

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1a,1b | do.md Phase 6 に Manual Linter Check セクション追加（ErbLinter コマンド含む） | [O] |
| 2 | 2 | do.md Phase 8 Report テンプレートに linter 追加 | [O] |
| 3 | 3 | do.md PostToolUse Hooks リストから「ErbLinter static analysis」行を削除 | [O] |

---

## Review Notes

- F272 は do.md PostToolUse Hooks リストへの記載と pre-commit 統合を完了。しかし PostToolUse Hooks は自動実行機構ではないため、明示的実行手順の追加が必要。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-31 18:49 | START | implementer | Tasks 1-3 | - |
| 2025-12-31 18:49 | END | implementer | Tasks 1-3 | SUCCESS |

---

## Links
- [index-features.md](index-features.md)
- [feature-272.md](feature-272.md) - 元の統合Feature
- [do.md](../../.claude/commands/do.md)
