using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// Real K1_EVENT.ERB conversion tests (Feature 764 - AC#7, AC#9, AC#10)
/// Tests end-to-end processing of actual EVENT file
/// </summary>
public class K1EventRealTests : IDisposable
{
    private readonly string _testOutputDir;
    private readonly string _talentCsvPath;
    private readonly string _schemaPath;
    private readonly string _k1EventPath;

    public K1EventRealTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"K1EventRealTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testOutputDir);

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
        _schemaPath = Path.Combine(baseDir, "..", "..", "..", "..", "YamlSchemaGen", "dialogue-schema.json");
        _k1EventPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("ERB", "口上", "1_美鈴", "KOJO_K1_EVENT.ERB");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir))
            Directory.Delete(_testOutputDir, recursive: true);
    }

    /// <summary>
    /// AC#7: Real K1_EVENT.ERB EVENT conversion produces exactly 2 YAML (Pos)
    /// Expected: K1_0 (partial - ARG==2 only) and K1_7 produce YAML
    /// Note: DATALIST path may additionally convert PRINTDATAL in KOJO_MESSAGE (not counted)
    /// </summary>
    [Fact]
    public async Task K1EventReal_Conversion_ProducesExactlyTwoEventYaml()
    {
        // Arrange
        Assert.True(File.Exists(_k1EventPath), $"K1_EVENT.ERB should exist at: {_k1EventPath}");

        var charDir = Path.GetDirectoryName(_k1EventPath);
        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var localGateResolver = new LocalGateResolver();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader, null, localGateResolver);

        // Act
        var results = await fileConverter.ConvertAsync(_k1EventPath, charDir!);

        // Assert - Verify exactly 2 EVENT YAML files exist
        var k1_0_yamlPath = Path.Combine(charDir!, "K1_EVENT_K1_0.yaml");
        var k1_7_yamlPath = Path.Combine(charDir!, "K1_EVENT_K1_7.yaml");

        Assert.True(File.Exists(k1_0_yamlPath), $"K1_0 YAML should exist: {k1_0_yamlPath}");
        Assert.True(File.Exists(k1_7_yamlPath), $"K1_7 YAML should exist: {k1_7_yamlPath}");

        // Verify successful conversion results
        var successResults = results.Where(r => r.Success).ToList();
        Assert.True(successResults.Count >= 2, "Expected at least 2 successful conversions");
    }

    /// <summary>
    /// AC#9: K1_0 ARG==2 branch content in YAML
    /// Expected: Dialogue text 「あ、えっと…お邪魔しちゃったかしら…？」 with displayMode "newline"
    /// </summary>
    [Fact]
    public async Task K1EventReal_K1_0_Arg2Branch_HasExpectedContent()
    {
        // Arrange
        Assert.True(File.Exists(_k1EventPath), $"K1_EVENT.ERB should exist at: {_k1EventPath}");

        var charDir = Path.GetDirectoryName(_k1EventPath);
        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var localGateResolver = new LocalGateResolver();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader, null, localGateResolver);

        // Act
        var results = await fileConverter.ConvertAsync(_k1EventPath, charDir!);

        // Assert - Find K1_0 output
        var k1_0_yamlPath = Path.Combine(charDir!, "K1_EVENT_K1_0.yaml");

        Assert.True(File.Exists(k1_0_yamlPath), $"K1_0 YAML should exist: {k1_0_yamlPath}");

        // Read YAML content
        var yamlContent = File.ReadAllText(k1_0_yamlPath);

        // Verify ARG==2 branch dialogue text (from line 31)
        Assert.Contains("あ、えっと…お邪魔しちゃったかしら…？", yamlContent);

        // Verify displayMode is "newline" (PRINTFORML)
        Assert.Contains("displayMode: newline", yamlContent);

        // Negative: ARG==0 and ARG==1 SELECTCASE branches should NOT appear
        Assert.DoesNotContain("どうしたの、こんなところで", yamlContent); // ARG==0 SELECTCASE content
        Assert.DoesNotContain("失礼しますね。ちょっと休憩", yamlContent); // ARG==1 SELECTCASE content
    }

    /// <summary>
    /// AC#10: K1_7 ARG==0..5 branches content in YAML (exactly 6 entries, no CFLAG)
    /// Expected: 6 entries with PRINTFORMW → displayMode "wait"
    /// Negative: Unconditional fallback text (「おやすみなさい。また明日ね。」from line 241) NOT in output
    /// </summary>
    [Fact]
    public async Task K1EventReal_K1_7_ArgBranches_HasSixEntriesNoFallback()
    {
        // Arrange
        Assert.True(File.Exists(_k1EventPath), $"K1_EVENT.ERB should exist at: {_k1EventPath}");

        var charDir = Path.GetDirectoryName(_k1EventPath);
        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var localGateResolver = new LocalGateResolver();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader, null, localGateResolver);

        // Act
        var results = await fileConverter.ConvertAsync(_k1EventPath, charDir!);

        // Assert - Find K1_7 output
        var k1_7_yamlPath = Path.Combine(charDir!, "K1_EVENT_K1_7.yaml");

        Assert.True(File.Exists(k1_7_yamlPath), $"K1_7 YAML should exist: {k1_7_yamlPath}");

        // Read YAML content
        var yamlContent = File.ReadAllText(k1_7_yamlPath);

        // Verify exactly 6 entries for ARG 0-5
        // Count "- id:" or "condition:" occurrences as proxy for entry count
        var entryCount = System.Text.RegularExpressions.Regex.Count(yamlContent, @"^\s*- id:", System.Text.RegularExpressions.RegexOptions.Multiline);
        Assert.Equal(6, entryCount);

        // Verify displayMode is "wait" (PRINTFORMW) for all entries
        var waitModeCount = System.Text.RegularExpressions.Regex.Count(yamlContent, @"displayMode:\s*wait");
        Assert.Equal(6, waitModeCount);

        // Verify some expected dialogue content from ARG branches
        Assert.Contains("ごめんね、眠くなっちゃった", yamlContent); // ARG==0 (line 202)
        Assert.Contains("ごめんなさい、もう限界", yamlContent); // ARG==1 (line 209)
        Assert.Contains("すぅ…すぅ", yamlContent); // ARG==2 (line 216)
        Assert.Contains("ん…ありがと…おやすみ", yamlContent); // ARG==3 (line 223)
        Assert.Contains("ここで寝ていいの", yamlContent); // ARG==4 (line 230)
        Assert.Contains("寝ちゃったのね。しょうがない", yamlContent); // ARG==5 (line 237)

        // Negative: CFLAG:睡眠 early-exit guard (lines 195-197) should NOT produce entry
        Assert.DoesNotContain("CFLAG", yamlContent);

        // Negative: Unconditional fallback (line 241) should NOT appear
        Assert.DoesNotContain("おやすみなさい。また明日ね", yamlContent);
    }

    /// <summary>
    /// AC#7: Verify non-convertible functions produce no output
    /// Expected: K1_1..K1_4, K1_8, K1_10 (SELECTCASE-only) produce no YAML
    /// </summary>
    [Fact]
    public async Task K1EventReal_SelectCaseOnlyFunctions_ProduceNoOutput()
    {
        // Arrange
        Assert.True(File.Exists(_k1EventPath), $"K1_EVENT.ERB should exist at: {_k1EventPath}");

        var charDir = Path.GetDirectoryName(_k1EventPath);
        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var localGateResolver = new LocalGateResolver();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader, null, localGateResolver);

        // Act
        var results = await fileConverter.ConvertAsync(_k1EventPath, charDir!);

        // Assert - Verify SELECTCASE-only functions did NOT produce YAML
        Assert.False(File.Exists(Path.Combine(charDir!, "K1_EVENT_K1_1.yaml")), "K1_1 should not produce YAML");
        Assert.False(File.Exists(Path.Combine(charDir!, "K1_EVENT_K1_2.yaml")), "K1_2 should not produce YAML");
        Assert.False(File.Exists(Path.Combine(charDir!, "K1_EVENT_K1_3.yaml")), "K1_3 should not produce YAML");
        Assert.False(File.Exists(Path.Combine(charDir!, "K1_EVENT_K1_4.yaml")), "K1_4 should not produce YAML");
        Assert.False(File.Exists(Path.Combine(charDir!, "K1_EVENT_K1_8.yaml")), "K1_8 should not produce YAML");
        Assert.False(File.Exists(Path.Combine(charDir!, "K1_EVENT_K1_10.yaml")), "K1_10 should not produce YAML");
    }
}
