# Feature 340: CFLAG ID Allocation Table

## Status: [DONE]

**Type**: docs
**Priority**: Medium
**Created**: 2026-01-04

---

## Summary

NTR system設計ドキュメントで提案されているCFLAG ID番号と既存CFLAG.csvの衝突を解決し、統一割当テーブルを作成する。

---

## Background

Feature 339の監査中に発見された問題。設計ドキュメントで提案されているIDが既存と衝突している。

### Conflicts Detected

| Proposed ID | Document | Existing Usage | Status |
|:-----------:|----------|----------------|:------:|
| 60-63, 70-78 | ntr-flavor-stats.md | 60-63: 刻印主人, 70-78: 妊娠 (64-69は未使用) | 🔴 Conflict |
| 80-81 | netorase-system.md (現在外出先, 外出同伴者) | 80: 誕生日, 81: ピル使用 | 🔴 Conflict |
| 200-209 | reconciliation-system.md | 200-219: 服装記憶 | 🔴 Conflict |
| 53-59 | netorase-system.md | (unused) | ✅ OK |

### Available ID Ranges

| Range | Slots | Recommended For |
|:-----:|:-----:|-----------------|
| 90-199 | 110 | Large allocations (flavor stats, reconciliation) |
| 260-279 | 20 | Medium allocations |
| 770-799 | 30 | Future expansion |

---

## Tasks

| # | AC# | Task | Status |
|:-:|:---:|------|:------:|
| 1 | 1 | Create unified CFLAG allocation table in reference/ | [x] |
| 2 | 2a,2b | Update designs/ntr-flavor-stats.md: 60-78 → 90-108 (add new IDs, remove old IDs) | [x] |
| 3 | 3a,3b | Update designs/netorase-system.md: 53-59,80-85 → 109-121 (add new IDs, remove old IDs) | [x] |
| 4 | 4a,4b | Update designs/reconciliation-system.md: 200-209 → 122-137 (add new IDs, remove old IDs) | [x] |
| 5 | 5 | Add collision verification statement to allocation table | [x] |
| 6 | 6 | Update reference/ntr-system-audit.md Issue 7 with resolution | [x] |

---

## Proposed Allocation

```
; === NTR System CFLAG Allocation (90-199) ===

; ntr-flavor-stats.md (90-108)
90,チンポ_長さ_ランク
91,チンポ_長さ_mm
92,チンポ_太さ_ランク
93,チンポ_太さ_mm
94,チンポ_硬度
95,チンポ_持続力
96,チンポ_精液量
97,チンポ_精液濃度
98,チンポ_回復力
99,顔ランク
100,フェロモン
101,威圧感
102,口開発度
103,乳開発度
104,膣開発度
105,肛開発度
106,子宮開発度
107,浮気耐性
108,快楽依存

; netorase-system.md (109-121)
109,寝取らせ許可レベル
110,寝取らせ積極度
111,寝取らせ対象指定
112,風俗勤務先
113,風俗勤務中
114,風俗客数累計
115,風俗収入累計
116,現在外出先
117,外出同伴者
118,寝取らせ受容度
119,秘密欲求度
120,風俗堕ち状態
121,主人公利用済み

; reconciliation-system.md (122-137)
122,プラトニック好感度
123,肉欲的好感度
124,信頼度
125,NTR経験回数
126,復縁回数
127,浮気リスク
128,比較基準_サイズ
129,比較基準_持続力
130,比較基準_顔
131,最終NTR相手
132,刻印封印度
133,誘惑段階
134,訪問者干渉度
135,元恋人フラグ
136,元人妻フラグ
137,成長を感じたフラグ
```

---

## AC (Acceptance Criteria)

| AC# | Description | Type | Target | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 統一テーブル作成 | file | Game/agents/reference/cflag-ntr-allocation.md | exists | - | [x] |
| 2a | ntr-flavor-stats 新ID追加 | file | Game/agents/designs/ntr-flavor-stats.md | contains | "90,チンポ_長さ_ランク" | [x] |
| 2b | ntr-flavor-stats 旧ID削除 | file | Game/agents/designs/ntr-flavor-stats.md | not_contains | "60,チンポ_サイズ" | [x] |
| 3a | netorase-system 新ID追加 | file | Game/agents/designs/netorase-system.md | contains | "109,寝取らせ許可レベル" | [x] |
| 3b | netorase-system 旧ID削除 | file | Game/agents/designs/netorase-system.md | not_contains | "53,寝取らせ許可レベル" | [x] |
| 4a | reconciliation-system 新ID追加 | file | Game/agents/designs/reconciliation-system.md | contains | "122,プラトニック好感度" | [x] |
| 4b | reconciliation-system 旧ID削除 | file | Game/agents/designs/reconciliation-system.md | not_contains | "200,プラトニック好感度" | [x] |
| 5 | 統一テーブルに衝突検証記載 | file | Game/agents/reference/cflag-ntr-allocation.md | contains | "衝突検証済み: ID 90-137は既存CFLAG.csv未使用" | [x] |
| 6 | 監査ドキュメント更新 | file | Game/agents/reference/ntr-system-audit.md | contains | "Resolved (Feature 340)" | [x] |

---

## Dependencies

- Feature 339 (DONE): NTR System Documentation SSOT Consolidation

---

## Notes

- 既存のCFLAG.csvは変更しない（提案段階のドキュメントのみ修正）
- 実装時にCFLAG.csvへ追加する
- **ID範囲検証**: CFLAG.csv確認済み - ID 89-199は未使用（88:里子人数→200:服装_衣服着用に飛んでいる）
- **変数名について**: ntr-flavor-stats.mdにはSection 7（簡略版: 17変数）とSection 2.1/2.6（詳細版: 19変数）の2つの命名方式がある。詳細版はチンポ_サイズを長さ/太さ×ランク/mmに分割、精液量を量/濃度に分割して19変数となる。本Featureでは詳細版を採用し、Task 2でSection 7の命名も統一する
- **変数マッピングについて**: 各設計ドキュメントでは変数が複数セクションに分散している。本Featureでは全変数を統合し、連続したID範囲に再割当する

### ID Mapping (Original → New)

> **Note**: 「Orig」列は設計ドキュメントで提案されているID（CFLAG.csv既存ではない）。CFLAG.csvとの衝突を解決するため、新ID範囲に再割当する。

**ntr-flavor-stats.md** (60-78 → 90-108):
| Orig | New | Variable |
|:----:|:---:|----------|
| 60 | 90 | チンポ_長さ_ランク |
| 61 | 91 | チンポ_長さ_mm |
| 62 | 92 | チンポ_太さ_ランク |
| 63 | 93 | チンポ_太さ_mm |
| 64 | 94 | チンポ_硬度 |
| 65 | 95 | チンポ_持続力 |
| 66 | 96 | チンポ_精液量 |
| 67 | 97 | チンポ_精液濃度 |
| 68 | 98 | チンポ_回復力 |
| 69 | 99 | 顔ランク |
| 70 | 100 | フェロモン |
| 71 | 101 | 威圧感 |
| 72 | 102 | 口開発度 |
| 73 | 103 | 乳開発度 |
| 74 | 104 | 膣開発度 |
| 75 | 105 | 肛開発度 |
| 76 | 106 | 子宮開発度 |
| 77 | 107 | 浮気耐性 |
| 78 | 108 | 快楽依存 |

**netorase-system.md** (53-59,80-85,new → 109-121):
| Orig | New | Variable | Source Line |
|:----:|:---:|----------|:-----------:|
| 53 | 109 | 寝取らせ許可レベル | L63 |
| 54 | 110 | 寝取らせ積極度 | L64 |
| 55 | 111 | 寝取らせ対象指定 | L65 |
| 56 | 112 | 風俗勤務先 | L414 |
| 57 | 113 | 風俗勤務中 | L415 |
| 58 | 114 | 風俗客数累計 | L416 |
| 59 | 115 | 風俗収入累計 | L417 |
| 80 | 116 | 現在外出先 | L570 |
| 81 | 117 | 外出同伴者 | L571 |
| new | 118 | 寝取らせ受容度 | (新規) |
| new | 119 | 秘密欲求度 | (新規) |
| 84 | 120 | 風俗堕ち状態 | L617 |
| 85 | 121 | 主人公利用済み | L618 |

**reconciliation-system.md** (200-209,82-83,散在 → 122-137):

> **Note**: 「散在」= ドキュメント内に散在する変数参照から新規ID割当。明示的なID定義がなかった変数。

| Orig | New | Variable | Source Line |
|:----:|:---:|----------|:-----------:|
| 200 | 122 | プラトニック好感度 | L60 |
| 201 | 123 | 肉欲的好感度 | L61 |
| 202 | 124 | 信頼度 | L62 |
| 203 | 125 | NTR経験回数 | L63 |
| 204 | 126 | 復縁回数 | L64 |
| 205 | 127 | 浮気リスク | L65 |
| 206 | 128 | 比較基準_サイズ | L66 |
| 207 | 129 | 比較基準_持続力 | L67 |
| 208 | 130 | 比較基準_顔 | L68 |
| 209 | 131 | 最終NTR相手 | L69 |
| 散在 | 132 | 刻印封印度 | L767,772,786 |
| 散在 | 133 | 誘惑段階 | L423 |
| 散在 | 134 | 訪問者干渉度 | L502,571,627 |
| 82 | 135 | 元恋人フラグ | L615 |
| 83 | 136 | 元人妻フラグ | L616 |
| 散在 | 137 | 成長を感じたフラグ | L1085,1092 |

---

## Links

- [feature-339.md](feature-339.md) - NTR System Documentation SSOT Consolidation (発見元)

---

## Progress Log

| Date | Phase | Notes |
|------|-------|-------|
| 2026-01-04 | Created | Feature 339監査中に衝突発見 |
| 2026-01-04 20:15 | Task 1 | Created cflag-ntr-allocation.md with collision verification |
| 2026-01-04 20:25 | Tasks 2-6 | Updated all design documents with new IDs, audit doc resolved |
