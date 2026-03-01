using Xunit;

namespace YamlTalentMigrator.Tests;

/// <summary>
/// Tests for YAML file processing logic.
/// Verifies ProcessFile method behavior with various YAML structures.
/// </summary>
public class ProcessFileTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<string, int>>>> _testConfig;

    public ProcessFileTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"YamlTalentMigrator_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        // Standard test config (F750 mappings)
        _testConfig = new Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<string, int>>>>
        {
            { 0, new Dictionary<string, Dictionary<int, Dictionary<string, int>>>
                {
                    { "TALENT", new Dictionary<int, Dictionary<string, int>>
                        {
                            { 16, new Dictionary<string, int> { { "ne", 0 } } }
                        }
                    }
                }
            },
            { 1, new Dictionary<string, Dictionary<int, Dictionary<string, int>>>
                {
                    { "TALENT", new Dictionary<int, Dictionary<string, int>>
                        {
                            { 3, new Dictionary<string, int> { { "ne", 0 } } }
                        }
                    }
                }
            },
            { 2, new Dictionary<string, Dictionary<int, Dictionary<string, int>>>
                {
                    { "TALENT", new Dictionary<int, Dictionary<string, int>>
                        {
                            { 17, new Dictionary<string, int> { { "ne", 0 } } }
                        }
                    }
                }
            }
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task ProcessFile_BranchesWithoutConditions_ModifiesFile()
    {
        // Arrange
        var yamlContent = @"character: テスト
situation: K1_test
branches:
- entries:
  - id: test1
    content: ""branch 0 content""
- entries:
  - id: test2
    content: ""branch 1 content""
- entries:
  - id: test3
    content: ""branch 2 content""
";
        var filePath = Path.Combine(_tempDir, "test1.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert
        Assert.True(result.Modified, "File should be modified when branches lack conditions");
        Assert.Equal(3, result.BranchesUpdated);

        // Verify file was actually updated
        var updatedContent = await File.ReadAllTextAsync(filePath);
        Assert.Contains("condition:", updatedContent);
        Assert.Contains("TALENT:", updatedContent);
    }

    [Fact]
    public async Task ProcessFile_DryRun_DoesNotModifyFile()
    {
        // Arrange
        var yamlContent = @"character: テスト
situation: K1_test
branches:
- entries:
  - id: test1
    content: ""branch 0 content""
";
        var filePath = Path.Combine(_tempDir, "test_dryrun.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);
        var originalContent = yamlContent;

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: true, _testConfig);

        // Assert
        Assert.True(result.Modified, "Should detect modifications needed");
        Assert.Equal(1, result.BranchesUpdated);

        // Verify file was NOT modified
        var actualContent = await File.ReadAllTextAsync(filePath);
        Assert.Equal(originalContent, actualContent);
    }

    [Fact]
    public async Task ProcessFile_AllBranchesHaveConditions_NoModification()
    {
        // Arrange
        var yamlContent = @"character: テスト
situation: K1_test
branches:
- entries:
  - id: test1
    content: ""branch 0 content""
  condition:
    TALENT:
      16:
        ne: 0
- entries:
  - id: test2
    content: ""branch 1 content""
  condition:
    TALENT:
      3:
        ne: 0
- entries:
  - id: test3
    content: ""branch 2 content""
  condition:
    TALENT:
      17:
        ne: 0
";
        var filePath = Path.Combine(_tempDir, "test_with_conditions.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert
        Assert.False(result.Modified, "File should not be modified when all branches have conditions");
        Assert.Equal(0, result.BranchesUpdated);
    }

    [Fact]
    public async Task ProcessFile_NoBranchesKey_NoModification()
    {
        // Arrange
        var yamlContent = @"character: テスト
situation: K1_test
entries:
- id: test1
  content: ""No branches structure""
";
        var filePath = Path.Combine(_tempDir, "test_no_branches.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert
        Assert.False(result.Modified, "File without branches key should not be modified");
        Assert.Equal(0, result.BranchesUpdated);
    }

    [Fact]
    public async Task ProcessFile_EmptyDocument_NoModification()
    {
        // Arrange
        var yamlContent = "";
        var filePath = Path.Combine(_tempDir, "test_empty.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert
        Assert.False(result.Modified, "Empty document should not be modified");
        Assert.Equal(0, result.BranchesUpdated);
    }

    [Fact]
    public async Task ProcessFile_NullRootNode_NoModification()
    {
        // Arrange
        var yamlContent = "---\n...\n";
        var filePath = Path.Combine(_tempDir, "test_null_root.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert
        Assert.False(result.Modified, "Document with null root should not be modified");
        Assert.Equal(0, result.BranchesUpdated);
    }

    [Fact]
    public async Task ProcessFile_BranchesNotSequence_NoModification()
    {
        // Arrange
        var yamlContent = @"character: テスト
situation: K1_test
branches: not_a_sequence
";
        var filePath = Path.Combine(_tempDir, "test_invalid_branches.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert
        Assert.False(result.Modified, "File with non-sequence branches should not be modified");
        Assert.Equal(0, result.BranchesUpdated);
    }

    [Fact]
    public async Task ProcessFile_PartialConditions_ModifiesOnlyMissingBranches()
    {
        // Arrange - Branch 0 has condition, Branch 1 and 2 don't
        var yamlContent = @"character: テスト
situation: K1_test
branches:
- entries:
  - id: test1
    content: ""branch 0 content""
  condition:
    TALENT:
      16:
        ne: 0
- entries:
  - id: test2
    content: ""branch 1 content""
- entries:
  - id: test3
    content: ""branch 2 content""
";
        var filePath = Path.Combine(_tempDir, "test_partial.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert
        Assert.True(result.Modified, "Should modify branches without conditions");
        Assert.Equal(2, result.BranchesUpdated); // Only branches 1 and 2
    }

    [Fact]
    public async Task ProcessFile_NullCondition_UpdatesBranch()
    {
        // Arrange
        var yamlContent = @"character: テスト
situation: K1_test
branches:
- entries:
  - id: test1
    content: ""branch 0 content""
  condition: null
";
        var filePath = Path.Combine(_tempDir, "test_null_condition.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert
        Assert.True(result.Modified, "Should update branch with null condition");
        Assert.Equal(1, result.BranchesUpdated);
    }

    [Fact]
    public async Task ProcessFile_EmptyCondition_UpdatesBranch()
    {
        // Arrange
        var yamlContent = @"character: テスト
situation: K1_test
branches:
- entries:
  - id: test1
    content: ""branch 0 content""
  condition: {}
";
        var filePath = Path.Combine(_tempDir, "test_empty_condition.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert
        Assert.True(result.Modified, "Should update branch with empty condition");
        Assert.Equal(1, result.BranchesUpdated);
    }

    [Fact]
    public async Task ProcessFile_MoreThanFourBranches_OnlyUpdatesFourBranches()
    {
        // Arrange
        var yamlContent = @"character: テスト
situation: K1_test
branches:
- entries:
  - id: test1
    content: ""branch 0""
- entries:
  - id: test2
    content: ""branch 1""
- entries:
  - id: test3
    content: ""branch 2""
- entries:
  - id: test4
    content: ""branch 3""
- entries:
  - id: test5
    content: ""branch 4 - should not be updated""
- entries:
  - id: test6
    content: ""branch 5 - should not be updated""
";
        var filePath = Path.Combine(_tempDir, "test_many_branches.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert
        Assert.True(result.Modified, "Should modify first 4 branches");
        Assert.Equal(4, result.BranchesUpdated); // Only branches 0-3

        // Verify branch 4 was not updated
        var updatedContent = await File.ReadAllTextAsync(filePath);
        var branch4StartIndex = updatedContent.IndexOf("branch 4");
        var nextBranchIndex = updatedContent.IndexOf("branch 5", branch4StartIndex);
        var branch4Section = updatedContent.Substring(branch4StartIndex, nextBranchIndex - branch4StartIndex);
        Assert.DoesNotContain("condition:", branch4Section);
    }

    [Fact]
    public async Task ProcessFile_InvalidYaml_ReturnsNoModification()
    {
        // Arrange - Malformed YAML
        var yamlContent = @"character: テスト
situation: K1_test
branches:
- entries:
  - id: test1
    content: ""unclosed quote
";
        var filePath = Path.Combine(_tempDir, "test_invalid.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert - ProcessFile catches exceptions and returns (false, 0)
        Assert.False(result.Modified, "Invalid YAML should not be modified");
        Assert.Equal(0, result.BranchesUpdated);
    }

    [Fact]
    public async Task ProcessFile_ComplexCondition_PreservedAsIs()
    {
        // Arrange - Branch with complex existing condition
        var yamlContent = @"character: テスト
situation: K1_test
branches:
- entries:
  - id: test1
    content: ""branch 0 content""
  condition:
    TALENT:
      16:
        ne: 0
      99:
        eq: 1
    FLAG:
      10:
        eq: 1
";
        var filePath = Path.Combine(_tempDir, "test_complex.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);
        var originalContent = await File.ReadAllTextAsync(filePath);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert - Should not modify (has non-empty condition)
        Assert.False(result.Modified, "Should not modify branch with complex condition");
        Assert.Equal(0, result.BranchesUpdated);

        var finalContent = await File.ReadAllTextAsync(filePath);
        Assert.Equal(originalContent, finalContent);
    }

    [Fact]
    public async Task ProcessFile_Branch3_GetsEmptyCondition()
    {
        // Arrange - Branch 3 (index 3) should get empty condition (not in config)
        var yamlContent = @"character: テスト
situation: K1_test
branches:
- entries:
  - id: test1
    content: ""branch 0""
- entries:
  - id: test2
    content: ""branch 1""
- entries:
  - id: test3
    content: ""branch 2""
- entries:
  - id: test4
    content: ""branch 3""
";
        var filePath = Path.Combine(_tempDir, "test_branch3.yaml");
        await File.WriteAllTextAsync(filePath, yamlContent);

        // Act
        var result = await InvokeProcessFile(filePath, dryRun: false, _testConfig);

        // Assert
        Assert.True(result.Modified, "Should modify all 4 branches");
        Assert.Equal(4, result.BranchesUpdated);

        // Verify branch 3 got empty condition (index 3 not in config)
        var updatedContent = await File.ReadAllTextAsync(filePath);
        // Branch 3 should have condition: {} since index 3 is not in branchConditions
        Assert.Contains("condition:", updatedContent);
    }

    /// <summary>
    /// Helper method to invoke private ProcessFile method via reflection
    /// </summary>
    private async Task<(bool Modified, int BranchesUpdated)> InvokeProcessFile(
        string filePath,
        bool dryRun,
        Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<string, int>>>> branchConditions)
    {
        var method = typeof(Program).GetMethod("ProcessFile",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (method == null)
        {
            throw new InvalidOperationException("ProcessFile method not found");
        }

        var task = method.Invoke(null, new object[] { filePath, dryRun, branchConditions }) as Task<(bool, int)>;
        if (task == null)
        {
            throw new InvalidOperationException("ProcessFile did not return expected Task type");
        }

        return await task;
    }
}
