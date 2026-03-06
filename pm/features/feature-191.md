# Feature 191: /imple Self-Audit Principle

## Status: [CANCELLED] → Feature 202 に統合

## Type: infra

## Background

### Problem
/imple ワークフローで発生した異常（BOM欠落、ファイル配置ミス、MODIFIER欠落等）が自動検出されなかった。

**ユーザーの指摘:**
> 具体的チェックリストを作ったら無限に増える。
> 正常に動くこと以外は全て異常として報告させたい。
> 私の指摘は全てあなたの発言から拾っている。
> あなたも同じことができるはず。

### Goal
**Self-Audit Principle**: Opus 自身の出力・観察から逸脱を検出し報告する。

### Root Cause
- チェックリストベースの検証は網羅性に限界がある
- 実際の異常は「期待と異なる出力」として現れる
- 人間（ユーザー）はその差分を即座に検出できるが、Opus は見逃す

---

## Design

### Self-Audit フロー

```
1. 各Phase完了時に「期待した結果」を明記
2. 実際の結果と比較
3. 差分があれば全て報告（理由を問わず）
```

**例:**
```
Expected: 10 files created in Game/ERB/口上/*/KOJO_K*_挿入.ERB
Actual: 3 files in _挿入.ERB, 7 files in _口挿入.ERB
→ DEVIATION: File naming inconsistency detected
```

### 実装方針

1. **ログは編集禁止** - logs/ 以下は読み取り専用
2. **tests/debug/ で調査** - 本番テストは変更禁止
3. **全ての「〜のはず」を検証** - 暗黙の前提を明示的に確認
4. **差分は全て報告** - 重要度判断はユーザーが行う

### Orchestrator (Opus) の責務

| 観察 | 期待 | 差分 | 報告 |
|------|------|------|------|
| "160/160 passed" | サブエージェント報告と一致 | 不一致 | 報告 |
| ファイル作成 | 指定パスに作成 | 別パスに作成 | 報告 |
| Hook 実行 | ログに記録 | ログ空 | 報告 |
| テスト実行 | エラーなし | パースエラー | 報告 |

### 既存ドキュメントとの整合

**regression-tester.md には正しく `--flow` と書いてある。**

問題: サブエージェントが従わなかった。

**対策:**
- Phase 8 後に Opus が直接コマンドを実行し、結果を検証
- サブエージェント報告を盲目的に信頼しない（既存ルール）

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | 期待値明記 | doc | contains | "Expected:" in Phase output | [ ] |
| 2 | 差分報告 | doc | contains | "DEVIATION:" when mismatch | [ ] |
| 3 | ログ不変 | code | not_contains | `Edit.*logs/` | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-2 | /imple コマンド修正（期待値/差分フォーマット） | [ ] |
| 2 | 3 | debugger.md にログ不変ルール追加 | [ ] |

---

## Example Output

```
=== Phase 4: Implementation ===

Expected:
- 10 files: Game/ERB/口上/{1-10}_*/KOJO_K{1-10}_挿入.ERB
- All files: UTF-8 BOM
- All functions: CALLF KOJO_MODIFIER_PRE/POST_COMMON

Actual:
- 3 files in _挿入.ERB, 7 files in _口挿入.ERB
- 1 file without BOM (K3)
- 1 function missing MODIFIER (K10)

DEVIATION:
1. File placement: 7/10 in wrong category file
2. Encoding: 1/10 missing BOM
3. Code: 1/10 missing MODIFIER calls

Action: STOP → Report to user
```

---

## Links

- [imple.md](../../.claude/commands/imple.md)
- [debugger.md](../../.claude/agents/debugger.md)
