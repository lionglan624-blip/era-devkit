using System.Text;

namespace ErbLinter.Data;

/// <summary>
/// Scans directories for ERB files
/// </summary>
public class FileScanner
{
    private static readonly string[] ErbExtensions = { ".erb", ".erh" };

    /// <summary>
    /// Scan a path for ERB files
    /// </summary>
    /// <param name="path">File or directory path</param>
    /// <returns>List of ERB file paths</returns>
    public IEnumerable<string> Scan(string path)
    {
        if (File.Exists(path))
        {
            // Single file
            if (IsErbFile(path))
            {
                yield return Path.GetFullPath(path);
            }
            yield break;
        }

        if (Directory.Exists(path))
        {
            // Recursive directory scan
            foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {
                if (IsErbFile(file))
                {
                    yield return Path.GetFullPath(file);
                }
            }
            yield break;
        }

        throw new DirectoryNotFoundException($"Path not found: {path}");
    }

    /// <summary>
    /// Check if a file is an ERB file
    /// </summary>
    private static bool IsErbFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ErbExtensions.Contains(ext);
    }

    /// <summary>
    /// Read file content with Shift-JIS encoding support
    /// </summary>
    /// <param name="path">File path</param>
    /// <returns>File lines</returns>
    public string[] ReadFileLines(string path)
    {
        // Register code pages for Shift-JIS support
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Try Shift-JIS first (common for ERA games)
        try
        {
            var shiftJis = Encoding.GetEncoding(932);
            return File.ReadAllLines(path, shiftJis);
        }
        catch
        {
            // Fallback to UTF-8
            return File.ReadAllLines(path, Encoding.UTF8);
        }
    }
}
