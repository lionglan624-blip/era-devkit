using ErbParser;
using Xunit;

namespace ErbParser.Tests;

[Trait("Category", "Unit")]
public class StainConditionParserTests
{
    private readonly StainConditionParser _parser = new();

    [Fact]
    public void Parse_NumericIndex_ReturnsStainRefWithIndex()
    {
        var result = _parser.Parse("STAIN:5");

        Assert.NotNull(result);
        Assert.Equal(5, result.Index);
        Assert.Null(result.Name);
        Assert.Null(result.Target);
        Assert.Null(result.Operator);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Parse_ZeroIndex_ReturnsStainRefWithZeroIndex()
    {
        var result = _parser.Parse("STAIN:0");

        Assert.NotNull(result);
        Assert.Equal(0, result.Index);
        Assert.Null(result.Name);
    }

    [Fact]
    public void Parse_StringName_ReturnsStainRefWithName()
    {
        var result = _parser.Parse("STAIN:汚れ");

        Assert.NotNull(result);
        Assert.Equal("汚れ", result.Name);
        Assert.Null(result.Index);
    }

    [Fact]
    public void Parse_TargetAndIndex_ReturnsStainRefWithTargetAndIndex()
    {
        var result = _parser.Parse("STAIN:MASTER:3");

        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.Equal(3, result.Index);
        Assert.Null(result.Name);
    }

    [Fact]
    public void Parse_TargetAndName_ReturnsStainRefWithTargetAndName()
    {
        var result = _parser.Parse("STAIN:TARGET:汚れ度");

        Assert.NotNull(result);
        Assert.Equal("TARGET", result.Target);
        Assert.Equal("汚れ度", result.Name);
        Assert.Null(result.Index);
    }

    [Theory]
    [InlineData("==", "1")]
    [InlineData("!=", "0")]
    [InlineData(">", "5")]
    [InlineData(">=", "10")]
    [InlineData("<", "3")]
    [InlineData("<=", "7")]
    public void Parse_WithOperator_ReturnsCorrectOperatorAndValue(string op, string value)
    {
        // Regex requires space before operators (except &) to delimit nameOrIndex
        var result = _parser.Parse($"STAIN:5 {op} {value}");

        Assert.NotNull(result);
        Assert.Equal(5, result.Index);
        Assert.Equal(op, result.Operator);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Parse_BitwiseAnd_NoSpaceRequired()
    {
        // & is excluded from nameOrIndex char class, so no space needed
        var result = _parser.Parse("STAIN:5&2");

        Assert.NotNull(result);
        Assert.Equal(5, result.Index);
        Assert.Equal("&", result.Operator);
        Assert.Equal("2", result.Value);
    }

    [Fact]
    public void Parse_WithOperatorAndSpaces_TrimsValue()
    {
        var result = _parser.Parse("STAIN:5 == 1");

        Assert.NotNull(result);
        Assert.Equal("==", result.Operator);
        Assert.Equal("1", result.Value);
    }

    [Fact]
    public void Parse_NullInput_ReturnsNull()
    {
        var result = _parser.Parse(null!);
        Assert.Null(result);
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsNull()
    {
        var result = _parser.Parse("");
        Assert.Null(result);
    }

    [Fact]
    public void Parse_WhitespaceInput_ReturnsNull()
    {
        var result = _parser.Parse("   ");
        Assert.Null(result);
    }

    [Theory]
    [InlineData("CFLAG:5")]
    [InlineData("FLAG:100")]
    [InlineData("TALENT:1")]
    [InlineData("EXP:3")]
    [InlineData("PALAM:7")]
    [InlineData("NOTAPREFIX:1")]
    public void Parse_WrongPrefix_ReturnsNull(string condition)
    {
        var result = _parser.Parse(condition);
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsStainRefType()
    {
        var result = _parser.Parse("STAIN:1");

        Assert.NotNull(result);
        Assert.IsType<StainRef>(result);
    }

    [Fact]
    public void Parse_InputWithLeadingTrailingWhitespace_StillParses()
    {
        var result = _parser.Parse("  STAIN:5  ");

        Assert.NotNull(result);
        Assert.Equal(5, result.Index);
    }

    [Fact]
    public void Parse_HighIndex_ReturnsCorrectIndex()
    {
        var result = _parser.Parse("STAIN:999");

        Assert.NotNull(result);
        Assert.Equal(999, result.Index);
    }

    [Fact]
    public void Parse_TargetWithIndexAndOperator_AllFieldsPopulated()
    {
        var result = _parser.Parse("STAIN:MASTER:3 >= 10");

        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.Equal(3, result.Index);
        Assert.Equal(">=", result.Operator);
        Assert.Equal("10", result.Value);
    }
}
