# Feature 772: Category 4 Dynamic LOCAL Conversion (EVENT Compound + RAND)

## Status: [CANCELLED]

> **Cancellation Reason**: 親F763も[CANCELLED]。等価性パイプライン完成(2364/2364 PASS)。UnresolvedLocalマーカーで防御的に動作しており実害なし。Category 4変換は将来必要時に再検討。

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: engine

## Review Context

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F763 |
| Discovery Point | FL POST-LOOP Step 6.3 (Mandatory Handoff) |
| Timestamp | 2026-02-10 |

### Identified Gap
F763 classified ~50 dynamic LOCAL occurrences into 4 categories. Category 4 (~22 occurrences: 12 EVENT compound + 10 RAND ELSEIF) contains genuine conditional patterns that drive condition evaluation. These were deferred from F763 because they require EVENT function conversion infrastructure (F764, now [DONE]) and span multiple non-K1 EVENT files. F763 added a defensive UnresolvedLocal marker but not actual conversion.

### Review Evidence
| Field | Value |
|-------|-------|
| Gap Source | FL Maintainability Review (Phase 3) + Philosophy Gate |
| Derived Task | "Convert Category 4 dynamic LOCAL conditions to YAML in EVENT files" |
| Comparison Result | "UnresolvedLocal marker provides handling but not conversion — Philosophy requires 'convertible'" |
| DEFER Reason | "F764 scoped to K1_EVENT only. Category 4 patterns span K4, K10 and other EVENT files requiring per-file conversion" |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/dotnet/ErbToYaml/ConditionSerializer.cs | UnresolvedLocal handler to be replaced with actual conversion |
| src/tools/dotnet/ErbToYaml/LocalGateResolver.cs | May need dynamic resolution extension |
| Game/ERB/口上/*_EVENT*.ERB | Source files containing Category 4 patterns |
| pm/features/feature-763.md | Parent feature with Category 4 classification |

### Parent Review Observations
F763 FL review identified that F764's completion ([DONE]) makes Category 4 technically feasible. However, F764 was scoped to K1_EVENT only. Category 4 compound LOCAL && MASTER_POSE patterns exist in K4, K10 and other EVENT files. RAND-based LOCAL branching in K4_会話親密.ERB uses PRINTFORM. Proper conversion requires per-file EVENT function processing beyond K1.

## Background

### Philosophy (Mid-term Vision)
(Inherited from F763/F761/F758) The ErbParser/ErbToYaml/KojoComparer tool pipeline is the SSOT for ERB-to-YAML condition conversion. All condition types used in kojo must be parseable, convertible, and verifiable within this pipeline to achieve full equivalence testing (F706 goal). Scope: Category 4 dynamic LOCAL patterns — compound EVENT conditions and RAND-based branching.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F763 (Dynamic LOCAL Variable Tracking) | [CANCELLED] | Provides Category 4 classification, defensive UnresolvedLocal handler, ConditionSerializer LocalRef case. **NOTE**: F763 cancelled (Cat 1-3 already handled, 591/591 PASS). Cat 4 classification remains valid as reference. |
| Predecessor | F764 (EVENT Function Conversion Pipeline) | [DONE] | Provides EVENT function processing infrastructure (K1_EVENT scope) |
| Related | F761 (LOCAL Variable Condition Tracking) | [DONE] | Static LOCAL gate resolution infrastructure |
| Related | F765 (SELECTCASE ARG Parsing) | [DONE] | ConditionSerializer extraction, SELECTCASE patterns |

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

---

## Review Notes

---

## Links
- [feature-763.md](feature-763.md) - Parent feature (dynamic LOCAL classification + defensive handler)
- [feature-764.md](feature-764.md) - EVENT Function Conversion Pipeline
- [feature-761.md](feature-761.md) - Static LOCAL gate resolution
- [feature-765.md](feature-765.md) - SELECTCASE ARG Parsing
