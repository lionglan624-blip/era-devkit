using System.Text;

namespace ErbLinter.Data;

/// <summary>
/// Loads CSV definition files (FLAG, CFLAG, Abl, Talent, etc.)
/// </summary>
public class CsvLoader
{
    /// <summary>
    /// Load a CSV file and return index-to-name mappings
    /// Format: index,name (lines starting with ; are comments)
    /// </summary>
    public Dictionary<int, string> LoadCsv(string path)
    {
        var result = new Dictionary<int, string>();

        if (!File.Exists(path))
            return result;

        // Register code pages for Shift-JIS support
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        string[] lines;
        try
        {
            var shiftJis = Encoding.GetEncoding(932);
            lines = File.ReadAllLines(path, shiftJis);
        }
        catch
        {
            // Fallback to UTF-8
            lines = File.ReadAllLines(path, Encoding.UTF8);
        }

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(";"))
                continue;

            // Parse index,name format
            var commaIndex = trimmed.IndexOf(',');
            if (commaIndex <= 0)
                continue;

            var indexPart = trimmed.Substring(0, commaIndex).Trim();
            var namePart = trimmed.Substring(commaIndex + 1).Trim();

            if (int.TryParse(indexPart, out var index) && !string.IsNullOrEmpty(namePart))
            {
                result[index] = namePart;
            }
        }

        return result;
    }
}
