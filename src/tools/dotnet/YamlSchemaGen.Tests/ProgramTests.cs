using System.Text.Json;
using Xunit;

namespace YamlSchemaGen.Tests;

public class ProgramTests
{
    [Fact]
    public void GenerateDialogueSchema_ReturnsNonNull()
    {
        // Act
        var schema = Program.GenerateDialogueSchema();

        // Assert
        Assert.NotNull(schema);
    }

    [Fact]
    public void GenerateDialogueSchema_SerializesToValidJson()
    {
        // Act
        var schema = Program.GenerateDialogueSchema();
        var json = JsonSerializer.Serialize(schema);

        // Assert - Should not throw
        var parsed = JsonDocument.Parse(json);
        Assert.NotNull(parsed);
    }

    [Fact]
    public void GenerateDialogueSchema_ContainsTopLevelProperties()
    {
        // Act
        var schema = Program.GenerateDialogueSchema();
        var json = JsonSerializer.Serialize(schema);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("schema", out _), "Missing 'schema' property");
        Assert.True(root.TryGetProperty("title", out _), "Missing 'title' property");
        Assert.True(root.TryGetProperty("description", out _), "Missing 'description' property");
        Assert.True(root.TryGetProperty("type", out _), "Missing 'type' property");
        Assert.True(root.TryGetProperty("properties", out _), "Missing 'properties' property");
        Assert.True(root.TryGetProperty("required", out _), "Missing 'required' property");
    }

    [Fact]
    public void GenerateDialogueSchema_PropertiesContainsExpectedFields()
    {
        // Act
        var schema = Program.GenerateDialogueSchema();
        var json = JsonSerializer.Serialize(schema);
        var doc = JsonDocument.Parse(json);
        var properties = doc.RootElement.GetProperty("properties");

        // Assert
        Assert.True(properties.TryGetProperty("character", out _), "Missing 'character' property");
        Assert.True(properties.TryGetProperty("situation", out _), "Missing 'situation' property");
        Assert.True(properties.TryGetProperty("branches", out _), "Missing 'branches' property");
    }

    [Fact]
    public void GenerateDialogueSchema_RequiredArrayContainsExpectedFields()
    {
        // Act
        var schema = Program.GenerateDialogueSchema();
        var json = JsonSerializer.Serialize(schema);
        var doc = JsonDocument.Parse(json);
        var required = doc.RootElement.GetProperty("required");

        // Assert
        var requiredList = required.EnumerateArray()
            .Select(e => e.GetString())
            .ToList();

        Assert.Contains("character", requiredList);
        Assert.Contains("situation", requiredList);
        Assert.Contains("branches", requiredList);
    }

    [Fact]
    public void GenerateDialogueSchema_BranchesItemsContainsConditionAndLines()
    {
        // Act
        var schema = Program.GenerateDialogueSchema();
        var json = JsonSerializer.Serialize(schema);
        var doc = JsonDocument.Parse(json);

        var branches = doc.RootElement
            .GetProperty("properties")
            .GetProperty("branches");

        var items = branches.GetProperty("items");
        var itemProperties = items.GetProperty("properties");

        // Assert
        Assert.True(itemProperties.TryGetProperty("condition", out _), "Missing 'condition' property in branch items");
        Assert.True(itemProperties.TryGetProperty("lines", out _), "Missing 'lines' property in branch items");
    }

    [Fact]
    public void GenerateDialogueSchema_ConditionContainsVariableTypes()
    {
        // Act
        var schema = Program.GenerateDialogueSchema();
        var json = JsonSerializer.Serialize(schema);
        var doc = JsonDocument.Parse(json);

        var condition = doc.RootElement
            .GetProperty("properties")
            .GetProperty("branches")
            .GetProperty("items")
            .GetProperty("properties")
            .GetProperty("condition");

        var conditionProperties = condition.GetProperty("properties");

        // Assert
        Assert.True(conditionProperties.TryGetProperty("TALENT", out _), "Missing 'TALENT' property");
        Assert.True(conditionProperties.TryGetProperty("ABL", out _), "Missing 'ABL' property");
        Assert.True(conditionProperties.TryGetProperty("EXP", out _), "Missing 'EXP' property");
        Assert.True(conditionProperties.TryGetProperty("FLAG", out _), "Missing 'FLAG' property");
        Assert.True(conditionProperties.TryGetProperty("CFLAG", out _), "Missing 'CFLAG' property");
    }

    [Fact]
    public void CreateVariableConditionSchema_ReturnsObjectWithTypeProperty()
    {
        // Act
        var schema = Program.CreateVariableConditionSchema("Test description");
        var json = JsonSerializer.Serialize(schema);
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.True(doc.RootElement.TryGetProperty("type", out var typeProperty));
        Assert.Equal("object", typeProperty.GetString());
    }

    [Fact]
    public void CreateVariableConditionSchema_ContainsProvidedDescription()
    {
        // Arrange
        const string testDescription = "Test variable description";

        // Act
        var schema = Program.CreateVariableConditionSchema(testDescription);
        var json = JsonSerializer.Serialize(schema);
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.True(doc.RootElement.TryGetProperty("description", out var descProperty));
        Assert.Equal(testDescription, descProperty.GetString());
    }

    [Fact]
    public void CreateVariableConditionSchema_ContainsPatternProperties()
    {
        // Act
        var schema = Program.CreateVariableConditionSchema("Test description");
        var json = JsonSerializer.Serialize(schema);
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.True(doc.RootElement.TryGetProperty("patternProperties", out var patternProps));

        // Verify the numeric pattern exists
        Assert.True(patternProps.TryGetProperty("^[0-9]+$", out var pattern));
        Assert.True(pattern.TryGetProperty("oneOf", out _));
    }

    [Fact]
    public void CreateVariableConditionSchema_OneOfContainsTwoOptions()
    {
        // Act
        var schema = Program.CreateVariableConditionSchema("Test description");
        var json = JsonSerializer.Serialize(schema);
        var doc = JsonDocument.Parse(json);

        // Assert
        var oneOf = doc.RootElement
            .GetProperty("patternProperties")
            .GetProperty("^[0-9]+$")
            .GetProperty("oneOf");

        var options = oneOf.EnumerateArray().ToList();
        Assert.Equal(2, options.Count);
    }

    [Fact]
    public void CreateVariableConditionSchema_SecondOneOfOptionContainsOperators()
    {
        // Act
        var schema = Program.CreateVariableConditionSchema("Test description");
        var json = JsonSerializer.Serialize(schema);
        var doc = JsonDocument.Parse(json);

        // Assert
        var oneOf = doc.RootElement
            .GetProperty("patternProperties")
            .GetProperty("^[0-9]+$")
            .GetProperty("oneOf");

        var options = oneOf.EnumerateArray().ToList();
        Assert.Equal(2, options.Count);

        // Second option should be the condition object
        var conditionObject = options[1];
        Assert.True(conditionObject.TryGetProperty("properties", out var props));

        // Verify all operators are present
        Assert.True(props.TryGetProperty("eq", out _), "Missing 'eq' operator");
        Assert.True(props.TryGetProperty("ne", out _), "Missing 'ne' operator");
        Assert.True(props.TryGetProperty("gt", out _), "Missing 'gt' operator");
        Assert.True(props.TryGetProperty("gte", out _), "Missing 'gte' operator");
        Assert.True(props.TryGetProperty("lt", out _), "Missing 'lt' operator");
        Assert.True(props.TryGetProperty("lte", out _), "Missing 'lte' operator");
    }

    [Fact]
    public void Main_WithNoArgs_ReturnsZero()
    {
        // Arrange
        var outputPath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "dialogue-schema.json");

        // Clean up any existing file
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        // Act
        var exitCode = Program.Main([]);

        // Assert
        Assert.Equal(0, exitCode);

        // Cleanup
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public void Main_CreatesOutputFile()
    {
        // Arrange
        var outputPath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "dialogue-schema.json");

        // Clean up any existing file
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        // Act
        var exitCode = Program.Main([]);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(outputPath), $"Output file not created at: {Path.GetFullPath(outputPath)}");

        // Cleanup
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public void Main_OutputFileContainsValidJson()
    {
        // Arrange
        var outputPath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "dialogue-schema.json");

        // Clean up any existing file
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        // Act
        var exitCode = Program.Main([]);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(outputPath));

        var jsonContent = File.ReadAllText(outputPath);
        var doc = JsonDocument.Parse(jsonContent); // Should not throw
        Assert.NotNull(doc);

        // Verify it contains expected properties
        Assert.True(doc.RootElement.TryGetProperty("schema", out _));
        Assert.True(doc.RootElement.TryGetProperty("title", out _));

        // Cleanup
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }
    }
}
