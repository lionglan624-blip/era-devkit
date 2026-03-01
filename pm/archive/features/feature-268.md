# Feature 268: AC 機械的検証の完全化 - 全 AC タイプで Log Verification 対応

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)

**全 Feature で全 AC を機械的監査で検証し、AC 基準を満たすことを保証する。オーケストレータがスキップや解釈により判定を変更できない、真の TDD を実現する。**

**アーキテクチャ**: ac-static-verifier.py が静的検証 (code/build/file 型) を実行し JSON ログを生成。verify-logs.py がログを読み取り監査結果を集計。

核心原則:
1. **機械的保証**: 人間の判断を介さず、全 AC が自動検証可能
2. **監査証跡**: 全 AC に対応するログファイルが存在し、追跡可能
3. **スキップ禁止**: code/build 型 AC も含め、オーケストレータの裁量でスキップ不可
4. **1:1 対応**: AC と検証ログが 1:1 で対応

### Problem (Current Issue)

F262 実行時に `verify-logs.py --scope feature:262` が `OK:0/0` を返した。

| 問題 | 詳細 |
|------|------|
| code 型 AC がログなし | Grep 検証は実行されるがログが残らない |
| build 型 AC がログなし | dotnet build 成功確認がログに残らない |
| オーケストレータ裁量 | Phase 3 スキップ条件に code/build 型が含まれる |
| 監査不能 | 後から「本当に検証したか」を確認できない |

現状の do.md Phase 3:
```
**Skip conditions**: AC already has test, AC Type=build/file/code, Feature Type=infra/kojo
```

→ これにより code/build 型 AC は TDD から除外されている。

### Goal (What to Achieve)

1. **全 AC タイプに対応した検証 JSON 生成** - code/build/file 型も含む
2. **verify-logs.py が全 AC をカウント** - `OK:12/12` のように全 AC が計上される
3. **Phase 3 AC Type スキップ条件の撤廃** - code/build/file 型 AC で TDD 適用 (Feature Type=infra/kojo スキップは維持)
4. **AC:Log 1:1 対応** - 各 AC に対応するログファイルが必ず存在

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ac-static-verifier.py 存在 | file | Glob | exists | tools/ac-static-verifier.py | [x] |
| 2 | code 型 AC 検証成功 (exit 0) | exit_code | Bash | succeeds | python tools/ac-static-verifier.py --feature 268 --ac-type code | [x] |
| 3 | code 型ログファイル生成 | file | Glob | exists | Game/logs/prod/ac/code/feature-268/code-result.json | [x] |
| 4 | build 型 AC 検証成功 (exit 0) | exit_code | Bash | succeeds | python tools/ac-static-verifier.py --feature 268 --ac-type build | [x] |
| 5 | build 型ログファイル生成 | file | Glob | exists | Game/logs/prod/ac/build/feature-268/build-result.json | [x] |
| 6 | file 型 AC 検証成功 (exit 0) | exit_code | Bash | succeeds | python tools/ac-static-verifier.py --feature 268 --ac-type file | [x] |
| 7 | file 型ログファイル生成 | file | Glob | exists | Game/logs/prod/ac/file/feature-268/file-result.json | [x] |
| 8 | verify-logs.py が新ログを読取 (N > 0) | output | Bash | matches | Feature-268:.*OK:[1-9][0-9]* | [x] |
| 9 | do.md Skip 条件 combined pattern なし | code | Grep(.claude/commands/do.md) | not_contains | AC Type=build/file/code | [x] |
| 10 | (reserved - AC 9 に統合済み) | - | - | - | - | [B] |
| 11 | (reserved - AC 9 に統合済み) | - | - | - | - | [B] |
| 12 | testing SKILL に静的検証ドキュメント追加 | code | Grep(.claude/skills/testing/SKILL.md) | contains | ac-static-verifier.py | [x] |
| 13 | ビルド成功 (bootstrap) | build | dotnet build | succeeds | dotnet build engine/uEmuera.Headless.csproj | [x] |
| 14 | 回帰テスト PASS (bootstrap) | test | --flow | succeeds | all scenarios pass (exit 0) | [x] |

### AC Details

**AC 1**: ac-static-verifier.py の存在確認
```bash
ls tools/ac-static-verifier.py  # exists
```

**AC 2-7**: 各 AC タイプの検証成功とログファイル生成

**Note**: AC 3, 5, 7 は AC 2, 4, 6 の実行結果として生成されるログを検証する。テスト順序依存があるため、AC 2→3, AC 4→5, AC 6→7 の順序で実行すること。

```bash
# code 型
python tools/ac-static-verifier.py --feature 268 --ac-type code  # exit 0 (AC 2)
# AC 3: Glob("Game/logs/prod/ac/code/feature-268/code-result.json") - file exists

# build 型
python tools/ac-static-verifier.py --feature 268 --ac-type build  # exit 0 (AC 4)
# AC 5: Glob("Game/logs/prod/ac/build/feature-268/build-result.json") - file exists

# file 型
python tools/ac-static-verifier.py --feature 268 --ac-type file  # exit 0 (AC 6)
# AC 7: Glob("Game/logs/prod/ac/file/feature-268/file-result.json") - file exists
```

**AC 8**: verify-logs.py が新ログディレクトリを読取 (N > 0 検証)

**Dependency**: Task 5 完了後に検証可能。verify-logs.py が Game/logs/prod/ac/{code,build,file}/ ディレクトリを読み取る必要がある。

**Test Command**:
```bash
python tools/verify-logs.py --scope feature:268
# 出力例: Feature-268:         OK:3/3
# → パターン 'Feature-268:.*OK:[1-9]' にマッチすれば PASS
# → OK:0/0 はマッチしないため FAIL (本 Feature が解決する問題)
```

**AC 9**: do.md Phase 3 Skip 条件から AC Type combined pattern を除外

**Assumption**: do.md の Skip 条件は `AC Type=build/file/code` の combined pattern として記載されており、個別の `AC Type=build`, `AC Type=file`, `AC Type=code` が別途追加されることはない。そのため combined pattern の not_contains チェックで十分。

**Test Command**:
```bash
# Grep with output_mode: content, verify 0 matches
grep -c "AC Type=build/file/code" .claude/commands/do.md  # Expected: 0 or "no match"
```

```markdown
# 修正前
Skip conditions: AC already has test, AC Type=build/file/code, Feature Type=infra/kojo

# 修正後
Skip conditions: AC already has test, Feature Type=infra/kojo
```

**AC 10-11**: (reserved) - AC 9 に統合済み

**AC 13-14**: ビルド/回帰テスト (bootstrap)

**Bootstrap Note**: AC 13 (build) と AC 14 (test) は F268 が解決しようとしている「ログなし」問題の対象 AC タイプである。F268 自身のこれらの AC は既存の do.md Phase 6 フローで検証され、ac-static-verifier.py による検証は将来の Feature から適用される。**F268 は ac-static-verifier.py を作成する Feature であるため、検証時点でツールが存在しない。これが bootstrap 例外の理由である。**

**Manual Verification**: AC 13/14 は F268 において手動検証で PASS とマークされる（ログ生成なし）。**AC 13/14 は F268 限定の bootstrap 例外である。F269 以降の全 Feature は build/test AC タイプに ac-static-verifier.py を使用しなければならない。**

---

## Tasks

**Exception to AC:Task 1:1 Rule**: Task 2-4 は AC ペアが順序依存（実行→ログ検証）のため結合。**AC ペアに実行順序依存がある場合（execute → verify result）、1 Task で複数 AC を検証することを許可する。**

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | ac-static-verifier.py スケルトン作成 | [X] |
| 2 | 2,3 | code 型 AC 検証機能実装 (Grep + JSON 出力) | [X] |
| 3 | 4,5 | build 型 AC 検証機能実装 (dotnet build + JSON 出力) | [X] |
| 4 | 6,7 | file 型 AC 検証機能実装 (Glob + JSON 出力) | [X] |
| 5 | 8 | verify-logs.py 動作確認のみ (既存 glob パターン対応済み、変更不要想定) | [X] |
| 6 | 9 | do.md Phase 3 スキップ条件から AC Type=build/file/code を削除 | [X] |
| 7 | 12 | testing SKILL に静的検証ドキュメント追加 | [X] |
| 8 | 13 | ビルド確認 | [X] |
| 9 | 14 | 回帰テスト実行 | [X] |

---

## Design Notes

### アーキテクチャ方針

**関心の分離**:
- **Engine (headless)**: ERB 実行専用 (--unit, --flow)
- **Python tools**: 静的検証 (code/build/file 型 AC)

Engine は ERB ランタイムに特化し、grep/build のような静的検証は Python ツールで実装する。

### ac-static-verifier.py 仕様案

**入力**: feature-{ID}.md から AC 定義を読み取り、Type=code/build/file の AC を検証

**AC Table → Verifier Field Mapping**:

| AC Table Column | Verifier Input | Description |
|-----------------|----------------|-------------|
| Type | verification_type | code/build/file を判定に使用 |
| Method | file_path (if Grep) | Grep(path) の場合、path を抽出 |
| Expected | pattern/path | 検証対象パターンまたはパス |
| Matcher | assertion_type | contains/not_contains/exists/succeeds |

**code 型検証** (Grep ベース):
```python
# feature-268.md の AC を解析し、Grep で検証
# Method: Grep(file_path) → file_path を抽出
# Expected: pattern → 検索パターン
# Matcher: contains/not_contains → マッチ有無判定
```

**build 型検証**:
```python
# dotnet build を実行し、exit code で判定
```

**file 型検証** (Glob ベース):
```python
# ファイル存在確認
```

**出力**: `Game/logs/prod/ac/{type}/feature-{ID}/{type}-result.json`

**Path Convention**: 全パスはリポジトリルートからの相対パス。verify-logs.py のデフォルトディレクトリは `Game/logs/prod`。

**Note**: code/build/file ディレクトリは既存の kojo/engine と並列に配置される。

**Windows 環境注意**: Glob ツールは Windows で動作が不安定な場合がある (CLAUDE.md 参照)。そのため、ファイル名は予測可能な形式 `{type}-result.json` を使用する。AC テーブルでは `*-result.json` パターンを使用するが、実際には 1 ファイルのみ生成される。

```
Game/logs/prod/ac/
├── kojo/feature-*/     ← 既存 (口上 AC ログ)
├── engine/             ← 既存 (エンジン AC ログ)
├── code/feature-268/code-result.json    ← NEW: code 型 AC の検証ログ
├── build/feature-268/build-result.json  ← NEW: build 型 AC の検証ログ
└── file/feature-268/file-result.json    ← NEW: file 型 AC の検証ログ
```

**Aggregation Rule**: Each AC type produces ONE aggregated log file per feature containing results for ALL ACs of that type as an array.

```json
{
  "feature": 268,
  "type": "code",
  "results": [
    {
      "ac_number": 9,
      "result": "PASS",
      "details": {"pattern": "AC Type=build/file/code", "matched_files": []}
    },
    {
      "ac_number": 12,
      "result": "PASS",
      "details": {"pattern": "ac-static-verifier.py", "matched_files": ["testing/SKILL.md"]}
    }
  ],
  "summary": {"total": 2, "passed": 2, "failed": 0}
}
```

### 実装方針

1. **ac-static-verifier.py**: code/build/file 型 AC を検証し JSON ログ出力
2. **verify-logs.py**: logs/prod/ac/{code,build,file}/ ディレクトリを読み取り、カウントに含める
3. **do.md**: Phase 3 で ac-static-verifier.py を呼び出し、全 AC タイプを検証
4. **testing SKILL**: 静的検証パイプラインのドキュメント追加

### 影響範囲

| Component | Change |
|-----------|--------|
| `tools/ac-static-verifier.py` | 新規作成 |
| `tools/verify-logs.py` | 動作確認のみ (既存 glob パターン `**/{scope}/*-result.json` で対応済み) |
| `.claude/commands/do.md` | Phase 3 修正 |
| `.claude/skills/testing/SKILL.md` | 静的検証ドキュメント |

---

## Review Notes

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-30 | create | opus | F262 実行中の監査課題から Feature 作成 | - |
| 2025-12-30 08:29 | START | implementer | Task 1 | - |
| 2025-12-30 08:29 | END | implementer | Task 1 | SUCCESS |
| 2025-12-30 08:30 | START | implementer | Task 2 | - |
| 2025-12-30 08:32 | END | implementer | Task 2 | SUCCESS |
| 2025-12-30 08:33 | START | implementer | Task 3 | - |
| 2025-12-30 08:35 | END | implementer | Task 3 | SUCCESS |
| 2025-12-30 08:37 | START | implementer | Task 4 | - |
| 2025-12-30 08:37 | END | implementer | Task 4 | SUCCESS |
| 2025-12-30 08:38 | START | implementer | Task 6 | - |
| 2025-12-30 08:38 | END | implementer | Task 6 | SUCCESS |
| 2025-12-30 08:39 | START | implementer | Task 5 | - |
| 2025-12-30 08:39 | END | implementer | Task 5 | SUCCESS |
| 2025-12-30 08:38 | START | implementer | Task 7 | - |
| 2025-12-30 08:38 | END | implementer | Task 7 | SUCCESS |
| 2025-12-30 08:40 | START | implementer | Task 8 | - |
| 2025-12-30 08:40 | END | implementer | Task 8 | SUCCESS |
| 2025-12-30 08:41 | START | implementer | Task 9 | - |
| 2025-12-30 08:41 | END | implementer | Task 9 | SUCCESS |

---

## Links

- [feature-262.md](feature-262.md) - 本 Feature のきっかけ (OK:0/0 問題)
- [do.md](../../.claude/commands/do.md) - Phase 3 スキップ条件
- [testing/SKILL.md](../../.claude/skills/testing/SKILL.md) - AC タイプ定義
