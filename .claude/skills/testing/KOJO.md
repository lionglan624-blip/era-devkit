# Kojo Testing (--unit)

Kojo testing-specific reference. See [SKILL.md](SKILL.md) for common information.

---

## Single Function Test

```bash
cd Game
dotnet run --project ../engine/uEmuera.Headless.csproj -- . \
  --unit "KOJO_MESSAGE_COM_K4_300" \
  --char "4" \
  --set "TALENT:TARGET:3=1"
```

---

## Variable Specification

**Format**: `<VAR_TYPE>:<INDEX>=<VALUE>` or `<VAR_TYPE>:TARGET:<INDEX>=<VALUE>`

| Type | Example |
|------|---------|
| `FLAG` | `FLAG:26=1` |
| `CFLAG` | `CFLAG:TARGET:2=5000` |
| `TALENT` | `TALENT:TARGET:3=1` |

**Post-Wakeup Injection** (F083): `--set-after-wakeup "CFLAG:4:現在位置=5"` (after `@EVENTTRAIN`)

---

## TALENT Index Reference

| TALENT Name | Index | Description |
|-------------|:-----:|-------------|
| Affection | 3 | Love-like emotion |
| Tsundere | 14 | Tsundere personality |
| Married | 15 | NTR-related |
| Lover | 16 | Highest affection |
| Fondness | 17 | Favorable emotion |

**Source**: `Game/data/Talent.yaml`
**Note**: Use numeric index, NOT talent names.

---

## JSON Scenario Test

```json
{
  "name": "Test name",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "4",
  "args": [1],
  "state": {
    "CFLAG:TARGET:2": 5000,
    "TALENT:TARGET:3": 1
  },
  "mock_rand": [0],
  "expect": {
    "output_contains": "expected text",
    "no_errors": true
  }
}
```

**Fields**: `call`, `character`, `args`, `state`, `mock_rand`, `expect`

**MockRand** (F091/092): `[0]` = first DATAFORM, `[1]` = second, etc.

---

## Batch Testing

**Important**: `--unit` accepts only ONE argument. Use glob pattern for multiple files.

```bash
# Correct - glob pattern
dotnet run ... --unit "tests/kojo/*.json" --parallel

# Wrong - multiple arguments not supported
dotnet run ... --unit "file1.json" "file2.json"  # NG
```

---

## Troubleshooting

### Empty Output from Kojo Test

1. Function doesn't exist - check spelling
2. Missing character - add `--char`
3. Condition not met - add `--set`

### SELECTCOM Issue

Use `_1` suffix: `--unit "KOJO_MESSAGE_COM_K4_301_1"`

### Status: OK Limitation

**⚠️ `Status: OK` does NOT mean no errors!**

Use JSON scenarios with `"expect": {"no_errors": true}`.

---

## Test Workflow

```
Look at kojo → Implement → Smoke test → E2E test (if branches) → Update mapper
```

---

## kojo_test_gen.py (Batch Test Generation)

**Purpose**: Auto-generate AC test JSON from DATALIST blocks

**COM→File mapping**: [`src/tools/kojo-mapper/com_file_map.json`](../../../src/tools/kojo-mapper/com_file_map.json) is SSOT. kojo_test_gen.py references this JSON for file placement.

```bash
python src/tools/kojo-mapper/kojo_test_gen.py --feature {ID} --com {COM_NUMBER} --output-dir test/ac/kojo/feature-{ID}/
```

### Parameters

| Parameter | Format | Example | Description |
|-----------|--------|---------|-------------|
| `--feature` | Number | `280` | Feature ID |
| `--com` | Number | `71` | COM number (not name) |
| `--output-dir` | Path | `test/ac/kojo/feature-280/` | Output directory |

### COM Number Reference

| COM# | Name | COM# | Name |
|:----:|------|:----:|------|
| 67 | Face-to-face sitting | 71 | Double penetration |
| 68 | Reverse sitting | 72 | Be double penetrated |
| 69 | Face-to-face anal sitting | 80 | Handjob |
| 70 | Reverse anal sitting | | |

**Source**: Identify COM name→number from Feature title

### Expected Output

```
Generated: test-{ID}-K1.json (16 tests)
Generated: test-{ID}-K2.json (16 tests)
...
Generated: test-{ID}-K10.json (16 tests)
```

10 files × 16 tests = 160 tests (4 branches × 4 patterns)
