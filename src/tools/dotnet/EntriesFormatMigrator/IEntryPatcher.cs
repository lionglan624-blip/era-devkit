namespace EntriesFormatMigrator;

public interface IEntryPatcher
{
    string PatchEntries(string fileContent);
    bool EntryHasCondition(string entryText);
}
