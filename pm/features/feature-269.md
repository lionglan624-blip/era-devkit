# Feature 269: 口上主客関係監査 - COMF/COMABLE 定義との整合性検証

## Status: [DONE]

## Type: infra

---

## Background

### Philosophy

口上�E COMF/COMABLE の定義に従い、正しい主客関係で記述されるべき、E
「誰が誰に何をするか」が COMF の PLAYER/TARGET 定義と一致してぁE��ければならなぁE��E

> **F261 で検証頁E�� E�E�主客関係）が漏れた。本 Feature はそ�E補完である、E*

### Problem

F261 Phase ② 監査で検証した頁E��:
- A: ファイル配置
- B: 重褁E
- C: SOURCE キーワード整吁E
- D: 冁E��品質

**未検証だった頁E��**:
- E: 主客関俁E(PLAYER/TARGET の整合性)

**発見された問顁E* (F265 FL 中に検�E):

K6 COM 43 (オナ�Eール) において:
- COMF43 定義: PLAYER ぁETARGET の男性器にオナ�Eールを使用
- 問題�E口上�E容: 「フランがオナ�Eールを手に取り MASTER に...、E
- 不整吁E PLAYER が行為老E��あるべきだが、口上では TARGET が行為老EↁE**主客送E�� (E1)**

### Goal

全口上関数 (F261 Phase ① で抽出した 1517 関数) につぁE��、COMF/COMABLE 定義と口上�E容の主客関係を検証し、不整合を検�Eする、E

---

## 新セチE��ョン実行ガイチE

> **Phase ① から開始、E261 Phase ① 成果物を�E利用する、E*

### 前提条件

| 頁E�� | 状慁E| パス |
|------|:----:|------|
| F261 Phase ① 関数リスチE| ✁E完亁E| `.tmp/f261-p1-v3/*.txt` |
| F261 Phase ① 統合結果 | ✁E完亁E| `.tmp/f261-phase1-results-v3.md` |

### 実行頁E��E

```
Phase ① ↁEPhase ② ↁEPhase ③ ↁEPhase ④
```

吁EPhase 完亁E��、次の Phase に進む前に出力ファイルの存在を確認せよ、E

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP ↁEAsk user for guidance.

### Phase ① COM 定義抽出

| 頁E�� | 値 |
|------|-----|
| **Agent** | general-purpose |
| **Model** | haiku |
| **対象** | COMF*.ERB + COMABLE*.ERB (紁E150 COM) |
| **単佁E* | 1 COM = 1 subagent dispatch |
| **並列度** | 10 並列バチE�� |
| **出力�E** | `.tmp/f269-p1/com-{NNN}.json` |
| **最終�E劁E* | `.tmp/f269-com-definitions.json` (全結果統吁E |

#### Input Template (厳宁E

```
## Task: COM 定義抽出

**COM 番号**: {COM_NUMBER}
**出力�E**: `.tmp/f269-p1/com-{COM_NUMBER}.json`

## 持E�� (厳宁E

1. Read チE�Eルで以下�Eファイルを読む:
   - `Game/ERB/COMF{COM_NUMBER}.ERB` (存在する場吁E
   - COMABLE ファイル (COM 番号に応じて選抁E:
     - COM 0-199: `Game/ERB/COMABLE.ERB` また�E `Game/ERB/COMABLE2.ERB`
     - COM 300-399: `Game/ERB/COMABLE_300.ERB`
     - COM 400-499: `Game/ERB/COMABLE_400.ERB`
   - 該当ファイルから `@COM_ABLE{COM_NUMBER}` 関数を検索

2. 以下を判宁E
   - **行為老E*: PLAYER ぁETARGET ぁE(TCVAR:116 等�E設定から判宁E
   - **被行為老E*: PLAYER ぁETARGET ぁE
   - **TARGET 条件**: HAS_PENIS, HAS_VAGINA 等�E条件があるか

3. Write チE�Eルで出力�Eファイルに保存すめE

## 禁止事頁E

- カウントや雁E��を出力してはならなぁE
- 説明文を追加してはならなぁE
- stdout への出力ではなく、忁E�� Write チE�Eルを使用すること
- 判定に迷った場合�E "uncertain" と記録し、推測してはならなぁE

## 出力形弁E(JSON)

{
  "com": {COM_NUMBER},
  "name": "COM名称",
  "actor": "PLAYER" | "TARGET" | "uncertain",
  "receiver": "PLAYER" | "TARGET" | "uncertain",
  "target_conditions": ["HAS_PENIS", ...] | [],
  "source_file": "COMF{COM_NUMBER}.ERB"
}
```

#### Output Template (厳宁E

吁Esubagent は持E��された JSON ファイルを作�Eする。集計や説明�E不要、E

---

### Phase ② 口上主客判宁E

| 頁E�� | 値 |
|------|-----|
| **Agent** | general-purpose |
| **Model** | haiku |
| **対象** | F261 Phase ① で抽出した 1517 口上関数 |
| **単佁E* | 1 COM ÁE1 キャラ = 1 subagent dispatch (F261 チE�Eタから実際のペア数を算�E) |
| **並列度** | 10 並列バチE�� |
| **入劁E* | `.tmp/f261-p1-v3/*.txt` + Phase ① の COM 定義 |
| **出力�E** | `.tmp/f269-p2/K{CHARA}_COM{COM}.jsonl` |
| **最終�E劁E* | `.tmp/f269-audit-results.jsonl` (全結果統吁E |

#### バッチ生成手頁E

1. `.tmp/f261-p1-v3/*.txt` から `KOJO_MESSAGE_COM_K*_*` パターンを抽出
2. COM 番号でグループ化
3. 吁E��ループに対して subagent めEdispatch

#### Input Template (厳宁E

```
## Task: 口上主客判宁E

**対象**: キャラ K{CHARA}, COM {COM_NUMBER}
**COM 定義**: `.tmp/f269-p1/com-{COM_NUMBER}.json`
**出力�E**: `.tmp/f269-p2/K{CHARA}_COM{COM_NUMBER}.jsonl`

## 持E�� (厳宁E

1. Read チE�Eルで COM 定義ファイルを読む
2. 以下�E口上ファイルめERead チE�Eルで読む:
   - `Game/ERB/口丁E{CHARA_FOLDER}/KOJO_K{CHARA}_*.ERB` から COM {COM_NUMBER} の関数
3. 吁E��上関数につぁE��、以下を判宁E
   - 口上�Eで**行為を行ってぁE��老E*は誰ぁE(PLAYER/TARGET)
   - 口上�Eで**行為を受けてぁE��老E*は誰ぁE(PLAYER/TARGET)
4. COM 定義と照合し、不整合を検�E
5. Write チE�Eルで出力�Eファイルに保存すめE

## 判定基溁E

- `%CALLNAME:MASTER%` + 動作動詁EↁEPLAYER が行為老E
- `%CALLNAME:TARGET%` + 動作動詁EↁETARGET が行為老E
- 受動慁E(`〜される`) は主客反転に注愁E
- 判定不�Eな場合�E "uncertain" と記録

## 禁止事頁E

- 推測してはならなぁE(不�EなめE"uncertain")
- 説明文を追加してはならなぁE
- stdout への出力ではなく、忁E�� Write チE�Eルを使用すること

## 出力形弁E(JSONL, 1衁E関数)

{"func": "@KOJO_MESSAGE_COM_K{CHARA}_{COM}", "kojo_actor": "PLAYER"|"TARGET"|"uncertain", "kojo_receiver": "PLAYER"|"TARGET"|"uncertain", "com_actor": "PLAYER"|"TARGET", "match": true|false|"uncertain", "category": "OK"|"E1"|"E2"|"E3", "note": ""}
```

#### カチE��リ定義

| カチE��リ | 判定条件 |
|----------|----------|
| OK | kojo_actor == com_actor AND kojo_receiver == com_receiver |
| E1 | 主客送E�� (kojo_actor != com_actor) |
| E2 | TARGET 条件刁E��不足 (HAS_PENIS 条件がある�Eに刁E��なぁE |
| E3 | 判定不�E (uncertain が含まれる) |

---

### Phase ③ 不整合レポ�Eト作�E

| 頁E�� | 値 |
|------|-----|
| **Agent** | general-purpose |
| **Model** | sonnet |
| **入劁E* | `.tmp/f269-audit-results.jsonl` |
| **出劁E* | `.tmp/f269-fix-candidates.md` |

#### Input Template (厳宁E

```
## Task: 不整合レポ�Eト作�E

**入劁E*: `.tmp/f269-audit-results.jsonl`
**出劁E*: `.tmp/f269-fix-candidates.md`

## 持E�� (厳宁E

1. Read チE�Eルで入力ファイルを読む
2. category != "OK" の全エントリを抽出
3. 以下�E形式でレポ�Eトを作�E
4. Write チE�Eルで出力ファイルに保存すめE

## 出力形弁E(Markdown)

# F269 不整合レポ�EチE

## Summary

- 総検査関数数: {N}
- OK: {N} ({%})
- E1 (主客送E��): {N} ({%})
- E2 (条件刁E��不足): {N} ({%})
- E3 (判定不�E): {N} ({%})

## E1: 主客送E��

| キャラ | COM | 関数吁E| 備老E|
|--------|-----|--------|------|
| K6 | COM43 | E1 | ... |
...

## E2: 条件刁E��不足

...

## E3: 判定不�E

...
```

---

### Phase ④ 修正 Feature 作�E

| 頁E�� | 値 |
|------|-----|
| **実行老E* | オーケストレータ (Opus) |
| **入劁E* | `.tmp/f269-fix-candidates.md` |
| **出劁E* | `pm/features/feature-270.md` (また�E適刁E��番号) |

#### 手頁E

1. Phase ③ のレポ�Eトを Read
2. E1/E2 の件数に応じて修正 Feature を作�E
3. F265 の残件があれ�E統合を検訁E

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase ① 完亁E COM 定義統合ファイル存在 | file | Glob(.tmp/f269-com-definitions.json) | exists | - | [x] |
| 2 | Phase ② 完亁E 監査結果統合ファイル存在 | file | Glob(.tmp/f269-audit-results.jsonl) | exists | - | [x] |
| 3 | Phase ③ 完亁E 不整合レポ�Eト存在 | file | Glob(.tmp/f269-fix-candidates.md) | exists | - | [x] |
| 4 | レポ�Eトに Summary セクション含む | file | Grep(.tmp/f269-fix-candidates.md) | contains | `## Summary` | [x] |
| 5 | K6 COM 43 ぁEE1 として検�EされめE| file | Grep(.tmp/f269-fix-candidates.md) | contains | `\| K6 \| COM43 \|` | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1.1 | - | COM 番号リスト生戁E(COMF*.ERB から抽出) | [x] |
| 1.2 | 1 | Phase ① dispatch (~150 COM ÁEhaiku) | [x] |
| 1.3 | 1 | Phase ① 結果統吁EↁE`.tmp/f269-com-definitions.json` | [x] |
| 2.1 | - | バッチリスト生戁E(F261 関数リストかめECOM×キャラ抽出) | [x] |
| 2.2 | 2 | Phase ② dispatch (haiku ÁEバッチ数) | [x] |
| 2.3 | 2 | Phase ② 結果統吁EↁE`.tmp/f269-audit-results.jsonl` | [x] |
| 3.1 | 3,4,5 | Phase ③ dispatch (sonnet ÁE1) | [x] |
| 4.1 | - | Phase ④ 修正 Feature 作�E | [x] |
| 5.1 | - | JSON解析エラー81件の調査 | [x] |
| 5.2 | - | レポ�Eト�E生�E�E�褁E��行JSON対応！E| [x] |

---

## Scope Note

本 Feature は調査/監査インフラ (infra) であり、口上修正は行わなぁE��E
修正は Phase ④ で作�Eする別 Feature で実施、E

**AC 検証制紁E*: AC#1-3 は `.tmp/` 配下�E一時ファイルを対象とする。Phase ①-③ の連続実行中のみ検証可能であり、`.tmp/` クリア後�E再検証不可、E

**AC:Task 1:1 例夁E*: 本 Feature は audit/research インフラであり、厳寁E�� 1:1 対応ではなぁEPhase 単位�E粒度を採用。Tasks 1.1, 2.1 は準備タスク�E��E力なし）、Task 3.1 は単一レポ�Eト�E力で褁E�� AC を検証、Task 4.1 は後綁EFeature 作�E�E�本 Feature スコープ外）、E

---

## Dependencies

| Feature | Relationship |
|---------|-------------|
| F261 | depends_on (Phase ① 関数リストを再利用) |
| F265 | blocks (F265 は F269 完亁E��E��) |

---

## Links

- [feature-261.md](feature-261.md) - 全ERB完�E調査 (関数リスト�E允E��ータ)
- [feature-265.md](feature-265.md) - 吁E��口上品質修正 (本 Feature により BLOCKED)

---

## Execution Log

| Date | Task | Action | Result |
|------|------|--------|--------|
| 2025-12-30 | 1.1 | COM番号リスト生戁E| 152 COMs extracted |
| 2025-12-30 | 1.2 | Phase ① dispatch | 152/152 complete |
| 2025-12-30 | 1.3 | Phase ① 結果統吁E| DONE (152 ↁE1 JSON) |
| 2025-12-30 | 2.1 | バッチリスト生戁E| 573 valid pairs |
| 2025-12-30 | 2.2 | Phase ② dispatch | 573/573 complete (5 batches + 2 retries) |
| 2025-12-30 | 2.3 | Phase ② 結果統吁E| DONE (1059 entries merged) |
| 2025-12-30 | 3.1 | Phase ③ レポ�Eト生戁E| DONE (E1:129, E2:49, E3:95) |
| 2025-12-30 | 4.1 | Phase ④ 修正 Feature 作�E | DONE (F270 created) |
| 2025-12-30 | 5.1 | JSON解析エラー調査 | 2ペア(K8 COM414/63)が褁E��行JSON出劁E|
| 2025-12-30 | 5.2 | レポ�Eト�E生�E | 977 entries (E1:137, E2:47, E3:165) |

### Current Status

**Phase ③-④完亁E*: 全Phase完亁E��監査レポ�Eト生成、修正Feature (F270) 作�E済み、E

**Result Summary** (再生成版):
- Phase ② 出劁E 1059 entries (573 pairs)
- レポ�Eト集訁E 977 entries (褁E��行JSON含む全エントリをパース)

**カチE��リ別**:
- OK: 628 (64.3%)
- E1 (主客送E��): 137 (14.0%)
- E2 (条件刁E��不足): 47 (4.8%)
- E3 (判定不�E): 165 (16.9%)

**Next**: AC 検証 ↁEPost-Review ↁE完亁E

**Files Created**:
- `.tmp/f269-p1/com-{N}.json` (152 files) - COM definition extractions
- `.tmp/f269-com-definitions.json` - Merged COM definitions
- `.tmp/f269-batch-list.json` - 573 (chara, com) pairs
- `.tmp/f269-p2/K{N}_COM{M}.jsonl` (573 files) - Phase ② audit results
- `.tmp/f269-audit-results.jsonl` - Merged audit results (1059 entries)

**Dispatch Pattern (参老E**:
- **シーケンシャルバッチ方弁E*: 10件完亁E��に次の10件めEdispatch
- Agent: haiku, subagent_type: general-purpose
- Each agent: Read COM定義 ↁEGrep 口上検索 ↁERead 口丁EↁE判宁EↁEWrite JSONL
- コンチE��ストウィンドウ消費を抑制するため、並列度を制陁E

**Remaining Work**:
1. Task 3.1: Phase ③ report generation
2. Task 4.1: Phase ④ fix Feature creation
