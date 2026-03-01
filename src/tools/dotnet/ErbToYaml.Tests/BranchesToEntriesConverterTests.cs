using System.Collections.Generic;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// Tests for BranchesToEntriesConverter compound condition support
/// Feature 755 - Task 9 (AC#8)
/// </summary>
public class BranchesToEntriesConverterTests
{
    /// <summary>
    /// AC#8: Test compound condition passthrough in branches-to-entries conversion
    /// Expected: AND compound condition is preserved without transformation
    /// Expected: GenerateId produces "and_compound_0" for compound entry
    /// </summary>
    [Fact]
    public void CompoundCondition_BranchesToEntries()
    {
        // Arrange - Create branches format with AND compound condition
        var branches = new List<object>
        {
            new Dictionary<string, object>
            {
                {
                    "lines", new List<string>
                    {
                        "複合条件分岐です"
                    }
                },
                {
                    "condition", new Dictionary<string, object>
                    {
                        {
                            "AND", new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    {
                                        "TALENT", new Dictionary<string, object>
                                        {
                                            {
                                                "3", new Dictionary<string, object>
                                                {
                                                    { "ne", 0 }
                                                }
                                            }
                                        }
                                    }
                                },
                                new Dictionary<string, object>
                                {
                                    {
                                        "CFLAG", new Dictionary<string, object>
                                        {
                                            {
                                                "1", new Dictionary<string, object>
                                                {
                                                    { "eq", 1 }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            new Dictionary<string, object>
            {
                {
                    "lines", new List<string>
                    {
                        "フォールバック"
                    }
                }
            }
        };

        // Act
        var entries = BranchesToEntriesConverter.Convert(branches);

        // Assert
        Assert.NotNull(entries);
        Assert.Equal(2, entries.Count);

        // Verify first entry (compound condition)
        var compoundEntry = entries[0];
        Assert.Equal("and_compound_0", compoundEntry["id"]);
        Assert.Equal("複合条件分岐です", compoundEntry["content"]);
        Assert.Equal(2, compoundEntry["priority"]);

        // Verify compound condition is passthrough-preserved (not transformed)
        Assert.True(compoundEntry.ContainsKey("condition"));
        var condition = (Dictionary<string, object>)compoundEntry["condition"];
        Assert.True(condition.ContainsKey("AND"));

        // Verify AND array structure is preserved
        var andArray = (List<object>)condition["AND"];
        Assert.Equal(2, andArray.Count);

        // Verify first operand (TALENT) is preserved
        var firstOperand = (Dictionary<string, object>)andArray[0];
        Assert.True(firstOperand.ContainsKey("TALENT"));

        // Verify second operand (CFLAG) is preserved
        var secondOperand = (Dictionary<string, object>)andArray[1];
        Assert.True(secondOperand.ContainsKey("CFLAG"));

        // Verify second entry (fallback)
        var fallbackEntry = entries[1];
        Assert.Equal("fallback", fallbackEntry["id"]);
        Assert.Equal("フォールバック", fallbackEntry["content"]);
        Assert.Equal(1, fallbackEntry["priority"]);
    }

    /// <summary>
    /// Test OR compound condition passthrough
    /// </summary>
    [Fact]
    public void OrCompoundCondition_BranchesToEntries()
    {
        // Arrange
        var branches = new List<object>
        {
            new Dictionary<string, object>
            {
                {
                    "lines", new List<string> { "OR条件" }
                },
                {
                    "condition", new Dictionary<string, object>
                    {
                        {
                            "OR", new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    { "TALENT", new Dictionary<string, object> { { "1", new Dictionary<string, object> { { "ne", 0 } } } } }
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
        Assert.Equal("or_compound_0", entries[0]["id"]);
        var condition = (Dictionary<string, object>)entries[0]["condition"];
        Assert.True(condition.ContainsKey("OR"));
    }

    /// <summary>
    /// Test NOT compound condition passthrough
    /// </summary>
    [Fact]
    public void NotCompoundCondition_BranchesToEntries()
    {
        // Arrange
        var branches = new List<object>
        {
            new Dictionary<string, object>
            {
                {
                    "lines", new List<string> { "NOT条件" }
                },
                {
                    "condition", new Dictionary<string, object>
                    {
                        {
                            "NOT", new Dictionary<string, object>
                            {
                                { "TALENT", new Dictionary<string, object> { { "1", new Dictionary<string, object> { { "ne", 0 } } } } }
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
        Assert.Equal("not_compound_0", entries[0]["id"]);
        var condition = (Dictionary<string, object>)entries[0]["condition"];
        Assert.True(condition.ContainsKey("NOT"));
    }
}
