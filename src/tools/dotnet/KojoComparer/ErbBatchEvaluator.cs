namespace KojoComparer;

/// <summary>
/// Batch executor for in-process ERB evaluation.
/// Uses ErbEvaluator for 100x+ speed improvement over subprocess-based execution.
/// </summary>
public class ErbBatchEvaluator : IBatchExecutor
{
    private readonly ErbEvaluator _evaluator;

    public ErbBatchEvaluator(ErbEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    /// <summary>
    /// Executes all test cases in parallel using in-process evaluation.
    /// Returns results keyed by function name (without @ prefix).
    /// </summary>
    public async Task<Dictionary<string, (string output, List<Era.Core.Dialogue.DisplayMode> displayModes)>> ExecuteAllAsync(
        List<TestCase> testCases)
    {
        var results = new Dictionary<string, (string output, List<Era.Core.Dialogue.DisplayMode> displayModes)>();
        var lockObject = new object();

        // Parallel execution - each evaluation is independent
        await Parallel.ForEachAsync(testCases, new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        }, async (testCase, ct) =>
        {
            try
            {
                var (output, displayModes) = await _evaluator.ExecuteAsync(
                    testCase.ErbFile,
                    testCase.FunctionName,
                    testCase.State);

                // Normalize @ prefix - store without @
                var normalizedKey = testCase.FunctionName.StartsWith("@")
                    ? testCase.FunctionName.Substring(1)
                    : testCase.FunctionName;

                lock (lockObject)
                {
                    if (!results.ContainsKey(normalizedKey))
                    {
                        results[normalizedKey] = (output, displayModes);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue processing other test cases
                Console.WriteLine($"ERROR: Failed to evaluate {testCase.FunctionName}: {ex.Message}");
            }
        });

        return results;
    }
}
