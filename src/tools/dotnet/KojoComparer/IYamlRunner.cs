using Era.Core.Types;

namespace KojoComparer;

public interface IYamlRunner
{
    string Render(string yamlFilePath, Dictionary<string, object> context);
    DialogueResult RenderWithMetadata(string yamlFilePath, Dictionary<string, object> context);
}
