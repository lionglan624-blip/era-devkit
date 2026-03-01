# Feature 142: kojo.md ↁEimple.md 統吁E+ How削除

## Status: [DONE]

## Type: infra

## Background

### Problem
- imple.md (677衁E と kojo.md (610衁E ぁE0%重褁E
- 変更時に2箁E��修正が忁E��、不整合リスク
- Step列挙�E�Eow�E�がエージェント�E判断を制陁E

### Goal
- kojo.md めEimple.md に統合！Eype: kojo刁E��！E
- Step列挙を削除し、判断基準�Eみ残す
- 1,287衁EↁE~200行に削渁E

### Context
- [anthropic-recommended-transition.md](../designs/anthropic-recommended-transition.md) Phase B
- Anthropic推奨原則#4: 最小ルール + エージェント判断

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | kojo.md削除 | output | contains | `"deleted"` | [x] |
| 2 | imple.md統合完亁E| file | contains | `"Type: kojo"` | [x] |
| 3 | imple.md行数削渁E| output | lte | `250` | [x] |
| 4 | Step列挙削除�E�既満足�E�| output | equals | `0` | [x] |
| 5 | /imple kojo動作確誁E| file | contains | `"kojo-writer"` | [x] |

**Note**: AC 4 is already satisfied in current imple.md (no "Step X:" patterns exist). Included as verification-only AC.

### AC Details

#### AC1: kojo.md削除

**Test Command**:
```bash
test ! -f .claude/commands/kojo.md && echo "deleted"
```

**Expected Output**: `deleted`

#### AC2: imple.md統合完亁E

**Test Command**:
```bash
cat .claude/commands/imple.md
```

**Expected Output**: File contains `Type: kojo` (routing logic for kojo-type features)

#### AC3: imple.md行数削渁E

**Test Command**:
```bash
wc -l .claude/commands/imple.md | awk '{print $1}'
```

**Expected Output**: Numeric value ≤ 250

#### AC4: Step列挙削除

**Test Command**:
```bash
grep -c "Step [0-9]:" .claude/commands/imple.md
```

**Expected Output**: `0` (exact match, no Step enumeration patterns found)

#### AC5: /imple kojo動作確誁E

**Test Command**:
```bash
# Run /imple on a kojo-type feature and check if kojo-writer appears in Execution Log
grep "kojo-writer" pm/features/feature-{kojo-feature-id}.md
```

**Expected Output**: Contains `kojo-writer` in the Execution Log (Agent column)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 2 | imple.md に Type: kojo ルーチE��ングロジチE��統吁E| [x] |
| 2 | 3 | imple.md を最小ルール + 判断基準�Eみに書き換え（行数削減！E| [x] |
| 3 | 1 | kojo.md を削除 | [○] |
| 4 | 4 | Step列挙削除を確認（既満足の検証�E�E| [x] |
| 5 | 5 | 手動チE��チE /imple でkojo featureが動作確誁E| [x] |

**Alignment**: 1 AC = 1 Task ✁E

---

## Transformation Rules

### 削除対象�E�Eow�E�E

```markdown
<!-- BEFORE: Step列挙 -->
## Step 1: 初期匁E
1. feature.mdを読む
2. initializer agentを起勁E
3. ...

## Step 2: AC検証
1. ac-tester agentを起勁E
2. ...
```

### 残す対象�E�Ehat/Why�E�E

```markdown
<!-- AFTER: 判断基準�Eみ -->
## Workflow

1. **Initialize**: Read feature.md, dispatch initializer
2. **Implement**: Dispatch based on Type (kojo→kojo-writer, erb/engine→implementer)
3. **Test**: Dispatch unit-tester for each AC
4. **Verify**: Dispatch ac-tester
5. **Finalize**: Dispatch finalizer

## Type Routing

| Type | Agent | Purpose |
|------|-------|---------|
| kojo | kojo-writer | Dialogue creation |
| erb | implementer | ERB code |
| engine | implementer | C# engine |

## Decision Criteria

- All ACs must pass before finalizing
- On failure: dispatch debugger (sonnet→opus escalation)
- Max 3 debug attempts per AC
```

---

## Target Structure

```markdown
# /imple - Feature Implementation

## Purpose
Execute feature implementation with subagents.

## Usage
/imple [feature-id]

## Workflow
[5-step summary, no detailed steps]

## Type Routing
[Table: Type ↁEAgent]

## Decision Criteria
[When to escalate, retry limits, success conditions]

## Agent References
[Links to agent MDs for details]
```

**Target: ~200 lines**

---

## Execution State

**Initializer Status**: ✁EINITIALIZED
- Feature ID: 142
- Type: erb
- Current Status: [WIP]
- Dispatch Chain: Ready for Task 1

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-20 | Initialize | initializer | Status ↁE[WIP], plan extraction | READY |
| 2025-12-20 10:46 | START | implementer | Task 1+2 | - |
| 2025-12-20 10:52 | END | implementer | Task 1+2 | SUCCESS (6min) |
| 2025-12-20 10:50 | START | implementer | Task 3 | - |
| 2025-12-20 10:50 | END | implementer | Task 3 | SUCCESS (0min) |
| 2025-12-20 10:50 | START | unit-tester | Task 4 (AC4 verification) | - |
| 2025-12-20 10:50 | END | unit-tester | Task 4 (Step enumeration check) | PASS (0 matches) |
| 2025-12-20 10:52 | START | finalizer | AC verification + status update | - |
| 2025-12-20 10:52 | END | finalizer | Feature 142 | DONE (0min) |

---

## Discovered Issues

| Issue | Type | Priority |
|-------|------|----------|
| | | |

---

## Links

- [anthropic-recommended-transition.md](../designs/anthropic-recommended-transition.md)
- [imple.md](../../../archive/claude_legacy_20251230/commands/imple.md)
- [kojo.md](../../../archive/claude_legacy_20251230/commands/kojo.md)
