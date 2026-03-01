using Xunit;

namespace EntriesFormatMigrator.Tests;

public class EntryPatcherTests
{
    private const string SampleYaml = @"character: 美鈴
situation: K1_会話親密
entries:
- id: fallback
  content: >-
    Line1 of P4 content.

    Line2 of P4 content.
  priority: 4
- id: talent_3_1
  content: >-
    Line1 of P3 content.

    Line2 of P3 content.
  priority: 3
  condition:
    type: Talent
    talentType: 3
    threshold: 1
- id: fallback
  content: >-
    Line1 of P2 content.

    Line2 of P2 content.
  priority: 2
- id: fallback
  content: >-
    Line1 of P1 content.

    Line2 of P1 content.
  priority: 1";

    [Fact]
    public void PatchEntries_WithStandardFourEntryFile_RenamesP4AndP2IdsAndAddsConditions()
    {
        // Arrange
        var patcher = new EntryPatcher();

        // Act
        var result = patcher.PatchEntries(SampleYaml);

        // Assert
        Assert.Contains("id: talent_16_0", result);
        Assert.Contains("id: talent_17_0", result);
        Assert.Contains("id: talent_3_1", result); // P3 unchanged
        Assert.Equal(1, CountOccurrences(result, "id: fallback")); // Only P1 should have fallback

        // Verify P4 condition (after P4's priority line)
        Assert.Contains("priority: 4\n  condition:\n    type: Talent\n    talentType: 16\n    threshold: 1", result);

        // Verify P2 condition (after P2's priority line)
        Assert.Contains("priority: 2\n  condition:\n    type: Talent\n    talentType: 17\n    threshold: 1", result);

        // Verify P3 condition unchanged
        Assert.Contains("talentType: 3", result);

        // Verify P1 has no condition
        var p1Section = result.Substring(result.LastIndexOf("priority: 1"));
        Assert.DoesNotContain("condition:", p1Section);
    }

    [Fact]
    public void PatchEntries_WithAlreadyMigratedFile_NoChanges()
    {
        // Arrange
        var alreadyMigrated = @"character: 美鈴
situation: K1_会話親密
entries:
- id: talent_16_0
  content: >-
    Line1 of P4 content.
  priority: 4
  condition:
    type: Talent
    talentType: 16
    threshold: 1
- id: talent_3_1
  content: >-
    Line1 of P3 content.
  priority: 3
  condition:
    type: Talent
    talentType: 3
    threshold: 1
- id: talent_17_0
  content: >-
    Line1 of P2 content.
  priority: 2
  condition:
    type: Talent
    talentType: 17
    threshold: 1
- id: fallback
  content: >-
    Line1 of P1 content.
  priority: 1";

        var patcher = new EntryPatcher();

        // Act
        var result = patcher.PatchEntries(alreadyMigrated);

        // Assert - no changes should be made (normalize line endings for comparison)
        Assert.Equal(alreadyMigrated.Replace("\r\n", "\n"), result);
    }

    [Fact]
    public void EntryHasCondition_WithConditionBlock_ReturnsTrue()
    {
        // Arrange
        var entryText = @"- id: talent_3_1
  content: >-
    Some content
  priority: 3
  condition:
    type: Talent
    talentType: 3
    threshold: 1";

        var patcher = new EntryPatcher();

        // Act
        var result = patcher.EntryHasCondition(entryText);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EntryHasCondition_WithoutConditionBlock_ReturnsFalse()
    {
        // Arrange
        var entryText = @"- id: fallback
  content: >-
    Some content
  priority: 4";

        var patcher = new EntryPatcher();

        // Act
        var result = patcher.EntryHasCondition(entryText);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PatchEntries_PreservesContentIntegrity()
    {
        // Arrange
        var patcher = new EntryPatcher();

        // Act
        var result = patcher.PatchEntries(SampleYaml);

        // Assert - content blocks should remain unchanged
        Assert.Contains("Line1 of P4 content.", result);
        Assert.Contains("Line2 of P4 content.", result);
        Assert.Contains("Line1 of P3 content.", result);
        Assert.Contains("Line2 of P3 content.", result);
        Assert.Contains("Line1 of P2 content.", result);
        Assert.Contains("Line2 of P2 content.", result);
        Assert.Contains("Line1 of P1 content.", result);
        Assert.Contains("Line2 of P1 content.", result);
    }

    [Fact]
    public void PatchEntries_P1EntryKeepsFallbackId()
    {
        // Arrange
        var patcher = new EntryPatcher();

        // Act
        var result = patcher.PatchEntries(SampleYaml);

        // Assert - P1 should still have "id: fallback"
        var lines = result.Split('\n');
        var p1IdLineIndex = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("priority: 1"))
            {
                // Find the id line before priority: 1
                for (int j = i - 1; j >= 0; j--)
                {
                    if (lines[j].Contains("- id:"))
                    {
                        p1IdLineIndex = j;
                        break;
                    }
                }
                break;
            }
        }

        Assert.True(p1IdLineIndex >= 0, "P1 id line not found");
        Assert.Contains("id: fallback", lines[p1IdLineIndex]);
    }

    [Fact]
    public void PatchEntries_WithEmptyContent_ReturnsEmptyString()
    {
        // Arrange
        var patcher = new EntryPatcher();
        var emptyContent = "";

        // Act
        var result = patcher.PatchEntries(emptyContent);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void PatchEntries_WithCRLFLineEndings_NormalizesToLF()
    {
        // Arrange
        var patcher = new EntryPatcher();
        var crlfContent = "character: 美鈴\r\nsituation: K1_会話親密\r\nentries:\r\n- id: fallback\r\n  content: >-\r\n    P4 content\r\n  priority: 4\r\n- id: fallback\r\n  content: >-\r\n    P1 content\r\n  priority: 1";

        // Act
        var result = patcher.PatchEntries(crlfContent);

        // Assert
        Assert.DoesNotContain("\r\n", result); // Should be normalized to \n
        Assert.Contains("id: talent_16_0", result); // Should still apply migrations
        Assert.Contains("priority: 4\n  condition:", result);
    }

    [Fact]
    public void PatchEntries_WithOnlyP1Entries_NoRenaming()
    {
        // Arrange
        var patcher = new EntryPatcher();
        var p1OnlyYaml = @"character: 美鈴
situation: K1_会話親密
entries:
- id: fallback
  content: >-
    P1 content
  priority: 1";

        // Act
        var result = patcher.PatchEntries(p1OnlyYaml);

        // Assert - normalize line endings for comparison (patcher always outputs LF)
        Assert.Equal(p1OnlyYaml.Replace("\r\n", "\n"), result); // No changes for P1-only files
        Assert.Contains("id: fallback", result);
        Assert.DoesNotContain("talent_", result);
    }

    [Fact]
    public void PatchEntries_WithP3OnlyWithCondition_NoChanges()
    {
        // Arrange
        var patcher = new EntryPatcher();
        var p3OnlyYaml = @"character: 美鈴
situation: K1_会話親密
entries:
- id: talent_3_1
  content: >-
    P3 content
  priority: 3
  condition:
    type: Talent
    talentType: 3
    threshold: 1
- id: fallback
  content: >-
    P1 content
  priority: 1";

        // Act
        var result = patcher.PatchEntries(p3OnlyYaml);

        // Assert - normalize line endings for comparison (patcher always outputs LF)
        Assert.Equal(p3OnlyYaml.Replace("\r\n", "\n"), result); // No changes - P3 already has condition, P1 unchanged
    }

    [Fact]
    public void PatchEntries_WithP4AndP2WithoutP3_StillAppliesMigration()
    {
        // Arrange
        var patcher = new EntryPatcher();
        var yaml = @"character: 美鈴
situation: K1_会話親密
entries:
- id: fallback
  content: >-
    P4 content
  priority: 4
- id: fallback
  content: >-
    P2 content
  priority: 2
- id: fallback
  content: >-
    P1 content
  priority: 1";

        // Act
        var result = patcher.PatchEntries(yaml);

        // Assert
        Assert.Contains("id: talent_16_0", result);
        Assert.Contains("id: talent_17_0", result);
        Assert.Contains("priority: 4\n  condition:\n    type: Talent\n    talentType: 16", result);
        Assert.Contains("priority: 2\n  condition:\n    type: Talent\n    talentType: 17", result);
    }

    [Fact]
    public void PatchEntries_WithMixedIndentation_PreservesIndentation()
    {
        // Arrange
        var patcher = new EntryPatcher();
        var yaml = @"character: 美鈴
entries:
- id: fallback
  content: >-
    P4 content
  priority: 4
- id: fallback
  priority: 1";

        // Act
        var result = patcher.PatchEntries(yaml);

        // Assert - condition block should have correct 2-space base indent, 4-space nested indent
        Assert.Contains("  condition:\n    type: Talent", result);
        Assert.Contains("    talentType: 16", result);
        Assert.Contains("    threshold: 1", result);
    }

    [Fact]
    public void EntryHasCondition_WithWhitespaceVariations_DetectsCorrectly()
    {
        // Arrange
        var patcher = new EntryPatcher();
        var entryWithSpaces = @"- id: talent_3_1
  content: >-
    Some content
  priority: 3
  condition:
    type: Talent";

        var entryWithoutCondition = @"- id: fallback
  content: >-
    Some content
  priority: 4";

        // Act & Assert
        Assert.True(patcher.EntryHasCondition(entryWithSpaces));
        Assert.False(patcher.EntryHasCondition(entryWithoutCondition));
    }

    [Fact]
    public void PatchEntries_WithNoEntriesSection_ReturnsUnchanged()
    {
        // Arrange
        var patcher = new EntryPatcher();
        var yamlWithoutEntries = @"character: 美鈴
situation: K1_会話親密
metadata:
  version: 1.0";

        // Act
        var result = patcher.PatchEntries(yamlWithoutEntries);

        // Assert - normalize line endings for comparison (patcher always outputs LF)
        Assert.Equal(yamlWithoutEntries.Replace("\r\n", "\n"), result);
    }

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
