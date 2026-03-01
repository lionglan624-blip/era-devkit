# Log: Feature 001 Phase 2 - EmueraConsole Refactoring

**Date**: 2024-12-09
**Tasks**: T2.1 (Analysis)

---

## T2.1: Dependency Analysis

### Existing Abstraction Layer

uEmuera already has abstraction layers that separate Unity-specific code:

| Namespace | Purpose | Unity Dependent |
|-----------|---------|-----------------|
| `uEmuera.Drawing` | System.Drawing types (Color, Font, Size, Rectangle) | No |
| `uEmuera.Forms` | Windows Forms types (MessageBox, Timer, ScrollBar) | No |
| `uEmuera.Window` | Window management (MainWindow, DebugDialog) | **Yes** (via GenericUtils) |

### Global Access Pattern

```csharp
// GlobalStatic.cs provides global access to core components
internal static class GlobalStatic
{
    public static MainWindow MainWindow;     // Window/UI
    public static EmueraConsole Console;     // I/O
    public static Process Process;           // ERB execution
    // ... other data components
}
```

### Dependency Graph

```
┌─────────────────────────────────────────────────────────────┐
│                     ERB Game Logic                          │
│  (Process.cs, Instruction.cs, FunctionMethod.cs, etc.)      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    GlobalStatic                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │   Console   │  │ MainWindow  │  │   Process   │         │
│  │ (Emuera-    │  │ (uEmuera.   │  │ (GameProc)  │         │
│  │  Console)   │  │  Window)    │  │             │         │
│  └──────┬──────┘  └──────┬──────┘  └─────────────┘         │
└─────────┼────────────────┼──────────────────────────────────┘
          │                │
          ▼                ▼
┌─────────────────┐  ┌─────────────────┐
│ PrintString-    │  │  GenericUtils   │
│ Buffer,         │  │  (Unity-        │
│ StringMeasure   │  │   specific)     │
│ (Pure C#)       │  └─────────────────┘
└─────────────────┘           │
                              ▼
                    ┌─────────────────┐
                    │    Unity UI     │
                    │  (TextMeshPro,  │
                    │   uGUI, etc.)   │
                    └─────────────────┘
```

### Unity-Dependent Files

| File | Dependency | Notes |
|------|------------|-------|
| `uEmuera/Window.cs` | GenericUtils | MainWindow.Update() calls GenericUtils for rendering |
| `GenericUtils.cs` | Unity TextMeshPro | Actual text rendering to UI |
| `SpriteManager.cs` | Unity Texture2D | Image loading and management |
| `EmueraBehaviour.cs` | MonoBehaviour | Unity lifecycle |
| `EmueraMain.cs` | Unity | Entry point |

### Pure C# Files (Headless Safe)

- `EmueraConsole.cs` (mostly) - Text buffering, style management
- `EmueraConsole.Print.cs` - Print methods
- `PrintStringBuffer.cs` - Line buffering
- `Process.cs` - ERB execution
- `GameData/*` - Variables, expressions, functions
- `Sub/*` - Parsing, data streams

### Revised Approach

Given the existing abstraction layer, the cleanest headless approach is:

1. **Create `HeadlessWindow`** - Subclass or replace `MainWindow` with console output
2. **Create `HeadlessRunner`** - Entry point that initializes headless mode
3. **Keep EmueraConsole mostly unchanged** - It already separates buffering from rendering

This minimizes changes to existing code and preserves Unity compatibility.

### Files to Create/Modify

| File | Action | Purpose |
|------|--------|---------|
| `Headless/HeadlessWindow.cs` | Create | Console-based MainWindow replacement |
| `Headless/HeadlessConsole.cs` | Create | IConsole implementation using EmueraConsole |
| `Headless/HeadlessRunner.cs` | Create | Entry point for headless mode |
| `GlobalStatic.cs` | Minor | No changes needed (uses existing pattern) |

---

## T2.2-T2.4: Headless Components (WIP)

### Files Created

| File | Purpose |
|------|---------|
| `Headless/HeadlessWindow.cs` | MainWindow replacement for console output |
| `Headless/HeadlessConsole.cs` | IConsole implementation (stdin/stdout) |
| `Headless/IWindow.cs` | Interface for window abstraction |

### Status

**Partial completion** - Foundation created but integration pending:
- HeadlessConsole implements IConsole interface
- HeadlessWindow provides MainWindow-like interface
- IWindow defines abstraction for window operations

**Remaining work for Phase 3**:
- EmueraConsole currently requires `MainWindow` directly
- Need to either:
  1. Modify EmueraConsole to accept IWindow
  2. Create adapter/wrapper pattern
- HeadlessRunner entry point (Phase 3)

---

## Commits

| Repository | Hash | Description |
|------------|------|-------------|
| uEmuera | 35a0a63 | feat: add headless mode window and console stubs |

---

## Links

- `WBS-001.md` - Work breakdown structure
- `logs-001-T1.md` - Phase 1 log
- `feature-001.md` - Feature specification
