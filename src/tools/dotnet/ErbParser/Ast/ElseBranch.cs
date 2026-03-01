namespace ErbParser.Ast;

/// <summary>
/// Represents an ELSE branch in an IF block
/// </summary>
public class ElseBranch
{
    public List<AstNode> Body { get; } = new();
}
