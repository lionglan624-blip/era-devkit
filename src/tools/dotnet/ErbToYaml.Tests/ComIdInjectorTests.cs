using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// Unit tests for ComIdInjector and InjectResult.
/// Covers ExtractComId (internal static via reflection), InjectAsync, InjectForErbFileAsync,
/// InjectComIdIntoFileAsync, and InjectResult tracking.
/// </summary>
public class ComIdInjectorTests : IDisposable
{
    private readonly string _tempDir;

    // Cached reflection access to internal static ExtractComId
    private static readonly MethodInfo ExtractComIdMethod =
        typeof(ComIdInjector).GetMethod("ExtractComId",
            BindingFlags.NonPublic | BindingFlags.Static)!;

    public ComIdInjectorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ComIdInjectorTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    /// <summary>Helper to invoke internal static ExtractComId via reflection.</summary>
    private static int? InvokeExtractComId(string functionName)
    {
        return (int?)ExtractComIdMethod.Invoke(null, [functionName]);
    }

    #region ExtractComId Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_StandardComFunction_ReturnsComId()
    {
        // @KOJO_MESSAGE_COM_K1_352 → comId = 352
        Assert.Equal(352, InvokeExtractComId("KOJO_MESSAGE_COM_K1_352"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_StandardComFunction_WithSubIndex_ReturnsComId()
    {
        // @KOJO_MESSAGE_COM_K1_100_0 → comId = 100
        Assert.Equal(100, InvokeExtractComId("KOJO_MESSAGE_COM_K1_100_0"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_CounterFunction_ReturnsComId()
    {
        // @KOJO_MESSAGE_COUNTER_K2_50 → comId = 50
        Assert.Equal(50, InvokeExtractComId("KOJO_MESSAGE_COUNTER_K2_50"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_NtrComFunction_ReturnsComId()
    {
        // @NTR_KOJO_MESSAGE_COM_K4_200 → comId = 200
        Assert.Equal(200, InvokeExtractComId("NTR_KOJO_MESSAGE_COM_K4_200"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_NtrDialogueFunction_ReturnsComId()
    {
        // @NTR_KOJO_K3_75 → comId = 75
        Assert.Equal(75, InvokeExtractComId("NTR_KOJO_K3_75"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_NtrDialogueFunctionWithSubIndex_ReturnsComId()
    {
        // @NTR_KOJO_K3_75_1 → comId = 75
        Assert.Equal(75, InvokeExtractComId("NTR_KOJO_K3_75_1"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_NtrKojoW_Variant_ReturnsComId()
    {
        // @NTR_KOJO_KW1_30 → comId = 30
        Assert.Equal(30, InvokeExtractComId("NTR_KOJO_KW1_30"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_MultiDigitCharId_ReturnsComId()
    {
        // @KOJO_MESSAGE_COM_K10_999 → comId = 999
        Assert.Equal(999, InvokeExtractComId("KOJO_MESSAGE_COM_K10_999"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_UnknownFunctionName_ReturnsNull()
    {
        // Unrelated function → no COM ID
        Assert.Null(InvokeExtractComId("SOME_RANDOM_FUNCTION"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_EmptyString_ReturnsNull()
    {
        Assert.Null(InvokeExtractComId(string.Empty));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_PlainKojoFunction_ReturnsNull()
    {
        // @KOJO_MESSAGE_K1_愛撫 does not match COM/COUNTER/NTR_KOJO patterns
        Assert.Null(InvokeExtractComId("KOJO_MESSAGE_K1_愛撫"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractComId_UCharId_ReturnsComId()
    {
        // @KOJO_MESSAGE_COM_KU_5 → comId = 5
        Assert.Equal(5, InvokeExtractComId("KOJO_MESSAGE_COM_KU_5"));
    }

    #endregion

    #region InjectResult Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void InjectResult_DefaultValues_AreZero()
    {
        var result = new InjectResult();
        Assert.Equal(0, result.Injected);
        Assert.Equal(0, result.Skipped);
        Assert.Empty(result.Errors);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InjectResult_IncrementInjected_Tracks()
    {
        var result = new InjectResult();
        result.Injected++;
        result.Injected++;
        Assert.Equal(2, result.Injected);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InjectResult_IncrementSkipped_Tracks()
    {
        var result = new InjectResult();
        result.Skipped++;
        Assert.Equal(1, result.Skipped);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InjectResult_AddError_TracksErrors()
    {
        var result = new InjectResult();
        result.Errors.Add("error1");
        result.Errors.Add("error2");
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains("error1", result.Errors);
        Assert.Contains("error2", result.Errors);
    }

    #endregion

    #region ComIdInjector Constructor Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_NullPathAnalyzer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ComIdInjector(null!));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ValidPathAnalyzer_Succeeds()
    {
        var pathAnalyzer = new PathAnalyzer();
        var injector = new ComIdInjector(pathAnalyzer);
        Assert.NotNull(injector);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_NullLocalGateResolver_IsOptional()
    {
        var pathAnalyzer = new PathAnalyzer();
        var injector = new ComIdInjector(pathAnalyzer, null);
        Assert.NotNull(injector);
    }

    #endregion

    #region InjectAsync Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InjectAsync_EmptyErbDirectory_ReturnsEmptyResult()
    {
        // Arrange
        var erbDir = Path.Combine(_tempDir, "erbs_empty");
        var yamlDir = Path.Combine(_tempDir, "yamls_empty");
        Directory.CreateDirectory(erbDir);
        Directory.CreateDirectory(yamlDir);

        var injector = new ComIdInjector(new PathAnalyzer());

        // Act
        var result = await injector.InjectAsync(erbDir, yamlDir);

        // Assert
        Assert.Equal(0, result.Injected);
        Assert.Equal(0, result.Skipped);
        Assert.Empty(result.Errors);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InjectAsync_ErbFileWithNoMatchingYamlDir_ReturnsNoInjections()
    {
        // Arrange - ERB exists but no matching YAML character directory
        var erbDir = Path.Combine(_tempDir, "erbs_nomatch");
        var charErbDir = Path.Combine(erbDir, "1_美鈴");
        var yamlDir = Path.Combine(_tempDir, "yamls_nomatch");
        Directory.CreateDirectory(charErbDir);
        Directory.CreateDirectory(yamlDir);

        var erbContent = BuildErbWithComFunction("KOJO_MESSAGE_COM_K1_10");
        File.WriteAllText(Path.Combine(charErbDir, "KOJO_K1_愛撫.ERB"), erbContent, Encoding.UTF8);

        var injector = new ComIdInjector(new PathAnalyzer());

        // Act
        var result = await injector.InjectAsync(erbDir, yamlDir);

        // Assert - no YAML directory for char "1", nothing injected
        Assert.Equal(0, result.Injected);
        Assert.Equal(0, result.Skipped);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InjectAsync_ErbWithComFunction_InjectsComIdIntoYaml()
    {
        // Arrange
        var erbDir = Path.Combine(_tempDir, "erbs_inject");
        var charErbDir = Path.Combine(erbDir, "1_美鈴");
        var yamlDir = Path.Combine(_tempDir, "yamls_inject");
        var charYamlDir = Path.Combine(yamlDir, "1_美鈴");
        Directory.CreateDirectory(charErbDir);
        Directory.CreateDirectory(charYamlDir);

        // ERB: function with COM ID 42
        var erbContent = BuildErbWithComFunction("KOJO_MESSAGE_COM_K1_42");
        File.WriteAllText(Path.Combine(charErbDir, "KOJO_K1_愛撫.ERB"), erbContent, Encoding.UTF8);

        // YAML: matching file without com_id
        var yamlPath = Path.Combine(charYamlDir, "K1_愛撫_0.yaml");
        File.WriteAllText(yamlPath, BuildYamlWithoutComId("K1_愛撫"), Encoding.UTF8);

        var injector = new ComIdInjector(new PathAnalyzer());

        // Act
        var result = await injector.InjectAsync(erbDir, yamlDir);

        // Assert
        Assert.Equal(1, result.Injected);
        Assert.Equal(0, result.Skipped);
        var updatedContent = File.ReadAllText(yamlPath, Encoding.UTF8);
        Assert.Contains("com_id: 42", updatedContent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InjectAsync_YamlAlreadyHasComId_Skips()
    {
        // Arrange
        var erbDir = Path.Combine(_tempDir, "erbs_skip");
        var charErbDir = Path.Combine(erbDir, "1_美鈴");
        var yamlDir = Path.Combine(_tempDir, "yamls_skip");
        var charYamlDir = Path.Combine(yamlDir, "1_美鈴");
        Directory.CreateDirectory(charErbDir);
        Directory.CreateDirectory(charYamlDir);

        var erbContent = BuildErbWithComFunction("KOJO_MESSAGE_COM_K1_42");
        File.WriteAllText(Path.Combine(charErbDir, "KOJO_K1_愛撫.ERB"), erbContent, Encoding.UTF8);

        var yamlPath = Path.Combine(charYamlDir, "K1_愛撫_0.yaml");
        File.WriteAllText(yamlPath, BuildYamlWithComId("K1_愛撫", 42), Encoding.UTF8);

        var injector = new ComIdInjector(new PathAnalyzer());

        // Act
        var result = await injector.InjectAsync(erbDir, yamlDir);

        // Assert
        Assert.Equal(0, result.Injected);
        Assert.Equal(1, result.Skipped);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InjectAsync_ErbFileWithNoPathPattern_IsSkipped()
    {
        // Arrange - ERB in non-numbered directory (PathAnalyzer throws ArgumentException)
        var erbDir = Path.Combine(_tempDir, "erbs_badpath");
        var invalidCharDir = Path.Combine(erbDir, "美鈴"); // no N_ prefix
        var yamlDir = Path.Combine(_tempDir, "yamls_badpath");
        Directory.CreateDirectory(invalidCharDir);
        Directory.CreateDirectory(yamlDir);

        var erbContent = BuildErbWithComFunction("KOJO_MESSAGE_COM_K1_10");
        File.WriteAllText(Path.Combine(invalidCharDir, "KOJO_K1_愛撫.ERB"), erbContent, Encoding.UTF8);

        var injector = new ComIdInjector(new PathAnalyzer());

        // Act - should not throw, silently skip unmatched paths
        var result = await injector.InjectAsync(erbDir, yamlDir);

        // Assert
        Assert.Equal(0, result.Injected);
        Assert.Empty(result.Errors);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InjectAsync_MultipleYamlFiles_InjectsIntoAll()
    {
        // Arrange - ERB with 2 COM functions → 2 YAML files
        var erbDir = Path.Combine(_tempDir, "erbs_multi");
        var charErbDir = Path.Combine(erbDir, "1_美鈴");
        var yamlDir = Path.Combine(_tempDir, "yamls_multi");
        var charYamlDir = Path.Combine(yamlDir, "1_美鈴");
        Directory.CreateDirectory(charErbDir);
        Directory.CreateDirectory(charYamlDir);

        var erbContent = BuildErbWithTwoComFunctions("KOJO_MESSAGE_COM_K1_10", "KOJO_MESSAGE_COM_K1_20");
        File.WriteAllText(Path.Combine(charErbDir, "KOJO_K1_愛撫.ERB"), erbContent, Encoding.UTF8);

        var yaml0Path = Path.Combine(charYamlDir, "K1_愛撫_0.yaml");
        var yaml1Path = Path.Combine(charYamlDir, "K1_愛撫_1.yaml");
        File.WriteAllText(yaml0Path, BuildYamlWithoutComId("K1_愛撫"), Encoding.UTF8);
        File.WriteAllText(yaml1Path, BuildYamlWithoutComId("K1_愛撫"), Encoding.UTF8);

        var injector = new ComIdInjector(new PathAnalyzer());

        // Act
        var result = await injector.InjectAsync(erbDir, yamlDir);

        // Assert
        Assert.Equal(2, result.Injected);
        Assert.Contains("com_id: 10", File.ReadAllText(yaml0Path, Encoding.UTF8));
        Assert.Contains("com_id: 20", File.ReadAllText(yaml1Path, Encoding.UTF8));
    }

    #endregion

    #region InjectComIdIntoFile (via InjectAsync integration)

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InjectAsync_YamlMissingSituationLine_ReportsError()
    {
        // Arrange - YAML without "situation:" line → InjectComIdIntoFileAsync adds to Errors
        var erbDir = Path.Combine(_tempDir, "erbs_nosit");
        var charErbDir = Path.Combine(erbDir, "1_美鈴");
        var yamlDir = Path.Combine(_tempDir, "yamls_nosit");
        var charYamlDir = Path.Combine(yamlDir, "1_美鈴");
        Directory.CreateDirectory(charErbDir);
        Directory.CreateDirectory(charYamlDir);

        var erbContent = BuildErbWithComFunction("KOJO_MESSAGE_COM_K1_99");
        File.WriteAllText(Path.Combine(charErbDir, "KOJO_K1_愛撫.ERB"), erbContent, Encoding.UTF8);

        // YAML without situation line
        var yamlPath = Path.Combine(charYamlDir, "K1_愛撫_0.yaml");
        File.WriteAllText(yamlPath,
            "character: 美鈴\nentries:\n  - lines:\n    - text: hello\n",
            Encoding.UTF8);

        var injector = new ComIdInjector(new PathAnalyzer());

        // Act
        var result = await injector.InjectAsync(erbDir, yamlDir);

        // Assert
        Assert.Equal(0, result.Injected);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("situation"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InjectAsync_ComIdInjected_AfterSituationLine()
    {
        // Arrange
        var erbDir = Path.Combine(_tempDir, "erbs_afterline");
        var charErbDir = Path.Combine(erbDir, "1_美鈴");
        var yamlDir = Path.Combine(_tempDir, "yamls_afterline");
        var charYamlDir = Path.Combine(yamlDir, "1_美鈴");
        Directory.CreateDirectory(charErbDir);
        Directory.CreateDirectory(charYamlDir);

        var erbContent = BuildErbWithComFunction("KOJO_MESSAGE_COM_K1_77");
        File.WriteAllText(Path.Combine(charErbDir, "KOJO_K1_愛撫.ERB"), erbContent, Encoding.UTF8);

        var yamlPath = Path.Combine(charYamlDir, "K1_愛撫_0.yaml");
        File.WriteAllText(yamlPath, BuildYamlWithoutComId("K1_愛撫"), Encoding.UTF8);

        var injector = new ComIdInjector(new PathAnalyzer());

        // Act
        await injector.InjectAsync(erbDir, yamlDir);

        // Assert: com_id appears immediately after situation line
        var content = File.ReadAllText(yamlPath, Encoding.UTF8);
        var lines = content.Split('\n');
        int situationIdx = Array.FindIndex(lines, l => l.StartsWith("situation:"));
        Assert.True(situationIdx >= 0, "situation line not found");
        Assert.True(situationIdx + 1 < lines.Length, "no line after situation");
        Assert.Contains("com_id: 77", lines[situationIdx + 1]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InjectAsync_SingleNonIndexedYamlFile_InjectsComId()
    {
        // Arrange - single file variant: K4_乳首責め.yaml (no _0 suffix)
        var erbDir = Path.Combine(_tempDir, "erbs_single");
        var charErbDir = Path.Combine(erbDir, "4_フラン");
        var yamlDir = Path.Combine(_tempDir, "yamls_single");
        var charYamlDir = Path.Combine(yamlDir, "4_フラン");
        Directory.CreateDirectory(charErbDir);
        Directory.CreateDirectory(charYamlDir);

        var erbContent = BuildErbWithComFunction("KOJO_MESSAGE_COM_K4_55");
        File.WriteAllText(Path.Combine(charErbDir, "KOJO_K4_乳首責め.ERB"), erbContent, Encoding.UTF8);

        var yamlPath = Path.Combine(charYamlDir, "K4_乳首責め.yaml");
        File.WriteAllText(yamlPath, BuildYamlWithoutComId("K4_乳首責め"), Encoding.UTF8);

        var injector = new ComIdInjector(new PathAnalyzer());

        // Act
        var result = await injector.InjectAsync(erbDir, yamlDir);

        // Assert
        Assert.Equal(1, result.Injected);
        Assert.Contains("com_id: 55", File.ReadAllText(yamlPath, Encoding.UTF8));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Builds minimal ERB content with a single function containing a PRINTDATA node.
    /// </summary>
    private static string BuildErbWithComFunction(string functionName)
    {
        return $"@{functionName}\n" +
               "PRINTDATA\n" +
               "DATALIST\n" +
               "DATAFORM テストセリフ\n" +
               "ENDLIST\n" +
               "ENDDATA\n";
    }

    /// <summary>
    /// Builds ERB content with two separate functions, each with a PRINTDATA node.
    /// </summary>
    private static string BuildErbWithTwoComFunctions(string functionName1, string functionName2)
    {
        return $"@{functionName1}\n" +
               "PRINTDATA\n" +
               "DATALIST\n" +
               "DATAFORM セリフA\n" +
               "ENDLIST\n" +
               "ENDDATA\n" +
               "\n" +
               $"@{functionName2}\n" +
               "PRINTDATA\n" +
               "DATALIST\n" +
               "DATAFORM セリフB\n" +
               "ENDLIST\n" +
               "ENDDATA\n";
    }

    /// <summary>
    /// Builds a minimal YAML file without com_id.
    /// </summary>
    private static string BuildYamlWithoutComId(string situation)
    {
        return $"character: テスト\n" +
               $"situation: {situation}\n" +
               "entries:\n" +
               "  - lines:\n" +
               "    - text: テストセリフ\n";
    }

    /// <summary>
    /// Builds a minimal YAML file that already has com_id.
    /// </summary>
    private static string BuildYamlWithComId(string situation, int comId)
    {
        return $"character: テスト\n" +
               $"situation: {situation}\n" +
               $"com_id: {comId}\n" +
               "entries:\n" +
               "  - lines:\n" +
               "    - text: テストセリフ\n";
    }

    #endregion
}
