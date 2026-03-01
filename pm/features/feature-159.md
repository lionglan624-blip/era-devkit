# Feature 159: CLI Option Rename: --kojo-test ↁE--unit

## Status: [DONE]

## Type: engine

## Background

### Problem
現在のCLIオプション `--kojo-test` は口上テスト専用とぁE��誤解を与える。実際にはERB関数の単体テスト�E般に使用可能、E

### Goal
`--kojo-test` めE`--unit` にリネ�Eムし、ドキュメント�E冁E��コードを一括更新、E

### Scope
- CLI option: `--kojo-test` ↁE`--unit`
- Internal naming: `KojoTest*` ↁE`UnitTest*` or keep internal names (TBD)
- Documentation: 全箁E��更新
- Test files: チE��レクトリ名�E参�E更新

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | --unit オプションが動佁E| exit_code | equals | "0" | [x] |
| 2 | --kojo-test が非推奨警告を出ぁE| output | contains | "deprecated" | [x] |
| 3 | ドキュメント�Eに --kojo-test が残ってぁE��ぁE| output | not_contains | "--kojo-test" | [x] |
| 4 | ビルド�E劁E| build | succeeds | - | [x] |
| 5 | 既存テスト回帰なぁE| exit_code | equals | "0" | [x] |

### AC Details

**AC1**: --unit オプションが動佁E
- **Test**: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/kojo/feature110-k1.json`
- **Expected**: Exit code 0

**AC2**: --kojo-test が非推奨警告を出ぁE
- **Test**: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --kojo-test tests/kojo/feature110-k1.json 2>&1`
- **Expected**: Output contains "deprecated" or similar warning, but still works

**AC3**: ドキュメント�Eに --kojo-test が残ってぁE��ぁE
- **Test**: `grep -r "--kojo-test" .claude/ pm/ --include="*.md"`
- **Expected**: No matches (ドキュメントから�E完�E削除、コード�Edeprecated aliasは維持E

**AC4**: ビルド�E劁E
- **Test**: `cd engine && dotnet build uEmuera.Headless.csproj`
- **Expected**: Build succeeds

**AC5**: 既存テスト回帰なぁE
- **Test**: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/kojo/feature110-k8.json`
- **Expected**: Exit code 0

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | HeadlessRunner.cs: --unit 追加、E-kojo-test めEdeprecated alias に | [x] |
| 2 | 3 | ドキュメント�E更新 (.claude/, pm/) | [x] |
| 3 | 4 | ビルド確誁E| [x] |
| 4 | 5 | 回帰チE��ト実衁E| [x] |

---

## Technical Notes

### 影響篁E��調査

**C# コーチE*:
- `HeadlessRunner.cs` - CLI parsing
- `KojoTestRunner.cs` - クラス名�E維持可能�E��E部実裁E��E
- `KojoTestConfig.cs` - 同丁E
- `KojoBatchRunner.cs` - 同丁E

**ドキュメンチE*:
- `.claude/commands/*.md`
- `.claude/skills/*.md`
- `pm/*.md`
- `pm/reference/*.md`

**チE��トファイル**:
- `test/kojo/` ↁE`test/unit/` (optional)
- JSON冁E�E参�E

### 検討事頁E

1. **冁E��クラス吁E*: `KojoTestRunner` ↁE`UnitTestRunner`?
   - Pro: 一貫性
   - Con: 大規模リファクタリング
   - **推奨**: CLI名�Eみ変更、�E部名�E維持E

2. **チE��トディレクトリ**: `tests/kojo/` ↁE`tests/unit/`?
   - **推奨**: 維持E��既存テスト多数�E�E

3. **後方互換性**: `--kojo-test` めEdeprecated alias として残す
   - 警告�E力して動作継綁E

---

## Dependencies

- **Blocked by**: なぁE
- **Blocks**: なぁE

---

## Review Notes

- **2025-12-21**: Type めE`infra` ↁE`engine` に修正�E�E#コード変更のため�E�E
- AC3 マッチャータイチE`file` ↁE`output` に修正
- 後方互換性: コードでは `--kojo-test` めEdeprecated alias として維持、ドキュメントから�E完�E削除
- 検訁E CHANGELOG/リリースノ�Eトへの記載、E-help出力�E更新もスコープに含める

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
| 2025-12-21 | finalizer | Status update: WIP ↁEDONE | COMPLETE |

---

## Links

- [index-features.md](../index-features.md)
- [HeadlessRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs)
