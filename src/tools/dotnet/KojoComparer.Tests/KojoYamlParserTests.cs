using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for KojoYamlParser.Parse() method.
/// Verifies YAML file parsing for branches-based kojo dialogue format.
/// </summary>
public class KojoYamlParserTests : IDisposable
{
    private readonly string _tempDir;
    private readonly KojoYamlParser _parser;

    public KojoYamlParserTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _parser = new KojoYamlParser();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // -------------------------------------------------------------------------
    // Happy-path tests
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_WithValidBranchesYaml_ReturnsFirstBranchLines()
    {
        // Arrange
        var yamlContent = @"character: 美鈴
situation: COM_0
branches:
- lines:
  - '最初の行'
  - '二番目の行'
  condition: {}
";
        var path = WriteTempYaml(yamlContent);

        // Act
        var result = _parser.Parse(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("最初の行", result[0]);
        Assert.Equal("二番目の行", result[1]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_WithMultipleBranches_ReturnsFirstBranchOnly()
    {
        // Arrange
        var yamlContent = @"character: 美鈴
situation: COM_0
branches:
- lines:
  - '第一ブランチ'
  condition:
    TALENT: {16: {ne: 0}}
- lines:
  - '第二ブランチ'
  condition: {}
";
        var path = WriteTempYaml(yamlContent);

        // Act
        var result = _parser.Parse(path);

        // Assert - always returns first branch
        Assert.Single(result);
        Assert.Equal("第一ブランチ", result[0]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_WithSingleLineFirstBranch_ReturnsSingleLine()
    {
        // Arrange
        var yamlContent = @"character: 魔理沙
situation: K10_会話親密
com_id: 302
branches:
- lines:
  - '単一行'
  condition: {}
";
        var path = WriteTempYaml(yamlContent);

        // Act
        var result = _parser.Parse(path);

        // Assert
        Assert.Single(result);
        Assert.Equal("単一行", result[0]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_WithNullLinesInFirstBranch_ReturnsEmptyList()
    {
        // Arrange: first branch has no lines key → Lines defaults to null/empty
        var yamlContent = @"character: テスト
situation: COM_0
branches:
- condition: {}
";
        var path = WriteTempYaml(yamlContent);

        // Act
        var result = _parser.Parse(path);

        // Assert: null lines → returns empty list (per Parse() fallback)
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_WithCharacterAndSituationFields_ParsesSuccessfully()
    {
        // Arrange: verify optional metadata fields don't break parsing
        var yamlContent = @"character: レミリア
situation: COM_200
com_id: 200
branches:
- lines:
  - 'お前も吸血鬼になる？'
  condition: {}
";
        var path = WriteTempYaml(yamlContent);

        // Act
        var result = _parser.Parse(path);

        // Assert
        Assert.Single(result);
        Assert.Equal("お前も吸血鬼になる？", result[0]);
    }

    // -------------------------------------------------------------------------
    // Error path tests
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDir, "does_not_exist.yaml");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _parser.Parse(nonExistentPath));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_WithFileNotFound_ErrorContainsFilePath()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDir, "missing_file.yaml");

        // Act
        var ex = Assert.Throws<FileNotFoundException>(() => _parser.Parse(nonExistentPath));

        // Assert: error message includes the file path
        Assert.Contains("missing_file.yaml", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_WithEmptyBranchesArray_ThrowsInvalidDataException()
    {
        // Arrange: branches key present but empty
        var yamlContent = @"character: テスト
situation: COM_0
branches: []
";
        var path = WriteTempYaml(yamlContent);

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => _parser.Parse(path));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_WithNoBranchesKey_ThrowsInvalidDataException()
    {
        // Arrange: YAML with no branches key at all (KojoFileData.Branches defaults to empty list)
        var yamlContent = @"character: テスト
situation: COM_0
";
        var path = WriteTempYaml(yamlContent);

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => _parser.Parse(path));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_WithEmptyBranches_ErrorContainsFilePath()
    {
        // Arrange
        var yamlContent = @"character: テスト
situation: COM_0
branches: []
";
        var path = WriteTempYaml(yamlContent, "empty_branches.yaml");

        // Act
        var ex = Assert.Throws<InvalidDataException>(() => _parser.Parse(path));

        // Assert: error message includes the file path
        Assert.Contains("empty_branches.yaml", ex.Message);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private string WriteTempYaml(string content, string? fileName = null)
    {
        fileName ??= Path.GetRandomFileName() + ".yaml";
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }
}
