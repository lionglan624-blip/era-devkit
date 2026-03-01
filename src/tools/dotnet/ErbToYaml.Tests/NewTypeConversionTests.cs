using System;
using System.Collections.Generic;
using System.IO;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

public class NewTypeConversionTests
{
    private readonly string _talentCsvPath;

    public NewTypeConversionTests()
    {
        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
    }

    [Fact]
    public void MarkRef_ProducesStructuredYaml()
    {
        // Parse "MARK:MASTER:好感度" → result["MARK"]["MASTER:好感度"] exists
        var converter = new DatalistConverter(_talentCsvPath);
        var result = converter.ParseCondition("MARK:MASTER:好感度");
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("MARK"), "Result should contain MARK key");
        var markDict = result["MARK"] as Dictionary<string, object>;
        Assert.NotNull(markDict);
        Assert.True(markDict.ContainsKey("MASTER:好感度"));
    }

    [Fact]
    public void TflagRef_ProducesStructuredYaml()
    {
        // Parse "TFLAG:コマンド成功度 == 1" → result["TFLAG"]["コマンド成功度"] exists
        var converter = new DatalistConverter(_talentCsvPath);
        var result = converter.ParseCondition("TFLAG:コマンド成功度 == 1");
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("TFLAG"), "Result should contain TFLAG key");
        var tflagDict = result["TFLAG"] as Dictionary<string, object>;
        Assert.NotNull(tflagDict);
        Assert.True(tflagDict.ContainsKey("コマンド成功度"));
    }
}
