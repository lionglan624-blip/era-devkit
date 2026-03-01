# Feature 057: K4 咲夜 COM統合

## Status: [DONE]

## Background

<!-- Session handoff: Record ALL discussion details here -->
- **Original problem**: 口上ファイルの保守性向上。KOJO_K4.ERBが6,082行で巨大、同一COMの関数が複数ファイルに分散
- **Considered alternatives**:
  - ❌ 案A: 現状維持（ファイル分割のみ） - 同一COM分散問題が残る
  - ❌ 案C: 1 COM = 1ファイル - ファイル数が爆発的に増加
  - ❌ 案D: シナリオベース分割 - 同一会話コマンドが複数ファイルに分割される
  - ✅ **案B: COMカテゴリ単位** - 会話系/愛撫系/口挿入系/日常系でグルーピング
- **Key decisions**:
  - 統合対象: `KOJO_MESSAGE_COM_K{N}_{CMD}`, `NTR_KOJO_MESSAGE_COM_K{N}_{CMD}`, 対あなた系
  - 統合対象外: NTR口上.ERB（シナリオベース）, WC系口上.ERB, SexHara休憩中口上.ERB - COM番号に依存しない別系統トリガー
  - NTR口上ファイル命名規則: 「シーン」→「シナリオ」に統一、シナリオ範囲表記（例: `NTR口上_シナリオ1-7.ERB`）
- **Session議論で確定** (2025-12-14):
  - 対あなた口上.ERB → **完全統合・削除**（COM関数→カテゴリ、非COM関数→EVENT.ERB）
  - COM463 → 日常.ERB に含める
  - COM20-21（キス）→ 愛撫.ERB に含める（口挿入ではない）
  - COM310-315 → 会話親密.ERB のまま（feature-057の分類を採用）
- **COMカテゴリ分割命名規則** (2025-12-14 追加決定):
  - 方式: **サブカテゴリ名** - 自己文書化、既存NTR口上と一貫性あり
  - 例: `KOJO_K4_会話親密.ERB` → `KOJO_K4_会話親密_告白.ERB`（分割時）
  - kojo-mapper: `.ERB`除去パターン (`_会話親密`) で分割ファイルも認識
- **Prerequisite**: Quick Win「kojo-mapper COMカテゴリ対応」（SCENE_TYPES更新）✅ 完了

## Overview

COMカテゴリベース分割の先行実施。最大ファイル（KOJO_K4.ERB 6,082行）を持つ咲夜で方針を検証し、他キャラへの展開パターンを確立する。

## Problem

- KOJO_K4.ERBが6,082行で巨大、メンテナンス困難
- 同一COMの関数が複数ファイルに分散（COM300: 3ファイル、COM312: 2ファイル）
- 口上追加時にどのファイルに追加すべきか不明確

## Goals

1. COMカテゴリベースでファイル再編成
2. 他キャラ展開用のパターンとスクリプト確立
3. kojo-mapper互換性確保

## Current Structure (K4 咲夜)

### ファイル一覧

| ファイル | 行数 | 役割 |
|----------|-----:|------|
| KOJO_K4.ERB | 6,082 | 基本口上（88関数） |
| KOJO_K4_NTR拡張.ERB | 870 | NTR版ラッパー（12関数） |
| 対あなた口上.ERB | 1,562 | 対プレイヤー視点（35関数） |
| NTR口上_基本.ERB | 3,619 | NTRシナリオ1-7 → リネーム予定 |
| NTR口上_シーン8.ERB | 3,553 | NTRシナリオ8 → リネーム予定 |
| NTR口上_シーン9.ERB | 3,697 | NTRシナリオ9 → リネーム予定 |
| NTR口上_追加.ERB | 1,905 | NTRシナリオ11-22 → リネーム予定 |
| NTR口上_お持ち帰り.ERB | 2,411 | お持ち帰り |
| NTR口上_野外調教.ERB | 1,084 | 野外調教 |
| WC系口上.ERB | 1,562 | WCシステム |
| SexHara休憩中口上.ERB | 1,008 | セクハラ |

### COM分布（統合対象）

| COM | 現在のファイル | 統合先 |
|-----|----------------|--------|
| 300-315 | KOJO_K4.ERB, 対あなた口上.ERB | KOJO_K4_会話親密.ERB |
| 350-352 | KOJO_K4.ERB, KOJO_K4_NTR拡張.ERB | KOJO_K4_会話親密.ERB |
| 0-9, 20-21, 40-48 | KOJO_K4.ERB | KOJO_K4_愛撫.ERB |
| 60-72 | KOJO_K4.ERB | KOJO_K4_挿入.ERB |
| 80-148, 180-203 | KOJO_K4.ERB | KOJO_K4_口挿入.ERB |
| 410-415, 463 | KOJO_K4.ERB, 対あなた口上.ERB | KOJO_K4_日常.ERB |
| EVENT, COUNTER, SeeYou | KOJO_K4.ERB, 対あなた口上.ERB | KOJO_K4_EVENT.ERB |

## Proposed Structure

```
4_咲夜/
├── KOJO_K4_会話親密.ERB     # COM300-315, 350-352 (通常+NTR+対あなた)
├── KOJO_K4_愛撫.ERB         # COM0-9, 20-21, 40-48
├── KOJO_K4_挿入.ERB         # COM60-72
├── KOJO_K4_口挿入.ERB       # COM80-148, 180-203
├── KOJO_K4_日常.ERB         # COM410-415, 463
├── KOJO_K4_EVENT.ERB        # KOJO_EVENT_K4_*, COUNTER_K4_*, SeeYou_K4_*
├── NTR口上_シナリオ1-7.ERB  # (リネーム: 旧NTR口上_基本.ERB)
├── NTR口上_シナリオ8.ERB    # (リネーム: 旧NTR口上_シーン8.ERB)
├── NTR口上_シナリオ9.ERB    # (リネーム: 旧NTR口上_シーン9.ERB)
├── NTR口上_シナリオ11-22.ERB # (リネーム: 旧NTR口上_追加.ERB)
├── NTR口上_お持ち帰り.ERB   # (統合対象外)
├── NTR口上_野外調教.ERB     # (統合対象外)
├── WC系口上.ERB             # (統合対象外)
└── SexHara休憩中口上.ERB    # (統合対象外)
# 削除: KOJO_K4.ERB, KOJO_K4_NTR拡張.ERB, 対あなた口上.ERB
```

## Acceptance Criteria

- [x] KOJO_K4.ERBがCOMカテゴリ別ファイルに分割される
- [x] KOJO_K4_NTR拡張.ERBの関数が対応カテゴリファイルに統合され、ファイル削除
- [x] 対あなた口上.ERBが完全統合・削除される（COM→カテゴリ、非COM→EVENT）
- [x] 全ファイルがErbLinterエラーなし
- [x] Headlessテストで既存口上が正常動作
- [x] kojo-mapperで関数数が維持される (703関数確認)
- [x] Build成功 (0 errors)
- [x] 他キャラ展開用スクリプト作成 (tools/reorganize_k4.py)
- [x] NTR口上ファイルが新命名規則でリネームされる

## Scope

### In Scope
- K4咲夜のCOMカテゴリベース再編成
- 対あなた口上.ERB、KOJO_K4_NTR拡張.ERBのCOM関連関数統合
- NTR口上ファイルの新命名規則リネーム（4ファイル）
- 展開パターンの文書化

### Out of Scope
- 他キャラクター（K2-K10）の再編成（別Feature）
- 口上内容の変更・追加
- NTR口上.ERBの内容変更（リネームのみ）

## Effort Estimate

- **Size**: Medium (6,082行 + 関連ファイル再編成)
- **Risk**: Medium (複数ファイル統合、動作検証必要)
- **Testability**: ★★★★☆ (既存テスト + kojo-mapper検証)

## Technical Notes

### 削除されるファイル

- `KOJO_K4.ERB` → 分割後削除
- `KOJO_K4_NTR拡張.ERB` → 統合後削除
- `対あなた口上.ERB` → 完全統合後削除（COM→カテゴリ、非COM→EVENT）

### kojo-mapper対応

- ✅ Quick Win完了: SCENE_TYPESにCOMカテゴリパターン追加 (74b33ba)
- ✅ `.ERB`除去パターンに変更済み (5554518) - 分割ファイル対応

## Known Issues (Testing Blockers)

### ~~1. Headlessモード起床後コマンド入力不具合~~ ✅ RESOLVED

- **現象**: 起床(100)後のメイン画面で、コマンド入力(400, 888, 300等)が消費されずに無視される
- **原因**: `Console.ReadLine()` のブロッキング呼び出しとゲーム状態遷移のタイミング不一致
- **修正**: HeadlessWindow.cs で stdin 入力をプリバッファリング方式に変更 (Feature 057)
- **修正コミット**: (pending)

### 2. シナリオ注入タイミング問題

- **現象**: キャラクター変数の注入は起床前に実行されるが、起床処理で位置(CFLAG:300)等がリセットされる
- **影響**: 状態注入で主人公/キャラの位置を設定しても、起床後に無効になる
- **回避策**: 起床後に移動コマンド(400)で位置を設定（Issue 1の修正により可能に）
- **追跡**: Wishlist「起床後の状態注入」

### テスト結果

- **スモークテスト**: ✅ 口上ファイル読み込み成功、ErbLinterエラーなし
- **E2Eテスト**: ✅ 起床後コマンド実行確認済み（Issue 1修正により）

## IMPORTANT: COM File Placement Rules

Feature 190 で以下の配置ルールが確定:

| COM Range | Category | File |
|-----------|----------|------|
| 60-72 | 挿入系 (膣/アナル) | `_挿入.ERB` |
| 80-85 | 手技系 (フェラ/パイズリ) | `_口挿入.ERB` |

**SSOT**: `.claude/skills/kojo-writing/SKILL.md`

詳細は Feature 190, 221 を参照。

## Links

- [index-features.md](index-features.md) - Feature tracking
- [kojo-reference.md](reference/kojo-reference.md) - File restructure policy and guidelines
