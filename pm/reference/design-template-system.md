# {機能名} システム設計

## Status: DRAFT

---

## 概要

{システムの目的、達成したいこと}

---

## 背景・動機

{解決したい課題、なぜこのシステムが必要か}

---

## 解決方針

{技術的アプローチ、実装方針}

---

## 実装フェーズ案

| Phase | 内容 | Feature候補 | AC目安 |
|:-----:|------|-------------|:------:|
| 1 | {Phase 1の内容} | feature-XXX | 8-12 |
| 2 | {Phase 2の内容} | feature-XXX | 8-15 |

**AC粒度ガイド**:
- 1 AC = 1 Task = 1 Dispatch
- ACはタスク可能な最小単位（ファイル作成/変更、検証可能な動作）
- 8-15 ACs per Feature推奨

---

## 技術的依存関係

### ERB依存
- {file/function dependencies}

### Engine依存
- {C# module dependencies}

### CSV依存
- {data definition dependencies}

---

## 未解決事項

| 項目 | 詳細 | 優先度 |
|------|------|:------:|
| {item} | {description} | {High/Med/Low} |

---

## 議論ログ

| 日付 | 内容 |
|------|------|
| {YYYY-MM-DD} | 初期作成 |

---

## Links

- [content-roadmap.md](../content-roadmap.md)
- [index-features.md](../index-features.md)
