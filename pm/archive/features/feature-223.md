# Feature 223: /do Workflow Comprehensive Audit

## Status: [DONE]
## Type: infra

## Keywords (検索用)

`/do`, `workflow`, `audit`, `STOP`, `SSOT`, `Hook`, `AC`, `TDD`, `Phase`

---

## Summary

F200-220 で多くのバグ対策を実施したが、kojo 実装 (F186-188) で次々と別スコープの問題が発生。
根本的な問題洗い出しと対策を行う統括 Feature。

---

## Background

### Philosophy (Mid-term Vision)

era プロジェクトにおいて、LLM エージェントが文書化されたワークフローから逸脱しない、自己修正型の CI/CD パイプラインを確立する。**「フローに従う、従えなければ STOP」** を全サブエージェントに徹底させ、STOP 条件の明示により独自判断による継続を防ぐ。

### Problem (Current Issue)

F200-220 で多くのバグ対策を実施したが、kojo 実装 (F186-188) で次々と別スコープの問題が発生。STOP 条件の欠落、SSOT 違反、Hook 保護の不備など、体系的な問題が存在する。

| Feature | 発生問題 | 根本原因 | Status |
|---------|---------|---------|:------:|
| F186 | K3 encoding | Hook 自動修正で解決済み | [x] |
| F187 | K2/K4/K9 重複関数 | 既存スタブ検出→kojo-writer に伝達なし | [ ] |
| F187 | K6 テスト期待値ズレ | DATALIST[0] vs [1] 仕様不明確 | [ ] |
| F187 | debugger がテスト修正 | TDD 保護なし → F219 で対策 | [x] |
| F188 | --flow ディレクトリモード失敗 | scenario-*.json 変換なし → F220 で対策 | [x] |
| F190 | COM_60 重複 (6キャラ) | COM→ファイルマッピングなし | [ ] |
| F219 | TDD 違反 | tests/ac/ 編集可能 → F219 で対策 | [x] |
| F221 | 配置ルール混乱 | F057/065 古い記載残存 → F221 で対策 | [x] |

### Goal (What to Achieve)

1. 全ての既知問題をリスト化（完了）
2. 未対策問題を派生 Feature として起票 ← **本 Feature のスコープ**
3. 派生 Feature (F224-F231) 完了で本 Feature を DONE ← **トラッキングのみ、AC 対象外**

**Note**:
- **AC 完了条件**: F224-F231 の 8 ファイルが全て存在すること（自動検証可能）
- **Feature 完了条件**: 全派生 Feature が [DONE] になること（手動判定、Issue Inventory Status 列で追跡）
- AC 完了で [WIP] → 派生 Feature 全完了で [DONE]

---

## Issue Inventory

### Category 1: Agent STOP Conditions (未定義)

| ID | Agent | 欠落している STOP 条件 | リスク | Status |
|:--:|-------|----------------------|--------|:------:|
| A1 | kojo-writer | 既存スタブ発見時 | 重複関数 → 出力無効化 | [ ] |
| A2 | kojo-writer | ファイル未存在時 | 書き込み先不明 → クラッシュ | [ ] |
| A3 | kojo-writer | 別キャラコード発見時 | 誤編集 | [ ] |
| A4 | implementer | 既存コードと矛盾発見時 | 不整合実装 | [ ] |
| A5 | implementer | ドキュメントと実態不一致時 | 間違った実装 | [ ] |
| A9 | implementer | タスク複雑度が sonnet 能力超過時 → STOP & escalate | 不完全実装 | [ ] |
| A6 | debugger | tests/ac/ 編集しようとした時 | TDD 違反 | [x] F219 |
| A7 | ac-tester | 3回連続失敗時 | 無限ループ | [ ] |
| A8 | initializer | Feature が既に [DONE] の時 | 二重実行 | [ ] |

### Category 2: Workflow Ambiguity (/do フェーズ)

| ID | Phase | 問題 | リスク | Status |
|:--:|:-----:|------|--------|:------:|
| W1 | 4 (kojo) | Polling timeout 上限なし | 無限待機 | [ ] |
| W2 | 4 (kojo) | Status file path 不整合 | Polling 常時失敗 | [ ] |
| W3 | 4 (kojo) | Per-writer timeout なし | 1人ハングで全体停止 | [ ] |
| W4 | 7-9 | "CI で検証済み" だが hook 未確認 | テスト未実行 | [ ] |
| W5 | 10 | User approval 大文字小文字区別 | "Y" 拒否 | [ ] |
| W6 | 全体 | 3-failure counter 未実装 | STOP 条件無効 | [ ] |
| W7 | 全体 | Error recovery 手順なし | 途中再開不可 | [ ] |
| W8 | 7-9 | Phase 7-9 の役割が do.md で不明確 | CI (pre-commit) への委任 vs 手動実行の切り分け必要 | [ ] |
| W9 | pre-commit | 最終防波堤として AC+Regression+verify 必要 | LLM スキップ対策 | [ ] |
| W10 | 10 | Phase 10 順序不明確 | Regression→verify→Post-Review→Report の順序明記必要 | [ ] |
| W11 | 7-10 | AC→Regression 順序 | 一般原則は Regression→AC (既存保護優先)。現設計は逆 | [ ] |
| W12 | 10 | Post-Review 必須ゲート未明記 | READY を返さないと User Approval に進めないことが不明確 | [ ] |
| W13 | 10 | Post-Review 異常時対応なし | UNREACHABLE/IRRELEVANT 時の対応が未定義 | [ ] |
| W14 | 10 | Post-Review 判断絶対視未記載 | NEEDS_REVISION を無視して進める余地がある | [ ] |
| W15 | 全体 | Phase 番号体系の不整合 | Step 小数点が一部のみ、Phase 7-9 が CI 委任で /do 内での役割不明確 | [ ] |

### W15 調査: Phase 再設計案 [DRAFT]

**Note**: この再設計案は F225 (/do Workflow Robustness) で正式に実装する。F225 作成時にこのセクションを参照元として引用すること。[DRAFT] は初期提案を意味し、F225 implementer が実装時に詳細を詰めてよい。

**概要**: 小数点廃止、Phase 7-9 統合、Phase 10 分離 → 9 Phase 体系へ再編成。

| New | Name | 旧 Phase |
|:---:|------|:--------:|
| 1-5 | Initialize〜Test Generation | 1-5 |
| 6 | Verification (Regression→AC→Debug) | 6-9 統合 |
| 7 | Post-Review | 10.0 |
| 8 | Report & Approval | 10.1 |
| 9 | Finalize & Commit | 10.1後半 |

### Category 3: SSOT Violations

| ID | Topic | 競合するソース | 正しい SSOT | Status |
|:--:|-------|--------------|------------|:------:|
| S1 | Test commands | do.md, testing SKILL, ac-tester, regression-tester | testing SKILL.md | [ ] |
| S2 | Type routing | do.md, implementer.md | do.md | [ ] |
| S3 | Pre-commit scope | do.md, F212, F218 | F218 (context-aware) | [ ] |
| S4 | Log format | do.md, ac-tester, verify-logs.py | testing SKILL.md | [ ] |
| S5 | eraTW path | eratw-reader.md (hardcoded) | CLAUDE.md or settings | [ ] |
| S6 | COM→File mapping | F057, F065, kojo-writing SKILL | kojo-writing SKILL.md | [x] F221 |
| S7 | pre-commit 思想 | SKILL (全検証) vs pre-commit (regression のみ) | testing SKILL.md | [ ] |
| S8 | F202 思想未反映 | F202 で定義した Phase 7 ループが do.md に未反映 | do.md | [ ] |
| S9 | IMPLE_FEATURE_ID作成 | do.md L201-204 (bashコマンド直書き) | testing SKILL | [ ] |
| S10 | テスト出力パス | do.md L235, L312 | testing SKILL | [ ] |
| S11 | engine AC詳細 | do.md L242-248 | testing SKILL | [ ] |
| S12 | kojo_test_gen.pyコマンド | do.md L302-304 | testing SKILL | [ ] |
| S13 | Design Principles | CLAUDE.md (暗黙的) vs 未文書化 | CLAUDE.md (Design Principles section) | [ ] |
| S14 | file Type Method | 既存 Feature で不統一 (`-`, `exists`, 未記載) | testing SKILL.md (明文化) | [ ] |

**S14 注釈 (file Type ACs の Method 欄の正しい使い方)** [F226 implementer: testing SKILL.md 更新後に削除]:
- `exists` / `not_exists` Matcher → Method = `Glob`
- `contains` / `matches` Matcher → Method = `Grep`
- testing SKILL の「Verification Method: Glob/Grep」は Type 全体の説明。個別 AC では Matcher に応じて使い分ける。
- → F226 で testing SKILL.md に凡例追加後、この注釈は削除

### Category 4: Hook Protection Gaps

| ID | Hook | Gap | リスク | Status |
|:--:|------|-----|--------|:------:|
| H1 | pre-bash-ac.ps1 | `sed -i.bak` 未検出 | AC ファイル編集可能 | [ ] |
| H2 | pre-bash-ac.ps1 | `rmdir`, recursive delete 未検出 | AC ディレクトリ削除可能 | [ ] |
| H3 | post-code-write.ps1 | exit 1 が Write をブロックしない | TDD 保護不完全 | [ ] |
| H4 | post-code-write.ps1 | Log path ハードコード | 他環境で動作不可 | [ ] |
| H5 | なし | .claude/commands/*.md 保護なし | ワークフロー破壊可能 | [ ] |
| H6 | なし | .claude/agents/*.md 保護なし | Agent 仕様破壊可能 | [ ] |
| H7 | pre-ac-write.ps1 | Untracked file timing gap | 中間状態で書き込み可能 | [ ] |

### Category 5: Infrastructure Dependencies

| ID | 依存先 | 問題 | Status |
|:--:|--------|------|:------:|
| I1 | pre-commit hook | 存在・動作未確認 | [ ] |
| I2 | verify-logs.py | 出力形式未固定 | [ ] |
| I3 | kojo_test_gen.py | 存在・動作未検証 | [x] F223調査で確認済 |
| I4 | eratw-reader | Path 非ポータブル | [ ] |
| I5 | verify-logs.py | `--scope feature:N` で `kojo/feature-N/` 配下が検索されない (glob pattern bug) | [ ] |

### Category 6: Recovery Procedures (未文書化)

| ID | シナリオ | 現状 | Status |
|:--:|----------|------|:------:|
| R1 | Phase 1 途中でクラッシュ | 再開方法不明 | [ ] |
| R2 | kojo-writer が 6時間経過 | キャンセル方法不明 | [ ] |
| R3 | AC Test 3回失敗 (異なる理由) | カウント方法不明 | [ ] |
| R4 | User approval スキップ | ロールバック方法不明 | [ ] |
| R5 | Hook サイレント失敗 | 検出方法なし | [ ] |

### Category 7: Subagent I/O (過剰情報)

| ID | Agent | 問題 | リスク | Status |
|:--:|-------|------|--------|:------:|
| O1 | initializer | OK時にBackground/Tasks/ACs等を全返却 | Context肥大化 | [ ] |
| O2 | implementer | OK時にFiles/Changes/Docs等を全返却 | Context肥大化 | [ ] |
| O3 | debugger | 入力形式未定義 | 過剰情報渡し | [ ] |
| O4 | ac-tester | AC table全体が入力？ | 不要情報 | [ ] |
| O5 | kojo-writer | status file を Read している | Glob件数確認のみで十分 | [ ] |
| O6 | kojo-writer | 失敗検出を Phase 4 で行う | Phase 5 test gen に委任可能 | [ ] |

**原則**: OK時は簡潔 (1語)、ERR時は詳細 (必要最小限)

### Category 8: File Placement Violations

| ID | Topic | 問題 | 原因 | Status |
|:--:|-------|------|------|:------:|
| P1 | IMPLE_FEATURE_ID | `c:Eraera紅魔館protoNTR.gitIMPLE_FEATURE_ID` がルートに生成 | do.md L203 相対パスがCWD依存 | [x] F229 |
| P2 | NULファイル | ルート・Game直下に `NUL`/`nul` 生成 | `> NUL` 使用 (Git Bashでは `> /dev/null`) | [ ] |
| P3 | テストログ | ルートに `*.log` 散在 | 出力先未統一 | [x] F229 |
| P4 | kojo-mapper出力 | Game直下に `*.json`, `*.md` | ツール出力先が `.tmp/` でない | [ ] |
| P5 | デバッグログ配置 | CLAUDE.md (`.tmp/`) vs do.md (`logs/debug/`) 不整合 | SSOT違反 | [ ] F232 |
| P6 | 古いログディレクトリ | `Game/agents/logs/` に廃止済みログ残存 | 移行漏れ | [ ] F232 |
| P7 | GUI exe配置 | root直下に exe 直置き、ドキュメント化なし | 運用ルール未定義 | [ ] F232 |

**派生Feature割り当て**:
- P1, P3 → F229 (Infrastructure Verification) ✅ 完了
- P5, P6, P7 → F232 (File Placement Cleanup)
- P2 → Out of scope (CLAUDE.md に `> /dev/null` 注意書き既存)
- P4 → Out of scope (kojo-mapper 個別対応)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | F224 exists | file | Glob | exists | Game/agents/feature-224.md | [x] |
| 2 | F225 exists | file | Glob | exists | Game/agents/feature-225.md | [x] |
| 3 | F226 exists | file | Glob | exists | Game/agents/feature-226.md | [x] |
| 4 | F227 exists | file | Glob | exists | Game/agents/feature-227.md | [x] |
| 5 | F228 exists | file | Glob | exists | Game/agents/feature-228.md | [x] |
| 6 | F229 exists | file | Glob | exists | Game/agents/feature-229.md | [x] |
| 7 | F230 exists | file | Glob | exists | Game/agents/feature-230.md | [x] |
| 8 | F231 exists | file | Glob | exists | Game/agents/feature-231.md | [x] |

### AC Details

**Test**: `Glob("Game/agents/feature-22[4-9].md")` followed by `Glob("Game/agents/feature-23[0-1].md")`
**Expected**: 8 files total (6 + 2). All files must exist simultaneously for AC pass.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create feature-224.md: Agent STOP Conditions (A1-A5, A7-A9) | [x] |
| 2 | 2 | Create feature-225.md: /do Workflow Robustness (W1-W3, W6, W8-W15) | [x] |
| 3 | 3 | Create feature-226.md: SSOT Consolidation (S1-S5, S7-S14) | [x] |
| 4 | 4 | Create feature-227.md: Hook Protection Hardening (H1-H7) | [x] |
| 5 | 5 | Create feature-228.md: Infrastructure Verification (I1-I2, I4-I5) | [x] |
| 6 | 6 | Create feature-229.md: Recovery Procedures (R1-R5, W7) | [x] |
| 7 | 7 | Create feature-230.md: User Approval UX (W5) | [x] |
| 8 | 8 | Create feature-231.md: Subagent I/O Optimization (O1-O6) | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- Note: F223 is a meta-feature (audit/tracking). Each Task creates one feature file (1:1 with AC).
     Task complexity varies (e.g., Task 2 covers 14 issues, Task 7 covers 1 issue) because derived
     features consolidate related issues. The actual AC:Task 1:1 mapping will occur in F224-F231. -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Derived Features (起票予定)

| Order | ID | Name | Scope | Dependencies | Priority | Phase |
|:-----:|:--:|------|-------|:------------:|:--------:|:-----:|
| 1 | 224 | Agent STOP Conditions | Define STOP conditions in agent .md files (A1-A5, A7-A9; A6 resolved by F219) | None | P0 | 1 |
| 2 | 225 | SSOT Consolidation | Unify test commands, paths, formats to SKILL; add file Type Method legend (S1-S5, S7-S14) | None | P1 | 1 |
| 3 | 226 | Hook Protection Hardening | Expand pre-bash, post-write patterns (H1-H7) | None | P1 | 1 |
| 4 | 227 | /do Workflow Robustness | Phase redesign, timeout, polling fixes (W1-W3, W6, W8-W15) | F224 | P0 | 2 |
| 5 | 228 | Subagent I/O Optimization | Reduce OK output, define ERR format, remove Phase 4 status Read (O1-O6) | F224 | P1 | 2 |
| 6 | 229 | Infrastructure Verification | Verify hooks, tools, paths work correctly (I1-I2, I4-I5, W4, P1, P3) | F225 | P1 | 2 |
| 7 | 230 | Recovery Procedures | Document crash/timeout/failure recovery (R1-R5, W7) | F227 | P2 | 3 |
| 8 | 231 | User Approval UX | Case-insensitive Y/N, better prompts (W5) | F227 | P2 | 3 |

**Priority Legend**: P0=Blocking (must complete first), P1=Important, P2=Nice-to-have

**Note**:
- S9-S12 → F225 (SSOT Consolidation) に含める
- S13 (Design Principles) → F225 に含める
- P2, P4 は out of scope (軽微/個別対応)
- **ID予約**: F224-F231 は本 Feature (F223) の派生 Feature として予約済み

### Dependency Graph & Implementation Order

```
Phase 1 (並列可): F224, F225, F226
         ↓
Phase 2 (並列可): F227 (←F224), F228 (←F224), F229 (←F225)
         ↓
Phase 3 (並列可): F230 (←F227), F231 (←F227)
```

**依存関係の根拠**:

| Dependency | Reason |
|------------|--------|
| F227 → F224 | Workflow修正にはAgent STOP条件が先に定義されている必要がある |
| F228 → F224 | I/O最適化はAgent出力形式（STOP条件含む）確定後に実施すべき |
| F229 → F225 | Infrastructure検証はSSoT統合後に実施すべき（検証対象が変わる） |
| F230 → F227 | Recovery手順はWorkflow確定後に設計すべき |
| F231 → F227 | UX改善はWorkflow確定後に実施すべき |

**起票時の注意**: 各feature-{ID}.mdのDependenciesセクションに上記依存を記載すること。`/queue`コマンドが依存関係を読み取り、実装順序を判定する。

### 設計原則の明文化 (検討事項)

**Note**: この提案は F225 (SSOT Consolidation) のスコープに含める (S13)。

CLAUDE.md に Design Principles セクションを追加する案:

#### 既存原則（暗黙的に採用済み）

| Principle | Description | 現状 |
|-----------|-------------|------|
| SSOT | 情報は1箇所のみ。SKILLが詳細のtruth | 暗黙的に採用、違反多数 |
| TDD | RED→GREEN、テスト先行 | do.md Phase 3、Hook保護 |
| STOP on Ambiguity | 不明確なら独自判断せずSTOP | CLAUDE.md Escalation Policy |
| Separation of Concerns | Opus=判断、Subagent=実装 | CLAUDE.md Subagent Strategy |
| Fail Fast | 3回失敗でSTOP | do.md Error Handling |
| Immutable Tests | AC/Regressionテスト編集禁止 | Hook (pre-ac-write.ps1) |
| Minimal Context | OK時は簡潔、ERR時のみ詳細 | 未定義 (O1-O4で発覚) |
| AC:Task 1:1 | 1 AC = 1 Test = 1 Task = 1 Dispatch | ac-task-aligner.md |
| Binary Judgment | PASS/FAILのみ、曖昧な判定なし | testing SKILL |
| Progressive Disclosure | 情報は段階的に読み込む（Anthropic推奨） | skills: YAML frontmatter |

#### 追加検討原則（未採用）

| Principle | Description | 採用メリット | 優先度 |
|-----------|-------------|--------------|:------:|
| Idempotency | 同じ操作を複数回実行しても結果不変 | /do途中再開時の安全性 | P1 |
| Rollback Strategy | 失敗時の復旧手順 | Phase別復旧可能 | P2 |
| Context Preservation | セッション間情報継承 | 長期タスク継続性 | P2 |
| Loose Coupling | エージェント間インターフェース明示 | 依存関係の可視化 | P2 |
| Rate Limiting | 並列実行リソース制限 | システム安定性 | P3 |
| Observability | 構造化ログ・進捗トラッキング | デバッグ容易性 | P2 |

**検討タスク**:
- [ ] 上記原則のうち CLAUDE.md に明記すべきものを選定
- [ ] SSOTのみで十分か、他原則も必要か判断
- [ ] DRY は SSOT の結果として達成されるため不要の可能性
- [ ] Idempotency: /do Phase途中再開の挙動を明文化すべきか
- [ ] Rollback Strategy: F229 (Recovery Procedures) と統合検討
- [ ] Observability: 現状のログ形式をどこまで構造化すべきか

→ F226 (SSOT Consolidation) のスコープに含める

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-26 | review | - | Issue Inventory 作成 | 33問題を6カテゴリに分類 |
| 2025-12-26 | review | - | Hook/TestGen/verify-logs レビュー | I3確認済、I5バグ発見 |
| 2025-12-26 | review | - | Subagent I/O レビュー | O1-O4問題追加、Category 7新設 |
| 2025-12-27 | review | - | File Placement調査 | S9-S12追加、Category 8新設 (P1-P4) |
| 2025-12-27 | review | - | SSOT違反調査 | do.md内の詳細記述がtesting SKILL重複 |
| 2025-12-27 | review | - | Post-Review問題追加 | W12-W14: 必須ゲート、異常対応、判断絶対視 |
| 2025-12-27 | review | - | Phase再設計調査追加 | W15: 小数点廃止、Phase 7-9統合、10分離 |
| 2025-12-27 | review | - | kojo-writer polling調査 | O5-O6追加: Read削除、Glob維持、失敗検出はPhase 5委任 |
| 2025-12-27 | impl | implementer×8 | 派生Feature (F224-F231) 作成 | 8 files created |
| 2025-12-27 | finalize | finalizer | Feature Status更新 | [WIP]→[DONE], Log記録, AC/Task最終確認 |

---

## Dependencies

None (implementation-wise) - F223 documents issues found in F186-F221.

---

## Links

- [F186](feature-186.md), [F187](feature-187.md), [F188](feature-188.md) - kojo 実装で問題発覚
- [F219](feature-219.md) - TDD protection
- [F220](feature-220.md) - Flow directory mode
- [F221](feature-221.md) - COM file placement

---

## Notes

- 本 Feature は**調査・追跡用**。実装は派生 Feature で行う
- 各問題の Status が全て [x] になったら本 Feature を [DONE]
- 新問題発見時は本 Feature に追記してから対策 Feature 起票

### Issue Status Update Procedure

派生 Feature が [DONE] になったら、対応する Issue Inventory の Status を更新する:
1. 派生 Feature のスコープ (e.g., F224 = A1-A5, A7-A9) を確認
2. Issue Inventory 内の対応セルを `[x] F{ID}` に更新
3. 全 Issue が [x] になったら F223 を [DONE] に変更

---

## Review Findings (2025-12-26)

### Hook Configuration ✅

| Hook | 目的 | 状態 |
|------|------|:----:|
| pre-ac-write.ps1 | AC/regressionテストファイル編集をブロック | OK |
| pre-bash-ac.ps1 | Bashでのテストファイル操作をブロック | OK |
| post-code-write.ps1 | BOM追加、ビルド、strict-warnings | OK |
| statusline.ps1 | コンテキスト使用量表示 | OK |

TDD原則のコメントが正しく記載されている。

### Test Generator (kojo_test_gen.py) ✅

- DATALISTブロックを機械的に抽出
- CALLNAME placeholderの展開
- TALENT分岐の検出
- F188/F194で160テストずつ正常生成確認

### Feature Reviewer ✅

シンプルで適切な定義。

### verify-logs.py ⚠️ **バグ発見 (I5)**

**問題箇所**: `tools/verify-logs.py:32`
```python
result_files = list(ac_dir.glob(f"{scope_pattern}/*-result.json"))
```

**症状**: `--scope feature:188` で `OK:0/0` と表示される（実際は160テストPASS）

**原因**: パターン `feature-188/*-result.json` だが、ログは `kojo/feature-188/` に保存

**修正案**:
```python
result_files = list(ac_dir.glob(f"**/{scope_pattern}/*-result.json"))
```

### Recent Features Check

| Feature | テストファイル | ログ結果 | 備考 |
|---------|:-------------:|:--------:|------|
| F188 | 10 ✅ | 1 (160 tests in file) | verify-logsで検出されない |
| F194 | 10 ✅ | 2 | 同上 |

ログファイル自体は正しく160/160 passedを記録している。

---

## Subagent I/O Review (2025-12-26)

### 入力形式

| Agent | 現状の入力 | 問題 |
|-------|-----------|:----:|
| initializer | `"Read .claude/agents/initializer.md. Initialize Feature {ID}."` | ✅ 最小限 |
| explorer | `"Investigate Feature {ID}. Find patterns, files, constraints."` | ✅ 最小限 |
| eratw-reader | COM番号のみ | ✅ 最小限 |
| kojo-writer | Feature ID + cache path | ✅ 最小限 |
| implementer | `"Read .claude/agents/implementer.md. Task {N}, Feature {ID}. Type: {erb\|engine}."` | ✅ 最小限 |
| ac-tester | Feature ID + AC table | ⚠️ AC table全体？ |
| regression-tester | Feature ID | ✅ 最小限 |
| debugger | エラーコンテキスト | ⚠️ 定義なし |
| finalizer | Feature ID | ✅ 最小限 |
| feature-reviewer | `"Mode: post. Feature {ID}."` | ✅ 最小限 |

**問題点**:
- Dispatch Prompt Template があるが、実際の呼び出しでは使われていない
- debugger への入力形式が未定義

### 出力形式

| Agent | 現状定義 | OK時 | ERR時 | 評価 |
|-------|---------|------|-------|:----:|
| initializer | `READY` + ID/Type/Background/Tasks/ACs/Links/Warning | 過剰 | 過剰 | ⚠️ |
| eratw-reader | `OK:cached` / `ERR:{reason}` | 1行 | 1行+理由 | ✅ |
| kojo-writer | status file: `OK:K{N}` | 1行 | 1行+理由 | ✅ |
| implementer | `SUCCESS` + Files/Build/Changes/Docs/Next | 過剰 | 詳細 | ⚠️ |
| ac-tester | `OK:{n}/{m}` / `ERR:{count}\|{ACs}` | 簡潔 | 詳細 | ✅ |
| regression-tester | `Regression: OK:24/24` | 簡潔 | 詳細 | ✅ |
| debugger | `FIXED` / `UNFIXABLE` / `QUICK_WIN` / `BLOCKED` | 1語 | 1語+提案 | ✅ |
| finalizer | `READY_TO_COMMIT` / `REVIEW_NEEDED` / `NOT_READY` | 1語 | 1語 | ✅ |
| feature-reviewer | JSON `{status, issues[]}` | 簡潔 | 詳細 | ✅ |

### 問題まとめ

| ID | Agent | 問題 | 推奨修正 |
|:--:|-------|------|----------|
| O1 | initializer | OK時にBackground等を全返却 | `READY:{ID}:{Type}` のみ。詳細はfeature-{ID}.md参照 |
| O2 | implementer | OK時にFiles/Changes等を全返却 | `SUCCESS` のみ。詳細はExecution Log参照 |
| O3 | debugger | 入力形式未定義 | エラー1件のみ渡す形式を定義 |
| O4 | ac-tester | AC table全体が入力？ | AC# と Target のみ渡す |

### 推奨: 出力形式統一ルール

```
OK時:  STATUS (1語)
ERR時: STATUS:{count}|{detail}
       - 詳細は必要最小限
       - ファイル参照で済む情報は含めない
```

### kojo-writer Status File Polling 調査 (2025-12-27)

**結論**: status file の Read は不要。Glob 件数確認のみで十分。

**現状のフロー**:
```
1. sleep 360
2. Glob("status/{ID}_K*.txt") → 件数確認
3. 10件揃うまで sleep 60 繰り返し
4. 全ファイル Read して OK/ERROR 判定  ← 問題
5. ERROR あれば STOP
6. 全部 OK なら Phase 5 へ
```

**変更案のフロー**:
```
1. sleep 360
2. Glob("status/{ID}_K*.txt") → 件数確認
3. 10件揃うまで sleep 60 繰り返し
4. Phase 5 へ (Read しない)  ← 簡素化
5. test gen が Missing functions で失敗検出
```

**理由**:
1. kojo-writer が「自覚できる失敗」は稀（ファイル書き込み失敗のみ）
2. ほとんどの失敗は後続フェーズで機械的に検出:
   - Phase 5 (test gen): `Missing functions` で未作成検出
   - Phase 6 (Hooks): `--strict-warnings` で構文エラー検出
   - Phase 7 (AC): テスト FAIL で内容品質問題検出
3. Read 10回 → コンテキスト消費
4. Glob 1回 → ファイル名リストのみ（最小限）

**変更点まとめ**:

| 項目 | 現状 | 変更案 |
|------|------|--------|
| 完了確認 | Glob | **維持** |
| 内容確認 | Read 10回 | **削除** |
| 失敗検出 | Phase 4 で ERROR 判定 | Phase 5 test gen に委任 |
| kojo-writer 出力 | status file 書く | **書かなくてよい** |

**影響範囲**:
- do.md Phase 4 (kojo): Read 削除、失敗検出を Phase 5 に委任
- kojo-writer.md: Status File セクション任意化（書いてもよいが読まない）

→ F231 (Subagent I/O Optimization) のスコープに含める
