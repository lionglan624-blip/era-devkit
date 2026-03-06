# Feature 307: F289 Phase 8 問題解決フロー改善

## Status: [DONE]

## Type: infra

## Background

### Problem
F289 実行中に Phase 8 の報告で以下の問題が発生:
1. ワークフロー逸脱（Bash 使用）が最終報告で漏れた
2. 問題発生時の対応フローが未定義だった
3. 「全ての問題 → ドキュメント改善」の原則が明示されていなかった

### Context
- F289 (COM_83 kojo) 実行中に ERB 関数存在確認で Bash を使用
- do.md は Grep ツール使用を指定していた
- 最終報告で deviation を報告せず、ユーザー指摘で発覚

---

## 議論の経緯

### 1. 発端: Bash 構文エラー

```
Bash(cd "C:/Era/..." && for i in 1..10; do result=$(grep -l "@KOJO_MESSAGE_COM_K${i}_83$" ...))
→ Error: syntax error near unexpected token `grep'
```

**問題**: do.md Phase 4 で Grep ツール使用を指定していたが、Bash コマンドを使用

### 2. 報告漏れの指摘

**ユーザー**: 「あらゆる手違いを報告する認識ではなかったか？」

**問題**: 最終報告の Execution Notes に deviation を記載しなかった

### 3. ドキュメント改善の原則

**ユーザー**: 「あらゆる問題はドキュメントとワークフローで改善すると書いていないか？」

**問題**: この原則が明示的に記載されていなかった

### 4. 逸脱の原因分析

| # | 原因 | 対処 | 評価 |
|:-:|------|------|------|
| 1 | Phase 4 実行時に do.md を再読しなかった | 運用改善 | △ |
| 2 | 10 回の Grep 呼び出しを「非効率」と判断 | 意識改善 | △ |
| 3 | do.md の記載が「例示」に見えた | ドキュメント明確化 | ○ |
| 4 | Grep ツール 10 並列呼び出しのコード例がない | コード例追加 | ◎ 推奨 |

**採用**: #4 - do.md に具体的なコード例を追加

### 5. 報告漏れの原因分析

| # | 原因 | 対処 | 評価 |
|:-:|------|------|------|
| 1 | エラーが解決したため「報告不要」と判断 | 解決済みも報告対象と明記 | ○ |
| 2 | 最終報告時に全 Phase を振り返らなかった | チェックリスト追加 | △ |
| 3 | Deviation 発生時に即座に記録しなかった | 即時記録ルール追加 | ◎ 推奨 |

**採用**: #3 - Deviation Recording ルールを do.md に追加

### 6. Phase 8 の構造改善

**ユーザー提案**:
- Phase 8 を「問題解決の場」として再定義
- 問題がある場合は Finalize を求めず、解決フローへ
- 各問題に対処案を提示し、ユーザー判断を求める

**採用**: Phase 8 を Step 8.1-8.4 に再構成

---

## 実施した修正

### 修正 1: do.md Phase 4 - ERB function existence check

**変更箇所**: `.claude/commands/do.md` Phase 4 kojo セクション

**変更内容**:
```markdown
4. If count == 10: **ERB function existence check**
   - For each K1-K10: Call Grep tool 10 times in parallel
   ```
   Grep("@KOJO_MESSAGE_COM_K1_{COM}", path: "Game/ERB/口上", output_mode: "files_with_matches")
   Grep("@KOJO_MESSAGE_COM_K2_{COM}", path: "Game/ERB/口上", output_mode: "files_with_matches")
   ...
   Grep("@KOJO_MESSAGE_COM_K10_{COM}", path: "Game/ERB/口上", output_mode: "files_with_matches")
   ```
   - **Do NOT use Bash**: Use Grep tool to avoid shell variable expansion issues
```

### 修正 2: do.md Phase 8 - Deviation Recording

**変更箇所**: `.claude/commands/do.md` Phase 8 Step 8.2

**変更内容**:
```markdown
**Deviation Recording**: When any error, retry, or workaround occurs during execution,
immediately record it in feature-{ID}.md Execution Log with result "DEVIATION".
Review all DEVIATION entries before proceeding.
```

### 修正 3: do.md Phase 8 - 問題解決フロー

**変更箇所**: `.claude/commands/do.md` Phase 8 全体を再構成

**変更内容**:
- Step 8.1: Verify All Tests (既存)
- Step 8.2: Check for Problems (新規) - Deviation 有無でルーティング
- Step 8.3: Problem Resolution (新規) - 問題解決フロー
- Step 8.4: Clean Report (既存を移動) - 問題なしの場合のみ到達可能

**Step 8.3 の構造**:
1. 問題を種類別に列挙 (test_fail/deviation/scope_out)
2. 各問題に対処テーブル (選択肢 + 推奨)
3. Feature 作成提案
4. ユーザー判断を求める
5. 対応完了後、Step 8.2 を再実行

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | do.md Phase 4 に Grep 具体例追加 | code | contains | "Grep(\"@KOJO_MESSAGE_COM_K1" | [x] |
| 2 | do.md Phase 4 に Bash 禁止追加 | code | contains | "Do NOT use Bash" | [x] |
| 3 | do.md Phase 8 に Deviation Recording 追加 | code | contains | "DEVIATION" | [x] |
| 4 | do.md Phase 8 に Step 8.3 Problem Resolution 追加 | code | contains | "Problem Resolution" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | do.md Phase 4 修正 | [x] |
| 2 | 3 | do.md Deviation Recording 追加 | [x] |
| 3 | 4 | do.md Phase 8 問題解決フロー追加 | [x] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 | deviation | - | Bash 構文エラー発生、Grep で回避 | DEVIATION |
| 2026-01-02 | report | - | 最終報告で deviation 漏れ | ユーザー指摘 |
| 2026-01-02 | discussion | - | 原因分析、対処検討 | 3点の修正決定 |
| 2026-01-02 | fix | - | do.md Phase 4 Grep 具体例追加 | OK |
| 2026-01-02 | fix | - | do.md Deviation Recording 追加 | OK |
| 2026-01-02 | fix | - | do.md Phase 8 問題解決フロー追加 | OK |
| 2026-01-02 | complete | - | 全修正完了、Feature 作成 | DONE |

## Links

- [index-features.md](index-features.md)
- Related: [feature-289.md](feature-289.md) - 発生源
- Modified: [do.md](../../.claude/commands/do.md)
