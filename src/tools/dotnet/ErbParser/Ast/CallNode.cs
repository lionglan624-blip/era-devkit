namespace ErbParser.Ast;

/// <summary>
/// Represents a CALL or CALLF statement.
/// </summary>
public class CallNode : AstNode
{
    /// <summary>
    /// The function name being called.
    /// </summary>
    public string FunctionName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is CALLF (returns value) vs CALL.
    /// </summary>
    public bool IsCallF { get; set; }
}
