using Era.Core.Dialogue;
using KojoComparer;
using Xunit;

namespace KojoComparer.Tests;

public class KojoBranchesParserEquipItemTests
{
    private readonly KojoBranchesParser _parser;

    public KojoBranchesParserEquipItemTests()
    {
        _parser = new KojoBranchesParser();
    }

    [Fact]
    public void EquipItemYaml_EquipConditionTrue()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - EQUIP branch text
  condition:
    EQUIP:
      MASTER:下半身上着１:
        ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "EQUIP:MASTER:下半身上着１", 1 }
        };

        var result = _parser.Parse(yamlContent, state);

        Assert.Single(result.DialogueLines);
        Assert.Equal("EQUIP branch text", result.DialogueLines[0].Text);
    }

    [Fact]
    public void EquipItemYaml_EquipConditionFalse()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - EQUIP branch text
  condition:
    EQUIP:
      MASTER:下半身上着１:
        ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "EQUIP:MASTER:下半身上着１", 0 }
        };

        var result = _parser.Parse(yamlContent, state);

        Assert.Single(result.DialogueLines);
        Assert.Equal("ELSE text", result.DialogueLines[0].Text);
    }

    [Fact]
    public void EquipItemYaml_ItemConditionTrue()
    {
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - ITEM branch text
  condition:
    ITEM:
      2:
        ne: 0
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "ITEM:2", 1 }
        };

        var result = _parser.Parse(yamlContent, state);

        Assert.Single(result.DialogueLines);
        Assert.Equal("ITEM branch text", result.DialogueLines[0].Text);
    }
}
