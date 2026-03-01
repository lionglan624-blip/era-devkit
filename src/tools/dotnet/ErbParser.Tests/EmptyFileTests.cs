using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#5: Empty file handled
/// </summary>
public class EmptyFileTests
{
    [Fact]
    public void ParseEmptyFile_ReturnsEmptyAst()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "empty.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert
        Assert.NotNull(ast);
        Assert.Empty(ast);
    }

    [Fact]
    public void ParseEmptyFile_DoesNotThrow()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "empty.erb");

        // Act & Assert
        var exception = Record.Exception(() => parser.Parse(testFile));
        Assert.Null(exception);
    }

    [Fact]
    public void ParseEmptyString_ReturnsEmptyAst()
    {
        // Arrange
        var parser = new ErbParser();

        // Act
        var ast = parser.ParseString(string.Empty);

        // Assert
        Assert.NotNull(ast);
        Assert.Empty(ast);
    }
}
