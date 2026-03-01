# Extensible Orchestrator Design

**Status**: DRAFT
**Feature**: [feature-612.md](../feature-612.md)
**Created**: 2026-01-24

---

## Overview

### Design Philosophy

Configuration-driven multi-phase agent orchestration establishes extensible patterns for workflow automation. This design eliminates hardcoded phase limitations by providing configurable orchestrator patterns that support variable phase counts, dynamic agent assignments, and flexible model selections across different workflow types.

**Responsibility**: This design defines abstract orchestration patterns and implementation templates. Individual skills (e.g., F610 Feature Creator) implement these patterns for their specific workflow requirements. The orchestrator design is the single source of truth for multi-phase workflow configuration.

**Scope**: This design covers:
- Configuration schema for orchestrator workflows
- Phase definition and dependency management
- Agent dispatch patterns with error handling
- Resume logic for interrupted workflows
- Context isolation strategies for context window efficiency
- Model selection per phase
- Validation integration patterns

**Non-Scope**: Specific workflow implementations (those belong to individual skills like feature-creator, run-workflow, fl-workflow).

### Context Isolation Principle

**CRITICAL**: Multi-phase workflows risk context window pollution when all agent outputs accumulate in the orchestrator's context. This design enforces context isolation via the `context: fork` mechanism for skills and `Task()` dispatch for inline agents.

**Single Source of Truth**: `context: fork` (skill YAML frontmatter) and `Task()` (dispatch call) are the authoritative mechanisms for context isolation. Orchestrator implementations MUST use these patterns to prevent context accumulation.

---

## OrchestrationConfig

### Configuration Schema

**Responsibility**: OrchestrationConfig defines the complete workflow structure in a declarative format, serving as the single source of truth for phase sequences, dependencies, and execution parameters.

```yaml
orchestration:
  name: "workflow-name"
  version: "1.0"

  # Variable phase count support - no hardcoded limits
  phases:
    - id: "phase-1"
      name: "Initialize"
      agent: "initializer"
      model: "haiku"
      required: true

    - id: "phase-2"
      name: "Analyze"
      agent: "analyzer"
      model: "sonnet"
      required: true
      depends_on: ["phase-1"]

    - id: "phase-3"
      name: "Validate"
      agent: "validator"
      model: "opus"
      required: false
      depends_on: ["phase-2"]

  # Global settings
  global:
    max_retries: 3
    timeout_seconds: 600
    resume_enabled: true
    context_isolation: true  # Enforce Task() dispatch

  # Error handling
  error_handling:
    on_agent_failure: "retry"  # retry | skip | abort
    on_validation_failure: "abort"
    escalation_model: "opus"  # Model to use when escalating

  # Validation
  validation:
    per_phase: true  # Validate after each phase
    final: true      # Validate at workflow end
```

### Schema Validation Rules

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `name` | string | Yes | Unique identifier, alphanumeric + hyphen |
| `version` | string | Yes | Semantic versioning (major.minor) |
| `phases` | array | Yes | Length >= 1, variable phase count (no upper limit) |
| `phases[].id` | string | Yes | Unique within workflow |
| `phases[].agent` | string | Yes | Valid agent name or skill name |
| `phases[].model` | enum | Yes | `haiku` \| `sonnet` \| `opus` |
| `phases[].depends_on` | array | No | References to existing phase IDs |
| `global.max_retries` | integer | No | Default: 3, range: 0-10 |
| `global.context_isolation` | boolean | No | Default: true (recommended) |

---

## PhaseDefinition

### Phase Structure

**Responsibility**: PhaseDefinition describes the metadata, inputs, outputs, and dependencies for a single phase in the orchestration workflow.

```yaml
phase:
  id: "phase-2-investigation"
  name: "Investigation"
  description: "Investigate codebase and extract requirements"

  # Agent configuration
  agent:
    type: "general-purpose"  # general-purpose | Explore | debugger | etc.
    name: "explorer"
    model: "sonnet"
    skill_file: null  # Optional: path to .claude/skills/{skill}/SKILL.md

  # Execution parameters
  execution:
    required: true
    timeout_seconds: 300
    retry_limit: 2

  # Dependencies
  dependencies:
    depends_on: ["phase-1-initialize"]
    blocks: ["phase-3-test-creation"]

  # Input/Output contracts
  io:
    inputs:
      - type: "file"
        path: "pm/features/feature-{ID}.md"
        section: "Background"
        required: true

    outputs:
      - type: "file_section"
        path: "pm/features/feature-{ID}.md"
        section: "## Investigation Results"
        format: "markdown"
        validation: "section_exists"
```

### Phase State Tracking

Each phase execution creates state markers in the target file:

```markdown
## Investigation Results

[Phase results content]

<!-- PHASE-2-COMPLETE: 2026-01-24T12:34:56Z -->
```

**State Detection Pattern**:
```
if Grep("<!-- PHASE-{ID}-COMPLETE:", target_file):
    phase_complete = true
else:
    phase_complete = false
```

---

## AgentDispatchPattern

### Task() Invocation Template

**Responsibility**: AgentDispatchPattern provides reusable templates for dispatching agents with proper error handling and context isolation.

#### Standard Dispatch (Context Isolation)

```python
# For context: fork skills (recommended)
def dispatch_skill(skill_name: str, args: str = None):
    """
    Dispatch skill with automatic context isolation.
    Context accumulation: NONE (context: fork)
    """
    result = Skill(skill_name, args=args)
    return result

# For inline agents (context accumulation risk)
def dispatch_agent(agent_type: str, agent_name: str, model: str, prompt: str):
    """
    Dispatch agent with Task() for manual context isolation.
    Context accumulation: PREVENTED (separate task context)
    """
    task_id = Task(
        subagent_type=agent_type,
        model=model,
        prompt=f"Read .claude/agents/{agent_name}.md and execute: {prompt}"
    )

    # Wait for completion
    result = TaskOutput(task_id)
    return parse_agent_output(result)
```

#### Error Handling Template

```python
def dispatch_with_retry(config: PhaseConfig, max_retries: int = 3):
    """
    Dispatch agent with retry logic and error escalation.

    Responsibility: Handle transient failures via retry, escalate persistent
    failures to higher-capability model or user intervention.
    """
    attempt = 0
    last_error = None

    while attempt < max_retries:
        try:
            # Dispatch based on context isolation setting
            if config.agent.skill_file:
                result = dispatch_skill(config.agent.name, args=config.prompt)
            else:
                result = dispatch_agent(
                    agent_type=config.agent.type,
                    agent_name=config.agent.name,
                    model=config.agent.model,
                    prompt=config.prompt
                )

            # Validate output
            if validate_output(result, config.io.outputs):
                return {"status": "SUCCESS", "result": result}
            else:
                raise ValidationError("Output validation failed")

        except Exception as e:
            attempt += 1
            last_error = e
            record_deviation(config.phase.id, attempt, str(e))

            if attempt >= max_retries:
                # Escalate to higher model
                if config.agent.model != "opus":
                    return escalate_to_opus(config, last_error)
                else:
                    # Already using opus, escalate to user
                    return {"status": "ERROR", "error": str(last_error)}

    return {"status": "ERROR", "error": str(last_error)}
```

#### Escalation Pattern

```python
def escalate_to_opus(config: PhaseConfig, error: Exception):
    """
    Escalate failed task to Opus model.

    Responsibility: Provide higher reasoning capability for complex failures
    that simpler models cannot resolve.
    """
    escalation_config = config.copy()
    escalation_config.agent.model = "opus"
    escalation_config.prompt = f"""
    Previous attempt failed with {config.agent.model}:
    Error: {error}

    Original task: {config.prompt}

    Please analyze the error and complete the task.
    """

    return dispatch_agent(
        agent_type=escalation_config.agent.type,
        agent_name=escalation_config.agent.name,
        model="opus",
        prompt=escalation_config.prompt
    )
```

---

## ResumeLogicPattern

### State Detection Logic

**Responsibility**: ResumeLogicPattern enables workflows to resume from interruption points by detecting completed phases and selecting the next pending phase.

#### Section Presence Detection

```python
def detect_completed_phases(config: OrchestrationConfig, target_file: str):
    """
    Detect which phases have completed by checking for completion markers.

    Returns: List of completed phase IDs
    """
    completed = []

    for phase in config.phases:
        # Check for completion marker
        marker_pattern = f"<!-- PHASE-{phase.id}-COMPLETE:"

        result = Grep(
            pattern=marker_pattern,
            path=target_file,
            output_mode="files_with_matches"
        )

        if result:
            completed.append(phase.id)

    return completed
```

#### Next Phase Selection

```python
def select_next_phase(config: OrchestrationConfig, target_file: str):
    """
    Select next phase to execute based on completion state and dependencies.

    Responsibility: Determine execution order respecting dependencies and
    current completion state.
    """
    completed = detect_completed_phases(config, target_file)

    for phase in config.phases:
        # Skip if already completed
        if phase.id in completed:
            continue

        # Check dependencies
        if phase.dependencies.depends_on:
            deps_satisfied = all(
                dep_id in completed
                for dep_id in phase.dependencies.depends_on
            )
            if not deps_satisfied:
                continue  # Dependencies not met

        # Found next phase
        return phase

    # All phases completed
    return None
```

#### Resume Entry Point

```python
def resume_workflow(config: OrchestrationConfig, target_file: str):
    """
    Resume workflow from interruption point.

    Entry point for resumed execution - detects state and continues from
    next pending phase.
    """
    next_phase = select_next_phase(config, target_file)

    if next_phase is None:
        return {"status": "COMPLETE", "message": "All phases completed"}

    # Log resume point
    log_event({
        "event": "RESUME",
        "phase": next_phase.id,
        "timestamp": current_timestamp()
    })

    # Execute from this phase
    return execute_phase(next_phase, config)
```

---

## ErrorHandlingStrategy

### Failure Management

**Responsibility**: ErrorHandlingStrategy defines how the orchestrator handles failures at different levels (agent failure, validation failure, timeout) with clear escalation paths and cleanup procedures.

#### Failure Classification

| Failure Type | Detection | Response | Escalation |
|--------------|-----------|----------|------------|
| Agent Failure | Bash exit ≠ 0, Agent ERR | Retry with same model | Escalate to Opus after max_retries |
| Validation Failure | Output validation fails | Re-execute phase | Escalate to Opus after max_retries |
| Timeout | Execution exceeds timeout_seconds | Cancel and retry | User intervention after max_retries |
| Dependency Failure | depends_on phase failed | Skip or abort workflow | User decision |

#### Retry Limits

```python
# Global retry configuration
GLOBAL_MAX_RETRIES = 3

# Per-phase retry overrides
phase_config = {
    "retry_limit": 2,  # Override global for this phase
}
```

#### Error Recovery Pattern

```python
def handle_phase_failure(phase: PhaseConfig, error: Exception, attempt: int):
    """
    Handle phase failure with progressive recovery strategy.

    Responsibility: Apply appropriate recovery strategy based on failure type
    and attempt count.
    """
    # Record deviation
    record_deviation(phase.id, attempt, str(error))

    # Determine recovery strategy
    if attempt < phase.execution.retry_limit:
        # Retry with same configuration
        return {"action": "RETRY", "config": phase}

    elif phase.agent.model != "opus":
        # Escalate to higher model
        return {"action": "ESCALATE", "model": "opus"}

    else:
        # Already using opus, check if phase is required
        if phase.execution.required:
            # Required phase failed - abort workflow
            return {"action": "ABORT", "reason": "Required phase failed"}
        else:
            # Optional phase failed - skip and continue
            return {"action": "SKIP", "reason": "Optional phase failed"}
```

#### Cleanup on Failure

```python
def cleanup_on_failure(phase: PhaseConfig, target_file: str):
    """
    Clean up partial outputs when phase fails.

    Responsibility: Prevent corrupted state from failed phases by removing
    incomplete outputs.
    """
    # Remove completion marker if present
    marker_pattern = f"<!-- PHASE-{phase.id}-COMPLETE:"

    # Check if partial output exists
    if Grep(pattern=marker_pattern, path=target_file):
        # Remove the marker (indicates incomplete execution)
        Edit(
            file_path=target_file,
            old_string=marker_pattern + ".*-->",
            new_string=""
        )

    # Log cleanup action
    log_event({
        "event": "CLEANUP",
        "phase": phase.id,
        "timestamp": current_timestamp()
    })
```

---

## ModelSelectionConfig

### Per-Phase Model Assignment

**Responsibility**: ModelSelectionConfig defines criteria for selecting haiku/sonnet/opus models per phase based on task complexity, reasoning requirements, and cost optimization.

#### Model Selection Criteria

| Model | Use Case | Reasoning Depth | Speed | Cost |
|-------|----------|----------------|-------|------|
| **haiku** | Simple transformations, state updates, format conversion | Low | Fast | Low |
| **sonnet** | Code implementation, design creation, investigation | Medium | Medium | Medium |
| **opus** | Complex decisions, holistic review, conflict resolution | High | Slow | High |

#### Decision Matrix

```yaml
model_selection:
  # State management phases
  - phase_pattern: "*-initialize"
    model: "haiku"
    reason: "Simple state updates and file creation"

  - phase_pattern: "*-finalize"
    model: "haiku"
    reason: "Mechanical state transitions and logging"

  # Investigation phases
  - phase_pattern: "*-investigation"
    model: "sonnet"
    reason: "Code exploration requires medium reasoning"

  - phase_pattern: "*-analyze"
    model: "sonnet"
    reason: "Analysis requires comprehensive understanding"

  # Implementation phases
  - phase_pattern: "*-implement"
    model: "sonnet"
    reason: "Code writing requires structured thinking"

  - phase_pattern: "*-test-creation"
    model: "sonnet"
    reason: "Test design requires understanding of requirements"

  # Review and validation phases
  - phase_pattern: "*-review"
    model: "opus"
    reason: "Holistic evaluation requires highest reasoning"

  - phase_pattern: "*-validate"
    model: "opus"
    reason: "Comprehensive validation requires deep analysis"

  - phase_pattern: "*-feasibility"
    model: "opus"
    reason: "Feasibility assessment requires broad context"
```

#### Dynamic Model Selection

```python
def select_model_for_phase(phase: PhaseConfig, context: dict):
    """
    Select appropriate model based on phase characteristics and context.

    Responsibility: Optimize cost/quality tradeoff by selecting minimum
    sufficient model for each phase.
    """
    # Explicit model assignment takes precedence
    if phase.agent.model:
        return phase.agent.model

    # Check for escalation context
    if context.get("escalated"):
        return "opus"

    # Pattern-based selection
    for rule in MODEL_SELECTION_RULES:
        if matches_pattern(phase.name, rule.phase_pattern):
            return rule.model

    # Default to sonnet for unknown patterns
    return "sonnet"
```

---

## ImplementationTemplate

### Concrete Code Examples

**Responsibility**: ImplementationTemplate provides complete orchestrator skeleton code that can be adapted for specific workflows.

#### Complete Orchestrator Skeleton

```python
"""
Extensible orchestrator implementation template.

This template demonstrates the complete orchestrator pattern with:
- Configuration loading
- Phase execution with dependencies
- Resume logic
- Error handling
- Context isolation
- Validation integration
"""

from dataclasses import dataclass
from typing import List, Optional, Dict
import yaml

@dataclass
class PhaseConfig:
    id: str
    name: str
    agent: dict
    execution: dict
    dependencies: dict
    io: dict

@dataclass
class OrchestrationConfig:
    name: str
    version: str
    phases: List[PhaseConfig]
    global_config: dict
    error_handling: dict
    validation: dict

class Orchestrator:
    """
    Multi-phase workflow orchestrator with extensible configuration.

    Supports variable phase count, dynamic dependencies, resume logic,
    and context isolation.
    """

    def __init__(self, config_path: str):
        self.config = self._load_config(config_path)
        self.execution_state = {}

    def _load_config(self, path: str) -> OrchestrationConfig:
        """Load and validate orchestration configuration."""
        with open(path) as f:
            data = yaml.safe_load(f)

        # Validate schema
        self._validate_config(data)

        # Parse phases
        phases = [PhaseConfig(**p) for p in data['orchestration']['phases']]

        return OrchestrationConfig(
            name=data['orchestration']['name'],
            version=data['orchestration']['version'],
            phases=phases,
            global_config=data['orchestration']['global'],
            error_handling=data['orchestration']['error_handling'],
            validation=data['orchestration']['validation']
        )

    def execute(self, target_file: str, resume: bool = False):
        """
        Execute orchestration workflow.

        Args:
            target_file: Target file for workflow (e.g., feature-{ID}.md)
            resume: If True, resume from last completed phase
        """
        if resume:
            next_phase = select_next_phase(self.config, target_file)
            if next_phase is None:
                return {"status": "COMPLETE"}
        else:
            next_phase = self.config.phases[0]

        # Execute phases in order
        while next_phase:
            result = self._execute_phase(next_phase, target_file)

            if result["status"] == "ERROR":
                return self._handle_error(next_phase, result)

            # Mark phase complete
            self._mark_complete(next_phase, target_file)

            # Select next phase
            next_phase = self._select_next(next_phase)

        return {"status": "SUCCESS"}

    def _execute_phase(self, phase: PhaseConfig, target_file: str):
        """Execute single phase with retry logic."""
        max_retries = phase.execution.get('retry_limit',
                                         self.config.global_config['max_retries'])

        for attempt in range(max_retries):
            try:
                # Dispatch agent
                result = dispatch_with_retry(phase, max_retries)

                # Validate output
                if self.config.validation['per_phase']:
                    validation = self._validate_phase_output(phase, result)
                    if not validation['valid']:
                        raise ValidationError(validation['errors'])

                return result

            except Exception as e:
                if attempt == max_retries - 1:
                    return {"status": "ERROR", "error": str(e)}

                record_deviation(phase.id, attempt + 1, str(e))

        return {"status": "ERROR", "error": "Max retries exceeded"}

    def _mark_complete(self, phase: PhaseConfig, target_file: str):
        """Mark phase as complete in target file."""
        marker = f"<!-- PHASE-{phase.id}-COMPLETE: {current_timestamp()} -->"

        # Append marker to appropriate section
        # Implementation depends on target file structure
        pass

    def _select_next(self, current_phase: PhaseConfig) -> Optional[PhaseConfig]:
        """Select next phase based on dependencies."""
        current_idx = self.config.phases.index(current_phase)

        # Try subsequent phases
        for phase in self.config.phases[current_idx + 1:]:
            # Check dependencies
            if self._dependencies_satisfied(phase):
                return phase

        return None

    def _dependencies_satisfied(self, phase: PhaseConfig) -> bool:
        """Check if all dependencies for phase are satisfied."""
        if not phase.dependencies.get('depends_on'):
            return True

        for dep_id in phase.dependencies['depends_on']:
            if dep_id not in self.execution_state:
                return False

        return True
```

#### Phase Execution Template

```python
def execute_phase_with_progressive_disclosure(phase_id: str, skill_path: str):
    """
    Execute phase using progressive disclosure pattern.

    This template shows how to chain phase files for progressive disclosure,
    following the run-workflow and fl-workflow patterns.
    """
    # Phase entry point
    phase_file = f"{skill_path}/PHASE-{phase_id}.md"

    # Read phase instructions
    content = Read(phase_file)

    # Execute phase logic
    # (phase file contains explicit Read() instructions for next phase)

    # Phase completion is signaled by:
    # 1. Completion marker in target file
    # 2. Explicit routing to next phase via Read() call

    return {"status": "COMPLETE", "next_phase": phase_id + 1}
```

---

## ValidationPatterns

### Quality Assurance Integration

**Responsibility**: ValidationPatterns define how to integrate quality checks at phase boundaries and workflow completion to ensure output correctness.

#### Per-Phase Validation

```python
def validate_phase_output(phase: PhaseConfig, result: dict) -> dict:
    """
    Validate phase output against expected contract.

    Returns: {"valid": bool, "errors": List[str]}
    """
    errors = []

    # Check required outputs
    for output_spec in phase.io.outputs:
        if output_spec.required:
            # File existence check
            if output_spec.type == "file":
                if not Glob(output_spec.path):
                    errors.append(f"Required file not found: {output_spec.path}")

            # Section existence check
            elif output_spec.type == "file_section":
                pattern = f"## {output_spec.section}"
                if not Grep(pattern=pattern, path=output_spec.path,
                           output_mode="files_with_matches"):
                    errors.append(f"Required section not found: {output_spec.section}")

            # Custom validation
            elif output_spec.validation:
                validation_result = run_validation(output_spec.validation, result)
                if not validation_result:
                    errors.append(f"Validation failed: {output_spec.validation}")

    return {
        "valid": len(errors) == 0,
        "errors": errors
    }
```

#### Overall Workflow Validation

```python
def validate_workflow_completion(config: OrchestrationConfig, target_file: str):
    """
    Validate complete workflow output.

    Responsibility: Ensure all required phases completed successfully and
    final output meets quality standards.
    """
    errors = []

    # Check all required phases completed
    completed = detect_completed_phases(config, target_file)

    required_phases = [p.id for p in config.phases if p.execution.required]
    missing_phases = set(required_phases) - set(completed)

    if missing_phases:
        errors.append(f"Required phases not completed: {missing_phases}")

    # Run final validation if configured
    if config.validation['final']:
        # Check for technical debt markers
        DEBT_MARKERS = ["TO" + "DO", "FIX" + "ME", "HA" + "CK"]  # Split to avoid self-match
        debt_check = Grep(
            pattern="|".join(DEBT_MARKERS),
            path=target_file,
            output_mode="count"
        )

        if debt_check > 0:
            errors.append(f"Technical debt markers found: {debt_check}")

        # Check for broken links (reference-checker pattern)
        link_validation = validate_links(target_file)
        if not link_validation['valid']:
            errors.extend(link_validation['errors'])

    return {
        "valid": len(errors) == 0,
        "errors": errors,
        "completed_phases": len(completed),
        "total_phases": len(config.phases)
    }
```

#### Validation Agent Dispatch

```python
def dispatch_validation_agent(phase: PhaseConfig, target_file: str):
    """
    Dispatch validation subagent for phase output review.

    This pattern follows fl-workflow's validation approach.
    """
    # Prepare validation prompt
    validation_prompt = f"""
    Validate the output of {phase.name} phase.

    Target file: {target_file}
    Expected outputs: {phase.io.outputs}

    Return JSON only:
    {{"status": "OK"}} or {{"status": "NEEDS_REVISION", "issues": [...]}}
    """

    # Dispatch validator
    result = Task(
        subagent_type="general-purpose",
        model="opus",  # Use opus for validation
        prompt=validation_prompt
    )

    # Parse JSON response
    output = TaskOutput(result)
    validation = parse_json_output(output)

    return validation
```

---

## ContextIsolationDesign

### Context Window Management

**Responsibility**: ContextIsolationDesign provides decision criteria for choosing between Task() dispatch and inline execution to prevent context window pollution in multi-phase workflows.

#### Decision Matrix

| Scenario | Pattern | Context Impact | When to Use |
|----------|---------|----------------|-------------|
| Skill with `context: fork` | `Skill(name)` | NONE (isolated) | Always preferred for complex workflows |
| Agent with long output | `Task()` dispatch | LOW (separate context) | Multi-phase workflows, long-running agents |
| Agent with short output | Inline execution | HIGH (accumulates) | Single-phase tasks, simple transformations |
| Multiple sequential agents | `Task()` dispatch | LOW (separate contexts) | Orchestration workflows |
| One-off utility agent | Inline execution | MEDIUM (single accumulation) | Simple tasks with short output |

#### Task() Dispatch Pattern (Recommended)

```python
def dispatch_isolated_agent(agent_name: str, model: str, prompt: str):
    """
    Dispatch agent with context isolation via Task().

    Responsibility: Prevent context window pollution by executing agent
    in separate task context.

    Context Impact: LOW - agent's full transcript is not loaded into
    orchestrator's context. Only final result is returned.
    """
    task_id = Task(
        subagent_type="general-purpose",
        model=model,
        prompt=f"Read .claude/agents/{agent_name}.md\n\n{prompt}"
    )

    # For long-running agents (e.g., kojo-writer), use file polling
    # instead of TaskOutput to avoid loading 100K+ token transcripts
    if is_long_running_agent(agent_name):
        return poll_for_completion(task_id, expected_output_file)
    else:
        # Short output agents - safe to use TaskOutput
        return TaskOutput(task_id)
```

#### Skill() Dispatch Pattern (Preferred)

```python
def dispatch_isolated_skill(skill_name: str, args: str = None):
    """
    Dispatch skill with automatic context isolation via context: fork.

    Responsibility: Leverage context: fork mechanism for zero context
    accumulation. This is the PREFERRED pattern for multi-phase workflows.

    Context Impact: NONE - skill executes in completely separate context,
    only SUCCESS/ERROR/BLOCKED status returned to orchestrator.
    """
    result = Skill(skill_name, args=args)

    # Result is lightweight status message, not full transcript
    return parse_skill_result(result)
```

#### Inline Execution Pattern (Use Sparingly)

```python
def execute_inline_agent(agent_name: str, model: str, prompt: str):
    """
    Execute agent inline without context isolation.

    WARNING: Context accumulation risk. Only use for simple tasks with
    short, predictable output.

    Context Impact: HIGH - full agent transcript accumulates in orchestrator
    context. Multiple inline agents can quickly exhaust context window.
    """
    # Read agent definition
    agent_def = Read(f".claude/agents/{agent_name}.md")

    # Execute agent logic inline
    # (agent's thinking and output all accumulate in context)

    return result
```

#### File Polling Pattern (Long-Running Agents)

```python
def poll_for_completion(task_id: str, expected_output_file: str,
                       timeout_seconds: int = 600):
    """
    Poll for task completion via file system instead of TaskOutput.

    Responsibility: Avoid loading 100K+ token transcripts from long-running
    agents (e.g., kojo-writer) by detecting completion via file presence.

    Context Impact: MINIMAL - only final status checked, not full transcript.
    """
    import time

    start_time = time.time()

    while time.time() - start_time < timeout_seconds:
        # Check for completion marker or output file
        if Glob(expected_output_file):
            # File created - task completed
            return {"status": "SUCCESS", "output_file": expected_output_file}

        # Wait before next check
        time.sleep(5)

    # Timeout
    return {"status": "TIMEOUT", "task_id": task_id}
```

#### Context Budget Guidelines

```python
# Context window budgets for different models
CONTEXT_BUDGETS = {
    "haiku": 200_000,
    "sonnet": 200_000,
    "opus": 200_000,
}

# Estimated token costs per pattern
PATTERN_COSTS = {
    "Skill() dispatch": 100,        # Status message only
    "Task() dispatch": 1_000,       # Result summary
    "TaskOutput()": 50_000,         # Full transcript (varies widely)
    "Inline execution": 100_000,    # Full accumulation
}

def estimate_context_usage(orchestration: OrchestrationConfig):
    """
    Estimate context window usage for orchestration workflow.

    Responsibility: Predict context usage to prevent mid-workflow failures
    due to context window exhaustion.
    """
    total_tokens = 0

    for phase in orchestration.phases:
        if phase.agent.skill_file:
            # Skill dispatch - minimal cost
            total_tokens += PATTERN_COSTS["Skill() dispatch"]
        else:
            # Agent dispatch - check if Task() used
            if orchestration.global_config.get('context_isolation'):
                total_tokens += PATTERN_COSTS["Task() dispatch"]
            else:
                # Inline execution - high cost
                total_tokens += PATTERN_COSTS["Inline execution"]

    budget = CONTEXT_BUDGETS[orchestration.global_config.get('model', 'sonnet')]

    return {
        "estimated_tokens": total_tokens,
        "budget": budget,
        "utilization": total_tokens / budget,
        "safe": total_tokens < budget * 0.8  # 80% threshold
    }
```

---

## Feature610Example

### Practical Implementation

**Responsibility**: Feature610Example demonstrates how the Feature Creator's specific 5-phase workflow would be configured using the extensible orchestrator patterns defined in this design.

#### F610 Workflow Configuration

```yaml
# .claude/skills/feature-creator/orchestration.yaml
orchestration:
  name: "feature-creator"
  version: "1.0"

  # F610: 5-phase workflow for feature creation
  # Demonstrates variable phase count (5 phases, not hardcoded limit)
  phases:
    - id: "phase-1-philosophy"
      name: "Philosophy Definition"
      agent:
        type: "general-purpose"
        name: "philosophy-definer"
        model: "opus"
        skill_file: null
      execution:
        required: true
        timeout_seconds: 300
        retry_limit: 2
      dependencies:
        depends_on: []
        blocks: ["phase-2-investigation"]
      io:
        inputs:
          - type: "context"
            source: "user_requirements"
            required: true
        outputs:
          - type: "file_section"
            path: "pm/features/feature-{ID}.md"
            section: "## Background\n### Philosophy"
            required: true
            validation: "section_exists"

    - id: "phase-2-investigation"
      name: "Technical Investigation"
      agent:
        type: "general-purpose"
        name: "tech-investigator"
        model: "sonnet"
        skill_file: null
      execution:
        required: true
        timeout_seconds: 600
        retry_limit: 3
      dependencies:
        depends_on: ["phase-1-philosophy"]
        blocks: ["phase-3-ac-design"]
      io:
        inputs:
          - type: "file_section"
            path: "pm/features/feature-{ID}.md"
            section: "## Background\n### Philosophy"
            required: true
        outputs:
          - type: "file_section"
            path: "pm/features/feature-{ID}.md"
            section: "## Background\n### Problem"
            required: true
          - type: "file_section"
            path: "pm/features/feature-{ID}.md"
            section: "## Background\n### Goal"
            required: true

    - id: "phase-3-ac-design"
      name: "AC Design"
      agent:
        type: "general-purpose"
        name: "ac-designer"
        model: "opus"
        skill_file: null
      execution:
        required: true
        timeout_seconds: 600
        retry_limit: 2
      dependencies:
        depends_on: ["phase-2-investigation"]
        blocks: ["phase-4-wbs"]
      io:
        inputs:
          - type: "file_section"
            path: "pm/features/feature-{ID}.md"
            section: "## Background"
            required: true
        outputs:
          - type: "file_section"
            path: "pm/features/feature-{ID}.md"
            section: "## Acceptance Criteria"
            required: true
            validation: "ac_table_format"

    - id: "phase-4-wbs"
      name: "WBS Generation"
      agent:
        type: "general-purpose"
        name: "wbs-generator"
        model: "sonnet"
        skill_file: null
      execution:
        required: true
        timeout_seconds: 300
        retry_limit: 2
      dependencies:
        depends_on: ["phase-3-ac-design"]
        blocks: ["phase-5-validation"]
      io:
        inputs:
          - type: "file_section"
            path: "pm/features/feature-{ID}.md"
            section: "## Acceptance Criteria"
            required: true
        outputs:
          - type: "file_section"
            path: "pm/features/feature-{ID}.md"
            section: "## Tasks"
            required: true
            validation: "task_table_format"

    - id: "phase-5-validation"
      name: "Feature Validation"
      agent:
        type: "general-purpose"
        name: "feature-validator"
        model: "opus"
        skill_file: null
      execution:
        required: true
        timeout_seconds: 300
        retry_limit: 2
      dependencies:
        depends_on: ["phase-4-wbs"]
        blocks: []
      io:
        inputs:
          - type: "file"
            path: "pm/features/feature-{ID}.md"
            required: true
        outputs:
          - type: "validation_result"
            format: "json"
            validation: "zero_issues"

  global:
    max_retries: 3
    timeout_seconds: 600
    resume_enabled: true
    context_isolation: true

  error_handling:
    on_agent_failure: "retry"
    on_validation_failure: "abort"
    escalation_model: "opus"

  validation:
    per_phase: true
    final: true
```

#### F610 Orchestrator Implementation

```python
"""
Feature Creator orchestrator implementation using extensible patterns.

This example shows how F610 implements the 5-phase workflow using the
orchestrator design patterns.
"""

class FeatureCreatorOrchestrator(Orchestrator):
    """
    Feature Creator specific orchestrator.

    Implements 5-phase workflow: Philosophy → Investigation → AC Design
    → WBS → Validation
    """

    def __init__(self):
        # Load F610-specific configuration
        super().__init__(".claude/skills/feature-creator/orchestration.yaml")

    def create_feature(self, feature_id: int, feature_type: str,
                       requirements: str):
        """
        Execute feature creation workflow.

        Args:
            feature_id: Feature ID number
            feature_type: kojo | erb | engine | infra | research
            requirements: User-provided requirements summary
        """
        target_file = f"pm/features/feature-{feature_id}.md"

        # Initialize feature file
        self._initialize_feature_file(feature_id, feature_type)

        # Execute 5-phase workflow
        context = {
            "feature_id": feature_id,
            "feature_type": feature_type,
            "user_requirements": requirements
        }

        result = self.execute(target_file, resume=False)

        if result["status"] == "SUCCESS":
            # Register in index
            self._register_in_index(feature_id, target_file)

            # Determine initial status (check predecessors)
            status = self._determine_status(target_file)

            return {
                "status": "SUCCESS",
                "feature_id": feature_id,
                "file": target_file,
                "initial_status": status
            }
        else:
            return result

    def _initialize_feature_file(self, feature_id: int, feature_type: str):
        """Create initial feature file structure."""
        template = Read("pm/reference/feature-template.md")

        # Substitute placeholders
        content = template.replace("{ID}", str(feature_id))
        content = content.replace("{Type}", feature_type)

        Write(f"pm/features/feature-{feature_id}.md", content)

    def _determine_status(self, target_file: str):
        """Determine initial status based on dependencies."""
        # Check Dependencies table for Predecessors
        deps = self._parse_dependencies(target_file)

        if not deps.get("Predecessor"):
            return "[PROPOSED]"

        # Check predecessor statuses
        for pred_id in deps["Predecessor"]:
            pred_file = f"pm/features/feature-{pred_id}.md"
            pred_status = self._get_feature_status(pred_file)

            if pred_status != "[DONE]":
                return "[BLOCKED]"

        return "[PROPOSED]"
```

#### Progressive Disclosure Integration

```python
"""
F610 can optionally use progressive disclosure by splitting phases into
separate files, following the run-workflow and fl-workflow patterns.
"""

# .claude/skills/feature-creator/SKILL.md
"""
---
name: feature-creator
description: Create feature files with embedded quality checklist
context: fork
agent: general-purpose
model: sonnet
---

# Feature Creator Skill

## Overview

| Phase | Name | File |
|:-----:|------|------|
| 1 | Philosophy Definition | Read(.claude/skills/feature-creator/PHASE-1.md) |
| 2 | Technical Investigation | Read(.claude/skills/feature-creator/PHASE-2.md) |
| 3 | AC Design | Read(.claude/skills/feature-creator/PHASE-3.md) |
| 4 | WBS Generation | Read(.claude/skills/feature-creator/PHASE-4.md) |
| 5 | Feature Validation | Read(.claude/skills/feature-creator/PHASE-5.md) |

## Start

Read(.claude/skills/feature-creator/PHASE-1.md)
"""

# Each phase file chains to next via Read() instruction
# Example: .claude/skills/feature-creator/PHASE-1.md
"""
# Phase 1: Philosophy Definition

## Input
- User requirements
- Feature type

## Procedure
1. Read type-specific guide (.claude/skills/feature-quality/{TYPE}.md)
2. Define Philosophy section
3. Write to feature-{ID}.md
4. Mark complete: <!-- PHASE-1-COMPLETE: {timestamp} -->

## Next Phase
Read(.claude/skills/feature-creator/PHASE-2.md)
"""
```

#### Extensibility Demonstration

```python
"""
F610 example demonstrates key extensibility features:

1. Variable phase count: 5 phases (not hardcoded limit)
   - Design supports 1-10+ phases via configuration
   - F610 chose 5 based on workflow requirements

2. Model flexibility: Uses all three models
   - Phase 1 (Philosophy): opus (complex reasoning)
   - Phase 2 (Investigation): sonnet (code exploration)
   - Phase 3 (AC Design): opus (requirement analysis)
   - Phase 4 (WBS): sonnet (task decomposition)
   - Phase 5 (Validation): opus (holistic review)

3. Resume capability: Can resume from any phase
   - Completion markers enable state detection
   - Dependencies ensure correct execution order

4. Error resilience: Retry + escalation strategy
   - Per-phase retry limits (2-3 attempts)
   - Escalate to opus on persistent failures
   - Validation at phase boundaries

5. Context isolation: context: fork for skill
   - Zero context accumulation in orchestrator
   - Each phase execution isolated
   - Prevents context window exhaustion

This configuration serves as a reference for other multi-phase workflows.
"""
```

---

## Integration with Existing Workflows

### run-workflow Integration

The run-workflow (9 phases) can be refactored to use these orchestrator patterns:

```yaml
# .claude/skills/run-workflow/orchestration.yaml (example)
orchestration:
  name: "run-workflow"
  version: "2.0"

  phases:
    - id: "phase-1-initialize"
      agent: {name: "initializer", model: "haiku"}
      # ... configuration following OrchestrationConfig schema

    # ... phases 2-9 with same pattern
```

### fl-workflow Integration

The fl-workflow (8 phases + loop control) demonstrates advanced routing:

```yaml
# .claude/skills/fl-workflow/orchestration.yaml (example)
orchestration:
  name: "fl-workflow"
  version: "2.0"

  global:
    loop_enabled: true
    max_iterations: 10
    loop_routing:
      - condition: "applied_fixes > 0"
        action: "goto_phase_1"
      - condition: "iteration >= max_iterations"
        action: "forward_only_mode"

  # ... phase definitions with routing table integration
```

---

## Single Source of Truth Enforcement

**CRITICAL**: This design document is the single source of truth for orchestrator patterns. All multi-phase workflow implementations MUST reference this design.

### SSOT Hierarchy

| Level | Document | Scope | Authority |
|-------|----------|-------|-----------|
| 1 | extensible-orchestrator.md (this design) | Abstract patterns | Highest - defines patterns |
| 2 | Skill SKILL.md (e.g., feature-creator) | Concrete workflow | Medium - implements patterns |
| 3 | Phase files (PHASE-N.md) | Phase details | Lowest - executes phase |

### Update Protocol

When modifying orchestrator patterns:

1. **Design changes**: Update this document FIRST
2. **Skill changes**: Update skill SKILL.md to conform to design
3. **Phase changes**: Update phase files to match skill structure

**Forbidden**: Modifying skills/phases without updating this design creates pattern drift and violates single source of truth.

### Pattern Compliance Verification

```python
def verify_pattern_compliance(skill_config: dict):
    """
    Verify skill configuration complies with orchestrator design patterns.

    Checks:
    - Configuration schema matches OrchestrationConfig
    - Phase definitions match PhaseDefinition
    - Error handling follows ErrorHandlingStrategy
    - Context isolation patterns used correctly
    """
    errors = []

    # Schema validation
    required_fields = ['name', 'version', 'phases', 'global']
    for field in required_fields:
        if field not in skill_config.get('orchestration', {}):
            errors.append(f"Missing required field: {field}")

    # Phase structure validation
    for phase in skill_config.get('orchestration', {}).get('phases', []):
        phase_errors = validate_phase_structure(phase)
        errors.extend(phase_errors)

    # Context isolation check
    if not skill_config.get('orchestration', {}).get('global', {}).get('context_isolation'):
        errors.append("Warning: context_isolation not enabled (recommended: true)")

    return {
        "compliant": len(errors) == 0,
        "errors": errors
    }
```

---

## Known Limitations and Future Extensions

### Current Limitations

1. **Parallel phase execution**: Current design supports sequential execution only. Parallel execution within a workflow requires additional dependency resolution logic.

2. **Dynamic phase generation**: Phases must be defined in configuration before workflow execution. Runtime phase generation not supported.

3. **Cross-workflow dependencies**: Dependencies between different orchestrated workflows not addressed.

### Future Extension Points

These extension points are documented for future enhancement but are NOT in scope for current design:

1. **Parallel Execution Support**: Add `parallel_group` field to PhaseDefinition to enable concurrent execution of independent phases.

2. **Conditional Phases**: Add `condition` field to PhaseDefinition for runtime phase skipping based on earlier results.

3. **Workflow Composition**: Define patterns for chaining multiple orchestrated workflows with dependency management.

4. **Live Monitoring**: Add event streaming for real-time workflow progress monitoring.

---

## References

- [Feature 612: Extensible Orchestrator Design](../feature-612.md)
- [Feature 610: Feature Creator 5-Phase Orchestrator Redesign](../feature-610.md)
- [run-workflow Skill](.claude/skills/run-workflow/SKILL.md)
- [fl-workflow Skill](.claude/skills/fl-workflow/SKILL.md)
- [feature-creator Skill](.claude/skills/feature-creator/SKILL.md)
- [Agent Registry](../../../.claude/reference/agent-registry.md)

---

## Revision History

| Date | Version | Changes |
|------|---------|---------|
| 2026-01-24 | 1.0 | Initial design creation |
