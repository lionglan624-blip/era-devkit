# SaveAnalyzer

Analyzes Emuera save files and outputs structured data in JSON format.

## Usage

```bash
# Full save analysis
dotnet run --project tools/SaveAnalyzer/SaveAnalyzer.csproj -- sav/save001.sav

# Header only
dotnet run --project tools/SaveAnalyzer/SaveAnalyzer.csproj -- sav/save001.sav --header

# Filter by variable
dotnet run --project tools/SaveAnalyzer/SaveAnalyzer.csproj -- sav/save001.sav -f FLAG

# Filter by character
dotnet run --project tools/SaveAnalyzer/SaveAnalyzer.csproj -- sav/save001.sav -c "0"
```

## Purpose

Extracts and displays save file data for debugging and analysis. Outputs structured JSON for game state inspection, variable tracking, and bug investigation.

## Key Features

- JSON output with UTF-8 encoding support
- Header-only mode for quick metadata inspection
- Variable filtering (`-f`, `--filter`)
- Character filtering (`-c`, `--character`)

## Key Components

- `SaveReader.cs` - Binary save file parser
- `Program.cs` - CLI interface

## Exit Codes

- 0: Analysis succeeded
- 1: File not found or parse error
