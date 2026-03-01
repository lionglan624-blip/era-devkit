using ErbParser;
using Xunit;

namespace ErbParser.Tests;

public class ItemConditionParserTests
{
    [Fact]
    public void ParseItemCondition_WithNumericIndex()
    {
        var parser = new ItemConditionParser();
        var result = parser.ParseItemCondition("ITEM:2");

        Assert.NotNull(result);
        Assert.Equal(2, result.Index);
        Assert.Null(result.Target);
        Assert.Null(result.Name);
    }

    [Fact]
    public void ParseItemCondition_WithIndexAndOperator()
    {
        var parser = new ItemConditionParser();
        var result = parser.ParseItemCondition("ITEM:2 != 0");

        Assert.NotNull(result);
        Assert.Equal(2, result.Index);
        Assert.Equal("!=", result.Operator);
        Assert.Equal("0", result.Value);
    }

    [Fact]
    public void ParseItemCondition_WithStringName()
    {
        var parser = new ItemConditionParser();
        var result = parser.ParseItemCondition("ITEM:アイテム名");

        Assert.NotNull(result);
        Assert.Equal("アイテム名", result.Name);
        Assert.Null(result.Index);
    }

    [Fact]
    public void ParseItemCondition_NullInput_ReturnsNull()
    {
        var parser = new ItemConditionParser();
        var result = parser.ParseItemCondition(null!);
        Assert.Null(result);
    }

    [Fact]
    public void ParseItemCondition_EmptyString_ReturnsNull()
    {
        var parser = new ItemConditionParser();
        var result = parser.ParseItemCondition(string.Empty);
        Assert.Null(result);
    }

    [Fact]
    public void ParseItemCondition_WrongPrefix_ReturnsNull()
    {
        var parser = new ItemConditionParser();
        var result = parser.ParseItemCondition("CFLAG:100");
        Assert.Null(result);
    }

    [Fact]
    public void ItemRef_ImplementsICondition()
    {
        var itemRef = new ItemRef { Index = 2 };
        ICondition condition = itemRef;
        Assert.NotNull(condition);
        Assert.IsAssignableFrom<ICondition>(itemRef);
    }
}
