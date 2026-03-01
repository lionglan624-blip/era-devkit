# LSP Daemon — C# Semantic Operations

Zero-token alternative to Serena MCP. HTTP daemon wrapping Serena's Python API (Roslyn LSP).

## Architecture

```
Bash("python tools/lsp.py find NtrEngine --path src/Era.Core/ --body")
  → HTTP POST http://127.0.0.1:19999
    → lsp-daemon.py (SerenaAgent + Roslyn LSP resident)
      → tool.apply(**args) → json.loads → clean JSON
    ← HTTP Response
  ← stdout
```

## PM2 Management

```bash
pm2 start ecosystem.config.js   # Start daemon
pm2 stop lsp-daemon              # Stop
pm2 restart lsp-daemon           # Restart (LSP re-index ~3s)
pm2 logs lsp-daemon              # View logs
pm2 logs lsp-daemon --lines 50   # Last 50 lines
pm2 status                       # All processes
dr                               # 全プロセス再起動 (dashboard + lsp-daemon)
```

- **autorestart**: Crash 時に自動復帰 (max 3回, 5s delay)
- **treekill**: 停止時に子プロセス (Python/Roslyn LS) も終了
- **Logs**: `.tmp/lsp-daemon-{out,error}.log`
- **Launcher**: `tools/lsp-daemon-launcher.cjs` (Windows の PM2 は Python を直接起動できないため Node.js 経由)

## CLI Usage

```bash
python tools/lsp.py <command> [args]
```

### Commands

| Command | Description | Example |
|---------|-------------|---------|
| `status` | Health check | `python tools/lsp.py status` |
| `symbols` | File symbol overview | `python tools/lsp.py symbols src/Era.Core/NtrEngine.cs --depth 1` |
| `find` | Search symbols by name | `python tools/lsp.py find NtrEngine --path src/Era.Core/ --body` |
| `refs` | Find references | `python tools/lsp.py refs Calculate --path src/Era.Core/NtrEngine.cs` |
| `rename` | Rename across codebase | `python tools/lsp.py rename OldName NewName --path File.cs` |
| `replace` | Replace symbol body | `python tools/lsp.py replace Class/Method --path File.cs --body "code"` |
| `insert-before` | Insert before symbol | `python tools/lsp.py insert-before Class --path File.cs --body "code"` |
| `insert-after` | Insert after symbol | `python tools/lsp.py insert-after Class --path File.cs --body "code"` |
| `restart` | Restart language server | `python tools/lsp.py restart` |

### Common Options

- `--path PATH` — Restrict search to file or directory. **Always specify** for faster results.
- `--depth N` — Child symbol depth (0=self only, 1=immediate children). Default: 0.
- `--body` — Include source code in output (find only; flag, not value).

### Name Path Patterns

Serena uses "name paths" — symbol hierarchy within a file:

```
ClassName                    → matches any symbol named ClassName
ClassName/MethodName         → method within class (relative path)
Namespace/ClassName/Method   → fully qualified within file
```

## Known Issues & Troubleshooting

### MSYS Path Conversion (Git Bash)

**Problem**: Git Bash converts `/SymbolName` (leading slash = absolute name path) to `C:/Program Files/Git/SymbolName`.

**Fix**: Use relative or fully qualified name paths without leading `/`:
```bash
# BAD — MSYS converts to Windows path
python tools/lsp.py refs /CharacterId --path File.cs

# GOOD — relative name path
python tools/lsp.py refs CharacterId --path File.cs

# GOOD — fully qualified (no leading /)
python tools/lsp.py refs Era.Core.Types/CharacterId --path File.cs
```

### Ambiguous Symbol Name

**Error**: `Found multiple N symbols matching 'Name'`

**Fix**: Use a more specific name path:
```bash
# BAD — matches struct AND constructor
python tools/lsp.py refs CharacterId --path src/Era.Core/Types/CharacterId.cs

# GOOD — qualify with parent
python tools/lsp.py refs Era.Core.Types/CharacterId --path src/Era.Core/Types/CharacterId.cs
```

### Result Too Long

**Error**: `The answer is too long (N characters)`

**Fix**: Narrow the search with `--path`:
```bash
# BAD — CharacterId is used everywhere
python tools/lsp.py refs Era.Core.Types/CharacterId --path src/Era.Core/Types/CharacterId.cs

# GOOD — search specific method's references instead
python tools/lsp.py refs NtrEngine/Calculate --path src/Era.Core/NtrEngine.cs
```

### Daemon Not Running

**Error**: `Cannot connect to daemon at http://127.0.0.1:19999`

**Fix**:
```bash
pm2 status                       # Check if lsp-daemon is online
pm2 start ecosystem.config.js    # Start if not running
pm2 logs lsp-daemon --lines 20   # Check for startup errors
```

### File Change Detection

Roslyn LSP は**自前の file watcher** でディスク変更を非同期検出する。
Claude Code の Edit/Write ツールで `.cs` ファイルを変更しても、数秒以内に自動反映される。
**通常 restart は不要。**

`restart` が必要なケース:
- `.csproj` / `.sln` の変更（プロジェクト構成変更）
- LSP がハングした場合
```bash
python tools/lsp.py restart      # Restart language server (re-indexes ~3s)
```

## Performance

| Operation | Latency | Notes |
|-----------|---------|-------|
| `status` | ~220ms | Python startup overhead |
| `symbols` | ~230ms | Cached after first call |
| `find` (single file) | ~225ms | With body included |
| `find` (project-wide) | ~630ms | src/Era.Core/ scope |
| `refs` | 0.7–1.4s | Depends on result count |
| LSP startup | ~3s | Roslyn indexing (cached) |

## Token Cost

| Approach | Fixed/Conv | Per Call | 5 Calls |
|----------|-----------|----------|---------|
| Serena MCP | 17,000 | ~300 | **18,500** |
| **CLI Daemon** | **0** | **~350** | **1,750** |

## Files

| File | Purpose |
|------|---------|
| `tools/lsp-daemon.py` | HTTP server (SerenaAgent wrapper) |
| `tools/lsp.py` | CLI client (stdlib only, no deps) |
| `tools/lsp-daemon-launcher.cjs` | PM2 Node.js launcher |
| `ecosystem.config.js` | PM2 ecosystem config |
| `fd-restart.cmd` | **全プロセス再起動** (dashboard + lsp-daemon) — `dr` コマンドから呼ばれる |
| `fd-lsp-start.cmd` | Manual start (foreground) |
| `fd-lsp-stop.cmd` | Manual stop (taskkill) |
