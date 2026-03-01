using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#3: Extract structure from kojo file
/// </summary>
public class KojoExtractionTests
{
    [Fact]
    public void ParseKojoFile_ContainsIfNode()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "sample_kojo.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert - use OfTypeFlatten to traverse FunctionDefNode.Body (F764)
        var ifNodes = ast.OfTypeFlatten<IfNode>().ToList();
        Assert.NotEmpty(ifNodes);
    }

    [Fact]
    public void ParseKojoFile_ContainsDatalistNode()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "sample_kojo.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert - use OfTypeFlatten to traverse FunctionDefNode.Body (F764)
        var datalistNodes = ast.OfTypeFlatten<DatalistNode>().ToList();
        Assert.NotEmpty(datalistNodes);
    }

    [Fact]
    public void ParseKojoFile_ContainsPrintformNode()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "sample_kojo.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert
        // PRINTFORML should be inside IF block - use OfTypeFlatten to traverse FunctionDefNode.Body (F764)
        var ifNodes = ast.OfTypeFlatten<IfNode>().ToList();
        Assert.NotEmpty(ifNodes);

        var printformNodes = ifNodes
            .SelectMany(ifNode => ifNode.Body.OfType<PrintformNode>())
            .ToList();
        Assert.NotEmpty(printformNodes);
    }

    [Fact]
    public void ParseKojoFile_DatalistHasCorrectDataforms()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "sample_kojo.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert - use OfTypeFlatten to traverse FunctionDefNode.Body (F764)
        var datalist = ast.OfTypeFlatten<DatalistNode>().First();
        Assert.Equal(2, datalist.DataForms.Count);
    }
}
