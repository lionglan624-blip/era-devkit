using System.Reflection;
using System.Text;
using Xunit;

namespace SaveAnalyzer.Tests;

[Trait("Category", "Unit")]
public class MainTests
{
    // Invoke the private static Main method via reflection
    private static int InvokeMain(string[] args)
    {
        var method = typeof(Program).GetMethod(
            "Main",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
            null,
            new[] { typeof(string[]) },
            null)
            ?? throw new InvalidOperationException("Could not find Main method on Program");

        var result = method.Invoke(null, new object[] { args });
        return (int)(result ?? throw new InvalidOperationException("Main returned null"));
    }

    private static string CreateMinimalSaveFile()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"main_test_save_{Guid.NewGuid()}.sav");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("shift_jis");

        using (var writer = new StreamWriter(tempPath, false, encoding))
        {
            writer.WriteLine("1234567890");
            writer.WriteLine("1");
            writer.WriteLine("2024-01-01 12:00:00 TestGame");
            writer.WriteLine("0");

            for (int i = 0; i < 60; i++)
                writer.WriteLine("__FINISHED");

            writer.WriteLine("__FINISHED");
        }

        return tempPath;
    }

    private static string CreateSaveFileWithCharacterAndFlags()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"main_test_save_char_{Guid.NewGuid()}.sav");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("shift_jis");

        using (var writer = new StreamWriter(tempPath, false, encoding))
        {
            writer.WriteLine("1234567890");
            writer.WriteLine("1");
            writer.WriteLine("2024-01-01 12:00:00 TestGame");
            writer.WriteLine("2");

            // Character 0: Sakuya (U+548C U+6642 U+591C)
            writer.WriteLine("\u548c\u6642\u591c");
            writer.WriteLine("\u548c\u6642\u591c");
            writer.WriteLine("0");
            writer.WriteLine("0");
            // 9 empty arrays before CFLAG (CFLAG is index 9 in CharacterArrayNames)
            for (int i = 0; i < 9; i++)
                writer.WriteLine("__FINISHED");
            // CFLAG array: index 0 = 0 (skipped), index 1 = 5
            writer.WriteLine("0");
            writer.WriteLine("5");
            writer.WriteLine("__FINISHED");
            // remaining 7 character arrays
            for (int i = 0; i < 7; i++)
                writer.WriteLine("__FINISHED");

            // Character 1: Meiling (U+7F8E U+9234)
            writer.WriteLine("\u7f8e\u9234");
            writer.WriteLine("\u7f8e\u9234");
            writer.WriteLine("0");
            writer.WriteLine("0");
            for (int i = 0; i < 17; i++)
                writer.WriteLine("__FINISHED");

            // Global arrays: DAY, MONEY, ITEM empty; FLAG has value 42 at index 1
            writer.WriteLine("__FINISHED"); // DAY
            writer.WriteLine("__FINISHED"); // MONEY
            writer.WriteLine("__FINISHED"); // ITEM
            // FLAG: index 0 = 0 (skipped), index 1 = 42
            writer.WriteLine("0");
            writer.WriteLine("42");
            writer.WriteLine("__FINISHED");
            // remaining 56 global arrays
            for (int i = 0; i < 56; i++)
                writer.WriteLine("__FINISHED");

            // Global string array (SAVESTR)
            writer.WriteLine("__FINISHED");
        }

        return tempPath;
    }

    private static string CaptureStdout(Action action)
    {
        var originalOut = Console.Out;
        using var sb = new StringWriter();
        Console.SetOut(sb);
        try
        {
            action();
        }
        finally
        {
            Console.SetOut(originalOut);
        }
        return sb.ToString();
    }

    private static string CaptureStderr(Action action)
    {
        var originalErr = Console.Error;
        using var sb = new StringWriter();
        Console.SetError(sb);
        try
        {
            action();
        }
        finally
        {
            Console.SetError(originalErr);
        }
        return sb.ToString();
    }

    [Fact]
    public void Main_NoArgs_PrintsUsageAndReturnsOne()
    {
        int exitCode = 0;
        var stdout = CaptureStdout(() =>
        {
            exitCode = InvokeMain([]);
        });

        Assert.Equal(1, exitCode);
        Assert.Contains("SaveAnalyzer", stdout);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Main_HelpFlagLong_PrintsUsageAndReturnsZero()
    {
        int exitCode = 0;
        var stdout = CaptureStdout(() =>
        {
            exitCode = InvokeMain(["--help"]);
        });

        Assert.Equal(0, exitCode);
        Assert.Contains("SaveAnalyzer", stdout);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Main_HelpFlagShort_PrintsUsageAndReturnsZero()
    {
        int exitCode = 0;
        var stdout = CaptureStdout(() =>
        {
            exitCode = InvokeMain(["-h"]);
        });

        Assert.Equal(0, exitCode);
        Assert.Contains("SaveAnalyzer", stdout);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Main_FilterFlagOnlyNoFilePath_WritesErrorAndReturnsOne()
    {
        int exitCode = 0;
        var stderr = CaptureStderr(() =>
        {
            exitCode = InvokeMain(["--filter", "FLAG"]);
        });

        Assert.Equal(1, exitCode);
        Assert.Contains("No save file specified", stderr);
    }

    [Fact]
    public void Main_CharacterFlagOnlyNoFilePath_WritesErrorAndReturnsOne()
    {
        int exitCode = 0;
        var stderr = CaptureStderr(() =>
        {
            exitCode = InvokeMain(["--character", "test"]);
        });

        Assert.Equal(1, exitCode);
        Assert.Contains("No save file specified", stderr);
    }

    [Fact]
    public void Main_HeaderFlagOnlyNoFilePath_WritesErrorAndReturnsOne()
    {
        int exitCode = 0;
        var stderr = CaptureStderr(() =>
        {
            exitCode = InvokeMain(["--header"]);
        });

        Assert.Equal(1, exitCode);
        Assert.Contains("No save file specified", stderr);
    }

    [Fact]
    public void Main_NonExistentFile_WritesErrorAndReturnsOne()
    {
        var fakeFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.sav");
        int exitCode = 0;

        var stderr = CaptureStderr(() =>
        {
            exitCode = InvokeMain([fakeFile]);
        });

        Assert.Equal(1, exitCode);
        Assert.Contains("File not found", stderr);
        Assert.Contains(fakeFile, stderr);
    }

    [Fact]
    public void Main_ValidFile_OutputsJsonAndReturnsZero()
    {
        var filePath = CreateMinimalSaveFile();

        try
        {
            int exitCode = 0;
            var stdout = CaptureStdout(() =>
            {
                exitCode = InvokeMain([filePath]);
            });

            Assert.Equal(0, exitCode);
            Assert.Contains("\"file\"", stdout);
            Assert.Contains("\"header\"", stdout);
            Assert.Contains("TestGame", stdout);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void Main_HeaderFlag_OutputsHeaderOnlyWithoutGlobalsOrCharacters()
    {
        var filePath = CreateMinimalSaveFile();

        try
        {
            int exitCode = 0;
            var stdout = CaptureStdout(() =>
            {
                exitCode = InvokeMain(["--header", filePath]);
            });

            Assert.Equal(0, exitCode);
            Assert.Contains("\"header\"", stdout);
            Assert.DoesNotContain("\"globals\"", stdout);
            Assert.DoesNotContain("\"characters\"", stdout);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void Main_FilterFlagLong_OutputsFilteredGlobals()
    {
        var filePath = CreateSaveFileWithCharacterAndFlags();

        try
        {
            int exitCode = 0;
            var stdout = CaptureStdout(() =>
            {
                exitCode = InvokeMain(["--filter", "FLAG", filePath]);
            });

            Assert.Equal(0, exitCode);
            Assert.Contains("FLAG", stdout);
            Assert.Contains("42", stdout);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void Main_FilterFlagShort_OutputsFilteredGlobals()
    {
        var filePath = CreateSaveFileWithCharacterAndFlags();

        try
        {
            int exitCode = 0;
            var stdout = CaptureStdout(() =>
            {
                exitCode = InvokeMain(["-f", "FLAG", filePath]);
            });

            Assert.Equal(0, exitCode);
            Assert.Contains("FLAG", stdout);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void Main_CharacterFlagLong_ShowsMatchingCharacterOnly()
    {
        var filePath = CreateSaveFileWithCharacterAndFlags();

        try
        {
            var sakuyaName = "\u548c\u6642\u591c";
            var meilingName = "\u7f8e\u9234";
            int exitCode = 0;

            var stdout = CaptureStdout(() =>
            {
                exitCode = InvokeMain(["--character", sakuyaName, filePath]);
            });

            Assert.Equal(0, exitCode);
            Assert.Contains(sakuyaName, stdout);
            Assert.DoesNotContain(meilingName, stdout);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void Main_CharacterFlagShort_ShowsMatchingCharacter()
    {
        var filePath = CreateSaveFileWithCharacterAndFlags();

        try
        {
            var sakuyaName = "\u548c\u6642\u591c";
            int exitCode = 0;

            var stdout = CaptureStdout(() =>
            {
                exitCode = InvokeMain(["-c", sakuyaName, filePath]);
            });

            Assert.Equal(0, exitCode);
            Assert.Contains(sakuyaName, stdout);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void Main_CharacterFilterOnly_SuppressesGlobals()
    {
        var filePath = CreateSaveFileWithCharacterAndFlags();

        try
        {
            var sakuyaName = "\u548c\u6642\u591c";
            int exitCode = 0;

            var stdout = CaptureStdout(() =>
            {
                exitCode = InvokeMain(["-c", sakuyaName, filePath]);
            });

            Assert.Equal(0, exitCode);
            Assert.DoesNotContain("\"globals\"", stdout);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void Main_CharacterAndVariableFilterCombined_ShowsFilteredCharacterAndGlobals()
    {
        var filePath = CreateSaveFileWithCharacterAndFlags();

        try
        {
            var sakuyaName = "\u548c\u6642\u591c";
            int exitCode = 0;

            var stdout = CaptureStdout(() =>
            {
                exitCode = InvokeMain(["-c", sakuyaName, "-f", "CFLAG", filePath]);
            });

            Assert.Equal(0, exitCode);
            Assert.Contains(sakuyaName, stdout);
            Assert.Contains("CFLAG", stdout);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void Main_FilterFlagIndexed_OutputsSingleIndexedValue()
    {
        var filePath = CreateSaveFileWithCharacterAndFlags();

        try
        {
            int exitCode = 0;
            var stdout = CaptureStdout(() =>
            {
                exitCode = InvokeMain(["--filter", "FLAG:1", filePath]);
            });

            Assert.Equal(0, exitCode);
            Assert.Contains("42", stdout);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void Main_UnknownDashFlag_TreatedAsMissingFilePath()
    {
        int exitCode = 0;
        var stderr = CaptureStderr(() =>
        {
            exitCode = InvokeMain(["--unknown-option"]);
        });

        Assert.Equal(1, exitCode);
        Assert.Contains("No save file specified", stderr);
    }
}
