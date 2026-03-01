using Era.Core.Dialogue;
using Xunit;

namespace KojoComparer.Tests;

public class KojoBranchesParserTests
{
    [Fact]
    public void Parse_WithLinesWithMetadata_ReturnsCorrectDisplayModes()
    {
        var yamlContent = @"character: 美鈴
situation: COM_100
branches:
- lines_with_metadata:
  - text: テスト行
    display_mode: newline
";

        var parser = new KojoBranchesParser();
        var result = parser.Parse(yamlContent);

        Assert.Single(result.DialogueLines);
        Assert.Equal("テスト行", result.DialogueLines[0].Text);
        Assert.Equal(DisplayMode.Newline, result.DialogueLines[0].DisplayMode);
    }

    [Fact]
    public void Parse_WithMultipleLinesWithMetadata_ReturnsMultipleDisplayModes()
    {
        var yamlContent = @"character: 美鈴
situation: COM_101
branches:
- lines_with_metadata:
  - text: 第一行
    display_mode: newline
  - text: 第二行
    display_mode: default
";

        var parser = new KojoBranchesParser();
        var result = parser.Parse(yamlContent);

        Assert.Equal(2, result.DialogueLines.Count);
        Assert.Equal("第一行", result.DialogueLines[0].Text);
        Assert.Equal(DisplayMode.Newline, result.DialogueLines[0].DisplayMode);
        Assert.Equal("第二行", result.DialogueLines[1].Text);
        Assert.Equal(DisplayMode.Default, result.DialogueLines[1].DisplayMode);
    }

    [Fact]
    public void Parse_WithComId_ParsesSuccessfully()
    {
        var yamlContent = @"character: 魔理沙
situation: K10_会話親密
com_id: 302
branches:
- lines:
  - 'テスト行1'
  - 'テスト行2'
  condition: {}
";

        var parser = new KojoBranchesParser();
        var result = parser.Parse(yamlContent);

        Assert.Equal(2, result.DialogueLines.Count);
        Assert.Equal("テスト行1", result.DialogueLines[0].Text);
        Assert.Equal("テスト行2", result.DialogueLines[1].Text);
    }
}
