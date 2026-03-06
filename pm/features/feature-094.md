# Feature 094: eratw-style Image Display (HTML_PRINT)

## Status: [DONE]

## Type: erb

## Background

### Problem
1. Current CBG images overlay on text, making it hard to read
2. Images don't clear when moving locations (remain on screen)
3. Display style differs from eraTW standard

### Goal
Replace CBG overlay system with eraTW-style HTML_PRINT inline image display where:
- Images appear in text flow (not overlaid)
- Multiple characters align horizontally
- Images scroll with text naturally
- Auto line-break after images

### Context
- eraTW uses `HTML_PRINT "<img src='...'>"` for image display
- uEmuera supports HTML_PRINT img tags (confirmed via ConsoleImagePart.cs)
- Reference: `C:\Users\siihe\OneDrive\同人ゲーム\eraTW4.920\ERB\グラフィック表示ライブラリ\グラフィック表示.ERB`

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | グラフィック表示ライブラリ存在 | file | exists | `Game/ERB/グラフィック表示/グラフィック表示.ERB` | [x] |
| 2 | HTML_PRINT画像表示動作 | output | contains | `<img src=` | [x] |
| 3 | 画像表示関数実装 | build | succeeds | - | [x] |
| 4 | 立ち絵表示関数更新 | code | not_contains | `CBGSETSPRITE(LOCALS` | [x] |

### AC Details

#### AC1: グラフィック表示ライブラリ存在

**Test Command**:
```bash
ls Game/ERB/グラフィック表示/グラフィック表示.ERB
```

**Expected Output**: File exists

#### AC2: HTML_PRINT画像表示動作

**Test Command**:
```bash
# Test ERB that calls 画像表示 function using HTML_PRINT
cd Game
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . --unit "TEST_画像表示" 2>&1 | grep "<img src="
```

**Expected Output**: Contains `<img src=` in stdout (HTML tag is printed to console in Headless mode)

#### AC3: 画像表示関数実装

**Test Command**:
```bash
cd Game && dotnet build && dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- .
```

**Expected**: Build succeeds with no ERB parse errors for new functions

#### AC4: 立ち絵表示関数更新

**Test Command**:
```bash
grep "CBGSETSPRITE(LOCALS" Game/ERB/グラフィック表示/立ち絵表示.ERB
```

**Expected**: grep returns no matches (exit code 1)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Port グラフィック表示.ERB from eraTW | [x] |
| 2 | 2 | Implement 画像表示/画像セット/画像一斉表示 functions | [x] |
| 3 | 3 | Add required global variables and verify build succeeds | [x] |
| 4 | 4 | Update 立ち絵表示.ERB to use HTML_PRINT approach | [x] |

---

## Implementation Details

### Files to Create

| File | Description |
|------|-------------|
| `Game/ERB/グラフィック表示/グラフィック表示.ERB` | Port from eraTW |

### Files to Modify

| File | Changes |
|------|---------|
| `Game/ERB/グラフィック表示/立ち絵表示.ERB` | Replace CBGSETSPRITE with HTML_PRINT |
| `Game/CSV/VAR/GLOBAL.CSV` or similar | Add required variables |

### Key Functions to Port

From eraTW `グラフィック表示.ERB`:
- `@画像表示` - Single image display
- `@画像セット` - Queue image for batch display
- `@画像一斉表示` - Display all queued images
- `@画像表示単独HTML` - Generate HTML img tag
- `@ピクセル自動改行` - Auto line-break based on image height

### Required Variables

```erb
; グラフィック表示用変数
TEMP_HTML,0,99        ; HTML buffer array
描画開始行数          ; Drawing start line
MAX_LAYER_NUM         ; Max layer number
TEMP_HTML_MAX_HEIGHT  ; Max height
デフォルトキャラ画像横幅  ; Default image width
キャラ画像表示サイズ比    ; Display scale ratio
```

### HTML_PRINT Syntax (uEmuera supported)

```erb
HTML_PRINT "<img src='SPRITE_NAME' height='100' width='100' ypos='0'>"
```

Attributes:
- `src` - Resource/sprite name
- `srcb` - Button hover image (optional)
- `height` - Height in px or % of font size
- `width` - Width in px or %
- `ypos` - Y position offset

---

## Execution State

**Current Phase**: Completed
**Active Agent**: finalizer
**Last Update**: 2025-12-17 16:30

### Progress Tracking
- [x] Port グラフィック表示.ERB from eraTW
- [x] Implement image display functions
- [x] Add required variables
- [x] Update 立ち絵表示.ERB

### Blockers
None identified.

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-17 | initializer | Initialize Feature 094 | [PROPOSED] → [APPROVED] |
| 2025-12-17 | implementer | Task 1: Port グラフィック表示.ERB from eraTW | SUCCESS - Created ERB+ERH files |
| 2025-12-17 | unit-tester | Task 1: Verify files exist and build succeeds | PASS - All checks passed |
| 2025-12-17 | unit-tester | Task 2: HTML_PRINT画像表示動作 (AC2) | PASS - `<img src=` tag generated |
| 2025-12-17 | unit-tester | Task 3: Build verification (AC3) | PASS - Build succeeds (0 errors) |
| 2025-12-17 | implementer | Task 4: Update 立ち絵表示.ERB to use HTML_PRINT | SUCCESS - Replaced CBGSETSPRITE with 画像表示 |
| 2025-12-17 | unit-tester | Task 4: Verify AC4 (no CBGSETSPRITE usage) | PASS - grep returns no matches, build succeeds |
| 2025-12-17 | ac-tester | AC2 Verification: HTML_PRINT image output | PASS - `<img src='MEIRIN_NORMAL' ...>` generated |
| 2025-12-17 | ac-tester | AC3 Verification: Build succeeds | PASS - Build successful (0 errors, 0 warnings) |
| 2025-12-17 | finalizer | Feature 094 Completion Verified | COMPLETE - All Tasks [x], All ACs [x], Status → [DONE] |

---

## Discovered Issues

| Issue | Type | Priority |
|-------|------|----------|
| UTF-8 BOM required for ERB files with Japanese default args | encoding | Low |

---

## Links

- [Plan File](../../.claude/plans/twinkling-snacking-clover.md)
- [eraTW Reference](C:\Users\siihe\OneDrive\同人ゲーム\eraTW4.920\ERB\グラフィック表示ライブラリ\グラフィック表示.ERB)
- [Feature 050](feature-050.md) - Original tachie implementation (CBG)
