using ErbParser.Ast;

namespace ErbToYaml;

/// <summary>
/// Interface for PRINTDATA to YAML conversion
/// Feature 634 - AC#13
/// </summary>
public interface IPrintDataConverter
{
    /// <summary>
    /// Convert a PrintDataNode to YAML dialogue format
    /// Extracts DataformNodes using PrintDataNode.GetDataForms() helper
    /// Note: Conditionals wrapping PRINTDATA are handled by FileConverter, not here
    /// </summary>
    /// <param name="printData">PrintData AST node</param>
    /// <param name="character">Character identifier</param>
    /// <param name="situation">Situation code</param>
    /// <returns>YAML string conforming to dialogue-schema.json</returns>
    string Convert(PrintDataNode printData, string character, string situation);
}
