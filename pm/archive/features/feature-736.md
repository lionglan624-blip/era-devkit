# Feature 736: Non-Dashboard Recovery (.claude/, KojoComparer, misc)

## Status: [CANCELLED]

### Cancellation Reason

完全復旧断念。手動復旧を開始したため。

## Type: infra

## Background

### Philosophy (Mid-term Vision)
All lost uncommitted changes must be recovered comprehensively. Dashboard recovery (F734/F735) covers only part of the damage from the `git checkout -- .` incident.

### Problem (Current Issue)
Beyond dashboard files, approximately 50 additional tracked files lost uncommitted changes:

**HIGH priority (affects Claude Code behavior):**
- `.claude/agents/` (12 files): ac-tester, ac-validator, com-auditor, doc-reviewer, eratw-reader, feasibility-checker, feature-reviewer, feature-validator, planning-validator, reference-checker, tech-investigator, wbs-generator
- `.claude/commands/` (7 files): audit, complete-feature, fc, kojo-init, next, plan, sync-deps
- `.claude/skills/` (~20 files): fl-workflow (SKILL + PHASE-1 to PHASE-7 + POST-LOOP), run-workflow (SKILL + PHASE-1 to PHASE-8 + PHASE-10), feature-quality (SKILL + ENGINE + INFRA), finalizer/SKILL, testing/SKILL
- `.claude/hooks/statusline.ps1`

**MEDIUM priority (tool code):**
- `tools/KojoComparer/BatchExecutor.cs`
- `tools/KojoComparer/BatchProcessor.cs`
- `tools/KojoComparer/FileDiscovery.cs`
- `tools/KojoComparer/Program.cs`
- `tools/KojoComparer.Tests/BatchProcessorTests.cs`

**LOW priority (reference docs):**
- `Game/agents/designs/` (26 files) - architecture/system design documents
- `Era.Core.Tests/TrainingIntegrationTests.cs` - minor edits
- `Game/agents/feature-703.md`, `feature-709.md` - status updates

### Goal (What to Achieve)
Use the F733 extraction tool to recover all non-dashboard lost changes. Prioritize `.claude/` workflow files (directly affect fc/fl/run quality), then KojoComparer, then docs.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F733 | [DONE] | Session extractor tool required to reconstruct files |
| **Blocker** | **F739** | **[CANCELLED]** | **Chain gap解消 - skip率<1%必要** |

## Links
[feature-736.md](feature-736.md)

## F734 Recovery Investigation Findings

Session-extractor (fixed in F734) recovered the following non-dashboard files. Recovery output already exists at `tools/session-extractor/.tmp/recovery/`.

### .claude/ files with applied changes (15 files)

| File | Applied/Total | Note |
|------|--------------|------|
| .claude/skills/run-workflow/PHASE-8.md | 16/32 | |
| .claude/agents/tech-investigator.md | 9/9 | |
| .claude/skills/finalizer/SKILL.md | 7/8 | |
| .claude/agents/tech-designer.md | 6/6 | |
| .claude/commands/switch.md | 6/6 | **NEW FILE** - not in git |
| .claude/commands/sync-deps.md | 5/5 | |
| .claude/skills/run-workflow/PHASE-6.md | 2/7 | |
| .claude/agents/finalizer.md | 2/3 | |
| .claude/agents/planning-validator.md | 0/5 | 0 applied |
| + ~7 more with 1-2 edits each | | |

56 additional .claude/ files had 0 applied edits (already at correct state or no recoverable session data).

### Additional scope items (discovered in F734)

**Era.Core changes** (not in original scope):
- Era.Core/IO/IFileSystem.cs (3/3 edits applied)
- Era.Core/Commands/Com/ICustomComLoader.cs (2/2 edits applied)
- Era.Core/Commands/Com/CustomComLoader.cs (1/3 edits applied)

**Era.Core.Tests changes** (not in original scope):
- MultiEntrySelectionTests.cs (3/5 edits applied)
- CustomComLoaderTests.cs (2/6 edits applied)

**Git hooks** (additional):
- .githooks/validate-sizes.sh (2/2 edits applied)

### Note on session-extractor

The F733 session-extractor had 2 bugs fixed in F734. The extractor now works correctly. Recovery output already exists at `tools/session-extractor/.tmp/recovery/` - no need to re-run.

All recovered files have confidence=low (edit chain gaps). Must compare recovered versions with current HEAD to determine what actually needs restoration.
