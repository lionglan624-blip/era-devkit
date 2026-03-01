using System.Collections.Generic;
using ErbParser.Ast;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// Tests for recursive ContainsConvertibleContent (AC#7)
/// Uses reflection to access the private method
/// </summary>
public class RecursiveConvertibleTests
{
    // Test through ErbParser + FileConverter integration
    // A nested IF LOCAL -> IF TALENT -> PRINTDATA should be detected

    [Fact]
    public void RecursiveConvertible_NestedIfWithPrintData_DetectsContent()
    {
        // Create nested structure: IF -> IF -> PRINTDATA
        var innerPrintData = new PrintDataNode
        {
            LineNumber = 3,
            SourceFile = "test.erb",
            Variant = "PRINTDATA"
        };
        // Add a DataformNode to inner PRINTDATA
        var dataformNode = new DataformNode
        {
            LineNumber = 4,
            SourceFile = "test.erb"
        };
        dataformNode.Arguments.Add("inner content");
        innerPrintData.Content.Add(dataformNode);

        var innerIf = new IfNode
        {
            LineNumber = 2,
            SourceFile = "test.erb",
            Condition = "TALENT:恋人"
        };
        innerIf.Body.Add(innerPrintData);

        var outerIf = new IfNode
        {
            LineNumber = 1,
            SourceFile = "test.erb",
            Condition = "LOCAL"
        };
        outerIf.Body.Add(innerIf);

        // Use FileConverter to check if outer IF is included
        // The bug: ContainsConvertibleContent only checks direct children
        // So outerIf.Body has an IfNode (not PrintDataNode), returns false

        // Create minimal mocks for FileConverter
        var pathAnalyzer = new TestPathAnalyzer();
        var printDataConverter = new TestPrintDataConverter();
        var datalistConverter = new TestDatalistConverter();

        var converter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter);

        // Parse the AST manually and check via reflection
        var method = typeof(FileConverter).GetMethod("ContainsConvertibleContent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);

        var result = (bool)method!.Invoke(converter, new object[] { outerIf })!;
        Assert.True(result, "Should detect nested PRINTDATA at depth 2");
    }

    [Fact]
    public void RecursiveConvertible_DirectPrintData_DetectsContent()
    {
        // Baseline: direct PRINTDATA should still work
        var printData = new PrintDataNode
        {
            LineNumber = 2,
            SourceFile = "test.erb",
            Variant = "PRINTDATA"
        };
        var dataformNode = new DataformNode
        {
            LineNumber = 3,
            SourceFile = "test.erb"
        };
        dataformNode.Arguments.Add("content");
        printData.Content.Add(dataformNode);

        var ifNode = new IfNode
        {
            LineNumber = 1,
            SourceFile = "test.erb",
            Condition = "TALENT:恋人"
        };
        ifNode.Body.Add(printData);

        var pathAnalyzer = new TestPathAnalyzer();
        var printDataConverter = new TestPrintDataConverter();
        var datalistConverter = new TestDatalistConverter();

        var converter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter);

        var method = typeof(FileConverter).GetMethod("ContainsConvertibleContent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);

        var result = (bool)method!.Invoke(converter, new object[] { ifNode })!;
        Assert.True(result, "Should detect direct PRINTDATA child");
    }

    [Fact]
    public void RecursiveConvertible_ThreeLevelNested_DetectsContent()
    {
        // Three levels deep: IF -> IF -> IF -> PRINTDATA
        var printData = new PrintDataNode
        {
            LineNumber = 4,
            SourceFile = "test.erb",
            Variant = "PRINTDATA"
        };
        var dataformNode = new DataformNode
        {
            LineNumber = 5,
            SourceFile = "test.erb"
        };
        dataformNode.Arguments.Add("deep content");
        printData.Content.Add(dataformNode);

        var level3 = new IfNode
        {
            LineNumber = 3,
            SourceFile = "test.erb",
            Condition = "ABL:0:5 >= 3"
        };
        level3.Body.Add(printData);

        var level2 = new IfNode
        {
            LineNumber = 2,
            SourceFile = "test.erb",
            Condition = "TALENT:恋人"
        };
        level2.Body.Add(level3);

        var level1 = new IfNode
        {
            LineNumber = 1,
            SourceFile = "test.erb",
            Condition = "LOCAL"
        };
        level1.Body.Add(level2);

        var pathAnalyzer = new TestPathAnalyzer();
        var printDataConverter = new TestPrintDataConverter();
        var datalistConverter = new TestDatalistConverter();

        var converter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter);

        var method = typeof(FileConverter).GetMethod("ContainsConvertibleContent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);

        var result = (bool)method!.Invoke(converter, new object[] { level1 })!;
        Assert.True(result, "Should detect PRINTDATA at depth 3");
    }

    // Minimal test doubles
    private sealed class TestPathAnalyzer : IPathAnalyzer
    {
        public (string Character, string Situation) Extract(string erbFilePath) => ("test", "test");
    }

    private sealed class TestPrintDataConverter : IPrintDataConverter
    {
        public string Convert(PrintDataNode node, string character, string situation) => "test: true";
    }

    private sealed class TestDatalistConverter : IDatalistConverter
    {
        public string Convert(DatalistNode node, string character, string situation) => "test: true";
        public void ValidateYaml(string yaml) { }
        public Dictionary<string, object>? ParseCondition(string condition) => null;
    }
}
