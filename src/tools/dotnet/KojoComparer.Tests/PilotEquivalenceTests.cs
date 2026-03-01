using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for F351 pilot data equivalence (AC#8).
/// Verifies 美鈴 COM_0 YAML rendering across all TALENT states.
/// </summary>
public class PilotEquivalenceTests
{
    // Test data path: tools/ErbToYaml.Tests/TestOutput/1_美鈴/COM_0.yaml

    private readonly YamlRunner _yamlRunner;
    private readonly OutputNormalizer _normalizer;

    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string YamlFilePath = Path.Combine(RepoRoot, "src", "tools", "dotnet", "ErbToYaml.Tests", "TestOutput", "1_美鈴", "COM_0.yaml");

    public PilotEquivalenceTests()
    {
        _yamlRunner = new YamlRunner();
        _normalizer = new OutputNormalizer();
    }

    /// <summary>
    /// Finds the repository root by walking up from the current directory.
    /// </summary>
    private static string FindRepoRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        throw new InvalidOperationException("Could not find repository root (no .git directory found)");
    }

    // ====================================================================
    // Unit Tests: YAML-only validation (no ERB runner required)
    // These tests validate YAML rendering in isolation to satisfy AC#4
    // ====================================================================

    [Fact]
    [Trait("Category", "Unit")]
    public void PilotYamlUnit_Lover_RendersExpectedContent()
    {
        // Arrange: TALENT:16 ne 0 (恋人)
        var yamlContext = new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 1 }, { "3", 0 }, { "17", 0 } } }
        };

        // Act
        var yamlOutput = _yamlRunner.Render(YamlFilePath, yamlContext);
        var normalized = _normalizer.Normalize(yamlOutput);

        // Assert: Verify key content from lover branch
        Assert.NotEmpty(normalized);
        Assert.Contains("気持ちいい", normalized);  // First line: "んっ……そこ、気持ちいい……"
        Assert.Contains("恋人に触れられる幸せ", normalized);  // Line 9: characteristic phrase
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void PilotYamlUnit_Yearning_RendersExpectedContent()
    {
        // Arrange: TALENT:3 ne 0 (恋慕)
        var yamlContext = new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 0 }, { "3", 1 }, { "17", 0 } } }
        };

        // Act
        var yamlOutput = _yamlRunner.Render(YamlFilePath, yamlContext);
        var normalized = _normalizer.Normalize(yamlOutput);

        // Assert: Verify key content from yearning branch
        Assert.NotEmpty(normalized);
        Assert.Contains("ちょ、ちょっと", normalized);  // First line: "ひゃっ……！　ちょ、ちょっと%CALLNAME:MASTER%……"
        Assert.Contains("嫌いになれないのが、悔しい", normalized);  // Line 52: characteristic phrase
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void PilotYamlUnit_DifferentTalentStates_ProduceDifferentOutputs()
    {
        // Arrange: Two different TALENT states
        var loverContext = new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 1 }, { "3", 0 }, { "17", 0 } } }
        };

        var yearningContext = new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 0 }, { "3", 1 }, { "17", 0 } } }
        };

        // Act
        var loverOutput = _yamlRunner.Render(YamlFilePath, loverContext);
        var yearningOutput = _yamlRunner.Render(YamlFilePath, yearningContext);

        // Assert: Different TALENT states should produce different outputs
        Assert.NotEqual(loverOutput, yearningOutput);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void PilotYamlUnit_AllBranchConditions_SelectCorrectBranch()
    {
        // Test all 4 branches from meirin_com0.yaml

        // Branch 1: TALENT:16 ne 0 (恋人)
        var lover = _yamlRunner.Render(YamlFilePath, new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 1 }, { "3", 0 }, { "17", 0 } } }
        });
        Assert.Contains("恋人に触れられる幸せ", lover);

        // Branch 2: TALENT:3 ne 0 (恋慕)
        var yearning = _yamlRunner.Render(YamlFilePath, new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 0 }, { "3", 1 }, { "17", 0 } } }
        });
        Assert.Contains("嫌いになれないのが、悔しい", yearning);

        // Branch 3: TALENT:17 ne 0 (思慕)
        var admiration = _yamlRunner.Render(YamlFilePath, new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 0 }, { "3", 0 }, { "17", 1 } } }
        });
        Assert.Contains("意外と大胆", admiration);

        // Branch 4: No conditions (なし)
        var none = _yamlRunner.Render(YamlFilePath, new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 0 }, { "3", 0 }, { "17", 0 } } }
        });
        Assert.Contains("何するんですか", none);

        // All branches should be different
        Assert.NotEqual(lover, yearning);
        Assert.NotEqual(lover, admiration);
        Assert.NotEqual(lover, none);
        Assert.NotEqual(yearning, admiration);
        Assert.NotEqual(yearning, none);
        Assert.NotEqual(admiration, none);
    }
}
