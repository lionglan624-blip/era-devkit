using System;
using System.Collections.Generic;
using System.IO;
using ErbToYaml;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ErbToYaml.Tests;

/// <summary>
/// Feature 755: Tests for compound condition support (CFLAG/TCVAR/TALENT with logical operators)
/// AC#4,5,6,7,13: Validates ICondition-to-YAML conversion with tree flattening
/// </summary>
public class CompoundConditionTests
{
    private readonly IDeserializer _yamlDeserializer;
    private readonly string _talentCsvPath;

    public CompoundConditionTests()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
    }

    /// <summary>
    /// AC#4: Test compound TALENT condition with AND operator
    /// Expected: TALENT:恋慕 && TALENT:淫乱 → { AND: [{ TALENT: { 3: { ne: 0 } } }, { TALENT: { 4: { ne: 0 } } }] }
    /// </summary>
    [Fact]
    public void CompoundCondition_And()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "TALENT:恋慕 && TALENT:淫乱";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("AND", result.Keys);

        var andList = result["AND"] as List<object>;
        Assert.NotNull(andList);
        Assert.Equal(2, andList.Count);

        // Verify first condition: TALENT:恋慕 (index 3)
        var firstCond = andList[0] as Dictionary<string, object>;
        Assert.NotNull(firstCond);
        Assert.Contains("TALENT", firstCond.Keys);

        var firstTalent = firstCond["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(firstTalent);
        Assert.Contains("3", firstTalent.Keys);

        // Verify second condition: TALENT:淫乱 (index 4)
        var secondCond = andList[1] as Dictionary<string, object>;
        Assert.NotNull(secondCond);
        Assert.Contains("TALENT", secondCond.Keys);

        var secondTalent = secondCond["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(secondTalent);
        Assert.Contains("4", secondTalent.Keys);
    }

    /// <summary>
    /// AC#5: Test compound condition with TCVAR
    /// Expected: TCVAR:302 != 0 && TALENT:恋慕 → { AND: [{ TCVAR: { "302": { ne: 0 } } }, { TALENT: { 3: { ne: 0 } } }] }
    /// </summary>
    [Fact]
    public void CompoundCondition_Tcvar()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "TCVAR:302 != 0 && TALENT:恋慕";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("AND", result.Keys);

        var andList = result["AND"] as List<object>;
        Assert.NotNull(andList);
        Assert.Equal(2, andList.Count);

        // Verify first condition: TCVAR:302
        var firstCond = andList[0] as Dictionary<string, object>;
        Assert.NotNull(firstCond);
        Assert.Contains("TCVAR", firstCond.Keys);

        var tcvar = firstCond["TCVAR"] as Dictionary<string, object>;
        Assert.NotNull(tcvar);
        Assert.Contains("302", tcvar.Keys);

        // Verify second condition: TALENT:恋慕 (index 3)
        var secondCond = andList[1] as Dictionary<string, object>;
        Assert.NotNull(secondCond);
        Assert.Contains("TALENT", secondCond.Keys);

        var talent = secondCond["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talent);
        Assert.Contains("3", talent.Keys);
    }

    /// <summary>
    /// AC#6: Test negated condition
    /// Expected: !TALENT:恋慕 → { NOT: { TALENT: { 3: { ne: 0 } } } }
    /// </summary>
    [Fact]
    public void CompoundCondition_Not()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "!TALENT:恋慕";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("NOT", result.Keys);

        var notContent = result["NOT"] as Dictionary<string, object>;
        Assert.NotNull(notContent);
        Assert.Contains("TALENT", notContent.Keys);

        var talent = notContent["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talent);
        Assert.Contains("3", talent.Keys);
    }

    /// <summary>
    /// AC#13: Test tree flattening for chained AND operations
    /// Expected: TALENT:恋慕 && TALENT:淫乱 && TALENT:服従 → { AND: [talent1, talent2, talent3] }
    /// Should produce flat array, not nested AND nodes
    /// </summary>
    [Fact]
    public void CompoundCondition_TreeFlattening()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "TALENT:恋慕 && TALENT:淫乱 && TALENT:服従";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("AND", result.Keys);

        var andList = result["AND"] as List<object>;
        Assert.NotNull(andList);

        // Tree flattening: should have 3 items at top level, not nested structure
        Assert.Equal(3, andList.Count);

        // Verify all three are TALENT conditions (not nested AND)
        foreach (var item in andList)
        {
            var cond = item as Dictionary<string, object>;
            Assert.NotNull(cond);
            Assert.Contains("TALENT", cond.Keys);
        }
    }

    /// <summary>
    /// AC#7: Test backward compatibility with single TALENT condition
    /// Expected: TALENT:恋慕 → { TALENT: { 3: { ne: 0 } } }
    /// Output should match pre-F755 format (no AND/OR wrapper)
    /// </summary>
    [Fact]
    public void SingleTalentCondition_BackwardCompat()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        var condition = "TALENT:恋慕";

        // Act
        var result = converter.ParseCondition(condition);

        // Assert
        Assert.NotNull(result);

        // Should have single TALENT key (not AND/OR)
        Assert.Contains("TALENT", result.Keys);
        Assert.DoesNotContain("AND", result.Keys);
        Assert.DoesNotContain("OR", result.Keys);

        var talent = result["TALENT"] as Dictionary<string, object>;
        Assert.NotNull(talent);
        Assert.Contains("3", talent.Keys);

        var talentValue = talent["3"] as Dictionary<string, object>;
        Assert.NotNull(talentValue);
        Assert.Contains("ne", talentValue.Keys);
        Assert.Equal("0", talentValue["ne"]);
    }
}
