# Feature 327: COM_98 後背位させる 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem
COM_98 (後背位させる) lacks Phase 8d quality dialogue for all characters.

### Goal
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8d: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character

### ⚠️ CRITICAL: COM_98 Semantics (MUST READ)
**COMF98.ERB mechanics require:**
- `TCVAR:116 = TARGET` → TARGET is the actor (行為者)
- `EXP:PLAYER:Ｖ経験 ++` → MASTER/PLAYER gains V experience
- `STAIN:Ｐ → STAIN:PLAYER:Ｖ` → TARGET's P goes into MASTER's V

**Meaning: TARGETがMASTERに後ろから挿入する**
- TARGET (キャラ) が MASTER (主人) を後背位で犯す
- MASTERが四つん這いになり、TARGETが後ろから挿入
- 「後ろから入れる」「MASTERの中に入っていく」等の表現を使用

**WRONG (禁止):**
- ❌ MASTERがTARGETに挿入
- ❌ TARGETが四つん這い
- ❌ 「MASTERに後ろから抱かれる」

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K1美鈴 COM_98 口上出力 | output | --unit tests/ac/kojo/feature-327/test-327-K1.json | contains | DATALIST output | [x] |
| 2 | K2小悪魔 COM_98 口上出力 | output | --unit tests/ac/kojo/feature-327/test-327-K2.json | contains | DATALIST output | [x] |
| 3 | K3パチュリー COM_98 口上出力 | output | --unit tests/ac/kojo/feature-327/test-327-K3.json | contains | DATALIST output | [x] |
| 4 | K4咲夜 COM_98 口上出力 | output | --unit tests/ac/kojo/feature-327/test-327-K4.json | contains | DATALIST output | [x] |
| 5 | K5レミリア COM_98 口上出力 | output | --unit tests/ac/kojo/feature-327/test-327-K5.json | contains | DATALIST output | [x] |
| 6 | K6フラン COM_98 口上出力 | output | --unit tests/ac/kojo/feature-327/test-327-K6.json | contains | DATALIST output | [x] |
| 7 | K7子悪魔 COM_98 口上出力 | output | --unit tests/ac/kojo/feature-327/test-327-K7.json | contains | DATALIST output | [x] |
| 8 | K8チルノ COM_98 口上出力 | output | --unit tests/ac/kojo/feature-327/test-327-K8.json | contains | DATALIST output | [x] |
| 9 | K9大妖精 COM_98 口上出力 | output | --unit tests/ac/kojo/feature-327/test-327-K9.json | contains | DATALIST output | [x] |
| 10 | K10魔理沙 COM_98 口上出力 | output | --unit tests/ac/kojo/feature-327/test-327-K10.json | contains | DATALIST output | [x] |
| 11 | Build succeeds | build | - | succeeds | - | [x] |
| 12 | Regression tests pass | output | --flow tests/regression/ | contains | "passed (100%)" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_98 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 | DEVIATION | eratw-reader | COM_98 section lookup | ERR:section_not_found - eraTW has no COM_98 |
| 2026-01-04 | NEEDS_REVISION | feature-reviewer | Post-review semantic check | Review initially misread COMF98 - corrected after analysis |
| 2026-01-04 | DEVIATION | opus | COMF98 re-analysis | Confirmed: TARGET inserts into MASTER is CORRECT per COMF98.ERB (STAIN:P→PLAYER:V, EXP:PLAYER:V++) |
| 2026-01-04 | DEVIATION | kojo-writer | K2,K4,K5,K6,K9 re-dispatch | Still showing MASTER→TARGET semantics. Kojo-writers not following COMF98 spec. |
| 2026-01-04 | FIX | opus | Delete wrong COM_98 + update spec with CRITICAL section | K2,K4,K5,K6,K9 deleted and re-dispatched |
| 2026-01-04 | OK | kojo-writer | K2,K4,K5,K6,K9 final dispatch | All 10 characters now have correct semantics (TARGET→MASTER) |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
