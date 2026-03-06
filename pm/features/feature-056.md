# Feature 056: kojo-guidelines修正

## Status: [DONE]

## Overview

kojo-reference.mdの品質基準を更新し、行数要件の必須化とABL:親密分岐のTALENT統一を行う。

Feature 054でTALENT:恋人/思慕が追加されたため、従来のABL:親密による分岐をTALENTベースに統一する。

## Problem

1. **行数要件が曖昧**: 現在の「4行以上推奨」が必須化されていない
2. **分岐基準の不一致**: ABL:親密とTALENTの両方が分岐条件として使用され混乱を招く
3. **Feature 054との整合性**: TALENT:恋人/思慕が追加されたが、guidelinesが未更新

## Goals

1. 行数要件「4行以上」を必須条件として明確化
2. ABL:親密分岐をTALENT分岐に統一（他の分岐条件は維持）
3. kojo-mapperとの整合性を確保

## Acceptance Criteria

- [x] kojo-reference.mdで行数要件が必須として記載
- [x] ABL:親密分岐がTALENT分岐に統一された記載
- [x] Feature 054で追加したTALENT:恋人/思慕が分岐条件として明記
- [x] 既存の分岐条件（ABL:処女/経験値/EXP等）は維持
- [x] Build succeeds
- [x] Documentation consistency verified

## Scope

### In Scope
- kojo-reference.mdの更新
- 行数要件の必須化
- ABL:親密分岐のTALENT統一

### Out of Scope
- 既存ERBコードの修正（後続Featureで対応）
- 他のABL分岐の変更（処女、経験値等は維持）
- kojo-mapperの機能変更（055で対応済み）

## Effort Estimate

- **Size**: XS (ドキュメント更新のみ)
- **Risk**: Low
- **Testability**: ★★★☆☆ (ドキュメント整合性チェック)

## Links

- [index-features.md](index-features.md) - Feature tracking
- [kojo-reference.md](reference/kojo-reference.md) - 品質基準（統合先）
- [feature-054.md](feature-054.md) - TALENT追加（前提Feature）
- [feature-055.md](feature-055.md) - kojo-mapper拡張（関連Feature）
