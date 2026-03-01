using Xunit;
using YamlDotNet.RepresentationModel;

namespace YamlTalentMigrator.Tests;

/// <summary>
/// Tests for ShouldUpdateBranch and ShouldUpdateBranchDict logic.
/// Tests both YAML node-based and Dictionary-based branch condition evaluation.
/// </summary>
public class BranchUpdateTests
{
    #region ShouldUpdateBranch Tests (YamlMappingNode)

    [Fact]
    public void ShouldUpdateBranch_NoConditionKey_ReturnsTrue()
    {
        // Arrange
        var yaml = @"entries:
- id: test1
  content: ""test content""
";
        var branch = ParseYamlBranch(yaml);

        // Act
        bool result = InvokeShouldUpdateBranch(branch, 0);

        // Assert
        Assert.True(result, "Branch without condition key should be updated");
    }

    [Fact]
    public void ShouldUpdateBranch_NullCondition_ReturnsTrue()
    {
        // Arrange
        var yaml = @"entries:
- id: test1
  content: ""test content""
condition: null
";
        var branch = ParseYamlBranch(yaml);

        // Act
        bool result = InvokeShouldUpdateBranch(branch, 0);

        // Assert
        Assert.True(result, "Branch with null condition should be updated");
    }

    [Fact]
    public void ShouldUpdateBranch_TildeCondition_ReturnsTrue()
    {
        // Arrange
        var yaml = @"entries:
- id: test1
  content: ""test content""
condition: ~
";
        var branch = ParseYamlBranch(yaml);

        // Act
        bool result = InvokeShouldUpdateBranch(branch, 0);

        // Assert
        Assert.True(result, "Branch with tilde (~) condition should be updated");
    }

    [Fact]
    public void ShouldUpdateBranch_EmptyMappingCondition_ReturnsTrue()
    {
        // Arrange
        var yaml = @"entries:
- id: test1
  content: ""test content""
condition: {}
";
        var branch = ParseYamlBranch(yaml);

        // Act
        bool result = InvokeShouldUpdateBranch(branch, 0);

        // Assert
        Assert.True(result, "Branch with empty mapping condition should be updated");
    }

    [Fact]
    public void ShouldUpdateBranch_ValidCondition_ReturnsFalse()
    {
        // Arrange
        var yaml = @"entries:
- id: test1
  content: ""test content""
condition:
  TALENT:
    16:
      ne: 0
";
        var branch = ParseYamlBranch(yaml);

        // Act
        bool result = InvokeShouldUpdateBranch(branch, 0);

        // Assert
        Assert.False(result, "Branch with valid condition should not be updated");
    }

    [Fact]
    public void ShouldUpdateBranch_IndexGreaterThan3_ReturnsFalse()
    {
        // Arrange
        var yaml = @"entries:
- id: test1
  content: ""test content""
";
        var branch = ParseYamlBranch(yaml);

        // Act
        bool result = InvokeShouldUpdateBranch(branch, 4);

        // Assert
        Assert.False(result, "Branch with index > 3 should not be updated");
    }

    [Fact]
    public void ShouldUpdateBranch_Index3_NoCondition_ReturnsTrue()
    {
        // Arrange
        var yaml = @"entries:
- id: test1
  content: ""test content""
";
        var branch = ParseYamlBranch(yaml);

        // Act
        bool result = InvokeShouldUpdateBranch(branch, 3);

        // Assert
        Assert.True(result, "Branch with index 3 and no condition should be updated");
    }

    [Fact]
    public void ShouldUpdateBranch_Index3_WithCondition_ReturnsFalse()
    {
        // Arrange
        var yaml = @"entries:
- id: test1
  content: ""test content""
condition:
  FLAG:
    10:
      eq: 1
";
        var branch = ParseYamlBranch(yaml);

        // Act
        bool result = InvokeShouldUpdateBranch(branch, 3);

        // Assert
        Assert.False(result, "Branch with index 3 and valid condition should not be updated");
    }

    #endregion

    #region ShouldUpdateBranchDict Tests (Dictionary)

    [Fact]
    public void ShouldUpdateBranchDict_NoConditionKey_ReturnsTrue()
    {
        // Arrange
        var branch = new Dictionary<object, object>
        {
            { "entries", new List<object>() }
        };

        // Act
        bool result = InvokeShouldUpdateBranchDict(branch, 0);

        // Assert
        Assert.True(result, "Branch dict without condition key should be updated");
    }

    [Fact]
    public void ShouldUpdateBranchDict_NullCondition_ReturnsTrue()
    {
        // Arrange
        var branch = new Dictionary<object, object>
        {
            { "entries", new List<object>() },
            { "condition", null! }
        };

        // Act
        bool result = InvokeShouldUpdateBranchDict(branch, 0);

        // Assert
        Assert.True(result, "Branch dict with null condition should be updated");
    }

    [Fact]
    public void ShouldUpdateBranchDict_EmptyDictCondition_ReturnsTrue()
    {
        // Arrange
        var branch = new Dictionary<object, object>
        {
            { "entries", new List<object>() },
            { "condition", new Dictionary<object, object>() }
        };

        // Act
        bool result = InvokeShouldUpdateBranchDict(branch, 0);

        // Assert
        Assert.True(result, "Branch dict with empty dict condition should be updated");
    }

    [Fact]
    public void ShouldUpdateBranchDict_ValidCondition_ReturnsFalse()
    {
        // Arrange
        var branch = new Dictionary<object, object>
        {
            { "entries", new List<object>() },
            { "condition", new Dictionary<object, object>
                {
                    { "TALENT", new Dictionary<object, object>
                        {
                            { 16, new Dictionary<object, object> { { "ne", 0 } } }
                        }
                    }
                }
            }
        };

        // Act
        bool result = InvokeShouldUpdateBranchDict(branch, 0);

        // Assert
        Assert.False(result, "Branch dict with valid condition should not be updated");
    }

    [Fact]
    public void ShouldUpdateBranchDict_IndexGreaterThan3_ReturnsFalse()
    {
        // Arrange
        var branch = new Dictionary<object, object>
        {
            { "entries", new List<object>() }
        };

        // Act
        bool result = InvokeShouldUpdateBranchDict(branch, 4);

        // Assert
        Assert.False(result, "Branch dict with index > 3 should not be updated");
    }

    [Fact]
    public void ShouldUpdateBranchDict_Index3_NoCondition_ReturnsTrue()
    {
        // Arrange
        var branch = new Dictionary<object, object>
        {
            { "entries", new List<object>() }
        };

        // Act
        bool result = InvokeShouldUpdateBranchDict(branch, 3);

        // Assert
        Assert.True(result, "Branch dict with index 3 and no condition should be updated");
    }

    [Fact]
    public void ShouldUpdateBranchDict_Index3_WithCondition_ReturnsFalse()
    {
        // Arrange
        var branch = new Dictionary<object, object>
        {
            { "entries", new List<object>() },
            { "condition", new Dictionary<object, object>
                {
                    { "FLAG", new Dictionary<object, object>
                        {
                            { 10, new Dictionary<object, object> { { "eq", 1 } } }
                        }
                    }
                }
            }
        };

        // Act
        bool result = InvokeShouldUpdateBranchDict(branch, 3);

        // Assert
        Assert.False(result, "Branch dict with index 3 and valid condition should not be updated");
    }

    [Fact]
    public void ShouldUpdateBranchDict_NonEmptyDictCondition_ReturnsFalse()
    {
        // Arrange
        var branch = new Dictionary<object, object>
        {
            { "entries", new List<object>() },
            { "condition", new Dictionary<object, object>
                {
                    { "someKey", "someValue" }
                }
            }
        };

        // Act
        bool result = InvokeShouldUpdateBranchDict(branch, 0);

        // Assert
        Assert.False(result, "Branch dict with non-empty dict condition should not be updated");
    }

    [Fact]
    public void ShouldUpdateBranchDict_ConditionIsString_ReturnsFalse()
    {
        // Arrange - condition is not a dict but a string
        var branch = new Dictionary<object, object>
        {
            { "entries", new List<object>() },
            { "condition", "some_string_value" }
        };

        // Act
        bool result = InvokeShouldUpdateBranchDict(branch, 0);

        // Assert
        Assert.False(result, "Branch dict with string condition should not be updated");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Parses YAML string into YamlMappingNode
    /// </summary>
    private YamlMappingNode ParseYamlBranch(string yaml)
    {
        var input = new StringReader(yaml);
        var yamlStream = new YamlStream();
        yamlStream.Load(input);

        if (yamlStream.Documents.Count == 0)
        {
            throw new InvalidOperationException("No YAML documents found");
        }

        var root = yamlStream.Documents[0].RootNode as YamlMappingNode;
        if (root == null)
        {
            throw new InvalidOperationException("Root node is not a mapping");
        }

        return root;
    }

    /// <summary>
    /// Invokes private ShouldUpdateBranch method via reflection
    /// </summary>
    private bool InvokeShouldUpdateBranch(YamlMappingNode branch, int index)
    {
        var method = typeof(Program).GetMethod("ShouldUpdateBranch",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (method == null)
        {
            throw new InvalidOperationException("ShouldUpdateBranch method not found");
        }

        var result = method.Invoke(null, new object[] { branch, index });
        return result is bool b && b;
    }

    /// <summary>
    /// Invokes private ShouldUpdateBranchDict method via reflection
    /// </summary>
    private bool InvokeShouldUpdateBranchDict(Dictionary<object, object> branch, int index)
    {
        var method = typeof(Program).GetMethod("ShouldUpdateBranchDict",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (method == null)
        {
            throw new InvalidOperationException("ShouldUpdateBranchDict method not found");
        }

        var result = method.Invoke(null, new object[] { branch, index });
        return result is bool b && b;
    }

    #endregion
}
