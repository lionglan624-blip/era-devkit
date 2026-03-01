# Feature 740: Session Extractor Session-Internal State Tracking

## Status: [CANCELLED]

### Cancellation Reason

完全復旧断念。手動復旧を開始したため。

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: infra

## Background

### Philosophy (Mid-term Vision)

**Complete recovery requires tracking file state as it existed within each session, not as it exists in git HEAD.**

The git incident (`git checkout -- .`) caused massive data loss. F738 achieved significant improvements (85% → 8.27% skip rate) but the remaining skips are **chain gaps** caused by a fundamental design flaw: using `git show HEAD:path` as the base content.

When HEAD contains intermediate edits (e.g., `4px` committed), but the session needs to apply `3px → 4px → 6px`, the first edit is skipped (already in HEAD), causing the second edit (`4px → 6px`) to fail because it can't find `4px` in the `3px` base.

**The root cause is NOT the chain gap itself, but using post-session git state for pre-session operations.**

### Problem (Current Issue)

Current session-extractor has a fundamental design flaw causing chain gaps:

**1. Uses `git show HEAD:path` as base content**
- HEAD contains state AFTER all sessions, not at session start
- When a session made edits A→B→C and B was committed, applying A→B fails (already done)
- When skipping A→B, the B→C edit fails because it can't find B in A

**2. Timestamp filtering causes operation loss**
- `operations.filter(op => op.timestamp > commitTimestamp)` skips operations
- Skipped operations break the edit chain
- Example: main.css `3px`→`4px`→`6px` becomes `3px`→`?`→`6px` (gap)

**3. Evidence from investigation**
- 93 skipped operations remain (8.27% skip rate)
- main.css has 530 missing lines due to chain gaps
- Multiple files have similar undetected gaps

**4. Root Cause Analysis**

| Current Approach | Problem | Result |
|-----------------|---------|--------|
| `git show HEAD:path` | HEAD is post-session state | Wrong base for early edits |
| Timestamp filter | Skips "already committed" ops | Breaks edit chains |
| Per-file processing | No cross-file state awareness | Inconsistent recovery |

### Goal (What to Achieve)

Implement **session-internal state tracking** to achieve complete recovery:

1. **Determine session start git state**: Get the git commit at the time each session began
2. **Use session-start state as base**: NOT HEAD, but the state when the session started
3. **Remove timestamp filtering**: Apply ALL operations in chronological order
4. **Track intermediate state in memory**: Maintain file state as it evolves through operations
5. **Achieve <1% skip rate**: Ideally 0% (skips only for truly new files with first-edit-not-write)

**Target**: Skip rate 8.27% → <1% (ideally 0%)

---

## Technical Design

### Algorithm Overview

```
1. For each session JSONL file:
   a. Extract session start timestamp from first message
   b. Determine git commit at that timestamp: git rev-list -1 --before=<timestamp> HEAD
   c. Collect all operations from session

2. Global sort all operations by timestamp

3. For each file:
   a. Find the earliest operation's session
   b. Get git state at that session's start time (NOT HEAD)
   c. Apply ALL operations in order (NO timestamp filtering)
   d. Track intermediate content in memory
```

### Key Changes to index.js

#### 1. Session Start Timestamp Extraction

```javascript
// In parseSession or new function:
function getSessionStartTimestamp(sessionPath) {
  // Read first line of JSONL to get session start time
  const firstLine = readFirstLine(sessionPath);
  const firstMessage = JSON.parse(firstLine);
  return new Date(firstMessage.timestamp).getTime();
}
```

#### 2. Git State at Session Start

```javascript
// Get commit at session start time (not HEAD)
function getGitStateAtTime(gitRoot, timestamp) {
  const isoTime = new Date(timestamp).toISOString();
  const result = execSync(
    `git rev-list -1 --before="${isoTime}" HEAD`,
    { encoding: 'utf8', cwd: gitRoot }
  ).trim();
  return result; // Returns commit hash, or empty if no commits before that time
}

// Get file content at specific commit
function getFileAtCommit(gitRoot, gitPath, commitHash) {
  if (!commitHash) return null; // File didn't exist yet
  try {
    return execSync(
      `git -c core.quotepath=false show ${commitHash}:"${gitPath}"`,
      { encoding: 'utf8', cwd: gitRoot }
    );
  } catch (e) {
    return null; // File not in that commit
  }
}
```

#### 3. Remove Timestamp Filtering

```javascript
// BEFORE (F738):
const filteredOps = operations.filter(op => op.timestamp > commitTimestamp);

// AFTER (F740):
// Apply ALL operations - no filtering
const allOps = operations; // Use all operations
```

#### 4. Session-Aware Base Content

```javascript
// For each file, find the session that first touched it
// Use that session's start state as base, not HEAD

for (const [filePath, operations] of operationsByFile.entries()) {
  const { gitRoot, relativePath: gitPath } = findGitRoot(filePath, basePath);

  // Find earliest operation's timestamp
  const earliestOp = operations[0]; // Already sorted
  const sessionStartTime = sessionStartTimes.get(earliestOp.sessionId);

  // Get git commit at session start
  const commitAtStart = getGitStateAtTime(gitRoot, sessionStartTime);

  // Get file content at that commit (not HEAD)
  const baseContent = getFileAtCommit(gitRoot, gitPath, commitAtStart);

  // Apply ALL operations (no filtering)
  const result = replayOperations(operations, baseContent);
}
```

### Handling Write-First vs Edit-First Files

| First Operation | Base Content | Action |
|-----------------|--------------|--------|
| Write | Ignored | Use Write content as starting point |
| Edit | Git state at session start | Apply edit to that state |
| Edit (file not in git) | null | SKIP first edit, apply rest (unavoidable) |

### Data Structures

```javascript
// Track session metadata
const sessionMetadata = new Map(); // sessionId -> { startTime, startCommit }

// Track file state across sessions
const fileStates = new Map(); // filePath -> { content, lastOpTimestamp }
```

### Memory Management

For large repositories, consider:
- Stream processing of JSONL files (already implemented)
- Incremental file state updates (new)
- Garbage collect processed session data (new)

---

## Content Reduction Policy

**CRITICAL: This policy MUST be followed for ALL files.**

### Rule

If recovered file has FEWER lines than pre-incident baseline:
1. **STOP** - Do not overwrite
2. **REPORT** - Show user: file path, pre-incident lines, recovered lines, difference
3. **EXPLAIN** - Document what sections are missing
4. **ASK** - Get explicit user approval with reason
5. **RECORD** - Log the decision in Execution Log

### Implementation

```javascript
// After reconstruction, before writing:
function validateLineCount(recoveredContent, gitRoot, gitPath, baselineCommit) {
  const baselineContent = getFileAtCommit(gitRoot, gitPath, baselineCommit);
  if (!baselineContent) return { valid: true }; // New file

  const baselineLines = baselineContent.split('\n').length;
  const recoveredLines = recoveredContent.split('\n').length;

  if (recoveredLines < baselineLines) {
    return {
      valid: false,
      baselineLines,
      recoveredLines,
      difference: baselineLines - recoveredLines
    };
  }
  return { valid: true };
}
```

### Exceptions (require documentation)

| Exception | Condition | Documentation Required |
|-----------|-----------|----------------------|
| Intentional deletion | User explicitly requested removal | Execution Log entry with user approval |
| Dead code removal | Code is provably unused | Execution Log entry with proof |
| Refactoring | Same functionality, fewer lines | Execution Log entry with equivalence proof |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Session start timestamp extraction | code | Grep(index.js) | contains | "getSessionStartTimestamp" OR equivalent | [ ] |
| 2 | Git state at timestamp lookup | code | Grep(index.js) | contains | "rev-list" | [ ] |
| 3 | No timestamp filtering | code | Grep(index.js) | not_contains | "filter.*commitTimestamp" | [ ] |
| 4 | Session-aware base content | code | Grep(index.js) | contains | "getFileAtCommit" OR "show ${commitHash}" | [ ] |
| 5 | main.css recovery (6px present) | output | Grep(.tmp/recovery/src/tools/node/feature-dashboard/frontend/src/main.css) | contains | "6px" | [ ] |
| 6 | main.css line count >= 1664 | output | Bash(wc -l) | gte | 1664 | [ ] |
| 7 | Overall skip rate < 1% | exit_code | node verify-skip-rate.js overall | succeeds | - | [ ] |
| 8 | claudeService.js zero skips | exit_code | node verify-skip-rate.js claudeService.js | succeeds | - | [ ] |
| 9 | App.jsx zero skips | exit_code | node verify-skip-rate.js App.jsx | succeeds | - | [ ] |
| 10 | No regression in currently working files | build | npm test (feature-dashboard) | succeeds | - | [ ] |

### AC Details

**AC5**: Verify the chain gap fix by checking that `6px` (the final state) is present in recovered main.css
**AC6**: Verify no content loss by checking line count against pre-incident baseline (b655b810)
**AC7**: Verify overall skip rate dropped from 8.27% to <1%
**AC10**: Regression test - ensure recovery improvements don't break files that were correctly recovered before

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Implement session start timestamp extraction from JSONL | [x] |
| 2 | 2 | Implement git state lookup at timestamp (git rev-list --before) | [x] |
| 3 | 3 | Remove timestamp filtering from index.js | [x] |
| 4 | 4 | Implement session-aware base content retrieval | [x] |
| 5 | 5,6,7,8,9 | Re-run full extraction and verify recovery | [ ] |
| 6 | 10 | Run regression tests for feature-dashboard | [ ] |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F738 | [DONE] | Session extractor improvements (baseline to improve) |
| Related | F739 | [SUPERSEDED] | Chain gap resolution (approach superseded by this) |
| Successor | F736 | [PROPOSED] | Non-Dashboard Recovery (depends on this for correct extraction) |

---

## Links

- [F733](feature-733.md) - Session JSONL Extractor Tool (original)
- [F734](feature-734.md) - Dashboard Backend Recovery
- [F735](feature-735.md) - Dashboard Frontend Recovery (caused main.css deletion)
- [F736](feature-736.md) - Non-Dashboard Recovery
- [F738](feature-738.md) - Session Extractor Complete Recovery v1
- [F739](feature-739.md) - Chain Gap Resolution (superseded by this feature)

---

## Review Notes

- This feature supersedes F739's approach (chain gap reclassification only)
- Focus shifted from "reduce skip count" to "fix root cause of skips"
- Session-internal state tracking is the key insight from deep investigation
- main.css incident (530 lines deleted) is the primary validation case

---

## Mandatory Handoffs

(None identified yet)

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-03 | DRAFT | Created based on F739 investigation findings |
| 2026-02-03 | Incident | main.css restored from b655b810 (1173 to 1664 lines) |
| 2026-02-03 | UPDATED | Revised with session-internal state tracking approach from deep investigation |
| 2026-02-03 | Implementation | Tasks 1-4 completed: Added getCommitAtTime/getFileAtCommit, removed timestamp filtering, implemented session-aware base content |
| 2026-02-03 12:29 | dry-run.js update | Added git-based content retrieval to dry-run.js (copied functions from index.js, updated replayInMemory and analyzeConcurrentEdits) |
