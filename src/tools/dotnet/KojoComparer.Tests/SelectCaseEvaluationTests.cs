using KojoComparer;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for KojoBranchesParser ARG OR condition evaluation (Feature 765 - AC#8)
/// </summary>
public class SelectCaseEvaluationTests
{
    private readonly KojoBranchesParser _parser;

    public SelectCaseEvaluationTests()
    {
        _parser = new KojoBranchesParser();
    }

    /// <summary>
    /// AC#8: KojoBranchesParser evaluates ARG OR conditions (branches-format capability test)
    /// Expected: OR(ARG==13, ARG==25) evaluates true when ARG:0 is 25
    /// </summary>
    [Fact]
    public void Evaluate_ArgOrCondition_WithMatchingState()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - OR match text
  condition:
    OR:
    - ARG:
        0:
          eq: 13
    - ARG:
        0:
          eq: 25
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>
        {
            { "ARG:0", 25 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("OR match text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test ARG OR condition with non-matching state
    /// Expected: Falls through to ELSE branch
    /// </summary>
    [Fact]
    public void Evaluate_ArgOrCondition_WithNonMatchingState()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - OR match text
  condition:
    OR:
    - ARG:
        0:
          eq: 13
    - ARG:
        0:
          eq: 25
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>
        {
            { "ARG:0", 99 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSE text", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test ARG OR condition matches first value
    /// Expected: Branch selected when ARG:0 is 13
    /// </summary>
    [Fact]
    public void Evaluate_ArgOrCondition_MatchesFirstValue()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - OR match text
  condition:
    OR:
    - ARG:
        0:
          eq: 13
    - ARG:
        0:
          eq: 25
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>
        {
            { "ARG:0", 13 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("OR match text", result.DialogueLines[0].Text);
    }
}
