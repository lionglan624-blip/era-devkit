using System;
using System.Collections.Generic;
using System.IO;
using ErbParser;
using ErbParser.Ast;
using ErbToYaml;
using Xunit;
using YamlDotNet.Serialization;

namespace ErbToYaml.Tests;

/// <summary>
/// Tests for SelectCaseConverter (Feature 765 - AC#5, AC#6, AC#7)
/// </summary>
public class SelectCaseConverterTests
{
    /// <summary>
    /// AC#5: Multi-value CASE produces OR condition
    /// Expected: CASE 13,25 → LogicalOp(||, ArgRef(==13), ArgRef(==25))
    /// </summary>
    [Fact]
    public void MultiValueCase_ProducesOrCondition()
    {
        // Setup
        var talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
        var talentLoader = new TalentCsvLoader(talentCsvPath);
        var variableTypePrefixes = new Dictionary<Type, string>
        {
            { typeof(CflagRef), "CFLAG" },
            { typeof(TcvarRef), "TCVAR" },
            { typeof(EquipRef), "EQUIP" },
            { typeof(ItemRef), "ITEM" },
            { typeof(StainRef), "STAIN" },
            { typeof(MarkRef), "MARK" },
            { typeof(ExpRef), "EXP" },
            { typeof(NowexRef), "NOWEX" },
            { typeof(AblRef), "ABL" },
            { typeof(FlagRef), "FLAG" },
            { typeof(TflagRef), "TFLAG" },
            { typeof(TequipRef), "TEQUIP" },
            { typeof(PalamRef), "PALAM" },
        };
        var conditionSerializer = new ConditionSerializer(talentLoader, null, variableTypePrefixes);
        var conditionExtractor = new ConditionExtractor();
        var converter = new SelectCaseConverter(conditionSerializer, conditionExtractor);

        // Create SelectCaseNode with CASE 13,25
        var selectCase = new SelectCaseNode { Subject = "ARG" };
        var branch = new CaseBranch();
        branch.Values.Add("13");
        branch.Values.Add("25");
        branch.Body.Add(new PrintformNode { Variant = "PRINTFORML", Content = "Test text" });
        selectCase.Branches.Add(branch);

        // Act
        var yaml = converter.Convert(selectCase, "Test", "Test");

        // Debug output
        Console.WriteLine("Generated YAML:");
        Console.WriteLine(yaml);

        // Assert: YAML contains OR condition with ARG:0 eq 13 and ARG:0 eq 25
        Assert.Contains("OR:", yaml);
        Assert.Contains("ARG:", yaml);
        Assert.Contains("eq:", yaml);
        Assert.Contains("13", yaml);
        Assert.Contains("25", yaml);
        Assert.Contains("Test text", yaml);
    }

    /// <summary>
    /// AC#6: SelectCaseConverter transforms SelectCaseNode to YAML
    /// Expected: Full YAML output with entries, conditions, and content
    /// </summary>
    [Fact]
    public void SelectCaseToYaml_ProducesCorrectOutput()
    {
        // Setup
        var talentCsvPath = Era.DevKit.TestUtils.GamePathHelper.Resolve("CSV", "Talent.csv");
        var talentLoader = new TalentCsvLoader(talentCsvPath);
        var variableTypePrefixes = new Dictionary<Type, string>
        {
            { typeof(CflagRef), "CFLAG" },
            { typeof(TcvarRef), "TCVAR" },
            { typeof(EquipRef), "EQUIP" },
            { typeof(ItemRef), "ITEM" },
            { typeof(StainRef), "STAIN" },
            { typeof(MarkRef), "MARK" },
            { typeof(ExpRef), "EXP" },
            { typeof(NowexRef), "NOWEX" },
            { typeof(AblRef), "ABL" },
            { typeof(FlagRef), "FLAG" },
            { typeof(TflagRef), "TFLAG" },
            { typeof(TequipRef), "TEQUIP" },
            { typeof(PalamRef), "PALAM" },
        };
        var conditionSerializer = new ConditionSerializer(talentLoader, null, variableTypePrefixes);
        var conditionExtractor = new ConditionExtractor();
        var converter = new SelectCaseConverter(conditionSerializer, conditionExtractor);

        // Create SelectCaseNode with 2 CASE branches + CASEELSE with nested IF
        var selectCase = new SelectCaseNode { Subject = "ARG" };

        // CASE 13,25
        var branch1 = new CaseBranch();
        branch1.Values.Add("13");
        branch1.Values.Add("25");
        branch1.Body.Add(new PrintformNode { Variant = "PRINTFORML", Content = "Case 13 or 25 text" });
        selectCase.Branches.Add(branch1);

        // CASE 21
        var branch2 = new CaseBranch();
        branch2.Values.Add("21");
        branch2.Body.Add(new PrintformNode { Variant = "PRINTFORML", Content = "Case 21 text" });
        selectCase.Branches.Add(branch2);

        // CASEELSE with nested IF
        var ifNode = new IfNode { Condition = "EQUIP:3 != 0" };
        ifNode.Body.Add(new PrintformNode { Variant = "PRINTFORML", Content = "IF EQUIP text" });
        var elseBranch = new ElseBranch();
        elseBranch.Body.Add(new PrintformNode { Variant = "PRINTFORML", Content = "ELSE text" });
        ifNode.ElseBranch = elseBranch;
        selectCase.CaseElse = new List<AstNode> { ifNode };

        // Act
        var yaml = converter.Convert(selectCase, "TestChar", "TestSit");

        // Debug output
        Console.WriteLine("Generated YAML:");
        Console.WriteLine(yaml);

        // Assert: Verify YAML structure
        Assert.Contains("character: TestChar", yaml);
        Assert.Contains("situation: TestSit", yaml);
        Assert.Contains("entries:", yaml);
        Assert.Contains("Case 13 or 25 text", yaml);
        Assert.Contains("Case 21 text", yaml);
        Assert.Contains("IF EQUIP text", yaml);
        Assert.Contains("ELSE text", yaml);
        Assert.Contains("OR:", yaml); // Multi-value CASE produces OR
        Assert.Contains("ARG:", yaml); // ARG conditions
        Assert.Contains("EQUIP", yaml); // Nested IF EQUIP condition (may have : or not)
    }

    /// <summary>
    /// AC#7: BranchesToEntriesConverter generates ARG condition IDs
    /// Expected: ARG conditions produce "arg_0_{index}" IDs
    /// </summary>
    [Fact]
    public void BranchesToEntries_ArgConditionId()
    {
        // This test verifies BranchesToEntriesConverter handles ARG conditions

        // Create branch with ARG condition
        var branches = new List<object>
        {
            new Dictionary<string, object>
            {
                { "lines", new List<string> { "ARG text" } },
                {
                    "condition", new Dictionary<string, object>
                    {
                        {
                            "ARG", new Dictionary<string, object>
                            {
                                {
                                    "0", new Dictionary<string, object>
                                    {
                                        { "eq", "13" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var entries = BranchesToEntriesConverter.Convert(branches);

        // Assert
        Assert.Single(entries);

        // F764: 4-segment format with value extracted from condition
        // Expected: arg_{argIndex}_{value}_{branchIndex} → "arg_0_13_0"
        Assert.Equal("arg_0_13_0", entries[0]["id"]);

        // F764: Verify ARG condition is passed through (not transformed)
        Assert.True(entries[0].ContainsKey("condition"));
        var condition = (Dictionary<string, object>)entries[0]["condition"];
        Assert.True(condition.ContainsKey("ARG"));
        var argDict = (Dictionary<string, object>)condition["ARG"];
        Assert.True(argDict.ContainsKey("0"));
        var opDict = (Dictionary<string, object>)argDict["0"];
        Assert.Equal("13", opDict["eq"]);
    }
}
