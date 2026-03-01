# Feature 741: Session Extractor Correct Recovery Algorithm

## Status: [CANCELLED]

### Cancellation Reason

**F741 was cancelled because it accepted 8.26% skip rate as "natural limit" instead of pursuing complete recovery (0% skip rate).**

The user's goal is **complete recovery** (all Edit/Write operations applied without skips). F741's approach (restore F738 + deduplication) cannot achieve this because:

1. HEAD-based approach has inherent 8.26% ceiling
2. Complete recovery requires JSONL-based reconstruction (F740's theory)
3. F740 failed due to implementation issues, not theory issues
4. A new feature (F742) will correctly implement F740's approach with proper deduplication

**Lesson**: Do not accept partial solutions when complete solutions are theoretically possible.

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

**Complete recovery from git incident requires mathematically correct algorithm, not heuristics.**

The git incident (`git checkout -- .`) caused massive data loss. Multiple features (F733-F740) attempted recovery with varying success. This feature consolidates learnings and implements the definitively correct approach.

---

### Complete Recovery Definition

> **完全復旧 = セッションに記録された全てのEdit/Write操作がスキップなしで適用され、ファイル状態がセッション履歴の最終状態と一致すること**

Verification criteria:
- Skip rate 0% (except legitimate "is_error" operations)
- Recovered file state matches session history final state

---

### Can F741 Achieve Complete Recovery?

**NO. F741 CANNOT achieve complete recovery.**

| Approach | Skip Rate | Complete Recovery? |
|----------|:---------:|:------------------:|
| F738 (HEAD + timestamp filter) | 8.26% | ❌ NO |
| F741 (F738 + deduplication) | ~8% | ❌ NO |
| Theoretical limit with HEAD-based | ~8% | ❌ NO |

**Why F741 cannot achieve 0% skip rate:**

The HEAD-based approach has a fundamental limitation:
1. Session records Edit A (X→Y), Edit B (Y→Z), Edit C (Z→W)
2. Edit A and B are committed (HEAD now contains Z)
3. Edit C is uncommitted (lost to git incident)
4. Recovery: timestamp filter keeps only Edit C
5. Edit C expects Z, finds Z in HEAD → SUCCESS

But when:
1. Session records Edit A (X→Y), Edit B (Y→Z)
2. Edit A is committed (HEAD contains Y), Edit B is uncommitted
3. Git incident loses Edit B
4. Recovery: timestamp filter keeps only Edit B
5. Edit B expects Y, finds Y in HEAD → SUCCESS

The **unavoidable 8.26%** are cases like:
1. Session records Edit A (X→Y), Edit B (Y→Z), Edit C (Z→W)
2. Edits A, B, C all committed at different times
3. Git incident + checkout reverts to an intermediate state
4. Some operations' old_string no longer exists in HEAD

**To achieve complete recovery (0%), a different approach is needed:**
- Pure JSONL-based reconstruction (ignore git entirely)
- Start from earliest known state, apply ALL operations
- Handle duplicates via toolUseId
- This is essentially what F740 attempted but failed due to implementation issues

### Problem (Current Issue)

**Previous approaches have fundamental design flaws:**

| Feature | Approach | Skip Rate | Flaw |
|---------|----------|:---------:|------|
| F738 | HEAD + timestamp filter | 8.26% | Natural chain gap limit (NOT a design flaw) |
| F739 | F738 + chain reclassification | 8.26% | Only relabels skips, no content recovery |
| F740 | session-start content + no filter | 21.74% | Duplicate ops + applying already-committed ops |

---

### Historical Failure Log (F733-F740)

#### F733: Session JSONL Extractor Tool [DONE]
- **Date**: 2026-01-31
- **Approach**: Basic JSONL parsing, Edit/Write extraction
- **Result**: Foundation tool created, but high skip rate (~85%)
- **Learning**: Need per-operation timestamps, nested git handling

#### F734: Dashboard Backend Recovery [DONE]
- **Date**: 2026-02-01
- **Approach**: Used F733 for backend files
- **Result**: 132/132 tests pass
- **Learning**: Tests passing doesn't guarantee complete content recovery

#### F735: Dashboard Frontend Recovery [DONE]
- **Date**: 2026-02-01
- **Approach**: Used F733 for frontend files
- **Result**: 224/224 tests pass, BUT main.css lost 530 lines
- **Learning**: Line count validation needed; tests don't cover CSS completeness
- **Incident**: main.css went from 1664 to 1173 lines (intentional "state refactoring" that deleted content)

#### F738: Session Extractor Complete Recovery v1 [DONE]
- **Date**: 2026-02-02
- **Approach**: 9 improvements including per-record timestamps, nested git detection, timestamp filtering
- **Result**: Skip rate 85% → 8.26% (93 skips / 1126 ops)
- **Learning**: Timestamp filter prevents re-applying committed operations
- **Key files with skips**: F570ProfilingTests.cs (10), F629DebugTests.cs (7), useWebSocket.js (7)

#### F739: Chain Gap Resolution [SUPERSEDED]
- **Date**: 2026-02-02
- **Approach**: Detect when old_string matches committed new_string, reclassify as "already committed"
- **Result**: Never implemented - SUPERSEDED by F740
- **Learning**: Reclassification changes warning messages but does NOT recover content
- **Verdict**: Correctly marked [SUPERSEDED]

#### F740: Session-Internal State Tracking [DRAFT]
- **Date**: 2026-02-03
- **Approach**: Use git state at first operation time (not HEAD), remove timestamp filter, apply ALL operations
- **Result**: Skip rate REGRESSED to 21.74% (4013 skips / 18477 ops)
- **Root Cause Analysis**:
  1. **Operation duplication**: Same operation appears in both subagent JSONL AND parent's progress messages
  2. **No deduplication**: Applying duplicate operations causes chain breaks
  3. **index-features.md**: 7 ops (F738) → 1426 ops (F740) = massive inflation from duplicates
- **Learning**: Timestamp filter was implicitly deduplicating; removing it exposed duplicates

---

### Analysis Flip-Flop History

| Date/Time | Conclusion | Reason | Validity |
|-----------|------------|--------|:--------:|
| 2026-02-03 AM | "F739 is correct" | User summary stated this | ❌ Wrong |
| 2026-02-03 Mid | "F740 is mathematically correct" | Theory looked sound | ⚠️ Partial |
| 2026-02-03 | "F740 failed, revert to F738" | 21.74% skip rate measured | ✅ Correct |
| 2026-02-03 | "F740 theory correct, impl buggy" | Deep analysis | ⚠️ Partial |
| 2026-02-03 | "Deduplication will fix it" | Identified duplicate ops | ⚠️ Partial |
| 2026-02-03 Late | "8.26% is natural limit" | Final deep review | ✅ Correct |

---

### Deep Explorer Final Analysis (2026-02-03)

**Key Findings:**

1. **F738's 8.26% is NOT a design flaw** - It's the natural chain gap rate for operations whose old_string genuinely doesn't exist in HEAD

2. **F740's 21.74% has TWO causes**:
   - Duplicate operations from subagent JSONL + parent progress messages
   - Applying already-committed operations that fail when content changed

3. **Deduplication alone is INSUFFICIENT** for <1% skip rate

4. **Better deduplication key**: Use `toolUseId` instead of (filePath|oldString|newString|timestamp)
   ```javascript
   // Each tool_use has unique ID - use this for deduplication
   const seen = new Set();
   const deduplicatedOps = allOperations.filter(op => {
     if (op.toolUseId && seen.has(op.toolUseId)) return false;
     if (op.toolUseId) seen.add(op.toolUseId);
     return true;
   });
   ```

5. **Recommended approach**: Restore F738's algorithm + add toolUseId deduplication

### Goal (What to Achieve)

Implement the **definitively correct** recovery algorithm:

1. **Restore F738's proven approach** (HEAD + timestamp filter)
2. **Add toolUseId-based deduplication** to handle edge cases
3. **Fix OUTPUT_DIR bug** (relative path causing wrong output location)
4. **Accept ~8% as practical limit** for automatic recovery
5. **Document remaining gaps** for manual intervention if needed

**Target**: Skip rate ~8% (F738 baseline) with cleaner implementation

---

## Technical Analysis

### Why F738's 8.26% is the Natural Limit (NOT a Design Flaw)

F738 algorithm:
```
base = git show HEAD:file
filter = ops where timestamp > lastCommitTimestamp
apply filtered ops to base
```

**This is CORRECT because:**
1. Operations before commit timestamp are already reflected in HEAD
2. Only uncommitted operations need to be recovered
3. The 8.26% represents operations whose `old_string` genuinely doesn't exist in HEAD (natural chain gaps)

**Chain gap scenario (unavoidable):**
```
T1: Edit A (X→Y) - applied and committed
T2: Edit B (Y→Z) - applied and committed
T3: Edit C (Z→W) - UNCOMMITTED (lost to git checkout)

Recovery:
- HEAD contains Z (from T2 commit)
- Filter: only Edit C passes (T3 > T2)
- Apply Edit C (Z→W): SUCCESS

The 8.26% are cases where intermediate state was lost.
```

### Why F739 Cannot Improve Recovery (CONFIRMED)

F739 only reclassifies warnings from "chain gap" to "already committed". **No content is recovered.**

### Why F740 Failed (21.74%) - Root Cause Confirmed

**Two independent problems:**

1. **Duplicate operations**: jsonl-parser extracts from:
   - Subagent JSONL files directly
   - Parent's `type: progress` messages containing subagent operations
   - Same operation appears twice with same `toolUseId`

2. **Applying committed operations**: F740 removed timestamp filter, causing:
   - Already-committed operations to be re-applied
   - Chain breaks when content has evolved

**Evidence**: index-features.md went from 7 ops (F738) to 1426 ops (F740)

### Correct Algorithm (F738 + Deduplication)

```
1. Parse all operations from all sessions
2. DEDUPLICATE by toolUseId (unique per operation)
3. Sort by timestamp globally
4. Group by file
5. For each file:
   a. Get base content from HEAD
   b. Get commit timestamp for file
   c. Filter: ops where timestamp > commitTimestamp
   d. Apply filtered ops in order
   e. Write result
```

**Key insight**: F738 was right. Add toolUseId deduplication as safety measure.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | F738 algorithm restored (HEAD + timestamp filter) | code | Grep(index.js) | contains | "timestamp > commitTimestamp" OR equivalent filter | [ ] |
| 2 | toolUseId deduplication added | code | Grep(index.js) | contains | "toolUseId" | [ ] |
| 3 | OUTPUT_DIR uses absolute path | code | Grep(index.js) | contains | "detectRepoRoot" in OUTPUT_DIR | [ ] |
| 4 | Skip rate <= 10% | output | summary.json calculation | lte | 10% | [ ] |
| 5 | Skip rate regression from F740 fixed | output | summary.json | lt | 21.74% | [ ] |
| 6 | main.css line count >= 1664 | output | wc -l (repo root recovery) | gte | 1664 | [ ] |
| 7 | Dashboard tests pass | exit_code | npm test (feature-dashboard) | succeeds | - | [ ] |

### AC Details

**AC1**: Restore F738's proven timestamp filter approach
**AC2**: Add toolUseId-based deduplication as safety measure
**AC3**: Fix OUTPUT_DIR bug so recovery files go to repo root .tmp/recovery/
**AC4-5**: Verify F740's regression (21.74%) is fixed, target ~8% (F738 baseline)
**AC6**: Verify main.css content recovery
**AC7**: Ensure no regression in working functionality

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Revert F740's changes: restore timestamp filter, use HEAD as base | [ ] |
| 2 | 2 | Add toolUseId-based deduplication | [ ] |
| 3 | 3 | Fix OUTPUT_DIR to use absolute path | [ ] |
| 4 | 4,5 | Run extraction and verify skip rate <= 10% | [ ] |
| 5 | 6 | Verify main.css line count >= 1664 | [ ] |
| 6 | 7 | Run dashboard regression tests | [ ] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Task 1: Restore F738 Algorithm

Revert the F740 changes in index.js:

**REMOVE** F740's session-start base content logic:
```javascript
// REMOVE these F740 additions:
// const earliestOpTimestamp = operations[0].timestamp;
// const commitAtStart = getCommitAtTime(gitRoot, earliestOpTimestamp);
// baseContent = getFileAtCommit(gitRoot, gitPath, commitAtStart);
```

**RESTORE** F738's HEAD-based approach:
```javascript
// Get last commit timestamp for this file
let commitTimestamp = 0;
try {
  const result = execSync(`git -c core.quotepath=false log -1 --format=%cI -- "${gitPath}"`, {
    encoding: 'utf8',
    cwd: gitRoot
  });
  if (result.trim()) {
    commitTimestamp = new Date(result.trim()).getTime();
  }
} catch (e) {
  // Untracked file - commitTimestamp remains 0
}

// Filter operations after commit
const filteredOps = operations.filter(op => op.timestamp > commitTimestamp);
if (filteredOps.length === 0) {
  continue; // All operations already committed
}

// Get HEAD content as base
let baseContent = null;
try {
  baseContent = execSync(`git -c core.quotepath=false show HEAD:"${gitPath}"`, {
    encoding: 'utf8',
    cwd: gitRoot
  });
} catch (e) {
  // New file (not in git)
}

const result = replayOperations(filteredOps, baseContent);
```

### Task 2: Add toolUseId Deduplication

Add after global sort (around line 230):

```javascript
// Step 2.5: Deduplicate by toolUseId
const seenToolUseIds = new Set();
const beforeCount = allOperations.length;
allOperations = allOperations.filter(op => {
  if (op.toolUseId) {
    if (seenToolUseIds.has(op.toolUseId)) {
      return false; // Duplicate
    }
    seenToolUseIds.add(op.toolUseId);
  }
  return true;
});
console.log(`Deduplicated: ${beforeCount} -> ${allOperations.length} operations (removed ${beforeCount - allOperations.length} duplicates)`);
```

### Task 3: Fix OUTPUT_DIR

Change line 34 from:
```javascript
const OUTPUT_DIR = '.tmp/recovery';
```

To:
```javascript
const OUTPUT_DIR = path.join(detectRepoRoot(), '.tmp', 'recovery');
```

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F733 | [DONE] | Original session extractor |
| Related | F738 | [DONE] | Skip rate improvements (baseline) |
| Related | F739 | [SUPERSEDED] | Chain gap reclassification (approach rejected) |
| Related | F740 | [DRAFT] | Session-internal state tracking (theory correct, implementation buggy) |
| Successor | F736 | [BLOCKED] | Non-Dashboard Recovery (depends on <1% skip rate) |

---

## Links

- [F733](feature-733.md) - Session JSONL Extractor Tool (original)
- [F738](feature-738.md) - Session Extractor Complete Recovery v1
- [F739](feature-739.md) - Chain Gap Resolution (superseded)
- [F740](feature-740.md) - Session-Internal State Tracking (theory source)
- [F736](feature-736.md) - Non-Dashboard Recovery (successor)

---

## Review Notes

**Final Conclusions (2026-02-03 Deep Review):**

1. **F738's approach was correct** - HEAD + timestamp filter is the right algorithm
2. **F739 was correctly [SUPERSEDED]** - Reclassification cannot improve recovery
3. **F740's theory was WRONG** - Applying all operations from old base causes regression
4. **8.26% is the natural limit** - Represents genuine chain gaps, not a design flaw
5. **This feature (F741)**: Restore F738 + add toolUseId deduplication + fix OUTPUT_DIR

**Why previous conclusions flip-flopped:**
- Incomplete analysis of operation duplication (subagent + progress messages)
- Confusion between "theory sounds correct" and "implementation works"
- Lack of rigorous testing before declaring approaches correct/incorrect

**Lesson learned:**
- Always verify with actual extraction run before changing approaches
- Skip rate must be measured, not assumed from theory

---

## Mandatory Handoffs

(None identified yet)

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-03 | DRAFT | Created based on comprehensive analysis of F733-F740 |
