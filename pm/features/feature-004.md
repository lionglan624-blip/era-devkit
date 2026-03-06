# Feature 004: Dead Code Detection

## Status: [DONE]

## Overview

ERB Linter (Feature 003) を拡張し、呼び出されない関数（デッドコード）を検出する機能を追加。386ファイル・7074関数のコードベースから未使用コードを特定し、メンテナンス性向上を支援する。

## Goals

1. **未使用関数検出**: 一度も呼び出されない `@FUNCNAME` を特定
2. **呼び出し元分析**: 各関数の呼び出し元リストを生成
3. **エントリポイント認識**: システム関数（@EVENTFIRST等）を除外
4. **レポート出力**: デッドコード候補をリスト化

## Use Case

```
開発者がコードベース整理を実施
    ↓
ErbLinter --dead-code 実行
    ↓
未使用関数リスト取得
    ↓
確認後、不要コード削除
    ↓
コードベース縮小・可読性向上
```

---

## Scope

### In Scope

| 機能 | 詳細 |
|------|------|
| 関数呼び出し解析 | CALL, TRYCALL, JUMP, GOTO ターゲット収集 |
| CALLFORM部分解析 | 静的に解決可能なパターンのみ |
| エントリポイント定義 | @EVENT*, @SHOW_*, @SYSTEM* 等 |
| 除外リスト | 設定ファイルで除外パターン指定可能 |

### Out of Scope (v1)

- 完全なCALLFORM動的解析（実行時のみ確定）
- 変数経由の関数呼び出し追跡
- クロスファイル依存グラフ可視化（別Feature候補）

---

## Technical Design

### 既存資産の活用

| コンポーネント | ファイル | 状態 | 用途 |
|---------------|----------|------|------|
| FunctionIndex | `Parser/FunctionIndex.cs` | 実装済 | 7074関数の定義位置 |
| FunctionAnalyzer | `Analyzer/FunctionAnalyzer.cs` | 実装済 | CALL/TRYCALLターゲット抽出 |
| FUNC001/002 | 同上 | 実装済 | 未定義関数警告 |

### アーキテクチャ

```
┌─────────────────────────────────────────────────────────┐
│                      Program.cs                          │
│  (既存) --dead-code オプション追加                        │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│                 DeadCodeAnalyzer.cs (新規)               │
│  ┌─────────────────────────────────────────────────────┐│
│  │ 1. BuildCallGraph()                                 ││
│  │    - FunctionAnalyzer.ExtractCallTarget() 使用      ││
│  │    - Dictionary<callee, HashSet<caller>> 構築       ││
│  ├─────────────────────────────────────────────────────┤│
│  │ 2. LoadEntryPoints()                                ││
│  │    - SystemEntryPoints.txt から読み込み             ││
│  │    - @EVENT*, @SHOW_* 等のパターン                  ││
│  ├─────────────────────────────────────────────────────┤│
│  │ 3. FindDeadCode()                                   ││
│  │    - callers[func].Count == 0 && !IsEntryPoint     ││
│  │    - DEAD001 Issue 生成                             ││
│  └─────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────┘
```

### 新規ファイル

| ファイル | 役割 |
|----------|------|
| `Analyzer/DeadCodeAnalyzer.cs` | デッドコード検出ロジック |
| `Config/SystemEntryPoints.txt` | エントリポイント定義 |

### エントリポイント定義

```text
# SystemEntryPoints.txt
# Emuera system callbacks
EVENTFIRST
EVENTEND
EVENTTURNEND
EVENTCOMEND
EVENTSHOP
EVENTTRAIN
EVENTBUY
EVENTSELL
EVENTLOAD
EVENTSAVE

# Display functions
SHOW_STATUS
SHOW_SHOP
SHOW_USERCOM
SHOW_ABLUP_SELECT

# User-defined entry patterns (regex)
^USERCOM\d+$
^COM\d+$
^COMF\d+$
^COMABLE.*$

# Kojo entry points
^KOJO_.*$
^NTR_KOJO_.*$
```

### データ構造

```csharp
public class DeadCodeAnalyzer
{
    // 関数名 -> 呼び出し元関数のセット
    private Dictionary<string, HashSet<string>> _callers = new();

    // エントリポイント（呼び出し元不要）
    private HashSet<string> _entryPoints = new();

    // エントリポイントパターン（正規表現）
    private List<Regex> _entryPatterns = new();

    public IEnumerable<Issue> Analyze(
        FunctionIndex funcIndex,
        Dictionary<string, List<string>> callTargets)
    {
        BuildCallGraph(callTargets);

        foreach (var func in funcIndex.GetAllFunctions())
        {
            if (!HasCallers(func.Name) && !IsEntryPoint(func.Name))
            {
                yield return new Issue(
                    func.File, func.Line, 1,
                    IssueLevel.Info, "DEAD001",
                    $"Function @{func.Name} is never called");
            }
        }
    }
}
```

### CLI拡張

```bash
# デッドコード検出
erb-linter --dead-code Game/ERB/

# エントリポイントファイル指定
erb-linter --dead-code --entry-points custom.txt Game/ERB/

# 呼び出し元表示（将来拡張）
erb-linter --callers FUNC_NAME Game/ERB/

# JSON出力
erb-linter --dead-code -f json -o dead-code.json Game/ERB/
```

### 処理フロー

```
1. ファイルスキャン (既存)
   ↓
2. 関数インデックス構築 (既存 FunctionIndex)
   ↓
3. CALL/TRYCALL/JUMP ターゲット収集 (既存 FunctionAnalyzer拡張)
   ↓
4. コールグラフ構築 (新規 DeadCodeAnalyzer)
   ↓
5. エントリポイント読み込み (新規)
   ↓
6. デッドコード判定・レポート (新規)
```

---

## Error Codes

| Code | Level | Description |
|------|-------|-------------|
| DEAD001 | Info | Function is never called (potential dead code) |
| DEAD002 | Info | Function only called by dead code (transitively dead) |

---

## Acceptance Criteria

- [x] エントリポイント関数リストを定義できる
- [x] 全関数の呼び出し元を収集できる
- [x] 呼び出されない関数をDEAD001として報告できる
- [x] 除外パターンを設定できる
- [x] Game/ERB/ 全体をスキャンして結果を得られる

---

## Risks

| リスク | 対策 |
|--------|------|
| CALLFORM動的呼び出しで誤検出 | Info レベル、除外リストで対応 |
| エントリポイント漏れ | 設定ファイルで追加可能に |
| 大量の警告でノイズ化 | フィルタリング、サマリー表示 |

---

## Dependencies

- Feature 003 (ERB Linter) - 完了済み、拡張ベース

---

## Estimated Effort

| Phase | Tasks | Estimate |
|-------|-------|----------|
| Design | エントリポイント定義、CLI設計 | 小 |
| Implementation | 呼び出し収集、判定ロジック | 小 |
| Testing | Game/ERB/での検証 | 小 |
| **Total** | | **小** (Feature 003拡張) |

---

## Links

- [WBS-004.md](WBS-004.md) - Work breakdown
- [feature-003.md](feature-003.md) - ERB Linter (base feature)
- [WBS-003.md](WBS-003.md) - ERB Linter WBS
- [agents.md](agents.md) - Workflow rules
