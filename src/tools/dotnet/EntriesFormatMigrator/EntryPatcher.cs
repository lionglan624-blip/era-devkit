using System.Text;
using System.Text.RegularExpressions;

namespace EntriesFormatMigrator;

public class EntryPatcher : IEntryPatcher
{
    private enum State { SCANNING, IN_ENTRY, AFTER_PRIORITY }

    private record EntryMetadata(int IdLineNumber, string IdValue, int Priority, bool HasCondition, int PriorityLineNumber);

    public string PatchEntries(string fileContent)
    {
        // Normalize line endings to \n for consistent processing
        var normalizedContent = fileContent.Replace("\r\n", "\n");
        var lines = normalizedContent.Split('\n');
        var entries = CollectEntryMetadata(lines);

        // Apply modifications in reverse order to preserve line numbers
        var modifiedLines = new List<string>(lines);
        foreach (var entry in entries.OrderByDescending(e => e.IdLineNumber))
        {
            if (entry.HasCondition)
                continue;

            // Apply ID rename and condition insertion based on priority
            if (entry.Priority == 4)
            {
                // Rename id: fallback -> id: talent_16_0
                modifiedLines[entry.IdLineNumber] = modifiedLines[entry.IdLineNumber].Replace("id: fallback", "id: talent_16_0");
                // Insert condition after priority line
                InsertConditionBlock(modifiedLines, entry.PriorityLineNumber, 16);
            }
            else if (entry.Priority == 2)
            {
                // Rename id: fallback -> id: talent_17_0
                modifiedLines[entry.IdLineNumber] = modifiedLines[entry.IdLineNumber].Replace("id: fallback", "id: talent_17_0");
                // Insert condition after priority line
                InsertConditionBlock(modifiedLines, entry.PriorityLineNumber, 17);
            }
            // Priority 1 (ELSE) and Priority 3 (talent_3_1) remain unchanged
        }

        return string.Join('\n', modifiedLines);
    }

    private List<EntryMetadata> CollectEntryMetadata(string[] lines)
    {
        var entries = new List<EntryMetadata>();
        var state = State.SCANNING;
        EntryMetadata? currentEntry = null;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            switch (state)
            {
                case State.SCANNING:
                    var idMatch = Regex.Match(line, @"^- id:\s*(.+)$");
                    if (idMatch.Success)
                    {
                        currentEntry = new EntryMetadata(
                            IdLineNumber: i,
                            IdValue: idMatch.Groups[1].Value.Trim(),
                            Priority: -1,
                            HasCondition: false,
                            PriorityLineNumber: -1
                        );
                        state = State.IN_ENTRY;
                    }
                    break;

                case State.IN_ENTRY:
                    var priorityMatch = Regex.Match(line, @"^  priority:\s*(\d+)$");
                    if (priorityMatch.Success && currentEntry != null)
                    {
                        currentEntry = currentEntry with
                        {
                            Priority = int.Parse(priorityMatch.Groups[1].Value),
                            PriorityLineNumber = i
                        };
                        state = State.AFTER_PRIORITY;
                    }
                    else if (Regex.IsMatch(line, @"^- id:"))
                    {
                        // Next entry started before finding priority - save current and start new
                        if (currentEntry != null)
                        {
                            entries.Add(currentEntry);
                        }
                        var newIdMatch = Regex.Match(line, @"^- id:\s*(.+)$");
                        currentEntry = new EntryMetadata(
                            IdLineNumber: i,
                            IdValue: newIdMatch.Groups[1].Value.Trim(),
                            Priority: -1,
                            HasCondition: false,
                            PriorityLineNumber: -1
                        );
                        state = State.IN_ENTRY;
                    }
                    break;

                case State.AFTER_PRIORITY:
                    if (Regex.IsMatch(line, @"^  condition:"))
                    {
                        currentEntry = currentEntry! with { HasCondition = true };
                        state = State.IN_ENTRY; // Continue scanning for next entry
                    }
                    else if (Regex.IsMatch(line, @"^- id:"))
                    {
                        // Next entry started - save current
                        if (currentEntry != null)
                        {
                            entries.Add(currentEntry);
                        }
                        var newIdMatch = Regex.Match(line, @"^- id:\s*(.+)$");
                        currentEntry = new EntryMetadata(
                            IdLineNumber: i,
                            IdValue: newIdMatch.Groups[1].Value.Trim(),
                            Priority: -1,
                            HasCondition: false,
                            PriorityLineNumber: -1
                        );
                        state = State.IN_ENTRY;
                    }
                    break;
            }
        }

        // Add last entry if exists
        if (currentEntry != null)
        {
            entries.Add(currentEntry);
        }

        return entries;
    }

    private void InsertConditionBlock(List<string> lines, int priorityLineNumber, int talentType)
    {
        var conditionBlock = new[]
        {
            "  condition:",
            "    type: Talent",
            $"    talentType: {talentType}",
            "    threshold: 1"
        };

        lines.InsertRange(priorityLineNumber + 1, conditionBlock);
    }

    public bool EntryHasCondition(string entryText)
    {
        return Regex.IsMatch(entryText, @"^  condition:", RegexOptions.Multiline);
    }
}
