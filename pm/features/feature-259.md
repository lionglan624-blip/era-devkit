# Feature 259: ERB関数重褁E��知の/doワークフロー統吁E

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
1. **実裁E��に問題を検知し、デバッグコストを削減すめE*
2. **F190 リファクタリング基準に基づき、誤刁E��されたファイルを集紁E��めE*
   - F190 で COM→ファイルマッピングが確宁E(COM 60-72 ↁE`_挿入.ERB`, COM 80-85 ↁE`_口挿入.ERB`)
   - こ�Eルールに反するスタブが残存してぁE��と重褁E��ラーの原因となめE
   - 重褁E��知チE�Eルは、F190 ルール違反の発見にも活用可能

### Problem
F241実裁E��、K2/K4/K9の`口挿入.ERB`に既存スタブが残っており、新実裁E�E`挿入.ERB`の関数がロード頁E��上書きされた。結果、AC検証で48/160チE��トが失敗し、デバッグが忁E��になった、E

**根本原因**: Emueraはファイルをアルファベット頁E��ロードするため、同名関数が褁E��ファイルに存在すると後からロードされた方が有効になる、E

### Goal
1. /do Phase 4�E�実裁E���E�で重褁E��数を検知し、kojo-writer dispatch前に警告�E修正を行う
2. **F241 で発生したスタブ重褁E��題�E再発防止**
   - K2/K4/K9 の `口挿入.ERB` に残存しぁECOM_64 スタチEↁE48/160 チE��ト失敁E
   - 同様�E問題を COM_65-68 実裁E��に検知・解決

### Context
- 発生箁E��: F241 Phase 6 (チE��チE��で2回修正)
- 影響: 開発時間ロス + context消費
- 対象: kojo Feature (COM実裁E��)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md Phase 4.0に重褁E��ェチE��手頁E��加 | file | Grep | contains | "Duplicate Check" | [x] |
| 2 | 重褁E��知スクリプト作�E | file | Glob | exists | src/tools/python/erb-duplicate-check.py | [x] |
| 3 | 検知チE��チE 重褁E��めE| output | python | contains | "DUPLICATE" | [x] |
| 4 | 検知チE��チE 重褁E��ぁE| output | python | contains | "OK" | [x] |
| 5 | スキャンモードテスチE| exit_code | python | succeeds | - | [x] |
| 6 | Build | build | dotnet build | succeeds | - | [x] |

### AC Details

**AC1**: `Grep("Duplicate Check", ".claude/commands/do.md")`

**AC2**: `Glob("src/tools/python/erb-duplicate-check.py")`

**AC3**: 重褁E��存在するケースでスクリプトを実行し、�E力に "DUPLICATE" が含まれることを確誁E
```bash
# チE��トデータ: チE��ト実行時に一時的なスタブファイルを作�E
# 1. チE��ト用ERBファイルを作�E (同名関数めEファイルに定義)
#    場所: .tmp/f259/ (CLAUDE.md temporary file guidelines)
# 2. スクリプト実衁E
# 3. チE��ト用ファイルを削除
python src/tools/python/erb-duplicate-check.py --function "KOJO_TEST_DUPLICATE" --path ".tmp/f259"
```
**Setup**: Task 2 実行時に `.tmp/f259/` 配下に一時的なチE��ト用スタブを作�E・削除

**AC4**: 重褁E��なぁE��ースでスクリプトを実行し、�E力に "OK" が含まれることを確誁E
```bash
python src/tools/python/erb-duplicate-check.py --function "KOJO_MESSAGE_COM_K1_64" --path "Game/ERB/口丁E1_美鈴"
```
**Precondition**: `KOJO_MESSAGE_COM_K1_64` は `Game/ERB/口丁E1_美鈴/KOJO_K1_挿入.ERB` に1つだけ存在�E�E241完亁E��より確認済み�E�E

**AC5**: スキャンモーチE(--function なぁE でチE��レクトリ冁E�E全@関数をスキャン。正常終亁E(exit code 0) を確誁E
```bash
python src/tools/python/erb-duplicate-check.py --path "Game/ERB/口丁E1_美鈴"
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 2 | erb-duplicate-check.py スクリプト作�E (スキャンモード含む) | [x] |
| 2 | 3 | 検知チE��チE 重褁E��めE(チE��トデータ作�E・削除含む) | [x] |
| 3 | 4 | 検知チE��チE 重褁E��ぁE| [x] |
| 4 | 5 | スキャンモードテスチE| [x] |
| 5 | 1 | do.md Phase 4.0 更新 | [x] |
| 6 | 6 | ビルド確誁E| [x] |

---

## Design

### 依存関俁E

- **Python**: 3.8+ (既存ツール kojo_mapper.py と同槁E

### スクリプト仕槁E

#### 単一関数チェチE�� (--function 持E��E
```bash
python src/tools/python/erb-duplicate-check.py --function "KOJO_MESSAGE_COM_K2_64" --path "Game/ERB/口丁E
```

**出劁E*:
```
# 重褁E��めE
DUPLICATE: KOJO_MESSAGE_COM_K2_64
  - Game/ERB/口丁E2_小悪魁EKOJO_K2_挿入.ERB:751
  - Game/ERB/口丁E2_小悪魁EKOJO_K2_口挿入.ERB:27

# 重褁E��ぁE
OK: KOJO_MESSAGE_COM_K2_64 (1 definition)
  - Game/ERB/口丁E2_小悪魁EKOJO_K2_挿入.ERB:751
```

#### スキャンモーチE(--function なぁE
```bash
python src/tools/python/erb-duplicate-check.py --path "Game/ERB/口丁E2_小悪魁E
```

**動佁E*: 持E��パス冁E�E全ERBファイルから `@` で始まる関数定義を抽出し、E��褁E��チェチE��

**出劁E*:
```
# 重褁E��めE
DUPLICATE: KOJO_MESSAGE_COM_K2_64
  - Game/ERB/口丁E2_小悪魁EKOJO_K2_挿入.ERB:751
  - Game/ERB/口丁E2_小悪魁EKOJO_K2_口挿入.ERB:27

# 重褁E��ぁE(すべてユニ�Eク)
OK: All 42 functions are unique
```

### do.md Phase 4.0 変更

既存�E `4.0 eraTW Cache` を拡張し、Duplicate Check を追加 (kojo type のみ):

```markdown
**4.0 Pre-Implementation Check** (kojo only):

**Duplicate Check**: For each target character K{N}:
python src/tools/python/erb-duplicate-check.py --function "KOJO_MESSAGE_COM_K{N}_{COM}" --path "Game/ERB/口丁E

| Result | Action |
|--------|--------|
| OK | Continue to eraTW Cache |
| DUPLICATE | Remove stub first, then continue |

**eraTW Cache**: Dispatch eratw-reader.
```

**Note**: Fail-fast principle により Duplicate Check めEeraTW Cache の前に配置。重褁E��あれば早期に発見�E解決、E

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 | Phase 1 | initializer | Feature init | READY |
| 2025-12-28 | Phase 2 | explorer | Investigate context | Complete |
| 2025-12-28 | Phase 4 | implementer | Tasks 1-5 | SUCCESS |
| 2025-12-28 | Phase 6 | - | AC verification | 6/6 PASS |
| 2025-12-28 | Phase 7 | feature-reviewer | Post-review | NEEDS_REVISION |
| 2025-12-28 | Phase 7 | - | Fix feature doc | Complete |
| 2025-12-28 | Phase 7 | feature-reviewer | Re-verify | READY |
| 2025-12-28 | Phase 8 | finalizer | Status update to [DONE] | Complete |

---

## Links

- [index-features.md](../index-features.md)
- [do.md](../../../archive/claude_legacy_20251230/commands/do.md)
- Related:
  - F241 (発見契橁E
  - [F190](feature-190.md) (COM→ファイルマッピング確宁E
  - [F221](feature-221.md) (_挿入.ERB vs _口挿入.ERB 整琁E
  - [F260](feature-260.md) (SSOT準拠提桁E- 本Featureの議論から派甁E
