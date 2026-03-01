# Feature 268: AC 機械皁E��証の完�E匁E- 全 AC タイプで Log Verification 対忁E


## Status: [DONE]



## Type: infra



## Background



### Philosophy (Mid-term Vision)



**全 Feature で全 AC を機械皁E��査で検証し、AC 基準を満たすことを保証する。オーケストレータがスキチE�EめE��釈により判定を変更できなぁE��真の TDD を実現する、E*



**アーキチE��チャ**: ac-static-verifier.py が静皁E��証 (code/build/file 垁E を実行し JSON ログを生成。verify-logs.py がログを読み取り監査結果を集計、E


核忁E��剁E

1. **機械皁E��証**: 人間�E判断を介さず、�E AC が�E動検証可能

2. **監査証跡**: 全 AC に対応するログファイルが存在し、追跡可能

3. **スキチE�E禁止**: code/build 垁EAC も含め、オーケストレータの裁E��でスキチE�E不可

4. **1:1 対忁E*: AC と検証ログぁE1:1 で対忁E


### Problem (Current Issue)



F262 実行時に `verify-logs.py --scope feature:262` ぁE`OK:0/0` を返した、E


| 問顁E| 詳細 |

|------|------|

| code 垁EAC がログなぁE| Grep 検証は実行されるがログが残らなぁE|

| build 垁EAC がログなぁE| dotnet build 成功確認がログに残らなぁE|

| オーケストレータ裁E�� | Phase 3 スキチE�E条件に code/build 型が含まれる |

| 監査不�E | 後から「本当に検証したか」を確認できなぁE|



現状の do.md Phase 3:

```

**Skip conditions**: AC already has test, AC Type=build/file/code, Feature Type=infra/kojo

```



ↁEこれにより code/build 垁EAC は TDD から除外されてぁE��、E


### Goal (What to Achieve)



1. **全 AC タイプに対応した検証 JSON 生�E** - code/build/file 型も含む

2. **verify-logs.py が�E AC をカウンチE* - `OK:12/12` のように全 AC が計上される

3. **Phase 3 AC Type スキチE�E条件の撤廁E* - code/build/file 垁EAC で TDD 適用 (Feature Type=infra/kojo スキチE�Eは維持E

4. **AC:Log 1:1 対忁E* - 吁EAC に対応するログファイルが忁E��存在



---



## Acceptance Criteria



### AC Definition Table



| AC# | Description | Type | Method | Matcher | Expected | Status |

|:---:|-------------|------|--------|---------|----------|:------:|

| 1 | ac-static-verifier.py 存在 | file | Glob | exists | src/tools/python/ac-static-verifier.py | [x] |

| 2 | code 垁EAC 検証成功 (exit 0) | exit_code | Bash | succeeds | python src/tools/python/ac-static-verifier.py --feature 268 --ac-type code | [x] |

| 3 | code 型ログファイル生�E | file | Glob | exists | _out/logs/prod/ac/code/feature-268/code-result.json | [x] |

| 4 | build 垁EAC 検証成功 (exit 0) | exit_code | Bash | succeeds | python src/tools/python/ac-static-verifier.py --feature 268 --ac-type build | [x] |

| 5 | build 型ログファイル生�E | file | Glob | exists | _out/logs/prod/ac/build/feature-268/build-result.json | [x] |

| 6 | file 垁EAC 検証成功 (exit 0) | exit_code | Bash | succeeds | python src/tools/python/ac-static-verifier.py --feature 268 --ac-type file | [x] |

| 7 | file 型ログファイル生�E | file | Glob | exists | _out/logs/prod/ac/file/feature-268/file-result.json | [x] |

| 8 | verify-logs.py が新ログを読叁E(N > 0) | output | Bash | matches | Feature-268:.*OK:[1-9][0-9]* | [x] |

| 9 | do.md Skip 条件 combined pattern なぁE| code | Grep(.claude/commands/do.md) | not_contains | AC Type=build/file/code | [x] |

| 10 | (reserved - AC 9 に統合済み) | - | - | - | - | [B] |

| 11 | (reserved - AC 9 に統合済み) | - | - | - | - | [B] |

| 12 | testing SKILL に静的検証ドキュメント追加 | code | Grep(.claude/skills/testing/SKILL.md) | contains | ac-static-verifier.py | [x] |

| 13 | ビルド�E劁E(bootstrap) | build | dotnet build | succeeds | dotnet build engine/uEmuera.Headless.csproj | [x] |

| 14 | 回帰チE��チEPASS (bootstrap) | test | --flow | succeeds | all scenarios pass (exit 0) | [x] |



### AC Details



**AC 1**: ac-static-verifier.py の存在確誁E
```bash

ls src/tools/python/ac-static-verifier.py  # exists

```



**AC 2-7**: 吁EAC タイプ�E検証成功とログファイル生�E



**Note**: AC 3, 5, 7 は AC 2, 4, 6 の実行結果として生�Eされるログを検証する。テスト頁E��依存があるため、AC 2ↁE, AC 4ↁE, AC 6ↁE の頁E��で実行すること、E


```bash

# code 垁E
python src/tools/python/ac-static-verifier.py --feature 268 --ac-type code  # exit 0 (AC 2)

# AC 3: Glob("_out/logs/prod/ac/code/feature-268/code-result.json") - file exists



# build 垁E
python src/tools/python/ac-static-verifier.py --feature 268 --ac-type build  # exit 0 (AC 4)

# AC 5: Glob("_out/logs/prod/ac/build/feature-268/build-result.json") - file exists



# file 垁E
python src/tools/python/ac-static-verifier.py --feature 268 --ac-type file  # exit 0 (AC 6)

# AC 7: Glob("_out/logs/prod/ac/file/feature-268/file-result.json") - file exists

```



**AC 8**: verify-logs.py が新ログチE��レクトリを読叁E(N > 0 検証)



**Dependency**: Task 5 完亁E��に検証可能。verify-logs.py ぁE_out/logs/prod/ac/{code,build,file}/ チE��レクトリを読み取る忁E��がある、E


**Test Command**:

```bash

python src/tools/python/verify-logs.py --scope feature:268

# 出力侁E Feature-268:         OK:3/3

# ↁEパターン 'Feature-268:.*OK:[1-9]' にマッチすれ�E PASS

# ↁEOK:0/0 はマッチしなぁE��めEFAIL (本 Feature が解決する問顁E

```



**AC 9**: do.md Phase 3 Skip 条件から AC Type combined pattern を除夁E


**Assumption**: do.md の Skip 条件は `AC Type=build/file/code` の combined pattern として記載されており、個別の `AC Type=build`, `AC Type=file`, `AC Type=code` が別途追加されることはなぁE��そのため combined pattern の not_contains チェチE��で十�E、E


**Test Command**:

```bash

# Grep with output_mode: content, verify 0 matches

grep -c "AC Type=build/file/code" .claude/commands/do.md  # Expected: 0 or "no match"

```



```markdown

# 修正剁E
Skip conditions: AC already has test, AC Type=build/file/code, Feature Type=infra/kojo



# 修正征E
Skip conditions: AC already has test, Feature Type=infra/kojo

```



**AC 10-11**: (reserved) - AC 9 に統合済み



**AC 13-14**: ビルチE回帰チE��チE(bootstrap)



**Bootstrap Note**: AC 13 (build) と AC 14 (test) は F268 が解決しよぁE��してぁE��「ログなし」問題�E対象 AC タイプである、E268 自身のこれら�E AC は既存�E do.md Phase 6 フローで検証され、ac-static-verifier.py による検証は封E��の Feature から適用される、E*F268 は ac-static-verifier.py を作�Eする Feature であるため、検証時点でチE�Eルが存在しなぁE��これが bootstrap 例外�E琁E��である、E*



**Manual Verification**: AC 13/14 は F268 において手動検証で PASS とマ�Eクされる（ログ生�Eなし）、E*AC 13/14 は F268 限定�E bootstrap 例外である、E269 以降�E全 Feature は build/test AC タイプに ac-static-verifier.py を使用しなければならなぁE��E*



---



## Tasks



**Exception to AC:Task 1:1 Rule**: Task 2-4 は AC ペアが頁E��依存（実行�Eログ検証�E��Eため結合、E*AC ペアに実行頁E��依存がある場合！Execute ↁEverify result�E�、E Task で褁E�� AC を検証することを許可する、E*



| Task# | AC# | Description | Status |

|:-----:|:---:|-------------|:------:|

| 1 | 1 | ac-static-verifier.py スケルトン作�E | [X] |

| 2 | 2,3 | code 垁EAC 検証機�E実裁E(Grep + JSON 出劁E | [X] |

| 3 | 4,5 | build 垁EAC 検証機�E実裁E(dotnet build + JSON 出劁E | [X] |

| 4 | 6,7 | file 垁EAC 検証機�E実裁E(Glob + JSON 出劁E | [X] |

| 5 | 8 | verify-logs.py 動作確認�Eみ (既孁Eglob パターン対応済み、変更不要想宁E | [X] |

| 6 | 9 | do.md Phase 3 スキチE�E条件から AC Type=build/file/code を削除 | [X] |

| 7 | 12 | testing SKILL に静的検証ドキュメント追加 | [X] |

| 8 | 13 | ビルド確誁E| [X] |

| 9 | 14 | 回帰チE��ト実衁E| [X] |



---



## Design Notes



### アーキチE��チャ方釁E


**関忁E�E刁E��**:

- **Engine (headless)**: ERB 実行専用 (--unit, --flow)

- **Python tools**: 静的検証 (code/build/file 垁EAC)



Engine は ERB ランタイムに特化し、grep/build のような静的検証は Python チE�Eルで実裁E��る、E


### ac-static-verifier.py 仕様桁E


**入劁E*: feature-{ID}.md から AC 定義を読み取り、Type=code/build/file の AC を検証



**AC Table ↁEVerifier Field Mapping**:



| AC Table Column | Verifier Input | Description |

|-----------------|----------------|-------------|

| Type | verification_type | code/build/file を判定に使用 |

| Method | file_path (if Grep) | Grep(path) の場合、path を抽出 |

| Expected | pattern/path | 検証対象パターンまた�Eパス |

| Matcher | assertion_type | contains/not_contains/exists/succeeds |



**code 型検証** (Grep ベ�Eス):

```python

# feature-268.md の AC を解析し、Grep で検証

# Method: Grep(file_path) ↁEfile_path を抽出

# Expected: pattern ↁE検索パターン

# Matcher: contains/not_contains ↁEマッチ有無判宁E
```



**build 型検証**:

```python

# dotnet build を実行し、exit code で判宁E
```



**file 型検証** (Glob ベ�Eス):

```python

# ファイル存在確誁E
```



**出劁E*: `_out/logs/prod/ac/{type}/feature-{ID}/{type}-result.json`



**Path Convention**: 全パスはリポジトリルートから�E相対パス。verify-logs.py のチE��ォルトディレクトリは `_out/logs/prod`、E


**Note**: code/build/file チE��レクトリは既存�E kojo/engine と並列に配置される、E


**Windows 環墁E��愁E*: Glob チE�Eルは Windows で動作が不安定な場合がある (CLAUDE.md 参�E)。そのため、ファイル名�E予測可能な形弁E`{type}-result.json` を使用する、EC チE�Eブルでは `*-result.json` パターンを使用するが、実際には 1 ファイルのみ生�Eされる、E


```

_out/logs/prod/ac/

├── kojo/feature-*/     ↁE既孁E(口丁EAC ログ)

├── engine/             ↁE既孁E(エンジン AC ログ)

├── code/feature-268/code-result.json    ↁENEW: code 垁EAC の検証ログ

├── build/feature-268/build-result.json  ↁENEW: build 垁EAC の検証ログ

└── file/feature-268/file-result.json    ↁENEW: file 垁EAC の検証ログ

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



### 実裁E��釁E


1. **ac-static-verifier.py**: code/build/file 垁EAC を検証ぁEJSON ログ出劁E
2. **verify-logs.py**: logs/prod/ac/{code,build,file}/ チE��レクトリを読み取り、カウントに含める

3. **do.md**: Phase 3 で ac-static-verifier.py を呼び出し、�E AC タイプを検証

4. **testing SKILL**: 静的検証パイプラインのドキュメント追加



### 影響篁E��



| Component | Change |

|-----------|--------|

| `src/tools/python/ac-static-verifier.py` | 新規作�E |

| `src/tools/python/verify-logs.py` | 動作確認�Eみ (既孁Eglob パターン `**/{scope}/*-result.json` で対応済み) |

| `.claude/commands/do.md` | Phase 3 修正 |

| `.claude/skills/testing/SKILL.md` | 静的検証ドキュメンチE|



---



## Review Notes



---



## Execution Log



| Timestamp | Event | Agent | Action | Result |

|-----------|:-----:|-------|--------|--------|

| 2025-12-30 | create | opus | F262 実行中の監査課題かめEFeature 作�E | - |

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



- [feature-262.md](feature-262.md) - 本 Feature のきっかけ (OK:0/0 問顁E

- [do.md](../../../archive/claude_legacy_20251230/commands/do.md) - Phase 3 スキチE�E条件

- [testing/SKILL.md](../../../archive/claude_legacy_20251230/skills/testing/SKILL.md) - AC タイプ定義

