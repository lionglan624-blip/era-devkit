# Feature 204: Test Infrastructure & Workflow Fixes

## Status: [DONE]

**Note**: Feature 203 (調査) の結果を受けて修正を実施する、E

## Type: infra

## Background

### Problem

Feature 186 (COM_60 正常佁E口丁E の実裁E��に、Eimple ワークフローの重大な欠陥が発覚した、E

**発覚した問題一覧**:

| # | カチE��リ | 問顁E| 影響 | F202状慁E|
|:-:|----------|------|------|:--------:|
| 1 | エンジン | flow test シナリオぁEFAIL | 回帰チE��チE3/24 FAIL | 未対忁E|
| 2 | サブエージェンチE| regression-tester が虚偽報呁E| 、E12/512 PASS」�E実際は 21/24 | 対応済※ |
| 3 | ドキュメンチE| kojo で dotnet test 未定義 | Opus が勝手に実行�E報呁E| 未対忁E|
| 4 | Python | kojo_test_gen.py ぁECOM_60 非対忁E| Phase 5 実行不可 | 未対忁E|
| 5 | ドキュメンチE| 回帰チE��ト件数・パス未定義 | 検証基準が曖昧 | 対応渁E|
| 6 | 運用 | 本番ログ収集の仕絁E��なぁE| 結果報告�E信頼性なぁE| 未対忁E|
| 7 | チE��トデータ | チE��ト名ぁE"Feature XXX" | 自動生成�E不備 | 未対忁E|

**※F202対応済�E補足**:
- #2: regression-tester.md にコマンド�ESUMMARY報告指示追加済。しかし Opus (imple.md) 側の検証義務が不十刁E��っぁE
- #5: regression-tester.md に `tests/regression/` (24件) 明記渁E

### F202との関俁E

F202 [DONE] は以下を「対応済み」としたが、一部は**実効性がなぁE*:

| F202対応頁E�� | 状慁E| 問顁E|
|-------------|:----:|------|
| regression-tester.md PRE-EXISTING判宁E| ✁E| 機�EしてぁE�� |
| regression-tester.md SUMMARYエビデンス報呁E| ⚠�E�E| 報告指示はあるがOpusが検証しなかっぁE|
| imple.md Phase 7 ルーチE| ✁E| 機�EしてぁE�� |
| imple.md DEVIATION報呁E| ✁E| 機�EしてぁE�� |
| imple.md Issues Found | ✁E| 機�EしてぁE�� |
| kojo-writer.md Hook BOM | ✁E| 機�EしてぁE�� |
| ac-tester.md {auto}→実値 | ✁E| 機�EしてぁE�� |
| imple.md Phase 8 盲信禁止 | ❁E| **斁E��あるが機�Eせず** (検証コマンドなぁE |

**F204の方釁E*: F203 (調査) で発見した問題を全て修正する、E

**対応頁E��** (F203 調査結果で更新):
1. flow test: `--flow` で実行すべきを `--unit` と誤訁EↁE**ドキュメント修正**
2. kojo_test_gen.py ぁECOM_48 専用ハ�EドコーチEↁE**Python 修正**
3. subagents に Skill tool がなぁEↁE**subagent 修正**
4. hooks の動作確誁EↁE**F203 調査結果征E��**
5. kojo で dotnet test 不要�E明記なぁEↁE**skill/subagent 修正**

### チE��ト件数の定義

| カチE��リ | 件数 | 意味 |
|----------|:----:|------|
| Regression Tests | 24 | `tests/regression/*.json` シナリオ数 |
| Kojo AC Tests | 51 | `tests/ac/kojo/**/*.json` ファイル数 |
| AC冁E��ストケース | 512+ | 各JSONファイル冁E�E個別チE��ト合訁E|

F186 Execution Log の、E12/512」�E AC冁E��ストケース数。、E4/24」�E Regression Tests、E

### Why This Happened

**根本原因**: /imple ワークフローとチE�Eルが断牁E��に開発され、統合テストされてぁE��ぁE��E

1. **kojo_test_gen.py**: Feature 182 (COM_48) 用にハ�EドコーチEↁE仁ECOM 非対忁E
   - `CHAR_MAP` が�Eて `_愛撫.ERB` 固宁E(COM_60 は `_挿入.ERB`)
2. **imple.md (Opus向け)**: サブエージェント報告�E検証義務なぁEↁE盲信
3. **imple.md**: Type 別チE��ト定義があぁE��ぁEↁEkojo で dotnet test 実衁E
4. **flow test**: Parse() で許可、RunWithCapture() で未実裁EↁE不整吁E

### Goal

1. ~~回帰チE��チE24/24 PASS を達戁E(flow test 修正)~~ ↁE**F206/F207 に刁E��**
2. F202で機�Eしなかった頁E��めE*直るまで修正**
3. kojo_test_gen.py を現存する口上ファイルに対忁E
4. F203 監査結果の Subagent Skill tool 問題を修正
5. 修正後、Feature 186 を�E検証して完亁E��せる

---

## Acceptance Criteria

### Part A: flow test 修正

**スコープ変更**: flow test のエンジン実裁E��正は別 Feature (F206/F207) に刁E��、E
本 Feature では Part A をスキチE�Eし、Part B 以降を対象とする、E

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| A1 | (F207 に移管) scenario-alice-sameroom.json PASS | - | - | - | [-] |
| A2 | (F207 に移管) scenario-conversation.json PASS | - | - | - | [-] |
| A3 | (F207 に移管) scenario-daiyousei-sameroom.json PASS | - | - | - | [-] |
| A4 | (F207 に移管) 全回帰チE��チEPASS | - | - | - | [-] |

**琁E��**: flow test の正しい実裁E��針を調査してから対応するため、E

### Part B: kojo_test_gen.py 修正

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| B1 | COM→ファイルマッピング表存在 | code | contains | "COM_FILE_MAP" | [x] |
| B2 | COM_60 でチE��ト生成�E劁E| exit_code | equals | 0 | [x] |
| B3 | 生�EチE��ト名に Feature ID | output | contains | "Feature 999" | [x] |
| B4a | 愛撫カチE��リ対忁E| code | contains | "_愛撫.ERB" | [x] |
| B4b | 乳首責めカチE��リ対忁E| code | contains | "_乳首責めEERB" | [x] |
| B4c | 挿入カチE��リ対忁E| code | contains | "_挿入.ERB" | [x] |

**Note**: 存在するファイルのみ対応。`_道�E.ERB`, `_口挿入.ERB` は現状ファイルが存在しなぁE��め未対応COMはエラーとする、E

**検証方況E*:
```bash
# B2: COM_60 チE��ト生戁E
python src/tools/kojo-mapper/kojo_test_gen.py --feature 999 --com 60 --output-dir .tmp/test-b2/
# ↁEexit 0, 10 files generated

# B3: チE��ト名確誁E
grep "Feature 999" .tmp/test-b2/*.json
```

### Part C: imple.md Phase 8 修正

**現状確誁E*: regression-tester.md は既に十�Eな記載がある
- `tests/regression/` パス ✁E
- 24件明訁E✁E
- 実行コマンチE(`--flow`) ✁E
- `logs/regression/` 参�E ✁E
- SUMMARY 行報告指示 ✁E

**問顁E**: imple.md (Opus 用) に Phase 8 の具体的持E��がなぁE
**問顁E**: Opus がサブエージェント報告を盲信した
**問顁E**: Opus ぁEkojo で dotnet test を勝手に実行しぁE

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| C1 | Opus 直接実行コマンド記輁E| file | contains | "--flow tests/regression/" | [x] |
| C2 | 24/24 目視確認指示 | file | contains | "24/24" | [x] |
| C3 | サブエージェント報告検証義勁E| file | contains | "盲目皁E��信頼しなぁE | [x] |
| C4 | kojo で dotnet test 不要�E訁E| file | contains | "kojo では dotnet test" | [x] |

**F202完亁E��誁E*:
- C3: ✁E既に imple.md Phase 8 に「サブエージェント報告を盲目皁E��信頼しなぁE��が追記済み
- C2: ⚠�E�Eregression-tester.md には24件明記あり、imple.mdには部刁E��
- C1: ❁E`--flow tests/regression/` は regression-tester.md にあるぁEimple.md (Opus向け) になぁE
- C4: ❁EType Routing チE�Eブルに `dotnet test` 列があるぁEkojo で不要�E明記なぁE

### Part D: ビルド検証

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| D1 | dotnet build 成功 | build | succeeds | - | [x] |
| D2 | dotnet test 成功 | test | succeeds | - | [x] |

### Part E: F203 監査結果修正 (Subagent Skill tool)

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| E1 | regression-tester に Skill tool 追加 | file | contains | "Skill" | [x] |
| E2 | ac-tester に Skill tool 追加 | file | contains | "Skill" | [x] |
| E3 | feature-reviewer に Skill tool 追加 | file | contains | "Skill" | [x] |
| E4 | spec-writer に Skill tool 追加 | file | contains | "Skill" | [x] |
| E5 | implementer に skill 参�E持E��追加 | file | contains | "erb-syntax" | [x] |
| E6 | feasibility-checker に skill 参�E持E��追加 | file | contains | "testing" | [x] |

**Note**: F203 問顁E15 (testing skill と agent.md の惁E��重褁E��琁E は大規模なため別 Feature に刁E��、E

---

## Tasks

### Phase 1: エンジン修正 (F207 に移管)

**スコープ変更**: flow test 修正は F206/F207 に刁E��、E

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1.1 | A1-A3 | (F207) KojoTestRunner に flow test 刁E��追加 | KojoTestRunner.cs | [-] |
| 1.2 | A1-A3 | (F207) flow test 用の実行パス実裁E| KojoTestRunner.cs | [-] |
| 1.3 | A4 | (F207) 回帰チE��ト�E件 PASS 確誁E| - | [-] |

### Phase 2: Python 修正

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 2.1 | B1,B4 | COM→ファイルマッピング表追加 | kojo_test_gen.py | [O] |
| 2.2 | B2 | batch generation でマッピング参�E | kojo_test_gen.py | [O] |
| 2.3 | B3 | チE��ト名に Feature ID 反映 | kojo_test_gen.py | [O] |
| 2.4 | B1-B4 | COM_60 でチE��ト生成確誁E| - | [O] |

### Phase 3: ドキュメント修正 (imple.md)

**注愁E*: regression-tester.md は既に十�E。imple.md (Opus 用) に追記が忁E��、E

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 3.1 | C1 | imple.md Phase 8 に Opus 直接実行コマンド追加 | .claude/commands/imple.md | [O] |
| 3.2 | C2 | imple.md Phase 8 に 24/24 目視確認指示追加 | .claude/commands/imple.md | [O] |
| 3.3 | C3 | (F202完亁E��E サブエージェント報告検証義勁E| .claude/commands/imple.md | [O] |
| 3.4 | C4 | imple.md に kojo で dotnet test 不要�E訁E| .claude/commands/imple.md | [O] |

### Phase 4: 検証

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 4.1 | D1,D2 | ビルド�EチE��ト確誁E| [O] |
| 4.2 | A4 | 回帰チE��チE24/24 確誁E| [-] (F207) |
| 4.3 | B2 | kojo_test_gen.py COM_60 確誁E| [O] |

### Phase 5: F203 監査結果修正 (Subagent Skill tool)

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 5.1 | E1 | regression-tester に Skill tool 追加 | .claude/agents/regression-tester.md | [O] |
| 5.2 | E2 | ac-tester に Skill tool 追加 | .claude/agents/ac-tester.md | [O] |
| 5.3 | E3 | feature-reviewer に Skill tool 追加 | .claude/agents/feature-reviewer.md | [O] |
| 5.4 | E4 | spec-writer に Skill tool 追加 | .claude/agents/spec-writer.md | [O] |
| 5.5 | E5 | implementer に skill 参�E持E��追加 | .claude/agents/implementer.md | [O] |
| 5.6 | E6 | feasibility-checker に skill 参�E持E��追加 | .claude/agents/feasibility-checker.md | [O] |

---

## Technical Details

### Task 1.1-1.2: KojoTestRunner flow test 対忁E(F207 に移管)

**F206 (調査) ↁEF207 (実裁E で対応予宁E*。正しい実裁E��針を調査してから実施、E

現状の scenario-*.json は状態注入のみで関数呼び出しがなぁE��E
何を PASS/FAIL とすべきか要調査:
- 状態注入成功 = PASS?
- 特定�E状態�E移を検証?
- ゲーム冁E��ベント発火を検証?

### Task 2.1: COM→ファイルマッピング表

```python
# kojo_test_gen.py に追加
# 実際に存在するファイルのみ対忁E
COM_FILE_MAP = {
    # 愛撫系 (0-6, 8-9) ↁE_愛撫.ERB
    0: "_愛撫.ERB",   # 愛撫
    1: "_愛撫.ERB",   # クンチE
    2: "_愛撫.ERB",   # フェラする
    3: "_愛撫.ERB",   # 持E��入めE
    4: "_愛撫.ERB",   # アナル舐め
    5: "_愛撫.ERB",   # アナル愛撫
    6: "_愛撫.ERB",   # 胸愛撫
    8: "_愛撫.ERB",   # 秘貝開帳
    9: "_愛撫.ERB",   # 自慰
    # 乳首責め系 (7, 10, 11) ↁE_乳首責めEERB
    7: "_乳首責めEERB",   # 乳首責めE
    10: "_乳首責めEERB",  # 乳首吸ぁE
    11: "_乳首責めEERB",  # 乳首吸わせ
    # 挿入系 (60-72) ↁE_挿入.ERB
    **{i: "_挿入.ERB" for i in range(60, 73)},
}

def get_erb_file_for_com(com_num: int) -> str:
    if com_num in COM_FILE_MAP:
        return COM_FILE_MAP[com_num]
    raise ValueError(f"Unsupported COM: {com_num} (no kojo file exists)")
```

**Note**: 道�E系 (40-48), 手技系 (80-85) 等�E口上ファイルが未実裁E�Eため対象外、E

### Task 3.1-3.2: ドキュメント修正

**imple.md Phase 8 追訁E*:
```markdown
## Phase 8: Regression

**チE��ト対象**: `tests/regression/` (24 シナリオ)

**実行コマンチE*:
```bash
cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --flow tests/regression/
```

**合格基溁E*: `24/24 passed`

**本番ログ確誁E*:
結果 JSON は `logs/regression/` に出力される、E
サブエージェント報告と照合忁E��、E
```

**Note**: regression-tester.md は既に正しいコマンチE(`--flow`) を記載済み。追記不要、E

### Task 3.4: kojo 用チE��ト定義

**imple.md に追訁E*:
```markdown
## Type別チE��ト定義

| Type | Phase 3 (TDD) | Phase 5 (Test Gen) | Phase 8 (Regression) | dotnet test |
|------|---------------|--------------------|--------------------|-------------|
| kojo | スキチE�E | kojo_test_gen.py | tests/regression/ | **不要E* |
| erb | チE��チESON作�E | スキチE�E | tests/regression/ | 不要E|
| engine | C# + ACチE��チE| スキチE�E | tests/regression/ | **忁E��E* |
| infra | スキチE�E | スキチE�E | tests/regression/ | 不要E|

**重要E*: kojo では dotnet test (engine.Tests) を実行しなぁE��E
```

---

## Root Cause Analysis

### なぜサブエージェントが虚偽報告したか

1. **regression-tester.md に具体的な実行手頁E��なかっぁE* ↁE**F202で対応済み**
   - 現在はコマンド�ESUMMARY報告指示あり

2. **Opus (imple.md) 側の検証義務が不十刁E��っぁE* ↁE**F202で部刁E��忁E*
   - Phase 8 に「盲目皁E��信頼しなぁE��追記済み
   - しかし�E体的な検証コマンドがなぁE

3. **チE��ト件数の混吁E*
   - 512件 (ACチE��トケース) と 24件 (Regression) を混吁E
   - ↁE本 Feature でチE��ト件数定義を�E確匁E

### なぁEkojo_test_gen.py ぁECOM_60 非対応だったか

1. **Feature 182 (COM_48) 用にハ�EドコーチE*
   - `CHAR_MAP` が�Eキャラ `_愛撫.ERB` 固宁E
   - COM_60 は `_挿入.ERB` なのに参�EできなぁE

2. **F202 では kojo_test_gen.py の検証 (AC7-12) めECOM_48 で実施**
   - 「正常に動作」と判定されたぁECOM_60 は未検証
   - ↁE本 Feature で COM→ファイルマッピング表を追加

### なぁEflow test ぁEFAIL したぁE

1. **機�Eの不整吁E*
   - KojoTestScenario.Parse() で flow test を許可 (Feature 174)
   - KojoTestRunner.RunWithCapture() で flow test 用刁E��なぁE
   - ↁE本 Feature でエンジン側に刁E��追加

---

## Prevention Measures

本 Feature 完亁E��、以下を運用に絁E��込む:

| 対筁E| 実裁E��所 | 効极E|
|------|----------|------|
| サブエージェント報告�E検証 | imple.md Phase 8 | 虚偽報告防止 |
| 本番ログ収集 | KojoBatchRunner.cs | 報告�E証跡 |
| 具体的なコマンド記輁E| regression-tester.md | 推測排除 |
| COM→ファイルマッピング | kojo_test_gen.py | 全 COM 対忁E|
| Type 別チE��ト定義 | imple.md | 曖昧さ排除 |

---

## F203 Infrastructure Audit Findings

**Note**: Feature 203 が実施したインフラ監査で発見した問題。本 Feature で対応するものと別 Feature に刁E��するも�Eを整琁E��E

### 本 Feature で対忁E(Part E / Phase 5)

| # | カチE��リ | 問顁E| 影響 | 修正対象 | AC |
|:-:|----------|------|------|---------|:--:|
| 8 | Subagent | regression-tester に Skill tool なぁE| testing skill 参�E不可 | regression-tester.md | E1 |
| 9 | Subagent | ac-tester に Skill tool なぁE| testing skill 参�E不可 | ac-tester.md | E2 |
| 10 | Subagent | feature-reviewer に Skill tool なぁE| 参�E記述あるのに使えなぁE| feature-reviewer.md | E3 |
| 11 | Subagent | spec-writer に Skill tool なぁE| 参�E記述あるのに使えなぁE| spec-writer.md | E4 |
| 12 | Subagent | implementer に skill 参�E持E��なぁE| どの skill を使ぁE��不�E | implementer.md | E5 |
| 13 | Subagent | feasibility-checker に skill 参�E持E��なぁE| Skill tool あるのに使われなぁE| feasibility-checker.md | E6 |

### 別 Feature に刁E��

| # | カチE��リ | 問顁E| 刁E��允E| 琁E�� |
|:-:|----------|------|:------:|------|
| 14 | Skill/Doc | testing skill と agent.md で惁E��重褁E| F208 | 大規模リファクタリング |
| 15 | Engine | flow test 未実裁E| F206/F207 | 調査→実裁E|
| 16 | Hook | Hook 動作�E明示皁EAC なぁE| F208 | 優先度佁E|
| 17 | Skill | testing/ERB.md, ENGINE.md 未確誁E| F208 | 優先度佁E|

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-24 | - | Feature 186 回帰チE��ト中に問題発要E| 3/24 FAIL, 虚偽報呁E|
| 2025-12-24 | - | Feature 203 作�E (旧 COM_64 口丁EↁE置揁E | - |
| 2025-12-24 | - | F202整合性確認�EFeature仕様更新 | F202残課題として位置づぁE|
| 2025-12-24 17:17 | - | Feature 203 監査完亁E| 追加問顁E9 件発要E(F204 に反映) |
| 2025-12-24 | initializer | Initialize Feature 204 | READY |
| 2025-12-24 | Explore | Investigation | READY |
| 2025-12-24 | implementer | Python修正 (kojo_test_gen.py) | SUCCESS |
| 2025-12-24 | implementer | ドキュメント修正 (imple.md) | SUCCESS |
| 2025-12-24 | implementer | Subagent Skill tool追加 (6 files) | SUCCESS |
| 2025-12-24 | - | Smoke Test (build) | PASS |
| 2025-12-24 | ac-tester | AC Verification | PASS:18/18 |
| 2025-12-24 | regression-tester | Regression Test | 24 EXECUTED (F207対応征E��) |
| 2025-12-24 | - | dotnet test | PASS:88/88 |
| 2025-12-24 | finalizer | Status Update | DONE (18/18 ACs, all Tasks [O]) |

---

## F186 Execution Log 訂正

F186 の Execution Log に、E12/512 PASS」「Regression OK」とあるが、これ�E以下�E通り訂正が忁E��E

| 頁E�� | 允E�E報呁E| 実際 |
|------|----------|------|
| regression-tester | OK:512/512 | **誤めE*: 512はACチE��トケース数、Regressionは別 |
| Regression結果 | 未記輁E| **21/24 PASS, 3 FAIL** (flow test 未実裁E |

**注愁E*: F186 は [WIP] のため、本 Feature (F203) 完亁E��に再検証が忁E��、E

---

## Links

- [feature-186.md](feature-186.md) - 発見�E
- [feature-202.md](feature-202.md) - 前回の workflow fix (不十刁E��っぁE
- [kojo_test_gen.py](../../src/tools/kojo-mapper/kojo_test_gen.py)
- [KojoTestRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs)
- [imple.md](../../../archive/claude_legacy_20251230/commands/imple.md)
- [regression-tester.md](../../../archive/claude_legacy_20251230/agents/regression-tester.md)
