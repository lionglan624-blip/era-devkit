# Feature 200: Workflow & Encoding Fixes

## Status: [CANCELLED] → Feature 202 に統合

## Type: infra

## Background

### Problem

Feature 190実装中に以下の問題が発見された:

1. **エンコーディング問題**: K4/K5/K10 `_挿入.ERB`がUTF-8で保存されており、Shift-JIS期待のエンジンで警告が発生
2. **重複関数残存**: implementerがCOM_60ブロック削除時に関連関数(@CHK_CANCEL_COM60等)を見逃した
3. **AC期待値の事前/事後問題**: AC2の期待値が実装前の状態(10)で定義されていたが、KU移動後は11が正しい
4. **imple手順の非網羅性**: 削除作業で関連関数を漏らさない手順がなかった

### Root Causes

| 問題 | 根本原因 | 修正箇所 |
|------|----------|----------|
| UTF-8ファイル | kojo-writerまたはBOMフックがUTF-8で書いた | hooks, kojo-writer.md |
| 関数残存 | implementer指示に「関連関数」の明示なし | implementer.md |
| AC期待値 | AC定義時に変更後状態を予測していない | feature-template.md |
| 削除漏れ | 削除手順に関連関数チェックなし | imple skill |

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | K4/K5/K10 _挿入.ERBがShift-JIS+BOM | file | encoding | "Shift-JIS BOM" | [ ] |
| 2 | --strict-warnings警告0件 | exit_code | equals | "0" | [ ] |
| 3 | kojo-writer.mdにエンコーディング明記 | code | contains | "Shift-JIS" | [ ] |
| 4 | implementer.mdに関連関数削除手順追加 | code | contains | "関連関数" | [ ] |
| 5 | feature-template.mdにAC事後状態ガイド | code | contains | "Expected (post-implementation)" | [ ] |

**AC1 Method**: `file --mime-encoding Game/ERB/口上/*/KOJO_K*_挿入.ERB | head -3`
**AC2 Method**: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --strict-warnings 2>&1 | grep -c "FATAL\|WARNING"` (expect 0)
**AC3 Method**: `grep "Shift-JIS" .claude/agents/kojo-writer.md`
**AC4 Method**: `grep "関連関数" .claude/agents/implementer.md`
**AC5 Method**: `grep "Expected (post-implementation)" Game/agents/reference/feature-template.md`

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | 1 | K4/K5/K10 _挿入.ERBをShift-JIS+BOMで再保存 | `*_挿入.ERB` | [ ] |
| 2 | 2 | 警告確認・追加修正 | - | [ ] |
| 3 | 3 | kojo-writer.mdにエンコーディング指示追加 | `.claude/agents/kojo-writer.md` | [ ] |
| 4 | 4 | implementer.mdに削除時の関連関数チェック追加 | `.claude/agents/implementer.md` | [ ] |
| 5 | 5 | feature-template.mdにAC期待値ガイド追加 | `Game/agents/reference/feature-template.md` | [ ] |

---

## Implementation Details

### Task 1: エンコーディング修正

問題ファイル:
- `Game/ERB/口上/4_咲夜/KOJO_K4_挿入.ERB`
- `Game/ERB/口上/5_レミリア/KOJO_K5_挿入.ERB`
- `Game/ERB/口上/10_魔理沙/KOJO_K10_挿入.ERB`

修正方法:
```powershell
# UTF-8→Shift-JIS変換 + BOM付加
$content = Get-Content -Path $file -Encoding UTF8
$sjis = [System.Text.Encoding]::GetEncoding("shift_jis")
# BOM (EF BB BF for UTF-8 is wrong; use Shift-JIS compatible marker)
```

**Note**: ERBファイルはShift-JIS BOM (FEFF as Shift-JIS bytes: 未対応ならBOMなし)

### Task 3: kojo-writer.md追記

```markdown
## Encoding

**CRITICAL**: 全ERBファイルはShift-JIS (CP932)で保存すること。
- UTF-8禁止（エンジンが解釈不可）
- BOM: あり（エンジンが自動検出）
```

### Task 4: implementer.md追記

```markdown
## Deletion Tasks

ブロック削除時は以下を確認:
1. 対象関数 (@FUNCTION_NAME)
2. 実装関数 (@FUNCTION_NAME_1, _2, ...)
3. **関連関数** (CHK_*, HELPER_* など同名パターン)
4. 呼び出し元がなくなる関数

削除後: `--strict-warnings` で重複定義エラーがないか確認
```

### Task 5: feature-template.md追記

```markdown
### AC Expected Values

**Expected値は実装完了後の状態を記載する**:
- ❌ Bad: "10" (現状のファイル数)
- ✅ Good: "11" (KU追加後のファイル数)

移動/追加タスクがある場合、Expected値は変更後の値を使用。
```

---

## Execution Log

<!-- Filled by /imple -->

---

## Links

- [feature-190.md](feature-190.md) - 問題発見元
- [kojo-writer.md](../../.claude/agents/kojo-writer.md)
- [implementer.md](../../.claude/agents/implementer.md)
