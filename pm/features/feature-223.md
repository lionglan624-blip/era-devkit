# Feature 223: /do Workflow Comprehensive Audit



## Status: [DONE]

## Type: infra



## Keywords (検索用)



`/do`, `workflow`, `audit`, `STOP`, `SSOT`, `Hook`, `AC`, `TDD`, `Phase`



---



## Summary



F200-220 で多くのバグ対策を実施したが、kojo 実裁E(F186-188) で次、E��別スコープ�E問題が発生、E
根本皁E��問題洗い出しと対策を行う統括 Feature、E


---



## Background



### Philosophy (Mid-term Vision)



era プロジェクトにおいて、LLM エージェントが斁E��化されたワークフローから逸脱しなぁE���E己修正型�E CI/CD パイプラインを確立する、E*「フローに従う、従えなければ STOP、E* を�Eサブエージェントに徹底させ、STOP 条件の明示により独自判断による継続を防ぐ、E


### Problem (Current Issue)



F200-220 で多くのバグ対策を実施したが、kojo 実裁E(F186-188) で次、E��別スコープ�E問題が発生。STOP 条件の欠落、SSOT 違反、Hook 保護の不備など、体系皁E��問題が存在する、E


| Feature | 発生問顁E| 根本原因 | Status |

|---------|---------|---------|:------:|

| F186 | K3 encoding | Hook 自動修正で解決済み | [x] |

| F187 | K2/K4/K9 重褁E��数 | 既存スタブ検�E→kojo-writer に伝達なぁE| [ ] |

| F187 | K6 チE��ト期征E��ズレ | DATALIST[0] vs [1] 仕様不�E確 | [ ] |

| F187 | debugger がテスト修正 | TDD 保護なぁEↁEF219 で対筁E| [x] |

| F188 | --flow チE��レクトリモード失敁E| scenario-*.json 変換なぁEↁEF220 で対筁E| [x] |

| F190 | COM_60 重褁E(6キャラ) | COM→ファイルマッピングなぁE| [ ] |

| F219 | TDD 違反 | tests/ac/ 編雁E��能 ↁEF219 で対筁E| [x] |

| F221 | 配置ルール混乱 | F057/065 古ぁE��載残孁EↁEF221 で対筁E| [x] |



### Goal (What to Achieve)



1. 全ての既知問題をリスト化�E�完亁E��E
2. 未対策問題を派甁EFeature として起票 ↁE**本 Feature のスコーチE*

3. 派甁EFeature (F224-F231) 完亁E��本 Feature めEDONE ↁE**トラチE��ングのみ、AC 対象夁E*



**Note**:

- **AC 完亁E��件**: F224-F231 の 8 ファイルが�Eて存在すること�E��E動検証可能�E�E
- **Feature 完亁E��件**: 全派甁EFeature ぁE[DONE] になること�E�手動判定、Issue Inventory Status 列で追跡�E�E
- AC 完亁E�� [WIP] ↁE派甁EFeature 全完亁E�� [DONE]



---



## Issue Inventory



### Category 1: Agent STOP Conditions (未定義)



| ID | Agent | 欠落してぁE�� STOP 条件 | リスク | Status |

|:--:|-------|----------------------|--------|:------:|

| A1 | kojo-writer | 既存スタブ発見時 | 重褁E��数 ↁE出力無効匁E| [ ] |

| A2 | kojo-writer | ファイル未存在晁E| 書き込み先不�E ↁEクラチE��ュ | [ ] |

| A3 | kojo-writer | 別キャラコード発見時 | 誤編雁E| [ ] |

| A4 | implementer | 既存コードと矛盾発見時 | 不整合実裁E| [ ] |

| A5 | implementer | ドキュメントと実�E不一致晁E| 間違った実裁E| [ ] |

| A9 | implementer | タスク褁E��度ぁEsonnet 能力趁E��晁EↁESTOP & escalate | 不完�E実裁E| [ ] |

| A6 | debugger | tests/ac/ 編雁E��ようとした晁E| TDD 違反 | [x] F219 |

| A7 | ac-tester | 3回連続失敗時 | 無限ルーチE| [ ] |

| A8 | initializer | Feature が既に [DONE] の晁E| 二重実衁E| [ ] |



### Category 2: Workflow Ambiguity (/do フェーズ)



| ID | Phase | 問顁E| リスク | Status |

|:--:|:-----:|------|--------|:------:|

| W1 | 4 (kojo) | Polling timeout 上限なぁE| 無限征E��E| [ ] |

| W2 | 4 (kojo) | Status file path 不整吁E| Polling 常時失敁E| [ ] |

| W3 | 4 (kojo) | Per-writer timeout なぁE| 1人ハングで全体停止 | [ ] |

| W4 | 7-9 | "CI で検証済み" だぁEhook 未確誁E| チE��ト未実衁E| [ ] |

| W5 | 10 | User approval 大斁E��小文字区別 | "Y" 拒否 | [ ] |

| W6 | 全佁E| 3-failure counter 未実裁E| STOP 条件無効 | [ ] |

| W7 | 全佁E| Error recovery 手頁E��ぁE| 途中再開不可 | [ ] |

| W8 | 7-9 | Phase 7-9 の役割ぁEdo.md で不�E確 | CI (pre-commit) への委任 vs 手動実行�E刁E��刁E��忁E��E| [ ] |

| W9 | pre-commit | 最終防波堤として AC+Regression+verify 忁E��E| LLM スキチE�E対筁E| [ ] |

| W10 | 10 | Phase 10 頁E��不�E確 | Regression→verify→Post-Review→Report の頁E���E記忁E��E| [ ] |

| W11 | 7-10 | AC→Regression 頁E��E| 一般原則は Regression→AC (既存保護優允E。現設計�E送E| [ ] |

| W12 | 10 | Post-Review 忁E��ゲート未明訁E| READY を返さなぁE�� User Approval に進めなぁE��とが不�E確 | [ ] |

| W13 | 10 | Post-Review 異常時対応なぁE| UNREACHABLE/IRRELEVANT 時�E対応が未定義 | [ ] |

| W14 | 10 | Post-Review 判断絶対視未記輁E| NEEDS_REVISION を無視して進める余地があめE| [ ] |

| W15 | 全佁E| Phase 番号体系の不整吁E| Step 小数点が一部のみ、Phase 7-9 ぁECI 委任で /do 冁E��の役割不�E確 | [ ] |



### W15 調査: Phase 再設計桁E[DRAFT]



**Note**: こ�E再設計案�E F225 (/do Workflow Robustness) で正式に実裁E��る、E225 作�E時にこ�Eセクションを参照允E��して引用すること、EDRAFT] は初期提案を意味し、F225 implementer が実裁E��に詳細を詰めてよい、E


**概要E*: 小数点廁E��、Phase 7-9 統合、Phase 10 刁E�� ↁE9 Phase 体系へ再編成、E


| New | Name | 旧 Phase |

|:---:|------|:--------:|

| 1-5 | Initialize〜Test Generation | 1-5 |

| 6 | Verification (Regression→AC→Debug) | 6-9 統吁E|

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

| S9 | IMPLE_FEATURE_ID作�E | do.md L201-204 (bashコマンド直書ぁE | testing SKILL | [ ] |

| S10 | チE��ト�E力パス | do.md L235, L312 | testing SKILL | [ ] |

| S11 | engine AC詳細 | do.md L242-248 | testing SKILL | [ ] |

| S12 | kojo_test_gen.pyコマンチE| do.md L302-304 | testing SKILL | [ ] |

| S13 | Design Principles | CLAUDE.md (暗黙的) vs 未斁E��匁E| CLAUDE.md (Design Principles section) | [ ] |

| S14 | file Type Method | 既孁EFeature で不統一 (`-`, `exists`, 未記輁E | testing SKILL.md (明文匁E | [ ] |



**S14 注釁E(file Type ACs の Method 欁E�E正しい使ぁE��)** [F226 implementer: testing SKILL.md 更新後に削除]:

- `exists` / `not_exists` Matcher ↁEMethod = `Glob`

- `contains` / `matches` Matcher ↁEMethod = `Grep`

- testing SKILL の「Verification Method: Glob/Grep」�E Type 全体�E説明。個別 AC では Matcher に応じて使ぁE�Eける、E
- ↁEF226 で testing SKILL.md に凡例追加後、この注釈�E削除



### Category 4: Hook Protection Gaps



| ID | Hook | Gap | リスク | Status |

|:--:|------|-----|--------|:------:|

| H1 | pre-bash-ac.ps1 | `sed -i.bak` 未検�E | AC ファイル編雁E��能 | [ ] |

| H2 | pre-bash-ac.ps1 | `rmdir`, recursive delete 未検�E | AC チE��レクトリ削除可能 | [ ] |

| H3 | post-code-write.ps1 | exit 1 ぁEWrite をブロチE��しなぁE| TDD 保護不完�E | [ ] |

| H4 | post-code-write.ps1 | Log path ハ�EドコーチE| 他環墁E��動作不可 | [ ] |

| H5 | なぁE| .claude/commands/*.md 保護なぁE| ワークフロー破壊可能 | [ ] |

| H6 | なぁE| .claude/agents/*.md 保護なぁE| Agent 仕様破壊可能 | [ ] |

| H7 | pre-ac-write.ps1 | Untracked file timing gap | 中間状態で書き込み可能 | [ ] |



### Category 5: Infrastructure Dependencies



| ID | 依存�E | 問顁E| Status |

|:--:|--------|------|:------:|

| I1 | pre-commit hook | 存在・動作未確誁E| [ ] |

| I2 | verify-logs.py | 出力形式未固宁E| [ ] |

| I3 | kojo_test_gen.py | 存在・動作未検証 | [x] F223調査で確認渁E|

| I4 | eratw-reader | Path 非�Eータブル | [ ] |

| I5 | verify-logs.py | `--scope feature:N` で `kojo/feature-N/` 配下が検索されなぁE(glob pattern bug) | [ ] |



### Category 6: Recovery Procedures (未斁E��匁E



| ID | シナリオ | 現状 | Status |

|:--:|----------|------|:------:|

| R1 | Phase 1 途中でクラチE��ュ | 再開方法不�E | [ ] |

| R2 | kojo-writer ぁE6時間経過 | キャンセル方法不�E | [ ] |

| R3 | AC Test 3回失敁E(異なる理由) | カウント方法不�E | [ ] |

| R4 | User approval スキチE�E | ロールバック方法不�E | [ ] |

| R5 | Hook サイレント失敁E| 検�E方法なぁE| [ ] |



### Category 7: Subagent I/O (過剰惁E��)



| ID | Agent | 問顁E| リスク | Status |

|:--:|-------|------|--------|:------:|

| O1 | initializer | OK時にBackground/Tasks/ACs等を全返却 | Context肥大匁E| [ ] |

| O2 | implementer | OK時にFiles/Changes/Docs等を全返却 | Context肥大匁E| [ ] |

| O3 | debugger | 入力形式未定義 | 過剰惁E��渡ぁE| [ ] |

| O4 | ac-tester | AC table全体が入力！E| 不要情報 | [ ] |

| O5 | kojo-writer | status file めERead してぁE�� | Glob件数確認�Eみで十�E | [ ] |

| O6 | kojo-writer | 失敗検�EめEPhase 4 で行う | Phase 5 test gen に委任可能 | [ ] |



**原則**: OK時�E簡潁E(1誁E、ERR時�E詳細 (忁E��最小限)



### Category 8: File Placement Violations



| ID | Topic | 問顁E| 原因 | Status |

|:--:|-------|------|------|:------:|

| P1 | IMPLE_FEATURE_ID | `c:Eraera紁E��館protoNTR.gitIMPLE_FEATURE_ID` がルートに生�E | do.md L203 相対パスがCWD依孁E| [x] F229 |

| P2 | NULファイル | ルート�EGame直下に `NUL`/`nul` 生�E | `> NUL` 使用 (Git Bashでは `> /dev/null`) | [ ] |

| P3 | チE��トログ | ルートに `*.log` 散在 | 出力�E未統一 | [x] F229 |

| P4 | kojo-mapper出劁E| Game直下に `*.json`, `*.md` | チE�Eル出力�EぁE`.tmp/` でなぁE| [ ] |

| P5 | チE��チE��ログ配置 | CLAUDE.md (`.tmp/`) vs do.md (`logs/debug/`) 不整吁E| SSOT違反 | [ ] F232 |

| P6 | 古ぁE��グチE��レクトリ | `pm/logs/` に廁E��済みログ残孁E| 移行漏れ | [ ] F232 |

| P7 | GUI exe配置 | root直下に exe 直置き、ドキュメント化なぁE| 運用ルール未定義 | [ ] F232 |



**派生Feature割り当て**:

- P1, P3 ↁEF229 (Infrastructure Verification) ✁E完亁E
- P5, P6, P7 ↁEF232 (File Placement Cleanup)

- P2 ↁEOut of scope (CLAUDE.md に `> /dev/null` 注意書き既孁E

- P4 ↁEOut of scope (kojo-mapper 個別対忁E



---



## Acceptance Criteria



| AC# | Description | Type | Method | Matcher | Expected | Status |

|:---:|-------------|------|--------|---------|----------|:------:|

| 1 | F224 exists | file | Glob | exists | pm/features/feature-224.md | [x] |

| 2 | F225 exists | file | Glob | exists | pm/features/feature-225.md | [x] |

| 3 | F226 exists | file | Glob | exists | pm/features/feature-226.md | [x] |

| 4 | F227 exists | file | Glob | exists | pm/features/feature-227.md | [x] |

| 5 | F228 exists | file | Glob | exists | pm/features/feature-228.md | [x] |

| 6 | F229 exists | file | Glob | exists | pm/features/feature-229.md | [x] |

| 7 | F230 exists | file | Glob | exists | pm/features/feature-230.md | [x] |

| 8 | F231 exists | file | Glob | exists | pm/features/feature-231.md | [x] |



### AC Details



**Test**: `Glob("pm/features/feature-22[4-9].md")` followed by `Glob("pm/features/feature-23[0-1].md")`

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



## Derived Features (起票予宁E



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

- S9-S12 ↁEF225 (SSOT Consolidation) に含める

- S13 (Design Principles) ↁEF225 に含める

- P2, P4 は out of scope (軽微/個別対忁E

- **ID予紁E*: F224-F231 は本 Feature (F223) の派甁EFeature として予紁E��み



### Dependency Graph & Implementation Order



```

Phase 1 (並列可): F224, F225, F226

         ↁE
Phase 2 (並列可): F227 (←F224), F228 (←F224), F229 (←F225)

         ↁE
Phase 3 (並列可): F230 (←F227), F231 (←F227)

```



**依存関係�E根拠**:



| Dependency | Reason |

|------------|--------|

| F227 ↁEF224 | Workflow修正にはAgent STOP条件が�Eに定義されてぁE��忁E��がある |

| F228 ↁEF224 | I/O最適化�EAgent出力形式！ETOP条件含む�E�確定後に実施すべぁE|

| F229 ↁEF225 | Infrastructure検証はSSoT統合後に実施すべき（検証対象が変わる！E|

| F230 ↁEF227 | Recovery手頁E�EWorkflow確定後に設計すべぁE|

| F231 ↁEF227 | UX改喁E�EWorkflow確定後に実施すべぁE|



**起票時�E注愁E*: 吁Eeature-{ID}.mdのDependenciesセクションに上記依存を記載すること。`/queue`コマンドが依存関係を読み取り、実裁E��E��を判定する、E


### 設計原剁E�E明文匁E(検討事頁E



**Note**: こ�E提案�E F225 (SSOT Consolidation) のスコープに含める (S13)、E


CLAUDE.md に Design Principles セクションを追加する桁E



#### 既存原剁E��暗黙的に採用済み�E�E


| Principle | Description | 現状 |

|-----------|-------------|------|

| SSOT | 惁E��は1箁E��のみ。SKILLが詳細のtruth | 暗黙的に採用、E��反多数 |

| TDD | RED→GREEN、テスト�E衁E| do.md Phase 3、Hook保護 |

| STOP on Ambiguity | 不�E確なら独自判断せずSTOP | CLAUDE.md Escalation Policy |

| Separation of Concerns | Opus=判断、Subagent=実裁E| CLAUDE.md Subagent Strategy |

| Fail Fast | 3回失敗でSTOP | do.md Error Handling |

| Immutable Tests | AC/RegressionチE��ト編雁E��止 | Hook (pre-ac-write.ps1) |

| Minimal Context | OK時�E簡潔、ERR時�Eみ詳細 | 未定義 (O1-O4で発要E |

| AC:Task 1:1 | 1 AC = 1 Test = 1 Task = 1 Dispatch | ac-task-aligner.md |

| Binary Judgment | PASS/FAILのみ、曖昧な判定なぁE| testing SKILL |

| Progressive Disclosure | 惁E��は段階的に読み込む�E�Enthropic推奨�E�E| skills: YAML frontmatter |



#### 追加検討原剁E��未採用�E�E


| Principle | Description | 採用メリチE�� | 優先度 |

|-----------|-------------|--------------|:------:|

| Idempotency | 同じ操作を褁E��回実行しても結果不夁E| /do途中再開時�E安�E性 | P1 |

| Rollback Strategy | 失敗時の復旧手頁E| Phase別復旧可能 | P2 |

| Context Preservation | セチE��ョン間情報継承 | 長期タスク継続性 | P2 |

| Loose Coupling | エージェント間インターフェース明示 | 依存関係�E可視化 | P2 |

| Rate Limiting | 並列実行リソース制陁E| シスチE��安定性 | P3 |

| Observability | 構造化ログ・進捗トラチE��ング | チE��チE��容易性 | P2 |



**検討タスク**:

- [ ] 上記原剁E�EぁE�� CLAUDE.md に明記すべきものを選宁E
- [ ] SSOTのみで十�Eか、他原剁E��忁E��か判断

- [ ] DRY は SSOT の結果として達�Eされるため不要�E可能性

- [ ] Idempotency: /do Phase途中再開の挙動を�E斁E��すべきか

- [ ] Rollback Strategy: F229 (Recovery Procedures) と統合検訁E
- [ ] Observability: 現状のログ形式をどこまで構造化すべきか



ↁEF226 (SSOT Consolidation) のスコープに含める



---



## Execution Log



| Timestamp | Event | Agent | Action | Result |

|-----------|:-----:|-------|--------|--------|

| 2025-12-26 | review | - | Issue Inventory 作�E | 33問題を6カチE��リに刁E��E|

| 2025-12-26 | review | - | Hook/TestGen/verify-logs レビュー | I3確認済、I5バグ発要E|

| 2025-12-26 | review | - | Subagent I/O レビュー | O1-O4問題追加、Category 7新設 |

| 2025-12-27 | review | - | File Placement調査 | S9-S12追加、Category 8新設 (P1-P4) |

| 2025-12-27 | review | - | SSOT違反調査 | do.md冁E�E詳細記述がtesting SKILL重褁E|

| 2025-12-27 | review | - | Post-Review問題追加 | W12-W14: 忁E��ゲート、異常対応、判断絶対要E|

| 2025-12-27 | review | - | Phase再設計調査追加 | W15: 小数点廁E��、Phase 7-9統合、E0刁E�� |

| 2025-12-27 | review | - | kojo-writer polling調査 | O5-O6追加: Read削除、Glob維持、失敗検�EはPhase 5委任 |

| 2025-12-27 | impl | implementerÁE | 派生Feature (F224-F231) 作�E | 8 files created |

| 2025-12-27 | finalize | finalizer | Feature Status更新 | [WIP]→[DONE], Log記録, AC/Task最終確誁E|



---



## Dependencies



None (implementation-wise) - F223 documents issues found in F186-F221.



---



## Links



- [F186](feature-186.md), [F187](feature-187.md), [F188](feature-188.md) - kojo 実裁E��問題発要E
- [F219](feature-219.md) - TDD protection

- [F220](feature-220.md) - Flow directory mode

- [F221](feature-221.md) - COM file placement



---



## Notes



- 本 Feature は**調査・追跡用**。実裁E�E派甁EFeature で行う

- 吁E��題�E Status が�Eて [x] になったら本 Feature めE[DONE]

- 新問題発見時は本 Feature に追記してから対筁EFeature 起票



### Issue Status Update Procedure



派甁EFeature ぁE[DONE] になったら、対応すめEIssue Inventory の Status を更新する:

1. 派甁EFeature のスコーチE(e.g., F224 = A1-A5, A7-A9) を確誁E
2. Issue Inventory 冁E�E対応セルめE`[x] F{ID}` に更新

3. 全 Issue ぁE[x] になったら F223 めE[DONE] に変更



---



## Review Findings (2025-12-26)



### Hook Configuration ✁E


| Hook | 目皁E| 状慁E|

|------|------|:----:|

| pre-ac-write.ps1 | AC/regressionチE��トファイル編雁E��ブロチE�� | OK |

| pre-bash-ac.ps1 | BashでのチE��トファイル操作をブロチE�� | OK |

| post-code-write.ps1 | BOM追加、ビルド、strict-warnings | OK |

| statusline.ps1 | コンチE��スト使用量表示 | OK |



TDD原則のコメントが正しく記載されてぁE��、E


### Test Generator (kojo_test_gen.py) ✁E


- DATALISTブロチE��を機械皁E��抽出

- CALLNAME placeholderの展開

- TALENT刁E���E検�E

- F188/F194で160チE��トずつ正常生�E確誁E


### Feature Reviewer ✁E


シンプルで適刁E��定義、E


### verify-logs.py ⚠�E�E**バグ発要E(I5)**



**問題箁E��**: `src/tools/python/verify-logs.py:32`

```python

result_files = list(ac_dir.glob(f"{scope_pattern}/*-result.json"))

```



**痁E��**: `--scope feature:188` で `OK:0/0` と表示される（実際は160チE��チEASS�E�E


**原因**: パターン `feature-188/*-result.json` だが、ログは `kojo/feature-188/` に保孁E


**修正桁E*:

```python

result_files = list(ac_dir.glob(f"**/{scope_pattern}/*-result.json"))

```



### Recent Features Check



| Feature | チE��トファイル | ログ結果 | 備老E|

|---------|:-------------:|:--------:|------|

| F188 | 10 ✁E| 1 (160 tests in file) | verify-logsで検�EされなぁE|

| F194 | 10 ✁E| 2 | 同丁E|



ログファイル自体�E正しく160/160 passedを記録してぁE��、E


---



## Subagent I/O Review (2025-12-26)



### 入力形弁E


| Agent | 現状の入劁E| 問顁E|

|-------|-----------|:----:|

| initializer | `"Read .claude/agents/initializer.md. Initialize Feature {ID}."` | ✁E最小限 |

| explorer | `"Investigate Feature {ID}. Find patterns, files, constraints."` | ✁E最小限 |

| eratw-reader | COM番号のみ | ✁E最小限 |

| kojo-writer | Feature ID + cache path | ✁E最小限 |

| implementer | `"Read .claude/agents/implementer.md. Task {N}, Feature {ID}. Type: {erb\|engine}."` | ✁E最小限 |

| ac-tester | Feature ID + AC table | ⚠�E�EAC table全体！E|

| regression-tester | Feature ID | ✁E最小限 |

| debugger | エラーコンチE��スチE| ⚠�E�E定義なぁE|

| finalizer | Feature ID | ✁E最小限 |

| feature-reviewer | `"Mode: post. Feature {ID}."` | ✁E最小限 |



**問題点**:

- Dispatch Prompt Template があるが、実際の呼び出しでは使われてぁE��ぁE
- debugger への入力形式が未定義



### 出力形弁E


| Agent | 現状定義 | OK晁E| ERR晁E| 評価 |

|-------|---------|------|-------|:----:|

| initializer | `READY` + ID/Type/Background/Tasks/ACs/Links/Warning | 過剰 | 過剰 | ⚠�E�E|

| eratw-reader | `OK:cached` / `ERR:{reason}` | 1衁E| 1衁E琁E�� | ✁E|

| kojo-writer | status file: `OK:K{N}` | 1衁E| 1衁E琁E�� | ✁E|

| implementer | `SUCCESS` + Files/Build/Changes/Docs/Next | 過剰 | 詳細 | ⚠�E�E|

| ac-tester | `OK:{n}/{m}` / `ERR:{count}\|{ACs}` | 簡潁E| 詳細 | ✁E|

| regression-tester | `Regression: OK:24/24` | 簡潁E| 詳細 | ✁E|

| debugger | `FIXED` / `UNFIXABLE` / `QUICK_WIN` / `BLOCKED` | 1誁E| 1誁E提桁E| ✁E|

| finalizer | `READY_TO_COMMIT` / `REVIEW_NEEDED` / `NOT_READY` | 1誁E| 1誁E| ✁E|

| feature-reviewer | JSON `{status, issues[]}` | 簡潁E| 詳細 | ✁E|



### 問題まとめE


| ID | Agent | 問顁E| 推奨修正 |

|:--:|-------|------|----------|

| O1 | initializer | OK時にBackground等を全返却 | `READY:{ID}:{Type}` のみ。詳細はfeature-{ID}.md参�E |

| O2 | implementer | OK時にFiles/Changes等を全返却 | `SUCCESS` のみ。詳細はExecution Log参�E |

| O3 | debugger | 入力形式未定義 | エラー1件のみ渡す形式を定義 |

| O4 | ac-tester | AC table全体が入力！E| AC# と Target のみ渡ぁE|



### 推奨: 出力形式統一ルール



```

OK晁E  STATUS (1誁E

ERR晁E STATUS:{count}|{detail}

       - 詳細は忁E��最小限

       - ファイル参�Eで済�E惁E��は含めなぁE
```



### kojo-writer Status File Polling 調査 (2025-12-27)



**結諁E*: status file の Read は不要、Elob 件数確認�Eみで十�E、E


**現状のフロー**:

```

1. sleep 360

2. Glob("status/{ID}_K*.txt") ↁE件数確誁E
3. 10件揁E��まで sleep 60 繰り返し

4. 全ファイル Read して OK/ERROR 判宁E ↁE問顁E
5. ERROR あれば STOP

6. 全部 OK なめEPhase 5 へ

```



**変更案�Eフロー**:

```

1. sleep 360

2. Glob("status/{ID}_K*.txt") ↁE件数確誁E
3. 10件揁E��まで sleep 60 繰り返し

4. Phase 5 へ (Read しなぁE  ↁE簡素匁E
5. test gen ぁEMissing functions で失敗検�E

```



**琁E��**:

1. kojo-writer が「�E覚できる失敗」�E稀�E�ファイル書き込み失敗�Eみ�E�E
2. ほとんどの失敗�E後続フェーズで機械皁E��検�E:

   - Phase 5 (test gen): `Missing functions` で未作�E検�E

   - Phase 6 (Hooks): `--strict-warnings` で構文エラー検�E

   - Phase 7 (AC): チE��チEFAIL で冁E��品質問題検�E

3. Read 10囁EↁEコンチE��スト消費

4. Glob 1囁EↁEファイル名リスト�Eみ�E�最小限�E�E


**変更点まとめE*:



| 頁E�� | 現状 | 変更桁E|

|------|------|--------|

| 完亁E��誁E| Glob | **維持E* |

| 冁E��確誁E| Read 10囁E| **削除** |

| 失敗検�E | Phase 4 で ERROR 判宁E| Phase 5 test gen に委任 |

| kojo-writer 出劁E| status file 書ぁE| **書かなくてよい** |



**影響篁E��**:

- do.md Phase 4 (kojo): Read 削除、失敗検�EめEPhase 5 に委任

- kojo-writer.md: Status File セクション任意化�E�書ぁE��もよぁE��読まなぁE��E


ↁEF231 (Subagent I/O Optimization) のスコープに含める

