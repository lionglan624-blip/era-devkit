using System;
using System.Collections.Generic;
using System.IO;
using ErbToYaml;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ErbToYaml.Tests;

/// <summary>
/// F757 AC#17,23,28,29,32: Bitwise operator YAML conversion tests
/// Verifies bitwise & operator converts to bitwise_and in YAML
/// </summary>
public class BitwiseConversionTests
{
    private readonly IDeserializer _yamlDeserializer;
    private readonly string _talentCsvPath;

    public BitwiseConversionTests()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
    }

    /// <summary>
    /// AC#17: Test STAIN bitwise → YAML output with bitwise_and operator through ConvertStainRef
    /// Input: STAIN:口 & 汚れ_精液
    /// Expected: { STAIN: { "口": { bitwise_and: "汚れ_精液" } } }
    /// </summary>
    [Fact]
    public void BitwiseConversion_StainToYaml()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "STAIN:口 & 汚れ_精液";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("STAIN", result.Keys);

        var stainDict = result["STAIN"] as Dictionary<string, object>;
        Assert.NotNull(stainDict);
        Assert.Contains("口", stainDict.Keys);

        var opDict = stainDict["口"] as Dictionary<string, object>;
        Assert.NotNull(opDict);
        Assert.Contains("bitwise_and", opDict.Keys);
        Assert.Equal("汚れ_精液", opDict["bitwise_and"]);
    }

    /// <summary>
    /// AC#23: Test DIM CONST resolution: STAIN:口 & 汚れ_精液 with mock IDimConstResolver
    /// resolving 汚れ_精液 → 4 produces YAML with bitwise_and: "4" (numeric)
    /// </summary>
    [Fact]
    public void BitwiseConversion_DimConstResolution()
    {
        // Arrange
        var mockResolver = new MockDimConstResolver(new Dictionary<string, int>
        {
            { "汚れ_精液", 4 }
        });
        var converter = new DatalistConverter(_talentCsvPath, mockResolver);
        var condition = "STAIN:口 & 汚れ_精液";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("STAIN", result.Keys);

        var stainDict = result["STAIN"] as Dictionary<string, object>;
        Assert.NotNull(stainDict);
        Assert.Contains("口", stainDict.Keys);

        var opDict = stainDict["口"] as Dictionary<string, object>;
        Assert.NotNull(opDict);
        Assert.Contains("bitwise_and", opDict.Keys);
        Assert.Equal("4", opDict["bitwise_and"]); // Should be resolved to numeric string "4"
    }

    /// <summary>
    /// AC#28: Test TALENT bitwise → YAML through ConvertTalentRef: TALENT:性別嗜好 & 1
    /// Note: This test requires 性別嗜好 to exist in Talent.csv
    /// Expected: { TALENT: { "index": { bitwise_and: "1" } } } where index is the CSV index
    /// </summary>
    [Fact]
    public void BitwiseConversion_TalentToYaml()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "TALENT:性別嗜好 & 1";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TALENT", result.Keys);

        var talentDict = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talentDict);
        // Should have one entry with the talent index
        Assert.Single(talentDict);

        // Extract the index and verify bitwise_and operator
        var indexKey = talentDict.Keys.First();
        var opDict = talentDict[indexKey] as Dictionary<string, object>;
        Assert.NotNull(opDict);
        Assert.Contains("bitwise_and", opDict.Keys);
        Assert.Equal("1", opDict["bitwise_and"]);
    }

    /// <summary>
    /// AC#29: Test CFLAG bitwise → YAML through ConvertCflagRef: CFLAG:奴隷:前回売春フラグ & 前回売春_初売春
    /// Expected: { CFLAG: { "奴隷:前回売春フラグ": { bitwise_and: "前回売春_初売春" } } }
    /// </summary>
    [Fact]
    public void BitwiseConversion_CflagToYaml()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "CFLAG:奴隷:前回売春フラグ & 前回売春_初売春";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("CFLAG", result.Keys);

        var cflagDict = result["CFLAG"] as Dictionary<string, object>;
        Assert.NotNull(cflagDict);
        Assert.Contains("奴隷:前回売春フラグ", cflagDict.Keys);

        var opDict = cflagDict["奴隷:前回売春フラグ"] as Dictionary<string, object>;
        Assert.NotNull(opDict);
        Assert.Contains("bitwise_and", opDict.Keys);
        Assert.Equal("前回売春_初売春", opDict["bitwise_and"]);
    }

    /// <summary>
    /// AC#32: Integration test loading real Game/ERB/DIM.ERH and verifying known constants:
    /// 汚れ_精液 → 4, 前回売春_初売春 → 1
    /// </summary>
    [Fact]
    public void BitwiseConversion_RealDimConstResolver()
    {
        // Arrange
        var dimErhPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("ERB", "DIM.ERH");

        // Skip test if DIM.ERH not found
        if (!File.Exists(dimErhPath))
        {
            // In CI or environments without Game folder, skip gracefully
            return;
        }

        var dimResolver = new DimConstResolver();
        dimResolver.LoadFromFile(dimErhPath);
        var converter = new DatalistConverter(_talentCsvPath, dimResolver);

        // Act - Test 汚れ_精液 → 4
        var stainCondition = "STAIN:口 & 汚れ_精液";
        var stainResult = converter.ParseCondition(stainCondition);

        // Assert STAIN
        Assert.NotNull(stainResult);
        Assert.Contains("STAIN", stainResult.Keys);
        var stainDict = stainResult["STAIN"] as Dictionary<string, object>;
        Assert.NotNull(stainDict);
        var stainOpDict = stainDict["口"] as Dictionary<string, object>;
        Assert.NotNull(stainOpDict);
        Assert.Contains("bitwise_and", stainOpDict.Keys);
        Assert.Equal("4", stainOpDict["bitwise_and"]); // 汚れ_精液 should resolve to 4

        // Act - Test 前回売春_初売春 → 1
        var cflagCondition = "CFLAG:奴隷:前回売春フラグ & 前回売春_初売春";
        var cflagResult = converter.ParseCondition(cflagCondition);

        // Assert CFLAG
        Assert.NotNull(cflagResult);
        Assert.Contains("CFLAG", cflagResult.Keys);
        var cflagDict = cflagResult["CFLAG"] as Dictionary<string, object>;
        Assert.NotNull(cflagDict);
        var cflagOpDict = cflagDict["奴隷:前回売春フラグ"] as Dictionary<string, object>;
        Assert.NotNull(cflagOpDict);
        Assert.Contains("bitwise_and", cflagOpDict.Keys);
        Assert.Equal("1", cflagOpDict["bitwise_and"]); // 前回売春_初売春 should resolve to 1
    }

    /// <summary>
    /// Mock implementation of IDimConstResolver for testing
    /// </summary>
    private sealed class MockDimConstResolver : IDimConstResolver
    {
        private readonly Dictionary<string, int> _constants;

        public MockDimConstResolver(Dictionary<string, int> constants)
        {
            _constants = constants;
        }

        public int? Resolve(string name)
        {
            return _constants.TryGetValue(name, out var value) ? value : null;
        }

        public string ResolveToString(string value)
        {
            // Try to parse as int first - if it's already numeric, return as-is
            if (int.TryParse(value, out _))
                return value;

            // Otherwise try to resolve as constant name
            var resolved = Resolve(value);
            return resolved?.ToString() ?? value;
        }
    }
}
