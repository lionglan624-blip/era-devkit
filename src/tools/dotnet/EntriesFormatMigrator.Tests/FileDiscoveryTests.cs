using Xunit;

namespace EntriesFormatMigrator.Tests;

public class FileDiscoveryTests
{
    [Fact]
    public void IsTargetFile_WithTalent_3_1_ReturnsTrue()
    {
        // Arrange
        var content = @"entries:
- id: fallback
  priority: 4
- id: talent_3_1
  priority: 3
- id: fallback
  priority: 2
- id: fallback
  priority: 1";

        // Act
        var result = FileDiscovery.IsTargetFile(content);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTargetFile_WithoutTalent_3_1_ReturnsFalse()
    {
        // Arrange
        var content = @"entries:
- id: fallback
  priority: 1";

        // Act
        var result = FileDiscovery.IsTargetFile(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateFourEntryStructure_WithValidFourEntries_ReturnsTrue()
    {
        // Arrange
        var content = "entries:\n" +
"- id: fallback\n" +
"  content: >-\n" +
"    Some content\n" +
"  priority: 4\n" +
"- id: talent_3_1\n" +
"  content: >-\n" +
"    Some content\n" +
"  priority: 3\n" +
"  condition:\n" +
"    type: Talent\n" +
"    talentType: 3\n" +
"    threshold: 1\n" +
"- id: fallback\n" +
"  content: >-\n" +
"    Some content\n" +
"  priority: 2\n" +
"- id: fallback\n" +
"  content: >-\n" +
"    Some content\n" +
"  priority: 1";

        // Act
        var result = FileDiscovery.ValidateFourEntryStructure(content);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateFourEntryStructure_WithSixEntries_ReturnsFalse()
    {
        // Arrange - NTR口上 pattern
        var content = @"entries:
- id: entry1
  priority: 6
- id: entry2
  priority: 5
- id: entry3
  priority: 4
- id: entry4
  priority: 3
- id: entry5
  priority: 2
- id: entry6
  priority: 1";

        // Act
        var result = FileDiscovery.ValidateFourEntryStructure(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateFourEntryStructure_WithOneEntry_ReturnsFalse()
    {
        // Arrange - EVENT/KU_日常 pattern
        var content = @"entries:
- id: fallback
  content: >-
    Some content
  priority: 1";

        // Act
        var result = FileDiscovery.ValidateFourEntryStructure(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateFourEntryStructure_WithWrongPriorityOrder_ReturnsFalse()
    {
        // Arrange
        var content = @"entries:
- id: entry1
  priority: 1
- id: entry2
  priority: 2
- id: entry3
  priority: 3
- id: entry4
  priority: 4";

        // Act
        var result = FileDiscovery.ValidateFourEntryStructure(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FindTargetFiles_FiltersCorrectly()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        mockFs.AddFile("test/file1.yaml", "character: Test\nentries:\n- id: fallback\n  priority: 4\n- id: talent_3_1\n  priority: 3\n- id: fallback\n  priority: 2\n- id: fallback\n  priority: 1");
        mockFs.AddFile("test/file2.yaml", "character: Test\nentries:\n- id: fallback\n  priority: 1");
        mockFs.AddFile("test/file3.yaml", "character: Test\nentries:\n- id: fallback\n  priority: 4\n- id: talent_3_1\n  priority: 3\n- id: fallback\n  priority: 2\n- id: fallback\n  priority: 1");

        var discovery = new FileDiscovery(mockFs);

        // Act
        var result = discovery.FindTargetFiles("test");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("test/file1.yaml", result);
        Assert.Contains("test/file3.yaml", result);
    }
}
