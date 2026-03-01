using System.Linq;
using ErbParser;

namespace KojoComparer;

/// <summary>
/// Shared utility for converting state dictionary format to context format for YamlRunner.
/// Extracted from BatchProcessor and Program to eliminate duplication.
/// </summary>
public static class StateConverter
{
    /// <summary>
    /// Converts state dictionary to context format for YamlRunner.
    /// </summary>
    /// <param name="state">State dict (e.g., {"TALENT:TARGET:16": 1})</param>
    /// <returns>Context dict (e.g., {"TALENT": {"16": 1}})</returns>
    public static Dictionary<string, object> ConvertStateToContext(Dictionary<string, int> state)
    {
        var context = new Dictionary<string, object>();

        foreach (var kvp in state)
        {
            var parts = kvp.Key.Split(':');
            if (parts.Length < 2)
                continue;

            var type = parts[0];
            string id;
            if (type == "TALENT" && parts.Length >= 3
                && TalentConditionParser.TargetKeywords.Contains(parts[1])
                && parts[1] != "TARGET")
            {
                // Keyword target: preserve as compound key (e.g., "PLAYER:16")
                id = string.Join(":", parts.Skip(1));
            }
            else
            {
                // Default: use last segment (backward compatible for ABL, TFLAG, and TALENT:TARGET:N)
                id = parts[parts.Length - 1];
            }

            if (!context.ContainsKey(type))
            {
                context[type] = new Dictionary<string, int>();
            }

            if (context[type] is Dictionary<string, int> typeDict)
            {
                typeDict[id] = kvp.Value;
            }
        }

        return context;
    }
}
