# Content Roadmap

era紅魔館protoNTR Long-term content and gameplay expansion plan.

**Role Division**:
- This file = Long-term vision (what to build)
- [index-features.md](index-features.md) = Execution management (what to do now)
- [designs/](designs/) = Detailed design (how to build)

---

## Milestone Overview

**Design Philosophy Change (Plan B Adoption)**:
- Old: "Same map = NTR progression" (automatic, monotonous)
- New: "Same map = Opportunity (OPP)" -> "Action choice" -> "Parameter change on success" -> "Threshold + Hub event = Phase transition"

**v1.0 Definition**: All 150 COMs x 10 characters kojo complete (C2 quality) = "Playable complete version"

### Version Roadmap

| Version | Content | System | Design | Description | Status |
|:-------:|:-------:|:------:|:------:|-------------|:------:|
| v0.4 | C0 | S0 | - | Foundation complete | Done |
| v0.5 | C1 | S0 | - | 300-series kojo | Done |
| v0.6 | C2 | S0 | - | 0-60系kojo (36 COM) | **Paused** |
| **-** | **-** | **-** | **[full-csharp-architecture.md](designs/full-csharp-architecture.md)** | **C#/Unity Migration (技術基盤変更)** | **WIP** |
| v0.6 | C2 | S0 | - | 0-60系kojo (再開) | - |
| v0.7 | - | S0 | [v0.7-audit-fix.md](designs/v0.7-audit-fix.md) | F253+F269監査結果修正 | - |
| v0.8 | C2 | S0 | [v0.8-kojo-80-90.md](designs/v0.8-kojo-80-90.md) | 80系kojo (COM_80-85, 6 COM) | Done |
| v0.8.1 | C2 | S0 | - | 90系kojo (COM_90-98, 9 COM; COM_99未実装) | **WIP** |
| v0.9 | C2 | S0 | - | 100-200 series kojo | - |
| **v1.0** | **C2 Complete** | **S0** | **-** | **350-600 series + quality check** | - |
| v1.1 | - | S0.5 | [new-commands.md](designs/new-commands.md) | New Commands (+37) **[C#]** | - |
| v1.2 | - | S0.6 | [m-orgasm-system.md](designs/m-orgasm-system.md) | M絶頂/五重絶頂 **[C#]** | - |
| v1.3-1.8 | C3-C10 | S0 | - | Kojo variations/events/NTR depth (Phase 8e-8l) | - |
| v2.x | - | S1 | [ntr-core-overview.md](reference/ntr-core-overview.md) | OPP + Parameter reform | - |
| v3.x | C11 | S2 | [phase-system.md](designs/phase-system.md) | Phase management + Body zones + **Phase 8m: MC interaction (~750 lines)** | - |
| v4.x-5.x | - | S3 | [ntr-core-overview.md](reference/ntr-core-overview.md), [visitor-type-system.md](designs/visitor-type-system.md) | Route branches + Visitor type selection + NTR completion | - |
| v6.x | - | S4 | [netorase-system.md](designs/netorase-system.md) | Netorase + Brothel | - |
| v7.x | - | S5 | [reconciliation-system.md](designs/reconciliation-system.md) | Reconciliation + Relapse risk | - |
| v8.x | - | S6 | [ntr-flavor-stats.md](designs/ntr-flavor-stats.md), [chastity-belt-system.md](designs/chastity-belt-system.md) | MC growth + Chastity belt | - |
| v9.x | - | S7 | [pregnancy-system.md](designs/pregnancy-system.md) | Pregnancy + Childbirth | - |
| v10.x | - | S8 | [incident-war-system.md](designs/incident-war-system.md) | Incident war + Danmaku | - |
| v11.x | - | S9 | - | Portraits + BGM/SE | - |

### Version History

| Version | Completed | Theme | Key Features |
|:-------:|:---------:|-------|--------------|
| v0.4 | 2024-12 | Foundation | F001-050: Headless engine, DI architecture, ERB Linter, kojo-mapper, 立ち絵基盤 |
| v0.5 | 2025-01 | 300系口上 | F051-093: COM統合 K1-K10, Headless test infrastructure, Phase 8c (C1) |
| v0.6 | WIP | 0-60系口上 | F094-: COM_0-72 coverage, Phase 8d (C2, 4-8 lines + emotion description) |

**v0.4 Baseline** (as of 2024-12):
- Engine: Headless mode, State injection, DI architecture
- Tools: ERB Linter, kojo-mapper, Save analyzer
- ERB: Major file splits (NTR.ERB, WC-related, TOILET-related)
- Kojo: Basic structure only (Phase 8a-b)

> Full feature history → [index-features-history.md](index-features-history.md)

---

## Content Track (Kojo)

Kojo quality levels. Can progress independently of System Track.

| Level | Phase | Lines/Branch | Patterns | Target |
|:-----:|:-----:|:------------:|:--------:|--------|
| C0 | 8a-8b | 1-2 lines | 1 | Technical foundation |
| C1 | 8c | 4 lines | 1 | Existing COMs |
| C2 | 8d | 4-8 lines | 1 | Existing COMs (emotion/scene description) |
| C3 | 8e | 4-8 lines | 4 | Existing COMs + variations |
| C4 | 8f | 15-30 lines | Special | First experience |
| C5 | 8g | 1-30 lines | Various | Event kojo (eraTW-style) |
| C6 | 8h | 4-8 lines | 9 levels | NTR kojo depth (FAV_* branches) |
| C7 | 8i | 4-8 lines | Location | Location/Situation kojo |
| C8 | 8j | 4-8 lines | NOWEX×TCVAR | Ejaculation-state kojo (射精状態分岐) |
| C9 | 8k | 4-8 lines | Situation | Special situation kojo (WC/SexHara/Bath) |
| C10 | 8l | 8-15 lines | FIRSTTIME | First execution guard (初回実行, ALL 88 COMs including 日常 300系/400系) |
| C11 | 8m | 3-10 lines | Phase (7) × 絶頂経験 (3) × 4 | MC interaction + 絶頂経験分岐 (~1030 lines) |
| C12 | 8n | 5-10 lines | 受容度/秘密/煽り | Netorase専用口上 (~2400 lines) |

**C3/Phase 8e Preparation**: [Feature 189](feature-189.md) - Systematic review of ABL/TALENT branching
- Define kojo branching patterns by 精液中毒 (ABL:31), 感度, 感覚, 経験値
- Template (KOJO_KX.ERB) guide added
- ABL:31 精液中毒 branch target: COM_81 (フェラチオ), 80-85系 (射精系)

**ABL/EXP Branch Infrastructure** (F216-F217):
- [Feature 216](feature-216.md): Branch functions implemented in COMMON_KOJO.ERB
  - `GET_ABL_BRANCH(TARGET, ABL#)` → 0:低(<1) / 1:中(1-2) / 2:高(≥3)
  - `GET_TALENT_BRANCH(TARGET, TALENT#)` → 0:通常 / 1:敏感
  - `GET_EXP_BRANCH(TARGET, EXP#)` → 0:未経験(<10) / 1:経験少(10-99) / 2:経験豊富(≥100)
- [Feature 217](feature-217.md): Branch calls integrated into kojo files
  - COM_6 (胸愛撫): ABL:3 (Ｂ感覚) - K1-K10 全10キャラ
  - COM_3 (指挿れ): ABL:1 (Ｖ感覚) + EXP:1 (Ｖ経験) - K1のみ
  - COM_5 (アナル愛撫): ABL:2 (Ａ感覚) - K1のみ
- **Next**: ABL/EXP分岐口上の作成（Phase 8e で対応）

**口上分岐構造** (F217実装済み → 口上テキスト未作成):

現在: TALENT 4分岐のみ (恋人/恋慕/思慕/なし × 4パターン = 16 DATALIST)
```
IF TALENT:恋人
  DATALIST × 4
ELSEIF TALENT:恋慕
  DATALIST × 4
ELSEIF TALENT:思慕
  DATALIST × 4
ELSE
  DATALIST × 4
ENDIF
```

目標: TALENT × ABL/EXP ネスト分岐 (モジュール式ではない)
```
LOCAL = GET_ABL_BRANCH(MASTER, 3)  ; ← F217で追加済み
IF TALENT:恋人
  IF LOCAL == 2      ; 高感度
    DATALIST × 4     ; ← 要作成
  ELSEIF LOCAL == 1  ; 中感度
    DATALIST × 4     ; ← 要作成
  ELSE               ; 低感度
    DATALIST × 4     ; ← 既存を流用可
  ENDIF
ELSEIF TALENT:恋慕
  ; 同様に 3分岐 × 4パターン
...
```

**作成ボリューム**:

| COM | 分岐 | 対象 | 現在 | 追加 | 合計 |
|-----|------|:----:|:----:|:----:|:----:|
| COM_6 | ABL:3 | K1-K10 | 16 | +32 | 48 DATALIST/キャラ |
| COM_3 | ABL:1×EXP:1 | K1 | 16 | +128 | 144 DATALIST |
| COM_5 | ABL:2 | K1 | 16 | +32 | 48 DATALIST |

計算: TALENT 4 × ABL 3 × パターン 4 = 48 (COM_6/COM_5)
COM_3: TALENT 4 × ABL 3 × EXP 3 × パターン 4 = 144

**優先順**: COM_6 K1 → COM_6 K2-K10 → COM_3 K1 → COM_5 K1

Phase 8 details -> [reference/kojo-phases.md](reference/kojo-phases.md)

---

## System Track (Features)

Game system features. Has dependencies.

| Level | Name | Content | Version |
|:-----:|------|---------|:-------:|
| S0 | proto | Existing NTR, training mode | v0.4-v1.6 |
| S0.5 | commands | New commands (+37) + TCVAR:116 initiative system | v1.7 |
| S0.6 | m-orgasm | M絶頂/五重絶頂 system (快Ｍ + 無自覚絶頂) | v1.8 |
| S1 | params | OPP + Parameter subdivision + MC emotion separation | v2.x |
| S2 | phase | Phase management + Body contact zones + Hub events | v3.x |
| S3 | route | Route branches (R1-R6) + **Visitor type selection** + Phase 5-7 + Exposure complete | v4.x-v5.x |
| S4 | netorase | Netorase + Brothel + External map | v6.x |
| S5 | rebuild | Reconciliation + Relapse risk + Visitor reaction | v7.x |
| S6 | growth | MC growth + Mark improvement + Chastity belt | v8.x |
| S7 | family | Pregnancy + Childbirth + Cuckoo | v9.x |
| S8 | war | Incident war + Factions + Danmaku battle | v10.x |
| S9 | media | Portraits + BGM + SE | v11.x |

NTR system details -> [reference/ntr-core-overview.md](reference/ntr-core-overview.md)

### Dependency Graph

```
v0.6 (Paused)
  |
  v
┌─────────────────────────────────────────┐
│  C#/Unity Migration (技術基盤変更)       │
│  ┌─────────────────────────────────┐    │
│  │ Phase 1-11:  Core Engine + DDD  │    │
│  │ Phase 12-15: Data/Kojo Migration│    │
│  │ Phase 16-28: Systems + Unity UI │    │
│  └─────────────────────────────────┘    │
│  → uEmuera/ERB eliminated               │
│  → DDD/SOLID/Event-Driven Architecture  │
│  Design: full-csharp-architecture.md    │
└─────────────────────────────────────────┘
  |
  v
v0.6 (再開) → v0.7 → v0.8 → v0.9
  |
  v
v1.0 (C2 Complete) --- 全COM kojo完了
  |
  v
v1.1 (New Commands +37) --- S0.5 [C#]
  |
  v
v1.2 (M絶頂/五重絶頂) --- S0.6 [C#]
  |
  v
v1.3-1.8 (Kojo C3-C10) --- Phase 8e-8l
  |
  v
v2.x (Parameter reform) --- S1
  |
  v
v3.x (Phase management) --- S2
  |
  v
v4.x-v5.x (Route + NTR complete)
  |
  +------------------+
  v                  v
v6.x (Netorase)    v7.x (Reconciliation)
  |                  |
  +--------+---------+
           v
v8.x (MC growth)
  |
  v
v9.x (Pregnancy)
  |
  v
v10.x (Incident war)
  |
  v
v11.x (Media)
```

### Architecture Migration Note

**C#/Unity Migration (技術基盤変更)**

Executed during v0.6 to establish C#/Unity foundation before content completion.

| Before (v0.x) | After (Migration) |
|:-------------:|:-----------------:|
| ERB + uEmuera | Pure C# + Unity |
| TALENT branching in ERB | YAML declarative conditions |
| uEmuera PRINT/INPUT | Unity TextMeshPro/UI |
| CFLAG/FLAG variables | C# typed GameState |

Design: [full-csharp-architecture.md](designs/full-csharp-architecture.md)

**Parallel Development**:
- v6.x and v7.x can be developed in parallel after v5.x
- Content Track can be developed ahead of System Track

---

## Current: C#/Unity Migration (v0.6 Paused)

### v0.4 Implemented Features

| Category | Content |
|----------|---------|
| **NTR System** | Visitor NTR, Peeping, Witnessing, Takeout |
| **Training Mode** | COMF 200+ commands |
| **Characters** | ~13 SDM residents |
| **Ability System** | Sensation, Intimacy, Obedience, Skills |
| **Kojo** | Phase 8a-c complete (C1 quality) |

### Phase 8d Goals

**Theme**: All COM coverage + Quality improvement

**Content**:
- **All COM coverage**: All series (0-600) COMs created in order
- **Quality improvement**: 8c completed (300, 301, 302) also remade to 8d quality
- Lines: 4-8 lines
- Emotion description: Psychology/emotion deepening
- Scene description: Narration for situation/atmosphere
- Structure: TALENT 4 branches x 1 pattern

**COM Series Order**: 300s -> 0s -> 20s -> 40s -> 60s (v0.6完了) -> **監査** -> 80s (v0.8完了) -> 90s (WIP) -> ... -> 600s -> **最終監査**

**v0.6完了条件**: 0系(12) + 20系(2) + 40系(9) + 60系(13) = 36 COM × 10キャラ = 360 DATALIST完了
**v0.8完了条件**: 80系(6) COM × 10キャラ = 60 DATALIST完了
**v0.8.1進行中**: 90系(9) COM × 10キャラ = 90 DATALIST (COM_99未実装)

**Progress tracking**: See [kojo-mapper](../../tools/kojo-mapper/) for coverage analysis

### Kojo Semantic Audit (F253)

品質確保のため、口上監査を以下のタイミングで実施:

| タイミング | 目的 | 対応 |
|------------|------|------|
| **v0.6完了後 (v0.7)** | 既存口上の問題発見・修正 | [v0.7-audit-fix.md](designs/v0.7-audit-fix.md) |
| **v1.0完了後** | 全COM口上の最終品質確認 | 再監査実施 |

**監査方法**: F253 com-auditor agent による全COM×全キャラの意味的整合性チェック

**監査結果**: [_out/logs/audit/summary.json](../logs/audit/summary.json)

---

## Kojo Reference

| Document | Content |
|----------|---------|
| [kojo-phases.md](reference/kojo-phases.md) | Phase 8g-8k detailed specs |
| [kojo-reference.md](reference/kojo-reference.md) | Technical reference |

---

## Future Ideas (Phase 8g+)

### Morning Naked Room Entry Kojo

Kojo for when MASTER enters a character's room naked in the morning.
Reaction and behavior branches by relationship (思慕/恋慕/恋人/人妻) and 羞恥心 trait.

**Branching Matrix**:

| Relationship | 羞恥心 | Kick Out | Kojo Type |
|--------------|:------:|:--------:|-----------|
| 思慕 or less | any | ✅ | Rejection/confusion |
| 恋慕 | 恥じらい | ✅ | Embarrassed rejection |
| 恋慕 | Normal | ❌ | Embarrassed but tolerant |
| 恋慕 | 恥薄い | ❌ | Light comment |
| 恋人 | 恥じらい | ❌ | Embarrassed |
| 恋人 | Normal | ❌ | Light scolding |
| 恋人 | 恥薄い | ❌ | Normal greeting |
| 人妻 | any | ❌ | Same as 恋人 |

**Related Systems**:
- Room entry event: `@KOJO_EVENT_K{N}_0`
- Naked check: `CFLAG:服装パターン == 0`
- 羞恥心: `TALENT:33` (-1=恥薄い, 1=恥じらい)
- Kick out mechanism: `TURN_OUT_WHEN_SLEEP` (MOVEMENT.ERB)

**Available Parameters**:
| TALENT# | Name | Value | Effect |
|---------|------|-------|--------|
| 23 | 性的興味 | -1=保守的 / 1=好奇心 | 保守的 resists sexual acts |
| 30 | 貞操 | -1=無頓着 / 1=貞操観念 | 貞操観念 rejects sexual contact |
| 33 | 羞恥心 | -1=恥薄い / 1=恥じらい | Reaction to exposure |
| 61 | 汚臭耐性 | -2=潔癖症 | Has 潔癖症 concept |

---

## Future: Dashboard Extensions (F606 OUT OF SCOPE)

F606 Real-time Dashboard Visualization のスコープ外機能（将来検討）:

| Phase | Extension | Description | Timing |
|:-----:|-----------|-------------|--------|
| 1 | Alert System | エラー閾値とアラート通知 | 基本ダッシュボード完成後 |
| 1 | Export Features | PDF/CSVレポート出力 | 基本ダッシュボード完成後 |
| 2 | Security | ダッシュボードアクセス認証 | 本格運用時 |
| 2 | Cross-platform Monitoring | Linux/macOS PerformanceCounter代替 | クロスプラットフォーム展開時 |
| 2 | Multi-Instance Dashboard | 分散環境での複数インスタンス監視 | 分散展開時 |
| 3 | External Tool Integration | Grafana等の外部監視ツール連携 | 既存インフラ統合時 |
| 3 | ML Analytics | 機械学習によるエラーパターン分析 | データ蓄積後 |

※ Feature IDは実装時に`/next`で割り当て

---

## Links

- [index-features.md](index-features.md) - Execution management (current work)
- [designs/](designs/) - Design proposals
- [reference/](reference/) - Technical references
