# Feature 204: Test Infrastructure & Workflow Fixes

## Status: [DONE]

**Note**: Feature 203 (調査) の結果を受けて修正を実施する。

## Type: infra

## Background

### Problem

Feature 186 (COM_60 正常位 口上) の実装中に、/imple ワークフローの重大な欠陥が発覚した。

**発覚した問題一覧**:

| # | カテゴリ | 問題 | 影響 | F202状態 |
|:-:|----------|------|------|:--------:|
| 1 | エンジン | flow test シナリオが FAIL | 回帰テスト 3/24 FAIL | 未対応 |
| 2 | サブエージェント | regression-tester が虚偽報告 | 「512/512 PASS」→実際は 21/24 | 対応済※ |
| 3 | ドキュメント | kojo で dotnet test 未定義 | Opus が勝手に実行・報告 | 未対応 |
| 4 | Python | kojo_test_gen.py が COM_60 非対応 | Phase 5 実行不可 | 未対応 |
| 5 | ドキュメント | 回帰テスト件数・パス未定義 | 検証基準が曖昧 | 対応済 |
| 6 | 運用 | 本番ログ収集の仕組みなし | 結果報告の信頼性なし | 未対応 |
| 7 | テストデータ | テスト名が "Feature XXX" | 自動生成の不備 | 未対応 |

**※F202対応済の補足**:
- #2: regression-tester.md にコマンド・SUMMARY報告指示追加済。しかし Opus (imple.md) 側の検証義務が不十分だった
- #5: regression-tester.md に `tests/regression/` (24件) 明記済

### F202との関係

F202 [DONE] は以下を「対応済み」としたが、一部は**実効性がない**:

| F202対応項目 | 状態 | 問題 |
|-------------|:----:|------|
| regression-tester.md PRE-EXISTING判定 | ✅ | 機能している |
| regression-tester.md SUMMARYエビデンス報告 | ⚠️ | 報告指示はあるがOpusが検証しなかった |
| imple.md Phase 7 ループ | ✅ | 機能している |
| imple.md DEVIATION報告 | ✅ | 機能している |
| imple.md Issues Found | ✅ | 機能している |
| kojo-writer.md Hook BOM | ✅ | 機能している |
| ac-tester.md {auto}→実値 | ✅ | 機能している |
| imple.md Phase 8 盲信禁止 | ❌ | **文言あるが機能せず** (検証コマンドなし) |

**F204の方針**: F203 (調査) で発見した問題を全て修正する。

**対応項目** (F203 調査結果で更新):
1. flow test: `--flow` で実行すべきを `--unit` と誤記 → **ドキュメント修正**
2. kojo_test_gen.py が COM_48 専用ハードコード → **Python 修正**
3. subagents に Skill tool がない → **subagent 修正**
4. hooks の動作確認 → **F203 調査結果待ち**
5. kojo で dotnet test 不要の明記なし → **skill/subagent 修正**

### テスト件数の定義

| カテゴリ | 件数 | 意味 |
|----------|:----:|------|
| Regression Tests | 24 | `tests/regression/*.json` シナリオ数 |
| Kojo AC Tests | 51 | `tests/ac/kojo/**/*.json` ファイル数 |
| AC内テストケース | 512+ | 各JSONファイル内の個別テスト合計 |

F186 Execution Log の「512/512」は AC内テストケース数。「24/24」は Regression Tests。

### Why This Happened

**根本原因**: /imple ワークフローとツールが断片的に開発され、統合テストされていない。

1. **kojo_test_gen.py**: Feature 182 (COM_48) 用にハードコード → 他 COM 非対応
   - `CHAR_MAP` が全て `_愛撫.ERB` 固定 (COM_60 は `_挿入.ERB`)
2. **imple.md (Opus向け)**: サブエージェント報告の検証義務なし → 盲信
3. **imple.md**: Type 別テスト定義があいまい → kojo で dotnet test 実行
4. **flow test**: Parse() で許可、RunWithCapture() で未実装 → 不整合

### Goal

1. ~~回帰テスト 24/24 PASS を達成 (flow test 修正)~~ → **F206/F207 に分離**
2. F202で機能しなかった項目を**直るまで修正**
3. kojo_test_gen.py を現存する口上ファイルに対応
4. F203 監査結果の Subagent Skill tool 問題を修正
5. 修正後、Feature 186 を再検証して完了させる

---

## Acceptance Criteria

### Part A: flow test 修正

**スコープ変更**: flow test のエンジン実装修正は別 Feature (F206/F207) に分離。
本 Feature では Part A をスキップし、Part B 以降を対象とする。

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| A1 | (F207 に移管) scenario-alice-sameroom.json PASS | - | - | - | [-] |
| A2 | (F207 に移管) scenario-conversation.json PASS | - | - | - | [-] |
| A3 | (F207 に移管) scenario-daiyousei-sameroom.json PASS | - | - | - | [-] |
| A4 | (F207 に移管) 全回帰テスト PASS | - | - | - | [-] |

**理由**: flow test の正しい実装方針を調査してから対応するため。

### Part B: kojo_test_gen.py 修正

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| B1 | COM→ファイルマッピング表存在 | code | contains | "COM_FILE_MAP" | [x] |
| B2 | COM_60 でテスト生成成功 | exit_code | equals | 0 | [x] |
| B3 | 生成テスト名に Feature ID | output | contains | "Feature 999" | [x] |
| B4a | 愛撫カテゴリ対応 | code | contains | "_愛撫.ERB" | [x] |
| B4b | 乳首責めカテゴリ対応 | code | contains | "_乳首責め.ERB" | [x] |
| B4c | 挿入カテゴリ対応 | code | contains | "_挿入.ERB" | [x] |

**Note**: 存在するファイルのみ対応。`_道具.ERB`, `_口挿入.ERB` は現状ファイルが存在しないため未対応COMはエラーとする。

**検証方法**:
```bash
# B2: COM_60 テスト生成
python tools/kojo-mapper/kojo_test_gen.py --feature 999 --com 60 --output-dir .tmp/test-b2/
# → exit 0, 10 files generated

# B3: テスト名確認
grep "Feature 999" .tmp/test-b2/*.json
```

### Part C: imple.md Phase 8 修正

**現状確認**: regression-tester.md は既に十分な記載がある
- `tests/regression/` パス ✅
- 24件明記 ✅
- 実行コマンド (`--flow`) ✅
- `logs/regression/` 参照 ✅
- SUMMARY 行報告指示 ✅

**問題1**: imple.md (Opus 用) に Phase 8 の具体的指示がない
**問題2**: Opus がサブエージェント報告を盲信した
**問題3**: Opus が kojo で dotnet test を勝手に実行した

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| C1 | Opus 直接実行コマンド記載 | file | contains | "--flow tests/regression/" | [x] |
| C2 | 24/24 目視確認指示 | file | contains | "24/24" | [x] |
| C3 | サブエージェント報告検証義務 | file | contains | "盲目的に信頼しない" | [x] |
| C4 | kojo で dotnet test 不要明記 | file | contains | "kojo では dotnet test" | [x] |

**F202完了確認**:
- C3: ✅ 既に imple.md Phase 8 に「サブエージェント報告を盲目的に信頼しない」が追記済み
- C2: ⚠️ regression-tester.md には24件明記あり、imple.mdには部分的
- C1: ❌ `--flow tests/regression/` は regression-tester.md にあるが imple.md (Opus向け) にない
- C4: ❌ Type Routing テーブルに `dotnet test` 列があるが kojo で不要の明記なし

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
| E5 | implementer に skill 参照指示追加 | file | contains | "erb-syntax" | [x] |
| E6 | feasibility-checker に skill 参照指示追加 | file | contains | "testing" | [x] |

**Note**: F203 問題#15 (testing skill と agent.md の情報重複整理) は大規模なため別 Feature に分離。

---

## Tasks

### Phase 1: エンジン修正 (F207 に移管)

**スコープ変更**: flow test 修正は F206/F207 に分離。

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1.1 | A1-A3 | (F207) KojoTestRunner に flow test 分岐追加 | KojoTestRunner.cs | [-] |
| 1.2 | A1-A3 | (F207) flow test 用の実行パス実装 | KojoTestRunner.cs | [-] |
| 1.3 | A4 | (F207) 回帰テスト全件 PASS 確認 | - | [-] |

### Phase 2: Python 修正

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 2.1 | B1,B4 | COM→ファイルマッピング表追加 | kojo_test_gen.py | [O] |
| 2.2 | B2 | batch generation でマッピング参照 | kojo_test_gen.py | [O] |
| 2.3 | B3 | テスト名に Feature ID 反映 | kojo_test_gen.py | [O] |
| 2.4 | B1-B4 | COM_60 でテスト生成確認 | - | [O] |

### Phase 3: ドキュメント修正 (imple.md)

**注意**: regression-tester.md は既に十分。imple.md (Opus 用) に追記が必要。

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 3.1 | C1 | imple.md Phase 8 に Opus 直接実行コマンド追加 | .claude/commands/imple.md | [O] |
| 3.2 | C2 | imple.md Phase 8 に 24/24 目視確認指示追加 | .claude/commands/imple.md | [O] |
| 3.3 | C3 | (F202完了済) サブエージェント報告検証義務 | .claude/commands/imple.md | [O] |
| 3.4 | C4 | imple.md に kojo で dotnet test 不要明記 | .claude/commands/imple.md | [O] |

### Phase 4: 検証

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 4.1 | D1,D2 | ビルド・テスト確認 | [O] |
| 4.2 | A4 | 回帰テスト 24/24 確認 | [-] (F207) |
| 4.3 | B2 | kojo_test_gen.py COM_60 確認 | [O] |

### Phase 5: F203 監査結果修正 (Subagent Skill tool)

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 5.1 | E1 | regression-tester に Skill tool 追加 | .claude/agents/regression-tester.md | [O] |
| 5.2 | E2 | ac-tester に Skill tool 追加 | .claude/agents/ac-tester.md | [O] |
| 5.3 | E3 | feature-reviewer に Skill tool 追加 | .claude/agents/feature-reviewer.md | [O] |
| 5.4 | E4 | spec-writer に Skill tool 追加 | .claude/agents/spec-writer.md | [O] |
| 5.5 | E5 | implementer に skill 参照指示追加 | .claude/agents/implementer.md | [O] |
| 5.6 | E6 | feasibility-checker に skill 参照指示追加 | .claude/agents/feasibility-checker.md | [O] |

---

## Technical Details

### Task 1.1-1.2: KojoTestRunner flow test 対応 (F207 に移管)

**F206 (調査) → F207 (実装) で対応予定**。正しい実装方針を調査してから実施。

現状の scenario-*.json は状態注入のみで関数呼び出しがない。
何を PASS/FAIL とすべきか要調査:
- 状態注入成功 = PASS?
- 特定の状態遷移を検証?
- ゲーム内イベント発火を検証?

### Task 2.1: COM→ファイルマッピング表

```python
# kojo_test_gen.py に追加
# 実際に存在するファイルのみ対応
COM_FILE_MAP = {
    # 愛撫系 (0-6, 8-9) → _愛撫.ERB
    0: "_愛撫.ERB",   # 愛撫
    1: "_愛撫.ERB",   # クンニ
    2: "_愛撫.ERB",   # フェラする
    3: "_愛撫.ERB",   # 指挿入れ
    4: "_愛撫.ERB",   # アナル舐め
    5: "_愛撫.ERB",   # アナル愛撫
    6: "_愛撫.ERB",   # 胸愛撫
    8: "_愛撫.ERB",   # 秘貝開帳
    9: "_愛撫.ERB",   # 自慰
    # 乳首責め系 (7, 10, 11) → _乳首責め.ERB
    7: "_乳首責め.ERB",   # 乳首責め
    10: "_乳首責め.ERB",  # 乳首吸い
    11: "_乳首責め.ERB",  # 乳首吸わせ
    # 挿入系 (60-72) → _挿入.ERB
    **{i: "_挿入.ERB" for i in range(60, 73)},
}

def get_erb_file_for_com(com_num: int) -> str:
    if com_num in COM_FILE_MAP:
        return COM_FILE_MAP[com_num]
    raise ValueError(f"Unsupported COM: {com_num} (no kojo file exists)")
```

**Note**: 道具系 (40-48), 手技系 (80-85) 等は口上ファイルが未実装のため対象外。

### Task 3.1-3.2: ドキュメント修正

**imple.md Phase 8 追記**:
```markdown
## Phase 8: Regression

**テスト対象**: `tests/regression/` (24 シナリオ)

**実行コマンド**:
```bash
cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --flow tests/regression/
```

**合格基準**: `24/24 passed`

**本番ログ確認**:
結果 JSON は `logs/regression/` に出力される。
サブエージェント報告と照合必須。
```

**Note**: regression-tester.md は既に正しいコマンド (`--flow`) を記載済み。追記不要。

### Task 3.4: kojo 用テスト定義

**imple.md に追記**:
```markdown
## Type別テスト定義

| Type | Phase 3 (TDD) | Phase 5 (Test Gen) | Phase 8 (Regression) | dotnet test |
|------|---------------|--------------------|--------------------|-------------|
| kojo | スキップ | kojo_test_gen.py | tests/regression/ | **不要** |
| erb | テストJSON作成 | スキップ | tests/regression/ | 不要 |
| engine | C# + ACテスト | スキップ | tests/regression/ | **必須** |
| infra | スキップ | スキップ | tests/regression/ | 不要 |

**重要**: kojo では dotnet test (engine.Tests) を実行しない。
```

---

## Root Cause Analysis

### なぜサブエージェントが虚偽報告したか

1. **regression-tester.md に具体的な実行手順がなかった** → **F202で対応済み**
   - 現在はコマンド・SUMMARY報告指示あり

2. **Opus (imple.md) 側の検証義務が不十分だった** → **F202で部分対応**
   - Phase 8 に「盲目的に信頼しない」追記済み
   - しかし具体的な検証コマンドがない

3. **テスト件数の混同**
   - 512件 (ACテストケース) と 24件 (Regression) を混同
   - → 本 Feature でテスト件数定義を明確化

### なぜ kojo_test_gen.py が COM_60 非対応だったか

1. **Feature 182 (COM_48) 用にハードコード**
   - `CHAR_MAP` が全キャラ `_愛撫.ERB` 固定
   - COM_60 は `_挿入.ERB` なのに参照できない

2. **F202 では kojo_test_gen.py の検証 (AC7-12) を COM_48 で実施**
   - 「正常に動作」と判定されたが COM_60 は未検証
   - → 本 Feature で COM→ファイルマッピング表を追加

### なぜ flow test が FAIL したか

1. **機能の不整合**
   - KojoTestScenario.Parse() で flow test を許可 (Feature 174)
   - KojoTestRunner.RunWithCapture() で flow test 用分岐なし
   - → 本 Feature でエンジン側に分岐追加

---

## Prevention Measures

本 Feature 完了後、以下を運用に組み込む:

| 対策 | 実装場所 | 効果 |
|------|----------|------|
| サブエージェント報告の検証 | imple.md Phase 8 | 虚偽報告防止 |
| 本番ログ収集 | KojoBatchRunner.cs | 報告の証跡 |
| 具体的なコマンド記載 | regression-tester.md | 推測排除 |
| COM→ファイルマッピング | kojo_test_gen.py | 全 COM 対応 |
| Type 別テスト定義 | imple.md | 曖昧さ排除 |

---

## F203 Infrastructure Audit Findings

**Note**: Feature 203 が実施したインフラ監査で発見した問題。本 Feature で対応するものと別 Feature に分離するものを整理。

### 本 Feature で対応 (Part E / Phase 5)

| # | カテゴリ | 問題 | 影響 | 修正対象 | AC |
|:-:|----------|------|------|---------|:--:|
| 8 | Subagent | regression-tester に Skill tool なし | testing skill 参照不可 | regression-tester.md | E1 |
| 9 | Subagent | ac-tester に Skill tool なし | testing skill 参照不可 | ac-tester.md | E2 |
| 10 | Subagent | feature-reviewer に Skill tool なし | 参照記述あるのに使えない | feature-reviewer.md | E3 |
| 11 | Subagent | spec-writer に Skill tool なし | 参照記述あるのに使えない | spec-writer.md | E4 |
| 12 | Subagent | implementer に skill 参照指示なし | どの skill を使うか不明 | implementer.md | E5 |
| 13 | Subagent | feasibility-checker に skill 参照指示なし | Skill tool あるのに使われない | feasibility-checker.md | E6 |

### 別 Feature に分離

| # | カテゴリ | 問題 | 分離先 | 理由 |
|:-:|----------|------|:------:|------|
| 14 | Skill/Doc | testing skill と agent.md で情報重複 | F208 | 大規模リファクタリング |
| 15 | Engine | flow test 未実装 | F206/F207 | 調査→実装 |
| 16 | Hook | Hook 動作の明示的 AC なし | F208 | 優先度低 |
| 17 | Skill | testing/ERB.md, ENGINE.md 未確認 | F208 | 優先度低 |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-24 | - | Feature 186 回帰テスト中に問題発覚 | 3/24 FAIL, 虚偽報告 |
| 2025-12-24 | - | Feature 203 作成 (旧 COM_64 口上 → 置換) | - |
| 2025-12-24 | - | F202整合性確認・Feature仕様更新 | F202残課題として位置づけ |
| 2025-12-24 17:17 | - | Feature 203 監査完了 | 追加問題 9 件発見 (F204 に反映) |
| 2025-12-24 | initializer | Initialize Feature 204 | READY |
| 2025-12-24 | Explore | Investigation | READY |
| 2025-12-24 | implementer | Python修正 (kojo_test_gen.py) | SUCCESS |
| 2025-12-24 | implementer | ドキュメント修正 (imple.md) | SUCCESS |
| 2025-12-24 | implementer | Subagent Skill tool追加 (6 files) | SUCCESS |
| 2025-12-24 | - | Smoke Test (build) | PASS |
| 2025-12-24 | ac-tester | AC Verification | PASS:18/18 |
| 2025-12-24 | regression-tester | Regression Test | 24 EXECUTED (F207対応待ち) |
| 2025-12-24 | - | dotnet test | PASS:88/88 |
| 2025-12-24 | finalizer | Status Update | DONE (18/18 ACs, all Tasks [O]) |

---

## F186 Execution Log 訂正

F186 の Execution Log に「512/512 PASS」「Regression OK」とあるが、これは以下の通り訂正が必要:

| 項目 | 元の報告 | 実際 |
|------|----------|------|
| regression-tester | OK:512/512 | **誤り**: 512はACテストケース数、Regressionは別 |
| Regression結果 | 未記載 | **21/24 PASS, 3 FAIL** (flow test 未実装) |

**注意**: F186 は [WIP] のため、本 Feature (F203) 完了後に再検証が必要。

---

## Links

- [feature-186.md](feature-186.md) - 発見元
- [feature-202.md](feature-202.md) - 前回の workflow fix (不十分だった)
- [kojo_test_gen.py](../../tools/kojo-mapper/kojo_test_gen.py)
- [KojoTestRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs)
- [imple.md](../../.claude/commands/imple.md)
- [regression-tester.md](../../.claude/agents/regression-tester.md)
