using Era.Core.Dialogue;
using KojoComparer;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Unit tests for KojoBranchesParser condition evaluation.
/// Verifies branch selection with TALENT conditions.
/// </summary>
public class KojoBranchesParserConditionTests
{
    private readonly KojoBranchesParser _parser;

    public KojoBranchesParserConditionTests()
    {
        _parser = new KojoBranchesParser();
    }

    /// <summary>
    /// Test case 1: State with TALENT:16 set should select Branch 0 (恋人).
    /// </summary>
    [Fact]
    public void Parse_WithTalent16Set_SelectsBranch0()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 恋人ブランチ
  condition:
    TALENT:
      16:
        ne: 0
- lines:
  - 恋慕ブランチ
  condition:
    TALENT:
      3:
        ne: 0
- lines:
  - 思慕ブランチ
  condition:
    TALENT:
      17:
        ne: 0
- lines:
  - ELSEブランチ
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
        Assert.Equal("恋人ブランチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test case 2: State with TALENT:3 set should select Branch 1 (恋慕).
    /// </summary>
    [Fact]
    public void Parse_WithTalent3Set_SelectsBranch1()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 恋人ブランチ
  condition:
    TALENT:
      16:
        ne: 0
- lines:
  - 恋慕ブランチ
  condition:
    TALENT:
      3:
        ne: 0
- lines:
  - 思慕ブランチ
  condition:
    TALENT:
      17:
        ne: 0
- lines:
  - ELSEブランチ
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:3", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("恋慕ブランチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test case 3: State with TALENT:17 set should select Branch 2 (思慕).
    /// </summary>
    [Fact]
    public void Parse_WithTalent17Set_SelectsBranch2()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 恋人ブランチ
  condition:
    TALENT:
      16:
        ne: 0
- lines:
  - 恋慕ブランチ
  condition:
    TALENT:
      3:
        ne: 0
- lines:
  - 思慕ブランチ
  condition:
    TALENT:
      17:
        ne: 0
- lines:
  - ELSEブランチ
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:17", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("思慕ブランチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test case 4: Empty state should select ELSE branch (last branch with empty condition).
    /// </summary>
    [Fact]
    public void Parse_WithEmptyState_SelectsElseBranch()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 恋人ブランチ
  condition:
    TALENT:
      16:
        ne: 0
- lines:
  - 恋慕ブランチ
  condition:
    TALENT:
      3:
        ne: 0
- lines:
  - 思慕ブランチ
  condition:
    TALENT:
      17:
        ne: 0
- lines:
  - ELSEブランチ
  condition: {}
";

        var state = new Dictionary<string, int>();

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSEブランチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test case 5: State with unmatched TALENT (e.g., TALENT:99) should select ELSE branch.
    /// </summary>
    [Fact]
    public void Parse_WithNoMatchingCondition_SelectsElse()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 恋人ブランチ
  condition:
    TALENT:
      16:
        ne: 0
- lines:
  - 恋慕ブランチ
  condition:
    TALENT:
      3:
        ne: 0
- lines:
  - 思慕ブランチ
  condition:
    TALENT:
      17:
        ne: 0
- lines:
  - ELSEブランチ
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:99", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSEブランチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test case 6: Null state should select ELSE branch.
    /// </summary>
    [Fact]
    public void Parse_WithNullState_SelectsElseBranch()
    {
        // Arrange
        var yamlContent = @"
character: 美鈴
situation: Test
branches:
- lines:
  - 恋人ブランチ
  condition:
    TALENT:
      16:
        ne: 0
- lines:
  - ELSEブランチ
  condition: {}
";

        // Act
        var result = _parser.Parse(yamlContent, state: null);

        // Assert
        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSEブランチ", result.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test case 7: Multiple operator types (eq, gt, lt, gte, lte) should work correctly.
    /// </summary>
    [Fact]
    public void Parse_WithDifferentOperators_EvaluatesCorrectly()
    {
        // Arrange - Test 'eq' operator
        var yamlEq = @"
character: Test
situation: Test
branches:
- lines:
  - Match eq
  condition:
    TALENT:
      5:
        eq: 10
- lines:
  - ELSE
  condition: {}
";

        var stateEq = new Dictionary<string, int> { { "TALENT:TARGET:5", 10 } };
        var resultEq = _parser.Parse(yamlEq, stateEq);
        Assert.Equal("Match eq", resultEq.DialogueLines[0].Text);

        // Arrange - Test 'gt' operator
        var yamlGt = @"
character: Test
situation: Test
branches:
- lines:
  - Match gt
  condition:
    TALENT:
      5:
        gt: 5
- lines:
  - ELSE
  condition: {}
";

        var stateGt = new Dictionary<string, int> { { "TALENT:TARGET:5", 10 } };
        var resultGt = _parser.Parse(yamlGt, stateGt);
        Assert.Equal("Match gt", resultGt.DialogueLines[0].Text);

        // Arrange - Test 'lt' operator
        var yamlLt = @"
character: Test
situation: Test
branches:
- lines:
  - Match lt
  condition:
    TALENT:
      5:
        lt: 15
- lines:
  - ELSE
  condition: {}
";

        var stateLt = new Dictionary<string, int> { { "TALENT:TARGET:5", 10 } };
        var resultLt = _parser.Parse(yamlLt, stateLt);
        Assert.Equal("Match lt", resultLt.DialogueLines[0].Text);

        // Arrange - Test 'gte' operator
        var yamlGte = @"
character: Test
situation: Test
branches:
- lines:
  - Match gte
  condition:
    TALENT:
      5:
        gte: 10
- lines:
  - ELSE
  condition: {}
";

        var stateGte = new Dictionary<string, int> { { "TALENT:TARGET:5", 10 } };
        var resultGte = _parser.Parse(yamlGte, stateGte);
        Assert.Equal("Match gte", resultGte.DialogueLines[0].Text);

        // Arrange - Test 'lte' operator
        var yamlLte = @"
character: Test
situation: Test
branches:
- lines:
  - Match lte
  condition:
    TALENT:
      5:
        lte: 10
- lines:
  - ELSE
  condition: {}
";

        var stateLte = new Dictionary<string, int> { { "TALENT:TARGET:5", 10 } };
        var resultLte = _parser.Parse(yamlLte, stateLte);
        Assert.Equal("Match lte", resultLte.DialogueLines[0].Text);
    }

    /// <summary>
    /// Test case 8: First matching branch should be selected (priority order).
    /// </summary>
    [Fact]
    public void Parse_WithMultipleMatchingConditions_SelectsFirstMatch()
    {
        // Arrange - Both TALENT:16 and TALENT:3 conditions would match
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - First match (恋人)
  condition:
    TALENT:
      16:
        ne: 0
- lines:
  - Second match (恋慕)
  condition:
    TALENT:
      3:
        ne: 0
- lines:
  - ELSE
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 1 },
            { "TALENT:TARGET:3", 1 }
        };

        // Act
        var result = _parser.Parse(yamlContent, state);

        // Assert - Should select first matching branch (恋人)
        Assert.Single(result.DialogueLines);
        Assert.Equal("First match (恋人)", result.DialogueLines[0].Text);
    }
}
