namespace ErbLinter.Parser;

/// <summary>
/// Tokenizes ERB script lines into tokens
/// </summary>
public class Tokenizer
{
    private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "IF", TokenType.If },
        { "ELSE", TokenType.Else },
        { "ELSEIF", TokenType.ElseIf },
        { "ENDIF", TokenType.EndIf },
        { "SIF", TokenType.Sif },
        { "FOR", TokenType.For },
        { "NEXT", TokenType.Next },
        { "WHILE", TokenType.While },
        { "WEND", TokenType.Wend },
        { "REPEAT", TokenType.Repeat },
        { "REND", TokenType.Rend },
        { "DO", TokenType.Do },
        { "LOOP", TokenType.Loop },
        { "SELECTCASE", TokenType.SelectCase },
        { "CASE", TokenType.Case },
        { "CASEELSE", TokenType.CaseElse },
        { "ENDSELECT", TokenType.EndSelect },
        { "CALL", TokenType.Call },
        { "TRYCALL", TokenType.TryCall },
        { "JUMP", TokenType.Jump },
        { "GOTO", TokenType.Goto },
        { "RETURN", TokenType.Return },
        { "PRINT", TokenType.Print },
        { "PRINTL", TokenType.PrintL },
        { "PRINTFORM", TokenType.PrintForm },
        { "PRINTFORML", TokenType.PrintFormL },
        { "PRINTDATA", TokenType.PrintData },
        { "ENDDATA", TokenType.EndData },
        { "DATA", TokenType.Data },
        { "DATAFORM", TokenType.DataForm },
    };

    /// <summary>
    /// Tokenize a single line of ERB code
    /// </summary>
    public IEnumerable<Token> TokenizeLine(string line, int lineNumber)
    {
        var tokens = new List<Token>();
        int pos = 0;

        while (pos < line.Length)
        {
            var c = line[pos];

            // Whitespace
            if (char.IsWhiteSpace(c))
            {
                var start = pos;
                while (pos < line.Length && char.IsWhiteSpace(line[pos]))
                    pos++;
                // Skip whitespace tokens (don't add to list)
                continue;
            }

            // Comment (everything after ;)
            if (c == ';')
            {
                tokens.Add(new Token(TokenType.Comment, line.Substring(pos), lineNumber, pos + 1, line.Length - pos));
                break;
            }

            // String literal
            if (c == '"')
            {
                var start = pos;
                pos++; // Skip opening quote
                while (pos < line.Length && line[pos] != '"')
                    pos++;
                if (pos < line.Length)
                    pos++; // Skip closing quote
                tokens.Add(new Token(TokenType.String, line.Substring(start, pos - start), lineNumber, start + 1, pos - start));
                continue;
            }

            // Number
            if (char.IsDigit(c) || (c == '-' && pos + 1 < line.Length && char.IsDigit(line[pos + 1])))
            {
                var start = pos;
                if (c == '-') pos++;

                // Check for hex
                if (pos + 1 < line.Length && line[pos] == '0' && (line[pos + 1] == 'x' || line[pos + 1] == 'X'))
                {
                    pos += 2;
                    while (pos < line.Length && IsHexDigit(line[pos]))
                        pos++;
                }
                else
                {
                    while (pos < line.Length && char.IsDigit(line[pos]))
                        pos++;
                }
                tokens.Add(new Token(TokenType.Number, line.Substring(start, pos - start), lineNumber, start + 1, pos - start));
                continue;
            }

            // Identifier or keyword
            if (char.IsLetter(c) || c == '_')
            {
                var start = pos;
                while (pos < line.Length && (char.IsLetterOrDigit(line[pos]) || line[pos] == '_'))
                    pos++;
                var value = line.Substring(start, pos - start);
                var type = Keywords.TryGetValue(value, out var kwType) ? kwType : TokenType.Identifier;
                tokens.Add(new Token(type, value, lineNumber, start + 1, pos - start));
                continue;
            }

            // Operators and punctuation
            var (tokenType, length) = GetOperatorOrPunctuation(line, pos);
            tokens.Add(new Token(tokenType, line.Substring(pos, length), lineNumber, pos + 1, length));
            pos += length;
        }

        return tokens;
    }

    private static (TokenType Type, int Length) GetOperatorOrPunctuation(string line, int pos)
    {
        var c = line[pos];
        var next = pos + 1 < line.Length ? line[pos + 1] : '\0';

        return (c, next) switch
        {
            ('=', '=') => (TokenType.Equal, 2),
            ('!', '=') => (TokenType.NotEqual, 2),
            ('<', '=') => (TokenType.LessEqual, 2),
            ('>', '=') => (TokenType.GreaterEqual, 2),
            ('&', '&') => (TokenType.And, 2),
            ('|', '|') => (TokenType.Or, 2),
            ('+', _) => (TokenType.Plus, 1),
            ('-', _) => (TokenType.Minus, 1),
            ('*', _) => (TokenType.Multiply, 1),
            ('/', _) => (TokenType.Divide, 1),
            ('%', _) => (TokenType.Percent, 1),
            ('=', _) => (TokenType.Assign, 1),
            ('<', _) => (TokenType.Less, 1),
            ('>', _) => (TokenType.Greater, 1),
            ('!', _) => (TokenType.Not, 1),
            ('&', _) => (TokenType.BitAnd, 1),
            ('|', _) => (TokenType.BitOr, 1),
            ('(', _) => (TokenType.OpenParen, 1),
            (')', _) => (TokenType.CloseParen, 1),
            ('{', _) => (TokenType.OpenBrace, 1),
            ('}', _) => (TokenType.CloseBrace, 1),
            ('[', _) => (TokenType.OpenBracket, 1),
            (']', _) => (TokenType.CloseBracket, 1),
            (',', _) => (TokenType.Comma, 1),
            (':', _) => (TokenType.Colon, 1),
            (';', _) => (TokenType.Semicolon, 1),
            ('@', _) => (TokenType.At, 1),
            ('#', _) => (TokenType.Hash, 1),
            _ => (TokenType.Unknown, 1)
        };
    }

    private static bool IsHexDigit(char c)
    {
        return char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    }
}
