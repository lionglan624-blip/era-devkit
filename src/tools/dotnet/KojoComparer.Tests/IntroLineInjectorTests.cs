#pragma warning disable CS8602 // Dereference of possibly null reference - Assert.NotNull guards these
#pragma warning disable xUnit1051 // Do not use TestContext in simple tests

using System.IO;
using System.Linq;
using KojoComparer;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KojoComparer.Tests;

/// <summary>
/// Tests for IntroLineInjector.ExtractIntroBranchesFromErb() branch order preservation.
/// F749: TALENT-aware Intro Line Injection.
/// </summary>
public class IntroLineInjectorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly IntroLineInjector _injector;

    public IntroLineInjectorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _injector = new IntroLineInjector();
    }

    /// <summary>
    /// AC#1: First IF branch intro extracts to index 0
    /// </summary>
    [Fact]
    public async Task BranchOrderExtraction_FirstIf()
    {
        // Arrange: Create ERB with IF branch containing intro line
        var erbPath = Path.Combine(_tempDir, "test_if.erb");
        var erbContent = @"
@COM_IF_BRANCH
IF TALENT:恋人
    PRINTFORM 恋人限定の台詞です
    PRINTDATA
    DATALIST
        DATAFORM 恋人なので
    ENDLIST
    ENDDATA
ENDIF
";
        await File.WriteAllTextAsync(erbPath, erbContent);

        // Create YAML with single branch
        var yamlPath = Path.Combine(_tempDir, "test_if.yaml");
        var yamlData = new KojoFileData
        {
            Character = "Test",
            Situation = "IF_BRANCH",
            Branches = new List<KojoBranch>
            {
                new KojoBranch
                {
                    Condition = new Dictionary<string, object> { { "TALENT", "恋人" } },
                    Lines = new List<string> { "恋人なので" }
                }
            }
        };
        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        await File.WriteAllTextAsync(yamlPath, serializer.Serialize(yamlData));

        // Act: Inject intro lines
        await _injector.InjectAsync(erbPath, yamlPath);

        // Assert: Intro line should be at index 0 of branches[0].lines
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        var result = deserializer.Deserialize<KojoFileData>(await File.ReadAllTextAsync(yamlPath));

        Assert.NotNull(result?.Branches);
        Assert.Single(result.Branches);
        Assert.NotNull(result.Branches[0]?.Lines);
        Assert.True(result.Branches[0].Lines!.Count >= 2);
        Assert.Equal("恋人限定の台詞です", result.Branches[0].Lines[0]);
        Assert.Equal("恋人なので", result.Branches[0].Lines[1]);
    }

    /// <summary>
    /// AC#2: First ELSEIF branch intro extracts to index 1
    /// </summary>
    [Fact]
    public async Task BranchOrderExtraction_FirstElseIf()
    {
        // Arrange: Create ERB with IF/ELSEIF branches
        var erbPath = Path.Combine(_tempDir, "test_elseif.erb");
        var erbContent = @"
@COM_IF_ELSEIF
IF TALENT:恋人
    PRINTFORM IF branch intro
    PRINTDATA
    DATALIST
        DATAFORM IF dialogue
    ENDLIST
    ENDDATA
ELSEIF TALENT:親友
    PRINTFORM ELSEIF branch intro
    PRINTDATA
    DATALIST
        DATAFORM ELSEIF dialogue
    ENDLIST
    ENDDATA
ENDIF
";
        await File.WriteAllTextAsync(erbPath, erbContent);

        // Create YAML with two branches
        var yamlPath = Path.Combine(_tempDir, "test_elseif.yaml");
        var yamlData = new KojoFileData
        {
            Character = "Test",
            Situation = "IF_ELSEIF",
            Branches = new List<KojoBranch>
            {
                new KojoBranch
                {
                    Condition = new Dictionary<string, object> { { "TALENT", "恋人" } },
                    Lines = new List<string> { "IF dialogue" }
                },
                new KojoBranch
                {
                    Condition = new Dictionary<string, object> { { "TALENT", "親友" } },
                    Lines = new List<string> { "ELSEIF dialogue" }
                }
            }
        };
        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        await File.WriteAllTextAsync(yamlPath, serializer.Serialize(yamlData));

        // Act: Inject intro lines
        await _injector.InjectAsync(erbPath, yamlPath);

        // Assert: ELSEIF intro should be at index 0 of branches[1].lines
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        var result = deserializer.Deserialize<KojoFileData>(await File.ReadAllTextAsync(yamlPath));

        Assert.NotNull(result?.Branches);
        Assert.Equal(2, result.Branches.Count);

        // Verify IF branch (index 0)
        Assert.NotNull(result.Branches[0]?.Lines);
        Assert.True(result.Branches[0].Lines!.Count >= 2);
        Assert.Equal("IF branch intro", result.Branches[0].Lines[0]);

        // Verify ELSEIF branch (index 1)
        Assert.NotNull(result.Branches[1]?.Lines);
        Assert.True(result.Branches[1].Lines!.Count >= 2);
        Assert.Equal("ELSEIF branch intro", result.Branches[1].Lines[0]);
    }

    /// <summary>
    /// AC#3: ELSE branch intro extracts to last index
    /// </summary>
    [Fact]
    public async Task BranchOrderExtraction_Else()
    {
        // Arrange: Create ERB with IF/ELSEIF/ELSE branches
        var erbPath = Path.Combine(_tempDir, "test_else.erb");
        var erbContent = @"
@COM_IF_ELSEIF_ELSE
IF TALENT:恋人
    PRINTFORM IF intro
    PRINTDATA
    DATALIST
        DATAFORM IF dialogue
    ENDLIST
    ENDDATA
ELSEIF TALENT:親友
    PRINTFORM ELSEIF intro
    PRINTDATA
    DATALIST
        DATAFORM ELSEIF dialogue
    ENDLIST
    ENDDATA
ELSE
    PRINTFORM ELSE intro
    PRINTDATA
    DATALIST
        DATAFORM ELSE dialogue
    ENDLIST
    ENDDATA
ENDIF
";
        await File.WriteAllTextAsync(erbPath, erbContent);

        // Create YAML with three branches
        var yamlPath = Path.Combine(_tempDir, "test_else.yaml");
        var yamlData = new KojoFileData
        {
            Character = "Test",
            Situation = "IF_ELSEIF_ELSE",
            Branches = new List<KojoBranch>
            {
                new KojoBranch
                {
                    Condition = new Dictionary<string, object> { { "TALENT", "恋人" } },
                    Lines = new List<string> { "IF dialogue" }
                },
                new KojoBranch
                {
                    Condition = new Dictionary<string, object> { { "TALENT", "親友" } },
                    Lines = new List<string> { "ELSEIF dialogue" }
                },
                new KojoBranch
                {
                    Condition = new Dictionary<string, object>(), // Empty = ELSE
                    Lines = new List<string> { "ELSE dialogue" }
                }
            }
        };
        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        await File.WriteAllTextAsync(yamlPath, serializer.Serialize(yamlData));

        // Act: Inject intro lines
        await _injector.InjectAsync(erbPath, yamlPath);

        // Assert: ELSE intro should be at index 0 of branches[last].lines
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        var result = deserializer.Deserialize<KojoFileData>(await File.ReadAllTextAsync(yamlPath));

        Assert.NotNull(result?.Branches);
        Assert.Equal(3, result.Branches.Count);

        // Verify IF branch (index 0)
        Assert.NotNull(result.Branches[0]?.Lines);
        Assert.True(result.Branches[0].Lines!.Count >= 2);
        Assert.Equal("IF intro", result.Branches[0].Lines[0]);

        // Verify ELSEIF branch (index 1)
        Assert.NotNull(result.Branches[1]?.Lines);
        Assert.True(result.Branches[1].Lines!.Count >= 2);
        Assert.Equal("ELSEIF intro", result.Branches[1].Lines[0]);

        // Verify ELSE branch (index 2 = last)
        Assert.NotNull(result.Branches[2]?.Lines);
        Assert.True(result.Branches[2].Lines!.Count >= 2);
        Assert.Equal("ELSE intro", result.Branches[2].Lines[0]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }
}
