using System.Text;
using System.Text.RegularExpressions;
using ErbParser;
using ErbParser.Ast;

namespace KojoComparer;

/// <summary>
/// Lightweight in-process ERB evaluator that walks the ErbParser AST to execute kojo functions.
/// Replaces subprocess-based ErbRunner for 100x+ speed improvement.
/// </summary>
public class ErbEvaluator : IErbRunner
{
    private readonly string _gamePath;
    private readonly Dictionary<string, int> _talentNameToIndex = new();
    private readonly Dictionary<string, int> _ablNameToIndex = new();
    private readonly Dictionary<string, int> _cflagNameToIndex = new();
    private readonly Dictionary<string, int> _characterConstants = new();
    private readonly ConditionExtractor _conditionExtractor = new();

    public ErbEvaluator(string gamePath)
    {
        _gamePath = gamePath;
        LoadCsvMappings();
        LoadCharacterConstants();
    }

    /// <summary>
    /// Loads CSV name→index mappings for TALENT, ABL, CFLAG, etc.
    /// Supplements TALENT.CSV with Talent.yaml for complete definitions (e.g., 恋人=16, 思慕=17).
    /// </summary>
    private void LoadCsvMappings()
    {
        // TALENT.CSV format: index,name,;comment
        LoadCsvMapping(Path.Combine(_gamePath, "CSV", "TALENT.csv"), _talentNameToIndex);
        LoadCsvMapping(Path.Combine(_gamePath, "CSV", "ABL.csv"), _ablNameToIndex);
        LoadCsvMapping(Path.Combine(_gamePath, "CSV", "CFLAG.csv"), _cflagNameToIndex);

        // Supplement with Talent.yaml (has complete definitions including 恋人=16, 思慕=17)
        var talentYamlPath = Path.Combine(_gamePath, "data", "Talent.yaml");
        if (File.Exists(talentYamlPath))
        {
            LoadTalentYaml(talentYamlPath);
        }
    }

    private void LoadCsvMapping(string csvPath, Dictionary<string, int> mapping)
    {
        if (!File.Exists(csvPath))
            return;

        var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith(";"))
                continue;

            // Format: index,name,;comment
            // Split by comma and extract first two fields
            var parts = line.Split(',');
            if (parts.Length >= 2 && int.TryParse(parts[0].Trim(), out var index))
            {
                var name = parts[1].Trim();
                // Remove inline comments (;...)
                var commentIndex = name.IndexOf(';');
                if (commentIndex >= 0)
                    name = name.Substring(0, commentIndex).Trim();

                if (!string.IsNullOrEmpty(name))
                    mapping[name] = index;
            }
        }
    }

    /// <summary>
    /// Loads supplementary TALENT definitions from Talent.yaml.
    /// Reads YAML using simple line-by-line parsing (no YAML library dependency).
    /// CSV takes precedence; only adds entries not already in _talentNameToIndex.
    /// </summary>
    private void LoadTalentYaml(string yamlPath)
    {
        var lines = File.ReadAllLines(yamlPath, Encoding.UTF8);
        int? currentIndex = null;

        foreach (var line in lines)
        {
            // Match: "  - index: 16"
            var indexMatch = Regex.Match(line, @"^\s*-?\s*index:\s*(\d+)");
            if (indexMatch.Success)
            {
                currentIndex = int.Parse(indexMatch.Groups[1].Value);
                continue;
            }

            // Match: "    name: \"恋人\""
            var nameMatch = Regex.Match(line, @"^\s*name:\s*[""'](.+?)[""']");
            if (nameMatch.Success && currentIndex.HasValue)
            {
                var name = nameMatch.Groups[1].Value;
                // CSV takes precedence - only add if not already present
                if (!_talentNameToIndex.ContainsKey(name))
                {
                    _talentNameToIndex[name] = currentIndex.Value;
                }
                currentIndex = null; // Reset for next entry
            }
        }
    }

    /// <summary>
    /// Loads character constants from DIM.ERH (e.g., 人物_レミリア = 5).
    /// These constants are used in assignment expressions and conditions.
    /// </summary>
    private void LoadCharacterConstants()
    {
        var dimPath = Path.Combine(_gamePath, "ERB", "DIM.ERH");
        if (!File.Exists(dimPath))
            return;

        var lines = File.ReadAllLines(dimPath, Encoding.UTF8);
        foreach (var line in lines)
        {
            // Match: #DIM CONST 人物_レミリア = 5
            var match = Regex.Match(line, @"^#DIM\s+CONST\s+(人物_\S+)\s*=\s*(\d+)");
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                var value = int.Parse(match.Groups[2].Value);
                _characterConstants[name] = value;
            }
        }
    }

    /// <summary>
    /// Executes an ERB function with given state and returns output.
    /// When the function is not found in the specified file, searches other ERB files
    /// in the same directory (matching uEmuera's behavior of loading all ERBs).
    /// </summary>
    public async Task<(string output, List<Era.Core.Dialogue.DisplayMode> displayModes)> ExecuteAsync(
        string erbFilePath, string functionName, Dictionary<string, int> state)
    {
        var parser = new ErbParser.ErbParser();
        var astNodes = parser.Parse(erbFilePath);

        // Strip @ prefix from function name for lookup
        var targetFunctionName = functionName.StartsWith("@") ? functionName.Substring(1) : functionName;

        // Find target function in specified file
        var functionNode = astNodes.OfType<FunctionDefNode>()
            .FirstOrDefault(f => f.FunctionName.Equals(targetFunctionName, StringComparison.OrdinalIgnoreCase));

        // If not found, search other ERB files in the same character directory
        if (functionNode == null)
        {
            var directory = Path.GetDirectoryName(erbFilePath);
            if (directory != null)
            {
                foreach (var otherErb in Directory.GetFiles(directory, "*.ERB"))
                {
                    if (Path.GetFullPath(otherErb) == Path.GetFullPath(erbFilePath))
                        continue;

                    var otherNodes = parser.Parse(otherErb);
                    var found = otherNodes.OfType<FunctionDefNode>()
                        .FirstOrDefault(f => f.FunctionName.Equals(targetFunctionName, StringComparison.OrdinalIgnoreCase));

                    if (found != null)
                    {
                        functionNode = found;
                        astNodes = otherNodes;
                        break;
                    }
                }
            }
        }

        if (functionNode == null)
        {
            throw new InvalidOperationException($"Function {targetFunctionName} not found in {erbFilePath} or sibling ERB files");
        }

        // Execute function body
        var context = new ExecutionContext(state, _talentNameToIndex, _ablNameToIndex, _cflagNameToIndex);

        // Store all functions for CALL/CALLF support
        context.AllFunctions = astNodes.OfType<FunctionDefNode>().ToDictionary(
            f => f.FunctionName,
            f => f,
            StringComparer.OrdinalIgnoreCase);

        await ExecuteNodesAsync(functionNode.Body, context);

        return (context.Output.ToString(), context.DisplayModes);
    }

    /// <summary>
    /// Executes a list of AST nodes.
    /// </summary>
    private async Task ExecuteNodesAsync(List<AstNode> nodes, ExecutionContext context)
    {
        foreach (var node in nodes)
        {
            if (context.Returned)
                break;

            await ExecuteNodeAsync(node, context);
        }
    }

    /// <summary>
    /// Executes a single AST node.
    /// </summary>
    private async Task ExecuteNodeAsync(AstNode node, ExecutionContext context)
    {
        switch (node)
        {
            case PrintformNode printform:
                await ExecutePrintformAsync(printform, context);
                break;

            case PrintDataNode printData:
                await ExecutePrintDataAsync(printData, context);
                break;

            case IfNode ifNode:
                await ExecuteIfAsync(ifNode, context);
                break;

            case SelectCaseNode selectCase:
                await ExecuteSelectCaseAsync(selectCase, context);
                break;

            case AssignmentNode assignment:
                ExecuteAssignment(assignment, context);
                break;

            case ReturnNode:
                context.Returned = true;
                break;

            case CallNode call:
                // Only handle intra-file calls (function exists in same file)
                if (context.AllFunctions.TryGetValue(call.FunctionName, out var calledFunc))
                {
                    await ExecuteNodesAsync(calledFunc.Body, context);
                }
                // External calls (TRAIN_MESSAGE, KOJO_MODIFIER_*, etc.) are silently skipped
                break;

            // Ignore other node types for lightweight kojo evaluation
            default:
                break;
        }
    }

    /// <summary>
    /// Executes PRINTFORM/PRINTFORML/PRINTFORMW command.
    /// </summary>
    private async Task ExecutePrintformAsync(PrintformNode node, ExecutionContext context)
    {
        context.Output.Append(node.Content);

        // Map variant to DisplayMode
        var displayMode = node.Variant.ToUpperInvariant() switch
        {
            "PRINTFORML" => Era.Core.Dialogue.DisplayMode.Newline,
            "PRINTFORMW" => Era.Core.Dialogue.DisplayMode.Wait,
            "PRINTFORMK" => Era.Core.Dialogue.DisplayMode.KeyWait,
            "PRINTFORMKL" => Era.Core.Dialogue.DisplayMode.KeyWaitNewline,
            "PRINTFORMKW" => Era.Core.Dialogue.DisplayMode.KeyWaitWait,
            "PRINTFORMD" => Era.Core.Dialogue.DisplayMode.Display,
            "PRINTFORMDL" => Era.Core.Dialogue.DisplayMode.DisplayNewline,
            "PRINTFORMDW" => Era.Core.Dialogue.DisplayMode.DisplayWait,
            _ => Era.Core.Dialogue.DisplayMode.Default
        };

        context.DisplayModes.Add(displayMode);

        // Add newline to output if DisplayMode implies one
        if (displayMode == Era.Core.Dialogue.DisplayMode.Newline ||
            displayMode == Era.Core.Dialogue.DisplayMode.KeyWaitNewline ||
            displayMode == Era.Core.Dialogue.DisplayMode.DisplayNewline)
        {
            context.Output.Append('\n');
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Executes PRINTDATA...ENDDATA block.
    /// For CompareSubset to work, we emit ALL DATALISTs (not just the first one).
    /// Within PRINTDATA.Content, execute IF nodes and DATALISTs.
    /// </summary>
    private async Task ExecutePrintDataAsync(PrintDataNode node, ExecutionContext context)
    {
        // Execute all content nodes within PRINTDATA
        foreach (var contentNode in node.Content)
        {
            if (contentNode is DatalistNode datalist)
            {
                await ExecuteDatalistAsync(datalist, context);
            }
            else if (contentNode is IfNode ifNode)
            {
                await ExecuteIfAsync(ifNode, context);
            }
            else if (contentNode is DataformNode dataform)
            {
                // Standalone DATAFORM within PRINTDATA
                await ExecuteDataformAsync(dataform, context);
            }
            else if (contentNode is PrintformNode printform)
            {
                // Standalone PRINTFORM within PRINTDATA
                await ExecutePrintformAsync(printform, context);
            }
        }

        // Note: DisplayMode for PRINTDATA is determined by the variant suffix
        // But since we're emitting individual DATAFORMs/PRINTFORMs, they already have their own DisplayModes
        // The PRINTDATA variant applies to the whole block, but in practice we ignore it
        // since the nested content already has display modes

        await Task.CompletedTask;
    }

    /// <summary>
    /// Executes a DATALIST block.
    /// Emits all non-empty DATAFORMs as lines.
    /// </summary>
    private async Task ExecuteDatalistAsync(DatalistNode node, ExecutionContext context)
    {
        // Execute conditional branches within DATALIST
        foreach (var ifNode in node.ConditionalBranches)
        {
            await ExecuteIfAsync(ifNode, context);
        }

        // Execute all DATAFORMs
        foreach (var dataform in node.DataForms)
        {
            await ExecuteDataformAsync(dataform, context);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Executes a DATAFORM line.
    /// Joins arguments and appends to output.
    /// </summary>
    private async Task ExecuteDataformAsync(DataformNode node, ExecutionContext context)
    {
        // Skip empty DATAFORMs (blank line separators)
        if (node.Arguments.Count == 0)
            return;

        // Join arguments (template segments) into a single string
        var line = string.Join("", node.Arguments.Select(arg => arg.ToString() ?? ""));

        // Skip if resulting line is empty/whitespace
        if (string.IsNullOrWhiteSpace(line))
            return;

        context.Output.Append(line);
        context.Output.Append('\n'); // DATAFORMs always end with newline
        context.DisplayModes.Add(Era.Core.Dialogue.DisplayMode.Newline);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Executes IF...ENDIF block.
    /// </summary>
    private async Task ExecuteIfAsync(IfNode node, ExecutionContext context)
    {
        // Evaluate condition
        if (EvaluateCondition(node.Condition, context))
        {
            await ExecuteNodesAsync(node.Body, context);
            return;
        }

        // Try ELSEIF branches
        foreach (var elseIfBranch in node.ElseIfBranches)
        {
            if (EvaluateCondition(elseIfBranch.Condition, context))
            {
                await ExecuteNodesAsync(elseIfBranch.Body, context);
                return;
            }
        }

        // ELSE branch
        if (node.ElseBranch != null)
        {
            await ExecuteNodesAsync(node.ElseBranch.Body, context);
        }
    }

    /// <summary>
    /// Executes SELECTCASE...ENDSELECT block.
    /// </summary>
    private async Task ExecuteSelectCaseAsync(SelectCaseNode node, ExecutionContext context)
    {
        // Evaluate subject (simple variable reference or expression)
        var subjectValue = EvaluateExpression(node.Subject, context);

        // Try each CASE branch
        foreach (var branch in node.Branches)
        {
            foreach (var caseValue in branch.Values)
            {
                var caseIntValue = EvaluateExpression(caseValue, context);
                if (subjectValue == caseIntValue)
                {
                    await ExecuteNodesAsync(branch.Body, context);
                    return;
                }
            }
        }

        // CASEELSE
        if (node.CaseElse != null)
        {
            await ExecuteNodesAsync(node.CaseElse, context);
        }
    }

    /// <summary>
    /// Executes assignment (LOCAL, ARG, etc.).
    /// Tracks variable assignments for condition evaluation (e.g., 奴隷 = 人物_レミリア).
    /// </summary>
    private void ExecuteAssignment(AssignmentNode node, ExecutionContext context)
    {
        // Parse variable name from Target (e.g., "LOCAL", "LOCAL:1", "奴隷")
        var varName = node.Target?.Trim();
        var valueExpr = node.Value?.Trim();

        if (string.IsNullOrEmpty(varName) || string.IsNullOrEmpty(valueExpr))
            return;

        // Evaluate the expression to get the value
        var value = EvaluateExpression(valueExpr, context);

        // Store in local variables with multiple keys for flexible lookup
        // 1. Store with "LOCAL:" prefix for explicit LOCAL references
        if (varName.StartsWith("LOCAL"))
        {
            context.LocalVariables[varName] = value;
        }
        else
        {
            // 2. Store by raw name for indirect references (e.g., TALENT:奴隷:恋慕)
            context.LocalVariables[varName] = value;
            // Also store with LOCAL: prefix for consistency
            context.LocalVariables[$"LOCAL:{varName}"] = value;
        }
    }

    /// <summary>
    /// Evaluates a condition string (from IF/ELSEIF).
    /// </summary>
    private bool EvaluateCondition(string conditionStr, ExecutionContext context)
    {
        if (string.IsNullOrWhiteSpace(conditionStr))
            return false;

        var condition = _conditionExtractor.Extract(conditionStr);
        if (condition == null)
            return false;

        return EvaluateConditionObject(condition, context);
    }

    /// <summary>
    /// Evaluates an ICondition object.
    /// </summary>
    private bool EvaluateConditionObject(ICondition condition, ExecutionContext context)
    {
        switch (condition)
        {
            case TalentRef talent:
                return EvaluateTalentRef(talent, context);

            case CflagRef cflag:
                return EvaluateVariableRef("CFLAG", cflag, context);

            case AblRef abl:
                return EvaluateVariableRef("ABL", abl, context);

            case FlagRef flag:
                return EvaluateVariableRef("FLAG", flag, context);

            case TflagRef tflag:
                return EvaluateVariableRef("TFLAG", tflag, context);

            case LocalRef local:
                return EvaluateLocalRef(local, context);

            case ArgRef arg:
                return EvaluateArgRef(arg, context);

            case LogicalOp logicalOp:
                return EvaluateLogicalOp(logicalOp, context);

            case NegatedCondition negated:
                return !EvaluateConditionObject(negated.Inner, context);

            case FunctionCall:
                // Function calls not supported in lightweight mode
                return false;

            case BitwiseComparisonCondition bitwise:
                return EvaluateBitwiseComparison(bitwise, context);

            default:
                return false;
        }
    }

    /// <summary>
    /// Evaluates TALENT reference.
    /// Supports indirect variable references (e.g., TALENT:奴隷:恋慕 where 奴隷 is a local variable).
    /// </summary>
    private bool EvaluateTalentRef(TalentRef talent, ExecutionContext context)
    {
        // Resolve index
        int index;
        if (talent.Index.HasValue)
        {
            index = talent.Index.Value;
        }
        else if (!string.IsNullOrEmpty(talent.Name))
        {
            if (!context.TalentNameToIndex.TryGetValue(talent.Name, out index))
                return false; // Unknown TALENT name
        }
        else
        {
            return false;
        }

        // Determine target - resolve indirect variable references
        var target = string.IsNullOrEmpty(talent.Target) ? "TARGET" : talent.Target;

        // If target is a local variable name (e.g., 奴隷), resolve to its value
        if (target != "TARGET" && context.LocalVariables.TryGetValue(target, out var targetCharId))
        {
            // Try with resolved character ID
            var resolvedKey = $"TALENT:{targetCharId}:{index}";
            if (context.State.TryGetValue(resolvedKey, out var resolvedValue))
            {
                return ApplyOperator(resolvedValue, talent.Operator, talent.Value);
            }

            // Also try TARGET (in kojo context, the target character IS the dialogue target)
            var targetKey = $"TALENT:TARGET:{index}";
            if (context.State.TryGetValue(targetKey, out var targetValue))
            {
                return ApplyOperator(targetValue, talent.Operator, talent.Value);
            }

            // If no state found, use 0
            return ApplyOperator(0, talent.Operator, talent.Value);
        }

        // Build state key: TALENT:TARGET:16
        var stateKey = $"TALENT:{target}:{index}";

        // Get value from state
        if (!context.State.TryGetValue(stateKey, out var value))
            value = 0;

        // Apply operator comparison
        return ApplyOperator(value, talent.Operator, talent.Value);
    }

    /// <summary>
    /// Evaluates variable reference (CFLAG, ABL, FLAG, etc.).
    /// </summary>
    private bool EvaluateVariableRef(string varType, VariableRef varRef, ExecutionContext context)
    {
        // Build state key
        string stateKey;
        if (varRef.Index.HasValue)
        {
            if (!string.IsNullOrEmpty(varRef.Target))
                stateKey = $"{varType}:{varRef.Target}:{varRef.Index.Value}";
            else
                stateKey = $"{varType}:{varRef.Index.Value}";
        }
        else if (!string.IsNullOrEmpty(varRef.Name))
        {
            // Resolve name to index
            var nameToIndexMap = varType switch
            {
                "ABL" => context.AblNameToIndex,
                "CFLAG" => context.CflagNameToIndex,
                _ => null
            };

            if (nameToIndexMap == null || !nameToIndexMap.TryGetValue(varRef.Name, out var index))
                return false;

            if (!string.IsNullOrEmpty(varRef.Target))
                stateKey = $"{varType}:{varRef.Target}:{index}";
            else
                stateKey = $"{varType}:{index}";
        }
        else
        {
            return false;
        }

        // Get value from state (or local variables for LOCAL/ARG)
        int value = 0;
        if (varType == "LOCAL" || varType == "ARG")
        {
            context.LocalVariables.TryGetValue(stateKey, out value);
        }
        else
        {
            context.State.TryGetValue(stateKey, out value);
        }

        // Apply operator comparison
        return ApplyOperator(value, varRef.Operator, varRef.Value);
    }

    /// <summary>
    /// Evaluates LOCAL variable reference.
    /// </summary>
    private bool EvaluateLocalRef(LocalRef local, ExecutionContext context)
    {
        var index = local.Index ?? 0;
        var stateKey = $"LOCAL:{index}";

        context.LocalVariables.TryGetValue(stateKey, out var value);

        return ApplyOperator(value, local.Operator, local.Value);
    }

    /// <summary>
    /// Evaluates ARG variable reference.
    /// </summary>
    private bool EvaluateArgRef(ArgRef arg, ExecutionContext context)
    {
        var index = arg.Index;
        var stateKey = $"ARG:{index}";

        context.LocalVariables.TryGetValue(stateKey, out var value);

        return ApplyOperator(value, arg.Operator, arg.Value);
    }

    /// <summary>
    /// Evaluates logical operator (AND/OR).
    /// </summary>
    private bool EvaluateLogicalOp(LogicalOp logicalOp, ExecutionContext context)
    {
        if (logicalOp.Left == null || logicalOp.Right == null)
            return false;

        var leftResult = EvaluateConditionObject(logicalOp.Left, context);
        var rightResult = EvaluateConditionObject(logicalOp.Right, context);

        return logicalOp.Operator switch
        {
            "&&" => leftResult && rightResult,
            "||" => leftResult || rightResult,
            _ => false
        };
    }

    /// <summary>
    /// Evaluates bitwise comparison: (VAR & mask) op value
    /// Example: (TALENT:性別嗜好 & 3) == 3
    /// </summary>
    private bool EvaluateBitwiseComparison(BitwiseComparisonCondition bitwise, ExecutionContext context)
    {
        // Step 1: Evaluate inner condition's variable reference to get the actual value
        int varValue = 0;
        int maskValue = 0;

        // The Inner condition should have Operator="&" and Value="{mask}"
        // We need to extract the variable value and the mask
        switch (bitwise.Inner)
        {
            case TalentRef talent:
                varValue = GetVariableValue("TALENT", talent.Target ?? "TARGET", talent.Index, talent.Name, context);
                if (!int.TryParse(talent.Value, out maskValue))
                    return false;
                break;

            case CflagRef cflag:
                varValue = GetVariableValue("CFLAG", cflag.Target, cflag.Index, cflag.Name, context);
                if (!int.TryParse(cflag.Value, out maskValue))
                    return false;
                break;

            case AblRef abl:
                varValue = GetVariableValue("ABL", abl.Target, abl.Index, abl.Name, context);
                if (!int.TryParse(abl.Value, out maskValue))
                    return false;
                break;

            default:
                return false;
        }

        // Step 2: Apply bitwise AND
        var bitwiseResult = varValue & maskValue;

        // Step 3: Compare result with expected value
        if (!int.TryParse(bitwise.ComparisonValue, out var expectedValue))
            return false;

        return bitwise.ComparisonOp switch
        {
            "==" => bitwiseResult == expectedValue,
            "!=" => bitwiseResult != expectedValue,
            ">" => bitwiseResult > expectedValue,
            "<" => bitwiseResult < expectedValue,
            ">=" => bitwiseResult >= expectedValue,
            "<=" => bitwiseResult <= expectedValue,
            _ => false
        };
    }

    /// <summary>
    /// Gets the integer value of a variable from state.
    /// Helper method for BitwiseComparison evaluation.
    /// </summary>
    private int GetVariableValue(string varType, string? target, int? index, string? name, ExecutionContext context)
    {
        // Determine index
        int actualIndex;
        if (index.HasValue)
        {
            actualIndex = index.Value;
        }
        else if (!string.IsNullOrEmpty(name))
        {
            var nameToIndexMap = varType switch
            {
                "TALENT" => context.TalentNameToIndex,
                "ABL" => context.AblNameToIndex,
                "CFLAG" => context.CflagNameToIndex,
                _ => null
            };

            if (nameToIndexMap == null || !nameToIndexMap.TryGetValue(name, out actualIndex))
                return 0;
        }
        else
        {
            return 0;
        }

        // Build state key
        var actualTarget = string.IsNullOrEmpty(target) ? "TARGET" : target;
        var stateKey = varType == "FLAG" || varType == "TFLAG"
            ? $"{varType}:{actualIndex}"
            : $"{varType}:{actualTarget}:{actualIndex}";

        // Get value
        return context.State.TryGetValue(stateKey, out var value) ? value : 0;
    }

    /// <summary>
    /// Applies operator comparison.
    /// If no operator is specified, treat as truthiness check (value != 0).
    /// </summary>
    private bool ApplyOperator(int actualValue, string? op, string? expectedValueStr)
    {
        // No operator → truthiness check
        if (string.IsNullOrEmpty(op))
            return actualValue != 0;

        // Parse expected value
        if (!int.TryParse(expectedValueStr, out var expectedValue))
            return false;

        return op switch
        {
            "==" => actualValue == expectedValue,
            "!=" => actualValue != expectedValue,
            ">" => actualValue > expectedValue,
            "<" => actualValue < expectedValue,
            ">=" => actualValue >= expectedValue,
            "<=" => actualValue <= expectedValue,
            "&" => (actualValue & expectedValue) != 0, // Bitwise AND check
            _ => false
        };
    }

    /// <summary>
    /// Evaluates a simple expression (for SELECTCASE subject, CASE values, etc.).
    /// Supports: literals, variable references (ARG/LOCAL/FLAG/TFLAG), and character constants (人物_レミリア).
    /// </summary>
    private int EvaluateExpression(string expression, ExecutionContext context)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return 0;

        expression = expression.Trim();

        // Literal integer
        if (int.TryParse(expression, out var literal))
            return literal;

        // Character constant (e.g., 人物_レミリア)
        if (_characterConstants.TryGetValue(expression, out var charConstant))
            return charConstant;

        // Variable reference (ARG, LOCAL, FLAG, etc.)
        // Simple pattern: ARG or ARG:0 or LOCAL:0, etc.
        var match = Regex.Match(expression, @"^(ARG|LOCAL|FLAG|TFLAG)(?::(\d+))?$");
        if (match.Success)
        {
            var varType = match.Groups[1].Value;
            var indexStr = match.Groups[2].Success ? match.Groups[2].Value : "0";
            if (!int.TryParse(indexStr, out var index))
                return 0;

            var stateKey = $"{varType}:{index}";

            if (varType == "ARG" || varType == "LOCAL")
            {
                return context.LocalVariables.TryGetValue(stateKey, out var value) ? value : 0;
            }
            else
            {
                return context.State.TryGetValue(stateKey, out var value) ? value : 0;
            }
        }

        // Unknown expression → return 0
        return 0;
    }
}

/// <summary>
/// Execution context for evaluating ERB functions.
/// </summary>
internal class ExecutionContext
{
    public Dictionary<string, int> State { get; }
    public Dictionary<string, int> LocalVariables { get; } = new();
    public StringBuilder Output { get; } = new();
    public List<Era.Core.Dialogue.DisplayMode> DisplayModes { get; } = new();
    public bool Returned { get; set; }

    // CSV name→index mappings
    public Dictionary<string, int> TalentNameToIndex { get; }
    public Dictionary<string, int> AblNameToIndex { get; }
    public Dictionary<string, int> CflagNameToIndex { get; }

    // All functions in the file for CALL/CALLF support
    public Dictionary<string, FunctionDefNode> AllFunctions { get; set; } = new();

    public ExecutionContext(
        Dictionary<string, int> state,
        Dictionary<string, int> talentNameToIndex,
        Dictionary<string, int> ablNameToIndex,
        Dictionary<string, int> cflagNameToIndex)
    {
        State = state;
        TalentNameToIndex = talentNameToIndex;
        AblNameToIndex = ablNameToIndex;
        CflagNameToIndex = cflagNameToIndex;
    }
}
