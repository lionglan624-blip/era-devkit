namespace ErbParser.Ast;

/// <summary>
/// Represents a PRINTFORM/PRINTFORML/PRINTFORMW command
/// </summary>
public class PrintformNode : AstNode
{
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// PRINTFORM variant (PRINTFORM, PRINTFORML, PRINTFORMW, etc.)
    /// W suffix = no newline (continuation pattern)
    /// </summary>
    public string Variant { get; set; } = "PRINTFORM";
}
