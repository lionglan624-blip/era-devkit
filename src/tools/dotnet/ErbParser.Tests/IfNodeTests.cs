using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#0: IfNode enhancement with ElseIf/Else branch support
/// </summary>
public class IfNodeTests
{
    [Fact]
    public void ElseIfElseBranchesExist()
    {
        // Arrange
        var parser = new ErbParser();
        var testFile = Path.Combine("TestData", "if_elseif_else.erb");

        // Act
        var ast = parser.Parse(testFile);

        // Assert
        var ifNode = ast.OfType<IfNode>().FirstOrDefault();
        Assert.NotNull(ifNode);

        // Verify ElseIfBranches property exists and is populated
        Assert.NotNull(ifNode.ElseIfBranches);
        Assert.Equal(2, ifNode.ElseIfBranches.Count); // ELSEIF TALENT:恋慕 and ELSEIF TALENT:思慕

        // Verify ElseIfBranches have correct conditions
        Assert.Contains("TALENT:恋慕", ifNode.ElseIfBranches[0].Condition);
        Assert.Contains("TALENT:思慕", ifNode.ElseIfBranches[1].Condition);

        // Verify ElseBranch property exists and is populated
        Assert.NotNull(ifNode.ElseBranch);
        Assert.NotEmpty(ifNode.ElseBranch.Body); // Should contain PRINTFORML
    }

    [Fact]
    public void IfNode_HasElseIfBranchesProperty()
    {
        // Arrange & Act
        var ifNode = new IfNode();

        // Assert
        Assert.NotNull(ifNode.ElseIfBranches);
        Assert.Empty(ifNode.ElseIfBranches); // Should be empty initially
    }

    [Fact]
    public void IfNode_HasElseBranchProperty()
    {
        // Arrange & Act
        var ifNode = new IfNode();

        // Assert
        Assert.Null(ifNode.ElseBranch); // Should be null initially
    }
}
