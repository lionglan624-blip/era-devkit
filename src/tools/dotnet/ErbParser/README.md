# ErbParser

Parses ERB source files into an Abstract Syntax Tree (AST) for analysis and migration.

## Usage

```bash
# Use as a library in C# code
var parser = new ErbParser.ErbParser();
var ast = parser.Parse("path/to/file.erb");
```

## Purpose

ErbParser converts ERB script files into a structured AST representation, enabling programmatic analysis and transformation. This is the foundation for tools like ErbToYaml that need to understand ERB code structure.

## Key Components

- `ErbParser.cs` - Main parser class
- `Ast/` - AST node definitions (FunctionDefNode, DatalistNode, etc.)

## Usage in Project

Used by ErbToYaml and KojoComparer for ERB code analysis.
