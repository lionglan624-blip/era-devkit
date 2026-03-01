using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#1: Parse simple DATALIST
/// </summary>
public class DatalistParseTests
{
    [Fact]
    public void ParseSimpleDatalist_ReturnsDatalistNode()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "simple_datalist.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert
        Assert.NotEmpty(ast);
        var datalist = ast.OfType<DatalistNode>().FirstOrDefault();
        Assert.NotNull(datalist);
        Assert.Equal(2, datalist.DataForms.Count);
    }

    [Fact]
    public void ParseSimpleDatalist_HasCorrectDataforms()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "simple_datalist.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert
        var datalist = ast.OfType<DatalistNode>().First();
        Assert.Equal(2, datalist.DataForms.Count);

        var firstDataform = datalist.DataForms[0];
        Assert.Equal(3, firstDataform.Arguments.Count);
    }
}
