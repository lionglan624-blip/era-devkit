using ErbParser;
using Xunit;

namespace ErbParser.Tests;

public class EquipConditionParserTests
{
    [Fact]
    public void ParseEquipCondition_WithTargetNameAndOperator()
    {
        var parser = new EquipConditionParser();
        var result = parser.ParseEquipCondition("EQUIP:MASTER:下半身上着１ != 0");

        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.Equal("下半身上着１", result.Name);
        Assert.Equal("!=", result.Operator);
        Assert.Equal("0", result.Value);
    }

    [Fact]
    public void ParseEquipCondition_WithTargetAndName()
    {
        var parser = new EquipConditionParser();
        var result = parser.ParseEquipCondition("EQUIP:MASTER:下半身上着１");

        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.Equal("下半身上着１", result.Name);
        Assert.Null(result.Operator);
        Assert.Null(result.Value);
    }

    [Fact]
    public void ParseEquipCondition_WithNumericIndex()
    {
        var parser = new EquipConditionParser();
        var result = parser.ParseEquipCondition("EQUIP:2");

        Assert.NotNull(result);
        Assert.Equal(2, result.Index);
        Assert.Null(result.Target);
        Assert.Null(result.Name);
    }

    [Fact]
    public void ParseEquipCondition_NullInput_ReturnsNull()
    {
        var parser = new EquipConditionParser();
        var result = parser.ParseEquipCondition(null!);
        Assert.Null(result);
    }

    [Fact]
    public void ParseEquipCondition_EmptyString_ReturnsNull()
    {
        var parser = new EquipConditionParser();
        var result = parser.ParseEquipCondition(string.Empty);
        Assert.Null(result);
    }

    [Fact]
    public void ParseEquipCondition_WrongPrefix_ReturnsNull()
    {
        var parser = new EquipConditionParser();
        var result = parser.ParseEquipCondition("TALENT:恋慕");
        Assert.Null(result);
    }

    [Fact]
    public void EquipRef_ImplementsICondition()
    {
        var equipRef = new EquipRef { Target = "MASTER", Name = "下半身上着１" };
        ICondition condition = equipRef;
        Assert.NotNull(condition);
        Assert.IsAssignableFrom<ICondition>(equipRef);
    }
}
