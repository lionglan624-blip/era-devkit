using System.IO;
using Xunit;

namespace KojoComparer.Tests;

public class DiagnosticTest
{
    [Fact]
    public void TestYamlFileCreationAndParsing()
    {
        // Create temp file
        var tempDir = Path.Combine(Path.GetTempPath(), "kojo_diagnostic");
        Directory.CreateDirectory(tempDir);

        var yamlFilePath = Path.Combine(tempDir, "meirin_com100.yaml");
        var yamlContent = "character: 美鈴\n" +
                          "situation: COM_100\n" +
                          "branches:\n" +
                          "- lines_with_metadata:\n" +
                          "  - text: テスト行\n" +
                          "    display_mode: newline\n";

        try
        {
            // Write file
            File.WriteAllText(yamlFilePath, yamlContent);

            // Verify file exists
            Assert.True(File.Exists(yamlFilePath), $"File should exist at {yamlFilePath}");

            // Read it back
            var readContent = File.ReadAllText(yamlFilePath);
            Assert.Contains("branches:", readContent);

            // Parse with KojoBranchesParser directly
            var parser = new KojoBranchesParser();
            var result = parser.Parse(readContent);

            Assert.Single(result.DialogueLines);
            Assert.Equal("テスト行", result.DialogueLines[0].Text);

            // Now test through YamlRunner
            var yamlRunner = new YamlRunner();
            var yamlContext = new Dictionary<string, object>();
            var yamlResult = yamlRunner.RenderWithMetadata(yamlFilePath, yamlContext);

            Assert.Single(yamlResult.DialogueLines);
            Assert.Equal("テスト行", yamlResult.DialogueLines[0].Text);
        }
        finally
        {
            if (File.Exists(yamlFilePath))
                File.Delete(yamlFilePath);
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir);
        }
    }
}
