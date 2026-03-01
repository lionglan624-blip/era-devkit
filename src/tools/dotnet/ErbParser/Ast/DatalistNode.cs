namespace ErbParser.Ast;

/// <summary>
/// Represents a DATALIST...ENDLIST block
/// </summary>
public class DatalistNode : AstNode
{
    public List<DataformNode> DataForms { get; } = new();

    /// <summary>
    /// Conditional branches (IF/ELSEIF/ELSE) within this DATALIST
    /// Added in F349 Task 2 to support conditional dialogue branches
    /// </summary>
    public List<IfNode> ConditionalBranches { get; } = new();
}
