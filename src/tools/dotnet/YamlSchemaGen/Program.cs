// YamlSchemaGen - Generate JSON Schema for YAML dialogue files
// Defines schema with common ERA variable types (TALENT, ABL, EXP, FLAG, CFLAG)

using System.Text.Json;
using System.Text.Json.Serialization;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("YamlSchemaGen.Tests")]

namespace YamlSchemaGen;

/// <summary>
/// Generates JSON Schema for YAML dialogue files with common ERA variable types.
/// Phase 1 uses hardcoded definitions for CHARACTER_DATA variables used in TALENT branching.
/// </summary>
internal class Program
{
    private static readonly JsonSerializerOptions s_indentedOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    internal static int Main(string[] args)
    {
        try
        {
            // Define output path relative to project directory
            var outputPath = Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "dialogue-schema.json");

            // Generate schema
            var schema = GenerateDialogueSchema();

            // Serialize with indentation for readability
            var json = JsonSerializer.Serialize(schema, s_indentedOptions);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (outputDir != null && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Write schema to file
            File.WriteAllText(outputPath, json);

            var fullPath = Path.GetFullPath(outputPath);
            Console.WriteLine($"Generated schema: {fullPath}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    /// <summary>
    /// Generates the JSON Schema for YAML dialogue files.
    /// Defines structure for character dialogue with TALENT-based branching.
    /// </summary>
    internal static object GenerateDialogueSchema()
    {
        return new
        {
            schema = "https://json-schema.org/draft-07/schema#",
            title = "ERA Dialogue YAML Schema",
            description = "Schema for character dialogue files with TALENT/ABL/EXP condition branching",
            type = "object",
            properties = new
            {
                character = new
                {
                    type = "string",
                    description = "Character identifier"
                },
                situation = new
                {
                    type = "string",
                    description = "Dialogue situation code (e.g., 'K4', 'K100')"
                },
                branches = new
                {
                    type = "array",
                    description = "Conditional dialogue branches",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            condition = new
                            {
                                type = "object",
                                description = "Condition for this dialogue branch",
                                properties = new
                                {
                                    TALENT = CreateVariableConditionSchema("Character talent flags"),
                                    ABL = CreateVariableConditionSchema("Character ability values"),
                                    EXP = CreateVariableConditionSchema("Character experience values"),
                                    FLAG = CreateVariableConditionSchema("Global flags"),
                                    CFLAG = CreateVariableConditionSchema("Character flags")
                                }
                            },
                            lines = new
                            {
                                type = "array",
                                description = "Dialogue lines for this branch",
                                items = new
                                {
                                    type = "string"
                                }
                            }
                        },
                        required = new[] { "lines" }
                    }
                }
            },
            required = new[] { "character", "situation", "branches" }
        };
    }

    /// <summary>
    /// Creates a schema definition for variable condition objects.
    /// Variables can have conditions like equals, greater than, less than, etc.
    /// Note: Accepts both integer and string types due to YAML serialization behavior.
    /// </summary>
    internal static object CreateVariableConditionSchema(string description)
    {
        // Define integer or string type for values (YAML may serialize as either)
        var integerOrStringType = new[] { "integer", "string" };

        var patternProperties = new Dictionary<string, object>
        {
            ["^[0-9]+$"] = new
            {
                oneOf = new object[]
                {
                    // Direct value (equality check) - can be integer or string
                    new { type = integerOrStringType },
                    // Condition object with operators
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            eq = new { type = integerOrStringType, description = "Equals" },
                            ne = new { type = integerOrStringType, description = "Not equals" },
                            gt = new { type = integerOrStringType, description = "Greater than" },
                            gte = new { type = integerOrStringType, description = "Greater than or equal" },
                            lt = new { type = integerOrStringType, description = "Less than" },
                            lte = new { type = integerOrStringType, description = "Less than or equal" }
                        },
                        additionalProperties = false
                    }
                }
            }
        };

        return new
        {
            type = "object",
            description,
            patternProperties
        };
    }
}
