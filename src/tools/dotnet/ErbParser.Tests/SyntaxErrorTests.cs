using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#2: Detect syntax errors
/// </summary>
public class SyntaxErrorTests
{
    [Fact]
    public void ParseInvalidSyntax_ThrowsParseException()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "invalid_syntax.erb");

        // Act & Assert
        var exception = Assert.Throws<ParseException>(() => parser.Parse(testFile));
        Assert.Contains("ENDLIST", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseException_ContainsFileName()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "invalid_syntax.erb");

        // Act & Assert
        var exception = Assert.Throws<ParseException>(() => parser.Parse(testFile));
        Assert.False(string.IsNullOrEmpty(exception.FileName));
    }

    [Fact]
    public void ParseException_ContainsLineNumber()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "invalid_syntax.erb");

        // Act & Assert
        var exception = Assert.Throws<ParseException>(() => parser.Parse(testFile));
        Assert.True(exception.LineNumber > 0);
    }
}
