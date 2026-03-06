# Feature 492: Deferred Task Phase Reference Audit

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

## Type: infra

## Created: 2026-01-14

---

## Summary

Audit all TODO/stub/out-of-scope markers in Era.Core and add explicit Phase references per Deferred Task Protocol.

**Output**: Updated code comments and feature files with `→ Phase N` references.

**Volume**: 4 orphan TODOs + 4 misleading patterns (1 acceptable + 3 out-of-scope) across 3 files (excluding COM command stubs covered by Phase 12).

---

## Background

### Philosophy (Mid-term Vision)

**Track What You Skip** - Defer is OK, forget is not. All deferred tasks must have concrete tracking destinations (CLAUDE.md Design Principles).

**Full Migration** - This migration is executed as a complete replacement. All game logic must be 100% C# before release (architecture.md Migration Strategy).

### Problem (Current Issue)

Root cause analysis (F490 investigation) revealed systemic documentation gap:

| Issue | Example | Impact |
|-------|---------|--------|
| TODO without Phase | `// TODO: when available` | Unknown completion timing |
| "acceptable" language | `acceptable for Phase 3` | Misread as "no action needed" |
| "out-of-scope" without target | `out-of-scope for Phase 3` | Unknown destination Phase |
| Missing SSOT link | Code ↔ architecture.md disconnected | Claude misinterprets as permanent |

**Scope**: Only orphan TODO markers and misleading "acceptable" language. COM command stubs (e.g., `executed (stub)`) are intentionally out-of-scope as they are expected Phase 12 behavior.

**Affected Files**:
- `Era.Core/Common/ClothingSystem.cs` - 1 "acceptable" pattern to reword + 3 "out-of-scope" patterns to add Phase reference
- `Era.Core/Common/GameInitialization.cs` - 3 orphan TODOs without Phase reference
- `Era.Core/Training/OrgasmProcessor.cs` - 1 orphan TODO without Phase reference

**Excluded (Phase 12 COM Implementation stubs)**:
- `Era.Core/Commands/Com/*` - 150+ files with expected stub behavior for Phase 12

### Goal (What to Achieve)

1. All TODO/stub comments have explicit `→ Phase N` reference
2. All "acceptable for Phase X" reworded to "deferred to Phase Y"
3. All "out-of-scope" have target Phase documented
4. F369 残課題 section added with Phase 22 reference

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | No orphan TODOs | code | Grep(Era.Core) | not_contains | `TODO.*when.*available` | [x] |
| 2 | No misleading "acceptable" | code | Grep(Era.Core) | not_contains | `acceptable for Phase` | [x] |
| 3 | All in-scope TODOs have Phase ref | manual | inspection | - | All TODO patterns contain "→ Phase" | [x] |
| 4 | All out-of-scope have Phase ref | code | Grep(Era.Core) | count_equals | `out-of-scope.*→ Phase` = 3 | [x] |
| 5 | F369 has 残課題 section | file | Grep(feature-369.md) | contains | `## 残課題` | [x] |
| 6 | Build succeeds | build | dotnet build | succeeds | exit code 0 | [x] |

### AC Details

**AC#1**: No orphan TODOs without Phase reference
- Test: `Grep pattern="TODO.*when.*available" path="Era.Core"`
- Expected: 0 matches (all TODOs should have `→ Phase N`)

**AC#2**: No misleading "acceptable for Phase" language
- Test: `Grep pattern="acceptable for Phase" path="Era.Core"`
- Expected: 0 matches (reworded to "deferred to Phase")

**AC#3**: All in-scope TODOs have Phase reference (excluding COM stubs)
- Test: Manual inspection of TODO patterns in Era.Core (excluding Commands/Com/**)
- Expected: All TODO patterns contain "→ Phase"

**AC#4**: All out-of-scope patterns have Phase reference
- Test: `Grep pattern="out-of-scope.*→ Phase" path="Era.Core" output_mode="count"`
- Expected: Count = 3 (all 3 out-of-scope comments have Phase destination)

**AC#5**: F369 残課題 section exists
- Test: `Grep pattern="## 残課題" path="Game/agents/feature-369.md"`
- Expected: Section exists with Phase 22 reference

**AC#6**: Build succeeds (comment changes don't break build)
- Test: `dotnet build Era.Core`
- Expected: Exit code 0

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Update Era.Core TODO/stub/out-of-scope comments with Phase references | [x] |
| 2 | 5 | Add 残課題 section to F369 with Phase 22 reference | [x] |
| 3 | 6 | Verify build succeeds | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Comment Format Standard

**Before**:
```csharp
// TODO: Replace with GlobalStatic accessors when available
```

**After**:
```csharp
// TODO: Replace with GlobalStatic accessors → Phase 22 (State Systems)
```

**Before**:
```csharp
/// - C# implementation does NOT set these flags (acceptable for Phase 3)
```

**After**:
```csharp
/// - C# implementation does NOT set these flags (→ Phase 22: State Systems for full migration)
```

### Phase Reference Map

| Current Pattern | Target Phase | Phase Name |
|-----------------|:------------:|------------|
| GlobalStatic accessors | 22 | State Systems |
| MARK access | 12 | Training Integration |
| ClothingSystem stubs | 22 | State Systems |
| NTR mark checking | 20-21 | NTR Mark Integration (per architecture.md Phase 24 directive) |

### F369 残課題 Section

Add to feature-369.md (placement: **after Links section, before Execution Log** per feature-template.md):

```markdown
---

## 残課題 (Deferred Tasks)

| Task | Reason | Target Phase | Tracking |
|------|--------|:------------:|:--------:|
| 今日のぱんつ full implementation | Complex 180-line randomizer | Phase 22 | architecture.md Phase 22 Tasks |
| 今日のぱんつADULT full implementation | Adult underwear selection | Phase 22 | architecture.md Phase 22 Tasks |
| CLOTHES_ACCESSORY full implementation | Accessory flags (結婚指輪/チョーカー/首輪) | Phase 22 | architecture.md Phase 22 Tasks |
| CLOTHES_Preset_NIGHTWEAR full implementation | Character nightwear presets | Phase 22 | architecture.md Phase 22 Tasks |
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Trigger | F490 | Root cause analysis identified this gap |
| Related | F369 | ClothingSystem stubs need 残課題 section |
| Reference | architecture.md | Phase definitions (SSOT) |

---

## Links

- [feature-490.md](feature-490.md) - Root cause analysis trigger
- [feature-369.md](feature-369.md) - ClothingSystem (needs 残課題)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase definitions
- [CLAUDE.md](../../CLAUDE.md) - Deferred Task Protocol

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-14 FL iter2**: [resolved] Phase2-Validate - AC Details AC#3: Fixed with two-step ripgrep command
- **2026-01-14 FL iter3**: [resolved] Phase2-Validate - NTR mark checking Phase: Changed to Phase 20-21 per architecture.md Phase 24 directive
- **2026-01-14 FL iter3**: [resolved] Phase2-Validate - Task#1 covers AC#1,2,3,4: User approved as atomic operation exception
- **2026-01-14 FL iter4**: [resolved] Phase2-Validate - NTR mark checking: Fixed to Phase 20-21 per architecture.md Phase 24 section line 4885
- **2026-01-14 FL iter4**: [resolved] Phase2-Validate - Implementation Contract 'After' example: Updated to preserve context while adding Phase reference
- **2026-01-14 FL iter6**: [resolved] Phase2-Validate - SCOMF stubs: Removed from Phase Reference Map as not in affected files scope
- **2026-01-14 FL iter7**: [resolved] Phase2-Validate - Implementation Contract 'After' example: Fixed to use consistent '→' format
- **2026-01-14 FL iter8**: [resolved] Phase2-Validate - AC#3 verification method: Changed to Type(manual) for clarity
- **2026-01-14 FL iter9**: [resolved] Phase2-Validate - Added AC#4 for out-of-scope patterns verification
- **2026-01-14 FL iter9**: [resolved] Phase2-Validate - F369 残課題 section placement: User approved explicit placement instruction
- **2026-01-14 FL iter10**: [resolved] Phase2-Validate - AC#4 two-step verification: User approved for automation

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-14 | create | opus | Created from F490 root cause analysis | PROPOSED |
| 2026-01-14 14:46 | START | implementer | Task 1 | - |
| 2026-01-14 14:46 | END | implementer | Task 1 | SUCCESS |
| 2026-01-14 14:47 | START | implementer | Task 2 | - |
| 2026-01-14 14:47 | END | implementer | Task 2 | SUCCESS |
