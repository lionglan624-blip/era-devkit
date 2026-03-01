using ErbParser;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#2, AC#5: Function call parsing tests
/// Verifies FunctionCallParser extracts function calls from condition strings
/// </summary>
public class FunctionExtractorTests
{
    /// <summary>
    /// AC#2.1: Extract function call with single argument
    /// Pattern: HAS_VAGINA(TARGET)
    /// </summary>
    [Fact]
    public void ExtractFunctionCall_SingleArgument()
    {
        // Arrange
        var parser = new FunctionCallParser();
        var condition = "HAS_VAGINA(TARGET)";

        // Act
        var result = parser.ParseFunctionCall(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("HAS_VAGINA", result.Name);
        Assert.NotNull(result.Args);
        Assert.Single(result.Args);
        Assert.Equal("TARGET", result.Args[0]);
    }

    /// <summary>
    /// AC#2.2: Extract function call with no arguments
    /// Pattern: FIRSTTIME()
    /// </summary>
    [Fact]
    public void ExtractFunctionCall_NoArguments()
    {
        // Arrange
        var parser = new FunctionCallParser();
        var condition = "FIRSTTIME()";

        // Act
        var result = parser.ParseFunctionCall(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FIRSTTIME", result.Name);
        Assert.NotNull(result.Args);
        Assert.Empty(result.Args); // Empty argument list
    }

    /// <summary>
    /// AC#2.3: Extract function call with multiple arguments
    /// Pattern: SOME_FUNC(ARG1, ARG2, ARG3)
    /// </summary>
    [Fact]
    public void ExtractFunctionCall_MultipleArguments()
    {
        // Arrange
        var parser = new FunctionCallParser();
        var condition = "SOME_FUNC(ARG1, ARG2, ARG3)";

        // Act
        var result = parser.ParseFunctionCall(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SOME_FUNC", result.Name);
        Assert.NotNull(result.Args);
        Assert.Equal(3, result.Args.Length);
        Assert.Equal("ARG1", result.Args[0]);
        Assert.Equal("ARG2", result.Args[1]);
        Assert.Equal("ARG3", result.Args[2]);
    }

    /// <summary>
    /// AC#2.4: Extract function call with numeric literal argument
    /// Pattern: CHECK_VALUE(100)
    /// </summary>
    [Fact]
    public void ExtractFunctionCall_NumericArgument()
    {
        // Arrange
        var parser = new FunctionCallParser();
        var condition = "CHECK_VALUE(100)";

        // Act
        var result = parser.ParseFunctionCall(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CHECK_VALUE", result.Name);
        Assert.NotNull(result.Args);
        Assert.Single(result.Args);
        Assert.Equal("100", result.Args[0]);
    }

    /// <summary>
    /// AC#2.5: FunctionCall implements ICondition interface
    /// </summary>
    [Fact]
    public void FunctionCall_ImplementsICondition()
    {
        // Arrange
        var funcCall = new FunctionCall
        {
            Name = "HAS_VAGINA",
            Args = new[] { "TARGET" }
        };

        // Act
        ICondition condition = funcCall;

        // Assert
        Assert.NotNull(condition);
        Assert.IsAssignableFrom<ICondition>(funcCall);
    }

    /// <summary>
    /// AC#5.1: Invalid function syntax - null input
    /// </summary>
    [Fact]
    public void InvalidFunctionSyntax_NullInput()
    {
        // Arrange
        var parser = new FunctionCallParser();

        // Act
        var result = parser.ParseFunctionCall(null!);

        // Assert
        Assert.Null(result); // Should return null gracefully
    }

    /// <summary>
    /// AC#5.2: Invalid function syntax - empty string
    /// </summary>
    [Fact]
    public void InvalidFunctionSyntax_EmptyString()
    {
        // Arrange
        var parser = new FunctionCallParser();

        // Act
        var result = parser.ParseFunctionCall(string.Empty);

        // Assert
        Assert.Null(result); // Should return null gracefully
    }

    /// <summary>
    /// AC#5.3: Invalid function syntax - missing opening parenthesis
    /// </summary>
    [Fact]
    public void InvalidFunctionSyntax_MissingOpenParen()
    {
        // Arrange
        var parser = new FunctionCallParser();

        // Act
        var result = parser.ParseFunctionCall("FUNCTION)");

        // Assert
        Assert.Null(result); // Should return null for malformed input
    }

    /// <summary>
    /// AC#5.4: Invalid function syntax - missing closing parenthesis
    /// </summary>
    [Fact]
    public void InvalidFunctionSyntax_MissingCloseParen()
    {
        // Arrange
        var parser = new FunctionCallParser();

        // Act
        var result = parser.ParseFunctionCall("FUNCTION(ARG");

        // Assert
        Assert.Null(result); // Should return null for malformed input
    }

    /// <summary>
    /// AC#5.5: Invalid function syntax - empty function name
    /// </summary>
    [Fact]
    public void InvalidFunctionSyntax_EmptyFunctionName()
    {
        // Arrange
        var parser = new FunctionCallParser();

        // Act
        var result = parser.ParseFunctionCall("(ARG)");

        // Assert
        Assert.Null(result); // Should return null for malformed input
    }

    /// <summary>
    /// AC#5.6: Invalid function syntax - unmatched parentheses
    /// </summary>
    [Fact]
    public void InvalidFunctionSyntax_UnmatchedParentheses()
    {
        // Arrange
        var parser = new FunctionCallParser();

        // Act
        var result = parser.ParseFunctionCall("FUNCTION((ARG)");

        // Assert
        Assert.Null(result); // Should return null for malformed input
    }

    /// <summary>
    /// AC#5 (F757/Task4): Parse nested function call with balanced parentheses
    /// Pattern: FIRSTTIME(TOSTR(350), 1)
    /// Expected: FunctionCall with Name="FIRSTTIME", Args=["TOSTR(350)", "1"]
    /// </summary>
    [Fact]
    public void TestNestedFunctionCall()
    {
        // Arrange
        var parser = new FunctionCallParser();
        var condition = "FIRSTTIME(TOSTR(350), 1)";

        // Act
        var result = parser.ParseFunctionCall(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FIRSTTIME", result.Name);
        Assert.NotNull(result.Args);
        Assert.Equal(2, result.Args.Length);
        Assert.Equal("TOSTR(350)", result.Args[0]);
        Assert.Equal("1", result.Args[1]);
    }

    /// <summary>
    /// AC#6 (F757/Task4): Parse nested function args with inner parens and commas
    /// Pattern: FIRSTTIME(TOSTR(TFLAG:50 + 500))
    /// Expected: FunctionCall with Name="FIRSTTIME", Args=["TOSTR(TFLAG:50 + 500)"]
    /// </summary>
    [Fact]
    public void TestNestedFunctionArgsWithInnerParens()
    {
        // Arrange
        var parser = new FunctionCallParser();
        var condition = "FIRSTTIME(TOSTR(TFLAG:50 + 500))";

        // Act
        var result = parser.ParseFunctionCall(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FIRSTTIME", result.Name);
        Assert.NotNull(result.Args);
        Assert.Single(result.Args);
        Assert.Equal("TOSTR(TFLAG:50 + 500)", result.Args[0]);
    }
}
