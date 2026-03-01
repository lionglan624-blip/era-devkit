# Feature 012: WarningCollector Service Extraction

## Status: [DONE]

## Overview

ParserMediator.csの警告収集機能を独立したサービスとして抽出し、テスト可能性とモジュール性を向上させる。

## Current State

ParserMediator.cs (175行) は以下の責務を持つstatic class:
1. **警告収集** (L24-159): ConfigWarn, Warn, FlushWarningList等
2. **Rename辞書管理** (L37-92): LoadEraExRenameFile, RenameDic
3. **Console初期化** (L31-35): Initialize

問題点:
- static classのため単体テスト困難
- 複数責務（警告収集 + Rename管理）が混在
- EmueraConsoleへの直接依存

## Goals

1. 警告収集機能を `IWarningCollector` インターフェースとして抽出
2. テスト時にモック/スタブ注入可能にする
3. ParserMediatorの責務を軽減（警告収集を委譲）

## Non-Goals

- Rename機能の分離（別Feature）
- ParserMediatorの完全なinstance化（別Feature: ParserMediator instance-based）
- EmueraConsole依存の完全除去

## Technical Approach

### New Interface

```csharp
namespace MinorShift.Emuera.Sub
{
    public interface IWarningCollector
    {
        void AddWarning(string message, ScriptPosition pos, int level, string stackTrace = null);
        bool HasWarnings { get; }
        void Clear();
        void Flush(IWarningOutput output);
        IReadOnlyList<Warning> GetWarnings();
    }

    public interface IWarningOutput
    {
        void PrintWarning(string message, ScriptPosition pos, int level);
        void PrintSystemLine(string line);
    }
}
```

### Implementation

```csharp
namespace MinorShift.Emuera.Sub
{
    public class WarningCollector : IWarningCollector
    {
        private readonly List<Warning> _warnings = new List<Warning>();

        public void AddWarning(string message, ScriptPosition pos, int level, string stackTrace = null)
        {
            _warnings.Add(new Warning(message, pos, level, stackTrace));
        }

        public bool HasWarnings => _warnings.Count > 0;

        public void Clear() => _warnings.Clear();

        public void Flush(IWarningOutput output)
        {
            foreach (var w in _warnings)
            {
                output.PrintWarning(w.Message, w.Position, w.Level);
                if (w.StackTrace != null)
                {
                    foreach (var line in w.StackTrace.Split('\n'))
                        output.PrintSystemLine(line);
                }
            }
            _warnings.Clear();
        }

        public IReadOnlyList<Warning> GetWarnings() => _warnings.AsReadOnly();
    }

    public class Warning
    {
        public string Message { get; }
        public ScriptPosition Position { get; }
        public int Level { get; }
        public string StackTrace { get; }

        public Warning(string message, ScriptPosition pos, int level, string stackTrace)
        {
            Message = message;
            Position = pos;
            Level = level;
            StackTrace = stackTrace;
        }
    }
}
```

### Migration Strategy

1. 新しいWarningCollector.csを作成
2. ParserMediatorに`IWarningCollector`インスタンスを保持
3. 既存のstatic methodsは委譲パターンで維持（後方互換）
4. EmueraConsoleにIWarningOutput実装を追加

### File Changes

| File | Change |
|------|--------|
| `Sub/WarningCollector.cs` | **New**: IWarningCollector, WarningCollector, Warning |
| `Sub/IWarningOutput.cs` | **New**: IWarningOutput interface |
| `GameData/ParserMediator.cs` | **Modify**: Delegate to WarningCollector |
| `GameView/EmueraConsole.cs` | **Modify**: Implement IWarningOutput |

## Success Criteria

- [ ] IWarningCollector interface defined
- [ ] WarningCollector implementation passes unit tests
- [ ] ParserMediator delegates to WarningCollector
- [ ] Existing warning functionality unchanged (regression test)
- [ ] Headless mode works correctly

## Testability Improvements

抽出後、以下のテストが可能に:

```csharp
// Unit test example
[Test]
public void WarningCollector_AddsWarning()
{
    var collector = new WarningCollector();
    collector.AddWarning("test", null, 1);
    Assert.IsTrue(collector.HasWarnings);
    Assert.AreEqual(1, collector.GetWarnings().Count);
}

// Integration test with mock
var mockOutput = new MockWarningOutput();
collector.Flush(mockOutput);
Assert.AreEqual(1, mockOutput.PrintedWarnings.Count);
```

## Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Static method呼び出し箇所の見落とし | Low | Medium | Grep for all Warn calls |
| 後方互換性の破壊 | Low | High | Keep static facade methods |

## Estimated Effort

- Implementation: ~2 hours
- Testing: ~1 hour
- Documentation: ~30 minutes

## Dependencies

- None (standalone refactoring)

---

## Links

- [ParserMediator.cs](../../uEmuera/Assets/Scripts/Emuera/GameData/ParserMediator.cs) - Current implementation
- [index-features.md](index-features.md) - Feature tracking
- [agents.md](agents.md) - Workflow rules
