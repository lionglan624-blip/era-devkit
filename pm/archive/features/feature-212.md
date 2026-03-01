# Feature 212: CI Integration (pre-commit hook)

## Status: [DONE]

## Type: infra

## Depends: [211]

## Background

### Problem

F205 で verify-logs.py を作成し、ログベースの機械的検証が可能になった。
しかし現状は imple.md Phase 7-9 でサブエージェント報告と照合する複雑なフローになっている。

**現状の問題**:
1. サブエージェント報告の信頼性問題（虚偽報告の可能性）
2. Phase 7-9 の照合ロジックが複雑
3. 毎回手動で verify-logs.py を実行する必要がある

### Solution

pre-commit hook で全テストを実行し、verify-logs.py で最終判定。
commit 時に機械的に Pass/Fail が決まり、Claude の恣意性を排除。

### Dependency

```
F205 (verify-logs.py) ← 完了
F206 (Flow調査) → F207 (Flow実装) → 24/24 PASS ← 完了
F208 (シナリオ期待値修正) ← 必須
F209 (Flow Mode State Fix) ← 必須
F210 (Scenario Input Sequence) ← 必須
F211 (Empty Line Bug Fix) ← 必須
F212 (本Feature)
F213 (Doc整理) ← F212 完了後
```

### Goal

1. pre-commit hook で全テストスイート実行
2. verify-logs.py で最終〇×判定
3. imple.md Phase 7-9 簡素化

---

## Acceptance Criteria

### Part A: pre-commit hook 作成

| AC# | Description | Type | Matcher | Expected | Target | Status |
|:---:|-------------|------|---------|----------|--------|:------:|
| A1 | pre-commit スクリプト存在 | file | exists | .githooks/pre-commit | .githooks/pre-commit | [x] |
| A2 | dotnet build 実行 | output | contains | "Build succeeded" | .githooks/pre-commit | [x] |
| A3 | dotnet test 実行 → TRX 出力 | file | exists | logs/prod/engine/test-result.trx | .githooks/pre-commit | [x] |
| A4 | --strict-warnings 実行 | exit_code | equals | 0 | .githooks/pre-commit | [x] |
| A5 | --flow regression テスト実行 | output | contains | "24/24" | .githooks/pre-commit | [x] |
| A6 | --unit AC テスト実行 | exit_code | equals | 0 | .githooks/pre-commit | [x] |
| A7 | verify-logs.py 最終判定 | exit_code | equals | 0 | .githooks/pre-commit | [x] |
| A8 | FAIL時 commit 拒否 | exit_code | equals | 1 | .githooks/pre-commit | [x] |
| A9 | PASS時 commit 許可 | exit_code | equals | 0 | .githooks/pre-commit | [x] |

### Part B: imple.md 簡素化

| AC# | Description | Type | Matcher | Expected | Target | Status |
|:---:|-------------|------|---------|----------|--------|:------:|
| B1 | Phase 7 簡素化 | file | contains | "CI で検証済み" | .claude/commands/imple.md | [x] |
| B2 | Phase 8 簡素化 | file | not_contains | "verify-logs.py を実行" | .claude/commands/imple.md | [x] |
| B3 | Phase 9 削除または簡素化 | file | not_contains | "照合方法" | .claude/commands/imple.md | [x] |

### Part C: ドキュメント

| AC# | Description | Type | Matcher | Expected | Target | Status |
|:---:|-------------|------|---------|----------|--------|:------:|
| C1 | CLAUDE.md に hook 設定方法追加 | file | contains | "git config core.hooksPath" | CLAUDE.md | [x] |
| C2 | testing/SKILL.md に CI セクション追加 | file | contains | "## CI (pre-commit)" | .claude/skills/testing/SKILL.md | [x] |

### Part D: ビルド確認

| AC# | Description | Type | Matcher | Expected | Target | Status |
|:---:|-------------|------|---------|----------|--------|:------:|
| D1 | dotnet build 成功 | build | succeeds | - | engine/uEmuera.Headless.csproj | [x] |
| D2 | hook 実行テスト | exit_code | equals | 0 | .githooks/pre-commit | [x] |

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | A1 | pre-commit hook スクリプト作成 | .githooks/pre-commit | [O] |
| 2 | A2 | dotnet build ステップ実装 | .githooks/pre-commit | [O] |
| 3 | A3 | dotnet test → TRX 出力実装 | .githooks/pre-commit | [O] |
| 4 | A4 | --strict-warnings 実行実装 | .githooks/pre-commit | [O] |
| 5 | A5 | --flow regression テスト実装 | .githooks/pre-commit | [O] |
| 6 | A6 | --unit AC テスト実装 | .githooks/pre-commit | [O] |
| 7 | A7 | verify-logs.py 最終判定実装 | .githooks/pre-commit | [O] |
| 8 | A8 | FAIL時 exit 1 実装 | .githooks/pre-commit | [O] |
| 9 | A9 | PASS時 exit 0 確認 | .githooks/pre-commit | [O] |
| 10 | B1 | Phase 7 簡素化 | .claude/commands/imple.md | [O] |
| 11 | B2 | Phase 8 簡素化 | .claude/commands/imple.md | [O] |
| 12 | B3 | Phase 9 簡素化 | .claude/commands/imple.md | [O] |
| 13 | C1 | CLAUDE.md 更新 | CLAUDE.md | [O] |
| 14 | C2 | testing/SKILL.md 更新 | .claude/skills/testing/SKILL.md | [O] |
| 15 | D1 | dotnet build 確認 | engine/uEmuera.Headless.csproj | [O] |
| 16 | D2 | hook 実行テスト | .githooks/pre-commit | [O] |

---

## Technical Details

### Task 1-9: pre-commit hook

**ファイル**: `.githooks/pre-commit`

**注意**: Git Bash 環境で実行される想定。Windows ネイティブ環境では PowerShell 版を別途用意する必要がある場合あり。

```bash
#!/bin/bash
set -e

echo "=== Pre-commit CI ==="

# 1. Build
echo "[1/6] dotnet build..."
dotnet build engine/uEmuera.Headless.csproj -v q

# 2. C# Unit Tests → TRX
echo "[2/6] dotnet test..."
dotnet test engine.Tests/ \
  --logger "trx;LogFileName=test-result.trx" \
  --results-directory Game/logs/prod/ac/engine \
  -v q

# 3. Strict warnings
echo "[3/6] strict-warnings..."
cd Game
dotnet run --project ../engine/uEmuera.Headless.csproj -- . --strict-warnings < NUL

# 4. Regression tests
echo "[4/6] regression tests..."
dotnet run --project ../engine/uEmuera.Headless.csproj -- . --flow "tests/regression/scenario-*.json"

# 5. AC tests
echo "[5/6] AC tests..."
dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit "tests/ac/**/*.json"

cd ..

# 6. Final verification
echo "[6/6] verify-logs.py..."
python tools/verify-logs.py --dir Game/logs/prod

echo "=== CI PASSED ==="
```

**設定方法**:
```bash
git config core.hooksPath .githooks
chmod +x .githooks/pre-commit
```

### Task 10-12: imple.md 簡素化

**Before (複雑)**:
- Phase 7: ac-tester dispatch → 報告確認
- Phase 8: regression-tester dispatch → verify-logs.py → 照合
- Phase 9: サブエージェント報告と verify-logs.py 結果を照合

**After (簡素)**:
- Phase 7-9 統合: 「pre-commit hook で全テスト実行済み。Phase 10 到達時点で全テスト PASS 確定。」

### --no-verify オプション

開発中は一時的にスキップ可能:
```bash
git commit --no-verify -m "WIP: ..."
```

最終 commit では CI 必須。

### Task 8-9: FAIL/PASS テスト方法

**A8 (FAIL時 commit 拒否) テスト方法**:
1. テストシナリオを一時的に壊す（例: 期待値を不正な値に変更）
2. `git commit` 実行
3. exit code 1 で拒否されることを確認
4. テストシナリオを元に戻す

**A9 (PASS時 commit 許可) テスト方法**:
1. 全テストが通る状態で `git commit` 実行
2. exit code 0 で commit が成功することを確認

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-24 | - | F205 レビュー時に CI 統合案として起案 | PROPOSED |
| 2025-12-25 | - | F209 挿入により 210 にリナンバー | PROPOSED |
| 2025-12-25 | - | F211 挿入により 212 にリナンバー | PROPOSED |
| 2025-12-25 | initializer | Feature 212 initialization: status PROPOSED→WIP | WIP |
| 2025-12-25 | implementer | pre-commit hook 作成、imple.md 簡素化、CLAUDE.md/SKILL.md 更新 | DONE |
| 2025-12-25 | ac-tester | 全16 AC 検証: PASS | OK:16/16 |
| 2025-12-25 | regression-tester | verify-logs.py 実行 | OK:937/937 |
| 2025-12-25 | feature-reviewer | Post-implementation review | READY |
| 2025-12-25 | finalizer | Status→DONE, F213起票（Thread Safety）| DONE |

---

## Design Review: pre-commit hook のテスト範囲 (F213 時点で追記)

### 疑問点

F213 レビュー時に pre-commit hook の設計思想について議論が発生。現状の hook は全テストスイートを実行しているが、これは本来の設計意図と異なる可能性がある。

### 現状の pre-commit hook

```bash
[1/6] dotnet build
[2/6] dotnet test (engine.Tests)
[3/6] strict-warnings
[4/6] regression tests (--flow)
[5/6] AC tests (--unit)      ← 約100秒
[6/6] verify-logs.py
```

**問題**: AC テストで約100秒かかり、commit に約2分必要。

### imple.md の設計思想（Type Routing）

imple.md では Feature Type に応じてテスト内容を変えている:

| Feature Type | 実行するテスト |
|--------------|----------------|
| kojo | `--unit` のみ（dotnet test 不要） |
| erb | `--flow` |
| engine | `dotnet test` |
| infra | build のみ |

> kojo では `dotnet test` (engine.Tests) を実行しない。

### テストの役割分担（本来の設計意図？）

| タイミング | 目的 | 実行内容 |
|------------|------|----------|
| /imple Phase 7 | **変更部分の保証** | AC テスト（この Feature が正しいか） |
| /imple Phase 8 | **既存部分の保証** | Regression テスト（他を壊していないか） |
| **pre-commit** | **最終確認** | 前提条件 + Regression のみ？ |

**疑問**: AC テストは /imple Phase 7 で既に実行済み。pre-commit で再実行する必要があるか？

### 考えられる選択肢

| 選択肢 | pre-commit 内容 | 所要時間 |
|--------|-----------------|----------|
| A: 現状維持 | 全テスト | 約2分 |
| B: AC 除外 | build + test + strict + regression + verify | 約10秒 |
| C: Type 依存 | 変更ファイルに応じて動的決定 | 複雑 |

### 未解決の論点

1. **AC テストの重複実行**: /imple で済んでいるなら pre-commit では不要では？
2. **dotnet test の位置づけ**: C# 変更時のみ必要だが、現状は常に実行
3. **strict-warnings の位置づけ**: ERB 変更時のみ必要だが、現状は常に実行
4. **Type Routing との整合性**: imple.md の思想と pre-commit が一致していない

### 結論 (2025-12-25)

**pre-commit hook は回帰テストのみ実行する**。

**理由**:
1. AC テストは /imple Phase 7 で既にループ検証済み（FAIL → debugger → 再実行、max 3回）
2. dotnet build/test は engine Type の AC 検証で実行済み
3. strict-warnings は Phase 6 Smoke Test で実行済み
4. pre-commit の責務は「既存を壊していないか」の最終確認のみ

**変更後の pre-commit**:
```bash
[1/2] regression tests (--flow)
[2/2] verify-logs.py
```

**所要時間**: 約4秒（従来約2分）

### 参照

- [feature-101.md](feature-101.md) - strict-warnings 設計（Design Decision セクション追記済み）
- [feature-213.md](feature-213.md) - 本議論の発生元
- [imple.md](../../.claude/commands/imple.md) - Type Routing セクション

---

## Links

- [feature-205.md](feature-205.md) - verify-logs.py 作成
- [feature-207.md](feature-207.md) - Flow Test 実装（前提）
- [feature-209.md](feature-209.md) - Flow Mode State Fix（前提）
- [feature-211.md](feature-211.md) - Empty Line Bug Fix（前提）
- [feature-213.md](feature-213.md) - Thread Safety（本Feature後）
- [feature-214.md](feature-214.md) - Doc 整理（F213後）
- [verify-logs.py](../../tools/verify-logs.py)
- [imple.md](../../.claude/commands/imple.md)
