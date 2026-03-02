# Feature 813: Post-Phase Review Phase 21

## Status: [DRAFT]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: infra

## Summary

Post-Phase Review for Phase 21: Counter System. Validates Phase 21 completion, reconciles architecture design with implementation, and resolves deferred obligations.

---

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity — Post-Phase Review ensures phase closure with zero outstanding obligations before progressing to Phase 22.

### Deferred Obligations

**N+4 --unit deprecation obligation** (DEFERRED from F782 → F783 → F813):
N+4 --unit deprecation: NOT_FEASIBLE — trigger condition: C# migration functionally complete (kojo no longer requires ERB test runner; kojo testing pipeline dependency resolved). Tracking destination: this feature — /fc時にdeprecation追跡タスクとして具体化する。

**F810 Mandatory Handoffs** (DEFERRED from F810 → F813):
1. **IComableUtilities/ICounterUtilities method duplication consolidation**: 4 methods overlap between IComableUtilities (F809/F810) and ICounterUtilities (F801/F804): TimeProgress (int vs int), IsAirMaster (int vs CharacterId), GetTargetNum (no param vs CharacterId), MasterPose (int,int,int vs int,int returning CharacterId). Consolidation to eliminate parallel interfaces with incompatible signatures for the same ERB functions.

**F805 Mandatory Handoffs** (DEFERRED from F805 → F813):
1. **DatuiMessage DRY extraction**: ERB DATUI_MESSAGE is a shared global function (Game/ERB/COUNTER_MESSAGE.ERB:484) used by both main counter and WC counter. F802 migrated it as `private void DatuiMessage` in CounterMessage.cs. F805 re-implements privately in WcCounterMessage.cs (unavoidable: F802's method is private and sealed). Post-phase, extract to shared `IDatuiMessageService` to eliminate duplication.

**F803 Mandatory Handoffs** (DEFERRED from F803 → F813):
1. **ICharacterStringVariables VariableStore implementation**: CSTR is not stored in VariableStore today; connecting the interface to the engine VariableStore is outside F803 scope. F803 provides interface definition + stub only; runtime VariableStore extension requires engine-layer changes.
2. **EXP_UP logic duplication**: CheckExpUp exists as private in AbilityGrowthProcessor; F803 adds public method to ICounterUtilities interface, creating duplication. Extraction to shared implementation is a refactoring concern.
3. **ICounterSourceHandler ISP violation**: single interface exposes 3 responsibilities (dispatch via HandleCounterSource, undressing via DatUI helpers, pain check via PainCheckVMaster). F803 scope cannot extract IUndressingHandler without additional interface churn; ISP compliance requires separate interfaces per responsibility.
   - **F805 DI impact**: When ISP extracts IUndressingHandler from ICounterSourceHandler, F805 (Toilet Counter Source) must update its DI injection — F805 consumes DatUI helpers via ICounterSourceHandler.
4. **CFlagIndex typed struct**: CFLAG dispatch branches use raw int private constants (CFLAG:MASTER:ローターA挿入, CFLAG:ARG:ぱんつ確認, etc.) instead of typed CFlagIndex. Cross-class reuse not yet needed; deferred per Key Decision.
5. **EquipIndex typed struct**: DATUI helper methods use raw int EQUIP constants (装備:17 レオタード, etc.) instead of typed EquipIndex. Cross-class reuse not yet needed; deferred per Key Decision.
6. **IShrinkageSystem runtime implementation**: F803 creates IShrinkageSystem interface (Era.Core/Counter/IShrinkageSystem.cs) with no engine-layer implementation; production calls use stub/no-op until implemented. Runtime 締り具合変動 logic requires engine-layer integration.

**F807 Mandatory Handoffs** (DEFERRED from F807 → F813):
1. **WcCounterMessageTease行動テストカバレッジ**: F807 ACは全て静的コードチェック（Grepパターン、ビルド成功）。ハンドラ分岐、INPUTループパス、NTR revelation、EQUIP mutationの行動等価性テストが未検証。F807 Philosophyは「structural migration equivalence」に限定済み；行動等価性はF813で検証。
2. **IWcCounterMessageTeaseインターフェース抽出検討**: WcCounterMessageTeaseは具象型で注入（Key Decision Row 1）。WcCounterMessage.Dispatch()の単体テストに全11依存の実体が必要。選択肢: (A) IWcCounterMessageTease作成、(B) 恒久的技術的負債として許容。
3. **キャラクターID定数統合**: MESSAGE27の13個のprivate const（キャラクターID）がF806 SEXと重複の可能性。共有CharacterConstantsクラスが存在しない。統合追跡が必要。
4. **WcCounterMessageコンストラクタ肥大化対策**: F807後11パラメータ、F806追加で12パラメータ。パラメータオブジェクト化またはハンドラディスパッチサービスへの統合を検討。
5. **CFlag/Cflag命名規則正規化**: WcCounterMessage.cs（F805）はCFlag prefix、WcCounterMessageItem/Ntr（F808）はCflag prefix。F807はF808パターンに従うが、WcCounterMessage.cs内の既存CFlag定数は未修正。命名規則統一が必要。
6. **AC#34ローカル関数施行ギャップ追跡**: AC#34のgrepパターン `private (static )?int \w+\(` はC#ローカル関数（`private`キーワードなし）を検出不可。Technical Designはローカル関数を明示的に禁止するが、機械的AC強制が存在しない。WcCounterMessageTeaseでのローカル関数使用は未検証。F813でレビュー時に静的解析ツールまたは追加grepパターンでローカル関数ゼロを確認する必要がある。

**F808 Mandatory Handoffs** (DEFERRED from F808 → F813):
1. **WcCounterMessageNtr責務分割**: ERBファイル `TOILET_COUNTER_MESSAGE_NTR.ERB` は「ペッティングとNTR関連」の寄せ集めだった。6メソッド中4つがNTR無関係（RotorOut=デバイス操作、OshikkoLooking=観察、WithFirstSex/WithPetting=性行為）。9依存コンストラクタはNTR管理クラスタ(2メソッド)のみが`_kojoMessage`,`_counterUtilities`,`_ntrUtility`を使用し、残り4メソッドは3-5依存で済む。分割推奨: IWcCounterMessageNtrObservation + IWcCounterMessageNtrRevelation。
2. **NtrReversalSource/NtrAgreeSource計算式乖離**: ERBは除算ベース動的スケーリング `(LOCAL:21/10)+10` 、C#は固定定数 `subReduction=50` 。等価性テストで検証が必要。

**F808 /fc教訓（プロセス改善）**:
Phase 21の分解（F783）はERBファイル単位で行い、/fcはその境界を無検証で踏襲した。結果、ERBの「残り物寄せ集めファイル」がそのままC#クラス境界になった。今後の/fcでは「ERBファイル境界 ≠ ドメイン境界」の検証ステップが必要。

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (Mandatory Handoff origin) |
| Predecessor | F801 | [DONE] | Main Counter Core |
| Predecessor | F802 | [DONE] | Main Counter Output |
| Predecessor | F803 | [DONE] | Main Counter Source |
| Predecessor | F804 | [DONE] | WC Counter Core |
| Predecessor | F805 | [DONE] | WC Counter Source + Message Core |
| Predecessor | F806 | [DRAFT] | WC Counter Message SEX |
| Predecessor | F807 | [DONE] | WC Counter Message TEASE |
| Predecessor | F808 | [DONE] | WC Counter Message ITEM + NTR |
| Predecessor | F809 | [DONE] | COMABLE Core |
| Predecessor | F810 | [DONE] | COMABLE Extended |
| Predecessor | F811 | [DONE] | SOURCE Entry System |
| Predecessor | F812 | [DONE] | SOURCE1 Extended |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | - | Unify MasterPose SSOT: consolidate ICounterUtilities.MasterPose (F803, (int,int)→CharacterId), IComableUtilities.MasterPose (F809, (int,int,int)→int), and ITouchStateManager.MasterPose (F811, (int,int,bool)→int canonical) into single canonical implementation. Adapter specs in F811 Mandatory Handoffs. | | [ ] |
| 2 | - | Register DI adapters: ICombinationCounter→F802 CounterCombination, IWcCombinationCounter→F802 WcCounterCombination (F811 stub replacement handoff) | | [ ] |
| 3 | - | Migrate F803 ITouchSet→ITouchStateManager (26 references). Parameter mapping: mode→targetPart, type→masterPart, target.Value→character, reset=false (F811 Mandatory Handoffs) | | [ ] |
| 4 | - | Consolidate F803 ITouchSet and F811 ITouchStateManager into single interface (F811 Mandatory Handoffs) | | [ ] |
| 5 | - | Verify IShrinkageSystem runtime implementation exists (F811 deferred item #6) | | [ ] |
| 6 | - | Update Baseline Measurement counts after all Phase 21 features complete | | [ ] |
| 7 | - | Analyzer NoWarn debt一括修正: (1) NoWarnからCA1510を除去, (2) `dotnet format analyzers erakoumakanNTR.sln --diagnostics CA1510 --severity error` で自動修正, (3) `dotnet build` で0 errors確認, (4) `dotnet test` で全テスト通過確認。失敗時はNoWarnに戻して次Phaseに繰り越し。対象ルールの優先順位と手順は `memory/analyzer-nowarn-debt.md` 参照 | | [ ] |
| 8 | - | Stryker.NET baseline計測: `cd Era.Core.Tests && dotnet stryker` を実行し、mutation score (killed%, survived%, total mutants) をprogress logに記録。これが以降のPost-Phase Reviewの比較baselineとなる | | [ ] |
| 9 | - | Dashboard lint/format verification: `cd src/tools/node/feature-dashboard && npm run lint` で0 errors確認 + `npm run format:check` でclean確認。warningは許容するがerrorは修正必須 | | [ ] |
| 10 | - | Push all commits to remote | | [ ] |
| 11 | - | F808 WcCounterMessageNtr責務分割: IWcCounterMessageNtrを2インターフェースに分割。(1) IWcCounterMessageNtrObservation (RotorOut, OshikkoLooking, WithFirstSex, WithPetting — 5依存) (2) IWcCounterMessageNtrRevelation (NtrRevelation, NtrRevelationAttack + private helpers — 7依存)。現状9依存コンストラクタはERBファイル境界の盲目的踏襲が原因。RotorOutのIWcCounterMessageItem移動も検討（デバイスライフサイクルの同一ドメイン）。F806/F807のインジェクション変更が必要 | | [ ] |
| 12 | - | F808 NtrReversalSource/NtrAgreeSource計算式等価性検証: ERBは動的スケーリング（除算ベース: (delta/10)+10等）、C#は固定定数（50,200,500等）。意図的簡略化か回帰バグか判定し、ERB等価が必要なら修正。対象: WcCounterMessageNtr.cs:512-602 vs TOILET_COUNTER_MESSAGE_NTR.ERB:960-1034 | | [ ] |
| 13 | - | CP-2 Step 2a E2E基盤構築: (1) `src/Era.Core.Tests/E2E/` ディレクトリ作成 (2) `AddEraCore()` DI全解決テスト（Phase 5-21 全サービス登録、例外なし） (3) Training→Counter cross-system フロー（seeded IRandomProvider 決定的実行）。設計根拠: `docs/architecture/migration/full-csharp-architecture.md` CP-2 Step 2a。**E2E失敗時の対応**: DI解決またはcross-systemフローが失敗し、障害箇所の切り分けが困難な場合、**実装を中断して F815 Golden Test Design を作成する**。理由: 150+ COMF × COMABLE × Counter が統合された状態で障害点を特定するには、関数単位の等価性検証（ERB出力 vs C#出力のゴールデンテスト）が障害分離レイヤーとして必要。ゴールデンテスト基盤を先に整備し、個別関数の等価性を確認した上でE2Eに再挑戦する。 | | [ ] |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|

---

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
- [Predecessor: F803](feature-803.md) - Counter Source (predecessor for Post-Phase Review)
- [Predecessor: F812](feature-812.md) - SOURCE1 Extended
