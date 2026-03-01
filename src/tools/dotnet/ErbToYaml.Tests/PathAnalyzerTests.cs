using System;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// TDD tests for Feature 634: Batch Conversion Tool - PathAnalyzer component
/// RED state tests - implementation does not exist yet
/// Tests AC#14
/// </summary>
public class PathAnalyzerTests
{
    /// <summary>
    /// AC#14: Test extraction from standard pattern
    /// Expected: Path "1_美鈴/KOJO_K1_愛撫.ERB" extracts character=美鈴, situation=K1_愛撫
    /// </summary>
    [Fact]
    public void Test_Extract_StandardPattern()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var erbPath = @"C:\Game\ERB\口上\1_美鈴\KOJO_K1_愛撫.ERB";

        // Act
        var (character, situation) = analyzer.Extract(erbPath);

        // Assert
        Assert.Equal("美鈴", character);
        Assert.Equal("K1_愛撫", situation);
    }

    /// <summary>
    /// AC#14: Test extraction with different numbered directory
    /// Expected: Handles various numbered prefixes (2_, 3_, 10_, etc.)
    /// </summary>
    [Fact]
    public void Test_Extract_DifferentNumberedDirectory()
    {
        // Arrange
        var analyzer = new PathAnalyzer();

        // Test various numbered directories
        var testCases = new[]
        {
            (@"口上\2_咲夜\KOJO_K2_Test.ERB", "咲夜", "K2_Test"),
            (@"口上\3_レミリア\KOJO_K3_Sample.ERB", "レミリア", "K3_Sample"),
            (@"口上\10_パチュリー\KOJO_K10_Multi.ERB", "パチュリー", "K10_Multi"),
            (@"C:\Path\11_フラン\KOJO_K11_Test.ERB", "フラン", "K11_Test")
        };

        foreach (var (path, expectedChar, expectedSit) in testCases)
        {
            // Act
            var (character, situation) = analyzer.Extract(path);

            // Assert
            Assert.Equal(expectedChar, character);
            Assert.Equal(expectedSit, situation);
        }
    }

    /// <summary>
    /// AC#14: Test extraction with filename containing multiple underscores
    /// Expected: Handles KOJO_K1_愛撫_詳細.ERB → situation=K1_愛撫_詳細
    /// </summary>
    [Fact]
    public void Test_Extract_FilenameWithMultipleUnderscores()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var erbPath = @"1_美鈴\KOJO_K1_愛撫_詳細_追加.ERB";

        // Act
        var (character, situation) = analyzer.Extract(erbPath);

        // Assert
        Assert.Equal("美鈴", character);
        Assert.Equal("K1_愛撫_詳細_追加", situation);
    }

    /// <summary>
    /// AC#14: Test extraction with Windows path separators
    /// Expected: Handles backslash path separators
    /// </summary>
    [Fact]
    public void Test_Extract_WindowsPathSeparators()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var erbPath = @"C:\Era\erakoumakanNTR\Game\ERB\口上\1_美鈴\KOJO_K1_愛撫.ERB";

        // Act
        var (character, situation) = analyzer.Extract(erbPath);

        // Assert
        Assert.Equal("美鈴", character);
        Assert.Equal("K1_愛撫", situation);
    }

    /// <summary>
    /// AC#14: Test extraction with Unix path separators
    /// Expected: Handles forward slash path separators
    /// </summary>
    [Fact]
    public void Test_Extract_UnixPathSeparators()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var erbPath = "口上/1_美鈴/KOJO_K1_愛撫.ERB";

        // Act
        var (character, situation) = analyzer.Extract(erbPath);

        // Assert
        Assert.Equal("美鈴", character);
        Assert.Equal("K1_愛撫", situation);
    }

    /// <summary>
    /// AC#14: Test extraction error on non-matching path (no numbered directory)
    /// Expected: Throws ArgumentException when directory doesn't match N_Character pattern
    /// </summary>
    [Fact]
    public void Test_Extract_NoNumberedDirectory_Throws()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var invalidPath = @"口上\美鈴\KOJO_K1_愛撫.ERB"; // Missing number prefix

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            analyzer.Extract(invalidPath);
        });

        Assert.Contains("does not match expected pattern", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// AC#14: Test extraction with non-KOJO prefix file in numbered directory
    /// With fallback pattern, non-KOJO files in numbered directories now extract successfully
    /// </summary>
    [Fact]
    public void Test_Extract_NoKojoPrefix_FallbackSucceeds()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var path = @"1_美鈴\K1_愛撫.ERB";

        // Act
        var (character, situation) = analyzer.Extract(path);

        // Assert
        Assert.Equal("美鈴", character);
        Assert.Equal("K1_愛撫", situation);
    }

    /// <summary>
    /// AC#14: Test extraction error on missing .ERB extension
    /// Expected: Throws ArgumentException when file doesn't have .ERB extension
    /// </summary>
    [Fact]
    public void Test_Extract_NoErbExtension_Throws()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var invalidPath = @"1_美鈴\KOJO_K1_愛撫.txt"; // Wrong extension

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            analyzer.Extract(invalidPath);
        });

        Assert.Contains("does not match expected pattern", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// AC#14: Test extraction with relative path
    /// Expected: Handles relative paths (just directory and filename)
    /// </summary>
    [Fact]
    public void Test_Extract_RelativePath()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var relativePath = @"1_美鈴\KOJO_K1_愛撫.ERB";

        // Act
        var (character, situation) = analyzer.Extract(relativePath);

        // Assert
        Assert.Equal("美鈴", character);
        Assert.Equal("K1_愛撫", situation);
    }

    /// <summary>
    /// AC#14: Test extraction with deep nested path
    /// Expected: Extracts from nearest numbered directory and filename
    /// </summary>
    [Fact]
    public void Test_Extract_DeepNestedPath()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var deepPath = @"C:\Era\erakoumakanNTR\Game\ERB\口上\1_美鈴\KOJO_K1_愛撫.ERB";

        // Act
        var (character, situation) = analyzer.Extract(deepPath);

        // Assert
        Assert.Equal("美鈴", character);
        Assert.Equal("K1_愛撫", situation);
    }

    /// <summary>
    /// AC#14: Test extraction with all 11 character directories
    /// Expected: Handles all actual character directory names from the game
    /// </summary>
    [Fact]
    public void Test_Extract_AllCharacterDirectories()
    {
        // Arrange - Test with representative characters from the game
        var analyzer = new PathAnalyzer();

        var testCases = new[]
        {
            (@"1_美鈴\KOJO_K1_Test.ERB", "美鈴"),
            (@"2_咲夜\KOJO_K2_Test.ERB", "咲夜"),
            (@"3_レミリア\KOJO_K3_Test.ERB", "レミリア"),
            (@"4_フラン\KOJO_K4_Test.ERB", "フラン"),
            (@"5_パチュリー\KOJO_K5_Test.ERB", "パチュリー"),
        };

        foreach (var (path, expectedChar) in testCases)
        {
            // Act
            var (character, _) = analyzer.Extract(path);

            // Assert
            Assert.Equal(expectedChar, character);
        }
    }

    /// <summary>
    /// AC#14: Test extraction error with null or empty path
    /// Expected: Throws ArgumentException for invalid input
    /// </summary>
    [Fact]
    public void Test_Extract_NullOrEmpty_Throws()
    {
        // Arrange
        var analyzer = new PathAnalyzer();

        // Act & Assert - Null path
        Assert.Throws<ArgumentException>(() => analyzer.Extract(null!));

        // Act & Assert - Empty path
        Assert.Throws<ArgumentException>(() => analyzer.Extract(string.Empty));

        // Act & Assert - Whitespace path
        Assert.Throws<ArgumentException>(() => analyzer.Extract("   "));
    }

    /// <summary>
    /// AC#14: Test extraction with Japanese characters in path
    /// Expected: Correctly handles full-width and half-width characters
    /// </summary>
    [Fact]
    public void Test_Extract_JapaneseCharacters()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var japanesePath = @"1_紅美鈴\KOJO_K1_愛撫.ERB";

        // Act
        var (character, situation) = analyzer.Extract(japanesePath);

        // Assert
        Assert.Equal("紅美鈴", character);
        Assert.Equal("K1_愛撫", situation);
    }

    /// <summary>
    /// AC#14: Test extraction with COM situation pattern
    /// Expected: Handles KOJO_COM_K1_0.ERB → situation=COM_K1_0
    /// </summary>
    [Fact]
    public void Test_Extract_ComSituationPattern()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var comPath = @"1_美鈴\KOJO_COM_K1_0.ERB";

        // Act
        var (character, situation) = analyzer.Extract(comPath);

        // Assert
        Assert.Equal("美鈴", character);
        Assert.Equal("COM_K1_0", situation);
    }

    /// <summary>
    /// AC#14: Test extraction case sensitivity
    /// Expected: Handles lowercase .erb extension (edge case)
    /// </summary>
    [Fact]
    public void Test_Extract_LowercaseExtension()
    {
        // Arrange
        var analyzer = new PathAnalyzer();
        var lowercasePath = @"1_美鈴\KOJO_K1_愛撫.erb";

        // Act
        var (character, situation) = analyzer.Extract(lowercasePath);

        // Assert
        Assert.Equal("美鈴", character);
        Assert.Equal("K1_愛撫", situation);
    }

    /// <summary>
    /// F639 AC#1: Test extraction from NTR口上 prefix pattern
    /// Expected: Path "4_咲夜\NTR口上_シナリオ8.ERB" extracts character=咲夜, situation=NTR口上_シナリオ8
    /// </summary>
    [Fact]
    public void Test_Extract_NtrKojoPrefix()
    {
        var analyzer = new PathAnalyzer();
        var (character, situation) = analyzer.Extract(@"4_咲夜\NTR口上_シナリオ8.ERB");
        Assert.Equal("咲夜", character);
        Assert.Equal("NTR口上_シナリオ8", situation);
    }

    /// <summary>
    /// F639 AC#1: Test extraction from SexHara prefix pattern
    /// Expected: Path "4_咲夜\SexHara休憩中口上.ERB" extracts character=咲夜, situation=SexHara休憩中口上
    /// </summary>
    [Fact]
    public void Test_Extract_SexHaraPrefix()
    {
        var analyzer = new PathAnalyzer();
        var (character, situation) = analyzer.Extract(@"4_咲夜\SexHara休憩中口上.ERB");
        Assert.Equal("咲夜", character);
        Assert.Equal("SexHara休憩中口上", situation);
    }

    /// <summary>
    /// F639 AC#1: Test extraction from WC系 prefix pattern
    /// Expected: Path "4_咲夜\WC系口上.ERB" extracts character=咲夜, situation=WC系口上
    /// </summary>
    [Fact]
    public void Test_Extract_WcKojoPrefix()
    {
        var analyzer = new PathAnalyzer();
        var (character, situation) = analyzer.Extract(@"4_咲夜\WC系口上.ERB");
        Assert.Equal("咲夜", character);
        Assert.Equal("WC系口上", situation);
    }

    /// <summary>
    /// F639 AC#1: Test extraction from NTR prefix with different character
    /// Expected: Path "1_美鈴\NTR口上_シナリオ1.ERB" extracts character=美鈴, situation=NTR口上_シナリオ1
    /// </summary>
    [Fact]
    public void Test_Extract_NtrPrefix_DifferentCharacter()
    {
        var analyzer = new PathAnalyzer();
        var (character, situation) = analyzer.Extract(@"1_美鈴\NTR口上_シナリオ1.ERB");
        Assert.Equal("美鈴", character);
        Assert.Equal("NTR口上_シナリオ1", situation);
    }

    /// <summary>
    /// F643 AC#2: Test extraction from U_汎用 directory with KOJO_ prefix
    /// Expected: Path "U_汎用\KOJO_KU_日常.ERB" extracts character=汎用, situation=KU_日常
    /// </summary>
    [Fact]
    public void Test_Extract_GenericKojoDirectory_KojoPrefix()
    {
        var analyzer = new PathAnalyzer();
        var (character, situation) = analyzer.Extract(@"U_汎用\KOJO_KU_日常.ERB");
        Assert.Equal("汎用", character);
        Assert.Equal("KU_日常", situation);
    }

    /// <summary>
    /// F643 AC#2: Test extraction from U_汎用 directory without KOJO_ prefix
    /// Expected: Path "U_汎用\NTR口上.ERB" extracts character=汎用, situation=NTR口上
    /// </summary>
    [Fact]
    public void Test_Extract_GenericKojoDirectory_NonKojoPrefix()
    {
        var analyzer = new PathAnalyzer();
        var (character, situation) = analyzer.Extract(@"U_汎用\NTR口上.ERB");
        Assert.Equal("汎用", character);
        Assert.Equal("NTR口上", situation);
    }

    /// <summary>
    /// F643 AC#2: Test extraction from U_汎用 with full path
    /// Expected: Full path extracts character=汎用, situation=KU_日常
    /// </summary>
    [Fact]
    public void Test_Extract_GenericKojoDirectory_FullPath()
    {
        var analyzer = new PathAnalyzer();
        var path = @"C:\Era\erakoumakanNTR\Game\ERB\口上\U_汎用\KOJO_KU_日常.ERB";
        var (character, situation) = analyzer.Extract(path);
        Assert.Equal("汎用", character);
        Assert.Equal("KU_日常", situation);
    }
}
