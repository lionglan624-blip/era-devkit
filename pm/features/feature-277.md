# Feature 277: kojo Feature の Phase 2 スキップ

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
コンテキストウィンドウは有限リソース。不要な調査でトークンを消費すべきでない。

### Problem (Current Issue)
- /do Phase 2 で explorer を dispatch し、詳細な調査レポートを取得
- kojo Feature では Implementation Contract が固定パターン
- explorer の出力がコンテキストを消費（F245 で約30%使用の一因）
- kojo では feature-{ID}.md の Contract に従うだけで十分

### Goal (What to Achieve)
kojo Type の場合、Phase 2 をスキップまたは最小限に簡略化し、コンテキスト消費を削減

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md Phase 2 に Type routing 追加 | code | Grep | contains | "Type: kojo" and "Skip" | [x] |
| 2 | kojo で explorer dispatch なし | code | Grep | contains | "erb/engine: explorer dispatch" | [x] |

### AC Details

**AC1**: Phase 2 セクションに Type による分岐を追加
```markdown
| Type | Phase 2 Action |
|------|----------------|
| kojo | スキップ（Contract に従う） |
| erb/engine/infra | explorer dispatch |
```

**AC2**: kojo の場合は explorer を呼ばないことを明記

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | do.md Phase 2 に Type routing 追加 | [x] |

---

## Review Notes

- F245 実行時に発見。kojo で explorer が冗長だった。
- コンテキスト消費削減が目的。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

---

## Links
- [index-features.md](index-features.md)
- [do.md](../../.claude/commands/do.md)
