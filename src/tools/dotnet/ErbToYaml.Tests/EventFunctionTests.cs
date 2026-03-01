using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ErbParser;
using ErbParser.Ast;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// Tests for EVENT function processing path (Feature 764 - AC#5, AC#6, AC#8, AC#11)
/// </summary>
public class EventFunctionTests : IDisposable
{
    private readonly string _testOutputDir;
    private readonly string _talentCsvPath;
    private readonly string _schemaPath;

    public EventFunctionTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"EventFunctionTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testOutputDir);

        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");

        _schemaPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "YamlSchemaGen", "dialogue-schema.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir))
            Directory.Delete(_testOutputDir, recursive: true);
    }

    /// <summary>
    /// AC#5: FileConverter processes FunctionDefNode with IF ARG/PRINTFORM/RETURN → produces YAML
    /// Expected: Conversion result with success=true and YAML output
    /// </summary>
    [Fact]
    public async Task EventFunction_IfArgPrintFormReturn_ProducesYaml()
    {
        // Arrange - Create ERB file with EVENT function
        var charDir = Path.Combine(_testOutputDir, "1_TestChar");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K1_EVENT.ERB");

        var content = @"@KOJO_EVENT_K1_0(ARG,ARG:1)
LOCAL = 1
IF LOCAL
    IF ARG == 2
        PRINTFORML 「Test dialogue for ARG==2」
        RETURN 0
    ENDIF
ENDIF
RETURN 0";

        File.WriteAllText(erbPath, content);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var localGateResolver = new LocalGateResolver();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader, null, localGateResolver);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.NotEmpty(results);
        var eventResult = results.FirstOrDefault(r => r.Success);
        Assert.NotNull(eventResult);
        Assert.True(eventResult.Success, $"Expected success but got error: {eventResult.Error}");
        Assert.Null(eventResult.Error);

        // Verify YAML file was created (construct expected path)
        var expectedYamlPath = Path.Combine(charDir, "K1_EVENT_K1_0.yaml");
        Assert.True(File.Exists(expectedYamlPath), $"YAML file should exist at: {expectedYamlPath}");
    }

    /// <summary>
    /// AC#5: RETURN branch-termination test
    /// Expected: Only pre-RETURN content appears in output
    /// </summary>
    [Fact]
    public async Task EventFunction_ReturnTerminatesBranch_OnlyPreReturnContent()
    {
        // Arrange
        var charDir = Path.Combine(_testOutputDir, "1_TestChar");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K1_EVENT.ERB");

        var content = @"@KOJO_EVENT_K1_TEST(ARG)
LOCAL = 1
IF LOCAL
    IF ARG == 0
        PRINTFORML 「First line before RETURN」
        PRINTFORML 「Second line before RETURN」
        RETURN 0
        PRINTFORML 「This should NOT appear」
    ENDIF
ENDIF
RETURN 0";

        File.WriteAllText(erbPath, content);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var localGateResolver = new LocalGateResolver();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader, null, localGateResolver);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.NotEmpty(results);
        var eventResult = results.FirstOrDefault(r => r.Success);
        Assert.NotNull(eventResult);

        // Read YAML output and verify content (construct expected path)
        var yamlPath = Path.Combine(charDir, "K1_EVENT_K1_TEST.yaml");
        var yamlContent = File.ReadAllText(yamlPath);
        Assert.Contains("First line before RETURN", yamlContent);
        Assert.Contains("Second line before RETURN", yamlContent);
        Assert.DoesNotContain("This should NOT appear", yamlContent);
    }

    /// <summary>
    /// AC#6: SELECTCASE-only function produces no output (Neg)
    /// Expected: No YAML output produced; no error thrown
    /// </summary>
    [Fact]
    public async Task EventFunction_SelectCaseOnly_ProducesNoOutput()
    {
        // Arrange - Create ERB file with SELECTCASE-only function (like K1_1)
        var charDir = Path.Combine(_testOutputDir, "1_TestChar");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K1_EVENT.ERB");

        var content = @"@KOJO_EVENT_K1_1(ARG,ARG:1)
LOCAL = 1
IF LOCAL
    SELECTCASE RAND:3
    CASE 0
        PRINTFORMW 「Random text 1」
    CASE 1
        PRINTFORMW 「Random text 2」
    CASE 2
        PRINTFORMW 「Random text 3」
    ENDSELECT
ENDIF
RETURN 0";

        File.WriteAllText(erbPath, content);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var localGateResolver = new LocalGateResolver();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader, null, localGateResolver);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert - Should complete without error, but no EVENT YAML produced
        // Verify no YAML file exists for K1_1 (SELECTCASE-only)
        var k1_1_yamlPath = Path.Combine(charDir, "K1_EVENT_K1_1.yaml");
        Assert.False(File.Exists(k1_1_yamlPath), "SELECTCASE-only function should not produce YAML output");
    }

    /// <summary>
    /// AC#6: Mixed function with SELECTCASE inside IF ARG branch - that branch skipped
    /// Expected: Branches with SELECTCASE are skipped, branches without SELECTCASE converted
    /// </summary>
    [Fact]
    public async Task EventFunction_MixedSelectCaseBranches_OnlyPureIfArgConverted()
    {
        // Arrange - Simulate K1_0 structure: ARG==0 has SELECTCASE, ARG==2 is pure PRINTFORM
        var charDir = Path.Combine(_testOutputDir, "1_TestChar");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K1_EVENT.ERB");

        var content = @"@KOJO_EVENT_K1_0(ARG,ARG:1)
LOCAL = 1
IF LOCAL
    IF ARG == 0
        SELECTCASE RAND:3
        CASE 0
            PRINTFORML 「Random」
        ENDSELECT
        RETURN 0
    ENDIF
    IF ARG == 2
        PRINTFORML 「Pure PRINTFORM branch」
        RETURN 0
    ENDIF
ENDIF
RETURN 0";

        File.WriteAllText(erbPath, content);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var localGateResolver = new LocalGateResolver();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader, null, localGateResolver);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.NotEmpty(results);
        var eventResult = results.FirstOrDefault(r => r.Success);
        Assert.NotNull(eventResult);

        // Read YAML and verify only ARG==2 branch appears
        var yamlPath = Path.Combine(charDir, "K1_EVENT_K1_0.yaml");
        var yamlContent = File.ReadAllText(yamlPath);
        Assert.Contains("Pure PRINTFORM branch", yamlContent);
        Assert.DoesNotContain("Random", yamlContent); // SELECTCASE branch should be skipped
    }

    /// <summary>
    /// AC#8: Multi-function input produces distinct filenames per function
    /// Expected: Each converted function maps to a distinct output file path
    /// </summary>
    [Fact]
    public async Task EventFunction_MultipleFunctions_DistinctOutputFiles()
    {
        // Arrange - Create ERB with 2 convertible functions
        var charDir = Path.Combine(_testOutputDir, "1_TestChar");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K1_EVENT.ERB");

        var content = @"@KOJO_EVENT_K1_0(ARG,ARG:1)
LOCAL = 1
IF LOCAL
    IF ARG == 2
        PRINTFORML 「Function 0 dialogue」
        RETURN 0
    ENDIF
ENDIF
RETURN 0

@KOJO_EVENT_K1_7(ARG,ARG:1)
LOCAL = 1
IF LOCAL
    IF ARG == 0
        PRINTFORMW 「Function 7 dialogue」
        RETURN 0
    ENDIF
ENDIF
RETURN 0";

        File.WriteAllText(erbPath, content);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var localGateResolver = new LocalGateResolver();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader, null, localGateResolver);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        var successResults = results.Where(r => r.Success).ToList();
        Assert.True(successResults.Count >= 2, "Expected at least 2 successful conversions");

        // Verify distinct YAML files were created
        var yamlPath0 = Path.Combine(charDir, "K1_EVENT_K1_0.yaml");
        var yamlPath7 = Path.Combine(charDir, "K1_EVENT_K1_7.yaml");

        Assert.True(File.Exists(yamlPath0), $"K1_0 YAML should exist at: {yamlPath0}");
        Assert.True(File.Exists(yamlPath7), $"K1_7 YAML should exist at: {yamlPath7}");

        // Verify content is distinct
        var content0 = File.ReadAllText(yamlPath0);
        var content7 = File.ReadAllText(yamlPath7);
        Assert.NotEqual(content0, content7);
    }

    /// <summary>
    /// AC#11: displayMode mapping - PRINTFORML → "newline", PRINTFORMW → "wait"
    /// Expected: YAML entries contain correct displayMode based on PRINTFORM variant
    /// </summary>
    [Fact]
    public async Task EventFunction_DisplayModeMapping_CorrectMapping()
    {
        // Arrange
        var charDir = Path.Combine(_testOutputDir, "1_TestChar");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K1_EVENT.ERB");

        var content = @"@KOJO_EVENT_K1_TEST(ARG)
LOCAL = 1
IF LOCAL
    IF ARG == 0
        PRINTFORML 「This should have newline mode」
        RETURN 0
    ENDIF
    IF ARG == 1
        PRINTFORMW 「This should have wait mode」
        RETURN 0
    ENDIF
ENDIF
RETURN 0";

        File.WriteAllText(erbPath, content);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var localGateResolver = new LocalGateResolver();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader, null, localGateResolver);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.NotEmpty(results);
        var eventResult = results.FirstOrDefault(r => r.Success);
        Assert.NotNull(eventResult);

        // Read YAML and verify displayMode
        var yamlPath = Path.Combine(charDir, "K1_EVENT_K1_TEST.yaml");
        var yamlContent = File.ReadAllText(yamlPath);

        // PRINTFORML → displayMode: newline
        Assert.Contains("displayMode: newline", yamlContent);
        Assert.Contains("This should have newline mode", yamlContent);

        // PRINTFORMW → displayMode: wait
        Assert.Contains("displayMode: wait", yamlContent);
        Assert.Contains("This should have wait mode", yamlContent);
    }
}
