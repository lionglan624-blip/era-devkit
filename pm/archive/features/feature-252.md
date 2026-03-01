# Feature 252: COM-Kojo Consistency Audit

## Status: [CANCELLED]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
口上は COMF で定義された SOURCE/EXP（増加する珠・経験）と論理的に整合すべき。
例: COM_69（対面座位アナル）は `SOURCE:快Ａ` を増加させるため、口上もアナル描写を含むべき。

### Problem (Current Issue)
- COMF ファイルには SOURCE（快V/快A/快B等）と EXP（V経験/A経験等）が定義されている
- 口上作成時にこれらの定義を参照せず、COM の行為内容と口上描写が乖離するリスクがある
- 現状、整合性チェックは人力確認のみで、網羅的な検証がない
- 例: V系 COM の口上に V 描写がない、A系 COM に A 描写がない等

### Goal (What to Achieve)
1. kojo-writing SKILL に COMF 参照手順を追加
2. 整合性チェックガイドライン作成
3. 既存口上の矛盾検出・報告（ログ出力）

### Session Context
- **SSOT 原則**: COMF ファイルが SOURCE/EXP の SSOT（派生ドキュメント不要）
- **連携**: kojo-init → kojo-writer → 整合チェック の一貫したフロー

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | kojo-writing SKILL に COMF 参照手順追加 | code | Grep | contains | "## COMF 参照手順" | [x] |
| 2 | 整合性ガイドライン作成（存在確認） | file | Glob | exists | Game/agents/reference/kojo-com-consistency.md | [x] |
| 3 | 整合チェック実行（60系） | file | Grep | contains | "Audit complete" | [x] |

### AC Details

**AC1**: `.claude/skills/kojo-writing/SKILL.md` に以下を追加
```markdown
## COMF 参照手順

口上作成前に `Game/ERB/COMF{NUM}.ERB` を確認:
- SOURCE:快Ｖ → 膣描写必須
- SOURCE:快Ａ → アナル描写必須
- SOURCE:快Ｂ → 胸描写必須
```

**AC2**: `Game/agents/reference/kojo-com-consistency.md` の存在確認（内容はガイドライン準拠を手動検証）

**AC3**: 整合チェック実行（60系のみ）
- ログ出力先: `Game/logs/debug/kojo-consistency-audit.log`
- 完了時に "Audit complete" を出力して検証可能にする

**監査アルゴリズム** (Task 3 実装詳細):
1. 各 COMF{COM}.ERB (COM=60-72) を読み込み SOURCE 行を抽出
2. SOURCE:快Ｖ/快Ａ/快Ｂ の有無を判定
3. 各キャラクター (CHAR=1-10) の口上ファイル検索:
   - キャラ別: `Game/ERB/口上/{CHAR}_*/KOJO_K{CHAR}_挿入.ERB`
   - 汎用: `Game/ERB/口上/U_汎用/KOJO_KU_挿入.ERB`
   - ファイル未存在 → "NOT_IMPLEMENTED" としてログ記録（矛盾ではない）
4. 口上内容（該当 COM 関数）と SOURCE 要件を照合:
   - 快Ｖ あり → 膣/挿入/奥 等のキーワード存在確認
   - 快Ａ あり → アナル/肛門/腸 等のキーワード存在確認
   - 快Ｂ あり → 胸/乳首 等のキーワード存在確認
5. 矛盾（SOURCE あり + キーワードなし）をログに記録
6. 全 COM チェック完了後 "Audit complete: X/Y passed" を出力

**実行方法**: Claude Agent (general-purpose) による手動実行

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo-writing SKILL に COMF 参照手順追加 | [O] |
| 2 | 2 | kojo-com-consistency.md 作成 | [O] |
| 3 | 3 | 整合チェック実行・矛盾報告（ログ出力） | [O] |

---

## Technical Notes

### COMF 構造

```erb
; COMF60.ERB (正常位) 抜粋
SOURCE:快Ｖ = 400
SOURCE:情愛 = 150
SOURCE:苦痛 = 500
SOURCE:露出 = 50
SOURCE:反感 = 300
EXP:Ｖ性交経験 ++
```

### SOURCE 種別と A/B/C/V/M 対応

| SOURCE | 部位 | 口上に期待する描写 | 検証対象 |
|--------|------|-------------------|:--------:|
| 快Ｖ | 膣 | 挿入、奥、膣内の感覚 | ✓ |
| 快Ａ | 肛門 | アナル、腸内の感覚 | ✓ |
| 快Ｂ | 胸 | 乳首、胸の感覚 | ✓ |
| 快Ｃ | 陰核 | クリトリスの感覚 | - |
| 快Ｍ | 口 | 口内、舌の感覚 | - |

※ 快Ｃ/快Ｍ は副次的 SOURCE のため本監査対象外（将来拡張可能）

### 検証対象 COM

- 60系: COM_60-72 (13 COM) - 挿入系（快V/快A/快B を持つ）

※ 対象外:
- 80系: 快Ｃ/快Ｂ が主（快V/快A なし）
- 90系: PLAYER 向け SOURCE が主（TARGET 口上との整合不要）
- 300系: 会話系（快V/快A/快B なし）

### 派生ドキュメント不要の理由

```
COMF{NUM}.ERB (SSOT)
    ↓
kojo-writer が直接参照
    ↓
口上作成
    ↓
整合チェック（ログ出力のみ）
```

`comf-source-map.md` のような派生データドキュメントは SSOT 違反になるため作成しない。

※ AC2 の `kojo-com-consistency.md` はプロセスガイドライン文書であり、COMF データの複製ではないため SSOT 準拠。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | Initialized | initializer | Status→WIP | READY:252:infra |
| 2025-12-27 22:47 | Task 1 | implementer | COMF参照手順追加 | SUCCESS |
| 2025-12-27 22:47 | Task 2 | implementer | ガイドライン作成 | SUCCESS |
| 2025-12-27 22:47 | Task 3 | implementer | 60系監査実行 | SUCCESS (16/120) |
| 2025-12-27 22:48 | Post-Review | feature-reviewer | Philosophy check | READY |
| 2025-12-27 22:52 | CANCELLED | - | キーワード監査→意味的整合監査に変更 | → F253 |

---

## Links

- [index-features.md](index-features.md)
- [feature-251.md](feature-251.md) - Phase 8 Summary 自動化
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
- [kojo-phases.md](reference/kojo-phases.md)
