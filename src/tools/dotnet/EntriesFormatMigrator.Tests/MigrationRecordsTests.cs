using Xunit;

namespace EntriesFormatMigrator.Tests;

public class MigrationRecordsTests
{
    [Fact]
    public void MigrationResult_Construction_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var changes = new List<string> { "Change 1", "Change 2" };
        var result = new MigrationResult(
            Modified: true,
            EntriesUpdated: 2,
            Changes: changes
        );

        // Assert
        Assert.True(result.Modified);
        Assert.Equal(2, result.EntriesUpdated);
        Assert.Equal(2, result.Changes.Count);
        Assert.Contains("Change 1", result.Changes);
        Assert.Contains("Change 2", result.Changes);
    }

    [Fact]
    public void MigrationResult_WithNoModifications_CreatesCorrectly()
    {
        // Arrange & Act
        var result = new MigrationResult(
            Modified: false,
            EntriesUpdated: 0,
            Changes: new List<string>()
        );

        // Assert
        Assert.False(result.Modified);
        Assert.Equal(0, result.EntriesUpdated);
        Assert.Empty(result.Changes);
    }

    [Fact]
    public void MigrationResult_RecordEquality_ComparesCorrectly()
    {
        // Arrange
        var changes1 = new List<string> { "Change A" };
        var changes2 = new List<string> { "Change A" };

        var result1 = new MigrationResult(true, 1, changes1);
        var result2 = new MigrationResult(true, 1, changes2);
        var result3 = new MigrationResult(false, 0, new List<string>());

        // Act & Assert
        // Note: Record equality compares by value for value types and reference for reference types
        Assert.Equal(result1.Modified, result2.Modified);
        Assert.Equal(result1.EntriesUpdated, result2.EntriesUpdated);
        Assert.NotEqual(result1, result3);
    }

    [Fact]
    public void MigrationSummary_Construction_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var failedFiles = new List<string> { "file1.yaml", "file2.yaml" };
        var summary = new MigrationSummary(
            Modified: 10,
            Skipped: 5,
            Failed: 2,
            FailedFiles: failedFiles
        );

        // Assert
        Assert.Equal(10, summary.Modified);
        Assert.Equal(5, summary.Skipped);
        Assert.Equal(2, summary.Failed);
        Assert.Equal(2, summary.FailedFiles.Count);
        Assert.Contains("file1.yaml", summary.FailedFiles);
        Assert.Contains("file2.yaml", summary.FailedFiles);
    }

    [Fact]
    public void MigrationSummary_WithNoFailures_CreatesCorrectly()
    {
        // Arrange & Act
        var summary = new MigrationSummary(
            Modified: 15,
            Skipped: 3,
            Failed: 0,
            FailedFiles: new List<string>()
        );

        // Assert
        Assert.Equal(15, summary.Modified);
        Assert.Equal(3, summary.Skipped);
        Assert.Equal(0, summary.Failed);
        Assert.Empty(summary.FailedFiles);
    }

    [Fact]
    public void MigrationSummary_WithAllZeros_CreatesCorrectly()
    {
        // Arrange & Act
        var summary = new MigrationSummary(0, 0, 0, new List<string>());

        // Assert
        Assert.Equal(0, summary.Modified);
        Assert.Equal(0, summary.Skipped);
        Assert.Equal(0, summary.Failed);
        Assert.Empty(summary.FailedFiles);
    }

    [Fact]
    public void MigrationSummary_RecordEquality_ComparesCorrectly()
    {
        // Arrange
        var failedFiles1 = new List<string> { "file1.yaml" };
        var failedFiles2 = new List<string> { "file1.yaml" };

        var summary1 = new MigrationSummary(10, 5, 1, failedFiles1);
        var summary2 = new MigrationSummary(10, 5, 1, failedFiles2);
        var summary3 = new MigrationSummary(5, 2, 0, new List<string>());

        // Act & Assert
        Assert.Equal(summary1.Modified, summary2.Modified);
        Assert.Equal(summary1.Skipped, summary2.Skipped);
        Assert.Equal(summary1.Failed, summary2.Failed);
        Assert.NotEqual(summary1, summary3);
    }

    [Fact]
    public void MigrationSummary_FailedFilesCount_MatchesFailedCount()
    {
        // Arrange
        var failedFiles = new List<string> { "file1.yaml", "file2.yaml", "file3.yaml" };

        // Act
        var summary = new MigrationSummary(0, 0, 3, failedFiles);

        // Assert
        Assert.Equal(summary.Failed, summary.FailedFiles.Count);
    }
}
