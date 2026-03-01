namespace ErbParser.Ast;

/// <summary>
/// Represents an assignment statement (e.g., LOCAL = 0, LOCAL:1 = 1)
/// Enables static analysis of LOCAL values in ErbToYaml preprocessing
/// </summary>
public class AssignmentNode : AstNode
{
    /// <summary>
    /// Left-hand side variable identifier (e.g., "LOCAL", "LOCAL:1")
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Right-hand side value as raw string (e.g., "0", "1", "GET_ABL_BRANCH()")
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
