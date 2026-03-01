# Feature 015: IFileSystem Abstraction

## Status: [DONE]

## Overview

File/Directory操作を抽象化し、テスト時にファイルシステムをモック可能にする。現在44箇所のFile.*と65箇所のDirectory.*呼び出しが直接System.IOに依存しており、ユニットテストが困難。

## Current State

### 影響範囲

| 操作 | 箇所数 | ファイル数 |
|------|--------|-----------|
| File.* | 44 | 16 |
| Directory.* | 65 | 9 |
| **合計** | **109** | **~20** |

### 主な依存ファイル

| ファイル | File.* | Directory.* | 重要度 |
|----------|--------|-------------|--------|
| Config.cs | 2 | 13 | 高 |
| VariableEvaluator.cs | 9 | 4 | 高 |
| Process.cs | 5 | - | 高 |
| HeadlessRunner.cs | 3 | 12 | 中 |
| Utils.cs | 2 | 17 | 中 |

## Proposed Solution

### Phase 1: Interface定義（このFeature）

```csharp
// Sub/IFileSystem.cs
internal interface IFileSystem
{
    bool FileExists(string path);
    string ReadAllText(string path, Encoding encoding);
    void WriteAllText(string path, string contents, Encoding encoding);
    Stream OpenRead(string path);

    bool DirectoryExists(string path);
    string[] GetFiles(string path, string searchPattern);
    string[] GetDirectories(string path);
    void CreateDirectory(string path);
}
```

### Phase 2: Default実装

```csharp
// Sub/FileSystem.cs
internal sealed class FileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
    // ... 他メソッド
}
```

### Phase 3: DI統合

```csharp
// GlobalStatic.cs
private static IFileSystem _fileSystem = new FileSystem();
public static IFileSystem FileSystemInstance
{
    get => _fileSystem;
    set => _fileSystem = value ?? new FileSystem();
}
```

### Phase 4: 段階的移行（別Feature）

最も影響の少ないファイルから順次移行:
1. GameBase.cs (1箇所)
2. ParserMediator.cs (1箇所)
3. ConstantData.cs (3箇所)
4. ... 以降継続

## Goals

1. **テスト可能性**: ファイル操作をモック可能に
2. **段階的移行**: 既存コードは一度に変更しない
3. **後方互換性**: 移行完了まで既存コードは動作継続

## Acceptance Criteria

- [x] IFileSystem interfaceが定義されている
- [x] FileSystem default実装が存在する
- [x] GlobalStaticにDI用プロパティ追加
- [x] ビルド成功
- [x] 単体テスト: モック注入確認
- [x] GameBase.csが新インターフェース使用（移行例）
- [x] engine-reference.md に文書化

## Scope

### In Scope
- IFileSystem interface作成
- FileSystem default実装
- GlobalStatic DI統合
- 1-2ファイルの移行例（GameBase.cs等）
- 単体テスト

### Out of Scope
- 全ファイルの移行（別Featureで段階的に実施）
- Path操作の抽象化（File/Directoryのみ）
- 非同期API（現状は同期のみ）

## Effort Estimate

- **Size**: Medium (2-3セッション)
- **Risk**: 🟡 Medium（広範囲影響だが段階的移行）
- **Files**: 2 create, 2-3 modify

## Dependencies

- Feature 014 (Interface Extraction) - 同パターン参照

## Risks

| Risk | Mitigation |
|------|------------|
| 109箇所の変更は大きすぎる | Phase 4は別Featureで分割 |
| パス操作の違い(Win/Unix) | Path.Combineは維持、File/Directory操作のみ抽象化 |
| Encoding問題 | Encoding引数を明示的に要求 |

---

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [feature-014.md](feature-014.md) - Reference pattern
- [engine-reference.md](../reference/engine-reference.md) - Architecture
