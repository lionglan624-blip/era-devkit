namespace ErbToYaml;

/// <summary>
/// Interface for single ERB file conversion orchestration
/// Feature 634 - AC#4
/// </summary>
public interface IFileConverter
{
    /// <summary>
    /// Convert a single ERB file to YAML output(s)
    /// Pipeline: Read ERB → Parse → Extract convertible nodes → Convert → Validate → Write YAML
    /// </summary>
    /// <param name="erbFilePath">Path to input ERB file</param>
    /// <param name="outputDirectory">Directory for output YAML files (pre-computed by BatchConverter)</param>
    /// <returns>List of conversion results (one per generated YAML file)</returns>
    Task<List<ConversionResult>> ConvertAsync(string erbFilePath, string outputDirectory);
}
