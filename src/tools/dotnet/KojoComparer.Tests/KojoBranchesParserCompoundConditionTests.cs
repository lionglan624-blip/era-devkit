using Era.Core.Dialogue;
using KojoComparer;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Unit tests for KojoBranchesParser compound condition evaluation.
/// Verifies AND, OR, NOT operators and nested compound conditions.
/// </summary>
public class KojoBranchesParserCompoundConditionTests
{
    private readonly KojoBranchesParser _parser;

    public KojoBranchesParserCompoundConditionTests()
    {
        _parser = new KojoBranchesParser();
    }

    /// <summary>
    /// T7 - AC#1: AND condition true when all sub-conditions true (Pos).
    /// </summary>
    [Fact]
    public void CompoundAnd_AllTrue()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - AND branch text
  condition:
    AND:
      - TALENT:
          16:
            ne: 0
      - TALENT:
          3:
            ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 1 },
            { "TALENT:TARGET:3", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("AND branch text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T7 - AC#2: AND condition false when any sub-condition false (Neg).
    /// </summary>
    [Fact]
    public void CompoundAnd_AnyFalse()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - AND branch text
  condition:
    AND:
      - TALENT:
          16:
            ne: 0
      - TALENT:
          3:
            ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 1 },
            { "TALENT:TARGET:3", 0 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSE text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T7 - AC#15: Empty AND array returns true (vacuous truth).
    /// </summary>
    [Fact]
    public void CompoundEmptyAnd()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - AND branch text
  condition:
    AND: []
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>();

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("AND branch text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T8 - AC#3: OR condition true when any sub-condition true (Pos).
    /// </summary>
    [Fact]
    public void CompoundOr_AnyTrue()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - OR branch text
  condition:
    OR:
      - TALENT:
          16:
            ne: 0
      - TALENT:
          3:
            ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 0 },
            { "TALENT:TARGET:3", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("OR branch text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T8 - AC#4: OR condition false when all sub-conditions false (Neg).
    /// </summary>
    [Fact]
    public void CompoundOr_AllFalse()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - OR branch text
  condition:
    OR:
      - TALENT:
          16:
            ne: 0
      - TALENT:
          3:
            ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 0 },
            { "TALENT:TARGET:3", 0 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSE text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T8 - AC#16: Empty OR array returns false.
    /// </summary>
    [Fact]
    public void CompoundEmptyOr()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - OR branch text
  condition:
    OR: []
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>();

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSE text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T9 - AC#5: NOT condition negates false to true.
    /// </summary>
    [Fact]
    public void CompoundNot_NegatesFalseToTrue()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - NOT branch text
  condition:
    NOT:
      TALENT:
        16:
          ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 0 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("NOT branch text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T9 - AC#5: NOT condition negates true to false.
    /// </summary>
    [Fact]
    public void CompoundNot_NegatesTrueToFalse()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - NOT branch text
  condition:
    NOT:
      TALENT:
        16:
          ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSE text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T10 - AC#6: Nested compound evaluates recursively (AND containing OR).
    /// </summary>
    [Fact]
    public void CompoundNested_AndContainingOr()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Nested branch text
  condition:
    AND:
      - TALENT:
          16:
            ne: 0
      - OR:
          - TALENT:
              3:
                ne: 0
          - TALENT:
              17:
                ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 1 },
            { "TALENT:TARGET:3", 0 },
            { "TALENT:TARGET:17", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("Nested branch text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T11 - AC#1,2: CFLAG key in compound sub-condition is accepted and evaluated (Pos).
    /// Verifies CFLAG is in the allowlist and can be evaluated alongside TALENT.
    /// </summary>
    [Fact]
    public void CompoundCflag()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - AND branch text
  condition:
    AND:
      - TALENT:
          16:
            ne: 0
      - CFLAG:
          300:
            eq: 5
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 1 },
            { "CFLAG:300", 5 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert - AND branch should be selected when both conditions are satisfied
        Assert.Single(result.DialogueLines);
        Assert.Equal("AND branch text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T11 - AC#1,2,3: TCVAR key in compound sub-condition with NOT is accepted and evaluated (Pos).
    /// Verifies TCVAR is in the allowlist and can be used with NOT operator.
    /// </summary>
    [Fact]
    public void CompoundTcvar()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - AND branch text
  condition:
    AND:
      - TCVAR:
          302:
            eq: 1
      - NOT:
          TALENT:
            3:
              ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TCVAR:302", 1 },
            { "TALENT:TARGET:3", 0 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert - AND branch should be selected when TCVAR:302==1 AND NOT(TALENT:3!=0)
        Assert.Single(result.DialogueLines);
        Assert.Equal("AND branch text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T11 - AC#3: Unknown key in compound sub-condition throws InvalidOperationException (Neg).
    /// Verifies that non-allowlisted keys (LOCAL, EQUIP, etc.) are still rejected.
    /// </summary>
    [Fact]
    public void CompoundNonTalent_UnknownKeyRejected()
    {
        // Arrange
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - AND branch text
  condition:
    AND:
      - TALENT:
          16:
            ne: 0
      - UNKNOWN:
          500:
            ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 1 }
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _parser.Parse(yamlContent, state));
        Assert.Contains("Compound conditions with unsupported keys", exception.Message);
        Assert.Contains("UNKNOWN", exception.Message);
    }

    /// <summary>
    /// T12 - AC#8: Single-condition YAML backward compatibility.
    /// </summary>
    [Fact]
    public void CompoundBackwardCompat_SingleConditionStillWorks()
    {
        // Arrange - existing single-condition format
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Single condition branch
  condition:
    TALENT:
      16:
        ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("Single condition branch", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// T13 - AC#14: Nesting depth exceeding 5 levels throws InvalidOperationException (Neg).
    /// </summary>
    [Fact]
    public void CompoundDepthLimit_ExceedsMaxThrows()
    {
        // Arrange - 7-level nested condition (triggers depth=6 in EvaluateCompoundCondition)
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Deep nested branch
  condition:
    AND:
      - AND:
          - AND:
              - AND:
                  - AND:
                      - AND:
                          - AND:
                              - TALENT:
                                  16:
                                    ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 1 }
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _parser.Parse(yamlContent, state));
        Assert.Contains("Compound condition nesting exceeds maximum depth", exception.Message);
        Assert.Contains("depth: 6", exception.Message);
    }
}
