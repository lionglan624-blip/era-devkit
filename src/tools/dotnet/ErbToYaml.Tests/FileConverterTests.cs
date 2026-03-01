using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ErbParser;
using ErbParser.Ast;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// TDD tests for Feature 634: Batch Conversion Tool - FileConverter component
/// RED state tests - implementation does not exist yet
/// Tests AC#4, AC#15, AC#16
/// </summary>
public class FileConverterTests : IDisposable
{
    private readonly string _testOutputDir;
    private readonly string _talentCsvPath;
    private readonly string _schemaPath;

    public FileConverterTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"ErbToYamlTest_Output_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testOutputDir);

        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");

        _schemaPath = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "YamlSchemaGen", "dialogue-schema.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir))
            Directory.Delete(_testOutputDir, recursive: true);
    }

    /// <summary>
    /// AC#4: Test single-file conversion orchestration
    /// Expected: FileConverter reads ERB file, parses, converts DATALIST to YAML, writes output
    /// </summary>
    [Fact]
    public async Task Test_SingleFileConversion_ParseConvertWrite()
    {
        // Arrange - Create temporary ERB file in numbered directory (PathAnalyzer requires this pattern)
        var charDir = Path.Combine(_testOutputDir, "1_TestChar");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K1_Test.ERB");
        File.WriteAllText(erbPath, CreateSimpleErbWithDatalist());

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.NotEmpty(results);
        Assert.True(results[0].Success, $"Expected success but got error: {results[0].Error}");
        Assert.Equal(erbPath, results[0].FilePath);
        Assert.Null(results[0].Error);

        // Verify YAML file was created
        var expectedYamlPath = Path.Combine(charDir, "K1_Test.yaml");
        Assert.True(File.Exists(expectedYamlPath), $"YAML file should exist at: {expectedYamlPath}");
    }

    /// <summary>
    /// AC#4: Test character/situation extraction from file path
    /// Expected: FileConverter uses PathAnalyzer to extract character=美鈴, situation=K1_愛撫
    /// from path pattern 1_美鈴/KOJO_K1_愛撫.ERB
    /// </summary>
    [Fact]
    public async Task Test_CharacterSituationExtraction_FromPath()
    {
        // Arrange - Create directory structure matching expected pattern
        var charDir = Path.Combine(_testOutputDir, "1_美鈴");
        Directory.CreateDirectory(charDir);

        var erbPath = Path.Combine(charDir, "KOJO_K1_愛撫.ERB");
        File.WriteAllText(erbPath, CreateSimpleErbWithDatalist());

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.True(results[0].Success);

        // Verify YAML file contains correct character and situation
        var yamlPath = Path.Combine(charDir, "K1_愛撫.yaml");
        Assert.True(File.Exists(yamlPath));

        var yamlContent = File.ReadAllText(yamlPath);
        Assert.Contains("character: 美鈴", yamlContent);
        Assert.Contains("situation: K1_愛撫", yamlContent);
    }

    /// <summary>
    /// AC#4: Test multiple DATALIST/PRINTDATA blocks generate indexed YAML files
    /// Expected: ERB file with 3 convertible blocks generates 3 YAML files with suffixes _0, _1, _2
    /// </summary>
    [Fact]
    public async Task Test_MultipleBlocks_GenerateIndexedFiles()
    {
        // Arrange - Create ERB with multiple PRINTDATA blocks
        var erbPath = Path.Combine(_testOutputDir, "1_美鈴", "KOJO_K1_Multi.ERB");
        Directory.CreateDirectory(Path.GetDirectoryName(erbPath)!);

        var erbContent = @"@TEST_FUNCTION
PRINTDATA
DATALIST
DATAFORM Line 1
ENDLIST
ENDDATA

PRINTDATA
DATALIST
DATAFORM Line 2
ENDLIST
ENDDATA

PRINTDATA
DATALIST
DATAFORM Line 3
ENDLIST
ENDDATA
";
        File.WriteAllText(erbPath, erbContent);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, Path.GetDirectoryName(erbPath)!);

        // Assert - Should generate 3 results
        Assert.Equal(3, results.Count);
        Assert.True(results.All(r => r.Success));

        // Verify indexed YAML files exist
        var outputDir = Path.GetDirectoryName(erbPath)!;
        Assert.True(File.Exists(Path.Combine(outputDir, "K1_Multi_0.yaml")));
        Assert.True(File.Exists(Path.Combine(outputDir, "K1_Multi_1.yaml")));
        Assert.True(File.Exists(Path.Combine(outputDir, "K1_Multi_2.yaml")));
    }

    /// <summary>
    /// AC#15: Test conditional structure preservation for IF-wrapped PRINTDATA
    /// Expected: IF/ELSEIF/ELSE wrapping PRINTDATA blocks are preserved as conditional branches in YAML
    /// </summary>
    [Fact]
    public async Task Test_ConditionalPreservation_IfWrappedPrintData()
    {
        // Arrange - Create ERB with IF wrapping PRINTDATA
        var erbPath = Path.Combine(_testOutputDir, "1_美鈴", "KOJO_K1_Conditional.ERB");
        Directory.CreateDirectory(Path.GetDirectoryName(erbPath)!);

        var erbContent = @"@TEST_FUNCTION
IF TALENT:恋人
    PRINTDATA
    DATALIST
    DATAFORM 恋人セリフ
    ENDLIST
    ENDDATA
ELSEIF TALENT:恋慕
    PRINTDATA
    DATALIST
    DATAFORM 恋慕セリフ
    ENDLIST
    ENDDATA
ELSE
    PRINTDATA
    DATALIST
    DATAFORM 通常セリフ
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
        var results = await fileConverter.ConvertAsync(erbPath, Path.GetDirectoryName(erbPath)!);

        // Assert
        Assert.Single(results); // Single YAML with multiple conditional branches
        Assert.True(results[0].Success);

        // Verify YAML has conditional structure
        var yamlPath = Path.Combine(Path.GetDirectoryName(erbPath)!, "K1_Conditional.yaml");
        Assert.True(File.Exists(yamlPath));

        var yamlContent = File.ReadAllText(yamlPath);
        // Should contain multiple branches with conditions
        Assert.Contains("entries:", yamlContent);
        Assert.Contains("condition:", yamlContent);
        Assert.Contains("type: Talent", yamlContent);
        // Should contain all three dialogue variants
        Assert.Contains("恋人セリフ", yamlContent);
        Assert.Contains("恋慕セリフ", yamlContent);
        Assert.Contains("通常セリフ", yamlContent);
    }

    /// <summary>
    /// AC#15: Test PRINTDATA without conditionals (simple case)
    /// Expected: Simple PRINTDATA generates single-branch YAML (no conditions)
    /// </summary>
    [Fact]
    public async Task Test_ConditionalPreservation_SimplePrintData()
    {
        // Arrange - Create ERB with simple PRINTDATA (no IF wrapper)
        var erbPath = Path.Combine(_testOutputDir, "1_美鈴", "KOJO_K1_Simple.ERB");
        Directory.CreateDirectory(Path.GetDirectoryName(erbPath)!);

        var erbContent = @"@TEST_FUNCTION
PRINTDATA
DATALIST
DATAFORM シンプルセリフ
ENDLIST
ENDDATA
";
        File.WriteAllText(erbPath, erbContent);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, Path.GetDirectoryName(erbPath)!);

        // Assert
        Assert.Single(results);
        Assert.True(results[0].Success);

        var yamlPath = Path.Combine(Path.GetDirectoryName(erbPath)!, "K1_Simple.yaml");
        var yamlContent = File.ReadAllText(yamlPath);

        // Should have branches but no condition field
        Assert.Contains("entries:", yamlContent);
        Assert.DoesNotContain("condition:", yamlContent);
        Assert.Contains("シンプルセリフ", yamlContent);
    }

    /// <summary>
    /// AC#16: Test schema validation for generated YAML
    /// Expected: Valid ERB generates schema-compliant YAML (no validation exception)
    /// </summary>
    [Fact]
    public async Task Test_SchemaValidation_ValidYamlPasses()
    {
        // Arrange - Create valid ERB
        var erbPath = Path.Combine(_testOutputDir, "1_美鈴", "KOJO_K1_Valid.ERB");
        Directory.CreateDirectory(Path.GetDirectoryName(erbPath)!);
        File.WriteAllText(erbPath, CreateSimpleErbWithDatalist());

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act & Assert - Should not throw SchemaValidationException
        var exception = await Record.ExceptionAsync(async () =>
        {
            await fileConverter.ConvertAsync(erbPath, Path.GetDirectoryName(erbPath)!);
        });

        Assert.Null(exception);
    }

    /// <summary>
    /// AC#16: Test schema validation rejects invalid YAML structure
    /// Expected: ERB that would generate invalid YAML (missing required fields) causes conversion failure
    /// </summary>
    [Fact]
    public async Task Test_SchemaValidation_InvalidYamlFails()
    {
        // Arrange - Create ERB that generates invalid YAML (edge case: empty DATALIST)
        var erbPath = Path.Combine(_testOutputDir, "1_美鈴", "KOJO_K1_Invalid.ERB");
        Directory.CreateDirectory(Path.GetDirectoryName(erbPath)!);

        // ERB with empty PRINTDATA (no DATALIST) - should fail validation
        var erbContent = @"@TEST_FUNCTION
PRINTDATA
; Empty - no content
ENDDATA
";
        File.WriteAllText(erbPath, erbContent);

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, Path.GetDirectoryName(erbPath)!);

        // Assert - Conversion should fail due to schema validation
        Assert.Single(results);
        Assert.False(results[0].Success);
        Assert.NotNull(results[0].Error);
    }

    /// <summary>
    /// F639 AC#10: Test FileConverter output filename for NTR prefix file
    /// Expected: NTR口上_シナリオ8.ERB → NTR口上_シナリオ8.yaml output filename
    /// </summary>
    [Fact]
    public async Task Test_OutputFilename_NtrPrefix()
    {
        // Arrange - Create directory and ERB file with NTR prefix
        var charDir = Path.Combine(_testOutputDir, "4_咲夜");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "NTR口上_シナリオ8.ERB");
        File.WriteAllText(erbPath, CreateSimpleErbWithDatalist());

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.True(results[0].Success, $"Expected success but got error: {results[0].Error}");
        var expectedYamlPath = Path.Combine(charDir, "NTR口上_シナリオ8.yaml");
        Assert.True(File.Exists(expectedYamlPath), $"YAML file should exist at: {expectedYamlPath}");
    }

    /// <summary>
    /// F639 AC#10: Test FileConverter output filename for SexHara prefix file
    /// Expected: SexHara休憩中口上.ERB → SexHara休憩中口上.yaml output filename
    /// </summary>
    [Fact]
    public async Task Test_OutputFilename_SexHaraPrefix()
    {
        // Arrange
        var charDir = Path.Combine(_testOutputDir, "4_咲夜");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "SexHara休憩中口上.ERB");
        File.WriteAllText(erbPath, CreateSimpleErbWithDatalist());

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.True(results[0].Success, $"Expected success but got error: {results[0].Error}");
        var expectedYamlPath = Path.Combine(charDir, "SexHara休憩中口上.yaml");
        Assert.True(File.Exists(expectedYamlPath), $"YAML file should exist at: {expectedYamlPath}");
    }

    /// <summary>
    /// F639 AC#10: Test FileConverter output filename for WC prefix file
    /// Expected: WC系口上.ERB → WC系口上.yaml output filename
    /// </summary>
    [Fact]
    public async Task Test_OutputFilename_WcPrefix()
    {
        // Arrange
        var charDir = Path.Combine(_testOutputDir, "4_咲夜");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "WC系口上.ERB");
        File.WriteAllText(erbPath, CreateSimpleErbWithDatalist());

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.True(results[0].Success, $"Expected success but got error: {results[0].Error}");
        var expectedYamlPath = Path.Combine(charDir, "WC系口上.yaml");
        Assert.True(File.Exists(expectedYamlPath), $"YAML file should exist at: {expectedYamlPath}");
    }

    /// <summary>
    /// F639 AC#10: Test FileConverter KOJO prefix regression (backward compatible)
    /// Expected: KOJO_K4_愛撫.ERB → K4_愛撫.yaml (unchanged from current behavior)
    /// </summary>
    [Fact]
    public async Task Test_OutputFilename_KojoPrefix_Regression()
    {
        // Arrange
        var charDir = Path.Combine(_testOutputDir, "4_咲夜");
        Directory.CreateDirectory(charDir);
        var erbPath = Path.Combine(charDir, "KOJO_K4_愛撫.ERB");
        File.WriteAllText(erbPath, CreateSimpleErbWithDatalist());

        var pathAnalyzer = new PathAnalyzer();
        var talentLoader = new TalentCsvLoader(_talentCsvPath);
        var datalistConverter = new DatalistConverter(_talentCsvPath, _schemaPath);
        var printDataConverter = new PrintDataConverter();
        var fileConverter = new FileConverter(pathAnalyzer, printDataConverter, datalistConverter, talentLoader);

        // Act
        var results = await fileConverter.ConvertAsync(erbPath, charDir);

        // Assert
        Assert.True(results[0].Success, $"Expected success but got error: {results[0].Error}");
        var expectedYamlPath = Path.Combine(charDir, "K4_愛撫.yaml");
        Assert.True(File.Exists(expectedYamlPath), $"YAML file should exist at: {expectedYamlPath}");
    }

    #region Helper Methods

    /// <summary>
    /// Create simple ERB content with DATALIST for testing
    /// </summary>
    private string CreateSimpleErbWithDatalist()
    {
        return @"@TEST_FUNCTION
PRINTDATA
DATALIST
DATAFORM テストセリフ1
DATAFORM テストセリフ2
ENDLIST
ENDDATA
";
    }

    #endregion
}
