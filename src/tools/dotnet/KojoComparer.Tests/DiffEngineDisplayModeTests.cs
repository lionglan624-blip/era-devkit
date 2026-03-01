using Era.Core.Dialogue;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for DiffEngine DisplayMode comparison (F677 AC#3, AC#4).
/// </summary>
public class DiffEngineDisplayModeTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Compare_WithDisplayModeMismatch_ReportsInDifferences()
    {
        var engine = new DiffEngine();
        var modesA = new List<DisplayMode> { DisplayMode.Wait };
        var modesB = new List<DisplayMode> { DisplayMode.Newline };

        var result = engine.Compare("text", "text", modesA, modesB);

        Assert.True(result.IsMatch); // text matches
        Assert.NotEmpty(result.DisplayModeDifferences);
        Assert.Contains(result.DisplayModeDifferences, d => d.Contains("differs"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Compare_WithMissingERBDisplayMode_ReportsInformational()
    {
        var engine = new DiffEngine();
        var modesB = new List<DisplayMode> { DisplayMode.Newline };

        var result = engine.Compare("text", "text", null, modesB);

        Assert.True(result.IsMatch); // text matches
        Assert.NotEmpty(result.DisplayModeDifferences);
        Assert.Contains(result.DisplayModeDifferences, d => d.Contains("YAML") && d.Contains("displayMode"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Compare_WithMatchingDisplayModes_EmptyDifferences()
    {
        var engine = new DiffEngine();
        var modes = new List<DisplayMode> { DisplayMode.Newline };

        var result = engine.Compare("text", "text", modes, modes);

        Assert.True(result.IsMatch);
        Assert.Empty(result.DisplayModeDifferences);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Compare_WithNullOnBothSides_EmptyDifferences()
    {
        var engine = new DiffEngine();

        var result = engine.Compare("text", "text", null, null);

        Assert.True(result.IsMatch);
        Assert.Empty(result.DisplayModeDifferences);
    }
}
