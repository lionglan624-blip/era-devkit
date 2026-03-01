using System.Text.Json;
using ErbParser;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#0: Polymorphic condition type tests
/// Verifies ICondition interface enables polymorphic condition handling
/// </summary>
public class IConditionInterfaceTests
{
    private static readonly JsonSerializerOptions s_indentedOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// AC#0.1: TalentRef instances can be assigned to ICondition variable
    /// </summary>
    [Fact]
    public void TalentRef_ImplementsICondition()
    {
        // Arrange
        var talentRef = new TalentRef
        {
            Target = "MASTER",
            Name = "恋人"
        };

        // Act
        ICondition condition = talentRef;

        // Assert
        Assert.NotNull(condition);
        Assert.IsAssignableFrom<ICondition>(talentRef);
    }

    /// <summary>
    /// AC#0.2: ConditionBranch.Condition property accepts TalentRef via ICondition
    /// </summary>
    [Fact]
    public void ConditionBranch_AcceptsTalentRefAsICondition()
    {
        // Arrange
        var talentRef = new TalentRef
        {
            Target = "MASTER",
            Name = "恋人"
        };

        // Act
        var branch = new ConditionBranch
        {
            Type = "if",
            Condition = talentRef,
            HasBody = true
        };

        // Assert
        Assert.NotNull(branch.Condition);
        Assert.IsAssignableFrom<ICondition>(branch.Condition);
        Assert.IsType<TalentRef>(branch.Condition);
    }

    /// <summary>
    /// AC#0.3: Existing TalentRef serialization unchanged (backward compatibility)
    /// </summary>
    [Fact]
    public void TalentRef_SerializationBackwardCompatible()
    {
        // Arrange
        var branch = new ConditionBranch
        {
            Type = "if",
            Condition = new TalentRef
            {
                Target = "MASTER",
                Name = "恋人"
            },
            HasBody = true
        };

        // Act
        var json = JsonSerializer.Serialize(branch, s_indentedOptions);

        // Assert - Should contain TalentRef properties
        Assert.Contains("\"type\"", json);
        Assert.Contains("\"if\"", json);
        Assert.Contains("\"condition\"", json);
        Assert.Contains("\"target\"", json);
        Assert.Contains("\"name\"", json);
        Assert.Contains("MASTER", json);
        Assert.Contains("恋人", json);
        Assert.Contains("\"hasBody\"", json);
        Assert.Contains("true", json);

        // Note: JSON output may include type discriminator with [JsonDerivedType]
        // As long as TalentRef properties are preserved, backward compatibility is maintained
    }

    /// <summary>
    /// AC#0.4: Multiple ICondition implementations can coexist in ConditionBranch
    /// </summary>
    [Fact]
    public void ConditionBranch_AcceptsMultipleIConditionTypes()
    {
        // Arrange - TalentRef
        var talentBranch = new ConditionBranch
        {
            Type = "if",
            Condition = new TalentRef { Target = "MASTER", Name = "恋人" },
            HasBody = true
        };

        // Arrange - CflagRef
        var cflagBranch = new ConditionBranch
        {
            Type = "elseif",
            Condition = new CflagRef { Target = "MASTER", Name = "現在位置" },
            HasBody = true
        };

        // Arrange - FunctionCall
        var funcBranch = new ConditionBranch
        {
            Type = "elseif",
            Condition = new FunctionCall { Name = "HAS_VAGINA", Args = new[] { "TARGET" } },
            HasBody = true
        };

        // Assert - All types are valid ICondition
        Assert.IsAssignableFrom<ICondition>(talentBranch.Condition);
        Assert.IsAssignableFrom<ICondition>(cflagBranch.Condition);
        Assert.IsAssignableFrom<ICondition>(funcBranch.Condition);

        // Assert - Specific types are preserved
        Assert.IsType<TalentRef>(talentBranch.Condition);
        Assert.IsType<CflagRef>(cflagBranch.Condition);
        Assert.IsType<FunctionCall>(funcBranch.Condition);
    }
}
