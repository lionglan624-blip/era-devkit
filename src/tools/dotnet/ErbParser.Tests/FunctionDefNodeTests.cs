using System.Linq;
using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// Tests for FunctionDefNode parsing (Feature 764 - AC#2)
/// Tests that parser produces FunctionDefNode from @-lines
/// </summary>
public class FunctionDefNodeTests
{
    private readonly ErbParser _parser = new();

    /// <summary>
    /// AC#2: Parse @-line with parameters produces FunctionDefNode
    /// Expected: FunctionDefNode with FunctionName, Parameters, Body
    /// </summary>
    [Fact]
    public void ParseFunctionDef_WithParameters_ProducesFunctionDefNode()
    {
        // Arrange
        var content = @"@KOJO_EVENT_K1_0(ARG,ARG:1)
LOCAL = 1
IF LOCAL
    PRINTFORML 「Test dialogue」
    RETURN 0
ENDIF
RETURN 0";

        // Act
        var ast = _parser.ParseString(content, "test.erb");

        // Assert
        var funcNode = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(funcNode);
        Assert.Equal("KOJO_EVENT_K1_0", funcNode.FunctionName);
        Assert.Equal(2, funcNode.Parameters.Count);
        Assert.Equal("ARG", funcNode.Parameters[0]);
        Assert.Equal("ARG:1", funcNode.Parameters[1]);
        Assert.NotEmpty(funcNode.Body);
    }

    /// <summary>
    /// AC#2: Parse @-line without parameters
    /// Expected: FunctionDefNode with empty Parameters list
    /// </summary>
    [Fact]
    public void ParseFunctionDef_NoParameters_ProducesFunctionDefNode()
    {
        // Arrange
        var content = @"@KOJO_K1
RETURN 1";

        // Act
        var ast = _parser.ParseString(content, "test.erb");

        // Assert
        var funcNode = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(funcNode);
        Assert.Equal("KOJO_K1", funcNode.FunctionName);
        Assert.Empty(funcNode.Parameters);
        Assert.NotEmpty(funcNode.Body);
    }

    /// <summary>
    /// AC#2: Multiple functions produce multiple FunctionDefNode
    /// Expected: AST contains 2 FunctionDefNode entries
    /// </summary>
    [Fact]
    public void ParseMultipleFunctions_ProducesMultipleFunctionDefNodes()
    {
        // Arrange
        var content = @"@KOJO_EVENT_K1_0(ARG,ARG:1)
PRINTFORML 「First function」
RETURN 0

@KOJO_EVENT_K1_7(ARG,ARG:1)
PRINTFORML 「Second function」
RETURN 0";

        // Act
        var ast = _parser.ParseString(content, "test.erb");

        // Assert
        var funcNodes = ast.OfType<FunctionDefNode>().ToList();
        Assert.Equal(2, funcNodes.Count);
        Assert.Equal("KOJO_EVENT_K1_0", funcNodes[0].FunctionName);
        Assert.Equal("KOJO_EVENT_K1_7", funcNodes[1].FunctionName);
    }

    /// <summary>
    /// AC#2: ReturnNode inside IF body
    /// Expected: ParseIfBlock must produce ReturnNode
    /// </summary>
    [Fact]
    public void ParseIfBlock_WithReturn_ProducesReturnNode()
    {
        // Arrange
        var content = @"@TEST_FUNC(ARG)
IF ARG == 2
    PRINTFORML 「Dialogue」
    RETURN 0
ENDIF";

        // Act
        var ast = _parser.ParseString(content, "test.erb");

        // Assert
        var funcNode = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(funcNode);
        var ifNode = funcNode.Body.OfType<IfNode>().FirstOrDefault();
        Assert.NotNull(ifNode);
        var returnNode = ifNode.Body.OfType<ReturnNode>().FirstOrDefault();
        Assert.NotNull(returnNode);
        Assert.Equal("0", returnNode.Value);
    }

    /// <summary>
    /// AC#2: SelectCaseNode inside IF body
    /// Expected: ParseIfBlock must produce SelectCaseNode for SELECTCASE...ENDSELECT
    /// </summary>
    [Fact]
    public void ParseIfBlock_WithSelectCase_ProducesSelectCaseNode()
    {
        // Arrange
        var content = @"@TEST_FUNC(ARG)
IF ARG == 0
    SELECTCASE RAND:3
    CASE 0
        PRINTFORML 「Case 0」
    CASE 1
        PRINTFORML 「Case 1」
    ENDSELECT
    RETURN 0
ENDIF";

        // Act
        var ast = _parser.ParseString(content, "test.erb");

        // Assert
        var funcNode = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(funcNode);
        var ifNode = funcNode.Body.OfType<IfNode>().FirstOrDefault();
        Assert.NotNull(ifNode);
        var selectCaseNode = ifNode.Body.OfType<SelectCaseNode>().FirstOrDefault();
        Assert.NotNull(selectCaseNode);
        Assert.Equal("RAND:3", selectCaseNode.Subject);
        Assert.Equal(2, selectCaseNode.Branches.Count);
    }

    /// <summary>
    /// AC#2: FunctionDefNode contains body nodes
    /// Expected: AssignmentNode, IfNode, ReturnNode all appear in Body
    /// </summary>
    [Fact]
    public void ParseFunctionDef_ContainsBodyNodes()
    {
        // Arrange
        var content = @"@KOJO_EVENT_K1_0(ARG,ARG:1)
LOCAL = 1
IF LOCAL
    IF ARG == 2
        PRINTFORML 「Test」
        RETURN 0
    ENDIF
ENDIF
RETURN 0";

        // Act
        var ast = _parser.ParseString(content, "test.erb");

        // Assert
        var funcNode = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(funcNode);

        // Body should contain AssignmentNode
        var assignmentNode = funcNode.Body.OfType<AssignmentNode>().FirstOrDefault();
        Assert.NotNull(assignmentNode);
        Assert.Equal("LOCAL", assignmentNode.Target);
        Assert.Equal("1", assignmentNode.Value);

        // Body should contain IfNode
        var ifNode = funcNode.Body.OfType<IfNode>().FirstOrDefault();
        Assert.NotNull(ifNode);
        Assert.Equal("LOCAL", ifNode.Condition);

        // Body should contain ReturnNode at end
        var returnNode = funcNode.Body.OfType<ReturnNode>().LastOrDefault();
        Assert.NotNull(returnNode);
        Assert.Equal("0", returnNode.Value);
    }
}
