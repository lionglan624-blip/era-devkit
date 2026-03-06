# Feature 048: Quick Start Test Migration

## Status: [DONE]

## Summary

Migrate test infrastructure from save-file-based to Quick Start-based approach, eliminating save compatibility issues and reducing maintenance cost.

## Background

Feature 047 introduced Quick Start (option 9 at MODE_SELECT) that bypasses all setup and goes directly to SHOP. Combined with `--inject` for state injection, this enables:

- **Deterministic test startup**: No save file dependency
- **Zero save compatibility issues**: Always works regardless of game updates
- **Lower maintenance**: No save file recreation needed

Current approach requires 6 save files that can break when game logic changes.

## Scope

### In Scope

1. Update test shell scripts to use Quick Start
2. Update input files (prepend `9` for Quick Start selection)
3. Update documentation (testing-reference.md, agents.md)
4. Deprecate save-file-based approach in docs

### Out of Scope

- Deleting existing save files (keep for backward compatibility)
- Modifying the Quick Start feature itself
- Changes to scenario JSON format (already works with Quick Start)

## Acceptance Criteria

- [x] `run-core-tests.sh` uses Quick Start instead of save file
- [x] `run-train-tests.sh` uses Quick Start instead of save file
- [x] `run-ntr-tests.sh` uses Quick Start instead of save file
- [x] All regression tests pass with new approach
- [x] `testing-reference.md` recommends Quick Start as default

## Technical Details

### Current Flow (Save-based)

```bash
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --load-file tests/core/regression-base.sav \
  --inject tests/core/scenario.json \
  < tests/core/input.txt
```

### New Flow (Quick Start-based)

```bash
# Input file starts with "9" to select Quick Start
echo "9" | cat - tests/core/input.txt | \
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --inject tests/core/scenario.json
```

Or modify input files directly:
```
# input.txt (new format)
9
100
400
...
```

### Files to Update

| File | Change |
|------|--------|
| `tests/run-core-tests.sh` | Remove `--load-file`, update input handling |
| `tests/run-train-tests.sh` | Same |
| `tests/run-ntr-tests.sh` | Same |
| `tests/core/input-*.txt` (5 files) | Prepend `9` |
| `tests/train/input-*.txt` (7 files) | Prepend `9` |
| `tests/ntr/input-*.txt` (2 files) | Prepend `9` |
| `Game/agents/reference/testing-reference.md` | Update Headless Test section and examples |
| `Game/agents/agents.md` | Update Headless Testing section |

## Effort Estimate

- Shell scripts: 3 files, ~30 min
- Input files: 14 files, ~15 min
- Documentation: 3 files, ~30 min
- Testing: ~30 min

**Total: ~2 hours**

## Risks

| Risk | Mitigation |
|------|------------|
| Quick Start state differs from save state | Verify with comparison tests |
| Training mode requires specific setup | Test 888/889 debug commands still work |

## Links

- [feature-047.md](feature-047.md) - Quick Start to Shop (prerequisite)
- [testing-reference.md](../reference/testing-reference.md) - Test strategy and scenario format
