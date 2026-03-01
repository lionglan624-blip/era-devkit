# Feature 214: Infrastructure Cleanup (Skill/Doc Dedup)

## Status: [DONE]

## Type: infra

## Depends: [212]

## Background

### Problem

F203 インフラ監査で発見された問題のうち、F204 から分離された項目。

**対象問題**:
| # | カテゴリ | 問題 | 影響 |
|:-:|----------|------|------|
| 14 | Skill/Doc | testing skill と agent.md で情報重複 | 二重メンテナンス |
| 16 | Hook | Hook 動作の明示的 AC なし | 動作保証がない |
| 17 | Skill | testing/ERB.md, ENGINE.md 未確認 | 内容不明 |

### Dependency

**F212 (CI統合) 完了済み**。本 Feature は CI 導入後の Doc 整理を行う。

### Goal

1. testing skill と agent.md の情報を整理し、重複を解消
2. Hook 動作テスト方針を文書化

**Out of Scope**:
- testing/ERB.md, ENGINE.md の内容整理: 存在確認済み、将来必要に応じて別 Feature で対応
- imple.md 簡素化: F212 で完了済み

---

## Acceptance Criteria

### Part A: Skill/Doc 重複解消

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| A1 | regression-tester.md PRE-EXISTING 参照化 | file | contains | "See SKILL.md" | [x] |
| A2 | testing skill PRE-EXISTING セクション追加 | file | contains | "## PRE-EXISTING Judgment" | [x] |

### Part B: Hook 動作 AC

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| B1 | Hook 動作テスト方針決定 | file | exists | Game/agents/designs/hook-testing.md | [x] |

### Part C: ビルド検証

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| C1 | dotnet build 成功 | build | succeeds | - | [x] |

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | A1 | regression-tester.md PRE-EXISTING を SKILL.md 参照に置換 | .claude/agents/regression-tester.md | [O] |
| 2 | A2 | testing skill に PRE-EXISTING 判定セクション追加 | .claude/skills/testing/SKILL.md | [O] |
| 3 | B1 | Hook 動作テスト方針ドキュメント作成 | Game/agents/designs/hook-testing.md | [O] |
| 4 | C1 | ビルド確認 | engine/uEmuera.Headless.csproj | [O] |

---

## Technical Details

### 整理方針

```
agent.md    → "何をするか" (ミッション、判断基準)
skill       → "どうやるか" (コマンド、構文、手順)
```

**F203 監査結果より**:

| 項目 | 現状 | 対応 |
|------|------|------|
| PRE-EXISTING 判定 | regression-tester.md にのみ | SKILL.md に移動 |
| Matchers | ac-tester.md で SSOT 参照済み | 維持 |

### Task 3: Hook 動作テスト方針

**現状**:
- F202 で「Hook は別チャネルで動作、AC1-6 は手動確認が必要」と結論
- Hook 出力は Edit/Write ツールの戻り値とは別に表示される
- 明示的な AC がない

**選択肢**:
1. 手動確認で十分とする (現状維持)
2. Hook 出力を検証する仕組みを追加
3. Hook 結果を log に出力して検証

**成果物**: 選択肢を比較検討したドキュメント (designs/hook-testing.md) を作成し、推奨アプローチを提案する

**Note**: F212 CI統合後の pre-commit hook を前提として選択肢を検討

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-24 | - | F204 から分離して作成 (旧F206) | PROPOSED |
| 2025-12-24 | - | F205/F206 再構成により F207 に変更 | PROPOSED |
| 2025-12-24 | - | F208 CI統合を先行させるため F209 にリナンバー | - |
| 2025-12-25 | - | F209 挿入により 211 にリナンバー | PROPOSED |
| 2025-12-25 | - | F211 挿入により 213 にリナンバー | PROPOSED |
| 2025-12-25 | - | F213 (Thread Safety) 挿入により 214 にリナンバー | PROPOSED |
| 2025-12-25 | implementer | Tasks 1-4 実装完了 | SUCCESS |
| 2025-12-25 | finalizer | Status update: [WIP] → [DONE] | FINALIZED |

---

## Links

- [feature-204.md](feature-204.md) - 分離元
- [feature-203.md](feature-203.md) - 監査結果
- [feature-213.md](feature-213.md) - Thread Safety（関連）
- [testing skill](../../.claude/skills/testing/)
- [regression-tester.md](../../.claude/agents/regression-tester.md)
- [ac-tester.md](../../.claude/agents/ac-tester.md)
