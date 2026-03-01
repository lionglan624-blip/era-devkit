# Feature 709: Multi-State Equivalence Testing per COM

## Status: [CANCELLED]

> **Cancellation Reason**: Multi-State等価性テストは既に達成済み。`KojoComparer --all --multi-state` で2364/2364 PASS (100%)。4状態(default/恋人/恋慕/思慕)×591テストが全通過しており、本Featureの目標は完全に実現された。

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

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Phase 19 (Kojo Conversion) requires complete conditional branch coverage to ensure ERB==YAML equivalence across all possible states, not just representative states.

### Problem (Current Issue)
F706 (KojoComparer Full Equivalence Verification) achieved **591/591 PASS (100%)** using empty state `{}` (フォールバック/ELSEブランチ選択)。Conditional branches for 恋人 (TALENT:16), 恋慕 (TALENT:3), 思慕 (TALENT:17) は未検証。

**Drift Note (2026-02-11)**: F706 [DONE]. テスト数は650→591に修正（Bug 3: ファントムテスト70件削除、COM_463サブ関数12件追加）。バッチインフラ (BatchExecutor, com_idメタデータ方式) は完全動作確認済み。

### Goal (What to Achieve)
1. Extend KojoComparer to test multiple states per COM (at minimum: 恋人, 恋慕, 思慕, なし)
2. Achieve full conditional branch coverage for all 650 COMs across all TALENT states
3. Provide comprehensive mechanical proof of ERB==YAML equivalence beyond representative states

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F706 | [DONE] | Full Equivalence Verification (591/591 PASS, batch infrastructure confirmed) |

---

## Links
- [feature-706.md](feature-706.md) - Parent feature (batch infrastructure)
