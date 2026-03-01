using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ErbParser.Ast;
using ErbToYaml;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ErbToYaml.Tests;

/// <summary>
/// TDD tests for Feature 361: Schema Validator Integration
/// RED state tests - implementation does not exist yet
/// </summary>
public class SchemaValidationTests
{
    private static readonly System.Text.Json.JsonSerializerOptions s_indentedOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _schemaPath;
    private readonly string _talentCsvPath;
    private readonly IDeserializer _yamlDeserializer;

    public SchemaValidationTests()
    {
        _schemaPath = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "YamlSchemaGen", "dialogue-schema.json");

        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// AC#1: Test schema validation integration in converter
    /// Expected: Converter validates YAML output against schema during conversion
    /// No exception thrown for valid YAML
    ///
    /// GREEN STATE: DatalistConverter validates against schema
    /// </summary>
    [Fact]
    public async Task Test_ConverterWithSchemaValidation_DoesNotThrowForValidYaml()
    {
        // Arrange
        var erbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", "simple_datalist.erb");
        var erbContent = File.ReadAllText(erbPath);

        var parser = new ErbParser.ErbParser();
        var ast = parser.ParseString(erbContent, erbPath);

        var datalistNode = FindDatalistNode(ast);
        Assert.NotNull(datalistNode);

        // GREEN: Constructor with schema validation now exists
        var converter = new DatalistConverter(_talentCsvPath, _schemaPath);

        // Act
        var yaml = converter.Convert(datalistNode!, "美鈴", "K4");

        // Assert - verify YAML was created and validation was performed
        Assert.NotNull(yaml);

        // Verify ValidateYaml method can be called without exception for valid YAML
        converter.ValidateYaml(yaml);
    }

    /// <summary>
    /// AC#2: Test invalid YAML rejected by converter
    /// Expected: SchemaValidationException thrown when YAML missing required field 'branches'
    ///
    /// GREEN STATE: DatalistConverter throws SchemaValidationException for invalid YAML
    /// </summary>
    [Fact]
    public async Task Test_InvalidYaml_ThrowsSchemaValidationException()
    {
        // Arrange
        var invalidYamlPath = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "TestData", "invalid-kojo.yaml");

        // Verify invalid fixture exists and is actually invalid
        Assert.True(File.Exists(invalidYamlPath), "Invalid YAML fixture must exist");
        var invalidYaml = File.ReadAllText(invalidYamlPath);

        // Verify the fixture is actually invalid by testing against schema directly
        var yamlObject = _yamlDeserializer.Deserialize<object>(invalidYaml);
        var json = System.Text.Json.JsonSerializer.Serialize(yamlObject, s_indentedOptions);

        var schemaJson = await File.ReadAllTextAsync(_schemaPath, TestContext.Current.CancellationToken);
        var schema = await NJsonSchema.JsonSchema.FromJsonAsync(schemaJson, TestContext.Current.CancellationToken);
        var validationErrors = schema.Validate(json);

        // Confirm fixture is actually invalid (has validation errors)
        Assert.NotEmpty(validationErrors);

        // GREEN: Constructor with schema validation now exists
        var converter = new DatalistConverter(_talentCsvPath, _schemaPath);

        // Act & Assert
        // GREEN: ValidateYaml method now exists and should throw SchemaValidationException
        Assert.Throws<SchemaValidationException>(() =>
        {
            converter.ValidateYaml(invalidYaml);
        });
    }

    /// <summary>
    /// AC#3: Test valid YAML passes validation
    /// Expected: meirin_com0.yaml fixture passes schema validation
    ///
    /// GREEN STATE: DatalistConverter.ValidateYaml method validates successfully
    /// </summary>
    [Fact]
    public async Task Test_ValidYaml_PassesValidation()
    {
        // Arrange
        var validYamlPath = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "TestOutput", "meirin_com0.yaml");

        // Verify valid fixture exists
        Assert.True(File.Exists(validYamlPath), "Valid YAML fixture (meirin_com0.yaml) must exist");
        var validYaml = File.ReadAllText(validYamlPath);

        // Verify the fixture is actually valid by testing against schema directly
        var yamlObject = _yamlDeserializer.Deserialize<object>(validYaml);
        var json = System.Text.Json.JsonSerializer.Serialize(yamlObject, s_indentedOptions);

        var schemaJson = await File.ReadAllTextAsync(_schemaPath, TestContext.Current.CancellationToken);
        var schema = await NJsonSchema.JsonSchema.FromJsonAsync(schemaJson, TestContext.Current.CancellationToken);
        var validationErrors = schema.Validate(json);

        // Confirm fixture is actually valid (no validation errors)
        Assert.Empty(validationErrors);

        // GREEN: Constructor with schema validation now exists
        var converter = new DatalistConverter(_talentCsvPath, _schemaPath);

        // Act & Assert
        // GREEN: ValidateYaml method should NOT throw exception for valid YAML
        var exception = Record.Exception(() => converter.ValidateYaml(validYaml));
        Assert.Null(exception);
    }

    /// <summary>
    /// Helper method to find DATALIST node in AST.
    /// Uses AstExtensions.OfTypeFlatten to traverse FunctionDefNode.Body (F764).
    /// </summary>
    private DatalistNode? FindDatalistNode(System.Collections.Generic.List<ErbParser.Ast.AstNode> ast)
    {
        return ast.OfTypeFlatten<DatalistNode>().FirstOrDefault();
    }
}
