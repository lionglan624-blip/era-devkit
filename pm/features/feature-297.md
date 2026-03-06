# Feature 297: Phase 対応ツール連携更新

## Status: [DONE]

## Type: infra

## Background

### Philosophy
kojo-writing SKILL を SSOT として、各ツール/コマンドが Phase 情報を参照し、Phase 8d/8j/8l に対応した実装フローを実現する

### Problem
現在のツール群は Phase 8d 固定:
- kojo_mapper.py: `CURRENT_PHASE = "C2"` ハードコード
- kojo-init.md: `(Phase 8d)` 固定テンプレート
- eratw-reader.md: Phase 別抽出方針なし

### Goal
各ツールを F296 の SKILL 拡張と連携させ、Phase 8j/8l の Feature 作成・実装が可能になるようにする。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | kojo_mapper.py に Phase 8j 対応 | code | Grep | contains | "8j" or updated PHASE_REQUIREMENTS | [x] |
| 2 | kojo_mapper.py に Phase 8l 対応 | code | Grep | contains | "8l" or updated PHASE_REQUIREMENTS | [x] |
| 3 | kojo-init.md に Phase 別テンプレート選択 | code | Grep | contains | "Phase 8l" and "Template" | [x] |
| 4 | eratw-reader.md に FIRSTTIME 抽出方針 | code | Grep | contains | "FIRSTTIME" | [x] |
| 5 | Phase 8l Feature 作成可能 | manual | manual | - | Feature file created with "Phase 8l" | [x] |

### AC Details

**AC5 Manual Verification Steps**:
1. Run `/kojo-init --phase 8l` (or equivalent Phase 8l invocation)
2. Verify feature-{ID}.md file is created
3. Verify file content contains "Phase 8l" in title

**Depends on**: AC3 (kojo-init Phase detection must be implemented first)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo_mapper.py の PHASE_REQUIREMENTS に 8j 追加 | [x] |
| 2 | 2 | kojo_mapper.py の PHASE_REQUIREMENTS に 8l 追加 | [x] |
| 3 | 3 | kojo-init.md に Phase 別テンプレート選択ロジック追加 | [x] |
| 4 | 4 | eratw-reader.md に Phase 別抽出方針追加（FIRSTTIME 識別方法含む） | [x] |
| 5 | 5 | 統合テスト: /kojo-init --phase 8l 実行し Feature ファイル作成を検証 | [x] |

---

## Implementation Notes

### 1. kojo_mapper.py 変更

**現行**:
```python
PHASE_REQUIREMENTS = {
    "C2": {"branch_type": "TALENT_4", "patterns": 1},
    ...
}
CURRENT_PHASE = "C2"
```

**変更案**:
```python
PHASE_REQUIREMENTS = {
    "C2": {"branch_type": "TALENT_4", "patterns": 1},  # Phase 8d (existing)
    "8j": {"branch_type": "NOWEX_TCVAR", "patterns": 4},  # 射精状態（NOWEX:MASTER:11 + TCVAR 分岐）
    "8l": {"branch_type": "FIRSTTIME", "patterns": 1},  # 初回実行（IF FIRSTTIME(SELECTCOM) 判定）
}
# 用語マッピング: Content Level (C2) = Phase 8d TALENT_4 品質基準
# Note: "C2" は既存の Content Level 名、"8j"/"8l" は新規 Phase 識別子
# 命名規則の混在を許容（後方互換性のため）
# CURRENT_PHASE は content-roadmap.md で管理、または引数で指定
```

**代替案**: SKILL を読んで動的に PHASE_REQUIREMENTS を構築

### 2. kojo-init.md 変更

**現行**: Phase 8d 固定テンプレート

**変更案**:
```markdown
### Step 1.5: Phase Detection

**引数フォーマット**:
- `/kojo-init` - 現在の Phase (8d) を使用
- `/kojo-init --phase 8l` - Phase 8l を明示指定
- `/kojo-init --phase 8j 80 81 82` - Phase 8j + 対象 COM 指定

**検出ロジック**:
1. 引数に `--phase` があれば使用
2. なければ `Game/agents/index-features.md` の "Current Phase:" 値を使用

**Phase 別テンプレート選択**:

| Phase | Template | Feature 命名 |
|:-----:|----------|--------------|
| 8d | 現行テンプレート | COM_{NUM} {name} 口上 (Phase 8d) |
| 8j | 射精状態テンプレート | COM_{NUM} 射精状態口上 (Phase 8j) |
| 8l | FIRSTTIME テンプレート | COM_{NUM} 初回実行口上 (Phase 8l) |
```

**後方互換性**: 既存の使用パターン（引数なし、または COM 番号のみ）は変更なく動作。

**代替案**: Phase 8j/8l は kojo-init ではなく手動作成

### 3. eratw-reader.md 変更

**現行**: COM 単位で eraTW 参照をキャッシュ

**追加案**:
```markdown
## Phase-specific Extraction

| Phase | 抽出対象 | Output |
|:-----:|----------|--------|
| 8d | TALENT 分岐パターン | cache/eratw-COM_{X}.txt |
| 8j | NOWEX/TCVAR 分岐 | cache/eratw-COM_{X}-8j.txt |
| 8l | FIRSTTIME ブロック | cache/eratw-COM_{X}-8l.txt |

### Phase 8l 参照方針
1. eraTW に合致 COM あり → FIRSTTIME ブロック抽出
2. eraTW に合致 COM なし → 参考資料パスを返す
   - 参考資料: kojo-phases.md "## Phase 8l: First Execution Guard" セクションを参照

### FIRSTTIME ブロック識別方法
eraTW ソースから FIRSTTIME ブロックを抽出する際:
1. Grep pattern: `IF FIRSTTIME\(SELECTCOM\)`
2. ブロック範囲: `IF FIRSTTIME` から対応する `ENDIF` まで
3. 出力先: `cache/eratw-COM_{X}-8l.txt`

### eratw-reader 呼び出し仕様
kojo-writer が Phase 別抽出を要求する際:
- 入力パラメータ: `{COM: 80, Phase: "8l"}`
- eratw-reader は Phase に応じて適切なブロックを抽出
```

### 4. 判断ポイント

| # | 項目 | 選択肢 | 推奨 |
|:-:|------|--------|:----:|
| 1 | Phase 8j/8l の Feature 作成方法 | A: kojo-init 拡張 / B: 手動作成 | A（将来拡張性） |
| 2 | kojo_mapper の Phase 管理 | A: SKILL 参照 / B: ハードコード更新 | A（SSOT 原則準拠）または B（シンプル、SKILL は概念的 SSOT として維持） |
| 3 | Phase 進行管理場所 | A: content-roadmap.md / B: kojo_mapper.py | A（SSOT） |

---

## Dependencies

- **前提**: F296 kojo-writing SKILL Phase 拡張 (現在: PROPOSED) - SKILL に Phase 8j/8l 定義が追加済みであること
  - **If F296 is not DONE, this feature cannot proceed.**
- **後続**: Phase 8j/8l kojo Feature 作成

---

## Reference (SSOT)

- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md) - Phase 定義（SSOT、F296 完了後）
- [kojo-phases.md](reference/kojo-phases.md) - Phase 詳細仕様（F295 完了済）
- [content-roadmap.md](content-roadmap.md) - Phase 進行管理（SSOT）
- [kojo_mapper.py](../../tools/kojo-mapper/kojo_mapper.py) - 変更対象
- [kojo-init.md](../../.claude/commands/kojo-init.md) - 変更対象
- [eratw-reader.md](../../.claude/agents/eratw-reader.md) - 変更対象
- [feature-296.md](feature-296.md) - SKILL 拡張 Feature（参考）

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 16:43 | START | implementer | Task 1-2 | - |
| 2026-01-01 16:43 | END | implementer | Task 1-2 | SUCCESS |
| 2026-01-01 16:43 | START | implementer | Task 3 | - |
| 2026-01-01 16:43 | END | implementer | Task 3 | SUCCESS |

---

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-295.md](feature-295.md)
- 依存Feature: [feature-296.md](feature-296.md)
