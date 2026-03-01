# Feature 252: COM-Kojo Consistency Audit

## Status: [CANCELLED]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
口上�E COMF で定義されぁESOURCE/EXP�E�増加する珠・経験）と論理皁E��整合すべき、E
侁E COM_69�E�対面座位アナル�E��E `SOURCE:快�E�` を増加させるため、口上もアナル描�Eを含むべき、E

### Problem (Current Issue)
- COMF ファイルには SOURCE�E�快V/快A/快B等）と EXP�E�E経騁EA経験等）が定義されてぁE��
- 口上作�E時にこれら�E定義を参照せず、COM の行為冁E��と口上描写が乖離するリスクがあめE
- 現状、整合性チェチE��は人力確認�Eみで、網羁E��な検証がなぁE
- 侁E V系 COM の口上に V 描�EがなぁE��A系 COM に A 描�EがなぁE��E

### Goal (What to Achieve)
1. kojo-writing SKILL に COMF 参�E手頁E��追加
2. 整合性チェチE��ガイドライン作�E
3. 既存口上�E矛盾検�E・報告（ログ出力！E

### Session Context
- **SSOT 原則**: COMF ファイルぁESOURCE/EXP の SSOT�E�派生ドキュメント不要E��E
- **連携**: kojo-init ↁEkojo-writer ↁE整合チェチE�� の一貫したフロー

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | kojo-writing SKILL に COMF 参�E手頁E��加 | code | Grep | contains | "## COMF 参�E手頁E | [x] |
| 2 | 整合性ガイドライン作�E�E�存在確認！E| file | Glob | exists | pm/reference/kojo-com-consistency.md | [x] |
| 3 | 整合チェチE��実行！E0系�E�E| file | Grep | contains | "Audit complete" | [x] |

### AC Details

**AC1**: `.claude/skills/kojo-writing/SKILL.md` に以下を追加
```markdown
## COMF 参�E手頁E

口上作�E前に `Game/ERB/COMF{NUM}.ERB` を確誁E
- SOURCE:快�E� ↁE膣描�E忁E��E
- SOURCE:快�E� ↁEアナル描�E忁E��E
- SOURCE:快�E� ↁE胸描�E忁E��E
```

**AC2**: `pm/reference/kojo-com-consistency.md` の存在確認（�E容はガイドライン準拠を手動検証�E�E

**AC3**: 整合チェチE��実行！E0系のみ�E�E
- ログ出力�E: `_out/logs/debug/kojo-consistency-audit.log`
- 完亁E��に "Audit complete" を�E力して検証可能にする

**監査アルゴリズム** (Task 3 実裁E��細):
1. 吁ECOMF{COM}.ERB (COM=60-72) を読み込み SOURCE 行を抽出
2. SOURCE:快�E�/快�E�/快�E� の有無を判宁E
3. 吁E��ャラクター (CHAR=1-10) の口上ファイル検索:
   - キャラ別: `Game/ERB/口丁E{CHAR}_*/KOJO_K{CHAR}_挿入.ERB`
   - 汎用: `Game/ERB/口丁EU_汎用/KOJO_KU_挿入.ERB`
   - ファイル未存在 ↁE"NOT_IMPLEMENTED" としてログ記録�E�矛盾ではなぁE��E
4. 口上�E容�E�該彁ECOM 関数�E�と SOURCE 要件を�E吁E
   - 快�E� あり ↁE膣/挿入/奥 等�Eキーワード存在確誁E
   - 快�E� あり ↁEアナル/肛門/腸 等�Eキーワード存在確誁E
   - 快�E� あり ↁE胸/乳馁E等�Eキーワード存在確誁E
5. 矛盾�E�EOURCE あり + キーワードなし）をログに記録
6. 全 COM チェチE��完亁E��E"Audit complete: X/Y passed" を�E劁E

**実行方況E*: Claude Agent (general-purpose) による手動実衁E

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo-writing SKILL に COMF 参�E手頁E��加 | [O] |
| 2 | 2 | kojo-com-consistency.md 作�E | [O] |
| 3 | 3 | 整合チェチE��実行�E矛盾報告（ログ出力！E| [O] |

---

## Technical Notes

### COMF 構造

```erb
; COMF60.ERB (正常佁E 抜粁E
SOURCE:快�E� = 400
SOURCE:惁E�E = 150
SOURCE:苦痁E= 500
SOURCE:露出 = 50
SOURCE:反感 = 300
EXP:�E�性交経騁E++
```

### SOURCE 種別と A/B/C/V/M 対忁E

| SOURCE | 部佁E| 口上に期征E��る描冁E| 検証対象 |
|--------|------|-------------------|:--------:|
| 快�E� | 膣 | 挿入、奥、�E冁E�E感要E| ✁E|
| 快�E� | 肛門 | アナル、�E冁E�E感要E| ✁E|
| 快�E� | 胸 | 乳首、�Eの感要E| ✁E|
| 快�E� | 陰核 | クリトリスの感要E| - |
| 快�E� | 口 | 口冁E���Eの感要E| - |

※ 快�E�/快�E� は副次皁ESOURCE のため本監査対象外（封E��拡張可能�E�E

### 検証対象 COM

- 60系: COM_60-72 (13 COM) - 挿入系�E�快V/快A/快B を持つ�E�E

※ 対象夁E
- 80系: 快�E�/快�E� が主�E�快V/快A なし！E
- 90系: PLAYER 向け SOURCE が主�E�EARGET 口上との整合不要E��E
- 300系: 会話系�E�快V/快A/快B なし！E

### 派生ドキュメント不要�E琁E��

```
COMF{NUM}.ERB (SSOT)
    ↁE
kojo-writer が直接参�E
    ↁE
口上作�E
    ↁE
整合チェチE���E�ログ出力�Eみ�E�E
```

`comf-source-map.md` のような派生データドキュメント�E SSOT 違反になるため作�EしなぁE��E

※ AC2 の `kojo-com-consistency.md` はプロセスガイドライン斁E��であり、COMF チE�Eタの褁E��ではなぁE��めESSOT 準拠、E

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | Initialized | initializer | Status→WIP | READY:252:infra |
| 2025-12-27 22:47 | Task 1 | implementer | COMF参�E手頁E��加 | SUCCESS |
| 2025-12-27 22:47 | Task 2 | implementer | ガイドライン作�E | SUCCESS |
| 2025-12-27 22:47 | Task 3 | implementer | 60系監査実衁E| SUCCESS (16/120) |
| 2025-12-27 22:48 | Post-Review | feature-reviewer | Philosophy check | READY |
| 2025-12-27 22:52 | CANCELLED | - | キーワード監査→意味皁E��合監査に変更 | ↁEF253 |

---

## Links

- [index-features.md](../index-features.md)
- [feature-251.md](feature-251.md) - Phase 8 Summary 自動化
- [kojo-writing SKILL](../../../archive/claude_legacy_20251230/skills/kojo-writing/SKILL.md)
- [kojo-phases.md](../reference/kojo-phases.md)
