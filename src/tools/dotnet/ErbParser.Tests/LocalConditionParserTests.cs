using ErbParser;
using Xunit;

namespace ErbParser.Tests;

public class LocalConditionParserTests
{
    private readonly LocalConditionParser _parser = new();

    // AC#2: BareLocal tests
    [Fact]
    public void ParsesBareLocal_ReturnsIndexNull()
    {
        var result = _parser.Parse("LOCAL");
        Assert.NotNull(result);
        Assert.Null(result.Index); // bare LOCAL = null index (implicit 0)
        Assert.Null(result.Operator);
        Assert.Null(result.Value);
    }

    [Fact]
    public void ParsesBareLocalWithOperator_ReturnsComparison()
    {
        var result = _parser.Parse("LOCAL == 1");
        Assert.NotNull(result);
        Assert.Null(result.Index);
        Assert.Equal("==", result.Operator);
        Assert.Equal("1", result.Value);
    }

    [Fact]
    public void ParsesBareLocalNotEquals_ReturnsComparison()
    {
        var result = _parser.Parse("LOCAL != 0");
        Assert.NotNull(result);
        Assert.Null(result.Index);
        Assert.Equal("!=", result.Operator);
        Assert.Equal("0", result.Value);
    }

    // AC#3: IndexedLocal tests
    [Fact]
    public void ParsesIndexedLocal_ReturnsIndex()
    {
        var result = _parser.Parse("LOCAL:1");
        Assert.NotNull(result);
        Assert.Equal(1, result.Index);
        Assert.Null(result.Operator);
        Assert.Null(result.Value);
    }

    [Fact]
    public void ParsesIndexedLocalWithComparison_ReturnsIndexAndOperator()
    {
        var result = _parser.Parse("LOCAL:1 == 0");
        Assert.NotNull(result);
        Assert.Equal(1, result.Index);
        Assert.Equal("==", result.Operator);
        Assert.Equal("0", result.Value);
    }

    [Fact]
    public void ParsesIndexedLocalNotEquals_ReturnsComparison()
    {
        var result = _parser.Parse("LOCAL:2 != 1");
        Assert.NotNull(result);
        Assert.Equal(2, result.Index);
        Assert.Equal("!=", result.Operator);
        Assert.Equal("1", result.Value);
    }

    // AC#4: RejectsNonLocal tests
    [Theory]
    [InlineData("CFLAG:0:100")]
    [InlineData("TALENT:恋人")]
    [InlineData("LOCALS")]
    [InlineData("")]
    public void RejectsNonLocal_ReturnsNull(string input)
    {
        var result = _parser.Parse(input);
        Assert.Null(result);
    }

    [Fact]
    public void RejectsNonLocal_NullInput_ReturnsNull()
    {
        var result = _parser.Parse(null!);
        Assert.Null(result);
    }

    // All operators (like ArgConditionParserTests)
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
        var result = _parser.Parse($"LOCAL {op} 5");
        Assert.NotNull(result);
        Assert.Equal(op, result.Operator);
        Assert.Equal("5", result.Value);
    }

    // AC#5: CompoundLocal tests (require registration in LogicalOperatorParser)
    [Fact]
    public void ParsesCompoundLocalWithTalent_ReturnsLogicalOp()
    {
        var parser = new LogicalOperatorParser();
        var result = parser.ParseLogicalExpression("LOCAL:1 && TALENT:恋人");
        Assert.NotNull(result);
        Assert.IsType<LogicalOp>(result);
        var logicalOp = (LogicalOp)result;
        Assert.Equal("&&", logicalOp.Operator);
        Assert.IsType<LocalRef>(logicalOp.Left);
        Assert.IsType<TalentRef>(logicalOp.Right);
    }

    [Fact]
    public void ParsesCompoundLocalWithOperator_ReturnsLogicalOp()
    {
        var parser = new LogicalOperatorParser();
        var result = parser.ParseLogicalExpression("LOCAL:1 && ARG == 2");
        Assert.NotNull(result);
        Assert.IsType<LogicalOp>(result);
        var logicalOp = (LogicalOp)result;
        Assert.Equal("&&", logicalOp.Operator);
        Assert.IsType<LocalRef>(logicalOp.Left);
        Assert.IsType<ArgRef>(logicalOp.Right);
    }
}
