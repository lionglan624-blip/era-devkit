using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for OutputNormalizer (AC#3).
/// Verifies whitespace and formatting normalization.
/// </summary>
public class OutputNormalizerTests
{
    [Fact]
    public void Normalize_WithVariableWhitespace_RemovesWhitespaceVariance()
    {
        // Arrange
        var normalizer = new OutputNormalizer();
        var input = "\n\n  最近一緒にいると  \n心が温かくなるのを感じます\n\n";
        var expected = "最近一緒にいると\n心が温かくなるのを感じます";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_WithColorCodes_RemovesColorCodes()
    {
        // Arrange
        var normalizer = new OutputNormalizer();
        var input = "[COLOR 0xFF0000]赤いテキスト[COLOR 0x00FF00]緑のテキスト";
        var expected = "赤いテキスト緑のテキスト";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_WithFullwidthSpaces_NormalizesSpaces()
    {
        // Arrange
        var normalizer = new OutputNormalizer();
        var input = "これは　全角　スペース です";
        var expected = "これは 全角 スペース です";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_CallnamePatterns()
    {
        // Arrange
        var normalizer = new OutputNormalizer();
        var input = "%CALLNAME:人物_美鈴%は最近一緒にいると心が温かくなるのを感じます。" +
                    "%CALLNAME:MASTER%は%CALLNAME:人物_咲夜%に話しかけた。" +
                    "%CALLNAME:UNKNOWN%パターンもテストします。";
        var expected = "<CALLNAME:CHAR>は最近一緒にいると心が温かくなるのを感じます。" +
                       "<CALLNAME:MASTER>は<CALLNAME:CHAR>に話しかけた。" +
                       "<CALLNAME:UNKNOWN>パターンもテストします。";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_ErbCallnamePatterns()
    {
        // Arrange
        var normalizer = new OutputNormalizer();
        var input = "<CALLNAME:人物_美鈴>は最近一緒にいると心が温かくなるのを感じます。" +
                    "<CALLNAME:MASTER>は<CALLNAME:人物_咲夜>に話しかけた。";
        var expected = "<CALLNAME:CHAR>は最近一緒にいると心が温かくなるのを感じます。" +
                       "<CALLNAME:MASTER>は<CALLNAME:CHAR>に話しかけた。";

        // Act
        var result = normalizer.Normalize(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_MixedCallnameFormats()
    {
        // Arrange
        var normalizer = new OutputNormalizer();
        // ERB output (angle brackets) and YAML output (percent signs) mixed
        var erbOutput = "<CALLNAME:MASTER>は<CALLNAME:人物_美鈴>と話す";
        var yamlOutput = "%CALLNAME:MASTER%は%CALLNAME:人物_美鈴%と話す";
        var expected = "<CALLNAME:MASTER>は<CALLNAME:CHAR>と話す";

        // Act
        var erbResult = normalizer.Normalize(erbOutput);
        var yamlResult = normalizer.Normalize(yamlOutput);

        // Assert - Both should normalize to the same result
        Assert.Equal(expected, erbResult);
        Assert.Equal(expected, yamlResult);
        Assert.Equal(erbResult, yamlResult);
    }
}
