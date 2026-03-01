using Era.Core.Dialogue;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for F678 displayMode equivalence (AC#10).
/// Unit tests validate DiffEngine displayMode comparison behavior.
/// </summary>
public class DisplayModeEquivalenceTests
{
    private readonly DiffEngine _diffEngine;

    public DisplayModeEquivalenceTests()
    {
        _diffEngine = new DiffEngine();
    }

    // ====================================================================
    // Unit Tests: Verify test infrastructure assumptions
    // ====================================================================

    [Fact]
    [Trait("Category", "Unit")]
    public void DisplayModeUnit_YamlRendererSupportsDisplayMode()
    {
        // This test validates that Era.Core.Dialogue.DisplayMode enum exists and can be used
        // YamlRunner path validation requires specific format (N_CharacterName/COM_NNN.yaml)
        // So we use a separate test (YamlRunnerDisplayModeTests) for actual YamlRunner testing

        // Arrange: Test displayMode enum values exist
        var defaultMode = DisplayMode.Default;
        var newlineMode = DisplayMode.Newline;

        // Act: Create list of displayModes (simulating what YamlRunner would return)
        var displayModes = new List<DisplayMode> { defaultMode, newlineMode };

        // Assert: DisplayMode enum values are correct
        Assert.Equal(2, displayModes.Count);
        Assert.Equal(DisplayMode.Default, displayModes[0]);
        Assert.Equal(DisplayMode.Newline, displayModes[1]);

        // Also verify enum has expected values for kojo use
        Assert.True(Enum.IsDefined(typeof(DisplayMode), "Default"));
        Assert.True(Enum.IsDefined(typeof(DisplayMode), "Newline"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DisplayModeUnit_DiffEngineAcceptsDisplayModesParameters()
    {
        // Arrange: Test data with matching displayModes
        var textA = "テスト行";
        var textB = "テスト行";
        var displayModesA = new List<DisplayMode> { DisplayMode.Newline };
        var displayModesB = new List<DisplayMode> { DisplayMode.Newline };

        // Act: Call DiffEngine.Compare with displayModes parameters
        var result = _diffEngine.Compare(textA, textB, displayModesA, displayModesB);

        // Assert: Comparison succeeds with matching displayModes
        Assert.NotNull(result);
        Assert.True(result.IsMatch, "DiffEngine should accept displayModes parameters without error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DisplayModeUnit_DiffEngineDetectsMismatchedDisplayModes()
    {
        // Arrange: Test data with mismatched displayModes
        var textA = "テスト行";
        var textB = "テスト行";
        var displayModesA = new List<DisplayMode> { DisplayMode.Newline };
        var displayModesB = new List<DisplayMode> { DisplayMode.Default };

        // Act: Call DiffEngine.Compare with mismatched displayModes
        var result = _diffEngine.Compare(textA, textB, displayModesA, displayModesB);

        // Assert: Comparison detects displayMode mismatch
        // Note: IsMatch is true because text matches - displayMode differences are informational (F677 design)
        Assert.NotNull(result);
        Assert.True(result.IsMatch, "Text matches so IsMatch should be true");
        // DisplayMode differences are reported in DisplayModeDifferences list (not Differences)
        Assert.NotEmpty(result.DisplayModeDifferences);
        Assert.Contains(result.DisplayModeDifferences, diff => diff.Contains("displayMode") || diff.Contains("DisplayMode"));
    }

}
