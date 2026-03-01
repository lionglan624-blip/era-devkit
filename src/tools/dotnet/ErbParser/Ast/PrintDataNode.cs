namespace ErbParser.Ast;

/// <summary>
/// Represents a PRINTDATA...ENDDATA block
/// Contains nested DATALIST blocks, IF conditionals, and standalone DATAFORM/PRINTFORM statements
/// </summary>
public class PrintDataNode : AstNode
{
    /// <summary>
    /// PRINTDATA variant type (PRINTDATA, PRINTDATAL, PRINTDATAW, PRINTDATAK, PRINTDATAKL, PRINTDATAKW, PRINTDATAD, PRINTDATADL, PRINTDATADW)
    /// </summary>
    public string Variant { get; set; } = string.Empty;

    /// <summary>
    /// Nested content within PRINTDATA block (DatalistNode, IfNode, DataformNode, PrintformNode)
    /// </summary>
    public List<AstNode> Content { get; } = new();

    /// <summary>
    /// Extract all DataformNode instances from nested content (recursive traversal)
    /// Used by F634 batch converter for flat dialogue content extraction
    /// </summary>
    /// <returns>Iterator of all DataformNode instances found in Content tree</returns>
    public IEnumerable<DataformNode> GetDataForms()
    {
        return ExtractDataFormsFromNodes(Content);
    }

    /// <summary>
    /// Recursively extracts DataformNode instances from a collection of AST nodes
    /// </summary>
    /// <param name="nodes">Collection of AST nodes to traverse</param>
    /// <returns>Iterator of all DataformNode instances found in the node tree</returns>
    private IEnumerable<DataformNode> ExtractDataFormsFromNodes(IEnumerable<AstNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is DataformNode dataform)
            {
                yield return dataform;
            }
            else if (node is DatalistNode datalist)
            {
                foreach (var df in datalist.DataForms)
                {
                    yield return df;
                }
            }
            else if (node is IfNode ifNode)
            {
                // Recursively extract from IF body
                foreach (var df in ExtractDataFormsFromNodes(ifNode.Body))
                {
                    yield return df;
                }

                // Extract from ELSEIF branches
                foreach (var elseIf in ifNode.ElseIfBranches)
                {
                    foreach (var df in ExtractDataFormsFromNodes(elseIf.Body))
                    {
                        yield return df;
                    }
                }

                // Extract from ELSE branch
                if (ifNode.ElseBranch != null)
                {
                    foreach (var df in ExtractDataFormsFromNodes(ifNode.ElseBranch.Body))
                    {
                        yield return df;
                    }
                }
            }
        }
    }
}
