using ErbParser;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// F759 AC#7: Parser unit tests for compound bitwise-comparison conditions
/// Tests BitwiseComparisonCondition parsing: (VAR & mask) op value
/// </summary>
public class BitwiseComparisonTests
{
    /// <summary>
    /// Positive: (TALENT:性別嗜好 & 3) == 3 → BitwiseComparisonCondition
    /// Named talent to avoid F760 dependency
    /// </summary>
    [Fact]
    public void BitwiseComparison_TalentNamedWithEquality()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "(TALENT:性別嗜好 & 3) == 3";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BitwiseComparisonCondition>(result);
        var bitwiseComp = (BitwiseComparisonCondition)result;
        Assert.Equal("==", bitwiseComp.ComparisonOp);
        Assert.Equal("3", bitwiseComp.ComparisonValue);

        // Inner should be TalentRef with & operator
        Assert.IsType<TalentRef>(bitwiseComp.Inner);
        var inner = (TalentRef)bitwiseComp.Inner;
        Assert.Equal("性別嗜好", inner.Name);
        Assert.Equal("&", inner.Operator);
        Assert.Equal("3", inner.Value);
    }

    /// <summary>
    /// Positive: (CFLAG:奴隷:フラグ & 1) != 0 → BitwiseComparisonCondition with !=
    /// </summary>
    [Fact]
    public void BitwiseComparison_CflagWithNotEqual()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "(CFLAG:奴隷:フラグ & 1) != 0";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BitwiseComparisonCondition>(result);
        var bitwiseComp = (BitwiseComparisonCondition)result;
        Assert.Equal("!=", bitwiseComp.ComparisonOp);
        Assert.Equal("0", bitwiseComp.ComparisonValue);

        // Inner should be CflagRef with & operator
        Assert.IsType<CflagRef>(bitwiseComp.Inner);
        var inner = (CflagRef)bitwiseComp.Inner;
        Assert.Equal("奴隷", inner.Target);
        Assert.Equal("フラグ", inner.Name);
        Assert.Equal("&", inner.Operator);
        Assert.Equal("1", inner.Value);
    }

    /// <summary>
    /// Positive: (TALENT:2 & 3) == 3 → BitwiseComparisonCondition
    /// Actual kojo pattern from KOJO_KU_愛撫.ERB:63
    /// TALENT:2 parses as Index=2 (F760: numeric parses as Index)
    /// </summary>
    [Fact]
    public void BitwiseComparison_ActualKojoPattern()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "(TALENT:2 & 3) == 3";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BitwiseComparisonCondition>(result);
        var bitwiseComp = (BitwiseComparisonCondition)result;
        Assert.Equal("==", bitwiseComp.ComparisonOp);
        Assert.Equal("3", bitwiseComp.ComparisonValue);

        // Inner should be TalentRef with Index=2, empty Name
        Assert.IsType<TalentRef>(bitwiseComp.Inner);
        var inner = (TalentRef)bitwiseComp.Inner;
        Assert.Equal(2, inner.Index);
        Assert.Equal("", inner.Name);
        Assert.Equal("&", inner.Operator);
        Assert.Equal("3", inner.Value);
    }

    /// <summary>
    /// Negative: (TALENT:性別嗜好 & 3) without comparison operator
    /// Should NOT match compound pattern - falls through to existing truthiness handling
    /// </summary>
    [Fact]
    public void BitwiseComparison_NoComparisonOperator_FallsThrough()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "(TALENT:性別嗜好 & 3)";

        // Act
        var result = extractor.Extract(condition);

        // Assert - should parse as regular truthiness TalentRef (paren stripped, existing logic)
        Assert.NotNull(result);
        Assert.IsType<TalentRef>(result);
        var talent = (TalentRef)result;
        Assert.Equal("性別嗜好", talent.Name);
        Assert.Equal("&", talent.Operator);
        Assert.Equal("3", talent.Value);
    }

    /// <summary>
    /// Negative: (TALENT:性別嗜好 & ) == 3 → malformed inner expression
    /// Should return null
    /// </summary>
    [Fact]
    public void BitwiseComparison_MalformedInner_ReturnsNull()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "(TALENT:性別嗜好 & ) == 3";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Negative: (TALENT:性別嗜好 == 3) == 5 → inner has non-bitwise operator
    /// Rejected by HasBitwiseOperator validation
    /// </summary>
    [Fact]
    public void BitwiseComparison_NonBitwiseInner_ReturnsNull()
    {
        // Arrange
        var extractor = new ConditionExtractor();
        var condition = "(TALENT:性別嗜好 == 3) == 5";

        // Act
        var result = extractor.Extract(condition);

        // Assert
        Assert.Null(result);
    }
}
