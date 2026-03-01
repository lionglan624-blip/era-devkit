using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// TDD tests for Feature 634: Batch Conversion Tool - BatchConverter component
/// RED state tests - implementation does not exist yet
/// Tests AC#3, AC#5, AC#6, AC#7
/// </summary>
public class BatchConverterTests : IDisposable
{
    private readonly string _testInputDir;
    private readonly string _testOutputDir;
    private readonly string _talentCsvPath;
    private readonly string _schemaPath;

    public BatchConverterTests()
    {
        // Create temporary test directories
        _testInputDir = Path.Combine(Path.GetTempPath(), $"ErbToYamlTest_Input_{Guid.NewGuid()}");
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"ErbToYamlTest_Output_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testInputDir);
        Directory.CreateDirectory(_testOutputDir);

        _talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");

        _schemaPath = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "YamlSchemaGen", "dialogue-schema.json");
    }

    public void Dispose()
    {
        // Clean up temporary directories
        if (Directory.Exists(_testInputDir))
            Directory.Delete(_testInputDir, recursive: true);
        if (Directory.Exists(_testOutputDir))
            Directory.Delete(_testOutputDir, recursive: true);
    }

    /// <summary>
    /// AC#3: Test recursive ERB file discovery
    /// Expected: BatchConverter finds all *.ERB files in nested directories
    /// </summary>
    [Fact]
    public async Task Test_RecursiveDiscovery_FindsAllErbFiles()
    {
        // Arrange - Create nested directory structure with ERB files
        var subdir1 = Path.Combine(_testInputDir, "1_Character1");
        var subdir2 = Path.Combine(_testInputDir, "2_Character2");
        Directory.CreateDirectory(subdir1);
        Directory.CreateDirectory(subdir2);

        // Create minimal valid ERB files
        var erb1 = Path.Combine(subdir1, "KOJO_K1_Test.ERB");
        var erb2 = Path.Combine(subdir2, "KOJO_K2_Test.ERB");
        var erb3 = Path.Combine(subdir2, "KOJO_K3_Test.ERB");

        File.WriteAllText(erb1, CreateMinimalErbContent());
        File.WriteAllText(erb2, CreateMinimalErbContent());
        File.WriteAllText(erb3, CreateMinimalErbContent());

        // Create FileConverter mock/stub
        var fileConverter = new TestFileConverter(_talentCsvPath, _schemaPath);
        var batchConverter = new BatchConverter(fileConverter);

        // Act
        var report = await batchConverter.ConvertAsync(_testInputDir, _testOutputDir);

        // Assert
        Assert.NotNull(report);
        Assert.Equal(3, report.Total); // Should find all 3 ERB files
    }

    /// <summary>
    /// AC#3: Test empty subdirectories don't cause errors
    /// Expected: No exception when subdirectory contains no ERB files
    /// </summary>
    [Fact]
    public async Task Test_RecursiveDiscovery_EmptySubdirectoriesNoError()
    {
        // Arrange - Create directory structure with empty subdirectory
        var subdir1 = Path.Combine(_testInputDir, "1_Character1");
        var subdir2 = Path.Combine(_testInputDir, "2_Character2_Empty");
        Directory.CreateDirectory(subdir1);
        Directory.CreateDirectory(subdir2);

        var erb1 = Path.Combine(subdir1, "KOJO_K1_Test.ERB");
        File.WriteAllText(erb1, CreateMinimalErbContent());

        var fileConverter = new TestFileConverter(_talentCsvPath, _schemaPath);
        var batchConverter = new BatchConverter(fileConverter);

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            await batchConverter.ConvertAsync(_testInputDir, _testOutputDir);
        });

        // Assert
        Assert.Null(exception); // No exception should be thrown
    }

    /// <summary>
    /// AC#5: Test directory structure preservation
    /// Expected: Output YAML files mirror input directory hierarchy
    /// </summary>
    [Fact]
    public async Task Test_DirectoryStructure_PreservesInputHierarchy()
    {
        // Arrange - Create nested input directory
        var subdir = Path.Combine(_testInputDir, "1_美鈴");
        Directory.CreateDirectory(subdir);

        var erbPath = Path.Combine(subdir, "KOJO_K1_愛撫.ERB");
        File.WriteAllText(erbPath, CreateMinimalErbContent());

        var fileConverter = new TestFileConverter(_talentCsvPath, _schemaPath);
        var batchConverter = new BatchConverter(fileConverter);

        // Act
        await batchConverter.ConvertAsync(_testInputDir, _testOutputDir);

        // Assert - Verify output directory structure mirrors input
        var expectedOutputSubdir = Path.Combine(_testOutputDir, "1_美鈴");
        Assert.True(Directory.Exists(expectedOutputSubdir),
            $"Output subdirectory should exist: {expectedOutputSubdir}");

        // Verify YAML file was created in correct location
        var expectedYamlPath = Path.Combine(expectedOutputSubdir, "KOJO_K1_愛撫.yaml");
        Assert.True(File.Exists(expectedYamlPath),
            $"YAML file should exist at: {expectedYamlPath}");
    }

    /// <summary>
    /// AC#6: Test batch summary report with correct counts
    /// Expected: BatchReport contains Total, Success, Failed counts (counted by ERB files)
    /// </summary>
    [Fact]
    public async Task Test_SummaryReport_CorrectCounts()
    {
        // Arrange - Create multiple ERB files
        var erb1 = Path.Combine(_testInputDir, "1_Char1", "KOJO_K1_Test.ERB");
        var erb2 = Path.Combine(_testInputDir, "2_Char2", "KOJO_K2_Test.ERB");
        Directory.CreateDirectory(Path.GetDirectoryName(erb1)!);
        Directory.CreateDirectory(Path.GetDirectoryName(erb2)!);

        File.WriteAllText(erb1, CreateMinimalErbContent());
        File.WriteAllText(erb2, CreateMinimalErbContent());

        var fileConverter = new TestFileConverter(_talentCsvPath, _schemaPath);
        var batchConverter = new BatchConverter(fileConverter);

        // Act
        var report = await batchConverter.ConvertAsync(_testInputDir, _testOutputDir);

        // Assert
        Assert.NotNull(report);
        Assert.Equal(2, report.Total);
        Assert.Equal(2, report.Success);
        Assert.Equal(0, report.Failed);
        Assert.Empty(report.Failures);
    }

    /// <summary>
    /// AC#7: Test continue-on-error behavior
    /// Expected: Failed file does not stop batch processing, all valid files processed
    /// </summary>
    [Fact]
    public async Task Test_ContinueOnError_ProcessesAllFiles()
    {
        // Arrange - Create mix of valid and malformed ERB files
        var validErb1 = Path.Combine(_testInputDir, "1_Char1", "KOJO_K1_Valid.ERB");
        var invalidErb = Path.Combine(_testInputDir, "2_Char2", "KOJO_K2_Invalid.ERB");
        var validErb2 = Path.Combine(_testInputDir, "3_Char3", "KOJO_K3_Valid.ERB");

        Directory.CreateDirectory(Path.GetDirectoryName(validErb1)!);
        Directory.CreateDirectory(Path.GetDirectoryName(invalidErb)!);
        Directory.CreateDirectory(Path.GetDirectoryName(validErb2)!);

        File.WriteAllText(validErb1, CreateMinimalErbContent());
        File.WriteAllText(invalidErb, "INVALID ERB SYNTAX ###");
        File.WriteAllText(validErb2, CreateMinimalErbContent());

        // Use a FileConverter that will throw on invalid ERB
        var fileConverter = new TestFileConverter(_talentCsvPath, _schemaPath, throwOnInvalid: true);
        var batchConverter = new BatchConverter(fileConverter);

        // Act
        var report = await batchConverter.ConvertAsync(_testInputDir, _testOutputDir);

        // Assert
        Assert.NotNull(report);
        Assert.Equal(3, report.Total); // All 3 files processed
        Assert.Equal(2, report.Success); // 2 valid files succeeded
        Assert.Equal(1, report.Failed); // 1 invalid file failed
        Assert.Single(report.Failures);

        // Verify the failed file is recorded
        var failure = report.Failures[0];
        Assert.False(failure.Success);
        Assert.Contains("Invalid", failure.FilePath);
        Assert.NotNull(failure.Error);
    }

    /// <summary>
    /// AC#7: Test BatchReport.Failures contains error details
    /// Expected: Failed files are recorded with FilePath and Error message
    /// </summary>
    [Fact]
    public async Task Test_BatchReport_ContainsFailureDetails()
    {
        // Arrange - Create one invalid ERB file
        var invalidErb = Path.Combine(_testInputDir, "1_Char", "KOJO_K1_Invalid.ERB");
        Directory.CreateDirectory(Path.GetDirectoryName(invalidErb)!);
        File.WriteAllText(invalidErb, "MALFORMED");

        var fileConverter = new TestFileConverter(_talentCsvPath, _schemaPath, throwOnInvalid: true);
        var batchConverter = new BatchConverter(fileConverter);

        // Act
        var report = await batchConverter.ConvertAsync(_testInputDir, _testOutputDir);

        // Assert
        Assert.Single(report.Failures);
        var failure = report.Failures[0];
        Assert.False(failure.Success);
        Assert.Contains("KOJO_K1_Invalid.ERB", failure.FilePath);
        Assert.NotEmpty(failure.Error!);
    }

    /// <summary>
    /// AC#1: Test parallel conversion produces correct results
    /// Expected: All 3 YAML files created successfully in parallel mode
    /// Feature 635 - RED test (BatchOptions does not exist yet)
    /// </summary>
    [Fact]
    public async Task Test_Parallel_CorrectResults()
    {
        // Arrange - Create 3 valid ERB files
        var erb1 = Path.Combine(_testInputDir, "1_Char1", "KOJO_K1_Test.ERB");
        var erb2 = Path.Combine(_testInputDir, "2_Char2", "KOJO_K2_Test.ERB");
        var erb3 = Path.Combine(_testInputDir, "3_Char3", "KOJO_K3_Test.ERB");

        Directory.CreateDirectory(Path.GetDirectoryName(erb1)!);
        Directory.CreateDirectory(Path.GetDirectoryName(erb2)!);
        Directory.CreateDirectory(Path.GetDirectoryName(erb3)!);

        File.WriteAllText(erb1, CreateMinimalErbContent());
        File.WriteAllText(erb2, CreateMinimalErbContent());
        File.WriteAllText(erb3, CreateMinimalErbContent());

        var fileConverter = new TestFileConverter(_talentCsvPath, _schemaPath);
        var batchConverter = new BatchConverter(fileConverter);

        // Act - Enable parallel processing
        var options = new BatchOptions
        {
            EnableParallel = true,
            MaxDegreeOfParallelism = 4
        };
        var report = await batchConverter.ConvertAsync(_testInputDir, _testOutputDir, options);

        // Assert - Verify all 3 YAML files created
        Assert.Equal(3, report.Total);
        Assert.Equal(3, report.Success);
        Assert.Equal(0, report.Failed);

        // Verify YAML files exist
        var yaml1 = Path.Combine(_testOutputDir, "1_Char1", "KOJO_K1_Test.yaml");
        var yaml2 = Path.Combine(_testOutputDir, "2_Char2", "KOJO_K2_Test.yaml");
        var yaml3 = Path.Combine(_testOutputDir, "3_Char3", "KOJO_K3_Test.yaml");

        Assert.True(File.Exists(yaml1), $"YAML file should exist: {yaml1}");
        Assert.True(File.Exists(yaml2), $"YAML file should exist: {yaml2}");
        Assert.True(File.Exists(yaml3), $"YAML file should exist: {yaml3}");
    }

    /// <summary>
    /// AC#2: Test parallel conversion report counts are correct
    /// Expected: BatchReport.Total == 3, Success == 3, Failed == 0
    /// Feature 635 - RED test (BatchOptions does not exist yet)
    /// </summary>
    [Fact]
    public async Task Test_Parallel_ReportCountsCorrect()
    {
        // Arrange - Create 3 valid ERB files
        var erb1 = Path.Combine(_testInputDir, "1_Char1", "KOJO_K1_Test.ERB");
        var erb2 = Path.Combine(_testInputDir, "2_Char2", "KOJO_K2_Test.ERB");
        var erb3 = Path.Combine(_testInputDir, "3_Char3", "KOJO_K3_Test.ERB");

        Directory.CreateDirectory(Path.GetDirectoryName(erb1)!);
        Directory.CreateDirectory(Path.GetDirectoryName(erb2)!);
        Directory.CreateDirectory(Path.GetDirectoryName(erb3)!);

        File.WriteAllText(erb1, CreateMinimalErbContent());
        File.WriteAllText(erb2, CreateMinimalErbContent());
        File.WriteAllText(erb3, CreateMinimalErbContent());

        var fileConverter = new TestFileConverter(_talentCsvPath, _schemaPath);
        var batchConverter = new BatchConverter(fileConverter);

        // Act - Enable parallel processing
        var options = new BatchOptions
        {
            EnableParallel = true,
            MaxDegreeOfParallelism = 4
        };
        var report = await batchConverter.ConvertAsync(_testInputDir, _testOutputDir, options);

        // Assert - Verify report counts
        Assert.NotNull(report);
        Assert.Equal(3, report.Total);
        Assert.Equal(3, report.Success);
        Assert.Equal(0, report.Failed);
        Assert.Empty(report.Failures);
    }

    /// <summary>
    /// AC#3: Test parallel conversion error isolation
    /// Expected: 2 succeed, 1 fails - errors in one file don't affect others
    /// Feature 635 - RED test (BatchOptions does not exist yet)
    /// </summary>
    [Fact]
    public async Task Test_Parallel_ErrorIsolation()
    {
        // Arrange - Create 2 valid, 1 invalid ERB file
        var validErb1 = Path.Combine(_testInputDir, "1_Char1", "KOJO_K1_Valid.ERB");
        var invalidErb = Path.Combine(_testInputDir, "2_Char2", "KOJO_K2_Invalid.ERB");
        var validErb2 = Path.Combine(_testInputDir, "3_Char3", "KOJO_K3_Valid.ERB");

        Directory.CreateDirectory(Path.GetDirectoryName(validErb1)!);
        Directory.CreateDirectory(Path.GetDirectoryName(invalidErb)!);
        Directory.CreateDirectory(Path.GetDirectoryName(validErb2)!);

        File.WriteAllText(validErb1, CreateMinimalErbContent());
        File.WriteAllText(invalidErb, "INVALID ERB SYNTAX ###");
        File.WriteAllText(validErb2, CreateMinimalErbContent());

        // Use FileConverter that throws on invalid
        var fileConverter = new TestFileConverter(_talentCsvPath, _schemaPath, throwOnInvalid: true);
        var batchConverter = new BatchConverter(fileConverter);

        // Act - Enable parallel processing
        var options = new BatchOptions
        {
            EnableParallel = true,
            MaxDegreeOfParallelism = 4
        };
        var report = await batchConverter.ConvertAsync(_testInputDir, _testOutputDir, options);

        // Assert - Verify 2 succeeded, 1 failed
        Assert.Equal(3, report.Total);
        Assert.Equal(2, report.Success);
        Assert.Equal(1, report.Failed);
        Assert.Single(report.Failures);

        // Verify failed file is recorded
        var failure = report.Failures[0];
        Assert.False(failure.Success);
        Assert.Contains("Invalid", failure.FilePath);
        Assert.NotNull(failure.Error);
    }

    /// <summary>
    /// AC#4: Test sequential processing as default behavior
    /// Expected: ConvertAsync() without options parameter uses sequential mode
    /// Feature 635 - RED test (modified ConvertAsync signature does not exist yet)
    /// </summary>
    [Fact]
    public async Task Test_Sequential_DefaultBehavior()
    {
        // Arrange - Create 3 valid ERB files
        var erb1 = Path.Combine(_testInputDir, "1_Char1", "KOJO_K1_Test.ERB");
        var erb2 = Path.Combine(_testInputDir, "2_Char2", "KOJO_K2_Test.ERB");
        var erb3 = Path.Combine(_testInputDir, "3_Char3", "KOJO_K3_Test.ERB");

        Directory.CreateDirectory(Path.GetDirectoryName(erb1)!);
        Directory.CreateDirectory(Path.GetDirectoryName(erb2)!);
        Directory.CreateDirectory(Path.GetDirectoryName(erb3)!);

        File.WriteAllText(erb1, CreateMinimalErbContent());
        File.WriteAllText(erb2, CreateMinimalErbContent());
        File.WriteAllText(erb3, CreateMinimalErbContent());

        var fileConverter = new TestFileConverter(_talentCsvPath, _schemaPath);
        var batchConverter = new BatchConverter(fileConverter);

        // Act - Call without BatchOptions (default sequential behavior)
        var report = await batchConverter.ConvertAsync(_testInputDir, _testOutputDir);

        // Assert - Verify sequential processing works (same result as before)
        Assert.NotNull(report);
        Assert.Equal(3, report.Total);
        Assert.Equal(3, report.Success);
        Assert.Equal(0, report.Failed);

        // Verify all YAML files created
        var yaml1 = Path.Combine(_testOutputDir, "1_Char1", "KOJO_K1_Test.yaml");
        var yaml2 = Path.Combine(_testOutputDir, "2_Char2", "KOJO_K2_Test.yaml");
        var yaml3 = Path.Combine(_testOutputDir, "3_Char3", "KOJO_K3_Test.yaml");

        Assert.True(File.Exists(yaml1), $"YAML file should exist: {yaml1}");
        Assert.True(File.Exists(yaml2), $"YAML file should exist: {yaml2}");
        Assert.True(File.Exists(yaml3), $"YAML file should exist: {yaml3}");
    }

    #region Helper Methods

    /// <summary>
    /// Create minimal valid ERB content for testing
    /// Contains a simple PRINTDATA block with DATALIST
    /// </summary>
    private string CreateMinimalErbContent()
    {
        return @"@TEST_FUNCTION
PRINTDATA
DATALIST
DATAFORM Test line 1
DATAFORM Test line 2
ENDLIST
ENDDATA
";
    }

    #endregion

    #region Test FileConverter Stub

    /// <summary>
    /// Test stub for IFileConverter - returns success for valid ERB, throws for invalid
    /// </summary>
    private sealed class TestFileConverter : IFileConverter
    {
        private readonly string _talentCsvPath;
        private readonly string _schemaPath;
        private readonly bool _throwOnInvalid;

        public TestFileConverter(string talentCsvPath, string schemaPath, bool throwOnInvalid = false)
        {
            _talentCsvPath = talentCsvPath;
            _schemaPath = schemaPath;
            _throwOnInvalid = throwOnInvalid;
        }

        public async Task<List<ConversionResult>> ConvertAsync(string erbFilePath, string outputDirectory)
        {
            await Task.CompletedTask; // Simulate async work

            // Check if file is marked as invalid
            var content = File.ReadAllText(erbFilePath);
            if (_throwOnInvalid && (content.Contains("INVALID") || content.Contains("MALFORMED")))
            {
                throw new InvalidOperationException("Invalid ERB syntax");
            }

            // Create output YAML file
            var filename = Path.GetFileNameWithoutExtension(erbFilePath);
            var yamlPath = Path.Combine(outputDirectory, $"{filename}.yaml");

            // Ensure output directory exists
            Directory.CreateDirectory(outputDirectory);

            // Write minimal YAML
            File.WriteAllText(yamlPath, "character: Test\nsituation: K1\nbranches:\n  - lines: ['Test']");

            return new List<ConversionResult>
            {
                new ConversionResult(
                    Success: true,
                    FilePath: erbFilePath,
                    Error: null
                )
            };
        }
    }

    #endregion
}
