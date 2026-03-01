using Xunit;

namespace EntriesFormatMigrator.Tests;

public class DiffReporterTests
{
    [Fact]
    public void LogDryRunChange_WithSingleChange_OutputsExpectedFormat()
    {
        // Arrange
        var reporter = new DiffReporter();
        var changes = new List<string> { "Renamed id: fallback -> id: talent_16_0 at line 5" };
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act
            reporter.LogDryRunChange("test/file1.yaml", changes);

            // Assert
            var result = output.ToString();
            Assert.Contains("[DRY-RUN] Would modify: test/file1.yaml", result);
            Assert.Contains("- Renamed id: fallback -> id: talent_16_0 at line 5", result);
        }
        finally
        {
            // Restore standard output
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
        }
    }

    [Fact]
    public void LogDryRunChange_WithMultipleChanges_OutputsAllChanges()
    {
        // Arrange
        var reporter = new DiffReporter();
        var changes = new List<string>
        {
            "Renamed id: fallback -> id: talent_16_0 at line 5",
            "Inserted condition block after line 7",
            "Renamed id: fallback -> id: talent_17_0 at line 15"
        };
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act
            reporter.LogDryRunChange("test/file2.yaml", changes);

            // Assert
            var result = output.ToString();
            Assert.Contains("[DRY-RUN] Would modify: test/file2.yaml", result);
            Assert.Contains("- Renamed id: fallback -> id: talent_16_0 at line 5", result);
            Assert.Contains("- Inserted condition block after line 7", result);
            Assert.Contains("- Renamed id: fallback -> id: talent_17_0 at line 15", result);
        }
        finally
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
        }
    }

    [Fact]
    public void LogDryRunChange_WithEmptyChanges_OutputsFilePathOnly()
    {
        // Arrange
        var reporter = new DiffReporter();
        var changes = new List<string>();
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act
            reporter.LogDryRunChange("test/empty.yaml", changes);

            // Assert
            var result = output.ToString();
            Assert.Contains("[DRY-RUN] Would modify: test/empty.yaml", result);
        }
        finally
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
        }
    }

    [Fact]
    public void LogSummary_DryRunMode_OutputsDryRunSummary()
    {
        // Arrange
        var reporter = new DiffReporter();
        var summary = new MigrationSummary(5, 2, 0, new List<string>());
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act
            reporter.LogSummary(summary, dryRun: true);

            // Assert
            var result = output.ToString();
            Assert.Contains("DRY-RUN SUMMARY", result);
            Assert.Contains("Modified: 5", result);
            Assert.Contains("Skipped:  2", result);
            Assert.Contains("Failed:   0", result);
            Assert.Contains("====", result); // Check for separator lines
        }
        finally
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
        }
    }

    [Fact]
    public void LogSummary_LiveMode_OutputsMigrationSummary()
    {
        // Arrange
        var reporter = new DiffReporter();
        var summary = new MigrationSummary(3, 1, 0, new List<string>());
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act
            reporter.LogSummary(summary, dryRun: false);

            // Assert
            var result = output.ToString();
            Assert.Contains("MIGRATION SUMMARY", result);
            Assert.DoesNotContain("DRY-RUN", result);
            Assert.Contains("Modified: 3", result);
            Assert.Contains("Skipped:  1", result);
            Assert.Contains("Failed:   0", result);
        }
        finally
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
        }
    }

    [Fact]
    public void LogSummary_WithFailures_OutputsFailedFilesList()
    {
        // Arrange
        var reporter = new DiffReporter();
        var failedFiles = new List<string>
        {
            "test/file1.yaml: Invalid YAML structure",
            "test/file2.yaml: File not found"
        };
        var summary = new MigrationSummary(0, 0, 2, failedFiles);
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act
            reporter.LogSummary(summary, dryRun: false);

            // Assert
            var result = output.ToString();
            Assert.Contains("Failed:   2", result);
            Assert.Contains("Failed files:", result);
            Assert.Contains("- test/file1.yaml: Invalid YAML structure", result);
            Assert.Contains("- test/file2.yaml: File not found", result);
        }
        finally
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
        }
    }

    [Fact]
    public void LogSummary_WithNoFailures_DoesNotOutputFailedFilesList()
    {
        // Arrange
        var reporter = new DiffReporter();
        var summary = new MigrationSummary(5, 2, 0, new List<string>());
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act
            reporter.LogSummary(summary, dryRun: false);

            // Assert
            var result = output.ToString();
            Assert.DoesNotContain("Failed files:", result);
        }
        finally
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
        }
    }

    [Fact]
    public void LogSummary_WithZeroCounts_OutputsZeros()
    {
        // Arrange
        var reporter = new DiffReporter();
        var summary = new MigrationSummary(0, 0, 0, new List<string>());
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act
            reporter.LogSummary(summary, dryRun: false);

            // Assert
            var result = output.ToString();
            Assert.Contains("Modified: 0", result);
            Assert.Contains("Skipped:  0", result);
            Assert.Contains("Failed:   0", result);
        }
        finally
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
        }
    }
}
