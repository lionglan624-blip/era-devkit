# F750 Investigation Report: TALENT Name Resolution Mechanism

## Executive Summary

This report investigates how Emuera engine resolves TALENT names like "恋人" and "思慕" that are NOT defined in Talent.csv. The investigation reveals that **all three TALENT names ARE properly resolved** via `Game/data/Talent.yaml` which supplements Talent.csv:

| TALENT Name | Index | Source |
|-------------|:-----:|--------|
| 恋人 | 16 | Talent.yaml |
| 恋慕 | 3 | Talent.csv + Talent.yaml |
| 思慕 | 17 | Talent.yaml |

**Key Finding**: Feature 711 implemented YAML supplement loading, which defines additional TALENT indices not present in the CSV file. This explains why the game loads without errors.

---

## Section 1: TALENT:恋人 Resolution

### Evidence from Talent.csv

**File**: `C:\Era\erakoumakanNTR\Game\CSV\Talent.csv`

Talent.csv defines TALENT indices 0-196 but does NOT include "恋人":
- Index 0: 処女
- Index 1: 童貞
- Index 2: 性別
- Index 3: 恋慕 (this is the only romantic-type talent defined)
- ... (no "恋人" anywhere)

### Evidence from ERB Files

**File**: `C:\Era\erakoumakanNTR\Game\ERB\COMF352.ERB` (line 24)
```erb
TALENT:TARGET:恋人 = 1
```

**File**: `C:\Era\erakoumakanNTR\Game\ERB\EVENTTURNEND.ERB` (line 111)
```erb
IF TALENT:奴隷:思慕 || TALENT:奴隷:恋慕 || TALENT:奴隷:恋人
```

**File**: `C:\Era\erakoumakanNTR\Game\ERB\口上\U_汎用\KOJO_KU_関係性.ERB` (line 17)
```erb
IF TALENT:奴隷:恋人
```

### Engine Resolution Mechanism Analysis

**File**: `C:\Era\erakoumakanNTR\engine\Assets\Scripts\Emuera\GameData\Expression\ExpressionParser.cs` (lines 270-272)
```csharp
if (varCode != VariableCode.__NULL__ && GlobalStatic.ConstantData.isDefined(varCode, idStr))
    return new SingleTerm(idStr);
GlobalStatic.IdentifierDictionary.ThrowException(idStr, false);
```

**File**: `C:\Era\erakoumakanNTR\engine\Assets\Scripts\Emuera\GameData\Variable\VariableStrArgTerm.cs` (lines 35-40)
```csharp
if (!dic.TryGetValue(key, out int i))
{
    if (errPos == null)
        throw new CodeEE("配列変数" + parentCode.ToString() + "の要素を文字列で指定することはできません");
    else
        throw new CodeEE(errPos + "の中に\"" + key + "\"の定義がありません");
}
```

### Resolution: **INDEX 16** (Confirmed)

**Critical Finding**: The engine loads TALENT definitions from **TWO sources**:
1. `Game/CSV/Talent.csv` - Traditional CSV format
2. `Game/data/Talent.yaml` - YAML supplement (Feature 711 implementation)

**File**: `Game/data/Talent.yaml` (lines 34-35)
```yaml
  - index: 16
    name: "恋人"
```

**Conclusion**: TALENT:恋人 resolves to **INDEX 16** via Talent.yaml.

---

## Section 2: TALENT:思慕 Resolution

### Evidence from Talent.csv

Talent.csv does NOT include "思慕":
- No entry with name "思慕" exists in the CSV file
- The closest entry is index 3: "恋慕"

### Evidence from ERB Files

**File**: `C:\Era\erakoumakanNTR\Game\ERB\EVENTTURNEND.ERB` (line 119)
```erb
TALENT:奴隷:思慕 = 1
```

**File**: `C:\Era\erakoumakanNTR\Game\ERB\EVENTTURNEND.ERB` (line 161)
```erb
TALENT:奴隷:思慕 = 0	;思慕をクリア
```

**File**: `C:\Era\erakoumakanNTR\Game\ERB\口上\9_大妖精\KOJO_K9_挿入.ERB` (multiple lines)
```erb
ELSEIF TALENT:思慕
```

### Engine Resolution Mechanism Analysis

Same mechanism as TALENT:恋人. The dictionary lookup fails because "思慕" is not defined in Talent.csv.

### Resolution: **INDEX 17** (Confirmed)

**File**: `Game/data/Talent.yaml` (lines 36-37)
```yaml
  - index: 17
    name: "思慕"
```

**Conclusion**: TALENT:思慕 resolves to **INDEX 17** via Talent.yaml.

---

## Section 3: Summary

### Concrete Numeric Mappings

| TALENT Name | Numeric Index | Source | Status |
|-------------|---------------|--------|--------|
| 恋慕 | 3 | Talent.csv line 7, Talent.yaml line 8-9 | **CONFIRMED** |
| 恋人 | 16 | Talent.yaml line 34-35 | **CONFIRMED** |
| 思慕 | 17 | Talent.yaml line 36-37 | **CONFIRMED** |

### Key Findings

1. **All three TALENT names are properly defined**:
   - `恋慕` (index 3) - Talent.csv and Talent.yaml
   - `恋人` (index 16) - Talent.yaml only
   - `思慕` (index 17) - Talent.yaml only

2. **DUAL data source mechanism**: uEmuera loads TALENT definitions from:
   - `Game/CSV/Talent.csv` - Traditional format, limited entries
   - `Game/data/Talent.yaml` - YAML supplement with additional entries (Feature 711 implementation)

3. **Parse-time resolution works**: Emuera resolves TALENT names at parse time. The engine populates ConstantData dictionary from BOTH sources before ERB parsing begins.

4. **Empirical verification**: Running `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --strict-warnings` confirms the game loads without errors.

5. **ERB branching order**: ERB uses `IF TALENT:恋人 / ELSEIF TALENT:恋慕 / ELSEIF TALENT:思慕 / ELSE` pattern, which corresponds to indices 16 → 3 → 17 → fallback.

### Investigation Resolved

The Feature 750 document states "Game runs without errors despite undefined TALENT constants." This is now explained:
- `Game/data/Talent.yaml` defines indices 16 (恋人) and 17 (思慕)
- Feature 711 implemented YAML supplement loading into ConstantData
- The engine correctly resolves these names at parse time

### Recommendations for YAML Migration (F750)

Now that TALENT mappings are confirmed, the YAML migration script can proceed with:

1. **Branch 0 (恋人)**: `{ TALENT: { 16: { ne: 0 } } }` - Check TALENT index 16 not equal to 0
2. **Branch 1 (恋慕)**: `{ TALENT: { 3: { ne: 0 } } }` - Check TALENT index 3 not equal to 0 (already partially migrated in some files)
3. **Branch 2 (思慕)**: `{ TALENT: { 17: { ne: 0 } } }` - Check TALENT index 17 not equal to 0
4. **Branch 3+ (ELSE)**: `{}` - Keep empty for fallback

### Semantic Note

The ERB branching order is: 恋人(16) → 恋慕(3) → 思慕(17) → ELSE

This is a **priority check** pattern:
- First check for highest relationship (恋人 = lover)
- Then check for mid relationship (恋慕 = romantic love)
- Then check for low relationship (思慕 = admiration)
- Finally fallback to no relationship

Each TALENT index is independent (not a value scale), so conditions use `ne: 0` (not equal to 0) to check if the TALENT flag is set.

---

## Appendix: File References

| File | Line | Content |
|------|------|---------|
| Game/CSV/Talent.csv | 7 | `3,恋慕,;愛情に似た感情を抱いている状態。` |
| Game/ERB/COMF352.ERB | 24 | `TALENT:TARGET:恋人 = 1` |
| Game/ERB/EVENTTURNEND.ERB | 111 | `IF TALENT:奴隷:思慕 \|\| TALENT:奴隷:恋慕 \|\| TALENT:奴隷:恋人` |
| Game/ERB/EVENTTURNEND.ERB | 119 | `TALENT:奴隷:思慕 = 1` |
| Game/ERB/EVENTTURNEND.ERB | 161 | `TALENT:奴隷:思慕 = 0` |
| Game/ERB/口上/U_汎用/KOJO_KU_関係性.ERB | 17 | `IF TALENT:奴隷:恋人` |
| engine/Assets/Scripts/Emuera/GameData/Expression/ExpressionParser.cs | 270-272 | Name resolution logic |
| engine/Assets/Scripts/Emuera/GameData/Variable/VariableStrArgTerm.cs | 35-40 | Dictionary lookup and error throwing |
| engine/Assets/Scripts/Emuera/GameData/ConstantData.cs | 697-714 | `isDefined()` method |
| Game/data/Talent.yaml | 34-35 | `恋人 = index 16` |
| Game/data/Talent.yaml | 36-37 | `思慕 = index 17` |
| Game/data/Talent.yaml | 8-9 | `恋慕 = index 3` |

---

*Report generated for Feature 750 T0 investigation task.*
*Updated: Investigation resolved - TALENT names confirmed via Talent.yaml supplement file.*
