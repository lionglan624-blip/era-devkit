using System.Collections.Generic;
using System.Linq;
using ErbParser;
using ErbParser.Ast;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// Tests for LOCAL=1;IF LOCAL envelope stripping (Feature 764 - AC#4)
/// Tests that LocalGateResolver strips LOCAL=1;IF LOCAL envelope from FunctionDefNode body
/// </summary>
public class LocalEnvelopeTests
{
    private readonly LocalGateResolver _resolver = new();

    /// <summary>
    /// AC#4: LOCAL = 1; IF LOCAL envelope stripped
    /// Expected: LocalGateResolver.Resolve() returns inner body (IF ARG conditions, PRINTFORM lines)
    /// </summary>
    [Fact]
    public void LocalEnvelope_LocalEqualsOne_StripsEnvelope()
    {
        // Arrange - Create AST with LOCAL = 1; IF LOCAL wrapping inner content
        var innerIfNode = new IfNode
        {
            Condition = "ARG == 2",
            LineNumber = 4,
            SourceFile = "test.erb"
        };
        innerIfNode.Body.Add(new PrintformNode
        {
            Content = "「Test dialogue」",
            Variant = "PRINTFORML",
            LineNumber = 5,
            SourceFile = "test.erb"
        });

        var outerIfNode = new IfNode
        {
            Condition = "LOCAL",
            LineNumber = 2,
            SourceFile = "test.erb"
        };
        outerIfNode.Body.Add(innerIfNode);

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL", Value = "1", LineNumber = 1, SourceFile = "test.erb" },
            outerIfNode
        };

        // Act
        var result = _resolver.Resolve(ast);

        // Assert - Outer IF LOCAL should be stripped, inner content promoted
        Assert.DoesNotContain(result, n => n is IfNode && ((IfNode)n).Condition == "LOCAL");
        Assert.Contains(result, n => n is IfNode && ((IfNode)n).Condition == "ARG == 2");
        var innerIf = result.OfType<IfNode>().FirstOrDefault(n => n.Condition == "ARG == 2");
        Assert.NotNull(innerIf);
        Assert.Single(innerIf.Body.OfType<PrintformNode>());
    }

    /// <summary>
    /// AC#4: LOCAL = 1; IF LOCAL with multiple inner statements
    /// Expected: All inner body content promoted
    /// </summary>
    [Fact]
    public void LocalEnvelope_MultipleInnerStatements_AllPromoted()
    {
        // Arrange
        var outerIfNode = new IfNode
        {
            Condition = "LOCAL",
            LineNumber = 2,
            SourceFile = "test.erb"
        };

        // Add multiple inner statements
        var ifArg0 = new IfNode { Condition = "ARG == 0", LineNumber = 3, SourceFile = "test.erb" };
        ifArg0.Body.Add(new PrintformNode { Content = "「ARG 0」", Variant = "PRINTFORML", LineNumber = 4, SourceFile = "test.erb" });

        var ifArg1 = new IfNode { Condition = "ARG == 1", LineNumber = 5, SourceFile = "test.erb" };
        ifArg1.Body.Add(new PrintformNode { Content = "「ARG 1」", Variant = "PRINTFORML", LineNumber = 6, SourceFile = "test.erb" });

        outerIfNode.Body.Add(ifArg0);
        outerIfNode.Body.Add(ifArg1);

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL", Value = "1", LineNumber = 1, SourceFile = "test.erb" },
            outerIfNode
        };

        // Act
        var result = _resolver.Resolve(ast);

        // Assert
        Assert.DoesNotContain(result, n => n is IfNode && ((IfNode)n).Condition == "LOCAL");
        var argIfs = result.OfType<IfNode>().Where(n => n.Condition.StartsWith("ARG")).ToList();
        Assert.Equal(2, argIfs.Count);
    }

    /// <summary>
    /// AC#4: Nested LOCAL envelope with CALL and SELECTCASE
    /// Expected: Envelope stripped, inner CALL and IF ARG preserved
    /// </summary>
    [Fact]
    public void LocalEnvelope_WithCallAndSelectCase_PreservesInnerContent()
    {
        // Arrange - Simulate K1_0 structure
        var selectCaseNode = new SelectCaseNode { Subject = "RAND:3", LineNumber = 6, SourceFile = "test.erb" };
        var caseBranch = new CaseBranch();
        caseBranch.Values.Add("0");
        caseBranch.Body.Add(new PrintformNode { Content = "「Random case」", Variant = "PRINTFORML", LineNumber = 7, SourceFile = "test.erb" });
        selectCaseNode.Branches.Add(caseBranch);

        var ifArgNode = new IfNode { Condition = "ARG == 0", LineNumber = 5, SourceFile = "test.erb" };
        ifArgNode.Body.Add(selectCaseNode);

        var outerIfNode = new IfNode
        {
            Condition = "LOCAL",
            LineNumber = 2,
            SourceFile = "test.erb"
        };
        // CALL would be represented as some node - using PrintformNode as placeholder
        outerIfNode.Body.Add(new PrintformNode { Content = "CALL 立ち絵表示(...)", Variant = "PRINTFORM", LineNumber = 3, SourceFile = "test.erb" });
        outerIfNode.Body.Add(ifArgNode);

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL", Value = "1", LineNumber = 1, SourceFile = "test.erb" },
            outerIfNode
        };

        // Act
        var result = _resolver.Resolve(ast);

        // Assert
        Assert.DoesNotContain(result, n => n is IfNode && ((IfNode)n).Condition == "LOCAL");
        Assert.Contains(result, n => n is IfNode && ((IfNode)n).Condition == "ARG == 0");
        var argIf = result.OfType<IfNode>().FirstOrDefault(n => n.Condition == "ARG == 0");
        Assert.NotNull(argIf);
        Assert.Contains(argIf.Body, n => n is SelectCaseNode);
    }

    /// <summary>
    /// AC#4: LOCAL = 0; IF LOCAL should exclude body (dead code)
    /// Expected: IfNode excluded, not promoted
    /// </summary>
    [Fact]
    public void LocalEnvelope_LocalEqualsZero_ExcludesBody()
    {
        // Arrange
        var ifNode = new IfNode
        {
            Condition = "LOCAL",
            LineNumber = 2,
            SourceFile = "test.erb"
        };
        ifNode.Body.Add(new PrintformNode { Content = "「Should not appear」", Variant = "PRINTFORML", LineNumber = 3, SourceFile = "test.erb" });

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL", Value = "0", LineNumber = 1, SourceFile = "test.erb" },
            ifNode
        };

        // Act
        var result = _resolver.Resolve(ast);

        // Assert - Dead code, should be excluded
        Assert.DoesNotContain(result, n => n is IfNode);
        Assert.DoesNotContain(result, n => n is PrintformNode);
    }

    /// <summary>
    /// AC#4: FunctionDefNode.Body processing
    /// Expected: LocalGateResolver can process FunctionDefNode.Body as input
    /// </summary>
    [Fact]
    public void LocalEnvelope_FunctionDefNodeBody_ProcessesCorrectly()
    {
        // Arrange - Simulate calling Resolve on FunctionDefNode.Body
        var funcBody = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL", Value = "1", LineNumber = 1, SourceFile = "test.erb" }
        };

        var ifNode = new IfNode { Condition = "LOCAL", LineNumber = 2, SourceFile = "test.erb" };
        ifNode.Body.Add(new PrintformNode { Content = "「Inner」", Variant = "PRINTFORML", LineNumber = 3, SourceFile = "test.erb" });
        funcBody.Add(ifNode);

        // Act - This simulates FileConverter calling LocalGateResolver.Resolve(functionDefNode.Body)
        var result = _resolver.Resolve(funcBody);

        // Assert - Assignment is kept, IF LOCAL gate is stripped, PrintformNode is promoted
        Assert.Contains(result, n => n is AssignmentNode && ((AssignmentNode)n).Target == "LOCAL");
        Assert.DoesNotContain(result, n => n is IfNode && ((IfNode)n).Condition == "LOCAL");
        Assert.Contains(result, n => n is PrintformNode);
    }
}
