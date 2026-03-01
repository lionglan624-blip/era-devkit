using ErbParser;
using Xunit;

namespace ErbParser.Tests;

public class ArgConditionParserTests
{
    private readonly ArgConditionParser _parser = new();

    [Fact]
    public void Parse_BareArg_ReturnsIndex0NoOperator()
    {
        var result = _parser.Parse("ARG");
        Assert.NotNull(result);
        Assert.Equal(0, result.Index);
        Assert.Null(result.Operator);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Parse_ArgWithComparison_ReturnsIndex0WithOperatorAndValue()
    {
        var result = _parser.Parse("ARG == 2");
        Assert.NotNull(result);
        Assert.Equal(0, result.Index);
        Assert.Equal("==", result.Operator);
        Assert.Equal("2", result.Value);
    }

    [Fact]
    public void Parse_IndexedArg_ReturnsIndexNoOperator()
    {
        var result = _parser.Parse("ARG:1");
        Assert.NotNull(result);
        Assert.Equal(1, result.Index);
        Assert.Null(result.Operator);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Parse_IndexedArgWithComparison_ReturnsIndexWithOperatorAndValue()
    {
        var result = _parser.Parse("ARG:1 == 3");
        Assert.NotNull(result);
        Assert.Equal(1, result.Index);
        Assert.Equal("==", result.Operator);
        Assert.Equal("3", result.Value);
    }

    [Theory]
    [InlineData("==")]
    [InlineData("!=")]
    [InlineData(">")]
    [InlineData(">=")]
    [InlineData("<")]
    [InlineData("<=")]
    [InlineData("&")]
    public void Parse_AllOperators_ReturnsCorrectOperator(string op)
    {
        var result = _parser.Parse($"ARG {op} 5");
        Assert.NotNull(result);
        Assert.Equal(op, result.Operator);
        Assert.Equal("5", result.Value);
    }

    [Fact]
    public void Parse_NullInput_ReturnsNull()
    {
        var result = _parser.Parse(null!);
        Assert.Null(result);
    }

    [Fact]
    public void Parse_EmptyString_ReturnsNull()
    {
        var result = _parser.Parse("");
        Assert.Null(result);
    }

    [Theory]
    [InlineData("CFLAG:1")]
    [InlineData("LOCAL:1")]
    [InlineData("FLAG:test")]
    public void Parse_NonArgPrefix_ReturnsNull(string input)
    {
        var result = _parser.Parse(input);
        Assert.Null(result);
    }
}
