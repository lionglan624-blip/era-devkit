using ErbParser;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// F757 AC#1-4: Bitwise operator parsing tests
/// Verifies bitwise & operator is correctly parsed for STAIN, CFLAG, and TALENT
/// and that logical && still works correctly (regression test)
/// </summary>
public class BitwiseOperatorTests
{
    /// <summary>
    /// AC#1: Test STAIN:口 & 汚れ_精液 → StainRef with Name="口", Operator="&", Value="汚れ_精液"
    /// </summary>
    [Fact]
    public void BitwiseOperator_StainCondition()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "STAIN:口 & 汚れ_精液";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<StainRef>(result);
        var stain = (StainRef)result;
        Assert.Equal("口", stain.Name);
        Assert.Equal("&", stain.Operator);
        Assert.Equal("汚れ_精液", stain.Value);
    }

    /// <summary>
    /// AC#2: Test CFLAG:奴隷:前回売春フラグ & 前回売春_初売春 → CflagRef with Target="奴隷", Name="前回売春フラグ", Operator="&", Value="前回売春_初売春"
    /// </summary>
    [Fact]
    public void BitwiseOperator_CflagCondition()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "CFLAG:奴隷:前回売春フラグ & 前回売春_初売春";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CflagRef>(result);
        var cflag = (CflagRef)result;
        Assert.Equal("奴隷", cflag.Target);
        Assert.Equal("前回売春フラグ", cflag.Name);
        Assert.Equal("&", cflag.Operator);
        Assert.Equal("前回売春_初売春", cflag.Value);
    }

    /// <summary>
    /// AC#3: Test TALENT:性別嗜好 & 1 → TalentRef with Name="性別嗜好", Operator="&", Value="1"
    /// </summary>
    [Fact]
    public void BitwiseOperator_TalentCondition()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "TALENT:性別嗜好 & 1";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TalentRef>(result);
        var talent = (TalentRef)result;
        Assert.Equal("性別嗜好", talent.Name);
        Assert.Equal("&", talent.Operator);
        Assert.Equal("1", talent.Value);
    }

    /// <summary>
    /// AC#4: Test that TALENT:恋人 && TALENT:性欲 still correctly splits into two conditions (regression)
    /// Verifies logical && is not broken by single & support
    /// </summary>
    [Fact]
    public void BitwiseOperator_LogicalAndRegression()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "TALENT:恋人 && TALENT:性欲";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LogicalOp>(result);
        var logicalOp = (LogicalOp)result;
        Assert.Equal("&&", logicalOp.Operator);

        // Left should be TalentRef for 恋人
        Assert.IsType<TalentRef>(logicalOp.Left);
        var leftTalent = (TalentRef)logicalOp.Left;
        Assert.Equal("恋人", leftTalent.Name);

        // Right should be TalentRef for 性欲
        Assert.IsType<TalentRef>(logicalOp.Right);
        var rightTalent = (TalentRef)logicalOp.Right;
        Assert.Equal("性欲", rightTalent.Name);
    }

    /// <summary>
    /// Additional test: STAIN with target prefix
    /// Test STAIN:奴隷:Ｖ & 汚れ_精液 → StainRef with Target="奴隷", Name="Ｖ", Operator="&", Value="汚れ_精液"
    /// </summary>
    [Fact]
    public void BitwiseOperator_StainWithTarget()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "STAIN:奴隷:Ｖ & 汚れ_精液";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<StainRef>(result);
        var stain = (StainRef)result;
        Assert.Equal("奴隷", stain.Target);
        Assert.Equal("Ｖ", stain.Name);
        Assert.Equal("&", stain.Operator);
        Assert.Equal("汚れ_精液", stain.Value);
    }

    /// <summary>
    /// Edge case: Bitwise without spaces around &
    /// Test STAIN:口&汚れ_精液 (no spaces) - should still parse correctly per Constraint C13
    /// </summary>
    [Fact]
    public void BitwiseOperator_NoSpaces()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "STAIN:口&汚れ_精液";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<StainRef>(result);
        var stain = (StainRef)result;
        Assert.Equal("口", stain.Name);
        Assert.Equal("&", stain.Operator);
        Assert.Equal("汚れ_精液", stain.Value);
    }
}
