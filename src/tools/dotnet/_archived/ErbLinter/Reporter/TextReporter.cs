namespace ErbLinter.Reporter;

/// <summary>
/// Text format reporter for console output
/// </summary>
public class TextReporter
{
    private readonly bool _useColor;

    public TextReporter(bool useColor = true)
    {
        _useColor = useColor && !Console.IsOutputRedirected;
    }

    /// <summary>
    /// Report issues to console
    /// </summary>
    public void Report(IEnumerable<Issue> issues, TextWriter output)
    {
        var issueList = issues.ToList();

        foreach (var issue in issueList)
        {
            WriteIssue(issue, output);
        }

        WriteSummary(issueList, output);
    }

    private void WriteIssue(Issue issue, TextWriter output)
    {
        if (_useColor)
        {
            var color = issue.Level switch
            {
                IssueLevel.Error => ConsoleColor.Red,
                IssueLevel.Warning => ConsoleColor.Yellow,
                IssueLevel.Info => ConsoleColor.Cyan,
                _ => ConsoleColor.White
            };

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            output.WriteLine(issue.ToString());
            Console.ForegroundColor = originalColor;
        }
        else
        {
            output.WriteLine(issue.ToString());
        }
    }

    private void WriteSummary(List<Issue> issues, TextWriter output)
    {
        output.WriteLine();
        output.WriteLine("Summary:");
        output.WriteLine($"  Errors:   {issues.Count(i => i.Level == IssueLevel.Error)}");
        output.WriteLine($"  Warnings: {issues.Count(i => i.Level == IssueLevel.Warning)}");
        output.WriteLine($"  Info:     {issues.Count(i => i.Level == IssueLevel.Info)}");
        output.WriteLine($"  Files:    {issues.Select(i => i.FilePath).Distinct().Count()}");
    }
}
