# Feature 336: 全口上セマンティクス監査・修正・恒久対策

## Status: [DONE]

## Type: infra

## Background

### Philosophy
口上は COMF*.ERB のゲームメカニクスと整合した内容であるべき。
また、同じ問題が再発しないよう、ワークフローとドキュメントを改善すべき。

### Problem
COM_98 実装時に発覚：kojo-writer が COMF98.ERB のメカニクス（TARGET→MASTER 挿入）を正しく解釈せず、一部キャラで逆のセマンティクス（MASTER→TARGET 挿入）を出力していた。

**根本原因仮説**:
1. kojo-writer.md の Workflow に COMF*.ERB 読み込みステップがない
2. eraTW に COM_98 がなく、コマンド名「させる」から誤推測
3. kojo-writing SKILL に COMF 確認手順はあるが、kojo-writer.md と整合していない
4. **COMF*.ERB 自体のコメントが誤っている可能性**
   - 例: COMF98 line 66 で「;奴隷のＰ⇔調教者の**Ａ**」と書いてあるが、実際のコードは「**Ｖ**」を処理
   - kojo-writer がコメントを読んでコードを読まなかった場合、誤解釈につながる

過去の「させる」系コマンドで同様の誤りがある可能性が高い。
また、COMF ファイル自体に矛盾やミスリーディングなコメントがある可能性がある。

### Prior Audits and Coverage Gap

F261/F269 (2025-12-28~29) で既存口上の包括的監査を実施済み。
しかし COM_90-99 系（「される」「させる」系）の口上は **監査後に新規作成** されたため未監査。
本 Feature は、これらの新規口上 + 既存口上の再検証を行う。

### Goal
1. 全ての既存口上を監査し、不整合を**特定**する（修正は別 Feature）
2. 根本原因を調査し、再発防止策を実装する
3. skills/agents を更新して恒久対策とする

### Scope Decision
- **F336 スコープ**: 監査 + 根本原因調査 + COMF 検証/修正 + 恒久対策
- **別 Feature**: 発見した不整合口上の修正（kojo-writer 再dispatch）
- **修正方法**: kojo-writer 再dispatch を優先（手動修正ではなく）

**スコープ境界の明確化**:
- **IN SCOPE**: COMF*.ERB ソースコードの修正（コメントとコードの不整合修正）
- **OUT OF SCOPE**: 口上 ERB ファイル（キャラクター台詞）の修正 → Fix Candidates として別 Feature へ

### Investigation Approach (FL議論結果)

**問題の核心**: kojo-writer が COMF を読んだと主張しながら、コメントだけ見てコードを見なかった可能性

**調査範囲**:
| 対象 | 内容 |
|------|------|
| TCVAR:116 パターン | TARGET/PLAYER どちらが行為者か |
| コメント vs コード | 「Ａ」と書いてあるが「Ｖ」を処理、等の不整合 |
| させる系コマンド | 命名と実際のセマンティクスの乖離 |
| 既存口上 | 上記と整合しているか |

**恒久対策 (4レベル)**:
| レベル | 対策 | 実装場所 |
|--------|------|----------|
| 1. 強制読み込み | Workflow に COMF 読み込みを必須ステップ化 | kojo-writer.md |
| 2. 明確な判定表 | TCVAR:116 の意味と口上表現の対応表 | kojo-writing SKILL |
| 3. チェックリスト | 「させる」系コマンドは特に注意を促す警告 | kojo-writing SKILL |
| 4. 監査結果参照 | audit.md を参照資料として登録 | Links セクション |

### Context
- 対象: 全 COM の口上（特に「させる」系コマンド）
- 検証基準: COMF*.ERB の以下の項目
  - `TCVAR:116` - 行為者 (PLAYER or TARGET)
  - `EXP:PLAYER:*` or `EXP:TARGET:*` - 経験値獲得者
  - `STAIN:*` - 汚れの移動方向
- F327 で発覚: COM_98 において K2,K4,K5,K6,K9 が逆セマンティクス（修正済み）

### 「させる」系コマンドの命名の罠
| コマンド名 | 印象 | 実際 (COMF) |
|------------|------|-------------|
| 後背位させる | MASTERがTARGETにさせる | TARGET が MASTER に挿入 |
| 正常位させる | MASTERがTARGETにさせる | TARGET が MASTER に挿入 |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 監査対象COM一覧作成 | file | Glob | exists | Game/agents/reference/kojo-semantics-audit.md | [x] |
| 2 | 全COMのセマンティクス定義 | file | Grep | contains | "## COM Semantics Analysis" | [x] |
| 3 | 不整合検出完了 | file | Grep | contains | "## Inconsistencies" | [x] |
| 4 | 根本原因分析完了 | file | Grep | contains | "## Root Cause Analysis" | [x] |
| 5 | COMFコメント検証完了 | file | Grep | contains | "## COMF Comment Verification" | [x] |
| 6 | COMFコメント修正完了 | file | Grep | contains | "COMF fixes applied: " | [x] |
| 7 | 修正対象リスト作成 | file | Grep | contains | "## Fix Candidates" | [x] |
| 8 | kojo-writer.md 更新 | file | Grep | contains | "COMF読み込み" | [x] |
| 9 | kojo-writing SKILL 更新 | file | Grep | contains | "TCVAR:116" | [x] |
| 10 | Build succeeds | build | - | succeeds | - | [x] |
| 11 | Regression tests pass | output | --flow tests/regression/ | contains | "passed" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | TCVAR:116 = TARGET となる全COM一覧抽出（COMF*.ERBから） | [x] |
| 2 | 2 | 各COMのTCVAR:116/EXP/STAINパターン解析・表作成 | [x] |
| 3 | 3 | 既存口上との整合性チェック・不整合リスト作成 (検証方法: com-auditor subagent による意味的監査) | [x] |
| 4 | 4 | F327実行ログ・kojo-writer動作分析・根本原因特定 | [x] |
| 5 | 5 | COMFコメントとコードの整合性検証（既知例: COMF98 line66「Ａ」→実際は「Ｖ」） | [x] |
| 6 | 6 | COMFコメント修正（誤りがあれば） | [x] |
| 7 | 7 | 修正対象リスト作成（別 Feature で kojo-writer 再dispatch 用） | [x] |
| 8 | 8 | kojo-writer.md に COMF 読み込みステップ追加 | [x] |
| 9 | 9 | kojo-writing SKILL に TCVAR:116 確認強化 | [x] |
| 10 | 10 | ビルド確認 | [x] |
| 11 | 11 | 回帰テスト実行 | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Phase 1: 監査 (Task 1-3)

| Step | Agent | Model | Input | Output |
|------|-------|-------|-------|--------|
| 1.1 | explorer | - | COMF*.ERB で TCVAR:116 = TARGET 検索 | COM 一覧 |
| 1.2 | explorer | - | 各 COMF のセマンティクス抽出 | 表形式データ |
| 1.3 | com-auditor | opus | 口上ファイルと COMF の意味的整合性監査 | 不整合リスト |

### Phase 2: 根本原因調査 (Task 4)

| Step | Agent | Model | Input | Output |
|------|-------|-------|-------|--------|
| 2.1 | explorer | - | feature-327.md Execution Log セクション読み込み | 時系列データ抽出 |
| 2.2 | explorer | - | kojo-writer.md と kojo-writing SKILL 比較 | 乖離箇所リスト |
| 2.3 | - | - | Orchestrator が根本原因と再発条件を特定 (意思決定 = orchestrator の役割) | Root Cause Analysis |

### Phase 3: COMF 検証・修正 + 修正対象リスト (Task 5-7)

| Step | Agent | Model | Input | Output |
|------|-------|-------|-------|--------|
| 3.1 | explorer | - | 全 COMF*.ERB のコメントとコード比較 | 不整合リスト |
| 3.2 | implementer | sonnet | COMF コメント修正（誤りがあれば） | 修正済み COMF |
| 3.3 | implementer | sonnet | 不整合口上の修正対象リスト作成 | Fix Candidates (別 Feature 用) |

**検証対象**:
- コメントの「Ａ」「Ｖ」「Ｐ」等が実際のコードと一致するか
- `;奴隷のＸ⇔調教者のＹ` 形式のコメントが正確か
- TCVAR:116 の行為者コメントが正確か

**修正対象リスト出力形式** (別 Feature 作成用):
```markdown
## Fix Candidates
| COM# | Character | File | Issue |
|------|-----------|------|-------|
| 97 | K2 | KOJO_K2_口挿入.ERB | MASTER→TARGET semantics (should be TARGET→MASTER) |
```

### Phase 4: 恒久対策 (Task 8-9)

| Step | Agent | Model | Input | Output |
|------|-------|-------|-------|--------|
| 4.1 | implementer | sonnet | kojo-writer.md 更新 | 更新済み agent |
| 4.2 | implementer | sonnet | kojo-writing SKILL 更新 | 更新済み SKILL |

### Phase 5: 検証 (Task 10-11)

| Step | Agent | Model | Input | Output |
|------|-------|-------|-------|--------|
| 5.1 | - | - | dotnet build | Build result |
| 5.2 | regression-tester | haiku | 回帰テスト | Test result |

---

## Expected Updates

### kojo-writer.md への追加

```markdown
## Workflow

1. Prompt解析
2. Skill読み込み: kojo-writing
3. Feature読み込み
4. **COMF読み込み**: `Game/ERB/COMF{N}.ERB` で TCVAR:116 確認 ← NEW
5. Cache読み込み: eratw-COM_{X}.txt
6. 対象ファイル特定
7. 既存チェック
8. ERB実装
9. Status出力
```

### kojo-writing SKILL への追加

```markdown
## COMF Verification (CRITICAL - MUST NOT SKIP)

**TCVAR:116 確認必須** - eraTW が存在しない COM では特に重要

| TCVAR:116 | 意味 | 口上での表現 |
|-----------|------|-------------|
| PLAYER | MASTER/PLAYER が行為者 | 「～してあげる」「～する」 |
| TARGET | TARGET/キャラ が行為者 | 「～される」「～してくれる」 |

**「させる」系コマンドの注意**:
- コマンド名「Xさせる」は MASTER 視点の命名
- 実際の行為者は COMF の TCVAR:116 で決まる
- eraTW 不在時は COMF が SSOT
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 14:22 | START | implementer | Task 1-3 (initial) | - |
| 2026-01-04 14:22 | END | implementer | Task 1-3 (initial) | 4件検出 (見落としあり) |
| 2026-01-04 | RE-AUDIT | com-auditor (opus) | COM 96 | 1件 INCONSISTENT |
| 2026-01-04 | RE-AUDIT | com-auditor (opus) | COM 97 | 9件 INCONSISTENT |
| 2026-01-04 | FULL-AUDIT | com-auditor (opus) | 全19 COM並列 | 40件 INCONSISTENT |
| 2026-01-04 | SUMMARY | - | COM 8,9,65,91,95 | ALL PASS |
| 2026-01-04 | SUMMARY | - | COM 11 | 9件 INCONSISTENT |
| 2026-01-04 | SUMMARY | - | COM 90 | 6件 INCONSISTENT |
| 2026-01-04 | SUMMARY | - | COM 92,93 | 各7件 INCONSISTENT |
| 2026-01-04 | SUMMARY | - | COM 94 | 1件 INCONSISTENT |
| 2026-01-04 | SUMMARY | - | COM 99,120,122,126,140,145,160 | NOT_IMPLEMENTED |
| 2026-01-04 | END | com-auditor | Task 3 | SUCCESS: 40件検出 → F338 |
| 2026-01-04 | Regression Test | regression-tester | Task 11 | OK:24/24 |

---

## Links

- [index-features.md](index-features.md)
- [feature-261.md](feature-261.md) - Prior audit (2025-12-28)
- [feature-269.md](feature-269.md) - Prior audit (2025-12-29)
- [feature-327.md](feature-327.md) - 発覚元Feature
- [feature-338.md](feature-338.md) - Fix Candidates 修正 (後続Feature)
- [kojo-semantics-audit.md](reference/kojo-semantics-audit.md) - 監査レポート
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
- [kojo-writer.md](../../.claude/agents/kojo-writer.md)
