# Feature 001: Headless Mode - Phase 3 Log

## Status: COMPLETE

Phase 3 focused on building and testing the headless mode executable.

---

## T3.1-T3.2: HeadlessWindow and HeadlessRunner

### Implementation

**HeadlessWindow.cs** - MainWindow subclass for console output:
- Inherits from MainWindow (defined in HeadlessStubs.cs for HEADLESS_MODE)
- Uses inherited `console_` field from base class
- Implements `FlushNewLines()` to output to stdout
- Implements `HandleInput()` to read from stdin

**HeadlessRunner.cs** - CLI entry point:
- `Main(string[] args)` - command-line entry point
- `Run(gamePath)` - main initialization and game loop
- `SetupDirectories()` - uses `Sys.SetWorkFolder()` and reflection for Program paths
- `RunGameLoop()` - main game loop with EmueraConsole

---

## T3.3: Create Headless .csproj

### Build Challenges Resolved

1. **Duplicate compile items (NETSDK1022)** - Added `EnableDefaultCompileItems=false`
2. **Multiple entry points (CS0017)** - Added `StartupObject` directive
3. **UnityEngine references** - Wrapped with `#if !HEADLESS_MODE`
4. **uEmuera.Media.SystemSounds** - Added stub namespace
5. **GenericUtils.CalcMd5ListForConfig** - Added MD5 methods to stub
6. **Global GenericUtils** - Added global class for ConfigData.cs

### Files Modified for Headless Compatibility

| File | Changes |
|------|---------|
| `EmueraConsole.cs` | `ClientWidth`/`ClientHeight` conditional stubs |
| `Process.cs` | `UnityEngine.Debug.Log` → `Console.Error.WriteLine` |
| `Drawing.cs` | `SetPixel`/`Save`/`texture` conditional wrappers |
| `GraphicsImage.cs` | `using UnityEngine` conditional |

### HeadlessStubs.cs Contents

Stubs provided for HEADLESS_MODE compilation:

- `uEmuera.GenericUtils` - UI stubs + MD5 calculation
- `uEmuera.SpriteManager` - Texture loading stubs
- `uEmuera.EmueraThread` - Thread management stub
- `uEmuera.Window.DebugDialog` - Debug dialog stub
- `uEmuera.Window.MainWindow` - Window base class stub
- `uEmuera.Media.SystemSounds` - Sound stubs
- `GenericUtils` (global) - For code without namespace prefix

---

## Build Result

```
ビルドに成功しました。
uEmuera.Headless -> uEmuera/bin/Debug/net8.0/uEmuera.Headless.dll
0 エラー
25 個の警告
```

---

## Commits

| Repository | Hash | Description |
|------------|------|-------------|
| uEmuera | 33694a7 | feat(headless): Phase 3 - Build headless mode successfully |

---

## Next Steps

- T3.4-T3.5: Wire input/output handling (if needed)
- Phase 4: Testing with actual ERA games

---

## Links

- [WBS-001.md](WBS-001.md) - Work breakdown structure
- [logs-001-T1.md](logs-001-T1.md) - Phase 1 log
- [logs-001-T2.md](logs-001-T2.md) - Phase 2 log
- [feature-001.md](feature-001.md) - Feature specification
