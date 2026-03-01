namespace ErbParser.Ast;

/// <summary>
/// Represents a DATAFORM line within a DATALIST
/// </summary>
public class DataformNode : AstNode
{
    public List<object> Arguments { get; } = new();
}
