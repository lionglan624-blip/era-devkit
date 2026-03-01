using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SaveAnalyzer;

public class SaveData
{
    public SaveHeader Header { get; set; } = new();
    public Dictionary<string, Dictionary<int, long>> GlobalArrays { get; set; } = new();
    public Dictionary<string, Dictionary<int, string>> GlobalStringArrays { get; set; } = new();
    public List<CharacterData> Characters { get; set; } = new();
    public int EmueraVersion { get; set; }
    public Dictionary<string, object> ExtendedData { get; set; } = new();
}

public class SaveHeader
{
    public long GameCode { get; set; }
    public long Version { get; set; }
    public string Timestamp { get; set; } = "";
    public string GameName { get; set; } = "";
    public int CharacterCount { get; set; }
}

public class CharacterData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string CallName { get; set; } = "";
    public long IsAssi { get; set; }
    public long No { get; set; }
    public Dictionary<string, Dictionary<int, long>> Arrays { get; set; } = new();
}

public class SaveReader : IDisposable
{
    private const string FINISHER = "__FINISHED";
    private const string EMU_1808_START = "__EMUERA_1808_STRAT__";
    private const string EMU_SEPARATOR = "__EMU_SEPARATOR__";

    // Global variable names in order (Eramaker format)
    private static readonly string[] GlobalArrayNames = {
        "DAY", "MONEY", "ITEM", "FLAG", "TFLAG", "UP", "PALAMLV", "EXPLV",
        "EJAC", "DOWN", "RESULT", "COUNT", "TARGET", "ASSI", "MASTER", "NOITEM",
        "LOSEBASE", "SELECTCOM", "ASSIPLAY", "PREVCOM", "NOTUSE_14", "NOTUSE_15",
        "TIME", "ITEMSALES", "PLAYER", "NEXTCOM", "PBAND", "BOUGHT",
        "NOTUSE_1C", "NOTUSE_1D", "A", "B", "C", "D", "E", "F", "G", "H",
        "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
        "U", "V", "W", "X", "Y", "Z", "NOTUSE_38", "NOTUSE_39", "NOTUSE_3A", "NOTUSE_3B"
    };

    // Character variable names in order (Eramaker format)
    private static readonly string[] CharacterArrayNames = {
        "BASE", "MAXBASE", "ABL", "TALENT", "EXP", "MARK", "PALAM", "SOURCE",
        "EX", "CFLAG", "JUEL", "RELATION", "EQUIP", "TEQUIP", "STAIN", "GOTJUEL", "NOWEX"
    };

    private readonly StreamReader _reader;
    private int _lineNumber;

    public SaveReader(string filePath)
    {
        // ERA save files use Shift-JIS encoding
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("shift_jis");
        _reader = new StreamReader(filePath, encoding);
    }

    public SaveData Read()
    {
        var data = new SaveData();

        // Read header
        data.Header = ReadHeader();

        // Read character data (comes BEFORE global variables in save file!)
        ReadCharacters(data, data.Header.CharacterCount);

        // Read global arrays (60 integer arrays + 1 string array)
        ReadGlobalArrays(data);

        // Find and read Emuera extended section
        ReadEmueraSection(data);

        return data;
    }

    private SaveHeader ReadHeader()
    {
        var header = new SaveHeader();

        header.GameCode = ReadInt64();
        header.Version = ReadInt64();

        // Line 3: timestamp + game name (may be combined)
        var timestampLine = ReadLine();
        var parts = timestampLine.Split(new[] { ' ' }, 3);
        if (parts.Length >= 2)
        {
            header.Timestamp = $"{parts[0]} {parts[1]}";
            if (parts.Length > 2)
                header.GameName = parts[2];
        }

        header.CharacterCount = (int)ReadInt64();

        return header;
    }

    private void ReadGlobalArrays(SaveData data)
    {
        // Read 60 integer arrays
        for (int i = 0; i < GlobalArrayNames.Length; i++)
        {
            var arrayData = ReadInt64ArraySparse();
            if (arrayData.Count > 0)
            {
                data.GlobalArrays[GlobalArrayNames[i]] = arrayData;
            }
        }

        // Read 1 string array (SAVESTR)
        var strArray = ReadStringArraySparse();
        if (strArray.Count > 0)
        {
            data.GlobalStringArrays["SAVESTR"] = strArray;
        }
    }

    private void ReadCharacters(SaveData data, int count)
    {
        for (int charIndex = 0; charIndex < count; charIndex++)
        {
            var character = new CharacterData { Id = charIndex };

            // Read 2 strings: NAME, CALLNAME
            character.Name = ReadLine();
            character.CallName = ReadLine();

            // Read 2 integers: ISASSI, NO
            character.IsAssi = ReadInt64();
            character.No = ReadInt64();

            // Read 17 integer arrays
            for (int i = 0; i < CharacterArrayNames.Length; i++)
            {
                var arrayData = ReadInt64ArraySparse();
                if (arrayData.Count > 0)
                {
                    character.Arrays[CharacterArrayNames[i]] = arrayData;
                }
            }

            // Read 0 string arrays (none in standard format)

            data.Characters.Add(character);
        }
    }

    private void ReadEmueraSection(SaveData data)
    {
        // Read until we find Emuera section or EOF
        while (!_reader.EndOfStream)
        {
            var line = ReadLine();

            if (line == EMU_1808_START)
            {
                data.EmueraVersion = 1808;
                ReadExtendedSection(data);
                break;
            }

            if (line.StartsWith("__EMUERA_"))
            {
                // Other Emuera versions
                if (line.Contains("1700"))
                    data.EmueraVersion = 1700;
                else if (line.Contains("1708"))
                    data.EmueraVersion = 1708;
                else if (line.Contains("1729"))
                    data.EmueraVersion = 1729;
                else if (line.Contains("1803"))
                    data.EmueraVersion = 1803;
                break;
            }
        }
    }

    private void ReadExtendedSection(SaveData data)
    {
        // Read extended format data
        while (!_reader.EndOfStream)
        {
            var line = ReadLine();

            if (line == EMU_SEPARATOR)
                continue;

            if (line == FINISHER)
                continue;

            // Try to parse key:value pairs
            var colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                var key = line.Substring(0, colonIndex);
                var value = line.Substring(colonIndex + 1);

                if (!data.ExtendedData.ContainsKey(key))
                {
                    if (long.TryParse(value, out var intValue))
                        data.ExtendedData[key] = intValue;
                    else
                        data.ExtendedData[key] = value;
                }
            }
        }
    }

    private string ReadLine()
    {
        _lineNumber++;
        return _reader.ReadLine() ?? "";
    }

    private long ReadInt64()
    {
        var line = ReadLine();
        if (long.TryParse(line, out var value))
            return value;
        return 0;
    }

    private Dictionary<int, long> ReadInt64ArraySparse()
    {
        var result = new Dictionary<int, long>();
        int index = 0;

        while (!_reader.EndOfStream)
        {
            var line = ReadLine();
            if (line == FINISHER)
                break;
            if (long.TryParse(line, out var value) && value != 0)
            {
                result[index] = value;
            }
            index++;
        }
        return result;
    }

    private Dictionary<int, string> ReadStringArraySparse()
    {
        var result = new Dictionary<int, string>();
        int index = 0;

        while (!_reader.EndOfStream)
        {
            var line = ReadLine();
            if (line == FINISHER)
                break;
            if (!string.IsNullOrEmpty(line))
            {
                result[index] = line;
            }
            index++;
        }
        return result;
    }

    public void Dispose()
    {
        _reader?.Dispose();
    }
}
