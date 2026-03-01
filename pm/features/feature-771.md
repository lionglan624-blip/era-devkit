# Feature 771: Unconditional/Fallback EVENT Dialogue Conversion and CALL Delegation

## Status: [CANCELLED]

> **Cancellation Reason**: 等価性パイプライン完成(2364/2364 PASS)。未変換EVENTパターン(unconditional fallback/CALL delegation)は等価性に影響なし。変換カバレッジ拡張は将来必要時に再検討。

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

## Background

### Philosophy (Mid-term Vision)
Complete EVENT function conversion coverage to enable full kojo ERB→YAML migration for character dialogue systems.

### Problem (Current Issue)
F764 EVENT conversion pipeline only handles conditional PRINTFORM statements inside IF blocks. Two patterns remain unprocessed:

1. **Unconditional fallback PRINTFORMW** - Standalone PRINTFORM statements after all ARG branches (e.g., K1_7 line 241) are silently skipped by the converter since the parser only processes IfNode structures
2. **CALL delegation statements** - Cross-function calls inside EVENT functions (e.g., K1_0 line 26 `CALL 立ち絵表示`, K1_10 line 402 `CALL KOJO_MESSAGE_K1_SeeYou_900_3`) require separate content resolution and call graph analysis to extract actual dialogue content

### Goal (What to Achieve)
Extend the EVENT conversion pipeline to handle unconditional/fallback dialogue output patterns and cross-function CALL delegation, completing the conversion coverage for EVENT functions.

---

## Links
[F764 - EVENT Function Conversion Pipeline](feature-764.md)
