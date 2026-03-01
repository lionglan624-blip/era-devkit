namespace ErbParser.Ast;

/// <summary>
/// Extension methods for AST traversal.
/// </summary>
public static class AstExtensions
{
    /// <summary>
    /// Flattens AST by traversing FunctionDefNode.Body recursively.
    /// Returns all nodes at the top level AND inside FunctionDefNode.Body.
    /// This is necessary because Feature 764 introduced FunctionDefNode wrapping
    /// for @-prefixed function definitions.
    /// </summary>
    public static IEnumerable<AstNode> FlattenFunctionBodies(this IEnumerable<AstNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;

            if (node is FunctionDefNode funcDef)
            {
                // Recursively flatten nested function definitions
                foreach (var bodyNode in funcDef.Body.FlattenFunctionBodies())
                {
                    yield return bodyNode;
                }
            }
        }
    }

    /// <summary>
    /// Convenience method to query types from potentially nested AST.
    /// Equivalent to nodes.FlattenFunctionBodies().OfType&lt;T&gt;()
    /// </summary>
    public static IEnumerable<T> OfTypeFlatten<T>(this IEnumerable<AstNode> nodes) where T : AstNode
    {
        return nodes.FlattenFunctionBodies().OfType<T>();
    }
}
