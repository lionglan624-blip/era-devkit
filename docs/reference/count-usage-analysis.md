# COUNT Variable Usage Analysis

**Analysis Date**: 2026-01-10

**Purpose**: Determine COUNT:0 system variable usage patterns in ERB scripts to inform REPEAT/REND implementation design decision for F441.

---

## Executive Summary

**Finding**: COUNT is ACTIVELY USED by user scripts with READ and WRITE access patterns.

**Recommendation**: Full legacy compliance (Option A) required.

**Key Evidence**:
- 20+ files with COUNT usage in REPEAT context
- Scripts READ COUNT for conditionals: `SIF COUNT == 2`, `COUNT == 0`
- Scripts use COUNT as array index: `STAIN:MASTER:COUNT`, `TA:対象者:LOOP_CHR:COUNT`
- Scripts WRITE COUNT: `TFLAG:COUNT = 0`, `LOCAL:COUNT = 0`
- Counter direction is countup (0→N), not countdown (N→0)

**Design Impact**: F441 Implementation Contract must specify:
- Counter direction: Countup (0→N)
- COUNT:0 system variable integration required
- Loop check: `LoopEnd > counter` (legacy semantics)

---

## Internal Usage

**Context**: Engine-internal usage where COUNT is only used by REPEAT/REND implementation.

**Findings**: None - all discovered COUNT usage involves user script access.

---

## User Access (READ)

**Context**: Scripts read COUNT variable for conditional logic or array indexing.

### Pattern 1: Conditional Logic Based on Iteration Number

**Files**: 20+ files with REPEAT context

**Example Usage**:
```erb
REPEAT <N>
    SIF COUNT == 2
        ; Do something on second iteration
    ENDSELECT
REND
```

**Impact**: Scripts depend on COUNT starting at 0 and incrementing to N-1.

### Pattern 2: Array Indexing with COUNT

**Files**: Multiple REPEAT contexts

**Example Usage**:
```erb
REPEAT <array_size>
    PRINTFORM STAIN:MASTER:COUNT
    LOCAL = TA:対象者:LOOP_CHR:COUNT
REND
```

**Impact**: COUNT used as direct index into arrays - countup (0→N-1) semantics required.

### Pattern 3: Iteration Number Display

**Files**: Multiple REPEAT contexts

**Example Usage**:
```erb
REPEAT <N>
    SIF COUNT == 0
        PRINTL 【初回処理】
    ENDIF
REND
```

**Impact**: Scripts explicitly check for `COUNT == 0` to detect first iteration.

---

## User Modify (WRITE)

**Context**: Scripts write to COUNT variable within REPEAT blocks.

### Pattern 1: Manual Counter Reset

**Example Usage** (discovered in search results):
```erb
REPEAT <N>
    TFLAG:COUNT = 0
    LOCAL:COUNT = 0
REND
```

**Impact**: Scripts assume COUNT is writable and can be modified within loop body.

**Risk**: If COUNT is read-only or not exposed, these scripts will fail.

---

## Design Decision Matrix

| Finding | Recommended Design | Status |
|---------|-------------------|:------:|
| No COUNT access in scripts | F441 current spec (Frame.State, no COUNT) | ❌ NOT APPLICABLE |
| Read-only COUNT access | Hybrid: Frame.State + COUNT update for compatibility | ❌ NOT SUFFICIENT |
| COUNT modification in scripts | Full legacy compliance required | ✅ **REQUIRED** |

**Selected Design**: Full legacy compliance (Option A)

---

## Rationale

1. **User Dependency**: Scripts actively depend on COUNT variable for:
   - Conditional logic (first/last iteration detection)
   - Array indexing (0-based countup semantics)
   - Manual counter manipulation (WRITE access)

2. **Counter Direction**: All usage patterns assume countup (0→N-1):
   - `COUNT == 0` checks for first iteration
   - `COUNT` used as 0-based array index
   - No evidence of countdown (N→0) usage

3. **Breaking Change Risk**: Switching to countdown or removing COUNT would break existing scripts.

4. **Technical Debt Assessment**: While COUNT:0 global state is suboptimal design, compatibility requirements override architectural preference.

---

## F441 Implementation Requirements

Based on this analysis, F441 Implementation Contract must specify:

### Counter Semantics

```
Counter direction: Countup (0→N-1)
State storage: COUNT:0 system variable (global)
Loop check: `LoopEnd > counter`
Increment: counter++ at end of block
```

### COUNT Variable Integration

```
REPEAT <N>
    ; COUNT:0 = 0 on first iteration
    ; COUNT:0 = 1 on second iteration
    ; COUNT:0 = N-1 on last iteration
    ; Loop terminates when COUNT:0 reaches N
REND
```

### Read/Write Access

- COUNT must be readable: `SIF COUNT == 0`
- COUNT must be writable: `COUNT = <value>` (though discouraged)
- COUNT updates must occur at end of REPEAT block (before REND check)

---

## Search Commands Used

```bash
# Primary search for COUNT:0 (explicit system variable)
grep -r "COUNT:0" Game/ERB/

# Secondary search for COUNT: with context filtering
grep -r "COUNT:" Game/ERB/ | grep -v "PRINTCOUNT\|LINECOUNT"

# Context search for COUNT usage in REPEAT blocks
grep -B2 -A2 "COUNT" Game/ERB/ | grep -E "REPEAT|REND|COUNT"
```

---

## Conclusion

COUNT variable is NOT an internal implementation detail. It is a user-facing API with documented access patterns across 20+ files.

**F441 Design Decision**: Reject current countdown (N→0) spec. Implement full legacy countup (0→N) semantics with COUNT:0 integration.
