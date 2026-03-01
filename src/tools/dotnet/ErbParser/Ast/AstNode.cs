namespace ErbParser.Ast;

/// <summary>
/// Base class for all AST nodes
/// </summary>
public abstract class AstNode
{
    public int LineNumber { get; set; }
    public string SourceFile { get; set; } = string.Empty;
}
