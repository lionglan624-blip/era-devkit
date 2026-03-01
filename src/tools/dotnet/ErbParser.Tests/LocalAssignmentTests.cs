using System.Linq;
using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

public class LocalAssignmentTests
{
    private readonly ErbParser _parser = new();

    [Fact]
    public void LocalAssignment_BareLocal_ProducesAssignmentNode()
    {
        // LOCAL = 0 inside function (F764: @FUNC creates FunctionDefNode)
        var content = "@FUNC\nLOCAL = 0\nENDFUNC";
        var ast = _parser.ParseString(content, "test.erb");
        var func = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(func);
        var assignments = func.Body.OfType<AssignmentNode>().ToList();
        Assert.Single(assignments);
        Assert.Equal("LOCAL", assignments[0].Target);
        Assert.Equal("0", assignments[0].Value);
    }

    [Fact]
    public void LocalAssignment_IndexedLocal_ProducesAssignmentNode()
    {
        var content = "@FUNC\nLOCAL:1 = 1\nENDFUNC";
        var ast = _parser.ParseString(content, "test.erb");
        var func = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(func);
        var assignments = func.Body.OfType<AssignmentNode>().ToList();
        Assert.Single(assignments);
        Assert.Equal("LOCAL:1", assignments[0].Target);
        Assert.Equal("1", assignments[0].Value);
    }

    [Fact]
    public void LocalAssignment_InsideIfBlock_ProducesAssignmentNode()
    {
        // Assignment inside IF block (ParseIfBlock path, F764: @FUNC creates FunctionDefNode)
        var content = "@FUNC\nIF TALENT:恋人\nLOCAL:1 = 1\nENDIF\nENDFUNC";
        var ast = _parser.ParseString(content, "test.erb");
        var func = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(func);
        var ifNodes = func.Body.OfType<IfNode>().ToList();
        Assert.Single(ifNodes);
        var assignments = ifNodes[0].Body.OfType<AssignmentNode>().ToList();
        Assert.Single(assignments);
        Assert.Equal("LOCAL:1", assignments[0].Target);
        Assert.Equal("1", assignments[0].Value);
    }

    [Fact]
    public void LocalAssignment_InsideElseIfBranch_ProducesAssignmentNode()
    {
        var content = "@FUNC\nIF TALENT:恋人\nPRINTFORML test\nELSEIF TALENT:好感\nLOCAL = 1\nENDIF\nENDFUNC";
        var ast = _parser.ParseString(content, "test.erb");
        var func = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(func);
        var ifNodes = func.Body.OfType<IfNode>().ToList();
        Assert.Single(ifNodes);
        Assert.Single(ifNodes[0].ElseIfBranches);
        var assignments = ifNodes[0].ElseIfBranches[0].Body.OfType<AssignmentNode>().ToList();
        Assert.Single(assignments);
        Assert.Equal("LOCAL", assignments[0].Target);
        Assert.Equal("1", assignments[0].Value);
    }

    [Fact]
    public void LocalAssignment_InsideElseBranch_ProducesAssignmentNode()
    {
        var content = "@FUNC\nIF TALENT:恋人\nPRINTFORML test\nELSE\nLOCAL:1 = 0\nENDIF\nENDFUNC";
        var ast = _parser.ParseString(content, "test.erb");
        var func = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(func);
        var ifNodes = func.Body.OfType<IfNode>().ToList();
        Assert.Single(ifNodes);
        Assert.NotNull(ifNodes[0].ElseBranch);
        var assignments = ifNodes[0].ElseBranch!.Body.OfType<AssignmentNode>().ToList();
        Assert.Single(assignments);
        Assert.Equal("LOCAL:1", assignments[0].Target);
        Assert.Equal("0", assignments[0].Value);
    }

    [Fact]
    public void LocalAssignment_FunctionCallValue_ProducesAssignmentNode()
    {
        // Function-call RHS (unresolvable by static analysis, F764: @FUNC creates FunctionDefNode)
        var content = "@FUNC\nLOCAL = GET_ABL_BRANCH()\nENDFUNC";
        var ast = _parser.ParseString(content, "test.erb");
        var func = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(func);
        var assignments = func.Body.OfType<AssignmentNode>().ToList();
        Assert.Single(assignments);
        Assert.Equal("LOCAL", assignments[0].Target);
        Assert.Equal("GET_ABL_BRANCH()", assignments[0].Value);
    }

    [Fact]
    public void LocalAssignment_NegativeValue_ProducesAssignmentNode()
    {
        var content = "@FUNC\nLOCAL = -1\nENDFUNC";
        var ast = _parser.ParseString(content, "test.erb");
        var func = ast.OfType<FunctionDefNode>().FirstOrDefault();
        Assert.NotNull(func);
        var assignments = func.Body.OfType<AssignmentNode>().ToList();
        Assert.Single(assignments);
        Assert.Equal("LOCAL", assignments[0].Target);
        Assert.Equal("-1", assignments[0].Value);
    }
}
