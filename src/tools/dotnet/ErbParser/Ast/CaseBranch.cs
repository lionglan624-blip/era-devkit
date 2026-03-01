namespace ErbParser.Ast;

/// <summary>
/// Represents a single CASE branch in a SELECTCASE block
/// CASE 13,25 → Values = ["13", "25"]
/// </summary>
public class CaseBranch
{
    /// <summary>
    /// Comma-separated values from CASE statement (e.g., ["13", "25"])
    /// Stored as strings to preserve original format and support future string matching
    /// </summary>
    public List<string> Values { get; } = new();

    /// <summary>
    /// Statements inside this CASE branch (PRINTFORML, nested IF, etc.)
    /// </summary>
    public List<AstNode> Body { get; } = new();
}
