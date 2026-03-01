# Feature 733: Session JSONL Extractor Tool

## Status: [DONE]

> **Scope Discipline**: This feature has a specific scope. Do not expand or "improve" it beyond the documented acceptance criteria. Scope additions require separate features.

## Type: infra

## Background

### Philosophy (Mid-term Vision)
When uncommitted work is lost due to destructive git operations, Claude Code session JSONL files serve as the sole recovery source. This tool extracts and replays Edit/Write operations from those sessions to reconstruct the lost file state. Initially motivated by the F729 incident, the tool is designed for reusability in future recovery scenarios.

### Problem (Current Issue)
`git checkout -- .` accidentally reverted all uncommitted changes across the entire repository (~80 tracked files). Changes were never committed or staged, so git cannot recover them. Affected areas: dashboard (16+ files), .claude/ workflow config (31 files), KojoComparer (5 files), variable_sizes.yaml, design docs (26 files). However, Claude Code session files (`C:\Users\siihe\.ccs\shared\projects\C--Era-erakoumakanNTR\*.jsonl`) contain full tool_use records with file_path/old_string/new_string (Edit) and file_path/content (Write). 1,020 session files (~4.8 GB) are available for extraction.

### Goal (What to Achieve)
Create a Node.js CLI tool that parses session JSONL files and reconstructs the final state of lost files by replaying Write/Edit operations in chronological order. Output reconstructed files to a staging directory for review and application.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Successor | F734 | [DONE] | Dashboard Backend Recovery - consumes F733 output |
| Successor | F735 | [BLOCKED] | Dashboard Frontend Recovery - consumes F733 output |
| Successor | F736 | [BLOCKED] | Non-Dashboard Recovery (.claude/, KojoComparer, misc) - consumes F733 output |
| **Successor** | **F738** | **[PROPOSED]** | **Session extractor完全復元 - Read結果活用でskip率改善** |

## Links
- [feature-734.md](feature-734.md) - Dashboard Backend Recovery (successor)
- [feature-735.md](feature-735.md) - Dashboard Frontend Recovery (successor)
- [feature-736.md](feature-736.md) - Non-Dashboard Recovery (successor)
- [feature-738.md](feature-738.md) - Session Extractor Complete Recovery (fix - Read結果活用)

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Dashboard changes (16+ files) were lost and cannot be recovered via git
2. Why: `git checkout -- .` was executed, reverting all uncommitted tracked file changes
3. Why: An ac-tester subagent executed the destructive git command during F729 run
4. Why: The subagent had permission to run arbitrary git commands without safety guardrails (deny rules added post-incident)
5. Why: No automated recovery mechanism exists for reconstructing file state from Claude Code session artifacts

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| 16+ dashboard files reverted to HEAD | No tool exists to extract and replay Edit/Write operations from session JSONL files |
| Git cannot recover uncommitted changes | Session JSONL files contain complete operation history but no extraction tool exists |

### Conclusion

The root cause is the absence of a recovery tool. The session data is available and complete (1,156 Edit + 72 Write operations across 90 sessions, 46 unique file paths). The problem is purely tooling: no mechanism exists to parse JSONL, identify relevant operations, and replay them to reconstruct file state.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F734 | [DRAFT] | Successor (consumer) | Backend recovery - applies F733 output to backend files |
| F735 | [DRAFT] | Successor (consumer) | Frontend recovery - applies F733 output to frontend files |
| F736 | [DRAFT] | Successor (consumer) | Non-dashboard recovery - applies F733 output to .claude/, KojoComparer, misc |
| F729 | [WIP] | Trigger incident | The ac-tester during F729 ran `git checkout -- .` |

### Pattern Analysis

This is the first incident of its kind. Post-incident, `git checkout`, `git restore`, `git reset --hard`, and `git clean` were added to deny rules in settings.json. The recovery tool (F733) addresses the gap for when prevention fails.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Session JSONL files confirmed to exist (1,020 files, ~4.8 GB total). Edit operations contain `file_path`, `old_string`, `new_string`, `replace_all`. Write operations contain `file_path`, `content`. Data format is JSON, parseable line-by-line. |
| Scope is realistic | YES | Algorithm is straightforward: filter sessions by file path, collect Write/Edit ops, replay chronologically. Node.js `readline` module handles streaming natively (no external deps needed). |
| No blocking constraints | YES | No predecessor dependencies. Node.js v24.12.0 available. Session files are readable. |

**Verdict**: FEASIBLE

### Data Format Verification

**Session JSONL structure** (verified empirically):

Each `.jsonl` file contains one JSON object per line with these record types:

1. **tool_use records** (in assistant messages):
   ```json
   {
     "type": "tool_use",
     "name": "Edit",
     "input": {
       "file_path": "C:\\Era\\erakoumakanNTR\\tools\\feature-dashboard\\...",
       "old_string": "...",
       "new_string": "...",
       "replace_all": false
     }
   }
   ```

2. **Write tool_use records**:
   ```json
   {
     "type": "tool_use",
     "name": "Write",
     "input": {
       "file_path": "C:\\Era\\erakoumakanNTR\\tools\\feature-dashboard\\...",
       "content": "full file content..."
     }
   }
   ```

3. **tool_result records** (confirmation of success/failure):
   ```json
   {
     "type": "tool_result",
     "tool_use_id": "toolu_...",
     "content": "File created successfully at: ...",
     "is_error": false
   }
   ```

**Key observations**:
- tool_use records are nested inside `message.content[]` array of assistant-type messages
- File paths use both `C:\` (backslash) and `C:/` (forward-slash) formats - normalization required
- Line order within a session = chronological order
- Session file mtime = session temporal ordering
- Largest file: 693 MB - streaming parser mandatory (cannot load into memory)

### Verified Statistics

| Metric | Value |
|--------|-------|
| Total JSONL files | 1,020 |
| Total size | ~4.8 GB |
| Largest file | 693 MB |
| Sessions with dashboard Edit/Write | 90 |
| Dashboard Edit operations | 1,156 |
| Dashboard Write operations | 72 |
| Unique file paths | 46 |
| Average file size | ~4.9 MB |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Node.js readline | Runtime (built-in) | None | Streaming line-by-line reader, no npm dependency needed |
| Node.js fs/path | Runtime (built-in) | None | File system and path operations |

No external npm dependencies required. All functionality achievable with Node.js built-in modules.

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| F734 (Dashboard Backend Recovery) | HIGH | Reads reconstructed files from `.tmp/recovery/` to restore backend |
| F735 (Dashboard Frontend Recovery) | HIGH | Reads reconstructed files from `.tmp/recovery/` to restore frontend |
| F736 (Non-Dashboard Recovery) | HIGH | Reads reconstructed files from `.tmp/recovery/` to restore .claude/, KojoComparer, misc |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/session-extractor/index.js | Create | CLI entry point and orchestration |
| tools/session-extractor/session-discovery.js | Create | Session file scanning, filtering, and mtime-based chronological sorting |
| tools/session-extractor/jsonl-parser.js | Create | Streaming JSONL line parser with tool_result error filtering |
| tools/session-extractor/edit-replayer.js | Create | Edit operation replay (old_string -> new_string replacement) |
| tools/session-extractor/summary-reporter.js | Create | Summary report generation with confidence metrics |
| tools/session-extractor/package.json | Create | Package manifest |
| .tmp/recovery/ | Create (output) | Reconstructed files output directory |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Streaming parsing required | 693 MB max file size | HIGH - Cannot use JSON.parse on whole file |
| Path normalization | Mixed `C:\` and `C:/` in data | MEDIUM - Must normalize before comparing |
| Exact string matching for Edit | Edit replay semantics | HIGH - old_string must match exactly or operation is skipped |
| Chronological ordering | Recovery correctness | HIGH - Session mtime ordering + line order within session |
| tool_result correlation | Error detection | LOW - Can optionally check is_error on tool_result to skip failed operations |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Edit chain gap (old_string mismatch) | Medium | Medium | Log warning and skip; output confidence metric per file |
| Path format inconsistency | Confirmed | Low | Normalize all paths to forward-slash before comparison |
| Large file parsing performance | Low | Low | Node.js readline handles streaming efficiently; tested on 693 MB files |
| Incomplete recovery (some edits from non-JSONL sources) | Low | Low | Summary report shows operation count and confidence per file |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "extracts and replays Edit/Write operations from sessions" | Tool must produce reconstructed files from JSONL session data | AC#5, AC#6, AC#7, AC#20, AC#21 |
| "Claude Code session JSONL files serve as the sole recovery source" | Parser must extract both Edit and Write tool_use records | AC#4, AC#5, AC#6 |
| "replaying Write/Edit operations in chronological order" | Operations must be applied in session-mtime + line-order sequence | AC#10 |
| "Output reconstructed files to a staging directory" | Files written to .tmp/recovery/ with correct relative paths | AC#11 |
| "generates a JSON summary report" | Summary report with per-file stats (sources, operation count, confidence) | AC#12, AC#13, AC#22, AC#23 |
| "No external npm dependencies" | package.json has no dependencies/devDependencies | AC#16 |
| "Streaming parser required" | readline-based streaming (not JSON.parse on whole file) | AC#4 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | index.js exists | file | Glob(tools/session-extractor/index.js) | exists | - | [x] |
| 1a | session-discovery.js exists | file | Glob(tools/session-extractor/session-discovery.js) | exists | - | [x] |
| 1b | summary-reporter.js exists | file | Glob(tools/session-extractor/summary-reporter.js) | exists | - | [x] |
| 2 | jsonl-parser.js exists | file | Glob(tools/session-extractor/jsonl-parser.js) | exists | - | [x] |
| 3 | edit-replayer.js exists | file | Glob(tools/session-extractor/edit-replayer.js) | exists | - | [x] |
| 4 | Streaming parser uses readline | code | Grep(tools/session-extractor/jsonl-parser.js) | contains | "readline.createInterface" | [x] |
| 5 | Edit tool_use extraction | code | Grep(tools/session-extractor/jsonl-parser.js) | contains | "old_string" | [x] |
| 6 | Write tool_use detection | code | Grep(tools/session-extractor/jsonl-parser.js) | contains | "\"Write\"" | [x] |
| 7 | Write content extraction | code | Grep(tools/session-extractor/jsonl-parser.js) | contains | "input.content" | [x] |
| 8 | Edit replayer applies old_string replacement | code | Grep(tools/session-extractor/edit-replayer.js) | contains | ".replace(" | [x] |
| 9 | replace_all loop implementation | code | Grep(tools/session-extractor/edit-replayer.js) | matches | "replaceAll|replace_all|while.*indexOf" | [x] |
| 10 | Chronological session ordering | code | Grep(tools/session-extractor/session-discovery.js) | matches | "mtime|sort.*chron|\\.stat" | [x] |
| 11 | Output directory is .tmp/recovery | code | Grep(tools/session-extractor/index.js) | contains | ".tmp/recovery" | [x] |
| 12 | Summary report generation | code | Grep(tools/session-extractor/summary-reporter.js) | contains | "JSON.stringify" | [x] |
| 13 | Confidence metric implementation | code | Grep(tools/session-extractor/summary-reporter.js) | contains | "confidence" | [x] |
| 14 | Path backslash replacement | code | Grep(tools/session-extractor/jsonl-parser.js) | contains | "replace(/" | [x] |
| 15 | Path normalization | code | Grep(tools/session-extractor/jsonl-parser.js) | contains | "normalize" | [x] |
| 16 | No external npm dependencies | code | Grep(tools/session-extractor/package.json) | not_matches | "\"dependencies\"\\s*:" | [x] |
| 17 | CLI entry point is executable | code | Grep(tools/session-extractor/index.js) | matches | "#!/usr/bin/env node|process.argv" | [x] |
| 18 | package.json exists with name field | file | Glob(tools/session-extractor/package.json) | exists | - | [x] |
| 19 | Test fixture JSONL exists | file | Glob(tools/session-extractor/test-fixtures/*.jsonl) | exists | - | [x] |
| 20 | Runtime: tool exits successfully on fixture | exit_code | node tools/session-extractor/index.js tools/session-extractor/test-fixtures | succeeds | - | [x] |
| 21 | Runtime: reconstructed file matches expected | file | Grep(.tmp/recovery/) | contains | "expected test content" | [x] |
| 22 | Runtime: summary.json is valid JSON | file | Glob(.tmp/recovery/summary.json) | exists | - | [x] |
| 23 | Runtime: chain gap produces non-high confidence | file | Grep(.tmp/recovery/summary.json) | matches | "medium\|low" | [x] |
| 24 | tool_result error filtering | code | Grep(tools/session-extractor/jsonl-parser.js) | contains | "is_error" | [x] |

### AC Details

**AC#1: index.js exists**
- Verifies the CLI entry point file is created at the expected location
- Thin orchestrator that coordinates session-discovery, parsing, replay, and reporting

**AC#1a: session-discovery.js exists**
- Verifies the session file scanning and filtering module is created
- Handles session directory scanning, mtime-based sorting, and file filtering

**AC#1b: summary-reporter.js exists**
- Verifies the summary report generation module is created
- Handles per-file metadata collection, confidence calculation, and JSON output

**AC#2: jsonl-parser.js exists**
- Verifies the streaming JSONL parser module is created
- Separation of concerns: parsing logic isolated from CLI and replay

**AC#3: edit-replayer.js exists**
- Verifies the Edit operation replay module is created
- Handles the old_string -> new_string replacement logic

**AC#4: Streaming parser uses readline**
- Verifies that the parser uses Node.js built-in readline for streaming
- Critical: 693 MB largest file cannot be loaded into memory with JSON.parse
- readline.createInterface enables line-by-line processing

**AC#5: Edit tool_use extraction**
- Verifies the parser can identify and extract Edit operations from JSONL
- Edit records have name="Edit" with input containing file_path, old_string, new_string
- Must handle nested structure inside message.content[] array

**AC#6: Write tool_use detection**
- Verifies the parser can identify Write operations in JSONL by checking for "Write" in tool_use records
- Separation of concern: detect Write operations vs extract content

**AC#7: Write content extraction**
- Verifies the parser can extract content field from Write tool_use records
- Write operations provide full file snapshots (latest Write = base for subsequent Edits)

**AC#8: Edit replayer applies old_string replacement**
- Verifies exact string matching replacement is implemented
- Algorithm: find old_string in current content, replace with new_string
- If old_string not found, operation should be skipped (chain gap)

**AC#9: replace_all loop implementation**
- Verifies that replace_all: true operations loop until no more matches found
- Edge case handling for multiple occurrences of old_string in content

**AC#10: Chronological session ordering**
- Verifies session sorting by modification time (mtime) for correct operation sequence
- Critical for recovery correctness: operations must replay in chronological order

**AC#11: Output directory is .tmp/recovery**
- Verifies reconstructed files are written to the correct staging directory
- Consumer features F734/F735 expect output at this location
- Relative path structure must be preserved under .tmp/recovery/

**AC#12: Summary report generation**
- Verifies the tool produces a JSON summary report
- Report should include per-file information: source sessions, operation count, confidence
- JSON.stringify confirms structured output (not ad-hoc text)

**AC#13: Confidence metric implementation**
- Verifies confidence calculation logic exists (high/medium/low based on applied percentage)
- Important for understanding recovery completeness

**AC#14: Path backslash replacement**
- Verifies backslash-to-forward-slash normalization for Windows paths
- Required for consistent path comparison across mixed formats

**AC#15: Path normalization**
- Verifies path.normalize() usage for consistent path handling
- Complements AC#14 for complete path normalization

**AC#16: No external npm dependencies**
- Verifies package.json does not declare any external dependencies
- All functionality uses Node.js built-ins (readline, fs, path)
- "dependencies" string should not appear in package.json at all

**AC#17: CLI entry point is executable**
- Verifies index.js has shebang line or processes command-line arguments
- Tool must be runnable via `node tools/session-extractor/index.js`
- process.argv usage indicates CLI argument handling

**AC#18: package.json exists with name field**
- Verifies the package manifest file exists
- Required for Node.js module identification and potential npm scripts

**AC#19: Test fixture JSONL exists**
- Verifies a synthetic test JSONL fixture file is created for runtime verification
- Fixture contains known Write and Edit operations with predictable output

**AC#20: Runtime: tool exits successfully on fixture**
- Verifies the tool can be executed end-to-end against test fixture data
- Exit code 0 confirms no crashes during parsing, replay, or output generation

**AC#21: Runtime: reconstructed file matches expected**
- Verifies the reconstructed output file contains the expected content
- Test fixture has Write("initial") then Edit("initial" → "expected test content")
- This is the core Philosophy verification: actual reconstruction correctness

**AC#22: Runtime: summary.json is valid JSON**
- Verifies the summary report file is produced at the expected location
- Machine-readable output required by consumer features F734/F735

**AC#23: Runtime: chain gap produces non-high confidence**
- Verifies that a test fixture with an intentional chain gap (old_string not found) produces medium or low confidence
- Confirms the confidence metric correctly reflects reconstruction quality

**AC#24: tool_result error filtering**
- Verifies that the parser checks is_error field in tool_result records
- Operations with is_error: true must be skipped to prevent replaying failed operations
- Correlates tool_use records with subsequent tool_result records by tool_use_id

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The tool uses a **five-module architecture** with clear separation of concerns:

1. **Session Discovery** (session-discovery.js): Scans session directory for .jsonl files, sorts by file mtime for chronological ordering. Handles file-system-level filtering only (by directory or filename patterns).

2. **Streaming JSONL Parser** (jsonl-parser.js): Line-by-line readline-based parser that extracts tool_use records from nested message.content[] arrays without loading entire files into memory. Handles 693 MB max file size constraint. Correlates tool_result records to skip failed operations (is_error: true).

3. **Edit Replayer** (edit-replayer.js): Simple string-based replacement engine that applies Edit operations sequentially. Uses indexOf + substring operations for exact matching, skips operations where old_string not found (chain gaps).

4. **Summary Reporter** (summary-reporter.js): Collects per-file metadata (sessions, operation counts), calculates confidence metrics (high/medium/low), generates JSON summary report to .tmp/recovery/summary.json.

5. **CLI Orchestrator** (index.js): Thin entry point that coordinates session-discovery, parsing, replay, and reporting. Handles CLI arguments and output directory creation.

**Rationale**: This satisfies all 24 ACs with Node.js built-ins only (readline, fs, path), handles large files via streaming, and provides the structured output required by successor features F734/F735.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create index.js at tools/session-extractor/ as thin CLI orchestrator |
| 1a | Create session-discovery.js for session scanning, filtering, mtime sorting |
| 1b | Create summary-reporter.js for report generation and confidence calculation |
| 2 | Create jsonl-parser.js at tools/session-extractor/ with streaming extraction |
| 3 | Create edit-replayer.js at tools/session-extractor/ with replacement logic |
| 4 | Import readline module and use createInterface({ input: fs.createReadStream() }) in jsonl-parser.js |
| 5 | Check record.type === 'tool_use' && record.name === 'Edit' and extract record.input.{file_path, old_string, new_string, replace_all} |
| 6 | Check record.type === 'tool_use' && record.name === 'Write' in tool_use detection |
| 7 | Extract record.input.content from Write tool_use records |
| 8 | Use currentContent.indexOf(oldString) to find match, then substring/slice to replace with newString |
| 9 | Implement replace_all: true with while loop calling indexOf until no more matches found |
| 10 | Sort sessions by fs.stat().mtime in session-discovery.js for chronological operation replay |
| 11 | Hardcode output base path as '.tmp/recovery' in index.js, preserve relative path structure |
| 12 | Collect per-file metadata in summary-reporter.js, write to .tmp/recovery/summary.json using JSON.stringify |
| 13 | Calculate confidence metric (high/medium/low) in summary-reporter.js based on applied operation percentage |
| 14 | Implement path.replace(/\\/g, '/') for backslash-to-forward-slash conversion |
| 15 | Use path.normalize() for consistent path handling |
| 16 | package.json contains only metadata fields (name, version, type: "module"), no dependencies/devDependencies keys |
| 17 | Add #!/usr/bin/env node shebang to index.js, accept session dir path via process.argv[2] |
| 18 | Create package.json with { "name": "session-extractor", "version": "1.0.0", "type": "module" } |
| 19 | Create test-fixtures/ directory with synthetic .jsonl files: (1) happy-path.jsonl with Write+Edit ops, (2) chain-gap.jsonl with intentional old_string mismatch |
| 20 | Run `node tools/session-extractor/index.js tools/session-extractor/test-fixtures` and verify exit code 0 |
| 21 | Verify .tmp/recovery/ contains reconstructed file with "expected test content" from Write("initial") + Edit("initial" → "expected test content") |
| 22 | Verify .tmp/recovery/summary.json exists after tool execution |
| 23 | Verify summary.json contains "medium" or "low" confidence for chain-gap fixture file |
| 24 | Correlate tool_use with subsequent tool_result; skip operations where is_error is true in jsonl-parser.js |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| JSONL Parsing Strategy | A) JSON.parse per line<br>B) Custom stream parser<br>C) readline module | C) readline | Built-in module, proven for large files, satisfies AC#4 requirement |
| Edit Replay Algorithm | A) String.replace() with regex<br>B) indexOf + substring<br>C) AST-based patching | B) indexOf + substring | Exact matching semantics, simple to verify, handles replace_all with loop |
| Session Ordering | A) Filename timestamp parsing<br>B) File mtime (fs.stat)<br>C) JSONL content timestamp | B) File mtime | Reliable, matches dashboard-recovery-plan.md spec, no parsing needed |
| Path Normalization | A) path.normalize()<br>B) String.replace(/\\\\/g, '/')<br>C) path.resolve() | A+B) Both | path.normalize for consistency, explicit backslash replacement for Windows paths |
| Output Structure | A) Flat .tmp/recovery/<br>B) Preserve relative paths<br>C) Hash-based naming | B) Preserve relative | Consumer features F734/F735 expect relative paths, matches original structure |
| Error Handling for Chain Gaps | A) Throw error and abort<br>B) Log warning and skip<br>C) Apply partial match | B) Log warning and skip | Matches dashboard-recovery-plan.md spec, provides confidence metric in summary |
| Summary Report Format | A) Plain text log<br>B) JSON with per-file stats<br>C) Markdown report | B) JSON with stats | Satisfies AC#12 JSON.stringify requirement, machine-readable for future automation |

### Output Contract (For Consumer Features)

**Directory Structure**: F734/F735/F736 depend on this exact output format:
```
.tmp/recovery/
├── src/tools/node/feature-dashboard/backend/src/services/*.js  # Backend files
├── src/tools/node/feature-dashboard/frontend/src/*.js         # Frontend files
├── .claude/                                          # Workflow config
└── summary.json                                      # Metadata
```

**Summary JSON Schema**:
```json
{
  "totalFiles": 46,
  "totalOperations": 1228,
  "sessionCount": 90,
  "confidence": "high|medium|low",
  "files": {
    "path/to/file.js": {
      "operations": 25,
      "sessions": ["session1.jsonl", "session2.jsonl"],
      "confidence": "high|medium|low"
    }
  }
}
```

### Interfaces / Data Structures

**jsonl-parser.js exports**:
```javascript
/**
 * Parses a single JSONL file and extracts tool_use records
 * @param {string} sessionPath - Absolute path to .jsonl file
 * @param {Set<string>} targetPaths - Set of normalized file paths to extract (content-based filtering)
 * @returns {Promise<Operation[]>} - Array of {type, timestamp, filePath, ...params} operations
 */
export async function parseSession(sessionPath, targetPaths)

/**
 * Operation record structure
 * @typedef {Object} Operation
 * @property {'write'|'edit'} type - Operation type
 * @property {number} timestamp - Session file mtime (all ops from same session share timestamp)
 * @property {string} filePath - Normalized file path
 * @property {string} [content] - For Write: full file content
 * @property {string} [oldString] - For Edit: string to find
 * @property {string} [newString] - For Edit: string to replace with
 * @property {boolean} [replaceAll] - For Edit: replace all occurrences (default: false)
 */
```

**edit-replayer.js exports**:
```javascript
/**
 * Applies a sequence of operations to reconstruct file state
 * @param {Operation[]} operations - Operations sorted chronologically
 * @param {string|null} baseContent - Initial file content (null if file is new)
 * @returns {{content: string, stats: {appliedOps: number, skippedOps: number, warnings: string[]}}}
 */
export function replayOperations(operations, baseContent)
```

**index.js Summary Report Structure**:
```json
{
  "metadata": {
    "timestamp": "2026-02-01T12:00:00.000Z",
    "sessionCount": 90,
    "totalOperations": 1228
  },
  "files": [
    {
      "path": "src/tools/node/feature-dashboard/backend/src/services/claudeService.js",
      "sourceSessions": 15,
      "operations": {
        "write": 2,
        "edit": 87
      },
      "applied": 85,
      "skipped": 4,
      "warnings": [
        "Edit operation skipped at session 20: old_string not found (chain gap)"
      ],
      "confidence": "high"
    }
  ]
}
```

**Confidence Calculation**:
- `high`: applied >= 95% of operations
- `medium`: applied >= 75% of operations
- `low`: applied < 75% of operations

### Implementation Sequence

1. **Create package.json** (AC#18, #16): Minimal manifest with name, version, type: "module"
2. **Create session-discovery.js** (AC#1a, #10): Session scanning, filtering, mtime sorting
3. **Create jsonl-parser.js** (AC#2, #4, #5, #6, #7, #14, #15, #24): Streaming parser with readline and tool_result error filtering
4. **Create edit-replayer.js** (AC#3, #8, #9): String replacement logic with gap handling
5. **Create summary-reporter.js** (AC#1b, #12, #13): Report generation with confidence metrics
6. **Create index.js** (AC#1, #11, #17): Thin CLI orchestrator

### Edge Case Handling

| Edge Case | Behavior |
|-----------|----------|
| Empty session file | Skip (readline emits no lines) |
| Malformed JSON line | Log error, skip line, continue parsing |
| Edit old_string not found | Log warning, skip operation, decrement confidence |
| Write with empty content | Accept (valid operation, empty file) |
| No operations for target file | Skip file, do not create output |
| Mixed backslash/forward-slash paths | Normalize to forward-slash before comparison |
| replace_all: true | Loop indexOf until no more matches found |
| Concurrent Edits at same timestamp | Process in line order within session (chronological) |
| Session file mtime identical | Sort by filename lexicographically (stable sort) - Note: Not verified by AC |

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 18,16 | Create package.json with name and type: "module" fields (no dependencies) | [x] |
| 2 | 1a,10 | Create session-discovery.js with session scanning, filtering, mtime sorting | [x] |
| 3 | 2,4,5,6,7,14,15,24 | Create jsonl-parser.js with readline streaming, Edit/Write extraction, path normalization, tool_result error filtering | [x] |
| 4 | 3,8,9 | Create edit-replayer.js with old_string replacement and replace_all loop logic | [x] |
| 5 | 1b,12,13 | Create summary-reporter.js with report generation and confidence metrics | [x] |
| 5a | 1,11,17 | Create index.js thin CLI orchestrator with .tmp/recovery output | [x] |
| 6 | 19 | Create test fixture JSONL with known Write/Edit operations and chain gap scenario | [x] |
| 7 | 20,21,22,23 | Run tool against test fixtures, verify output correctness and confidence metrics | [x] |
| 8 | 1,1a,1b,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24 | Verify all ACs pass | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | package.json spec from Technical Design | package.json created |
| 2 | implementer | sonnet | T2 | session-discovery.js spec from Technical Design | session-discovery.js created |
| 3 | implementer | sonnet | T3 | jsonl-parser.js spec from Technical Design | jsonl-parser.js created |
| 4 | implementer | sonnet | T4 | edit-replayer.js spec from Technical Design | edit-replayer.js created |
| 5 | implementer | sonnet | T5 | summary-reporter.js spec from Technical Design | summary-reporter.js created |
| 5a | implementer | sonnet | T5a | index.js spec from Technical Design | index.js created |
| 6 | implementer | sonnet | T6 | Test fixture spec from Technical Design | test-fixtures/ created |
| 7 | ac-tester | haiku | T7 | Runtime ACs against test fixtures | Runtime verification results |
| 8 | ac-tester | haiku | T8 | AC table with all 24 ACs | Final verification results |

**Constraints** (from Technical Design):
1. Streaming parsing required (693 MB max file size constraint)
2. No external npm dependencies (Node.js built-ins only)
3. Path normalization required for mixed backslash/forward-slash formats
4. Exact string matching for Edit operations (skip if old_string not found)
5. Chronological ordering: session mtime + line order within session
6. Output to .tmp/recovery/ preserving relative path structure

**Pre-conditions**:
- Node.js v24.12.0 available
- tools/session-extractor/ directory exists (create if needed)
- .tmp/recovery/ directory will be created by tool if not exists

**Success Criteria**:
- All 26 ACs pass
- Tool can be invoked via `node tools/session-extractor/index.js <session-dir>`
- No build errors (no compilation step needed - pure JavaScript)
- Summary report generated at .tmp/recovery/summary.json

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation
4. If recovery tool fails, manual file reconstruction from session JSONL remains available

**Recovery Impact**:
- This tool is used for one-time recovery. If the tool has bugs, F734/F735 can be blocked until fixed
- The session JSONL data is immutable (already written), so there is no data loss risk
- Failed recovery attempts can be retried by deleting .tmp/recovery/ and re-running

## Mandatory Handoffs

<!--
Track these when detected during implementation:
- Discovered engineering debt (debt-<id>.md files)
- Related system impact (existing-analysis-<id>.md files)
- New tech requests (tech-request-<id>.md files)
- Missing documentation (doc-analysis-<id>.md files)

Format:
| Type | Destination | Task | Description |
|------|-------------|------|-------------|
-->

**None required**

---

## Review Notes

<!--
MANDATORY: Use this format for pending items:
- [pending] <phase> iter<N>: <description>

MANDATORY: Use this format for resolved items:
- [resolved-applied] <phase> iter<N>: <description>
- [resolved-invalid] <phase> iter<N>: <description>
-->

- [resolved-applied] Phase1-Critical iter4: All 18 ACs are static code pattern checks → Added runtime ACs #19-23 with test fixtures and execution verification
- [resolved-applied] Phase2-Critical iter4: No AC verifies chronological replay → AC#20,21 verify runtime execution with correct ordering via fixture
- [resolved-applied] Phase2-Critical iter4: No test fixtures or test infrastructure task → Added T6 (create fixtures) and T7 (runtime verification)
- [resolved-applied] Phase2-Major iter4: tool_result correlation → Added AC#24 (is_error filtering) and included in T3
- [resolved-applied] Phase2-Major iter4: index.js single responsibility violation → Split into session-discovery.js, summary-reporter.js, and thin index.js orchestrator. Added AC#1a, AC#1b.
- [resolved-applied] Phase3-Critical iter4: Zero runtime ACs → Added AC#19-23 for runtime verification
- [resolved-applied] Phase3-Major iter4: AC#16 pattern too broad → Changed to `"dependencies"\s*:` to match JSON key only
- [resolved-applied] Phase3-Major iter4: No task creates test fixtures → Added T6 and T7
- [resolved-applied] Phase1-Critical iter5: Zero runtime verification ACs → Added AC#19-23
- [resolved-applied] Phase1-Critical iter5: No task creates test fixtures → Added T6 and T7
- [resolved-applied] Phase1-Critical iter5: Philosophy Derivation falsely claims coverage → Updated derivation to include AC#20,21
- [resolved-applied] Phase2-Critical iter5: Zero runtime verification ACs → Added AC#19-23
- [resolved-applied] Phase2-Critical iter5: Derivation table falsely claims coverage → Updated derivation to include AC#20,21
- [resolved-applied] Phase2-Critical iter5: No task creates test fixtures → Added T6 and T7
- [resolved-applied] Phase2-Critical iter5: Philosophy scope mismatch → Narrowed Philosophy to focus on this specific incident recovery. Removed general capability claim.
- [resolved-applied] Phase3-Critical iter5: Zero runtime verification ACs → Added AC#19-23
- [resolved-applied] Phase3-Critical iter5: Derivation falsely claims coverage → Updated derivation to include AC#20,21
- [resolved-applied] Phase3-Critical iter5: No task creates test fixtures → Added T6 and T7

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-02-02 06:49 | T2 | session-discovery.js created - Session discovery with mtime-based chronological sorting |
| 2026-02-02 06:52 | T4 | edit-replayer.js created - String-based edit operation replay with replace_all support |
| 2026-02-02 06:52 | T5 | summary-reporter.js created - Summary report generation with confidence calculation |
| 2026-02-02 06:56 | DEVIATION | T7 runtime | exit 1 - parseSession received object instead of string path (index.js line 135) | Fixed: destructure session.path/session.mtime |
| 2026-02-02 06:56 | DEVIATION | T7 runtime | exit 1 - generateSummary interface mismatch (expected map, received array) | Fixed: rewrote generateSummary to accept array+counts |
| 2026-02-02 06:57 | T7 | Runtime verification passed - all 4 runtime ACs (20-23) verified |
| 2026-02-02 07:00 | DEVIATION | ac-static-verifier | exit 1 - AC#6 FAIL (escaped quote parsing) and AC#23 FAIL (pipe regex parsing) | PRE-EXISTING verifier limitation; manually verified both PASS |
| 2026-02-02 07:05 | DEVIATION | pre-commit hook | exit 1 - Era.Core.Tests build error (GrowthResult type missing in TrainingIntegrationTests.cs) | PRE-EXISTING; unrelated to F733 |
