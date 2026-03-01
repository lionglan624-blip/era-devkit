# Feature 185: Kojo Test Auto-Generation Script



## Status: [DONE]



## Type: infra



## Background



### Problem



test-generator サブエージェンチE(LLM) ぁEERB の DATALIST 冁E��を不正確に抽出し、テスト期征E��が実裁E��一致しなぁE��ースが発生、E


Feature 182 で発生した問顁E

| チE��ト期征E�� (LLM生�E) | ERB実際 |

|------------------------|---------|

| `「……むきゅー」` | `「……むきゅ」` |

| `「……っ�E�E��　な、何を……�E�」` | `「……っ�E�E��　胸に、何を……�E�」` |



これにより:

1. チE��ト失敁EↁE誤ってチE��トを修正 (ワークフロー違反)

2. TDD原則の破綻 (チE��トが実裁E��合わせて変更されぁE



### Goal



ERB ファイルから DATALIST を機械皁E��抽出し、E00%正確なチE��チESONを�E動生成するスクリプトを作�E、E


### Context



- ERB 構文は規則皁E��正規表現でパ�Eス可能

- DATALIST 構造: `DATALIST` ... `ENDLIST` ブロチE��

- TALENT 刁E��E `IF TALENT:恋人` / `ELSEIF TALENT:恋�E` / etc.

- 既存ツール: `src/tools/kojo-mapper/kojo_mapper.py` を拡張して実裁E


---



## Acceptance Criteria



| AC# | Description | Type | Method | Matcher | Expected | Status |

|:---:|-------------|------|--------|---------|----------|:------:|

| 1 | DATALIST 抽出 (CALL解決) | output | python src/tools/kojo-mapper/kojo_test_gen.py --function KOJO_MESSAGE_COM_K1_48 Game/ERB/口丁E1_美鈴/KOJO_K1_愛撫.ERB | contains | "Found 16 DATALIST blocks" | [x] |

| 2 | 全4刁E��識別確誁E| output | python src/tools/kojo-mapper/kojo_test_gen.py --function KOJO_MESSAGE_COM_K1_48 --verbose Game/ERB/口丁E1_美鈴/KOJO_K1_愛撫.ERB | contains | "TALENT branches: 恋人, 恋�E, 思�E, なぁE | [x] |

| 3 | 生�EJSONが実行可能 | exit_code | dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-185/test-185-K1.json | succeeds | - | [x] |

| 4 | Feature 182 再生成で全PASS | output | dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-182/ | contains | "160/160 passed" | [x] |

| 5 | test-generator.md 削除確誁E| file | .claude/agents/test-generator.md | not_exists | - | [x] |

| 6 | imple.md で Python 使用 | code | .claude/commands/imple.md | contains | "kojo_test_gen.py" | [x] |

| 7 | imple.md から test-generator 削除 | code | .claude/commands/imple.md | not_contains | "test-generator" | [x] |

| 8 | kojo-writer.md から test-generator 削除 | code | .claude/agents/kojo-writer.md | not_contains | "test-generator" | [x] |

| 9 | CLAUDE.md から test-generator 削除 | code | CLAUDE.md | not_contains | "test-generator" | [x] |



> **Note AC1**: 16 = 4刁E��E(恋人/恋�E/思�E/なぁE ÁE4パターン/刁E��、EOJO_K1_愛撫.ERB:4071 `@KOJO_MESSAGE_COM_K1_48_1` で確認済み、E
> **Note AC4**: 160チE��チE= 10キャラ ÁE4刁E��EÁE4パターン、Eeature 182チE��ト�Eユーザーにより事前削除済み、本Feature実裁E��に再生成、E


### ぁE��わる試騁E(Negative Tests)



| AC# | Description | Type | Method | Matcher | Expected | Status |

|:---:|-------------|------|--------|---------|----------|:------:|

| 10 | 存在しなぁE��数吁E(exit) | exit_code | python src/tools/kojo-mapper/kojo_test_gen.py --function NONEXISTENT_FUNC Game/ERB/口丁E1_美鈴/KOJO_K1_愛撫.ERB | fails | - | [x] |

| 11 | 存在しなぁE��数吁E(msg) | output | (AC10と同一コマンチE | contains | "Error: Function 'NONEXISTENT_FUNC' not found" | [x] |

| 12 | DATALISTなし関数 (output) | output | python src/tools/kojo-mapper/kojo_test_gen.py --function KOJO_MODIFIER_PRE_COMMON Game/ERB/口丁EU_汎用/KOJO_MODIFIER_COMMON.ERB | contains | "Found 0 DATALIST blocks" | [x] |

| 13 | DATALISTなし関数 (exit) | exit_code | (AC12と同一コマンチE | succeeds | - | [x] |

| 14 | 存在しなぁE��ァイル | exit_code | python src/tools/kojo-mapper/kojo_test_gen.py --function X nonexistent.ERB | fails | - | [x] |

| 15 | 未知プレースホルダ警呁E| output | python src/tools/kojo-mapper/kojo_test_gen.py --test-unknown-placeholder | contains | "Warning: Unknown placeholder" | [x] |



> **Note AC12-13**: `KOJO_MODIFIER_PRE_COMMON`はPRINTFORMLのみ使用、DATALISTなし。正常終亁E��Exit 0�E�で0件報告、E
> **Note AC15**: `--test-unknown-placeholder` オプションでチE��ト用ダミ�EERBを生成し警告�E力を検証、E


---



## Tasks



| Task# | AC# | Target | Description | Status |

|:-----:|:---:|--------|-------------|:------:|

| 1 | 1 | src/tools/kojo-mapper/kojo_test_gen.py | DATALIST抽出�E�EALLチェーン解決含む�E�E| [O] |

| 2 | 2 | src/tools/kojo-mapper/kojo_test_gen.py | TALENT刁E��識別�E�Eetect_relationship_branches再利用�E�E| [O] |

| 3 | 3 | src/tools/kojo-mapper/kojo_test_gen.py | チE��チESON出力フォーマッタ実裁E| [O] |

| 4 | 4 | test/ac/kojo/feature-182/*.json | Feature 182 チE��ト�E生�E・検証 | [O] |

| 5 | 5 | .claude/agents/test-generator.md | test-generator.md 削除 | [O] |

| 6 | 6,7 | .claude/commands/imple.md | test-generator ↁEkojo_test_gen.py 置揁E| [O] |

| 7 | 8 | .claude/agents/kojo-writer.md | test-generator 参�E削除 | [O] |

| 8 | 9 | CLAUDE.md | test-generator 行削除 | [O] |

| 9 | 10,11 | src/tools/kojo-mapper/kojo_test_gen.py | 関数未発見時のエラーハンドリング | [O] |

| 10 | 12,13 | src/tools/kojo-mapper/kojo_test_gen.py | DATALISTなし関数の0件報告（正常終亁E��E| [O] |

| 11 | 14 | src/tools/kojo-mapper/kojo_test_gen.py | ファイル未発見時のエラーハンドリング | [O] |

| 12 | 15 | src/tools/kojo-mapper/kojo_test_gen.py | 未知プレースホルダの警告�E劁E| [O] |



<!-- AC:Task 1:1 validated by ac-task-aligner -->



---



## Design Notes



### スクリプト構�E



```

src/tools/kojo-mapper/

├── kojo_mapper.py       # 既存（�E析�EカバレチE��用�E�E
├── kojo_test_gen.py     # 新規（テスチESON生�E用�E�E
└── README.md            # 使用方況E
```



### kojo_mapper.py 再利用部刁E


| 機�E | 既存関数 | 再利用方況E|

|------|----------|------------|

| ERBパ�Eス | `parse_erb_file()` | そ�Eまま使用 |

| 関数抽出 | `KojoFunction` dataclass | import |

| TALENT刁E��検�E | `detect_relationship_branches()` | import |

| DATALIST検�E | `has_printdata` フラグ | 拡張してコンチE��チE��出 |



### 新規実裁E��刁E


1. **CALL チェーン解決**: `@KOJO_MESSAGE_COM_K1_48` ↁE`CALL KOJO_MESSAGE_COM_K1_48_1` を追跡

2. **DATALIST コンチE��チE��出**: `DATALIST`〜`ENDLIST` 間�E `DATAFORM` 行を収集

3. **プレースホルダ展開**: ランタイム変数を固定値に展開

4. **JSON出劁E*: Feature 182形式�EチE��チESON生�E



### CALLNAME マッピング



| プレースホルダ | 展開値 |

|---------------|--------|

| `%CALLNAME:MASTER%` | あなぁE|

| `%CALLNAME:人物_美鈴%` | 美鈴 |

| `%CALLNAME:人物_小悪魁E` | 小悪魁E|

| `%CALLNAME:人物_パチュリー%` | パチュリー |

| `%CALLNAME:人物_咲夁E` | 咲夁E|

| `%CALLNAME:人物_レミリア%` | レミリア |

| `%CALLNAME:人物_フラン%` | フラン |

| `%CALLNAME:人物_子悪魁E` | 子悪魁E|

| `%CALLNAME:人物_チルチE` | チルチE|

| `%CALLNAME:人物_大妖精%` | 大妖精 |

| `%CALLNAME:人物_魔理沁E` | 魔理沁E|



### 出力JSON形弁E


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

        "output_contains": ["展開済みDATAFORM衁E.."]

      }

    }

  ]

}

```



### TALENT ↁEstate マッピング



| 刁E��条件 | state設宁E|

|---------|-----------|

| `TALENT:恋人` | `{"TALENT:TARGET:16": 1}` |

| `TALENT:恋�E` | `{"TALENT:TARGET:3": 1}` |

| `TALENT:思�E` | `{"TALENT:TARGET:17": 1}` |

| ELSE (なぁE | `{}` |



### CLI仕槁E


```bash

# 単一関数からチE��ト生戁E
python kojo_test_gen.py --function KOJO_MESSAGE_COM_K1_48 \

  --output tests/ac/kojo/feature-185/test-185-K1.json \

  Game/ERB/口丁E1_美鈴/KOJO_K1_愛撫.ERB



# Feature全体生成！E0キャラ刁E��E
python kojo_test_gen.py --feature 182 --com 48 \

  --output-dir tests/ac/kojo/feature-182/

```



### ERB パ�Eス対象



```erb

@KOJO_MESSAGE_COM_K{N}_{COM}

...

IF TALENT:恋人

    PRINTDATA

        DATALIST

            DATAFORM "セリチE"

            DATAFORM "セリチE"

        ENDLIST

        DATALIST ... ENDLIST  ; pattern 1

        DATALIST ... ENDLIST  ; pattern 2

        DATALIST ... ENDLIST  ; pattern 3

    ENDDATA

ELSEIF TALENT:恋�E

    ...

```



---



## Execution Log



| Timestamp | Event | Agent | Action | Result |

|-----------|-------|-------|--------|--------|

| 2025-12-23 10:00 | Initialization | initializer | Feature 185 initialized, status set to [WIP] | Ready |

| 2025-12-23 | Pre-cleanup | User | Feature 182チE��トファイル削除 (test-182-K1~K10.json) | Done |

| 2025-12-23 21:00 | START | implementer | Task 1-12 implementation | - |

| 2025-12-23 21:08 | END | implementer | All 12 tasks completed | SUCCESS |

| 2025-12-23 | Bug Report | User | Feature 182 tests failing: %CALLNAME:TARGET% not expanded | Issue detected |

| 2025-12-23 | Fix | debugger | Add TARGET placeholder expansion to kojo_test_gen.py | FIXED |

| 2025-12-23 | Regenerate | debugger | Regenerate Feature 182 tests with TARGET expansion | 160 tests generated |

| 2025-12-23 | Verify | debugger | Test K3, K4, K7 (previously failing) | All PASS |

| 2025-12-23 22:00 | FINALIZATION | finalizer | Status [WIP] ↁE[DONE], all 15 ACs verified, 12 tasks completed | READY_TO_COMMIT |



---



## Links



- [index-features.md](../index-features.md)

- [kojo-mapper (参老E](../../src/tools/kojo-mapper/)

- [Feature 182 (問題発生�E)](feature-182.md)

