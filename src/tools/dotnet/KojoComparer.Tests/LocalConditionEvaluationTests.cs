using KojoComparer;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for LOCAL condition evaluation in KojoBranchesParser (AC#10)
/// All test names contain "Local" to match AC filter DisplayName~Local
/// </summary>
public class LocalConditionEvaluationTests
{
    private readonly KojoBranchesParser _parser;

    public LocalConditionEvaluationTests()
    {
        _parser = new KojoBranchesParser();
    }

    [Fact]
    public void Evaluate_LocalCondition_WithMatchingState_ReturnsTrue()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - LOCAL match text
  condition:
    LOCAL:
      0:
        eq: 1
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>
        {
            { "LOCAL:0", 1 }
        };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("LOCAL match text", result.DialogueLines[0].Text);
    }

    [Fact]
    public void Evaluate_LocalCondition_WithNonMatchingState_ReturnsFalse()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - LOCAL match text
  condition:
    LOCAL:
      0:
        eq: 1
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>
        {
            { "LOCAL:0", 0 }
        };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSE text", result.DialogueLines[0].Text);
    }

    [Fact]
    public void Evaluate_IndexedLocalCondition_WithMatchingState_ReturnsTrue()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Indexed LOCAL match
  condition:
    LOCAL:
      1:
        eq: 0
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>
        {
            { "LOCAL:1", 0 }
        };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("Indexed LOCAL match", result.DialogueLines[0].Text);
    }

    [Fact]
    public void Evaluate_LocalCondition_WithMissingState_ReturnsElse()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - LOCAL match text
  condition:
    LOCAL:
      0:
        eq: 1
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>();
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSE text", result.DialogueLines[0].Text);
    }
}
