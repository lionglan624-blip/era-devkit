using ErbParser;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#1, AC#4: CFLAG reference parsing tests
/// Verifies CflagConditionParser extracts CFLAG references from condition strings
/// </summary>
public class CflagExtractorTests
{
    /// <summary>
    /// AC#1.1: Extract CFLAG reference with target and name
    /// Pattern: CFLAG:MASTER:現在位置
    /// </summary>
    [Fact]
    public void ExtractCflagReference_WithTargetAndName()
    {
        // Arrange
        var parser = new CflagConditionParser();
        var condition = "CFLAG:MASTER:現在位置";

        // Act
        var result = parser.ParseCflagCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.Equal("現在位置", result.Name);
        Assert.Null(result.Index); // Name form, not index form
    }

    /// <summary>
    /// AC#1.2: Extract CFLAG reference with index only
    /// Pattern: CFLAG:100
    /// </summary>
    [Fact]
    public void ExtractCflagReference_WithIndexOnly()
    {
        // Arrange
        var parser = new CflagConditionParser();
        var condition = "CFLAG:100";

        // Act
        var result = parser.ParseCflagCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Index);
        Assert.Null(result.Target); // No target prefix
        Assert.Null(result.Name); // Index form, not name form
    }

    /// <summary>
    /// AC#1.3: Extract CFLAG reference with name only
    /// Pattern: CFLAG:睡眠
    /// </summary>
    [Fact]
    public void ExtractCflagReference_WithNameOnly()
    {
        // Arrange
        var parser = new CflagConditionParser();
        var condition = "CFLAG:睡眠";

        // Act
        var result = parser.ParseCflagCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("睡眠", result.Name);
        Assert.Null(result.Target); // No target prefix
        Assert.Null(result.Index); // Name form, not index form
    }

    /// <summary>
    /// AC#1.4: Extract CFLAG reference with operator and value
    /// Pattern: CFLAG:MASTER:現在位置 == 100
    /// </summary>
    [Fact]
    public void ExtractCflagReference_WithOperatorAndValue()
    {
        // Arrange
        var parser = new CflagConditionParser();
        var condition = "CFLAG:MASTER:現在位置 == 100";

        // Act
        var result = parser.ParseCflagCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.Equal("現在位置", result.Name);
        Assert.Equal("==", result.Operator);
        Assert.Equal("100", result.Value);
    }

    /// <summary>
    /// AC#1.5: CflagRef implements ICondition interface
    /// </summary>
    [Fact]
    public void CflagRef_ImplementsICondition()
    {
        // Arrange
        var cflagRef = new CflagRef
        {
            Target = "MASTER",
            Name = "現在位置"
        };

        // Act
        ICondition condition = cflagRef;

        // Assert
        Assert.NotNull(condition);
        Assert.IsAssignableFrom<ICondition>(cflagRef);
    }

    /// <summary>
    /// AC#4.1: Invalid CFLAG reference - null input
    /// </summary>
    [Fact]
    public void InvalidCflagReference_NullInput()
    {
        // Arrange
        var parser = new CflagConditionParser();

        // Act
        var result = parser.ParseCflagCondition(null!);

        // Assert
        Assert.Null(result); // Should return null gracefully
    }

    /// <summary>
    /// AC#4.2: Invalid CFLAG reference - empty string
    /// </summary>
    [Fact]
    public void InvalidCflagReference_EmptyString()
    {
        // Arrange
        var parser = new CflagConditionParser();

        // Act
        var result = parser.ParseCflagCondition(string.Empty);

        // Assert
        Assert.Null(result); // Should return null gracefully
    }

    /// <summary>
    /// AC#4.3: Invalid CFLAG reference - malformed syntax "CFLAG:"
    /// </summary>
    [Fact]
    public void InvalidCflagReference_EmptyCflag()
    {
        // Arrange
        var parser = new CflagConditionParser();

        // Act
        var result = parser.ParseCflagCondition("CFLAG:");

        // Assert
        Assert.Null(result); // Should return null for malformed input
    }

    /// <summary>
    /// AC#4.4: Invalid CFLAG reference - double separator "CFLAG::name"
    /// </summary>
    [Fact]
    public void InvalidCflagReference_DoubleSeparator()
    {
        // Arrange
        var parser = new CflagConditionParser();

        // Act
        var result = parser.ParseCflagCondition("CFLAG::現在位置");

        // Assert
        Assert.Null(result); // Should return null for malformed input
    }

    /// <summary>
    /// AC#4.5: Invalid CFLAG reference - no separator "CFLAG"
    /// </summary>
    [Fact]
    public void InvalidCflagReference_NoSeparator()
    {
        // Arrange
        var parser = new CflagConditionParser();

        // Act
        var result = parser.ParseCflagCondition("CFLAG");

        // Assert
        Assert.Null(result); // Should return null for malformed input
    }
}
