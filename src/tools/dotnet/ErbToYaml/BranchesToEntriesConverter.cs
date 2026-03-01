using System;
using System.Collections.Generic;
using System.Linq;

namespace ErbToYaml;

/// <summary>
/// Converts legacy branches format to canonical entries format
/// Feature 675 - Task 2
/// </summary>
public static class BranchesToEntriesConverter
{
    /// <summary>
    /// Convert legacy branches format to entries format
    /// </summary>
    /// <param name="branches">List of branch dictionaries with lines/condition</param>
    /// <returns>List of entry dictionaries with id/content/priority/condition</returns>
    public static List<Dictionary<string, object>> Convert(List<object> branches)
    {
        var entries = new List<Dictionary<string, object>>();
        int branchCount = branches.Count;

        for (int i = 0; i < branchCount; i++)
        {
            var branch = (Dictionary<string, object>)branches[i];
            var lines = (List<string>)branch["lines"];
            var condition = branch.ContainsKey("condition")
                ? (Dictionary<string, object>)branch["condition"]
                : null;

            // Join lines into content (preserve line breaks)
            var content = string.Join("\n", lines);

            // Generate ID from condition or use "fallback"
            var id = GenerateId(condition, i);

            // Calculate priority (reverse index: first branch = highest)
            var priority = branchCount - i;

            var entry = new Dictionary<string, object>
            {
                { "id", id },
                { "content", content },
                { "priority", priority }
            };

            // Transform condition from nested dict to DialogueCondition format
            if (condition != null && condition.Count > 0)
            {
                var transformedCondition = TransformCondition(condition);
                if (transformedCondition != null)
                {
                    entry["condition"] = transformedCondition;
                }
            }

            if (branch.ContainsKey("displayMode"))
            {
                entry["displayMode"] = branch["displayMode"];
            }

            entries.Add(entry);
        }

        return entries;
    }

    private static string GenerateId(Dictionary<string, object>? condition, int index)
    {
        if (condition == null || condition.Count == 0)
            return "fallback";

        // Handle compound conditions
        if (condition.ContainsKey("AND"))
            return $"and_compound_{index}";
        if (condition.ContainsKey("OR"))
            return $"or_compound_{index}";
        if (condition.ContainsKey("NOT"))
            return $"not_compound_{index}";

        // Extract condition type and generate semantic ID with branch index to prevent collisions
        if (condition.ContainsKey("TALENT"))
        {
            var talentDict = (Dictionary<string, object>)condition["TALENT"];
            var talentId = talentDict.Keys.First(); // e.g., "3"
            return $"talent_{talentId}_{index}";
        }
        if (condition.ContainsKey("ABL"))
        {
            var ablDict = (Dictionary<string, object>)condition["ABL"];
            var ablId = ablDict.Keys.First();
            return $"abl_{ablId}_{index}";
        }
        // F764: Handle ARG conditions - 4-segment format with value
        if (condition.ContainsKey("ARG"))
        {
            var argDict = (Dictionary<string, object>)condition["ARG"];
            var argIndex = argDict.Keys.First(); // "0" for ARG, "1" for ARG:1
            var opDict = (Dictionary<string, object>)argDict[argIndex];
            var value = opDict.Values.First()?.ToString() ?? "0"; // Extract value from operator dict
            return $"arg_{argIndex}_{value}_{index}";
        }

        // Fallback for unknown condition types
        return $"condition_{index}";
    }

    private static Dictionary<string, object>? TransformCondition(Dictionary<string, object> legacyCondition)
    {
        // Passthrough compound conditions (AND/OR/NOT) and ARG conditions without transformation
        // F764: ARG conditions use nested format (schema expects { ARG: { "0": { eq: "2" } } })
        if (legacyCondition.ContainsKey("AND") ||
            legacyCondition.ContainsKey("OR") ||
            legacyCondition.ContainsKey("NOT") ||
            legacyCondition.ContainsKey("ARG"))
        {
            return legacyCondition;
        }

        // Transform { TALENT: { 3: { ne: 0 } } }
        // → { type: "Talent", talentType: "3", threshold: 1 }

        if (legacyCondition.ContainsKey("TALENT"))
        {
            var talentDict = (Dictionary<string, object>)legacyCondition["TALENT"];
            var talentId = talentDict.Keys.First();
            var operatorDict = (Dictionary<string, object>)talentDict[talentId];

            // Extract operator (ne, eq, gt, etc.)
            var op = operatorDict.Keys.First();
            var value = operatorDict[op].ToString() ?? "0";

            // Map to threshold (ne 0 → threshold 1, eq 0 → threshold 0, etc.)
            int threshold = MapOperatorToThreshold(op, value);

            return new Dictionary<string, object>
            {
                { "type", "Talent" },
                { "talentType", talentId },
                { "threshold", threshold }
            };
        }

        // Handle ABL conditions
        if (legacyCondition.ContainsKey("ABL"))
        {
            var ablDict = (Dictionary<string, object>)legacyCondition["ABL"];
            var ablId = ablDict.Keys.First();
            var operatorDict = (Dictionary<string, object>)ablDict[ablId];
            var op = operatorDict.Keys.First();
            var value = operatorDict[op].ToString() ?? "0";
            int threshold = MapOperatorToThreshold(op, value);

            return new Dictionary<string, object>
            {
                { "type", "Abl" },
                { "ablType", ablId },
                { "threshold", threshold }
            };
        }

        // Handle EXP conditions
        if (legacyCondition.ContainsKey("EXP"))
        {
            var expDict = (Dictionary<string, object>)legacyCondition["EXP"];
            var expId = expDict.Keys.First();
            var operatorDict = (Dictionary<string, object>)expDict[expId];
            var op = operatorDict.Keys.First();
            var value = operatorDict[op].ToString() ?? "0";
            int threshold = MapOperatorToThreshold(op, value);

            return new Dictionary<string, object>
            {
                { "type", "Exp" },
                { "expType", expId },
                { "threshold", threshold }
            };
        }

        // Handle FLAG conditions
        if (legacyCondition.ContainsKey("FLAG"))
        {
            var flagDict = (Dictionary<string, object>)legacyCondition["FLAG"];
            var flagId = flagDict.Keys.First();
            var operatorDict = (Dictionary<string, object>)flagDict[flagId];
            var op = operatorDict.Keys.First();
            var value = operatorDict[op].ToString() ?? "0";
            int threshold = MapOperatorToThreshold(op, value);

            return new Dictionary<string, object>
            {
                { "type", "Flag" },
                { "flagId", flagId },
                { "threshold", threshold }
            };
        }

        // Handle CFLAG conditions
        if (legacyCondition.ContainsKey("CFLAG"))
        {
            var cflagDict = (Dictionary<string, object>)legacyCondition["CFLAG"];
            var cflagId = cflagDict.Keys.First();
            var operatorDict = (Dictionary<string, object>)cflagDict[cflagId];
            var op = operatorDict.Keys.First();
            var value = operatorDict[op].ToString() ?? "0";
            int threshold = MapOperatorToThreshold(op, value);

            return new Dictionary<string, object>
            {
                { "type", "CFlag" },
                { "cflagId", cflagId },
                { "threshold", threshold }
            };
        }

        return null;
    }

    private static int MapOperatorToThreshold(string op, string value)
    {
        // For existence checks (ne 0), threshold is 1
        // For exact value checks (eq N), threshold is N
        // For comparison (gt N), threshold is N+1
        return op switch
        {
            "ne" when value == "0" => 1,
            "eq" => int.Parse(value),
            "gt" => int.Parse(value) + 1,
            "gte" => int.Parse(value),
            _ => 1 // Default: treat as existence check
        };
    }
}
