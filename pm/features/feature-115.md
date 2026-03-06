# Feature 115: COM_315 クリ愛撫 口上 (Phase 8)

## Status: [COMPLETE]

## Type: kojo

## Background

### Problem
COM_315 (クリ愛撫) lacks Phase 8 quality dialogue for all characters.

### Goal
Create Phase 8 quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8: 4分岐 × 4パターン = 160 DATALIST
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character (160 DATALIST total)

---

## Acceptance Criteria

| AC# | Char | Type | Matcher | Expected | MockRand | Status |
|:---:|------|------|---------|----------|:--------:|:------:|
| 1 | K1美鈴 | output | contains | "そこ、気持ちいい" | [0] | [x] |
| 2 | K2小悪魔 | output | contains | "ひゃんっ……そ、そこ……だめですわぁ……" | [0] | [x] |
| 3 | K3パチュリー | output | contains | "本には載っていない知識ね。あなたから、学ばせてもらうわ" | [0] | [x] |
| 4 | K4咲夜 | output | contains | "そんなに、優しく触られると" | [0] | [x] |
| 5 | K5レミリア | output | contains | "もっと、私を悦ばせなさい……永遠に……" | [0] | [x] |
| 6 | K6フラン | output | contains | "そこ、気持ちいいのさ" | [0] | [x] |
| 7 | K7子悪魔 | output | contains | "そこぉ……だめぇ……あたし、へんになっちゃう……" | [0] | [x] |
| 8 | K8チルノ | output | contains | "最強のあたいが、こんなに気持ちよくなっちゃうなんてね" | [0] | [x] |
| 9 | K9大妖精 | output | contains | "んっ……%CALLNAME:MASTER%の指、あったかいね……" | [0] | [x] |
| 10 | K10魔理沙 | output | contains | "そこ、いいぜ" | [0] | [x] |
| 11 | Build | build | succeeds | - | - | [x] |
| 12 | Regression | output | contains | "passed (100%)" | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_315 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-18 | kojo-writer×10 | K1-K10 COM_315 口上作成 | SUCCESS (160 DATALIST) |
| 2025-12-18 | opus | Build verification | SUCCESS (0 errors) |
| 2025-12-18 | ac-tester | AC1-AC10 Verification | SUCCESS (10/10 PASS) |

---

## AC Test Results Summary

**All 10 character ACs passed verification**:

| AC | Character | Expected String | Match | Evidence |
|----|-----------|-----------------|-------|----------|
| 1 | K1美鈴 | "そこ、気持ちいい" | ✓ | DATALIST variant 1 (mock_rand=[0]) |
| 2 | K2小悪魔 | "ひゃんっ……そ、そこ……だめですわぁ……" | ✓ | DATALIST variant 1 (mock_rand=[0]) |
| 3 | K3パチュリー | "本には載っていない知識ね。あなたから、学ばせてもらうわ" | ✓ | DATALIST variant 1 (mock_rand=[0]) |
| 4 | K4咲夜 | "そんなに、優しく触られると" | ✓ | DATALIST variant 1 (mock_rand=[0]) |
| 5 | K5レミリア | "もっと、私を悦ばせなさい……永遠に……" | ✓ | DATALIST variant 1 (mock_rand=[0]) |
| 6 | K6フラン | "そこ、気持ちいいのさ" | ✓ | DATALIST variant 1 (mock_rand=[0]) |
| 7 | K7子悪魔 | "そこぉ……だめぇ……あたし、へんになっちゃう……" | ✓ | DATALIST variant 1 (mock_rand=[0]) |
| 8 | K8チルノ | "最強のあたいが、こんなに気持ちよくなっちゃうなんてね" | ✓ | DATALIST variant 1 (mock_rand=[0]) |
| 9 | K9大妖精 | "んっ……%CALLNAME:MASTER%の指、あったかいね……" | ✓ | DATALIST variant 1 (mock_rand=[0]) |
| 10 | K10魔理沙 | "そこ、いいぜ" | ✓ | DATALIST variant 1 (mock_rand=[0]) |

**Command Template**:
```bash
cd Game
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --unit "tests/test-115-acN.json" 
```

**Status**: All tests completed with 1/1 (100%) pass rate per AC.

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
