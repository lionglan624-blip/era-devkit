namespace ErbParser;

/// <summary>
/// Main condition extractor that detects and parses various condition types.
/// Delegates to LogicalOperatorParser which handles all condition types including:
/// - TALENT references (TALENT:恋人)
/// - CFLAG references (CFLAG:MASTER:100)
/// - Function calls (HAS_VAGINA(TARGET))
/// - Logical operators (&&, ||)
/// - Combined complex conditions
/// </summary>
public class ConditionExtractor
{
    private readonly LogicalOperatorParser _parser = new();

    /// <summary>
    /// Extract a condition from a condition string.
    /// Returns appropriate ICondition subtype (TalentRef, CflagRef, FunctionCall, LogicalOp)
    /// or null if the condition cannot be parsed.
    /// </summary>
    /// <param name="condition">Condition string from IF/ELSEIF statement</param>
    /// <returns>Parsed ICondition or null for invalid/empty input</returns>
    public ICondition? Extract(string condition)
    {
        // Delegate to LogicalOperatorParser which handles:
        // - Atomic conditions (TALENT, CFLAG, function)
        // - Logical operators (&& and ||)
        // - Operator precedence and chaining
        return _parser.ParseLogicalExpression(condition);
    }
}
