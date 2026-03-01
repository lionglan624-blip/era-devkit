using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ErbToYaml;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ErbToYaml.Tests;

/// <summary>
/// TDD tests for Feature 748: ErbToYaml Intro Line Extraction
/// Tests AC#6, AC#7, AC#8
/// Verifies PRINTFORM[WL] extraction before PRINTDATA blocks
/// </summary>
public class IntroLineExtractionTests : IDisposable
{
    private readonly string _testOutputDir;
    private readonly string _talentCsvPath;
    private readonly string _schemaPath;
    private readonly IDeserializer _yamlDeserializer;

    public IntroLineExtractionTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"ErbToYamlTest_IntroLine_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testOutputDir);

        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");

        _schemaPath = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "YamlSchemaGen", "dialogue-schema.json");

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir))
            Directory.Delete(_testOutputDir, recursive: true);
    }

    /// <summary>
    /// AC#6: Test single PRINTFORM intro line extraction
    /// Expected: PRINTFORM text appears as first line in YAML lines[] array
    /// </summary>
    [Fact]
    public async Task Test_IntroLineExtraction_SinglePrintform()
    {
        // Arrange - Create ERB with IF → PRINTFORM → PRINTDATA pattern
        var charDir = Path.Combine(_testOutputDir, "1_美鈴");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K1_IntroTest.ERB");

        var erbContent = @"@TEST_FUNCTION
IF TALENT:恋人
    PRINTFORM 最近一緒にいると、お互い無言でも落ち着く。
    PRINTDATA
    DATALIST
    DATAFORM 「話を聞いていた」
    DATAFORM 「うんうん」
    ENDLIST
    ENDDATA
ENDIF
";
        File.WriteAllText(erbPath, erbContent);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.True(results[0].Success, $"Expected success but got error: {results[0].Error}");

        var yamlPath = Path.Combine(charDir, "K1_IntroTest.yaml");
        Assert.True(File.Exists(yamlPath), $"YAML file should exist at: {yamlPath}");

        var yamlContent = File.ReadAllText(yamlPath);
        var yamlObject = _yamlDeserializer.Deserialize<dynamic>(yamlContent);

        // Verify intro line is in YAML
        Assert.Contains("最近一緒にいると", yamlContent);

        // Verify intro line appears before dialogue lines (order preserved)
        var introIndex = yamlContent.IndexOf("最近一緒にいると");
        var dialogueIndex = yamlContent.IndexOf("話を聞いていた");
        Assert.True(introIndex < dialogueIndex, "Intro line should appear before dialogue lines in YAML");
    }

    /// <summary>
    /// AC#7: Test PRINTFORM + PRINTFORMW continuation pattern
    /// Expected: PRINTFORMW concatenates with previous line without space/newline
    /// </summary>
    [Fact]
    public async Task Test_IntroLineExtraction_PrintformwContinuation()
    {
        // Arrange - Create ERB with PRINTFORM + PRINTFORMW continuation
        var charDir = Path.Combine(_testOutputDir, "1_美鈴");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K1_Continuation.ERB");

        var erbContent = @"@TEST_FUNCTION
IF TALENT:恋人
    PRINTFORM %CALLNAME:TARGET%は%CALLNAME:MASTER%の言葉に
    PRINTFORMW 、眩しい笑顔を見せた。
    PRINTDATA
    DATALIST
    DATAFORM 「――へへ、話は聞いてて飽きないぜ」
    ENDLIST
    ENDDATA
ENDIF
";
        File.WriteAllText(erbPath, erbContent);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.True(results[0].Success, $"Expected success but got error: {results[0].Error}");

        var yamlPath = Path.Combine(charDir, "K1_Continuation.yaml");
        Assert.True(File.Exists(yamlPath));

        var yamlContent = File.ReadAllText(yamlPath);

        // Verify PRINTFORM and PRINTFORMW are concatenated (no newline separator)
        Assert.Contains("%CALLNAME:TARGET%は%CALLNAME:MASTER%の言葉に、眩しい笑顔を見せた。", yamlContent);

        // Verify NOT split into separate lines
        var yamlLines = yamlContent.Split('\n');
        var introLineCount = yamlLines.Count(line => line.Contains("%CALLNAME:TARGET%は%CALLNAME:MASTER%の言葉に"));
        Assert.Equal(1, introLineCount); // Should be single concatenated line, not multiple lines
    }

    /// <summary>
    /// AC#8: Test ERB expression preservation in intro lines
    /// Expected: Expressions like %CALLNAME:TARGET% preserved verbatim, not evaluated
    /// </summary>
    [Fact]
    public async Task Test_IntroLineExtraction_ExpressionPreservation()
    {
        // Arrange - Create ERB with expressions in PRINTFORM
        var charDir = Path.Combine(_testOutputDir, "1_美鈴");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K1_Expression.ERB");

        var erbContent = @"@TEST_FUNCTION
IF TALENT:恋人
    PRINTFORM %CALLNAME:TARGET%は微笑んだ。
    PRINTDATA
    DATALIST
    DATAFORM 「%CALLNAME:MASTER%、今日は楽しかったな」
    ENDLIST
    ENDDATA
ENDIF
";
        File.WriteAllText(erbPath, erbContent);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.True(results[0].Success, $"Expected success but got error: {results[0].Error}");

        var yamlPath = Path.Combine(charDir, "K1_Expression.yaml");
        Assert.True(File.Exists(yamlPath));

        var yamlContent = File.ReadAllText(yamlPath);

        // Verify expressions are preserved literally (not evaluated/replaced)
        Assert.Contains("%CALLNAME:TARGET%は微笑んだ。", yamlContent);
        Assert.Contains("%CALLNAME:MASTER%", yamlContent);

        // Verify expressions not evaluated to empty string or placeholder
        Assert.DoesNotContain("は微笑んだ。", yamlContent.Replace("%CALLNAME:TARGET%は微笑んだ。", ""));
    }

    /// <summary>
    /// Additional test: Verify multiple consecutive PRINTFORMW continuations
    /// Edge case: PRINTFORM + PRINTFORMW + PRINTFORMW (3-part concatenation)
    /// </summary>
    [Fact]
    public async Task Test_IntroLineExtraction_MultipleContinuations()
    {
        // Arrange
        var charDir = Path.Combine(_testOutputDir, "1_美鈴");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K1_MultiContinue.ERB");

        var erbContent = @"@TEST_FUNCTION
IF TALENT:恋人
    PRINTFORM パート1
    PRINTFORMW パート2
    PRINTFORMW パート3
    PRINTDATA
    DATALIST
    DATAFORM 「セリフ」
    ENDLIST
    ENDDATA
ENDIF
";
        File.WriteAllText(erbPath, erbContent);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.True(results[0].Success);

        var yamlPath = Path.Combine(charDir, "K1_MultiContinue.yaml");
        var yamlContent = File.ReadAllText(yamlPath);

        // Verify all three parts concatenated into single line
        Assert.Contains("パート1パート2パート3", yamlContent);
    }

}
