# Feature 123: C# Unit Test プロジェクト修復

Type: engine
Status: [DONE]

## Summary

`uEmuera.Tests.csproj` のビルドエラーを修復し、C# Unit Test を実行可能にする。

## Background

現状の問題:
- `dotnet build uEmuera/uEmuera.Tests.csproj` が 197 エラーで失敗
- xUnit パッケージは `project.assets.json` に存在するが、コンパイル時に参照解決できない
- `uEmuera.Headless.csproj` への ProjectReference との依存関係問題の可能性

エラー例:
```
error CS0246: 型または名前空間の名前 'Xunit' が見つかりませんでした
error CS0246: 型または名前空間の名前 'Fact' が見つかりませんでした
```

## Design Decision

| 項目 | 決定 |
|------|------|
| **根本原因** | objフォルダ衝突 - 両プロジェクトが同じ `obj/` を共有し `project.assets.json` が上書きされる |
| **解決策** | テストプロジェクトを専用フォルダに移動（.NET標準構造） |
| 優先度 | Medium (機能開発はブロックしない) |

**現状** (問題):
```
uEmuera/
├── uEmuera.Headless.csproj
├── uEmuera.Tests.csproj    ← 同居
├── Tests/                   ← テストコード
└── obj/                     ← 衝突
```

**修正後** (標準構造):
```
uEmuera/
├── uEmuera.Headless.csproj
└── obj/

uEmuera.Tests/               ← 新規フォルダ
├── uEmuera.Tests.csproj
├── Tests/                   ← 移動
└── obj/                     ← 自然に分離
```

**作業内容**:
1. `uEmuera.Tests/` フォルダ作成
2. `uEmuera.Tests.csproj` を移動
3. `Tests/` フォルダを移動
4. ProjectReference パス修正 (`../uEmuera/uEmuera.Headless.csproj`)
5. 古い obj/ 削除して再ビルド

## Investigation Tasks (完了)

1. ✅ ProjectReference の依存関係グラフを確認
2. ✅ xUnit DLL の実際のパスを確認 → 存在する
3. ✅ objフォルダ衝突が根本原因と特定
4. → テストプロジェクトを専用フォルダに移動で解決

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | テストプロジェクトビルド成功 | build | succeeds | - | [x] |
| 2 | dotnet test 実行可能 | output | contains | "Test Run Successful" | [x] |

## Tasks

| Task# | AC | Description | Status |
|:-----:|:--:|-------------|:------:|
| 1 | 1 | 依存関係調査 & csproj 修正 | [x] |
| 2 | 2 | テスト実行確認 | [x] |

## Execution State

| Field | Value |
|-------|-------|
| Initialized At | 2025-12-19 |
| Current Phase | Complete |
| Assigned Agent | implementer (opus) |
| Next Step | ALL_TASKS_DONE |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2025-12-19 08:50 | START | implementer | Task 1 | - |
| 2025-12-19 08:52 | END | implementer | Task 1 | SUCCESS (2min) |
| 2025-12-19 12:34 | UNIT_TEST | unit-tester | Task 1 | PASS (0 errors, build succeeded) |
| 2025-12-19 08:54 | START | implementer | Task 2 | - |
| 2025-12-19 08:54 | END | implementer | Task 2 | SUCCESS (<1min) |
| 2025-12-19 08:55 | UNIT_TEST | unit-tester | Task 2 | PASS (85/85 tests pass, exit 0) |
| 2025-12-19 10:33 | END | finalizer | Feature 123 | DONE (finalized) |

## Notes

- 発見: Feature 122 実装中の Regression Test で検出
- `Temp\**\*` 除外は Feature 122 で追加済み（部分的修正）
- 本番ビルド (`uEmuera.Headless.csproj`) には影響なし
