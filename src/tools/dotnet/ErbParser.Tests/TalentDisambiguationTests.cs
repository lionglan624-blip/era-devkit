using System.Text.Json;
using ErbParser;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#1-5, 11-12: TALENT disambiguation logic tests (F760)
/// Tests for Target/Name/Index field disambiguation based on keyword detection
/// </summary>
public class TalentDisambiguationTests
{
    private static readonly JsonSerializerOptions s_indentedOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// AC#1: TALENT:2 parses as Index=2
    /// Numeric-only second part should be treated as Index, not Name
    /// </summary>
    [Fact]
    public void Parse_NumericOnly_AssignsToIndex()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:2 & 2";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Index); // Should assign to Index
        Assert.True(string.IsNullOrEmpty(result.Name)); // Name should be empty or null
        Assert.Equal("&", result.Operator);
        Assert.Equal("2", result.Value);
    }

    /// <summary>
    /// AC#2: TALENT:PLAYER parses as Target=PLAYER
    /// Reserved keyword should be treated as Target, not Name
    /// </summary>
    [Fact]
    public void Parse_ReservedKeyword_AssignsToTarget()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:PLAYER & 2";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PLAYER", result.Target); // Should assign to Target
        Assert.True(string.IsNullOrEmpty(result.Name)); // Name should be empty or null
        Assert.Null(result.Index); // Index should be null
        Assert.Equal("&", result.Operator);
        Assert.Equal("2", result.Value);
    }

    /// <summary>
    /// AC#3: TALENT:恋人 parses as Name=恋人 (backward compatibility)
    /// Non-keyword, non-numeric should preserve existing behavior (assign to Name)
    /// </summary>
    [Fact]
    public void Parse_NonKeyword_AssignsToName()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:恋人";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("恋人", result.Name); // Should assign to Name
        Assert.True(string.IsNullOrEmpty(result.Target)); // Target should be empty
        Assert.Null(result.Index); // Index should be null
    }

    /// <summary>
    /// AC#4: Three-part pattern TALENT:PLAYER:処女 (C3)
    /// Target:Name pattern
    /// </summary>
    [Fact]
    public void Parse_ThreePart_TargetAndName()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:PLAYER:処女";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PLAYER", result.Target);
        Assert.Equal("処女", result.Name);
        Assert.Null(result.Index); // Index should be null for non-numeric Name
    }

    /// <summary>
    /// AC#4: Three-part pattern TALENT:PLAYER:2 (C8)
    /// Target:Index pattern
    /// </summary>
    [Fact]
    public void Parse_ThreePart_TargetAndIndex()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:PLAYER:2";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PLAYER", result.Target);
        Assert.Equal(2, result.Index); // Numeric third part should be Index
        Assert.True(string.IsNullOrEmpty(result.Name)); // Name should be empty
    }

    /// <summary>
    /// AC#4: Three-part pattern TALENT:6:NTR
    /// Numeric Target with Name
    /// </summary>
    [Fact]
    public void Parse_ThreePart_NumericTargetAndName()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:6:NTR";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("6", result.Target); // First part is Target
        Assert.Equal("NTR", result.Name); // Third part is non-numeric Name
        Assert.Null(result.Index); // Index should be null
    }

    /// <summary>
    /// AC#4: Three-part pattern TALENT:6:PLAYER
    /// Reserved keyword in Name position should still be treated as Name (not Target)
    /// because it's the third part (context determines meaning)
    /// </summary>
    [Fact]
    public void Parse_ThreePart_KeywordInNamePosition()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:6:PLAYER";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("6", result.Target); // First part is Target
        Assert.Equal("PLAYER", result.Name); // Third part is Name, even though it's a keyword
        Assert.Null(result.Index); // Index should be null
    }

    /// <summary>
    /// AC#5: Non-keyword identifier should parse as Name
    /// Custom identifiers like "人物_主人公" should not be matched as Target keyword
    /// </summary>
    [Fact]
    public void Parse_CustomIdentifier_AssignsToName()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:人物_主人公";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("人物_主人公", result.Name); // Should assign to Name
        Assert.True(string.IsNullOrEmpty(result.Target)); // Target should be empty
        Assert.Null(result.Index); // Index should be null
    }

    /// <summary>
    /// AC#12: JSON backward compatibility - deserialize without "index" field
    /// Legacy JSON without "index" property should deserialize successfully with Index=null
    /// </summary>
    [Fact]
    public void JsonDeserialize_WithoutIndexField_ReturnsNullIndex()
    {
        // Arrange
        var json = """
        {
            "target": "PLAYER",
            "name": "恋人",
            "operator": "==",
            "value": "1"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<TalentRef>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PLAYER", result.Target);
        Assert.Equal("恋人", result.Name);
        Assert.Null(result.Index); // Should be null when not present in JSON
        Assert.Equal("==", result.Operator);
        Assert.Equal("1", result.Value);
    }

    /// <summary>
    /// AC#12: JSON backward compatibility - serialize with Index field
    /// TalentRef with Index should serialize to JSON containing "index" property
    /// </summary>
    [Fact]
    public void JsonSerialize_WithIndex_IncludesIndexField()
    {
        // Arrange
        var talentRef = new TalentRef
        {
            Target = "PLAYER",
            Index = 2,
            Name = "",
            Operator = "&",
            Value = "2"
        };

        // Act
        var json = JsonSerializer.Serialize(talentRef, s_indentedOptions);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"index\"", json); // Should contain index property
        Assert.Contains("2", json); // Should contain the index value
        Assert.Contains("\"target\"", json);
        Assert.Contains("PLAYER", json);
    }

    /// <summary>
    /// AC#1: Additional test for numeric-only Index with different value
    /// </summary>
    [Fact]
    public void Parse_NumericOnly_DifferentIndex()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:5 == 1";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Index);
        Assert.True(string.IsNullOrEmpty(result.Name));
        Assert.Equal("==", result.Operator);
        Assert.Equal("1", result.Value);
    }

    /// <summary>
    /// AC#2: Additional test for other reserved keywords (MASTER, TARGET)
    /// </summary>
    [Fact]
    public void Parse_ReservedKeyword_MASTER()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:MASTER & 1";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.True(string.IsNullOrEmpty(result.Name));
        Assert.Null(result.Index);
    }

    /// <summary>
    /// AC#2: Additional test for TARGET keyword
    /// </summary>
    [Fact]
    public void Parse_ReservedKeyword_TARGET()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:TARGET & 1";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TARGET", result.Target);
        Assert.True(string.IsNullOrEmpty(result.Name));
        Assert.Null(result.Index);
    }
}
