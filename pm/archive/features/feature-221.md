# Feature 221: _挿入.ERB vs _口挿入.ERB 混乱の解消

## Status: [DONE]
## Type: infra

## Keywords (検索用)

`_挿入.ERB`, `_口挿入.ERB`, `COM 60`, `COM 60-72`, `COM 80-85`, `ファイル配置`, `重複`, `どっちに書く`

---

## Summary

**問題**: COM 60-72 を `_挿入.ERB` と `_口挿入.ERB` のどちらに書くべきか混乱が発生。

**結論**: **`_挿入.ERB` が正解**。Feature 190 で確定済み。

**SSOT**: `.claude/skills/kojo-writing/SKILL.md`

---

## Background

### 時系列: なぜ混乱が起きたか

| Date | Feature | 決定内容 | 問題 |
|------|---------|----------|------|
| 2025-12-14 | F057 | K4 COM統合設計: `60-148 → _口挿入.ERB` | **誤り**: 挿入系と手技系を区別せず |
| 2025-12-14 | F065 | 全キャラ展開: F057 のパターンを踏襲 | F057 の誤りを継承 |
| 2025-12-24 | F186 | COM_60 実装時に `_挿入.ERB` を新規作成 | F057 と矛盾するが実態は正しい |
| 2025-12-24 | F190 | **正式決定**: COM 60-72 → `_挿入.ERB`, COM 80-85 → `_口挿入.ERB` | ✅ |
| 2025-12-24 | - | SKILL.md を F190 に基づき更新 | ✅ |
| 2025-12-26 | F221(本件) | F057/065 の古い記載を発見、修正 | 本 Feature |

### 根本原因

**Feature 057 の設計時点で、COM 60-72（膣/アナル挿入）と COM 80-85（フェラ/パイズリ）の区別がなかった。**

「口挿入」という名前から COM 60-148 を全て含めてしまったが、実際には:
- COM 60-72: **挿入系**（膣挿入、アナル挿入）→ `_挿入.ERB`
- COM 80-85: **手技系**（フェラ、パイズリ）→ `_口挿入.ERB`

という分類が適切。Feature 190 でこれを正式に確定。

### 現在の状態

| 項目 | 状態 | 備考 |
|------|:----:|------|
| 実際のファイル配置 | ✅ | Feature 190 で修正済み |
| kojo-writing SKILL.md | ✅ | Feature 190 反映済み |
| Feature 057 の記載 | ✅ | Feature 221 で修正完了 |
| Feature 065 の記載 | ✅ | Feature 221 で修正完了 |

---

## 正しい COM → ファイル配置ルール

```
| COM Range | Category | File |
|-----------|----------|------|
| 60-72     | 挿入系   | _挿入.ERB |
| 80-85     | 手技系   | _口挿入.ERB |
```

**覚え方**: 「挿入」は膣/アナル、「口挿入」は口を使う側（フェラ等）

---

## Goals

1. Feature 057/065 のドキュメントを修正
2. 今後の誤りを防止するため WARNING を追加
3. 検索可能なキーワードを残す

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | Feature 057 に正しい COM 配置記載 | code | contains | "60-72" | [x] |
| 2 | Feature 057 に WARNING 追加 | code | contains | "IMPORTANT" | [x] |
| 3 | Feature 065 に正しい COM 配置記載 | code | contains | "_挿入.ERB" | [x] |
| 4 | Feature 065 に WARNING 追加 | code | contains | "IMPORTANT" | [x] |
| 5 | ビルド成功 | build | succeeds | - | [x] |

## Tasks

| # | Task | AC# | Status |
|:-:|------|:---:|:------:|
| 1 | Feature 057 の COM マッピング表を修正 | 1 | [x] |
| 2 | Feature 057 に WARNING セクション追加 | 2 | [x] |
| 3 | Feature 065 のファイル構造コメントを修正 | 3 | [x] |
| 4 | Feature 065 に WARNING セクション追加 | 4 | [x] |
| 5 | ビルド成功確認 | 5 | [x] |

---

## Documentation Update

### Feature 057 への修正

**COM分布表の修正**:

修正前:
```markdown
| 60-148, 180-203 | KOJO_K4.ERB | KOJO_K4_口挿入.ERB |
```

修正後:
```markdown
| 60-72 | KOJO_K4.ERB | KOJO_K4_挿入.ERB |
| 80-148, 180-203 | KOJO_K4.ERB | KOJO_K4_口挿入.ERB |
```

**WARNING セクション追加**:
```markdown
## IMPORTANT: COM File Placement Rules

Feature 190 で以下の配置ルールが確定:

| COM Range | Category | File |
|-----------|----------|------|
| 60-72 | 挿入系 (膣/アナル) | `_挿入.ERB` |
| 80-85 | 手技系 (フェラ/パイズリ) | `_口挿入.ERB` |

**SSOT**: `.claude/skills/kojo-writing/SKILL.md`

詳細は Feature 190, 221 を参照。
```

### Feature 065 への修正

**ファイル構造の修正**:

修正前:
```markdown
├── KOJO_K{N}_口挿入.ERB     # COM60-148, 180-203
```

修正後:
```markdown
├── KOJO_K{N}_挿入.ERB       # COM60-72
├── KOJO_K{N}_口挿入.ERB     # COM80-148, 180-203
```

---

## Lessons Learned

### 再発防止策

1. **SSOT を明確に**: COM 配置ルールは `kojo-writing SKILL.md` が Single Source of Truth
2. **Feature 間の整合性**: 後続 Feature で設計変更があった場合、元 Feature も更新する
3. **命名の曖昧さに注意**: 「口挿入」という名前が誤解を招いた（口を使う ≠ 口に挿入）

### 将来同じ問題に遭遇したら

1. `_挿入.ERB` と `_口挿入.ERB` で迷ったら → **SKILL.md を確認**
2. Feature 057/065 の記載と SKILL.md が矛盾していたら → **SKILL.md が正**
3. 経緯を知りたければ → **Feature 190, 221 を参照**

---

## Related Features

| Feature | 役割 |
|---------|------|
| F057 | K4 COM 統合（元の設計、一部誤り含む） |
| F065 | 全キャラ展開（F057 のパターン継承） |
| F186 | COM_60 実装（`_挿入.ERB` 新規作成） |
| F190 | **COM 配置ルール確定**（重複解消） |
| F221 | 本件（ドキュメント整合性回復） |

---

## Execution Log

| Date | Task | Action | Result |
|------|------|--------|--------|
| 2025-12-26 | initialization | Feature 221 initialized, [WIP] | Ready |
| 2025-12-26 | T1-T4 | Doc fixes to F057/F065 | Done |
| 2025-12-26 | T5 | Build verification | PASS |
| 2025-12-26 | finalization | Feature 221 marked [DONE], all objectives verified | COMPLETED |

## Notes

- Feature 190 で COM 60-72 → `_挿入.ERB` が確定済み
- 重複スタブ削除も Feature 190 で完了済み
- この Feature は**ドキュメントの整合性回復のみ**が目的
