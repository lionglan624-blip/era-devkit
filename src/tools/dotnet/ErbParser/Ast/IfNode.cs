namespace ErbParser.Ast;

/// <summary>
/// Represents an IF...ENDIF block
/// </summary>
public class IfNode : AstNode
{
    public string Condition { get; set; } = string.Empty;
    public List<AstNode> Body { get; } = new();
    public List<ElseIfBranch> ElseIfBranches { get; } = new();
    public ElseBranch? ElseBranch { get; set; }
}
