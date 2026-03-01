using Era.Core.IO;

namespace EntriesFormatMigrator;

public interface IMigrationFileSystem : IFileSystem
{
    void WriteAllText(string path, string content);
}
