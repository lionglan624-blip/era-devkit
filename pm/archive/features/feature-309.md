# Feature 309: kojo-writing SKILL DATAFORM テンプレート修正

## Status: [DONE]

## Type: infra

## Background

### Philosophy
kojo-writer が正しい ERB 形式を出力するために、SKILL テンプレートが正確である必要がある

### Problem
kojo-writing SKILL.md の DATALIST テンプレート (lines 301-304) が DATAFORM プレフィックスを含んでいない。
これにより F291 で K4 の DATAFORM 欠落問題が発生した。

**現在のテンプレート (誤り)**:
```erb
DATALIST
　「セリフ」
地の文
ENDLIST
```

**正しい形式**:
```erb
DATALIST
	DATAFORM             ; 空行生成（セクション区切り）
	DATAFORM 「セリフ」
	DATAFORM 地の文
ENDLIST
```

### Goal
SKILL.md の DATALIST テンプレートを正しい DATAFORM 形式に修正する

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SKILL.md DATALIST内にDATAFORM記載 | code | Grep(.claude/skills/kojo-writing/SKILL.md) | contains | "\tDATAFORM" | [x] |
| 2 | セリフ行にDATAFORMプレフィックス | code | Grep(.claude/skills/kojo-writing/SKILL.md) | contains | "DATAFORM 「セリフ」" | [x] |
| 3 | 地の文行にDATAFORMプレフィックス | code | Grep(.claude/skills/kojo-writing/SKILL.md) | contains | "DATAFORM 地の文" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | SKILL.md DATALIST 内に空行 DATAFORM 追加 | [x] |
| 2 | 2 | SKILL.md セリフ行に DATAFORM プレフィックス追加 | [x] |
| 3 | 3 | SKILL.md 地の文行に DATAFORM プレフィックス追加 | [x] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 15:53 | START | implementer | Tasks 1-3 | - |
| 2026-01-02 15:53 | END | implementer | Tasks 1-3 | SUCCESS |

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-291.md](feature-291.md)
- 対象: [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
