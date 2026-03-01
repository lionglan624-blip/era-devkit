using ErbParser;

namespace ErbToYaml;

/// <summary>
/// Shared condition serialization utility extracted from DatalistConverter
/// Handles ICondition → YAML dict conversion for all variable types
/// Both DatalistConverter and SelectCaseConverter depend on IConditionSerializer
/// Feature 765 - Task 3
/// </summary>
public class ConditionSerializer : IConditionSerializer
{
    private readonly TalentCsvLoader _talentLoader;
    private readonly IDimConstResolver? _dimConstResolver;
    private readonly Dictionary<Type, string> _variableTypePrefixes;

    public ConditionSerializer(
        TalentCsvLoader talentLoader,
        IDimConstResolver? dimConstResolver,
        Dictionary<Type, string> variableTypePrefixes)
    {
        _talentLoader = talentLoader ?? throw new ArgumentNullException(nameof(talentLoader));
        _variableTypePrefixes = variableTypePrefixes ?? throw new ArgumentNullException(nameof(variableTypePrefixes));
        _dimConstResolver = dimConstResolver;
    }

    /// <summary>
    /// Convert ICondition to YAML dictionary format
    /// F755 AC#4,5,6,7,13: Handles TALENT, CFLAG, TCVAR, negation, and logical operations
    /// </summary>
    public Dictionary<string, object>? ConvertConditionToYaml(ICondition condition)
    {
        switch (condition)
        {
            case TalentRef talent:
                return ConvertTalentRef(talent);
            case VariableRef varRef when _variableTypePrefixes.ContainsKey(varRef.GetType()):
                return ConvertVariableRef(varRef);
            case ArgRef argRef:
                return ConvertArgRef(argRef);
            case NegatedCondition negated:
                return ConvertNegatedCondition(negated);
            case LogicalOp logical:
                return ConvertLogicalOp(logical);
            case FunctionCall function:
                return new Dictionary<string, object>
                {
                    { "FUNCTION", new Dictionary<string, object>
                        {
                            { "name", function.Name },
                            { "args", function.Args }
                        }
                    }
                };
            case BitwiseComparisonCondition bitwiseComp:
                return ConvertBitwiseComparisonCondition(bitwiseComp);
            default:
                return null;
        }
    }

    /// <summary>
    /// Map ERB comparison operators to YAML operator format
    /// Shared by TALENT, CFLAG, and TCVAR conversion
    /// </summary>
    public Dictionary<string, object> MapErbOperatorToYaml(string? erbOperator, string? value)
    {
        string op = erbOperator ?? "!=";
        string val = value ?? "0";

        // Apply DIM CONST resolution if available
        if (_dimConstResolver != null)
            val = _dimConstResolver.ResolveToString(val);

        var operatorValue = new Dictionary<string, object>();
        switch (op)
        {
            case "==":
                operatorValue["eq"] = val;
                break;
            case "!=":
                operatorValue["ne"] = val;
                break;
            case ">":
                operatorValue["gt"] = val;
                break;
            case ">=":
                operatorValue["gte"] = val;
                break;
            case "<":
                operatorValue["lt"] = val;
                break;
            case "<=":
                operatorValue["lte"] = val;
                break;
            case "&":
                operatorValue["bitwise_and"] = val;
                break;
            default:
                operatorValue["ne"] = "0";
                break;
        }
        return operatorValue;
    }

    /// <summary>
    /// Convert ArgRef to YAML format
    /// Pattern: { "ARG": { "0": { "eq": "2" } } }
    /// </summary>
    private Dictionary<string, object> ConvertArgRef(ArgRef argRef)
    {
        var key = argRef.Index.ToString();
        return BuildConditionDict("ARG", key, argRef.Operator, argRef.Value);
    }

    /// <summary>
    /// Convert TalentRef to YAML format with target-aware key encoding
    /// </summary>
    private Dictionary<string, object> ConvertTalentRef(TalentRef talent)
    {
        var yamlKey = ResolveTalentKey(talent);
        if (yamlKey == null)
            return new Dictionary<string, object>();

        return BuildConditionDict("TALENT", yamlKey, talent.Operator, talent.Value);
    }

    /// <summary>
    /// Resolve TalentRef to YAML key string. Shared by ConvertTalentRef and
    /// ResolveInnerBitwiseRef to eliminate key resolution duplication.
    /// Returns null if resolution fails.
    /// </summary>
    private string? ResolveTalentKey(TalentRef talent)
    {
        string? talentKey = null;

        // Step 1: Resolve the index/name portion
        if (!string.IsNullOrEmpty(talent.Name))
        {
            var talentIndex = _talentLoader.GetTalentIndex(talent.Name);
            if (talentIndex == null)
            {
                Console.Error.WriteLine($"Warning: Talent '{talent.Name}' not found in Talent.csv");
                return null;
            }
            talentKey = talentIndex.Value.ToString();
        }
        else if (talent.Index.HasValue)
        {
            talentKey = talent.Index.Value.ToString();
        }

        // Step 2: Build YAML key with target encoding
        var isKeywordTarget = !string.IsNullOrEmpty(talent.Target)
            && TalentConditionParser.TargetKeywords.Contains(talent.Target);

        if (isKeywordTarget && talentKey != null)
            return $"{talent.Target}:{talentKey}";
        if (talentKey != null)
            return talentKey;
        if (isKeywordTarget)
            return talent.Target;

        Console.Error.WriteLine("Warning: TalentRef with no Name, Index, or Target");
        return null;
    }

    /// <summary>
    /// Build a standard condition dictionary { prefix: { key: { op: value } } }.
    /// Shared by ConvertTalentRef and ConvertVariableRef to reduce duplication.
    /// </summary>
    private Dictionary<string, object> BuildConditionDict(
        string prefix, string key, string? op, string? value)
    {
        return new Dictionary<string, object>
        {
            { prefix, new Dictionary<string, object>
                {
                    { key, MapErbOperatorToYaml(op, value) }
                }
            }
        };
    }

    /// <summary>
    /// Build variable key from VariableRef (shared helper)
    /// </summary>
    private static string BuildVariableKey(VariableRef varRef)
    {
        if (varRef.Index.HasValue)
            return varRef.Index.Value.ToString();
        return varRef.Target != null ? $"{varRef.Target}:{varRef.Name}" : varRef.Name!;
    }

    /// <summary>
    /// Convert any VariableRef to YAML format using prefix dictionary lookup.
    /// TcvarRef special case (C4): ignores Target field in key construction.
    /// </summary>
    private Dictionary<string, object> ConvertVariableRef(VariableRef varRef)
    {
        string key;
        if (varRef is TcvarRef tcvar)
        {
            key = tcvar.Index.HasValue ? tcvar.Index.Value.ToString() : tcvar.Name!;
        }
        else
        {
            key = BuildVariableKey(varRef);
        }

        var prefix = _variableTypePrefixes[varRef.GetType()];

        return BuildConditionDict(prefix, key, varRef.Operator, varRef.Value);
    }

    /// <summary>
    /// Convert NegatedCondition to YAML NOT format
    /// Pattern: { "NOT": { inner_condition } }
    /// </summary>
    private Dictionary<string, object>? ConvertNegatedCondition(NegatedCondition negated)
    {
        var innerYaml = ConvertConditionToYaml(negated.Inner);
        if (innerYaml == null)
            return null;

        return new Dictionary<string, object>
        {
            { "NOT", innerYaml }
        };
    }

    /// <summary>
    /// Convert LogicalOp to YAML AND/OR format with tree flattening
    /// Pattern: { "AND": [cond1, cond2, cond3] }
    /// Flattens nested operations with same operator (A && B && C → [A, B, C] not [[A, B], C])
    /// F755 AC#13: Tree flattening optimization
    /// </summary>
    private Dictionary<string, object>? ConvertLogicalOp(LogicalOp logical)
    {
        var flattenedItems = FlattenLogicalOp(logical, logical.Operator).ToList();
        var yamlItems = flattenedItems.Select(ConvertConditionToYaml).ToList();

        if (yamlItems.Any(y => y == null))
            return null;

        string yamlOperator = logical.Operator switch
        {
            "&&" => "AND",
            "||" => "OR",
            _ => throw new ArgumentException($"Unknown logical operator: {logical.Operator}")
        };

        // Cast to List<object> for YAML serialization
        return new Dictionary<string, object>
        {
            { yamlOperator, yamlItems.Cast<object>().ToList() }
        };
    }

    /// <summary>
    /// Flatten nested LogicalOp nodes with the same operator
    /// Example: (A && B) && C → [A, B, C] instead of nested structure
    /// </summary>
    private IEnumerable<ICondition> FlattenLogicalOp(ICondition condition, string targetOperator)
    {
        if (condition is LogicalOp logical && logical.Operator == targetOperator)
        {
            foreach (var item in FlattenLogicalOp(logical.Left!, targetOperator))
                yield return item;
            foreach (var item in FlattenLogicalOp(logical.Right!, targetOperator))
                yield return item;
        }
        else
        {
            yield return condition;
        }
    }

    /// <summary>
    /// Normalize ERB comparison operator to YAML format.
    /// F759: Shared helper for compound bitwise-comparison conversion.
    /// </summary>
    private static string NormalizeErbOperator(string erbOp) => erbOp switch
    {
        "==" => "eq",
        "!=" => "ne",
        ">" => "gt",
        ">=" => "gte",
        "<" => "lt",
        "<=" => "lte",
        _ => throw new ArgumentException($"Unknown ERB operator: {erbOp}")
    };

    /// <summary>
    /// Convert BitwiseComparisonCondition to YAML format with bitwise_and_cmp operator.
    /// F759: Inner must have Operator="&amp;" (validated at parse time by HasBitwiseOperator)
    /// </summary>
    private Dictionary<string, object>? ConvertBitwiseComparisonCondition(BitwiseComparisonCondition bitwiseComp)
    {
        var (variableType, variableKey, mask) = ResolveInnerBitwiseRef(bitwiseComp.Inner);
        if (variableType == null)
            return null;

        var normalizedOp = NormalizeErbOperator(bitwiseComp.ComparisonOp);

        var resolvedMask = _dimConstResolver?.ResolveToString(mask) ?? mask;
        var resolvedValue = _dimConstResolver?.ResolveToString(bitwiseComp.ComparisonValue)
                            ?? bitwiseComp.ComparisonValue;

        return new Dictionary<string, object>
        {
            { variableType, new Dictionary<string, object>
                {
                    { variableKey, new Dictionary<string, object>
                        {
                            { "bitwise_and_cmp", new Dictionary<string, object>
                                {
                                    { "mask", resolvedMask },
                                    { "op", normalizedOp },
                                    { "value", resolvedValue }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Extract variable type, key, and mask from inner bitwise condition.
    /// F759: Reuses existing key resolution (TalentCsvLoader, BuildVariableKey).
    /// </summary>
    private (string? variableType, string variableKey, string mask) ResolveInnerBitwiseRef(ICondition inner)
    {
        switch (inner)
        {
            case TalentRef talent when talent.Operator == "&":
                var bitwiseKey = ResolveTalentKey(talent);
                if (bitwiseKey == null)
                    return (null, "", "");
                return ("TALENT", bitwiseKey, talent.Value ?? "0");

            case VariableRef varRef when varRef.Operator == "&"
                                      && _variableTypePrefixes.ContainsKey(varRef.GetType()):
                return (_variableTypePrefixes[varRef.GetType()],
                        BuildVariableKey(varRef),
                        varRef.Value ?? "0");

            default:
                Console.Error.WriteLine($"Warning: Unsupported inner condition type for compound bitwise: {inner.GetType().Name}");
                return (null, "", "");
        }
    }
}
