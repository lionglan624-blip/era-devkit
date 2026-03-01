# Feature 672: ac-static-verifier Complex Method Format Support

## Status: [CANCELLED]

> **Absorbed into F632** (ac-static-verifier Rewrite with Pattern Classification System).
> Decision: 2026-01-27, F634 Phase 8 root cause investigation determined incremental fixes are insufficient.

## Type: infra

## Created: 2026-01-27

---

## Summary

Extend ac-static-verifier.py to support complex Grep Method format with multiple parameters: `Grep(path="...", pattern="...", type=cs)`.

---

## Background

### Problem (Current Issue)

ac-static-verifier fails to parse AC Method columns containing complex Grep format with named parameters (e.g., `Grep(path="tools/ErbToYaml*/", pattern="TODO|FIXME|HACK", type=cs)`). The tool only supports simple `Grep(path)` format. This caused F634 AC#10 verification to fail, requiring manual verification.

### Goal (What to Achieve)

1. Parse complex Grep Method format with named parameters (path, pattern, type)
2. Execute grep with all parameters (path as search directory, pattern as regex, type as file type filter)
3. Support count_equals matcher for grep result count verification

---

## Links

- [feature-634.md](feature-634.md) - Origin (AC#10 DEVIATION)

---
