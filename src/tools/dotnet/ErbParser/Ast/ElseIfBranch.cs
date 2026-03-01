namespace ErbParser.Ast;

/// <summary>
/// Represents an ELSEIF branch in an IF block
/// </summary>
public class ElseIfBranch
{
    public string Condition { get; set; } = string.Empty;
    public List<AstNode> Body { get; } = new();
}
