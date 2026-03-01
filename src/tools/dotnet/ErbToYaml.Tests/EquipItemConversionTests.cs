using System;
using System.Collections.Generic;
using System.IO;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

public class EquipItemConversionTests
{
    private readonly string _talentCsvPath;

    public EquipItemConversionTests()
    {
        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
    }

    [Fact]
    public void EquipRef_ProducesStructuredYaml()
    {
        var converter = new DatalistConverter(_talentCsvPath);
        var result = converter.ParseCondition("EQUIP:MASTER:下半身上着１ != 0");

        Assert.NotNull(result);
        Assert.True(result.ContainsKey("EQUIP"), "Result should contain EQUIP key");

        var equipDict = result["EQUIP"] as Dictionary<string, object>;
        Assert.NotNull(equipDict);
        Assert.True(equipDict.ContainsKey("MASTER:下半身上着１"));
    }

    [Fact]
    public void ItemRef_ProducesStructuredYaml()
    {
        var converter = new DatalistConverter(_talentCsvPath);
        var result = converter.ParseCondition("ITEM:2 != 0");

        Assert.NotNull(result);
        Assert.True(result.ContainsKey("ITEM"), "Result should contain ITEM key");

        var itemDict = result["ITEM"] as Dictionary<string, object>;
        Assert.NotNull(itemDict);
        Assert.True(itemDict.ContainsKey("2"));
    }
}
