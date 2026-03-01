using Era.Core;
using Era.Core.Dialogue;
using Era.Core.Types;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for YamlRunner displayMode extraction (F677 AC#5).
/// Verifies RenderWithMetadata() correctly extracts displayMode from YAML files.
/// </summary>
public class YamlRunnerDisplayModeTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void RenderWithMetadata_WithDisplayMode_ExtractsCorrectly()
    {
        // Arrange
        var repoRoot = FindRepoRoot();
        var yamlFilePath = Path.Combine(repoRoot, "src", "tools", "dotnet", "KojoComparer.Tests", "TestData", "meirin_com200.yaml");

        var yamlRunner = new YamlRunner();
        var context = new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 1 } } }
        };

        // Act
        var dialogueResult = yamlRunner.RenderWithMetadata(yamlFilePath, context);

        // Assert
        Assert.NotNull(dialogueResult);
        Assert.NotEmpty(dialogueResult.DialogueLines);
        Assert.Equal(DisplayMode.Newline, dialogueResult.DialogueLines[0].DisplayMode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RenderWithMetadata_WithoutDisplayMode_ReturnsDefault()
    {
        // Arrange
        var repoRoot = FindRepoRoot();
        var yamlFilePath = Path.Combine(repoRoot, "src", "tools", "dotnet", "KojoComparer.Tests", "TestData", "meirin_com201.yaml");

        var yamlRunner = new YamlRunner();
        var context = new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 1 } } }
        };

        // Act
        var dialogueResult = yamlRunner.RenderWithMetadata(yamlFilePath, context);

        // Assert
        Assert.NotNull(dialogueResult);
        Assert.NotEmpty(dialogueResult.DialogueLines);
        Assert.Equal(DisplayMode.Default, dialogueResult.DialogueLines[0].DisplayMode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Render_BackwardCompatibility_ReturnsTextOnly()
    {
        // Arrange
        var repoRoot = FindRepoRoot();
        var yamlFilePath = Path.Combine(repoRoot, "src", "tools", "dotnet", "KojoComparer.Tests", "TestData", "meirin_com200.yaml");

        var yamlRunner = new YamlRunner();
        var context = new Dictionary<string, object>
        {
            { "TALENT", new Dictionary<string, int> { { "16", 1 } } }
        };

        // Act
        var text = yamlRunner.Render(yamlFilePath, context);

        // Assert
        Assert.NotNull(text);
        Assert.IsType<string>(text);
        // Verify that Render() still returns text-only format (no change from current behavior)
        Assert.Contains("最近一緒にいると", text);
    }

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
}
