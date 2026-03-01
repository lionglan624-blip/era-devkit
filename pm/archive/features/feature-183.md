# Feature 183: Test Workflow Integrity & Protection

## Status: [DONE]

## Type: infra

## Background

### Problem

Feature 181 (COM_47 ニプルキャップ 口上) の実装中に複数のワークフロー問題が発覚した。

#### 発生した問題の詳細

**1. テストシナリオ編集が可能だった**

Feature 163 で tests/ac/ への既存ファイル編集をブロックする Hooks を実装したが、以下の抜け穴があった：

```
test-generator が tests/ac/kojo/feature-181/*.json を新規作成
    ↓
テスト実行: 97/160 passed, 63 failed
    ↓
debugger が tests/ac/kojo/feature-181/*.json の expected フィールドを編集
    ↓
Hooks がブロックしなかった（新規作成されたファイルは「既存」判定されない？）
```

これはTDD原則に反する。テストシナリオは変更禁止であるべき。

**2. test-generator が不正確なテストを生成**

```
kojo-writer が ERB 実装
    ↓
test-generator がステータスファイルの期待値を全パターンに適用
    ↓
実際のERB出力と期待値が不一致（63/160 テスト失敗）
```

問題：test-generator が「期待値」を推測しているが、実装内容と一致しない。

根本原因：コンテンツ系(kojo)では「テストが失敗すること自体がおかしい」。実装が正しければテストは通るはず。失敗するのはワークフローかプログラムのバグ。

**3. テストログの永続化不足**

| ログ種別 | 現状 | あるべき姿 |
|----------|------|-----------|
| PASS | 上書き保存 | 上書きでOK |
| FAIL | 上書き保存（消える） | 履歴として保持 |
| コンソール出力 | 永続化なし | FAIL時は保存 |

**4. Execution Log への記録不足**

Feature 181 の Execution Log には debugger 呼び出しが記録されなかった：

```markdown
# 実際の記録（不十分）
| 2025-12-22 | initializer | Initialize Feature 181 | READY |
| 2025-12-22 | ac-tester | Verify all ACs (160/160 tests) | PASS |

# あるべき記録
| 2025-12-22 | initializer | Initialize Feature 181 | READY |
| 2025-12-22 | - | unit test | FAIL:63/160 |
| 2025-12-22 | debugger | テストJSON期待値修正 | FIXED:63件 |  ← 本来禁止
| 2025-12-22 | - | unit test (retry) | PASS:160/160 |
| 2025-12-22 | ac-tester | Verify all ACs | PASS |
```

**5. Phase 10 レポートの不備**

オーケストレータが「Warnings/Issues: なし」と報告したが、実際には：
- テスト失敗 63件
- debugger による修正
- テストシナリオ編集（禁止事項違反）

が発生していた。

### Goal

1. tests/ac/, tests/regression/ の完全保護
2. 正規デバッグフロー（tests/debug/ 使用）の確立
3. FAIL時のログ永続化
4. Execution Log への適切な記録
5. ワークフロードキュメントの整備

### Context

#### 正規フロー（あるべき姿）

```
本番テスト FAIL
    ↓
debugger に調査指示（オーケストレータは手を動かさない）
    ↓
tests/debug/ にテスト作成、logs/debug/ にログ出力
    ↓
実装を修正（テストシナリオは変更禁止）
    ↓
本番テスト再実行
    ↓
PASS → 次のPhaseへ
FAIL → 繰り返し or ユーザ報告
```

#### TDD原則

- AC基準（feature-{ID}.md の AC table）は変更禁止
- テストシナリオ（tests/ac/, tests/regression/）は変更禁止
- AC基準/テストシナリオがおかしい場合はユーザに報告

#### 問題のスコープ

以下は全てスコープ内の問題として報告・議論が必要：

| 問題 | 対応 |
|------|------|
| テストFAIL | 報告 + 対処 |
| リトライ | 報告 |
| バグ | 報告 + 対処 |
| ドキュメント不明瞭 | 報告 + 議論 |
| ミス | 報告 + 議論 |

→ 次のfeature立てるか、運用フロー/ドキュメント整備か議論

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | tests/ac/ 新規作成後の編集ブロック | exit_code | equals | 2 | [x] |
| 2 | tests/regression/ 新規作成後の編集ブロック | exit_code | equals | 2 | [x] |
| 3 | tests/debug/ 編集許可 | exit_code | equals | 0 | [x] |
| 4 | FAIL時ログ保持（別ファイル名） | file | exists | logs/ac/*/failed-*.json | [B] → [feature-184](feature-184.md) |
| 5 | imple.md Phase 6-10 ワークフロー更新 | code | contains | "tests/debug/" | [x] |
| 6 | debugger.md ルール追加 | code | contains | "本番テスト編集禁止" | [x] |
| 7 | Execution Log形式更新 | code | contains | "FAIL件数" | [x] |
| 8 | testing skill 更新 | code | contains | "debug vs 本番" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | pre-ac-write.ps1 修正（新規作成直後の編集もブロック） | [O] |
| 2 | 3 | tests/debug/ 許可確認 | [O] |
| 3 | 4 | エンジン: FAIL時に別ファイル名で保存 (failed-{timestamp}.json) | [SKIP] |
| 4 | 5 | imple.md 更新 | [O] |
| 5 | 6 | debugger.md 更新 | [O] |
| 6 | 7 | feature-template.md Execution Log形式更新 | [O] |
| 7 | 8 | testing skill 更新 | [O] |

---

## Design

### Task 1: Hooks 修正 (Git状態ベース)

```powershell
# pre-ac-write.ps1 修正案
# Git状態で未コミットの新規ファイルを判定

# ac/ - 新規作成 → 許可
if (-not (Test-Path $path)) {
    Write-Host "[Hook] New AC file allowed: $path"
    exit 0
}

# Git状態チェック: untracked (??) = 新規作成されたがコミットされていない
$gitStatus = git status --porcelain -- $path 2>$null
if ($gitStatus -match '^\?\?') {
    Write-Error "[BLOCKED] Cannot modify uncommitted new AC file: $path"
    exit 2
}

# 既存ファイル（コミット済み）→ ブロック
Write-Error "[BLOCKED] Cannot modify existing AC file: $path"
exit 2
```

**利点**:
- セッション跨ぎでも正確に動作
- コミット後は自動的に編集許可解除
- 一時ファイル管理不要

### Task 3: エンジン修正

```csharp
// TestPathUtils.cs 修正案
public static string DeriveLogPath(string testPath, bool isFailed = false)
{
    var logPath = testPath.Replace("tests/", "logs/");
    var dir = Path.GetDirectoryName(logPath);
    var name = Path.GetFileNameWithoutExtension(logPath);

    if (isFailed)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        return Path.Combine(dir, $"failed-{name}-{timestamp}.json");
    }

    return Path.Combine(dir, $"{name}-result.json");
}
```

### Task 4-7: ドキュメント更新

#### imple.md 追加内容

```markdown
## FAIL時の対応フロー

1. **本番テスト FAIL**
   - Execution Log に記録: `| {date} | - | {test_type} test | FAIL:{N}/{M} |`

2. **debugger 呼び出し**
   - tests/debug/ にテスト作成
   - logs/debug/ にログ出力
   - 本番テスト (tests/ac/, tests/regression/) は編集禁止

3. **修正後**
   - 本番テスト再実行
   - Execution Log に記録: `| {date} | - | {test_type} test (retry) | PASS:{N}/{M} |`

## Phase 10: Completion

**ログ検証** (Report前に必須):
1. logs/ac/{type}/feature-{ID}/ の全ログを読む
2. logs/regression/ の全ログを読む
3. 各ファイルの summary.failed を確認
4. failed > 0 があれば Warnings に記録
5. logs/*/failed-*.json が存在すれば Issues に記録
```

---

## Review Notes

- **2025-12-23**: Task 1 実装方式を「一時ファイル追跡」から「Git状態ベース」に変更。セッション跨ぎ問題を回避、コミットで自動解除される自然なフロー。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-23 | Initialize | initializer | Feature state validation | READY |
| 2025-12-23 | implement | implementer | Tasks 1,2,4-7 (infra) | SUCCESS |
| 2025-12-23 | Finalize | finalizer | Verify completion, update status | DONE |

---

## Links

- [feature-163.md](feature-163.md) - AC/Regression Protection Hooks (元実装)
- [feature-164.md](feature-164.md) - Engine Log Path Auto-Determination
- [feature-181.md](feature-181.md) - 問題発覚元
