# JSON Schemas

JSON Schema definitions for ERA game configuration files.

## Schemas

### GameBase.schema.json

Validates `Game/config/gamebase.yaml` — game metadata (code, version, title, author, year, additional info).

### VariableSize.schema.json

Validates `Game/config/variable_sizes.yaml` — array variable size definitions (DAY, MONEY, TIME, ITEM, etc.). Used by `.githooks/validate-sizes.sh` to cross-check against `Game/data/*.yaml` max indices.

## Usage

These schemas are referenced by the `YamlValidator` tool and git hooks for configuration validation.
