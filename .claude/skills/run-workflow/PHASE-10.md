# Phase 10: Finalize, Commit & CodeRabbit Review

## Goal

Remaining issue Feature creation + finalizer dispatch + commit.

---

## Step 10.0: Handoff Transfer & Completeness Gate (MANDATORY)

**CRITICAL: Phase 10 entry gate. Do NOT skip.**

### 10.0.1: Destination Column Check

```bash
# Check Mandatory Handoffs has no empty Destination
grep -A 100 "Mandatory Handoffs" pm/features/feature-{ID}.md | grep -P "^\|[^|]+\|\s*-?\s*\|" | grep -v "Destination"
```

**Verify mechanically**:
1. `Mandatory Handoffs` table: every row has non-empty `Destination` column (no `-` or blank)
2. `Mandatory Handoffs` table: every row has non-empty `Destination ID` column
3. For Action=A items: DRAFT file exists (`test -f pm/features/feature-{N}.md`)

| Result | Action |
|--------|--------|
| All destinations filled + files exist | Proceed to 10.0.2 |
| Any empty destination or missing file | **STOP** → Return to Phase 9.8 |

### 10.0.2: Transfer Verification (F811 Lesson, F805 Lesson)

**転記は Phase 9.4.1 で完了済み。ここでは検証のみ。**

```
FOR each Mandatory Handoff row:
  Verify: Transferred = [x] AND Result is filled (非空)
```

| Result | Action |
|--------|--------|
| All `[x]` + Result filled | Proceed to Step 10.1 |
| Any `[ ]` or empty Result | **STOP** → Return to Phase 9.4.1 to execute transfer |

---

## Step 10.1: Verify DRAFT Features (from Phase 9.4/9.8)

DRAFTs should already have been created in Phase 9.4 (Handoff Destination) or Phase 9.8 (Remaining Issues).
Phase 10.0 Handoff Completeness Gate verified their existence.

**This step only handles missed items** (if Phase 10.0 gate passed but items remain):

1. Read `Skill(feature-quality)` for type-specific guide
2. Get next Feature ID from `index-features.md`
3. Create `feature-{ID}.md` as [DRAFT] per Deviation Context template (Phase 9.4)
4. Add to `index-features.md` Active Features as [DRAFT]
5. Report: "Feature {IDs} created as [DRAFT] — run `/fc {ID}` to complete"

---

## Step 10.1.5: Created Features Dependency Setup (research type only)

**Trigger**: Feature type is `research` AND DRAFTs were created during implementation

**Purpose**: Ensure created features have proper dependency graph and execution order.

### Procedure

1. **Analyze Dependencies**
   - Read each created DRAFT's Dependencies section
   - Identify Predecessor/Successor relationships between created features
   - Build dependency graph

2. **Determine Execution Layers**
   ```
   Layer 1: Features with only external predecessors (e.g., current feature)
   Layer 2: Features depending on Layer 1
   Layer 3+: Features depending on previous layers
   ```

3. **Set Status by Layer**
   | Layer | Status | Rationale |
   |-------|--------|-----------|
   | Layer 1 | [PROPOSED] | Ready for /fl |
   | Layer 2+ | [BLOCKED] | Waiting for predecessors |

4. **Update index-features.md**
   - Group features by execution layer
   - Add "Depends On" column for Layer 2+
   - Document parallel execution possibilities

5. **Update Feature Files**
   - Fix any stale predecessor references
   - Set correct [BLOCKED]/[PROPOSED] status

6. **Commit Dependency Setup**
   ```bash
   git commit -m "docs: Setup dependency graph for F{created_ids}"
   ```

### Example Output

```markdown
### Phase N: {Phase Name}

**Layer 1** (並列可能):
| ID | Status | Name | Links |
| F101 | [PROPOSED] | ... | ... |
| F102 | [PROPOSED] | ... | ... |

**Layer 2** (Layer 1 完了後):
| ID | Status | Name | Depends On | Links |
| F103 | [BLOCKED] | ... | F101 | ... |
| F104 | [BLOCKED] | ... | F101, F102 | ... |
```

---

## Step 10.2: Dispatch Finalizer

**Always dispatch finalizer** (both normal and blocked paths):

**CRITICAL**: Use Task() with explicit `model: "sonnet"`, NOT Skill(). Skill frontmatter cannot enforce model (Claude Code Issue #14882/#17283). Without explicit model override, finalizer inherits session model (opus), wasting ~85K opus tokens on mechanical status updates.

```
Task(subagent_type: "general-purpose",
     model: "sonnet",
     prompt: "Read .claude/skills/finalizer/SKILL.md and execute for Feature {ID}.
              OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON is a protocol violation.")
```

Finalizer handles routing internally:
- All `[x]` → `[DONE]` (normal path)
- Any `[B]` (no `[-]`/`[ ]`) → `[BLOCKED]` (atomic update of feature-{ID}.md + index-features.md)
- Any `[-]`/`[ ]` → `NOT_READY`

| Result | Action |
|--------|--------|
| READY_TO_COMMIT | Proceed to commit |
| BLOCKED | Proceed to commit (partial work) |
| NOT_READY | **STOP** → Report what's missing |

> **Log Lifecycle Note**: The finalizer Step 5 automatically removes feature-scoped AC logs (`_out/logs/prod/ac/*/feature-{ID}/` directories and `_out/logs/prod/ac/engine/feature-{ID}*.trx` files) when a feature transitions to [DONE]. This prevents log accumulation from completed features. Logs remain available during the entire /run workflow (Phases 1-9) and are only cleaned after all verification passes. See finalizer/SKILL.md Step 5 for details. (log cleanup)

---

## Step 10.3: Commit

### Commit Scope Rule

**Feature関連の変更のみコミットする。** Feature実装中に副次的に発生した無関係な変更（formatter auto-fix等）は別タイミングでコミットすること。

### Commit Ordering Rule (CodeRabbit連携)

複数コミットが必要な場合、**Feature本体のコミットを最後にする**。

```
1st: 補助的コミット（dependency setup, DRAFT creation等）
2nd: Feature本体コミット  ← HEAD（CodeRabbitレビュー対象）
```

**理由**: Step 10.4 CodeRabbitは `--base-commit HEAD~N` でFeatureコミットをレビューする。Feature本体が最後であれば `HEAD~1` で常に正しい対象になる。

### base-commit の決定

```pseudocode
N = number of feature-related commits in this Phase 10
base_commit = HEAD~N
```

| パターン | N | base-commit |
|----------|:-:|-------------|
| Feature本体のみ（1コミット） | 1 | HEAD~1 |
| 補助 + Feature本体（2コミット） | 2 | HEAD~2 |
| 補助2つ + Feature本体（3コミット） | 3 | HEAD~3 |

### Commit Format

```bash
git commit -m "$(cat <<'EOF'
feat(F{ID}): {summary}

{details if needed}

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
EOF
)"
```

---

## Step 10.4: CodeRabbit Review

### Type Routing

| Type | Action | Reason |
|------|--------|--------|
| engine | Execute | C# code changes |
| infra | Execute | C#/Python changes がある場合 |
| erb | Execute | C#移行コードをcore repoで生成。対象repoでCodeRabbit実行 |
| kojo | **Skip** | ERBはpath_filtersで除外済み |
| research | **Skip** | DRAFTドキュメント作成のみ、コード変更なし |

### Procedure

1. base-commit を Step 10.3 の決定ルールに従い算出する。

2. CodeRabbit CLIを実行：
   ```bash
   MSYS_NO_PATHCONV=1 wsl -- bash -c "cd /mnt/c/Era/devkit && \
     /home/siihe/.local/bin/coderabbit review --plain --type committed --base-commit HEAD~{N} 2>&1"
   ```

   > **⚠️ `2>&1` 必須**: CodeRabbit CLIはstdout出力だが、Bash tool経由で出力が空になるケースが確認されている。
   > `2>&1` を付与してstderr/stdoutを統合すること。exit 0 で出力が空の場合は異常 — 再実行して確認すること。

   > **⚠️ ブランチ名スラッシュ禁止**: CodeRabbit CLIはブランチ名の `/` を除去して `git rev-parse` に渡すため、`refactor/xxx` のようなブランチで失敗する。
   > パラレポでCodeRabbit実行時はブランチ名にスラッシュがないことを確認すること。

3. 結果の判定：

   | 結果 | Action |
   |------|--------|
   | 指摘0件（`Review completed: 0 findings` が出力される） | Completion へ |
   | **exit 0 で出力が空** | **異常 — `2>&1` 付きで再実行** |
   | 指摘あり（Major） | DEVIATION記録 → debugger dispatch → 修正 → 再commit → 再レビュー（最大2回） |
   | 指摘あり（Minor/Nitpick） | Execution Logに記録 → Completion へ |
   | CLI error (exit ≠ 0, 指摘なし) | DEVIATION記録 → Skip → Completion へ |

4. Major指摘の修正ループ（最大2回）：
   ```
   CodeRabbit Major指摘 → debugger dispatch → 修正 → git commit --amend → coderabbit review → 判定
   ```
   3回目でまだMajorが残る場合 → Execution Logに記録して Completion へ

### Execution Log 記録 (MANDATORY)

**全タイプで必ず1行記録する。** Skip も記録対象。後から実施/未実施を判別可能にする。

```
| {timestamp} | CodeRabbit | {result} | {detail} |
```

| 状況 | result | detail |
|------|--------|--------|
| Skip (erb/kojo/research) | `Skip ({type})` | `-` |
| 実行・指摘0件 | `0 findings` | `-` |
| 実行・Minor のみ | `{N} Minor (修正不要)` | 指摘概要 |
| 実行・Major → 修正済み | `{N} Major → fixed` | debugger |
| 実行・CLI error | `CLI error (exit {N})` | DEVIATION |

### DEVIATION Check
- CodeRabbit Major指摘 = DEVIATION
- CLI error = DEVIATION
- Minor/Nitpick = NOT DEVIATION（記録のみ）
- Skip = NOT DEVIATION

---

## DEVIATION Check

**CRITICAL (SKILL.md Global Rules)**:
- exit ≠ 0 = DEVIATION（例外なし）
- 判断・解釈・免除は**禁止**
- 「手動検証済み」「環境問題」「PRE-EXISTING」も記録必須

| Event | Is DEVIATION? |
|-------|:-------------:|
| Finalizer BLOCKED | Yes |
| Commit failed | Yes |
| All SUCCESS | No |

---

## 最終チェック

```bash
# deviation-logとfeature-{ID}.mdのDEVIATION数を最終確認
cat _out/tmp/deviation-log.txt 2>/dev/null || echo "No deviation log"
grep -c DEVIATION pm/features/feature-{ID}.md
```

→ 記録漏れがあれば追記してからコミット

---

## Completion

Report to user:
```
Feature {ID} completed and committed.
Commit: {hash}
```

**Workflow complete.**
