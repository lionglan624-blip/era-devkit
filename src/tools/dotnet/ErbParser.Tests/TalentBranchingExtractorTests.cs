using System.Text.Json;
using ErbParser;
using ErbParser.Ast;
using Xunit;

namespace ErbParser.Tests;

/// <summary>
/// AC#1-5: TALENT branching extraction tests
/// </summary>
public class TalentBranchingExtractorTests
{
    private static readonly JsonSerializerOptions s_indentedOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// AC#1: Extract simple TALENT condition
    /// </summary>
    [Fact]
    public void ExtractSimpleCondition()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:恋人";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("恋人", result.Name);
        // When no explicit target, should default to empty or implicit target
        Assert.NotNull(result.Target);
    }

    [Fact]
    public void ExtractSimpleCondition_WithTarget()
    {
        // Arrange
        var parser = new TalentConditionParser();
        var condition = "TALENT:MASTER:恋人";

        // Act
        var result = parser.ParseTalentCondition(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.Equal("恋人", result.Name);
    }

    /// <summary>
    /// AC#2: Handle complex branching (IF/ELSEIF/ELSEIF/ELSE)
    /// </summary>
    [Fact]
    public void HandleComplexBranching()
    {
        // Arrange
        var erbParser = new ErbParser();
        var testFile = Path.Combine("TestData", "if_elseif_else.erb");

        // Act
        var ast = erbParser.Parse(testFile);
        var ifNode = ast.OfType<IfNode>().First();

        // Extract branches
        var branches = new List<ConditionBranch>();

        // IF branch
        var ifBranch = new ConditionBranch
        {
            Type = "if",
            Condition = new TalentConditionParser().ParseTalentCondition(ifNode.Condition),
            HasBody = ifNode.Body.Count > 0
        };
        branches.Add(ifBranch);

        // ELSEIF branches
        foreach (var elseIf in ifNode.ElseIfBranches)
        {
            var elseIfBranch = new ConditionBranch
            {
                Type = "elseif",
                Condition = new TalentConditionParser().ParseTalentCondition(elseIf.Condition),
                HasBody = elseIf.Body.Count > 0
            };
            branches.Add(elseIfBranch);
        }

        // ELSE branch
        if (ifNode.ElseBranch != null)
        {
            var elseBranch = new ConditionBranch
            {
                Type = "else",
                Condition = null,
                HasBody = ifNode.ElseBranch.Body.Count > 0
            };
            branches.Add(elseBranch);
        }

        // Assert
        Assert.Equal(4, branches.Count); // IF + 2 ELSEIF + ELSE
        Assert.Equal("if", branches[0].Type);
        Assert.Equal("elseif", branches[1].Type);
        Assert.Equal("elseif", branches[2].Type);
        Assert.Equal("else", branches[3].Type);

        // Verify conditions are parsed
        Assert.NotNull(branches[0].Condition);
        Assert.NotNull(branches[1].Condition);
        Assert.NotNull(branches[2].Condition);
        Assert.Null(branches[3].Condition); // ELSE has no condition
    }

    /// <summary>
    /// AC#3: Serialize condition tree to JSON
    /// </summary>
    [Fact]
    public void SerializeConditionTree()
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

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"type\"", json);
        Assert.Contains("\"if\"", json);
        Assert.Contains("\"condition\"", json);
        Assert.Contains("\"target\"", json);
        Assert.Contains("\"name\"", json);
        Assert.Contains("MASTER", json);
        Assert.Contains("恋人", json);
        Assert.Contains("\"hasBody\"", json);
        Assert.Contains("true", json);
    }

    /// <summary>
    /// AC#4: Invalid TALENT reference (null/empty) - graceful handling
    /// </summary>
    [Fact]
    public void InvalidTalentReference()
    {
        // Arrange
        var parser = new TalentConditionParser();

        // Act & Assert - null input
        var nullResult = parser.ParseTalentCondition(null!);
        Assert.Null(nullResult); // Should return null gracefully

        // Act & Assert - empty input
        var emptyResult = parser.ParseTalentCondition(string.Empty);
        Assert.Null(emptyResult); // Should return null gracefully
    }

    /// <summary>
    /// AC#5: Malformed TALENT condition - graceful error handling
    /// </summary>
    [Fact]
    public void MalformedTalentCondition()
    {
        // Arrange
        var parser = new TalentConditionParser();

        // Act & Assert - empty TALENT reference "TALENT:"
        var emptyTalent = parser.ParseTalentCondition("TALENT:");
        Assert.Null(emptyTalent); // Should return null for malformed input

        // Act & Assert - invalid syntax "TALENT::name"
        var doubleSeparator = parser.ParseTalentCondition("TALENT::恋人");
        Assert.Null(doubleSeparator); // Should return null for malformed input

        // Act & Assert - just "TALENT" without separator
        var noSeparator = parser.ParseTalentCondition("TALENT");
        Assert.Null(noSeparator); // Should return null for malformed input
    }
}
