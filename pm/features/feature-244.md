# Feature 244: COM_67 蟇ｾ髱｢蠎ｧ菴・蜿｣荳・(Phase 8d)

## Status: [PROPOSED]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature 繝輔ぃ繧､繝ｫ縺ｯ SSOT 貅匁侠縺ｧ莉悶Ρ繝ｼ繧ｯ繝輔Ο繝ｼ・・do, /fl, ac-tester・峨→騾｣謳ｺ縺吶ｋ

### Problem
COM_67 (蟇ｾ髱｢蠎ｧ菴・ lacks Phase 8d quality dialogue for all characters.

### Goal
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8d: 蜈ｨCOM邯ｲ鄒・+ 蜩∬ｳｪ謾ｹ菫ｮ
- Quality reference: eraTW髴雁､｢
- Structure: 4蛻・ｲ・ﾃ・4繝代ち繝ｼ繝ｳ per character
- File: KOJO_K{N}_謖ｿ蜈･.ERB (COM 60-72 range)

---

## Acceptance Criteria

| AC# | Char | Type | Method | Matcher | Expected | MockRand | Status |
|:---:|------|------|--------|---------|----------|:--------:|:------:|
| 1 | K1鄒朱斡 | output | --unit | contains | "{auto}" | [0] | [ ] |
| 2 | K2蟆乗が鬲・| output | --unit | contains | "{auto}" | [0] | [ ] |
| 3 | K3繝代メ繝･繝ｪ繝ｼ | output | --unit | contains | "{auto}" | [0] | [ ] |
| 4 | K4蜥ｲ螟・| output | --unit | contains | "{auto}" | [0] | [ ] |
| 5 | K5繝ｬ繝溘Μ繧｢ | output | --unit | contains | "{auto}" | [0] | [ ] |
| 6 | K6繝輔Λ繝ｳ | output | --unit | contains | "{auto}" | [0] | [ ] |
| 7 | K7蟄先が鬲・| output | --unit | contains | "{auto}" | [0] | [ ] |
| 8 | K8繝√Ν繝・| output | --unit | contains | "{auto}" | [0] | [ ] |
| 9 | K9螟ｧ螯也ｲｾ | output | --unit | contains | "{auto}" | [0] | [ ] |
| 10 | K10鬲皮炊豐・| output | --unit | contains | "{auto}" | [0] | [ ] |
| 11 | Build | build | - | succeeds | - | - | [ ] |
| 12 | Regression | output | - | contains | "passed (100%)" | - | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_67 蜿｣荳贋ｽ懈・ (4蛻・ｲ静・繝代ち繝ｼ繝ｳ) | [ ] |
| 2 | 11 | 繝薙Ν繝臥｢ｺ隱・| [ ] |
| 3 | 12 | 蝗槫ｸｰ繝・せ繝・| [ ] |
| 4 | 1-10 | AC讀懆ｨｼ | [ ] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| | | | | |

---

## Links

- [index-features.md](../index-features.md)
- [kojo-reference.md](../reference/kojo-reference.md)
- [kojo-writing SKILL](../../../archive/claude_legacy_20251230/skills/kojo-writing/SKILL.md)
