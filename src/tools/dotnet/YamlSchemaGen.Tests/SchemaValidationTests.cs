using System.Text.Json;
using NJsonSchema;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlSchemaGen.Tests;

public class SchemaValidationTests
{
    private static readonly string SchemaPath = Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..", "YamlSchemaGen", "dialogue-schema.json");

    private static readonly string SampleYamlPath = Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..", "..", "..", "..", "test", "fixtures", "yaml", "sample-dialogue.yaml");

    [Fact]
    public async Task Schema_ValidatesSampleDialogue()
    {
        // Arrange
        Assert.True(File.Exists(SchemaPath), $"Schema file not found at: {Path.GetFullPath(SchemaPath)}");
        Assert.True(File.Exists(SampleYamlPath), $"Sample YAML file not found at: {Path.GetFullPath(SampleYamlPath)}");

        var schemaJson = await File.ReadAllTextAsync(SchemaPath, TestContext.Current.CancellationToken);
        var schema = await JsonSchema.FromJsonAsync(schemaJson, TestContext.Current.CancellationToken);

        var yamlContent = await File.ReadAllTextAsync(SampleYamlPath, TestContext.Current.CancellationToken);

        // Use YamlDotNet with proper type preservation
        var deserializer = new DeserializerBuilder()
            .Build();
        var yamlObject = deserializer.Deserialize(yamlContent);

        // Convert to JSON with proper type preservation (matching YamlValidator pipeline)
        var jsonContent = JsonSerializer.Serialize(ConvertToJsonCompatible(yamlObject));

        // Act
        var errors = schema.Validate(jsonContent);

        // Assert
        if (errors.Count > 0)
        {
            // Debug: Print errors for diagnosis
            foreach (var error in errors)
            {
                System.Diagnostics.Debug.WriteLine($"Validation Error: {error}");
            }
        }
        Assert.Empty(errors);
    }

    private static object? ConvertToJsonCompatible(object? yamlObject)
    {
        return yamlObject switch
        {
            Dictionary<object, object> dict => dict.ToDictionary(
                kvp => kvp.Key.ToString() ?? string.Empty,
                kvp => ConvertToJsonCompatible(kvp.Value)
            ),
            List<object> list => list.Select(ConvertToJsonCompatible).ToList(),
            _ => yamlObject
        };
    }

    [Fact]
    public async Task Schema_ContainsTalentDefinition()
    {
        // Arrange
        Assert.True(File.Exists(SchemaPath), $"Schema file not found at: {Path.GetFullPath(SchemaPath)}");

        var schemaJson = await File.ReadAllTextAsync(SchemaPath, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("TALENT", schemaJson);
    }

    [Fact]
    public async Task Schema_ContainsAblDefinition()
    {
        // Arrange
        Assert.True(File.Exists(SchemaPath), $"Schema file not found at: {Path.GetFullPath(SchemaPath)}");

        var schemaJson = await File.ReadAllTextAsync(SchemaPath, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("ABL", schemaJson);
    }

    [Fact]
    public async Task Schema_ContainsExpDefinition()
    {
        // Arrange
        Assert.True(File.Exists(SchemaPath), $"Schema file not found at: {Path.GetFullPath(SchemaPath)}");

        var schemaJson = await File.ReadAllTextAsync(SchemaPath, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("EXP", schemaJson);
    }
}
