# Feature 337: Phase System / Netorase System Integration

## Status: DONE

**Type**: docs
**Created**: 2026-01-04
**Completed**: 2026-01-04

---

## Summary

phase-system.md と netorase-system.md の設計整合性を確保する。

## Problem

1. **バージョン不整合**: netorase-system.md が v2.0-v2.1 (S2) としているが、content-roadmap.md では Netorase は v6.x (S4) に配置
2. **相互参照不足**: Phase System の Route (R1-R6) や Phase 進行と Netorase System の関係が未定義
3. **パラメータ連携不足**: 許可レベルと Phase パラメータの関係が不明確

## Solution

1. netorase-system.md のバージョンを v6.x (S4) に修正
2. Phase System 統合セクションを追加
3. 許可レベル ↔ Phase パラメータの関係を定義

## Changes

| File | Change |
|------|--------|
| netorase-system.md | Version: v2.0-v2.1 (S2) → v6.x (S4) |
| netorase-system.md | Added "Phase System Integration" section |

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | netorase-system.md version is v6.x | output | contains | "v6.x" | [x] |
| 2 | Phase integration section exists | output | contains | "Phase System Integration" | [x] |

---

## Progress Log

### 2026-01-04

- Created feature
- Fixed version in netorase-system.md
- Added Phase System Integration section
- Marked DONE
