# Research Type Quality Guide

Issues specific to `Type: research` features (Feature to create Features).

---

## Granularity

- **Investigation report** producing sub-features
- AC count: 3-5
- Main deliverable: feature-{ID}.md files

---

## Characteristics

Research features are **Planning Features** that:
1. Analyze a domain/phase
2. Create sub-features as output
3. Have two FL review phases (pre: design, post: coverage)

---

## Common Issues

### Issue 1: Output Not Clearly Defined

**Symptom**: Summary doesn't specify what features will be created.

**Example (Bad)**:
```markdown
## Summary

Investigate Phase 8 requirements.
```

**Example (Good)**:
```markdown
## Summary

**Feature to create Features**: Phase 8 Planning

Create sub-features for Phase 8 expression system:
- F416: ExpressionParser Migration
- F417: Operator Implementation
- F418-F420: Built-in Functions (Core/Data/Game)
- F421: Function Call Mechanism
- F422: Type Conversion & Casting
```

---

### Issue 2: Missing Coverage AC

**Symptom**: No AC to verify all required sub-features created.

**Example (Good)**:
```markdown
| 3 | Coverage: All Phase 8 components | file | Glob | count_equals | 7 | [ ] |
```

With AC Details:
```markdown
**AC#3**: All Phase 8 components have sub-features
- Parser (F416)
- Operators (F417)
- Functions Core/Data/Game (F418-F420)
- Function Call Mechanism (F421)
- Type Conversion (F422)
- Total: 7 sub-features
```

---

### Issue 3: Successor Features Not Linked

**Symptom**: Created features not linked in Dependencies.

**Example (Good)**:
```markdown
## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Successor | F416 | ExpressionParser Migration (created by this feature) |
| Successor | F417 | Operator Implementation (created by this feature) |
| Successor | F418 | Built-in Functions Core (created by this feature) |
```

---

### Issue 4: Pre/Post FL Review Not Planned

**Symptom**: Research feature without two-phase review.

**Workflow**:
```
[DRAFT] → /fc → [PROPOSED] → /fl (pre) → [REVIEWED] → /run → [DONE] → /fl (post)
                        ↑                                        ↑
                   Design verification                      Coverage verification
```

**ACs should include**:
```markdown
| 4 | Pre-FL: Design validation | review | /fl pre | succeeds | - | [ ] |
| 5 | Post-FL: Coverage validation | review | /fl post | succeeds | - | [ ] |
```

---

### Issue 5: Non-Standard File Patterns in Sub-Feature ACs

**Symptom**: ACs use non-standard Glob patterns like `feature-*-post-phase.md` or overly narrow ranges like `feature-45[1-9].md`.

**Example (Bad)**:
```markdown
| 4 | Post-Phase Review created | file | Glob | exists | "feature-*-post-phase.md" | [ ] |
| 5 | Phase 13 Planning created | file | Glob | exists | "feature-*-phase13.md" | [ ] |
```

**Example (Good)**:
```markdown
| 4 | Post-Phase Review in index | file | Grep | contains | "Post-Phase Review.*Phase 12" | [ ] |
| 5 | Phase 13 Planning in index | file | Grep | contains | "Phase 13 Planning" | [ ] |
```

**Fix**: Use Grep in index-features.md to verify sub-feature registration, or use specific feature IDs when known.

---

### Issue 6: AC Expected Conflicts with Existing Content

**Symptom**: AC's Expected value already appears in Scope Reference section, causing immediate PASS without work.

**Example (Bad)**:
```markdown
| 2 | COM categorization complete | file | Grep | contains | "COM Category Breakdown:" | [ ] |
```
(Where "COM Category Breakdown:" exists in the feature's own Scope Reference section)

**Example (Good)**:
```markdown
| 2 | COM categorization complete | file | Grep | contains | "Phase 12 COM Analysis:" | [ ] |
```
(Use unique marker that appears only in Execution Log after work is done)

**Fix**: Use unique markers for deliverables that distinguish them from reference/planning sections.

---

### Issue 7: Analysis Method Not Documented

**Symptom**: How to investigate not specified.

**Example (Good)**:
```markdown
## Implementation Contract

### Analysis Method

1. Read legacy source files:
   - `engine/Assets/Scripts/Emuera/GameData/Expression/*.cs`
   - `engine/Assets/Scripts/Emuera/GameData/Function/*.cs`

2. Identify migration targets:
   - Static classes → DI-compatible classes
   - IOperandTerm → AST nodes
   - Global state → Scoped services

3. Group by dependency:
   - Parser before operators
   - Operators before functions
   - Core functions before game functions
```

---

### Issue 8: Regex Pattern Asymmetry in Range Checks

**Symptom**: Regex OR alternatives have inconsistent structure (e.g., first has delimiters, second doesn't).

**Example (Bad)**:
```markdown
| 6 | index updated | file | Grep | contains | "\\| 45[2-9]\\|| 46[0-9]" | [ ] |
```
First alternative `\\| 45[2-9]\\|` matches `| 452 |` with pipes, second ` 46[0-9]` matches only `460` without trailing pipe.

**Example (Good)**:
```markdown
| 6 | index updated | file | Grep | contains | "\\| 45[2-9] \\||\\| 46[0-9] \\|" | [ ] |
```
Both alternatives have symmetric structure with leading and trailing pipe delimiters.

**Fix**: Ensure all regex OR alternatives have identical delimiter structure for consistency and maintainability.

---

### Issue 9: Incomplete Interface Coverage in Pattern

**Symptom**: OR pattern for interface verification missing one or more interfaces.

**Example (Bad)**:
```markdown
| 3 | Interfaces created | file | Grep | contains | "IDialogueLoader\\|IConditionEvaluator\\|IDialogueRenderer" | [ ] |
```
(Missing IDialogueSelector when architecture defines 4 interfaces)

**Example (Good)**:
```markdown
| 3 | Interfaces created | file | Grep | contains | "IDialogueLoader\\|IConditionEvaluator\\|IDialogueRenderer\\|IDialogueSelector" | [ ] |
```

**Fix**: Verify pattern includes all items from architecture.md before writing AC.

---

### Issue 10: Multi-Command AC Without Conjunction Note

**Symptom**: AC Details shows multiple commands but doesn't specify if both must pass.

**Example (Bad)**:
```markdown
**AC#6**: Philosophy verified
- Method: `Grep(pattern, "feature-54*.md")` and `Grep(pattern, "feature-55[0-5].md")`
- Expected: Philosophy inheritance
```

**Example (Good)**:
```markdown
**AC#6**: Philosophy verified
- Method: `Grep(pattern, "feature-54*.md")` and `Grep(pattern, "feature-55[0-5].md")`
- Expected: Philosophy inheritance
- Note: Both Grep commands must return matches for AC to PASS
```

**Fix**: Always specify if multiple commands require AND or OR logic.

---

### Issue 11: Missing Decomposition Rationale

**Symptom**: Feature count differs from architecture task count without explanation.

**Example (Bad)**:
```markdown
### Expected Feature ID Allocation

| Component | Feature ID | Justification |
|-----------|------------|---------------|
| Pattern | F546-F548 | 3 features |

**Total**: 14 sub-features
```
(Architecture has 13 tasks but allocation shows 14 features - no explanation)

**Example (Good)**:
```markdown
**Decomposition Rationale**: architecture.md lists 13 tasks but allocation shows 14 features because:
- Specification Pattern (architecture Tasks 5-6) expanded into 3 features (F546-F548) for proper granularity per feature-template.md 8-15 AC guideline
```

**Fix**: Document why architecture task count differs from feature count.

---

### Issue 12: Matcher Type Mismatch with Pattern

**Symptom**: AC uses `contains` matcher but Expected has regex patterns (`.*`, `|`, `[0-9]`).

**Example (Bad)**:
```markdown
| 2 | Boundary documented | file | Grep | contains | "C# Engine.*YAML Content" | [ ] |
```
`contains` is for literal substring matching, not regex.

**Example (Good)**:
```markdown
| 2 | Boundary documented | file | Grep | matches | "C# Engine.*YAML Content" | [ ] |
```
Or for literal matching:
```markdown
| 2 | Boundary documented | file | Grep | contains | "C# Engine" | [ ] |
```

**Fix**: Use `matches` for regex patterns, `contains` for literal substrings.

---

### Issue 13: Missing Analysis Quality Traceability

**Symptom**: Research deliverable doesn't verify correspondence between analysis findings and recommendations.

**Example (Bad)**:
```markdown
| 3 | Bottleneck analysis included | file | Grep | contains | "Bottleneck Analysis" | [ ] |
| 4 | Recommendations documented | file | Grep | contains | "Recommendations" | [ ] |
```
No verification that bottlenecks map to specific recommendations.

**Example (Good)**:
```markdown
| 3 | Bottleneck analysis included | file | Grep | contains | "Bottleneck Analysis" | [ ] |
| 4 | Recommendations documented | file | Grep | contains | "Recommendations" | [ ] |
| 8 | Bottleneck-recommendation traceability | file | Grep | contains | "対応:" | [ ] |
```

**Fix**: Add explicit traceability markers for analytical coherence and decision-making quality.

---

### Issue 14: Performance Analysis Lacks Statistical Rigor

**Symptom**: Profiling methodology missing statistical validity requirements.

**Example (Bad)**:
```markdown
### Research Methodology
- Run profiling against COM scenarios (simple, medium, complex)
```

**Example (Good)**:
```markdown
### Research Methodology
- Run profiling against COM scenarios (simple, medium, complex)
- **Profiling Protocol**:
  - Minimum 10 measurement runs per scenario
  - 2 warm-up runs before measurement (JIT compilation, cache loading)
  - Test scenarios: Simple (1-2 effects), Medium (3-5 effects), Complex (6+ effects with conditions)
  - Record mean, standard deviation, min/max for statistical validity
```

**Fix**: Specify quantitative measurement protocols for reliable performance analysis.

---

### Issue 15: Missing Next Feature Number Increment AC

**Symptom**: Planning Feature creates sub-features and updates index but no AC verifies the Next Feature number was incremented past all allocated IDs.

**Example (Bad)**:
```markdown
| 2 | Index updated | file | Grep | count_equals | 10 | [ ] |
```
(Verifies sub-features registered but not that Next Feature number prevents ID collision)

**Example (Good)**:
```markdown
| 2 | Index updated | file | Grep | count_equals | 10 | [ ] |
| 6 | Next Feature number incremented | file | Grep("Next Feature number") | contains | "784" | [ ] |
```

**Fix**: Always include an AC to verify Next Feature number was incremented past the last allocated ID. Without this, subsequent `/fc` invocations may allocate conflicting IDs.

---

### Issue 16: Transition Feature Existence Not Verified

**Symptom**: AC verifies transition features (Post-Phase Review, Next Phase Planning) are registered in index but not that the DRAFT files actually exist on disk.

**Example (Bad)**:
```markdown
| 3 | Transition features created | file | Grep(index-features.md) | contains | "Post-Phase Review" | [ ] |
```
(Index registration verified but file may not exist)

**Example (Good)**:
```markdown
| 3 | Transition features created | file | Glob(feature-78[23].md) + Grep(index-features.md) | count_equals + contains | 2 AND "Post-Phase Review Phase 20" AND "Phase 21 Planning" | [ ] |
```

**Fix**: Use Glob to verify DRAFT file existence AND Grep to verify index registration. Both checks must pass (AND conjunction). Document conjunction logic in AC Details Note per Issue 10.

---

## Checklist

- [ ] Summary explicitly states "Feature to create Features"
- [ ] Expected sub-features listed in Summary
- [ ] Feature IDs assigned in PROPOSED stage (e.g., F444-F447, not F4XX placeholders)
- [ ] AC#2-N use Glob exists for file existence, not Grep contains
- [ ] Sub-feature verification uses standard patterns (feature-{ID}.md or Grep in index)
- [ ] Regex patterns with OR have symmetric structure (Issue 8)
- [ ] AC Expected values are unique (not already in Scope Reference section)
- [ ] All created features linked as Successors
- [ ] Pre/Post FL review handled by /fl workflow (implicit, no explicit ACs required)
- [ ] Analysis method documented
- [ ] 3-5 AC count guideline (flexible: more ACs acceptable with justification in Review Notes, see F424/F437 precedent)
- [ ] Phase Planning features: Implementation Contract has Expected Feature ID Allocation table with task-to-feature mapping and decomposition rationale (see F450, F463, F541)
- [ ] Multi-command ACs specify conjunction logic (AND/OR) in AC Details Note
- [ ] Decomposition rationale documents task-to-feature count differences
- [ ] AC Matcher matches pattern type (`contains` for literals, `matches` for regex)
- [ ] Analysis deliverables have quality traceability (correspondence between findings and conclusions)
- [ ] Performance/measurement research includes statistical rigor (run count, warm-up, validity metrics)
- [ ] Phase Planning features: AC includes Next Feature number increment verification (Issue 15)
- [ ] Phase Planning features: Transition feature ACs verify both file existence (Glob) and index registration (Grep) (Issue 16)
