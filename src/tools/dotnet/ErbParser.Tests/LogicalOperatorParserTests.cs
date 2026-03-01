using ErbParser;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#6: Logical operator parsing tests
/// Verifies LogicalOperatorParser handles && and || operators
/// </summary>
public class LogicalOperatorParserTests
{
    /// <summary>
    /// AC#6.1: Parse AND operator (&&)
    /// Pattern: condition1 && condition2
    /// </summary>
    [Fact]
    public void ParseAndOr_AndOperator()
    {
        // Arrange
        var parser = new LogicalOperatorParser();
        var condition = "TALENT:恋人 && CFLAG:MASTER:100";

        // Act
        var result = parser.ParseLogicalExpression(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LogicalOp>(result);
        var logicalOp = (LogicalOp)result;
        Assert.Equal("&&", logicalOp.Operator);
        Assert.NotNull(logicalOp.Left);
        Assert.NotNull(logicalOp.Right);

        // Left should be TalentRef
        Assert.IsType<TalentRef>(logicalOp.Left);
        var leftTalent = (TalentRef)logicalOp.Left;
        Assert.Equal("恋人", leftTalent.Name);

        // Right should be CflagRef
        Assert.IsType<CflagRef>(logicalOp.Right);
        var rightCflag = (CflagRef)logicalOp.Right;
        Assert.Equal("MASTER", rightCflag.Target);
    }

    /// <summary>
    /// AC#6.2: Parse OR operator (||)
    /// Pattern: condition1 || condition2
    /// </summary>
    [Fact]
    public void ParseAndOr_OrOperator()
    {
        // Arrange
        var parser = new LogicalOperatorParser();
        var condition = "TALENT:恋人 || HAS_VAGINA(TARGET)";

        // Act
        var result = parser.ParseLogicalExpression(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LogicalOp>(result);
        var logicalOp = (LogicalOp)result;
        Assert.Equal("||", logicalOp.Operator);
        Assert.NotNull(logicalOp.Left);
        Assert.NotNull(logicalOp.Right);

        // Left should be TalentRef
        Assert.IsType<TalentRef>(logicalOp.Left);
        var leftTalent = (TalentRef)logicalOp.Left;
        Assert.Equal("恋人", leftTalent.Name);

        // Right should be FunctionCall
        Assert.IsType<FunctionCall>(logicalOp.Right);
        var rightFunc = (FunctionCall)logicalOp.Right;
        Assert.Equal("HAS_VAGINA", rightFunc.Name);
    }

    /// <summary>
    /// AC#6.3: Parse chained logical operators (left-associative)
    /// Pattern: cond1 && cond2 && cond3
    /// Should parse as: (cond1 && cond2) && cond3
    /// </summary>
    [Fact]
    public void ParseAndOr_ChainedOperators()
    {
        // Arrange
        var parser = new LogicalOperatorParser();
        var condition = "TALENT:恋人 && CFLAG:MASTER:100 && FIRSTTIME()";

        // Act
        var result = parser.ParseLogicalExpression(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LogicalOp>(result);
        var logicalOp = (LogicalOp)result;
        Assert.Equal("&&", logicalOp.Operator);

        // Left should be LogicalOp (nested)
        Assert.IsType<LogicalOp>(logicalOp.Left);
        var leftLogical = (LogicalOp)logicalOp.Left;
        Assert.Equal("&&", leftLogical.Operator);
        Assert.IsType<TalentRef>(leftLogical.Left);
        Assert.IsType<CflagRef>(leftLogical.Right);

        // Right should be FunctionCall
        Assert.IsType<FunctionCall>(logicalOp.Right);
    }

    /// <summary>
    /// AC#6.4: Parse mixed AND/OR operators (AND has higher precedence)
    /// Pattern: cond1 || cond2 && cond3
    /// Should parse as: cond1 || (cond2 && cond3)
    /// </summary>
    [Fact]
    public void ParseAndOr_MixedOperatorPrecedence()
    {
        // Arrange
        var parser = new LogicalOperatorParser();
        var condition = "TALENT:恋人 || CFLAG:MASTER:100 && FIRSTTIME()";

        // Act
        var result = parser.ParseLogicalExpression(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LogicalOp>(result);
        var logicalOp = (LogicalOp)result;
        Assert.Equal("||", logicalOp.Operator);

        // Left should be TalentRef
        Assert.IsType<TalentRef>(logicalOp.Left);

        // Right should be LogicalOp (AND has higher precedence)
        Assert.IsType<LogicalOp>(logicalOp.Right);
        var rightLogical = (LogicalOp)logicalOp.Right;
        Assert.Equal("&&", rightLogical.Operator);
        Assert.IsType<CflagRef>(rightLogical.Left);
        Assert.IsType<FunctionCall>(rightLogical.Right);
    }

    /// <summary>
    /// AC#6.5: LogicalOp implements ICondition interface
    /// </summary>
    [Fact]
    public void LogicalOp_ImplementsICondition()
    {
        // Arrange
        var logicalOp = new LogicalOp
        {
            Left = new TalentRef { Name = "恋人" },
            Operator = "&&",
            Right = new CflagRef { Name = "睡眠" }
        };

        // Act
        ICondition condition = logicalOp;

        // Assert
        Assert.NotNull(condition);
        Assert.IsAssignableFrom<ICondition>(logicalOp);
    }

    /// <summary>
    /// AC#6.6: Parse single condition without operators
    /// Should return the condition itself, not a LogicalOp
    /// </summary>
    [Fact]
    public void ParseAndOr_SingleConditionNoOperator()
    {
        // Arrange
        var parser = new LogicalOperatorParser();
        var condition = "TALENT:恋人";

        // Act
        var result = parser.ParseLogicalExpression(condition);

        // Assert
        Assert.NotNull(result);
        // Should return TalentRef directly, not wrapped in LogicalOp
        Assert.IsType<TalentRef>(result);
        var talent = (TalentRef)result;
        Assert.Equal("恋人", talent.Name);
    }

    /// <summary>
    /// AC#6.7: Invalid logical expression - null input
    /// </summary>
    [Fact]
    public void ParseAndOr_NullInput()
    {
        // Arrange
        var parser = new LogicalOperatorParser();

        // Act
        var result = parser.ParseLogicalExpression(null!);

        // Assert
        Assert.Null(result); // Should return null gracefully
    }

    /// <summary>
    /// AC#6.8: Invalid logical expression - empty string
    /// </summary>
    [Fact]
    public void ParseAndOr_EmptyString()
    {
        // Arrange
        var parser = new LogicalOperatorParser();

        // Act
        var result = parser.ParseLogicalExpression(string.Empty);

        // Assert
        Assert.Null(result); // Should return null gracefully
    }

    /// <summary>
    /// AC#14.8: Parse negation prefix
    /// Pattern: !TALENT:X
    /// </summary>
    [Fact]
    public void ParseNegation_TalentCondition()
    {
        // Arrange
        var parser = new LogicalOperatorParser();
        var condition = "!TALENT:恋人";

        // Act
        var result = parser.ParseLogicalExpression(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NegatedCondition>(result);
        var negated = (NegatedCondition)result;
        Assert.NotNull(negated.Inner);
        Assert.IsType<TalentRef>(negated.Inner);
        var talent = (TalentRef)negated.Inner;
        Assert.Equal("恋人", talent.Name);
    }

    /// <summary>
    /// AC#14.9: Parse parenthesized expression
    /// Pattern: (TALENT:X || TALENT:Y) && TALENT:Z
    /// </summary>
    [Fact]
    public void ParseParenthesizedExpression_OrInsideAnd()
    {
        // Arrange
        var parser = new LogicalOperatorParser();
        var condition = "(TALENT:恋人 || TALENT:思慕) && TALENT:親愛";

        // Act
        var result = parser.ParseLogicalExpression(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LogicalOp>(result);
        var andOp = (LogicalOp)result;
        Assert.Equal("&&", andOp.Operator);

        // Left should be LogicalOp with || (from parenthesized expression)
        Assert.IsType<LogicalOp>(andOp.Left);
        var orOp = (LogicalOp)andOp.Left;
        Assert.Equal("||", orOp.Operator);
        Assert.IsType<TalentRef>(orOp.Left);
        Assert.IsType<TalentRef>(orOp.Right);

        // Right should be TalentRef
        Assert.IsType<TalentRef>(andOp.Right);
        var rightTalent = (TalentRef)andOp.Right;
        Assert.Equal("親愛", rightTalent.Name);
    }

    /// <summary>
    /// AC#14.10: Parse nested negation
    /// Pattern: !!TALENT:X (double negation)
    /// </summary>
    [Fact]
    public void ParseNegation_DoubleNegation()
    {
        // Arrange
        var parser = new LogicalOperatorParser();
        var condition = "!!TALENT:恋人";

        // Act
        var result = parser.ParseLogicalExpression(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NegatedCondition>(result);
        var outerNegation = (NegatedCondition)result;
        Assert.IsType<NegatedCondition>(outerNegation.Inner);
        var innerNegation = (NegatedCondition)outerNegation.Inner;
        Assert.IsType<TalentRef>(innerNegation.Inner);
    }

    /// <summary>
    /// AC#14.11: Parse negation with parentheses
    /// Pattern: !(TALENT:X && TALENT:Y)
    /// </summary>
    [Fact]
    public void ParseNegation_ParenthesizedExpression()
    {
        // Arrange
        var parser = new LogicalOperatorParser();
        var condition = "!(TALENT:恋人 && TALENT:思慕)";

        // Act
        var result = parser.ParseLogicalExpression(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NegatedCondition>(result);
        var negated = (NegatedCondition)result;
        Assert.IsType<LogicalOp>(negated.Inner);
        var andOp = (LogicalOp)negated.Inner;
        Assert.Equal("&&", andOp.Operator);
        Assert.IsType<TalentRef>(andOp.Left);
        Assert.IsType<TalentRef>(andOp.Right);
    }

    /// <summary>
    /// AC#14.12: NegatedCondition implements ICondition interface
    /// </summary>
    [Fact]
    public void NegatedCondition_ImplementsICondition()
    {
        // Arrange
        var negated = new NegatedCondition
        {
            Inner = new TalentRef { Name = "恋人" }
        };

        // Act
        ICondition condition = negated;

        // Assert
        Assert.NotNull(condition);
        Assert.IsAssignableFrom<ICondition>(negated);
    }

    /// <summary>
    /// F766 AC#5: Non-matching outer parens should NOT be stripped
    /// Pattern: (expr/(24*60)) == (DATETIME()/(24*60))
    /// The opening ( at position 0 matches an intermediate ), not the final one
    /// </summary>
    [Fact]
    public void ParenStripping_NonMatchingOuterParens_NotStripped()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "(CFLAG:奴隷:NTR訪問者と最後にセックスした日時/(24*60)) == (DATETIME()/(24*60))";

        // Act
        var result = extractor.Extract(condition);

        // Assert - should return null (not stripped, no arithmetic parser exists)
        Assert.Null(result);
    }

    /// <summary>
    /// F766 AC#6: Arithmetic paren pattern (simpler case)
    /// Pattern: (A) == (B) with both sides parenthesized
    /// Should NOT have outer parens stripped
    /// </summary>
    [Fact]
    public void ParenStripping_ArithmeticParenPattern_ReturnsNull()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "(TALENT:恋人) == (TALENT:思慕)";

        // Act
        var result = extractor.Extract(condition);

        // Assert - should return null (not stripped, falls through all parsers)
        Assert.Null(result);
    }
}
