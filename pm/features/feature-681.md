# Feature 681: Multi-Entry Selection and Rendering Pipeline

## Status: [DONE]

## Type: engine

## Created: 2026-01-30

---

## Summary

Extend Era.Core's KojoEngine to support multi-entry selection and rendering pipeline. Current design assumes single-entry selection (one DialogueEntry per GetDialogue call). This feature enables selecting and rendering multiple entries, propagating per-entry displayMode metadata to DialogueLine instances in the aggregated DialogueResult.

---

## Background

F676 added displayMode propagation for single-entry rendering. The current KojoEngine.GetDialogue() selects one entry via IDialogueSelector.Select() and renders it. Multi-entry selection requires pipeline changes to aggregate results from multiple entries while preserving per-entry displayMode semantics.

---

## Links

- [feature-676.md](feature-676.md) - Era.Core Renderer DisplayMode Integration (Predecessor - single-entry displayMode propagation)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F676 | [DONE] | Single-entry DisplayMode propagation through pipeline |

---

## Notes

- Created by F676 残課題 (deferred item)
- Single-entry displayMode mapping pattern will need refactoring for multi-entry
- Per-entry metadata in DialogueLine enables different display modes within one dialogue result

---

## Execution Log

| Timestamp | Event | Source | Action | Detail |
|-----------|-------|--------|--------|--------|
| 2026-01-30 | START | /run | Phase 1 | F681 resume (already WIP) |
| 2026-01-30 | DEVIATION | Bash | dotnet build Era.Core.Tests | exit code 1 - implementer subagent reported success but did not write files; manual implementation required |
| 2026-01-30 | DEVIATION | Bash | dotnet build Era.Core.Tests | exit code 1 - 3 existing mock IKojoEngine implementations missing GetDialogueMulti method after interface change |
| 2026-01-30 | END | /run | Phase 8 | All 17 ACs PASS, 1443 tests pass |
