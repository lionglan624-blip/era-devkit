using System;
using System.Collections.Generic;
using System.IO;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// Feature 760: Tests for TALENT target/numeric index pattern support
/// AC#6-9, AC#21: Validates ConvertTalentRef and ResolveInnerBitwiseRef with three conversion paths
///
/// TDD RED Phase: These tests will FAIL because:
/// - TalentRef has no Index property yet (AC#7, AC#9 numeric patterns)
/// - ConvertTalentRef only handles Name-based CSV lookup (AC#7, AC#8)
/// - ResolveInnerBitwiseRef only uses talent.Name for CSV lookup (AC#21)
/// </summary>
public class TalentConversionTests
{
    private readonly string _talentCsvPath;

    public TalentConversionTests()
    {
        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
    }

    /// <summary>
    /// AC#6: ConvertTalentRef Name path uses CSV lookup
    /// Expected: TALENT:恋慕 → { "TALENT": { "3": { "ne": "0" } } }
    /// This is existing behavior and should PASS
    /// </summary>
    [Fact]
    public void ConvertTalentRef_NamePath_UsesCsvLookup()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "TALENT:恋慕";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TALENT", result.Keys);

        var talentDict = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talentDict);

        // 恋慕 is index 3 in Talent.csv
        Assert.Contains("3", talentDict.Keys);

        var operatorDict = talentDict["3"] as Dictionary<string, object>;
        Assert.NotNull(operatorDict);
        Assert.Contains("ne", operatorDict.Keys);
        Assert.Equal("0", operatorDict["ne"]);
    }

    /// <summary>
    /// AC#7: ConvertTalentRef Index path uses direct index
    /// Expected: TALENT:2 → { "TALENT": { "2": { "ne": "0" } } }
    /// Currently FAILS: parser puts "2" in Name, CSV lookup fails, returns empty dict
    /// After F760: parser produces Index=2, ConvertTalentRef uses direct index
    /// </summary>
    [Fact]
    public void ConvertTalentRef_IndexPath_UsesDirectIndex()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "TALENT:2";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TALENT", result.Keys);

        var talentDict = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talentDict);

        // Direct numeric index (no CSV lookup)
        Assert.Contains("2", talentDict.Keys);

        var operatorDict = talentDict["2"] as Dictionary<string, object>;
        Assert.NotNull(operatorDict);
        Assert.Contains("ne", operatorDict.Keys);
        Assert.Equal("0", operatorDict["ne"]);
    }

    /// <summary>
    /// AC#8: ConvertTalentRef Target-only path preserves symbolic reference
    /// Expected: TALENT:PLAYER & 2 → { "TALENT": { "PLAYER": { "bitwise_and": "2" } } }
    /// Currently FAILS: PLAYER goes to Name, not Target
    /// After F760: parser produces Target=PLAYER, ConvertTalentRef preserves symbolic key
    ///
    /// Known limitation: TALENT:PLAYER evaluates to default-0 until F769 runtime state injection
    /// </summary>
    [Fact]
    public void ConvertTalentRef_TargetOnlyPath_PreservesSymbolicReference()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "TALENT:PLAYER & 2";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TALENT", result.Keys);

        var talentDict = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talentDict);

        // Symbolic target reference preserved (not CSV resolved)
        Assert.Contains("PLAYER", talentDict.Keys);

        var operatorDict = talentDict["PLAYER"] as Dictionary<string, object>;
        Assert.NotNull(operatorDict);
        Assert.Contains("bitwise_and", operatorDict.Keys);
        Assert.Equal("2", operatorDict["bitwise_and"]);
    }

    /// <summary>
    /// AC#9: Three-part pattern regression - Keyword target + Name (CSV lookup)
    /// Expected: TALENT:PLAYER:処女 → { "TALENT": { "PLAYER:0": { "ne": "0" } } }
    /// Currently FAILS: produces { "TALENT": { "0": { "ne": "0" } } } (target lost)
    /// After F760: ConvertTalentRef produces compound key "PLAYER:0"
    /// </summary>
    [Fact]
    public void ConvertTalentRef_ThreePartKeywordTarget_CompoundKey()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "TALENT:PLAYER:処女";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TALENT", result.Keys);

        var talentDict = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talentDict);

        // Compound key preserves target + CSV-resolved index
        // 処女 is index 0 in Talent.csv
        Assert.Contains("PLAYER:0", talentDict.Keys);

        var operatorDict = talentDict["PLAYER:0"] as Dictionary<string, object>;
        Assert.NotNull(operatorDict);
        Assert.Contains("ne", operatorDict.Keys);
        Assert.Equal("0", operatorDict["ne"]);
    }

    /// <summary>
    /// AC#9: Three-part pattern regression - Keyword target + Numeric index
    /// Expected: TALENT:PLAYER:2 → { "TALENT": { "PLAYER:2": { "ne": "0" } } }
    /// Currently FAILS: parser doesn't disambiguate numeric index in three-part patterns
    /// After F760: ConvertTalentRef produces compound key "PLAYER:2"
    /// </summary>
    [Fact]
    public void ConvertTalentRef_ThreePartKeywordTargetNumericIndex_CompoundKey()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "TALENT:PLAYER:2";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TALENT", result.Keys);

        var talentDict = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talentDict);

        // Compound key preserves target + direct index
        Assert.Contains("PLAYER:2", talentDict.Keys);

        var operatorDict = talentDict["PLAYER:2"] as Dictionary<string, object>;
        Assert.NotNull(operatorDict);
        Assert.Contains("ne", operatorDict.Keys);
        Assert.Equal("0", operatorDict["ne"]);
    }

    /// <summary>
    /// AC#9: Three-part pattern regression - Numeric target + Name (backward compat)
    /// Expected: TALENT:5:NTR → { "TALENT": { "6": { "ne": "0" } } }
    /// Numeric target is character index and should be discarded (backward compatible)
    /// Should PASS: existing behavior preserved (Name="NTR" → CSV lookup)
    /// </summary>
    [Fact]
    public void ConvertTalentRef_ThreePartNumericTarget_DiscardsTarget()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "TALENT:5:NTR";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TALENT", result.Keys);

        var talentDict = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talentDict);

        // Numeric target discarded, uses CSV lookup on Name="NTR"
        // NTR is index 6 in Talent.csv
        Assert.Contains("6", talentDict.Keys);

        var operatorDict = talentDict["6"] as Dictionary<string, object>;
        Assert.NotNull(operatorDict);
        Assert.Contains("ne", operatorDict.Keys);
        Assert.Equal("0", operatorDict["ne"]);
    }

    /// <summary>
    /// AC#21: ResolveInnerBitwiseRef handles TalentRef.Index (direct index)
    /// Expected: (TALENT:2 & 3) == 3 → compound bitwise with Index=2
    /// Currently FAILS: parser produces Name="2", ResolveInnerBitwiseRef CSV lookup fails
    /// After F760: ResolveInnerBitwiseRef uses Index directly for key resolution
    /// </summary>
    [Fact]
    public void ResolveInnerBitwiseRef_TalentRefIndex_UsesDirectIndex()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "(TALENT:2 & 3) == 3";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TALENT", result.Keys);

        var talentDict = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talentDict);

        Assert.Contains("2", talentDict.Keys);

        var keyDict = talentDict["2"] as Dictionary<string, object>;
        Assert.NotNull(keyDict);
        Assert.Contains("bitwise_and_cmp", keyDict.Keys);

        var bitwiseCmpDict = keyDict["bitwise_and_cmp"] as Dictionary<string, object>;
        Assert.NotNull(bitwiseCmpDict);
        Assert.Contains("mask", bitwiseCmpDict.Keys);
        Assert.Equal("3", bitwiseCmpDict["mask"]);
        Assert.Contains("op", bitwiseCmpDict.Keys);
        Assert.Equal("eq", bitwiseCmpDict["op"]);
        Assert.Contains("value", bitwiseCmpDict.Keys);
        Assert.Equal("3", bitwiseCmpDict["value"]);
    }

    /// <summary>
    /// AC#21: ResolveInnerBitwiseRef handles TalentRef.Name (CSV lookup, existing behavior)
    /// Expected: (TALENT:NTR & 1) == 1 → compound bitwise with Name="NTR"
    /// Should PASS: existing CSV lookup path preserved
    /// </summary>
    [Fact]
    public void ResolveInnerBitwiseRef_TalentRefName_UsesCsvLookup()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "(TALENT:NTR & 1) == 1";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TALENT", result.Keys);

        var talentDict = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talentDict);

        // CSV lookup: NTR is index 6 in Talent.csv
        Assert.Contains("6", talentDict.Keys);

        var keyDict = talentDict["6"] as Dictionary<string, object>;
        Assert.NotNull(keyDict);
        Assert.Contains("bitwise_and_cmp", keyDict.Keys);

        var bitwiseCmpDict = keyDict["bitwise_and_cmp"] as Dictionary<string, object>;
        Assert.NotNull(bitwiseCmpDict);
        Assert.Contains("mask", bitwiseCmpDict.Keys);
        Assert.Equal("1", bitwiseCmpDict["mask"]);
        Assert.Contains("op", bitwiseCmpDict.Keys);
        Assert.Equal("eq", bitwiseCmpDict["op"]);
        Assert.Contains("value", bitwiseCmpDict.Keys);
        Assert.Equal("1", bitwiseCmpDict["value"]);
    }

    /// <summary>
    /// AC#21: ResolveInnerBitwiseRef handles TalentRef with Target (compound key)
    /// Expected: (TALENT:PLAYER:2 & 3) == 3 → compound bitwise with compound key "PLAYER:2"
    /// Currently FAILS: parser doesn't produce Target+Index correctly
    /// After F760: ResolveInnerBitwiseRef produces compound key for target-qualified patterns
    /// </summary>
    [Fact]
    public void ResolveInnerBitwiseRef_TalentRefTarget_CompoundKey()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "(TALENT:PLAYER:2 & 3) == 3";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TALENT", result.Keys);

        var talentDict = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talentDict);

        // Compound key preserves target + index
        Assert.Contains("PLAYER:2", talentDict.Keys);

        var keyDict = talentDict["PLAYER:2"] as Dictionary<string, object>;
        Assert.NotNull(keyDict);
        Assert.Contains("bitwise_and_cmp", keyDict.Keys);

        var bitwiseCmpDict = keyDict["bitwise_and_cmp"] as Dictionary<string, object>;
        Assert.NotNull(bitwiseCmpDict);
        Assert.Contains("mask", bitwiseCmpDict.Keys);
        Assert.Equal("3", bitwiseCmpDict["mask"]);
        Assert.Contains("op", bitwiseCmpDict.Keys);
        Assert.Equal("eq", bitwiseCmpDict["op"]);
        Assert.Contains("value", bitwiseCmpDict.Keys);
        Assert.Equal("3", bitwiseCmpDict["value"]);
    }
}
