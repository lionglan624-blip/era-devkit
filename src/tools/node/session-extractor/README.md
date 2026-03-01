# Session Extractor

Extract user prompts from Claude Code session files for analysis.

## prompt-extractor.cjs

Extracts user prompts from session JSONL files to understand "what was requested" during dashboard/handoff development.

### Usage

```bash
node tools/session-extractor/prompt-extractor.cjs
```

### Input

Session JSONL files at `C:\Users\siihe\.claude\projects\C--Era-devkit\*.jsonl`

### Output

Markdown file at `.tmp/user-prompts-dashboard.md` with:
- Chronological list of user prompts
- Filtered for dashboard/handoff relevance
- Prompts truncated to 200 chars for readability

### Filtering Criteria

**Genuine User Prompts:**
- `type === "user"`
- `userType === "external"`
- `isMeta !== true`
- `message.content` does NOT start with `[` (excludes tool results)

**Relevance:**
- Session contains "dashboard", "handoff", "HANDOFF", or "feature-dashboard" anywhere

### Output Format

```markdown
# User Prompts from Dashboard/Handoff Sessions

## Session: {session-uuid}
Summary: {summary if available}

| Timestamp | Prompt (truncated to 200 chars) |
|-----------|--------------------------------|
| 2026-01-28 15:30 | Dashboard起動時にWebSocketエラーが出る... |
```

### Implementation Notes

- Uses streaming (readline) to handle large files efficiently
- Handles content as both string and array format
- Sorts sessions by timestamp (oldest first)
- Escapes pipe characters in output for markdown table compatibility
