# YamlSchemaGen

JSON Schema generator for YAML dialogue files used in ERA game development.

## Purpose

YamlSchemaGen generates a JSON Schema (`dialogue-schema.json`) that defines the structure and validation rules for YAML-based character dialogue files. This schema enables:

- **Early error detection** - Catch structural errors during development before runtime
- **Type safety** - Enforce correct data types for game variables (TALENT, ABL, EXP, FLAG, CFLAG)
- **IDE support** - Enable autocomplete and inline validation in editors that support JSON Schema
- **Documentation** - Provide a machine-readable specification of the dialogue format

The generated schema defines the structure for character dialogue with conditional branching based on game state variables.

## Usage

### Generate Schema

Run the schema generator from the repository root:

```bash
dotnet run --project tools/YamlSchemaGen/
```

This creates `tools/YamlSchemaGen/dialogue-schema.json`.

### CLI Output

```
Generated schema: C:\Era\erakoumakanNTR\tools\YamlSchemaGen\dialogue-schema.json
```

Exit codes:
- `0` - Schema generated successfully
- `1` - Generation failed (with error message)

### Integration with Build Process

You can integrate schema generation into your build workflow:

```bash
# Regenerate schema before validation
dotnet run --project tools/YamlSchemaGen/ && \
  dotnet run --project tools/YamlValidator/ -- \
    --schema tools/YamlSchemaGen/dialogue-schema.json \
    --validate-all Game/YAML/Kojo/
```

## Schema Versioning

### When to Update the Schema

Update the schema definition when:

1. **New variable types** - Adding support for new ERA variable types (e.g., MARK, PALAM)
2. **Structure changes** - Modifying dialogue file structure (new required fields, nested objects)
3. **Condition operators** - Adding new comparison operators beyond `eq`, `ne`, `gt`, `gte`, `lt`, `lte`
4. **Validation rules** - Changing property requirements or type constraints

### Schema Evolution Workflow

1. **Modify `Program.cs`** - Update `GenerateDialogueSchema()` or `CreateVariableConditionSchema()` methods
2. **Add unit tests** - Update `tools/YamlSchemaGen.Tests/SchemaValidationTests.cs` with new validation cases
3. **Regenerate schema** - Run `dotnet run --project tools/YamlSchemaGen/`
4. **Verify changes** - Check `dialogue-schema.json` diff to confirm expected changes
5. **Test validation** - Run YamlValidator against existing YAML files to detect breaking changes

### Breaking vs Non-Breaking Changes

**Breaking Changes** (require YAML file updates):
- Adding new required properties
- Removing existing properties
- Changing property types
- Tightening validation rules

**Non-Breaking Changes** (backward compatible):
- Adding optional properties
- Adding new variable types
- Adding new condition operators
- Relaxing validation rules

### Version Control

The schema file (`dialogue-schema.json`) is checked into version control. When making changes:

```bash
# Review schema changes before committing
git diff tools/YamlSchemaGen/dialogue-schema.json

# Commit with descriptive message
git add tools/YamlSchemaGen/Program.cs tools/YamlSchemaGen/dialogue-schema.json
git commit -m "feat: Add MARK variable type support to dialogue schema"
```

## Output

### Schema Structure

The generated `dialogue-schema.json` is a JSON Schema Draft-07 document defining:

**Root Properties:**
- `character` (string, required) - Character identifier
- `situation` (string, required) - Dialogue situation code (e.g., "K4", "K100")
- `branches` (array, required) - Conditional dialogue branches

**Branch Structure:**
- `condition` (object, optional) - Condition for displaying this branch
- `lines` (array, required) - Dialogue text lines

**Supported Variable Types:**

Each variable type (`TALENT`, `ABL`, `EXP`, `FLAG`, `CFLAG`) supports:

1. **Direct equality check:**
   ```yaml
   TALENT:
     123: 1  # TALENT:123 equals 1
   ```

2. **Comparison operators:**
   ```yaml
   ABL:
     5:
       gte: 50  # ABL:5 >= 50
   ```

**Available Operators:**
- `eq` - Equals
- `ne` - Not equals
- `gt` - Greater than
- `gte` - Greater than or equal
- `lt` - Less than
- `lte` - Less than or equal

### Type Handling

The schema accepts both integer and string types for variable values due to YAML serialization behavior. This ensures compatibility with various YAML parsers.

## Integration

### YamlValidator Integration

The YamlValidator tool consumes the generated schema:

```bash
dotnet run --project tools/YamlValidator/ -- \
  --schema tools/YamlSchemaGen/dialogue-schema.json \
  --yaml Game/YAML/Kojo/COM_K1_0.yaml
```

See [tools/YamlValidator/README.md](../YamlValidator/README.md) for validation details.

### IDE Integration

Configure your IDE/editor to use the generated schema for YAML validation:

**VS Code** - Add to workspace settings (`.vscode/settings.json`):
```json
{
  "yaml.schemas": {
    "tools/YamlSchemaGen/dialogue-schema.json": "Game/YAML/Kojo/**/*.yaml"
  }
}
```

Refer to feature F599 (IDE/Editor Integration for YAML Schema) for comprehensive IDE setup.

### Unit Testing

Schema generation is validated by unit tests in `tools/YamlSchemaGen.Tests/`:

```bash
dotnet test tools/YamlSchemaGen.Tests/
```

Tests verify:
- Schema generates valid JSON Schema structure
- Required variable types (TALENT, ABL, EXP, FLAG, CFLAG) are defined
- Schema validates conformant YAML files successfully

## References

- [JSON Schema Draft-07](https://json-schema.org/draft-07/schema)
- [YamlValidator](../YamlValidator/README.md) - Validation tool using generated schema
- [Feature F590](../../pm/features/feature-590.md) - YAML Schema Validation Tools
- [Feature F599](../../pm/features/feature-599.md) - IDE/Editor Integration
