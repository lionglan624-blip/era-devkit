using ErbParser;

namespace ErbToYaml;

/// <summary>
/// Interface for converting ICondition AST nodes to YAML dictionary format
/// Extracted from DatalistConverter to enable reuse by SelectCaseConverter
/// Feature 765 - Task 3
/// </summary>
public interface IConditionSerializer
{
    /// <summary>
    /// Convert ICondition AST node to YAML dictionary format
    /// Returns null if condition cannot be converted
    /// </summary>
    Dictionary<string, object>? ConvertConditionToYaml(ICondition condition);

    /// <summary>
    /// Map ERB comparison operator to YAML operator format
    /// Example: ("==", "13") → { "eq": "13" }
    /// </summary>
    Dictionary<string, object> MapErbOperatorToYaml(string? erbOperator, string? value);
}
