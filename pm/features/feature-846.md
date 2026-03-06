# Feature 846: feature-status.py Active→Recently Completed migration gap

## Status: [DRAFT]

## Type: infra

## Deviation Context
<!-- Written by /run Phase 9. Raw facts only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F842 |
| Discovery Phase | Phase 10 (CodeRabbit Review) |
| Timestamp | 2026-03-06 |

### Observable Symptom
CodeRabbit review of F842 commit found F828 and F843 listed as [DONE] in the Active Features table of index-features.md, despite both being completed features that should have been moved to Recently Completed by their respective finalizer sessions.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `coderabbit review --plain --type committed --base-commit HEAD~1` |
| Exit Code | 0 |
| Error Output | 1 finding: "F843 appears in both Active Features and Recently Completed" |
| Expected | [DONE] features removed from Active, present only in Recently Completed |
| Actual | F828 [DONE] in Active only (not in Recently Completed); F843 [DONE] in both Active and Recently Completed |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/python/feature-status.py | Tool responsible for atomic status transitions including Active→Recently Completed migration |
| pm/index-features.md | Contains Active Features and Recently Completed tables |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| Manual Edit of index-features.md | Fixed | Workaround — root cause in feature-status.py or finalizer workflow not addressed |

### Parent Session Observations
Two distinct failure patterns observed: (1) F843 appeared in both Active and Recently Completed — `feature-status.py set 843 DONE` added to Recently Completed but failed to remove from Active. (2) F828 appeared only in Active as [DONE] — `feature-status.py set 828 DONE` may not have been run at all, or it failed to move the entry. F842 session JSONL confirms F842's own `feature-status.py set 842 DONE` executed correctly (output: "Active -> Recently Completed"). The bug is in **F828 and F843's respective finalizer sessions**, not F842's. Investigation: (a) find F828/F843 finalizer subagent JSONLs, (b) check if `feature-status.py set {ID} DONE` was invoked, (c) if invoked, check its output for anomalies (e.g., "Active -> Recently Completed" missing).

## Background

### Philosophy (Mid-term Vision)
feature-status.py is the SSOT tool for atomic feature status transitions. When it reports success, the Active and Recently Completed tables must be consistent — no [DONE] entries in Active, no missing entries in Recently Completed.

### Problem (Current Issue)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

### Goal (What to Achieve)
<!-- To be populated by /fc Phase 1 Deep Investigation -->

## Review Notes

<!-- FL persist_pending entries will be recorded here -->

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|---------------|--------------|
