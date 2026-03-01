# Kojo Type Quality Guide

Issues specific to `Type: kojo` features (dialogue implementation).

---

## Granularity

- **1 COM = 1 Feature**
- Volume limit: ~2,500 lines (10 characters)
- AC count: 12

---

## Common Issues

### Issue 1: Missing TALENT Branch Verification

**Symptom**: AC only checks one TALENT branch.

**Example (Bad)**:
```markdown
| 1 | 思慕台詞出力 | output | --unit | contains | "最近一緒にいると" | [ ] |
```

**Example (Good)**:
```markdown
| 1 | Lover TALENT dialogue | output | --unit | contains | "あなたと一緒にいられて" | [ ] |
| 2 | Love TALENT dialogue | output | --unit | contains | "もっと近くにいたい" | [ ] |
| 3 | Crush TALENT dialogue | output | --unit | contains | "最近一緒にいると" | [ ] |
| 4 | No TALENT dialogue | output | --unit | contains | "別に何でもない" | [ ] |
```

**Fix**: Verify all 4 TALENT branches (Lover/Love/Crush/None).

---

### Issue 2: Line Count Not Verified

**Symptom**: No AC for Phase 8d line count requirement (4-8 lines per branch).

**Example (Good)**:
```markdown
| 5 | 行数基準 | code | Grep | count_equals | 4-8 lines per DATAFORM | [ ] |
```

Or in AC Details:
```markdown
**AC#5**: Each TALENT branch has 4-8 lines of dialogue
- Verify with manual review or kojo-mapper --quality
```

---

### Issue 3: Missing eraTW Reference

**Symptom**: No eraTW source reference for COM implementation.

**Example (Good)**:
```markdown
## Implementation Contract

### eraTW Reference

**Source**: `C:\Users\siihe\OneDrive\同人ゲーム\eraTW4.920\ERB\口上\COM21.ERB`
**Lines**: 1-150 (Crush obtained section)

Use eratw-reader agent to extract reference.
```

---

### Issue 4: Emotion/Scene Description Missing

**Symptom**: Dialogue text without context (Phase 8d requirement).

**Example (Bad)**:
```erb
PRINTFORML 最近一緒にいると落ち着く
```

**Example (Good)**:
```erb
; Crush obtained - with calm expression
PRINTFORML 最近一緒にいると落ち着く
```

**AC for this**:
```markdown
| 6 | Emotion/scene description | code | Grep | contains | "; Crush obtained" | [ ] |
```

---

### Issue 5: Character Voice Inconsistency

**Symptom**: Dialogue doesn't match character personality.

**Checklist**:
- [ ] First-person pronoun consistent (私/あたし/僕/etc.)
- [ ] Speech style matches character (ですます/である/casual)
- [ ] Form of address correct (主人/あなた/ご主人様)

See [kojo-writing SKILL](../kojo-writing/SKILL.md) for character reference.

---

## Checklist

- [ ] All 4 TALENT branches have ACs
- [ ] Line count requirement verified (4-8 lines)
- [ ] eraTW reference documented
- [ ] Emotion/scene description included
- [ ] Character voice consistent
- [ ] 1 COM = 1 Feature granularity respected
