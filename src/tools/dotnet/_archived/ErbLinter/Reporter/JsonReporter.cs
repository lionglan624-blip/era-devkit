using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErbLinter.Reporter;

/// <summary>
/// JSON format reporter for machine-readable output
/// </summary>
public class JsonReporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Report issues as JSON
    /// </summary>
    public void Report(IEnumerable<Issue> issues, TextWriter output)
    {
        var issueList = issues.ToList();

        var report = new JsonReport
        {
            Summary = new Summary
            {
                Errors = issueList.Count(i => i.Level == IssueLevel.Error),
                Warnings = issueList.Count(i => i.Level == IssueLevel.Warning),
                Info = issueList.Count(i => i.Level == IssueLevel.Info),
                FilesScanned = issueList.Select(i => i.FilePath).Distinct().Count()
            },
            Issues = issueList.Select(i => new JsonIssue
            {
                File = i.FilePath,
                Line = i.Line,
                Column = i.Column,
                Level = i.Level.ToString().ToLower(),
                Code = i.Code,
                Message = i.Message
            }).ToList()
        };

        var json = JsonSerializer.Serialize(report, JsonOptions);
        output.WriteLine(json);
    }

    private class JsonReport
    {
        public Summary Summary { get; set; } = new();
        public List<JsonIssue> Issues { get; set; } = new();
    }

    private class Summary
    {
        public int Errors { get; set; }
        public int Warnings { get; set; }
        public int Info { get; set; }
        public int FilesScanned { get; set; }
    }

    private class JsonIssue
    {
        public string File { get; set; } = "";
        public int Line { get; set; }
        public int Column { get; set; }
        public string Level { get; set; } = "";
        public string Code { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
