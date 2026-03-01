using Era.Core.Dialogue;
using Era.Core.Types;

namespace KojoComparer;

/// <summary>
/// Processes multiple kojo files and generates batch reports.
/// </summary>
public class BatchProcessor
{
    private readonly IErbRunner _erbRunner;
    private readonly IYamlRunner _yamlRunner;
    private readonly OutputNormalizer _normalizer;
    private readonly DiffEngine _diffEngine;
    private readonly IBatchExecutor _batchExecutor;

    public BatchProcessor(IErbRunner erbRunner, IYamlRunner yamlRunner, OutputNormalizer normalizer, DiffEngine diffEngine, IBatchExecutor batchExecutor)
    {
        _erbRunner = erbRunner;
        _yamlRunner = yamlRunner;
        _normalizer = normalizer;
        _diffEngine = diffEngine;
        _batchExecutor = batchExecutor;
    }

    /// <summary>
    /// Batch report summary.
    /// </summary>
    public class BatchReport
    {
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public List<string> Failures { get; set; } = new();
    }

    /// <summary>
    /// Processes all discovered test cases and generates summary report.
    /// Used with --all flag and FileDiscovery.
    /// </summary>
    /// <param name="testCases">List of test cases from FileDiscovery</param>
    /// <returns>Batch report with TotalTests, PassedTests, FailedTests</returns>
    public async Task<BatchReport> ProcessAllAsync(List<TestCase> testCases)
    {
        var report = new BatchReport();

        // Phase 1-3: Execute all ERB cases in batch
        Dictionary<string, (string output, List<Era.Core.Dialogue.DisplayMode> displayModes)> erbResults;

        try
        {
            erbResults = await _batchExecutor.ExecuteAllAsync(testCases);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Batch execution failed: {ex.Message}", ex);
        }

        // Phase 4: Compare each test case
        foreach (var testCase in testCases)
        {
            report.TotalTests++;

            try
            {
                // Look up ERB result by function name (normalize @ prefix at this boundary)
                var normalizedFuncName = testCase.FunctionName.TrimStart('@');
                if (!erbResults.TryGetValue(normalizedFuncName, out var erbResult))
                {
                    report.FailedTests++;
                    report.Failures.Add($"ERROR: COM_{testCase.ComId:D3} ({testCase.CharacterId}) - No batch result found for {testCase.FunctionName}");
                    continue;
                }

                var (erbOutput, erbDisplayModes) = erbResult;
                var comparisonResult = CompareTestCase(testCase, erbOutput, erbDisplayModes);

                if (comparisonResult.IsMatch)
                {
                    report.PassedTests++;
                }
                else
                {
                    report.FailedTests++;
                    report.Failures.Add($"FAIL: COM_{testCase.ComId:D3} ({testCase.CharacterId})");
                    report.Failures.AddRange(comparisonResult.Differences);
                }
            }
            catch (Exception ex)
            {
                report.FailedTests++;
                report.Failures.Add($"ERROR: COM_{testCase.ComId:D3} - {ex.Message}");
            }
        }

        return report;
    }

    /// <summary>
    /// Processes multiple ERB/YAML file pairs and generates summary.
    /// </summary>
    /// <param name="erbDirectory">Directory containing ERB files</param>
    /// <param name="yamlDirectory">Directory containing YAML files</param>
    /// <param name="functionName">ERB function name to test</param>
    /// <param name="states">List of states to test</param>
    /// <returns>Batch report</returns>
    public async Task<BatchReport> ProcessAsync(string erbDirectory, string yamlDirectory, string functionName, List<Dictionary<string, int>> states)
    {
        var report = new BatchReport();

        // Find all ERB files in directory
        var erbFiles = Directory.GetFiles(erbDirectory, "*.ERB", SearchOption.AllDirectories);

        foreach (var erbFile in erbFiles)
        {
            // Try to find corresponding YAML file
            var erbFileName = Path.GetFileNameWithoutExtension(erbFile);
            var yamlFiles = Directory.GetFiles(yamlDirectory, "*.yaml", SearchOption.AllDirectories)
                .Where(f => Path.GetFileNameWithoutExtension(f).Contains(erbFileName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (yamlFiles.Length == 0)
            {
                report.Failures.Add($"No matching YAML file found for {erbFileName}");
                continue;
            }

            var yamlFile = yamlFiles[0]; // Use first match

            // Test each state
            foreach (var state in states)
            {
                report.TotalTests++;

                try
                {
                    // Execute ERB
                    var (erbOutput, erbDisplayModes) = await _erbRunner.ExecuteAsync(erbFile, functionName, state);

                    // Create test case for comparison
                    var testCase = new TestCase
                    {
                        YamlFile = yamlFile,
                        State = state,
                        ComId = 0, // Not used in this path
                        CharacterId = "0", // Not used
                        FunctionName = functionName
                    };

                    var comparison = CompareTestCase(testCase, erbOutput, erbDisplayModes);

                    if (comparison.IsMatch)
                    {
                        report.PassedTests++;
                    }
                    else
                    {
                        report.FailedTests++;
                        var stateStr = string.Join(", ", state.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                        report.Failures.Add($"FAIL: {erbFileName} ({stateStr})");
                        report.Failures.AddRange(comparison.Differences);
                    }
                }
                catch (Exception ex)
                {
                    report.FailedTests++;
                    report.Failures.Add($"ERROR: {erbFileName} - {ex.Message}");
                }
            }
        }

        return report;
    }

    /// <summary>
    /// Shared comparison logic for both ProcessAllAsync and ProcessAsync.
    /// Renders YAML for the test case and compares against ERB output.
    /// </summary>
    private DiffEngine.ComparisonResult CompareTestCase(TestCase testCase, string erbOutput, List<Era.Core.Dialogue.DisplayMode> erbDisplayModes)
    {
        var normalizedErb = _normalizer.Normalize(erbOutput);

        // Convert state to context format for YamlRunner
        var context = StateConverter.ConvertStateToContext(testCase.State);

        // Render YAML with metadata
        var yamlResult = _yamlRunner.RenderWithMetadata(testCase.YamlFile, context);
        var yamlOutput = string.Join("\n", yamlResult.DialogueLines.Select(dl => dl.Text));
        var normalizedYaml = _normalizer.Normalize(yamlOutput);
        var yamlDisplayModes = yamlResult.DialogueLines.Select(dl => dl.DisplayMode).ToList();

        // Compare with ERB displayModes from structuredOutput
        return _diffEngine.CompareSubset(normalizedErb, normalizedYaml, displayModesA: erbDisplayModes, displayModesB: yamlDisplayModes);
    }
}
