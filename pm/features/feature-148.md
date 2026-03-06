# Feature 148: Test Strategy Refactoring (Anthropic BP準拠)

## Status: [DONE]

## Type: infra

## Background

### Problem

現在のテスト戦略にAnthropicベストプラクティスとの乖離がある：

1. **testing-reference.md (1246行)** をサブエージェントが毎回読む → コンテキスト浪費
2. **unit-tester** の役割が曖昧（「変化点テスト」の定義不明確）
3. **テストシナリオ作成者が未定義** → TDD不完全
4. **Smoke Test** がサブエージェント実行 → Hooksで自動化すべき

### Goal

Anthropic推奨のテスト戦略に準拠：
- Rules-based feedback (Hooks): strict, smoke
- Matcher-based verification (Subagents): AC test, regression
- TDD的アプローチ: テスト定義 → 実装 → 検証

### Context

- Anthropic: [How we built our multi-agent research system](https://www.anthropic.com/engineering/multi-agent-research-system)
- Anthropic: [Building agents with the Claude Agent SDK](https://www.anthropic.com/engineering/building-agents-with-the-claude-agent-sdk)
- Feature 140: PostToolUse Hook (BOM + Build + Strict)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | testing-reference.md参照削除 | file | not_contains | "testing-reference.md" | [x] |
| 2 | smoke-tester.md 作成 | file | exists | ".claude/agents/smoke-tester.md" | [x] |
| 3 | unit-tester.md 削除 | file | not_exists | ".claude/agents/unit-tester.md" | [x] |
| 4 | smoke hook 追加 | file | contains | "smoke" | [x] |
| 5 | imple.md Phase更新 | file | contains | "Test Creation" | [x] |
| 6 | ac-tester Thinking追加 | file | contains | "Thinking Protocol" | [x] |
| 7 | ビルド成功 | build | succeeds | - | [x] |

### AC Details

#### AC1: testing-reference.md参照削除

全サブエージェントMDから `testing-reference.md` への参照を削除。

**Test Command**:
```bash
grep -r "testing-reference.md" .claude/agents/*.md
```

**Expected**: 0 matches

#### AC2: smoke-tester.md 作成

unit-tester.md を smoke-tester.md にリネーム＆役割再定義。

**Expected Content**:
```markdown
# Smoke Tester Agent
- 目的: エラーなく実行できるか確認
- 判定: Exit 0 + 出力あり + エラーログなし
- 検証しないこと: 出力内容の正しさ (→ AC Testで検証)
```

#### AC3: unit-tester.md 削除

旧ファイルが残っていないことを確認。

#### AC4: smoke hook 追加

PostToolUse hookにsmoke test追加。

**Location**: `.claude/settings.json` or hook script

#### AC5: imple.md Phase更新

TDD的フロー追加:
```
Phase 2.5: Test Creation (NEW)
  - AC定義 → テストシナリオJSON生成
  - 実行してFAIL確認
```

#### AC6: ac-tester Thinking追加

Thinking Protocolセクション追加:
```markdown
## Thinking Protocol
1. Read AC definition
2. Determine test command
3. Execute and capture output
4. Apply matcher strictly
5. Classify result
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Remove testing-reference.md refs from agent MDs | [x] |
| 2 | 2,3 | Rename unit-tester.md → smoke-tester.md with new role | [x] |
| 3 | 4 | Add smoke test to PostToolUse hooks | [x] |
| 4 | 5 | Update imple.md with Phase 2.5 Test Creation | [x] |
| 5 | 6 | Add Thinking Protocol to ac-tester.md | [x] |
| 6 | 7 | Verify build passes | [x] |

---

## Design Details

### 1. Test Strategy (Revised)

```
┌─────────────────────────────────────────────────────┐
│  Hooks (自動・即時) - Rules-based                    │
├─────────────────────────────────────────────────────┤
│  PostToolUse:                                        │
│    - BOM check (既存)                                │
│    - Build check (既存)                              │
│    - Strict warnings (既存)                          │
│    - Smoke test (NEW) ← ERB編集後に自動実行          │
└─────────────────────────────────────────────────────┘
                         ↓ PASS
┌─────────────────────────────────────────────────────┐
│  Subagents (手動・検証) - Matcher-based              │
├─────────────────────────────────────────────────────┤
│  Phase 7: ac-tester                                  │
│    - AC定義に基づく厳密な検証                         │
│    - contains/equals/matches                         │
│                                                      │
│  Phase 6: regression-tester                          │
│    - 既存テストスイート実行                           │
│    - 他機能への影響確認                               │
└─────────────────────────────────────────────────────┘
```

### 2. Phase Flow (Revised)

```
Phase 1: Initialize
Phase 2: Investigation
Phase 2.5: Test Creation (NEW)  ← TDD: テスト定義
    - AC定義からテストシナリオJSON生成
    - 実行してFAIL確認 (RED)
Phase 3: Implementation
    - Hooks自動実行 (strict, smoke)
Phase 4: (削除 - Hooksに統合)
Phase 5: Debug (Hooks FAIL時)
Phase 6: Regression
Phase 7: AC Verification (GREEN)
Phase 8: Completion
```

### 3. smoke-tester.md (New)

```markdown
# Smoke Tester Agent

実装直後の動作確認。エラーなく実行できるかを検証。

## 判定基準

| 結果 | 条件 |
|------|------|
| PASS | Exit 0 + 出力あり + エラーログなし |
| FAIL | Exit ≠ 0 または ERROR 検出 |

## 検証しないこと

- 出力内容の正しさ（→ AC Testで検証）
- 期待文字列の有無（→ AC Testで検証）

## Test Commands

| Type | Command |
|------|---------|
| kojo | `--unit "{func}" --char {N}` |
| erb | `--inject scenario.json` |
| engine | `dotnet test` |

## Output

| Status | Format |
|--------|--------|
| PASS | `OK:SMOKE:{func}` |
| FAIL | `ERR:SMOKE:{func}\|{error}` |
```

### 4. Smoke Hook Implementation

```bash
# .claude/hooks/post-tool-use-smoke.sh
FILE="$1"
if [[ "$FILE" == *.ERB ]]; then
  FUNC=$(basename "$FILE" .ERB)
  cd Game
  OUTPUT=$(dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
    --unit "$FUNC" 2>&1)
  if echo "$OUTPUT" | grep -qE "ERROR|FATAL|Exception"; then
    echo "[SMOKE FAIL] $FUNC"
    exit 1
  fi
  echo "[SMOKE PASS] $FUNC"
fi
```

### 5. Anthropic BP Alignment

| Anthropic推奨 | 対応 | Status |
|---------------|------|:------:|
| Rules-based feedback | Hooks (strict, smoke) | Task 3 |
| Maximize context efficiency | Remove testing-ref refs | Task 1 |
| Single responsibility | smoke ≠ AC | Task 2 |
| TDD approach | Phase 2.5 | Task 4 |
| Extended thinking | Thinking Protocol | Task 5 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| | | | | |

---

## Discovered Issues

| Issue | Type | Priority |
|-------|------|----------|
| | | |

---

## Links

- [Feature 140: PostToolUse Hook](feature-140.md)
- [Feature 143: Agent MD転換](feature-143.md)
- [Anthropic Multi-Agent BP](https://www.anthropic.com/engineering/multi-agent-research-system)
