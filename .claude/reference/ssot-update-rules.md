# SSOT Update Rules

> **Purpose**: Define which SSOT documents must be updated when specific files are changed.
>
> **Last Updated**: 2026-01-08

---

## Update Matrix

| Change Target | Required SSOT Update | Section |
|---------------|---------------------|---------|
| `src/Era.Core/Types/*.cs` (new) | `.claude/skills/engine-dev/PATTERNS.md` | Strongly Typed Variable Indices |
| `src/Era.Core/Domain/**/*.cs` (new) | `.claude/skills/engine-dev/PATTERNS.md` | Phase 13 DDD Pattern |
| `src/Era.Core/Interfaces/IVariableStore.cs` (new methods) | `.claude/skills/engine-dev/PATTERNS.md` | Variable Store Interface |
| `src/Era.Core/Interfaces/*.cs` (new interface) | `.claude/skills/engine-dev/INTERFACES.md` | Core Interfaces |
| `engine/` ERB command | `.claude/skills/erb-syntax/SKILL.md` | Commands section |
| `engine/` system function | `.claude/skills/engine-dev/SKILL.md` | Extension Points |
| `Game/ERB/` new pattern | `.claude/skills/erb-syntax/SKILL.md` | Relevant pattern section |
| `Game/ERB/口上/` dialogue pattern | `.claude/skills/kojo-writing/SKILL.md` | Relevant section |
| Test pattern (new) | `.claude/skills/testing/SKILL.md` | Test Types or Patterns |
| `.claude/commands/*.md` | `CLAUDE.md` | Slash Commands table |
| `.claude/agents/*.md` | `.claude/reference/agent-registry.md` | Agent Table |
| `tools/` new tool | `CLAUDE.md` | Project Structure |

---

## Update Format

### For Index Types (src/Era.Core/Types)

Add to PATTERNS.md "Strongly Typed Variable Indices" section:
```csharp
{TypeName} {varName} = {TypeName}.{WellKnownValue};  // {Array} array index (2D) - F{ID}
```

### For IVariableStore Methods

Add to PATTERNS.md "Variable Store Interface" section:
```csharp
// F{ID}: {Description}
Result<int> {varName} = store.Get{Array}(character, {IndexType}.{Value});
```

### For Slash Commands

Add to CLAUDE.md table:
```markdown
| `/{command}` | {Description} |
```

---

## Verification

### Pre-Commit Check

Before finalizing a feature, verify:

1. **New types created?** → Check engine-dev PATTERNS.md updated
2. **New interface methods?** → Check engine-dev INTERFACES.md or PATTERNS.md updated
3. **New slash command?** → Check CLAUDE.md table updated
4. **New agent?** → Check `.claude/reference/agent-registry.md` Agent Table updated

### doc-check Mode

feature-reviewer doc-check should verify:
1. Changed files are not referenced with stale information
2. New public APIs are documented in appropriate SKILL

---

## Rationale

- **Discoverability**: New developers/agents can find available APIs
- **Long-term maintainability**: No hidden "etc." accumulating undocumented items
- **Zero technical debt**: Documentation updated synchronously with code
- **SOLID compliance**: Interface growth is visible and trackable
