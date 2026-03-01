using ErbParser;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#14: TCVAR reference parsing tests
/// Verifies TcvarConditionParser extracts TCVAR references from condition strings
/// </summary>
public class TcvarExtractorTests
{
    /// <summary>
    /// AC#14.1: Extract TCVAR reference with index only
    /// Pattern: TCVAR:302
    /// </summary>
    [Fact]
    public void ExtractTcvarReference_WithIndexOnly()
    {
        // Arrange
        var parser = new TcvarConditionParser();
        var condition = "TCVAR:302";

        // Act
        var result = parser.ParseTcvarCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(302, result.Index);
        Assert.Null(result.Target); // No target prefix
        Assert.Null(result.Name); // Index form, not name form
    }

    /// <summary>
    /// AC#14.2: Extract TCVAR reference with operator and value
    /// Pattern: TCVAR:302 != 0
    /// </summary>
    [Fact]
    public void ExtractTcvarReference_WithOperatorAndValue()
    {
        // Arrange
        var parser = new TcvarConditionParser();
        var condition = "TCVAR:302 != 0";

        // Act
        var result = parser.ParseTcvarCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(302, result.Index);
        Assert.Equal("!=", result.Operator);
        Assert.Equal("0", result.Value);
    }

    /// <summary>
    /// AC#14.3: Extract TCVAR reference with target and name
    /// Pattern: TCVAR:MASTER:妊娠
    /// </summary>
    [Fact]
    public void ExtractTcvarReference_WithTargetAndName()
    {
        // Arrange
        var parser = new TcvarConditionParser();
        var condition = "TCVAR:MASTER:妊娠";

        // Act
        var result = parser.ParseTcvarCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.Equal("妊娠", result.Name);
        Assert.Null(result.Index); // Name form, not index form
    }

    /// <summary>
    /// AC#14.4: TcvarRef implements ICondition interface
    /// </summary>
    [Fact]
    public void TcvarRef_ImplementsICondition()
    {
        // Arrange
        var tcvarRef = new TcvarRef
        {
            Index = 302
        };

        // Act
        ICondition condition = tcvarRef;

        // Assert
        Assert.NotNull(condition);
        Assert.IsAssignableFrom<ICondition>(tcvarRef);
    }

    /// <summary>
    /// AC#14.5: Invalid TCVAR reference - null input
    /// </summary>
    [Fact]
    public void InvalidTcvarReference_NullInput()
    {
        // Arrange
        var parser = new TcvarConditionParser();

        // Act
        var result = parser.ParseTcvarCondition(null!);

        // Assert
        Assert.Null(result); // Should return null gracefully
    }

    /// <summary>
    /// AC#14.6: Invalid TCVAR reference - empty string
    /// </summary>
    [Fact]
    public void InvalidTcvarReference_EmptyString()
    {
        // Arrange
        var parser = new TcvarConditionParser();

        // Act
        var result = parser.ParseTcvarCondition(string.Empty);

        // Assert
        Assert.Null(result); // Should return null gracefully
    }

    /// <summary>
    /// AC#14.7: Invalid TCVAR reference - malformed syntax "TCVAR:"
    /// </summary>
    [Fact]
    public void InvalidTcvarReference_EmptyTcvar()
    {
        // Arrange
        var parser = new TcvarConditionParser();

        // Act
        var result = parser.ParseTcvarCondition("TCVAR:");

        // Assert
        Assert.Null(result); // Should return null for malformed input
    }
}
