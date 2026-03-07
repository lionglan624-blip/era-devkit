using NJsonSchema;
using Xunit;

namespace YamlValidator.Tests;

public class ProgramTests
{
    #region ParseArguments Tests

    [Fact]
    public void ParseArguments_EmptyArgs_SetsNoOptions()
    {
        // Arrange
        string[] args = [];

        // Act
        var options = Program.ParseArguments(args);

        // Assert
        Assert.False(options.ShowHelp);
        Assert.Null(options.SchemaPath);
        Assert.Null(options.YamlPath);
        Assert.Null(options.ValidateAllPath);
    }

    [Fact]
    public void ParseArguments_WithHelp_SetsShowHelpTrue()
    {
        // Arrange
        string[] args = ["--help"];

        // Act
        var options = Program.ParseArguments(args);

        // Assert
        Assert.True(options.ShowHelp);
    }

    [Fact]
    public void ParseArguments_WithShortHelp_SetsShowHelpTrue()
    {
        // Arrange
        string[] args = ["-h"];

        // Act
        var options = Program.ParseArguments(args);

        // Assert
        Assert.True(options.ShowHelp);
    }

    [Fact]
    public void ParseArguments_WithSchemaPath_SetsSchemaPath()
    {
        // Arrange
        string[] args = ["--schema", "test.json"];

        // Act
        var options = Program.ParseArguments(args);

        // Assert
        Assert.Equal("test.json", options.SchemaPath);
    }

    [Fact]
    public void ParseArguments_WithYamlPath_SetsYamlPath()
    {
        // Arrange
        string[] args = ["--yaml", "test.yaml"];

        // Act
        var options = Program.ParseArguments(args);

        // Assert
        Assert.Equal("test.yaml", options.YamlPath);
    }

    [Fact]
    public void ParseArguments_WithValidateAll_SetsValidateAllPath()
    {
        // Arrange
        string[] args = ["--validate-all", "testdir"];

        // Act
        var options = Program.ParseArguments(args);

        // Assert
        Assert.Equal("testdir", options.ValidateAllPath);
    }

    [Fact]
    public void ParseArguments_WithSchemaAndYaml_SetsBothPaths()
    {
        // Arrange
        string[] args = ["--schema", "schema.json", "--yaml", "file.yaml"];

        // Act
        var options = Program.ParseArguments(args);

        // Assert
        Assert.Equal("schema.json", options.SchemaPath);
        Assert.Equal("file.yaml", options.YamlPath);
    }

    [Fact]
    public void ParseArguments_WithSchemaAndValidateAll_SetsBothPaths()
    {
        // Arrange
        string[] args = ["--schema", "schema.json", "--validate-all", "directory"];

        // Act
        var options = Program.ParseArguments(args);

        // Assert
        Assert.Equal("schema.json", options.SchemaPath);
        Assert.Equal("directory", options.ValidateAllPath);
    }

    [Fact]
    public void ParseArguments_WithUnknownArg_DoesNotThrow()
    {
        // Arrange
        string[] args = ["unknown-arg"];

        // Act & Assert - should not throw, just warning to stderr
        var options = Program.ParseArguments(args);
        Assert.NotNull(options);
    }

    [Fact]
    public void ParseArguments_WithUnknownFlag_DoesNotCrash()
    {
        // Arrange
        string[] args = ["--unknown-flag"];

        // Act & Assert - should not throw
        var options = Program.ParseArguments(args);
        Assert.NotNull(options);
    }

    #endregion

    #region ValidateFile Tests

    [Fact]
    public async Task ValidateFile_ValidYaml_ReturnsZero()
    {
        // Arrange
        var schemaPath = Path.Combine("TestData", "test_schema.json");
        var yamlPath = Path.Combine("TestData", "valid.yaml");
        var schema = await JsonSchema.FromFileAsync(schemaPath, TestContext.Current.CancellationToken);

        // Act
        var result = await Program.ValidateFile(schema, yamlPath);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ValidateFile_InvalidYaml_ReturnsNonZero()
    {
        // Arrange
        var schemaPath = Path.Combine("TestData", "test_schema.json");
        var yamlPath = Path.Combine("TestData", "invalid.yaml");
        var schema = await JsonSchema.FromFileAsync(schemaPath, TestContext.Current.CancellationToken);

        // Act
        var result = await Program.ValidateFile(schema, yamlPath);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ValidateFile_FileNotFound_ReturnsNonZero()
    {
        // Arrange
        var schemaPath = Path.Combine("TestData", "test_schema.json");
        var yamlPath = "nonexistent.yaml";
        var schema = await JsonSchema.FromFileAsync(schemaPath, TestContext.Current.CancellationToken);

        // Act
        var result = await Program.ValidateFile(schema, yamlPath);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ValidateFile_AllKojoYamlFiles_ShouldPass()
    {
        // Arrange
        var schemaPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "YamlSchemaGen", "dialogue-schema.json"));
        var kojoDir = Era.DevKit.TestUtils.GamePathHelper.Resolve("YAML", "Kojo");

        Assert.True(File.Exists(schemaPath), $"Schema not found: {schemaPath}");
        Assert.True(Directory.Exists(kojoDir), $"Kojo directory not found: {kojoDir}");

        var schema = await JsonSchema.FromFileAsync(schemaPath, TestContext.Current.CancellationToken);
        var yamlFiles = Directory.GetFiles(kojoDir, "*.yaml", SearchOption.AllDirectories);

        // Act
        var failures = new System.Collections.Concurrent.ConcurrentBag<string>();
        await Parallel.ForEachAsync(yamlFiles, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, async (yamlFile, ct) =>
        {
            var result = await Program.ValidateFile(schema, yamlFile);
            if (result != 0)
            {
                failures.Add(Path.GetFileName(yamlFile));
            }
        });

        // Assert
        Assert.True(failures.IsEmpty,
            $"{failures.Count}/{yamlFiles.Length} files failed validation:\n{string.Join("\n", failures.Take(20))}");
    }

    #endregion

    #region ConvertToJsonCompatible Tests

    [Fact]
    public void ConvertToJsonCompatible_WithDictionary_ConvertsToDictionaryStringObject()
    {
        // Arrange
        var input = new Dictionary<object, object> { { "key", "value" } };

        // Act
        var result = Program.ConvertToJsonCompatible(input);

        // Assert
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        Assert.Equal("value", dict["key"]);
    }

    [Fact]
    public void ConvertToJsonCompatible_WithNestedDictionary_ConvertsRecursively()
    {
        // Arrange
        var inner = new Dictionary<object, object> { { "nested_key", "nested_value" } };
        var input = new Dictionary<object, object> { { "outer_key", inner } };

        // Act
        var result = Program.ConvertToJsonCompatible(input);

        // Assert
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        var nested = Assert.IsType<Dictionary<string, object?>>(dict["outer_key"]);
        Assert.Equal("nested_value", nested["nested_key"]);
    }

    [Fact]
    public void ConvertToJsonCompatible_WithList_ConvertsList()
    {
        // Arrange
        var input = new List<object> { "item1", "item2", 42 };

        // Act
        var result = Program.ConvertToJsonCompatible(input);

        // Assert
        var list = Assert.IsType<List<object?>>(result);
        Assert.Equal(3, list.Count);
        Assert.Equal("item1", list[0]);
        Assert.Equal("item2", list[1]);
        Assert.Equal(42, list[2]);
    }

    [Fact]
    public void ConvertToJsonCompatible_WithNestedList_ConvertsRecursively()
    {
        // Arrange
        var inner = new List<object> { "nested_item" };
        var input = new List<object> { "outer_item", inner };

        // Act
        var result = Program.ConvertToJsonCompatible(input);

        // Assert
        var list = Assert.IsType<List<object?>>(result);
        Assert.Equal(2, list.Count);
        Assert.Equal("outer_item", list[0]);
        var nested = Assert.IsType<List<object?>>(list[1]);
        Assert.Single(nested);
        Assert.Equal("nested_item", nested[0]);
    }

    [Fact]
    public void ConvertToJsonCompatible_WithString_ReturnsString()
    {
        // Arrange
        var input = "test_string";

        // Act
        var result = Program.ConvertToJsonCompatible(input);

        // Assert
        Assert.Equal("test_string", result);
    }

    [Fact]
    public void ConvertToJsonCompatible_WithInteger_ReturnsInteger()
    {
        // Arrange
        var input = 42;

        // Act
        var result = Program.ConvertToJsonCompatible(input);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void ConvertToJsonCompatible_WithNull_ReturnsNull()
    {
        // Arrange
        object? input = null;

        // Act
        var result = Program.ConvertToJsonCompatible(input);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ConvertToJsonCompatible_WithComplexStructure_ConvertsCorrectly()
    {
        // Arrange
        var input = new Dictionary<object, object>
        {
            { "name", "test" },
            { "age", 30 },
            { "tags", new List<object> { "tag1", "tag2" } },
            { "metadata", new Dictionary<object, object> { { "key", "value" } } }
        };

        // Act
        var result = Program.ConvertToJsonCompatible(input);

        // Assert
        var dict = Assert.IsType<Dictionary<string, object?>>(result);
        Assert.Equal("test", dict["name"]);
        Assert.Equal(30, dict["age"]);
        var tags = Assert.IsType<List<object?>>(dict["tags"]);
        Assert.Equal(2, tags.Count);
        var metadata = Assert.IsType<Dictionary<string, object?>>(dict["metadata"]);
        Assert.Equal("value", metadata["key"]);
    }

    #endregion

    #region ValidateDirectory Tests

    [Fact]
    public async Task ValidateDirectory_DirectoryNotFound_ReturnsOne()
    {
        // Arrange
        var schema = await JsonSchema.FromJsonAsync(@"{
            ""type"": ""object"",
            ""properties"": { ""name"": { ""type"": ""string"" } },
            ""required"": [""name""]
        }", TestContext.Current.CancellationToken);
        var nonExistentDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        // Act
        var result = await Program.ValidateDirectory(schema, nonExistentDir);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ValidateDirectory_EmptyDirectory_ReturnsZero()
    {
        // Arrange
        var schema = await JsonSchema.FromJsonAsync(@"{
            ""type"": ""object"",
            ""properties"": { ""name"": { ""type"": ""string"" } },
            ""required"": [""name""]
        }", TestContext.Current.CancellationToken);
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(tempDir);

            // Act
            var result = await Program.ValidateDirectory(schema, tempDir);

            // Assert
            Assert.Equal(0, result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task ValidateDirectory_WithValidFiles_ReturnsZero()
    {
        // Arrange
        var schema = await JsonSchema.FromJsonAsync(@"{
            ""type"": ""object"",
            ""properties"": { ""name"": { ""type"": ""string"" } },
            ""required"": [""name""]
        }", TestContext.Current.CancellationToken);
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(tempDir);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "valid1.yaml"), "name: test1", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "valid2.yaml"), "name: test2", TestContext.Current.CancellationToken);

            // Act
            var result = await Program.ValidateDirectory(schema, tempDir);

            // Assert
            Assert.Equal(0, result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task ValidateDirectory_WithMixedValidInvalidFiles_ReturnsOne()
    {
        // Arrange
        var schema = await JsonSchema.FromJsonAsync(@"{
            ""type"": ""object"",
            ""properties"": { ""name"": { ""type"": ""string"" } },
            ""required"": [""name""]
        }", TestContext.Current.CancellationToken);
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(tempDir);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "valid.yaml"), "name: test", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "invalid.yaml"), "age: 30", TestContext.Current.CancellationToken);  // missing required 'name'

            // Act
            var result = await Program.ValidateDirectory(schema, tempDir);

            // Assert
            Assert.Equal(1, result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task ValidateDirectory_WithSubdirectories_ValidatesRecursively()
    {
        // Arrange
        var schema = await JsonSchema.FromJsonAsync(@"{
            ""type"": ""object"",
            ""properties"": { ""name"": { ""type"": ""string"" } },
            ""required"": [""name""]
        }", TestContext.Current.CancellationToken);
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(tempDir);
            var subDir = Path.Combine(tempDir, "subdir");
            Directory.CreateDirectory(subDir);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "root.yaml"), "name: root", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(subDir, "nested.yaml"), "name: nested", TestContext.Current.CancellationToken);

            // Act
            var result = await Program.ValidateDirectory(schema, tempDir);

            // Assert
            Assert.Equal(0, result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    #endregion

    #region ValidateFile Edge Cases

    [Fact]
    public async Task ValidateFile_MalformedYaml_ReturnsOne()
    {
        // Arrange
        var schema = await JsonSchema.FromJsonAsync(@"{
            ""type"": ""object"",
            ""properties"": { ""name"": { ""type"": ""string"" } },
            ""required"": [""name""]
        }", TestContext.Current.CancellationToken);
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".yaml");

        try
        {
            await File.WriteAllTextAsync(tempFile, "  invalid:\n  - [unclosed", TestContext.Current.CancellationToken);

            // Act
            var result = await Program.ValidateFile(schema, tempFile);

            // Assert
            Assert.Equal(1, result);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ValidateFile_ValidYamlButFailsSchema_ReturnsOne()
    {
        // Arrange
        var schema = await JsonSchema.FromJsonAsync(@"{
            ""type"": ""object"",
            ""properties"": {
                ""name"": { ""type"": ""string"" },
                ""age"": { ""type"": ""integer"" }
            },
            ""required"": [""name"", ""age""]
        }", TestContext.Current.CancellationToken);
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".yaml");

        try
        {
            await File.WriteAllTextAsync(tempFile, "name: John", TestContext.Current.CancellationToken);  // missing required 'age'

            // Act
            var result = await Program.ValidateFile(schema, tempFile);

            // Assert
            Assert.Equal(1, result);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ValidateFile_YamlWithWrongTypeForSchema_ReturnsOne()
    {
        // Arrange
        var schema = await JsonSchema.FromJsonAsync(@"{
            ""type"": ""object"",
            ""properties"": {
                ""name"": { ""type"": ""string"" },
                ""age"": { ""type"": ""integer"" }
            },
            ""required"": [""name"", ""age""]
        }", TestContext.Current.CancellationToken);
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".yaml");

        try
        {
            await File.WriteAllTextAsync(tempFile, "name: John\nage: \"not a number\"", TestContext.Current.CancellationToken);

            // Act
            var result = await Program.ValidateFile(schema, tempFile);

            // Assert
            Assert.Equal(1, result);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    #endregion

    #region ParseArguments Edge Cases

    [Fact]
    public void ParseArguments_SchemaAtEndWithoutValue_DoesNotSetSchemaPath()
    {
        // Arrange
        string[] args = ["--yaml", "test.yaml", "--schema"];

        // Act
        var options = Program.ParseArguments(args);

        // Assert
        Assert.Equal("test.yaml", options.YamlPath);
        Assert.Null(options.SchemaPath);  // No value after --schema
    }

    [Fact]
    public void ParseArguments_YamlAtEndWithoutValue_DoesNotSetYamlPath()
    {
        // Arrange
        string[] args = ["--schema", "test.json", "--yaml"];

        // Act
        var options = Program.ParseArguments(args);

        // Assert
        Assert.Equal("test.json", options.SchemaPath);
        Assert.Null(options.YamlPath);  // No value after --yaml
    }

    [Fact]
    public void ParseArguments_ValidateAllAtEndWithoutValue_DoesNotSetValidateAllPath()
    {
        // Arrange
        string[] args = ["--schema", "test.json", "--validate-all"];

        // Act
        var options = Program.ParseArguments(args);

        // Assert
        Assert.Equal("test.json", options.SchemaPath);
        Assert.Null(options.ValidateAllPath);  // No value after --validate-all
    }

    #endregion
}
