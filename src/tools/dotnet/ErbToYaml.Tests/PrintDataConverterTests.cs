using System;
using System.Collections.Generic;
using System.Linq;
using ErbParser.Ast;
using ErbToYaml;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ErbToYaml.Tests;

/// <summary>
/// TDD tests for Feature 634: Batch Conversion Tool - PrintDataConverter component
/// RED state tests - implementation does not exist yet
/// Tests AC#13
/// </summary>
public class PrintDataConverterTests
{
    private readonly IDeserializer _yamlDeserializer;

    public PrintDataConverterTests()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// AC#13: Test conversion of simple PrintDataNode with single DATAFORM
    /// Expected: PrintDataNode converts to YAML with character, situation, and lines
    /// </summary>
    [Fact]
    public void Test_SimplePrintData_SingleDataForm()
    {
        // Arrange - Create PrintDataNode with single DATAFORM
        var printDataNode = new PrintDataNode
        {
            Variant = "PRINTDATA"
        };

        var dataformNode = new DataformNode();
        dataformNode.Arguments.Add("テストセリフです");
        printDataNode.Content.Add(dataformNode);

        var converter = new PrintDataConverter();

        // Act
        var yaml = converter.Convert(printDataNode, "美鈴", "K1_愛撫");

        // Assert
        Assert.NotNull(yaml);
        Assert.NotEmpty(yaml);

        // Verify YAML structure
        Assert.Contains("character: 美鈴", yaml);
        Assert.Contains("situation: K1_愛撫", yaml);
        Assert.Contains("entries:", yaml);
        Assert.Contains("テストセリフです", yaml);
    }

    /// <summary>
    /// AC#13: Test conversion of PrintDataNode with multiple DATAFORMs
    /// Expected: All DATAFORM lines appear in YAML output
    /// </summary>
    [Fact]
    public void Test_PrintData_MultipleForms()
    {
        // Arrange - Create PrintDataNode with multiple DATAFORMs
        var printDataNode = new PrintDataNode
        {
            Variant = "PRINTDATA"
        };

        var dataform1 = new DataformNode();
        dataform1.Arguments.Add("セリフ1");
        var dataform2 = new DataformNode();
        dataform2.Arguments.Add("セリフ2");
        var dataform3 = new DataformNode();
        dataform3.Arguments.Add("セリフ3");

        printDataNode.Content.Add(dataform1);
        printDataNode.Content.Add(dataform2);
        printDataNode.Content.Add(dataform3);

        var converter = new PrintDataConverter();

        // Act
        var yaml = converter.Convert(printDataNode, "咲夜", "K2_Test");

        // Assert
        Assert.NotNull(yaml);

        // Verify all dialogue lines are present
        Assert.Contains("セリフ1", yaml);
        Assert.Contains("セリフ2", yaml);
        Assert.Contains("セリフ3", yaml);
    }

    /// <summary>
    /// AC#13: Test PrintDataNode with nested DATALIST extracts DataForms
    /// Expected: GetDataForms() helper extracts DataformNodes from nested DATALIST
    /// </summary>
    [Fact]
    public void Test_PrintData_WithNestedDatalist()
    {
        // Arrange - Create PrintDataNode containing DATALIST
        var printDataNode = new PrintDataNode
        {
            Variant = "PRINTDATA"
        };

        var datalistNode = new DatalistNode();
        var dataform1 = new DataformNode();
        dataform1.Arguments.Add("リスト内セリフ1");
        var dataform2 = new DataformNode();
        dataform2.Arguments.Add("リスト内セリフ2");

        datalistNode.DataForms.Add(dataform1);
        datalistNode.DataForms.Add(dataform2);
        printDataNode.Content.Add(datalistNode);

        var converter = new PrintDataConverter();

        // Act
        var yaml = converter.Convert(printDataNode, "レミリア", "K3_Test");

        // Assert
        Assert.NotNull(yaml);
        Assert.Contains("リスト内セリフ1", yaml);
        Assert.Contains("リスト内セリフ2", yaml);
    }

    /// <summary>
    /// AC#13: Test PrintDataNode uses GetDataForms() helper
    /// Expected: GetDataForms() correctly extracts DataForms from Content tree
    /// </summary>
    [Fact]
    public void Test_PrintData_GetDataFormsHelper()
    {
        // Arrange - Create complex PrintDataNode structure
        var printDataNode = new PrintDataNode
        {
            Variant = "PRINTDATA"
        };

        // Direct DATAFORM
        var directDataform = new DataformNode();
        directDataform.Arguments.Add("Direct line");
        printDataNode.Content.Add(directDataform);

        // DATALIST with DATAFORMs
        var datalist = new DatalistNode();
        var listDataform = new DataformNode();
        listDataform.Arguments.Add("List line");
        datalist.DataForms.Add(listDataform);
        printDataNode.Content.Add(datalist);

        // Act - Use GetDataForms() helper
        var dataforms = printDataNode.GetDataForms().ToList();

        // Assert
        Assert.Equal(2, dataforms.Count);
        Assert.Contains(dataforms, df => df.Arguments.Contains("Direct line"));
        Assert.Contains(dataforms, df => df.Arguments.Contains("List line"));
    }

    /// <summary>
    /// AC#13: Test output YAML is deserializable
    /// Expected: Generated YAML can be parsed back to object structure
    /// </summary>
    [Fact]
    public void Test_PrintData_OutputIsValidYaml()
    {
        // Arrange
        var printDataNode = new PrintDataNode
        {
            Variant = "PRINTDATA"
        };

        var dataform = new DataformNode();
        dataform.Arguments.Add("テストライン");
        printDataNode.Content.Add(dataform);

        var converter = new PrintDataConverter();

        // Act
        var yaml = converter.Convert(printDataNode, "パチュリー", "K4_Test");

        // Assert - Should be parseable as YAML
        var exception = Record.Exception(() =>
        {
            var yamlObject = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);
            Assert.NotNull(yamlObject);
            Assert.True(yamlObject.ContainsKey("character"));
            Assert.True(yamlObject.ContainsKey("situation"));
            Assert.True(yamlObject.ContainsKey("entries"));
        });

        Assert.Null(exception);
    }

    /// <summary>
    /// AC#13: Test PrintDataConverter handles empty PrintDataNode
    /// Expected: Empty PrintDataNode generates valid YAML with empty branches
    /// </summary>
    [Fact]
    public void Test_PrintData_EmptyContent()
    {
        // Arrange - Empty PrintDataNode
        var printDataNode = new PrintDataNode
        {
            Variant = "PRINTDATA"
        };
        // No content added

        var converter = new PrintDataConverter();

        // Act
        var yaml = converter.Convert(printDataNode, "フラン", "K5_Test");

        // Assert - Should generate valid YAML structure even with no lines
        Assert.NotNull(yaml);
        Assert.Contains("character: フラン", yaml);
        Assert.Contains("situation: K5_Test", yaml);
        Assert.Contains("entries:", yaml);
    }

    /// <summary>
    /// AC#13: Test PrintDataConverter with null arguments throws
    /// Expected: ArgumentNullException when printData is null
    /// </summary>
    [Fact]
    public void Test_PrintData_NullInput_Throws()
    {
        // Arrange
        var converter = new PrintDataConverter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            converter.Convert(null!, "美鈴", "K1");
        });
    }

    /// <summary>
    /// AC#13: Test different PRINTDATA variants (L, W, K, D, etc.)
    /// Expected: Variant metadata is preserved in output (F671 will map to display behavior)
    /// </summary>
    [Fact]
    public void Test_PrintData_DifferentVariants()
    {
        // Arrange - Test multiple PRINTDATA variants
        var variants = new[] { "PRINTDATA", "PRINTDATAL", "PRINTDATAW", "PRINTDATAK", "PRINTDATAD" };

        var converter = new PrintDataConverter();

        foreach (var variant in variants)
        {
            var printDataNode = new PrintDataNode
            {
                Variant = variant
            };

            var dataform = new DataformNode();
            dataform.Arguments.Add($"Test line for {variant}");
            printDataNode.Content.Add(dataform);

            // Act
            var yaml = converter.Convert(printDataNode, "美鈴", "K1");

            // Assert - All variants should produce valid YAML
            // Note: F634 converts content only; variant semantics deferred to F671
            Assert.NotNull(yaml);
            Assert.Contains("character: 美鈴", yaml);
            Assert.Contains($"Test line for {variant}", yaml);
        }
    }

    /// <summary>
    /// F671 AC#5: Test PRINTDATAL produces displayMode: newline
    /// TDD RED state - displayMode feature not yet implemented
    /// </summary>
    [Fact]
    public void Convert_PrintDataL_ProducesDisplayModeNewline()
    {
        // Arrange
        var printDataNode = new PrintDataNode
        {
            Variant = "PRINTDATAL"
        };

        var dataform = new DataformNode();
        dataform.Arguments.Add("テストセリフ");
        printDataNode.Content.Add(dataform);

        var converter = new PrintDataConverter();

        // Act
        var yaml = converter.Convert(printDataNode, "美鈴", "K1_愛撫");

        // Assert
        var yamlObject = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);
        var entries = (List<object>)yamlObject["entries"];
        var firstEntry = (Dictionary<object, object>)entries[0];

        Assert.True(firstEntry.ContainsKey("displayMode"), "displayMode key should be present for PRINTDATAL");
        Assert.Equal("newline", firstEntry["displayMode"].ToString());
    }

    /// <summary>
    /// F671 AC#6, AC#10: Test all PRINTDATA variants map to correct displayMode values
    /// TDD RED state - displayMode mapping not yet implemented
    /// </summary>
    [Theory]
    [InlineData("PRINTDATA", null)]
    [InlineData("PRINTDATAL", "newline")]
    [InlineData("PRINTDATAW", "wait")]
    [InlineData("PRINTDATAK", "keyWait")]
    [InlineData("PRINTDATAKL", "keyWaitNewline")]
    [InlineData("PRINTDATAKW", "keyWaitWait")]
    [InlineData("PRINTDATAD", "display")]
    [InlineData("PRINTDATADL", "displayNewline")]
    [InlineData("PRINTDATADW", "displayWait")]
    public void Convert_AllVariantsMapping_ProduceCorrectDisplayMode(string variant, string? expectedDisplayMode)
    {
        // Arrange
        var printDataNode = new PrintDataNode
        {
            Variant = variant
        };

        var dataform = new DataformNode();
        dataform.Arguments.Add("テストセリフ");
        printDataNode.Content.Add(dataform);

        var converter = new PrintDataConverter();

        // Act
        var yaml = converter.Convert(printDataNode, "美鈴", "K1_愛撫");

        // Assert
        var yamlObject = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);
        var entries = (List<object>)yamlObject["entries"];
        var firstEntry = (Dictionary<object, object>)entries[0];

        if (expectedDisplayMode == null)
        {
            Assert.False(firstEntry.ContainsKey("displayMode"), $"displayMode should be absent for {variant}");
        }
        else
        {
            Assert.True(firstEntry.ContainsKey("displayMode"), $"displayMode should be present for {variant}");
            Assert.Equal(expectedDisplayMode, firstEntry["displayMode"].ToString());
        }
    }

    /// <summary>
    /// F671 AC#9: Test default PRINTDATA omits displayMode for backward compatibility
    /// TDD RED state - verification test (may pass if implementation hasn't changed)
    /// </summary>
    [Fact]
    public void Convert_DefaultPrintData_OmitsDisplayMode_BackwardCompatibility()
    {
        // Arrange
        var printDataNode = new PrintDataNode
        {
            Variant = "PRINTDATA"
        };

        var dataform = new DataformNode();
        dataform.Arguments.Add("テストセリフ");
        printDataNode.Content.Add(dataform);

        var converter = new PrintDataConverter();

        // Act
        var yaml = converter.Convert(printDataNode, "美鈴", "K1_愛撫");

        // Assert
        var yamlObject = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);
        var entries = (List<object>)yamlObject["entries"];
        var firstEntry = (Dictionary<object, object>)entries[0];

        Assert.False(firstEntry.ContainsKey("displayMode"), "displayMode should not be present for default PRINTDATA");
    }
}
