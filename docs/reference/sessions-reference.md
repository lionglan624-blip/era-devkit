# Sessions Reference

> **Purpose:** 会話の保存・再開、Subagent間のコンテキスト継承

---

## 基本概念

**Sessions** = 会話履歴・ツール実行結果・コンテキストの保存・再開機能

---

## CLI使い方

```bash
claude --continue    # 最後の会話を再開
claude --resume      # セッション選択メニュー
```

セッション内:
```
/resume              # 別の会話に切り替え
/rename feature-130  # 名前を付ける
```

---

## Task tool での resume

```
Task(
  subagent_type: "general-purpose",
  model: "sonnet",
  prompt: "Continue the analysis",
  resume: "agent-abc123"  # 前回のAgent ID
)
```

---

## 1:1 vs 1:N パターン

### 1:1 (シーケンシャル) ← resume向き

```
Agent A → Agent B → Agent C
  ↓ resume   ↓ resume
```

### 1:N (並列) ← ファイルキャッシュ向き

```
Agent A → cache/file.txt
              ↓
    ┌─────────┼─────────┐
    ↓         ↓         ↓
Agent B1   Agent B2   Agent B3
```

**結論**: 1対多はresumeではなくファイルキャッシュ使用

---

## 参考

- [Claude Code Sessions公式](https://docs.anthropic.com/en/docs/claude-code/sessions)
