namespace ErbToYaml;

/// <summary>
/// Represents the result of converting a single ERB file to YAML
/// Feature 634 - AC#8
/// </summary>
public record ConversionResult(
    bool Success,
    string FilePath,
    string? Error
);
