using Xunit;

namespace KojoQualityValidator.Tests;

public class QualityValidatorTests
{
    [Fact]
    public void Validate_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 4, MinLinesPerEntry: 4);
        var filePath = Path.Combine("TestData", "quality-pass.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(4, result.EntryCount);
    }

    [Fact]
    public void Validate_WithInsufficientEntries_ReturnsFailure()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 4, MinLinesPerEntry: 4);
        var filePath = Path.Combine("TestData", "quality-fail.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Entry count"));
    }

    [Fact]
    public void Validate_WithInsufficientLines_ReturnsFailure()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 2, MinLinesPerEntry: 4);
        var filePath = Path.Combine("TestData", "quality-fail.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Entry[0]") && e.Contains("Lines"));
    }

    [Fact]
    public void Validate_WithCustomThresholds_AppliesCorrectly()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 2, MinLinesPerEntry: 3);
        var filePath = Path.Combine("TestData", "quality-fail.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert - should pass with lower thresholds
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithNonExistentFile_ThrowsException()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule();
        var filePath = "nonexistent.yaml";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => validator.Validate(filePath, rule));
    }
}
