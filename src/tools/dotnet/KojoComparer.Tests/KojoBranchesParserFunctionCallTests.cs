using KojoComparer;
using Xunit;

namespace KojoComparer.Tests;

public class KojoBranchesParserFunctionCallTests
{
    [Fact]
    public void FunctionCallCompound_WithNullDelegate_ReturnsFalse()
    {
        var parser = new KojoBranchesParser();
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Function branch text
  condition:
    AND:
      - TALENT:
          16:
            ne: 0
      - FUNCTION:
          name: HAS_VAGINA
          args:
            - TARGET
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>
        {
            { "TALENT:16", 1 }
        };

        var result = parser.Parse(yamlContent, state);
        Assert.Equal("ELSE text", result.DialogueLines[0].Text);
    }

    [Fact]
    public void FunctionCallSimple_WithNullDelegate_ReturnsFalse()
    {
        var parser = new KojoBranchesParser();
        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - Function branch text
  condition:
    FUNCTION:
      name: HAS_VAGINA
      args:
        - TARGET
- lines:
  - ELSE text
  condition: {}
";

        var state = new Dictionary<string, int>();

        var result = parser.Parse(yamlContent, state);
        Assert.Equal("ELSE text", result.DialogueLines[0].Text);
    }

    [Fact]
    public void FunctionCallSimple_WithDelegateReturningTrue_SelectsIfBranch()
    {
        // AC#8: FUNCTION evaluation uses configurable delegate (Positive test)
        var parser = new KojoBranchesParser((name, args) =>
        {
            return name == "FIRSTTIME" && args.Length == 1 && args[0] == "350";
        });

        var yamlContent = @"
character: Test
situation: Test
branches:
- lines:
  - 初回テスト
  condition:
    FUNCTION:
      name: FIRSTTIME
      args:
        - ""350""
- lines:
  - デフォルト
  condition: {}
";

        var state = new Dictionary<string, int>();

        var result = parser.Parse(yamlContent, state);
        Assert.Equal("初回テスト", result.DialogueLines[0].Text);
    }
}
