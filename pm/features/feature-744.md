# Feature 744: Incident-Point Recovery (Simplified Approach)

## Status: [CANCELLED]

### Cancellation Reason

完全復旧断念。手動復旧を開始したため。

## Type: infra

## Background

### Philosophy (Mid-term Vision)

**Complete recovery = Start from known good state (incident-point HEAD) + replay all operations after that point.**

Previous approaches (F733-F742) failed because they used complex strategies (timestamp filtering, session-start state, virtual state tracking) that introduced chain gaps. The simplest approach: use the exact commit that existed when the incident happened, and replay ALL operations from that point forward.

---

### Problem (Current Issue)

All previous recovery attempts failed:

| Feature | Approach | Result |
|---------|----------|--------|
| F733 | Base=null, all ops | 85% skip |
| F738 | Base=HEAD, filter by commit time | 8.26% skip |
| F740 | Base=git at first op time | 21.74% skip |
| F742 | F740 + dedup | 9.32% skip, merge broke code |
| F743 | Validation-first hybrid | Not implemented |

**Root cause of failures**: Using wrong base state. Operations expect a specific file state, but the base state didn't match.

### Key Insight

At the incident (`git checkout -- .`), the working directory was reverted to HEAD at that moment. That HEAD is a **known good state** where all committed code was consistent.

Operations recorded in session files were building on that HEAD. If we:
1. Start from that exact HEAD
2. Replay ALL operations recorded after that point
3. Operations should chain correctly (no gaps)

---

### Goal (What to Achieve)

1. Identify the exact commit at incident time: `088f4408`
2. Extract all operations after `2026-02-01 21:29:27` (that commit's timestamp)
3. Replay operations starting from that commit's file states
4. Achieve 0% skip rate (or identify truly concurrent edits)

---

## Technical Design

### Approach

```
1. Base commit: 088f4408 (2026-02-01 21:29:27)
2. For each file:
   a. Get file content at 088f4408: git show 088f4408:<path>
   b. Extract operations for that file with timestamp > 2026-02-01 21:29:27
   c. Sort by timestamp
   d. Deduplicate by toolUseId
   e. Apply all operations sequentially
3. Validate result (tests, undefined references)
4. Merge to HEAD
```

### Why This Should Work

| Problem | Solution |
|---------|----------|
| Wrong base state | Use exact commit at incident time |
| Chain gaps from filtering | No filtering - apply ALL ops |
| Multi-session state divergence | All sessions started from same HEAD |
| Definition skipped, usage applied | Definitions and usages both applied in order |

### Key Difference from Previous Approaches

| Aspect | F738 | F740/F742 | F744 |
|--------|------|-----------|------|
| Base | Current HEAD | Git at first op time | **Incident-point commit** |
| Filter | By commit timestamp | None | By incident timestamp |
| Scope | Per-file filter | Per-file git lookup | **Global incident time** |

---

## Implementation Changes

### index.js modifications

```javascript
// F744: Use fixed incident-point commit instead of per-file git lookup
const INCIDENT_COMMIT = '088f4408';
const INCIDENT_TIMESTAMP = new Date('2026-02-01T21:29:27+09:00').getTime();

// For each file:
// 1. Get base content from incident commit (not current HEAD, not first-op time)
const baseContent = execSync(
  `git show ${INCIDENT_COMMIT}:"${gitPath}"`,
  { encoding: 'utf8', cwd: gitRoot }
);

// 2. Filter operations to only those AFTER incident
const postIncidentOps = operations.filter(op => op.timestamp > INCIDENT_TIMESTAMP);

// 3. Apply all operations (no additional filtering)
const result = replayOperations(postIncidentOps, baseContent);
```

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | Incident commit identified | code | contains | `088f4408` | [ ] |
| 2 | Base content from incident commit | code | contains | `git show 088f4408:` | [ ] |
| 3 | Operations filtered by incident timestamp | code | contains | `INCIDENT_TIMESTAMP` | [ ] |
| 4 | Skip rate < 1% | output | lte | 1% | [ ] |
| 5 | Dashboard tests pass after merge | test | succeeds | npm test | [ ] |
| 6 | No undefined references | output | not_contains | "is not defined" | [ ] |
| 7 | main.css has 6px borders | output | contains | "6px solid" | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Modify index.js to use incident-point approach | [ ] |
| 2 | 4 | Run extraction and measure skip rate | [ ] |
| 3 | 5,6 | Validate recovered code (tests, references) | [ ] |
| 4 | 7 | Verify specific lost content (6px, etc.) | [ ] |
| 5 | - | Merge to HEAD if validation passes | [ ] |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F742 | [DRAFT] | Previous approach (session-start state + dedup) |
| Related | F743 | [DRAFT] | Validation-first hybrid (not implemented) |
| Predecessor | F733 | [DONE] | Original session extractor |

---

## Links

- [F742](feature-742.md) - Session Extractor Complete Recovery
- [F743](feature-743.md) - Fundamental Redesign (validation-first)
- [F738](feature-738.md) - Session Extractor v1

---

## Review Notes

### Why This Approach is Different

Previous approaches all had one flaw: **they tried to be smart about the base state**.

- F738: "Use current HEAD, filter old ops" → Wrong: HEAD changed since incident
- F740: "Use git at first op time" → Wrong: First op might be from different session
- F742: "Add deduplication" → Helped, but wrong base state remained
- F743: "Validate before merge" → Detects problems, doesn't fix them

F744's insight: **Don't be smart. Use the exact state that existed when the incident happened.**

The incident was `git checkout -- .` which reverted to HEAD at `088f4408`. All session operations were recorded while building on states derived from that HEAD. Therefore, replaying from that exact point should produce consistent results.

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-03 | DRAFT | Created after F742/F743 analysis showed need for simpler approach |
| 2026-02-03 | IDENTIFIED | Incident commit: 088f4408 (2026-02-01 21:29:27) |

