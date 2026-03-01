# Feature 159: CLI Option Rename: --kojo-test → --unit

## Status: [DONE]

## Type: engine

## Background

### Problem
現在のCLIオプション `--kojo-test` は口上テスト専用という誤解を与える。実際にはERB関数の単体テスト全般に使用可能。

### Goal
`--kojo-test` を `--unit` にリネームし、ドキュメント・内部コードを一括更新。

### Scope
- CLI option: `--kojo-test` → `--unit`
- Internal naming: `KojoTest*` → `UnitTest*` or keep internal names (TBD)
- Documentation: 全箇所更新
- Test files: ディレクトリ名・参照更新

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | --unit オプションが動作 | exit_code | equals | "0" | [x] |
| 2 | --kojo-test が非推奨警告を出す | output | contains | "deprecated" | [x] |
| 3 | ドキュメント内に --kojo-test が残っていない | output | not_contains | "--kojo-test" | [x] |
| 4 | ビルド成功 | build | succeeds | - | [x] |
| 5 | 既存テスト回帰なし | exit_code | equals | "0" | [x] |

### AC Details

**AC1**: --unit オプションが動作
- **Test**: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/kojo/feature110-k1.json`
- **Expected**: Exit code 0

**AC2**: --kojo-test が非推奨警告を出す
- **Test**: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --kojo-test tests/kojo/feature110-k1.json 2>&1`
- **Expected**: Output contains "deprecated" or similar warning, but still works

**AC3**: ドキュメント内に --kojo-test が残っていない
- **Test**: `grep -r "--kojo-test" .claude/ Game/agents/ --include="*.md"`
- **Expected**: No matches (ドキュメントからは完全削除、コードのdeprecated aliasは維持)

**AC4**: ビルド成功
- **Test**: `cd engine && dotnet build uEmuera.Headless.csproj`
- **Expected**: Build succeeds

**AC5**: 既存テスト回帰なし
- **Test**: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/kojo/feature110-k8.json`
- **Expected**: Exit code 0

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | HeadlessRunner.cs: --unit 追加、--kojo-test を deprecated alias に | [x] |
| 2 | 3 | ドキュメント全更新 (.claude/, Game/agents/) | [x] |
| 3 | 4 | ビルド確認 | [x] |
| 4 | 5 | 回帰テスト実行 | [x] |

---

## Technical Notes

### 影響範囲調査

**C# コード**:
- `HeadlessRunner.cs` - CLI parsing
- `KojoTestRunner.cs` - クラス名は維持可能（内部実装）
- `KojoTestConfig.cs` - 同上
- `KojoBatchRunner.cs` - 同上

**ドキュメント**:
- `.claude/commands/*.md`
- `.claude/skills/*.md`
- `Game/agents/*.md`
- `Game/agents/reference/*.md`

**テストファイル**:
- `Game/tests/kojo/` → `Game/tests/unit/` (optional)
- JSON内の参照

### 検討事項

1. **内部クラス名**: `KojoTestRunner` → `UnitTestRunner`?
   - Pro: 一貫性
   - Con: 大規模リファクタリング
   - **推奨**: CLI名のみ変更、内部名は維持

2. **テストディレクトリ**: `tests/kojo/` → `tests/unit/`?
   - **推奨**: 維持（既存テスト多数）

3. **後方互換性**: `--kojo-test` を deprecated alias として残す
   - 警告出力して動作継続

---

## Dependencies

- **Blocked by**: なし
- **Blocks**: なし

---

## Review Notes

- **2025-12-21**: Type を `infra` → `engine` に修正（C#コード変更のため）
- AC3 マッチャータイプ `file` → `output` に修正
- 後方互換性: コードでは `--kojo-test` を deprecated alias として維持、ドキュメントからは完全削除
- 検討: CHANGELOG/リリースノートへの記載、--help出力の更新もスコープに含める

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-21 | initializer | Initialize Feature 159 | READY |
| 2025-12-21 | Explore | Investigate --kojo-test usage | 57 files identified |
| 2025-12-21 | implementer | Task 1: CLI option + deprecation warning | SUCCESS |
| 2025-12-21 | implementer | Task 2: Documentation update (51 files) | SUCCESS |
| 2025-12-21 | regression-tester | Full regression suite | 5/5 PASS |
| 2025-12-21 | ac-tester | AC1-5 verification | 5/5 PASS |
| 2025-12-21 | finalizer | Status update: WIP → DONE | COMPLETE |

---

## Links

- [index-features.md](index-features.md)
- [HeadlessRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs)
