using KojoComparer;
using Xunit;

namespace KojoComparer.Tests;

public class ArgConditionEvaluationTests
{
    private readonly KojoBranchesParser _parser;

    public ArgConditionEvaluationTests()
    {
        _parser = new KojoBranchesParser();
    }

    [Fact]
    public void Evaluate_ArgCondition_WithMatchingState_ReturnsTrue()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - ARG match text
  condition:
    ARG:
      0:
        eq: 2
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>
        {
            { "ARG:0", 2 }
        };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("ARG match text", result.DialogueLines[0].Text);
    }

    [Fact]
    public void Evaluate_ArgCondition_WithNonMatchingState_ReturnsFalse()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - ARG match text
  condition:
    ARG:
      0:
        eq: 2
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>
        {
            { "ARG:0", 1 }
        };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSE text", result.DialogueLines[0].Text);
    }

    [Fact]
    public void Evaluate_IndexedArgCondition_WithMatchingState_ReturnsTrue()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Indexed ARG match
  condition:
    ARG:
      1:
        eq: 3
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>
        {
            { "ARG:1", 3 }
        };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("Indexed ARG match", result.DialogueLines[0].Text);
    }
}
