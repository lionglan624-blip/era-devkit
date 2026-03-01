using Era.Core.Dialogue;

namespace KojoComparer;

/// <summary>
/// Compares two normalized outputs and reports differences.
/// </summary>
public class DiffEngine
{
    /// <summary>
    /// Result of comparison operation.
    /// </summary>
    public class ComparisonResult
    {
        public bool IsMatch { get; set; }
        public List<string> Differences { get; set; } = new();
        public List<string> DisplayModeDifferences { get; set; } = new();
    }

    /// <summary>
    /// Performs line-by-line comparison and reports mismatches.
    /// Optionally compares displayMode metadata when provided.
    /// </summary>
    public ComparisonResult Compare(
        string normalizedA,
        string normalizedB,
        List<DisplayMode>? displayModesA = null,
        List<DisplayMode>? displayModesB = null)
    {
        var result = new ComparisonResult();

        // Quick equality check
        if (normalizedA == normalizedB)
        {
            result.IsMatch = true;
        }
        else
        {
            // Split into lines for detailed comparison
            var linesA = normalizedA.Split('\n');
            var linesB = normalizedB.Split('\n');

            // Check line count mismatch
            if (linesA.Length != linesB.Length)
            {
                result.IsMatch = false;
                result.Differences.Add($"Line count mismatch: ERB has {linesA.Length} lines, YAML has {linesB.Length} lines");
            }

            // Compare line by line
            var maxLines = Math.Max(linesA.Length, linesB.Length);
            for (int i = 0; i < maxLines; i++)
            {
                var lineA = i < linesA.Length ? linesA[i] : "(missing)";
                var lineB = i < linesB.Length ? linesB[i] : "(missing)";

                if (lineA != lineB)
                {
                    result.IsMatch = false;
                    result.Differences.Add($"Line {i + 1} differs:");
                    result.Differences.Add($"  ERB:  \"{lineA}\"");
                    result.Differences.Add($"  YAML: \"{lineB}\"");
                }
            }
        }

        // DisplayMode comparison
        if (displayModesA != null && displayModesB != null)
        {
            CompareDisplayModes(result, displayModesA, displayModesB);
        }
        else if (displayModesA != null || displayModesB != null)
        {
            var side = displayModesA != null ? "ERB" : "YAML";
            result.DisplayModeDifferences.Add($"INFO: {side} has displayMode metadata, other side does not");
        }

        return result;
    }

    /// <summary>
    /// Performs subset comparison: verifies all ERB lines exist in YAML content.
    /// Handles PRINTDATA/DATALIST random selection semantics.
    /// DisplayMode comparison is skipped (incompatible with subset matching).
    /// </summary>
    public ComparisonResult CompareSubset(
        string normalizedErb,
        string normalizedYaml,
        List<DisplayMode>? displayModesA = null,
        List<DisplayMode>? displayModesB = null)
    {
        var result = new ComparisonResult();

        // Quick equality check
        if (normalizedErb == normalizedYaml)
        {
            result.IsMatch = true;
            return result;
        }

        // Split into lines
        var erbLines = normalizedErb.Split('\n');
        var yamlLines = normalizedYaml.Split('\n');

        // Build HashSet for O(1) lookup
        var yamlLineSet = new HashSet<string>(yamlLines);

        // Check each ERB line exists in YAML
        bool allLinesExist = true;
        for (int i = 0; i < erbLines.Length; i++)
        {
            var erbLine = erbLines[i];
            if (!yamlLineSet.Contains(erbLine))
            {
                allLinesExist = false;
                result.Differences.Add($"ERB line {i + 1} not found in YAML:");
                result.Differences.Add($"  \"{erbLine}\"");
            }
        }

        result.IsMatch = allLinesExist;

        // DisplayMode comparison note
        if (displayModesA != null || displayModesB != null)
        {
            result.DisplayModeDifferences.Add("INFO: DisplayMode comparison skipped in subset matching mode");
        }

        return result;
    }

    private void CompareDisplayModes(ComparisonResult result, List<DisplayMode> modesA, List<DisplayMode> modesB)
    {
        if (modesA.Count != modesB.Count)
        {
            result.DisplayModeDifferences.Add($"DisplayMode count mismatch: ERB has {modesA.Count}, YAML has {modesB.Count}");
            return;
        }

        for (int i = 0; i < modesA.Count; i++)
        {
            if (modesA[i] != modesB[i])
            {
                result.DisplayModeDifferences.Add($"Line {i + 1} displayMode differs: ERB=\"{modesA[i]}\", YAML=\"{modesB[i]}\"");
            }
        }
    }
}
