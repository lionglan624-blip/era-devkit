using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// F759 AC#8: Conversion unit tests for BitwiseComparisonCondition → YAML
/// Tests bitwise_and_cmp operator generation with mask/op/value structure
/// </summary>
public class BitwiseComparisonConversionTests
{
    private readonly string _talentCsvPath;

    public BitwiseComparisonConversionTests()
    {
        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
    }

    /// <summary>
    /// Positive: (TALENT:性別嗜好 & 3) == 3 → YAML with bitwise_and_cmp operator
    /// Verifies YAML structure contains mask, op, and value fields
    /// </summary>
    [Fact]
    public void BitwiseComparisonConversion_TalentToYaml()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "(TALENT:性別嗜好 & 3) == 3";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TALENT", result.Keys);

        var talentDict = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talentDict);
        Assert.Single(talentDict);

        // Extract the index key and verify bitwise_and_cmp operator
        var indexKey = talentDict.Keys.First();
        var opDict = talentDict[indexKey] as Dictionary<string, object>;
        Assert.NotNull(opDict);
        Assert.Contains("bitwise_and_cmp", opDict.Keys);

        // Verify nested structure: { mask: "3", op: "eq", value: "3" }
        var bitwiseCmpDict = opDict["bitwise_and_cmp"] as Dictionary<string, object>;
        Assert.NotNull(bitwiseCmpDict);
        Assert.Equal("3", bitwiseCmpDict["mask"]);
        Assert.Equal("eq", bitwiseCmpDict["op"]);
        Assert.Equal("3", bitwiseCmpDict["value"]);
    }

    /// <summary>
    /// Positive: (CFLAG:奴隷:フラグ & 1) != 0 → YAML with bitwise_and_cmp and ne operator
    /// Verifies CFLAG conversion with correct key construction
    /// </summary>
    [Fact]
    public void BitwiseComparisonConversion_CflagToYaml()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "(CFLAG:奴隷:フラグ & 1) != 0";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("CFLAG", result.Keys);

        var cflagDict = result["CFLAG"] as Dictionary<string, object>;
        Assert.NotNull(cflagDict);
        Assert.Contains("奴隷:フラグ", cflagDict.Keys);

        var opDict = cflagDict["奴隷:フラグ"] as Dictionary<string, object>;
        Assert.NotNull(opDict);
        Assert.Contains("bitwise_and_cmp", opDict.Keys);

        var bitwiseCmpDict = opDict["bitwise_and_cmp"] as Dictionary<string, object>;
        Assert.NotNull(bitwiseCmpDict);
        Assert.Equal("1", bitwiseCmpDict["mask"]);
        Assert.Equal("ne", bitwiseCmpDict["op"]);
        Assert.Equal("0", bitwiseCmpDict["value"]);
    }

    /// <summary>
    /// Negative: Unsupported inner condition type returns null/empty from conversion
    /// The parser should reject non-bitwise inner at parse time (HasBitwiseOperator),
    /// so this tests the defensive fallback in DatalistConverter
    /// </summary>
    [Fact]
    public void BitwiseComparisonConversion_UnsupportedInner_ReturnsEmpty()
    {
        // Arrange - (TALENT:性別嗜好 == 3) == 5 has non-bitwise inner
        // ParseAtomicCondition should reject this at parse time
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "(TALENT:性別嗜好 == 3) == 5";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert - Should return empty dict (parse failure produces null → ParseCondition returns {})
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
