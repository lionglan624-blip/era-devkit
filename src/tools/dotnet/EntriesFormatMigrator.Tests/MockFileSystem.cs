namespace EntriesFormatMigrator.Tests;

public class MockFileSystem : IMigrationFileSystem
{
    private readonly Dictionary<string, string> _files = new();

    public void AddFile(string path, string content)
    {
        _files[path] = content;
    }

    public string[] GetDirectories(string path)
    {
        return Array.Empty<string>();
    }

    public string[] GetFiles(string path, string searchPattern)
    {
        return _files.Keys.Where(k => k.StartsWith(path) && k.EndsWith(".yaml")).ToArray();
    }

    public bool DirectoryExists(string path)
    {
        return true;
    }

    public bool FileExists(string path)
    {
        return _files.ContainsKey(path);
    }

    public string ReadAllText(string path)
    {
        if (!_files.ContainsKey(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }
        return _files[path];
    }

    public DateTime GetLastWriteTime(string path)
    {
        return DateTime.Now;
    }

    public void WriteAllText(string path, string content)
    {
        _files[path] = content;
    }

    public string GetWrittenContent(string path)
    {
        return _files.ContainsKey(path) ? _files[path] : string.Empty;
    }
}
