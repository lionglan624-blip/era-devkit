using System;
using System.Collections.Generic;
using System.IO;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

public class FunctionCallConversionTests
{
    private readonly string _talentCsvPath;

    public FunctionCallConversionTests()
    {
        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
    }

    [Fact]
    public void FunctionCall_ProducesStructuredYaml()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);

        // Act
        var result = converter.ParseCondition("HAS_VAGINA(TARGET)");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("FUNCTION"), "Result should contain FUNCTION key");

        var funcDict = result["FUNCTION"] as Dictionary<string, object>;
        Assert.NotNull(funcDict);
        Assert.Equal("HAS_VAGINA", funcDict["name"]);
    }
}
