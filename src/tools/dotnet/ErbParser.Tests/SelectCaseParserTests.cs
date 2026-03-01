using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// Tests for SELECTCASE ARG parsing (Feature 765 - AC#3, AC#4, AC#14)
/// </summary>
public class SelectCaseParserTests
{
    /// <summary>
    /// AC#3: Parse basic SELECTCASE ARG structure
    /// Expected: SelectCaseNode with Subject="ARG", 2 CaseBranch entries, CaseElse body
    /// </summary>
    [Fact]
    public void ParseSelectCase_BasicStructure()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "selectcase_basic.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert (F764: @TEST_SELECTCASE creates FunctionDefNode, SelectCaseNode is inside Body)
        var func = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(func);
        var node = func.Body.OfType<SelectCaseNode>().FirstOrDefault();
        Assert.NotNull(node);
        Assert.Equal("ARG", node.Subject);
        Assert.Equal(2, node.Branches.Count);

        // CASE 13,25
        Assert.Equal(2, node.Branches[0].Values.Count);
        Assert.Equal("13", node.Branches[0].Values[0]);
        Assert.Equal("25", node.Branches[0].Values[1]);
        Assert.Single(node.Branches[0].Body);

        // CASE 21
        Assert.Single(node.Branches[1].Values);
        Assert.Equal("21", node.Branches[1].Values[0]);

        // CASEELSE
        Assert.NotNull(node.CaseElse);
        Assert.NotEmpty(node.CaseElse);
    }

    /// <summary>
    /// AC#4: Parse CASEELSE with nested IF/ELSE/ENDIF
    /// Expected: CaseElse body contains IfNode with correct structure
    /// </summary>
    [Fact]
    public void ParseSelectCase_CaseElseWithNestedIf()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "selectcase_nested_if.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert (F764: @TEST_SELECTCASE_NESTED_IF creates FunctionDefNode, SelectCaseNode is inside Body)
        var func = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(func);
        var node = func.Body.OfType<SelectCaseNode>().FirstOrDefault();
        Assert.NotNull(node);
        Assert.NotNull(node.CaseElse);

        // CASEELSE should contain an IfNode
        var ifNode = node.CaseElse.OfType<IfNode>().FirstOrDefault();
        Assert.NotNull(ifNode);

        // Verify nested IF has condition
        Assert.Contains("EQUIP", ifNode.Condition);

        // Verify nested IF has body
        Assert.NotEmpty(ifNode.Body);

        // Verify nested IF has ELSE branch
        Assert.NotNull(ifNode.ElseBranch);
        Assert.NotEmpty(ifNode.ElseBranch.Body);
    }

    /// <summary>
    /// AC#14: Unclosed SELECTCASE throws ParseException
    /// Expected: ParseException with message indicating unclosed SELECTCASE
    /// </summary>
    [Fact]
    public void ParseSelectCase_UnclosedThrowsException()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "selectcase_unclosed.erb");

        // Act & Assert
        var exception = Assert.Throws<ParseException>(() => parser.Parse(testFile));
        Assert.Contains("SELECTCASE", exception.Message);
        Assert.Contains("ENDSELECT", exception.Message);
    }
}
