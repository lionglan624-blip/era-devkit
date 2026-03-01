namespace ErbParser.Ast;

/// <summary>
/// Represents a SELECTCASE...ENDSELECT block
/// Supports SELECTCASE Subject with CASE values, CASEELSE, and nested statements
/// </summary>
public class SelectCaseNode : AstNode
{
    /// <summary>
    /// The expression being switched on (e.g., "ARG", "ARG:1", "PALAM:欲情")
    /// Stored as string to support future non-ARG SELECTCASE without AST redesign
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// List of CASE branches with value matchers and bodies
    /// </summary>
    public List<CaseBranch> Branches { get; } = new();

    /// <summary>
    /// Optional CASEELSE body (fallback when no CASE matches)
    /// Nullable: CASEELSE is optional in ERA
    /// </summary>
    public List<AstNode>? CaseElse { get; set; }
}
