using Era.Core.Dialogue;
using KojoComparer;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Feature 760: Target-aware TALENT state key support.
/// Tests for compound keys like "PLAYER:16" and symbolic keys like "PLAYER".
/// These tests verify KojoBranchesParser, YamlRunner, and StateConverter can handle target-aware TALENT conditions.
/// </summary>
[Trait("Category", "F760New")]
public class TalentTargetAwareTests
{
    private readonly KojoBranchesParser _parser;

    public TalentTargetAwareTests()
    {
        _parser = new KojoBranchesParser();
    }

    #region AC#10: KojoBranchesParser target-aware state key

    /// <summary>
    /// AC#10 Test 1: Compound key "PLAYER:16" in YAML condition should match state key TALENT:PLAYER:16.
    /// Current implementation has "vacuous truth" bug: int.TryParse("PLAYER:16") fails, skips key,
    /// empty foreach completes, returns TRUE, selects first branch (wrong reason).
    /// This test currently PASSES but for the wrong reason (should evaluate state value, not vacuously succeed).
    /// After fix: Should properly evaluate TALENT:PLAYER:16 state value and match when ne 0.
    /// </summary>
    [Fact]
    public void Parse_CompoundKey_MatchesTargetAwareStateKey()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Target match
  condition:
    TALENT:
      ""PLAYER:16"":
        ne: 0
- lines:
  - ELSE
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:PLAYER:16", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        // EXPECTED: Should select "Target match" branch (by evaluating state value)
        // ACTUAL (VACUOUS PASS): Selects "Target match" because empty foreach returns true
        // This is a RED test that passes for the WRONG reason - implementation is still broken
        Assert.Single(result.DialogueLines);
        Assert.Equal("Target match", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// AC#10 Test 2: Simple numeric key "16" in YAML condition should match state key TALENT:TARGET:16.
    /// Current implementation FAILS: Parser produces "TALENT:16" not "TALENT:TARGET:16".
    /// </summary>
    [Fact]
    public void Parse_NumericKey_MatchesTargetDefaultStateKey()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Numeric match
  condition:
    TALENT:
      16:
        ne: 0
- lines:
  - ELSE
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        // EXPECTED: Should select "Numeric match" branch (TARGET is default target)
        // ACTUAL (RED): Selects "ELSE" branch because state key is "TALENT:TARGET:16" but parser looks for "TALENT:16"
        Assert.Single(result.DialogueLines);
        Assert.Equal("Numeric match", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// AC#10 Test 3: Symbolic key "PLAYER" in YAML condition should match state key TALENT:PLAYER.
    /// Current implementation has "vacuous truth" bug: int.TryParse("PLAYER") fails, skips key,
    /// empty foreach completes, returns TRUE, selects first branch (wrong reason).
    /// This test currently PASSES but for the wrong reason (should evaluate state value, not vacuously succeed).
    /// After fix: Should properly evaluate TALENT:PLAYER state value and match when ne 0.
    /// </summary>
    [Fact]
    public void Parse_SymbolicKey_MatchesSymbolicStateKey()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Symbolic match
  condition:
    TALENT:
      PLAYER:
        ne: 0
- lines:
  - ELSE
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:PLAYER", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        // EXPECTED: Should select "Symbolic match" branch (by evaluating state value)
        // ACTUAL (VACUOUS PASS): Selects "Symbolic match" because empty foreach returns true
        // This is a RED test that passes for the WRONG reason - implementation is still broken
        Assert.Single(result.DialogueLines);
        Assert.Equal("Symbolic match", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// AC#10 Test 4 (Vacuous Truth Bug Demonstration): Compound key condition should FAIL when state value is 0.
    /// This test demonstrates the "vacuous truth" bug: compound keys are skipped, empty foreach returns TRUE,
    /// causing condition to pass even when state value is 0 (should fail ne: 0 check).
    /// This test currently FAILS (correctly identifies the bug).
    /// After fix: Should properly evaluate TALENT:PLAYER:16 value (0) and reject ne: 0 condition.
    /// </summary>
    [Fact]
    public void Parse_CompoundKey_FailsWhenStateValueIsZero()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Should not match (value is 0)
  condition:
    TALENT:
      ""PLAYER:16"":
        ne: 0
- lines:
  - ELSE (correct)
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:PLAYER:16", 0 }  // Value is 0, should NOT match ne: 0
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        // EXPECTED: Should select "ELSE" branch (value 0 does not satisfy ne: 0)
        // ACTUAL (BUG): Selects "Should not match" because vacuous truth bug makes condition pass
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSE (correct)", result.DialogueLines[0].Text);
    }

    #endregion

    #region AC#15: End-to-end round-trip test

    /// <summary>
    /// AC#15: End-to-end test verifying parse→convert→evaluate target preservation.
    /// Tests compound key "PLAYER:16" round-trip through KojoBranchesParser.
    /// Current implementation has "vacuous truth" bug: All compound keys are skipped,
    /// empty foreach returns TRUE, selects first branch (wrong reason).
    /// This test currently PASSES but for the wrong reason (should evaluate state values, not vacuously succeed).
    /// After fix: Should properly evaluate TALENT:PLAYER:16 (set) vs TALENT:OTHER:16 (not set).
    /// </summary>
    [Fact]
    public void Parse_EndToEnd_PreservesCompoundKeyTarget()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Player talent match
  condition:
    TALENT:
      ""PLAYER:16"":
        ne: 0
- lines:
  - Other talent match
  condition:
    TALENT:
      ""OTHER:16"":
        ne: 0
- lines:
  - ELSE
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:PLAYER:16", 1 },
            { "TALENT:OTHER:16", 0 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        // EXPECTED: Should select "Player talent match" branch (by evaluating state values)
        // ACTUAL (VACUOUS PASS): Selects "Player talent match" because empty foreach returns true
        // This is a RED test that passes for the WRONG reason - implementation is still broken
        Assert.Single(result.DialogueLines);
        Assert.Equal("Player talent match", result.DialogueLines[0].Text);
    }

    #endregion

    #region AC#22: StateConverter preserves compound keys

    /// <summary>
    /// AC#22 Test 1: StateConverter should preserve compound keys.
    /// TALENT:PLAYER:16 → context { "TALENT": { "PLAYER:16": 1 } }
    /// Current implementation FAILS: StateConverter strips intermediate segments, producing { "TALENT": { "16": 1 } }
    /// </summary>
    [Fact]
    public void StateConverter_PreservesCompoundKey()
    {
        // Arrange
        var state = new Dictionary<string, int>
        {
            { "TALENT:PLAYER:16", 1 }
        };

        // Act
        var context = StateConverter.ConvertStateToContext(state);

        // Assert
        // EXPECTED: context["TALENT"] should contain key "PLAYER:16"
        // ACTUAL (RED): context["TALENT"] contains key "16" (target lost)
        Assert.True(context.ContainsKey("TALENT"));
        var talentDict = context["TALENT"] as Dictionary<string, int>;
        Assert.NotNull(talentDict);
        Assert.True(talentDict.ContainsKey("PLAYER:16"));
        Assert.Equal(1, talentDict["PLAYER:16"]);
    }

    /// <summary>
    /// AC#22 Test 2: StateConverter should strip default TARGET target for backward compatibility.
    /// TALENT:TARGET:16 → context { "TALENT": { "16": 1 } }
    /// Current implementation PASSES: This is the current behavior.
    /// </summary>
    [Fact]
    public void StateConverter_StripsDefaultTarget()
    {
        // Arrange
        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 1 }
        };

        // Act
        var context = StateConverter.ConvertStateToContext(state);

        // Assert
        // EXPECTED: context["TALENT"] should contain key "16" (default target stripped)
        // ACTUAL: Should PASS with current implementation
        Assert.True(context.ContainsKey("TALENT"));
        var talentDict = context["TALENT"] as Dictionary<string, int>;
        Assert.NotNull(talentDict);
        Assert.True(talentDict.ContainsKey("16"));
        Assert.Equal(1, talentDict["16"]);
    }

    /// <summary>
    /// AC#22 Test 3: StateConverter should not change non-TALENT keys.
    /// ABL:TARGET:5 → context { "ABL": { "5": 1 } }
    /// Current implementation PASSES: ABL is not affected by F760 changes.
    /// </summary>
    [Fact]
    public void StateConverter_NonTalentUnchanged()
    {
        // Arrange
        var state = new Dictionary<string, int>
        {
            { "ABL:TARGET:5", 1 }
        };

        // Act
        var context = StateConverter.ConvertStateToContext(state);

        // Assert
        // EXPECTED: context["ABL"] should contain key "5"
        // ACTUAL: Should PASS with current implementation
        Assert.True(context.ContainsKey("ABL"));
        var ablDict = context["ABL"] as Dictionary<string, int>;
        Assert.NotNull(ablDict);
        Assert.True(ablDict.ContainsKey("5"));
        Assert.Equal(1, ablDict["5"]);
    }

    /// <summary>
    /// AC#22 Test 4: StateConverter should handle symbolic TALENT keys.
    /// TALENT:PLAYER → context { "TALENT": { "PLAYER": 1 } }
    /// Current implementation: Takes last segment "PLAYER", which happens to be correct for 2-segment keys.
    /// This test currently PASSES but needs verification that it's intentional, not accidental.
    /// </summary>
    [Fact]
    public void StateConverter_PreservesSymbolicKey()
    {
        // Arrange
        var state = new Dictionary<string, int>
        {
            { "TALENT:PLAYER", 1 }
        };

        // Act
        var context = StateConverter.ConvertStateToContext(state);

        // Assert
        // EXPECTED: context["TALENT"] should contain key "PLAYER"
        // ACTUAL: Currently PASSES (last segment = "PLAYER" for 2-segment keys)
        Assert.True(context.ContainsKey("TALENT"));
        var talentDict = context["TALENT"] as Dictionary<string, int>;
        Assert.NotNull(talentDict);
        Assert.True(talentDict.ContainsKey("PLAYER"));
        Assert.Equal(1, talentDict["PLAYER"]);
    }

    #endregion
}
