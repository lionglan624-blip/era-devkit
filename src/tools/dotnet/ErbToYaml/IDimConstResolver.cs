namespace ErbToYaml;

public interface IDimConstResolver
{
    int? Resolve(string name);
    string ResolveToString(string value);
}
