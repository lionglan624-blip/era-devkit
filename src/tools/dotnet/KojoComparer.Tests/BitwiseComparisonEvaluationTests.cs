using Era.Core.Dialogue;
using KojoComparer;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// F759 AC#9: Evaluation unit tests for bitwise_and_cmp operator
/// Tests two-stage evaluation: (stateValue &amp; mask) op expectedValue
/// CRITICAL: Tests both TALENT inline path and EvaluateVariableCondition path
/// </summary>
public class BitwiseComparisonEvaluationTests
{
    private readonly KojoBranchesParser _parser;

    public BitwiseComparisonEvaluationTests()
    {
        _parser = new KojoBranchesParser();
    }

    /// <summary>
    /// TALENT path: state TALENT:16=7, mask=3, compare eq 3 → (7 &amp; 3) = 3 == 3 → true
    /// </summary>
    [Fact]
    public void BitwiseComparisonEvaluation_TalentPath_EqTrue()
    {
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - マッチ
  condition:
    TALENT:
      16:
        bitwise_and_cmp:
          mask: ""3""
          op: eq
          value: ""3""
- lines:
  - ミスマッチ
  condition: {}
";
        var state = new Dictionary<string, int> { { "TALENT:TARGET:16", 7 } };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("マッチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// TALENT path: state TALENT:16=5, mask=3, compare eq 1 → (5 &amp; 3) = 1 == 1 → true
    /// </summary>
    [Fact]
    public void BitwiseComparisonEvaluation_TalentPath_EqTrue2()
    {
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - マッチ
  condition:
    TALENT:
      16:
        bitwise_and_cmp:
          mask: ""3""
          op: eq
          value: ""1""
- lines:
  - ミスマッチ
  condition: {}
";
        var state = new Dictionary<string, int> { { "TALENT:TARGET:16", 5 } };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("マッチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// TALENT path: state TALENT:16=6, mask=3, compare eq 3 → (6 &amp; 3) = 2 != 3 → false
    /// </summary>
    [Fact]
    public void BitwiseComparisonEvaluation_TalentPath_EqFalse()
    {
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - マッチ
  condition:
    TALENT:
      16:
        bitwise_and_cmp:
          mask: ""3""
          op: eq
          value: ""3""
- lines:
  - ミスマッチ
  condition: {}
";
        var state = new Dictionary<string, int> { { "TALENT:TARGET:16", 6 } };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("ミスマッチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// TALENT path: state TALENT:16=0, mask=3, compare eq 3 → (0 &amp; 3) = 0 != 3 → false
    /// </summary>
    [Fact]
    public void BitwiseComparisonEvaluation_TalentPath_ZeroValue()
    {
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - マッチ
  condition:
    TALENT:
      16:
        bitwise_and_cmp:
          mask: ""3""
          op: eq
          value: ""3""
- lines:
  - ミスマッチ
  condition: {}
";
        var state = new Dictionary<string, int> { { "TALENT:TARGET:16", 0 } };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("ミスマッチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// TALENT path: ne operator - state TALENT:16=7, mask=3, compare ne 1 → (7 &amp; 3) = 3 != 1 → true
    /// </summary>
    [Fact]
    public void BitwiseComparisonEvaluation_TalentPath_NeTrue()
    {
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - マッチ
  condition:
    TALENT:
      16:
        bitwise_and_cmp:
          mask: ""3""
          op: ne
          value: ""1""
- lines:
  - ミスマッチ
  condition: {}
";
        var state = new Dictionary<string, int> { { "TALENT:TARGET:16", 7 } };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("マッチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// EvaluateVariableCondition path: CFLAG state=7, mask=3, compare eq 3 → true
    /// </summary>
    [Fact]
    public void BitwiseComparisonEvaluation_CflagPath_EqTrue()
    {
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - マッチ
  condition:
    CFLAG:
      0:
        bitwise_and_cmp:
          mask: ""3""
          op: eq
          value: ""3""
- lines:
  - ミスマッチ
  condition: {}
";
        var state = new Dictionary<string, int> { { "CFLAG:0", 7 } };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("マッチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// EvaluateVariableCondition path: CFLAG state=6, mask=3, compare eq 3 → (6 &amp; 3) = 2 != 3 → false
    /// </summary>
    [Fact]
    public void BitwiseComparisonEvaluation_CflagPath_EqFalse()
    {
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - マッチ
  condition:
    CFLAG:
      0:
        bitwise_and_cmp:
          mask: ""3""
          op: eq
          value: ""3""
- lines:
  - ミスマッチ
  condition: {}
";
        var state = new Dictionary<string, int> { { "CFLAG:0", 6 } };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("ミスマッチ", result.DialogueLines[0].Text);
    }
}
