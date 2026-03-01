using System.Text;
using ErbParser.Ast;

namespace ErbParser;

/// <summary>
/// Main ERB parser that converts ERB source files into AST
/// </summary>
public class ErbParser
{
    /// <summary>
    /// Parse ERB source file and return AST
    /// </summary>
    /// <param name="filePath">Path to ERB file</param>
    /// <returns>List of top-level AST nodes</returns>
    public List<AstNode> Parse(string filePath)
    {
        string source = File.ReadAllText(filePath, Encoding.UTF8);
        return ParseString(source, filePath);
    }

    /// <summary>
    /// Parse ERB source from string and return AST
    /// </summary>
    /// <param name="source">ERB source code</param>
    /// <param name="fileName">Virtual file name for error reporting</param>
    /// <returns>List of top-level AST nodes</returns>
    public List<AstNode> ParseString(string source, string fileName = "<string>")
    {
        var nodes = new List<AstNode>();
        var lines = source.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        int lineNumber = 0;
        bool inDatalist = false;
        DatalistNode? currentDatalist = null;
        bool inPrintData = false;
        PrintDataNode? currentPrintData = null;
        bool inStrData = false;
        FunctionDefNode? currentFunction = null;
        var dimDeclaredVars = new HashSet<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            lineNumber = i + 1;
            string line = lines[i].Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                continue;

            // Check for function definition (@-lines)
            if (line.StartsWith("@"))
            {
                // Close previous function if open
                if (currentFunction != null)
                {
                    nodes.Add(currentFunction);
                }

                // Parse function signature with regex: ^@(\w+)(?:\((.*)\))?$
                var funcMatch = System.Text.RegularExpressions.Regex.Match(line, @"^@(\w+)(?:\((.*)\))?$");
                if (funcMatch.Success)
                {
                    currentFunction = new FunctionDefNode
                    {
                        FunctionName = funcMatch.Groups[1].Value,
                        LineNumber = lineNumber,
                        SourceFile = fileName
                    };

                    // Parse parameters if present
                    if (funcMatch.Groups[2].Success && !string.IsNullOrWhiteSpace(funcMatch.Groups[2].Value))
                    {
                        var paramString = funcMatch.Groups[2].Value;
                        var parameters = paramString.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p));
                        currentFunction.Parameters.AddRange(parameters);
                    }

                    // Clear DIM declarations for new function scope
                    dimDeclaredVars.Clear();
                }
                continue;
            }

            // Check for DATALIST
            if (line.Equals("DATALIST", StringComparison.OrdinalIgnoreCase))
            {
                if (inDatalist)
                {
                    throw new ParseException("Nested DATALIST is not allowed", fileName, lineNumber);
                }
                inDatalist = true;
                currentDatalist = new DatalistNode
                {
                    LineNumber = lineNumber,
                    SourceFile = fileName
                };
                continue;
            }

            // Check for ENDLIST
            if (line.Equals("ENDLIST", StringComparison.OrdinalIgnoreCase))
            {
                if (!inDatalist || currentDatalist == null)
                {
                    throw new ParseException("ENDLIST without matching DATALIST", fileName, lineNumber);
                }

                // If we're inside PRINTDATA, add the datalist to currentPrintData.Content instead of top-level nodes
                if (inPrintData && currentPrintData != null)
                {
                    currentPrintData.Content.Add(currentDatalist);
                }
                else if (currentFunction != null)
                {
                    currentFunction.Body.Add(currentDatalist);
                }
                else
                {
                    nodes.Add(currentDatalist);
                }

                inDatalist = false;
                currentDatalist = null;
                continue;
            }

            // Check for PRINTDATA (9 variants)
            if (line.StartsWith("PRINTDATA", StringComparison.OrdinalIgnoreCase))
            {
                if (inPrintData)
                {
                    throw new ParseException("Nested PRINTDATA is not allowed", fileName, lineNumber);
                }
                inPrintData = true;
                currentPrintData = new PrintDataNode
                {
                    LineNumber = lineNumber,
                    SourceFile = fileName,
                    Variant = line.Split()[0]
                };
                continue;
            }

            // Check for ENDDATA
            if (line.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
            {
                if (inPrintData && currentPrintData != null)
                {
                    if (currentFunction != null)
                        currentFunction.Body.Add(currentPrintData);
                    else
                        nodes.Add(currentPrintData);
                    inPrintData = false;
                    currentPrintData = null;
                }
                else if (inStrData)
                {
                    // STRDATA content is skipped (not added to AST)
                    inStrData = false;
                }
                else
                {
                    throw new ParseException("ENDDATA without matching PRINTDATA", fileName, lineNumber);
                }
                continue;
            }

            // Check for STRDATA
            if (line.StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase))
            {
                if (inStrData)
                {
                    throw new ParseException("Nested STRDATA is not allowed", fileName, lineNumber);
                }
                inStrData = true;
                continue;
            }

            // Check for DATAFORM (inside DATALIST or standalone in PRINTDATA)
            if (line.StartsWith("DATAFORM", StringComparison.OrdinalIgnoreCase))
            {
                var dataform = ParseDataform(line, fileName, lineNumber);

                if (inDatalist && currentDatalist != null)
                {
                    // Inside DATALIST block
                    currentDatalist.DataForms.Add(dataform);
                }
                else if (inPrintData && !inDatalist && currentPrintData != null)
                {
                    // Standalone DATAFORM inside PRINTDATA (not in DATALIST)
                    currentPrintData.Content.Add(dataform);
                }
                else
                {
                    throw new ParseException("DATAFORM outside of DATALIST or PRINTDATA block", fileName, lineNumber);
                }
                continue;
            }

            // Check for IF block
            if (line.StartsWith("IF ", StringComparison.OrdinalIgnoreCase))
            {
                var ifNode = ParseIfBlock(lines, ref i, fileName, inDatalist || inPrintData);

                // F349 Task 2: IF blocks inside DATALIST are conditional branches
                if (inDatalist && currentDatalist != null)
                {
                    currentDatalist.ConditionalBranches.Add(ifNode);
                }
                else if (inPrintData && !inDatalist && currentPrintData != null)
                {
                    // IF blocks inside PRINTDATA (not in DATALIST) are added to Content
                    currentPrintData.Content.Add(ifNode);
                }
                else if (currentFunction != null)
                {
                    currentFunction.Body.Add(ifNode);
                }
                else
                {
                    nodes.Add(ifNode);
                }

                lineNumber = i + 1;
                continue;
            }

            // Check for PRINTFORM/PRINTFORML (standalone)
            if (line.StartsWith("PRINTFORM", StringComparison.OrdinalIgnoreCase))
            {
                int spaceIndex = line.IndexOf(' ');
                string content = spaceIndex > 0 ? line.Substring(spaceIndex + 1).Trim() : "";

                var printNode = new PrintformNode
                {
                    LineNumber = lineNumber,
                    SourceFile = fileName,
                    Content = content,
                    Variant = line.Split()[0]
                };

                // If inside PRINTDATA, add to Content; otherwise add as top-level node
                if (inPrintData && currentPrintData != null)
                {
                    currentPrintData.Content.Add(printNode);
                }
                else if (currentFunction != null)
                {
                    currentFunction.Body.Add(printNode);
                }
                else
                {
                    nodes.Add(printNode);
                }
                continue;
            }

            // LOCAL assignment recognition (F761)
            var localAssignmentMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(LOCAL(?::\d+)?)\s*=\s*(.+)$");
            if (localAssignmentMatch.Success)
            {
                var assignmentNode = new AssignmentNode
                {
                    Target = localAssignmentMatch.Groups[1].Value,
                    Value = localAssignmentMatch.Groups[2].Value.Trim(),
                    LineNumber = lineNumber,
                    SourceFile = fileName
                };
                if (currentFunction != null)
                    currentFunction.Body.Add(assignmentNode);
                else
                    nodes.Add(assignmentNode);
                continue;
            }

            // #DIM-declared variable assignment recognition
            if (dimDeclaredVars.Count > 0)
            {
                var dimAssignmentMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(\w+)\s*=\s*(.+)$");
                if (dimAssignmentMatch.Success && dimDeclaredVars.Contains(dimAssignmentMatch.Groups[1].Value))
                {
                    var assignmentNode = new AssignmentNode
                    {
                        Target = dimAssignmentMatch.Groups[1].Value,
                        Value = dimAssignmentMatch.Groups[2].Value.Trim(),
                        LineNumber = lineNumber,
                        SourceFile = fileName
                    };
                    if (currentFunction != null)
                        currentFunction.Body.Add(assignmentNode);
                    else
                        nodes.Add(assignmentNode);
                    continue;
                }
            }

            // Check for SELECTCASE block
            if (line.StartsWith("SELECTCASE", StringComparison.OrdinalIgnoreCase))
            {
                var selectCaseNode = ParseSelectCaseBlock(lines, ref i, fileName);
                if (currentFunction != null)
                    currentFunction.Body.Add(selectCaseNode);
                else
                    nodes.Add(selectCaseNode);
                lineNumber = i + 1;
                continue;
            }

            // Check for RETURN statement
            if (line.StartsWith("RETURN", StringComparison.OrdinalIgnoreCase))
            {
                // Extract value after RETURN keyword
                string value = "";
                int spaceIndex = line.IndexOf(' ');
                if (spaceIndex > 0 && spaceIndex + 1 < line.Length)
                {
                    value = line.Substring(spaceIndex + 1).Trim();
                }

                var returnNode = new ReturnNode
                {
                    Value = value,
                    LineNumber = lineNumber,
                    SourceFile = fileName
                };

                if (currentFunction != null)
                    currentFunction.Body.Add(returnNode);
                else
                    nodes.Add(returnNode);
                continue;
            }

            // Check for CALL/CALLF statements
            if (line.StartsWith("CALL ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALLF ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALL\t", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALLF\t", StringComparison.OrdinalIgnoreCase))
            {
                var isCallF = line.StartsWith("CALLF", StringComparison.OrdinalIgnoreCase);
                var spaceIndex = line.IndexOfAny(new[] { ' ', '\t' });
                var funcName = spaceIndex > 0 ? line.Substring(spaceIndex + 1).Trim() : "";

                var callNode = new CallNode
                {
                    FunctionName = funcName,
                    IsCallF = isCallF,
                    LineNumber = lineNumber,
                    SourceFile = fileName
                };

                if (currentFunction != null)
                    currentFunction.Body.Add(callNode);
                else
                    nodes.Add(callNode);
                continue;
            }

            // #DIM variable declaration recognition
            var dimMatch = System.Text.RegularExpressions.Regex.Match(line, @"^#DIM\s+(\w+)");
            if (dimMatch.Success)
            {
                dimDeclaredVars.Add(dimMatch.Groups[1].Value);
                continue;
            }

            // Skip other unrecognized lines
        }

        // Close last function at EOF
        if (currentFunction != null)
        {
            nodes.Add(currentFunction);
        }

        // Check for unclosed DATALIST
        if (inDatalist)
        {
            throw new ParseException("DATALIST without matching ENDLIST", fileName, lineNumber);
        }

        // Check for unclosed PRINTDATA
        if (inPrintData)
        {
            throw new ParseException("PRINTDATA without matching ENDDATA", fileName, lineNumber);
        }

        // Check for unclosed STRDATA
        if (inStrData)
        {
            throw new ParseException("STRDATA without matching ENDDATA", fileName, lineNumber);
        }

        return nodes;
    }

    private DataformNode ParseDataform(string line, string fileName, int lineNumber)
    {
        var node = new DataformNode
        {
            LineNumber = lineNumber,
            SourceFile = fileName
        };

        // Extract arguments after "DATAFORM "
        int startIndex = line.IndexOf(' ') + 1;
        if (startIndex <= 0 || startIndex >= line.Length)
            return node;

        string argsString = line.Substring(startIndex);
        var args = ParseArguments(argsString);

        foreach (var arg in args)
        {
            node.Arguments.Add(arg);
        }

        return node;
    }

    private List<object> ParseArguments(string argsString)
    {
        var arguments = new List<object>();
        var currentArg = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < argsString.Length; i++)
        {
            char c = argsString[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                currentArg.Append(c);
            }
            else if (c == ',' && !inQuotes)
            {
                // End of argument
                string arg = currentArg.ToString().Trim();
                if (!string.IsNullOrEmpty(arg))
                {
                    arguments.Add(ParseSingleArgument(arg));
                }
                currentArg.Clear();
            }
            else
            {
                currentArg.Append(c);
            }
        }

        // Add last argument
        string lastArg = currentArg.ToString().Trim();
        if (!string.IsNullOrEmpty(lastArg))
        {
            arguments.Add(ParseSingleArgument(lastArg));
        }

        return arguments;
    }

    private object ParseSingleArgument(string arg)
    {
        // If quoted string, return as string
        if (arg.StartsWith("\"") && arg.EndsWith("\""))
        {
            return arg.Substring(1, arg.Length - 2);
        }

        // Try to parse as integer
        if (int.TryParse(arg, out int intValue))
        {
            return intValue;
        }

        // Default to string
        return arg;
    }

    private IfNode ParseIfBlock(string[] lines, ref int currentIndex, string fileName, bool inDatalist = false)
    {
        int lineNumber = currentIndex + 1;
        string line = lines[currentIndex].Trim();

        var ifNode = new IfNode
        {
            LineNumber = lineNumber,
            SourceFile = fileName
        };

        // Extract condition
        int ifKeywordEnd = line.IndexOf(' ');
        if (ifKeywordEnd > 0 && ifKeywordEnd + 1 < line.Length)
        {
            ifNode.Condition = line.Substring(ifKeywordEnd + 1).Trim();
        }

        // Parse IF body
        currentIndex++;
        while (currentIndex < lines.Length)
        {
            lineNumber = currentIndex + 1;
            line = lines[currentIndex].Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
            {
                currentIndex++;
                continue;
            }

            // Check for ENDIF
            if (line.Equals("ENDIF", StringComparison.OrdinalIgnoreCase))
            {
                return ifNode;
            }

            // Check for ELSEIF
            if (line.StartsWith("ELSEIF", StringComparison.OrdinalIgnoreCase))
            {
                var elseIfBranch = ParseElseIfBranch(lines, ref currentIndex, fileName, inDatalist);
                ifNode.ElseIfBranches.Add(elseIfBranch);
                continue;
            }

            // Check for ELSE
            if (line.Equals("ELSE", StringComparison.OrdinalIgnoreCase))
            {
                ifNode.ElseBranch = ParseElseBranch(lines, ref currentIndex, fileName, inDatalist);
                continue;
            }

            // Check for nested IF
            if (line.StartsWith("IF ", StringComparison.OrdinalIgnoreCase))
            {
                var nestedIf = ParseIfBlock(lines, ref currentIndex, fileName, inDatalist);
                ifNode.Body.Add(nestedIf);
                currentIndex++;
                continue;
            }

            // F349 Task 2: Handle DATAFORM inside IF when in DATALIST
            if (inDatalist && line.StartsWith("DATAFORM", StringComparison.OrdinalIgnoreCase))
            {
                var dataform = ParseDataform(line, fileName, lineNumber);
                ifNode.Body.Add(dataform);
                currentIndex++;
                continue;
            }

            // Check for PRINTDATA inside IF (F634: IF-wrapped PRINTDATA blocks)
            if (line.StartsWith("PRINTDATA", StringComparison.OrdinalIgnoreCase))
            {
                var printDataNode = new PrintDataNode
                {
                    LineNumber = lineNumber,
                    SourceFile = fileName,
                    Variant = line.Split()[0]
                };

                // Parse PRINTDATA content until ENDDATA
                currentIndex++;
                bool inPrintDataBlock = true;
                bool inNestedDatalist = false;
                DatalistNode? nestedDatalist = null;

                while (currentIndex < lines.Length && inPrintDataBlock)
                {
                    lineNumber = currentIndex + 1;
                    string printDataLine = lines[currentIndex].Trim();

                    if (string.IsNullOrWhiteSpace(printDataLine) || printDataLine.StartsWith(";"))
                    {
                        currentIndex++;
                        continue;
                    }

                    if (printDataLine.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
                    {
                        inPrintDataBlock = false;
                        break;
                    }

                    // Handle DATALIST inside PRINTDATA
                    if (printDataLine.Equals("DATALIST", StringComparison.OrdinalIgnoreCase))
                    {
                        inNestedDatalist = true;
                        nestedDatalist = new DatalistNode
                        {
                            LineNumber = lineNumber,
                            SourceFile = fileName
                        };
                        currentIndex++;
                        continue;
                    }

                    // Handle ENDLIST
                    if (printDataLine.Equals("ENDLIST", StringComparison.OrdinalIgnoreCase))
                    {
                        if (inNestedDatalist && nestedDatalist != null)
                        {
                            printDataNode.Content.Add(nestedDatalist);
                            inNestedDatalist = false;
                            nestedDatalist = null;
                        }
                        currentIndex++;
                        continue;
                    }

                    // Handle DATAFORM
                    if (printDataLine.StartsWith("DATAFORM", StringComparison.OrdinalIgnoreCase))
                    {
                        var dataform = ParseDataform(printDataLine, fileName, lineNumber);
                        if (inNestedDatalist && nestedDatalist != null)
                        {
                            nestedDatalist.DataForms.Add(dataform);
                        }
                        else
                        {
                            printDataNode.Content.Add(dataform);
                        }
                        currentIndex++;
                        continue;
                    }

                    currentIndex++;
                }

                ifNode.Body.Add(printDataNode);
                currentIndex++;
                continue;
            }

            // Check for STRDATA inside IF
            if (line.StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase))
            {
                // Skip STRDATA content until ENDDATA
                currentIndex++;
                while (currentIndex < lines.Length)
                {
                    lineNumber = currentIndex + 1;
                    string strDataLine = lines[currentIndex].Trim();

                    if (string.IsNullOrWhiteSpace(strDataLine) || strDataLine.StartsWith(";"))
                    {
                        currentIndex++;
                        continue;
                    }

                    if (strDataLine.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
                    {
                        break; // STRDATA block complete
                    }

                    // Skip STRDATA content (not added to AST)
                    currentIndex++;
                }
                currentIndex++;
                continue;
            }

            // Check for PRINTFORM/PRINTFORML inside IF
            if (line.StartsWith("PRINTFORM", StringComparison.OrdinalIgnoreCase))
            {
                int spaceIndex = line.IndexOf(' ');
                string content = spaceIndex > 0 ? line.Substring(spaceIndex + 1).Trim() : "";

                var printNode = new PrintformNode
                {
                    LineNumber = lineNumber,
                    SourceFile = fileName,
                    Content = content,
                    Variant = line.Split()[0]
                };
                ifNode.Body.Add(printNode);
                currentIndex++;
                continue;
            }

            // LOCAL assignment recognition (F761)
            var localAssignMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(LOCAL(?::\d+)?)\s*=\s*(.+)$");
            if (localAssignMatch.Success)
            {
                ifNode.Body.Add(new AssignmentNode
                {
                    Target = localAssignMatch.Groups[1].Value,
                    Value = localAssignMatch.Groups[2].Value.Trim(),
                    LineNumber = lineNumber,
                    SourceFile = fileName
                });
                currentIndex++;
                continue;
            }

            // Check for SELECTCASE inside IF
            if (line.StartsWith("SELECTCASE", StringComparison.OrdinalIgnoreCase))
            {
                var selectCaseNode = ParseSelectCaseBlock(lines, ref currentIndex, fileName);
                ifNode.Body.Add(selectCaseNode);
                currentIndex++;
                continue;
            }

            // Check for RETURN inside IF
            if (line.StartsWith("RETURN", StringComparison.OrdinalIgnoreCase))
            {
                string value = "";
                int spaceIndex = line.IndexOf(' ');
                if (spaceIndex > 0 && spaceIndex + 1 < line.Length)
                {
                    value = line.Substring(spaceIndex + 1).Trim();
                }

                ifNode.Body.Add(new ReturnNode
                {
                    Value = value,
                    LineNumber = lineNumber,
                    SourceFile = fileName
                });
                currentIndex++;
                continue;
            }

            // Check for CALL/CALLF inside IF
            if (line.StartsWith("CALL ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALLF ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALL\t", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALLF\t", StringComparison.OrdinalIgnoreCase))
            {
                var isCallF = line.StartsWith("CALLF", StringComparison.OrdinalIgnoreCase);
                var spaceIndex = line.IndexOfAny(new[] { ' ', '\t' });
                var funcName = spaceIndex > 0 ? line.Substring(spaceIndex + 1).Trim() : "";

                ifNode.Body.Add(new CallNode
                {
                    FunctionName = funcName,
                    IsCallF = isCallF,
                    LineNumber = lineNumber,
                    SourceFile = fileName
                });
                currentIndex++;
                continue;
            }

            // Skip other statements inside IF
            currentIndex++;
        }

        throw new ParseException("IF without matching ENDIF", fileName, ifNode.LineNumber);
    }

    private ElseIfBranch ParseElseIfBranch(string[] lines, ref int currentIndex, string fileName, bool inDatalist = false)
    {
        int lineNumber = currentIndex + 1;
        string line = lines[currentIndex].Trim();

        var elseIfBranch = new ElseIfBranch();

        // Extract condition from ELSEIF line
        int elseIfKeywordEnd = line.IndexOf(' ');
        if (elseIfKeywordEnd > 0 && elseIfKeywordEnd + 1 < line.Length)
        {
            elseIfBranch.Condition = line.Substring(elseIfKeywordEnd + 1).Trim();
        }

        // Parse ELSEIF body
        currentIndex++;
        while (currentIndex < lines.Length)
        {
            lineNumber = currentIndex + 1;
            line = lines[currentIndex].Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
            {
                currentIndex++;
                continue;
            }

            // Check for end of ELSEIF (another ELSEIF, ELSE, or ENDIF)
            if (line.Equals("ENDIF", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("ELSEIF", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("ELSE", StringComparison.OrdinalIgnoreCase))
            {
                return elseIfBranch;
            }

            // Check for nested IF
            if (line.StartsWith("IF ", StringComparison.OrdinalIgnoreCase))
            {
                var nestedIf = ParseIfBlock(lines, ref currentIndex, fileName, inDatalist);
                elseIfBranch.Body.Add(nestedIf);
                currentIndex++;
                continue;
            }

            // F349 Task 2: Handle DATAFORM inside ELSEIF when in DATALIST
            if (inDatalist && line.StartsWith("DATAFORM", StringComparison.OrdinalIgnoreCase))
            {
                var dataform = ParseDataform(line, fileName, lineNumber);
                elseIfBranch.Body.Add(dataform);
                currentIndex++;
                continue;
            }

            // Check for PRINTDATA inside ELSEIF (F634: IF-wrapped PRINTDATA blocks)
            if (line.StartsWith("PRINTDATA", StringComparison.OrdinalIgnoreCase))
            {
                var printDataNode = new PrintDataNode
                {
                    LineNumber = lineNumber,
                    SourceFile = fileName,
                    Variant = line.Split()[0]
                };

                // Parse PRINTDATA content until ENDDATA
                currentIndex++;
                bool inPrintDataBlock = true;
                bool inNestedDatalist = false;
                DatalistNode? nestedDatalist = null;

                while (currentIndex < lines.Length && inPrintDataBlock)
                {
                    lineNumber = currentIndex + 1;
                    string printDataLine = lines[currentIndex].Trim();

                    if (string.IsNullOrWhiteSpace(printDataLine) || printDataLine.StartsWith(";"))
                    {
                        currentIndex++;
                        continue;
                    }

                    if (printDataLine.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
                    {
                        inPrintDataBlock = false;
                        break;
                    }

                    // Handle DATALIST inside PRINTDATA
                    if (printDataLine.Equals("DATALIST", StringComparison.OrdinalIgnoreCase))
                    {
                        inNestedDatalist = true;
                        nestedDatalist = new DatalistNode
                        {
                            LineNumber = lineNumber,
                            SourceFile = fileName
                        };
                        currentIndex++;
                        continue;
                    }

                    // Handle ENDLIST
                    if (printDataLine.Equals("ENDLIST", StringComparison.OrdinalIgnoreCase))
                    {
                        if (inNestedDatalist && nestedDatalist != null)
                        {
                            printDataNode.Content.Add(nestedDatalist);
                            inNestedDatalist = false;
                            nestedDatalist = null;
                        }
                        currentIndex++;
                        continue;
                    }

                    // Handle DATAFORM
                    if (printDataLine.StartsWith("DATAFORM", StringComparison.OrdinalIgnoreCase))
                    {
                        var dataform = ParseDataform(printDataLine, fileName, lineNumber);
                        if (inNestedDatalist && nestedDatalist != null)
                        {
                            nestedDatalist.DataForms.Add(dataform);
                        }
                        else
                        {
                            printDataNode.Content.Add(dataform);
                        }
                        currentIndex++;
                        continue;
                    }

                    currentIndex++;
                }

                elseIfBranch.Body.Add(printDataNode);
                currentIndex++;
                continue;
            }

            // Check for STRDATA inside ELSEIF
            if (line.StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase))
            {
                // Skip STRDATA content until ENDDATA
                currentIndex++;
                while (currentIndex < lines.Length)
                {
                    lineNumber = currentIndex + 1;
                    string strDataLine = lines[currentIndex].Trim();

                    if (string.IsNullOrWhiteSpace(strDataLine) || strDataLine.StartsWith(";"))
                    {
                        currentIndex++;
                        continue;
                    }

                    if (strDataLine.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
                    {
                        break; // STRDATA block complete
                    }

                    // Skip STRDATA content (not added to AST)
                    currentIndex++;
                }
                currentIndex++;
                continue;
            }

            // Check for PRINTFORM/PRINTFORML
            if (line.StartsWith("PRINTFORM", StringComparison.OrdinalIgnoreCase))
            {
                int spaceIndex = line.IndexOf(' ');
                string content = spaceIndex > 0 ? line.Substring(spaceIndex + 1).Trim() : "";

                var printNode = new PrintformNode
                {
                    LineNumber = lineNumber,
                    SourceFile = fileName,
                    Content = content,
                    Variant = line.Split()[0]
                };
                elseIfBranch.Body.Add(printNode);
                currentIndex++;
                continue;
            }

            // LOCAL assignment recognition (F761)
            var localAssignMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(LOCAL(?::\d+)?)\s*=\s*(.+)$");
            if (localAssignMatch.Success)
            {
                elseIfBranch.Body.Add(new AssignmentNode
                {
                    Target = localAssignMatch.Groups[1].Value,
                    Value = localAssignMatch.Groups[2].Value.Trim(),
                    LineNumber = lineNumber,
                    SourceFile = fileName
                });
                currentIndex++;
                continue;
            }

            // Check for SELECTCASE inside ELSEIF
            if (line.StartsWith("SELECTCASE", StringComparison.OrdinalIgnoreCase))
            {
                var selectCaseNode = ParseSelectCaseBlock(lines, ref currentIndex, fileName);
                elseIfBranch.Body.Add(selectCaseNode);
                currentIndex++;
                continue;
            }

            // Check for RETURN inside ELSEIF
            if (line.StartsWith("RETURN", StringComparison.OrdinalIgnoreCase))
            {
                string value = "";
                int spaceIndex = line.IndexOf(' ');
                if (spaceIndex > 0 && spaceIndex + 1 < line.Length)
                {
                    value = line.Substring(spaceIndex + 1).Trim();
                }

                elseIfBranch.Body.Add(new ReturnNode
                {
                    Value = value,
                    LineNumber = lineNumber,
                    SourceFile = fileName
                });
                currentIndex++;
                continue;
            }

            // Check for CALL/CALLF inside ELSEIF
            if (line.StartsWith("CALL ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALLF ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALL\t", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALLF\t", StringComparison.OrdinalIgnoreCase))
            {
                var isCallF = line.StartsWith("CALLF", StringComparison.OrdinalIgnoreCase);
                var spaceIndex = line.IndexOfAny(new[] { ' ', '\t' });
                var funcName = spaceIndex > 0 ? line.Substring(spaceIndex + 1).Trim() : "";

                elseIfBranch.Body.Add(new CallNode
                {
                    FunctionName = funcName,
                    IsCallF = isCallF,
                    LineNumber = lineNumber,
                    SourceFile = fileName
                });
                currentIndex++;
                continue;
            }

            // Skip other statements
            currentIndex++;
        }

        return elseIfBranch;
    }

    private ElseBranch ParseElseBranch(string[] lines, ref int currentIndex, string fileName, bool inDatalist = false)
    {
        var elseBranch = new ElseBranch();

        // Parse ELSE body
        currentIndex++;
        while (currentIndex < lines.Length)
        {
            int lineNumber = currentIndex + 1;
            string line = lines[currentIndex].Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
            {
                currentIndex++;
                continue;
            }

            // Check for ENDIF
            if (line.Equals("ENDIF", StringComparison.OrdinalIgnoreCase))
            {
                return elseBranch;
            }

            // Check for nested IF
            if (line.StartsWith("IF ", StringComparison.OrdinalIgnoreCase))
            {
                var nestedIf = ParseIfBlock(lines, ref currentIndex, fileName, inDatalist);
                elseBranch.Body.Add(nestedIf);
                currentIndex++;
                continue;
            }

            // F349 Task 2: Handle DATAFORM inside ELSE when in DATALIST
            if (inDatalist && line.StartsWith("DATAFORM", StringComparison.OrdinalIgnoreCase))
            {
                var dataform = ParseDataform(line, fileName, lineNumber);
                elseBranch.Body.Add(dataform);
                currentIndex++;
                continue;
            }

            // Check for PRINTDATA inside ELSE (F634: IF-wrapped PRINTDATA blocks)
            if (line.StartsWith("PRINTDATA", StringComparison.OrdinalIgnoreCase))
            {
                var printDataNode = new PrintDataNode
                {
                    LineNumber = lineNumber,
                    SourceFile = fileName,
                    Variant = line.Split()[0]
                };

                // Parse PRINTDATA content until ENDDATA
                currentIndex++;
                bool inPrintDataBlock = true;
                bool inNestedDatalist = false;
                DatalistNode? nestedDatalist = null;

                while (currentIndex < lines.Length && inPrintDataBlock)
                {
                    lineNumber = currentIndex + 1;
                    string printDataLine = lines[currentIndex].Trim();

                    if (string.IsNullOrWhiteSpace(printDataLine) || printDataLine.StartsWith(";"))
                    {
                        currentIndex++;
                        continue;
                    }

                    if (printDataLine.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
                    {
                        inPrintDataBlock = false;
                        break;
                    }

                    // Handle DATALIST inside PRINTDATA
                    if (printDataLine.Equals("DATALIST", StringComparison.OrdinalIgnoreCase))
                    {
                        inNestedDatalist = true;
                        nestedDatalist = new DatalistNode
                        {
                            LineNumber = lineNumber,
                            SourceFile = fileName
                        };
                        currentIndex++;
                        continue;
                    }

                    // Handle ENDLIST
                    if (printDataLine.Equals("ENDLIST", StringComparison.OrdinalIgnoreCase))
                    {
                        if (inNestedDatalist && nestedDatalist != null)
                        {
                            printDataNode.Content.Add(nestedDatalist);
                            inNestedDatalist = false;
                            nestedDatalist = null;
                        }
                        currentIndex++;
                        continue;
                    }

                    // Handle DATAFORM
                    if (printDataLine.StartsWith("DATAFORM", StringComparison.OrdinalIgnoreCase))
                    {
                        var dataform = ParseDataform(printDataLine, fileName, lineNumber);
                        if (inNestedDatalist && nestedDatalist != null)
                        {
                            nestedDatalist.DataForms.Add(dataform);
                        }
                        else
                        {
                            printDataNode.Content.Add(dataform);
                        }
                        currentIndex++;
                        continue;
                    }

                    currentIndex++;
                }

                elseBranch.Body.Add(printDataNode);
                currentIndex++;
                continue;
            }

            // Check for STRDATA inside ELSE
            if (line.StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase))
            {
                // Skip STRDATA content until ENDDATA
                currentIndex++;
                while (currentIndex < lines.Length)
                {
                    lineNumber = currentIndex + 1;
                    string strDataLine = lines[currentIndex].Trim();

                    if (string.IsNullOrWhiteSpace(strDataLine) || strDataLine.StartsWith(";"))
                    {
                        currentIndex++;
                        continue;
                    }

                    if (strDataLine.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
                    {
                        break; // STRDATA block complete
                    }

                    // Skip STRDATA content (not added to AST)
                    currentIndex++;
                }
                currentIndex++;
                continue;
            }

            // Check for PRINTFORM/PRINTFORML
            if (line.StartsWith("PRINTFORM", StringComparison.OrdinalIgnoreCase))
            {
                int spaceIndex = line.IndexOf(' ');
                string content = spaceIndex > 0 ? line.Substring(spaceIndex + 1).Trim() : "";

                var printNode = new PrintformNode
                {
                    LineNumber = lineNumber,
                    SourceFile = fileName,
                    Content = content,
                    Variant = line.Split()[0]
                };
                elseBranch.Body.Add(printNode);
                currentIndex++;
                continue;
            }

            // LOCAL assignment recognition (F761)
            var localAssignMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(LOCAL(?::\d+)?)\s*=\s*(.+)$");
            if (localAssignMatch.Success)
            {
                elseBranch.Body.Add(new AssignmentNode
                {
                    Target = localAssignMatch.Groups[1].Value,
                    Value = localAssignMatch.Groups[2].Value.Trim(),
                    LineNumber = lineNumber,
                    SourceFile = fileName
                });
                currentIndex++;
                continue;
            }

            // Check for SELECTCASE inside ELSE
            if (line.StartsWith("SELECTCASE", StringComparison.OrdinalIgnoreCase))
            {
                var selectCaseNode = ParseSelectCaseBlock(lines, ref currentIndex, fileName);
                elseBranch.Body.Add(selectCaseNode);
                currentIndex++;
                continue;
            }

            // Check for RETURN inside ELSE
            if (line.StartsWith("RETURN", StringComparison.OrdinalIgnoreCase))
            {
                string value = "";
                int spaceIndex = line.IndexOf(' ');
                if (spaceIndex > 0 && spaceIndex + 1 < line.Length)
                {
                    value = line.Substring(spaceIndex + 1).Trim();
                }

                elseBranch.Body.Add(new ReturnNode
                {
                    Value = value,
                    LineNumber = lineNumber,
                    SourceFile = fileName
                });
                currentIndex++;
                continue;
            }

            // Check for CALL/CALLF inside ELSE
            if (line.StartsWith("CALL ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALLF ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALL\t", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("CALLF\t", StringComparison.OrdinalIgnoreCase))
            {
                var isCallF = line.StartsWith("CALLF", StringComparison.OrdinalIgnoreCase);
                var spaceIndex = line.IndexOfAny(new[] { ' ', '\t' });
                var funcName = spaceIndex > 0 ? line.Substring(spaceIndex + 1).Trim() : "";

                elseBranch.Body.Add(new CallNode
                {
                    FunctionName = funcName,
                    IsCallF = isCallF,
                    LineNumber = lineNumber,
                    SourceFile = fileName
                });
                currentIndex++;
                continue;
            }

            // Skip other statements
            currentIndex++;
        }

        return elseBranch;
    }

    private SelectCaseNode ParseSelectCaseBlock(string[] lines, ref int currentIndex, string fileName)
    {
        // 1. Extract subject from "SELECTCASE ARG" (trim "SELECTCASE " prefix)
        var line = lines[currentIndex].Trim();
        var subject = line.Substring("SELECTCASE".Length).Trim();

        var node = new SelectCaseNode
        {
            Subject = subject,
            LineNumber = currentIndex + 1,
            SourceFile = fileName
        };

        currentIndex++;
        CaseBranch? currentBranch = null;
        bool inCaseElse = false;
        List<AstNode>? caseElseBody = null;

        // 2. Loop through lines parsing CASE/CASEELSE/ENDSELECT
        while (currentIndex < lines.Length)
        {
            int lineNumber = currentIndex + 1;
            line = lines[currentIndex].Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
            {
                currentIndex++;
                continue;
            }

            // ENDSELECT - return completed node
            if (line.Equals("ENDSELECT", StringComparison.OrdinalIgnoreCase))
            {
                if (caseElseBody != null)
                    node.CaseElse = caseElseBody;
                return node;
            }

            // CASEELSE
            if (line.Equals("CASEELSE", StringComparison.OrdinalIgnoreCase))
            {
                currentBranch = null;
                inCaseElse = true;
                caseElseBody = new List<AstNode>();
                currentIndex++;
                continue;
            }

            // CASE values
            if (line.StartsWith("CASE ", StringComparison.OrdinalIgnoreCase) || line.StartsWith("CASE\t", StringComparison.OrdinalIgnoreCase))
            {
                var values = line.Substring(5).Split(',').Select(v => v.Trim()).ToList();
                currentBranch = new CaseBranch();
                currentBranch.Values.AddRange(values);
                node.Branches.Add(currentBranch);
                inCaseElse = false;
                currentIndex++;
                continue;
            }

            // Body statements
            var targetBody = inCaseElse ? caseElseBody : currentBranch?.Body;
            if (targetBody != null)
            {
                // Handle nested IF blocks
                if (line.StartsWith("IF ", StringComparison.OrdinalIgnoreCase) || line.StartsWith("IF\t", StringComparison.OrdinalIgnoreCase))
                {
                    var ifNode = ParseIfBlock(lines, ref currentIndex, fileName, false);
                    targetBody.Add(ifNode);
                    currentIndex++;
                    continue;
                }

                // Handle PRINTFORML and PRINTFORM
                if (line.StartsWith("PRINTFORM", StringComparison.OrdinalIgnoreCase))
                {
                    int spaceIndex = line.IndexOf(' ');
                    string content = spaceIndex > 0 ? line.Substring(spaceIndex + 1).Trim() : "";
                    targetBody.Add(new PrintformNode
                    {
                        Content = content,
                        LineNumber = lineNumber,
                        SourceFile = fileName,
                        Variant = line.Split()[0]
                    });
                    currentIndex++;
                    continue;
                }

                // Handle DATAFORM
                if (line.StartsWith("DATAFORM", StringComparison.OrdinalIgnoreCase))
                {
                    var dataform = ParseDataform(line, fileName, lineNumber);
                    targetBody.Add(dataform);
                    currentIndex++;
                    continue;
                }

                // Handle LOCAL assignment
                var localAssignMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(LOCAL(?::\d+)?)\s*=\s*(.+)$");
                if (localAssignMatch.Success)
                {
                    targetBody.Add(new AssignmentNode
                    {
                        Target = localAssignMatch.Groups[1].Value,
                        Value = localAssignMatch.Groups[2].Value.Trim(),
                        LineNumber = lineNumber,
                        SourceFile = fileName
                    });
                    currentIndex++;
                    continue;
                }

                // Handle CALL/CALLF statements
                if (line.StartsWith("CALL ", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("CALLF ", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("CALL\t", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("CALLF\t", StringComparison.OrdinalIgnoreCase))
                {
                    var isCallF = line.StartsWith("CALLF", StringComparison.OrdinalIgnoreCase);
                    var spaceIndex = line.IndexOfAny(new[] { ' ', '\t' });
                    var funcName = spaceIndex > 0 ? line.Substring(spaceIndex + 1).Trim() : "";

                    targetBody.Add(new CallNode
                    {
                        FunctionName = funcName,
                        IsCallF = isCallF,
                        LineNumber = lineNumber,
                        SourceFile = fileName
                    });
                    currentIndex++;
                    continue;
                }
            }

            currentIndex++;
        }

        // 3. EOF before ENDSELECT
        throw new ParseException("SELECTCASE without matching ENDSELECT", fileName, currentIndex);
    }
}

/// <summary>
/// Exception thrown when ERB parsing fails
/// </summary>
public class ParseException : Exception
{
    public string FileName { get; }
    public int LineNumber { get; }

    public ParseException(string message, string fileName, int lineNumber)
        : base($"{fileName}:{lineNumber}: {message}")
    {
        FileName = fileName;
        LineNumber = lineNumber;
    }

    public ParseException(string message) : base(message)
    {
        FileName = string.Empty;
        LineNumber = 0;
    }
}
