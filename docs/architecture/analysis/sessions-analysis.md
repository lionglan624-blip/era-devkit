# Sessions-based Context Sharing Analysis

## Feature Context

| Key | Value |
|-----|-------|
| Feature ID | 141 |
| Purpose | Evaluate Sessions (resume) for 1:1 sequential subagent patterns |
| Reference | [sessions-reference.md](../reference/sessions-reference.md) |
| Prior Analysis | [anthropic-recommended-transition.md](anthropic-recommended-transition.md) |

---

## Pattern Analysis

### Summary Table

| # | Pattern | Current Implementation | Context Passing | Separation Importance | Sessions Fit |
|:-:|---------|----------------------|-----------------|:---------------------:|:------------:|
| 1 | implementer → ac-tester | feature.md経由 | File-based (structured output in feature.md) | **HIGH** | LOW |
| 2 | implementer → unit-tester | feature.md経由 | File-based (Task# in feature.md) | **HIGH** | LOW |
| 3 | kojo-writer → unit-tester | status file polling | File-based (status/{ID}_K{N}.txt) | **HIGH** | LOW |
| 4 | debugger → unit-tester | Direct (dispatch chain) | Implicit (fix applied, re-test) | MEDIUM | MEDIUM |

### Pattern 1: implementer → ac-tester

| Aspect | Current | With Sessions |
|--------|---------|---------------|
| **Context Flow** | implementer writes structured output to feature.md → ac-tester reads | ac-tester resumes implementer session, inherits full context |
| **Separation** | Complete - ac-tester evaluates without implementation bias | Compromised - inherits implementer's internal reasoning |
| **Bias Risk** | None - fresh evaluation | High - may accept "it works" based on implementer confidence |
| **Auditability** | Full - all outputs in feature.md | Reduced - session state not externally visible |

**Analysis**: Separation is critical. The ac-tester MUST evaluate objectively without knowing WHY the implementer thinks the code works. File-based is correct.

### Pattern 2: implementer → unit-tester

| Aspect | Current | With Sessions |
|--------|---------|---------------|
| **Context Flow** | unit-tester reads feature.md for Task#, runs test independently | unit-tester resumes implementer session |
| **Separation** | Complete - test execution is objective | Compromised - may skip tests based on implementer confidence |
| **Simplicity** | unit-tester only needs Task# and test command | Would receive 100K+ tokens of implementation context |
| **Cost** | Haiku model, minimal context | Would inherit large context, increased cost |

**Analysis**: unit-tester needs only Task# to run tests. Inheriting full implementation context is wasteful and potentially biasing. File-based is correct.

### Pattern 3: kojo-writer → unit-tester

| Aspect | Current | With Sessions |
|--------|---------|---------------|
| **Context Flow** | kojo-writer writes status file → Opus polls → dispatches unit-tester | unit-tester resumes kojo-writer session |
| **Why Polling** | kojo-writer generates 100K+ tokens, TaskOutput would bloat orchestrator | Sessions would share this context |
| **Separation** | Complete - test execution independent | Compromised - tester may trust kojo-writer's "done" signal |
| **Context Size** | ~50 bytes (status file) | 100K+ tokens (full session) |

**Analysis**: The current polling pattern exists specifically to avoid context bloat. Sessions would defeat this purpose entirely. File-based is mandatory for performance.

### Pattern 4: debugger → unit-tester

| Aspect | Current | With Sessions |
|--------|---------|---------------|
| **Context Flow** | debugger fixes code → Opus dispatches unit-tester with same Task# | unit-tester resumes debugger session |
| **Separation** | Moderate - unit-tester re-executes test regardless of debugger confidence | Reduced - may trust "FIXED" status |
| **Benefit** | Clean re-test ensures actual fix | Debugger context may help diagnose if still fails |
| **Risk** | None | Low - debugger context is diagnostic, not implementation |

**Analysis**: This is the only pattern where Sessions MIGHT add value. Debugger context could help if re-test fails again. However, current design works well and separation ensures objective verification.

---

## Recommendation: KEEP_FILE_BASED

---

## Rationale

### 1. Separation Principle (作成者と評価者の分離)

The core architectural principle driving this decision is **separation of creator and evaluator roles**. This pattern appears in 3 of 4 analyzed patterns with HIGH importance:

| Pattern | Creator | Evaluator | Separation Importance |
|---------|---------|-----------|:---------------------:|
| implementer → ac-tester | implementer | ac-tester | **HIGH** |
| implementer → unit-tester | implementer | unit-tester | **HIGH** |
| kojo-writer → unit-tester | kojo-writer | unit-tester | **HIGH** |
| debugger → unit-tester | debugger | unit-tester | MEDIUM |

**Why separation matters**: When a tester inherits the creator's full context (including internal reasoning, confidence signals, and implementation decisions), objectivity is compromised. The evaluator may unconsciously trust the creator's "it works" signals rather than performing independent verification.

**File-based approach enforces separation**: By communicating only through structured files (feature.md, status files), the evaluator receives ONLY the information needed for evaluation - not the creator's internal reasoning process.

### 2. Context Inheritance Concern (バイアス継承の懸念)

Sessions-based context sharing creates an **implicit bias inheritance path**:

```
implementer session:
  - 100K+ tokens of implementation context
  - Internal reasoning: "This approach should work because..."
  - Confidence signals embedded in natural language

                    ↓ resume ↓

ac-tester inherits:
  - Full implementation context (unnecessary for testing)
  - Implementer's confidence bias
  - May skip verification if implementer expressed high confidence
```

**File-based approach isolates context**: The tester starts with a fresh context, reading only structured output (Task completion status, file paths changed). This forces genuine re-evaluation rather than inherited assumption.

### 3. 1:N Parallelism Requirement

The kojo-writer workflow demonstrates why Sessions is fundamentally unsuitable:

```
eratw-reader → kojo-writer ×10 (parallel batch)
```

Sessions are inherently 1:1 sequential. File-based coordination (status file polling) enables:
- Multiple kojo-writers executing in parallel
- Independent success/failure tracking per writer
- No blocking on session serialization

### 4. Context Efficiency

| Method | Context Size | Use Case |
|--------|-------------|----------|
| File-based (status file) | ~100 bytes | Pass Task#, status only |
| File-based (feature.md) | ~5KB | Structured progress tracking |
| Sessions (TaskOutput resume) | ~550K tokens | Full session inheritance |

For unit-tester (Haiku model), inheriting 550K tokens of implementation context is:
- **Wasteful**: Haiku only needs Task# to run tests
- **Expensive**: Token cost scales with context size
- **Slow**: Context processing time increases

### 5. Current Design Alignment with Anthropic Best Practices

The existing file-based architecture already follows Anthropic's recommended patterns:

| Anthropic Principle | Current Implementation | Status |
|---------------------|----------------------|:------:|
| Structured communication | feature.md with defined schema | ALIGNED |
| Clear agent boundaries | Dispatcher controls all transitions | ALIGNED |
| Audit trail | Execution Log in feature.md | ALIGNED |
| Cost efficiency | Haiku for testing, Opus for creation | ALIGNED |

**Sessions would not improve alignment** - it would reduce it by blurring agent boundaries and audit trails.

### Conclusion

**KEEP_FILE_BASED** is the correct choice because:

1. **Separation principle is paramount** - 3/4 patterns require independent evaluation
2. **Bias inheritance is a real risk** - objective testing requires fresh context
3. **1:N parallelism is required** - Sessions cannot support batch workflows
4. **Context efficiency matters** - file-based is 5000x more efficient
5. **Current design is already optimal** - no improvement from Sessions adoption

---

## Decision Log

| Date | Decision | By |
|------|----------|-----|
| 2025-12-20 | Pattern analysis completed | implementer |
| 2025-12-20 | Rationale documented with separation principle analysis | implementer |
