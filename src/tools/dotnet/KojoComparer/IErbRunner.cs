namespace KojoComparer;

public interface IErbRunner
{
    Task<(string output, List<Era.Core.Dialogue.DisplayMode> displayModes)> ExecuteAsync(string erbFilePath, string functionName, Dictionary<string, int> state);
}
