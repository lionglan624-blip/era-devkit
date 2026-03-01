using Era.Core.Dialogue;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for ErbBatchEvaluator.ExecuteAllAsync.
/// Uses real ErbEvaluator and temp ERB files to test parallel batch execution.
/// </summary>
public class ErbBatchEvaluatorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ErbEvaluator _erbEvaluator;

    public ErbBatchEvaluatorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _erbEvaluator = new ErbEvaluator(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // -------------------------------------------------------------------------
    // Constructor test
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void ErbBatchEvaluator_ConstructsSuccessfully()
    {
        // Act
        var evaluator = new ErbBatchEvaluator(_erbEvaluator);

        // Assert
        Assert.NotNull(evaluator);
    }

    // -------------------------------------------------------------------------
    // ExecuteAllAsync: empty list
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAllAsync_WithEmptyList_ReturnsEmptyDictionary()
    {
        // Arrange
        var batchEvaluator = new ErbBatchEvaluator(_erbEvaluator);

        // Act
        var results = await batchEvaluator.ExecuteAllAsync(new List<TestCase>());

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    // -------------------------------------------------------------------------
    // ExecuteAllAsync: single test case
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAllAsync_WithSingleTestCase_ExecutesAndReturnsResult()
    {
        // Arrange
        var erbContent = @"@KOJO_BATCH_TEST
PRINTFORML バッチテスト台詞
";
        var erbPath = WriteErb("batch_test.ERB", erbContent);

        var batchEvaluator = new ErbBatchEvaluator(_erbEvaluator);
        var testCases = new List<TestCase>
        {
            new TestCase
            {
                ErbFile = erbPath,
                YamlFile = "unused.yaml",
                FunctionName = "@KOJO_BATCH_TEST",
                ComId = 0,
                CharacterId = "1",
                State = new Dictionary<string, int>()
            }
        };

        // Act
        var results = await batchEvaluator.ExecuteAllAsync(testCases);

        // Assert
        Assert.Single(results);
        Assert.True(results.ContainsKey("KOJO_BATCH_TEST"));
        Assert.Contains("バッチテスト台詞", results["KOJO_BATCH_TEST"].output);
    }

    // -------------------------------------------------------------------------
    // ExecuteAllAsync: multiple test cases
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAllAsync_WithMultipleTestCases_ExecutesAll()
    {
        // Arrange: two different functions in two different ERB files
        var erb1Content = @"@KOJO_FUNC_A
PRINTFORML 関数A台詞
";
        var erb2Content = @"@KOJO_FUNC_B
PRINTFORML 関数B台詞
";
        var erbPath1 = WriteErb("batch_a.ERB", erb1Content);
        var erbPath2 = WriteErb("batch_b.ERB", erb2Content);

        var batchEvaluator = new ErbBatchEvaluator(_erbEvaluator);
        var testCases = new List<TestCase>
        {
            new TestCase
            {
                ErbFile = erbPath1,
                YamlFile = "unused.yaml",
                FunctionName = "@KOJO_FUNC_A",
                ComId = 1,
                CharacterId = "1",
                State = new Dictionary<string, int>()
            },
            new TestCase
            {
                ErbFile = erbPath2,
                YamlFile = "unused.yaml",
                FunctionName = "@KOJO_FUNC_B",
                ComId = 2,
                CharacterId = "1",
                State = new Dictionary<string, int>()
            }
        };

        // Act
        var results = await batchEvaluator.ExecuteAllAsync(testCases);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.True(results.ContainsKey("KOJO_FUNC_A"));
        Assert.True(results.ContainsKey("KOJO_FUNC_B"));
        Assert.Contains("関数A台詞", results["KOJO_FUNC_A"].output);
        Assert.Contains("関数B台詞", results["KOJO_FUNC_B"].output);
    }

    // -------------------------------------------------------------------------
    // ExecuteAllAsync: normalizes @ prefix in key
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAllAsync_FunctionNameWithAtPrefix_StoresWithoutAtInKey()
    {
        // Arrange
        var erbContent = @"@KOJO_AT_PREFIX
PRINTFORML アットテスト
";
        var erbPath = WriteErb("batch_at.ERB", erbContent);

        var batchEvaluator = new ErbBatchEvaluator(_erbEvaluator);
        var testCases = new List<TestCase>
        {
            new TestCase
            {
                ErbFile = erbPath,
                YamlFile = "unused.yaml",
                FunctionName = "@KOJO_AT_PREFIX",
                ComId = 0,
                CharacterId = "1",
                State = new Dictionary<string, int>()
            }
        };

        // Act
        var results = await batchEvaluator.ExecuteAllAsync(testCases);

        // Assert: key stored without @ prefix
        Assert.True(results.ContainsKey("KOJO_AT_PREFIX"));
        Assert.False(results.ContainsKey("@KOJO_AT_PREFIX"));
    }

    // -------------------------------------------------------------------------
    // ExecuteAllAsync: handles error gracefully (skips failed case)
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAllAsync_WhenOneCaseFails_ContinuesOthers()
    {
        // Arrange: first test case points to non-existent ERB file (will fail),
        // second is valid
        var erbContent = @"@KOJO_VALID
PRINTFORML 有効台詞
";
        var validPath = WriteErb("batch_valid.ERB", erbContent);
        var invalidPath = Path.Combine(_tempDir, "nonexistent.ERB");

        var batchEvaluator = new ErbBatchEvaluator(_erbEvaluator);
        var testCases = new List<TestCase>
        {
            new TestCase
            {
                ErbFile = invalidPath,  // This will fail
                YamlFile = "unused.yaml",
                FunctionName = "@KOJO_INVALID",
                ComId = 0,
                CharacterId = "1",
                State = new Dictionary<string, int>()
            },
            new TestCase
            {
                ErbFile = validPath,  // This succeeds
                YamlFile = "unused.yaml",
                FunctionName = "@KOJO_VALID",
                ComId = 1,
                CharacterId = "1",
                State = new Dictionary<string, int>()
            }
        };

        // Act - should not throw even when one case fails
        var results = await batchEvaluator.ExecuteAllAsync(testCases);

        // Assert: valid case still executed
        Assert.True(results.ContainsKey("KOJO_VALID"));
        Assert.Contains("有効台詞", results["KOJO_VALID"].output);
        // Failed case not in results
        Assert.False(results.ContainsKey("KOJO_INVALID"));
    }

    // -------------------------------------------------------------------------
    // ExecuteAllAsync: deduplication (same function name twice - only first stored)
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAllAsync_WithDuplicateFunctionName_StoresOnlyFirst()
    {
        // Arrange: same function name in two test cases
        var erbContent = @"@KOJO_DUPE
PRINTFORML 重複テスト
";
        var erbPath = WriteErb("batch_dupe.ERB", erbContent);

        var batchEvaluator = new ErbBatchEvaluator(_erbEvaluator);
        var testCases = new List<TestCase>
        {
            new TestCase
            {
                ErbFile = erbPath,
                YamlFile = "unused.yaml",
                FunctionName = "@KOJO_DUPE",
                ComId = 1,
                CharacterId = "1",
                State = new Dictionary<string, int>()
            },
            new TestCase
            {
                ErbFile = erbPath,
                YamlFile = "unused.yaml",
                FunctionName = "@KOJO_DUPE",
                ComId = 2,
                CharacterId = "1",
                State = new Dictionary<string, int>()
            }
        };

        // Act
        var results = await batchEvaluator.ExecuteAllAsync(testCases);

        // Assert: only one entry with this key
        Assert.True(results.ContainsKey("KOJO_DUPE"));
        Assert.Single(results);
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private string WriteErb(string fileName, string content)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content, System.Text.Encoding.UTF8);
        return path;
    }
}
