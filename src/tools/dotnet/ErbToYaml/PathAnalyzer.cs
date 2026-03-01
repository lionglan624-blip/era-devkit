using System.Text.RegularExpressions;

namespace ErbToYaml;

/// <summary>
/// Extracts character and situation from ERB file paths
/// Feature 634 - AC#14
/// Pattern: 口上/1_美鈴/KOJO_K1_愛撫.ERB → (character: "美鈴", situation: "K1_愛撫")
/// </summary>
public class PathAnalyzer : IPathAnalyzer
{
    // Pattern to match: N_CharacterName directory and KOJO_Situation.ERB filename
    // Supports both Windows (\) and Unix (/) path separators
    private static readonly Regex PathPattern = new Regex(
        @"(?:^|[\\/])([A-Z\d]+)_([^\\/]+)[\\/]KOJO_(.+)\.(?:ERB|erb)$",
        RegexOptions.Compiled
    );

    // Fallback pattern for non-KOJO files: N_CharacterName/Filename.ERB
    private static readonly Regex FallbackPattern = new Regex(
        @"(?:^|[\\/])([A-Z\d]+)_([^\\/]+)[\\/](.+)\.(?:ERB|erb)$",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Extract character and situation from ERB file path
    /// Pattern: 口上/1_美鈴/KOJO_K1_愛撫.ERB → (character: "美鈴", situation: "K1_愛撫")
    /// Fallback: KOJO_K1_愛撫.ERB → (character: "Unknown", situation: "K1_愛撫")
    /// </summary>
    /// <param name="erbFilePath">Path to ERB file</param>
    /// <returns>Tuple of (Character, Situation)</returns>
    /// <exception cref="ArgumentException">Thrown when path does not match expected pattern</exception>
    public (string Character, string Situation) Extract(string erbFilePath)
    {
        if (string.IsNullOrWhiteSpace(erbFilePath))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(erbFilePath));
        }

        var match = PathPattern.Match(erbFilePath);

        if (match.Success)
        {
            // Extract character name (group 2, after removing number prefix)
            var character = match.Groups[2].Value;

            // Extract situation (group 3, everything after KOJO_ and before .ERB)
            var situation = match.Groups[3].Value;

            return (character, situation);
        }

        // Step 2: Fallback for non-KOJO files (NTR口上_, SexHara, WC系, etc.)
        var fallbackMatch = FallbackPattern.Match(erbFilePath);
        if (fallbackMatch.Success)
        {
            var character = fallbackMatch.Groups[2].Value;  // e.g., "咲夜"
            var filename = fallbackMatch.Groups[3].Value;   // e.g., "NTR口上_シナリオ8"
            return (character, filename);
        }

        throw new ArgumentException(
            $"Path does not match expected pattern (N_CharacterName/KOJO_Situation.ERB): {erbFilePath}",
            nameof(erbFilePath)
        );
    }
}
