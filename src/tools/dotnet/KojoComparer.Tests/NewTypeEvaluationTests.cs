using Era.Core.Dialogue;
using KojoComparer;
using Xunit;

namespace KojoComparer.Tests;

public class NewTypeEvaluationTests
{
    private readonly KojoBranchesParser _parser;

    public NewTypeEvaluationTests()
    {
        _parser = new KojoBranchesParser();
    }

    [Fact]
    public void MarkYaml_MarkConditionTrue()
    {
        // YAML with condition: { MARK: { "MASTER:100": { ne: 0 } } }
        // State: { "MARK:MASTER:100": 1 }
        // Expected: MARK branch text selected
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - MARK branch text
  condition:
    MARK:
      MASTER:100:
        ne: 0
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>
        {
            { "MARK:MASTER:100", 1 }
        };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("MARK branch text", result.DialogueLines[0].Text);
    }

    [Fact]
    public void TflagYaml_TflagConditionTrue()
    {
        // YAML with condition: { TFLAG: { "コマンド成功度": { eq: 1 } } }
        // State: { "TFLAG:コマンド成功度": 1 }
        // Expected: TFLAG branch text selected
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - TFLAG branch text
  condition:
    TFLAG:
      コマンド成功度:
        eq: 1
- lines:
  - ELSE text
  condition: {}
";
        var state = new Dictionary<string, int>
        {
            { "TFLAG:コマンド成功度", 1 }
        };
        var result = _parser.Parse(yamlContent, state);
        Assert.Single(result.DialogueLines);
        Assert.Equal("TFLAG branch text", result.DialogueLines[0].Text);
    }
}
