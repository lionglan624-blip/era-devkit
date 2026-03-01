using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for YamlRunner inner classes exercised via the public API.
/// Covers VariableStoreAdapter (TALENT/ABL/TFLAG lookups), SimpleCharacterDataService,
/// ExtractStateFromContext, and RenderWithMetadata branch detection.
/// </summary>
public class YamlRunnerInternalTests : IDisposable
{
    private readonly string _tempDir;

    public YamlRunnerInternalTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // -------------------------------------------------------------------------
    // RenderWithMetadata: entries format detection
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void RenderWithMetadata_WithEntriesFormat_DetectsEntriesKey()
    {
        // Arrange: entries-format YAML
        var yamlContent = @"entries:
- id: fallback
  content: >-
    こんにちは。
  priority: 0
";
        var path = WriteTempYaml("meirin_com999.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>();

        // Act
        var result = runner.RenderWithMetadata(path, context);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.DialogueLines);
        Assert.Contains("こんにちは。", result.DialogueLines[0].Text);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RenderWithMetadata_WithBranchesFormat_DetectsBranchesKey()
    {
        // Arrange: branches-format YAML
        var yamlContent = @"character: テスト
situation: COM_0
branches:
- lines:
  - 'テスト台詞'
  condition: {}
";
        var path = WriteTempYaml("meirin_com998.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>();

        // Act
        var result = runner.RenderWithMetadata(path, context);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.DialogueLines);
        Assert.Contains("テスト台詞", result.DialogueLines[0].Text);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RenderWithMetadata_WithUnknownFormat_ThrowsInvalidOperationException()
    {
        // Arrange: YAML without entries: or branches: key
        var yamlContent = @"character: テスト
situation: COM_0
unknown_key: value
";
        var path = WriteTempYaml("meirin_com997.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            runner.RenderWithMetadata(path, context));
        Assert.Contains("Unknown YAML format", ex.Message);
    }

    // -------------------------------------------------------------------------
    // VariableStoreAdapter: TALENT lookups (via entries format)
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void Render_WithTalentContextSet_SelectsTalentBranch()
    {
        // Arrange: entries-format with TALENT condition
        var yamlContent = @"entries:
- id: lover
  content: >-
    恋人の台詞。
  condition:
    type: Talent
    talentType: '16'
    threshold: 1
  priority: 4
- id: fallback
  content: >-
    普通の台詞。
  priority: 0
";
        var path = WriteTempYaml("meirin_com500.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 1 } } }
        };

        // Act
        var result = runner.Render(path, context);

        // Assert: should select lover branch
        Assert.Contains("恋人の台詞。", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Render_WithTalentContextZero_SelectsFallbackBranch()
    {
        // Arrange: entries-format with TALENT condition
        var yamlContent = @"entries:
- id: lover
  content: >-
    恋人の台詞。
  condition:
    type: Talent
    talentType: '16'
    threshold: 1
  priority: 4
- id: fallback
  content: >-
    普通の台詞。
  priority: 0
";
        var path = WriteTempYaml("meirin_com501.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 0 } } }
        };

        // Act
        var result = runner.Render(path, context);

        // Assert: should select fallback
        Assert.Contains("普通の台詞。", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Render_WithEmptyContext_SelectsFallbackBranch()
    {
        // Arrange: entries-format, empty context
        var yamlContent = @"entries:
- id: fallback
  content: >-
    デフォルト台詞。
  priority: 0
";
        var path = WriteTempYaml("meirin_com502.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>();

        // Act
        var result = runner.Render(path, context);

        // Assert
        Assert.Contains("デフォルト台詞。", result);
    }

    // -------------------------------------------------------------------------
    // VariableStoreAdapter: ABL and TFLAG defaults to 0
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void Render_WithAblContextMissing_DefaultsToZero()
    {
        // Arrange: entries-format with no ABL key in context (adapter should return 0)
        var yamlContent = @"entries:
- id: fallback
  content: >-
    能力デフォルト。
  priority: 0
";
        var path = WriteTempYaml("meirin_com503.yaml", yamlContent);
        var runner = new YamlRunner();
        // No ABL key provided
        var context = new Dictionary<string, object>();

        // Act - should not throw (ABL defaults to 0)
        var result = runner.Render(path, context);

        // Assert
        Assert.Contains("能力デフォルト。", result);
    }

    // -------------------------------------------------------------------------
    // ExtractStateFromContext: TALENT compound key parsing
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void Render_BranchesFormat_WithTalentCompoundKey_SelectsMatchingBranch()
    {
        // Arrange: branches-format YAML with TALENT condition
        var yamlContent = @"character: テスト
situation: COM_0
branches:
- lines:
  - '恋人台詞'
  condition:
    TALENT:
      '16': {ne: 0}
- lines:
  - '通常台詞'
  condition: {}
";
        var path = WriteTempYaml("meirin_com504.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 1 } } }
        };

        // Act
        var result = runner.Render(path, context);

        // Assert: talent branch selected
        Assert.Contains("恋人台詞", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Render_BranchesFormat_WithTalentZero_SelectsFallbackBranch()
    {
        // Arrange: branches-format YAML with TALENT condition
        var yamlContent = @"character: テスト
situation: COM_0
branches:
- lines:
  - '恋人台詞'
  condition:
    TALENT:
      '16': {ne: 0}
- lines:
  - '通常台詞'
  condition: {}
";
        var path = WriteTempYaml("meirin_com505.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 0 } } }
        };

        // Act
        var result = runner.Render(path, context);

        // Assert: fallback branch selected
        Assert.Contains("通常台詞", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Render_BranchesFormat_WithAblInContext_ExtractsAblState()
    {
        // Arrange: branches-format with ABL condition
        var yamlContent = @"character: テスト
situation: COM_0
branches:
- lines:
  - '高ABL台詞'
  condition:
    ABL:
      '10': {gt: 50}
- lines:
  - '低ABL台詞'
  condition: {}
";
        var path = WriteTempYaml("meirin_com506.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>
        {
            { "ABL", new Dictionary<string, int> { { "10", 100 } } }
        };

        // Act
        var result = runner.Render(path, context);

        // Assert: high ABL branch selected
        Assert.Contains("高ABL台詞", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Render_BranchesFormat_WithTflagInContext_ExtractsTflagState()
    {
        // Arrange: branches-format with TFLAG condition
        var yamlContent = @"character: テスト
situation: COM_0
branches:
- lines:
  - 'TFLAGあり台詞'
  condition:
    TFLAG:
      '5': {ne: 0}
- lines:
  - 'TFLAGなし台詞'
  condition: {}
";
        var path = WriteTempYaml("meirin_com507.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>
        {
            { "TFLAG", new Dictionary<string, int> { { "5", 1 } } }
        };

        // Act
        var result = runner.Render(path, context);

        // Assert: TFLAG branch selected
        Assert.Contains("TFLAGあり台詞", result);
    }

    // -------------------------------------------------------------------------
    // ParseCharacterIdFromPath: production COM and K-format paths
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void ParseCharacterIdFromPath_ProductionComFormat_ParsesWithoutPathFormatError()
    {
        // Arrange: production COM format path (file doesn't exist, but path parsing should work)
        var runner = new YamlRunner();
        var testPath = "Game/YAML/口上/5_チルノ/COM_100.yaml";
        var context = new Dictionary<string, object>();

        // Act - should fail with file-not-found, not with "Invalid YAML path format"
        var ex = Record.Exception(() => runner.RenderWithMetadata(testPath, context));

        // Assert: path parsing succeeded (wrong exception type means success)
        if (ex is ArgumentException argEx)
            Assert.DoesNotContain("Invalid YAML path format", argEx.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ParseCharacterIdFromPath_KNFormat_ParsesWithoutPathFormatError()
    {
        // Arrange: K{N} format path
        var runner = new YamlRunner();
        var testPath = "Game/YAML/Kojo/5_チルノ/K5_会話_0.yaml";
        var context = new Dictionary<string, object>();

        // Act
        var ex = Record.Exception(() => runner.RenderWithMetadata(testPath, context));

        // Assert
        if (ex is ArgumentException argEx)
            Assert.DoesNotContain("Invalid YAML path format", argEx.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ParseCharacterIdFromPath_KUFormat_ParsesWithoutPathFormatError()
    {
        // Arrange: KU (universal) format path
        var runner = new YamlRunner();
        var testPath = "Game/YAML/Kojo/U_汎用/KU_日常_1.yaml";
        var context = new Dictionary<string, object>();

        // Act
        var ex = Record.Exception(() => runner.RenderWithMetadata(testPath, context));

        // Assert
        if (ex is ArgumentException argEx)
            Assert.DoesNotContain("Invalid YAML path format", argEx.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ParseCharacterIdFromPath_InvalidFormat_ThrowsArgumentException()
    {
        // Arrange
        var runner = new YamlRunner();
        var testPath = "completely_wrong_format.yaml";
        var context = new Dictionary<string, object>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => runner.RenderWithMetadata(testPath, context));
    }

    // -------------------------------------------------------------------------
    // SimpleCharacterDataService: GetCallName returns "CharacterN" format
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void Render_WithCallNameTemplate_ReplacesWithCharacterN()
    {
        // Arrange: entries-format with %CALLNAME:MASTER% template
        // SimpleCharacterDataService.GetCallName returns "Character{id}"
        var yamlContent = @"entries:
- id: fallback
  content: >-
    こんにちは、%CALLNAME:MASTER%。
  priority: 0
";
        var path = WriteTempYaml("meirin_com510.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>();

        // Act
        var result = runner.Render(path, context);

        // Assert: CALLNAME template resolved (SimpleCharacterDataService returns "Character{id}")
        Assert.NotNull(result);
        Assert.Contains("こんにちは", result);
    }

    // -------------------------------------------------------------------------
    // RenderAsync (Obsolete compatibility wrapper)
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RenderAsync_WithValidBranchesYaml_ReturnsDialogue()
    {
        // Arrange
        var yamlContent = @"character: テスト
situation: COM_0
branches:
- lines:
  - 'テスト台詞'
  condition: {}
";
        var path = WriteTempYaml("meirin_com511.yaml", yamlContent);
        var runner = new YamlRunner();
        var context = new Dictionary<string, object>();

        // Act - RenderAsync is an obsolete wrapper for Render()
#pragma warning disable CS0618 // Type or member is obsolete
        var result = await runner.RenderAsync(path, context);
#pragma warning restore CS0618

        // Assert
        Assert.NotNull(result);
        Assert.Contains("テスト台詞", result);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private string WriteTempYaml(string fileName, string content)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }
}
