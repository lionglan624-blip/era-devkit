using System.Collections.Generic;
using System.Linq;
using ErbParser;
using ErbParser.Ast;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

public class LocalGateResolverTests
{
    private readonly LocalGateResolver _resolver = new();

    // AC#8: LocalGateDeadCode tests
    [Fact]
    public void LocalGateDeadCode_ExcludesSection()
    {
        // LOCAL = 0 followed by IF LOCAL → exclude IfNode
        var ifNode = new IfNode
        {
            Condition = "LOCAL",
            LineNumber = 2,
            SourceFile = "test.erb"
        };
        ifNode.Body.Add(new PrintDataNode { LineNumber = 3, SourceFile = "test.erb", Variant = "PRINTDATA" });

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL", Value = "0", LineNumber = 1, SourceFile = "test.erb" },
            ifNode
        };

        var result = _resolver.Resolve(ast);

        // IfNode should be excluded (dead code)
        Assert.DoesNotContain(result, n => n is IfNode);
        // AssignmentNode may or may not be in result (implementation detail)
    }

    [Fact]
    public void LocalGateDeadCode_CompoundCondition()
    {
        // LOCAL:1 = 0 followed by IF LOCAL:1 && TALENT:恋人 → exclude entire IfNode
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

    [Fact]
    public void LocalGateDeadCode_UnresolvedPreserved()
    {
        // IF LOCAL before any assignment → IfNode preserved unchanged
        var ifNode = new IfNode
        {
            Condition = "LOCAL",
            LineNumber = 1,
            SourceFile = "test.erb"
        };
        ifNode.Body.Add(new PrintDataNode { LineNumber = 2, SourceFile = "test.erb", Variant = "PRINTDATA" });

        var ast = new List<AstNode> { ifNode };
        var result = _resolver.Resolve(ast);
        Assert.Contains(result, n => n is IfNode);
    }

    // AC#9: LocalGateStrip tests
    [Fact]
    public void LocalGateStrip_PreservesContent()
    {
        // LOCAL = 1 followed by IF LOCAL → strip gate, promote body
        var printData = new PrintDataNode { LineNumber = 3, SourceFile = "test.erb", Variant = "PRINTDATA" };

        var ifNode = new IfNode
        {
            Condition = "LOCAL",
            LineNumber = 2,
            SourceFile = "test.erb"
        };
        ifNode.Body.Add(printData);

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL", Value = "1", LineNumber = 1, SourceFile = "test.erb" },
            ifNode
        };

        var result = _resolver.Resolve(ast);

        // IfNode should be stripped, body content promoted
        Assert.DoesNotContain(result, n => n is IfNode);
        Assert.Contains(result, n => n is PrintDataNode);
    }

    [Fact]
    public void LocalGateStrip_UnwrapsGate()
    {
        // LOCAL:1 = 1 followed by IF LOCAL:1 → unwrap, preserve body
        var datalist = new DatalistNode { LineNumber = 3, SourceFile = "test.erb" };

        var ifNode = new IfNode
        {
            Condition = "LOCAL:1",
            LineNumber = 2,
            SourceFile = "test.erb"
        };
        ifNode.Body.Add(datalist);

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL:1", Value = "1", LineNumber = 1, SourceFile = "test.erb" },
            ifNode
        };

        var result = _resolver.Resolve(ast);
        Assert.DoesNotContain(result, n => n is IfNode);
        Assert.Contains(result, n => n is DatalistNode);
    }

    [Fact]
    public void LocalGateStrip_NegativeOneGate()
    {
        // LOCAL = -1 (non-zero = truthy) → strip gate
        var printData = new PrintDataNode { LineNumber = 3, SourceFile = "test.erb", Variant = "PRINTDATA" };

        var ifNode = new IfNode
        {
            Condition = "LOCAL",
            LineNumber = 2,
            SourceFile = "test.erb"
        };
        ifNode.Body.Add(printData);

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL", Value = "-1", LineNumber = 1, SourceFile = "test.erb" },
            ifNode
        };

        var result = _resolver.Resolve(ast);
        Assert.DoesNotContain(result, n => n is IfNode);
        Assert.Contains(result, n => n is PrintDataNode);
    }

    // Sequential reassignment test
    [Fact]
    public void LocalGateDeadCode_SequentialReassignment()
    {
        // LOCAL = 1, then LOCAL = 0, then IF LOCAL → dead code (last value wins)
        var ifNode = new IfNode
        {
            Condition = "LOCAL",
            LineNumber = 3,
            SourceFile = "test.erb"
        };
        ifNode.Body.Add(new PrintDataNode { LineNumber = 4, SourceFile = "test.erb", Variant = "PRINTDATA" });

        var ast = new List<AstNode>
        {
            new AssignmentNode { Target = "LOCAL", Value = "1", LineNumber = 1, SourceFile = "test.erb" },
            new AssignmentNode { Target = "LOCAL", Value = "0", LineNumber = 2, SourceFile = "test.erb" },
            ifNode
        };

        var result = _resolver.Resolve(ast);
        Assert.DoesNotContain(result, n => n is IfNode);
    }
}
