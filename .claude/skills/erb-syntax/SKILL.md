---
name: erb-syntax
description: ERB scripting syntax reference. Use when working with ERB files, editing ERB code, RETURN rules, variable types, control flow, PRINT commands.
---

# ERB Language Reference

Quick reference for ERB scripting.

---

## Function Calls

| Syntax | Description |
|--------|-------------|
| `CALL FUNCNAME` | Standard call |
| `TRYCALL FUNCNAME` | Optional call |
| `CALLFORM "FUNC%s%"` | Dynamic name |
| `RETURN value` | Return from function |
| `RETURNF value` | Return from `#FUNCTION` |

---

## Data Types

| Variable | Scope | Description |
|----------|-------|-------------|
| `LOCAL` | Function | Local integer |
| `ARG` | Function | Argument integer |
| `FLAG` | Global | Global flags |
| `CFLAG` | Character | Character flags |
| `TALENT` | Character | Talents |
| `ABL` | Character | Abilities |
| `LOCALS` | Function | Local string |
| `CSTR` | Character | Character string |

**Character Access**: `TALENT:TARGET:0`, `CFLAG:MASTER:100`

---

## System Registers

| Register | Description |
|----------|-------------|
| `MASTER` | Player character |
| `TARGET` | Current target |
| `ASSI` | Assistant |
| `RESULT` | Last function result (int) |
| `RESULTS` | Last function result (string) |

---

## Control Flow

```erb
IF condition
ELSEIF condition
ELSE
ENDIF

SIF condition
    statement

FOR var, start, end, end, step
NEXT

SELECTCASE expression
    CASE value1
    CASEELSE
ENDSELECT
```

---

## Output Commands

| Command | Description |
|---------|-------------|
| `PRINT text` | Print |
| `PRINTL text` | Print + newline |
| `PRINTFORM text` | Print with substitution |
| `PRINTFORML text` | PRINTFORM + newline |

**Substitution**: `{FLAG:0}`, `%LOCALS:0%`

---

## Input Commands

| Command | Description |
|---------|-------------|
| `INPUT` | Wait for int → RESULT |
| `INPUTS` | Wait for string → RESULTS |
| `WAIT` | Wait for any key |

---

## Procedure

1. Read `pm/features/feature-{ID}.md` for task requirements
2. Identify target ERB files using Glob
3. Read existing code patterns in target file
4. Implement changes following existing conventions
5. Run `dotnet run ... --unit` to verify no syntax errors
6. Check for loading warnings with `--strict-warnings < /dev/null`

## Quality

### Required Items

- [ ] Every function ends with `RETURN {value}` (not bare RETURN)
- [ ] UTF-8 with BOM encoding (existing files maintain encoding)
- [ ] Tab-based indentation, consistent with file
- [ ] Variables declared before use (`#DIM`, `#DIMS`)

### Recommended Items

- [ ] Comments in Japanese, concise, on separate line
- [ ] Labels use `$label` format for GOTO targets
- [ ] Avoid GOTO when possible, prefer structured control flow

### NG Items

| Situation | NG Expression |
|-----------|---------------|
| Function returns | Bare `RETURN` without value |
| Variable usage | Using undeclared variables |
| Control flow | Excessive GOTO usage instead of IF/FOR/SELECTCASE |

---

## Constraints

| Constraint | Rationale |
|------------|-----------|
| No bare RETURN | All functions must return explicit values (`RETURN 0`, not bare `RETURN`). `#FUNCTION` directive requires `RETURNF` instead of `RETURN`. |
| No undeclared variables | Variables must be declared before use (`#DIM`, `#DIMS`) to prevent runtime errors. |
| No GOTO abuse | Avoid excessive GOTO usage. Prefer structured control flow (IF/ELSEIF/ELSE, FOR, SELECTCASE) for code readability and maintainability. |

---

## Links

- [kojo-reference.md](../../docs/reference/kojo-reference.md)
