using System.Text.Json;
using Xunit;

namespace KojoComparer.Tests;

// Additional coverage tests for FileDiscovery.
// Targets untested branches: GenerateFunctionName, BuildComIdMap, FindComRangesForErb
// (skip_combinations, character_overrides, KU_ pattern, unimplemented ranges),
// FindYamlFileForSubFunction, GetSubFunctionYamlFileCount, and full end-to-end flows.
[Trait("Category", "Unit")]
public class FileDiscoveryCoverageTests
{
    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static void Cleanup(params string[] paths)
    {
        foreach (var p in paths)
        {
            if (File.Exists(p))
                File.Delete(p);
            else if (Directory.Exists(p))
                Directory.Delete(p, recursive: true);
        }
    }

    private static (FileDiscovery discovery, string erbDir, string yamlDir, string mapFile)
        CreateLayout(string mapJson)
    {
        var erbDir = Path.Combine(Path.GetTempPath(), "fd_erb_" + Guid.NewGuid().ToString("N"));
        var yamlDir = Path.Combine(Path.GetTempPath(), "fd_yaml_" + Guid.NewGuid().ToString("N"));
        var mapFile = Path.Combine(Path.GetTempPath(), "fd_map_" + Guid.NewGuid().ToString("N") + ".json");
        Directory.CreateDirectory(erbDir);
        Directory.CreateDirectory(yamlDir);
        File.WriteAllText(mapFile, mapJson);
        return (new FileDiscovery(erbDir, yamlDir, mapFile), erbDir, yamlDir, mapFile);
    }

    private static string SimpleRangeMap(int start, int end, string file, bool implemented = true)
        => "{\"ranges\":[{\"start\":" + start + ",\"end\":" + end + ",\"file\":\"" + file + "\",\"implemented\":" + (implemented ? "true" : "false") + "}]}";

    private static void CreateKojoErb(string erbDir, string charSubDir, string erbFileName)
    {
        var dir = Path.Combine(erbDir, charSubDir);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, erbFileName), "");
    }

    private static void CreateKojoYaml(string yamlDir, string charSubDir, string yamlFileName, int comId)
    {
        var dir = Path.Combine(yamlDir, charSubDir);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, yamlFileName), "com_id: " + comId + "\ncharacter: test\n");
    }

    // ----------------------------------------------------------------
    // GenerateFunctionName - standard COM (no sub-function)
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_StandardCom_FunctionNameUsesFullComId()
    {
        var mapJson = SimpleRangeMap(42, 42, "_aibu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");
            CreateKojoYaml(yamlDir, "1_meirin", "K1_aibu.yaml", 42);

            var cases = disc.DiscoverTestCases();

            var tc = Assert.Single(cases);
            Assert.Equal("@KOJO_MESSAGE_COM_K1_42", tc.FunctionName);
            Assert.Equal("1", tc.CharacterId);
            Assert.Equal(42, tc.ComId);
            Assert.Null(tc.SubFunctionIndex);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    [Fact]
    public void DiscoverTestCases_HighComId_FunctionNameContainsFullId()
    {
        var mapJson = SimpleRangeMap(301, 301, "_kaiwashinmitsu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "10_marisa", "KOJO_K10_kaiwashinmitsu.ERB");
            CreateKojoYaml(yamlDir, "10_marisa", "K10_kaiwashinmitsu.yaml", 301);

            var cases = disc.DiscoverTestCases();

            var tc = Assert.Single(cases);
            Assert.Equal("@KOJO_MESSAGE_COM_K10_301", tc.FunctionName);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // GenerateFunctionName - sub-function overload (with index suffix)
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_SubFunctionCom_FunctionNameIncludesSubIndex()
    {
        var mapJson = "{\"ranges\":[{\"start\":463,\"end\":463,\"file\":\"_nichijo.ERB\",\"implemented\":true,\"sub_functions\":[0,1]}]}";
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_nichijo.ERB");
            var ydir = Path.Combine(yamlDir, "1_meirin");
            Directory.CreateDirectory(ydir);
            File.WriteAllText(Path.Combine(ydir, "K1_nichijo_0.yaml"), "com_id: 463\ncharacter: test\n");
            File.WriteAllText(Path.Combine(ydir, "K1_nichijo_1.yaml"), "com_id: 463\ncharacter: test\n");

            var cases = disc.DiscoverTestCases();

            Assert.Equal(2, cases.Count);
            var sub0 = cases.First(tc => tc.SubFunctionIndex == 0);
            var sub1 = cases.First(tc => tc.SubFunctionIndex == 1);
            Assert.Equal("@KOJO_MESSAGE_COM_K1_463_0", sub0.FunctionName);
            Assert.Equal("@KOJO_MESSAGE_COM_K1_463_1", sub1.FunctionName);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    [Fact]
    public void DiscoverTestCases_SubFunctionCom_MismatchedYamlCount_Skipped()
    {
        // 3 sub-functions declared but only 2 YAML files: positional mapping unreliable, skip
        var mapJson = "{\"ranges\":[{\"start\":463,\"end\":463,\"file\":\"_nichijo.ERB\",\"implemented\":true,\"sub_functions\":[0,1,2]}]}";
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_nichijo.ERB");
            var ydir = Path.Combine(yamlDir, "1_meirin");
            Directory.CreateDirectory(ydir);
            File.WriteAllText(Path.Combine(ydir, "K1_nichijo_0.yaml"), "com_id: 463\ncharacter: test\n");
            File.WriteAllText(Path.Combine(ydir, "K1_nichijo_1.yaml"), "com_id: 463\ncharacter: test\n");

            var cases = disc.DiscoverTestCases();

            Assert.Empty(cases);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // KU_ (universal character) pattern
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_KuCharacter_ExtractsCharacterIdU()
    {
        var mapJson = SimpleRangeMap(5, 5, "_nichijo.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "U_hanyo", "KOJO_KU_nichijo.ERB");
            CreateKojoYaml(yamlDir, "U_hanyo", "KU_nichijo.yaml", 5);

            var cases = disc.DiscoverTestCases();

            var tc = Assert.Single(cases);
            Assert.Equal("U", tc.CharacterId);
            Assert.Equal("@KOJO_MESSAGE_COM_KU_5", tc.FunctionName);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // Unimplemented range is skipped
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_UnimplementedRange_ReturnsEmpty()
    {
        var mapJson = SimpleRangeMap(10, 10, "_aibu.ERB", implemented: false);
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");
            CreateKojoYaml(yamlDir, "1_meirin", "K1_aibu.yaml", 10);

            var cases = disc.DiscoverTestCases();

            Assert.Empty(cases);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // skip_combinations - matching character+file is skipped
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_SkipCombination_SkipsMatchingCharacterAndFile()
    {
        var mapJson = "{\"ranges\":[{\"start\":20,\"end\":20,\"file\":\"_aibu.ERB\",\"implemented\":true}]," +
                      "\"skipCombinations\":[{\"character\":\"K1\",\"file\":\"_aibu.ERB\"}]}";
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");
            CreateKojoYaml(yamlDir, "1_meirin", "K1_aibu.yaml", 20);

            var cases = disc.DiscoverTestCases();

            Assert.Empty(cases);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    [Fact]
    public void DiscoverTestCases_SkipCombination_OnlySkipsMatchingCharacter()
    {
        // K1 is skipped; K2 with same file should still produce test cases
        var mapJson = "{\"ranges\":[{\"start\":20,\"end\":20,\"file\":\"_aibu.ERB\",\"implemented\":true}]," +
                      "\"skipCombinations\":[{\"character\":\"K1\",\"file\":\"_aibu.ERB\"}]}";
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "2_cirno", "KOJO_K2_aibu.ERB");
            CreateKojoYaml(yamlDir, "2_cirno", "K2_aibu.yaml", 20);

            var cases = disc.DiscoverTestCases();

            Assert.Single(cases);
            Assert.Equal("2", cases[0].CharacterId);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // character_overrides - COM redirected to different file
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_CharacterOverride_SkipsComThatBelongsToDifferentFile()
    {
        // COM 30 overridden to _tokushu.ERB for K1 -> not in _aibu.ERB
        var mapJson = "{\"ranges\":[{\"start\":30,\"end\":30,\"file\":\"_aibu.ERB\",\"implemented\":true}]," +
                      "\"characterOverrides\":{\"K1\":{\"30\":\"_tokushu.ERB\"}}}";
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");
            CreateKojoYaml(yamlDir, "1_meirin", "K1_aibu.yaml", 30);

            var cases = disc.DiscoverTestCases();

            Assert.Empty(cases);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    [Fact]
    public void DiscoverTestCases_CharacterOverride_IncludesComWhenFileMatches()
    {
        // Override points back to same file -> COM is still included
        var mapJson = "{\"ranges\":[{\"start\":31,\"end\":31,\"file\":\"_aibu.ERB\",\"implemented\":true}]," +
                      "\"characterOverrides\":{\"K1\":{\"31\":\"_aibu.ERB\"}}}";
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");
            CreateKojoYaml(yamlDir, "1_meirin", "K1_aibu.yaml", 31);

            var cases = disc.DiscoverTestCases();

            Assert.Single(cases);
            Assert.Equal(31, cases[0].ComId);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // BuildComIdMap - non-K-pattern YAML files are filtered out
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_NonKPatternYaml_Ignored()
    {
        var mapJson = SimpleRangeMap(50, 50, "_aibu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");

            var ydir = Path.Combine(yamlDir, "1_meirin");
            Directory.CreateDirectory(ydir);
            // Non-K-pattern files that should be filtered
            File.WriteAllText(Path.Combine(ydir, "NTR_koujou_A.yaml"), "com_id: 50\ncharacter: test\n");
            File.WriteAllText(Path.Combine(ydir, "WC_aibu.yaml"), "com_id: 50\ncharacter: test\n");
            File.WriteAllText(Path.Combine(ydir, "SexHara_xxx.yaml"), "com_id: 50\ncharacter: test\n");
            // Only this K-pattern file should be included
            File.WriteAllText(Path.Combine(ydir, "K1_aibu.yaml"), "com_id: 50\ncharacter: test\n");

            var cases = disc.DiscoverTestCases();

            Assert.Single(cases);
            Assert.EndsWith("K1_aibu.yaml", cases[0].YamlFile, StringComparison.OrdinalIgnoreCase);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    [Fact]
    public void DiscoverTestCases_YamlWithoutComId_Ignored()
    {
        var mapJson = SimpleRangeMap(60, 60, "_aibu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");

            var ydir = Path.Combine(yamlDir, "1_meirin");
            Directory.CreateDirectory(ydir);
            // K-pattern file but no com_id field
            File.WriteAllText(Path.Combine(ydir, "K1_aibu.yaml"),
                "character: meirin\nbranches:\n- lines:\n  - test\n");

            var cases = disc.DiscoverTestCases();

            Assert.Empty(cases);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // BuildComIdMap - multi-file sorted by numeric suffix
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_SubFunctions_YamlsSortedByNumericSuffix()
    {
        // Files created in reverse order; result must be sorted 0 < 1 by suffix
        var mapJson = "{\"ranges\":[{\"start\":463,\"end\":463,\"file\":\"_nichijo.ERB\",\"implemented\":true,\"sub_functions\":[0,1]}]}";
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_nichijo.ERB");
            var ydir = Path.Combine(yamlDir, "1_meirin");
            Directory.CreateDirectory(ydir);
            File.WriteAllText(Path.Combine(ydir, "K1_nichijo_1.yaml"), "com_id: 463\ncharacter: test\n");
            File.WriteAllText(Path.Combine(ydir, "K1_nichijo_0.yaml"), "com_id: 463\ncharacter: test\n");

            var cases = disc.DiscoverTestCases();

            Assert.Equal(2, cases.Count);
            var sub0 = cases.First(tc => tc.SubFunctionIndex == 0);
            var sub1 = cases.First(tc => tc.SubFunctionIndex == 1);
            Assert.EndsWith("_0.yaml", sub0.YamlFile, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith("_1.yaml", sub1.YamlFile, StringComparison.OrdinalIgnoreCase);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // GetOrBuildComIdMap - no matching character directory
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_NoMatchingYamlCharDir_ReturnsEmpty()
    {
        var mapJson = SimpleRangeMap(0, 0, "_aibu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");
            // No yamlDir/1_* created intentionally

            var cases = disc.DiscoverTestCases();

            Assert.Empty(cases);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // GetOrBuildComIdMap - cache reuse across multiple COMs for same character
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_MultipleComsForSameCharacter_BothResolved()
    {
        var mapJson = SimpleRangeMap(1, 2, "_aibu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");
            var ydir = Path.Combine(yamlDir, "1_meirin");
            Directory.CreateDirectory(ydir);
            File.WriteAllText(Path.Combine(ydir, "K1_com1.yaml"), "com_id: 1\ncharacter: test\n");
            File.WriteAllText(Path.Combine(ydir, "K1_com2.yaml"), "com_id: 2\ncharacter: test\n");

            var cases = disc.DiscoverTestCases();

            Assert.Equal(2, cases.Count);
            Assert.Contains(cases, tc => tc.ComId == 1);
            Assert.Contains(cases, tc => tc.ComId == 2);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // GetRepresentativeState - always returns empty dictionary
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_StateIsAlwaysEmpty()
    {
        var mapJson = SimpleRangeMap(100, 100, "_aibu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");
            CreateKojoYaml(yamlDir, "1_meirin", "K1_aibu.yaml", 100);

            var cases = disc.DiscoverTestCases();

            var tc = Assert.Single(cases);
            Assert.Empty(tc.State);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // ERB files that do not start with KOJO_K are skipped
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_ErbFileNotStartingWithKojoK_Skipped()
    {
        var mapJson = SimpleRangeMap(0, 0, "_aibu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            var edir = Path.Combine(erbDir, "1_meirin");
            Directory.CreateDirectory(edir);
            File.WriteAllText(Path.Combine(edir, "KOJO_MODIFIER_aibu.ERB"), "");
            File.WriteAllText(Path.Combine(edir, "COM_HELPER.ERB"), "");

            var cases = disc.DiscoverTestCases();

            Assert.Empty(cases);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // ERB filename KOJO_K{N} without trailing category - regex fails
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_ErbFilenameWithoutCategorySuffix_ProducesNoTestCases()
    {
        var mapJson = SimpleRangeMap(0, 0, "_aibu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            var edir = Path.Combine(erbDir, "1_meirin");
            Directory.CreateDirectory(edir);
            File.WriteAllText(Path.Combine(edir, "KOJO_K1.ERB"), "");

            var cases = disc.DiscoverTestCases();

            Assert.Empty(cases);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // com_id on line 5 (within first-10 window) - still parsed
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_ComIdOnLine5_StillParsed()
    {
        var mapJson = SimpleRangeMap(77, 77, "_aibu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");

            var ydir = Path.Combine(yamlDir, "1_meirin");
            Directory.CreateDirectory(ydir);
            File.WriteAllText(Path.Combine(ydir, "K1_aibu.yaml"),
                "character: meirin\n" +
                "situation: COM_77\n" +
                "# some comment\n" +
                "author: test\n" +
                "com_id: 77\n" +
                "branches:\n");

            var cases = disc.DiscoverTestCases();

            var tc = Assert.Single(cases);
            Assert.Equal(77, tc.ComId);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // com_id on line 11 (beyond first-10 limit) - not found
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_ComIdBeyondLine10_NotFound()
    {
        var mapJson = SimpleRangeMap(88, 88, "_aibu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");

            var ydir = Path.Combine(yamlDir, "1_meirin");
            Directory.CreateDirectory(ydir);
            // 10 filler lines then com_id on line 11 - beyond the scan window
            var lines = string.Join("\n",
                Enumerable.Range(0, 10).Select(i => "# filler " + i).Append("com_id: 88"));
            File.WriteAllText(Path.Combine(ydir, "K1_aibu.yaml"), lines);

            var cases = disc.DiscoverTestCases();

            Assert.Empty(cases);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // Multiple implemented ranges in same map
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_MultipleImplementedRanges_AllResolved()
    {
        var mapJson = "{\"ranges\":[" +
                      "{\"start\":10,\"end\":11,\"file\":\"_aibu.ERB\",\"implemented\":true}," +
                      "{\"start\":20,\"end\":20,\"file\":\"_aibu.ERB\",\"implemented\":true}" +
                      "]}";
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "1_meirin", "KOJO_K1_aibu.ERB");

            var ydir = Path.Combine(yamlDir, "1_meirin");
            Directory.CreateDirectory(ydir);
            File.WriteAllText(Path.Combine(ydir, "K1_com10.yaml"), "com_id: 10\ncharacter: test\n");
            File.WriteAllText(Path.Combine(ydir, "K1_com11.yaml"), "com_id: 11\ncharacter: test\n");
            File.WriteAllText(Path.Combine(ydir, "K1_com20.yaml"), "com_id: 20\ncharacter: test\n");

            var cases = disc.DiscoverTestCases();

            Assert.Equal(3, cases.Count);
            Assert.Contains(cases, tc => tc.ComId == 10);
            Assert.Contains(cases, tc => tc.ComId == 11);
            Assert.Contains(cases, tc => tc.ComId == 20);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // LoadComFileMap - empty JSON object (null Ranges) throws
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_MapWithNullRanges_ThrowsInvalidOperation()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "fd_null_" + Guid.NewGuid().ToString("N") + ".json");
        var tempErbDir = Path.Combine(Path.GetTempPath(), "fd_erb_" + Guid.NewGuid().ToString("N"));
        var tempYamlDir = Path.Combine(Path.GetTempPath(), "fd_yaml_" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(tempErbDir);
            Directory.CreateDirectory(tempYamlDir);
            File.WriteAllText(tempFile, "{\"ranges\":null}");
            var disc = new FileDiscovery(tempErbDir, tempYamlDir, tempFile);

            Assert.Throws<InvalidOperationException>(() => disc.DiscoverTestCases());
        }
        finally { Cleanup(tempFile, tempErbDir, tempYamlDir); }
    }

    // ----------------------------------------------------------------
    // TestCase fields are fully populated for standard COM
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_TestCaseFields_AllPopulated()
    {
        var mapJson = SimpleRangeMap(7, 7, "_aibu.ERB");
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            var eCharDir = Path.Combine(erbDir, "3_patchouli");
            Directory.CreateDirectory(eCharDir);
            var erbFilePath = Path.Combine(eCharDir, "KOJO_K3_aibu.ERB");
            File.WriteAllText(erbFilePath, "");

            var yCharDir = Path.Combine(yamlDir, "3_patchouli");
            Directory.CreateDirectory(yCharDir);
            var yamlFilePath = Path.Combine(yCharDir, "K3_aibu.yaml");
            File.WriteAllText(yamlFilePath, "com_id: 7\ncharacter: patchouli\n");

            var cases = disc.DiscoverTestCases();

            var tc = Assert.Single(cases);
            Assert.Equal(erbFilePath, tc.ErbFile);
            Assert.Equal(yamlFilePath, tc.YamlFile);
            Assert.Equal("3", tc.CharacterId);
            Assert.Equal(7, tc.ComId);
            Assert.Equal("@KOJO_MESSAGE_COM_K3_7", tc.FunctionName);
            Assert.Null(tc.SubFunctionIndex);
            Assert.NotNull(tc.State);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }

    // ----------------------------------------------------------------
    // Sub-function COM with no YAML directory for the character
    // ----------------------------------------------------------------

    [Fact]
    public void DiscoverTestCases_SubFunctionCom_WithNoYamlCharDir_ProducesNoTestCase()
    {
        var mapJson = "{\"ranges\":[{\"start\":463,\"end\":463,\"file\":\"_nichijo.ERB\",\"implemented\":true,\"sub_functions\":[0,1]}]}";
        var (disc, erbDir, yamlDir, mapFile) = CreateLayout(mapJson);
        try
        {
            CreateKojoErb(erbDir, "5_reimu", "KOJO_K5_nichijo.ERB");
            // No yamlDir/5_* created intentionally

            var cases = disc.DiscoverTestCases();

            Assert.Empty(cases);
        }
        finally { Cleanup(erbDir, yamlDir, mapFile); }
    }
}
