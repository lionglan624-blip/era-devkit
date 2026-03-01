using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for DiffEngine (AC#4).
/// Verifies line-by-line comparison and mismatch reporting.
/// </summary>
public class DiffEngineTests
{
    [Fact]
    public void Compare_WithMatchingStrings_ReturnsMatch()
    {
        // Arrange
        var engine = new DiffEngine();
        var textA = "最近一緒にいると\n心が温かくなるのを感じます";
        var textB = "最近一緒にいると\n心が温かくなるのを感じます";

        // Act
        var result = engine.Compare(textA, textB);

        // Assert
        Assert.True(result.IsMatch);
        Assert.Empty(result.Differences);
    }

    [Fact]
    public void Compare_WithOneLineDifference_ReportsLineNumberAndDiff()
    {
        // Arrange
        var engine = new DiffEngine();
        var textA = "最近一緒にいると\n心が温かくなるのを感じます";
        var textB = "最近一緒にいると\n心がポカポカしますわ";

        // Act
        var result = engine.Compare(textA, textB);

        // Assert
        Assert.False(result.IsMatch);
        Assert.Contains("Line 2 differs:", result.Differences);
        Assert.Contains("  ERB:  \"心が温かくなるのを感じます\"", result.Differences);
        Assert.Contains("  YAML: \"心がポカポカしますわ\"", result.Differences);
    }

    [Fact]
    public void Compare_WithMultipleLineDifferences_ReportsAllDifferences()
    {
        // Arrange
        var engine = new DiffEngine();
        var textA = "一行目\n二行目\n三行目";
        var textB = "一行目が違う\n二行目\n三行目も違う";

        // Act
        var result = engine.Compare(textA, textB);

        // Assert
        Assert.False(result.IsMatch);
        Assert.Contains("Line 1 differs:", result.Differences);
        Assert.Contains("Line 3 differs:", result.Differences);
    }

    /// <summary>
    /// AC#2: CompareSubset returns IsMatch=true when all ERB lines exist in YAML content.
    /// </summary>
    [Fact]
    public void Subset_AllLinesExist_ReturnsMatch()
    {
        // Arrange
        var engine = new DiffEngine();

        // ERB output: 1 randomly-selected PRINTDATA pattern (6 lines)
        var erbOutput = "最近一緒にいると\n心が温かくなるのを感じます\nこれが……恋、ですか？\n私、嬉しいです\nマスター様のそばにいられて\n本当に幸せです";

        // YAML content: All 4 concatenated DATALIST patterns (24 lines)
        var yamlContent =
            "最近一緒にいると\n心が温かくなるのを感じます\nこれが……恋、ですか？\n私、嬉しいです\nマスター様のそばにいられて\n本当に幸せです\n" + // Pattern 1
            "マスター様を見ていると\n胸がドキドキします\nこんな気持ち初めてで\n戸惑ってしまいます\nでも、悪くない感じです\nむしろ、嬉しいかも\n" + // Pattern 2
            "いつもお世話になっています\n感謝の気持ちでいっぱいです\n本当にありがとうございます\nこれからもよろしくお願いします\n一緒にいられて光栄です\n毎日が楽しいです\n" + // Pattern 3
            "あなたのことを考えると\n自然と笑顔になります\nそばにいてくれるだけで\n心が落ち着きます\n大切な存在です\n離れたくないです"; // Pattern 4

        // Act
        var result = engine.CompareSubset(erbOutput, yamlContent);

        // Assert
        Assert.True(result.IsMatch);
        Assert.Empty(result.Differences);
    }

    /// <summary>
    /// AC#3: CompareSubset returns IsMatch=false when ERB contains line not in YAML.
    /// </summary>
    [Fact]
    public void Subset_ExtraLines_ReturnsMismatch()
    {
        // Arrange
        var engine = new DiffEngine();

        // ERB output with a fabricated line not in YAML
        var erbOutput = "最近一緒にいると\n心が温かくなるのを感じます\nこの台詞はYAMLに存在しません\n私、嬉しいです\nマスター様のそばにいられて\n本当に幸せです";

        // YAML content: All 4 concatenated DATALIST patterns
        var yamlContent =
            "最近一緒にいると\n心が温かくなるのを感じます\nこれが……恋、ですか？\n私、嬉しいです\nマスター様のそばにいられて\n本当に幸せです\n" +
            "マスター様を見ていると\n胸がドキドキします\nこんな気持ち初めてで\n戸惑ってしまいます\nでも、悪くない感じです\nむしろ、嬉しいかも\n" +
            "いつもお世話になっています\n感謝の気持ちでいっぱいです\n本当にありがとうございます\nこれからもよろしくお願いします\n一緒にいられて光栄です\n毎日が楽しいです\n" +
            "あなたのことを考えると\n自然と笑顔になります\nそばにいてくれるだけで\n心が落ち着きます\n大切な存在です\n離れたくないです";

        // Act
        var result = engine.CompareSubset(erbOutput, yamlContent);

        // Assert
        Assert.False(result.IsMatch);
        Assert.Contains("ERB line 3 not found in YAML:", result.Differences);
        Assert.Contains("  \"この台詞はYAMLに存在しません\"", result.Differences);
    }

    /// <summary>
    /// AC#4: CompareSubset verifies ALL ERB lines exist (not partial match).
    /// Tests that 5/6 matching lines still returns IsMatch=false.
    /// </summary>
    [Fact]
    public void Subset_PartialMatch_ReturnsMismatch()
    {
        // Arrange
        var engine = new DiffEngine();

        // ERB output: 6 lines where only 5 exist in YAML (line 4 is modified)
        var erbOutput = "最近一緒にいると\n心が温かくなるのを感じます\nこれが……恋、ですか？\n部分的に違う台詞です\nマスター様のそばにいられて\n本当に幸せです";

        // YAML content: All 4 concatenated DATALIST patterns
        var yamlContent =
            "最近一緒にいると\n心が温かくなるのを感じます\nこれが……恋、ですか？\n私、嬉しいです\nマスター様のそばにいられて\n本当に幸せです\n" +
            "マスター様を見ていると\n胸がドキドキします\nこんな気持ち初めてで\n戸惑ってしまいます\nでも、悪くない感じです\nむしろ、嬉しいかも\n" +
            "いつもお世話になっています\n感謝の気持ちでいっぱいです\n本当にありがとうございます\nこれからもよろしくお願いします\n一緒にいられて光栄です\n毎日が楽しいです\n" +
            "あなたのことを考えると\n自然と笑顔になります\nそばにいてくれるだけで\n心が落ち着きます\n大切な存在です\n離れたくないです";

        // Act
        var result = engine.CompareSubset(erbOutput, yamlContent);

        // Assert
        Assert.False(result.IsMatch);
        Assert.Contains("ERB line 4 not found in YAML:", result.Differences);
        Assert.Contains("  \"部分的に違う台詞です\"", result.Differences);
    }

    /// <summary>
    /// AC#8: CompareSubset catches completely fabricated ERB lines (false positive detection).
    /// </summary>
    [Fact]
    public void Subset_NonExistentLine_ReturnsMismatch()
    {
        // Arrange
        var engine = new DiffEngine();

        // ERB output: Completely fabricated dialogue not in YAML
        var erbOutput = "この台詞は完全に偽物です\n存在しないはずの内容\nテストデータに含まれていません";

        // YAML content: All 4 concatenated DATALIST patterns
        var yamlContent =
            "最近一緒にいると\n心が温かくなるのを感じます\nこれが……恋、ですか？\n私、嬉しいです\nマスター様のそばにいられて\n本当に幸せです\n" +
            "マスター様を見ていると\n胸がドキドキします\nこんな気持ち初めてで\n戸惑ってしまいます\nでも、悪くない感じです\nむしろ、嬉しいかも\n" +
            "いつもお世話になっています\n感謝の気持ちでいっぱいです\n本当にありがとうございます\nこれからもよろしくお願いします\n一緒にいられて光栄です\n毎日が楽しいです\n" +
            "あなたのことを考えると\n自然と笑顔になります\nそばにいてくれるだけで\n心が落ち着きます\n大切な存在です\n離れたくないです";

        // Act
        var result = engine.CompareSubset(erbOutput, yamlContent);

        // Assert
        Assert.False(result.IsMatch);
        Assert.Contains("ERB line 1 not found in YAML:", result.Differences);
        Assert.Contains("  \"この台詞は完全に偽物です\"", result.Differences);
        Assert.Contains("ERB line 2 not found in YAML:", result.Differences);
        Assert.Contains("  \"存在しないはずの内容\"", result.Differences);
        Assert.Contains("ERB line 3 not found in YAML:", result.Differences);
        Assert.Contains("  \"テストデータに含まれていません\"", result.Differences);
    }
}
