using System;
using System.Collections.Generic;
using System.IO;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

public class ArgConditionConversionTests
{
    private readonly string _talentCsvPath;

    public ArgConditionConversionTests()
    {
        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
    }

    [Fact]
    public void ConvertArgRef_BareArg_ProducesCorrectYaml()
    {
        // "ARG" → { "ARG": { "0": { "ne": "0" } } }
        var converter = new DatalistConverter(_talentCsvPath);
        var result = converter.ParseCondition("ARG");
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("ARG"), "Result should contain ARG key");
        var argDict = result["ARG"] as Dictionary<string, object>;
        Assert.NotNull(argDict);
        Assert.True(argDict.ContainsKey("0"));
        var opDict = argDict["0"] as Dictionary<string, object>;
        Assert.NotNull(opDict);
        Assert.True(opDict.ContainsKey("ne"));
        Assert.Equal("0", opDict["ne"]);
    }

    [Fact]
    public void ConvertArgRef_ArgWithComparison_ProducesCorrectYaml()
    {
        // "ARG == 2" → { "ARG": { "0": { "eq": "2" } } }
        var converter = new DatalistConverter(_talentCsvPath);
        var result = converter.ParseCondition("ARG == 2");
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("ARG"), "Result should contain ARG key");
        var argDict = result["ARG"] as Dictionary<string, object>;
        Assert.NotNull(argDict);
        Assert.True(argDict.ContainsKey("0"));
        var opDict = argDict["0"] as Dictionary<string, object>;
        Assert.NotNull(opDict);
        Assert.True(opDict.ContainsKey("eq"));
        Assert.Equal("2", opDict["eq"]);
    }

    [Fact]
    public void ConvertArgRef_IndexedArg_ProducesCorrectYaml()
    {
        // "ARG:1" → { "ARG": { "1": { "ne": "0" } } }
        var converter = new DatalistConverter(_talentCsvPath);
        var result = converter.ParseCondition("ARG:1");
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("ARG"), "Result should contain ARG key");
        var argDict = result["ARG"] as Dictionary<string, object>;
        Assert.NotNull(argDict);
        Assert.True(argDict.ContainsKey("1"));
        var opDict = argDict["1"] as Dictionary<string, object>;
        Assert.NotNull(opDict);
        Assert.True(opDict.ContainsKey("ne"));
        Assert.Equal("0", opDict["ne"]);
    }

    [Fact]
    public void ConvertArgRef_IndexedArgWithComparison_ProducesCorrectYaml()
    {
        // "ARG:1 == 3" → { "ARG": { "1": { "eq": "3" } } }
        var converter = new DatalistConverter(_talentCsvPath);
        var result = converter.ParseCondition("ARG:1 == 3");
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("ARG"), "Result should contain ARG key");
        var argDict = result["ARG"] as Dictionary<string, object>;
        Assert.NotNull(argDict);
        Assert.True(argDict.ContainsKey("1"));
        var opDict = argDict["1"] as Dictionary<string, object>;
        Assert.NotNull(opDict);
        Assert.True(opDict.ContainsKey("eq"));
        Assert.Equal("3", opDict["eq"]);
    }
}
