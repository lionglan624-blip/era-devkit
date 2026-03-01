namespace ErbToYaml;

/// <summary>
/// Interface for extracting character and situation from ERB file paths
/// Feature 634 - AC#14
/// </summary>
public interface IPathAnalyzer
{
    /// <summary>
    /// Extract character and situation from ERB file path
    /// Pattern: 口上/1_美鈴/KOJO_K1_愛撫.ERB → (character: "美鈴", situation: "K1_愛撫")
    /// </summary>
    /// <param name="erbFilePath">Path to ERB file</param>
    /// <returns>Tuple of (Character, Situation)</returns>
    /// <exception cref="ArgumentException">Thrown when path does not match expected pattern</exception>
    (string Character, string Situation) Extract(string erbFilePath);
}
