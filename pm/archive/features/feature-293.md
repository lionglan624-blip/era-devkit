# Feature 293: kojo-writer 実装確認強化

## Status: [DONE]

## Type: infra

## Background

### Philosophy
エージェント手順違反の余地をなくし、確実に実装が完了したことを検証する仕組みを構築する。

### Problem
F288 で K5 kojo-writer が「ドキュメントを読んでサマリーを返すだけ」で終了し、実際の ERB コードを書かなかった。status file は作成されなかったが、TaskOutput では completed と表示された。

### Goal
1. kojo-writer.md に「必ず ERB ファイルを編集する」ことを冒頭で明示
2. polling 完了後（status file count==10）に ERB 関数存在確認ステップを追加

### Context
- 発生: F288 K5 (COM_82 パイズリ)
- 原因: エージェントが dispatch prompt を情報要求と誤解
- 対策: 二重検証（status file + Grep）

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | kojo-writer.md に実装必須の宣言追加 | code | Grep .claude/agents/kojo-writer.md | contains | "MUST edit ERB files" | [x] |
| 2 | do.md polling 後に ERB 関数存在確認追加 | code | Grep .claude/commands/do.md | contains | "ERB function existence check" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo-writer.md の # Kojo Writer Agent 直後、## Input 前に "MUST edit ERB files" 宣言を追加 | [○] |
| 2 | 2 | do.md Phase 4 Type: kojo の Polling Logic (count==10) 後、Phase 5 移行前に "ERB function existence check" ステップを追加 | [○] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| | | | | |

---

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-288.md](feature-288.md)
