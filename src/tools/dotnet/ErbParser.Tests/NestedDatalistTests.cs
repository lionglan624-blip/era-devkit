using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#4: Invalid nested DATALIST rejected
/// </summary>
public class NestedDatalistTests
{
    [Fact]
    public void ParseNestedDatalist_ThrowsParseException()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "nested_datalist.erb");

        // Act & Assert
        var exception = Assert.Throws<ParseException>(() => parser.Parse(testFile));
        Assert.Contains("nested", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseNestedDatalist_ExceptionIndicatesDatalistContext()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "nested_datalist.erb");

        // Act & Assert
        var exception = Assert.Throws<ParseException>(() => parser.Parse(testFile));
        Assert.Contains("DATALIST", exception.Message);
    }
}
