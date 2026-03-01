using Xunit;

namespace KojoQualityValidator.Tests;

public class IntegrationTests
{
    private static int RunCLI(params string[] args)
    {
        return Program.Main(args);
    }

    [Fact]
    public void CLI_WithPassingFile_ReturnsExitCode0()
    {
        // Arrange
        var testFile = Path.GetFullPath(Path.Combine("TestData", "quality-pass.yaml"));

        // Act
        var exitCode = RunCLI("--files", testFile);

        // Assert
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void CLI_WithFailingFile_ReturnsExitCode1()
    {
        // Arrange
        var testFile = Path.GetFullPath(Path.Combine("TestData", "quality-fail.yaml"));

        // Act
        var exitCode = RunCLI("--files", testFile);

        // Assert
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public void CLI_WithCustomMinEntries_AppliesThreshold()
    {
        // Arrange
        var testFile = Path.GetFullPath(Path.Combine("TestData", "quality-fail.yaml"));

        // Act - Lower threshold should pass
        var exitCode = RunCLI("--files", testFile, "--min-entries", "2");

        // Assert
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void CLI_WithCustomMinLines_AppliesThreshold()
    {
        // Arrange
        var testFile = Path.GetFullPath(Path.Combine("TestData", "quality-fail.yaml"));

        // Act - Lower line threshold should pass
        var exitCode = RunCLI("--files", testFile, "--min-lines", "3");

        // Assert
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void CLI_WithNoArguments_ReturnsNonZeroExitCode()
    {
        // Act
        var exitCode = RunCLI();

        // Assert - Should fail with validation error
        Assert.NotEqual(0, exitCode);
    }
}
