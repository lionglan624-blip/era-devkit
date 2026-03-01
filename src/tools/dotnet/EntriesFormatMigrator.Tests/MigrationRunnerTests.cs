using Xunit;

namespace EntriesFormatMigrator.Tests;

public class MigrationRunnerTests
{
    private const string SampleYaml = @"character: 美鈴
situation: K1_会話親密
entries:
- id: fallback
  content: >-
    P4 content
  priority: 4
- id: talent_3_1
  content: >-
    P3 content
  priority: 3
  condition:
    type: Talent
    talentType: 3
    threshold: 1
- id: fallback
  content: >-
    P2 content
  priority: 2
- id: fallback
  content: >-
    P1 content
  priority: 1";

    [Fact]
    public async Task RunAsync_DryRun_DoesNotWriteFiles()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        mockFs.AddFile("test/file1.yaml", SampleYaml);

        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(mockFs, patcher, reporter);

        // Act
        var summary = await runner.RunAsync("test", dryRun: true);

        // Assert
        Assert.Equal(1, summary.Modified);
        Assert.Equal(0, summary.Failed);

        // Verify file was not actually modified
        var content = mockFs.ReadAllText("test/file1.yaml");
        Assert.Equal(SampleYaml, content);
    }

    [Fact]
    public async Task RunAsync_LiveMode_WritesFiles()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        mockFs.AddFile("test/file1.yaml", SampleYaml);

        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(mockFs, patcher, reporter);

        // Act
        var summary = await runner.RunAsync("test", dryRun: false);

        // Assert
        Assert.Equal(1, summary.Modified);
        Assert.Equal(0, summary.Failed);

        // Verify file was actually modified
        var content = mockFs.ReadAllText("test/file1.yaml");
        Assert.NotEqual(SampleYaml, content);
        Assert.Contains("id: talent_16_0", content);
        Assert.Contains("id: talent_17_0", content);
    }

    [Fact]
    public async Task RunAsync_WithInvalidStructure_SkipsFile()
    {
        // Arrange
        var invalidYaml = @"character: Test
entries:
- id: fallback
  priority: 1";

        var mockFs = new MockFileSystem();
        mockFs.AddFile("test/invalid.yaml", invalidYaml);

        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(mockFs, patcher, reporter);

        // Act
        var summary = await runner.RunAsync("test", dryRun: false);

        // Assert
        Assert.Equal(0, summary.Modified);
        Assert.Equal(0, summary.Skipped); // Not counted because not a target file
        Assert.Equal(0, summary.Failed);
    }

    [Fact]
    public void ProcessFile_WithAlreadyMigrated_ReportsNotModified()
    {
        // Arrange
        var alreadyMigrated = "character: 美鈴\nentries:\n- id: talent_16_0\n  priority: 4\n  condition:\n    type: Talent\n    talentType: 16\n    threshold: 1\n- id: talent_3_1\n  priority: 3\n  condition:\n    type: Talent\n    talentType: 3\n    threshold: 1\n- id: talent_17_0\n  priority: 2\n  condition:\n    type: Talent\n    talentType: 17\n    threshold: 1\n- id: fallback\n  priority: 1";

        var mockFs = new MockFileSystem();
        mockFs.AddFile("test/migrated.yaml", alreadyMigrated);

        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(mockFs, patcher, reporter);

        // Act
        var result = runner.ProcessFile("test/migrated.yaml", dryRun: false);

        // Assert
        Assert.False(result.Modified);
        Assert.Equal(0, result.EntriesUpdated);
    }

    [Fact]
    public async Task RunAsync_WithMultipleFiles_CountsCorrectly()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        mockFs.AddFile("test/file1.yaml", SampleYaml);
        mockFs.AddFile("test/file2.yaml", SampleYaml);

        var alreadyMigrated = "character: Test\nentries:\n- id: talent_16_0\n  priority: 4\n  condition:\n    type: Talent\n    talentType: 16\n    threshold: 1\n- id: talent_3_1\n  priority: 3\n  condition:\n    type: Talent\n    talentType: 3\n    threshold: 1\n- id: talent_17_0\n  priority: 2\n  condition:\n    type: Talent\n    talentType: 17\n    threshold: 1\n- id: fallback\n  priority: 1";

        mockFs.AddFile("test/file3.yaml", alreadyMigrated);

        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(mockFs, patcher, reporter);

        // Act
        var summary = await runner.RunAsync("test", dryRun: false);

        // Assert
        Assert.Equal(2, summary.Modified); // file1 and file2
        Assert.Equal(1, summary.Skipped);  // file3 (already migrated)
        Assert.Equal(0, summary.Failed);
    }

    [Fact]
    public void ProcessFile_WithModifiedContent_ReturnsCorrectChanges()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        mockFs.AddFile("test/file.yaml", SampleYaml);

        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(mockFs, patcher, reporter);

        // Act
        var result = runner.ProcessFile("test/file.yaml", dryRun: false);

        // Assert
        Assert.True(result.Modified);
        Assert.Equal(2, result.EntriesUpdated); // P4 and P2 renamed
        Assert.True(result.Changes.Count > 0);
        Assert.Contains(result.Changes, c => c.Contains("Renamed id: fallback -> ") && c.Contains("talent_16_0"));
        Assert.Contains(result.Changes, c => c.Contains("Renamed id: fallback -> ") && c.Contains("talent_17_0"));
    }

    [Fact]
    public void ProcessFile_DryRunMode_DoesNotWriteFile()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        mockFs.AddFile("test/file.yaml", SampleYaml);

        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(mockFs, patcher, reporter);

        // Act
        var result = runner.ProcessFile("test/file.yaml", dryRun: true);

        // Assert
        Assert.True(result.Modified);

        // Verify file content unchanged
        var content = mockFs.ReadAllText("test/file.yaml");
        Assert.Equal(SampleYaml, content);
    }

    [Fact]
    public void ProcessFile_LiveMode_WritesModifiedFile()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        mockFs.AddFile("test/file.yaml", SampleYaml);

        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(mockFs, patcher, reporter);

        // Act
        var result = runner.ProcessFile("test/file.yaml", dryRun: false);

        // Assert
        Assert.True(result.Modified);

        // Verify file content changed
        var content = mockFs.ReadAllText("test/file.yaml");
        Assert.NotEqual(SampleYaml, content);
        Assert.Contains("id: talent_16_0", content);
    }

    [Fact]
    public async Task RunAsync_WithProcessingError_RecordsFailure()
    {
        // Arrange
        var targetYaml = @"character: 美鈴
situation: K1_会話親密
entries:
- id: fallback
  content: >-
    P4 content
  priority: 4
- id: talent_3_1
  content: >-
    P3 content
  priority: 3
  condition:
    type: Talent
    talentType: 3
    threshold: 1
- id: fallback
  content: >-
    P2 content
  priority: 2
- id: fallback
  content: >-
    P1 content
  priority: 1";

        // Create a custom mock that throws during write (simulating disk full or permission error)
        var throwingMockFs = new ThrowingMockFileSystem();
        throwingMockFs.AddFile("test/error.yaml", targetYaml);
        throwingMockFs.SetThrowOnWrite("test/error.yaml");

        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(throwingMockFs, patcher, reporter);

        // Capture error output
        var errorOutput = new StringWriter();
        Console.SetError(errorOutput);

        try
        {
            // Act
            var summary = await runner.RunAsync("test", dryRun: false);

            // Assert
            Assert.Equal(1, summary.Failed);
            Assert.Single(summary.FailedFiles);
            Assert.Contains("test/error.yaml", summary.FailedFiles[0]);

            var errorText = errorOutput.ToString();
            Assert.Contains("ERROR processing test/error.yaml", errorText);
        }
        finally
        {
            // Restore standard error
            var standardError = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
            Console.SetError(standardError);
        }
    }

    [Fact]
    public async Task RunAsync_WithNoTargetFiles_ReturnsZeroSummary()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        // No files added

        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(mockFs, patcher, reporter);

        // Act
        var summary = await runner.RunAsync("test", dryRun: false);

        // Assert
        Assert.Equal(0, summary.Modified);
        Assert.Equal(0, summary.Skipped);
        Assert.Equal(0, summary.Failed);
        Assert.Empty(summary.FailedFiles);
    }

    [Fact]
    public void ProcessFile_CountsConditionInsertions()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        mockFs.AddFile("test/file.yaml", SampleYaml);

        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(mockFs, patcher, reporter);

        // Act
        var result = runner.ProcessFile("test/file.yaml", dryRun: false);

        // Assert
        Assert.Contains(result.Changes, c => c.Contains("Inserted condition block"));
    }

    // Helper class to simulate file write errors
    private sealed class ThrowingMockFileSystem : IMigrationFileSystem
    {
        private readonly Dictionary<string, string> _files = new();
        private readonly HashSet<string> _throwOnWritePaths = new();

        public void SetThrowOnWrite(string path)
        {
            _throwOnWritePaths.Add(path);
        }

        public void AddFile(string path, string content)
        {
            _files[path] = content;
        }

        public string[] GetDirectories(string path) => Array.Empty<string>();

        public string[] GetFiles(string path, string searchPattern)
        {
            return _files.Keys.Where(k => k.StartsWith(path) && k.EndsWith(".yaml")).ToArray();
        }

        public bool DirectoryExists(string path) => true;

        public bool FileExists(string path) => _files.ContainsKey(path);

        public string ReadAllText(string path)
        {
            if (!_files.ContainsKey(path))
            {
                throw new FileNotFoundException($"File not found: {path}");
            }
            return _files[path];
        }

        public DateTime GetLastWriteTime(string path) => DateTime.Now;

        public void WriteAllText(string path, string content)
        {
            if (_throwOnWritePaths.Contains(path))
            {
                throw new IOException($"Simulated write error for {path}");
            }
            _files[path] = content;
        }
    }
}
