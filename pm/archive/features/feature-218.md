# Feature 218: Context-Aware CI Verification Scope

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
CI verification should be minimal, fast, and contextually relevant. Each verification step should have a clear purpose aligned with the current work context.

### Problem (Current Issue)
1. **verify-logs.py aggregates all accumulated logs** - Pre-commit checks 937 items when only 24 regression tests ran
2. **AC/Engine/Regression as separate categories** - Engine unit tests are conceptually "Engine AC" but treated differently
3. **Context-blind verification** - Same verification runs for `/do {ID}` and general commits

Current structure:
```
Game/logs/prod/
├── ac/           ← Feature AC logs (accumulated)
├── regression/   ← Flow test logs
└── engine/       ← Separate location for engine tests (PROBLEM)
```

Target structure:
```
Game/logs/prod/
├── ac/
│   ├── engine/       ← Engine AC (unified)
│   └── feature-*/    ← Feature AC
└── regression/       ← System AC (flow tests)
```

### Goal (What to Achieve)
1. **Unified AC taxonomy**: Engine tests = Engine AC, Feature tests = Feature AC, Regression = System AC
2. **Context-aware verification scope**:
   - `/do {ID}` → Feature AC + Regression
   - General commit → Regression only
   - Engine change → Engine AC + Regression
3. **Clean log lifecycle**: Logs relevant to current context only

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Engine tests output to unified log dir | file | static | exists | Game/logs/prod/ac/engine/test-result.trx | [ ] |
| 2 | verify-logs.py accepts --scope flag | output | cli | contains | "--scope" | [ ] |
| 3 | --scope=regression checks only regression | output | cli | not_contains | AC: | [ ] |
| 4 | --scope=feature:217 checks feature + regression | output | cli | contains | Feature-217: | [ ] |
| 5 | pre-commit uses --scope=regression by default | file | static | contains | --scope regression | [ ] |
| 6 | /do creates .git/IMPLE_FEATURE_ID file | file | static | equals | 217 | [ ] |
| 7 | pre-commit reads file and uses correct scope | file | static | contains | --scope feature: | [ ] |

### AC Details

**AC1**: Engine test output path change
- Current: `Game/logs/prod/engine/test-result.trx`
- New: `Game/logs/prod/ac/engine/test-result.trx`
- Implementation: Update `dotnet test --results-directory` parameter in testing SKILL docs
- Also update: verify-logs.py engine path lookup (line ~85: `engine_dir = prod_dir / "ac" / "engine"`)

**AC2-4**: verify-logs.py enhancement
```bash
# Regression only (pre-commit)
python tools/verify-logs.py --dir Game/logs/prod --scope regression

# Feature-specific (imple)
python tools/verify-logs.py --dir Game/logs/prod --scope feature:217

# All (full audit)
python tools/verify-logs.py --dir Game/logs/prod --scope all
```

**AC2**: verify-logs.py --scope flag
- Command: `python tools/verify-logs.py --help`
- Expected output contains: "--scope"

**AC5**: pre-commit uses --scope=regression by default
- Verification: Check .githooks/pre-commit file content for "--scope regression"
- Type: static code inspection

**AC3**: --scope=regression output verification
- Command: `python tools/verify-logs.py --dir Game/logs/prod --scope regression`
- Verification: Output should NOT contain "AC:" section header
- Implementation: When scope=regression, verify-logs.py skips AC directory processing entirely

**AC4**: --scope=feature output format
- Command: `python tools/verify-logs.py --dir Game/logs/prod --scope feature:217`
- Expected output contains: "Feature-217:" section header

**AC6**: /do creates .git/IMPLE_FEATURE_ID file (relative to repository root)
- Verification: After running `/do 217`, check `cat .git/IMPLE_FEATURE_ID`
- Expected output: "217" (file overwrite is sufficient, explicit delete not required)

**AC7**: pre-commit reads file and uses correct scope
- Verification: Check .githooks/pre-commit file content for file-based scope logic
- Expected: Script contains logic to read .git/IMPLE_FEATURE_ID and use "--scope feature:"

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Update dotnet test --results-directory and verify-logs.py engine path | [x] |
| 2 | 2 | Add --scope flag to verify-logs.py | [x] |
| 3 | 3 | Implement --scope=regression behavior | [x] |
| 4 | 4 | Implement --scope=feature:{ID} behavior | [x] |
| 5 | 5 | Update pre-commit to use --scope=regression by default | [x] |
| 6 | 6 | Add file creation step to /do workflow (.claude/commands/do.md) | [x] |
| 7 | 7 | Add file check to pre-commit for dynamic scope selection | [x] |

---

## Design Notes

### Unified AC Taxonomy

```
AC (Acceptance Criteria)
├── Engine AC     (logs/prod/ac/engine/)    - C# unit tests
├── Feature AC    (logs/prod/ac/feature-*)  - ERB/kojo tests
└── System AC     (logs/prod/regression/)   - Flow scenarios
```

### Scope Behavior

| Scope | Directories Checked |
|-------|---------------------|
| `regression` | regression/ only |
| `feature:{ID}` | ac/feature-{ID}/ + regression/ |
| `engine` | ac/engine/ + regression/ |
| `all` | ac/** + regression/ |

### Design Decisions

1. **Engine logs move under ac/**: `logs/prod/ac/engine/` (unified taxonomy)
2. **Feature ID passing**: File-based approach
   - `/do 217` writes `217` to `.git/IMPLE_FEATURE_ID`
   - pre-commit reads the file: `if [ -f .git/IMPLE_FEATURE_ID ]; then FEATURE_ID=$(cat .git/IMPLE_FEATURE_ID); --scope=feature:$FEATURE_ID; else --scope=regression; fi`
   - File cleanup: File overwrite is sufficient (stale file with old ID is acceptable - conservative scope selection)
3. **Log retention**: Not in scope (future feature)

---

## Review Notes

- **2025-12-25**: 旧F216からリナンバー。

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-26 | Initialize | initializer | Set status WIP, ready for implementation | READY |
| 2025-12-26 | Task 1-4 | implementer | Update verify-logs.py with --scope flag and path updates | [x] |
| 2025-12-26 | Task 5-7 | implementer | Update pre-commit and do.md for dynamic scope | [x] |
| 2025-12-26 | Post-Review | feature-reviewer | Found doc inconsistencies in SKILL.md and feature-205.md | NEEDS_REVISION |
| 2025-12-26 | Fix Docs | implementer | Updated all engine path references for consistency | [x] |

## Links
- Related: [F212](feature-212.md) (pre-commit simplification)
- Related: [F203](feature-203.md) (infrastructure audit)
