using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// Test suite for STRDATA...ENDDATA parsing (F650)
/// </summary>
public class StrDataParseTests
{
    /// <summary>
    /// AC#4: Parser succeeds on basic STRDATA block
    /// </summary>
    [Fact]
    public void ParsesBasicStrDataBlock()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "basic_strdata.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert
        // STRDATA blocks should be skipped - no AST nodes created
        Assert.Empty(ast);
    }

    /// <summary>
    /// AC#5: Parser throws on nested STRDATA
    /// </summary>
    [Fact]
    public void ThrowsOnNestedStrData()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "nested_strdata.erb");

        // Act & Assert
        var exception = Assert.Throws<ParseException>(() => parser.Parse(testFile));
        Assert.Contains("Nested STRDATA", exception.Message);
    }

    /// <summary>
    /// AC#11: Parser throws on unclosed STRDATA at EOF
    /// </summary>
    [Fact]
    public void ThrowsOnUnclosedStrData()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "unclosed_strdata.erb");

        // Act & Assert
        var exception = Assert.Throws<ParseException>(() => parser.Parse(testFile));
        Assert.Contains("STRDATA without matching ENDDATA", exception.Message);
    }

    /// <summary>
    /// AC#6: Parser succeeds on Sakuya NTR file with STRDATA
    /// </summary>
    [Fact]
    public void ParsesSakuyaNtrFile()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Era.DevKit.TestUtils.GamePathHelper.Resolve("ERB", "口上", "4_咲夜", "NTR口上_お持ち帰り.ERB");

        // Act
        var ast = parser.Parse(testFile);

        // Assert
        // File should parse successfully without throwing ParseException
        // AST should contain nodes from PRINTDATA blocks (but not from STRDATA blocks)
        Assert.NotEmpty(ast);
    }

    /// <summary>
    /// AC#7: Parser succeeds on Marisa NTR file with STRDATA
    /// </summary>
    [Fact]
    public void ParsesMarisaNtrFile()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Era.DevKit.TestUtils.GamePathHelper.Resolve("ERB", "口上", "10_魔理沙", "NTR口上_お持ち帰り.ERB");

        // Act
        var ast = parser.Parse(testFile);

        // Assert
        // File should parse successfully without throwing ParseException
        // AST should contain nodes from PRINTDATA blocks (but not from STRDATA blocks)
        Assert.NotEmpty(ast);
    }
}
