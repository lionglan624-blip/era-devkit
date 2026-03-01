# Feature 200: Workflow & Encoding Fixes

## Status: [CANCELLED] ↁEFeature 202 に統吁E

## Type: infra

## Background

### Problem

Feature 190実裁E��に以下�E問題が発見された:

1. **エンコーチE��ング問顁E*: K4/K5/K10 `_挿入.ERB`がUTF-8で保存されており、Shift-JIS期征E�Eエンジンで警告が発甁E
2. **重褁E��数残孁E*: implementerがCOM_60ブロチE��削除時に関連関数(@CHK_CANCEL_COM60筁Eを見送E��ぁE
3. **AC期征E��の事前/事後問顁E*: AC2の期征E��が実裁E��の状慁E10)で定義されてぁE��が、KU移動後�E11が正しい
4. **imple手頁E�E非網羁E��**: 削除作業で関連関数を漏らさなぁE��頁E��なかっぁE

### Root Causes

| 問顁E| 根本原因 | 修正箁E�� |
|------|----------|----------|
| UTF-8ファイル | kojo-writerまた�EBOMフックがUTF-8で書ぁE�� | hooks, kojo-writer.md |
| 関数残孁E| implementer持E��に「関連関数」�E明示なぁE| implementer.md |
| AC期征E�� | AC定義時に変更後状態を予測してぁE��ぁE| feature-template.md |
| 削除漏れ | 削除手頁E��関連関数チェチE��なぁE| imple skill |

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | K4/K5/K10 _挿入.ERBがShift-JIS+BOM | file | encoding | "Shift-JIS BOM" | [ ] |
| 2 | --strict-warnings警呁E件 | exit_code | equals | "0" | [ ] |
| 3 | kojo-writer.mdにエンコーチE��ング明訁E| code | contains | "Shift-JIS" | [ ] |
| 4 | implementer.mdに関連関数削除手頁E��加 | code | contains | "関連関数" | [ ] |
| 5 | feature-template.mdにAC事後状態ガイチE| code | contains | "Expected (post-implementation)" | [ ] |

**AC1 Method**: `file --mime-encoding Game/ERB/口丁E*/KOJO_K*_挿入.ERB | head -3`
**AC2 Method**: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --strict-warnings 2>&1 | grep -c "FATAL\|WARNING"` (expect 0)
**AC3 Method**: `grep "Shift-JIS" .claude/agents/kojo-writer.md`
**AC4 Method**: `grep "関連関数" .claude/agents/implementer.md`
**AC5 Method**: `grep "Expected (post-implementation)" pm/reference/feature-template.md`

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | 1 | K4/K5/K10 _挿入.ERBをShift-JIS+BOMで再保孁E| `*_挿入.ERB` | [ ] |
| 2 | 2 | 警告確認�E追加修正 | - | [ ] |
| 3 | 3 | kojo-writer.mdにエンコーチE��ング持E��追加 | `.claude/agents/kojo-writer.md` | [ ] |
| 4 | 4 | implementer.mdに削除時�E関連関数チェチE��追加 | `.claude/agents/implementer.md` | [ ] |
| 5 | 5 | feature-template.mdにAC期征E��ガイド追加 | `pm/reference/feature-template.md` | [ ] |

---

## Implementation Details

### Task 1: エンコーチE��ング修正

問題ファイル:
- `Game/ERB/口丁E4_咲夁EKOJO_K4_挿入.ERB`
- `Game/ERB/口丁E5_レミリア/KOJO_K5_挿入.ERB`
- `Game/ERB/口丁E10_魔理沁EKOJO_K10_挿入.ERB`

修正方況E
```powershell
# UTF-8→Shift-JIS変換 + BOM付加
$content = Get-Content -Path $file -Encoding UTF8
$sjis = [System.Text.Encoding]::GetEncoding("shift_jis")
# BOM (EF BB BF for UTF-8 is wrong; use Shift-JIS compatible marker)
```

**Note**: ERBファイルはShift-JIS BOM (FEFF as Shift-JIS bytes: 未対応ならBOMなぁE

### Task 3: kojo-writer.md追訁E

```markdown
## Encoding

**CRITICAL**: 全ERBファイルはShift-JIS (CP932)で保存すること、E
- UTF-8禁止�E�エンジンが解釈不可�E�E
- BOM: あり�E�エンジンが�E動検�E�E�E
```

### Task 4: implementer.md追訁E

```markdown
## Deletion Tasks

ブロチE��削除時�E以下を確誁E
1. 対象関数 (@FUNCTION_NAME)
2. 実裁E��数 (@FUNCTION_NAME_1, _2, ...)
3. **関連関数** (CHK_*, HELPER_* など同名パターン)
4. 呼び出し�Eがなくなる関数

削除征E `--strict-warnings` で重褁E��義エラーがなぁE��確誁E
```

### Task 5: feature-template.md追訁E

```markdown
### AC Expected Values

**Expected値は実裁E��亁E���E状態を記載すめE*:
- ❁EBad: "10" (現状のファイル数)
- ✁EGood: "11" (KU追加後�Eファイル数)

移勁E追加タスクがある場合、Expected値は変更後�E値を使用、E
```

---

## Execution Log

<!-- Filled by /imple -->

---

## Links

- [feature-190.md](feature-190.md) - 問題発見�E
- [kojo-writer.md](../../../archive/claude_legacy_20251230/agents/kojo-writer.md)
- [implementer.md](../../../archive/claude_legacy_20251230/agents/implementer.md)
