# Feature 686: KojoTestRunner/InteractiveRunner DisplayMode Integration

## Status: [DONE]

## Type: research

## Created: 2026-01-30

---

## Summary

Integration of DisplayModeConsumer with KojoTestRunner and InteractiveRunner to enable YAML-sourced dialogue with DisplayMode behavior in engine test runners. This enables testing and interactive execution of YAML dialogues with correct wait/key-wait blocking and display-style rendering behavior.

---

## Links

- [feature-684.md](feature-684.md) - GUI Consumer Display Mode Interpretation (Predecessor - DisplayModeConsumer creation)
- [feature-687.md](feature-687.md) - KojoTestRunner DisplayMode Enhanced Reporting (Successor - created by this research)
- [feature-688.md](feature-688.md) - InteractiveRunner DisplayMode JSON Response (Successor - created by this research)

---

## Notes

- Created as F684 残課題 - DisplayModeConsumer exists and is tested but not yet consumed by engine runners
- Scope: Modify KojoTestRunner.cs and InteractiveRunner.cs to use DisplayModeConsumer.Render() instead of direct text output
- Target: Replace direct console.PrintLine(text) with DisplayModeConsumer.Render(dialogueResult, console)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F684 | [DONE] | DisplayModeConsumer class creation. Must be completed first. |

---

## Background

### Philosophy

KojoTestRunner and InteractiveRunner should eventually provide DisplayMode interpretation for YAML-sourced dialogue. However, architectural analysis reveals that DisplayModeConsumer integration requires DialogueResult access, which these runners currently lack (they execute ERB scripts directly via EmueraConsole). Integration requires either creating a YAML execution path or developing alternative approaches.

### Problem

KojoTestRunner and InteractiveRunner output plain text without DisplayMode interpretation. YAML dialogue with DisplayMode metadata is not rendered with proper wait/key-wait blocking or display-style behavior in test and interactive environments.

#### Root Cause Analysis

KojoTestRunner and InteractiveRunner execute ERB scripts directly via EmueraConsole, not YAML dialogue through KojoEngine. They don't produce DialogueResult objects. The runners capture ERB output from EmueraConsole display lines (via CaptureOutput() in KojoTestRunner), not from YAML dialogue rendering. Integration requires identifying where DialogueResult can be accessed in YAML-mode execution or creating a new execution path that uses KojoEngine→DialogueResult→DisplayModeConsumer.

### Goal

**Research-Phase Goal**: Investigate architectural requirements for enabling DisplayModeConsumer integration in KojoTestRunner and InteractiveRunner. Determine if integration requires creating a new YAML execution path or if other approaches are feasible. Document findings and create successor feature if architectural changes are needed.

### Recommended approach:

**Key Finding**: Both KojoTestRunner and InteractiveRunner execute ERB scripts directly via `CalledFunction.CallFunction()` → `state_.IntoFunction()` → `EmueraConsole`. They capture output via `console.GetDisplayLinesForuEmuera(i).ToString()`. Neither runner produces `DialogueResult` objects required by `DisplayModeConsumer.Render()`.

**Integration Options Evaluated**:

1. **Option A - YAML Execution Path**: Create new execution mode that uses KojoEngine → DialogueResult → DisplayModeConsumer
   - Pros: Direct integration with DisplayModeConsumer
   - Cons: Requires significant architectural changes to runners

2. **Option B - Parse Display Lines**: Modify output capture to reconstruct DisplayMode from console display lines
   - Pros: No DialogueResult required
   - Cons: DisplayMode info lost at EmueraConsole level

3. **Option C - Leverage DisplayModeCapture (F678)**: Use existing DisplayModeCapture infrastructure which already captures DisplayMode per line
   - Pros: F678 already captures `StructuredOutput` with DisplayMode metadata; minimal changes needed
   - Cons: Doesn't provide runtime wait/key-wait behavior (test mode only)

**Recommended approach: Option C - Leverage DisplayModeCapture**

F678 already introduced `DisplayModeCapture.StartCapture()` and `DisplayModeCapture.GetLines()` which captures DisplayMode metadata per line. The test result `StructuredOutput` field already contains this data. Integration should:
- F687 (KojoTestRunner): Enhance test output reporting to expose DisplayMode metadata from StructuredOutput
- F688 (InteractiveRunner): Add StructuredOutput to JSON response for DisplayMode-aware clients

This approach builds on existing F678 infrastructure without requiring DialogueResult integration.

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Method | Status |
|:---:|-------------|------|---------|----------|--------|:------:|
| 1 | Research documents architectural requirements for KojoTestRunner integration | file | exists | Game/agents/feature-687.md | Glob | [x] |
| 2 | Research documents architectural requirements for InteractiveRunner integration | file | exists | Game/agents/feature-688.md | Glob | [x] |
| 3 | Integration approach decision documented in F686 | code | contains | "Recommended approach:" | Grep(Game/agents/feature-686.md) | [x] |

---

## Tasks

| Task# | Description | Acceptance Criteria | Status |
|:-----:|-------------|:-------------------:|:------:|
| 1 | Analyze KojoTestRunner architecture and document DisplayModeConsumer integration requirements | AC#1 | [x] |
| 2 | Analyze InteractiveRunner architecture and document DisplayModeConsumer integration requirements | AC#2 | [x] |
| 3 | Research alternative integration approaches (YAML execution path, other options) | AC#3 | [x] |
| 4 | Document recommended approach in F686 | AC#3 | [x] |
| 5 | Create F687/F688 DRAFT files and register in index-features.md | AC#1, AC#2 | [x] |

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| KojoTestRunner DisplayMode reporting | Out of scope - implementation | Feature | F687 | Task#5 |
| InteractiveRunner DisplayMode JSON | Out of scope - implementation | Feature | F688 | Task#5 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-31 | init | initializer | Status [REVIEWED]→[WIP] | READY |
| 2026-01-31 | investigate | orchestrator | Read KojoTestRunner.cs, InteractiveRunner.cs, DisplayModeConsumer.cs | Artifacts confirmed |
| 2026-01-31 | research | orchestrator | Analyze integration options (A/B/C) | Option C recommended |
| 2026-01-31 | implement | orchestrator | Create F687, F688 DRAFT files | SUCCESS |
| 2026-01-31 | implement | orchestrator | Register F687, F688 in index-features.md | SUCCESS |
| 2026-01-31 | verify | orchestrator | AC verification (AC#1-3) | PASS:3/3 |