# Feature 001: Headless Mode

**Status**: [DONE]
**Created**: 2024-12-09
**Approved**: 2024-12-09

---

## Overview

Enable uEmuera to run ERB games without Unity UI, using standard console I/O for input/output.

## Goals

1. Run ERB scripts from command line without launching Unity
2. Support stdin/stdout for scripted interaction
3. Enable automated testing of ERB games
4. Provide foundation for CI/CD integration

## Non-Goals

- Full GUI replacement (this is CLI only)
- Graphics rendering (stubs only)
- Audio playback (stubs only)

## Success Criteria

- [x] Can start a new game via CLI
- [x] Can send input commands via stdin
- [x] Game output appears on stdout
- [x] Can run to completion without Unity dependencies
- [x] Existing uEmuera Unity mode continues to work

## Technical Approach

### Architecture

```
┌─────────────────────────────────────────┐
│           uEmuera Core (C#)             │
│  ┌─────────────────────────────────────┐│
│  │   Process.cs (ERB Execution)        ││
│  │   GameData/* (Variables, State)     ││
│  │   GlobalStatic.cs (Service Registry)││
│  └─────────────────────────────────────┘│
│                    │                     │
│           ┌───────┴───────┐              │
│           │ IConsole      │              │
│           │ Interface     │              │
│           └───────┬───────┘              │
│        ┌──────────┼──────────┐           │
│        ▼          ▼          ▼           │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐  │
│  │ Unity    │ │ Headless │ │ (Future) │  │
│  │ Console  │ │ Console  │ │ Web/etc  │  │
│  └──────────┘ └──────────┘ └──────────┘  │
└─────────────────────────────────────────┘
```

### Key Components

1. **IConsoleIO Interface**
   - `IConsoleOutput`: Print, PrintLine, Clear, SetColor, etc.
   - `IConsoleInput`: ReadLine, WaitKey, etc.

2. **HeadlessConsole Implementation**
   - Stdin → IConsoleInput
   - Stdout → IConsoleOutput
   - Stubs for graphics commands

3. **HeadlessRunner Entry Point**
   - Parse command line arguments
   - Initialize headless console
   - Bootstrap game execution

4. **Build Configuration**
   - Separate .csproj for headless build
   - .NET Core/Standard target for cross-platform

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Some ERB commands assume GUI | Medium | Implement no-op stubs |
| Timer-based features | Low | Use System.Threading timers |
| Graphics commands (GCREATE) | Low | Return success, no actual rendering |
| Save/Load compatibility | Medium | Ensure binary format unchanged |

## Dependencies

- None (standalone feature)

## Acceptance Checklist

- [x] All WBS tasks completed (`[○]`)
- [x] Manual test: New game starts via CLI
- [x] Manual test: Can make choices via stdin
- [x] Manual test: Unity mode still works
- [x] Documentation updated

---

## Links

- `WBS-001.md` - Work breakdown structure
- `agents/agents.md` - Workflow rules
- `agents/reference/engine-reference.md` - Architecture documentation
- `agents/index-features.md` - Feature tracking
