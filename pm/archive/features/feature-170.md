# Feature 170: Default Parallel Execution for Directory Mode

## Status: [DONE]

## Type: engine

## Background

### Problem

`--unit` でディレクトリ指定時、デフォルトでは順次実行となり、20ファイル×23秒 = 約8分かかる。
並列実行（`--parallel`）を明示的に指定すれば約14秒で完了するが、ユーザーは毎回指定が必要。

### Goal

ディレクトリモード（複数ファイル）では並列実行をデフォルトにし、`--sequential` オプションで順次実行を明示的に選択可能にする。

### Context

- Feature 169 でディレクトリモードのメモリリーク/ハング問題は解決済み
- 現状: `--parallel` なし → 順次（遅い）、`--parallel` あり → 並列（高速）
- 変更後: デフォルト → 並列（高速）、`--sequential` あり → 順次

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | ディレクトリ指定でデフォルト並列実行 | output | contains | "[ProcessParallel] Workers:" | [ ] |
| 2 | --sequential オプションで順次実行 | output | not_contains | "[ProcessParallel]" | [ ] |
| 3 | 単一ファイル指定は従来通り（並列なし） | output | not_contains | "[ProcessParallel]" | [ ] |
| 4 | --parallel 明示でも並列実行（後方互換） | output | contains | "[ProcessParallel] Workers:" | [ ] |
| 5 | --help に --sequential オプション表示 | output | contains | "--sequential" | [ ] |
| 6 | engine ビルド成功 | build | succeeds | - | [ ] |

### AC Details

**AC 1**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-155/ --verbose`
- Verify output contains `[ProcessParallel] Workers:`

**AC 2**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-155/ --sequential --verbose`
- Verify output does NOT contain `[ProcessParallel]`

**AC 3**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-155/feature-155-K1.json --verbose`
- Verify output does NOT contain `[ProcessParallel]`

**AC 4**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-155/ --parallel --verbose`
- Verify output contains `[ProcessParallel] Workers:`

**AC 5**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --help`
- Verify output contains `--sequential`

**AC 6**: `dotnet build engine/uEmuera.Headless.csproj`
- Verify build succeeds

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | KojoBatchRunner でディレクトリモードのデフォルト並列実行を実装 | [ ] |
| 2 | 2 | HeadlessOptions に --sequential オプション追加し順次実行ロジック実装 | [ ] |
| 3 | 3 | 単一ファイル指定時は並列化しない条件を確認・維持 | [ ] |
| 4 | 4 | --parallel 明示指定の後方互換性を確認 | [ ] |
| 5 | 5 | --help に --sequential オプションのヘルプテキストを追加 | [ ] |
| 6 | 6 | engine ビルド成功確認 | [ ] |

---

## Implementation Notes

### Mutual Exclusivity

`--parallel` と `--sequential` が同時に指定された場合:
- **推奨動作**: `--sequential` が優先（安全側に倒す）
- **理由**: 順次実行は常に安全。並列実行はデフォルトなので明示不要

### Implementation Steps

1. **HeadlessOptions** (HeadlessRunner.cs ~L58):
   ```csharp
   public bool Sequential { get; set; }
   ```

2. **ParseArguments** (HeadlessRunner.cs ~L812):
   ```csharp
   else if (arg == "--sequential")
   {
       options.Sequential = true;
   }
   ```

3. **KojoBatchRunner** (KojoBatchRunner.cs ~L120):
   ```csharp
   bool isDirectoryMode = scenarioFiles.Count > 1 && !scenarioFiles[0].Contains("*");

   // Feature 170: Default parallel for directory mode
   // --sequential takes precedence over --parallel (safety first)
   if (isDirectoryMode && !options.Sequential && !options.Parallel)
   {
       options.Parallel = true;  // Auto-enable parallel mode
   }

   bool useProcessIsolation = isDirectoryMode && !options.Parallel;
   ```

4. **ShowHelp** (HeadlessRunner.cs ~L1029):
   ```csharp
   Console.WriteLine("  --sequential           Force sequential execution (disable auto-parallel)");
   ```

### Before/After Summary

| Flags | Before (169) | After (170) |
|-------|--------------|-------------|
| (none) + directory | Sequential | **Parallel** |
| `--parallel` + directory | Parallel | Parallel |
| `--sequential` + directory | N/A | **Sequential** |
| `--parallel --sequential` | N/A | **Sequential** (safety) |
| (any) + single file | Sequential | Sequential |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 | CREATE | - | Feature 170 作成 | Feature 169 からスコープ分離 |
| 2025-12-21 | INIT | initializer | Status PROPOSED → WIP | Ready for implementation |
| 2025-12-21 | IMPL | implementer | HeadlessRunner/KojoBatchRunner 実装 | SUCCESS |
| 2025-12-21 | TEST | ac-tester | AC 1-6 全検証 | ALL PASS |

---

## Links

- [feature-169.md](feature-169.md) - ディレクトリモードのメモリリーク修正
