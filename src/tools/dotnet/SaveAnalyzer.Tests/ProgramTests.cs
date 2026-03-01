using Xunit;

namespace SaveAnalyzer.Tests;

public class ProgramTests
{
    [Fact]
    public void FilterGlobals_NullFilter_ReturnsOriginal()
    {
        // Arrange
        var globals = new Dictionary<string, Dictionary<int, long>>
        {
            ["FLAG"] = new Dictionary<int, long> { { 0, 1 }, { 1, 2 } },
            ["TFLAG"] = new Dictionary<int, long> { { 0, 3 } }
        };

        // Act
        var result = Program.FilterGlobals(globals, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("FLAG"));
        Assert.True(result.ContainsKey("TFLAG"));
    }

    [Fact]
    public void FilterGlobals_VariableFilter_ReturnsMatching()
    {
        // Arrange
        var globals = new Dictionary<string, Dictionary<int, long>>
        {
            ["FLAG"] = new Dictionary<int, long> { { 0, 1 }, { 1, 2 } },
            ["TFLAG"] = new Dictionary<int, long> { { 0, 3 } }
        };

        // Act
        var result = Program.FilterGlobals(globals, "FLAG");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.ContainsKey("FLAG"));
        Assert.False(result.ContainsKey("TFLAG"));
    }

    [Fact]
    public void FilterGlobals_IndexedFilter_ReturnsSingleValue()
    {
        // Arrange
        var globals = new Dictionary<string, Dictionary<int, long>>
        {
            ["FLAG"] = new Dictionary<int, long> { { 0, 1 }, { 1, 2 }, { 2, 3 } }
        };

        // Act
        var result = Program.FilterGlobals(globals, "FLAG:1");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.ContainsKey("FLAG"));
        Assert.Single(result["FLAG"]);
        Assert.Equal(2, result["FLAG"][1]);
    }

    [Fact]
    public void FilterCharacters_NullFilters_ReturnsAll()
    {
        // Arrange
        var characters = new List<CharacterData>
        {
            new CharacterData { Id = 0, Name = "咲夜", CallName = "咲夜", Arrays = new Dictionary<string, Dictionary<int, long>>() },
            new CharacterData { Id = 1, Name = "美鈴", CallName = "美鈴", Arrays = new Dictionary<string, Dictionary<int, long>>() }
        };

        // Act
        var result = Program.FilterCharacters(characters, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FilterCharacters_CharacterFilter_ReturnsMatching()
    {
        // Arrange
        var characters = new List<CharacterData>
        {
            new CharacterData { Id = 0, Name = "咲夜", CallName = "咲夜", Arrays = new Dictionary<string, Dictionary<int, long>>() },
            new CharacterData { Id = 1, Name = "美鈴", CallName = "美鈴", Arrays = new Dictionary<string, Dictionary<int, long>>() }
        };

        // Act
        var result = Program.FilterCharacters(characters, "咲夜", null);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public void FilterGlobals_EmptyFilter_ReturnsOriginal()
    {
        // Arrange
        var globals = new Dictionary<string, Dictionary<int, long>>
        {
            ["FLAG"] = new Dictionary<int, long> { { 0, 1 } }
        };

        // Act
        var result = Program.FilterGlobals(globals, "");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public void FilterGlobals_IndexedFilter_MissingIndex_ReturnsNull()
    {
        // Arrange
        var globals = new Dictionary<string, Dictionary<int, long>>
        {
            ["FLAG"] = new Dictionary<int, long> { { 0, 1 }, { 1, 2 } }
        };

        // Act
        var result = Program.FilterGlobals(globals, "FLAG:99");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FilterGlobals_CaseInsensitive_ReturnsMatching()
    {
        // Arrange
        var globals = new Dictionary<string, Dictionary<int, long>>
        {
            ["FLAG"] = new Dictionary<int, long> { { 0, 1 } }
        };

        // Act
        var result = Program.FilterGlobals(globals, "flag");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.ContainsKey("FLAG"));
    }

    [Fact]
    public void FilterGlobals_NonExistentVariable_ReturnsNull()
    {
        // Arrange
        var globals = new Dictionary<string, Dictionary<int, long>>
        {
            ["FLAG"] = new Dictionary<int, long> { { 0, 1 } }
        };

        // Act
        var result = Program.FilterGlobals(globals, "NONEXISTENT");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FilterCharacters_VariableFilter_ReturnsOnlyMatching()
    {
        // Arrange
        var characters = new List<CharacterData>
        {
            new CharacterData
            {
                Id = 0,
                Name = "咲夜",
                CallName = "咲夜",
                Arrays = new Dictionary<string, Dictionary<int, long>>
                {
                    ["CFLAG"] = new Dictionary<int, long> { { 297, 1 } }
                }
            },
            new CharacterData
            {
                Id = 1,
                Name = "美鈴",
                CallName = "美鈴",
                Arrays = new Dictionary<string, Dictionary<int, long>>
                {
                    ["BASE"] = new Dictionary<int, long> { { 0, 5 } }
                }
            }
        };

        // Act
        var result = Program.FilterCharacters(characters, null, "CFLAG");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Only 咲夜 has CFLAG
    }

    [Fact]
    public void FilterCharacters_VariableIndexFilter_ReturnsMatching()
    {
        // Arrange
        var characters = new List<CharacterData>
        {
            new CharacterData
            {
                Id = 0,
                Name = "咲夜",
                CallName = "咲夜",
                Arrays = new Dictionary<string, Dictionary<int, long>>
                {
                    ["CFLAG"] = new Dictionary<int, long> { { 297, 1 }, { 298, 2 } }
                }
            }
        };

        // Act
        var result = Program.FilterCharacters(characters, null, "CFLAG:297");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        // Result should only contain CFLAG:297
    }

    [Fact]
    public void FilterCharacters_CombinedFilters_ReturnsIntersection()
    {
        // Arrange
        var characters = new List<CharacterData>
        {
            new CharacterData
            {
                Id = 0,
                Name = "咲夜",
                CallName = "咲夜",
                Arrays = new Dictionary<string, Dictionary<int, long>>
                {
                    ["CFLAG"] = new Dictionary<int, long> { { 297, 1 } }
                }
            },
            new CharacterData
            {
                Id = 1,
                Name = "美鈴",
                CallName = "美鈴",
                Arrays = new Dictionary<string, Dictionary<int, long>>
                {
                    ["CFLAG"] = new Dictionary<int, long> { { 297, 2 } }
                }
            }
        };

        // Act
        var result = Program.FilterCharacters(characters, "咲夜", "CFLAG");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Only 咲夜 matches both filters
    }

    [Fact]
    public void FilterCharacters_CallNameFilter_ReturnsMatching()
    {
        // Arrange
        var characters = new List<CharacterData>
        {
            new CharacterData { Id = 0, Name = "十六夜咲夜", CallName = "咲夜", Arrays = new Dictionary<string, Dictionary<int, long>>() },
            new CharacterData { Id = 1, Name = "紅美鈴", CallName = "美鈴", Arrays = new Dictionary<string, Dictionary<int, long>>() }
        };

        // Act
        var result = Program.FilterCharacters(characters, "咲夜", null);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Matches both Name and CallName
    }
}
