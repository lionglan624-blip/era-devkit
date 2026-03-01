namespace ErbLinter.Reporter;

/// <summary>
/// Issue severity level
/// </summary>
public enum IssueLevel
{
    Error,
    Warning,
    Info
}

/// <summary>
/// Represents a single linter issue
/// </summary>
public record Issue(
    string FilePath,
    int Line,
    int Column,
    IssueLevel Level,
    string Code,
    string Message
)
{
    public override string ToString()
    {
        var levelStr = Level switch
        {
            IssueLevel.Error => "error",
            IssueLevel.Warning => "warning",
            IssueLevel.Info => "info",
            _ => "unknown"
        };
        return $"{FilePath}:{Line}:{Column}: {levelStr}: [{Code}] {Message}";
    }
}
