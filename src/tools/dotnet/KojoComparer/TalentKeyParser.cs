using System;

namespace KojoComparer;

/// <summary>
/// Utility for parsing TALENT YAML keys into (target, index) components.
/// Shared by KojoBranchesParser.EvaluateCondition, YamlRunner.ExtractStateFromContext,
/// and StateConverter.ConvertStateToContext.
/// </summary>
internal static class TalentKeyParser
{
    /// <summary>
    /// Parse a TALENT YAML key into (target, index) components.
    /// Supports three formats:
    ///   "16"         → (null, 16)        — backward compatible numeric
    ///   "PLAYER:16"  → ("PLAYER", 16)    — compound target:index
    ///   "PLAYER"     → ("PLAYER", null)  — symbolic target-only
    /// </summary>
    internal static (string? Target, int? Index) ParseTalentYamlKey(string key)
    {
        var colonIdx = key.IndexOf(':');
        if (colonIdx >= 0)
        {
            // Compound key: "TARGET:INDEX"
            var target = key[..colonIdx];
            var indexStr = key[(colonIdx + 1)..];
            if (int.TryParse(indexStr, out var index))
                return (target, index);
            // Malformed compound key (non-numeric after colon)
            Console.Error.WriteLine($"Warning: Malformed compound TALENT key '{key}' — non-numeric index portion '{indexStr}'");
            return (target, null);
        }

        // Single key: numeric or symbolic
        if (int.TryParse(key, out var numericIndex))
            return (null, numericIndex);

        // Non-numeric single key: symbolic target reference (e.g., "PLAYER")
        return (key, null);
    }
}
