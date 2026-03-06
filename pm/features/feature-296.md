# Feature 296: kojo-writing SKILL Phase 拡張

## Status: [DONE]

## Type: infra

## Background

### Philosophy
kojo-writing SKILL を Phase 定義の SSOT (Single Source of Truth) にし、Phase 8d/8j/8l の品質基準・テンプレートを一元管理する

### Problem
現在の kojo-writing SKILL は Phase 8d の品質基準と ERB Template のみを定義。F295 で追加される Phase 8j (射精状態) / Phase 8l (初回実行) に対応する定義がない。

### Goal
kojo-writing SKILL に Phase Definitions セクションを追加し、Phase 別の品質基準・ERB Template・対象 COM を明記する。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase Definitions セクション存在 | code | Grep(SKILL.md) | contains | "## Phase Definitions" | [x] |
| 2 | Phase 8d 定義存在 | code | Grep(SKILL.md) | contains | "Phase 8d" | [x] |
| 3 | Phase 8j 定義存在 | code | Grep(SKILL.md) | contains | "Phase 8j" | [x] |
| 4 | Phase 8l 定義存在 | code | Grep(SKILL.md) | contains | "Phase 8l" | [x] |
| 5 | FIRSTTIME キーワード存在 | code | Grep(SKILL.md) | contains | "FIRSTTIME" | [x] |
| 6 | 完全分岐 RETURN 存在 | code | Grep(SKILL.md) | contains | "RETURN 1" | [x] |
| 7 | Phase 別品質基準存在 | code | Grep(SKILL.md) | contains | "8-15行以上" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Phase Definitions セクションを SKILL.md に追加 | [x] |
| 2 | 2 | Phase 8d 定義を整理・追加 | [x] |
| 3 | 3 | Phase 8j (射精状態) 定義を追加 | [x] |
| 4 | 4 | Phase 8l (初回実行) 定義を追加 | [x] |
| 5 | 5 | FIRSTTIME テンプレートを追加 | [x] |
| 6 | 6 | 完全分岐 RETURN パターンを追加 | [x] |
| 7 | 7 | Phase 別品質基準（8-15行以上）を明記 | [x] |

---

## Implementation Notes

### SKILL 構造変更

**現行**:
```markdown
## Branching
## ERB Template (8d のみ)
## Quality (8d のみ)
```

**変更後**:
```markdown
## Phase Definitions
├── Phase 8d: 基本口上
├── Phase 8j: 射精状態
└── Phase 8l: 初回実行

## Quality Standards (Phase別)
├── 8d: TALENT 4分岐, 4-8行
├── 8j: 完全分岐, 4-8行以上, 分岐ごとに4パターン
└── 8l: 完全分岐, 8-15行以上

## ERB Templates (Phase別)
├── 8d: モジュール式 (PRE/POST + 基本口上)
└── 8j/8l: 完全分岐 (即RETURN)
```

### Phase 定義詳細

| Phase | Name | 分岐方式 | 行数 | パターン | 対象 COM |
|:-----:|------|:--------:|:----:|:--------:|----------|
| 8d | 基本口上 | ネスト (TALENT 4分岐) | 4-8行 | 4 | 全 COM |
| 8j | 射精状態 | 完全分岐 (即RETURN) | 4-8行以上 | 分岐ごとに4 | COM_80-85 |
| 8l | 初回実行 | 完全分岐 (即RETURN) | 8-15行以上 | 1 | 全 88 COM (日常含む) |

### 完全分岐 ERB Template

```erb
;=== Phase 8l: FIRSTTIME（最優先判定） ===
IF FIRSTTIME(SELECTCOM)
    PRINTFORML
    PRINTFORMW 「セリフ」
    PRINTFORMW 地の文（8-15行以上）
    ...
    RETURN 1
ENDIF

;=== Phase 8j: 射精状態 ===
IF NOWEX:MASTER:11  ; 射精中
    IF TCVAR:精飲経験 >= 10
        PRINTFORMW ...（経験者）
    ELSE
        PRINTFORMW ...（初心者）
    ENDIF
    RETURN 1
ENDIF

;=== Phase 8d: 基本口上（モジュール式） ===
CALL TRAIN_MESSAGE
CALLF KOJO_MODIFIER_PRE_COMMON
CALL KOJO_MESSAGE_COM_K{N}_{X}_1
CALLF KOJO_MODIFIER_POST_COMMON
RETURN RESULT
```

---

## Dependencies

- **前提**: content-roadmap.md / kojo-phases.md に Phase 8j/8l が定義済み（F295 の成果物）
- **後続**: F297（ツール連携更新）

---

## Reference (SSOT)

- [content-roadmap.md](content-roadmap.md) - Phase 定義（SSOT）
- [kojo-phases.md](reference/kojo-phases.md) - Phase 詳細仕様（SSOT）
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md) - 変更対象
- [feature-295.md](feature-295.md) - 前提 Feature（依存）

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 | Phase 1 | initializer | Feature init | READY |
| 2026-01-01 | Phase 4 | implementer | SKILL.md update | SUCCESS |
| 2026-01-01 | Phase 6 | - | AC verification (Grep) | 7/7 PASS |
| 2026-01-01 | Phase 7 | feature-reviewer | Post-review | READY |
| 2026-01-01 | Phase 9 | finalizer | Status update | [DONE] |

---

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-295.md](feature-295.md)
