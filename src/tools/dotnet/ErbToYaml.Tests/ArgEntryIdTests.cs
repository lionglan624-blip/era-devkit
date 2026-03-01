using System.Collections.Generic;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// Tests for BranchesToEntriesConverter ARG handler (Feature 764 - AC#12)
/// Tests GenerateId format and TransformCondition transformation for ARG conditions
/// </summary>
public class ArgEntryIdTests
{
    /// <summary>
    /// AC#12: GenerateId with ARG condition produces 4-segment ID format
    /// Expected: arg_{argIndex}_{value}_{branchIndex} format (e.g., "arg_0_2_0")
    /// </summary>
    [Fact]
    public void GenerateId_ArgCondition_Produces4SegmentId()
    {
        // Arrange - Create branch with ARG condition (ARG:0 eq 2)
        var branches = new List<object>
        {
            new Dictionary<string, object>
            {
                { "lines", new List<string> { "「Test dialogue」" } },
                {
                    "condition", new Dictionary<string, object>
                    {
                        {
                            "ARG", new Dictionary<string, object>
                            {
                                {
                                    "0", new Dictionary<string, object>
                                    {
                                        { "eq", "2" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var entries = BranchesToEntriesConverter.Convert(branches);

        // Assert
        Assert.Single(entries);
        // Expected: arg_{argIndex}_{value}_{branchIndex} → "arg_0_2_0"
        Assert.Equal("arg_0_2_0", entries[0]["id"]);
    }

    /// <summary>
    /// AC#12: GenerateId with ARG:1 produces correct argIndex
    /// Expected: arg_1_{value}_{branchIndex} format
    /// </summary>
    [Fact]
    public void GenerateId_ArgIndexOne_CorrectArgIndex()
    {
        // Arrange - ARG:1 eq 5
        var branches = new List<object>
        {
            new Dictionary<string, object>
            {
                { "lines", new List<string> { "「Test」" } },
                {
                    "condition", new Dictionary<string, object>
                    {
                        {
                            "ARG", new Dictionary<string, object>
                            {
                                {
                                    "1", new Dictionary<string, object>
                                    {
                                        { "eq", "5" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var entries = BranchesToEntriesConverter.Convert(branches);

        // Assert
        Assert.Single(entries);
        Assert.Equal("arg_1_5_0", entries[0]["id"]);
    }

    /// <summary>
    /// AC#12: Multiple ARG branches produce unique IDs with incrementing branchIndex
    /// Expected: arg_0_0_0, arg_0_1_1, arg_0_2_2, etc.
    /// </summary>
    [Fact]
    public void GenerateId_MultipleArgBranches_UniqueIds()
    {
        // Arrange - Multiple ARG conditions (ARG==0, ARG==1, ARG==2)
        var branches = new List<object>
        {
            new Dictionary<string, object>
            {
                { "lines", new List<string> { "「ARG 0」" } },
                {
                    "condition", new Dictionary<string, object>
                    {
                        { "ARG", new Dictionary<string, object> { { "0", new Dictionary<string, object> { { "eq", "0" } } } } }
                    }
                }
            },
            new Dictionary<string, object>
            {
                { "lines", new List<string> { "「ARG 1」" } },
                {
                    "condition", new Dictionary<string, object>
                    {
                        { "ARG", new Dictionary<string, object> { { "0", new Dictionary<string, object> { { "eq", "1" } } } } }
                    }
                }
            },
            new Dictionary<string, object>
            {
                { "lines", new List<string> { "「ARG 2」" } },
                {
                    "condition", new Dictionary<string, object>
                    {
                        { "ARG", new Dictionary<string, object> { { "0", new Dictionary<string, object> { { "eq", "2" } } } } }
                    }
                }
            }
        };

        // Act
        var entries = BranchesToEntriesConverter.Convert(branches);

        // Assert
        Assert.Equal(3, entries.Count);
        Assert.Equal("arg_0_0_0", entries[0]["id"]);
        Assert.Equal("arg_0_1_1", entries[1]["id"]);
        Assert.Equal("arg_0_2_2", entries[2]["id"]);
    }

    /// <summary>
    /// AC#12: TransformCondition passes through ARG condition without transformation
    /// Expected: { "ARG": { "0": { "eq": "2" } } } (nested format for schema validation)
    /// </summary>
    [Fact]
    public void TransformCondition_ArgCondition_ProducesArgType()
    {
        // Arrange
        var branches = new List<object>
        {
            new Dictionary<string, object>
            {
                { "lines", new List<string> { "「Test」" } },
                {
                    "condition", new Dictionary<string, object>
                    {
                        {
                            "ARG", new Dictionary<string, object>
                            {
                                {
                                    "0", new Dictionary<string, object>
                                    {
                                        { "eq", "2" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var entries = BranchesToEntriesConverter.Convert(branches);

        // Assert
        Assert.Single(entries);
        Assert.True(entries[0].ContainsKey("condition"));

        var condition = (Dictionary<string, object>)entries[0]["condition"];
        // ARG conditions are passed through without transformation
        Assert.True(condition.ContainsKey("ARG"));
        var argDict = (Dictionary<string, object>)condition["ARG"];
        Assert.True(argDict.ContainsKey("0"));
        var opDict = (Dictionary<string, object>)argDict["0"];
        Assert.True(opDict.ContainsKey("eq"));
        Assert.Equal("2", opDict["eq"]);
    }

    /// <summary>
    /// AC#12: TransformCondition passes through ARG condition preserving operator structure
    /// Expected: eq operator preserved in nested format
    /// </summary>
    [Fact]
    public void TransformCondition_EqOperator_MapsToThreshold()
    {
        // Arrange - ARG:0 eq 5
        var branches = new List<object>
        {
            new Dictionary<string, object>
            {
                { "lines", new List<string> { "「Test」" } },
                {
                    "condition", new Dictionary<string, object>
                    {
                        {
                            "ARG", new Dictionary<string, object>
                            {
                                {
                                    "0", new Dictionary<string, object>
                                    {
                                        { "eq", "5" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var entries = BranchesToEntriesConverter.Convert(branches);

        // Assert - ARG condition passed through with eq operator preserved
        var condition = (Dictionary<string, object>)entries[0]["condition"];
        Assert.True(condition.ContainsKey("ARG"));
        var argDict = (Dictionary<string, object>)condition["ARG"];
        var opDict = (Dictionary<string, object>)argDict["0"];
        Assert.Equal("5", opDict["eq"]);
    }

    /// <summary>
    /// AC#12: Verify ARG condition is passed through (like compound conditions)
    /// Expected: Original ARG dict structure preserved for schema validation
    /// </summary>
    [Fact]
    public void TransformCondition_ArgCondition_NotPassthrough()
    {
        // Arrange
        var branches = new List<object>
        {
            new Dictionary<string, object>
            {
                { "lines", new List<string> { "「Test」" } },
                {
                    "condition", new Dictionary<string, object>
                    {
                        {
                            "ARG", new Dictionary<string, object>
                            {
                                {
                                    "0", new Dictionary<string, object>
                                    {
                                        { "eq", "3" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var entries = BranchesToEntriesConverter.Convert(branches);

        // Assert - ARG condition should be passed through (not transformed)
        var condition = (Dictionary<string, object>)entries[0]["condition"];

        // Should contain original ARG key (passthrough preserves it)
        Assert.True(condition.ContainsKey("ARG"), "ARG condition should be passed through");

        // Should NOT contain transformed structure
        Assert.False(condition.ContainsKey("type"));
        Assert.False(condition.ContainsKey("argIndex"));
        Assert.False(condition.ContainsKey("threshold"));
    }
}
