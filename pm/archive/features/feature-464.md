# Feature 464: COM Semantic Naming Refactoring

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: engine

## Created: 2026-01-12

---

## Summary

Refactor COM naming from ID-centric (Com42.cs) to semantic naming (ClitoralCap.cs) across C# codebase and YAML schema design.

**Scope**:
- Rename 133 existing COM .cs files to semantic names
- Restructure directories from `Com0xx/` to game-loop-based categories (`Daily/`, `Training/`, `Utility/`, etc.)
- Introduce `[ComId(N)]` attribute for legacy ID reference
- Update ComRegistry to use attribute-based discovery
- Update architecture.md YAML schema to semantic naming

**Output**:
- `Era.Core/Commands/Com/{Category}/*.cs` - Semantically named COM implementations
- `Era.Core/Commands/Com/ComIdAttribute.cs` - Legacy ID attribute
- Updated `full-csharp-architecture.md` YAML schema

**Total Volume**: 133 file renames + attribute addition + registry refactor + docs update

---

## Background

### Philosophy (Mid-term Vision)

**Technical Debt Zero Migration** - The C#/Unity migration aims to eliminate ERB legacy patterns, not carry them forward. ID-centric naming (Com42, Com100) is an ERB artifact that reduces code readability and should be replaced with self-documenting semantic names while preserving ID compatibility through attributes.

### Problem (Current Issue)

Current COM naming follows ERB conventions:
- Files: `Com42.cs`, `Com100.cs` - No indication of what command does
- Classes: `class Com42` - Requires opening file to understand purpose
- Directories: `Com0xx/`, `Com1xx/` - Numeric grouping, not semantic

This creates:
1. Poor discoverability (must open file to know what it does)
2. Magic numbers throughout codebase
3. Inconsistency if YAML uses semantic names but C# uses IDs

### Goal (What to Achieve)

1. **Semantic file/class names**: `ClitoralCap.cs`, `class ClitoralCap`
2. **Game-loop-based directories**: `Daily/`, `Training/`, `Utility/`, `Masturbation/`, `Visitor/`, `System/`
3. **Training subcategories**: `Training/Touch/`, `Training/Oral/`, `Training/Equipment/`, etc.
4. **ID preserved via attribute**: `[ComId(42)] public class ClitoralCap`
5. **ComRegistry updated**: Attribute-based ID lookup
6. **Architecture.md updated**: YAML schema uses semantic action names
7. **Zero breaking changes**: All existing references work via ID attribute

---

## Impact Analysis

| Component | Change | Impact |
|-----------|--------|--------|
| Era.Core/Commands/Com/*.cs | Rename files + classes | 133 files |
| Era.Core/Commands/Com/ComRegistry.cs | Attribute-based discovery | 1 file |
| Era.Core/Commands/Com/ComIdAttribute.cs | New attribute | 1 file |
| Era.Core.Tests/ | Update test references | ~5 files |
| full-csharp-architecture.md | YAML schema update | 1 file |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ComIdAttribute.cs exists | file | Glob | exists | Era.Core/Commands/Com/ComIdAttribute.cs | [x] |
| 2 | Game-loop directories exist | file | ls | count_gte | 6 directories (Daily, Training, Utility, Masturbation, Visitor, System) | [x] |
| 3 | No Com[0-9]*.cs files remain | file | Glob | not_exists | Era.Core/Commands/Com/**/Com[0-9]*.cs | [x] |
| 4 | All COMs have [ComId] attribute | code | Grep | count_equals | `\[ComId\([0-9]+\)\]` (133) | [x] |
| 5 | ComRegistry uses attribute lookup | code | Grep | contains | GetCustomAttribute.*ComIdAttribute | [x] |
| 6 | Build succeeds | build | dotnet build | succeeds | - | [x] |
| 7 | All tests pass | test | dotnet test | succeeds | - | [x] |
| 8 | YAML schema updated | file | Grep | contains | `pattern:.*[A-Z][a-z]+` | [x] |
| 9 | Zero technical debt | code | Grep | not_contains | TODO\|FIXME\|HACK | [x] |

### AC Details

**AC#1**: ComIdAttribute.cs exists
- Test: Glob pattern="Era.Core/Commands/Com/ComIdAttribute.cs"
- Expected: File exists

**AC#2**: Game-loop directories exist
- Test: `ls Era.Core/Commands/Com/`
- Expected: Daily/, Training/, Utility/, Masturbation/, Visitor/, System/ directories
- Training/ contains subcategories: Touch/, Oral/, Penetration/, Equipment/, Bondage/, Undressing/

**AC#3**: No numeric COM files remain
- Test: Glob pattern="Era.Core/Commands/Com/**/Com[0-9]*.cs"
- Expected: 0 matches (all renamed to semantic names)

**AC#4**: All COMs have [ComId] attribute
- Test: Grep pattern=`\[ComId\([0-9]+\)\]` path="Era.Core/Commands/Com"
- Expected: 133 matches (one per COM class)

**AC#5**: ComRegistry attribute lookup
- Test: Grep pattern="GetCustomAttribute.*ComIdAttribute" path="Era.Core/Commands/Com/ComRegistry.cs"
- Expected: Contains attribute-based ID extraction

**AC#6**: Build succeeds
- Test: `dotnet build Era.Core/Era.Core.csproj`
- Expected: Exit code 0

**AC#7**: All tests pass
- Test: `dotnet test`
- Expected: All tests pass

**AC#8**: YAML schema updated to semantic names
- Test: Grep pattern=`pattern:.*[A-Z][a-z]+` path="Game/agents/designs/full-csharp-architecture.md"
- Expected: Action pattern uses PascalCase names instead of COM_[0-9]+
- Note: Already updated in architecture.md (F464 annotation). This AC verifies the existing state.

**AC#9**: Zero technical debt
- Paths: Era.Core/Commands/Com/
- Test: Grep pattern="TODO|FIXME|HACK"
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create ComIdAttribute.cs | [x] |
| 2 | 2 | Create category directory structure | [x] |
| 3 | 3 | Rename all 133 COM files to semantic names | [x] |
| 4 | 4 | Add [ComId] attribute to all COM classes | [x] |
| 5 | 5 | Update ComRegistry for attribute-based lookup | [x] |
| 6 | 6 | Fix build errors | [x] |
| 7 | 7 | Fix test failures | [x] |
| 8 | 8 | Verify architecture.md YAML schema (already updated) | [x] |
| 9 | 9 | Verify zero technical debt | [x] |

---

## Implementation Contract

### ComIdAttribute

```csharp
// Era.Core/Commands/Com/ComIdAttribute.cs
namespace Era.Core.Commands.Com;

/// <summary>
/// Specifies the legacy COM ID for ERB compatibility and registry lookup.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ComIdAttribute : Attribute
{
    public int Id { get; }

    public ComIdAttribute(int id)
    {
        Id = id;
    }
}
```

### Directory Structure (Game-Loop Based)

```
Era.Core/Commands/Com/
│
├── Daily/                    # 日常ゲームループ (COM301-316, note: COM300 not yet implemented)
│   ├── ServeTea.cs               # COM301: お茶を淹れる
│   ├── Skinship.cs               # COM302: スキンシップ
│   ├── PatButt.cs                # COM310: 尻を撫でる
│   ├── Hug.cs                    # COM311: 抱き付く
│   └── ...
│
├── Training/                 # 調教ゲームループ (COM0-199, 500-599)
│   ├── Touch/                    # 愛撫系
│   │   ├── Caress.cs                 # COM0: 愛撫
│   │   ├── BreastCaress.cs           # COM6: 胸愛撫
│   │   ├── NippleTorture.cs          # COM7: 乳首責め
│   │   ├── NippleSuck.cs             # COM10: 乳首吸い
│   │   └── Kiss.cs                   # COM20: キス
│   ├── Oral/                     # 口系
│   │   ├── Cunnilingus.cs            # COM1: クンニ
│   │   ├── PerformFellatio.cs        # COM2: フェラする
│   │   ├── Fellatio.cs               # COM81: フェラチオ
│   │   └── Irrumatio.cs              # COM140: イラマチオ
│   ├── Penetration/              # 挿入系
│   │   ├── FingerInsertion.cs        # COM3: 指挿入れ
│   │   ├── Missionary.cs             # COM60: 正常位
│   │   ├── Doggy.cs                  # COM61: 後背位
│   │   ├── Cowgirl.cs                # COM65: 騎乗位
│   │   └── DoubleHole.cs             # COM71: 二穴挿し
│   ├── Equipment/                # 道具系
│   │   ├── ClitoralCap.cs            # COM42: クリキャップ
│   │   ├── Onahole.cs                # COM43: オナホール
│   │   ├── Vibrator.cs               # COM44: バイブ
│   │   ├── AnalVibrator.cs           # COM45: アナルバイブ
│   │   ├── AnalBeads.cs              # COM46: アナルビーズ
│   │   ├── NippleCap.cs              # COM47: ニプルキャップ
│   │   └── MilkingMachine.cs         # COM48: 搾乳機
│   ├── Bondage/                  # 拘束・SM系
│   │   ├── Spanking.cs               # COM100: スパンキング
│   │   ├── Whip.cs                   # COM101: 鞭
│   │   ├── Rope.cs                   # COM105: 縄
│   │   └── AnalElectrode.cs          # COM148: アナル電極
│   └── Undressing/               # 脱衣系
│       ├── UndressTop.cs             # COM200: 上半身脱衣
│       ├── UndressBra.cs             # COM202: ブラ脱衣
│       └── UndressPanties.cs         # COM203: パンツ脱衣
│
├── Masturbation/             # 自慰ゲームループ (COM600-699)
│   ├── SelfCaress.cs             # COM600: 愛撫(自慰)
│   ├── SelfFellatio.cs           # COM602: セルフフェラ
│   └── SelfVibrator.cs           # COM644: バイブ(自慰)
│
├── Utility/                  # プレイヤー行動 (COM400-419)
│   ├── Move.cs                   # COM400: 移動
│   ├── Collection.cs             # COM401: コレクション
│   ├── Sleep.cs                  # COM402: 就寝
│   ├── Rest.cs                   # COM403: 休憩
│   ├── Clean.cs                  # COM410: 掃除
│   ├── Study.cs                  # COM412: 勉強
│   └── Cook.cs                   # COM413: 料理を作る
│
├── Visitor/                  # NTR/訪問者 (COM460-490)
│   ├── TalkToVisitor.cs          # COM460: 訪問者と会話
│   ├── Escape.cs                 # COM461: 脱走する
│   ├── GuideVisitor.cs           # COM464: 訪問者を案内する
│   └── GoOut.cs                  # COM490: 外出する
│
└── System/                   # システム (reserved for COM888, COM999 - not yet implemented)
    └── (placeholder for future DayEnd.cs, Dummy.cs)
```

**Categorization Principle**: Directories correspond to game loops (TFLAG:COMABLE管理), not legacy ID ranges.

**Phase 25 Extension Files (COMF*ex)**: The "ex" suffix from ERB is unnecessary in semantic naming. Use function-based names and place under parent category. See [architecture.md Phase 25 Semantic Naming](designs/full-csharp-architecture.md).

### Example Renamed COM

```csharp
// Era.Core/Commands/Com/Training/Equipment/ClitoralCap.cs
using Era.Core.Training;
using Era.Core.Types;

namespace Era.Core.Commands.Com.Training.Equipment;

/// <summary>
/// Clitoral Cap equipment command with continuous effect.
/// </summary>
[ComId(42)]
public class ClitoralCap : EquipmentComBase
{
    public override string Name => "クリキャップ";  // Display name unchanged

    public override Result<ComResult> Execute(IComContext ctx)
    {
        // Implementation unchanged
    }
}
```

### Updated ComRegistry

```csharp
// Era.Core/Commands/Com/ComRegistry.cs
private void DiscoverComs()
{
    var assembly = Assembly.GetExecutingAssembly();
    var comTypes = assembly.GetTypes()
        .Where(t => typeof(ICom).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false });

    foreach (var type in comTypes)
    {
        var attr = type.GetCustomAttribute<ComIdAttribute>();
        if (attr == null) continue;  // Skip types without [ComId]

        try
        {
            if (Activator.CreateInstance(type) is ICom com)
            {
                _coms[new ComId(attr.Id)] = com;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to instantiate COM type {type.Name}: {ex.Message}");
        }
    }
}
```

### YAML Schema Update (architecture.md)

Before:
```yaml
action:
  type: string
  pattern: "^COM_[0-9]+$"
```

After:
```yaml
action:
  type: string
  pattern: "^[A-Z][a-zA-Z]+$"  # PascalCase semantic names
  examples:
    - "ClitoralCap"
    - "Kiss"
    - "Fellatio"
```

---

## COM Name Mapping Reference

| Game Loop | Directory | COM Range | Example Mapping |
|-----------|-----------|-----------|-----------------|
| 日常 | Daily/ | 301-316 | Com301→ServeTea, Com302→Skinship (note: Com300 not yet implemented) |
| 調教 | Training/Touch/ | 0-20 | Com0→Caress, Com6→BreastCaress, Com10→NippleSuck, Com20→Kiss |
| 調教 | Training/Oral/ | 1-2, 80-89, 140 | Com1→Cunnilingus, Com81→Fellatio, Com140→Irrumatio |
| 調教 | Training/Penetration/ | 3, 60-72 | Com3→FingerInsertion, Com60→Missionary, Com65→Cowgirl |
| 調教 | Training/Equipment/ | 40-48 | Com42→ClitoralCap, Com44→Vibrator, Com48→MilkingMachine |
| 調教 | Training/Bondage/ | 100-149 | Com100→Spanking, Com101→Whip, Com105→Rope |
| 調教 | Training/Undressing/ | 200-203 | Com200→UndressTop, Com202→UndressBra |
| 自慰 | Masturbation/ | 600-699 | Com600→SelfCaress, Com644→SelfVibrator |
| - | Utility/ | 400-419 | Com400→Move, Com402→Sleep, Com413→Cook |
| NTR | Visitor/ | 460-490 | Com460→TalkToVisitor, Com490→GoOut |
| - | System/ | (reserved) | Com888, Com999 not yet implemented |

> **Note**: Full mapping to be created during Task 3 execution based on each COM's `Name` property (Japanese) and ERB source documentation.

---

## Dependencies

| Feature | Relationship | Description |
|---------|--------------|-------------|
| F452-F457 | Predecessor | Created the 136 COM files being renamed |
| F458-F459 | Successor | Will use new naming convention |
| F460-F463 | Successor | Phase 12 post-phase features |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - YAML schema to update
- [F452](feature-452.md) - Phase 12 COM Migration (created files)
- [F458](feature-458.md) - Extended Actions Migration (successor)
- [F459](feature-459.md) - Special Processing Migration (successor)
- [F460](feature-460.md) - Phase 7 Technical Debt Resolution
- [F461](feature-461.md) - Phase 9 System Integration (successor)
- [F462](feature-462.md) - Post-Phase Review Phase 12 (successor)
- [F463](feature-463.md) - Phase 13 Planning (successor)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-12 11:44 | START | implementer | Task 1 | - |
| 2026-01-12 11:44 | END | implementer | Task 1 | SUCCESS |
| 2026-01-12 11:45 | START | implementer | Task 2 | - |
| 2026-01-12 11:46 | END | implementer | Task 2 | SUCCESS |
| 2026-01-12 11:50 | START | implementer | Task 3 | - |
| 2026-01-12 11:53 | END | implementer | Task 3 | SUCCESS |
| 2026-01-12 11:56 | START | implementer | Task 5 | - |
| 2026-01-12 11:56 | END | implementer | Task 5 | SUCCESS |

---

## Review Notes

- **2026-01-12 FL iter0**: [resolved] Volume waiver granted: 136 file renames are atomic refactoring operation, cannot be split without breaking build between features. Follows F452 precedent for batch COM operations.
