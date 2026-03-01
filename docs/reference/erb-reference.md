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

**RETURN Rules**:
- Bare `RETURN` is **error** - must include value (`RETURN 0`)
- `#FUNCTION` directive: use `RETURNF` instead of `RETURN`

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

FOR var, start, end, step
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

## Links

- [kojo-reference.md](kojo-reference.md)
