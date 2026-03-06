# Feature 322: eratw-reader に Bash ツール追加

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Subagent は自身の agent.md に記載された優先順位と手順を、ツール制約なしに実行できるべきである。

### Problem (Current Issue)
eratw-reader.md では eraTW パス取得の優先順位を以下のように定義している：
1. 環境変数 `ERATW_PATH`
2. CLAUDE.md の記載
3. Fallback パス

しかし、eratw-reader の tools は `Read, Write, Grep` のみで **Bash がない**。
`Read/Write/Grep` ツールでは環境変数にアクセスできないため、優先順位1を実行することが不可能。

**発見経緯**: F319 実行時、eratw-reader が `ERR:file_not_found` を返した。調査の結果、tools と優先順位の不整合が判明。

### Goal (What to Achieve)
eratw-reader.md の tools に Bash を追加し、環境変数 `ERATW_PATH` を取得可能にする。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | eratw-reader.md の tools に Bash 含む | code | Grep | contains | "tools: Read, Write, Grep, Bash" | [x] |
| 2 | 環境変数取得手順が Process に記載 | code | Grep | contains | "echo $ERATW_PATH" | [x] |
| 3 | eratw-reader dispatch 後 cache 作成 | file | Glob | exists | Game/agents/cache/eratw-COM_80.txt (※ERATW_PATH要設定) | [x] |
| 4 | Decision Criteria で bash ファイル操作のみ禁止 | code | Grep | contains | "NEVER use Bash for file read/write" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | eratw-reader.md の tools 行に Bash 追加 | [x] |
| 2 | 2 | Process セクションに環境変数取得手順追加 | [x] |
| 3 | 3 | cache 削除 → eratw-reader dispatch COM_80 → cache 再作成確認 | [x] |
| 4 | 4 | Decision Criteria を「NEVER use Bash for file read/write」に修正 | [x] |

---

## 設計詳細

### 現状

```yaml
# eratw-reader.md line 5
tools: Read, Write, Grep
```

### 変更後

```yaml
tools: Read, Write, Grep, Bash
```

### Process 追加内容

**変更方法**: Step 0 として挿入。既存の Steps 1-4 は変更なし。

```markdown
## Process

0. **Bash** で環境変数 `ERATW_PATH` を取得
   ```bash
   echo $ERATW_PATH
   ```
   - 設定されていれば → そのパスを使用
   - 未設定 → CLAUDE.md fallback を使用

1. **Read** eraTW file first ...
```

### 代替案（不採用）

| 案 | 内容 | 評価 |
|----|------|:----:|
| A | tools に Bash 追加 | ◎ 採用 |
| B | 優先順位から環境変数を削除 | △ 設計思想に反する |
| C | CLAUDE.md パスをハードコード | × メンテナンス性低下 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-03 18:40 | START | implementer | Task 1-4 | - |
| 2026-01-03 18:40 | END | implementer | Task 1 | SUCCESS |
| 2026-01-03 18:40 | END | implementer | Task 2 | SUCCESS |
| 2026-01-03 18:40 | END | implementer | Task 3 | PARTIAL (cache deleted) |
| 2026-01-03 18:41 | END | eratw-reader | COM_80 dispatch | SUCCESS (cache created) |
| 2026-01-03 18:40 | END | implementer | Task 4 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- 発見元: [feature-319.md](feature-319.md)
- 対象: [eratw-reader.md](../../.claude/agents/eratw-reader.md)
