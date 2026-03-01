namespace KojoComparer;

public interface IBatchExecutor
{
    Task<Dictionary<string, (string output, List<Era.Core.Dialogue.DisplayMode> displayModes)>> ExecuteAllAsync(List<TestCase> testCases);
}
