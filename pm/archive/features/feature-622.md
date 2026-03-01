# Feature 622: [DRAFT] Testing SKILL Documentation Update

## Status: [DRAFT]

## Type: infra

## Background

### Philosophy (思想・上位目標)
Testing SKILL documentation should reflect the latest ac-static-verifier capabilities to ensure accurate guidance for feature developers.

### Problem (現状の問題)
F621 implements new ac-static-verifier patterns and error guidance that should be documented in the testing SKILL.

### Goal (このFeatureで達成すること)
Update testing SKILL documentation to reflect F621 improvements:
1. Document new Method formats (`Grep path` and `Grep(path)`)
2. Document bracket escape normalization (`\\[DRAFT\\]` → `[DRAFT]`)
3. Document backtick handling in Expected values
4. Document regex metacharacter detection in `contains` matcher

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F621 | [PROPOSED] | ac-static-verifier Pattern Parsing Fundamental Fix |

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Testing SKILL updated | manual | - | - | Document review confirms updates | [ ] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Update testing SKILL with F621 pattern improvements | [ ] |

## Links
- [F621: ac-static-verifier Pattern Parsing Fundamental Fix](feature-621.md)
- [testing SKILL](../../.claude/skills/testing/SKILL.md)