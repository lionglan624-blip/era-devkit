using ErbParser;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#3: Combined condition extraction integration tests
/// Verifies ConditionExtractor.Extract() detects TALENT/CFLAG/function patterns
/// and returns appropriate ICondition subtypes
/// </summary>
public class ConditionExtractorTests
{
    /// <summary>
    /// AC#3.1: Extract TALENT condition
    /// Input: "TALENT:恋人"
    /// Expected: TalentRef instance
    /// </summary>
    [Fact]
    public void CombinedExtraction_TalentCondition()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "TALENT:恋人";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TalentRef>(result);
        var talent = (TalentRef)result;
        Assert.Equal("恋人", talent.Name);
    }

    /// <summary>
    /// AC#3.2: Extract CFLAG condition
    /// Input: "CFLAG:MASTER:現在位置"
    /// Expected: CflagRef instance
    /// </summary>
    [Fact]
    public void CombinedExtraction_CflagCondition()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "CFLAG:MASTER:現在位置";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CflagRef>(result);
        var cflag = (CflagRef)result;
        Assert.Equal("MASTER", cflag.Target);
        Assert.Equal("現在位置", cflag.Name);
    }

    /// <summary>
    /// AC#3.3: Extract function call condition
    /// Input: "HAS_VAGINA(TARGET)"
    /// Expected: FunctionCall instance
    /// </summary>
    [Fact]
    public void CombinedExtraction_FunctionCallCondition()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "HAS_VAGINA(TARGET)";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FunctionCall>(result);
        var func = (FunctionCall)result;
        Assert.Equal("HAS_VAGINA", func.Name);
        Assert.Single(func.Args);
        Assert.Equal("TARGET", func.Args[0]);
    }

    /// <summary>
    /// AC#3.4: Extract logical AND expression
    /// Input: "TALENT:恋人 && CFLAG:MASTER:100"
    /// Expected: LogicalOp tree with TalentRef and CflagRef children
    /// </summary>
    [Fact]
    public void CombinedExtraction_LogicalAndExpression()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "TALENT:恋人 && CFLAG:MASTER:100";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LogicalOp>(result);
        var logicalOp = (LogicalOp)result;
        Assert.Equal("&&", logicalOp.Operator);

        // Left should be TalentRef
        Assert.IsType<TalentRef>(logicalOp.Left);
        var talent = (TalentRef)logicalOp.Left;
        Assert.Equal("恋人", talent.Name);

        // Right should be CflagRef
        Assert.IsType<CflagRef>(logicalOp.Right);
        var cflag = (CflagRef)logicalOp.Right;
        Assert.Equal("MASTER", cflag.Target);
    }

    /// <summary>
    /// AC#3.5: Extract complex logical expression with multiple operators
    /// Input: "TALENT:恋人 && CFLAG:MASTER:100 || FIRSTTIME()"
    /// Expected: Nested LogicalOp tree
    /// </summary>
    [Fact]
    public void CombinedExtraction_ComplexLogicalExpression()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "TALENT:恋人 && CFLAG:MASTER:100 || FIRSTTIME()";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LogicalOp>(result);
        var rootOp = (LogicalOp)result;

        // Root should be OR (lower precedence)
        Assert.Equal("||", rootOp.Operator);

        // Left should be LogicalOp (AND)
        Assert.IsType<LogicalOp>(rootOp.Left);
        var leftOp = (LogicalOp)rootOp.Left;
        Assert.Equal("&&", leftOp.Operator);
        Assert.IsType<TalentRef>(leftOp.Left);
        Assert.IsType<CflagRef>(leftOp.Right);

        // Right should be FunctionCall
        Assert.IsType<FunctionCall>(rootOp.Right);
    }

    /// <summary>
    /// AC#3.6: Extract condition with comparison operator
    /// Input: "TALENT:MASTER:恋人 != 0"
    /// Expected: TalentRef with Operator and Value
    /// </summary>
    [Fact]
    public void CombinedExtraction_ConditionWithComparison()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "TALENT:MASTER:恋人 != 0";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TalentRef>(result);
        var talent = (TalentRef)result;
        Assert.Equal("MASTER", talent.Target);
        Assert.Equal("恋人", talent.Name);
        Assert.Equal("!=", talent.Operator);
        Assert.Equal("0", talent.Value);
    }

    /// <summary>
    /// AC#3.7: All extracted types implement ICondition
    /// Verifies polymorphic condition tree structure
    /// </summary>
    [Fact]
    public void CombinedExtraction_AllTypesImplementICondition()
    {
        // Arrange
        var extractor = new ConditionExtractor();

        // Act - Extract different condition types
        var talentResult = extractor.Extract("TALENT:恋人");
        var cflagResult = extractor.Extract("CFLAG:MASTER:100");
        var funcResult = extractor.Extract("HAS_VAGINA(TARGET)");
        var logicalResult = extractor.Extract("TALENT:恋人 && CFLAG:MASTER:100");

        // Assert - All should implement ICondition
        Assert.IsAssignableFrom<ICondition>(talentResult);
        Assert.IsAssignableFrom<ICondition>(cflagResult);
        Assert.IsAssignableFrom<ICondition>(funcResult);
        Assert.IsAssignableFrom<ICondition>(logicalResult);
    }

    /// <summary>
    /// AC#3.8: Invalid condition returns null
    /// Input: null, empty, or unrecognized pattern
    /// Expected: null
    /// </summary>
    [Fact]
    public void CombinedExtraction_InvalidConditionReturnsNull()
    {
        // Arrange
        var extractor = new ConditionExtractor();

        // Act & Assert - null input
        var nullResult = extractor.Extract(null!);
        Assert.Null(nullResult);

        // Act & Assert - empty input
        var emptyResult = extractor.Extract(string.Empty);
        Assert.Null(emptyResult);

        // Act & Assert - unrecognized pattern
        var unknownResult = extractor.Extract("UNKNOWN_PATTERN");
        Assert.Null(unknownResult);
    }

    /// <summary>
    /// AC#3.9: Integration with ConditionBranch
    /// Verifies extracted conditions can be assigned to ConditionBranch.Condition
    /// </summary>
    [Fact]
    public void CombinedExtraction_IntegrationWithConditionBranch()
    {
        // Arrange
        var extractor = new ConditionExtractor();

        // Act - Extract different condition types
        var talentCondition = extractor.Extract("TALENT:恋人");
        var cflagCondition = extractor.Extract("CFLAG:MASTER:100");
        var logicalCondition = extractor.Extract("TALENT:恋人 && CFLAG:MASTER:100");

        // Create branches with extracted conditions
        var talentBranch = new ConditionBranch
        {
            Type = "if",
            Condition = talentCondition,
            HasBody = true
        };

        var cflagBranch = new ConditionBranch
        {
            Type = "elseif",
            Condition = cflagCondition,
            HasBody = true
        };

        var logicalBranch = new ConditionBranch
        {
            Type = "elseif",
            Condition = logicalCondition,
            HasBody = true
        };

        // Assert - All branches have valid conditions
        Assert.NotNull(talentBranch.Condition);
        Assert.NotNull(cflagBranch.Condition);
        Assert.NotNull(logicalBranch.Condition);

        Assert.IsType<TalentRef>(talentBranch.Condition);
        Assert.IsType<CflagRef>(cflagBranch.Condition);
        Assert.IsType<LogicalOp>(logicalBranch.Condition);
    }
}
