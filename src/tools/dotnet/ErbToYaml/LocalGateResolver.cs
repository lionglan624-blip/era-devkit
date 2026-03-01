using ErbParser;
using ErbParser.Ast;

namespace ErbToYaml;

/// <summary>
/// Resolves static LOCAL gates (LOCAL=0 → exclude, LOCAL=1 → strip)
/// Applied before DATALIST-to-YAML conversion
/// Single-pass sequential walk handles LOCAL reassignment patterns (0→1→0→...)
/// </summary>
public class LocalGateResolver : ILocalGateResolver
{
    private readonly LogicalOperatorParser _conditionParser = new();

    public List<AstNode> Resolve(List<AstNode> ast)
    {
        var result = new List<AstNode>();
        var localValues = new Dictionary<string, int?>();

        foreach (var node in ast)
        {
            if (node is AssignmentNode assignment && assignment.Target.StartsWith("LOCAL"))
            {
                // Track LOCAL value
                if (int.TryParse(assignment.Value, out var intValue))
                    localValues[assignment.Target] = intValue;
                else
                    localValues[assignment.Target] = null; // unresolvable (function call)

                // Keep assignment in output (but may be consumed by later processing)
                result.Add(node);
                continue;
            }

            if (node is IfNode ifNode)
            {
                var processed = ProcessIfNode(ifNode, localValues);
                if (processed != null)
                    result.AddRange(processed);
                continue;
            }

            result.Add(node);
        }

        return result;
    }

    /// <summary>
    /// Process an IfNode against known LOCAL values.
    /// Returns null to exclude (dead code), modified nodes for gate stripping,
    /// or original node if unresolved.
    /// </summary>
    private List<AstNode>? ProcessIfNode(IfNode ifNode, Dictionary<string, int?> localValues)
    {
        // Check if condition involves LOCAL
        var condition = ifNode.Condition?.Trim();
        if (string.IsNullOrEmpty(condition))
            return new List<AstNode> { ifNode };

        // Parse condition to check for LOCAL reference
        var parsed = _conditionParser.ParseLogicalExpression(condition);

        // If parsing failed, try manual string-based LOCAL detection for compound conditions
        if (parsed == null && condition.Contains("&&") && condition.Contains("LOCAL"))
        {
            return ProcessStringBasedCompoundGate(ifNode, condition, localValues);
        }

        // Simple LOCAL condition (no compound)
        if (parsed is LocalRef localRef)
        {
            return ProcessSimpleLocalGate(ifNode, localRef, localValues);
        }

        // Compound condition with LOCAL (e.g., LOCAL:1 && TALENT:恋人)
        if (parsed is LogicalOp logicalOp && logicalOp.Operator == "&&")
        {
            return ProcessCompoundLocalGate(ifNode, logicalOp, condition, localValues);
        }

        // If ELSEIF/ELSE exist, preserve unchanged but recurse into body
        if (ifNode.ElseIfBranches.Count > 0 || ifNode.ElseBranch != null)
        {
            return new List<AstNode> { ifNode };
        }

        // Non-LOCAL condition — recurse into body for nested LOCAL gates
        var resolvedBody = Resolve(ifNode.Body);
        if (resolvedBody != ifNode.Body)
        {
            ifNode.Body.Clear();
            ifNode.Body.AddRange(resolvedBody);
        }
        return new List<AstNode> { ifNode };
    }

    private List<AstNode>? ProcessSimpleLocalGate(IfNode ifNode, LocalRef localRef, Dictionary<string, int?> localValues)
    {
        var key = GetLocalKey(localRef);

        if (!localValues.ContainsKey(key))
            return new List<AstNode> { ifNode }; // Unresolved — preserve

        var value = localValues[key];
        if (value == null)
            return new List<AstNode> { ifNode }; // Function-result — preserve

        // Check if LOCAL has explicit operator/value comparison
        if (localRef.Operator != null && localRef.Value != null)
        {
            var conditionMet = EvaluateComparison(value.Value, localRef.Operator, localRef.Value);
            if (!conditionMet)
                return null; // Dead code — exclude

            // Condition met — strip gate, promote body (recurse for nested gates)
            return Resolve(ifNode.Body);
        }

        // Truthiness check (bare LOCAL or LOCAL:N)
        if (value.Value == 0)
            return null; // Dead code — exclude

        // Non-zero (truthy) — strip gate, promote body (recurse for nested gates)
        return Resolve(ifNode.Body);
    }

    private List<AstNode>? ProcessCompoundLocalGate(IfNode ifNode, LogicalOp logicalOp, string originalCondition, Dictionary<string, int?> localValues)
    {
        // Find LOCAL component in compound condition
        LocalRef? localRef = null;
        ICondition? nonLocalPart = null;

        if (logicalOp.Left is LocalRef leftLocal)
        {
            localRef = leftLocal;
            nonLocalPart = logicalOp.Right;
        }
        else if (logicalOp.Right is LocalRef rightLocal)
        {
            localRef = rightLocal;
            nonLocalPart = logicalOp.Left;
        }

        if (localRef == null)
        {
            // No LOCAL in compound — recurse into body
            var resolvedBody = Resolve(ifNode.Body);
            ifNode.Body.Clear();
            ifNode.Body.AddRange(resolvedBody);
            return new List<AstNode> { ifNode };
        }

        var key = GetLocalKey(localRef);

        if (!localValues.ContainsKey(key))
            return new List<AstNode> { ifNode }; // Unresolved

        var value = localValues[key];
        if (value == null)
            return new List<AstNode> { ifNode }; // Unresolvable

        // For && (AND): if LOCAL is false (0), entire condition is false → exclude
        bool localTruthy;
        if (localRef.Operator != null && localRef.Value != null)
            localTruthy = EvaluateComparison(value.Value, localRef.Operator, localRef.Value);
        else
            localTruthy = value.Value != 0;

        if (!localTruthy)
            return null; // Dead code (AND requires all true)

        // LOCAL is truthy — strip LOCAL from compound, keep non-LOCAL part
        if (nonLocalPart == null)
        {
            // All parts were LOCAL — strip entire gate
            return Resolve(ifNode.Body);
        }

        // Reconstruct condition from string by removing LOCAL part
        var newCondition = ExtractNonLocalPart(originalCondition, key);
        if (string.IsNullOrWhiteSpace(newCondition))
        {
            // Fallback: strip entire gate if extraction failed
            return Resolve(ifNode.Body);
        }

        ifNode.Condition = newCondition;

        // Recurse into body for nested LOCAL gates
        var nestedResolved = Resolve(ifNode.Body);
        ifNode.Body.Clear();
        ifNode.Body.AddRange(nestedResolved);

        return new List<AstNode> { ifNode };
    }

    private string ExtractNonLocalPart(string condition, string localKey)
    {
        // Split on && and remove the LOCAL part
        var parts = condition.Split(new[] { "&&" }, StringSplitOptions.None);
        var remainingParts = new List<string>();

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            // Check if this part is the LOCAL we're removing
            if (!IsLocalPart(trimmed, localKey))
            {
                remainingParts.Add(trimmed);
            }
        }

        return string.Join(" && ", remainingParts);
    }

    /// <summary>
    /// Fallback string-based processing for compound gates when LogicalOperatorParser returns null
    /// (e.g., when the non-LOCAL part isn't recognized by the parser)
    /// </summary>
    private List<AstNode>? ProcessStringBasedCompoundGate(IfNode ifNode, string condition, Dictionary<string, int?> localValues)
    {
        // Split on && to find LOCAL part
        var parts = condition.Split(new[] { "&&" }, StringSplitOptions.None);
        string? localPart = null;
        string? localKey = null;

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            // Try to parse this part as LOCAL
            var parsed = _conditionParser.ParseLogicalExpression(trimmed);
            if (parsed is LocalRef localRef)
            {
                localPart = trimmed;
                localKey = GetLocalKey(localRef);
                break;
            }
        }

        if (localKey == null || !localValues.ContainsKey(localKey))
            return new List<AstNode> { ifNode };

        var value = localValues[localKey];
        if (value == null)
            return new List<AstNode> { ifNode };

        // Check truthiness
        bool localTruthy = value.Value != 0;

        if (!localTruthy)
            return null; // Dead code

        // Strip LOCAL from condition
        var remainingParts = new List<string>();
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed != localPart)
            {
                remainingParts.Add(trimmed);
            }
        }

        if (remainingParts.Count == 0)
            return Resolve(ifNode.Body);

        var newCondition = string.Join(" && ", remainingParts);
        ifNode.Condition = newCondition;

        // Recurse into body
        var resolvedBody = Resolve(ifNode.Body);
        ifNode.Body.Clear();
        ifNode.Body.AddRange(resolvedBody);

        return new List<AstNode> { ifNode };
    }

    private bool IsLocalPart(string part, string localKey)
    {
        // Check if this string part matches the LOCAL key we're looking for
        // e.g., "LOCAL:1", "LOCAL", "LOCAL:1 == 1"
        var parsed = _conditionParser.ParseLogicalExpression(part);
        if (parsed is LocalRef localRef)
        {
            var partKey = GetLocalKey(localRef);
            return partKey == localKey;
        }
        return false;
    }

    private string GetLocalKey(LocalRef localRef)
    {
        return localRef.Index == null ? "LOCAL" : $"LOCAL:{localRef.Index}";
    }

    private bool EvaluateComparison(int localValue, string op, string rhsStr)
    {
        if (!int.TryParse(rhsStr.Trim(), out var rhs))
            return false; // Non-integer RHS — cannot evaluate

        return op switch
        {
            "==" => localValue == rhs,
            "!=" => localValue != rhs,
            ">" => localValue > rhs,
            ">=" => localValue >= rhs,
            "<" => localValue < rhs,
            "<=" => localValue <= rhs,
            _ => false
        };
    }
}
