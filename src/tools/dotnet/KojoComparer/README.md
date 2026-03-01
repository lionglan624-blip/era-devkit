# KojoComparer

Validates ERB-YAML equivalence by comparing dialogue output between ERB and YAML implementations.

## Usage

```bash
# Batch mode - test all kojo files
cd tools/KojoComparer
dotnet run -- --all [--multi-state]

# Single comparison
dotnet run -- --erb <path> --function <name> --yaml <path> --talent <state>

# Inject intro metadata
dotnet run -- --inject-intro --erb <path> --yaml <path>
```

## Purpose

Ensures YAML dialogue files produce identical output to their ERB counterparts. Tests multiple game states (default, 恋人, 恋慕, 思慕) to verify TALENT branching logic correctness. Critical for ERB→YAML migration verification.

## Key Features

- In-process ERB evaluation (no subprocess overhead)
- Multi-state testing with configurable profiles
- Output normalization and diff generation
- Intro metadata injection for schema validation
- Uses state-profiles.json for test configurations

## Dependencies

- ErbParser (ERB parsing)
- Era.Core (YAML rendering)
- YamlDotNet (YAML loading)

## Exit Codes

- 0: All comparisons passed
- 1: Comparison failed or invalid arguments
