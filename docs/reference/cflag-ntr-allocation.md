# CFLAG NTR System Allocation Table

Unified CFLAG ID allocation for NTR system design documents.

**衝突検証済み: ID 90-137は既存CFLAG.csv未使用**

Verified against CFLAG.csv (ID 88:里子人数 → 200:服装_衣服着用). IDs 89-199 are available.

---

## Allocation Overview

| Range | System | Document | Variable Count |
|:-----:|--------|----------|:--------------:|
| 90-108 | NTR Flavor Stats | designs/ntr-flavor-stats.md | 19 |
| 109-121 | Netorase System | designs/netorase-system.md | 13 |
| 122-137 | Reconciliation System | designs/reconciliation-system.md | 16 |

**Total Allocated**: 48 CFLAGs (90-137)

---

## NTR Flavor Stats (90-108)

Physical and psychological attributes for NTR flavor generation.

| ID | Variable | Description | Source |
|:--:|----------|-------------|:------:|
| 90 | チンポ_長さ_ランク | Penis length rank (S/A/B/C/D) | ntr-flavor-stats.md |
| 91 | チンポ_長さ_mm | Penis length in mm | ntr-flavor-stats.md |
| 92 | チンポ_太さ_ランク | Penis girth rank (S/A/B/C/D) | ntr-flavor-stats.md |
| 93 | チンポ_太さ_mm | Penis girth in mm | ntr-flavor-stats.md |
| 94 | チンポ_硬度 | Penis hardness (0-100) | ntr-flavor-stats.md |
| 95 | チンポ_持続力 | Sexual stamina (0-100) | ntr-flavor-stats.md |
| 96 | チンポ_精液量 | Semen volume (0-100) | ntr-flavor-stats.md |
| 97 | チンポ_精液濃度 | Semen density (0-100) | ntr-flavor-stats.md |
| 98 | チンポ_回復力 | Recovery speed (0-100) | ntr-flavor-stats.md |
| 99 | 顔ランク | Face attractiveness rank (S/A/B/C/D) | ntr-flavor-stats.md |
| 100 | フェロモン | Pheromone intensity (0-100) | ntr-flavor-stats.md |
| 101 | 威圧感 | Intimidation factor (0-100) | ntr-flavor-stats.md |
| 102 | 口開発度 | Oral development level (0-100) | ntr-flavor-stats.md |
| 103 | 乳開発度 | Breast development level (0-100) | ntr-flavor-stats.md |
| 104 | 膣開発度 | Vaginal development level (0-100) | ntr-flavor-stats.md |
| 105 | 肛開発度 | Anal development level (0-100) | ntr-flavor-stats.md |
| 106 | 子宮開発度 | Uterine development level (0-100) | ntr-flavor-stats.md |
| 107 | 浮気耐性 | Resistance to cheating (0-100) | ntr-flavor-stats.md |
| 108 | 快楽依存 | Pleasure dependency (0-100) | ntr-flavor-stats.md |

---

## Netorase System (109-121)

Consensual sharing system state variables.

| ID | Variable | Description | Source |
|:--:|----------|-------------|:------:|
| 109 | 寝取らせ許可レベル | Permission level (0-5) | netorase-system.md:L63 |
| 110 | 寝取らせ積極度 | Player proactivity (0-100) | netorase-system.md:L64 |
| 111 | 寝取らせ対象指定 | Designated partner (character ID) | netorase-system.md:L65 |
| 112 | 風俗勤務先 | Prostitution workplace (0-3) | netorase-system.md:L414 |
| 113 | 風俗勤務中 | Currently working (bool) | netorase-system.md:L415 |
| 114 | 風俗客数累計 | Total customer count | netorase-system.md:L416 |
| 115 | 風俗収入累計 | Total income earned | netorase-system.md:L417 |
| 116 | 現在外出先 | Current outing location (0-4) | netorase-system.md:L570 |
| 117 | 外出同伴者 | Outing companion (character ID) | netorase-system.md:L571 |
| 118 | 寝取らせ受容度 | Acceptance level (0-100) | netorase-system.md |
| 119 | 秘密欲求度 | Desire for secrecy (0-100) | netorase-system.md |
| 120 | 風俗堕ち状態 | Prostitution corruption state (0-3) | netorase-system.md:L617 |
| 121 | 主人公利用済み | Player has used service (bool) | netorase-system.md:L618 |

---

## Reconciliation System (122-137)

Relationship recovery and growth system state variables.

| ID | Variable | Description | Source |
|:--:|----------|-------------|:------:|
| 122 | プラトニック好感度 | Platonic affection (0-1000) | reconciliation-system.md:L60 |
| 123 | 肉欲的好感度 | Carnal affection (0-1000) | reconciliation-system.md:L61 |
| 124 | 信頼度 | Trust level (0-1000) | reconciliation-system.md:L62 |
| 125 | NTR経験回数 | NTR event count | reconciliation-system.md:L63 |
| 126 | 復縁回数 | Reconciliation count | reconciliation-system.md:L64 |
| 127 | 浮気リスク | Current cheating risk (0-100) | reconciliation-system.md:L65 |
| 128 | 比較基準_サイズ | Size comparison baseline (mm) | reconciliation-system.md:L66 |
| 129 | 比較基準_持続力 | Stamina comparison baseline (0-100) | reconciliation-system.md:L67 |
| 130 | 比較基準_顔 | Face comparison baseline (rank) | reconciliation-system.md:L68 |
| 131 | 最終NTR相手 | Last NTR partner (character ID) | reconciliation-system.md:L69 |
| 132 | 刻印封印度 | Imprint seal level (0-100) | reconciliation-system.md:L767,772,786 |
| 133 | 誘惑段階 | Seduction phase (0-5) | reconciliation-system.md:L423 |
| 134 | 訪問者干渉度 | Visitor interference level (0-100) | reconciliation-system.md:L502,571,627 |
| 135 | 元恋人フラグ | Ex-lover flag (bool) | reconciliation-system.md:L615 |
| 136 | 元人妻フラグ | Ex-wife flag (bool) | reconciliation-system.md:L616 |
| 137 | 成長を感じたフラグ | Growth recognized flag (bool) | reconciliation-system.md:L1085,1092 |

---

## ID Migration History

This allocation resolves conflicts identified in Feature 339 audit.

### ntr-flavor-stats.md: 60-78 → 90-108

**Conflicts Resolved**:
- 60-63: Previously conflicted with 刻印主人 (existing CFLAG)
- 70-78: Previously conflicted with 妊娠 variables (existing CFLAG)

### netorase-system.md: 53-59,80-85 → 109-121

**Conflicts Resolved**:
- 80-81: Previously conflicted with 誕生日/ピル使用 (existing CFLAG)

**Note**: Original 53-59 range was available but moved for better grouping.

### reconciliation-system.md: 200-209,82-83,散在 → 122-137

**Conflicts Resolved**:
- 200-209: Previously conflicted with 服装記憶 variables (existing CFLAG)

**Note**: Variables previously scattered across multiple sections now consolidated into contiguous range.

---

## Implementation Status

| Document | Status | Feature Link |
|----------|:------:|:------------:|
| ntr-flavor-stats.md | Updated | [Feature 340](../feature-340.md) |
| netorase-system.md | Updated | [Feature 340](../feature-340.md) |
| reconciliation-system.md | Updated | [Feature 340](../feature-340.md) |
| CFLAG.csv | Not yet implemented | Future |

**Note**: Actual CFLAG.csv implementation will occur when these systems are implemented in ERB code. This allocation table serves as design reference to prevent future ID conflicts.

---

## Related Documents

- [ntr-system-audit.md](ntr-system-audit.md) - Issue 7 resolved by this allocation
- [ntr-flavor-stats.md](../designs/ntr-flavor-stats.md)
- [netorase-system.md](../designs/netorase-system.md)
- [reconciliation-system.md](../designs/reconciliation-system.md)
- [Feature 340](../feature-340.md) - CFLAG ID Allocation Table creation
