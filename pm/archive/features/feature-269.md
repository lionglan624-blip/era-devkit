# Feature 269: 口上主客関係監査 - COMF/COMABLE 定義との整合性検証

## Status: [DONE]

## Type: infra

---

## Background

### Philosophy

口上は COMF/COMABLE の定義に従い、正しい主客関係で記述されるべき。
「誰が誰に何をするか」が COMF の PLAYER/TARGET 定義と一致していなければならない。

> **F261 で検証項目 E（主客関係）が漏れた。本 Feature はその補完である。**

### Problem

F261 Phase ② 監査で検証した項目:
- A: ファイル配置
- B: 重複
- C: SOURCE キーワード整合
- D: 内容品質

**未検証だった項目**:
- E: 主客関係 (PLAYER/TARGET の整合性)

**発見された問題** (F265 FL 中に検出):

K6 COM 43 (オナホール) において:
- COMF43 定義: PLAYER が TARGET の男性器にオナホールを使用
- 問題の口上内容: 「フランがオナホールを手に取り MASTER に...」
- 不整合: PLAYER が行為者であるべきだが、口上では TARGET が行為者 → **主客逆転 (E1)**

### Goal

全口上関数 (F261 Phase ① で抽出した 1517 関数) について、COMF/COMABLE 定義と口上内容の主客関係を検証し、不整合を検出する。

---

## 新セッション実行ガイド

> **Phase ① から開始。F261 Phase ① 成果物を再利用する。**

### 前提条件

| 項目 | 状態 | パス |
|------|:----:|------|
| F261 Phase ① 関数リスト | ✅ 完了 | `.tmp/f261-p1-v3/*.txt` |
| F261 Phase ① 統合結果 | ✅ 完了 | `.tmp/f261-phase1-results-v3.md` |

### 実行順序

```
Phase ① → Phase ② → Phase ③ → Phase ④
```

各 Phase 完了後、次の Phase に進む前に出力ファイルの存在を確認せよ。

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Phase ① COM 定義抽出

| 項目 | 値 |
|------|-----|
| **Agent** | general-purpose |
| **Model** | haiku |
| **対象** | COMF*.ERB + COMABLE*.ERB (約 150 COM) |
| **単位** | 1 COM = 1 subagent dispatch |
| **並列度** | 10 並列バッチ |
| **出力先** | `.tmp/f269-p1/com-{NNN}.json` |
| **最終出力** | `.tmp/f269-com-definitions.json` (全結果統合) |

#### Input Template (厳守)

```
## Task: COM 定義抽出

**COM 番号**: {COM_NUMBER}
**出力先**: `.tmp/f269-p1/com-{COM_NUMBER}.json`

## 指示 (厳守)

1. Read ツールで以下のファイルを読む:
   - `Game/ERB/COMF{COM_NUMBER}.ERB` (存在する場合)
   - COMABLE ファイル (COM 番号に応じて選択):
     - COM 0-199: `Game/ERB/COMABLE.ERB` または `Game/ERB/COMABLE2.ERB`
     - COM 300-399: `Game/ERB/COMABLE_300.ERB`
     - COM 400-499: `Game/ERB/COMABLE_400.ERB`
   - 該当ファイルから `@COM_ABLE{COM_NUMBER}` 関数を検索

2. 以下を判定:
   - **行為者**: PLAYER か TARGET か (TCVAR:116 等の設定から判定)
   - **被行為者**: PLAYER か TARGET か
   - **TARGET 条件**: HAS_PENIS, HAS_VAGINA 等の条件があるか

3. Write ツールで出力先ファイルに保存する

## 禁止事項

- カウントや集計を出力してはならない
- 説明文を追加してはならない
- stdout への出力ではなく、必ず Write ツールを使用すること
- 判定に迷った場合は "uncertain" と記録し、推測してはならない

## 出力形式 (JSON)

{
  "com": {COM_NUMBER},
  "name": "COM名称",
  "actor": "PLAYER" | "TARGET" | "uncertain",
  "receiver": "PLAYER" | "TARGET" | "uncertain",
  "target_conditions": ["HAS_PENIS", ...] | [],
  "source_file": "COMF{COM_NUMBER}.ERB"
}
```

#### Output Template (厳守)

各 subagent は指定された JSON ファイルを作成する。集計や説明は不要。

---

### Phase ② 口上主客判定

| 項目 | 値 |
|------|-----|
| **Agent** | general-purpose |
| **Model** | haiku |
| **対象** | F261 Phase ① で抽出した 1517 口上関数 |
| **単位** | 1 COM × 1 キャラ = 1 subagent dispatch (F261 データから実際のペア数を算出) |
| **並列度** | 10 並列バッチ |
| **入力** | `.tmp/f261-p1-v3/*.txt` + Phase ① の COM 定義 |
| **出力先** | `.tmp/f269-p2/K{CHARA}_COM{COM}.jsonl` |
| **最終出力** | `.tmp/f269-audit-results.jsonl` (全結果統合) |

#### バッチ生成手順

1. `.tmp/f261-p1-v3/*.txt` から `KOJO_MESSAGE_COM_K*_*` パターンを抽出
2. COM 番号でグループ化
3. 各グループに対して subagent を dispatch

#### Input Template (厳守)

```
## Task: 口上主客判定

**対象**: キャラ K{CHARA}, COM {COM_NUMBER}
**COM 定義**: `.tmp/f269-p1/com-{COM_NUMBER}.json`
**出力先**: `.tmp/f269-p2/K{CHARA}_COM{COM_NUMBER}.jsonl`

## 指示 (厳守)

1. Read ツールで COM 定義ファイルを読む
2. 以下の口上ファイルを Read ツールで読む:
   - `Game/ERB/口上/{CHARA_FOLDER}/KOJO_K{CHARA}_*.ERB` から COM {COM_NUMBER} の関数
3. 各口上関数について、以下を判定:
   - 口上内で**行為を行っている者**は誰か (PLAYER/TARGET)
   - 口上内で**行為を受けている者**は誰か (PLAYER/TARGET)
4. COM 定義と照合し、不整合を検出
5. Write ツールで出力先ファイルに保存する

## 判定基準

- `%CALLNAME:MASTER%` + 動作動詞 → PLAYER が行為者
- `%CALLNAME:TARGET%` + 動作動詞 → TARGET が行為者
- 受動態 (`〜される`) は主客反転に注意
- 判定不能な場合は "uncertain" と記録

## 禁止事項

- 推測してはならない (不明なら "uncertain")
- 説明文を追加してはならない
- stdout への出力ではなく、必ず Write ツールを使用すること

## 出力形式 (JSONL, 1行1関数)

{"func": "@KOJO_MESSAGE_COM_K{CHARA}_{COM}", "kojo_actor": "PLAYER"|"TARGET"|"uncertain", "kojo_receiver": "PLAYER"|"TARGET"|"uncertain", "com_actor": "PLAYER"|"TARGET", "match": true|false|"uncertain", "category": "OK"|"E1"|"E2"|"E3", "note": ""}
```

#### カテゴリ定義

| カテゴリ | 判定条件 |
|----------|----------|
| OK | kojo_actor == com_actor AND kojo_receiver == com_receiver |
| E1 | 主客逆転 (kojo_actor != com_actor) |
| E2 | TARGET 条件分岐不足 (HAS_PENIS 条件があるのに分岐なし) |
| E3 | 判定不能 (uncertain が含まれる) |

---

### Phase ③ 不整合レポート作成

| 項目 | 値 |
|------|-----|
| **Agent** | general-purpose |
| **Model** | sonnet |
| **入力** | `.tmp/f269-audit-results.jsonl` |
| **出力** | `.tmp/f269-fix-candidates.md` |

#### Input Template (厳守)

```
## Task: 不整合レポート作成

**入力**: `.tmp/f269-audit-results.jsonl`
**出力**: `.tmp/f269-fix-candidates.md`

## 指示 (厳守)

1. Read ツールで入力ファイルを読む
2. category != "OK" の全エントリを抽出
3. 以下の形式でレポートを作成
4. Write ツールで出力ファイルに保存する

## 出力形式 (Markdown)

# F269 不整合レポート

## Summary

- 総検査関数数: {N}
- OK: {N} ({%})
- E1 (主客逆転): {N} ({%})
- E2 (条件分岐不足): {N} ({%})
- E3 (判定不能): {N} ({%})

## E1: 主客逆転

| キャラ | COM | 関数名 | 備考 |
|--------|-----|--------|------|
| K6 | COM43 | E1 | ... |
...

## E2: 条件分岐不足

...

## E3: 判定不能

...
```

---

### Phase ④ 修正 Feature 作成

| 項目 | 値 |
|------|-----|
| **実行者** | オーケストレータ (Opus) |
| **入力** | `.tmp/f269-fix-candidates.md` |
| **出力** | `Game/agents/feature-270.md` (または適切な番号) |

#### 手順

1. Phase ③ のレポートを Read
2. E1/E2 の件数に応じて修正 Feature を作成
3. F265 の残件があれば統合を検討

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase ① 完了: COM 定義統合ファイル存在 | file | Glob(.tmp/f269-com-definitions.json) | exists | - | [x] |
| 2 | Phase ② 完了: 監査結果統合ファイル存在 | file | Glob(.tmp/f269-audit-results.jsonl) | exists | - | [x] |
| 3 | Phase ③ 完了: 不整合レポート存在 | file | Glob(.tmp/f269-fix-candidates.md) | exists | - | [x] |
| 4 | レポートに Summary セクション含む | file | Grep(.tmp/f269-fix-candidates.md) | contains | `## Summary` | [x] |
| 5 | K6 COM 43 が E1 として検出される | file | Grep(.tmp/f269-fix-candidates.md) | contains | `\| K6 \| COM43 \|` | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1.1 | - | COM 番号リスト生成 (COMF*.ERB から抽出) | [x] |
| 1.2 | 1 | Phase ① dispatch (~150 COM × haiku) | [x] |
| 1.3 | 1 | Phase ① 結果統合 → `.tmp/f269-com-definitions.json` | [x] |
| 2.1 | - | バッチリスト生成 (F261 関数リストから COM×キャラ抽出) | [x] |
| 2.2 | 2 | Phase ② dispatch (haiku × バッチ数) | [x] |
| 2.3 | 2 | Phase ② 結果統合 → `.tmp/f269-audit-results.jsonl` | [x] |
| 3.1 | 3,4,5 | Phase ③ dispatch (sonnet × 1) | [x] |
| 4.1 | - | Phase ④ 修正 Feature 作成 | [x] |
| 5.1 | - | JSON解析エラー81件の調査 | [x] |
| 5.2 | - | レポート再生成（複数行JSON対応） | [x] |

---

## Scope Note

本 Feature は調査/監査インフラ (infra) であり、口上修正は行わない。
修正は Phase ④ で作成する別 Feature で実施。

**AC 検証制約**: AC#1-3 は `.tmp/` 配下の一時ファイルを対象とする。Phase ①-③ の連続実行中のみ検証可能であり、`.tmp/` クリア後は再検証不可。

**AC:Task 1:1 例外**: 本 Feature は audit/research インフラであり、厳密な 1:1 対応ではなく Phase 単位の粒度を採用。Tasks 1.1, 2.1 は準備タスク（出力なし）、Task 3.1 は単一レポート出力で複数 AC を検証、Task 4.1 は後続 Feature 作成（本 Feature スコープ外）。

---

## Dependencies

| Feature | Relationship |
|---------|-------------|
| F261 | depends_on (Phase ① 関数リストを再利用) |
| F265 | blocks (F265 は F269 完了待ち) |

---

## Links

- [feature-261.md](feature-261.md) - 全ERB完全調査 (関数リストの元データ)
- [feature-265.md](feature-265.md) - 各種口上品質修正 (本 Feature により BLOCKED)

---

## Execution Log

| Date | Task | Action | Result |
|------|------|--------|--------|
| 2025-12-30 | 1.1 | COM番号リスト生成 | 152 COMs extracted |
| 2025-12-30 | 1.2 | Phase ① dispatch | 152/152 complete |
| 2025-12-30 | 1.3 | Phase ① 結果統合 | DONE (152 → 1 JSON) |
| 2025-12-30 | 2.1 | バッチリスト生成 | 573 valid pairs |
| 2025-12-30 | 2.2 | Phase ② dispatch | 573/573 complete (5 batches + 2 retries) |
| 2025-12-30 | 2.3 | Phase ② 結果統合 | DONE (1059 entries merged) |
| 2025-12-30 | 3.1 | Phase ③ レポート生成 | DONE (E1:129, E2:49, E3:95) |
| 2025-12-30 | 4.1 | Phase ④ 修正 Feature 作成 | DONE (F270 created) |
| 2025-12-30 | 5.1 | JSON解析エラー調査 | 2ペア(K8 COM414/63)が複数行JSON出力 |
| 2025-12-30 | 5.2 | レポート再生成 | 977 entries (E1:137, E2:47, E3:165) |

### Current Status

**Phase ③-④完了**: 全Phase完了。監査レポート生成、修正Feature (F270) 作成済み。

**Result Summary** (再生成版):
- Phase ② 出力: 1059 entries (573 pairs)
- レポート集計: 977 entries (複数行JSON含む全エントリをパース)

**カテゴリ別**:
- OK: 628 (64.3%)
- E1 (主客逆転): 137 (14.0%)
- E2 (条件分岐不足): 47 (4.8%)
- E3 (判定不能): 165 (16.9%)

**Next**: AC 検証 → Post-Review → 完了

**Files Created**:
- `.tmp/f269-p1/com-{N}.json` (152 files) - COM definition extractions
- `.tmp/f269-com-definitions.json` - Merged COM definitions
- `.tmp/f269-batch-list.json` - 573 (chara, com) pairs
- `.tmp/f269-p2/K{N}_COM{M}.jsonl` (573 files) - Phase ② audit results
- `.tmp/f269-audit-results.jsonl` - Merged audit results (1059 entries)

**Dispatch Pattern (参考)**:
- **シーケンシャルバッチ方式**: 10件完了後に次の10件を dispatch
- Agent: haiku, subagent_type: general-purpose
- Each agent: Read COM定義 → Grep 口上検索 → Read 口上 → 判定 → Write JSONL
- コンテキストウィンドウ消費を抑制するため、並列度を制限

**Remaining Work**:
1. Task 3.1: Phase ③ report generation
2. Task 4.1: Phase ④ fix Feature creation
