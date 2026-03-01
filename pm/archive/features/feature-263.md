# Feature 263: Subagent SSOT 完全移行 - 全 Type で Minimal Dispatch

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)

> **Subagents は SSOT を参照して自律的に正しく実行する。dispatch 側の過剰指定を排除する。**

F260 で kojo type について Philosophy を達成。本 Feature で全 Type (erb/engine/infra) に拡張する。

### Problem (Current Issue)

1. **do.md に2つの dispatch 形式が共存**
   - Dispatch Prompt Template (verbose)
   - Minimal Dispatch Format
   - どちらを使うべきか曖昧

2. **erb-syntax / engine-dev SKILL が不完全**
   - kojo-writing: Procedure, Template, Quality, Constraints 完備
   - erb-syntax: 構文リファレンスのみ (Procedure なし)
   - engine-dev: インターフェースリファレンスのみ (Procedure なし)

3. **implementer agent が verbose dispatch に依存**
   - kojo-writer は Minimal Dispatch で動作可能
   - implementer はまだ verbose 形式を想定

### Goal (What to Achieve)

1. **do.md 整理**: verbose Dispatch Prompt Template を削除
2. **SKILL 拡充**: erb-syntax / engine-dev に Procedure, Template, Quality 追加
3. **Agent 整備**: implementer に明示的な Skill 呼び出しパターン追加 (erb-syntax, engine-dev)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | do.md から verbose Dispatch Prompt Template 削除 | file | Grep | not_contains | "## Dispatch Prompt Template" | [x] |
| 2 | erb-syntax に Procedure セクション追加 | file | Grep | contains | "## Procedure" | [x] |
| 3 | erb-syntax に Quality セクション追加 | file | Grep | contains | "## Quality" | [x] |
| 4 | engine-dev に Procedure セクション追加 | file | Grep | contains | "## Procedure" | [x] |
| 5 | engine-dev に Quality セクション追加 | file | Grep | contains | "## Quality" | [x] |
| 6 | implementer.md に erb-syntax Skill 呼び出し追加 | file | Grep | contains | "Skill(erb-syntax)" | [x] |
| 7 | implementer.md に engine-dev Skill 呼び出し追加 | file | Grep | contains | "Skill(engine-dev)" | [x] |

### AC Details

*Manual verification reference (AC Table defines automated testing via Grep tool matcher):*

**AC1**: `Grep` with `not_contains` on `.claude/commands/do.md`
**AC2-5**: `Grep` with `contains` on `.claude/skills/{skill}/SKILL.md`
**AC6-7**: `Grep` with `contains` on `.claude/agents/implementer.md`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | do.md から Dispatch Prompt Template セクション削除 | [O] |
| 2 | 2,3 | erb-syntax SKILL に Procedure, Quality 追加 | [O] |
| 3 | 4,5 | engine-dev SKILL に Procedure, Quality 追加 | [O] |
| 4 | 6,7 | implementer.md に Skill(erb-syntax), Skill(engine-dev) 呼び出し追加 | [O] |

<!-- AC:Task Mapping - Same-file changes grouped for efficiency -->
<!-- Task 1: AC1 (single AC) -->
<!-- Task 2: AC 2,3 same file (erb-syntax SKILL) -->
<!-- Task 3: AC 4,5 same file (engine-dev SKILL) -->
<!-- Task 4: AC 6,7 same file (implementer.md) -->

---

## Design Notes

### erb-syntax SKILL 追加内容

```markdown
## Procedure

1. Read `Game/agents/feature-{ID}.md` for task requirements
2. Identify target ERB files using Glob
3. Read existing code patterns in target file
4. Implement changes following existing conventions
5. Run `dotnet run ... --unit` or `--flow` (per AC requirements) to verify no syntax errors
6. Check for loading warnings with `--strict-warnings < /dev/null`

## Quality

| Check | Criteria |
|-------|----------|
| RETURN | Every function ends with `RETURN {value}` |
| Encoding | UTF-8 with BOM (existing files maintain encoding) |
| Indentation | Tab-based, consistent with file |
| Comments | Japanese, concise, on separate line |
| Variables | Declare before use (`#DIM`, `#DIMS`) |
| Labels | `$label` for GOTO targets, avoid GOTO when possible |
```

### engine-dev SKILL 追加内容

```markdown
## Procedure

1. Read `Game/agents/feature-{ID}.md` for task requirements
2. Identify target C# files using Glob in `engine/` directory
3. Read existing interface patterns and dependencies
4. Implement changes following existing conventions
5. Run `dotnet build` to verify compilation
6. Run `dotnet test` to verify tests pass

## Quality

| Check | Criteria |
|-------|----------|
| Interface | Use GlobalStatic for service dependencies |
| Testing | Add unit tests in `engine.Tests/` for new code |
| Naming | Follow existing conventions (PascalCase methods, camelCase locals) |
| Null Safety | Use nullable annotations (`?`) where appropriate |
| Error Handling | Use structured error types, not exceptions for flow control |
| Documentation | XML comments for public APIs |
```

### implementer.md 追加内容

Task 4 で implementer.md に以下の Skill 呼び出しパターンを追加:

```markdown
## Skill References

When implementing ERB features, invoke `Skill(erb-syntax)` for syntax rules.
When implementing engine features, invoke `Skill(engine-dev)` for C# conventions.
```

*Note: AC6-7 は上記の `Skill(erb-syntax)` と `Skill(engine-dev)` 文字列の存在を Grep で検証する。*

### do.md 変更

- `## Dispatch Prompt Template` セクションとそのコードブロック (約20行) を削除
- Minimal Dispatch Format セクションのみ残す

---

## Review Notes

- F260 Post-Review で検出された minor issue の解決
- F261/F262 とは独立して実行可能

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-29 | Feature completion | finalizer | Mark all tasks [O], all ACs [x], status [DONE] | READY_TO_COMMIT |

---

## Links

- [feature-260.md](feature-260.md) - Dispatch 最小化 + Subagent 整合 (kojo)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md) - 参考実装
- [do.md](../../.claude/commands/do.md) - 修正対象
