# Feature 570: YAML COM Performance Analysis

## Status: [DONE]

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

## Type: research

## Background

### Philosophy (Mid-term Vision)
Performance optimization must be evidence-based. Before implementing caching, lazy loading, or other optimizations, baseline measurements and bottleneck identification are required. This research establishes the SSOT for YAML COM performance characteristics, enabling data-driven optimization decisions in follow-up features.

### Problem (Current Issue)
F565 introduced YAML-based COM runtime, but no performance profiling has been conducted. Without baseline metrics, it's unknown whether optimization is needed, and if so, where bottlenecks exist. Premature optimization without profiling data risks wasting effort on non-critical paths.

### Goal (What to Achieve)
Profile YAML COM execution to establish baseline metrics, identify performance bottlenecks (if any), and produce actionable recommendations for optimization. Output: Performance Analysis Report documenting measurements and recommendations.

**Feature to create Features**: Performance analysis producing optimization feature(s) if needed.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Profiling report exists | file | Glob | exists | Game/agents/f570-performance-report.md | [x] |
| 2 | Baseline metrics documented | file | Grep(Game/agents/f570-performance-report.md) | contains | "Baseline Metrics" | [x] |
| 3 | Bottleneck analysis included | file | Grep(Game/agents/f570-performance-report.md) | contains | "Bottleneck Analysis" | [x] |
| 4 | Recommendations documented | file | Grep(Game/agents/f570-performance-report.md) | contains | "Recommendations" | [x] |
| 5 | Instrumentation code added | file | Grep(Era.Core/) | matches | Stopwatch.*(parsing\|validation\|condition\|effect) | [x] |
| 6a | Follow-up feature registered | file | Grep(Game/agents/index-features.md) | contains | "F570 optimization" | [x] |
| 6b | No optimization documented | file | Grep(Game/agents/f570-performance-report.md) | contains | "No Optimization Required" | N/A |
| 7 | Bottleneck threshold applied | file | Grep(Game/agents/f570-performance-report.md) | contains | ">10% of total" | [x] |
| 8 | Bottleneck-recommendation traceability | file | Grep(Game/agents/f570-performance-report.md) | contains | "対応:" | [x] |

### AC Details

| AC# | Verification Method |
|:---:|---------------------|
| 1 | `Glob(Game/agents/f570-performance-report.md)` returns file path |
| 2 | `Grep(Game/agents/f570-performance-report.md)` contains "## Baseline Metrics" section with timing data (ms) |
| 3 | `Grep(Game/agents/f570-performance-report.md)` contains "## Bottleneck Analysis" section identifying hot paths |
| 4 | `Grep(Game/agents/f570-performance-report.md)` contains "## Recommendations" section with prioritized optimization list |
| 5 | `Grep(Era.Core/)` matches regex pattern for Stopwatch instrumentation (parsing in Era.Core/Data/YamlComLoader, validation/condition/effect in Era.Core/Commands/Com/) |
| 6a | `Grep(Game/agents/index-features.md)` contains "F570 optimization" - verifies follow-up feature registered in index if optimization needed |
| 6b | `Grep(Game/agents/f570-performance-report.md)` contains "No Optimization Required" if no optimization needed. **Note**: AC#6a XOR AC#6b - exactly one must pass. Task#5 enforces this by requiring mandatory completion of one outcome based on analysis results. |
| 7 | `Grep(Game/agents/f570-performance-report.md)` contains ">10% of total" indicating bottleneck threshold methodology was applied |
| 8 | `Grep(Game/agents/f570-performance-report.md)` contains "対応:" indicating explicit correspondence between identified bottlenecks and recommendations |

---

## Implementation Contract

### Research Methodology

1. **Profiling Setup**
   - Add timing instrumentation to:
     - `YamlComLoader.Load()` - YAML parsing and deserialization (Era.Core/Data/)
     - `YamlComExecutor.Execute()` - overall execution
     - `YamlComValidator.ValidateSchema()` - schema validation
     - `YamlComExecutor.CheckConditions()` - condition evaluation (private method: instrument internally or change to internal for profiling)
     - Effect handler `Apply()` methods in Era.Core/Effects/ (SourceEffectHandler, SourceScaleEffectHandler, DownbaseEffectHandler, ExpEffectHandler)
   - Measure: YAML parsing, schema validation, condition evaluation, effect execution
   - Run profiling against COM scenarios (simple, medium, complex)
   - **Profiling Protocol**:
     - Minimum 10 measurement runs per scenario
     - 2 warm-up runs before measurement (JIT compilation, cache loading)
     - Test scenarios: Simple (1-2 effects), Medium (3-5 effects), Complex (6+ effects with conditions)
     - Record mean, standard deviation, min/max for statistical validity

2. **Baseline Metrics to Collect**
   - Total COM execution time (ms)
   - YAML parsing time (ms) - from YamlComLoader
   - Schema validation time (ms) - from YamlComValidator
   - Condition evaluation time (ms)
   - Effect execution time (ms per effect type)
   - Memory allocation (if significant)

3. **Bottleneck Identification**
   - Compare timing across execution phases
   - Identify operations > 10% of total time
   - Document call frequency and cumulative impact

4. **Report Structure**
   ```markdown
   # F570 Performance Analysis Report

   ## Baseline Metrics
   | Metric | Simple COM | Medium COM | Complex COM |
   |--------|------------|------------|-------------|
   | Total (ms) | X | Y | Z |
   | Parsing (ms) | ... | ... | ... |
   | Conditions (ms) | ... | ... | ... |
   | Effects (ms) | ... | ... | ... |

   ## Bottleneck Analysis
   - Primary bottleneck: {description}
   - Secondary bottleneck: {description}

   ## Recommendations
   1. {Priority 1 optimization with expected impact}
   2. {Priority 2 optimization with expected impact}
   ```

5. **Follow-up Feature Creation**
   - If optimization needed: Create F570-follow-up with specific optimization tasks
   - If no optimization needed: Document "no action required" with evidence

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | 5 | Add Stopwatch instrumentation: YamlComLoader.Load(), YamlComExecutor.Execute(), YamlComValidator.ValidateSchema(), YamlComExecutor.CheckConditions(), Effect handler Apply() methods | [x] |
| 1 | 1 | Create profiling report file | [x] |
| 2 | 2 | Collect and document baseline metrics | [x] |
| 3a | 3 | Analyze bottlenecks and identify hot paths | [x] |
| 3b | 7 | Apply >10% threshold methodology and document in report | [x] |
| 4a | 4 | Write recommendations | [x] |
| 4b | 8 | Ensure explicit correspondence between bottlenecks and recommendations using "対応:" markers | [x] |
| 5 | 6a,6b | Create follow-up feature (6a) OR document "No Optimization Required" (6b) | [x] |

---

## Dependencies

| Type | Feature | Relationship | Notes |
|------|---------|--------------|-------|
| Predecessor | F565 | Successor | COM YAML Runtime Integration |
| Successor | F580 | Created | COM Loader F570 optimization |

---

## Links

- [index-features.md](index-features.md)
- [feature-565.md](feature-565.md) - COM YAML Runtime Integration
- [feature-569.md](feature-569.md) - Related successor feature
- [feature-571.md](feature-571.md) - Related successor feature
- [feature-572.md](feature-572.md) - Related successor feature
- [feature-573.md](feature-573.md) - Related successor feature
- [feature-580.md](feature-580.md) - COM Loader F570 optimization (follow-up)
- [f570-performance-report.md](f570-performance-report.md) - Performance Analysis Report

---

## Review Notes
- [resolved] Phase0-RefCheck iter0: Original AC#1 referenced non-existent file. Resolved by changing to research type with report-based ACs.
- [resolved] Phase1-Uncertain iter1: Follow-up feature creation tracking added as AC#6 and Task#5.
- [resolved] Phase1-Uncertain iter2: AC#5 pattern enhanced to verify multiple profiling points (parsing, condition, effect).
- [applied] Phase1-Uncertain iter2: Bottleneck-to-recommendation traceability - Added AC#8 with "対応:" marker verification and Task#4b for explicit correspondence implementation.
- [resolved] Phase1 iter3: AC#5 matcher changed to 'matches' for regex support; AC#1 Method/Expected split corrected; AC#6 split into 6a/6b for proper OR logic.
- [resolved] Phase1 iter3: Task#0 clarified - Apply() encompasses EvaluateFormula timing (private static method).
- [applied] Phase1-Uncertain iter3: Profiling methodology specifics - Added detailed profiling protocol to Implementation Contract: minimum 10 runs, warm-up runs, scenario definitions, statistical validity requirements.
- [resolved] Phase1 iter4: Architecture mismatch fixed - YAML parsing occurs in YamlComLoader, not YamlComExecutor. Expanded AC#5 scope to Era.Core/Commands/Com, added YamlComLoader.Load() and YamlComValidator.ValidateSchema() to instrumentation targets, removed trivial EffectContext creation.
- [resolved] Phase1 iter5: AC#5 Grep path expanded to Era.Core (YamlComLoader in Era.Core/Data, not Commands/Com). Added CheckConditions() to instrumentation targets for separate condition evaluation timing.
- [pending] Phase1-Uncertain iter5: AC#5 regex pattern is implementation-directive (prescribes variable naming). Simpler 'Stopwatch' pattern with count_equals could be more flexible. Current pattern accepted as sufficient constraint.
- [resolved] Phase1 iter6: IEffectHandler.Apply() is interface - replaced with concrete effect handler Apply() methods in Implementation Contract and Task#0.
- [resolved] Phase1 iter7: AC#6a changed from non-standard Glob pattern to Grep index-features.md for follow-up feature verification.
- [resolved] Phase1 iter8: Feature name in index-features.md updated from "Performance Optimization" to "YAML COM Performance Analysis" for consistency.
- [resolved] Phase2 iter9: AC#6a/6b XOR enforcement clarified - Task#5 enforces mandatory completion of exactly one outcome.
- [pending] Phase2-Uncertain iter9: Philosophy claims "enabling data-driven optimization decisions" but ACs verify section existence, not decision-enablement quality. Accepted as qualitative review responsibility.
- [resolved] Phase2 iter10: CheckConditions private method instrumentation approach clarified in Implementation Contract.
- [pending] Phase2-Uncertain iter10: AC#5 regex pattern couples verification to variable naming convention. count_equals could be more robust, but current pattern is acceptable.
- [resolved] Phase3 iter10: AC format standardized - Type changed to 'file' where appropriate, Method format changed to Grep(path) per SSOT.
- [resolved] Phase5 iter10: AC:Task 1:1 violation fixed - Task#3 split into Task#3a (AC#3) and Task#3b (AC#7).
- [pending] Phase5-PlanningValidation iter1: Feature type mismatch: planning-validator is designed for 'Feature to create Features' that decompose architecture.md Phases into sub-features (pattern: F398, F409). F570 is a performance research feature that conditionally creates follow-up optimization feature(s) based on analysis results - not a Phase Planning feature.

### AC Count Justification
9 ACs (guideline: 3-5). Justification: Research type with comprehensive methodology verification and XOR condition split:
- AC#5: Instrumentation coverage with regex pattern matching
- AC#6a/6b: Follow-up feature XOR "no action" documentation (mutually exclusive outcomes)
- AC#7: Methodology compliance (>10% threshold)
- AC#8: Bottleneck-recommendation traceability for analysis quality

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-21 11:03 | START | implementer | Task 0 | - |
| 2026-01-21 11:03 | END | implementer | Task 0 | SUCCESS |
| 2026-01-21 11:10 | START | implementer | Task 1,2,3a,3b,4a,4b | - |
| 2026-01-21 11:10 | END | implementer | Task 1,2,3a,3b,4a,4b | SUCCESS |

---
