using System.Text;
using Xunit;

namespace SaveAnalyzer.Tests;

public class SaveReaderTests
{
    private static string CreateMinimalSaveFile()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_save_{Guid.NewGuid()}.sav");

        // ERA save files use Shift-JIS encoding
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("shift_jis");

        using (var writer = new StreamWriter(tempPath, false, encoding))
        {
            // Header
            writer.WriteLine("1234567890"); // GameCode
            writer.WriteLine("1"); // Version
            writer.WriteLine("2024-01-01 12:00:00 TestGame"); // Timestamp + GameName
            writer.WriteLine("1"); // CharacterCount = 1

            // Character 0
            writer.WriteLine("テスト"); // Name
            writer.WriteLine("テスト"); // CallName
            writer.WriteLine("0"); // IsAssi
            writer.WriteLine("0"); // No

            // Character arrays (17 arrays: BASE, MAXBASE, ABL, TALENT, EXP, MARK, PALAM, SOURCE, EX, CFLAG, JUEL, RELATION, EQUIP, TEQUIP, STAIN, GOTJUEL, NOWEX)
            for (int i = 0; i < 17; i++)
            {
                // Minimal array: just write FINISHER
                writer.WriteLine("__FINISHED");
            }

            // Global arrays (60 integer arrays)
            for (int i = 0; i < 60; i++)
            {
                writer.WriteLine("__FINISHED");
            }

            // Global string array (1 array: SAVESTR)
            writer.WriteLine("__FINISHED");
        }

        return tempPath;
    }

    private static string CreateSaveFileWithData()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_save_data_{Guid.NewGuid()}.sav");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("shift_jis");

        using (var writer = new StreamWriter(tempPath, false, encoding))
        {
            // Header
            writer.WriteLine("9876543210"); // GameCode
            writer.WriteLine("100"); // Version
            writer.WriteLine("2024-12-25 15:30:45 EraGame"); // Timestamp + GameName
            writer.WriteLine("2"); // CharacterCount = 2

            // Character 0
            writer.WriteLine("咲夜"); // Name
            writer.WriteLine("咲夜さん"); // CallName
            writer.WriteLine("1"); // IsAssi
            writer.WriteLine("5"); // No

            // Character arrays - BASE with some values
            writer.WriteLine("100"); // BASE[0]
            writer.WriteLine("200"); // BASE[1]
            writer.WriteLine("__FINISHED");

            // Remaining 16 character arrays (MAXBASE through NOWEX)
            for (int i = 0; i < 16; i++)
            {
                writer.WriteLine("__FINISHED");
            }

            // Character 1
            writer.WriteLine("美鈴"); // Name
            writer.WriteLine("美鈴"); // CallName
            writer.WriteLine("0"); // IsAssi
            writer.WriteLine("10"); // No

            // Character arrays (17 arrays)
            for (int i = 0; i < 17; i++)
            {
                writer.WriteLine("__FINISHED");
            }

            // Global arrays - FLAG with some values
            // First is DAY
            writer.WriteLine("__FINISHED");
            // Second is MONEY
            writer.WriteLine("__FINISHED");
            // Third is ITEM
            writer.WriteLine("__FINISHED");
            // Fourth is FLAG
            writer.WriteLine("0"); // FLAG[0]
            writer.WriteLine("1"); // FLAG[1]
            writer.WriteLine("0"); // FLAG[2]
            writer.WriteLine("0"); // FLAG[3]
            writer.WriteLine("5"); // FLAG[4]
            writer.WriteLine("__FINISHED");

            // Remaining global arrays (56 more)
            for (int i = 0; i < 56; i++)
            {
                writer.WriteLine("__FINISHED");
            }

            // Global string array (SAVESTR)
            writer.WriteLine("test string"); // SAVESTR[0]
            writer.WriteLine("__FINISHED");
        }

        return tempPath;
    }

    [Fact]
    public void SaveReader_ReadMinimalFile_Success()
    {
        // Arrange
        var filePath = CreateMinimalSaveFile();

        try
        {
            // Act
            using var reader = new SaveReader(filePath);
            var data = reader.Read();

            // Assert
            Assert.NotNull(data);
            Assert.NotNull(data.Header);
            Assert.Equal(1234567890, data.Header.GameCode);
            Assert.Equal(1, data.Header.Version);
            Assert.Equal("2024-01-01 12:00:00", data.Header.Timestamp);
            Assert.Equal("TestGame", data.Header.GameName);
            Assert.Equal(1, data.Header.CharacterCount);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveReader_ReadCharacters_Success()
    {
        // Arrange
        var filePath = CreateMinimalSaveFile();

        try
        {
            // Act
            using var reader = new SaveReader(filePath);
            var data = reader.Read();

            // Assert
            Assert.NotNull(data.Characters);
            Assert.Single(data.Characters);
            Assert.Equal(0, data.Characters[0].Id);
            Assert.Equal("テスト", data.Characters[0].Name);
            Assert.Equal("テスト", data.Characters[0].CallName);
            Assert.Equal(0, data.Characters[0].IsAssi);
            Assert.Equal(0, data.Characters[0].No);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveReader_ReadFileWithData_ParsesCorrectly()
    {
        // Arrange
        var filePath = CreateSaveFileWithData();

        try
        {
            // Act
            using var reader = new SaveReader(filePath);
            var data = reader.Read();

            // Assert - Header
            Assert.Equal(9876543210, data.Header.GameCode);
            Assert.Equal(100, data.Header.Version);
            Assert.Equal("2024-12-25 15:30:45", data.Header.Timestamp);
            Assert.Equal("EraGame", data.Header.GameName);
            Assert.Equal(2, data.Header.CharacterCount);

            // Assert - Characters
            Assert.Equal(2, data.Characters.Count);
            Assert.Equal("咲夜", data.Characters[0].Name);
            Assert.Equal("咲夜さん", data.Characters[0].CallName);
            Assert.Equal(1, data.Characters[0].IsAssi);
            Assert.Equal(5, data.Characters[0].No);

            // Character arrays
            Assert.True(data.Characters[0].Arrays.ContainsKey("BASE"));
            Assert.Equal(100, data.Characters[0].Arrays["BASE"][0]);
            Assert.Equal(200, data.Characters[0].Arrays["BASE"][1]);

            Assert.Equal("美鈴", data.Characters[1].Name);
            Assert.Equal(0, data.Characters[1].IsAssi);
            Assert.Equal(10, data.Characters[1].No);

            // Assert - Global arrays
            Assert.True(data.GlobalArrays.ContainsKey("FLAG"));
            Assert.Equal(1, data.GlobalArrays["FLAG"][1]);
            Assert.Equal(5, data.GlobalArrays["FLAG"][4]);

            // Assert - Global string arrays
            Assert.True(data.GlobalStringArrays.ContainsKey("SAVESTR"));
            Assert.Equal("test string", data.GlobalStringArrays["SAVESTR"][0]);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveReader_DisposesCorrectly()
    {
        // Arrange
        var filePath = CreateMinimalSaveFile();

        try
        {
            // Act
            SaveData data;
            using (var reader = new SaveReader(filePath))
            {
                data = reader.Read();
            }

            // Assert - should not throw
            Assert.NotNull(data);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveReader_NonExistentFile_ThrowsException()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), "nonexistent_save.sav");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => new SaveReader(filePath));
    }

    [Fact]
    public void SaveReader_HeaderWithoutGameName_ParsesCorrectly()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_save_no_gamename_{Guid.NewGuid()}.sav");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("shift_jis");

        using (var writer = new StreamWriter(tempPath, false, encoding))
        {
            writer.WriteLine("1234567890");
            writer.WriteLine("1");
            writer.WriteLine("2024-01-01 12:00:00"); // No game name
            writer.WriteLine("0"); // No characters

            // Global arrays (60)
            for (int i = 0; i < 60; i++)
            {
                writer.WriteLine("__FINISHED");
            }

            // Global string array (1)
            writer.WriteLine("__FINISHED");
        }

        try
        {
            // Act
            using var reader = new SaveReader(tempPath);
            var data = reader.Read();

            // Assert
            Assert.Equal("2024-01-01 12:00:00", data.Header.Timestamp);
            Assert.Equal("", data.Header.GameName);
            Assert.Equal(0, data.Header.CharacterCount);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public void SaveReader_SparseArrays_OnlyStoresNonZeroValues()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_save_sparse_{Guid.NewGuid()}.sav");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("shift_jis");

        using (var writer = new StreamWriter(tempPath, false, encoding))
        {
            writer.WriteLine("1");
            writer.WriteLine("1");
            writer.WriteLine("2024-01-01 12:00:00");
            writer.WriteLine("1"); // 1 character

            // Character
            writer.WriteLine("Test");
            writer.WriteLine("Test");
            writer.WriteLine("0");
            writer.WriteLine("0");

            // Character arrays (17) - BASE with sparse values
            writer.WriteLine("0"); // [0] = 0, should not be stored
            writer.WriteLine("100"); // [1] = 100, should be stored
            writer.WriteLine("0"); // [2] = 0, should not be stored
            writer.WriteLine("200"); // [3] = 200, should be stored
            writer.WriteLine("__FINISHED");

            // Remaining character arrays
            for (int i = 0; i < 16; i++)
            {
                writer.WriteLine("__FINISHED");
            }

            // Global arrays (60)
            for (int i = 0; i < 60; i++)
            {
                writer.WriteLine("__FINISHED");
            }

            // Global string array
            writer.WriteLine("__FINISHED");
        }

        try
        {
            // Act
            using var reader = new SaveReader(tempPath);
            var data = reader.Read();

            // Assert
            var baseArray = data.Characters[0].Arrays["BASE"];
            Assert.Equal(2, baseArray.Count); // Only non-zero values
            Assert.False(baseArray.ContainsKey(0)); // Zero value not stored
            Assert.True(baseArray.ContainsKey(1));
            Assert.Equal(100, baseArray[1]);
            Assert.False(baseArray.ContainsKey(2)); // Zero value not stored
            Assert.True(baseArray.ContainsKey(3));
            Assert.Equal(200, baseArray[3]);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
