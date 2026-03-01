# Feature 261: 全ERB完�E調査 - 口上関数網羁E��監査

## Status: [DONE]

## Type: research

---

## Initializer Output

**READY:261:research**

Initialized for Phase ② execution:
- Phase ① v3: COMPLETED (466/466 files extracted)
- Phase ①検証: COMPLETED (user approved 2025-12-29)
- Phase ① Task 1.6: COMPLETED (unified results)
- Phase ② Design (v3): COMPLETED (batch processing design)
- Ready to proceed: Task 2.1 (dispatch list generation)

---

## 🚨 新セチE��ョン実行ガイチE(2025-12-29 更新)

> **Phase ② から再開する。Phase ① v3 完亁E��み、E*

### 現在の状慁E

| Phase | 状慁E| 詳細 |
|:-----:|:----:|------|
| ① v3 | ✁E完亁E| 466/466 ファイル抽出済み ↁE`.tmp/f261-p1-v3/*.txt` |
| ①検証 | ✁E完亁E| 照合検証4件OK、表記揺れ対処方針決宁E|
| ①承誁E| ✁E承認渁E| 2025-12-29 ユーザー承認完亁E|
| ② | 未実衁E| 紁E133 batch dispatch (v3 設訁E |
| ③④ | 未実衁E| Phase ② 結果を集紁EↁEF262 作�E |

### 次セチE��ョンでの実行手頁E

1. **統合ファイル作�E**
   - `.tmp/f261-phase1-results-v3.md` を作�E (AC 1 允E��)

2. **Phase ② 実衁E*
   - COM 番号抽出 (正規化込み):
     ```bash
     cat .tmp/f261-p1-v3/*.txt | grep -o 'KOJO_MESSAGE_COM_K[0-9U]*_[0-9]*' | \
       sed 's/.*_K[0-9U]*_//' | sed 's/^00$/0/' | sort -n | uniq
     ```
   - 紁E133 batch ÁEcom-auditor (haiku) dispatch
   - 出劁E `.tmp/f261-p2/batch-{NNN}.jsonl`

3. **Phase ③④ 実衁E*
   - Phase ② 結果を集紁EↁEF262 作�E

### 重要な決定事頁E

| 頁E�� | 決宁E|
|------|------|
| 表記揺めE`_00` | Phase ② 抽出時に `_0` に正規化 |
| COM 数 | 92倁E(旧参老E��87は計算誤めE |
| Phase ① 出劁E| 変更不要E(原ファイル忠宁E |

### 参老E 失敗�E极E

**詳細**: 「Phase ① v1/v2 失敗�E析」セクション参�E

| 問顁E| 対筁E|
|------|------|
| v2 で関数名リストなぁE| v3 プロンプトで禁止事頁E�E訁E|
| 87 COM ぁEgrep 由来 | Phase ① v3 結果から抽出 |
| Phase ② 設計が未検証のまま進衁E| **Phase ①検証 で STOP、ユーザー協議** |
| v3 チE��プレぁEstdout 前提だっぁE| **Write チE�Eル使用に修正** (方針と乖離してぁE��) |

---

## Background

### Problem
F190/F241/F242 で同じパターンの誤配置問題が繰り返し発生。根本原因は COM→File 正規�EチE��ングぁECOM 0-85 のみ定義され、COM 100+ が未定義だったこと、E

**こ�E問題を二度と繰り返さなぁE��本 Feature で完�Eに解決する、E*

### Context
- 関連: F057 (K4 COM統吁E, F190 (COM_60重褁E, F221 (挿入/口挿入混乱解涁E
- 現行SKILL: `.claude/skills/kojo-writing/SKILL.md` に COM 0-85 のみ記輁E

---

## Philosophy (変更不可)

### なぜ「�EERB」なのぁE

> **「口上ERBだけで十�E」�E許容しなぁE��E*

1. **過去の失敁E*: 「十刁E��と判断した篁E��の外に問題が存在し、同じ問題が繰り返し発生しぁE
2. **100%の信頼**: 部刁E��な調査では「調査してぁE��ぁE��E��」への不信が残る
3. **完�Eな区画**: 本 Feature 完亁E��、「�EERBを調査済み」と断言できる状態を作る

### なぜ「LLM直接確認」なのぁE

> **Grep/正規表現は完�E性を保証できなぁE��E*

1. **パターン漏れ**: 想定外�E関数命名パターンを見送E��可能性
2. **コメントアウチE*: `;@KOJO_*` の扱ぁE��曖昧
3. **エンコーチE��ング**: Shift_JIS 等で誤動作�E可能性

**唯一の完�Eな方況E*: LLM が�Eファイルを直接 Read し、人間�E目で確認する�Eと同等�E精度で調査する、E

### なぜ「意味皁E��査」なのぁE

> **ファイル配置だけでは不十刁E��口上�E容と COMF 定義の整合性が忁E��、E*

1. **SOURCE/EXP 整吁E*: COM の SOURCE:快�E�/快�E�/快�E� と口上描写が一致してぁE��ぁE
2. **重褁E���E**: 同一関数が褁E��ファイルに存在しなぁE��
3. **冁E��品質**: 口上がそ�ECOMの行為を正しく描�EしてぁE��ぁE
4. **主客関俁E*: COMF の PLAYER/TARGET 定義と口上�E行為老E被行為老E��一致してぁE��ぁEↁE**F269 で実施**

### 教訁E(2025-12-30 追訁E

> **検証頁E��自体�E網羁E��を担保する�Eロセスがなかった、E*

F261 設計時、E��E�� 1-3 のみ定義し頁E�� 4�E�主客関係）が漏れた、E
F265 実裁E��に K6 COM 43 で主客送E��が発覚し、F269 として追加調査が忁E��になった、E

「完�Eな調査」を謳ぁE��ら、検証頁E��自体�E完�E性も事前に検証すべきだった、E

---

## Goals

1. **Phase ① 全ERB関数抽出**: 全466件のERBファイルから関数名�E数を抽出
2. **Phase ② 意味皁E��査**: 各COMごとに口上�E容とSOURCE/EXPの整合性を監査
3. **Phase ③ 修正候補�E劁E*: 検�Eした全問題をファイル出劁E
4. **Phase ④ F262作�E**: 修正用 Feature を作�E

---

## Investigation Plan

### Scope
- **対象**: Game/ERB/ 配下�E全ERBファイル (466件)
- **目皁E*: 口上関数めE00%網羁E��に把握し、�E問題を検�E

### Execution Method

> **重要E subagents による並列バチE��実衁E*
>
> リクエスト数上限に配�Eし、E��刁E��バッチサイズで並列実行する、E
> オーケストレータは「やりすぎ」と判断してはならなぁE��これが完�E性の代償である、E

#### Phase ① 全ERB関数抽出 (v3)

> **CRITICAL: v1/v2 は失敗。v3 で再実行忁E��。詳細は「Phase ① v1/v2 失敗�E析」セクション参�E、E*

| 頁E�� | 値 |
|------|-----|
| **Agent** | general-purpose |
| **Model** | haiku |
| **対象** | **466 ERB ファイル全件** (篁E��縮小禁止) |
| **単佁E* | 1 ERB = 1 subagent dispatch |
| **並列度** | 10-20 並列バチE�� |
| **出力�E** | `.tmp/f261-p1-v3/{親チE��レクトリ}_{ファイル名}.txt` |
| **最終�E劁E* | `.tmp/f261-phase1-results-v3.md` (全結果統吁E |

##### Input Template (v3・厳宁E

```
## Task: ERB 関数名抽出

**ファイル**: `{ERB_PATH}`
**出力�E**: `.tmp/f261-p1-v3/{OUTPUT_NAME}.txt`

## 持E�� (厳宁E

1. Read チE�Eルで上記ファイルを読む
2. `@` で始まる行を**すべて**抽出する (`;@` で始まるコメントアウト�E除夁E
3. **Write チE�Eルで出力�Eファイルに保存すめE*

## 禁止事頁E

- ❁Eカウントを出力してはならなぁE
- ❁E「Total: N functions」�Eような雁E��を出力してはならなぁE
- ❁E関数名以外�E説明を追加してはならなぁE
- ❁Estdout への出力ではなく、忁E�� Write チE�Eルを使用すること

## 出力形弁E(ファイル冁E��)

@FUNCTION_NAME_1
@FUNCTION_NAME_2
@FUNCTION_NAME_3
...

関数ぁE0 件の場合�E空ファイルを作�Eせよ、E
```

##### OUTPUT_NAME 生�E規則

| ERB パス | OUTPUT_NAME |
|----------|-------------|
| `Game/ERB/ABC.ERB` | `ERB_ABC` |
| `Game/ERB/NTR/NTR.ERB` | `NTR_NTR` |
| `Game/ERB/口丁E1_美鈴/KOJO_K1_愛撫.ERB` | `1_美鈴_KOJO_K1_愛撫` |

規則: `{親チE��レクトリ名}_{ファイル吁E拡張子なぁE}`

##### Output Template (v3・厳宁E

```
@KOJO_MESSAGE_COM_K1_0
@KOJO_MESSAGE_COM_K1_0_1
@KOJO_MESSAGE_COM_K1_1
...
```

**検証**: 出力に `@` が含まれてぁE��ければ再実行、E

##### 完亁E��宁E(v3)

```bash
ls .tmp/f261-p1-v3/*.txt | wc -l
# 結果ぁE466 なら完亁E
```

##### Phase ① 完亁E���E処琁E

1. 全 `.txt` ファイルから KOJO_MESSAGE_COM 関数を抽出
2. ユニ�Eク COM 番号リストを作�E
3. `.tmp/f261-phase1-results-v3.md` に統合�E劁E

#### Phase ①検証 (CHECKPOINT)

> **CRITICAL: Phase ① v3 完亁E��、Phase ② 開始前に忁E��実行。ユーザー承認なしに Phase ② に進むことは禁止、E*

##### 目皁E

Phase ① v3 出力が Phase ② 以降で使用可能な形式であることを検証し、Phase ② の設計をユーザーと協議する、E

##### 検証頁E��

| # | 検証冁E�� | 方況E| 期征E�� |
|:-:|----------|------|--------|
| 1 | ファイル数 | `ls .tmp/f261-p1-v3/*.txt \| wc -l` | 466 |
| 2 | 出力形弁E| サンプル 5件めERead で確誁E| 1衁E関数、`@` で始まめE|
| 3 | encoding | `file .tmp/f261-p1-v3/*.txt \| head -5` | UTF-8 or ASCII |
| 4 | 空ファイル | 0バイトファイルの有無 | 許容 (関数なしERB) |
| 5 | 形式逸脱 | カウント表記、説明文の混入 | なぁE|

##### 検証結果報告テンプレーチE

```markdown
## Phase ①検証 結果

| # | 検証冁E�� | 結果 | 詳細 |
|:-:|----------|:----:|------|
| 1 | ファイル数 | ✁E❁E| {N}/466 |
| 2 | 出力形弁E| ✁E❁E| {サンプル確認結果} |
| 3 | encoding | ✁E❁E| {検�Eされたencoding} |
| 4 | 空ファイル | ✁E❁E| {N}件 |
| 5 | 形式逸脱 | ✁E❁E| {逸脱冁E�� or なし} |

## Phase ② 設計協議

### 現行設計�E問題点
- Phase ② STEP 2 が原ERBファイルに Grep を使用 (Philosophy 違反の可能性)

### 提桁E Phase ① v3 結果を活用した設訁E
- STEP 2 めEPhase ① v3 出力から�E検索に変更
- ファイルパス復允E��ジチE��: `{親チE��レクトリ}_{ファイル名}.txt` ↁE`Game/ERB/...`

### ユーザー確認事頁E
1. Phase ① v3 出力�E品質は許容可能か！E
2. Phase ② STEP 2 の修正案を承認するか�E�E
3. そ�E他�E懸念点はあるか！E
```

##### 次スチE��チE

- **検証 OK + ユーザー承誁E*: Phase ② に進む
- **検証 NG**: 問題を修正し、Phase ① v3 を部刁E��また�E全面皁E��再実衁E
- **設計変更忁E��E*: ユーザーと協議し、Phase ② 設計を修正してから進む

---

#### Phase ② 意味皁E��査

> **CRITICAL: Phase ①検証 完亁E�Eユーザー承認後に実行すること、E*
> **設訁E v3 (2025-12-29) - バッチ�E琁E��詳細は「Phase ② v3 設計変更」セクション参�E、E*

##### 監査対象

> **Phase ① v3 結果 (.tmp/f261-p1-v3/*.txt) の全関数を監査する、E*
> **grep で COM 番号を抽出する旧設訁E(v2) は廁E��。ファイルベ�Eスバッチ�E琁E��移行、E*

**確定値** (2025-12-29 検証済み):

| 頁E�� | 値 |
|------|---:|
| 口上関数総数 | 1517 |
| - @KOJO_MESSAGE_COM | 1475 |
| - @NTR_KOJO_MESSAGE_COM | 42 |
| unique COM 番号 | 92 |
| Phase ① 出力ファイル | 466 |

**計測誤差の原因と正しい調査方況E*:

初期計測で 1472 とぁE��値が�Eた原因:
1. awk でサイズ刁E��E��算時、`find ... -name "KOJO_*.ERB"` でファイルを絞り込んだ
2. grep パターン `^@KOJO_MESSAGE_COM_K[0-9U]*_[0-9]` で `NTR_` prefix を除外してぁE��
3. サイズ計算�E awk スクリプトが最終関数を二重カウントする場合があっぁE

正しい調査方況E
```bash
# Phase ① 結果から全口上関数を抽出 (NTR_ prefix 含む)
cat .tmp/f261-p1-v3/*.txt | grep -c '^@\(NTR_\)\?KOJO_MESSAGE_COM_K[0-9U]*_[0-9]'
# ↁE1517

# 冁E��確誁E
cat .tmp/f261-p1-v3/*.txt | grep -c '^@KOJO_MESSAGE_COM'      # ↁE1475
cat .tmp/f261-p1-v3/*.txt | grep -c '^@NTR_KOJO_MESSAGE_COM'  # ↁE42
```

**注愁E*: 厁EERB からの grep ではなく、Phase ① 結果を使用すること。Phase ① で LLM が抽出した結果が正となる、E

**サイズ刁E��E* (KOJO_*.ERB のみ計測、NTR口丁E42 関数は未計測だが�E体�E 3% 未満):

| サイズ | 関数数 | 平坁E��E|
|--------|-------:|-------:|
| 0-50 衁E| 996 | 16 |
| 51-100 衁E| 58 | 70 |
| 101-200 衁E| 356 | 181 |
| 201-400 衁E| 40 | 226 |
| 401-1000 衁E| 16 | 549 |
| 1001+ 衁E| 6 | 1672 |
| (小訁E | 1473 | - |

##### 実行単佁E バッチ�E琁E

> **設計原剁E*: ファイル優允E+ サイズ制紁E(1000 衁Ebatch、コンチE��スチE70% 上限)

| 頁E�� | 値 |
|------|-----|
| **Agent** | com-auditor |
| **Model** | haiku |
| **単佁E* | **ファイルベ�EスバッチE* (紁E132 dispatch) |
| **出力�E** | `.tmp/f261-p2/batch-{NNN}.jsonl` |
| **最終�E劁E* | `.tmp/f261-phase2-results.md` (全結果統吁E |

**dispatch 数見積もめE*:

| サイズ | 関数数 | 関数/batch | batch数 |
|--------|-------:|-----------:|--------:|
| 0-50 衁E| 996 | 60 | 17 |
| 51-100 衁E| 58 | 14 | 5 |
| 101-200 衁E| 356 | 5 | 72 |
| 201-400 衁E| 40 | 4 | 10 |
| 401-1000 衁E| 16 | 1 | 16 |
| 1001+ 衁E(刁E��) | 6 | - | 12 |
| NTR口丁E(未計測) | 42 | 60 | 1 |
| **合訁E* | 1517 | - | **紁E133** |

**コンチE��スト計算根拠**:

| 頁E�� | 値 |
|------|---:|
| haiku コンチE��スト上限 | 200K tokens |
| 安�Eマ�Eジン (auto compact 回避) | 70% |
| 使用可能 | 140K tokens |
| シスチE�� + 出力予紁E| 15K tokens |
| 入力用 | 125K tokens |
| 日本語変換 (1斁E��≈2.5tokens) | 50,000 斁E��E|
| 行数換箁E(50斁E��E衁E | **1000 衁Ebatch** |

##### バッチ割当アルゴリズム (オーケストレータ実衁E

Phase ② 開始前にオーケストレータが実行する。subagent には実行しなぁE��E

```
入劁E Phase ① 結果 (.tmp/f261-p1-v3/*.txt)
出劁E .tmp/f261-p2-dispatch-list.json

STEP 1: ファイル→関数マッピング構篁E
  - 吁E.txt ファイルをパース
  - **フィルタ**: `@(NTR_)?KOJO_MESSAGE_COM_K[0-9U]+_[0-9]` パターンのみ抽出
  - 監査対象外を除夁E @MESSAGE_COM{N}, @MESSAGE_COM_300_*, @NTR_MESSAGE_COM
  - 出劁E [(ファイル吁E 関数吁E, ...]

STEP 2: 関数サイズ計箁E
  - 吁E��数につぁE��厁EERB から行数を計箁E(awk)
  - コマンド侁E
    awk '/^@(NTR_)?KOJO_MESSAGE_COM_K[0-9U]*_[0-9]/{if(start) print NR-start, prev; start=NR; prev=$0} END{if(start) print NR-start+1, prev}' file.ERB
  - 注: NTR_ プレフィチE��スも対忁E
  - 注: これは「何が口上か」ではなく「何行か」�E計測 ↁEPhilosophy 準拠

STEP 3: バッチ割彁E
  - ファイル単位でグループ化
  - 吁E��ァイルグループ�Eで:
    - 累積行数ぁE1000 行を趁E��たら新 batch
    - 1 関数で 1000 行趁E��ら�E割 batch (褁E��囁ERead)
  - 頁E��: ファイル名�Eアルファベット頁E(再現性確俁E

STEP 4: dispatch リスト生戁E
  - 出力形弁E
    [
      {"batch_id": "batch-001", "functions": [{"file": "...", "func": "...", "lines": 45}, ...]},
      ...
    ]
```

##### subagent 実行手頁E(1 batch につぁE

subagent は以下�E手頁E��**こ�E頁E��で**実行する。スキチE�E禁止、E

```
入劁E
  - batch_id: batch-{NNN}
  - 対象関数リスチE [(ファイル吁E 関数吁E, ...]  ↁEdispatch リストから取征E
  - Phase①結果チE��レクトリ: .tmp/f261-p1-v3/

STEP 1: 対象関数の冁E��読み取り
  - 吁E��数につぁE��:
    - ファイル名から�E ERB パスを復允E
      - `1_美鈴_KOJO_K1_愛撫.txt` ↁE`Game/ERB/口丁E1_美鈴/KOJO_K1_愛撫.ERB`
      - `10_魔理沙_KOJO_K10_会話親寁Etxt` ↁE`Game/ERB/口丁E10_魔理沁EKOJO_K10_会話親寁EERB`
      - `U_汎用_KOJO_KU_口挿入.txt` ↁE`Game/ERB/口丁EU_汎用/KOJO_KU_口挿入.ERB`
    - Read: 該当ファイルの該当関数部刁E
      - 関数の篁E��: `@関数名` で始まり、次の `@` で始まる行�E直前また�E EOF まで
    - 関数名かめECOM 番号を抽出 (パ�Eス、grep 不要E:
      - `@KOJO_MESSAGE_COM_K1_60_1` ↁECOM=60
      - `@NTR_KOJO_MESSAGE_COM_K1_312` ↁECOM=312
      - パターン: /_K[0-9U]+_([0-9]+)/ の第1キャプチャグルーチE

STEP 2: COMF 定義取征E(batch 冁E�E unique COM のみ)
  - 吁ECOM につぁE��:
    - Read: Game/ERB/COMF{NUM}.ERB
    - 抽出: COM吁E SOURCE, EXP
    - キャチE��ュして重褁ERead を回避

STEP 3: 監査頁E��チェチE�� (吁E��数につぁE��)
  以下�E3頁E��をチェチE��する、EつでめENG なめENG、E

  A. ファイル配置チェチE��
     - 判定基溁E(kojo-writing SKILL 準拠):
       - COM 0-11, 20-21, 40-48 ↁE_愛撫.ERB
       - COM 60-72 ↁE_挿入.ERB
       - COM 80-85, 100-106, 120-126, 140-148, 180-189, 200-203 ↁE_口挿入.ERB
       - COM 300-316, 350-352 ↁE_会話親寁EERB
       - COM 410-415, 463 ↁE_日常.ERB
     - OK: 正規ファイルに配置されてぁE��
     - NG: 正規ファイル以外に配置されてぁE��

  B. 重褁E��ェチE�� ↁE**Phase ③ で実施** (全関数の惁E��が忁E��E
     - Phase ② では SKIP (常に "-")

  C. SOURCE 整合チェチE��
     - SOURCE:快�E� ↁE膣/挿入に関する描�Eがあるか
     - SOURCE:快�E� ↁEアナルに関する描�Eがあるか
     - SOURCE:快�E� ↁE胸に関する描�Eがあるか
     - SOURCE:快�E� ↁEクリトリスに関する描�Eがあるか
     - SOURCE がなぁEそ�E仁EↁEこ�EチェチE��は SKIP
     - OK: SOURCE に対応する描写がある
     - NG: SOURCE に対応する描写がなぁE

  D. 冁E��品質チェチE��
     - COM名�E行為を描写してぁE��ぁE(侁E COM 60=正常佁EↁE正常位�E描�Eがあるか)
     - OK: 行為に関する描�EがあめE
     - NG: 行為に関する描�EがなぁE無関係な冁E��

STEP 4: 結果出劁E
  - ファイル: .tmp/f261-p2/batch-{NNN}.jsonl
  - フォーマッチE 1衁E関数の JSONL (下記参照)
```

##### 1001+ 行関数の刁E��処琁E

以下�E 6 関数は 1 batch で処琁E��可能。�E割して褁E��囁ERead する:

| 関数吁E| 行数 | 刁E��数 |
|--------|-----:|-------:|
| @KOJO_MESSAGE_COM_K3_7_1 | 2040 | 3 |
| @KOJO_MESSAGE_COM_K7_65_1 | 1919 | 2 |
| @KOJO_MESSAGE_COM_K9_7_1 | 1591 | 2 |
| @KOJO_MESSAGE_COM_K10_415_1 | 1584 | 2 |
| @KOJO_MESSAGE_COM_K8_463_5 | 1475 | 2 |
| @KOJO_MESSAGE_COM_K1_7_1 | 1425 | 2 |

**刁E��処琁E��頁E*:
1. 関数めE800 行ごとに刁E��
2. 吁E�E割を頁E�� Read
3. 全刁E��の冁E��を統合して監査
4. 1 つの JSONL 行として出劁E

##### Output Format (JSONL)

吁E��数につぁE1 行�E JSON を�E劁E

```json
{"file":"1_美鈴_KOJO_K1_愛撫.ERB","func":"@KOJO_MESSAGE_COM_K1_60","com":60,"chara":"K1","A":"OK","C":"OK","D":"OK","issue":null}
{"file":"2_小悪魔_KOJO_K2_口挿入.ERB","func":"@KOJO_MESSAGE_COM_K2_60","com":60,"chara":"K2","A":"NG","C":"OK","D":"OK","issue":"A: 正規�E置は _挿入.ERB"}
```

| フィールチE| 垁E| 説昁E|
|------------|-----|------|
| file | string | 允EERB ファイル吁E|
| func | string | 関数吁E|
| com | number | COM 番号 |
| chara | string | キャラ識別孁E(K1-K10, KU) |
| A | string | 配置チェチE��結果 (OK/NG/SKIP) |
| C | string | SOURCE 整合結果 (OK/NG/SKIP) |
| D | string | 品質チェチE��結果 (OK/NG/SKIP) |
| issue | string/null | 問題詳細 (なければ null) |

**B 重褁E��ェチE��は Phase ③ で実施** (全関数の file+func を集紁E��て判宁E

##### 完亁E��宁E

```bash
# batch ファイル数の確誁E
ls .tmp/f261-p2/batch-*.jsonl | wc -l
# 結果ぁEdispatch リスト�E batch 数 (紁E132) と一致すれば完亁E

# 関数数の確誁E(全 batch の行数合訁E
wc -l .tmp/f261-p2/batch-*.jsonl | tail -1
# 結果ぁE1517 なら完亁E
```

#### Phase ③ 修正候補�E劁E
- Phase ② の JSONL 結果を統吁E
- B 重褁E��ェチE��を実施 (同一関数名が褁E��ファイルに存在するぁE
- 以下�EカチE��リで修正候補を整琁E
  - A: 誤配置 (口挿入.ERB に COM 60-72 筁E
  - B: 重褁E(同一関数が褁E��ファイルに存在)
  - C: SOURCE 不整吁E(SOURCE:快�E� なのにアナル描�Eなし筁E
  - D: 品質問顁E(行為描�Eなし筁E
- **出劁E*: `.tmp/f261-fix-candidates.md`

#### Phase ④ F262作�E
- Phase ③ の修正候補を入力として feature-262.md を作�E
- F262 で全修正を実衁E

### Output Format
| Phase | バッチ�E劁E| 最終�E劁E|
|-------|------------|----------|
| ① | `.tmp/f261-p1-v3/{ファイル名}.txt` | `.tmp/f261-phase1-results-v3.md` |
| ② | `.tmp/f261-p2/batch-{NNN}.jsonl` | `.tmp/f261-phase2-results.md` |
| ③ | - | `.tmp/f261-fix-candidates.md` |
| ④ | - | `pm/features/feature-262.md` |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase ① 完亁E 全ERB関数抽出 (v3) | file | Glob | exists | .tmp/f261-phase1-results-v3.md | [x] |
| 1.5 | Phase ①検証: ユーザー承誁E| manual | user | approved | ユーザーぁEPhase ② 進行を承誁E| [x] |
| 2 | Phase ② 完亁E 全COM意味皁E��査 | file | Glob | exists | .tmp/f261-phase2-results.md | [x] |
| 3 | Phase ③ 完亁E 修正候補�E劁E| file | Glob | exists | .tmp/f261-fix-candidates.md | [x] |
| 4 | Phase ④ 完亁E F262 作�E | file | Glob | exists | pm/features/feature-262.md | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Phase ① 全ERB関数抽出 (466件) - v3 完亁E| [x] |
| 1.5 | 1.5 | Phase ①検証: 出力検証 + ユーザー協議 + 承認取征E| [x] |
| 1.6 | 1 | 統合ファイル `.tmp/f261-phase1-results-v3.md` 作�E | [x] |
| 2.0 | 2 | Phase ② 設訁E v3 バッチ�E琁E��計完亁E| [x] |
| 2.1 | 2 | Phase ② 準備: dispatch リスト生戁E| [x] |
| 2.2 | 2 | Phase ② 実衁E バッチ監査 (189 batch, 10 parallel) | [x] |
| 2.3 | 2 | Phase ② 完亁E 結果統合ファイル作�E | [x] |
| 3 | 3 | Phase ③ 修正候補集紁E(B重褁E��ェチE��含む) | [x] |
| 4 | 4 | Phase ④ F262 作�E | [x] |

### Task 2.0 詳細 (Phase ② 設訁E 完亁E

**v3 バッチ�E琁E��訁E* (2025-12-29 ユーザー協議により決宁E

| 頁E�� | v2 設訁E(廁E��) | v3 設訁E(採用) |
|------|---------------|---------------|
| 単佁E| 1 COM = 1 subagent | ファイルベ�EスバッチE|
| dispatch 数 | 92 | 紁E132 |
| grep 使用 | あり (STEP 2) | なぁE|
| コンチE��スト管琁E| なぁE| 70% 上限、E000 衁Ebatch |

### Task 2.1 詳細 (dispatch リスト生戁E

**オーケストレータが実衁E* (subagent ではなぁE

```
入劁E Phase ① 結果 (.tmp/f261-p1-v3/*.txt)
出劁E .tmp/f261-p2-dispatch-list.json

STEP 1: 吁E.txt をパース ↁE(ファイル吁E 関数吁E リスチE
STEP 2: 吁E��数のサイズめEawk で計箁E(厁EERB から)
STEP 3: ファイル単位でグループ化、E000 行上限でバッチ�E割
STEP 4: JSON 出劁E
```

**完亁E��宁E*: `.tmp/f261-p2-dispatch-list.json` が存在

### Task 2.2 詳細 (バッチ監査実衁E

**入劁E*: dispatch リスチE(Task 2.1 出劁E `.tmp/f261-p2-dispatch-list.json`)

**実行方弁E* (2025-12-29 決宁E:

| 頁E�� | 値 |
|------|-----|
| 総バチE��数 | 111 |
| 並列数 | 10 |
| ラウンド数 | 12 (最終ラウンド�E 1 batch) |
| Agent | general-purpose (haiku) |
| 出力方弁E| Write (TaskOutput 不使用) |
| 完亁E��知 | Glob polling |

**実行手頁E*:
```
1. 10 batch めErun_in_background: true で dispatch
2. subagent は .tmp/f261-p2/batch-{NNN}.jsonl に Write
3. Glob(".tmp/f261-p2/batch-*.jsonl") で完亁E��を確誁E
4. 10 batch 完亁E��、次の 10 batch めEdispatch
5. 全 111 batch 完亁E��で繰り返し
```

**吁Ebatch の処琁E*:

| STEP | 操佁E| 入劁E| 出劁E|
|:----:|------|------|------|
| 1 | 関数冁E��読み取り | 允EERB | 関数本佁E|
| 2 | COMF 定義取征E| COMF{NUM}.ERB | COM吁E SOURCE, EXP |
| 3 | 監査チェチE�� | STEP 1-2 | A/C/D 結果 |
| 4 | 結果出劁E| 監査結果 | batch-{NNN}.jsonl |

**ファイル名復允E��則**:
| Phase ① ファイル吁E| 復允E��果 |
|-------------------|---------|
| `1_美鈴_KOJO_K1_愛撫.txt` | `Game/ERB/口丁E1_美鈴/KOJO_K1_愛撫.ERB` |
| `U_汎用_KOJO_KU_口挿入.txt` | `Game/ERB/口丁EU_汎用/KOJO_KU_口挿入.ERB` |

**完亁E��宁E*:
```bash
wc -l .tmp/f261-p2/batch-*.jsonl | tail -1
# 結果ぁE1515 なら完亁E
```

### Task 2.3 詳細 (結果統吁E

```bash
cat .tmp/f261-p2/batch-*.jsonl > .tmp/f261-phase2-results.jsonl
# ↁE.tmp/f261-phase2-results.md に変換 (サマリ付き)
```

### 完亁E��みタスク

- **Task 1.5**: 検証完亁E�Eユーザー承認渁E(2025-12-29)
- **Task 1.6**: 統合ファイル作�E渁E(2025-12-29、E276衁E
- **Task 2.0**: v3 バッチ�E琁E��計完亁E(2025-12-29)

---

## Completed Pre-work (参老E��報)

以下�E本 Feature 開始前に完亁E��た予備調査。Phase ①②③ の完�E調査で上書きされる、E

- [x] SKILL に COM 100-203 マッピング追加
- [x] COMABLE.ERB + COMF*.ERB から全COMカチE��リ抽出
- [x] 調査結果チE�Eブル作�E (暫定版)
- [x] 誤配置一覧作�E (暫定版)

---

## Investigation Results (暫定版 - Phase ①②③ で上書き予宁E

> **注愁E*: 以下�E予備調査の結果であり、Phase ①②③ の完�E調査結果で上書きされる、E
> 暫定版は Grep ベ�Eスであり、E00% の完�E性は保証されてぁE��ぁE��E

### 調査完亁E 2025-12-28 (暫宁E

### 調査ソース

| ソース | 用送E|
|--------|------|
| `Game/ERB/COMABLE.ERB` | COM番台とカチE��リ名定義 |
| `Game/ERB/COMF*.ERB` | 個別COMのヘッダコメンチE|
| 実際のERBファイル | 現状の関数配置 |

### COM カチE��リ一覧 (COMABLE.ERB + COMF ヘッダ)

| 番台 | COMABLE定義 | COMF侁E| 正規ファイル |
|:----:|-------------|--------|--------------|
| 0番台 (0-11) | 愛撫系コマンチE| 胸揉み筁E| `_愛撫.ERB` |
| 20番台 (20-21) | コミュニケーション系 | キス | `_愛撫.ERB` |
| 40番台 (40-48) | 道�E使用コマンチE| バイブ筁E| `_愛撫.ERB` |
| 60番台 (60-72) | セチE��ス系コマンチE| 正常位筁E| `_挿入.ERB` |
| 80番台 (80-85) | 奉仕系コマンチE| 手淫, フェラ | `_口挿入.ERB` |
| 100番台 (100-106) | SM系コマンチE| スパンキング筁E| `_口挿入.ERB` |
| 120番台 (120-126) | 助扁Eレズプレイ | クンニ強制筁E| `_口挿入.ERB` |
| 140番台 (140-148) | ハ�Eド調教コマンチE| イラマチオ筁E| `_口挿入.ERB` |
| 180番台 (180-189) | 特殊コマンチE| ローション筁E| `_口挿入.ERB` |
| 200番台 (200-203) | 脱衣系 | 上半身脱衣筁E| `_口挿入.ERB` |
| 300番台 (300-316) | 会話親寁E�� | おしめE��り筁E| `_会話親寁EERB` |
| 350番台 (350-352) | 会話親寁E�� | 告白筁E| `_会話親寁EERB` |
| 410番台 (410-415, 463) | 日常系 | 散歩筁E| `_日常.ERB` |
| 500番台 | 派生コマンチE| - | (口上対象夁E |
| 600番台 | 自慰系コマンチE| - | (口上対象夁E |

### 正規ファイル配置ルール

| 正規ファイル | COM篁E�� | カチE��リ |
|--------------|---------|----------|
| `_愛撫.ERB` | 0-11, 20-21, 40-48 | 愛撫/キス/道�E |
| `_挿入.ERB` | 60-72 | 膣/アナル挿入 |
| `_口挿入.ERB` | 80-85, 100-106, 120-126, 140-148, 180-189, 200-203 | 奉仁ESM/ハ�EチE特殁E脱衣 |
| `_会話親寁EERB` | 300-316, 350-352 | 会話親寁E|
| `_日常.ERB` | 410-415, 463 | 日常 |

**配置原則**:
- `_口挿入.ERB` = 「�E/アナル挿入以外�E調教系全般、E
- 命名�E歴史皁E��緯で残ってぁE��が、実�Eは「非挿入調教、E

### 実際のファイル冁EOM刁E��E(調査結果)

| ファイル | 実際のCOM | 正要E| 状慁E|
|----------|-----------|------|:----:|
| 愛撫.ERB | 0-11, 20-21, 40-48 | 0-11, 20-21, 40-48 | OK |
| 挿入.ERB | 60-65 | 60-72 | OK |
| 口挿入.ERB | **61-71**, 80-85, 100-203 | 80-203 | **誤配置あり** |
| 会話親寁EERB | 300-316, 350-352 | 300-316, 350-352 | OK |
| 日常.ERB | 410-415, 463 | 410-415, 463 | OK |

---

## 口上関数全件調査

### 調査対象
`@KOJO_MESSAGE_COM_K*_*` 関数として ERB ファイルに実裁E��れてぁE��全口上関数

### 口上対象COM一覧

#### 愛撫系 (_愛撫.ERB)
COM: 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 20, 21, 40, 41, 42, 43, 44, 45, 46, 47, 48

#### 挿入系 (_挿入.ERB)
COM: 60, 61, 62, 63, 64, 65

#### 口挿入系 (_口挿入.ERB) ※誤配置含む
COM: **65, 66, 67, 68, 69, 70, 71** (誤配置), 80, 81, 82, 83, 84, 85, 100, 101, 102, 103, 104, 105, 106, 120, 121, 123, 126, 140, 141, 142, 143, 144, 145, 146, 147, 148, 180, 181, 182, 183, 187, 188, 189, 200, 201, 202, 203

#### 会話親寁E�� (_会話親寁EERB)
COM: 300, 301, 302, 310, 311, 312, 313, 314, 315, 316, 350, 351, 352

#### 日常系 (_日常.ERB)
COM: 410, 411, 412, 413, 414, 415, 463

---

### キャラ別 COM 実裁E��況E

#### 愛撫.ERB (正要E 0-11, 20-21, 40-48)

| キャラ | 実裁EOM | 状慁E|
|--------|---------|:----:|
| K1 | 0-11, 20-21, 40-48 | OK |
| K2 | 0-11, 20-21, 40-48 | OK |
| K3 | 0-11, 20-21, 40-48 | OK |
| K4 | 0-11, 20-21, 40-48 | OK |
| K5 | 0-11, 20-21, 40-48 | OK |
| K6 | 0-11, 20-21, 40-48 | OK |
| K7 | 0-11, 20-21, 40-48 | OK |
| K8 | 0-11, 20-21, 40-48 | OK |
| K9 | 0-11, 20-21, 40-48 | OK |
| K10 | 0-11, 20-21, 40-48 | OK |
| KU | 0-11, 20-21, 40-48 | OK |

#### 挿入.ERB (正要E 60-72)

| キャラ | 実裁EOM | 状慁E|
|--------|---------|:----:|
| K1 | 60-65 | OK (部刁E��裁E |
| K2 | 60-64 | OK (部刁E��裁E |
| K3 | 60-65 | OK (部刁E��裁E |
| K4 | 60-64 | OK (部刁E��裁E |
| K5 | 60-65 | OK (部刁E��裁E |
| K6 | 60-65 | OK (部刁E��裁E |
| K7 | 60-65 | OK (部刁E��裁E |
| K8 | 60-65 | OK (部刁E��裁E |
| K9 | 60-64 | OK (部刁E��裁E |
| K10 | 60-65 | OK (部刁E��裁E |
| KU | 60 | OK (部刁E��裁E |

#### 口挿入.ERB (正要E 80-203)

| キャラ | 実裁EOM | 誤配置 | 状慁E|
|--------|---------|--------|:----:|
| K2 | 65-71, 80-203 | **65-71** | 要修正 |
| K4 | 65-71, 80-203 | **65-71** | 要修正 |
| K9 | 65-71, 80-203 | **65-71** | 要修正 |
| KU | 61-71, 80-203 | **61-71** | 要修正 |

#### 会話親寁EERB (正要E 300-316, 350-352)

| キャラ | 実裁EOM | 状慁E|
|--------|---------|:----:|
| K1 | 300-316 | OK |
| K2 | 300-316, 350-352 | OK |
| K3 | 300-316 | OK |
| K4 | 300-316, 350-352 | OK |
| K5 | 300-316, 350 | OK |
| K6 | 300-316 | OK |
| K7 | 300-316 | OK |
| K8 | 300-316, 350-352 | OK |
| K9 | 300-316, 350-352 | OK |
| K10 | 300-316, 350-352 | OK |
| KU | 300-315, 350-352 | OK |

#### 日常.ERB (正要E 410-415, 463)

| キャラ | 実裁EOM | 状慁E|
|--------|---------|:----:|
| K2 | 410-415 | OK (463未実裁E |
| K4 | 410-415 | OK (463未実裁E |
| K8 | 410-415 | OK (463未実裁E |
| K9 | 410-415 | OK (463未実裁E |
| K10 | 410-415 | OK (463未実裁E |
| KU | 410-415 | OK (463未実裁E |

---

## 誤配置一覧

### 口挿入.ERB に誤配置されてぁE�� COM 60-72

| キャラ | 誤配置COM | 挿入.ERB の状慁E| 対処 |
|--------|:---------:|-----------------|------|
| K2 | 65-71 | 60-64 (65-71 なぁE | **移勁E* |
| K4 | 65-71 | 60-64 (65-71 なぁE | **移勁E* |
| K9 | 65-71 | 60-64 (65-71 なぁE | **移勁E* |
| KU | 61-71 | 60 のみ (61-71 なぁE | **移勁E* |

### 愛撫.ERB / 会話親寁EERB / 日常.ERB

問題なし（正規�E置のみ�E�E

---

## Output

1. **更新 SKILL**: `.claude/skills/kojo-writing/SKILL.md` に COM 100-203 追加
2. **誤配置一覧**: 本 Feature 冁E(後綁EF262 への入劁E

---

## Links

- [feature-057.md](feature-057.md) - K4 COM統吁E(允E�E刁E��設訁E
- [feature-190.md](feature-190.md) - COM_60 重褁E��涁E
- [feature-221.md](feature-221.md) - 挿入/口挿入混乱解涁E
- [kojo-writing SKILL](../../../archive/claude_legacy_20251230/skills/kojo-writing/SKILL.md) - SSOT

---

## Execution Preparation (2025-12-29)

### 調査経緯

#### 1. 初期調査: ファイル数の不一致

Explore エージェントが、E09件」と報告、Eeature記載�E、E66件」と不一致、E

ユーザーから、E66と409の差の琁E��を�Eに知りたぁE��と持E��を受け、詳細調査を実施、E

```bash
find Game/ERB -name "*.ERB" | wc -l  # ↁE466
```

**原因**: Explore エージェント�Eカウントミス。実際は466件で正しかった、E

#### 2. チE��レクトリ構造確誁E

| チE��レクトリ | ERB数 |
|--------------|------:|
| Root (Game/ERB/) | 292 |
| NTR | 14 |
| グラフィチE��表示 | 2 |
| 会話拡張 | 10 |
| 口丁E| 114 |
| 外�E拡張 | 2 |
| 妖精メイド拡張 | 15 |
| 経歴拡張 | 1 |
| 訪問老E��E��張 | 16 |
| **合訁E* | **466** |

#### 3. 実行方式�E検訁E

**課顁E: コンチE��ストウィンドウ**

ユーザーから「subagentのコンチE��ストウィンドウの限界を�E念してぁE��」と持E��、E

ファイルサイズ調査を実施:
- 最大: 8,926衁E(NTR口丁EERB) ≁E150Kト�Eクン
- 平坁E��上ファイル: 3,000-5,000衁E≁E50-80Kト�Eクン
- haiku コンチE��スト上限: 200K

**結諁E*: 1ファイル単位なら上限冁E��収まる、E

**課顁E: オーケストレータのコンチE��スチE*

ユーザーから「思老E��程が出力されるTaskOutputだと恐らくパンクする」と持E��、E

466件の結果を�EてTaskOutputで受け取るとオーケストレータのコンチE��ストが爁E��する問題、E

**解決筁E*: subagentは結果をファイル出力、オーケストレータはGlobで完亁E��認（ファイル存在チェチE���E�。TaskOutput不使用、E

#### 4. パイロチE��チE��チE

ユーザーから「最初に1個だけ実施し、Eつ目以降�EめE��方を調整してほしい。意図通り、目皁E��り、思想通りsubagentsが動ぁE��ぁE��か」と持E��、E

**対象**: `Game/ERB/口丁E1_美鈴/KOJO_K1_愛撫.ERB`

| 検証頁E�� | grep | subagent | 結果 |
|----------|------|----------|------|
| 関数リスチE| 44件 | 44件 | ✁E完�E一致 |
| カウント表訁E| - | 48と誤訁E| ❁E誤めE|

**評価**: 関数リスト�E正確。カウント誤りあるが、Phase ② への入力データ�E�どのファイルにどの関数�E��E正確に取得可能、E

#### 5. 重褁E���Eの拁E��確誁E

ユーザーから「関数の重褁E�Eどのphaseが担ぁE��」と確認、E

| Phase | 役割 |
|-------|------|
| Phase ① | ファイル→関数マッピング作�E (入力データ) |
| Phase ② | **重褁E���E実衁E* (Step 4) |
| Phase ③ | 検�E結果の雁E��E�E出劁E|

**確認結果**: Phase ① のリストが正確なめEPhase ② の重褁E���Eは機�Eする、E

### 決定事頁E

| 頁E�� | 決宁E| 琁E�� |
|------|------|------|
| 調査対象 | 全ERB (466件) | Philosophy準拠「�EERBを調査、E|
| 単佁E| 1 ERB = 1 subagent | Feature記載通り |
| 出力方弁E| ファイル出劁E| オーケストレータのコンチE��スト節紁E|
| 出力�E | `.tmp/f261-p1/{filename}.md` | - |
| 完亁E��誁E| Glob (ファイル存在) | TaskOutput不使用 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 | 調査 | opus | COMABLE/COMF解极E| 全COM構造把握 |
| 2025-12-28 | 再定義 | opus | F261めEresearch に変更 | 完亁E|
| 2025-12-28 | SKILL更新 | opus | COM 100-203 マッピング追加 | 完亁E|
| 2025-12-28 | 全件調査 | opus | 口上関数全件grep調査 | 誤配置4件検�E (暫宁E |
| 2025-12-28 | 誤配置修正 | opus | 「削除」�E「移動」に訂正 | 完亁E|
| 2025-12-28 | **計画再定義** | opus | 全ERB完�E調査に拡張 | Philosophy/AC/Tasks 更新 |
| 2025-12-29 | 実行準備 | opus | ファイル数確定�E実行方針決宁E| 466件確誁E|
| 2025-12-29 | パイロチE�� | haiku | KOJO_K1_愛撫.ERB 関数抽出 | リスト正確 |
| 2025-12-29 | Phase ① v1 | haiku | 全466 ERB関数抽出 (バッチE | 問題あり�Ev2再実行決宁E|
| 2025-12-29 | 整琁E| opus | create_markdowns.sh めE.tmp/ に移勁E| 完亁E|
| 2025-12-29 | Phase ① v2 | haiku | 全466 ERB関数抽出 (24バッチE補宁EバッチE | 完亁E 466件 |
| 2025-12-29 | Phase ② v1 | sonnet | 全COM意味皁E��査 (COM篁E��バッチE�E) | **不完�E**: 定義と乖離 |
| 2025-12-29 | Phase ③ v1 | opus | 修正候補集紁E�E出劁E| **無効**: Phase②に依孁E|
| 2025-12-29 | Phase ④ v1 | opus | F262 作�E | **無効**: Phase③に依孁E|
| 2025-12-29 | **リセチE��** | opus | Phase②-④再定義、F262削除、旧出力削除 | Task 2-4 [ ]、AC 2-4 [ ] |
| 2025-12-29 | **Phase① v3決宁E* | opus | 全タスクリセチE��、v3プロンプト設訁E| Task 1-4 [ ]、AC 1-4 [ ] |
| 2025-12-29 | **CHECKPOINT追加** | opus | Phase ①検証 追加 (Phase ①→② 閁E | ユーザー承認忁E��化 |
| 2025-12-29 | **チE��プレ修正** | opus | v3 Input Template に Write チE�Eル追加 | stdout→ファイル出劁E|
| 2025-12-29 | **Phase ① v3 完亁E* | haiku | 全286残件めE5バッチで並列実衁E| 466/466件完亁E|
| 2025-12-29 | **Phase ①検証 完亁E* | opus+haiku | 照合検証4件実施、表記揺れ発要E| 全件一致、ユーザー承認征E�� |
| 2025-12-29 | **Phase ① 承誁E* | user | 品質承認、統合ファイルはPhase②開始時に作�E | Phase ② 実行可 |
| 2025-12-29 | **Phase ② v2 設訁E* | opus+user | STEP 2 めEPhase ① 結果検索に変更 | Philosophy 準拠 |
| 2025-12-29 | **grep 正当性検証** | opus+user | EVENT_KOJO.ERB 確認、パターン完�E性確誁E| 92 COM 確宁E|
| 2025-12-29 | **確定値** | - | 口上関数 1515、unique COM 92 | Phase ② 実行可 |
| 2025-12-29 | **Phase ② v3 設訁E* | opus+user | 技術的検証: 1 COM = 1 subagent 不可能 | コンチE��スチE20 倍趁E�� |
| 2025-12-29 | **v3 設計協議** | opus+user | バッチ�E琁E��計、E0% マ�Eジン、E000 衁Ebatch | 紁E132 dispatch |
| 2025-12-29 | **v3 設計完亁E* | opus | feature.md 更新: Phase②/Tasks/設計変更セクション | Task 2.0 [x] |
| 2025-12-29 | **数値確誁E* | opus+user | 1472ↁE515 修正、NTR_KOJO 42件発要E| 確定値更新 |
| 2025-12-29 | **パターン網羁E��誁E* | opus | 全 MESSAGE_COM パターン検証 | 監査対象外パターン除夁E|
| 2025-12-29 | **v1 残骸削除** | opus | .tmp/f261-p2-com*.md 6件削除 | v3 設計に統一 |
| 2025-12-29 | **Phase ② 完亁E* | haikuÁE89 | 全 1506 関数監査 | 46 NG 検�E |
| 2025-12-29 | **Phase ③ 完亁E* | opus | 修正候補抽出 | .tmp/f261-fix-candidates.md |
| 2025-12-29 | **Phase ④ 完亁E* | opus | F262 作�E | pm/features/feature-262.md |
| 2025-12-29 | **F261 完亁E* | opus | Status ↁEDONE | 全 AC 達�E |

---

## Phase ② v3 関数パターン確誁E(2025-12-29)

### 全 MESSAGE_COM パターン刁E��

Phase ① 結果から抽出した全パターン:

| パターン | 件数 | 監査対象 | 琁E�� |
|----------|-----:|:--------:|------|
| @KOJO_MESSAGE_COM_K{N}_{COM} | 1473 | ✁E| EVENT_KOJO.ERB で呼び出されめE|
| @NTR_KOJO_MESSAGE_COM_K{N}_{COM} | 42 | ✁E| EVENT_KOJO.ERB で呼び出されめE|
| @MESSAGE_COM{N} | ~130 | ❁E| EVENT_MESSAGE_COM.ERB (エンジン側) |
| @MESSAGE_COM_300_* | 10 | ❁E| 会話拡張、KOJO_ プレフィチE��スなぁE|
| @NTR_MESSAGE_COM_K{N}_{COM} | 14 | ❁E| KOJO_ プレフィチE��スなし、呼び出されなぁE|

### 監査対象の確宁E

**監査対象**: `@(NTR_)?KOJO_MESSAGE_COM_K[0-9U]+_[0-9]+` パターンのみ

- 合訁E 1475 + 42 = **1517 関数**
- EVENT_KOJO.ERB の TRYCALLLIST で呼び出されるパターンに限宁E

### 監査対象外�E琁E��

1. **@MESSAGE_COM{N}** (EVENT_MESSAGE_COM.ERB)
   - エンジン側の関数定義、キャラ口上ではなぁE

2. **@MESSAGE_COM_300_*** (会話拡張)
   - 会話拡張_ComF300ex*.ERB に存在
   - KOJO_ プレフィチE��スがなく、EVENT_KOJO.ERB で呼び出されなぁE

3. **@NTR_MESSAGE_COM** (KOJO_ なぁE
   - EVENT_KOJO.ERB は `NTR_KOJO_MESSAGE_COM` を呼び出ぁE
   - `NTR_MESSAGE_COM` (KOJO_ なぁE は呼び出されなぁE
   - 無効な関数定義の可能性

---

## Phase ② v2 設計変更 (2025-12-29)

### 問題提起

Phase ② 開始前にユーザーから持E��:

> 「unique を抜き�Eしてそれぞれに実行することが思想にあってぁE���E�v2 から v3 めE��直した琁E��はなんだっけ？、E

### v2 ↁEv3 めE��直し�E琁E�� (振り返り)

| 問顁E| 詳細 |
|------|------|
| v2 出力形式�E逸脱 | カウント�Eみ、E*関数名リストなぁE* |
| Phase ② への連鎖障害 | 関数名リストがなぁE��めEgrep で COM 番号抽出 ↁE**Philosophy 違反** |

**Philosophy の核忁E*:
> Grep/正規表現は完�E性を保証できなぁE��E

### 現衁EPhase ② 設計�E問顁E

STEP 2 (修正剁E:
```
STEP 2: 口上関数検索
  - Grep: Game/ERB/口丁E で "@KOJO_MESSAGE_COM_K.*_{NUM}$" を検索
```

ↁE**厁EERB ファイルに grep** = Philosophy 違反

v3 でめE��直して LLM が�Eファイルを直接確認した�Eに、Phase ② で再�E grep を使ぁE�Eは矛盾、E

### 解決筁E

| 頁E�� | 修正剁E| 修正征E|
|------|--------|--------|
| STEP 2 検索対象 | `Game/ERB/口丁E` (厁EERB) | `.tmp/f261-p1-v3/*.txt` (Phase ① 結果) |
| grep の意味 | 発要E(完�E性不保証) | 既知チE�Eタの検索 (LLM 検証済み) |

### 設計原剁E�E整琁E

| 操佁E| 対象 | Philosophy 判宁E|
|------|------|:---------------:|
| 発要E(何が存在するぁE | 厁EERB | ❁Egrep 禁止、LLM 忁E��E|
| 検索 (既知チE�Eタから抽出) | Phase ① 結果 | ✁Egrep OK (LLM 検証済み) |
| 冁E��読み取り (監査用) | 厁EERB | ✁ERead OK (対象は既知) |

### C/D チェチE��につぁE��

| チェチE�� | 入劁E| 厁EERB 参�E |
|----------|------|:-----------:|
| A 配置 | Phase ① 結果 (ファイル吁E | 不要E|
| B 重褁E| Phase ① 結果 (関数吁E | 不要E|
| C SOURCE 整吁E| 口上�E容 | **忁E��E* (STEP 3 で Read) |
| D 冁E��品質 | 口上�E容 | **忁E��E* (STEP 3 で Read) |

C/D は口上�E容の意味皁E��査であり、原 ERB の Read が忁E��、E
ただし、STEP 2 で「何を読むか」�E Phase ① 結果から特定済みなので、Philosophy 準拠、E

### grep パターンの正当性検証 (2025-12-29)

#### 問題提起

> 「GREPで絶対に口上�Eみを抜けるか？口上が `@KOJO_MESSAGE_COM_K{N}_{NUM}` だと断言できるか？、E

Philosophy:
> **パターン漏れ**: 想定外�E関数命名パターンを見送E��可能性

#### 検証: プログラムによる呼び出しパターン

EVENT_KOJO.ERB 38-43行目:
```erb
TRYCALLLIST
    FUNC NTR_KOJO%RESULTS%_MESSAGE_COM_K{対象}_{SELECTCOM}
    FUNC KOJO%RESULTS%_MESSAGE_COM_K{対象}_{SELECTCOM}
    FUNC KOJO%RESULTS%_MESSAGE_COM_KU_{SELECTCOM}
    FUNC TRAIN_MESSAGE
ENDFUNC
```

**プログラムが呼び出すパターン (完�E牁E**:

| パターン | 侁E| 呼び出し�E |
|----------|-----|-----------|
| 基本形 | `KOJO_MESSAGE_COM_K1_60` | EVENT_KOJO |
| 汎用形 | `KOJO_MESSAGE_COM_KU_60` | EVENT_KOJO |
| NTR形 | `NTR_KOJO_MESSAGE_COM_K1_60` | EVENT_KOJO |
| 派生形 | `KOJO_MESSAGE_COM_K1_60_1` | COMF*.ERB |

#### 結諁E

**断言できる**: プログラムは `*KOJO*_MESSAGE_COM_K{N}_{NUM}*` パターンでしか口上関数を呼び出さなぁE��E

ↁEgrep で `KOJO_MESSAGE_COM` を含む関数を抽出すれば **完�E性が保証されめE*、E

#### grep パターン修正

| 頁E�� | 修正剁E| 修正征E|
|------|--------|--------|
| パターン | `KOJO_MESSAGE_COM_K[0-9U]*_{NUM}$` | `KOJO_MESSAGE_COM_K[0-9U]*_{NUM}` |
| 琁E�� | 基本形のみ | 派生形 (`_1`, `_2` 筁E も含む |

#### 確定値 (2025-12-29 検証済み)

| 頁E�� | 値 |
|------|---:|
| 口上関数総数 | 1517 |
| unique COM 番号 | 92 |

**派生形の侁E*:
- `@KOJO_MESSAGE_COM_K1_0` (基本形、COM 0)
- `@KOJO_MESSAGE_COM_K1_0_1` (派生形、COM 0)
- `@KOJO_MESSAGE_COM_K1_60_1` (派生形、COM 60)

派生形は同じ COM にカウントされる、E~Phase ② は 92 COM ÁEsubagent で実行、E~ (v3 で廁E��)

---

## Phase ② v3 設計変更 (2025-12-29)

### 問題提起: 1 COM = 1 subagent の実現可能性

v2 設計、E2 COM ÁEsubagent」�E技術的検証を実施、E

#### 調査: COM あたり�E関数数刁E��E

```
COM 300: 131 関数 (最大)
COM 463: 54 関数
COM 312: 34 関数
...
多くの COM: 22 関数程度
```

#### 調査: COM 300 の処琁E��忁E��なコンチE��スチE

COM 300 (会話親寁E の監査には 11 キャラ刁E�E会話親寁E��ァイルめERead する忁E��がある:

| ファイル | サイズ |
|---------|-------:|
| 咲夜_会話親寁EERB | 208 KB |
| レミリア_会話親寁EERB | 168 KB |
| パチュリー_会話親寁EERB | 162 KB |
| ... | ... |
| **合訁E* | **1.5 MB** |

**ト�Eクン見積もめE*:
- 1.5 MB ÁE3 tokens/斁E��E(日本誁E ≁E**4,500,000 tokens**
- haiku コンチE��スト上限: **200,000 tokens**

**結諁E*: 1 COM = 1 subagent は **技術的に不可能**、EOM 300 だけで haiku コンチE��ストを 20 倍趁E��、E

### 代替案�E検訁E

#### 桁EA: 1 関数 = 1 subagent (1515 dispatch)

| 頁E�� | 値 |
|------|-----|
| dispatch 数 | 1515 |
| grep 使用 | なぁE|
| 吁Esubagent の負荷 | 佁E(1 関数のみ) |

**調査: 関数サイズ刁E��E*:

| サイズ | 関数数 | % | haiku で処琁E|
|--------|-------:|---:|:------------:|
| 0-50 衁E| 996 | 68% | ✁E|
| 51-100 衁E| 58 | 4% | ✁E|
| 101-200 衁E| 356 | 24% | ✁E|
| 201-400 衁E| 40 | 3% | ✁E|
| 401-1000 衁E| 16 | 1% | ✁E(ギリギリ) |
| 1001+ 衁E| 6 | <1% | ❁E(刁E��忁E��E |

**問顁E*: 1515 dispatch は多すぎる ↁEバッチ化を検訁E

#### 桁EB: ファイルベ�Eスバッチ�E琁E

ユーザーからの提桁E
> 「負荷が小さぁE��のは 1 subagent : multi 関数にするか？、E
> 、E515 回ディスパッチでも構わなぁE��、E

**設訁E*:
- ファイル単位でグループ化
- サイズ制紁E(1000 衁Ebatch) でバッチ�E割
- 紁E132 dispatch に削渁E

### コンチE��スト計箁E

**安�Eマ�Eジンの議諁E*:

ユーザー:
> 「安�Eマ�Eジンはどの程度設ける�E�E0%未満でめEauto compact は発動すると思うが、E

**決宁E*: 70% を上限とする

| 頁E�� | 値 |
|------|---:|
| haiku コンチE��スチE| 200K tokens |
| 安�Eマ�Eジン | 70% |
| 使用可能 | 140K tokens |
| シスチE�� + 出劁E| 15K tokens |
| 入力用 | 125K tokens |
| ↁE行数換箁E| **1000 衁Ebatch** |

### sonnet vs haiku

ユーザーからの確誁E
> 「VSCode 拡張の sonnet は本当に大きいのか？、E

**回筁E*: 全モチE��同じ **200K tokens**。sonnet が大きいとぁE��前提は誤り、E

ↁE1001+ 行関数は刁E��読み込みで対忁E(sonnet にフォールバックしても解決しなぁE

### Philosophy 整合性確誁E

| スチE��チE| 処琁E| grep 使用 | 判宁E|
|----------|------|:---------:|:----:|
| 関数リスト取征E| Phase ① 結果をパース | なぁE| ✁E|
| サイズ計測 | 厁EERB めEawk で行数カウンチE| なぁE| ✁E|
| バッチ割彁E| アルゴリズム | なぁE| ✁E|
| 監査実衁E| LLM 直接 Read | なぁE| ✁E|

**サイズ計測につぁE��**:
- 「何が口上か」�E判定ではなぁE(それは Phase ① で完亁E
- 「何行か」�E計測のみ ↁEPhilosophy 準拠

### 最終設訁E(v3)

| 頁E�� | 値 |
|------|-----|
| 単佁E| ファイルベ�EスバッチE|
| dispatch 数 | 紁E132 |
| grep 使用 | なぁE|
| コンチE��スト管琁E| 70% 上限、E000 衁Ebatch |
| 1001+ 行関数 | 刁E��読み込み (6 関数、E2 dispatch) |

---

## Phase ①検証 結果 (2025-12-29)

### 基本検証

| # | 検証冁E�� | 結果 | 詳細 |
|:-:|----------|:----:|------|
| 1 | ファイル数 | ✁E| 466/466 |
| 2 | 出力形弁E| ✁E| 1衁E関数、�Eて `@` で始まめE|
| 3 | encoding | ✁E| UTF-8/ASCII text |
| 4 | 空ファイル | ✁E| 7件 (関数なしERB、許容) |
| 5 | 形式逸脱 | ✁E| カウント表記�E説明文の混入なぁE|

### 照合検証 (subagent による原ファイルとの比輁E

| サンプル | Phase① 出劁E| 原ファイル | 結果 |
|----------|------------:|----------:|:----:|
| K1愛撫 (1_美鈴) | 44 | 44 | ✁E|
| K4挿入 (4_咲夁E | 11 | 11 | ✁E|
| KU口挿入 (U_汎用) | 113 | 113 | ✁E|
| EVENT_MESSAGE_COM | 146 | 146 | ✁E|

**結諁E*: Phase ① v3 出力�E信頼できる、E

### 発見事頁E COM 番号の表記揺めE

#### 現象

COM 番号抽出時に、E0」が検�EされぁE

```bash
cat .tmp/f261-p1-v3/*.txt | grep -o 'KOJO_MESSAGE_COM_K[0-9U]*_[0-9]*' | \
  sed 's/.*_//' | sort -n | uniq
# 結果に "0" と "00" の両方が含まれる
```

#### 原因

原ファイルでの命名規則の揺めE

| ファイル | 関数吁E| 意味 |
|----------|--------|------|
| K10愛撫 | `@KOJO_MESSAGE_COM_K10_00` | COM 0 (ゼロパディング) |
| K10愛撫 | `@KOJO_MESSAGE_COM_K10_0_1` | COM 0 派生関数 |

6ファイル�E�E2, K4, K8, K9, K10, KU�E�で COM 0 めE`_00` と表記、E

#### 対処方釁E

**Phase ② 開始時に正規化する** (Phase ① 出力�E原ファイル忠実�Eまま保持):

```bash
# Phase ② COM 番号抽出 (正規化牁E
cat .tmp/f261-p1-v3/*.txt | grep -o 'KOJO_MESSAGE_COM_K[0-9U]*_[0-9]*' | \
  sed 's/.*_//' | sed 's/^00$/0/' | sort -n | uniq
```

**琁E��**:
- Phase ① は「原ファイルの忠実な抽出」が目皁EↁE変更しなぁE
- Phase ② は「COM 単位�E監査」が目皁EↁE意味皁E��同一の COM を統吁E

#### 事後調査結果 (2025-12-30 F265 FL 中に判昁E

**`_0` と `_00` は表記揺れではなく、別目皁E�E関数だった、E*

EVENT_KOJO.ERB のチE��スパッチロジチE��:
```erb
; 1. メイン関数を呼び出ぁE(COM 番号に対忁E
TRYCALLLIST
    FUNC KOJO_MESSAGE_COM_K{対象}_{SELECTCOM}  ; e.g., _0 for COM 0
    ...
ENDFUNC

; 2. メインが�E劁ERETURN 1)した場合、_00 を追加で呼び出ぁE
IF RESULT
    TRYCALLLIST
        FUNC KOJO_MESSAGE_COM_K{対象}_00  ; 汎用追加処琁E
        ...
    ENDFUNC
ENDIF
```

| 関数 | 目皁E| 侁E|
|------|------|-----|
| `_0` | **COM 0 愛撫の口丁E* (メイン) | `@KOJO_MESSAGE_COM_K4_0` |
| `_00` | **全 COM 共通�E追加処琁E* (汎用フォールバック) | `@KOJO_MESSAGE_COM_K4_00` |

**結諁E*:
- 統一不要E��別目皁E��E
- `_00` が空スタブなのは正常�E�汎用ハンドラとして使わなぁE��合！E
- Phase ② 監査での「K4 COM 0 Empty stub」�E `_00` を指しており、E*false positive**

### ユーザー協議結果

| 確認事頁E| 結果 |
|----------|------|
| Phase ① v3 出力�E品質 | ✁E**承認渁E* (2025-12-29) |
| Phase ② STEP 2 修正桁E| grep ↁEPhase ① v3 結果からの検索に変更 |
| 表記揺れ対処 | Phase ② 開始時に正規化 |
| Phase ② 実衁E| **別セチE��ョンで実衁E* |

---

## Phase ① v1/v2 失敗�E极E(2025-12-29)

> **重要E 新セチE��ョンでの実行前に忁E��読むこと**

### 失敗�E経緯

| バ�Eジョン | 問顁E| 結果 |
|:----------:|------|------|
| v1 | ファイル名衝突E+ 処琁E��れ | 174件欠落 |
| v2 | **出力形式が定義と乖離** | カウント�Eみ、E��数名リストなぁE|

### v2 の根本問顁E

#### 問顁E: 出力形式�E逸脱

**期征E��た�E劁E* (Investigation Plan の Output Template):
```
- KOJO_MESSAGE_COM: 44 functions
  - @KOJO_MESSAGE_COM_K1_0
  - @KOJO_MESSAGE_COM_K1_1
  ...
```

**実際の出劁E* (v2):
```
- KOJO_MESSAGE_COM: 44 functions
(関数名リストなぁE
```

subagent は Output Template を無視し、カウント�Eみ出力した、E

#### 問顁E: Phase ② への連鎖障害

Phase ① の出力に関数名リストがなぁE��めE
- Phase ② の監査対象 COM 番号めEPhase ① 結果から特定不可
- 代替として grep で COM 番号を抽出 ↁE**Philosophy 違反**

#### 問顁E: 、E7 COM」�E信頼性

、E7 COM」�E grep で抽出した値であり、LLM 直接確認ではなぁE
```bash
grep -roh '@KOJO_MESSAGE_COM_K[0-9U]*_[0-9]*' Game/ERB/口丁E | ... | uniq
# ↁE87 unique COM numbers
```

Philosophy 明訁E
> **Grep/正規表現は完�E性を保証できなぁE��E*

### v3 で解決する方況E

| 問顁E| v2 | v3 |
|------|-----|-----|
| 出力形弁E| カウント�Eみ | **関数名�Eみ** (カウント禁止) |
| プロンプト | 褁E�� (刁E��EカウンチE | **単紁E* (抽出のみ) |
| 禁止事頁E| なぁE| **明訁E* (カウント禁止) |

### v3 パイロチE��チE��ト結果

対象: `Game/ERB/口丁E1_美鈴/KOJO_K1_愛撫.ERB`

| 検証頁E�� | grep | subagent (v3) | 結果 |
|----------|:----:|:-------------:|:----:|
| 関数数 | 44 | 44 | ✁E完�E一致 |
| 関数名リスチE| 一致 | 一致 | ✁E完�E一致 |
| 出力形弁E| - | 関数名�Eみ | ✁E期征E��り |

**成功要因**:
- タスクを単純化 (関数名抽出のみ)
- 禁止事頁E��明訁E(カウント�E力禁止)
- 出力形式を厳格匁E(1衁E関数)

---

## Methodology Notes

### Phase ① 実行方弁E
- **允E�E計画**: 1 ERB = 1 subagent (466 dispatch)
- **実際の実衁E*: 20 files/batch ÁE24 batches + 4 supplementary batches
- **琁E��**: コンチE��ストウィンドウ節紁E��並列効玁E
- **結果**: 466/466 ファイル完�E抽出達�E

### Phase ② v1 実行方弁E(不完�E・めE��直ぁE
- **允E�E計画**: 1 COM = 1 subagent
- **実際の実衁E*: COM篁E��バッチEÁE6 (COM 0-48, 60-72, 80-85, 100-148, 180-203, 300-463)
- **問題点**:
  - 対象選宁E Phase① 発見COMではなく事前定義篁E��を使用
  - 監査頁E��: 配置+重褁E�Eみ、SOURCE整吁E冁E��品質が未実施
- **結諁E*: 定義と乖離 ↁE**Phase② v2 で再実衁E*

### Philosophy 達�E評価 (Phase① のみ)
- **「�EERBを調査、E*: 達�E (466/466 files)
- **「LLM直接確認、E*: 達�E (全ファイルめELLM ぁERead で直接読叁E
- **「意味皁E��査、E*: **未達�E** (Phase② v2 で再実行予宁E

---

## Phase ① v1 実行経緯と問題�E极E(2025-12-29)

### 実行�E容

- **バッチ実衁E*: 20ファイル/バッチEÁE紁E4バッチE
- **出力�E**: `.tmp/f261-p1/{basename}.md`
- **完亁E��要E*: `ls .tmp/f261-p1/*.md | wc -l` でファイル数確誁E

### 結果

| 頁E�� | 値 |
|------|---:|
| 入力ERBファイル | 466 |
| 出力ファイル数 | 619 |
| 抽出KOJO_MESSAGE_COM関数 | 2,175 |

### 問題発甁E

#### 問顁E: ファイル名衝突E

最初�Eバッチ完亁E��、E58件完亁E��認識したが、実際は174件欠落、E

**原因**: 同名ファイルが褁E��チE��レクトリに存在し、�E力が上書きされた、E

| 衝突ファイル吁E| 存在数 | 上書き消失 |
|---------------|-------:|----------:|
| WC系口丁EERB | 10 | 9 |
| SexHara休�E中口丁EERB | 10 | 9 |
| NTR口上_お持ち帰めEERB | 10 | 9 |
| NTR口丁EERB | 9 | 8 |
| NTR口上_野外調敁EERB | 2 | 1 |
| COMF421.ERB | 2 | 1 |
| **合訁E* | 43 | 37 |

#### 問顁E: バッチ�E琁E��れ

174件欠落のぁE��、衝突による消失は37件のみ。残り137件は**バッチ�E琁E�E体が実行されなかっぁE*、E

| チE��レクトリ | 欠落件数 | 原因 |
|--------------|--------:|------|
| NTR/ | 11 | 処琁E��れ |
| グラフィチE��表示/ | 2 | 処琁E��れ |
| 会話拡張/ | 10 | 処琁E��れ |
| 口丁E吁E��ャラ | ~100 | 衝突E処琁E��れ混在 |
| 拡張系 | ~50 | 処琁E��れ |

#### 問顁E: 事後対応�E不整吁E

欠落検�E後、E74件のみ親チE��レクトリプレフィチE��ス付きで再実行、E

結果、�E力が**混在状慁E*に:
- プレフィチE��スなぁE 允E�Eバッチで処琁E��れたファイル
- プレフィチE��スあり: 追加バッチで処琁E��れたファイル

### 根本原因

1. **設計ミス**: 出力ファイル名に親チE��レクトリを含めなかっぁE
2. **検証不足**: ファイル数だけで完亁E��定し、衝突を見送E��ぁE
3. **場当たり対忁E*: 欠落刁E�Eみ命名規則を変更し、一貫性を失っぁE

### ユーザー持E��

> 「衝突検知が正しい根拠は�E�また、最初に検�Eされなかった理由も知りたぁE��E

衝突検知は**推測に過ぎぁE*、厳寁E��検証はしてぁE��かった。�E琁E��れも大きな原因であり、説明が不正確だった、E

> 「思想を老E��ると、Eつの瑕疵も許したく�EなぁE��議論をもとにプレフィチE��スありで再度めE��直すことを提案する、E

### v2 再実行方釁E

| 頁E�� | v1 | v2 |
|------|-----|-----|
| 出力�E | `.tmp/f261-p1/` | `.tmp/f261-p1-v2/` (新要E |
| 命名規則 | `{basename}.md` | `{親チE��レクトリ}_{basename}.md` |
| 適用篁E�� | 衝突ファイルのみ (場当ためE | **全ファイル統一** |
| 完亁E��証 | ファイル数のみ | **466件一致を確誁E* |

#### プレフィチE��ス規則

| ERBパス | 出力ファイル吁E|
|---------|---------------|
| `Game/ERB/ABL.ERB` | `ERB_ABL.md` |
| `Game/ERB/NTR/NTR.ERB` | `NTR_NTR.md` |
| `Game/ERB/口丁E1_美鈴/KOJO_K1_愛撫.ERB` | `1_美鈴_KOJO_K1_愛撫.md` |
| `Game/ERB/妖精メイド拡張/COMF421.ERB` | `妖精メイド拡張_COMF421.md` |

---

## Notes

### F057/F065 の、E0-148, 180-203 ↁE口挿入」設計につぁE��

F057 設計時点で COM 80-203 めE`_口挿入.ERB` にまとめたのは**意図皁E*であっぁE
- COM 80-85: 奉仕系 (フェラ筁E
- COM 100+: SM/ハ�EチE特殁E(ぁE��れも非挿入)

命名�E「口」�E「口を使ぁE��ではなく、「�E/アナル**以夁E*」とぁE��意味で使われてぁE��、E

### 後綁EFeature

- **F262**: ファイル配置修正 (A カチE��リ 13 関数)
- **F265**: 吁E��口上品質修正 (C/D 残件 - 褁E��キャラ)
- **F266**: K4 口挿入 SOURCE 修正 (C カチE��リ 18 関数)
- **F267**: K4 NTR 口上スタブ実裁E(D カチE��リ 4 関数)

---

## 既知の問顁E Phase ② 出力形式不一致 (2025-12-29)

### 現象

Phase ② batch 監査の出力形式が一部の batch で異なってぁE��:

| 形弁E| 侁E| 行数 |
|------|-----|------|
| 正しい JSONL | batch-001 | 1衁E関数 |
| ネスト形弁E| batch-090 | 1行で batch 全佁E(functions 配�E冁E��褁E��関数) |

### 影響

- dispatch list: **1515 関数**
- 出力行数: **1509 衁E*
- 差異 6 衁E= ネスト形弁Ebatch で褁E��関数ぁE1 行にまとめられた

### 結諁E

**チE�Eタ欠落なぁE* - 全 1515 関数は監査済み。Phase ③ の fix-candidates 抽出 (46 件) も正常、E

### 原因

subagent prompt で出力形式を厳寁E��強制しなかった。一部の haiku が独自解釈でネスチEJSON を�E力、E

### 封E��への教訁E

batch 処琁E�� subagent に JSONL 出力を求める場吁E

```
## 出力形弁E(厳宁E
- 1関数 = 1衁E
- ネスト禁止
- batch 全体を 1 JSON にまとめてはならなぁE
```
