# AC Matcher Mapping

Mapping between AC table format and Engine implementation.

## Usage

AC tables use `Type` + `Matcher` columns. Engine implements specific check functions.
This mapping ensures consistency between feature specs and implementation.

## Mapping Table

| AC Type | AC Matcher | Engine Implementation | Source |
|---------|------------|----------------------|--------|
| output | equals | equals | KojoExpectValidator |
| output | contains | contains | KojoExpectValidator |
| output | not_contains | not_contains | KojoExpectValidator |
| output | matches | matches (regex) | KojoExpectValidator |
| variable | equals | variable_equals | KojoExpectValidator |
| variable | gt | variable_gt | KojoExpectValidator |
| variable | gte | variable_gte | KojoExpectValidator |
| variable | lt | variable_lt | KojoExpectValidator |
| variable | lte | variable_lte | KojoExpectValidator |
| build | succeeds | exit_code == 0 | Bash |
| build | fails | exit_code != 0 | Bash |
| exit_code | equals | exit_code == N | Bash |
| exit_code | succeeds | exit_code == 0 | Bash |
| exit_code | fails | exit_code != 0 | Bash |
| file | exists | file_exists | KojoExpectValidator (Feature 160) |
| file | not_exists | file_not_exists | KojoExpectValidator (Feature 160) |
| code | contains | Grep/grep (manual) | ac-tester |
| code | not_contains | Grep/grep (manual) | ac-tester |
| code | matches | Grep/grep (manual) | ac-tester |

## AC Type Descriptions

| Type | Purpose | Example Expected |
|------|---------|------------------|
| output | Console/game output text | "最近一緒にいると" |
| variable | Game variable value | "12345" |
| build | Build process result | - |
| exit_code | Process exit code | 0 |
| file | File/directory existence | "test/ac/erb/" |
| code | Source code content | "public class Foo" |

## Validation Rules

1. **Type + Matcher must be valid combination** (see mapping table)
2. **Expected format must match Type**:
   - output/variable/code: Quoted string or regex
   - build/exit_code: Number or `-` for succeeds/fails
   - file: Path string

## Adding New Matchers

When adding new matcher types:

1. Implement in Engine (e.g., KojoExpectValidator.cs)
2. Add to this mapping table
3. Update ac-validator.md Valid Matchers list

## Python Implementation

Kojo AC検証は `kojo_test_gen.py` で実装。

| Usage | Command |
|-------|---------|
| Single function | `python tools/kojo-mapper/kojo_test_gen.py --function {FUNC} {ERB}` |
| Feature batch | `python tools/kojo-mapper/kojo_test_gen.py --feature {ID} --com {COM} --output-dir {DIR}` |

**Options**:
- `--verbose`: TALENT分岐詳細表示
- `--output`: 単一関数テストJSON出力先

## Related

- [ac-validator.md](../../../.claude/agents/ac-validator.md) - Uses this mapping for validation
- [feature-template.md](feature-template.md) - AC table format
