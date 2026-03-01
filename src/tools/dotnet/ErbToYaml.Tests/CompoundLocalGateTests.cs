using System.Collections.Generic;
using System.Linq;
using ErbParser;
using ErbParser.Ast;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// Tests for compound LOCAL gate decomposition (AC#17)
/// When LOCAL:1=1, IF LOCAL:1 && OTHER → IF OTHER (strip LOCAL from compound)
/// </summary>
public class CompoundLocalGateTests
{
    private readonly LocalGateResolver _resolver = new();

    [Fact]
    public void CompoundLocalGate_StripLocalFromCompound_PreservesFirstTime()
    {
        // LOCAL:1 = 1, IF LOCAL:1 && FIRSTTIME → IF FIRSTTIME
        var ifNode = new IfNode
        {
            Condition = "LOCAL:1 && FIRSTTIME",
            LineNumber = 2,
            SourceFile = "test.erb"
        };
        ifNode.Body.Add(new PrintDataNode { LineNumber = 3, SourceFile = "test.erb", Variant = "PRINTDATA" });

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL:1", Value = "1", LineNumber = 1, SourceFile = "test.erb" },
            ifNode
        };

        var result = _resolver.Resolve(ast);

        // IfNode should remain but with modified condition (FIRSTTIME only)
        var ifNodes = result.OfType<IfNode>().ToList();
        Assert.Single(ifNodes);
        Assert.Equal("FIRSTTIME", ifNodes[0].Condition.Trim());
    }

    [Fact]
    public void CompoundLocalGate_StripLocalFromCompound_PreservesArgCondition()
    {
        // LOCAL:1 = 1, IF LOCAL:1 && ARG == 2 → IF ARG == 2
        var ifNode = new IfNode
        {
            Condition = "LOCAL:1 && ARG == 2",
            LineNumber = 2,
            SourceFile = "test.erb"
        };
        ifNode.Body.Add(new PrintDataNode { LineNumber = 3, SourceFile = "test.erb", Variant = "PRINTDATA" });

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL:1", Value = "1", LineNumber = 1, SourceFile = "test.erb" },
            ifNode
        };

        var result = _resolver.Resolve(ast);

        var ifNodes = result.OfType<IfNode>().ToList();
        Assert.Single(ifNodes);
        Assert.Equal("ARG == 2", ifNodes[0].Condition.Trim());
    }

    [Fact]
    public void CompoundLocalGate_DeadCodeCompound_ExcludesEntireNode()
    {
        // LOCAL:1 = 0, IF LOCAL:1 && TALENT:恋人 → exclude (dead code, && requires all true)
        var ifNode = new IfNode
        {
            Condition = "LOCAL:1 && TALENT:恋人",
            LineNumber = 2,
            SourceFile = "test.erb"
        };
        ifNode.Body.Add(new PrintDataNode { LineNumber = 3, SourceFile = "test.erb", Variant = "PRINTDATA" });

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL:1", Value = "0", LineNumber = 1, SourceFile = "test.erb" },
            ifNode
        };

        var result = _resolver.Resolve(ast);
        Assert.DoesNotContain(result, n => n is IfNode);
    }
}
