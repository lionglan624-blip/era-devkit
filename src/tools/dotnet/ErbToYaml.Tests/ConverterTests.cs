using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErbParser;
using ErbParser.Ast;
using ErbToYaml;
using NJsonSchema;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ErbToYaml.Tests;

/// <summary>
/// TDD tests for Feature 349: DATALIST→YAML Converter
/// Phase 3 - RED state tests (implementation does not exist yet)
/// </summary>
public class ConverterTests
{
    private static readonly System.Text.Json.JsonSerializerOptions s_indentedOptions = new()
    {
        WriteIndented = true
    };

    private readonly IDeserializer _yamlDeserializer;
    private readonly string _schemaPath;
    private readonly string _talentCsvPath;

    public ConverterTests()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _schemaPath = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "YamlSchemaGen", "dialogue-schema.json");

        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
    }

    /// <summary>
    /// AC#1: Test converting simple DATALIST to YAML
    /// Expected: Simple DATALIST with 2 DATAFORM lines converts to YAML with branches array
    /// </summary>
    [Fact]
    public void ConvertSimpleDatalist()
    {
        // Arrange
        var erbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", "simple_datalist.erb");
        var erbContent = File.ReadAllText(erbPath);

        var parser = new ErbParser.ErbParser();
        var ast = parser.ParseString(erbContent, erbPath);

        // Find DATALIST node
        var datalistNode = FindDatalistNode(ast);
        Assert.NotNull(datalistNode);

        var converter = new DatalistConverter(_talentCsvPath);

        // Act
        var yaml = converter.Convert(datalistNode!, "美鈴", "K4");

        // Assert
        Assert.NotNull(yaml);
        Assert.Contains("character: 美鈴", yaml);
        Assert.Contains("situation: K4", yaml);
        Assert.Contains("entries:", yaml);
        Assert.Contains("初めまして、です", yaml);
        Assert.Contains("よろしくお願いしますね", yaml);
    }

    /// <summary>
    /// AC#2: Test embedding TALENT conditions with CSV lookup
    /// Expected: TalentRef objects are converted to numeric indices via Talent.csv lookup
    /// Output YAML contains condition objects matching dialogue-schema.json structure
    /// </summary>
    [Fact]
    public void EmbedConditions()
    {
        // Arrange
        var erbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", "datalist_with_conditions.erb");
        var erbContent = File.ReadAllText(erbPath);

        var parser = new ErbParser.ErbParser();
        var ast = parser.ParseString(erbContent, erbPath);

        var datalistNode = FindDatalistNode(ast);
        Assert.NotNull(datalistNode);

        var converter = new DatalistConverter(_talentCsvPath);

        // Act
        var yaml = converter.Convert(datalistNode!, "美鈴", "K100");

        // Assert
        Assert.NotNull(yaml);

        // Verify YAML structure contains condition objects
        Assert.Contains("condition:", yaml);
        Assert.Contains("type: Talent", yaml);
        Assert.Contains("talentType", yaml);

        // Verify that TALENT names are converted to numeric indices
        // (恋慕 should be converted to its CSV index, not appear as string)
        Assert.DoesNotContain("恋慕", yaml);

        // Verify dialogue lines from conditional branches
        Assert.Contains("最近一緒にいると、胸がドキドキするのです...", yaml);
        Assert.Contains("一緒にいると、安心します", yaml);
        Assert.Contains("こんにちは", yaml);
    }

    /// <summary>
    /// AC#3: Test schema validation
    /// Expected: Converter output validates against dialogue-schema.json
    /// </summary>
    [Fact]
    public async Task SchemaValidation()
    {
        // Arrange
        var erbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", "simple_datalist.erb");
        var erbContent = File.ReadAllText(erbPath);

        var parser = new ErbParser.ErbParser();
        var ast = parser.ParseString(erbContent, erbPath);

        var datalistNode = FindDatalistNode(ast);
        Assert.NotNull(datalistNode);

        var converter = new DatalistConverter(_talentCsvPath);

        // Act
        var yaml = converter.Convert(datalistNode!, "美鈴", "K4");

        // Convert YAML to JSON for schema validation
        var yamlObject = _yamlDeserializer.Deserialize<object>(yaml);
        var json = System.Text.Json.JsonSerializer.Serialize(yamlObject, s_indentedOptions);

        // Load schema
        var schemaJson = await File.ReadAllTextAsync(_schemaPath, TestContext.Current.CancellationToken);
        var schema = await JsonSchema.FromJsonAsync(schemaJson, TestContext.Current.CancellationToken);

        // Assert
        var validationErrors = schema.Validate(json);
        Assert.Empty(validationErrors);
    }

    /// <summary>
    /// AC#4: Test exception for malformed AST
    /// Expected: Converter throws appropriate exception with clear error message
    /// </summary>
    [Fact]
    public void InvalidInput()
    {
        // Arrange
        var converter = new DatalistConverter(_talentCsvPath);
        DatalistNode? invalidNode = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            converter.Convert(invalidNode!, "美鈴", "K4"));
    }

    /// <summary>
    /// AC#5: Test graceful handling of missing condition references
    /// Expected: No exception thrown, warning logged to Console.Error,
    /// output YAML contains empty condition object {}
    /// </summary>
    [Fact]
    public void MissingCondition()
    {
        // Arrange
        // Create a DATALIST node with a non-existent TALENT reference
        var datalistNode = new DatalistNode();
        var ifNode = new IfNode
        {
            Condition = "TALENT:NonExistentTalent"
        };
        var dataformNode = new DataformNode();
        dataformNode.Arguments.Add("Test dialogue");
        ifNode.Body.Add(dataformNode);

        // Add IF node to DATALIST's conditional branches
        datalistNode.ConditionalBranches.Add(ifNode);

        var converter = new DatalistConverter(_talentCsvPath);

        // Capture Console.Error output
        var originalError = Console.Error;
        using var errorWriter = new StringWriter();
        Console.SetError(errorWriter);

        try
        {
            // Act
            var yaml = converter.Convert(datalistNode, "美鈴", "K4");

            // Assert
            Assert.NotNull(yaml);

            // Verify warning was logged to Console.Error
            var errorOutput = errorWriter.ToString();
            Assert.Contains("NonExistentTalent", errorOutput);
            Assert.Contains("not found", errorOutput, StringComparison.OrdinalIgnoreCase);

            // Verify output contains empty condition object (schema-valid)
            var yamlObject = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);
            Assert.NotNull(yamlObject);

            // The YAML should be valid even with missing condition
            Assert.Contains("entries", yamlObject.Keys);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    /// AC#3: Test DATALIST inside FunctionDefNode backward compatibility
    /// Expected: FileConverter can convert files with @-function definitions wrapping DATALIST blocks
    /// Feature 764 - FunctionDefNode must not break existing DATALIST conversion
    /// </summary>
    [Fact]
    public async Task DatalistInFunction()
    {
        // Arrange - Create ERB file with @-function definition wrapping DATALIST
        var testDir = Path.Combine(Path.GetTempPath(), $"DatalistInFunctionTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);

        try
        {
            var charDir = Path.Combine(testDir, "1_美鈴");
            Directory.CreateDirectory(charDir);
            var erbPath = Path.Combine(charDir, "KOJO_K1_愛撫.ERB");

            // Create synthetic ERB with @-function definition + DATALIST
            var content = @"@KOJO_MESSAGE_COM_K1_0_1
;TALENT branching with DATALIST
IF TALENT:恋人
    PRINTDATA
        DATALIST
            DATAFORM
            DATAFORM 「んっ……そこ、気持ちいい……」
            DATAFORM %CALLNAME:人物_美鈴%は%CALLNAME:MASTER%に身を預け、されるがままになっている。
        ENDLIST
    ENDDATA
    PRINTFORMW
ELSEIF TALENT:恋慕
    PRINTDATA
        DATALIST
            DATAFORM
            DATAFORM 「ひゃっ……！　ちょ、ちょっと%CALLNAME:MASTER%……」
            DATAFORM %CALLNAME:人物_美鈴%は驚いた顔をしながらも、拒むことはしなかった。
        ENDLIST
    ENDDATA
    PRINTFORMW
ENDIF
RETURN RESULT";

            File.WriteAllText(erbPath, content);

            var pathAnalyzer = new PathAnalyzer();
            var talentLoader = new TalentCsvLoader(_talentCsvPath);
            var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
            var printDataConverter = new PrintDataConverter();
            var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader, null, null);

            // Act - FileConverter should successfully convert despite FunctionDefNode wrapper
            var results = await fileConverter.ConvertAsync(erbPath, charDir);

            // Assert
            Assert.NotEmpty(results);
            var successResult = results.FirstOrDefault(r => r.Success);
            Assert.NotNull(successResult);
            Assert.True(successResult.Success, $"Expected success but got error: {successResult.Error}");
            Assert.Null(successResult.Error);

            // Verify YAML file was created (DATALIST inside function should still be converted)
            var yamlFiles = Directory.GetFiles(charDir, "*.yaml");
            Assert.NotEmpty(yamlFiles);

            // Verify YAML content contains dialogue from DATALIST
            var yamlContent = File.ReadAllText(yamlFiles[0]);
            Assert.Contains("character: 美鈴", yamlContent);
            Assert.Contains("situation: K1", yamlContent);
            Assert.Contains("んっ……そこ、気持ちいい……", yamlContent);
            Assert.Contains("ひゃっ……！　ちょ、ちょっと", yamlContent);
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, recursive: true);
        }
    }

    /// <summary>
    /// Helper method to find DATALIST node in AST.
    /// Uses AstExtensions.OfTypeFlatten to traverse FunctionDefNode.Body (F764).
    /// </summary>
    private DatalistNode? FindDatalistNode(List<AstNode> ast)
    {
        return ast.OfTypeFlatten<DatalistNode>().FirstOrDefault();
    }
}
