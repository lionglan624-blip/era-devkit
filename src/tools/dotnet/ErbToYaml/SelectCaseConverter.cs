using ErbParser;
using ErbParser.Ast;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ErbToYaml;

/// <summary>
/// Converts SELECTCASE blocks from ERB AST to YAML dialogue files
/// Transforms CASE branches to IF-equivalent conditions with OR logic for multi-value CASE
/// Feature 765 - Task 3
/// </summary>
public class SelectCaseConverter : ISelectCaseConverter
{
    private readonly IConditionSerializer _conditionSerializer;
    private readonly ConditionExtractor _conditionExtractor;

    public SelectCaseConverter(IConditionSerializer conditionSerializer, ConditionExtractor conditionExtractor)
    {
        _conditionSerializer = conditionSerializer ?? throw new ArgumentNullException(nameof(conditionSerializer));
        _conditionExtractor = conditionExtractor ?? throw new ArgumentNullException(nameof(conditionExtractor));
    }

    /// <summary>
    /// Convert SelectCaseNode to YAML dialogue format
    /// </summary>
    public string Convert(SelectCaseNode selectCase, string character, string situation)
    {
        if (selectCase == null)
            throw new ArgumentNullException(nameof(selectCase));

        var branches = new List<object>();

        // Transform CASE branches
        foreach (var caseBranch in selectCase.Branches)
        {
            // Build OR condition from values
            ICondition? condition = null;
            foreach (var value in caseBranch.Values)
            {
                var argRef = new ArgRef { Index = 0, Operator = "==", Value = value };
                if (condition == null)
                    condition = argRef;
                else
                    condition = new LogicalOp { Left = condition, Operator = "||", Right = argRef };
            }

            // Extract lines from body (PRINTFORML nodes)
            var lines = ExtractLinesFromBody(caseBranch.Body);

            // Create branch dict
            var branch = new Dictionary<string, object> { { "lines", lines } };
            if (condition != null)
            {
                var conditionYaml = _conditionSerializer.ConvertConditionToYaml(condition);
                if (conditionYaml != null)
                {
                    branch["condition"] = conditionYaml;
                }
            }
            branches.Add(branch);
        }

        // Transform CASEELSE (if present)
        if (selectCase.CaseElse != null)
        {
            var nestedIfs = selectCase.CaseElse.OfType<IfNode>().ToList();
            if (nestedIfs.Any())
            {
                // Recursive sub-branch conversion (reuse ConvertConditionalBranches pattern)
                var subBranches = ConvertConditionalBranches(nestedIfs);
                branches.AddRange(subBranches);
            }
            else
            {
                // Simple CASEELSE (no nested IF)
                var lines = ExtractLinesFromBody(selectCase.CaseElse);
                if (lines.Count > 0)
                {
                    var elseBranch = new Dictionary<string, object> { { "lines", lines } };
                    branches.Add(elseBranch);
                }
            }
        }

        // Convert to entries format
        var entries = BranchesToEntriesConverter.Convert(branches);

        // Serialize to YAML
        var dialogueData = new Dictionary<string, object>
        {
            { "character", character },
            { "situation", situation },
            { "entries", entries }
        };
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return serializer.Serialize(dialogueData);
    }

    /// <summary>
    /// Extract lines from PrintformNode.Content in body
    /// </summary>
    private List<string> ExtractLinesFromBody(List<AstNode> body)
    {
        var lines = new List<string>();
        foreach (var node in body)
        {
            if (node is PrintformNode printform)
            {
                lines.Add(printform.Content);
            }
        }
        return lines;
    }

    /// <summary>
    /// Convert conditional branches (IF/ELSEIF/ELSE) from CASEELSE body
    /// Adapted from DatalistConverter.ConvertConditionalBranches pattern
    /// Uses PrintformNode.Content instead of DataformNode.Arguments
    /// </summary>
    private List<object> ConvertConditionalBranches(List<IfNode> ifNodes)
    {
        var branches = new List<object>();

        foreach (var ifNode in ifNodes)
        {
            // IF branch
            var ifBranch = ProcessPrintformBranch(ifNode.Condition, ifNode.Body);
            if (ifBranch != null)
            {
                branches.Add(ifBranch);
            }

            // ELSEIF branches
            foreach (var elseIfBranch in ifNode.ElseIfBranches)
            {
                var branch = ProcessPrintformBranch(elseIfBranch.Condition, elseIfBranch.Body);
                if (branch != null)
                {
                    branches.Add(branch);
                }
            }

            // ELSE branch (no condition)
            if (ifNode.ElseBranch != null)
            {
                var elseBranch = ProcessPrintformBranch(null, ifNode.ElseBranch.Body);
                if (elseBranch != null)
                {
                    branches.Add(elseBranch);
                }
            }
        }

        return branches;
    }

    /// <summary>
    /// Process a single branch with condition and body
    /// Extracts lines from PrintformNode.Content
    /// </summary>
    private Dictionary<string, object>? ProcessPrintformBranch(string? condition, List<AstNode> body)
    {
        var lines = ExtractLinesFromBody(body);
        if (lines.Count == 0)
            return null;

        var branch = new Dictionary<string, object> { { "lines", lines } };

        // Parse and add condition if present
        if (!string.IsNullOrWhiteSpace(condition))
        {
            var parsedCondition = _conditionExtractor.Extract(condition);
            if (parsedCondition != null)
            {
                var conditionYaml = _conditionSerializer.ConvertConditionToYaml(parsedCondition);
                if (conditionYaml != null)
                {
                    branch["condition"] = conditionYaml;
                }
            }
        }

        return branch;
    }
}
