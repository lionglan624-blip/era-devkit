# Test Scenarios

## Schema

All test scenarios follow the unified JSON schema defined in `schema.json`.

### Supported Formats

1. **Single Test**: Direct test with `call` and `expect`
2. **Test Suite**: Multiple tests with optional `defaults`
3. **Scenario Setup**: Character/state configuration without execution

See `schema.json` for complete specification.

### Common Fields

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Required. Test/suite name |
| `description` | string | What this test verifies |
| `call` | string | ERB function to invoke (single test) |
| `character` | string/int | Target character ID |
| `state` | object | Game variables to set |
| `mock_rand` | array | Deterministic random values |
| `expect` | object | Expected outcomes (see below) |

### Expectation Matchers

| Matcher | Type | Description |
|---------|------|-------------|
| `output_contains` | string/array | Must appear in output |
| `not_contains` | string/array | Must NOT appear in output |
| `output_matches` | string/array | Regex patterns to match |
| `no_errors` | boolean | Assert error-free execution |
| `exit_code` | integer | Expected exit code |
| `variable_equals` | object | Variable equality checks |
| `variable_gt/gte/lt/lte` | object | Variable comparisons |

### Example: Single Test

```json
{
  "name": "K1 COM_315 Test",
  "character": "1",
  "call": "KOJO_MESSAGE_COM_K1_315",
  "state": {
    "TALENT:TARGET:16": 1
  },
  "mock_rand": [0],
  "expect": {
    "output_contains": "expected text",
    "no_errors": true
  }
}
```

### Example: Test Suite

```json
{
  "name": "K4 Test Suite",
  "defaults": {
    "character": "4",
    "state": {
      "CFLAG:TARGET:2": 5000
    }
  },
  "tests": [
    {
      "name": "Basic test",
      "call": "KOJO_MESSAGE_COM_K4_300",
      "expect": {
        "no_errors": true
      }
    }
  ]
}
```

## Directory Structure

| Directory | Purpose | Used By |
|-----------|---------|---------|
| `core/` | Game logic tests (variables, flow) | `--flow`, regression |
| `kojo/` | Kojo unit tests (function output) | `--unit` |
| `kojo/scenarios/` | Kojo scenario variants | `--unit` |
| `regression/` | Regression tests (release gate) | regression-tester |
| `train/` | Train mode tests | `--flow` |
| `ntr/` | NTR system tests | `--flow` |
| `unit/` | Full coverage tests (160+ patterns) | `--unit --parallel` |
| `fixtures/` | Variable injection tests | `--flow` |
| `output/` | Test results (gitignored) | - |

## Naming Conventions

| Pattern | Example | Description |
|---------|---------|-------------|
| `scenario-sc-{NNN}-*.json` | scenario-sc-001-shiboo-threshold.json | Core scenario |
| `kojo-com{XXX}.json` | kojo-com300.json | Kojo regression |
| `feature-{ID}-*.json` | feature-129/kojo-129-K1.json | Feature-specific |
| `k{N}-*.json` | k4-basic.json | Character-specific |

## Test Types

### Smoke Test (no_errors)
```json
{ "expect": { "no_errors": true } }
```

### AC Test (output_contains)
```json
{ "expect": { "output_contains": "expected text" } }
```

### Full Coverage
```json
{
  "expect": { "no_errors": true },
  "mock_rand": [0]  // Deterministic pattern
}
```

