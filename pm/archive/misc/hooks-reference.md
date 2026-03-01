# Hooks Reference

> **Purpose:** Enforce with code instead of writing "you must do X" in documentation

---

## Basic Concept

**What are Hooks**: Shell commands automatically executed when tools run.

| Documentation | Hooks Implementation |
|--------------|---------|
| "Build after ERB modification" | Auto-build with PostToolUse |
| "CSV editing prohibited" | Block with PreToolUse |
| "Save with BOM" | Auto-convert with PostToolUse |

---

## Events

| Event | Timing |
|---------|-----------|
| **PreToolUse** | Before tool execution |
| **PostToolUse** | After tool execution |
| **Stop** | When Claude completes |
| **UserPromptSubmit** | When prompt is submitted |

**Exit Codes**: 0=success, 2=block

---

## Basic Syntax

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "your-command-here"
          }
        ]
      }
    ]
  }
}
```

**Matcher**: `"Write"` (exact match), `"Edit|Write"` (OR), `"Notebook.*"` (regex), `"*"` (all tools)

---

## Project-Specific Examples

### Auto-build after ERB write

```json
{
  "hooks": {
    "PostToolUse": [{
      "matcher": "Edit|Write",
      "hooks": [{
        "type": "command",
        "command": "powershell -NoProfile -Command \"$path = $env:CLAUDE_FILE_PATH; if ($path -match '\\.(erb|erh)$') { cd Game; dotnet build ../uEmuera/uEmuera.Headless.csproj --verbosity quiet }\""
      }]
    }]
  }
}
```

### Protected files (dangerous files)

```json
{
  "hooks": {
    "PreToolUse": [{
      "matcher": "Edit|Write",
      "hooks": [{
        "type": "command",
        "command": "powershell -NoProfile -Command \"$data = $input | ConvertFrom-Json; $path = $data.tool_input.file_path; if ($path -match '\\.(csv|sav)$' -or $path -match '\\.git') { Write-Error '[Hook] Protected'; exit 2 }\""
      }]
    }]
  }
}
```

---

## Environment Variables

| Variable | Content |
|------|------|
| `CLAUDE_FILE_PATH` | Target file path |
| `CLAUDE_PROJECT_DIR` | Project root |

---

## Reference

- [Claude Code Hooks Official Documentation](https://docs.anthropic.com/en/docs/claude-code/hooks)
