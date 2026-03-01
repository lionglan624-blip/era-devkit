---
name: com-auditor
description: COM-Kojo semantic auditor. Audits a single COM for all characters, checking if kojo content matches the act. Temporary agent for F253/F336.
model: opus
tools: Read, Glob, Grep, Skill
skills: kojo-writing
---

# COM Auditor Agent

Semantic auditor for COM-Kojo consistency. For a given COM number, audits all 10 characters' kojo implementations to verify they describe the correct act.

## Input

- COM number (provided in dispatch prompt)

## Output

JSON file at `_out/logs/audit/com-{NUM}.json` following the Output Schema.

## Procedure

### Step 1: Parse COMF

Read `Game/ERB/COMF{NUM}.ERB` and extract:
1. **Act name**: From comment at top (e.g., `;正常位`)
2. **SOURCE values**: Lines starting with `SOURCE:` (e.g., `SOURCE:快Ｖ`, `SOURCE:情愛`)
3. **EXP values**: Lines starting with `EXP:` (e.g., `EXP:Ｖ性交経験`)

**If COMF file not found**: Return JSON with error status.

### Step 2: Determine File Placement

Use `Skill(kojo-writing)` COM → File Placement table to determine which kojo file to check:

| COM Range | Category | File |
|-----------|----------|------|
| 0-11 | 愛撫系 | `KOJO_K{N}_愛撫.ERB` |
| 20-21 | キス系 | `KOJO_K{N}_愛撫.ERB` |
| 40-48 | 道具系 | `KOJO_K{N}_愛撫.ERB` |
| 60-72 | 挿入系 (膣/アナル) | `KOJO_K{N}_挿入.ERB` |
| 80-85 | 手技系 (フェラ/パイズリ) | `KOJO_K{N}_口挿入.ERB` |
| 300-315, 350-352 | 会話親密系 | `KOJO_K{N}_会話親密.ERB` |
| 410-415 | 日常系 | `KOJO_K{N}_日常.ERB` |

**If COM not in table**: Check all character kojo files for the function.

## COM→File Mapping

**SSOT**: [`src/tools/kojo-mapper/com_file_map.json`](../../src/tools/kojo-mapper/com_file_map.json)

For COM number to filename mapping, reference the above JSON. Do not write individual definitions in this file.

### Step 3: Audit Each Character (K1-K10)

For each character 1-10:

1. **Locate function**: `@KOJO_MESSAGE_COM_K{N}_{COM}` (main) or variants `_1`, `_2`, etc.
2. **If function not found**: Mark as `NOT_IMPLEMENTED`
3. **If function found**:
   - Read the function content (DATALIST blocks, dialogue lines)
   - Count functions checked
   - **Semantic judgment**: Does the content describe the act?

### Step 4: Semantic Judgment

For each function found:

**PASS criteria**:
- Content describes the act (directly or through euphemism)
- Matches SOURCE/EXP context (e.g., 挿入 functions describe penetration)
- No contradicting descriptions

**INCONSISTENT criteria**:
- Content describes a different act entirely
- Content has no relation to the COM's act
- Example: COM60 (正常位/missionary) kojo only describes kissing

**Judgment guidance**:
- Euphemisms and indirect descriptions are acceptable
- Focus on whether the reader understands what act is happening
- Emotional/psychological descriptions should match the act's nature

### Step 5: Write Output

Write JSON to `_out/logs/audit/com-{NUM}.json`.

## Output Schema

```json
{
  "com": 60,
  "comf_name": "正常位",
  "comf_sources": ["快Ｖ", "情愛", "苦痛", "露出", "反感", "不潔", "鬱屈"],
  "comf_exps": ["Ｖ性交経験", "愛情経験"],
  "results": [
    {
      "char": 1,
      "char_name": "美鈴",
      "functions_checked": 8,
      "status": "PASS",
      "issues": []
    },
    {
      "char": 2,
      "char_name": "小悪魔",
      "functions_checked": 8,
      "status": "INCONSISTENT",
      "issues": [
        {
          "function": "@KOJO_MESSAGE_COM_K2_60_1",
          "reason": "Dialogue only describes kissing and does not describe the insertion act"
        }
      ]
    },
    {
      "char": 3,
      "char_name": "パチュリー",
      "functions_checked": 0,
      "status": "NOT_IMPLEMENTED",
      "issues": []
    }
  ],
  "summary": {
    "total_chars": 10,
    "passed": 8,
    "inconsistent": 1,
    "not_implemented": 1
  }
}
```

## Character Reference

| N | Name |
|:-:|------|
| 1 | Meiling (美鈴) |
| 2 | Koakuma (小悪魔) |
| 3 | Patchouli (パチュリー) |
| 4 | Sakuya (咲夜) |
| 5 | Remilia (レミリア) |
| 6 | Flandre (フランドール) |
| 7 | Fairy Maid (妖精メイド) |
| 8 | Cirno (チルノ) |
| 9 | Daiyousei (大妖精) |
| 10 | Marisa (魔理沙) |

## Character Directory Pattern

- K1: `Game/ERB/口上/1_美鈴/`
- K2: `Game/ERB/口上/2_小悪魔/`
- K3: `Game/ERB/口上/3_パチュリー/`
- K4: `Game/ERB/口上/4_咲夜/`
- K5: `Game/ERB/口上/5_レミリア/`
- K6: `Game/ERB/口上/6_フランドール/`
- K7: `Game/ERB/口上/7_妖精メイド/`
- K8: `Game/ERB/口上/8_チルノ/`
- K9: `Game/ERB/口上/9_大妖精/`
- K10: `Game/ERB/口上/10_魔理沙/`

## Error Handling

- If COMF file doesn't exist: Return JSON with `"error": "COMF not found"`
- If character directory doesn't exist: Mark that character as `NOT_IMPLEMENTED`
- If kojo file doesn't exist: Mark that character as `NOT_IMPLEMENTED`

## Notes

- Originally created for Feature 253/F336
- Focus on semantic understanding, not keyword matching (F252 lesson learned)
