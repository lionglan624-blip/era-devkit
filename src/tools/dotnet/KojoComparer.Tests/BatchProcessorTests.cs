using Era.Core.Dialogue;
using Era.Core.Types;
using Moq;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for BatchProcessor.ProcessAllAsync and BatchProcessor.BatchReport.
/// Uses Moq to mock IBatchExecutor, IErbRunner, IYamlRunner dependencies.
/// </summary>
public class BatchProcessorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly OutputNormalizer _normalizer;
    private readonly DiffEngine _diffEngine;

    public BatchProcessorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _normalizer = new OutputNormalizer();
        _diffEngine = new DiffEngine();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // -------------------------------------------------------------------------
    // BatchReport properties
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void BatchReport_DefaultValues_AreZeroAndEmpty()
    {
        // Arrange & Act
        var report = new BatchProcessor.BatchReport();

        // Assert
        Assert.Equal(0, report.TotalTests);
        Assert.Equal(0, report.PassedTests);
        Assert.Equal(0, report.FailedTests);
        Assert.Empty(report.Failures);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void BatchReport_CanSetAllProperties()
    {
        // Arrange & Act
        var report = new BatchProcessor.BatchReport
        {
            TotalTests = 10,
            PassedTests = 7,
            FailedTests = 3,
            Failures = new List<string> { "FAIL: COM_001 (1)" }
        };

        // Assert
        Assert.Equal(10, report.TotalTests);
        Assert.Equal(7, report.PassedTests);
        Assert.Equal(3, report.FailedTests);
        Assert.Single(report.Failures);
    }

    // -------------------------------------------------------------------------
    // ProcessAllAsync: empty test cases
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ProcessAllAsync_WithEmptyTestCases_ReturnsZeroReport()
    {
        // Arrange
        var mockBatchExecutor = new Mock<IBatchExecutor>();
        mockBatchExecutor.Setup(b => b.ExecuteAllAsync(It.IsAny<List<TestCase>>()))
            .ReturnsAsync(new Dictionary<string, (string output, List<DisplayMode> displayModes)>());

        var mockErbRunner = new Mock<IErbRunner>();
        var mockYamlRunner = new Mock<IYamlRunner>();

        var processor = new BatchProcessor(
            mockErbRunner.Object,
            mockYamlRunner.Object,
            _normalizer,
            _diffEngine,
            mockBatchExecutor.Object);

        // Act
        var report = await processor.ProcessAllAsync(new List<TestCase>());

        // Assert
        Assert.Equal(0, report.TotalTests);
        Assert.Equal(0, report.PassedTests);
        Assert.Equal(0, report.FailedTests);
        Assert.Empty(report.Failures);
    }

    // -------------------------------------------------------------------------
    // ProcessAllAsync: missing batch result
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ProcessAllAsync_WhenBatchResultMissing_ReportsFailure()
    {
        // Arrange: batch executor returns no results (empty dict)
        var mockBatchExecutor = new Mock<IBatchExecutor>();
        mockBatchExecutor.Setup(b => b.ExecuteAllAsync(It.IsAny<List<TestCase>>()))
            .ReturnsAsync(new Dictionary<string, (string output, List<DisplayMode> displayModes)>());

        var mockErbRunner = new Mock<IErbRunner>();
        var mockYamlRunner = new Mock<IYamlRunner>();

        var processor = new BatchProcessor(
            mockErbRunner.Object,
            mockYamlRunner.Object,
            _normalizer,
            _diffEngine,
            mockBatchExecutor.Object);

        var testCases = new List<TestCase>
        {
            new TestCase
            {
                ErbFile = "test.ERB",
                YamlFile = "test.yaml",
                FunctionName = "@KOJO_TEST",
                ComId = 1,
                CharacterId = "1",
                State = new Dictionary<string, int>()
            }
        };

        // Act
        var report = await processor.ProcessAllAsync(testCases);

        // Assert: one failure because no batch result found
        Assert.Equal(1, report.TotalTests);
        Assert.Equal(0, report.PassedTests);
        Assert.Equal(1, report.FailedTests);
        Assert.Single(report.Failures);
        Assert.Contains("No batch result found", report.Failures[0]);
    }

    // -------------------------------------------------------------------------
    // ProcessAllAsync: matching output passes
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ProcessAllAsync_WhenOutputMatches_ReportsPass()
    {
        // Arrange: create a real YAML file and matching ERB output
        var yamlContent = @"character: テスト
situation: COM_0
branches:
- lines:
  - 'テスト台詞'
  condition: {}
";
        var yamlPath = Path.Combine(_tempDir, "meirin_com999.yaml");
        File.WriteAllText(yamlPath, yamlContent);

        var matchingOutput = "テスト台詞\n";
        var displayModes = new List<DisplayMode> { DisplayMode.Newline };

        var mockBatchExecutor = new Mock<IBatchExecutor>();
        mockBatchExecutor.Setup(b => b.ExecuteAllAsync(It.IsAny<List<TestCase>>()))
            .ReturnsAsync(new Dictionary<string, (string output, List<DisplayMode> displayModes)>
            {
                { "KOJO_TEST", (matchingOutput, displayModes) }
            });

        var mockErbRunner = new Mock<IErbRunner>();

        // Use real YamlRunner to render the branches-format YAML
        var mockYamlRunner = new Mock<IYamlRunner>();
        mockYamlRunner.Setup(y => y.RenderWithMetadata(yamlPath, It.IsAny<Dictionary<string, object>>()))
            .Returns(DialogueResult.Create(new List<DialogueLine>
            {
                new DialogueLine("テスト台詞", DisplayMode.Default)
            }));

        var processor = new BatchProcessor(
            mockErbRunner.Object,
            mockYamlRunner.Object,
            _normalizer,
            _diffEngine,
            mockBatchExecutor.Object);

        var testCases = new List<TestCase>
        {
            new TestCase
            {
                ErbFile = "test.ERB",
                YamlFile = yamlPath,
                FunctionName = "@KOJO_TEST",
                ComId = 999,
                CharacterId = "1",
                State = new Dictionary<string, int>()
            }
        };

        // Act
        var report = await processor.ProcessAllAsync(testCases);

        // Assert: output matches → pass
        Assert.Equal(1, report.TotalTests);
        Assert.Equal(1, report.PassedTests);
        Assert.Equal(0, report.FailedTests);
    }

    // -------------------------------------------------------------------------
    // ProcessAllAsync: batch executor throws
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ProcessAllAsync_WhenBatchExecutorThrows_WrapsException()
    {
        // Arrange
        var mockBatchExecutor = new Mock<IBatchExecutor>();
        mockBatchExecutor.Setup(b => b.ExecuteAllAsync(It.IsAny<List<TestCase>>()))
            .ThrowsAsync(new Exception("Executor failure"));

        var mockErbRunner = new Mock<IErbRunner>();
        var mockYamlRunner = new Mock<IYamlRunner>();

        var processor = new BatchProcessor(
            mockErbRunner.Object,
            mockYamlRunner.Object,
            _normalizer,
            _diffEngine,
            mockBatchExecutor.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => processor.ProcessAllAsync(new List<TestCase> {
                new TestCase { FunctionName = "F", ComId = 1, CharacterId = "1", State = new() }
            }));
        Assert.Contains("Batch execution failed", ex.Message);
    }

    // -------------------------------------------------------------------------
    // ProcessAsync: no ERB files
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ProcessAsync_WithNoErbFiles_ReturnsEmptyReport()
    {
        // Arrange: directories with no ERB files
        var erbDir = Path.Combine(_tempDir, "erb_empty");
        var yamlDir = Path.Combine(_tempDir, "yaml_empty");
        Directory.CreateDirectory(erbDir);
        Directory.CreateDirectory(yamlDir);

        var mockBatchExecutor = new Mock<IBatchExecutor>();
        var mockErbRunner = new Mock<IErbRunner>();
        var mockYamlRunner = new Mock<IYamlRunner>();

        var processor = new BatchProcessor(
            mockErbRunner.Object,
            mockYamlRunner.Object,
            _normalizer,
            _diffEngine,
            mockBatchExecutor.Object);

        // Act
        var report = await processor.ProcessAsync(erbDir, yamlDir, "@KOJO_TEST", new List<Dictionary<string, int>>());

        // Assert
        Assert.Equal(0, report.TotalTests);
        Assert.Equal(0, report.PassedTests);
        Assert.Equal(0, report.FailedTests);
    }

    // -------------------------------------------------------------------------
    // ProcessAsync: ERB file with no matching YAML
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ProcessAsync_WithNoMatchingYaml_AddsFailureMessage()
    {
        // Arrange: ERB dir has a file, YAML dir is empty
        var erbDir = Path.Combine(_tempDir, "erb_has");
        var yamlDir = Path.Combine(_tempDir, "yaml_none");
        Directory.CreateDirectory(erbDir);
        Directory.CreateDirectory(yamlDir);
        File.WriteAllText(Path.Combine(erbDir, "KOJO1.ERB"), "@KOJO1\nPRINTFORML テスト\n");

        var mockBatchExecutor = new Mock<IBatchExecutor>();
        var mockErbRunner = new Mock<IErbRunner>();
        var mockYamlRunner = new Mock<IYamlRunner>();

        var processor = new BatchProcessor(
            mockErbRunner.Object,
            mockYamlRunner.Object,
            _normalizer,
            _diffEngine,
            mockBatchExecutor.Object);

        var states = new List<Dictionary<string, int>> { new Dictionary<string, int>() };

        // Act
        var report = await processor.ProcessAsync(erbDir, yamlDir, "@KOJO1", states);

        // Assert: failure message for no matching YAML
        Assert.Contains(report.Failures, f => f.Contains("No matching YAML file"));
    }
}
