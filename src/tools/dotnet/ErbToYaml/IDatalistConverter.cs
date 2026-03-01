using ErbParser.Ast;

namespace ErbToYaml;

/// <summary>
/// Interface for DATALIST to YAML conversion
/// Feature 634 - testability interface for existing DatalistConverter
/// </summary>
public interface IDatalistConverter
{
    /// <summary>
    /// Convert a DATALIST node to YAML dialogue format
    /// </summary>
    /// <param name="datalist">DATALIST AST node</param>
    /// <param name="character">Character identifier</param>
    /// <param name="situation">Situation code (e.g., K4, K100)</param>
    /// <returns>YAML string conforming to dialogue-schema.json</returns>
    string Convert(DatalistNode datalist, string character, string situation);

    /// <summary>
    /// Validate YAML content against loaded schema
    /// Throws SchemaValidationException on failure
    /// No-op if constructor used without schemaPath
    /// </summary>
    /// <param name="yaml">YAML content to validate</param>
    void ValidateYaml(string yaml);

    /// <summary>
    /// Parse condition string and convert to dialogue-schema.json format
    /// Feature 649 - eliminate FileConverter condition parsing duplication
    /// </summary>
    /// <param name="condition">Condition string (e.g., "TALENT:恋慕")</param>
    /// <returns>Condition object matching dialogue-schema.json structure, or empty dict if parsing fails</returns>
    Dictionary<string, object>? ParseCondition(string condition);
}
