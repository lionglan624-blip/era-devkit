using System.Text.RegularExpressions;

namespace KojoComparer;

/// <summary>
/// Normalizes output text for comparison.
/// Removes formatting variance while preserving semantic content.
/// </summary>
public class OutputNormalizer
{
    // YAML format: %CALLNAME:X%
    private static readonly Regex YamlCallnamePattern = new Regex(
        @"%CALLNAME:(?<target>[^%]+)%",
        RegexOptions.Compiled
    );

    // ERB format: <CALLNAME:X>
    private static readonly Regex ErbCallnamePattern = new Regex(
        @"<CALLNAME:(?<target>[^>]+)>",
        RegexOptions.Compiled
    );

    // Known character names that should be normalized to <CALLNAME:CHAR>
    // This handles ERB output where ERA engine has already substituted %CALLNAME:人物_X% with actual names
    private static readonly string[] KnownCharacterNames = new[]
    {
        "美鈴", "咲夜", "パチュリー", "小悪魔", "フラン", "レミリア",
        "魔理沙", "チルノ", "大妖精", "子悪魔"
    };

    // Known master/player names that should be normalized to <CALLNAME:MASTER>
    // This handles ERB output where ERA engine has already substituted %CALLNAME:MASTER% with actual player name
    private static readonly string[] KnownMasterNames = new[]
    {
        "あなた"
    };
    /// <summary>
    /// Normalizes output text by:
    /// 1. Trimming leading/trailing whitespace per line
    /// 2. Removing empty lines
    /// 3. Normalizing fullwidth/halfwidth spaces
    /// 4. Removing color codes
    /// 5. Normalizing line endings (CRLF -> LF)
    /// 6. Removing DATAFORM prefix artifacts
    /// 7. Normalizing CALLNAME patterns
    /// </summary>
    /// <param name="rawOutput">Raw output text</param>
    /// <returns>Normalized text</returns>
    public string Normalize(string rawOutput)
    {
        if (string.IsNullOrEmpty(rawOutput))
            return string.Empty;

        var text = rawOutput;

        // 1. Normalize line endings (CRLF -> LF)
        text = text.Replace("\r\n", "\n");

        // 2. Remove DATAFORM prefix (ERB output artifact)
        text = Regex.Replace(text, @"^DATAFORM\s*", "", RegexOptions.Multiline);

        // 3. Remove DRAWLINE output (ERB separator lines)
        text = Regex.Replace(text, @"^-{20,}$", "", RegexOptions.Multiline);

        // 4. Remove TRAIN_MESSAGE boilerplate text (ERB only, not in YAML)
        // These are template messages that appear before kojo output
        text = Regex.Replace(text, @"^.+とつながったまま.+を丹念に愛撫した…$", "", RegexOptions.Multiline);
        text = Regex.Replace(text, @"^.+はくわえ込んだペニスを締め付けながら.+反応している…$", "", RegexOptions.Multiline);

        // 5. Remove conversation intro lines (COM_300 generates these outside of YAML scope)
        text = Regex.Replace(text, @"^.+は.+と.{0,10}話をした$", "", RegexOptions.Multiline);

        // 6. Remove color codes (e.g., [COLOR 0xFF0000])
        text = Regex.Replace(text, @"\[COLOR\s+0x[0-9A-Fa-f]+\]", "");

        // 7. Normalize fullwidth/halfwidth spaces
        // Convert fullwidth space (U+3000) to halfwidth space (U+0020)
        text = text.Replace('\u3000', ' ');

        // 8. Normalize CALLNAME patterns
        text = NormalizeCallname(text);

        // 9. Split PRINTFORMW concatenation: PRINTFORMW (no newline) followed by
        //    PRINTDATA content creates 。「 on a single line. Split into separate lines.
        //    Safe for both sides: if YAML also has 。「 in one entry, it gets split identically.
        text = text.Replace("。「", "。\n「");

        // 10. Trim leading/trailing whitespace per line
        var lines = text.Split('\n');
        var trimmedLines = lines.Select(line => line.Trim()).ToArray();

        // 10. Remove empty lines (consecutive newlines -> single newline)
        var nonEmptyLines = trimmedLines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

        // Join lines with newline
        return string.Join("\n", nonEmptyLines);
    }

    private string NormalizeCallname(string text)
    {
        var result = text;

        // First, normalize YAML-style %CALLNAME:...% patterns (from YAML output)
        result = YamlCallnamePattern.Replace(result, match =>
        {
            var target = match.Groups["target"].Value;
            // Normalize different target types to consistent placeholders
            if (target == "MASTER")
                return "<CALLNAME:MASTER>";
            else if (target == "TARGET" || target.StartsWith("人物_"))
                return "<CALLNAME:CHAR>"; // TARGET refers to the dialogue target character
            else
                return $"<CALLNAME:{target}>"; // Fallback for unknown patterns
        });

        // Then, normalize ERB-style <CALLNAME:...> patterns (from ERB output)
        result = ErbCallnamePattern.Replace(result, match =>
        {
            var target = match.Groups["target"].Value;
            // Normalize different target types to consistent placeholders
            if (target == "MASTER")
                return "<CALLNAME:MASTER>";
            else if (target == "TARGET" || target.StartsWith("人物_"))
                return "<CALLNAME:CHAR>"; // TARGET refers to the dialogue target character
            else
                return $"<CALLNAME:{target}>"; // Fallback for unknown patterns
        });

        // Then, normalize known character names (from ERB output where ERA engine
        // has already substituted %CALLNAME:人物_X% with actual names)
        foreach (var name in KnownCharacterNames)
        {
            result = result.Replace(name, "<CALLNAME:CHAR>");
        }

        // Finally, normalize known master/player names (from ERB output where ERA engine
        // has already substituted %CALLNAME:MASTER% with actual player name)
        foreach (var name in KnownMasterNames)
        {
            result = result.Replace(name, "<CALLNAME:MASTER>");
        }

        return result;
    }
}
