# Feature 754: YAML Format Unification (branches → entries)

## Status: [CANCELLED]

> **Cancelled**: 2026-02-11 — 591/591 PASS (F706) が混在形式で成立。残存~108 パチュリーファイルは機能的問題なし。形式統一は cosmetic cleanup であり優先度低。

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Phase 19 completion requires all YAML files to use the standardized entries format consumed by Era.Core runtime. A single canonical format eliminates parser complexity and ensures consistent behavior.

### Problem (Current Issue)
Historical artifact: YAML files exist in two incompatible formats:
- **branches format** (legacy): Created by initial ErbToYaml, used by KojoComparer
- **entries format** (canonical): Designed in F675 for Era.Core DialogueRenderer

F750 added TALENT conditions to branches format files. Equivalence verification (F706) must complete before format conversion to ensure no data loss.

**Drift Note (2026-02-10)**: 528/636 files already converted to entries: format. Only ~108 files remain in branches: format (all in `3_パチュリー_yaml/` directory). Scope significantly reduced from original estimate.

### Goal (What to Achieve)
Convert remaining ~108 パチュリー YAML files from branches to entries format after F706 validates ERB==YAML equivalence.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F706 | [DONE] | Equivalence verification must pass before format conversion |
| Related | F750 | [DONE] | TALENT conditions added to branches format |
| Related | F675 | [DONE] | entries format design and ErbToYaml implementation |

---

## Scope

Convert remaining ~108 パチュリー YAML files from branches to entries format using existing BranchesToEntriesConverter infrastructure.

**Out of Scope**:
- KojoComparer modification (evaluate separately if branches reading can be removed)
- New YAML schema validation (existing dialogue-schema.json supports entries)

---

## Links
- [feature-706.md](feature-706.md) - Equivalence verification (predecessor)
- [feature-750.md](feature-750.md) - TALENT condition migration
- [feature-675.md](feature-675.md) - entries format design

---

<!-- fc-phase-1-completed -->
<!-- Remaining sections to be completed via /fc 754 -->
