using ErbParser.Ast;

namespace ErbToYaml;

/// <summary>
/// Interface for converting SelectCaseNode AST to YAML dialogue format
/// Feature 765 - Task 3
/// </summary>
public interface ISelectCaseConverter
{
    /// <summary>
    /// Convert SelectCaseNode to YAML dialogue format
    /// Transforms CASE branches to IF-equivalent conditions
    /// </summary>
    /// <param name="selectCase">SelectCaseNode from ErbParser</param>
    /// <param name="character">Character identifier</param>
    /// <param name="situation">Situation code</param>
    /// <returns>YAML string</returns>
    string Convert(SelectCaseNode selectCase, string character, string situation);
}
