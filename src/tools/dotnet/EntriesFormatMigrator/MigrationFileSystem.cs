namespace EntriesFormatMigrator;

public class MigrationFileSystem : IMigrationFileSystem
{
    public string[] GetDirectories(string path) => Directory.GetDirectories(path);

    public string[] GetFiles(string path, string searchPattern)
        => Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public string ReadAllText(string path) => File.ReadAllText(path);

    public DateTime GetLastWriteTime(string path) => File.GetLastWriteTime(path);

    public void WriteAllText(string path, string content) => File.WriteAllText(path, content);
}
