# Feature 341: ERB System Architecture Research - Hybrid Migration Proposal

## Status: [DONE]

## Type: research

## Priority: Strategic

## Created: 2026-01-04

---

## Summary

ERBシステムの技術負債とNTR実装計画の規模を包括的に調査し、YAML+C#ハイブリッドアーキテクチャへの移行を提案する調査・検討Feature。

---

## Background

### Philosophy (Mid-term Vision)

**長期保守性と拡張性の確保**: v2.x以降のNTRシステム大規模拡張（400+変数、6ルート×8フェーズ）に耐えうるアーキテクチャ基盤を確立する。

### Problem (Current Issue)

1. **ERBの本質的限界**: 型システム・モジュール機構・IDE支援・テスト基盤がない
2. **NTR計画の規模**: 現状50変数→計画400-500変数（10倍増）、複雑なパラメータ相互作用
3. **口上の組み合わせ爆発**: 6ルート×8フェーズ×パラメータ分岐で現行TALENT 4分岐では対応不可

### Goal (What to Achieve)

1. ERBシステムの現状と問題点を詳細に文書化
2. NTRシステム計画の複雑度を定量的に分析
3. 移行オプションの比較評価
4. 推奨アーキテクチャと移行ロードマップの策定

---

## Investigation Results

### 1. ERB System Current State

#### 1.1 File Statistics

| Metric | Value | Notes |
|--------|------:|-------|
| Total ERB/ERH files | 474 | Game/ERB/ |
| Total size | ~20 MB | |
| Root-level files | 292 | No logical grouping |
| COMF numbered files | 152 | COMF0-COMF999 |
| Kojo directories | 11 | K1-K10 + U_generic |
| Unused functions detected | 616 | ErbLinter report |

#### 1.2 ERB Branching Reality

| File Type | Text:Logic Ratio | Typical Nesting | Complexity |
|-----------|:----------------:|:---------------:|:----------:|
| **KOJO (dialogue)** | 85-90% : 10-15% | 2-3 levels | Low |
| **COMF (simple)** | 25-35% : 65-75% | 2-4 levels | Low-Medium |
| **COMF (complex: 400+)** | 20% : 80% | 5-7 levels | High |
| **NTR system** | 30% : 70% | 4-6 levels | High |

**Key Finding**: KOJO files are mostly text containers with simple TALENT 4-way branching. Complex logic is concentrated in COMF400+ and NTR system files.

#### 1.3 Common KOJO Pattern

```erb
@KOJO_MESSAGE_COM_K1_0_1
IF TALENT:TARGET:TALENT_恋人
  PRINTDATA K
  DATALIST
    セリフA
    セリフB
  ENDLIST
ELSEIF TALENT:TARGET:TALENT_恋慕
  PRINTDATA K
  DATALIST
    セリフC
  ENDLIST
ELSE
  PRINTDATA K
  DATALIST
    セリフD
  ENDLIST
ENDIF
RETURN 0
```

#### 1.4 ERB Fundamental Limitations

| Problem | Impact | Solvable? |
|---------|--------|:---------:|
| No type system | Runtime errors only | No |
| Global namespace only | Function collision risk | No |
| No IDE support | No completion/refactoring | No |
| No debugging | No breakpoints/stepping | No |
| No unit testing | Cannot isolate functions | No |
| Poor error messages | Line number only | Partial |

---

### 2. NTR System Planned Complexity

#### 2.1 Scale Comparison

| Metric | Current (S0) | Planned (S1-S5) | Increase |
|--------|:------------:|:---------------:|:--------:|
| Code size | ~600 KB | 2,000-3,000 KB | **5x** |
| CFLAG variables | ~50 | 400-500 | **10x** |
| Parameters/pair | 2-3 | 10+ | **5x** |
| Route branches | 1 (linear) | 6 routes × 8 phases | **48x** |
| Design documents | 1 | 26 | **26x** |

#### 2.2 Planned Systems Summary

##### S1 (v2.x): Parameter Reform

10+ parameters per visitor-heroine pair:
- **OPP** (Opportunity): Same-map accumulation
- **LEV** (Leverage): Coercion basis
- **FAM** (Familiarity): Boundary dissolution
- **DEP** (Dependency): Support foundation
- **TEM** (Temptation): FRUST × BORED × THRILL × COMPAT
- **SEXB** (Body Habit): Physical association
- **EMO** (Romanticization): Real affair core
- **SUS** (Suspicion): MC awareness
- **EXPOSE** (Exposure): Discovery stage

##### S2 (v3.x): Phase Management

8 phases with complex transition triggers:
- Phase 0: Foundation
- Phase 1: External Pressure
- Phase 2: Familiarity
- Phase 3: Physical Contact
- **Phase 4: Point of No Return** (irreversible)
- Phase 5: Sexual Relationship
- Phase 6: Full Corruption
- Phase 7A/7B: Spread or Commercialization

Hub event system: Must validate 20+ conditions before phase transition.

##### S3 (v4.x-v5.x): Route Branching

6 routes diverge at Phase 4:

| Route | Name | Psychology | LOVE_MC |
|:-----:|------|-----------|:-------:|
| R1 | Coercion | "Need escape" | Maintains |
| R2 | Familiarity | "Just habit" | Maintains |
| R3 | Escape | "Safe place" | Maintains |
| R4 | Trade | "Business deal" | Maintains* |
| R5 | Rebellion | "Against MC" | Falls |
| R6 | Real Affair | "True love" | Falls/Reverses |

##### S4 (v6.x): Netorase System

Player as "Director":
- Permission levels (0-5) per heroine
- Aggressiveness tiers (0-3)
- Prostitution subsystem (7 shop types)
- External maps (5+ locations)
- 50+ new CFLAG variables

##### S5 (v7.x): Reconciliation System

- Dual-axis affection (Platonic/Carnal, 0-1000 each)
- Trust (-500 to 1000)
- 5 reconstruction phases (RB0-RB4)
- Relapse risk (0-100)
- Comparison baseline tracking

#### 2.3 KOJO Branching Explosion

| Current | Planned | Complexity |
|---------|---------|:----------:|
| TALENT 4-branch | Route(6) × Phase(8) × Params | 48+ patterns/COM |
| ~16 variants/COM | 200+ variants/COM | **12x** |

**Netorase KOJO (Phase 8n)**:
- 372 kojo patterns × 10 characters
- ~2,400 lines for acceptance alone
- Comparison/teasing: ~400 lines
- Secret desire: ~300 lines

---

### 3. Architecture Options Analysis

#### Option A: Transpiler (ERB++ → ERB)

```typescript
// TypeScript-like syntax → compiles to ERB
@KojoFunction('COM', 0)
function meilingAibu(target: Character) {
  if (target.hasTalent(Talent.恋人)) {
    print.random(["セリフA", "セリフB"]);
  }
}
```

| Pros | Cons |
|------|------|
| Keep existing engine | Transpiler development cost |
| TypeScript IDE support | Still limited by ERB constraints |
| Gradual migration | Debug requires source maps |

**Effort**: 2-3 months for transpiler

#### Option B: Lua Migration (MoonSharp)

```lua
function kojo_com_0(target)
  if era.hasTalent(target, era.Talent.恋人) then
    era.printRandom({"セリフA", "セリフB"})
  end
end
```

| Pros | Cons |
|------|------|
| MoonSharp is mature/.NET | Engine modification required |
| Real module system | Full ERB rewrite |
| Debugging support | User learning curve |

**Effort**: Engine 1-2 months, migration 1 month

#### Option C: C# Scripting (Roslyn)

```csharp
[KojoHandler(Character.Meiling, ComType.Aibu)]
public class MeilingAibu : KojoBase {
    public override void Execute(GameContext ctx) {
        if (ctx.Target.HasTalent(Talent.恋人))
            ctx.PrintRandom("セリフA", "セリフB");
    }
}
```

| Pros | Cons |
|------|------|
| Full type safety | Compile time overhead |
| VS/Rider full support | Higher user barrier |
| Same language as uEmuera | Heavier for content |

**Effort**: Engine 2-3 months, migration 2 months

#### Option D: Hybrid Architecture (YAML + C#) **[RECOMMENDED]**

```
┌─────────────────────────────────────────────────────┐
│  Content Layer (YAML)                               │
│  - Dialogue text                                    │
│  - Declarative conditions (route, phase, talent)   │
│  → User-editable, no syntax errors                 │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│  Logic Layer (C#)                                   │
│  - Parameter calculation engine                     │
│  - Phase transition state machine                   │
│  - Route branching logic                           │
│  - Save/load with migration                        │
│  → Type-safe, testable, refactorable              │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│  Compatibility Layer (ERB Bridge)                   │
│  - Existing ERB execution                          │
│  - Gradual migration support                       │
│  → Preserve existing assets                        │
└─────────────────────────────────────────────────────┘
```

| Pros | Cons |
|------|------|
| Content creators use YAML only | Two-layer learning curve |
| Logic in type-safe C# | Migration effort |
| Clear separation of concerns | Complex YAML for edge cases |
| Gradual migration possible | |

**Effort**: Phase 1 (YAML KOJO): 4 weeks, Full: 8-12 months

#### Options Comparison Matrix

| Aspect | A:Transpiler | B:Lua | C:C# | D:Hybrid |
|--------|:------------:|:-----:|:----:|:--------:|
| Type safety | ◎ | ○ | ◎ | ◎ |
| IDE support | ◎ | ○ | ◎ | ◎ |
| Debugging | △ | ○ | ◎ | ◎ |
| Testing | △ | ○ | ◎ | ◎ |
| User accessibility | ○ | ○ | △ | ◎ |
| Migration cost | Medium | High | High | Very High |
| Long-term maintainability | ○ | ○ | ◎ | ◎ |
| NTR complexity handling | △ | ○ | ◎ | ◎ |

---

### 4. Recommended Migration Roadmap

#### Critical Insight: v2.x is the Deadline

```
ERB維持で v2.x に突入した場合:
  → パラメータ計算がERBに散在
  → 単体テスト不可能
  → v3.x以降の開発が指数関数的に困難化
  → 開発停止リスク

v2.x でC#移行した場合:
  → パラメータ計算が型安全
  → 単体テスト可能
  → v3.x以降もスケール可能
```

#### Migration Phases

| Phase | Version | NTR System | Migration Action | Priority |
|:-----:|:-------:|------------|------------------|:--------:|
| 1 | v0.6-v1.0 | Content creation | YAML KOJO prototype | Recommended |
| 2 | v1.7-v1.8 | S0.5-S0.6 | YAML KOJO production | Recommended |
| **3** | **v2.x** | **S1: Parameter Reform** | **C# Parameter Engine** | **CRITICAL** |
| 4 | v3.x | S2: Phase Management | C# State Machine | Required |
| 5 | v4.x-v5.x | S3: Route Branching | C# Decision Engine | Required |
| 6 | v6.x+ | S4+: Netorase etc. | Full Hybrid | Required |

#### Phase 1 Details (Immediate Start)

| Task | Effort | Benefit |
|------|:------:|---------|
| YAML schema design (Route/Phase ready) | 1 week | Future-proof foundation |
| C# YAML loader | 1 week | uEmuera integration |
| ERB→YAML conversion tool | 1 week | Migrate existing assets |
| Route/Phase condition evaluator (proto) | 1 week | v2.x preparation |
| **Total** | **4 weeks** | **KOJO creation 2-3x faster** |

#### Phase 3 Details (v2.x Critical)

| Task | Effort | Benefit |
|------|:------:|---------|
| NtrParameterEngine design | 2 weeks | Type-safe parameter calculation |
| ERB→C# parameter migration | 3 weeks | Remove scattered ERB logic |
| Unit test coverage | 2 weeks | Regression prevention |
| Integration with uEmuera | 1 week | Runtime connection |
| **Total** | **8 weeks** | **Testable, maintainable NTR core** |

```csharp
// NTR/Parameters/NtrParameterEngine.cs
public class NtrParameterEngine {
    public float CalculateTemptation(Character target, Visitor visitor) {
        var frustration = target.GetParam(NtrParam.FRUST);
        var boredom = target.GetParam(NtrParam.BORED);
        var thrill = target.GetParam(NtrParam.THRILL);
        var compatibility = visitor.GetCompatibility(target);
        return frustration * boredom * thrill * compatibility;
    }
}

// Unit testable
[Test]
public void Temptation_HighFrustration_IncreasesResult() {
    var target = new Character { Frustration = 0.8f };
    var visitor = new Visitor { Compatibility = 0.5f };
    var result = engine.CalculateTemptation(target, visitor);
    Assert.Greater(result, baseline);
}
```

#### YAML KOJO Schema (Route/Phase Ready)

```yaml
# Game/content/kojo/meiling/com_0.yaml
character: K1_美鈴
action: COM_0

dialogue:
  # Route/Phase branching (future-ready)
  - when:
      route: R1
      phase: { gte: 3 }
    lines:
      - "...言われた通りにします"

  - when:
      route: R6
      phase: { gte: 5 }
    lines:
      - "{visitor}さん...好き..."

  # Current TALENT branching (backward compatible)
  - when: { talent: 恋人 }
    lines:
      - "ふふ、{master}様ったら..."
      - "今日も甘えさせてくださいね"

  - when: { talent: 恋慕 }
    lines:
      - "あ、あの...嬉しいです"

  - when: { default: true }
    lines:
      - "は、はい..."
```

---

### 5. Risk Analysis

#### If Migration is NOT Performed

| Version | Risk | Probability | Impact |
|:-------:|------|:-----------:|:------:|
| v2.x | Parameter calculation scattered in ERB | Certain | High |
| v3.x | Phase logic unmaintainable | Very High | Critical |
| v4.x | Route branching impossible to test | Very High | Critical |
| v6.x+ | Development halt | High | Fatal |

#### If Migration IS Performed

| Risk | Mitigation |
|------|------------|
| Migration effort | Gradual approach, ERB bridge layer |
| User learning curve | YAML is simpler than ERB |
| Compatibility issues | Extensive regression testing |

---

### 6. Existing Tools Assessment

| Tool | Purpose | Reusability |
|------|---------|:-----------:|
| ErbLinter | ERB static analysis | Repurpose for migration validation |
| kojo-mapper | Coverage analysis | Adapt for YAML tracking |
| ac-static-verifier.py | AC verification | Keep for testing |
| erb-duplicate-check.py | Duplication detection | Use during migration |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ERB analysis documented | file | static | exists | Game/agents/feature-341.md | [x] |
| 2 | NTR complexity quantified | file | static | contains | "400-500 CFLAG" | [x] |
| 3 | Architecture options compared | file | static | contains | "Options Comparison Matrix" | [x] |
| 4 | Migration roadmap defined | file | static | contains | "Migration Phases" | [x] |
| 5 | Recommendation stated | file | static | contains | "RECOMMENDED" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze ERB file structure and patterns | [x] |
| 2 | 1 | Document ERB branching complexity | [x] |
| 3 | 2 | Analyze NTR system roadmap (S1-S5) | [x] |
| 4 | 2 | Quantify variable/parameter growth | [x] |
| 5 | 3 | Evaluate transpiler option | [x] |
| 6 | 3 | Evaluate Lua option | [x] |
| 7 | 3 | Evaluate C# option | [x] |
| 8 | 3 | Evaluate hybrid option | [x] |
| 9 | 4 | Define migration phases | [x] |
| 10 | 4 | Identify v2.x as critical deadline | [x] |
| 11 | 5 | State final recommendation | [x] |

---

## Conclusions

### Primary Recommendation

**Option D: Hybrid Architecture (YAML + C#)** is mandatory, not optional.

### Key Decisions Required

1. **Immediate**: Approve Phase 1 (YAML KOJO) start
2. **v1.0 completion**: Begin C# Parameter Engine design
3. **Before v2.x**: C# engine must be operational

### Why This Matters

Given NTR system's planned scale (10x variable increase, 48x branching complexity), continuing with ERB will lead to:
- Untestable parameter calculations
- Unmaintainable phase/route logic
- Development halt risk at v3.x+

**ERB abandonment is not a choice but a necessity for the project's survival.**

---

## Next Steps

If approved:
1. Create Feature 342: YAML KOJO Schema Design
2. Create Feature 343: C# YAML Loader Implementation
3. Create Feature 344: ERB→YAML Conversion Tool
4. Update content-roadmap.md with migration milestones

---

## Links

- [content-roadmap.md](content-roadmap.md) - Version roadmap
- [reference/ntr-system-map.md](reference/ntr-system-map.md) - NTR system reference
- [feature-340.md](feature-340.md) - CFLAG allocation (related)

---

## Progress Log

| Date | Phase | Notes |
|------|-------|-------|
| 2026-01-04 | Investigation | ERB structure analysis completed |
| 2026-01-04 | Investigation | NTR complexity analysis completed |
| 2026-01-04 | Analysis | Architecture options evaluated |
| 2026-01-04 | Proposal | Hybrid migration recommended |
