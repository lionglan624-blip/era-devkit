using System;
using System.IO;
using System.Threading.Tasks;
using ErbToYaml;
using Xunit;

namespace ErbToYaml.Tests;

/// <summary>
/// TDD tests for Feature 634: Batch Conversion Tool - Program CLI argument validation
/// Tests AC#11: Program.cs returns exit code 1 on invalid arguments
/// </summary>
public class InvalidArgumentsTests
{
    /// <summary>
    /// AC#11: Test no arguments returns exit code 1
    /// Expected: Program.Main returns 1 when called with no arguments
    /// </summary>
    [Fact]
    public async Task Test_NoArguments_ReturnsExitCode1()
    {
        // Arrange
        string[] args = Array.Empty<string>();

        // Act
        int exitCode = await Program.Main(args);

        // Assert
        Assert.Equal(1, exitCode);
    }

    /// <summary>
    /// AC#11: Test --batch without directory argument returns exit code 1
    /// Expected: Program.Main returns 1 when --batch flag has no value
    /// </summary>
    [Fact]
    public async Task Test_BatchWithoutDirectory_ReturnsExitCode1()
    {
        // Arrange - --batch flag with no directory argument
        string[] args = new[] { "--batch" };

        // Act
        int exitCode = await Program.Main(args);

        // Assert
        Assert.Equal(1, exitCode);
    }

    /// <summary>
    /// AC#11: Test --batch with non-existent directory returns exit code 1
    /// Expected: Program.Main returns 1 when input directory does not exist
    /// </summary>
    [Fact]
    public async Task Test_BatchWithNonExistentDirectory_ReturnsExitCode1()
    {
        // Arrange - Use a directory path that is guaranteed not to exist
        string nonExistentDir = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}");
        string[] args = new[] { "--batch", nonExistentDir };

        // Ensure directory does not exist
        Assert.False(Directory.Exists(nonExistentDir), "Test precondition: directory should not exist");

        // Act
        int exitCode = await Program.Main(args);

        // Assert
        Assert.Equal(1, exitCode);
    }

    /// <summary>
    /// AC#11: Test single argument returns exit code 1
    /// Expected: Program.Main returns 1 when only one argument is provided
    /// (neither batch mode nor valid single-file mode)
    /// </summary>
    [Fact]
    public async Task Test_SingleArgument_ReturnsExitCode1()
    {
        // Arrange - Single argument that is not --batch
        string[] args = new[] { "somefile.erb" };

        // Act
        int exitCode = await Program.Main(args);

        // Assert
        Assert.Equal(1, exitCode);
    }
}
