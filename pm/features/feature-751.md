# Feature 751: TALENT Semantic Mapping Validation

## Status: [CANCELLED]

> **Cancellation Reason**: Multi-Stateテスト(2364/2364 PASS)がTALENTセマンティック検証を完全に包含。恋人/恋慕/思慕の各ブランチ選択がERB==YAMLで機械的に証明済み。F709と同時にobsolete。

## Type: infra

## Background

### Philosophy (Mid-term Vision)
After achieving structural equivalence (ERB==YAML output matching), semantic validation ensures that TALENT-aware branch selection correctly reflects ERB behavior across different game states.

### Problem (Current Issue)
F750 implements TALENT condition injection and branch selection, but validation only uses empty state (representative test). True semantic validation requires testing with different TALENT states to confirm branches match ERB semantics.

**Drift Note (2026-02-11)**: F706 [DONE] (591/591 PASS)。空state検証は完了。F773でentries形式TALENT条件移行も完了。本Featureの焦点は多状態（恋人/恋慕/思慕/なし）でのセマンティック検証に限定される。F709と統合検討も可。

### Goal (What to Achieve)
Validate that KojoBranchesParser correctly selects branches based on TALENT state values, matching ERB runtime behavior.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F750 | [DONE] | TALENT condition injection and parser enhancement |
| Predecessor | F706 | [DONE] | KojoComparer batch infrastructure (591/591 PASS) |

---

## Scope

Deferred from F750 残課題. Resolves circular dependency between F750 and F706 by creating dedicated validation feature.

---

## Links
- [feature-750.md](feature-750.md) - TALENT condition injection (predecessor)
- [feature-706.md](feature-706.md) - KojoComparer batch infrastructure (predecessor)

---

<!-- fc-phase-1-completed -->
<!-- Remaining sections to be completed via /fc 751 -->
