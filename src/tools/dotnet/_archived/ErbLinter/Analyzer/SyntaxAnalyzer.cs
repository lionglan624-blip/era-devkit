using ErbLinter.Reporter;

namespace ErbLinter.Analyzer;

/// <summary>
/// Analyzes ERB syntax for block matching errors
/// </summary>
public class SyntaxAnalyzer
{
    /// <summary>
    /// Default nesting depth threshold for warnings
    /// </summary>
    public const int DefaultNestingThreshold = 10;

    /// <summary>
    /// Block type for tracking nested structures
    /// </summary>
    private record BlockInfo(string Type, int Line);

    /// <summary>
    /// Nesting depth threshold for warnings (0 = disabled)
    /// </summary>
    public int NestingThreshold { get; set; } = DefaultNestingThreshold;

    /// <summary>
    /// Analyze a file for syntax errors
    /// </summary>
    public IEnumerable<Issue> Analyze(string filePath, string[] lines)
    {
        var issues = new List<Issue>();
        var blockStack = new Stack<BlockInfo>();
        var maxDepthWarned = 0; // Track to avoid duplicate warnings

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lineNum = i + 1;

            // Skip comments and empty lines
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                continue;

            // Check for parenthesis mismatch
            var parenIssue = CheckParentheses(filePath, lineNum, lines[i]);
            if (parenIssue != null)
                issues.Add(parenIssue);

            // Check for block start/end
            var blockStart = GetBlockStart(line);
            var blockEnd = GetBlockEnd(line);

            if (blockStart != null)
            {
                blockStack.Push(new BlockInfo(blockStart, lineNum));

                // Check nesting depth warning
                if (NestingThreshold > 0 && blockStack.Count > NestingThreshold && blockStack.Count > maxDepthWarned)
                {
                    maxDepthWarned = blockStack.Count;
                    issues.Add(new Issue(filePath, lineNum, 1, IssueLevel.Warning, "ERB004",
                        $"Deep nesting detected (depth {blockStack.Count}, threshold {NestingThreshold})"));
                }
            }
            else if (blockEnd != null)
            {
                if (blockStack.Count == 0)
                {
                    var code = GetErrorCode(blockEnd);
                    issues.Add(new Issue(filePath, lineNum, 1, IssueLevel.Error, code,
                        $"{blockEnd} without matching {GetMatchingStart(blockEnd)}"));
                }
                else
                {
                    var top = blockStack.Peek();
                    var expectedEnd = GetMatchingEnd(top.Type);

                    if (expectedEnd == blockEnd)
                    {
                        blockStack.Pop();
                    }
                    else
                    {
                        // Mismatched block
                        var code = GetErrorCode(blockEnd);
                        issues.Add(new Issue(filePath, lineNum, 1, IssueLevel.Error, code,
                            $"{blockEnd} found but expected {expectedEnd} (matching {top.Type} at line {top.Line})"));
                        // Don't pop - let the error cascade
                    }
                }
            }
        }

        // Report unmatched block starts
        while (blockStack.Count > 0)
        {
            var block = blockStack.Pop();
            var code = GetErrorCode(block.Type);
            issues.Add(new Issue(filePath, block.Line, 1, IssueLevel.Error, code,
                $"{block.Type} without matching {GetMatchingEnd(block.Type)}"));
        }

        return issues;
    }

    /// <summary>
    /// Check if line starts a block, return block type or null
    /// </summary>
    private static string? GetBlockStart(string line)
    {
        // IF block (but not SIF which is single-line)
        if ((line.StartsWith("IF ", StringComparison.OrdinalIgnoreCase) ||
             line.StartsWith("IF(", StringComparison.OrdinalIgnoreCase) ||
             line.Equals("IF", StringComparison.OrdinalIgnoreCase)) &&
            !line.StartsWith("SIF", StringComparison.OrdinalIgnoreCase))
        {
            return "IF";
        }

        // FOR block
        if (line.StartsWith("FOR ", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("FOR(", StringComparison.OrdinalIgnoreCase))
        {
            return "FOR";
        }

        // REPEAT block
        if (line.StartsWith("REPEAT ", StringComparison.OrdinalIgnoreCase) ||
            line.Equals("REPEAT", StringComparison.OrdinalIgnoreCase))
        {
            return "REPEAT";
        }

        // WHILE block
        if (line.StartsWith("WHILE ", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("WHILE(", StringComparison.OrdinalIgnoreCase))
        {
            return "WHILE";
        }

        // DO block (DO...LOOP)
        if (line.Equals("DO", StringComparison.OrdinalIgnoreCase))
        {
            return "DO";
        }

        // SELECTCASE block
        if (line.StartsWith("SELECTCASE ", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("SELECTCASE(", StringComparison.OrdinalIgnoreCase))
        {
            return "SELECTCASE";
        }

        // TRYC block variants (TRYC, TRYCCALL, TRYCCALLFORM, TRYCJUMP, TRYCGOTO)
        // These all use CATCH...ENDCATCH structure
        // Note: TRYCALL (single C) is different - it's a single-line statement
        if (line.StartsWith("TRYC", StringComparison.OrdinalIgnoreCase))
        {
            // Check for TRYC alone
            if (line.Length == 4) return "TRYC";

            var afterTryc = line.Substring(4);
            // TRYC followed by space or comment
            if (char.IsWhiteSpace(afterTryc[0]) || afterTryc[0] == ';') return "TRYC";

            // TRYCC variants (TRYCCALL, TRYCCALLFORM, TRYCJUMP, TRYCGOTO)
            if (afterTryc.StartsWith("C", StringComparison.OrdinalIgnoreCase) ||  // TRYCCALL, TRYCCALLFORM
                afterTryc.StartsWith("JUMP", StringComparison.OrdinalIgnoreCase) || // TRYCJUMP
                afterTryc.StartsWith("GOTO", StringComparison.OrdinalIgnoreCase))   // TRYCGOTO
            {
                return "TRYC";
            }
        }

        // TRYCALL/TRYJUMP/TRYGOTO (single C) don't need blocks - they're single-line statements

        // PRINTDATA block (including PRINTDATAL, PRINTDATAW, PRINTDATAK variants)
        if (line.StartsWith("PRINTDATA", StringComparison.OrdinalIgnoreCase))
        {
            // Check for exact PRINTDATA (9 chars)
            if (line.Length == 9) return "PRINTDATA";

            var nextChar = line[9];
            // PRINTDATA followed by space or comment
            if (char.IsWhiteSpace(nextChar) || nextChar == ';') return "PRINTDATA";

            // Check for L, W, K variants (PRINTDATAL, PRINTDATAW, PRINTDATAK)
            if (nextChar == 'L' || nextChar == 'l' ||
                nextChar == 'W' || nextChar == 'w' ||
                nextChar == 'K' || nextChar == 'k')
            {
                // Variant is 10 chars total
                if (line.Length == 10) return "PRINTDATA";
                var afterVariant = line[10];
                if (char.IsWhiteSpace(afterVariant) || afterVariant == ';') return "PRINTDATA";
            }
        }

        // DATALIST block
        if (IsKeyword(line, "DATALIST"))
        {
            return "DATALIST";
        }

        // STRDATA block
        if (line.StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase))
        {
            if (line.Length == 7 || char.IsWhiteSpace(line[7]) || line[7] == ';')
                return "STRDATA";
        }

        // NOSKIP block
        if (IsKeyword(line, "NOSKIP"))
        {
            return "NOSKIP";
        }

        return null;
    }

    /// <summary>
    /// Check if line ends a block, return block end type or null
    /// </summary>
    private static string? GetBlockEnd(string line)
    {
        if (IsKeyword(line, "ENDIF")) return "ENDIF";
        if (IsKeyword(line, "NEXT")) return "NEXT";
        if (IsKeyword(line, "REND")) return "REND";
        if (IsKeyword(line, "WEND")) return "WEND";
        if (IsKeyword(line, "LOOP")) return "LOOP";
        if (IsKeyword(line, "ENDSELECT")) return "ENDSELECT";
        if (IsKeyword(line, "ENDCATCH")) return "ENDCATCH";
        if (IsKeyword(line, "ENDDATA")) return "ENDDATA";
        if (IsKeyword(line, "ENDLIST")) return "ENDLIST";
        if (IsKeyword(line, "ENDNOSKIP")) return "ENDNOSKIP";

        return null;
    }

    /// <summary>
    /// Check if line is or starts with a keyword
    /// </summary>
    private static bool IsKeyword(string line, string keyword)
    {
        if (!line.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
            return false;

        if (line.Length == keyword.Length)
            return true;

        var nextChar = line[keyword.Length];
        return char.IsWhiteSpace(nextChar) || nextChar == ';';
    }

    /// <summary>
    /// Get matching end keyword for a start keyword
    /// </summary>
    private static string GetMatchingEnd(string start)
    {
        return start switch
        {
            "IF" => "ENDIF",
            "FOR" => "NEXT",
            "REPEAT" => "REND",
            "WHILE" => "WEND",
            "DO" => "LOOP",
            "SELECTCASE" => "ENDSELECT",
            "TRYC" => "ENDCATCH",
            "PRINTDATA" => "ENDDATA",
            "STRDATA" => "ENDDATA",
            "DATALIST" => "ENDLIST",
            "NOSKIP" => "ENDNOSKIP",
            _ => "END" + start
        };
    }

    /// <summary>
    /// Get matching start keyword for an end keyword
    /// </summary>
    private static string GetMatchingStart(string end)
    {
        return end switch
        {
            "ENDIF" => "IF",
            "NEXT" => "FOR",
            "REND" => "REPEAT",
            "WEND" => "WHILE",
            "LOOP" => "DO",
            "ENDSELECT" => "SELECTCASE",
            "ENDCATCH" => "TRYC",
            "ENDDATA" => "PRINTDATA/STRDATA",
            "ENDLIST" => "DATALIST",
            "ENDNOSKIP" => "NOSKIP",
            _ => end.Replace("END", "")
        };
    }

    /// <summary>
    /// Get error code for a block type
    /// </summary>
    private static string GetErrorCode(string blockType)
    {
        return blockType switch
        {
            "IF" or "ENDIF" => "ERB001",
            "FOR" or "NEXT" => "ERB002",
            "REPEAT" or "REND" => "ERB002",
            "WHILE" or "WEND" => "ERB002",
            "DO" or "LOOP" => "ERB002",
            "SELECTCASE" or "ENDSELECT" => "ERB002",
            _ => "ERB002"
        };
    }

    /// <summary>
    /// Check for unbalanced parentheses in a line
    /// </summary>
    private static Issue? CheckParentheses(string filePath, int lineNum, string line)
    {
        var trimmed = line.TrimStart();

        // Skip PRINT/PRINTFORM lines - they have special string interpolation syntax
        // where parentheses don't need to match (they're often literal output)
        if (trimmed.StartsWith("PRINT", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("DATA", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("STR", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Skip comment lines
        var commentIndex = line.IndexOf(';');
        var code = commentIndex >= 0 ? line.Substring(0, commentIndex) : line;

        var parenStack = new Stack<(char Char, int Col)>();
        var inString = false;
        var stringChar = '\0';

        for (int i = 0; i < code.Length; i++)
        {
            var c = code[i];

            // Track string literals (double quotes)
            if (c == '"' && !inString)
            {
                inString = true;
                stringChar = '"';
                continue;
            }
            if (c == stringChar && inString)
            {
                inString = false;
                continue;
            }

            // Skip characters inside strings
            if (inString)
                continue;

            // Track parentheses and braces
            if (c == '(' || c == '{')
            {
                parenStack.Push((c, i + 1));
            }
            else if (c == ')' || c == '}')
            {
                var expected = c == ')' ? '(' : '{';
                if (parenStack.Count == 0)
                {
                    return new Issue(filePath, lineNum, i + 1, IssueLevel.Error, "ERB003",
                        $"Unmatched closing '{c}'");
                }
                var top = parenStack.Pop();
                if (top.Char != expected)
                {
                    return new Issue(filePath, lineNum, i + 1, IssueLevel.Error, "ERB003",
                        $"Mismatched '{c}' - expected closing '{(top.Char == '(' ? ')' : '}')}' for '{top.Char}' at column {top.Col}");
                }
            }
        }

        // Check for unclosed parentheses
        if (parenStack.Count > 0)
        {
            var unclosed = parenStack.Pop();
            return new Issue(filePath, lineNum, unclosed.Col, IssueLevel.Error, "ERB003",
                $"Unclosed '{unclosed.Char}'");
        }

        return null;
    }
}
