# Feature 185: Kojo Test Auto-Generation Script

## Status: [DONE]

## Type: infra

## Background

### Problem

test-generator サブエージェント (LLM) が ERB の DATALIST 内容を不正確に抽出し、テスト期待値が実装と一致しないケースが発生。

Feature 182 で発生した問題:
| テスト期待値 (LLM生成) | ERB実際 |
|------------------------|---------|
| `「……むきゅー」` | `「……むきゅ」` |
| `「……っ！？　な、何を……！」` | `「……っ！？　胸に、何を……！」` |

これにより:
1. テスト失敗 → 誤ってテストを修正 (ワークフロー違反)
2. TDD原則の破綻 (テストが実装に合わせて変更された)

### Goal

ERB ファイルから DATALIST を機械的に抽出し、100%正確なテストJSONを自動生成するスクリプトを作成。

### Context

- ERB 構文は規則的で正規表現でパース可能
- DATALIST 構造: `DATALIST` ... `ENDLIST` ブロック
- TALENT 分岐: `IF TALENT:恋人` / `ELSEIF TALENT:恋慕` / etc.
- 既存ツール: `tools/kojo-mapper/kojo_mapper.py` を拡張して実装

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | DATALIST 抽出 (CALL解決) | output | python tools/kojo-mapper/kojo_test_gen.py --function KOJO_MESSAGE_COM_K1_48 Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB | contains | "Found 16 DATALIST blocks" | [x] |
| 2 | 全4分岐識別確認 | output | python tools/kojo-mapper/kojo_test_gen.py --function KOJO_MESSAGE_COM_K1_48 --verbose Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB | contains | "TALENT branches: 恋人, 恋慕, 思慕, なし" | [x] |
| 3 | 生成JSONが実行可能 | exit_code | dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-185/test-185-K1.json | succeeds | - | [x] |
| 4 | Feature 182 再生成で全PASS | output | dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-182/ | contains | "160/160 passed" | [x] |
| 5 | test-generator.md 削除確認 | file | .claude/agents/test-generator.md | not_exists | - | [x] |
| 6 | imple.md で Python 使用 | code | .claude/commands/imple.md | contains | "kojo_test_gen.py" | [x] |
| 7 | imple.md から test-generator 削除 | code | .claude/commands/imple.md | not_contains | "test-generator" | [x] |
| 8 | kojo-writer.md から test-generator 削除 | code | .claude/agents/kojo-writer.md | not_contains | "test-generator" | [x] |
| 9 | CLAUDE.md から test-generator 削除 | code | CLAUDE.md | not_contains | "test-generator" | [x] |

> **Note AC1**: 16 = 4分岐 (恋人/恋慕/思慕/なし) × 4パターン/分岐。KOJO_K1_愛撫.ERB:4071 `@KOJO_MESSAGE_COM_K1_48_1` で確認済み。
> **Note AC4**: 160テスト = 10キャラ × 4分岐 × 4パターン。Feature 182テストはユーザーにより事前削除済み、本Feature実装後に再生成。

### いじわる試験 (Negative Tests)

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 10 | 存在しない関数名 (exit) | exit_code | python tools/kojo-mapper/kojo_test_gen.py --function NONEXISTENT_FUNC Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB | fails | - | [x] |
| 11 | 存在しない関数名 (msg) | output | (AC10と同一コマンド) | contains | "Error: Function 'NONEXISTENT_FUNC' not found" | [x] |
| 12 | DATALISTなし関数 (output) | output | python tools/kojo-mapper/kojo_test_gen.py --function KOJO_MODIFIER_PRE_COMMON Game/ERB/口上/U_汎用/KOJO_MODIFIER_COMMON.ERB | contains | "Found 0 DATALIST blocks" | [x] |
| 13 | DATALISTなし関数 (exit) | exit_code | (AC12と同一コマンド) | succeeds | - | [x] |
| 14 | 存在しないファイル | exit_code | python tools/kojo-mapper/kojo_test_gen.py --function X nonexistent.ERB | fails | - | [x] |
| 15 | 未知プレースホルダ警告 | output | python tools/kojo-mapper/kojo_test_gen.py --test-unknown-placeholder | contains | "Warning: Unknown placeholder" | [x] |

> **Note AC12-13**: `KOJO_MODIFIER_PRE_COMMON`はPRINTFORMLのみ使用、DATALISTなし。正常終了（exit 0）で0件報告。
> **Note AC15**: `--test-unknown-placeholder` オプションでテスト用ダミーERBを生成し警告出力を検証。

---

## Tasks

| Task# | AC# | Target | Description | Status |
|:-----:|:---:|--------|-------------|:------:|
| 1 | 1 | tools/kojo-mapper/kojo_test_gen.py | DATALIST抽出（CALLチェーン解決含む） | [O] |
| 2 | 2 | tools/kojo-mapper/kojo_test_gen.py | TALENT分岐識別（detect_relationship_branches再利用） | [O] |
| 3 | 3 | tools/kojo-mapper/kojo_test_gen.py | テストJSON出力フォーマッタ実装 | [O] |
| 4 | 4 | Game/tests/ac/kojo/feature-182/*.json | Feature 182 テスト再生成・検証 | [O] |
| 5 | 5 | .claude/agents/test-generator.md | test-generator.md 削除 | [O] |
| 6 | 6,7 | .claude/commands/imple.md | test-generator → kojo_test_gen.py 置換 | [O] |
| 7 | 8 | .claude/agents/kojo-writer.md | test-generator 参照削除 | [O] |
| 8 | 9 | CLAUDE.md | test-generator 行削除 | [O] |
| 9 | 10,11 | tools/kojo-mapper/kojo_test_gen.py | 関数未発見時のエラーハンドリング | [O] |
| 10 | 12,13 | tools/kojo-mapper/kojo_test_gen.py | DATALISTなし関数の0件報告（正常終了） | [O] |
| 11 | 14 | tools/kojo-mapper/kojo_test_gen.py | ファイル未発見時のエラーハンドリング | [O] |
| 12 | 15 | tools/kojo-mapper/kojo_test_gen.py | 未知プレースホルダの警告出力 | [O] |

<!-- AC:Task 1:1 validated by ac-task-aligner -->

---

## Design Notes

### スクリプト構成

```
tools/kojo-mapper/
├── kojo_mapper.py       # 既存（分析・カバレッジ用）
├── kojo_test_gen.py     # 新規（テストJSON生成用）
└── README.md            # 使用方法
```

### kojo_mapper.py 再利用部分

| 機能 | 既存関数 | 再利用方法 |
|------|----------|------------|
| ERBパース | `parse_erb_file()` | そのまま使用 |
| 関数抽出 | `KojoFunction` dataclass | import |
| TALENT分岐検出 | `detect_relationship_branches()` | import |
| DATALIST検出 | `has_printdata` フラグ | 拡張してコンテンツ抽出 |

### 新規実装部分

1. **CALL チェーン解決**: `@KOJO_MESSAGE_COM_K1_48` → `CALL KOJO_MESSAGE_COM_K1_48_1` を追跡
2. **DATALIST コンテンツ抽出**: `DATALIST`〜`ENDLIST` 間の `DATAFORM` 行を収集
3. **プレースホルダ展開**: ランタイム変数を固定値に展開
4. **JSON出力**: Feature 182形式のテストJSON生成

### CALLNAME マッピング

| プレースホルダ | 展開値 |
|---------------|--------|
| `%CALLNAME:MASTER%` | あなた |
| `%CALLNAME:人物_美鈴%` | 美鈴 |
| `%CALLNAME:人物_小悪魔%` | 小悪魔 |
| `%CALLNAME:人物_パチュリー%` | パチュリー |
| `%CALLNAME:人物_咲夜%` | 咲夜 |
| `%CALLNAME:人物_レミリア%` | レミリア |
| `%CALLNAME:人物_フラン%` | フラン |
| `%CALLNAME:人物_子悪魔%` | 子悪魔 |
| `%CALLNAME:人物_チルノ%` | チルノ |
| `%CALLNAME:人物_大妖精%` | 大妖精 |
| `%CALLNAME:人物_魔理沙%` | 魔理沙 |

### 出力JSON形式

Feature 182形式を踏襲:
```json
{
  "name": "Feature XXX: K1 COM_YY",
  "defaults": { "character": "1" },
  "tests": [
    {
      "name": "恋人_pattern0",
      "call": "KOJO_MESSAGE_COM_K1_YY",
      "mock_rand": [0],
      "state": {"TALENT:TARGET:16": 1},
      "expect": {
        "output_contains": ["展開済みDATAFORM行..."]
      }
    }
  ]
}
```

### TALENT → state マッピング

| 分岐条件 | state設定 |
|---------|-----------|
| `TALENT:恋人` | `{"TALENT:TARGET:16": 1}` |
| `TALENT:恋慕` | `{"TALENT:TARGET:3": 1}` |
| `TALENT:思慕` | `{"TALENT:TARGET:17": 1}` |
| ELSE (なし) | `{}` |

### CLI仕様

```bash
# 単一関数からテスト生成
python kojo_test_gen.py --function KOJO_MESSAGE_COM_K1_48 \
  --output tests/ac/kojo/feature-185/test-185-K1.json \
  Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB

# Feature全体生成（10キャラ分）
python kojo_test_gen.py --feature 182 --com 48 \
  --output-dir tests/ac/kojo/feature-182/
```

### ERB パース対象

```erb
@KOJO_MESSAGE_COM_K{N}_{COM}
...
IF TALENT:恋人
    PRINTDATA
        DATALIST
            DATAFORM "セリフ1"
            DATAFORM "セリフ2"
        ENDLIST
        DATALIST ... ENDLIST  ; pattern 1
        DATALIST ... ENDLIST  ; pattern 2
        DATALIST ... ENDLIST  ; pattern 3
    ENDDATA
ELSEIF TALENT:恋慕
    ...
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2025-12-23 10:00 | Initialization | initializer | Feature 185 initialized, status set to [WIP] | Ready |
| 2025-12-23 | Pre-cleanup | User | Feature 182テストファイル削除 (test-182-K1~K10.json) | Done |
| 2025-12-23 21:00 | START | implementer | Task 1-12 implementation | - |
| 2025-12-23 21:08 | END | implementer | All 12 tasks completed | SUCCESS |
| 2025-12-23 | Bug Report | User | Feature 182 tests failing: %CALLNAME:TARGET% not expanded | Issue detected |
| 2025-12-23 | Fix | debugger | Add TARGET placeholder expansion to kojo_test_gen.py | FIXED |
| 2025-12-23 | Regenerate | debugger | Regenerate Feature 182 tests with TARGET expansion | 160 tests generated |
| 2025-12-23 | Verify | debugger | Test K3, K4, K7 (previously failing) | All PASS |
| 2025-12-23 22:00 | FINALIZATION | finalizer | Status [WIP] → [DONE], all 15 ACs verified, 12 tasks completed | READY_TO_COMMIT |

---

## Links

- [index-features.md](index-features.md)
- [kojo-mapper (参考)](../../tools/kojo-mapper/)
- [Feature 182 (問題発生元)](feature-182.md)
