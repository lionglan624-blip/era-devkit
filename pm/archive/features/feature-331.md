# Feature 331: com_file_map.json Extension Workflow

## Status: [WITHDRAWN]

> **Withdrawn Reason**: F329調査時にF323の存在を見落とし、誤った前提で作成された。
> F323で com_file_map.json は COM 0-699 全範囲をカバー済み。roadmap でも COM 700+ は未定義。
> 本Featureは不要。

## Type: infra

## Background

### Philosophy (Mid-term Vision)
F320 philosophy: "将来の COM 追加時の修正コストをゼロにする" (zero cost for future COM additions)

### Problem (Current Issue)
com_file_map.json extension process undefined when new COM added.

**Evidence**:
- F319: COM_94 not in ranges (90-93 stopped at 93), required ADHOC_FIX
- 7 DEVIATION entries in F319 Execution Log
- F320 designed com_file_map.json SSOT but did not define extension workflow

**Root Cause**:
- F320 philosophy promised zero-cost COM additions
- Reality: No documented procedure for extending ranges or adding character_overrides
- No agent assigned responsibility for detecting "Unsupported COM" errors and proposing com_file_map.json updates

**Impact**:
- F319: 7 DEVIATION entries (file not found, unsupported COM, manual JSON edits)
- F324: 0 DEVIATION entries (F320 fixed ranges to include COM 90-95, but only because COM_95 was within fixed range)

**Why F324 Had Zero DEVIATIONs**:
- F320 extended ranges to include COM 90-95 after F319 discovered the gap
- COM_95 fell within newly extended range → no "Unsupported COM" error
- NOT because workflow improved, but because F320 happened to include the needed COM in its fix

### Goal (What to Achieve)
Define automatic Feature creation when "Unsupported COM" detected, preventing future DEVIATION-heavy executions like F319.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md Phase 4 COM coverage pre-check | file | Grep(do.md) | matches | Phase 4.*kojo.*COM.*com_file_map | [ ] |
| 2 | do.md Phase 5 error handling for Unsupported COM | file | Grep(do.md) | matches | Unsupported COM.*Feature.*STOP | [ ] |
| 3 | kojo-init.md COM coverage check | file | Grep(kojo-init.md) | contains | com_file_map.json.*coverage.*before.*Feature | [ ] |
| 4 | com_file_map.json extension procedure documented | file | Grep(reference/) | exists | com-file-map-extension.md | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add COM coverage pre-check in do.md Phase 4 (kojo only) | [ ] |
| 2 | 2 | Add error handling in do.md Phase 5 for "Unsupported COM" | [ ] |
| 3 | 3 | Add COM coverage check in kojo-init.md before batch Feature creation | [ ] |
| 4 | 4 | Document com_file_map.json extension procedure in reference/ | [ ] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| - | - | - | - | - |

---

## Links

- [index-features.md](index-features.md)
- [feature-329.md](feature-329.md) - Parent investigation
- [feature-319.md](feature-319.md) - 7 DEVIATION source
- [feature-320.md](feature-320.md) - com_file_map.json SSOT design
- [kojo-update-gaps.md](reference/kojo-update-gaps.md) - Workflow gap analysis
