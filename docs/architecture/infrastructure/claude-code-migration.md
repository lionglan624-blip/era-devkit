# Claude Code Migration Plan

Migration plan for transitioning from current project-specific structure to Anthropic-recommended Claude Code architecture.

---

## ⚠️ REVISION NOTICE (2025-12-20 再検証)

**初期計画は大幅に誤っていたため、縮小版に修正しました。**

**修正の理由**:
1. Skills に `paths` フィールドは存在しない（Rulesのみ）
2. このプロジェクトは **subagent 中心**のため、Skills/Rules の効果は**限定的**

**追加判明事項（2025-12-20 再検証）**:
- `skills:` フィールドは subagent frontmatter に**公式に存在する**（6フィールドの1つ）
- ただし subagent への Skills ロード効果は**公式ドキュメントで不明確**
- Feature 150 で実験後に判断する方針

---

## Overview (REVISED)

**変更されたスコープ**:
- ~~reference/*.md → Skills 移行~~ → **維持が必要**（subagent用）
- ~~subagent frontmatter 更新~~ → **変更不要**（機能が存在しない）
- Rules: Opus向け path-based reminders のみ（小規模）
- Skills: オプション（効果限定的）

**Total Duration**: ~~5 Features (149-153)~~ → **1-2 Features (150-151)**
**Current Feature**: 149 (Design Phase - REVISED)

---

## Migration Phases

### Phase 0: Preparation (Feature 149 - Current)

**Goal**: Design and approval

**Deliverables**:
- [x] `claude-code-current.md` - Current state analysis
- [x] `claude-code-gap-analysis.md` - BP delta identification
- [x] `claude-code-target.md` - Target architecture design
- [ ] `claude-code-migration.md` - This document
- [ ] User approval for migration

**Success Criteria**:
- All design documents reviewed
- User approval obtained
- Baseline metrics recorded (see section below)

**Rollback**: N/A (design only)

---

### Phase 1: Skills Infrastructure (Feature 150)

**Goal**: Validate Skills progressive disclosure mechanism

**Changes**:
```
.claude/
└── skills/
    └── erb-lang/
        ├── SKILL.md              # 50 tokens - entry point
        ├── basics.md             # 300 tokens - function calls, data types
        ├── control-flow.md       # 200 tokens - IF/SELECTCASE/loops
        ├── output.md             # 200 tokens - PRINT/DRAWLINE
        └── advanced.md           # 250 tokens - CALLFORM/TRYCALL
```

**Implementation Steps**:
1. Create `.claude/skills/erb-lang/` from `pm/reference/erb-reference.md`
2. Update one subagent (implementer.md) to reference `erb-lang` skill
3. Test progressive disclosure with `/imple` on ERB task
4. Measure token usage vs baseline

**Test Cases**:
- `/imple` on erb-type feature (use existing or create simple test)
- Verify skill auto-discovery
- Verify progressive disclosure (only SKILL.md loaded initially)
- Verify detailed file loaded on-demand

**Success Criteria**:
- Skill discovered automatically
- Token usage ≤ baseline
- `/imple` workflow unchanged
- implementer.md updated successfully

**Rollback Trigger**:
- Skill not discovered
- Token usage > baseline + 10%
- Workflow regression

**Rollback Steps**:
```bash
# 1. Delete .claude/skills/
rm -rf .claude/skills/

# 2. Restore implementer.md from backup
git restore .claude/agents/implementer.md

# 3. Verify rollback
# Test /imple with erb-type feature
```

**Rollback Verification**:
- `/imple` works with original reference docs
- No skill-related errors

---

### Phase 2: Complete Skills Migration (Feature 151)

**Goal**: Migrate all reference docs to skills

**Changes**:
```
.claude/skills/
├── erb-lang/          # From erb-reference.md (Phase 1)
├── kojo-writing/      # From kojo-reference.md + kojo-phases.md + kojo-canon-lines.md
├── engine-arch/       # From engine-reference.md
├── ntr-system/        # From ntr-system-map.md
├── testing/           # From testing-reference.md
└── sessions/          # From sessions-reference.md
```

**Implementation Steps**:
1. Create remaining skills (kojo-writing, engine-arch, etc.)
2. Update all relevant subagents:
   - `kojo-writer.md` → use `kojo-writing` skill
   - `implementer.md` → use `engine-arch` skill (for engine tasks)
   - `ac-tester.md` → use `testing` skill
3. Deprecate `pm/reference/` (move to `pm/reference.backup/`)
4. Test all workflows (`/next`, `/imple`, kojo-writer, ac-validator)

**Success Criteria**:
- All 6+ skills created
- All subagents updated
- All workflows functional
- Token usage ≤ baseline

**Rollback Trigger**:
- Multiple workflow regressions
- Token usage > baseline + 15%
- Skill discovery failures

**Rollback Steps**:
```bash
# 1. Restore reference docs
mv pm/reference.backup/ pm/reference/

# 2. Restore all subagent prompts
git restore .claude/agents/

# 3. Delete skills (keep erb-lang for analysis)
rm -rf .claude/skills/kojo-writing/
rm -rf .claude/skills/engine-arch/
rm -rf .claude/skills/ntr-system/
rm -rf .claude/skills/testing/
rm -rf .claude/skills/sessions/

# 4. Verify rollback
# Test /next, /imple, kojo workflows
```

**Rollback Verification**:
- All workflows restored
- Reference docs readable
- No skill errors

---

### Phase 3: Rules Implementation (Feature 152)

**Goal**: Implement path-conditional auto-loading

**Changes**:
```
.claude/rules/
├── 00-always.md       # Project conventions (commit, language, AC format)
├── 10-erb-files.md    # Auto-load for ERB/**/*.ERB, ERB/**/*.ERH
├── 20-kojo-files.md   # Auto-load for ERB/口上/**/*.ERB
├── 30-engine-files.md # Auto-load for uEmuera/**/*.cs
└── 40-test-files.md   # Auto-load for test/**/*
```

**Rule Examples**:

`00-always.md`:
```markdown
---
name: Project Conventions
description: Always-active rules for this repository
---

## Commit Convention
feat/fix/docs/refactor/test

## Language
- Docs: English
- User: Japanese
- Code comments: English
```

`10-erb-files.md`:
```markdown
---
name: ERB File Rules
description: Active when editing ERB scripts
paths:
  - "Game/ERB/**/*.ERB"
  - "Game/ERB/**/*.ERH"
---

## ERB Scripting Rules
- Use `RETURNF` in `#FUNCTION` blocks
- Never bare `RETURN` (must have value)
- Prefer `TRYCALL` for optional handlers
```

**Implementation Steps**:
1. Create `.claude/rules/` directory
2. Create `00-always.md` (extract from CLAUDE.md)
3. Create path-specific rules (10-40)
4. Update CLAUDE.md to reference rules system
5. Test auto-loading with file edits in different paths

**Test Cases**:
- Edit `Game/ERB/NTR/TEST.ERB` → verify `10-erb-files.md` loaded
- Edit `Game/ERB/口上/美鈴.ERB` → verify `20-kojo-files.md` loaded
- Edit `uEmuera/EraCS/Instruction.cs` → verify `30-engine-files.md` loaded

**Success Criteria**:
- Rules auto-load based on path
- No conflicts between rules
- Token usage ≤ baseline
- Workflows functional

**Rollback Trigger**:
- Rule conflicts causing errors
- Auto-loading not working
- Token usage increase

**Rollback Steps**:
```bash
# 1. Delete .claude/rules/
rm -rf .claude/rules/

# 2. Restore CLAUDE.md
git restore CLAUDE.md

# 3. Verify rollback
# Rules should no longer auto-load
```

**Rollback Verification**:
- No rule-related errors
- Workflows use only CLAUDE.md

---

### Phase 4: Cleanup (Feature 153)

**Goal**: Finalize migration, remove deprecated files

**Changes**:
- Delete `pm/reference.backup/`
- Update CLAUDE.md to reference skills/rules
- Remove redundant content from CLAUDE.md (moved to rules)
- Final documentation pass

**Implementation Steps**:
1. Verify all workflows stable for 1 week
2. Delete `pm/reference.backup/`
3. Update CLAUDE.md:
   - Remove content now in rules
   - Add section on skills usage
   - Add section on rules system
4. Update `index-features.md` to mark 149-153 complete

**Success Criteria**:
- All deprecated files removed
- CLAUDE.md streamlined
- Documentation accurate
- Token metrics improved

**Point of No Return**: This phase cannot be rolled back without recreating reference docs.

**Mitigation**: Keep git history for reference doc recovery if needed.

---

## Verification Checklist per Phase

| Check | Phase 1 | Phase 2 | Phase 3 | Phase 4 |
|-------|:-------:|:-------:|:-------:|:-------:|
| `/imple` works | ✓ | ✓ | ✓ | ✓ |
| `/next` works | ✓ | ✓ | ✓ | ✓ |
| `kojo-writer` works | - | ✓ | ✓ | ✓ |
| `ac-validator` works | ✓ | ✓ | ✓ | ✓ |
| Token baseline | ✓ | ✓ | ✓ | ✓ |
| No regressions | ✓ | ✓ | ✓ | ✓ |
| Skills discovered | ✓ | ✓ | ✓ | ✓ |
| Rules auto-load | - | - | ✓ | ✓ |

**Testing Protocol**:
1. Run test before change (record baseline)
2. Implement phase changes
3. Run test after change
4. Compare results
5. If regression: rollback immediately

---

## Token Metrics Baseline

### Baseline Measurement (Phase 0)

**Method**: Measure tokens in typical workflows

| Workflow | Current Tokens | Target Tokens | Measurement Method |
|----------|:--------------:|:-------------:|-------------------|
| `/imple` (ERB) | TBD | -30% | Count tokens in implementer invocation |
| `/imple` (engine) | TBD | -30% | Count tokens in implementer invocation |
| `/next` | TBD | -10% | Count tokens in next command invocation |
| `kojo-writer` | TBD | -40% | Count tokens in kojo-writer subagent |

**How to Measure**:
1. Enable token counting in Claude Code
2. Run workflow 3 times
3. Average token counts
4. Record in `feature-149.md` Execution Log

**Target Metrics**:
- **ERB tasks**: 30% reduction (erb-reference.md ~3000 tokens → 500 tokens progressive)
- **Kojo tasks**: 40% reduction (kojo-reference.md + kojo-phases.md + kojo-canon-lines.md ~5000 tokens → 600 tokens progressive)
- **Engine tasks**: 30% reduction (engine-reference.md ~2500 tokens → 400 tokens progressive)
- **Overhead**: Skills discovery should add <50 tokens

**Acceptable Range**:
- Phase 1: +10% allowed (infrastructure overhead)
- Phase 2: -20% minimum (major migration)
- Phase 3: -5% additional (rules efficiency)
- Phase 4: -30% total target

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation | Owner |
|------|:----------:|:------:|------------|-------|
| Skills not discovered | Medium | High | Explicit testing in Phase 1, rollback ready | Feature 150 |
| Rules conflict | Low | Medium | Path specificity design, testing | Feature 152 |
| Workflow regression | Medium | High | Per-phase verification, rollback procedures | All phases |
| Token increase | Low | Medium | Baseline metrics, measurement after each phase | All phases |
| Progressive disclosure broken | Medium | High | Test with minimal context, verify on-demand loading | Phase 1 |
| Path matching failure | Low | Medium | Test multiple file paths, verify regex | Phase 3 |
| Documentation drift | Low | Low | Doc-reviewer on Phase 4 | Feature 153 |

**Critical Risks** (High Impact):
1. **Skills not discovered** → Phase 1 explicitly tests this, rollback if fails
2. **Workflow regression** → Every phase has rollback procedure
3. **Progressive disclosure broken** → Phase 1 validates, blocks Phase 2 if fails

**Risk Mitigation Strategy**:
- **Incremental**: One phase at a time, no parallel migrations
- **Reversible**: Rollback procedures for Phases 1-3
- **Measured**: Token metrics before/after each phase
- **Tested**: Verification checklist required for phase completion

---

## Dependencies

### Feature Dependencies (Blocking)

```
149 (Design)
 ├── 150 (Skills Infra) ──┐
 │                        │
 ├── 151 (Skills Complete)│
 │    │                   │
 │    └── 153 (Cleanup)   │
 │                        │
 └── 152 (Rules) ─────────┘
           │
           └── 153 (Cleanup)
```

**Blocking Relationships**:
- Feature 150 blocks Feature 151 (must validate skills first)
- Feature 151 blocks Feature 153 (must complete migration before cleanup)
- Feature 152 can run parallel with 151 (independent system)
- Feature 153 is final cleanup (requires 151 + 152 complete)

### Technical Dependencies

| Phase | Requires | Provides |
|-------|----------|----------|
| 0 (149) | - | Design approval, baselines |
| 1 (150) | Design approval | Skills mechanism validation |
| 2 (151) | Phase 1 success | All skills migrated |
| 3 (152) | - | Rules system |
| 4 (153) | Phase 2 + 3 success | Clean final state |

**Parallel Execution**:
- Phase 3 (Feature 152) can start after Phase 1 completes
- Phase 2 and Phase 3 can run in parallel
- Phase 4 must wait for both Phase 2 and Phase 3

---

## Success Criteria

### Overall Migration Success

All conditions must be met:

1. **Functional Parity**
   - All slash commands work (`/next`, `/imple`, `/queue`, `/roadmap`)
   - All subagents functional (kojo-writer, implementer, ac-validator, etc.)
   - No new errors or failures

2. **Token Efficiency**
   - Overall token usage reduced by ≥30%
   - No single workflow increases >5%
   - Progressive disclosure verified working

3. **Architecture Compliance**
   - Skills follow Anthropic BP (≤500 lines, progressive disclosure)
   - Rules follow Anthropic BP (path-conditional auto-loading)
   - Subagents streamlined (reference docs → skills)
   - CLAUDE.md streamlined (conventions → rules)

4. **Documentation Quality**
   - CLAUDE.md updated and accurate
   - Skills documented
   - Rules documented
   - Migration complete in index-features.md

5. **Stability**
   - No regressions in test suite
   - All ACs pass for Features 150-153
   - 1 week stable operation before Phase 4

### Per-Phase Success Criteria

**Phase 1 (Feature 150)**:
- [ ] `.claude/skills/erb-lang/` created
- [ ] `implementer.md` updated to use skill
- [ ] Skill auto-discovered in test
- [ ] Progressive disclosure verified
- [ ] Token usage ≤ baseline + 10%

**Phase 2 (Feature 151)**:
- [ ] All 6+ skills created
- [ ] All subagents updated
- [ ] `pm/reference/` deprecated
- [ ] All workflows tested and passing
- [ ] Token usage ≤ baseline - 20%

**Phase 3 (Feature 152)**:
- [ ] `.claude/rules/` directory created
- [ ] 5 rule files created (00-always, 10-40)
- [ ] Path-conditional loading verified
- [ ] No rule conflicts
- [ ] Token usage ≤ baseline - 5% additional

**Phase 4 (Feature 153)**:
- [ ] `reference.backup/` deleted
- [ ] CLAUDE.md updated
- [ ] Documentation reviewed
- [ ] Token usage ≤ baseline - 30%
- [ ] 1 week stability verified

---

## Execution Timeline

| Feature | Phase | Duration (est.) | Dependencies | Start After |
|:-------:|-------|:---------------:|--------------|-------------|
| 149 | Design | 2 days | None | Immediate |
| 150 | Skills Infra | 1 day | 149 approval | 149 complete |
| 151 | Skills Complete | 2 days | 150 success | 150 complete |
| 152 | Rules | 1 day | 150 success | 150 complete |
| 153 | Cleanup | 1 day | 151 + 152 + 1 week stable | 151 & 152 + 1 week |

**Total Duration**: ~7 days + 1 week stabilization = ~2 weeks

**Critical Path**: 149 → 150 → 151 → (1 week) → 153

**Parallel Path**: 150 → 152 → (wait for 151) → 153

---

## Rollback Decision Matrix

| Scenario | Phase | Decision | Action |
|----------|-------|----------|--------|
| Skill not discovered | 1 | ROLLBACK | Delete .claude/skills/, restore implementer.md |
| Token usage +15% | 1 | ROLLBACK | Full rollback, investigate |
| Token usage +10% | 1 | INVESTIGATE | Continue with monitoring |
| One workflow fails | 2 | ROLLBACK | Full rollback, fix design |
| Multiple workflows fail | 2 | ROLLBACK | Full rollback, abort migration |
| Rules conflict | 3 | ROLLBACK | Delete .claude/rules/, restore CLAUDE.md |
| Path matching fails | 3 | FIX | Adjust path patterns, no rollback needed |
| Token target missed | 4 | DOCUMENT | Record actual metrics, analyze |

**Rollback Authority**:
- Automated: Token usage >20% increase → immediate rollback
- Manual: Workflow regression → user decision
- Design: Multiple issues in Phase 1 → abort migration (return to Feature 149)

---

## Post-Migration Validation

After Phase 4 completion, validate:

1. **Functional Tests** (run all workflows)
   - `/next` → propose next feature
   - `/imple` on erb-type feature
   - `/imple` on kojo-type feature
   - `/imple` on engine-type feature
   - `kojo-writer` subagent standalone

2. **Token Measurements** (compare to baseline)
   - Record final token counts
   - Calculate % reduction
   - Document in Feature 153

3. **Regression Tests** (ensure no breakage)
   - Run full test suite
   - Verify all existing features still work
   - Check AC validation still works

4. **Documentation Review** (ensure accuracy)
   - CLAUDE.md reflects new structure
   - Skills documented
   - Rules documented
   - index-features.md updated

5. **User Acceptance**
   - User verifies workflows
   - User approves final state
   - Migration marked complete

---

## Lessons Learned Template

After Phase 4, document in Feature 153:

### What Worked Well
- (TBD after migration)

### What Didn't Work
- (TBD after migration)

### Unexpected Issues
- (TBD after migration)

### Recommendations for Future Migrations
- (TBD after migration)

---

## Appendix A: Skills Structure Reference

### Skill Template

```
.claude/skills/{skill-name}/
├── SKILL.md              # Entry point (50-100 tokens)
├── overview.md           # High-level concepts (200-300 tokens)
├── reference.md          # Detailed reference (300-500 tokens)
└── examples.md           # Code examples (300-500 tokens)
```

### SKILL.md Template

```markdown
# {Skill Name}

Brief description (1-2 sentences).

## Quick Reference

- Key concept 1
- Key concept 2
- Key concept 3

## Files

- `overview.md` - High-level concepts
- `reference.md` - Detailed reference
- `examples.md` - Code examples

## When to Use

Use this skill when [describe use case].
```

---

## Appendix B: Rules Structure Reference

### Rule Template

```markdown
---
name: {Rule Name}
description: {Brief description}
paths:
  - "{glob-pattern-1}"
  - "{glob-pattern-2}"
---

## {Section 1}

Content...

## {Section 2}

Content...
```

### Path Matching Examples

```yaml
# Match all ERB files
paths:
  - "Game/ERB/**/*.ERB"
  - "Game/ERB/**/*.ERH"

# Match only kojo files
paths:
  - "Game/ERB/口上/**/*.ERB"

# Match C# engine files
paths:
  - "uEmuera/**/*.cs"
```

---

## Appendix C: Token Measurement Procedure

### Setup

1. Open Claude Code
2. Enable verbose logging (if available)
3. Prepare test scenarios

### Measurement

For each workflow:

```bash
# 1. Clear context
# Start fresh conversation

# 2. Run workflow
/imple {feature-id}

# 3. Record tokens
# Check Claude Code token counter
# Record in spreadsheet:
# Workflow | Run 1 | Run 2 | Run 3 | Average

# 4. Repeat 3 times
# Average the results
```

### Recording Template

```markdown
## Token Baseline - {Date}

### /imple (ERB type)
- Run 1: {tokens}
- Run 2: {tokens}
- Run 3: {tokens}
- Average: {tokens}

### /imple (kojo type)
- Run 1: {tokens}
- Run 2: {tokens}
- Run 3: {tokens}
- Average: {tokens}

### /imple (engine type)
- Run 1: {tokens}
- Run 2: {tokens}
- Run 3: {tokens}
- Average: {tokens}

### /next
- Run 1: {tokens}
- Run 2: {tokens}
- Run 3: {tokens}
- Average: {tokens}
```

---

## Document History

| Version | Date | Author | Changes |
|:-------:|------|--------|---------|
| 1.0 | 2025-12-20 | implementer (sonnet) | Initial migration plan |

---

**End of Migration Plan**
