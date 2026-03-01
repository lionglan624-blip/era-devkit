using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ErbToYaml;

/// <summary>
/// Loads Talent.csv and provides name→index mapping
/// F349 Task 2 implementation
/// </summary>
public class TalentCsvLoader
{
    private readonly Dictionary<string, int> _talentNameToIndex = new();

    /// <summary>
    /// Load Talent.csv and build name→index mapping
    /// </summary>
    /// <param name="talentCsvPath">Path to Talent.csv</param>
    public TalentCsvLoader(string talentCsvPath)
    {
        if (!File.Exists(talentCsvPath))
        {
            throw new FileNotFoundException($"Talent.csv not found at: {talentCsvPath}");
        }

        LoadTalentCsv(talentCsvPath);
    }

    /// <summary>
    /// Get talent index by name
    /// </summary>
    /// <param name="talentName">Talent name (e.g., "恋慕")</param>
    /// <returns>Talent index, or null if not found</returns>
    public int? GetTalentIndex(string talentName)
    {
        if (string.IsNullOrWhiteSpace(talentName))
        {
            return null;
        }

        if (_talentNameToIndex.TryGetValue(talentName, out int index))
        {
            return index;
        }

        return null;
    }

    private void LoadTalentCsv(string talentCsvPath)
    {
        var lines = File.ReadAllLines(talentCsvPath);

        foreach (var line in lines)
        {
            // Skip empty lines and comment lines
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith(";"))
            {
                continue;
            }

            // Parse CSV line: INDEX,NAME,DESCRIPTION
            // Example: 3,恋慕,;愛情に似た感情を抱いている状態。
            var parts = line.Split(',');

            if (parts.Length < 2)
            {
                continue; // Skip malformed lines
            }

            // Extract index and name
            var indexStr = parts[0].Trim();
            var name = parts[1].Trim();

            // Parse index
            if (!int.TryParse(indexStr, out int index))
            {
                continue; // Skip if index is not a number
            }

            // Skip if name is empty
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            // Add to mapping (only if not already present)
            if (!_talentNameToIndex.ContainsKey(name))
            {
                _talentNameToIndex[name] = index;
            }
        }
    }
}
