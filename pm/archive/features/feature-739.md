# Feature 739: Session Extractor Chain Gap Resolution

## Status: [CANCELLED]

### Cancellation Reason

完全復旧断念。手動復旧を開始したため。

## Type: infra

## Background

### Philosophy (Mid-term Vision)

**完全復元が唯一の正解。chain gapは設計限界ではなく、解決すべき課題。**

F738でskip率を85%→8.26%に改善したが、残りの93 skipsは「chain gap」（編集操作のold_stringが見つからない）によるもの。これらは「中間状態がコミット済み」の操作であり、タイムスタンプフィルタでは捕捉できない依存関係を持つ。

### Problem (Current Issue)

session-extractor (F738完了後) には以下の問題が残る:

1. **Chain gap**: 編集Aで X→Y、編集Bで Y→Z の場合、Aがコミット済みでもBはYを探すが、HEADにはZがある
2. **Skip率8.26%**: 93/1126操作がスキップされている
3. **重要ファイルへの影響**: App.jsx (3 skips)、server.js (6 skips) など

結果: F735/F736の完全復元が不可能

### Goal (What to Achieve)

session-extractorを拡張し、**chain gapを解決**する:

1. 編集チェーンの依存関係を分析
2. コミット済み編集の「結果」を追跡
3. 依存する編集のold_stringを更新

Target: skip率 8.26% → <1%

### Delegated Obligations (F738から委譲)

**ツール実行義務**: F739完了後、以下を実行すること：
1. `node tools/session-extractor/index.js "C:/Users/siihe/.ccs/shared/projects/C--Era-erakoumakanNTR"` でF735/F736用のrecoveryを再生成
2. `.tmp/recovery/` の内容でF735/F736のファイル復元を実行
3. skip率検証: `node tools/session-extractor/verify-skip-rate.js overall` で<1%を確認

## Links

- [F738](feature-738.md) - Session Extractor Complete Recovery (Predecessor)
- [F735](feature-735.md) - Dashboard Frontend Recovery (Consumer)
- [F736](feature-736.md) - Non-Dashboard Recovery (Consumer)

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F738 | [DONE] | Session extractor baseline improvements (skip率85%→8.26%) |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Node.js built-ins | Runtime | None | readline, fs, path (already used in F733/F738) |
| Git CLI | Runtime | None | Already used for git show/log commands |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| F735 | HIGH | Dashboard Frontend Recovery - requires <1% skip rate |
| F736 | HIGH | Non-Dashboard Recovery - requires <1% skip rate |

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: 93 edit operations (8.26%) are skipped during recovery replay
2. Why: edit-replayer.js cannot find old_string in current file content
3. Why: The old_string refers to intermediate state that was committed and then edited again
4. Why: Current replay uses HEAD content as base, but some operations were recorded before intermediate commits
5. Why: **edit-replayer.js has no chain dependency tracking** - it treats each operation independently without analyzing how operations relate to each other through their old_string/new_string pairs

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 93 operations skipped (8.26% skip rate) | No edit chain dependency analysis in edit-replayer.js |
| old_string not found in current content | Operations are filtered by timestamp but not by content dependency |
| Important files affected (App.jsx, server.js) | Chain gaps occur in actively edited files with multiple commits during session |

### Conclusion

The root cause is the **lack of edit chain dependency tracking**. The current implementation:
1. Gets HEAD content as base
2. Filters operations by `timestamp > commitTimestamp`
3. Applies operations sequentially

This works for simple cases but fails when:
- Edit A changes X→Y (recorded at T1)
- Commit happens (Y in HEAD)
- Edit B changes Y→Z (recorded at T3)
- Another commit happens (Z in HEAD)
- Edit C changes Z→W (recorded at T5)

At recovery time: HEAD=W, but Edit B expects Y, Edit C expects Z. Both fail.

**Solution direction**: Track the edit chain by analyzing old_string/new_string relationships. If an operation's old_string matches a previous operation's new_string, and that previous operation was already committed (skipped by timestamp filter), the current operation should also be skipped (it's already applied).

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F733 | [DONE] | Predecessor | Original session extractor - established architecture |
| F738 | [DONE] | Predecessor | Skip rate 85%→8.26%, identified chain gap problem |
| F735 | [DONE] | Consumer | Dashboard Frontend Recovery - 再生成で品質向上 |
| F736 | [BLOCKED] | Consumer | Non-Dashboard Recovery - blocked until <1% skip rate |

### Pattern Analysis

This is the first occurrence of chain gap resolution. The problem was not anticipated in F733's original design, which assumed linear replay would work. F738 reduced skip rate significantly by adding timestamp filtering and nested git detection, but revealed that chain gaps require fundamentally different handling.

**Why chain gaps exist**: Claude Code sessions span multiple commits. When a file is edited, committed, edited again, and committed again, the JSONL records intermediate states that no longer exist in HEAD. The current timestamp filter catches "operations before last commit" but cannot detect "operations whose results were already committed in a subsequent commit."

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All information needed for chain detection exists in JSONL (old_string, new_string, timestamp). Algorithm: compare old_string with previous operations' new_string to detect chains. |
| Scope is realistic | YES | Single file modification (edit-replayer.js), algorithm is deterministic string matching |
| No blocking constraints | YES | No external dependencies needed, existing test infrastructure available |

**Verdict**: FEASIBLE

**Algorithm sketch**:
1. Build a map of committed edits: `{new_string → operation}` for operations filtered out by timestamp
2. For each operation to apply, check if its old_string matches any committed edit's new_string
3. If match found, the current operation's effect is already in HEAD (skip it)
4. This handles transitive chains: if A→B→C are all committed, operations B and C are both skipped

**Edge cases**:
- Multiple operations with same new_string: Track by file path + position
- Partial string matches: Require exact match (same as current Edit semantics)
- replace_all operations: Each occurrence is an independent chain

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/session-extractor/edit-replayer.js | Update | Add chain gap detection logic |
| tools/session-extractor/index.js | Update | Pass committed operations info to replayer |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Exact string matching | Edit tool semantics | MEDIUM - Chain detection must use exact match, not fuzzy |
| Operation order matters | Chronological replay | LOW - Already sorted by timestamp |
| Memory for chain map | Large sessions | LOW - Only stores committed operations, not full content |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Chain detection misses edge cases | Medium | Medium | Comprehensive test fixtures for various chain patterns |
| Performance impact on large sessions | Low | Low | O(n) pass to build chain map, O(1) lookup per operation |
| False positive chain detection | Low | High | Require exact old_string match + same file path |
| replace_all complicates chain tracking | Medium | Medium | Track each occurrence separately or skip chain detection for replace_all |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The chain gap problem requires tracking committed operations to detect when an edit operation has already been applied. The solution extends the existing replay pipeline with a two-pass chain detection algorithm:

#### Algorithm Overview

**Pass 1: Build Committed Edit Chain Map**
- Input: All operations filtered OUT by timestamp filter (operations with `timestamp <= commitTimestamp`)
- Process: For each edit operation, store a mapping from its `old_string` to its `new_string` (keyed by file path)
- Output: Chain map structure: `{ filePath → [{ oldString, newString, timestamp }] }`

**Pass 2: Replay with Chain Detection**
- Input: All operations filtered IN by timestamp filter (operations with `timestamp > commitTimestamp`)
- For each edit operation:
  1. **First**: Check if `old_string` exists in current content
  2. If exists → Apply operation (standard replay) - **no chain detection needed**
  3. If NOT exists → Check chain map for `old_string` matching any committed `new_string`
     - If match found: Skip as "chain gap (already committed)" - the operation's result is already in HEAD
     - If no match: Skip as "old_string not found (chain gap)" - standard chain gap error
- **Critical**: Chain detection is only triggered when `old_string` is NOT found in content. This prevents false positives where a valid operation would be incorrectly skipped.

#### Implementation Strategy

**File: tools/session-extractor/edit-replayer.js**
- Add new function: `buildCommittedChainMap(operations)` → returns chain map
- Modify `replayOperations(operations, baseContent, committedOperations)`:
  - Add `committedOperations` parameter
  - Build chain map from committedOperations at start
  - **Modified logic flow** (critical fix):
    1. First check if `old_string` exists in content (existing logic)
    2. If found → apply operation (no change)
    3. If NOT found → check chain map BEFORE emitting "old_string not found" warning
    4. If chain match → emit `"Edit operation skipped: chain gap (already committed)"`
    5. If no chain match → emit existing `"Edit operation skipped: old_string not found (chain gap)"`
- Keep existing logic unchanged for write operations

**File: tools/session-extractor/index.js**
- Split operation filtering into two groups:
  - `committedOps`: operations with `timestamp <= commitTimestamp`
  - `filteredOps`: operations with `timestamp > commitTimestamp` (existing)
- Pass `committedOps` to `replayOperations(filteredOps, baseContent, committedOps)`

#### Edge Cases Handled

1. **replace_all operations**: Each occurrence tracked separately with position offsets
2. **Multiple edits with same old_string**: Chain map stores all matches as array
3. **Partial chains**: If A→B is committed but B→C is not, C still applied normally
4. **Empty old_string**: Already handled by existing validation (skip with warning)
5. **New files (baseContent = null)**: Chain detection not applicable (no HEAD to compare)

#### Algorithm Correctness

**Claim**: This algorithm resolves chain gaps without false positives.

**Proof sketch**:
1. For any edit operation, first check if `old_string` exists in current content
2. If `old_string` EXISTS in content → Apply operation. No chain detection needed (content is in expected state)
3. If `old_string` NOT FOUND in content → Check chain map:
   - If `old_string` matches a committed `new_string`: The intermediate state was committed and later changed. Skip as "already committed"
   - If no match: Standard "old_string not found" error

**Why this prevents false positives**:
- Consider Edit C (Z→W) where HEAD=Z and chain map has Y→Z
- Step 1: Check if "Z" exists in content → YES (HEAD=Z)
- Step 2: Apply Edit C → content becomes W ✓
- Chain map is NOT consulted because `old_string` was found

**Why this resolves chain gaps**:
- Consider Edit D (Y→Q) where HEAD=Z and chain map has X→Y
- Step 1: Check if "Y" exists in content → NO (HEAD=Z)
- Step 3: Check chain map → "Y" matches committed new_string from Edit A
- Skip Edit D as "chain gap (already committed)" ✓

**Skip rate calculation**:
- Baseline: 93 skips / 1126 operations = 8.26%
- Expected chain gaps: ~80 operations (based on F738 analysis)
- Target: <12 skips / 1126 operations = <1%

### AC Coverage Matrix

| AC# | Satisfied By | Design Element |
|:---:|--------------|----------------|
| 1 | Chain detection algorithm | Two-pass algorithm reduces skip rate from 8.26% to <1% |
| 2 | Chain detection algorithm | Resolves ~80 chain gap skips, bringing total from 93 to <12 |
| 3 | Code implementation | `buildCommittedChainMap()` function and chain detection logic |
| 4 | Code implementation | Chain map uses `{ oldString, newString }` structure for A→B→C chains |
| 5 | Code implementation | `committedOps` parameter passed from index.js, chain map built in replayOperations |
| 6 | Algorithm correctness | App.jsx (3 skips), server.js (6 skips) chain gaps resolved by chain detection |
| 7 | No breaking changes | Algorithm extends existing logic, write operations unchanged |
| 8 | Conservative skip logic | Chain detection only adds skips for confirmed matches, no false positives |
| 9 | Implementation quality | JavaScript syntax validation via node --check |
| 10 | Post-implementation task | Run extraction command after code changes complete |

### Key Design Decisions

#### Decision 1: Two-Pass vs Single-Pass

**Chosen**: Two-pass algorithm (build chain map → replay with detection)

**Rationale**:
- Single-pass would require lookahead to detect chains, complicating chronological replay
- Two-pass separates concerns: Pass 1 = chain analysis, Pass 2 = replay
- Memory overhead is O(n) for committed operations, acceptable for typical sessions

**Trade-off**: Additional memory for chain map vs. code complexity

#### Decision 2: Exact Match vs Fuzzy Match

**Chosen**: Exact string match for chain detection

**Rationale**:
- Consistent with Edit tool semantics (exact match only)
- Avoids false positives from fuzzy matching
- Chain gaps are deterministic (exact old_string/new_string pairs)

**Trade-off**: Cannot handle renamed variables/refactoring that change old_string, but this is out of scope (not a chain gap, different problem)

#### Decision 3: Chain Map Structure

**Chosen**: `Map<filePath, Array<{oldString, newString, timestamp}>>`

**Rationale**:
- Keyed by file path to avoid cross-file false matches
- Array handles multiple edits with same old_string (rare but possible)
- Timestamp included for debugging/logging
- O(1) file path lookup, O(n) linear search within file (n is small per file)

**Alternative considered**: Hash-based map by `oldString` for O(1) lookup
**Rejected**: Would require composite key `(filePath, oldString)` and complicate replace_all handling

#### Decision 4: Skip Committed Chains Entirely

**Chosen**: Skip operations that match committed chains, do not attempt application

**Rationale**:
- If old_string matches a committed new_string, the operation result is already in HEAD
- Attempting application would fail (old_string not found) and create warning noise
- Preemptive skip based on chain analysis is semantically correct

**Alternative considered**: Attempt application anyway, let natural old_string check fail
**Rejected**: Creates confusing warnings ("old_string not found" vs "chain gap resolved")

#### Decision 5: Transitive Chain Handling

**Chosen**: Implicit transitive chain resolution through sequential replay

**Rationale**:
- If A→B→C are all committed, chain map has both A→B and B→C
- Operation C checks if its old_string (B) matches any committed new_string → match found → skip
- Transitive chains (A→B→C→D) resolved automatically by same logic

**Implementation**: No special transitive detection needed, single-level chain check suffices

#### Decision 6: Integration Point

**Chosen**: Modify `replayOperations()` signature to accept `committedOperations`

**Rationale**:
- Minimal API change (add one parameter with default = [])
- Keeps chain logic encapsulated in edit-replayer.js
- index.js remains thin coordinator, only responsible for splitting committed/filtered ops

**Alternative considered**: Build chain map in index.js and pass to replayOperations
**Rejected**: Violates separation of concerns (index.js should not know replay internals)

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "完全復元が唯一の正解" (Complete recovery is the only correct answer) | Skip rate must reach near-zero (<1%) | AC#1, AC#2 |
| "chain gapは設計限界ではなく、解決すべき課題" (Chain gap is not a design limitation but a problem to solve) | Chain gap detection algorithm must be implemented | AC#3, AC#4, AC#5 |
| "F735/F736の完全復元が不可能" (Current state cannot fully recover F735/F736) | Recovery must work on dashboard files (App.jsx, server.js) | AC#6 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Skip rate <1% overall | exit_code | Bash | succeeds | node verify-skip-rate.js overall exits 0 | [ ] |
| 2 | Improved skip count (<12 skips from 93) | output | Bash | lt | 12 | [ ] |
| 3 | Chain detection logic exists | code | Grep(tools/session-extractor/edit-replayer.js) | contains | "chain" | [ ] |
| 4 | Simple chain A->B->C handled | code | Grep(tools/session-extractor/edit-replayer.js) | matches | "old_?[Ss]tring.*new_?[Ss]tring" | [ ] |
| 5 | Committed operations tracked | code | Grep(tools/session-extractor/edit-replayer.js) | contains | "committed" | [ ] |
| 6 | Dashboard files recoverable | output | Bash | not_contains | "App.jsx" and "server.js" in skip warnings | [ ] |
| 7 | All tests pass (including new chain detection tests) | exit_code | Bash | succeeds | npm test in session-extractor | [ ] |
| 8 | No regression in applied operations | output | Bash | gte | 1033 applied ops (current baseline) | [ ] |
| 9 | Build succeeds (no syntax errors) | exit_code | Bash | succeeds | node --check edit-replayer.js | [ ] |
| 10 | Delegated obligation executed | file | Glob(.tmp/recovery/**/*) | exists | Recovery files generated | [ ] |

**Note**: 10 ACs within infra range (8-15). AC#1-2 verify the core success metric (skip rate). AC#3-5 verify implementation approach. AC#6 verifies real-world impact. AC#7-9 verify no regression. AC#10 verifies delegated obligation.

### AC Details

**AC#1: Skip rate <1% overall**
- Purpose: Primary success metric from Philosophy - "完全復元が唯一の正解"
- Method: `node tools/session-extractor/verify-skip-rate.js overall`
- Exit code 0 means skip rate < 1% threshold met
- This is the ultimate pass/fail criterion for the feature

**AC#2: Improved skip count (<12 skips from 93)**
- Purpose: Quantitative improvement verification
- Baseline: 93 skips (8.26% of 1126 operations)
- Target: <12 skips (<1% of 1126 operations)
- Method: Parse output from verify-skip-rate.js for skip count

**AC#3: Chain detection logic exists**
- Purpose: Verify the core algorithm is implemented
- Method: Grep for "chain" keyword in edit-replayer.js
- Indicates chain gap detection is present (not just comments)

**AC#4: Simple chain A->B->C handled**
- Purpose: Verify chain dependency tracking via old_string/new_string relationship
- Method: Regex match for code that compares old_string with new_string
- Pattern handles both camelCase (oldString/newString) and snake_case (old_string/new_string)

**AC#5: Committed operations tracked**
- Purpose: Verify committed operations are stored for chain lookup
- Method: Grep for "committed" keyword indicating committed operations map
- Algorithm requirement: Build map of committed edits for chain detection

**AC#6: Dashboard files recoverable**
- Purpose: Real-world impact verification on important files
- Files affected by chain gaps: App.jsx (3 skips), server.js (6 skips)
- Method: Run extraction and verify neither App.jsx nor server.js appear in skip warnings
- Verifies F735/F736 can proceed after this fix

**AC#7: All tests pass (including new chain detection tests)**
- Purpose: No regression + new chain detection logic verified
- Method: `npm test` in tools/session-extractor directory
- Prerequisite: T7 creates test file and adds test script to package.json
- Tests: Simple chain (A→B), transitive chain (A→B→C), no chain match, backward compatibility (empty committedOps)

**AC#8: No regression in applied operations**
- Purpose: Chain detection should not reduce valid operations
- Baseline: 1033 applied operations (1126 - 93 skipped)
- Verify applied operations count >= 1033 after fix
- Method: Parse output from verify-skip-rate.js for applied count

**AC#9: Build succeeds (no syntax errors)**
- Purpose: Basic syntax validation
- Method: `node --check tools/session-extractor/edit-replayer.js`
- Ensures no syntax errors introduced during implementation

**AC#10: Delegated obligation executed**
- Purpose: Fulfill F738's delegated obligation
- Method: Verify .tmp/recovery/ contains generated recovery files
- Validates that recovery extraction was run post-implementation

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| T1 | 3,4,5 | Implement buildCommittedChainMap() function in edit-replayer.js | [ ] |
| T2 | 3,4,5 | Modify replayOperations() to accept committedOperations parameter and build chain map | [ ] |
| T3 | 3,4,5 | Add chain detection logic before edit application (check old_string against committed new_strings) | [ ] |
| T4 | 3,5 | Add warning message for chain gap skips: "Edit operation skipped: chain gap (already committed)" | [ ] |
| T5 | 5 | Modify index.js to split operations into committedOps and filteredOps groups | [ ] |
| T6 | 5 | Pass committedOps to replayOperations() call in index.js | [ ] |
| T7 | 7 | Create unit tests for chain detection: simple chain (A→B), transitive chain (A→B→C), no chain match, and add test script to package.json | [ ] |
| T8 | 9 | Run node --check on all modified JS files to validate syntax | [ ] |
| T9 | 7 | Run npm test in tools/session-extractor to verify all tests pass | [ ] |
| T10 | 1,2,6,8,10 | Run session extractor: node tools/session-extractor/index.js and verify skip rate <1% | [ ] |

## Implementation Contract

### Input Constraints

1. **committedOperations parameter**:
   - Type: Array of operation objects
   - Schema: Same as existing operations (must have `type`, `file_path`, `old_string`, `new_string` for edits)
   - Source: Filtered from full operation list where `timestamp <= commitTimestamp`
   - Default: `[]` (empty array for backward compatibility)

2. **Chain map key structure**:
   - Key: `file_path` (absolute path string)
   - Value: Array of `{ oldString, newString, timestamp }` objects
   - Rationale: File-scoped to avoid cross-file false matches

3. **Exact string matching**:
   - Chain detection uses `===` equality for old_string/new_string comparison
   - Consistent with Edit tool semantics (no fuzzy matching)
   - Case-sensitive, whitespace-sensitive

### Output Guarantees

1. **Skip rate reduction**:
   - Guarantee: Skip rate will decrease from 8.26% to <1%
   - Mechanism: Chain gap operations (estimated ~80) will be preemptively skipped
   - Evidence: AC#1 exit code validation, AC#2 quantitative skip count

2. **No false positives**:
   - Guarantee: Chain detection will not skip operations that should be applied
   - Mechanism: Exact match requirement + file path scoping
   - Validation: AC#8 verifies applied operations count >= 1033 (no regression)

3. **Transitive chain resolution**:
   - Guarantee: Multi-level chains (A→B→C→D) are resolved automatically
   - Mechanism: Sequential replay checks each operation's old_string against all committed new_strings
   - Example: If A→B→C are committed, operation D with old_string=C will be skipped

4. **Backward compatibility**:
   - Guarantee: If committedOperations=[] (not provided), behavior is unchanged
   - Mechanism: Empty chain map results in no chain matches → standard replay logic
   - Validation: AC#7 ensures existing tests pass

### Algorithm Invariants

1. **Chain map completeness**:
   - Invariant: All edit operations with `timestamp <= commitTimestamp` are in chain map
   - Maintained by: index.js splitting logic + buildCommittedChainMap() processing

2. **Operation order preservation**:
   - Invariant: Operations are replayed in chronological order (timestamp ascending)
   - Maintained by: Existing sort logic in index.js (unchanged)

3. **File path isolation**:
   - Invariant: Chain detection only compares operations within same file
   - Maintained by: Chain map keyed by file_path, lookup scoped to current operation's file

4. **Non-destructive skip logic**:
   - Invariant: Skipping an operation due to chain gap does not affect subsequent operations
   - Maintained by: Skip increments warning count but does not modify baseContent

### Edge Case Handling

1. **replace_all operations**:
   - Behavior: Each occurrence tracked separately in chain map
   - Implementation: Store array of `{ oldString, newString, position }` for multi-occurrence edits
   - Note: Current implementation may defer this complexity (track first occurrence only)

2. **Multiple edits with same old_string**:
   - Behavior: Chain map stores all matches as array, check against all
   - Implementation: Linear search through array for file path key

3. **Empty old_string**:
   - Behavior: Already handled by existing validation (skip with warning)
   - Chain detection: Not applicable (no old_string to match)

4. **New files (baseContent = null)**:
   - Behavior: Chain detection skipped for write operations
   - Rationale: No HEAD content to compare against

5. **Partial chains**:
   - Behavior: If A→B is committed but B→C is not, C is applied normally
   - Implementation: No chain match found for C's old_string → standard replay

### Error Handling

1. **Malformed committedOperations**:
   - Condition: Array contains non-object elements or missing required fields
   - Response: Skip malformed entries during chain map building, log warning
   - Fallback: Continue with partial chain map

2. **Chain map lookup failure**:
   - Condition: file_path not found in chain map
   - Response: No chain match → proceed with standard replay logic
   - Rationale: Absence of committed operations for a file is valid state

3. **Syntax errors**:
   - Detection: AC#9 node --check validation
   - Prevention: Incremental testing during implementation
   - Escalation: If syntax errors persist after 3 attempts, STOP and report

### Performance Characteristics

1. **Time complexity**:
   - Chain map building: O(n) where n = number of committed operations
   - Chain detection per operation: O(m) where m = number of committed edits for that file (typically small)
   - Overall: O(n + k*m) where k = operations to replay (acceptable)

2. **Space complexity**:
   - Chain map size: O(n) where n = number of committed operations
   - Typical session: ~100-500 committed operations → ~50-250 KB memory
   - Acceptable for Node.js runtime

3. **No disk I/O overhead**:
   - Chain map built in memory from already-loaded operations
   - No additional git commands or file reads

### Testing Strategy

1. **Unit test coverage** (AC#7):
   - Existing tests must pass (no regression)
   - New tests: Simple chain (A→B), transitive chain (A→B→C), no chain match

2. **Integration test** (AC#1, AC#2):
   - Real session replay with verify-skip-rate.js
   - Validates end-to-end skip rate reduction

3. **Specific file validation** (AC#6):
   - Dashboard files (App.jsx, server.js) must not appear in skip warnings
   - Real-world impact verification

### Rollback Plan

If implementation fails after 3 attempts:
1. Revert edit-replayer.js and index.js changes
2. Keep committedOperations parameter with default=[] for future retry
3. Document failure reason in feature file
4. Escalate to user for guidance

---

## Review Notes

- F738から分離した残課題
- chain gapは「設計限界」ではなく「未解決課題」として扱う
- F735/F736の完全復元にはこのFeatureの完了が必要

---

## Reference (from previous session)

### Root Cause Analysis (reference)

Original chain gap mechanism description from DRAFT creation:

```
Timeline:
  T1: Edit A (X → Y) - 操作記録
  T2: Commit - Yがgit HEADに
  T3: Edit B (Y → Z) - 操作記録

Recovery attempt:
  1. git show HEAD → 内容はZ (T2でYがコミット、その後の編集でZ)
  2. Filter: T3 > T2 なのでEdit Bを適用しようとする
  3. FAIL: old_string "Y" が見つからない（現在の内容はZ）
```

### 解決アプローチ候補 (reference)

1. **Edit chain reconstruction**: 操作間の依存関係を分析し、適用順序を再構築
2. **Content-aware filtering**: コミット内容とold_stringを比較し、既に適用済みの操作を検出
3. **Fuzzy matching**: old_stringの近似マッチングで適用可能な操作を検出

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-02 | DRAFT | Created from F738 deferred item |
| 2026-02-02 | Phase 2 | tech-investigator: Root Cause Analysis, Feasibility Assessment completed |
