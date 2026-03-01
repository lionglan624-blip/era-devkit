namespace ErbParser.Ast;

public class FunctionDefNode : AstNode
{
    public string FunctionName { get; set; } = string.Empty;
    public List<string> Parameters { get; } = new();
    public List<AstNode> Body { get; } = new();
}
