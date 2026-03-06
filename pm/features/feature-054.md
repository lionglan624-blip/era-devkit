# Feature 054: TALENT追加

## Status: [DONE]

## Summary

Talent.csv に「恋人」「思慕」を追加し、口上の4段階好感度分岐を可能にする。

## Background

reference/kojo-reference.md では4段階分岐（恋人/恋慕/思慕/なし）を推奨しているが、現在の Talent.csv には：
- TALENT:恋慕 (ID 3) - 存在
- TALENT:親愛 (ID 148) - 存在（未使用）
- TALENT:恋人 - **存在しない**
- TALENT:思慕 - **存在しない**

口上実装（Phase 8b）の前提として、TALENTを追加する必要がある。

## Scope

### In Scope

1. Talent.csv に以下を追加：
   - TALENT:恋人 (新規ID割当)
   - TALENT:思慕 (新規ID割当)

2. 4段階分岐の定義を明確化

### Out of Scope

- kojo-mapper の修正 (Feature 055)
- kojo-reference.md の修正 (Feature 056)
- 既存口上コードの修正

## Technical Details

### TALENT 4段階分岐

| Level | TALENT | ID | Description |
|-------|--------|-----|-------------|
| 4 | 恋人 | 16 | 最高の親愛度。恋人関係 |
| 3 | 恋慕 | 3 | 愛情に似た感情（既存） |
| 2 | 思慕 | 17 | 好意的な感情 |
| 1 | なし | - | デフォルト状態 |

### ID割当

陥落素質系列の後ろに配置：
- 恋人: **ID 16**
- 思慕: **ID 17**

> **Note**: ID 10-14 は性格系、ID 15 は人妻で使用済み

### 使い分け

| 用途 | 基準 |
|------|------|
| 親愛度分岐（新規） | **TALENT**（ABL:親密から統一） |
| 他の分岐 | 好感度、刻印等は維持 |
| ゲーム進行・アンロック | ABL:親密（例：Lv9で結婚指輪贈呈可能） |

**Note**: 既存口上（K2,K4,K8等）はABL:親密分岐のまま。移行不要。

## Acceptance Criteria

- [x] Talent.csv に TALENT:恋人 が追加されている
- [x] Talent.csv に TALENT:思慕 が追加されている
- [x] headless テストで新TALENTが認識される
- [x] 既存の口上が正常動作する（回帰テスト）

## Dependencies

- Feature 053 [DONE]: Feature番号割当計画

## Test Plan

1. **ロードテスト**: headless 起動で Talent.csv がエラーなくロードされる
2. **回帰テスト**: 既存シナリオが正常動作

## Links

- [index-features.md](index-features.md) - Feature tracking
- [kojo-reference.md](reference/kojo-reference.md) - 口上ガイドライン
- [Talent.csv](../CSV/Talent.csv) - TALENT定義ファイル
