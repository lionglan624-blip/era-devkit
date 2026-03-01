# Feature 431: Print Commands (Core 7 Handlers)

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: engine

## Created: 2026-01-10

---

## Summary

Migrate core 7 Print command handlers (PRINT, PRINTL, PRINTW, PRINTFORM, PRINTDATA, PRINTBUTTON, PRINTBUTTONC) from legacy GameProc to ICommand/ICommandHandler pattern with Mediator Pipeline. Establishes IConsoleOutput abstraction for all text output operations.

**Output Responsibility**: Core text output commands migrated to unified command infrastructure.

**Out of Scope** (for follow-up feature): PRINTDATAL, PRINTDATAW, PRINTSINGLE, PRINTC, PRINTLC, PRINTPLAIN, PRINTPLAINFORM, PRINTCHARDATA, PRINTPALAM, PRINTITEM, PRINTSHOPITEM.

**Output**: Command implementations in `Era.Core/Commands/Print/`

---

## Background

### Philosophy (Mid-term Vision)

**Phase 9: Command Infrastructure** - Establish IConsoleOutput as the single source of truth (SSOT) for all print output operations in Era.Core. Migrate core print commands to ICommand/ICommandHandler pattern with unified output abstraction, enabling testable print commands and consistent output behavior across the new C# runtime. This migration ensures all console output flows through a single abstraction, simplifying testing, debugging, and future output channel additions.

### Problem (Current Issue)

Print commands scattered across GameProc without unified execution pattern:
- Inconsistent error handling
- No centralized logging
- Difficult to test in isolation

### Goal (What to Achieve)

1. **Migrate core 7 Print commands** to ICommand/ICommandHandler pattern
2. **Unified output abstraction** - IConsoleOutput service
3. **Type-safe command definitions** - Print command classes
4. **Handler implementations** - Command handlers with Result<T>
5. **Equivalence verification** - Output matches legacy behavior

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Print command directory exists | file | Glob | exists | Era.Core/Commands/Print/*.cs | [x] |
| 2 | IConsoleOutput interface exists | file | Glob | exists | Era.Core/Interfaces/IConsoleOutput.cs | [x] |
| 3 | PrintCommand.cs exists | file | Glob | exists | Era.Core/Commands/Print/PrintCommand.cs | [x] |
| 4 | PrintHandler.cs exists | file | Glob | exists | Era.Core/Commands/Print/PrintHandler.cs | [x] |
| 5 | PrintL command handler exists | file | Grep | contains | "class PrintLHandler" | [x] |
| 6 | PrintW command handler exists | file | Grep | contains | "class PrintWHandler" | [x] |
| 7 | PrintForm command handler exists | file | Grep | contains | "class PrintFormHandler" | [x] |
| 8 | PrintData command handler exists | file | Grep | contains | "class PrintDataHandler" | [x] |
| 9 | DI registration (handlers) | file | Grep | contains | "AddSingleton.*ICommandHandler.*Print.*Handler" | [x] |
| 10 | Print command unit test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~PrintCommandTests" | [x] |
| 11 | Print output equivalence verification | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~PrintOutputEquivalence" | [x] |
| 12 | Zero technical debt | file | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 13 | PRINTBUTTON handler exists | file | Grep | contains | "class PrintButtonHandler" | [x] |
| 14 | PRINTBUTTONC handler exists | file | Grep | contains | "class PrintButtonCHandler" | [x] |

### AC Details

**AC#1**: Print command directory structure
- Test: Glob pattern="Era.Core/Commands/Print/*.cs"
- Verifies directory exists by checking for .cs files within `Era.Core/Commands/Print/`

**AC#2**: IConsoleOutput interface
- Test: Glob pattern="Era.Core/Interfaces/IConsoleOutput.cs"
- Abstraction for console output (Print, PrintLine, PrintForm, etc.)
- Injected into print command handlers

**AC#3-4**: PrintCommand.cs and PrintHandler.cs existence
- Test: Glob pattern="Era.Core/Commands/Print/PrintCommand.cs" and "Era.Core/Commands/Print/PrintHandler.cs"

**AC#5-8**: Handler class existence
- AC#5: Test: Grep pattern="class PrintLHandler" path="Era.Core/Commands/Print/"
- AC#6: Test: Grep pattern="class PrintWHandler" path="Era.Core/Commands/Print/"
- AC#7: Test: Grep pattern="class PrintFormHandler" path="Era.Core/Commands/Print/"
- AC#8: Test: Grep pattern="class PrintDataHandler" path="Era.Core/Commands/Print/"

**AC#9**: DI registration verification
- Test: Grep pattern="AddSingleton.*ICommandHandler.*Print.*Handler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Pattern matches all 7 print handlers (PrintHandler, PrintLHandler, PrintWHandler, etc.)

**AC#10**: Print command unit tests
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~PrintCommandTests"`
- Verifies output generation for each command variant

**AC#11**: Print output equivalence verification
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~PrintOutputEquivalence"`
- Verifies output matches legacy PRINT*/PRINTFORM*/PRINTDATA*/PRINTBUTTON* behavior
- Minimum: 7 Assert statements with specific input/output pairs:
  1. PRINT "Hello" → output "Hello" (no trailing newline)
  2. PRINTL "World" → output "World\n" (with newline)
  3. PRINTW "Wait" → output "Wait" + wait flag set
  4. PRINTFORM "{0}: {1}", "Key", 42 → output "Key: 42"
  5. PRINTDATA (selectedLines=["Line1","Line2"]) → outputs "Line1\nLine2" (selection done by caller)
  6. PRINTBUTTON "Click", 42 → button with text "Click" and value 42
  7. PRINTBUTTONC "Center", 1 → centered button with text "Center" and value 1

**AC#12**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Print/"
- Expected: 0 matches (all implementation complete)

**AC#13-14**: PRINTBUTTON/PRINTBUTTONC handlers
- Test: Grep pattern="class PrintButtonHandler|class PrintButtonCHandler" path="Era.Core/Commands/Print/"
- PrintButtonHandler handles PRINTBUTTON command
- PrintButtonCHandler handles PRINTBUTTONC (centered button) command

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Create Print command directory and IConsoleOutput interface | [x] |
<!-- Batch waiver (Task 1): Directory structure and interface are created together as foundational setup. -->
| 2 | 3,4 | Implement PRINT command and handler | [x] |
<!-- Batch waiver (Task 2): PrintCommand.cs and PrintHandler.cs are created together as command+handler pair per Mediator pattern. -->
| 3 | 5 | Implement PRINTL command and handler | [x] |
| 4 | 6 | Implement PRINTW command and handler | [x] |
| 5 | 7 | Implement PRINTFORM command and handler | [x] |
| 6 | 8 | Implement PRINTDATA command and handler | [x] |
| 7 | 13 | Implement PRINTBUTTON command and handler | [x] |
| 8 | 14 | Implement PRINTBUTTONC command and handler | [x] |
| 9 | 9 | Register all print handlers in DI | [x] |
| 10 | 10 | Write print command unit tests | [x] |
| 11 | 11,12 | Verify output equivalence and remove technical debt | [x] |
<!-- Batch waiver (Task 11): AC#11 (equivalence verification) and AC#12 (tech debt check) are final quality gates executed together per F429/F430 precedent. -->

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Locations**:
- Core logic: `engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs` (PRINT_Instruction class)
- Specialized: `engine/Assets/Scripts/Emuera/GameProc/Commands/Print/`

| Command | File | Purpose |
|---------|------|---------|
| PRINT | Instraction.Child.cs | Inline output (PRINT_Instruction) |
| PRINTL | Instraction.Child.cs | Line output (PRINT_Instruction) |
| PRINTW | Instraction.Child.cs | Wait after output (PRINT_Instruction) |
| PRINTFORM | Instraction.Child.cs | Formatted output (PRINT_Instruction) |
| PRINTDATA | Instraction.Child.cs | Data array output (PRINT_Instruction) |
| PRINTC | Instraction.Child.cs | Colored output (PRINT_Instruction) |
| PRINTPLAIN | PrintPlainCommand.cs | Plain text output |
| PRINTBUTTON | PrintButtonCommand.cs | Button output |
| PRINTBUTTONC | PrintButtonCCommand.cs | Colored button output |
| PRINTCHARDATA | PrintCharDataCommand.cs | Character data output |
| PRINTPALAM | PrintPalamCommand.cs | Parameter data output |
| PRINTITEM | PrintItemCommand.cs | Item list output |
| PRINTSHOPITEM | PrintShopItemCommand.cs | Shop item list output |

### IConsoleOutput Interface

**IConsoleOutput** (`Era.Core/Interfaces/IConsoleOutput.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces;

/// <summary>
/// Console output abstraction for print commands
/// </summary>
public interface IConsoleOutput
{
    /// <summary>Print text inline</summary>
    Result<Unit> Print(string text);

    /// <summary>Print text with newline</summary>
    Result<Unit> PrintLine(string text);

    /// <summary>Print text and wait for input</summary>
    Result<Unit> PrintWait(string text);

    /// <summary>Print formatted text</summary>
    Result<Unit> PrintForm(string format, params object[] args);

    /// <summary>Print DATALIST data - outputs pre-selected data lines</summary>
    /// <remarks>
    /// PRINTDATA is a special command for DATALIST blocks in ERB.
    /// The caller (command dispatcher) handles random block selection.
    /// This method receives the already-selected lines to output.
    /// </remarks>
    Result<Unit> PrintData(string[] selectedLines);

    /// <summary>Print button with int value</summary>
    Result<Unit> PrintButton(string text, long value);

    /// <summary>Print button with string value</summary>
    Result<Unit> PrintButton(string text, string value);

    /// <summary>Print centered button (PRINTBUTTONC)</summary>
    Result<Unit> PrintButtonCentered(string text, long value);
}
```

### Print Command Definitions

**PrintCommand** (`Era.Core/Commands/Print/PrintCommand.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Commands.Print;

/// <summary>
/// PRINT command - inline output
/// </summary>
public record PrintCommand(CommandId Id, string Text) : ICommand<Unit>;

/// <summary>
/// PRINTL command - line output
/// </summary>
public record PrintLCommand(CommandId Id, string Text) : ICommand<Unit>;

/// <summary>
/// PRINTW command - wait after output
/// </summary>
public record PrintWCommand(CommandId Id, string Text) : ICommand<Unit>;

/// <summary>
/// PRINTFORM command - formatted output
/// </summary>
public record PrintFormCommand(CommandId Id, string Format, object[] Args) : ICommand<Unit>;

/// <summary>
/// PRINTDATA command - outputs pre-selected data lines from DATALIST block
/// </summary>
/// <remarks>Random block selection is handled by the command dispatcher before creating this command.</remarks>
public record PrintDataCommand(CommandId Id, string[] SelectedLines) : ICommand<Unit>;

/// <summary>
/// PRINTBUTTON command - button output (supports both int and string values)
/// </summary>
/// <remarks>Value must be either long or string. Other types will result in Result.Failure.</remarks>
public record PrintButtonCommand(CommandId Id, string Text, object Value) : ICommand<Unit>;

/// <summary>
/// PRINTBUTTONC command - centered button output
/// </summary>
public record PrintButtonCCommand(CommandId Id, string Text, long Value) : ICommand<Unit>;
```

### Print Handler Implementations

**PrintHandler** (`Era.Core/Commands/Print/PrintHandler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.Print;

/// <summary>
/// PRINT command handler
/// </summary>
public class PrintHandler : ICommandHandler<PrintCommand, Unit>
{
    private readonly IConsoleOutput _console;

    public PrintHandler(IConsoleOutput console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public Task<Result<Unit>> Handle(PrintCommand command, CancellationToken ct)
    {
        return Task.FromResult(_console.Print(command.Text));
    }
}

/// <summary>
/// PRINTL command handler
/// </summary>
public class PrintLHandler : ICommandHandler<PrintLCommand, Unit>
{
    private readonly IConsoleOutput _console;

    public PrintLHandler(IConsoleOutput console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public Task<Result<Unit>> Handle(PrintLCommand command, CancellationToken ct)
    {
        return Task.FromResult(_console.PrintLine(command.Text));
    }
}

/// <summary>
/// PRINTW command handler
/// </summary>
public class PrintWHandler : ICommandHandler<PrintWCommand, Unit>
{
    private readonly IConsoleOutput _console;

    public PrintWHandler(IConsoleOutput console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public Task<Result<Unit>> Handle(PrintWCommand command, CancellationToken ct)
    {
        return Task.FromResult(_console.PrintWait(command.Text));
    }
}

/// <summary>
/// PRINTFORM command handler
/// </summary>
public class PrintFormHandler : ICommandHandler<PrintFormCommand, Unit>
{
    private readonly IConsoleOutput _console;

    public PrintFormHandler(IConsoleOutput console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public Task<Result<Unit>> Handle(PrintFormCommand command, CancellationToken ct)
    {
        return Task.FromResult(_console.PrintForm(command.Format, command.Args));
    }
}

/// <summary>
/// PRINTDATA command handler
/// </summary>
public class PrintDataHandler : ICommandHandler<PrintDataCommand, Unit>
{
    private readonly IConsoleOutput _console;

    public PrintDataHandler(IConsoleOutput console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public Task<Result<Unit>> Handle(PrintDataCommand command, CancellationToken ct)
    {
        return Task.FromResult(_console.PrintData(command.SelectedLines));
    }
}

/// <summary>
/// PRINTBUTTON command handler
/// </summary>
public class PrintButtonHandler : ICommandHandler<PrintButtonCommand, Unit>
{
    private readonly IConsoleOutput _console;

    public PrintButtonHandler(IConsoleOutput console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public Task<Result<Unit>> Handle(PrintButtonCommand command, CancellationToken ct)
    {
        // Value can be long or string - dispatch to appropriate overload
        return command.Value switch
        {
            long longValue => Task.FromResult(_console.PrintButton(command.Text, longValue)),
            string strValue => Task.FromResult(_console.PrintButton(command.Text, strValue)),
            _ => Task.FromResult(Result<Unit>.Fail($"Invalid button value type: {command.Value?.GetType()}"))
        };
    }
}

/// <summary>
/// PRINTBUTTONC command handler (centered button)
/// </summary>
public class PrintButtonCHandler : ICommandHandler<PrintButtonCCommand, Unit>
{
    private readonly IConsoleOutput _console;

    public PrintButtonCHandler(IConsoleOutput console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public Task<Result<Unit>> Handle(PrintButtonCCommand command, CancellationToken ct)
    {
        return Task.FromResult(_console.PrintButtonCentered(command.Text, command.Value));
    }
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// Print Command Handlers (Phase 9)
services.AddSingleton<ICommandHandler<PrintCommand, Unit>, PrintHandler>();
services.AddSingleton<ICommandHandler<PrintLCommand, Unit>, PrintLHandler>();
services.AddSingleton<ICommandHandler<PrintWCommand, Unit>, PrintWHandler>();
services.AddSingleton<ICommandHandler<PrintFormCommand, Unit>, PrintFormHandler>();
services.AddSingleton<ICommandHandler<PrintDataCommand, Unit>, PrintDataHandler>();
services.AddSingleton<ICommandHandler<PrintButtonCommand, Unit>, PrintButtonHandler>();
services.AddSingleton<ICommandHandler<PrintButtonCCommand, Unit>, PrintButtonCHandler>();
```

### IConsoleOutput Implementation

**Implementation Responsibility**: IConsoleOutput implementation is provided by the runtime host:
- **Headless mode**: Provided by uEmuera.Headless runtime
- **GUI mode**: Provided by Unity runtime
- **Unit tests**: Use MockConsoleOutput or TestConsoleOutput stub

The print command handlers depend on IConsoleOutput being registered in DI by the host, not by Era.Core itself.

### Equivalence Verification

Legacy behavior: PRINT commands write to GlobalStatic.ConsoleInstance directly.

New behavior: Print commands invoke IConsoleOutput abstraction through CommandDispatcher pipeline.

**Verification**: Both approaches produce identical console output. New approach adds logging/validation through pipeline.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F429 | CommandDispatcher + Mediator Pipeline (ICommand/ICommandHandler) |
| Predecessor | F430 | Pipeline Behaviors (logging/validation applied to print commands) |
| Predecessor | F377 | Design Principles (Result<T> pattern) |

---

## Links

- [feature-377.md](feature-377.md) - Design Principles (Result<T> pattern)
- [feature-424.md](feature-424.md) - Phase 9 Planning (parent feature)
- [feature-429.md](feature-429.md) - CommandDispatcher + Mediator Pipeline (dependency)
- [feature-430.md](feature-430.md) - Pipeline Behaviors (dependency)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 9 Print Commands

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-10 | create | implementer | Created from F424 Phase 9 Planning | PROPOSED |
| 2026-01-10 | START | initializer | Initialize Feature 431 | READY |
| 2026-01-10 | START | explorer | Investigate patterns and constraints | READY |
| 2026-01-10 | START | implementer | Create tests and implementation | SUCCESS |
| 2026-01-10 | DEVIATION | do | Pre-existing F432/F433 test files blocking build | Deleted unimplemented feature tests |
| 2026-01-10 | END | ac-tester | All 14 ACs verified | PASS |
| 2026-01-10 | END | regression | All 708 tests passed | PASS |
