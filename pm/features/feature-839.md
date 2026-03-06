# Feature 839: Enable EnforceCodeStyleInBuild in core repo (Symmetric with F837)

## Status: [DRAFT]

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
Build-time enforcement is the SSOT for code style compliance across all repositories. The core repo must have identical enforcement to devkit.

### Problem (Current Issue)
F837 enabled `EnforceCodeStyleInBuild` and added `dotnet_code_quality_unused_parameters` in the devkit repo. The core repo (`C:\Era\core`) shares an identical `.editorconfig` but lacks the same `Directory.Build.props` property. This creates an enforcement gap where devkit enforces IDE-prefix rules at build time but core does not.

### Goal (What to Achieve)
Apply symmetric changes to the core repo:
1. Add `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` to `C:\Era\core\Directory.Build.props`
2. Add `dotnet_code_quality_unused_parameters = all:suggestion` to `C:\Era\core\.editorconfig`
3. Verify build succeeds (existing IDE1006 pragma in `LegacyYamlDialogueLoader.cs:202,209` must be preserved)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F837 | [DONE] | Devkit-side EnforceCodeStyleInBuild enablement |

---

## Links

[Predecessor: F837](feature-837.md) - Enable EnforceCodeStyleInBuild for IDE-prefix Rule Enforcement (devkit)
[Related: F831](feature-831.md) - Roslynator Analyzers Investigation (parent research)
