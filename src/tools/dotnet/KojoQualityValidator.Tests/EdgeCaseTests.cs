using Xunit;

namespace KojoQualityValidator.Tests;

public class EdgeCaseTests
{
    [Fact]
    public void QualityRule_DefaultConstructor_HasDefaultValues()
    {
        // Arrange & Act
        var rule = new QualityRule();

        // Assert
        Assert.Equal(4, rule.MinEntries);
        Assert.Equal(4, rule.MinLinesPerEntry);
    }

    [Fact]
    public void QualityRule_CustomConstruction_StoresCustomValues()
    {
        // Arrange & Act
        var rule = new QualityRule(MinEntries: 10, MinLinesPerEntry: 8);

        // Assert
        Assert.Equal(10, rule.MinEntries);
        Assert.Equal(8, rule.MinLinesPerEntry);
    }

    [Fact]
    public void QualityRule_PartialConstruction_UsesDefaults()
    {
        // Arrange & Act
        var rule = new QualityRule(MinEntries: 5);

        // Assert
        Assert.Equal(5, rule.MinEntries);
        Assert.Equal(4, rule.MinLinesPerEntry); // Default
    }

    [Fact]
    public void ValidationResult_Properties_AreInitializedCorrectly()
    {
        // Arrange & Act
        var result = new ValidationResult
        {
            FilePath = "test.yaml",
            IsValid = true,
            Errors = new List<string> { "error1", "error2" },
            EntryCount = 5
        };

        // Assert
        Assert.Equal("test.yaml", result.FilePath);
        Assert.True(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(5, result.EntryCount);
    }

    [Fact]
    public void ValidationResult_DefaultState_HasEmptyCollections()
    {
        // Arrange & Act
        var result = new ValidationResult();

        // Assert
        Assert.Empty(result.FilePath);
        Assert.False(result.IsValid); // Default bool value
        Assert.Empty(result.Errors);
        Assert.Equal(0, result.EntryCount);
    }

    [Fact]
    public void Validate_WithEmptyEntriesList_FailsEntryCountValidation()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 4, MinLinesPerEntry: 4);
        var filePath = Path.Combine("TestData", "empty-entries.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(0, result.EntryCount);
        Assert.Single(result.Errors);
        Assert.Contains("Entry count 0 < 4", result.Errors);
    }

    [Fact]
    public void Validate_WithMixedEntryQuality_FailsOnlyInsufficientEntries()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 5, MinLinesPerEntry: 4);
        var filePath = Path.Combine("TestData", "mixed-quality.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(5, result.EntryCount);
        Assert.Equal(2, result.Errors.Count); // Entry[2] and Entry[3] have < 4 lines
        Assert.Contains(result.Errors, e => e.Contains("Entry[2]"));
        Assert.Contains(result.Errors, e => e.Contains("Entry[3]"));
    }

    [Fact]
    public void Validate_WithWhitespaceLines_CountsOnlyNonEmptyLines()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 4, MinLinesPerEntry: 4);
        var filePath = Path.Combine("TestData", "whitespace-lines.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(4, result.EntryCount);
        // Entry[0] has only 2 non-empty lines, Entry[1] has only 2 non-empty lines
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Contains("Entry[0]") && e.Contains("Lines 2 < 4"));
        Assert.Contains(result.Errors, e => e.Contains("Entry[1]") && e.Contains("Lines 2 < 4"));
    }

    [Fact]
    public void Validate_EntryCountInResult_MatchesActualFileEntryCount()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 1, MinLinesPerEntry: 1);
        var filePath = Path.Combine("TestData", "quality-pass.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(4, result.EntryCount); // quality-pass.yaml has exactly 4 entries
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithZeroMinimums_AlwaysPasses()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 0, MinLinesPerEntry: 0);
        var filePath = Path.Combine("TestData", "empty-entries.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(0, result.EntryCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithHighMinEntries_FailsWithSpecificErrorMessage()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 100, MinLinesPerEntry: 1);
        var filePath = Path.Combine("TestData", "quality-pass.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(4, result.EntryCount);
        Assert.Contains("Entry count 4 < 100", result.Errors);
    }

    [Fact]
    public void Validate_WithHighMinLinesPerEntry_FailsAllEntries()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 1, MinLinesPerEntry: 100);
        var filePath = Path.Combine("TestData", "quality-pass.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(4, result.EntryCount);
        // All 4 entries should fail (each has only 4 lines)
        Assert.Equal(4, result.Errors.Count);
        Assert.All(result.Errors, error => Assert.Contains("Lines 4 < 100", error));
    }

    [Fact]
    public void Validate_FilePathInResult_MatchesInputPath()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule();
        var filePath = Path.Combine("TestData", "quality-pass.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.Equal(filePath, result.FilePath);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ExactThreshold_Passes()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 4, MinLinesPerEntry: 4);
        var filePath = Path.Combine("TestData", "quality-pass.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(4, result.EntryCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_OneEntryBelowThreshold_FailsWithSpecificIndex()
    {
        // Arrange
        var validator = new QualityValidator();
        var rule = new QualityRule(MinEntries: 2, MinLinesPerEntry: 4);
        var filePath = Path.Combine("TestData", "quality-fail.yaml");

        // Act
        var result = validator.Validate(filePath, rule);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.EntryCount);
        Assert.Single(result.Errors);
        Assert.Contains("Entry[0]: Lines 3 < 4", result.Errors);
    }
}
