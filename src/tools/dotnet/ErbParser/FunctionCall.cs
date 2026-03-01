using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a function call in a condition expression
/// Pattern: FunctionName(arg1, arg2, ...)
/// </summary>
public class FunctionCall : ICondition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("args")]
    public string[] Args { get; set; } = Array.Empty<string>();
}
