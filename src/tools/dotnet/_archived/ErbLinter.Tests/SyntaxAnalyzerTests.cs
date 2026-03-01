using ErbLinter.Analyzer;
using ErbLinter.Reporter;
using Xunit;

namespace ErbLinter.Tests;

/// <summary>
/// Unit tests for SyntaxAnalyzer block structure validation
/// </summary>
public class SyntaxAnalyzerTests
{
    private readonly SyntaxAnalyzer _analyzer = new();
    private const string TestFile = "test.ERB";

    #region IF/ENDIF Tests

    [Fact]
    public void Analyze_MatchedIfEndif_NoErrors()
    {
        var lines = new[] { "IF X == 1", "  PRINTL Hello", "ENDIF" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_UnmatchedIf_ReportsError()
    {
        var lines = new[] { "IF X == 1", "  PRINTL Hello" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Single(issues.Where(i => i.Level == IssueLevel.Error));
        var error = issues.First(i => i.Level == IssueLevel.Error);
        Assert.Contains("IF", error.Message);
        Assert.Contains("ENDIF", error.Message);
    }

    [Fact]
    public void Analyze_UnmatchedEndif_ReportsError()
    {
        var lines = new[] { "ENDIF" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Single(issues.Where(i => i.Level == IssueLevel.Error));
        var error = issues.First(i => i.Level == IssueLevel.Error);
        Assert.Contains("ENDIF", error.Message);
        Assert.Contains("IF", error.Message);
    }

    [Fact]
    public void Analyze_SIF_NotTracked()
    {
        // SIF is single-line conditional, should not require ENDIF
        var lines = new[] { "SIF X == 1", "  PRINTL Hello" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_IfWithElseif_NoErrors()
    {
        var lines = new[] { "IF X == 1", "  PRINTL One", "ELSEIF X == 2", "  PRINTL Two", "ENDIF" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_IfWithElse_NoErrors()
    {
        var lines = new[] { "IF X == 1", "  PRINTL One", "ELSE", "  PRINTL Other", "ENDIF" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    #endregion

    #region SELECTCASE Tests

    [Fact]
    public void Analyze_MatchedSelectCase_NoErrors()
    {
        var lines = new[] { "SELECTCASE X", "CASE 1", "  PRINTL One", "CASE 2", "  PRINTL Two", "ENDSELECT" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_SelectCaseWithCaseElse_NoErrors()
    {
        var lines = new[] { "SELECTCASE X", "CASE 1", "  PRINTL One", "CASEELSE", "  PRINTL Default", "ENDSELECT" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_UnmatchedSelectCase_ReportsError()
    {
        var lines = new[] { "SELECTCASE X", "CASE 1", "  PRINTL One" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Single(issues.Where(i => i.Level == IssueLevel.Error));
        var error = issues.First(i => i.Level == IssueLevel.Error);
        Assert.Contains("SELECTCASE", error.Message);
        Assert.Contains("ENDSELECT", error.Message);
    }

    #endregion

    #region Loop Tests

    [Fact]
    public void Analyze_MatchedRepeat_NoErrors()
    {
        var lines = new[] { "REPEAT 5", "  PRINTL Loop", "REND" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_MatchedFor_NoErrors()
    {
        var lines = new[] { "FOR I, 0, 10", "  PRINTL %I%", "NEXT" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_MatchedWhile_NoErrors()
    {
        var lines = new[] { "WHILE X < 10", "  X += 1", "WEND" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_MatchedDoLoop_NoErrors()
    {
        var lines = new[] { "DO", "  X += 1", "LOOP" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_UnmatchedRepeat_ReportsError()
    {
        var lines = new[] { "REPEAT 5", "  PRINTL Loop" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Single(issues.Where(i => i.Level == IssueLevel.Error));
        var error = issues.First(i => i.Level == IssueLevel.Error);
        Assert.Contains("REPEAT", error.Message);
        Assert.Contains("REND", error.Message);
    }

    #endregion

    #region Mismatch Tests

    [Fact]
    public void Analyze_MismatchedBlocks_ReportsError()
    {
        var lines = new[] { "IF X == 1", "  PRINTL Hello", "ENDSELECT" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.NotEmpty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_NestedBlocks_NoErrors()
    {
        var lines = new[]
        {
            "IF X == 1",
            "  FOR I, 0, 5",
            "    PRINTL %I%",
            "  NEXT",
            "ENDIF"
        };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_DeeplyNestedBlocks_NoErrors()
    {
        var lines = new[]
        {
            "IF A",
            "  IF B",
            "    IF C",
            "      SELECTCASE X",
            "      CASE 1",
            "        REPEAT 3",
            "          PRINTL Deep",
            "        REND",
            "      ENDSELECT",
            "    ENDIF",
            "  ENDIF",
            "ENDIF"
        };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    #endregion

    #region Parentheses Tests

    [Fact]
    public void Analyze_BalancedParentheses_NoErrors()
    {
        var lines = new[] { "IF (X == 1)", "ENDIF" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_UnclosedParenthesis_ReportsError()
    {
        var lines = new[] { "IF (X == 1", "ENDIF" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Single(issues.Where(i => i.Level == IssueLevel.Error && i.Code == "ERB003"));
    }

    [Fact]
    public void Analyze_ExtraClosingParenthesis_ReportsError()
    {
        var lines = new[] { "IF X == 1)", "ENDIF" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Single(issues.Where(i => i.Level == IssueLevel.Error && i.Code == "ERB003"));
    }

    #endregion

    #region Special Block Tests

    [Fact]
    public void Analyze_TryCatch_NoErrors()
    {
        var lines = new[] { "TRYCCALL SOMEFUNC", "CATCH", "  PRINTL Error", "ENDCATCH" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_PrintData_NoErrors()
    {
        var lines = new[] { "PRINTDATA", "  DATA Hello", "  DATA World", "ENDDATA" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_DataList_NoErrors()
    {
        var lines = new[] { "DATALIST", "  DATA 1", "  DATA 2", "ENDLIST" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_NoSkip_NoErrors()
    {
        var lines = new[] { "NOSKIP", "  PRINTL Something", "ENDNOSKIP" };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    #endregion

    #region Comment and Whitespace Tests

    [Fact]
    public void Analyze_CommentsIgnored()
    {
        var lines = new[]
        {
            "; This is a comment",
            "IF X == 1",
            "  ; Another comment",
            "ENDIF"
        };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    [Fact]
    public void Analyze_EmptyLinesIgnored()
    {
        var lines = new[]
        {
            "",
            "IF X == 1",
            "",
            "  PRINTL Hello",
            "",
            "ENDIF",
            ""
        };

        var issues = _analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
    }

    #endregion

    #region Nesting Depth Warning Tests

    [Fact]
    public void Analyze_DeepNesting_WarnsAtThreshold()
    {
        // Create nesting depth of 11 (exceeds default threshold of 10)
        var lines = new List<string>();
        for (int i = 0; i < 11; i++)
            lines.Add($"IF LEVEL{i}");
        lines.Add("  PRINTL Deep");
        for (int i = 0; i < 11; i++)
            lines.Add("ENDIF");

        var issues = _analyzer.Analyze(TestFile, lines.ToArray()).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
        Assert.Single(issues.Where(i => i.Level == IssueLevel.Warning && i.Code == "ERB004"));
        var warning = issues.First(i => i.Code == "ERB004");
        Assert.Contains("depth 11", warning.Message);
    }

    [Fact]
    public void Analyze_NestingAtThreshold_NoWarning()
    {
        // Create nesting depth of exactly 10 (at threshold, not exceeding)
        var lines = new List<string>();
        for (int i = 0; i < 10; i++)
            lines.Add($"IF LEVEL{i}");
        lines.Add("  PRINTL Deep");
        for (int i = 0; i < 10; i++)
            lines.Add("ENDIF");

        var issues = _analyzer.Analyze(TestFile, lines.ToArray()).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
        Assert.Empty(issues.Where(i => i.Code == "ERB004"));
    }

    [Fact]
    public void Analyze_CustomThreshold_WarnsAtCustomLevel()
    {
        var analyzer = new SyntaxAnalyzer { NestingThreshold = 3 };
        var lines = new[]
        {
            "IF A",
            "  IF B",
            "    IF C",
            "      IF D", // 4th level - exceeds threshold of 3
            "        PRINTL Deep",
            "      ENDIF",
            "    ENDIF",
            "  ENDIF",
            "ENDIF"
        };

        var issues = analyzer.Analyze(TestFile, lines).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
        Assert.Single(issues.Where(i => i.Code == "ERB004"));
    }

    [Fact]
    public void Analyze_ThresholdDisabled_NoWarning()
    {
        var analyzer = new SyntaxAnalyzer { NestingThreshold = 0 }; // Disabled
        var lines = new List<string>();
        for (int i = 0; i < 20; i++) // Very deep nesting
            lines.Add($"IF LEVEL{i}");
        lines.Add("  PRINTL Deep");
        for (int i = 0; i < 20; i++)
            lines.Add("ENDIF");

        var issues = analyzer.Analyze(TestFile, lines.ToArray()).ToList();

        Assert.Empty(issues.Where(i => i.Level == IssueLevel.Error));
        Assert.Empty(issues.Where(i => i.Code == "ERB004"));
    }

    [Fact]
    public void Analyze_DeeperNesting_OnlyOneWarning()
    {
        // Verify we only get one warning even if depth increases further
        var lines = new List<string>();
        for (int i = 0; i < 15; i++) // Goes to depth 15
            lines.Add($"IF LEVEL{i}");
        lines.Add("  PRINTL Deep");
        for (int i = 0; i < 15; i++)
            lines.Add("ENDIF");

        var issues = _analyzer.Analyze(TestFile, lines.ToArray()).ToList();

        // Should get warnings at depth 11, 12, 13, 14, 15 but each only once
        var warnings = issues.Where(i => i.Code == "ERB004").ToList();
        Assert.Equal(5, warnings.Count);
    }

    #endregion
}
