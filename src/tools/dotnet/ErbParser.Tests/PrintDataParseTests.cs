using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// Test suite for PRINTDATA...ENDDATA parsing (F633)
/// </summary>
public class PrintDataParseTests
{
    /// <summary>
    /// AC#8: Parser succeeds on simple PRINTDATA
    /// </summary>
    [Fact]
    public void ParseSimplePrintData()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "simple_printdata.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert
        Assert.NotEmpty(ast);
        var printData = ast.OfType<PrintDataNode>().FirstOrDefault();
        Assert.NotNull(printData);
        Assert.Equal("PRINTDATAL", printData.Variant);
        Assert.Equal(2, printData.Content.Count);
    }

    /// <summary>
    /// AC#6: Parser nests DATALIST in PrintDataNode
    /// </summary>
    [Fact]
    public void PrintDataNestedDatalist()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "printdata_nested_datalist.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert
        Assert.NotEmpty(ast);
        var printData = ast.OfType<PrintDataNode>().FirstOrDefault();
        Assert.NotNull(printData);
        Assert.Equal("PRINTDATAL", printData.Variant);

        // Should contain one DatalistNode
        var datalist = printData.Content.OfType<DatalistNode>().FirstOrDefault();
        Assert.NotNull(datalist);
        Assert.Equal(2, datalist.DataForms.Count);
    }

    /// <summary>
    /// AC#7: Parser handles IF inside PRINTDATA
    /// </summary>
    [Fact]
    public void PrintDataConditional()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "printdata_conditional.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert
        Assert.NotEmpty(ast);
        var printData = ast.OfType<PrintDataNode>().FirstOrDefault();
        Assert.NotNull(printData);
        Assert.Equal("PRINTDATA", printData.Variant);

        // Should contain one IfNode
        var ifNode = printData.Content.OfType<IfNode>().FirstOrDefault();
        Assert.NotNull(ifNode);
        Assert.Equal("FLAG:1", ifNode.Condition);
    }

    /// <summary>
    /// AC#9: Parser error on unclosed PRINTDATA
    /// </summary>
    [Fact]
    public void UnclosedPrintData()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "unclosed_printdata.erb");

        // Act & Assert
        Assert.Throws<ParseException>(() => parser.Parse(testFile));
    }

    /// <summary>
    /// AC#10: PrintDataNode.GetDataForms() extracts content
    /// </summary>
    [Fact]
    public void PrintDataGetDataForms_ExtractsContent()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "simple_printdata.erb");

        // Act
        var ast = parser.Parse(testFile);
        var printData = ast.OfType<PrintDataNode>().First();
        var dataForms = printData.GetDataForms().ToList();

        // Assert
        Assert.Equal(2, dataForms.Count);
        Assert.Equal("line1", dataForms[0].Arguments[0]);
        Assert.Equal("line2", dataForms[1].Arguments[0]);
    }

    /// <summary>
    /// AC#10: GetDataForms() recursively extracts from nested DATALIST
    /// </summary>
    [Fact]
    public void PrintDataGetDataForms_FromNestedDatalist()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "printdata_nested_datalist.erb");

        // Act
        var ast = parser.Parse(testFile);
        var printData = ast.OfType<PrintDataNode>().First();
        var dataForms = printData.GetDataForms().ToList();

        // Assert
        Assert.Equal(2, dataForms.Count);
        Assert.Equal("hello", dataForms[0].Arguments[0]);
        Assert.Equal("world", dataForms[1].Arguments[0]);
    }

    /// <summary>
    /// AC#10: GetDataForms() recursively extracts from IF blocks
    /// </summary>
    [Fact]
    public void PrintDataGetDataForms_FromConditional()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "printdata_conditional.erb");

        // Act
        var ast = parser.Parse(testFile);
        var printData = ast.OfType<PrintDataNode>().First();
        var dataForms = printData.GetDataForms().ToList();

        // Assert
        Assert.Single(dataForms);
        Assert.Equal("conditional line", dataForms[0].Arguments[0]);
    }
}
