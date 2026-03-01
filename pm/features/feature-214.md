# Feature 214: Infrastructure Cleanup (Skill/Doc Dedup)

## Status: [DONE]

## Type: infra

## Depends: [212]

## Background

### Problem

F203 インフラ監査で発見された問題�EぁE��、F204 から刁E��された頁E��、E

**対象問顁E*:
| # | カチE��リ | 問顁E| 影響 |
|:-:|----------|------|------|
| 14 | Skill/Doc | testing skill と agent.md で惁E��重褁E| 二重メンチE��ンス |
| 16 | Hook | Hook 動作�E明示皁EAC なぁE| 動作保証がなぁE|
| 17 | Skill | testing/ERB.md, ENGINE.md 未確誁E| 冁E��不�E |

### Dependency

**F212 (CI統吁E 完亁E��み**。本 Feature は CI 導�E後�E Doc 整琁E��行う、E

### Goal

1. testing skill と agent.md の惁E��を整琁E��、E��褁E��解涁E
2. Hook 動作テスト方針を斁E��匁E

**Out of Scope**:
- testing/ERB.md, ENGINE.md の冁E��整琁E 存在確認済み、封E��忁E��に応じて別 Feature で対忁E
- imple.md 簡素匁E F212 で完亁E��み

---

## Acceptance Criteria

### Part A: Skill/Doc 重褁E��涁E

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| A1 | regression-tester.md PRE-EXISTING 参�E匁E| file | contains | "See SKILL.md" | [x] |
| A2 | testing skill PRE-EXISTING セクション追加 | file | contains | "## PRE-EXISTING Judgment" | [x] |

### Part B: Hook 動佁EAC

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| B1 | Hook 動作テスト方針決宁E| file | exists | docs/architecture/hook-testing.md | [x] |

### Part C: ビルド検証

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| C1 | dotnet build 成功 | build | succeeds | - | [x] |

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | A1 | regression-tester.md PRE-EXISTING めESKILL.md 参�Eに置揁E| .claude/agents/regression-tester.md | [O] |
| 2 | A2 | testing skill に PRE-EXISTING 判定セクション追加 | .claude/skills/testing/SKILL.md | [O] |
| 3 | B1 | Hook 動作テスト方針ドキュメント作�E | docs/architecture/hook-testing.md | [O] |
| 4 | C1 | ビルド確誁E| engine/uEmuera.Headless.csproj | [O] |

---

## Technical Details

### 整琁E��釁E

```
agent.md    ↁE"何をするぁE (ミッション、判断基溁E
skill       ↁE"どぁE��るか" (コマンド、構文、手頁E
```

**F203 監査結果より**:

| 頁E�� | 現状 | 対忁E|
|------|------|------|
| PRE-EXISTING 判宁E| regression-tester.md にのみ | SKILL.md に移勁E|
| Matchers | ac-tester.md で SSOT 参�E済み | 維持E|

### Task 3: Hook 動作テスト方釁E

**現状**:
- F202 で「Hook は別チャネルで動作、AC1-6 は手動確認が忁E��」と結諁E
- Hook 出力�E Edit/Write チE�Eルの戻り値とは別に表示されめE
- 明示皁E�� AC がなぁE

**選択肢**:
1. 手動確認で十�Eとする (現状維持E
2. Hook 出力を検証する仕絁E��を追加
3. Hook 結果めElog に出力して検証

**成果物**: 選択肢を比輁E��討したドキュメンチE(designs/hook-testing.md) を作�Eし、推奨アプローチを提案すめE

**Note**: F212 CI統合後�E pre-commit hook を前提として選択肢を検訁E

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-24 | - | F204 から刁E��して作�E (旧F206) | PROPOSED |
| 2025-12-24 | - | F205/F206 再構�Eにより F207 に変更 | PROPOSED |
| 2025-12-24 | - | F208 CI統合を先行させるため F209 にリナンバ�E | - |
| 2025-12-25 | - | F209 挿入により 211 にリナンバ�E | PROPOSED |
| 2025-12-25 | - | F211 挿入により 213 にリナンバ�E | PROPOSED |
| 2025-12-25 | - | F213 (Thread Safety) 挿入により 214 にリナンバ�E | PROPOSED |
| 2025-12-25 | implementer | Tasks 1-4 実裁E��亁E| SUCCESS |
| 2025-12-25 | finalizer | Status update: [WIP] ↁE[DONE] | FINALIZED |

---

## Links

- [feature-204.md](feature-204.md) - 刁E��允E
- [feature-203.md](feature-203.md) - 監査結果
- [feature-213.md](feature-213.md) - Thread Safety�E�関連�E�E
- [testing skill](../../../archive/claude_legacy_20251230/skills/testing/)
- [regression-tester.md](../../../archive/claude_legacy_20251230/agents/regression-tester.md)
- [ac-tester.md](../../../archive/claude_legacy_20251230/agents/ac-tester.md)
