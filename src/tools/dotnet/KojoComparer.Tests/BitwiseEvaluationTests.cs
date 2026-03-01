using Era.Core.Dialogue;
using KojoComparer;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// F757 AC#20,22: Bitwise operator runtime evaluation tests
/// Verifies bitwise_and operator is correctly evaluated against game state
/// </summary>
public class BitwiseEvaluationTests
{
    private readonly KojoBranchesParser _parser;

    public BitwiseEvaluationTests()
    {
        _parser = new KojoBranchesParser();
    }

    /// <summary>
    /// AC#20: Test EvaluateVariableCondition handles bitwise_and operator
    /// State has STAIN:口 = 5 (binary 101), condition has bitwise_and: "4" (binary 100)
    /// Should return true because (5 & 4 = 4 ≠ 0)
    /// </summary>
    [Fact]
    public void BitwiseEvaluation_StainBitwiseAnd_TrueCase()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 汚れあり
  condition:
    STAIN:
      口:
        bitwise_and: ""4""
- lines:
  - 汚れなし
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "STAIN:口", 5 } // binary 101
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("汚れあり", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// AC#20: Test bitwise_and operator - False case
    /// State has STAIN:口 = 2 (binary 010), condition has bitwise_and: "4" (binary 100)
    /// Should return false because (2 & 4 = 0)
    /// </summary>
    [Fact]
    public void BitwiseEvaluation_StainBitwiseAnd_FalseCase()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 汚れあり
  condition:
    STAIN:
      口:
        bitwise_and: ""4""
- lines:
  - 汚れなし
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "STAIN:口", 2 } // binary 010, does not have bit 4 set
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("汚れなし", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// AC#22: Test TALENT-specific evaluator handles bitwise_and
    /// State has TALENT:50 = 3 (binary 11), YAML condition has bitwise_and: "1" (binary 01)
    /// Should return true because (3 & 1 = 1 ≠ 0)
    /// </summary>
    [Fact]
    public void BitwiseEvaluation_TalentBitwiseAnd_TrueCase()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 性別嗜好マッチ
  condition:
    TALENT:
      50:
        bitwise_and: ""1""
- lines:
  - 性別嗜好ミスマッチ
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:50", 3 } // binary 11
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("性別嗜好マッチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// AC#22: Test TALENT bitwise_and - False case
    /// State has TALENT:50 = 2 (binary 10), condition has bitwise_and: "1" (binary 01)
    /// Should return false because (2 & 1 = 0)
    /// </summary>
    [Fact]
    public void BitwiseEvaluation_TalentBitwiseAnd_FalseCase()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 性別嗜好マッチ
  condition:
    TALENT:
      50:
        bitwise_and: ""1""
- lines:
  - 性別嗜好ミスマッチ
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:50", 2 } // binary 10, bit 1 not set
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("性別嗜好ミスマッチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test CFLAG bitwise_and evaluation
    /// State has CFLAG:奴隷:前回売春フラグ = 3 (binary 11), condition has bitwise_and: "1"
    /// Should return true because (3 & 1 = 1 ≠ 0)
    /// </summary>
    [Fact]
    public void BitwiseEvaluation_CflagBitwiseAnd()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 初売春フラグあり
  condition:
    CFLAG:
      奴隷:前回売春フラグ:
        bitwise_and: ""1""
- lines:
  - 初売春フラグなし
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "CFLAG:奴隷:前回売春フラグ", 3 } // binary 11
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("初売春フラグあり", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test multiple bitwise conditions with compound AND
    /// Both STAIN and TALENT bitwise conditions must be true
    /// </summary>
    [Fact]
    public void BitwiseEvaluation_CompoundCondition()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 両方マッチ
  condition:
    AND:
    - STAIN:
        口:
          bitwise_and: ""4""
    - TALENT:
        50:
          bitwise_and: ""1""
- lines:
  - ミスマッチ
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "STAIN:口", 5 },   // binary 101, has bit 4
            { "TALENT:TARGET:50", 3 }   // binary 11, has bit 1
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("両方マッチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test bitwise evaluation with zero state value
    /// State has STAIN:口 = 0, any bitwise_and should return false
    /// </summary>
    [Fact]
    public void BitwiseEvaluation_ZeroStateValue()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 汚れあり
  condition:
    STAIN:
      口:
        bitwise_and: ""4""
- lines:
  - 汚れなし
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "STAIN:口", 0 } // All bits zero
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("汚れなし", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test bitwise evaluation with missing state key
    /// State does not have STAIN:口, should default to 0 and return false
    /// </summary>
    [Fact]
    public void BitwiseEvaluation_MissingStateKey()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 汚れあり
  condition:
    STAIN:
      口:
        bitwise_and: ""4""
- lines:
  - 汚れなし
  condition: {}
";

        var state = new Dictionary<string, int>(); // Empty state

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("汚れなし", result.DialogueLines[0].Text);
    }
}
