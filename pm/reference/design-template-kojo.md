# {Version} {番台}番台COM口上設計

## Status: DRAFT

---

## スコープ

| 番台 | COM数 | 優先度 | キャラ | 分岐 | 行数 |
|------|:-----:|:------:|:------:|:----:|:----:|
| {番台} | {N} | ★★★ | 10全員 | TALENT×{N} | {N}-{N} |

**説明**: {この番台の口上の特徴、プレイ内容}

---

## 口上量予想

計算:
```
{N} COM × 10キャラ × {N}分岐 × {N}行 = 約{total}行
```

**工数予想**: {N}日 × {N}COM = 約{N}日

---

## Feature分割案

| ID | Type | Name | COM範囲 | AC数 |
|:---|:----:|------|:-------:|:----:|
| F-XXX | kojo | COM_{N}-{N} {名前} | {N} COM | 10-12 |
| F-XXX | kojo | COM_{N}-{N} {名前} | {N} COM | 10-12 |

**分割方針**: {1-2 COM per Feature, プレイパターンで分割}

---

## 口上パターン参考

既存実装からの参考:
- **COM_{N}**: {pattern summary}
- **分岐**: TALENT:{talent_name} による {N}パターン
- **平均行数**: {N}行/キャラ

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
