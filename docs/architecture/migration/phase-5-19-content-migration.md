# Phase 5-19: Variable System through Kojo Conversion

**Parent**: [full-csharp-architecture.md](../full-csharp-architecture.md)

---

### Phase 5: Variable System

**Phase Status**: DONE

**Goal**: 変数システムの移行（全後続 Phase の基盤）

**Background**: 現行エンジンは 500+ 変数コードと複雑な配列ストレージを持つ。これが全ゲームロジックの基盤。

> **⚠️ Phase 4 Design Requirements（必須）**
>
> このPhaseから全実装にPhase 4パターンを適用すること。
>
> **原則（YAGNI/KISS優先）**:
> - **やること**: static class禁止（インターフェース必須）、Strongly Typed ID使用、Result型使用
> - **やらないこと**: 行数による機械的分割、将来使うかもしれない抽象化
> - 詳細は [F377 Design Principles](../feature-377.md#design-principles) 参照
>
> **Strongly Typed IDs**:
> - `FlagIndex` - FLAG/TFLAG配列インデックス用
> - `CharacterFlagIndex` - CFLAG用
> - `AbilityIndex`, `TalentIndex`, `PalamIndex`, `ExpIndex` - 2D配列インデックス用
> - `LocalVariableIndex` - LOCAL変数用
> - Note: VariableId は VariableCode enum (F385) と重複するため削除
>
> **インターフェース定義**:
> ```csharp
> public interface IVariableStore
> {
>     int GetFlag(FlagIndex index);
>     void SetFlag(FlagIndex index, int value);
>     Result<int> GetCharacterFlag(CharacterId character, CharacterFlagIndex flag);
> }
> ```
>
> **DI登録**:
> - `IVariableStore` → `VariableStore`
> - `IVariableScope` → `VariableScope`
>
> **Result型使用**:
> - 変数アクセス失敗時は例外ではなく `Result<T>` を返す
> - 無効なインデックスアクセス → `Result.Fail("Invalid index")`

**Tasks**:
1. VariableCode enum 移行（FLAG, CFLAG, TFLAG, ABL, TALENT 等）
2. VariableData ストレージ実装（1D/2D/3D 配列対応）
3. スコープ管理（LOCAL, GLOBAL, CHARACTER）
4. 変数名解決（IdentifierDictionary）
5. CSV からの変数定義ロード
6. **Create Phase 5 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 5 セクションとの整合確認必須
7. **Create Phase 6 Planning feature** (type: research, include transition feature tasks)

**Source Analysis**:

| Current File | Lines | Purpose |
|--------------|:-----:|---------|
| `VariableCode.cs` | ~1500 | 500+ 変数コード定義 |
| `VariableData.cs` | ~800 | 配列ストレージ |
| `VariableEvaluator.cs` | ~400 | 変数評価 |
| `VariableLocal.cs` | ~200 | ローカルスコープ |
| `CharacterData.cs` | ~300 | キャラクター変数 |

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Variables/VariableCode.cs` | 変数コード enum |
| `src/Era.Core/Variables/IVariableStore.cs` | ストレージインターフェース |
| `src/Era.Core/Variables/VariableStore.cs` | 配列ストレージ実装 |
| `src/Era.Core/Variables/VariableScope.cs` | スコープ管理 |
| `src/Era.Core/Variables/CharacterVariables.cs` | キャラクター変数 |
| `src/Era.Core/Variables/VariableResolver.cs` | 変数名解決 |
| `src/Era.Core/Variables/VariableDefinitionLoader.cs` | CSV定義読込 |

**Key Variable Categories**:

| Category | Count | Description |
|----------|:-----:|-------------|
| FLAG | 66 | グローバルフラグ |
| CFLAG | 489 | キャラクターフラグ |
| TFLAG | 80 | ターン一時フラグ |
| ABL | 150 | 能力値 |
| TALENT | 50 | 素質 |
| PALAM | 30 | パラメータ |
| EXP | 20 | 経験値 |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=Variables"
# All variable operations match legacy behavior
```

**Next**: Create Phase 6 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 6: Ability & Training Foundation

**Phase Status**: DONE

**Goal**: 能力成長・訓練システムの基盤移行（Counter System の前提条件）

**Background**: 訓練システムは ABL/EXP の成長ロジックに依存。Phase 5 (Variables) 完了後、Phase 20 (Counter) の前に必要。

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **原則（YAGNI/KISS優先）**:
> - **やること**: static class禁止、Strongly Typed ID使用、Result型使用
> - **やらないこと**: 行数による機械的分割、将来使うかもしれない抽象化
> - 詳細は [F377 Design Principles](../feature-377.md#design-principles) 参照
>
> **Strongly Typed IDs**:
> - `AbilityId` - 能力値参照（ABL配列インデックス）
> - `TalentId` - 素質参照（TALENT配列インデックス）
> - `ExperienceId` - 経験値参照（EXP配列インデックス）
>
> **インターフェース定義**:
> ```csharp
> public interface IAbilitySystem
> {
>     Result<int> GetAbility(CharacterId character, AbilityId ability);
>     Result<bool> HasTalent(CharacterId character, TalentId talent);
>     void ApplyGrowth(CharacterId character, GrowthResult growth);
> }
>
> public interface ITrainingValidator
> {
>     Result<TrainingValidation> Validate(CharacterId target, CommandId command);
> }
> ```
>
> **DI登録**:
> - `IAbilitySystem` → `AbilitySystem`
> - `ITrainingValidator` → `TrainingValidator`
>
> **責務分割**（複数責務の場合のみ）:
> - ABL.ERB: 「能力取得」と「成長計算」が別責務なら分割、そうでなければ単一クラス
> - TRACHECK*.ERB: 検証種別が独立した責務なら分割

**Tasks**:
1. ✅ ABL.ERB 移行（能力システムコア）→ **F392 DONE**
2. ✅ ABL_UP_DATA.ERB 移行（能力成長データ）→ **F392 DONE**
3. ✅ ABLUP.ERB 移行（能力上昇処理）→ **F392 DONE**
4. ✅ TRACHECK*.ERB 移行（訓練検証 4種）→ **F393 DONE**
5. BEFORETRAIN.ERB 移行（訓練前処理）→ F394
6. AFTERTRA.ERB 移行（訓練後処理）→ F394
7. **Create Phase 6 Post-Phase Review feature** (type: infra) → F397 - 本ドキュメント Phase 6 セクションとの整合確認必須
8. **Create Phase 7 Planning feature** (type: research, include transition feature tasks) → F398

**Source Analysis**:

| File | Lines | Purpose | Complexity |
|------|:-----:|---------|:----------:|
| `ABL.ERB` | ~800 | 能力システムコア | **HIGH** |
| `ABL_UP_DATA.ERB` | ~500 | 成長データ定義 | HIGH |
| `ABLUP.ERB` | ~400 | 能力上昇計算 | Medium |
| `TRACHECK.ERB` | ~600 | 訓練検証メイン | HIGH |
| `TRACHECK_ABLUP.ERB` | ~300 | 能力成長チェック | Medium |
| `TRACHECK_EQUIP.ERB` | ~200 | 装備チェック | Medium |
| `TRACHECK_ORGASM.ERB` | ~200 | 絶頂チェック | Medium |
| `BEFORETRAIN.ERB` | ~400 | 訓練前セットアップ | Medium |
| `AFTERTRA.ERB` | ~300 | 訓練後クリーンアップ | Medium |

**Deliverables**:

| Component | Responsibility | Status |
|-----------|----------------|:------:|
| `src/Era.Core/Ability/IAbilitySystem.cs` | 能力システムインターフェース | ✅ F392 |
| `src/Era.Core/Ability/AbilitySystem.cs` | 能力システム実装 | ✅ F392 |
| `src/Era.Core/Ability/AbilityGrowth.cs` | 成長計算 | ✅ F392 |
| `src/Era.Core/Ability/GrowthData.cs` | 成長データ定義 | ✅ F392 |
| `src/Era.Core/Training/ITrainingProcessor.cs` | 訓練プロセッサインターフェース | ✅ F393 |
| `src/Era.Core/Training/TrainingProcessor.cs` | 訓練プロセッサ実装 | ✅ F393 |
| `src/Era.Core/Training/IBasicChecksProcessor.cs` | 基本チェックインターフェース | ✅ F393 |
| `src/Era.Core/Training/BasicChecksProcessor.cs` | 基本チェック実装 | ✅ F393 |
| `src/Era.Core/Training/TrainingSetup.cs` | 前後処理 | F394 |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=Ability"
# All ability growth calculations match legacy behavior
```

**Next**: Create Phase 7 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 7: Technical Debt Consolidation (NEW)

**Phase Status**: DONE

**Goal**: Phase 5-6で蓄積した技術負債の解消 + Phase 8以降の基盤確立

**Background**: Phase 6 (Training) で IVariableStore が22メソッドに肥大化し、ISP違反が発生。Callback injection パターンがDI未登録のまま使用されている。Phase 8 (Expression & Function) 以降の安定した基盤のため、ここで解消する。

> **⚠️ Phase 4 継承要件（必須）**
>
> **ISP適用対象**:
> - `IVariableStore` (22 methods) → 専門インターフェースに分割
>
> **DI正式化対象**:
> - Callback injection パターン → Factory registration
>
> **技術負債ゼロ原則**:
> - LegacyStateChange 完全削除
> - OrgasmProcessor/EquipmentProcessor 完成

**Tasks** → **Feature Mapping**:

| Task | Feature | Scope |
|------|:-------:|-------|
| 1. IVariableStore Interface Segregation | F404 | ISP準拠分割 |
| 2. Callback Injection DI Formalization | F405 | Factory登録 |
| 3a. StateChange - Training namespace | F402 | Training (Phase 6 継続) |
| 3b. StateChange - Character namespace | F403 | Character namespace |
| 4. Equipment/OrgasmProcessor 完成 | F406 | Processor completion |
| 5. Training Integration Tests | F407 | Integration tests |
| 6. Phase 7 Post-Phase Review (type: infra) | F408 | Phase 7 review |
| 7. Phase 8 Planning (type: research) | F409 | Next phase planning |
| 8. TrainingStateProcessor (Phase 6 残課題) | F410 | StateChange unification |
| 9. ISP Consumer Migration (CANCELLED) | F411 | Absorbed by F404 |
| 10. TEQUIP/CDOWN Variable Accessor | F412 | Variable accessor addition |
| 11. AbilityGrowthProcessor Bug Fix | F413 | TalentIndex bug fix |
| 12. F419 Callback Unification | F414 | Callback pattern unification |
| 13. Callback Factory Investigation | F415 | Result<T> error handling |

**Interface Segregation Design** (F408 Post-Phase Review で実装確認済み):

```csharp
// Before: Monolithic IVariableStore (34 methods)
public interface IVariableStore
{
    // 34 methods mixed together...
}

// After: 5 Specialized interfaces (ISP compliant)
public interface IVariableStore  // Core (14 methods)
{
    int GetFlag(FlagIndex index);
    void SetFlag(FlagIndex index, int value);
    int GetTFlag(FlagIndex index);
    void SetTFlag(FlagIndex index, int value);
    Result<int> GetCharacterFlag(CharacterId character, CharacterFlagIndex index);
    void SetCharacterFlag(CharacterId character, CharacterFlagIndex index, int value);
    Result<int> GetAbility(CharacterId character, AbilityIndex index);
    void SetAbility(CharacterId character, AbilityIndex index, int value);
    Result<int> GetTalent(CharacterId character, TalentIndex index);
    void SetTalent(CharacterId character, TalentIndex index, int value);
    Result<int> GetPalam(CharacterId character, PalamIndex index);
    void SetPalam(CharacterId character, PalamIndex index, int value);
    Result<int> GetExp(CharacterId character, ExpIndex index);
    void SetExp(CharacterId character, ExpIndex index, int value);
}

public interface ITrainingVariables  // Training-specific (6 methods)
{
    Result<int> GetBase(CharacterId character, BaseIndex index);
    void SetBase(CharacterId character, BaseIndex index, int value);
    Result<int> GetTCVar(CharacterId character, TCVarIndex index);
    void SetTCVar(CharacterId character, TCVarIndex index, int value);
    Result<int> GetCup(CharacterId character, CupIndex index);
    void SetCup(CharacterId character, CupIndex index, int value);
}

public interface ICharacterStateVariables  // State tracking (12 methods) - F412 expanded
{
    Result<int> GetSource(CharacterId character, SourceIndex index);
    void SetSource(CharacterId character, SourceIndex index, int value);
    Result<int> GetMark(CharacterId character, MarkIndex index);
    void SetMark(CharacterId character, MarkIndex index, int value);
    Result<int> GetNowEx(CharacterId character, NowExIndex index);
    void SetNowEx(CharacterId character, NowExIndex index, int value);
    Result<int> GetMaxBase(CharacterId character, MaxBaseIndex index);
    void SetMaxBase(CharacterId character, MaxBaseIndex index, int value);
    Result<int> GetCDown(CharacterId character, PalamIndex index);  // F412
    void SetCDown(CharacterId character, PalamIndex index, int value);
    Result<int> GetEx(CharacterId character, ExIndex index);  // F412
    void SetEx(CharacterId character, ExIndex index, int value);
}

public interface IJuelVariables  // Juel system (6 methods)
{
    Result<int> GetJuel(CharacterId character, int index);
    void SetJuel(CharacterId character, int index, int value);
    Result<int> GetGotJuel(CharacterId character, int index);
    void SetGotJuel(CharacterId character, int index, int value);
    Result<int> GetPalamLv(int index);  // Non-character-scoped
    void SetPalamLv(int index, int value);
}

public interface ITEquipVariables  // Equipment flags (2 methods) - F412 NEW
{
    Result<int> GetTEquip(CharacterId character, int equipmentIndex);
    void SetTEquip(CharacterId character, int equipmentIndex, int value);
}
```

**Callback DI Formalization**:

```csharp
// src/Era.Core/DependencyInjection/CallbackFactories.cs
public static class CallbackFactories
{
    public static IServiceCollection AddTrainingCallbacks(this IServiceCollection services)
    {
        // CUP array accessor for MarkSystem
        services.AddSingleton<Func<CharacterId, CupIndex, int>>(sp =>
        {
            var vars = sp.GetRequiredService<ITrainingVariables>();
            return (character, cupIndex) => vars.GetCup(character, cupIndex) switch
            {
                { IsSuccess: true } r => r.Value,
                _ => 0
            };
        });

        // TEQUIP accessor for VirginityManager
        services.AddSingleton<Func<CharacterId, int, bool>>(sp =>
        {
            var vars = sp.GetRequiredService<ITEquipVariables>();
            return (character, index) => vars.GetTEquip(character, index) switch
            {
                { IsSuccess: true } r => r.Value != 0,
                _ => false
            };
        });

        return services;
    }
}
```

**Deliverables**:

| Component | Responsibility | Feature |
|-----------|----------------|:-------:|
| `src/Era.Core/Variables/ITrainingVariables.cs` | Training変数インターフェース | F404 |
| `src/Era.Core/Variables/ICharacterStateVariables.cs` | State変数インターフェース | F404 |
| `src/Era.Core/Variables/IJuelVariables.cs` | Juel変数インターフェース | F404 |
| `src/Era.Core/DependencyInjection/CallbackFactories.cs` | Callback DI登録 | F405 |
| `src/Era.Core/Training/StateChange.cs` | Training namespace完成 | F402 |
| `src/Era.Core/Character/*.cs` | Character namespace StateChange | F403 |
| `src/Era.Core/Training/EquipmentProcessor.cs` | 装備処理完成 | F406 |
| `src/Era.Core/Training/OrgasmProcessor.cs` | 絶頂処理完成 | F406 |
| `src/Era.Core.Tests/Training/TrainingIntegrationTests.cs` | 統合テスト | F407 |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=Training"
dotnet test Era.Core.Tests --filter "Category=Integration"
# All tests pass with new interface structure
```

**Success Criteria** (F408 Post-Phase Review で検証済み):
- [x] IVariableStore が ISP 準拠（5インターフェース分割）
- [x] Callback injection が DI 正式登録
- [x] StateChange 12サブタイプ完成
- [x] LegacyStateChange 完全削除
- [x] OrgasmProcessor/EquipmentProcessor 100% 実装
- [x] Training 統合テスト追加

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 7: Technical Debt Consolidation」を継承。個別貢献（ISP/DI/Processor等）を明記 | Grep "Technical Debt Consolidation" |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains 検証 |
| **Tasks: 移行完了** | 既存 consumer の移行タスクを含む（ad-hoc → DI等） | AC に移行検証 |
| **Tasks: 等価性検証** | ERB 元実装との等価性テストを含む（processor系） | AC にテストファイル存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧に負債検証あり |

> **等価性検証 Scope**: 等価性検証は ERB migration features（既存 ERB ファイルを C# に移行する場合）にのみ適用。新規 C# 実装（レガシー ERB なし）は対象外。Architecture Review (Phase 15) も新たなドキュメント作成のため対象外。

**Checklist for Planning Feature** (F398等が sub-feature 作成時に確認):
1. [ ] Philosophy に Phase 統一思想を継承したか
2. [ ] Tasks に負債解消タスク（TODO/FIXME削除）を含めたか
3. [ ] Tasks に既存 consumer 移行タスクを含めたか
4. [ ] AC に負債ゼロ検証を含めたか
5. [ ] AC に ERB 等価性検証を含めたか（該当する場合）

**Next**: Create Phase 8 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 8: Expression & Function System

**Phase Status**: DONE

**Goal**: 式評価と組み込み関数の移行

**Background**: ERB の数式・条件式を評価するシステム。100+ 組み込み関数を含む。

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **原則（YAGNI/KISS優先）**:
> - **やること**: static class禁止、Strongly Typed ID使用、Result型使用
> - **やらないこと**: 行数による機械的分割、将来使うかもしれない抽象化
> - 詳細は [F377 Design Principles](../feature-377.md#design-principles) 参照
>
> **インターフェース定義**:
> ```csharp
> public interface IExpressionEvaluator
> {
>     Result<object> Evaluate(Expression expr, IEvaluationContext context);
> }
>
> public interface IFunctionRegistry
> {
>     Result<IBuiltInFunction> GetFunction(string name);
>     void Register(string name, IBuiltInFunction function);
> }
> ```
>
> **DI登録**:
> - `IExpressionEvaluator` → `ExpressionEvaluator`
> - `IFunctionRegistry` → `FunctionRegistry`
>
> **Result型使用**:
> - 式評価失敗 → `Result.Fail("Division by zero")` 等
> - 未定義関数呼び出し → `Result.Fail("Function not found")`
>
> **責務分割**（複数責務の場合のみ）:
> - 100+ 組み込み関数: カテゴリが異なる責務なら分割（Math/String/Array等）、そうでなければFunctionRegistryに集約

**Tasks** → **Feature Mapping**:

| Task | Feature | Scope |
|------|:-------:|-------|
| 1. ExpressionParser 移行（AST 生成）| F416-F418 | Expression parsing |
| 2. 演算子実装（30+ 演算子）| F419 | Operators |
| 3a. 組み込み関数実装 - String拡張 | F425 | String functions (STRFIND, REPLACE, etc.) |
| 3b. 組み込み関数実装 - 比較関数 | F426 | Value comparison (MAX, MIN, LIMIT, etc.) |
| 3c. 組み込み関数実装 - エンジン依存 | F428 | Engine-dependent functions (ISSKIP, etc.) |
| 3d. 共通化リファクタリング (ShiftJisHelper) | F427 | DRY refactoring during F419 |
| 4. 関数呼び出しメカニズム | F420 | Function call mechanism |
| 5. 型変換・キャスト | F421-F422 | Type conversion |
| 6. Post-Phase Review (type: infra) | F423 | Phase 8 review |
| 7. Phase 9 Planning (type: research) | F424 | Next phase planning |
| 8. Repository Merge Infrastructure | F438 | engine submodule統合（Phase 9 開始前推奨）|

**Source Analysis**:

| Current File | Lines | Purpose |
|--------------|:-----:|---------|
| `ExpressionParser.cs` | ~500 | 式パース |
| `ExpressionMediator.cs` | ~300 | 式評価 |
| `OperatorMethod.cs` | ~400 | 演算子実装 |
| `FunctionMethod.cs` | ~800 | 組み込み関数 |

**Operator Categories**:

| Category | Operators | Count |
|----------|-----------|:-----:|
| Arithmetic | `+`, `-`, `*`, `/`, `%`, `**` | 6 |
| Comparison | `==`, `!=`, `<`, `>`, `<=`, `>=` | 6 |
| Logical | `&&`, `\|\|`, `!`, `^` | 4 |
| Bitwise | `&`, `\|`, `~`, `<<`, `>>` | 5 |
| String | `+` (concat), `*` (repeat) | 2 |
| Ternary | `? :` | 1 |

**Built-in Function Categories**:

| Category | Examples | Count |
|----------|----------|:-----:|
| Math | ABS, MAX, MIN, POWER, SQRT, LOG | 15+ |
| String | SUBSTRING, LENGTH, FIND, REPLACE, TOSTR | 20+ |
| Array | ARRAYSIZE, ARRAYSEARCH, ARRAYCOPY, ARRAYSHIFT | 15+ |
| Random | RAND, RANDSELECT | 5+ |
| Conversion | TOINT, TOSTR, ISNUMERIC | 10+ |
| Character | GETPALAM, GETEXP, GETTARGET | 20+ |
| System | GETTIME, GETMILLISECOND | 5+ |

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Expressions/ExpressionParser.cs` | 式パース |
| `src/Era.Core/Expressions/ExpressionEvaluator.cs` | 式評価 |
| `src/Era.Core/Expressions/Operators.cs` | 演算子実装 |
| `src/Era.Core/Functions/BuiltInFunctions.cs` | 組み込み関数 |
| `src/Era.Core/Functions/FunctionRegistry.cs` | 関数登録 |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=Expressions"
# Expression evaluation matches legacy behavior
```

**Success Criteria**:
- [ ] ExpressionParser が AST 生成
- [ ] 30+ 演算子が実装済み
- [ ] 100+ 組み込み関数が実装済み
- [ ] FunctionRegistry が DI 登録済み
- [ ] 式評価が legacy と等価

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 8: Expression & Function System」を継承。個別貢献を明記 | Grep "Expression & Function" |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains 検証 |
| **Tasks: 等価性検証** | legacy 実装との等価性テストを含む | AC にテストファイル存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧に負債検証あり |

**Checklist for Planning Feature** (F409等が sub-feature 作成時に確認):
1. [ ] Philosophy に Phase 統一思想を継承したか
2. [ ] Tasks に負債解消タスク（TODO/FIXME削除）を含めたか
3. [ ] AC に負債ゼロ検証を含めたか
4. [ ] AC に legacy 等価性検証を含めたか

**Next**: Create Phase 9 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 9: Command Infrastructure + Mediator Pipeline

**Phase Status**: DONE

**Goal**: コマンドシステムの移行（60+ コマンド + 16 SCOMF）+ **Mediator Pattern導入**

**Background**: ERB スクリプトの各命令（PRINT, IF, FOR, CALL 等）を実行するシステム。**Pipeline Behaviorsで横断的関心事を統一的に処理。**

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **原則（YAGNI/KISS優先）**:
> - **やること**: static class禁止、Strongly Typed ID使用、Result型使用
> - **やらないこと**: 行数による機械的分割、将来使うかもしれない抽象化
> - 詳細は [F377 Design Principles](../feature-377.md#design-principles) 参照
>
> **Strongly Typed IDs**:
> - `CommandId` - コマンド識別
>
> **インターフェース定義**:
> ```csharp
> public interface ICommand<TResult>
> {
>     CommandId Id { get; }
> }
>
> public interface ICommandHandler<TCommand, TResult>
>     where TCommand : ICommand<TResult>
> {
>     Task<Result<TResult>> Handle(TCommand command, CancellationToken ct);
> }
>
> public interface ICommandDispatcher
> {
>     Task<Result<TResult>> Dispatch<TResult>(ICommand<TResult> command, CancellationToken ct = default);
> }
>
> // 🆕 Pipeline Behavior (横断的関心事)
> public interface IPipelineBehavior<TCommand, TResult>
>     where TCommand : ICommand<TResult>
> {
>     Task<Result<TResult>> Handle(TCommand request,
>         Func<Task<Result<TResult>>> next, CancellationToken ct);
> }
> ```
>
> **DI登録**:
> - `ICommandDispatcher` → `CommandDispatcher`
> - `IPipelineBehavior<,>` → 複数登録（順序重要）
>
> **責務分割**（複数責務の場合のみ）:
> - 60+ コマンド: 各コマンドは `ICommandHandler` 実装クラス（1コマンド=1責務なので自然に分離）
> - カテゴリ別サブディレクトリ（`Print/`, `Flow/`）は整理目的であり、責務分割ではない

**Tasks** → **Feature Mapping**:

| Task | Feature | Scope |
|------|:-------:|-------|
| 1-3. CommandDispatcher + Interfaces + IPipelineBehavior | F429 | Architecture foundation |
| 4-5. LoggingBehavior + ValidationBehavior + TransactionBehavior | F430 | Pipeline Behaviors |
| 6a. Print Commands (15+) | F431 | PRINT, PRINTL, PRINTW, PRINTFORM, PRINTDATA |
| 6b. Flow Control Commands (18+) | F432 | IF/FOR/WHILE/CALL/RETURN |
| 6c. Variable & Array Commands (9+) | F433 | LET, VARSET, ARRAYCOPY |
| 6d. System Commands (16+) | F434 | Character/Style/System |
| 7. 実行コンテキスト管理 (CommandContext) | F429 | Included in F429 |
| 8. フロー制御（IF/FOR/WHILE/CALL/RETURN）| F432 | Included in F432 |
| 8.5. GameInitialization GlobalStatic accessor migration | F434 | 3 TODOs (Phase 7 引き継ぎ) → **Deferred to Phase 12** |
| 9. SCOMF*.ERB 16ファイル移行 | F435 | Special training commands |
| 10. Post-Phase Review (type: infra) | F436 | Review F429-F435 |
| 11. Phase 10 Planning (type: research) | F437 | Next phase sub-features |
| 12. Fix Era.Core compiler warnings (CS0652, CS8625) | F440 | Phase 8-9 technical debt |

**Pipeline Behaviors（横断的関心事）**:

| Behavior | 責務 | 順序 |
|----------|------|:----:|
| `LoggingBehavior` | コマンド実行ログ | 1 |
| `ValidationBehavior` | 入力バリデーション | 2 |
| `TransactionBehavior` | UoWトランザクション管理 | 3 |

```csharp
// src/Era.Core/Commands/Behaviors/LoggingBehavior.cs
public class LoggingBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ILogger<LoggingBehavior<TCommand, TResult>> _logger;

    public async Task<Result<TResult>> Handle(TCommand request,
        Func<Task<Result<TResult>>> next, CancellationToken ct)
    {
        var commandName = typeof(TCommand).Name;
        _logger.LogInformation("Executing {Command}", commandName);
        var stopwatch = Stopwatch.StartNew();

        var result = await next();

        _logger.LogInformation("Completed {Command} in {Elapsed}ms: {Success}",
            commandName, stopwatch.ElapsedMilliseconds, result is Result<TResult>.Success);
        return result;
    }
}
```

**Source Analysis**:

| Current Directory | Files | Purpose |
|-------------------|:-----:|---------|
| `GameProc/Commands/` | 60+ | コマンド実装 |
| `CommandRegistry.cs` | 1 | コマンド登録 |
| `Game/ERB/SCOMF*.ERB` | 16 | 特殊コマンド |

**Command Categories**:

| Category | Commands | Count |
|----------|----------|:-----:|
| Print | PRINT, PRINTL, PRINTW, PRINTFORM, PRINTDATA | 15+ |
| Flow | IF, ELSEIF, ELSE, ENDIF, FOR, NEXT, WHILE, WEND | 10+ |
| Call | CALL, CALLFORM, RETURN, GOTO, JUMP | 8+ |
| Variable | LET, VARSET, VARSIZE | 5+ |
| Array | ARRAYCOPY, ARRAYREMOVE, ARRAYSHIFT, ARRAYSORT | 4 |
| Character | ADDCHARA, DELCHARA, PICKUPCHARA | 5+ |
| Style | SETCOLOR, SETFONT, ALIGNMENT | 6+ |
| System | SAVEGAME, LOADGAME, QUIT | 5+ |

**SCOMF (Special Commands)**:

| File | Purpose |
|------|---------|
| SCOMF1.ERB - SCOMF16.ERB | 特殊訓練コマンド（ゲーム固有ロジック） |

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Commands/ICommand.cs` | コマンドインターフェース |
| `src/Era.Core/Commands/CommandRegistry.cs` | コマンド登録 |
| `src/Era.Core/Commands/CommandContext.cs` | 実行コンテキスト |
| `src/Era.Core/Commands/Print/*.cs` | 出力コマンド群 |
| `src/Era.Core/Commands/Flow/*.cs` | フロー制御群 |
| `src/Era.Core/Commands/Array/*.cs` | 配列操作群 |
| `src/Era.Core/Commands/Special/*.cs` | **SCOMF特殊コマンド群** |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=Commands"
# All command execution matches legacy behavior
```

**Success Criteria**:
- [x] 60+ コマンドが実装済み
- [x] 16 SCOMF が実装済み
- [x] CommandRegistry が DI 登録済み
- [x] Mediator Pipeline が機能
- [x] コマンド実行が legacy と等価

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 9: Command Infrastructure」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **Tasks: 等価性検証** | legacy 実装との等価性テストを含む | AC にテスト存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Technical Debt Deferred to Phase 9** (from Phase 7 Review):

> **⚠️ Phase 7 引き継ぎ事項**: GameInitialization の GlobalStatic accessor TODOs

| File | Location | Current | Target |
|------|----------|---------|--------|
| `src/Era.Core/Common/GameInitialization.cs` | Line 319 | TODO: Replace with GlobalStatic | `GlobalStatic.GetFlag()` accessor |
| `src/Era.Core/Common/GameInitialization.cs` | Line 339 | TODO: Replace with GlobalStatic | `GlobalStatic.GetCFlag()` accessor |
| `src/Era.Core/Common/GameInitialization.cs` | Line 358 | TODO: Replace with GlobalStatic | `GlobalStatic.GetTFlag()` accessor |

**Rationale**: Phase 9 introduces Command + Mediator Pipeline which standardizes accessor patterns. GlobalStatic accessors should be implemented as part of the unified accessor strategy.

**Tasks Addition**:
- Task 8.5: **GameInitialization GlobalStatic accessor migration** (3 TODOs)

**Next**: Create Phase 10 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

**Phase 9 Implementation Notes:**

**Completed**: 2026-01-11

**Implementation Decisions**:
- CommandDispatcher with Mediator pipeline provides clean separation of concerns
- Pipeline behaviors (Logging→Validation→Transaction) ensure consistent command execution
- SCOMF special commands (1-16) migrated as individual handler classes
- REPEAT/REND and GOTO/JUMP label commands complete the flow control set

**Deferred Items**:
- None - all Phase 9 Success Criteria met

**Lessons Learned**:
- Breaking commands into individual handlers improves testability (148 equivalence tests)
- DI registration pattern scales well (16 SCOMF handlers registered cleanly)
- Pipeline ordering is critical for proper behavior composition

**Test Coverage**:
- 105 Category=Commands tests passing
- 16 SCOMF-specific tests passing
- 148 equivalence tests verifying legacy compatibility
- 3 pipeline ordering tests validating behavior sequence

### Phase 10: Runtime Upgrade (NEW)

**Phase Status**: DONE

**Goal**: .NET 8 → .NET 10, C# 12 → C# 14 アップグレード

**Background**: Era.Core開発が進行中（Phase 9完了時点）。Phase 12以降のCOM実装・DDD基盤でC# 14の新機能（primary constructors, extension members等）を活用するため、早期にランタイムをアップグレード。

**Scope**: Era.Core + tools + Headless（Unity GUI 除外）

| Project | Current | Target |
|---------|---------|--------|
| Era.Core | net8.0 / C# 12 | net10.0 / C# 14 |
| Era.Core.Tests | net8.0 | net10.0 |
| engine/uEmuera.Headless | net8.0 / C# 12 | net10.0 / C# 14 |
| engine.Tests | net8.0 | net10.0 |
| tools/* | net8.0 | net10.0 |
| **Unity GUI** | Unity 6 | **変更なし** |

**Tasks**:
1. TargetFramework を net10.0 に変更（6プロジェクト）
2. LangVersion を 14 に設定
3. NuGet パッケージ更新（下記詳細参照）
4. ビルド・テスト確認
5. C# 14 skill 作成 (`.claude/skills/csharp-14.md`)
6. Type Design Guidelines 更新（C# 14パターン追加）
7. engine-dev skill から csharp-14 skill への参照追加
8. **Create Phase 10 Post-Phase Review feature** (type: infra)
9. **Create Phase 11 Planning feature** (type: research)

**NuGet Package Updates (Task 3 詳細)**:

> **調査日**: 2026-01-10

| パッケージ | 現行 | Target | 備考 |
|-----------|------|--------|------|
| Microsoft.NET.Test.Sdk | 17.11.1 / 17.8.0 | **18.0.1** | .NET 10 対応必須 |
| Microsoft.Extensions.DependencyInjection | 10.0.1 | 10.0.x | 維持 |
| Microsoft.Extensions.DependencyInjection.Abstractions | 8.0.0 | **10.0.x** | .NET 10 整合 |
| Microsoft.Extensions.Logging.Abstractions | 8.0.0 | **10.0.x** | .NET 10 整合 |
| System.Text.Encoding.CodePages | 8.0.0 | **10.0.x** | .NET 10 整合 |
| System.Text.Json | 8.0.5 | **10.0.x** | .NET 10 整合 |
| YamlDotNet | 16.2.1 / 15.1.0 | 16.2.1+ | バージョン統一 |
| NJsonSchema | 11.1.0 / 11.0.0 | 11.1.0+ | バージョン統一 |
| xunit | 2.9.2 / 2.6.2 | 2.9.x | v2 維持（v3 は別 Phase） |
| xunit.runner.visualstudio | 2.8.2 / 2.5.4 | 2.8.x | バージョン統一 |
| coverlet.collector | 6.0.2 / 6.0.0 | 6.0.2 | バージョン統一 |
| Moq | 4.20.70 | 4.20.x | 維持 |

**既存バージョン不整合** (Phase 10 前に解消推奨):

| プロジェクト | 問題 |
|-------------|------|
| uEmuera.Tests | Test.Sdk 17.8.0, xunit 2.6.2 (古い) |
| ErbLinter.Tests | Test.Sdk 17.8.0, xunit 2.6.2 (古い) |
| YamlValidator | YamlDotNet 15.1.0, NJsonSchema 11.0.0 (古い) |

**xUnit v3 移行について**:

xUnit v3 (3.2.0) は .NET 10 最適対応だが、**破壊的変更** を含む。
本 Phase では v2 を維持し、v3 移行は別フェーズ（後続挿入予定）で実施。

**C# 14 Key Features to Document**:

| Feature | Usage in Era.Core |
|---------|-------------------|
| **Extension members** | IGameContext, IVariableStore 拡張 |
| **Primary constructors** | DI対象クラス（TrainingProcessor等）の簡潔化 |
| **field keyword** | プロパティのバッキングフィールド省略 |
| **Null-conditional assignment** | `?.` への代入 |
| **Partial constructors** | Source generator 連携 |

**Type Design Guidelines Additions** (Phase 4 セクションに追記):

```csharp
// C# 14: Primary Constructor (DI推奨パターン)
public class TrainingProcessor(
    IBasicChecksProcessor basicChecks,
    IAbilityGrowthProcessor abilityGrowth) : ITrainingProcessor
{
    public Result<TrainingResult> Process(...) => ...
}

// C# 14: Extension Members
extension GameContextExtensions for IGameContext
{
    public bool IsMainCharacter(CharacterId id)
        => id == CharacterId.You || id == CharacterId.Meiling;

    public static IGameContext Empty => new NullGameContext();
}

// C# 14: field keyword
public string Name
{
    get => field;
    set => field = value?.Trim() ?? throw new ArgumentNullException();
}
```

**Verification**:
```bash
dotnet build Era.Core
dotnet build engine/uEmuera.Headless.csproj
dotnet test Era.Core.Tests
dotnet test engine.Tests
# 全プロジェクトビルド成功、全テストPASS
```

**Success Criteria**:
- [x] 全プロジェクト .NET 10 ビルド成功
- [x] 全テスト PASS
- [x] NuGet パッケージ統一完了（バージョン不整合解消）
- [x] C# 14 skill 作成完了
- [x] Type Design Guidelines 更新完了

### Phase 10 Implementation Notes:

**Completion Date**: 2026-01-11

**Features**:
- F444: .NET 10 / C# 14 Core Upgrade - [DONE]
- F445: C# 14 Documentation - [DONE]

**Success Criteria Status**:
- [x] All projects build with .NET 10
- [x] All tests pass
- [x] NuGet packages unified to .NET 10 compatible versions
- [x] C# 14 skill created (.claude/skills/csharp-14/SKILL.md)
- [x] Type Design Guidelines updated with C# 14 patterns

**Implementation Notes**:
- 6 projects upgraded: Era.Core, Era.Core.Tests, uEmuera.Headless, engine.Tests, tools/* (ErbParser, ErbToYaml, KojoComparer, YamlSchemaGen, YamlValidator and their test projects)
- Existing version inconsistencies resolved (uEmuera.Tests, ErbLinter.Tests, YamlValidator)
- Unity GUI excluded from upgrade (remains on Unity 6 / .NET Framework)
- All 1246 tests pass successfully (Era.Core.Tests: 773, uEmuera.Tests: 382, ErbParser.Tests: 65, ErbToYaml.Tests: 10, KojoComparer.Tests: 12, YamlSchemaGen.Tests: 4)
- Microsoft.NET.Test.Sdk unified to version 18.0.1 across all test projects

**Deferred Items**: None

**Phase 11 transition approved**: 2026-01-11

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 10: Runtime Upgrade」を継承 | Grep |
| **Tasks: スキル作成** | csharp-14.md skill 作成を含む | ファイル存在確認 |
| **AC: ビルド検証** | 全プロジェクトビルド成功を検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 11 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 11: xUnit v3 Migration (NEW)

**Phase Status**: DONE

**Goal**: xUnit v2 → v3 移行（破壊的変更対応）

**Background**: xUnit v3 (3.2.0) は .NET 10 に最適化されているが、v2 から API 変更を含む。Phase 10 で .NET 10 アップグレード完了後、安定した状態で移行を実施。

**Scope**: 6 active test projects (ErbLinter.Tests excluded - archived, .NET 8.0)

| Project | Current | Target |
|---------|---------|--------|
| Era.Core.Tests | xunit 2.9.x | xunit.v3 3.2.x |
| uEmuera.Tests | xunit 2.9.x | xunit.v3 3.2.x |
| ErbParser.Tests | xunit 2.9.x | xunit.v3 3.2.x |
| ErbToYaml.Tests | xunit 2.9.x | xunit.v3 3.2.x |
| KojoComparer.Tests | xunit 2.9.x | xunit.v3 3.2.x |
| YamlSchemaGen.Tests | xunit 2.9.x | xunit.v3 3.2.x |

**Excluded**: ErbLinter.Tests (tools/_archived/, .NET 8.0) - remains on xUnit v2

**Tasks**:
1. xUnit v3 移行ガイド調査・影響分析
2. パッケージ参照変更 (`xunit` → `xunit.v3`)
3. xunit.runner.visualstudio 3.x へ更新
4. テストコード修正（API 変更対応）
5. MTP (Microsoft Test Platform) v2 対応確認
6. 全テスト実行・PASS 確認
7. **Create Phase 11 Post-Phase Review feature** (type: infra)
8. **Create Phase 12 Planning feature** (type: research)
9. xUnit1051 警告対応 - `TestContext.Current.CancellationToken` 使用 (from F448)

**xUnit v3 Breaking Changes**:

| 変更点 | v2 | v3 | 対応 |
|--------|----|----|------|
| パッケージ名 | `xunit` | `xunit.v3` | 参照変更 |
| Runner | `xunit.runner.visualstudio` 2.x | `xunit.runner.visualstudio` 3.x | バージョン変更 |
| Assert API | 一部変更 | 新 API | コード修正 |
| Theory data | `MemberData` 等 | 拡張 | 確認・修正 |

**References**:
- [xUnit v3 Release Notes](https://xunit.net/releases/v3/3.2.0)
- [xUnit v3 Migration Guide](https://xunit.net/docs/getting-started/v3/migration)

**Verification**:
```bash
dotnet test Era.Core.Tests
dotnet test engine.Tests
dotnet test tools/ErbParser.Tests
dotnet test tools/ErbToYaml.Tests
dotnet test tools/KojoComparer.Tests
# 全テストPASS
```

**Success Criteria**:
- [x] 全テストプロジェクト xUnit v3 移行完了
- [x] 全テスト PASS
- [x] MTP v2 対応確認

### Phase 11 Implementation Notes:

**Completion Date**: 2026-01-11

**Features**:
- F448: xUnit v3 Migration - [DONE]

**Success Criteria Status** (English translation of architecture.md Japanese criteria):
- [x] All test projects use xUnit v3 (xunit.v3 package)
- [x] All tests pass with xUnit v3
- [x] MTP v2 compatibility verified

**Implementation Notes**:
- 6 active test projects migrated: Era.Core.Tests, uEmuera.Tests, ErbParser.Tests, ErbToYaml.Tests, KojoComparer.Tests, YamlSchemaGen.Tests
- Excluded: ErbLinter.Tests (tools/_archived/, .NET 8.0 - remains on xUnit v2)
- Package references updated: `xunit` → `xunit.v3` (3.2.x)
- Runner updated: `xunit.runner.visualstudio` 2.x → `xunit.runner.visualstudio` 3.x
- API changes handled per migration guide:
  - xUnit1051 warnings addressed by using `TestContext.Current.CancellationToken` in async tests
  - All Assert APIs verified compatible with v3
  - Theory data sources (MemberData) verified and working
- All 6 test projects build and pass successfully with xUnit v3

**Deferred Items**: None

**Phase 12 transition approved**: 2026-01-11

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 11: xUnit v3 Migration」を継承 | Grep |
| **AC: テスト PASS** | 全テストプロジェクトの PASS を検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 12 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 12: COM Implementation (was Phase 11)

**Phase Status**: DONE

**Goal**: COMコマンド実装の移行（150+ COMF ファイル）

**Background**: COMF*.ERB はゲームの訓練コマンド（COM_0〜COM_999）の実装本体。Phase 9 のコマンド基盤上で動作する。

**CRITICAL**: これがゲームロジックの最大部分。150+ ファイル、推定 50,000+ 行。

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **Strongly Typed IDs**:
> - `ComId` - COMコマンド番号（COM_0, COM_100, 等）
> - `ExpType`, `PalamType` - 経験値・パラメータ種別
>
> **インターフェース定義**:
> ```csharp
> public interface ICom
> {
>     ComId Id { get; }
>     string Name { get; }
>     Result<ComResult> Execute(IComContext context);
> }
>
> public interface IComContext
> {
>     CharacterId Target { get; }
>     CharacterId Actor { get; }
>     IAbilitySystem Abilities { get; }
>     IKojoEngine Kojo { get; }
> }
> ```
>
> **DI登録**:
> - `IComRegistry` → `ComRegistry` (150+ COM登録)
> - `IComContext` → `ComContext`
>
> **SRP分割**:
> - 各COMは1クラス1ファイル（セマンティック名: `ClitoralCap.cs`, not `Com42.cs`）
> - 共通ロジック → `ComBase` または専用ヘルパークラス
> - ゲームループ別ディレクトリ（`Daily/`, `Training/`, `Utility/`, `Masturbation/`, `Visitor/`, `System/`）
> - Training/内はアクション種別でサブ分類（`Touch/`, `Oral/`, `Equipment/`, 等）
> - レガシーID → `[ComId(N)]` 属性で保持

**Tasks**:
1. COMF ファイル分析・分類
2. COM 実装パターン抽出
3. C# クラス設計（継承/コンポジション）
4. 150+ COM の逐次移行
5. 各 COM の単体テスト
5.5. **COM Semantic Naming Refactoring** (F464) - ID-centric naming → semantic naming migration
6. **Create Phase 12 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 12 セクションとの整合確認必須
7. **Create Phase 13 Planning feature** (type: research, include transition feature tasks)

**Source Analysis**:

| Range | Files | Category |
|-------|:-----:|----------|
| COMF0-99 | ~20 | 基本行動（会話、移動等） |
| COMF100-199 | ~30 | 愛撫系（キス、タッチ等） |
| COMF200-299 | ~15 | 特殊行動 |
| COMF300-399 | ~20 | 会話系（会話拡張含む） |
| COMF400-499 | ~40 | 訓練系（メイン訓練） |
| COMF500-599 | ~10 | 追加訓練 |
| COMF600-699 | ~10 | 拡張行動 |
| COMF888, 999 | 2 | 特殊処理 |
| **Total** | **~150** | |

> **⚠️ F464 Directory Structure**: 上記のID範囲による分類はレガシー。
> 実際のディレクトリ構造はゲームループ別（`Daily/`, `Training/`, `Utility/`, `Masturbation/`, `Visitor/`, `System/`）。
> 詳細は [F464](../feature-464.md) 参照。

**Equipment Handler Migration (from F406 Deferred)**:

> **⚠️ F406 引き継ぎ事項**: EquipmentProcessor.ProcessEquipment が EQUIP_COM* を呼び出すが、
> これらの実装本体は COMF*.ERB 内に定義されている。Phase 12 で移行すること。

| ERB Function | Location | Purpose |
|--------------|----------|---------|
| EQUIP_COM42 | COMF42.ERB:55 | クリキャップ効果 |
| EQUIP_COM43 | COMF43.ERB:60 | オナホール効果 |
| EQUIP_COM44 | COMF44.ERB:89 | バイブ効果 |
| EQUIP_COM45 | COMF45.ERB | アナルバイブ効果 |
| EQUIP_COM46 | COMF46.ERB | アナルビーズ効果 |
| EQUIP_COM47 | COMF47.ERB | ニプルキャップ効果 |
| EQUIP_COM48 | COMF48.ERB | 搾乳機効果 |
| EQUIP_COM104 | COMF104.ERB | アイマスク効果 |
| EQUIP_COM105 | COMF105.ERB | 縄緊縛効果 |
| EQUIP_COM106 | COMF106.ERB | ボールギャグ効果 |
| EQUIP_COM146 | COMF146.ERB | 浣腸効果 |
| EQUIP_COM147 | COMF147.ERB | 拡張バルーン効果 |
| EQUIP_COM148 | COMF148.ERB | アナル電極効果 |
| EQUIP_COM183 | COMF183.ERB | ビデオ撮影効果 |
| EQUIP_COM184 | COMF184.ERB | 野外プレイ効果 |
| EQUIP_COM187 | COMF187.ERB | お風呂場プレイ効果 |
| EQUIP_COM188 | COMF188.ERB | シャワー効果 |
| EQUIP_COM189 | COMF189.ERB | 新妻プレイ効果 |

**C# Integration Pattern**:
```csharp
// src/Era.Core/Training/EquipmentProcessor.cs (F406で実装済み)
// 各 EQUIP_COM* は対応する ComXX クラスのメソッドとして移行
private void ProcessClitCap(CharacterId target, EquipmentResult result)
{
    // Phase 10 で実装: Com42.ExecuteEquipmentEffect(target) を呼び出す
}
```

**F407 Deferred AC (from Phase 7)**:

> **⚠️ F407 引き継ぎ事項**: TrainingIntegrationTests.EquipmentOrgasmInteraction テストは
> EquipmentProcessor ハンドラーがスタブのため full data flow 検証ができない。
> EQUIP_COM42-189 移行後にテスト拡充すること。

| AC | Test Method | Condition |
|----|-------------|-----------|
| F407-AC#6 | EquipmentOrgasmInteraction | After EQUIP_COM42-189 migration complete |

**Test Enhancement Task**:
```csharp
// src/Era.Core.Tests/TrainingIntegrationTests.cs
// Phase 12 完了後に以下を追加:
// 1. TEQUIP 値設定 → OrgasmProcessor 結果への影響検証
// 2. Equipment effect → SOURCE/PALAM 変化の data flow 検証
```

**Video Recording / TSTR Migration (from F406 Deferred)**:

> **⚠️ F406 引き継ぎ事項**: OrgasmProcessor (lines 90-91) でビデオ撮影処理がスキップされている。
> TSTR accessor 実装後に移行すること。

| Item | Dependency | Target |
|------|------------|--------|
| TSTR accessor | IVariableStore 拡張 | Phase 12 前提条件 |
| Video recording logic | TEQUIP:ビデオ撮影, TSTR:動画撮影記録 | EQUIP_COM183 移行時 |

**COM Implementation Pattern**:
```csharp
// src/Era.Core/Commands/Com/Com100_Kiss.cs
public class Com100_Kiss : ComBase
{
    public override int ComId => 100;
    public override string Name => "キス";

    public override ComResult Execute(ComContext ctx)
    {
        // Parameter calculation
        var pleasure = CalculatePleasure(ctx.Target, ctx.Actor);

        // State modification
        ctx.Target.AddExp(ExpType.Kiss, pleasure);
        ctx.Target.AddPalam(PalamType.Pleasure, pleasure);

        // Message generation (delegates to KojoEngine)
        return new ComResult
        {
            Success = true,
            Message = ctx.Kojo.GetDialogue(ctx.Target.Id, "COM_100", ctx.EvalContext)
        };
    }
}
```

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Commands/Com/ComBase.cs` | COM 基底クラス |
| `src/Era.Core/Commands/Com/EquipmentComBase.cs` | 装具COM 基底クラス |
| `src/Era.Core/Commands/Com/ComContext.cs` | COM 実行コンテキスト |
| `src/Era.Core/Commands/Com/ComResult.cs` | COM 実行結果 |
| `src/Era.Core/Commands/Com/ICom.cs` | COM インターフェース |
| `src/Era.Core/Commands/Com/IComContext.cs` | コンテキストインターフェース |
| `src/Era.Core/Commands/Com/IEquipmentCom.cs` | 装具COM インターフェース |
| `src/Era.Core/Commands/Com/IComRegistry.cs` | COM 登録インターフェース |
| `src/Era.Core/Commands/Com/ComIdAttribute.cs` | レガシーID属性 |
| `src/Era.Core/Commands/Com/Daily/*.cs` | 日常行動 (17) |
| `src/Era.Core/Commands/Com/Masturbation/*.cs` | 自慰系 (17) |
| `src/Era.Core/Commands/Com/System/*.cs` | システム (2) |
| `src/Era.Core/Commands/Com/Utility/*.cs` | ユーティリティ (22) |
| `src/Era.Core/Commands/Com/Visitor/*.cs` | 来訪者 (4) |
| `src/Era.Core/Commands/Com/Training/Bondage/*.cs` | 拘束系 (11) |
| `src/Era.Core/Commands/Com/Training/Equipment/*.cs` | 装具系 (17) |
| `src/Era.Core/Commands/Com/Training/Oral/*.cs` | 奉仕系 (16) |
| `src/Era.Core/Commands/Com/Training/Penetration/*.cs` | 挿入系 (26) |
| `src/Era.Core/Commands/Com/Training/Touch/*.cs` | 愛撫系 (14) |
| `src/Era.Core/Commands/Com/Training/Undressing/*.cs` | 脱衣系 (4) |
| `src/Era.Core/Commands/Com/Training/Utility/*.cs` | 訓練補助 (2) |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=Com"
# All COM implementations match legacy behavior
```

**Success Criteria**:
- [x] 150+ COMF が実装済み
- [x] ComBase 基底クラスが確立
- [x] KojoEngine 連携が機能
- [x] COM 実行が legacy と等価

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 12: COM Implementation」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **Tasks: 等価性検証** | legacy COMF との等価性テストを含む | AC にテスト存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Technical Debt Deferred to Phase 12** (from Phase 7 Review):

> **⚠️ Phase 7 引き継ぎ事項**: Training統合・Callback実装・TODOクリーンアップ

**1. TrainingProcessor Integration (17 lines dead code)**:

| File | Lines | Action |
|------|-------|--------|
| `src/Era.Core/Training/TrainingProcessor.cs` | 85-90 | Uncomment Equipment processor call |
| `src/Era.Core/Training/TrainingProcessor.cs` | 92-97 | Uncomment Orgasm processor call |
| `src/Era.Core/Training/TrainingProcessor.cs` | 103-107 | Uncomment Favor calculation (requires Task 2) |

**Rationale**: F406 completed Equipment/OrgasmProcessor implementations. Phase 12 provides COM context needed for full integration.

**2. Well-Known Index Additions**:

| File | Location | Index | Value | Purpose |
|------|----------|-------|:-----:|---------|
| `src/Era.Core/Types/CharacterFlagIndex.cs` | - | `Favor` | TBD | CFLAG:好感度 (TrainingProcessor L102) |
| `src/Era.Core/Types/TCVarIndex.cs` | - | `Actor` | TBD | ExperienceGrowthCalculator L50 |

**3. Callback Implementations (F405 pattern)**:

| File | Location | Callback | Pattern |
|------|----------|----------|---------|
| `src/Era.Core/Character/ExperienceGrowthCalculator.cs` | L106 | JUEL accessor | Apply F405 `Func<CharacterId, JuelIndex, int>` |
| `src/Era.Core/Character/VirginityManager.cs` | L60 | TEQUIP accessor | Apply F405 `Func<CharacterId, int, bool>` |

**4. TODO Comment Cleanup**:

| File | Count | Action |
|------|:-----:|--------|
| `src/Era.Core.Tests/OrgasmProcessorEquivalenceTests.cs` | 13 | Remove outdated "IJuelVariables not implemented" comments |
| `src/Era.Core/Character/ExperienceGrowthCalculator.cs` | 1 | Remove CupChange TODO (not needed - CUP is session-temporary) |

**Tasks Addition**:
- Task 5.5: **TrainingProcessor integration** (uncomment 17 lines, verify with tests) ✅ F460
- Task 5.6: **Well-known index additions** (CharacterFlagIndex.Favor, TCVarIndex.Actor) ✅ F460
- Task 5.7: **JUEL/TEQUIP callback implementation** (F405 pattern) ✅ F460
- Task 5.8: **TODO comment cleanup** (14 outdated comments) ✅ F460
- Task 5.9: **TrainingProcessor training context parameterization** (from F460 残課題)
  - Location: `src/Era.Core/Training/TrainingProcessor.cs` L102
  - Current: `_favorCalculator.CalculateFavor(target, CharacterId.Reimu, 0, 0)`
  - Target: Use `IComContext.Actor` instead of hardcoded `CharacterId.Reimu`
  - Dependency: Requires IComContext to be passed through Process() method

**Technical Debt Deferred from Phase 9** (from F434):

> **⚠️ F434 引き継ぎ事項**: System Commands stub implementations と GlobalStatic accessor

**1. GlobalStatic Accessor Migration (Task 8.5 from Phase 9)**:

| File | Line | Current | Target |
|------|:----:|---------|--------|
| `Era.Core\Common\GameInitialization.cs` | 319 | TODO: Replace with GlobalStatic | `GlobalStatic.GetFlag()` accessor |
| `Era.Core\Common\GameInitialization.cs` | 339 | TODO: Replace with GlobalStatic | `GlobalStatic.GetCFlag()` accessor |
| `Era.Core\Common\GameInitialization.cs` | 358 | TODO: Replace with GlobalStatic | `GlobalStatic.GetTFlag()` accessor |

**Rationale**: GlobalStatic.GetFlag/GetCFlag/GetTFlag accessors do not exist. Creating them requires defining the complete state accessor pattern, which is Phase 21 State Systems scope.

**2. System Commands Engine Integration (from F434)**:

| Service | File | Current | Target |
|---------|------|---------|--------|
| CharacterManager | `src/Era.Core/Commands/System/CharacterManager.cs` | Stub (returns Fail) | Delegate to `GlobalStatic.VEvaluator` |
| StyleManager | `src/Era.Core/Commands/System/StyleManager.cs` | Stub (returns Fail) | Delegate to `GlobalStatic.Console` |
| GameState | `src/Era.Core/Commands/System/GameState.cs` | Stub (returns Fail) | Delegate to `GlobalStatic.Process` |

**3. Additional System Commands (from F434 scope reduction)**:

| Command | Category | Description | Priority |
|---------|----------|-------------|:--------:|
| SETCOLORBYNAME | Style | Set color by name lookup | Low |
| BEGIN | System | Begin block command | Low |
| DRAWLINE | System | Draw horizontal line | Low |

**Tasks Addition (from F434)**:
- Task 5.9: **GlobalStatic accessor migration** (3 TODOs in GameInitialization.cs)
- Task 5.10: **System Commands engine integration** (CharacterManager/StyleManager/GameState → engine)
- Task 5.11: **Additional system commands** (SETCOLORBYNAME, BEGIN, DRAWLINE - optional)

**Next**: Create Phase 13 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 13: DDD Foundation (was Phase 12)

**Phase Status**: DONE

**Goal**: Domain-Driven Design基盤の確立（Aggregate Root, Repository, UnitOfWork patterns, **Domain Events基盤**）

**Background**: Phase 9までで基本機能は揃うが、ゲーム状態がフラット配列（ERBレガシー）のまま。Phase 10でドメインモデルを導入し、ビジネスルールのカプセル化と不変条件の保証を実現する。

> **⚠️ DDD原則（このPhaseの本質）**
>
> **Aggregate Root Pattern**:
> - キャラクター状態をCharacter集約ルートにカプセル化
> - 不変条件（invariants）を集約内で保証
> - 外部からは集約ルート経由でのみアクセス
>
> **Repository Pattern**:
> - 永続化の詳細を隠蔽
> - インメモリ実装でテスト容易性確保
> - 将来のDB移行に備えた抽象化
>
> **Unit of Work Pattern**:
> - 複数集約への変更を原子的にコミット
> - トランザクション境界の明示

**Tasks**:
1. Aggregate Root基盤クラス定義
2. Character Aggregate実装（CharacterStats, AbilitySet, TalentSet Value Objects含む）
3. IRepository<T>インターフェース定義
4. IUnitOfWork定義
5. InMemoryRepository実装（テスト用）
6. UnitOfWork実装
7. TransactionBehavior UoW完全実装 (from F430) - Phase 9 stub → BeginTransaction/Commit/Rollback
8. 既存IVariableStoreとの橋渡しアダプター
9. DI統合（ServiceCollectionExtensions更新）
10. **SCOMF full logic implementation** (F435 stubs → full SOURCE/STAIN/EXP/TCVAR logic) - Phase 9 stub handlers を完全実装に置換
11. **IDomainEvent基盤インターフェース + DomainEventBase実装** (Phase 28 から移動 — Phase 24 NTR設計で必要)
12. **IDomainEventPublisher + DomainEventPublisher実装** (Phase 28 から移動)
13. **EventHandlerRegistry実装** (Phase 28 から移動)
14. **Character Aggregateへのイベント発行統合** (Phase 28 Task#5 から移動)
15. **Create Phase 13 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 13 セクションとの整合確認必須
16. **Create Phase 14 Planning feature** (type: research, include transition feature tasks)

**Core Interfaces**:

```csharp
// src/Era.Core/Domain/AggregateRoot.cs
public abstract class AggregateRoot<TId> where TId : struct
{
    public TId Id { get; protected set; }
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

// src/Era.Core/Domain/IRepository.cs
public interface IRepository<T, TId>
    where T : AggregateRoot<TId>
    where TId : struct
{
    Result<T> GetById(TId id);
    IReadOnlyList<T> GetAll();
    void Add(T aggregate);
    void Update(T aggregate);
    void Remove(TId id);
}

// src/Era.Core/Domain/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IRepository<Character, CharacterId> Characters { get; }
    Task<int> CommitAsync(CancellationToken ct = default);
    void Rollback();
}
```

**Character Aggregate Example**:

```csharp
// src/Era.Core/Domain/Aggregates/Character.cs
public class Character : AggregateRoot<CharacterId>
{
    public CharacterName Name { get; private set; }
    public CharacterStats Stats { get; private set; }
    public AbilitySet Abilities { get; private set; }
    public TalentSet Talents { get; private set; }

    // ドメインメソッド - ビジネスルールをカプセル化
    public Result<GrowthResult> ApplyTraining(TrainingAction action)
    {
        if (Stats.Stamina < action.RequiredStamina)
            return Result<GrowthResult>.Fail("Insufficient stamina");

        var growth = CalculateGrowth(action);
        Abilities = Abilities.Apply(growth);
        Stats = Stats.ConsumeStamina(action.RequiredStamina);

        AddDomainEvent(new TrainingCompleted(Id, action, growth));
        return Result<GrowthResult>.Ok(growth);
    }
}

// src/Era.Core/Domain/ValueObjects/CharacterStats.cs
public readonly record struct CharacterStats(
    int Health,
    int Stamina,
    int Frustration,
    int Loyalty
)
{
    public CharacterStats ConsumeStamina(int amount) =>
        this with { Stamina = Math.Max(0, Stamina - amount) };
}
```

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Domain/AggregateRoot.cs` | 集約ルート基盤クラス |
| `src/Era.Core/Domain/Aggregates/Character.cs` | キャラクター集約 |
| `src/Era.Core/Domain/ValueObjects/*.cs` | CharacterStats, AbilitySet等 |
| `src/Era.Core/Domain/IRepository.cs` | リポジトリインターフェース |
| `src/Era.Core/Domain/IUnitOfWork.cs` | UoWインターフェース |
| `src/Era.Core/Infrastructure/InMemoryRepository.cs` | インメモリ実装 |
| `src/Era.Core/Infrastructure/UnitOfWork.cs` | UoW実装 |
| `src/Era.Core/Infrastructure/VariableStoreAdapter.cs` | レガシー橋渡し |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=DDD"
# Aggregate invariants enforced
# Repository operations correct
# UnitOfWork transaction semantics verified
```

**Success Criteria**:
- [x] Aggregate Root パターン確立
- [x] Repository パターン確立
- [x] UnitOfWork パターン確立
- [x] Domain Events 基盤構築

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 13: DDD Foundation」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 14 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 14: Era.Core Engine (was Phase 13)

**Phase Status**: DONE

**Goal**: Pure C# game engine (headless実行可能)

**Prerequisites**: Phase 3-13 完了（System, Architecture, Variable, Ability, Expression, Command, COM, DDD Foundation）

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **インターフェース定義（エンジンコア）**:
> ```csharp
> public interface IGameEngine
> {
>     IGameState State { get; }
>     Result<GameTick> ProcessTurn();
>     void Initialize(GameConfig config);
> }
>
> public interface IKojoEngine
> {
>     Result<DialogueResult> GetDialogue(CharacterId character, ComId com, IEvaluationContext ctx);
> }
>
> public interface INtrEngine
> {
>     Result<NtrParameters> Calculate(CharacterId target, CharacterId actor, NtrAction action);
> }
>
> public interface IStateManager
> {
>     Result<GameState> Load(string path);
>     Result<Unit> Save(string path, GameState state);
> }
> ```
>
> **DI登録（全コアサービス）**:
> - `IGameEngine` → `GameEngine`
> - `IKojoEngine` → `KojoEngine`
> - `INtrEngine` → `NtrEngine`
> - `IStateManager` → `StateManager`
> - `IProcessState` → `ProcessState`
> - `IInputHandler` → `InputHandler`
>
> **この Phase がDI構成の統合ポイント**:
> Phase 5-9 で作成した全インターフェースをここで結合し、DIコンテナに登録する。

**Tasks**:
1. Implement `GameEngine` (main loop)
2. Implement `StateManager` (save/load JSON)
3. Implement `KojoEngine` (YAML parsing, condition evaluation)
4. Implement `CommandProcessor` (COM execution)
5. Implement `NtrEngine` (parameter calculations)
6. Implement `HeadlessUI` (console-based testing)
7. **Implement `ProcessState` (実行状態機械)**
8. **Implement `InputHandler` (入力待ち処理)**
9. ~~**Migrate `CHARA_SET.ERB` (キャラクターセットアップ)**~~ **[CANCELLED]** - CHARA_SET.ERB is a reset menu UI (@CHARA_RESET), not initialization logic. Character setup functionality already implemented in F377 ICharacterSetup.
10. ~~**Migrate `MANSETTTING.ERB` (男性キャラ設定)**~~ **[CANCELLED]** - MANSETTTING.ERB is a visitor settings menu (@MAN_SET), not male character configuration. Gender validation already implemented in F377 ICharacterSetup.
11. **Create Phase 14 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 14 セクションとの整合確認必須
12. **Create Phase 15 Planning feature** (type: research, include transition feature tasks)
13. **Implement SCOMF prerequisite checks** (IsScenarioAvailable の詳細条件 - TALENT/ABL/FLAG による実行可否判定、F473 からの延期タスク)
14. **HeadlessUI deeper integration** (GameEngine で HeadlessUI 自動インスタンス化、ProcessTurn 後の自動 OutputState 呼び出し、from F479)

**Process State Machine** (Previously undocumented):

| Component | Purpose |
|-----------|---------|
| `ProcessState` | 関数呼び出しスタック、ローカル変数スコープ |
| `CallStack` | CALL/RETURN のアドレス管理 |
| `ExecutionContext` | 現在の実行位置、システム状態 |

**Input Handling System**:

| Component | Purpose |
|-----------|---------|
| `InputHandler` | INPUT/INPUTS コマンド処理 |
| `InputRequest` | 入力待ちリクエスト（数値/文字列） |
| `InputValidator` | 入力検証 |

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/GameEngine.cs` | Main game loop |
| `src/Era.Core/StateManager.cs` | Game state management |
| `src/Era.Core/KojoEngine.cs` | Dialogue selection |
| `src/Era.Core/CommandProcessor.cs` | COM execution |
| `src/Era.Core/NtrEngine.cs` | NTR parameter calculations |
| `src/Era.Core/HeadlessUI.cs` | Test/debug UI |
| `src/Era.Core/Process/ProcessState.cs` | **実行状態機械** |
| `src/Era.Core/Process/CallStack.cs` | **呼び出しスタック** |
| `src/Era.Core/Input/InputHandler.cs` | **入力処理** |
| `src/Era.Core/Input/InputRequest.cs` | **入力リクエスト** |
| `src/Era.Core/Common/CharacterSetup.cs` | **キャラセットアップ** (F377で実装済み) |
| ~~`src/Era.Core/Character/MaleSettings.cs`~~ | ~~**男性キャラ設定**~~ [CANCELLED - F377 ICharacterSetup でカバー] |

**Character Setup Files** [2026-01-13 REVISED: ERB files are menu UI, not library logic]:

| File | Lines | Purpose | Status |
|------|------:|---------|:------:|
| `CHARA_SET.ERB` | 190 | ~~キャラクター初期化・セットアップ~~ → 実際はリセットメニューUI (@CHARA_RESET) | NOT_MIGRATED (UI) |
| `MANSETTTING.ERB` | ~300 | ~~男性キャラクター固有設定~~ → 実際は進入者設定メニュー (@MAN_SET) | NOT_MIGRATED (UI) |

> **Note**: Character initialization logic is already implemented in F377 ICharacterSetup (src/Era.Core/Common/CharacterSetup.cs).

**Verification**:
```bash
dotnet test Era.Core.Tests
# All unit tests pass, headless demo runs
```

**Success Criteria**:
- [x] Era.Core が headless 実行可能
- [x] ProcessState 状態機械が確立
- [x] CharacterSetup が C# 実装 (F377 で完了、Task 9-10 は対象ファイル誤認のため CANCELLED)
- [x] 全テスト PASS

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 14: Era.Core Engine」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **Tasks: 等価性検証** | legacy 実装との等価性テストを含む | AC にテスト存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 15 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 15: Architecture Review (was Phase 14)

**Phase Status**: DONE

**Goal**: Phase 1-14 実装経験に基づく構造検証と軽量リファクタリング

**Background**: 150+ COM実装（Phase 12）、DDD基盤（Phase 13）、エンジン基盤（Phase 14）完了後、大規模並列フェーズ（Phase 19-21）前に構造を確定。

**Test Strategy Context**: ERB時代は技術負債によりテストで問題を抽出できなかった。C#移行を機にテスト戦略を確立する。シミュレーションゲーム特有の課題（ランダム要素、成長要素）に対応したテスト設計が必要。口上はYAMLスキーマ検証で十分、回帰テスト不要。

> **⚠️ Phase 4 準拠確認（このPhaseの主目的）**
>
> **レビュー観点**:
> - [ ] 全クラスがSRP準拠か（1クラス1責務）
> - [ ] 全依存がインターフェース経由か（DIP）
> - [ ] Strongly Typed IDsが一貫して使用されているか
> - [ ] Result型が適切に使用されているか（例外ではなく）
> - [ ] DIコンテナへの登録漏れがないか
>
> **修正が必要な場合**:
> - Phase 4パターンからの逸脱を発見 → このPhaseで修正
> - 新パターンの発見 → Type Design Guidelines に追記

**Tasks**:
1. コードレビュー（Phase 1-12 全実装）
2. フォルダ構造の妥当性検証
3. 命名規則の一貫性確認
4. テスタビリティ課題の洗い出し
5. **Test Strategy Design（テスト戦略設計）**
   - 5.1. Era.Core 現状調査（ランダム要素、状態管理パターン）
   - 5.2. Era.Core.Tests 構造分析（現在のテストカバレッジ、パターン）
   - 5.3. ERB ランダム/成長パターン抽出（RAND, 成長計算の使用箇所）
   - 5.4. IRandomProvider 設計（DI可能なランダム抽象化）
   - 5.5. テストレイヤー構造定義（Unit/Integration/E2E の責務分離）
   - 5.6. E2E テスト方針策定（シード固定、不変条件、Golden Master）
   - 5.7. 各 Phase の Tasks/AC への反映
   - 5.8. **/do コマンド テスト実行設計**:
     - Phase 3 (TDD): C# テスト作成・RED 確認方法
     - Phase 6 (Verification): AC 検証実行・ログ出力・verify-logs.py 連携
     - TRX 出力先: `_out/logs/prod/ac/engine/feature-{ID}/`
     - 結果形式: TRX (C#)、JSON (AC)
   - 5.9. **AC 検証フロー定義**:
     - AC Type 別検証方法（test/code/file/build/output）
     - ログ出力タイミングと形式
     - verify-logs.py との連携（スコープ指定、結果集計）
   - 5.10. **pre-commit テスト実行定義**:
     - 実行対象: `dotnet test Era.Core.Tests`（全テスト）
     - 実行条件: 常時（feature 実装中は feature スコープ追加）
     - Linter: C# は Roslyn Analyzer で代替（ErbLinter 廃止）
6. 必要に応じた構造リファクタリング
7. 設計ドキュメント更新
8. **Create Phase 15 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 15 セクションとの整合確認必須
9. **Create Phase 16 Planning feature** (type: research, include transition feature tasks)

**Review Checklist**:

| 観点 | 確認項目 |
|------|----------|
| **SRP** | 各クラスが単一責任か |
| **OCP** | 拡張に開き、修正に閉じているか（特にレジストリ系） |
| **DIP** | インターフェース依存が適切か |
| **Common/ 肥大化** | Common/ に複数責務が混在していないか → 分割検討 |
| **Folder Structure** | Com/ の番号別 vs 機能別 |
| **Naming** | 一貫した命名規則か |
| **Testability** | Mock注入が容易か |
| **Random DI** | ランダム要素が IRandomProvider 経由で注入可能か |
| **Test Layers** | Unit/Integration/E2E の責務が明確に分離されているか |
| **横断的関心事** | ログ、検証の配置 |

**Scope Limits** (過剰リファクタ防止):

| 許可 | 禁止 |
|------|------|
| ✅ フォルダ構造変更 | ❌ 動作するコードの全面書き直し |
| ✅ インターフェース追加 | ❌ 新機能追加 |
| ✅ 命名リネーム | ❌ 投機的抽象化 |
| ✅ テスト追加 | ❌ Phase 16以降の先取り |

**Decision Points**:

| 判断項目 | 選択肢 |
|----------|--------|
| Com/ 構造 | 番号別維持 / 機能別に再編 |
| Extensions/ | 維持 / Features/ に統合 |
| 横断的関心事 | Infrastructure/ 追加 / 現状維持 |

**Deliverables**:

| 成果物 | 内容 |
|--------|------|
| `designs/architecture-review-15.md` | 検証結果レポート |
| `designs/test-strategy.md` | テスト戦略設計書（下記セクション必須） |
| `src/Era.Core/Random/IRandomProvider.cs` | ランダム抽象化インターフェース |
| `full-csharp-architecture.md` | テスト戦略セクション追加、各Phase Tasks/AC更新 |
| リファクタリング済みコード | 構造変更があれば |

**test-strategy.md 必須セクション**:

| セクション | 内容 |
|-----------|------|
| **1. Test Layers** | Unit/Integration/E2E の責務分離、各レイヤーの対象と目的 |
| **2. Test Types** | AC検証、回帰、Linter、統合、E2E の役割と実行タイミング |
| **3. /do Command Integration** | Phase 3 (TDD)、Phase 6 (Verification) でのテスト実行詳細 |
| **4. Log Output** | TRX/JSON 出力先、verify-logs.py 連携、スコープ指定 |
| **5. Pre-commit Hook** | 実行対象、条件、Roslyn Analyzer 設定 |
| **6. AC Verification Flow** | AC Type 別検証方法、ログ形式、結果判定基準 |
| **7. IRandomProvider** | 設計、DI 登録、テストでのシード固定方法 |

**Exit Criteria**:
- Phase 16以降の構造が確定
- 技術的負債の意図的受け入れを文書化
- レビューチェックリスト全項目完了
- テスト戦略が確定し、各Phaseに反映済み

**Verification**:
```bash
dotnet build Era.Core
dotnet test Era.Core.Tests
# 全テストパス（リファクタ後も動作保証）
# IRandomProvider が DI コンテナに登録可能
```

**Success Criteria**:
- [x] アーキテクチャ一貫性確認
- [x] 技術的負債の意図的受け入れを文書化
- [x] リファクタ後テスト PASS
- [x] テスト戦略設計書完成（全7セクション）
- [x] IRandomProvider 実装完了
- [x] /do コマンド テスト実行設計完了
- [x] AC 検証フロー定義完了
- [x] pre-commit テスト実行定義完了

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 15: Architecture Review」を継承 | Grep |
| **Tasks: 負債解消** | 負債の意図的受け入れ/解消を文書化 | ドキュメント確認 |
| **Tasks: テスト戦略** | IRandomProvider 設計、テストレイヤー定義を含む | designs/test-strategy.md 存在確認 |
| **Tasks: /do 連携** | Phase 3/6 のテスト実行方法を定義 | test-strategy.md セクション3 |
| **Tasks: AC 検証** | AC Type 別検証フローを定義 | test-strategy.md セクション6 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |
| **AC: テスト戦略** | テスト戦略設計書完成、IRandomProvider 実装を検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 16 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

---

**Known Technical Debt (Deferred to Phase 14)**:

**Tracking Policy**: Technical debt follows `.claude/reference/deferred-task-protocol.md`:
- Option A: Create Feature immediately
- Option B: Add to existing Feature's Tasks
- Option C: Add to architecture.md Phase Tasks

This section documents WHAT the debt is. Resolution tracking uses the 3 options above.

以下は Phase 8 で発見され、Phase 14 Architecture Review で対処予定の技術的負債:

| ID | 対象 | 問題 | 推奨対策 | 発見日 |
|----|------|------|----------|--------|
| TD-P14-001 | `src/Era.Core/Expressions/Operators.cs` `OperatorRegistry` | **OCP違反**: `EvaluateBinary()` が if/else チェーンで演算子をハードコード。新演算子追加時にメソッド修正が必要 | Strategy Pattern または Dictionary<string, IOperator> でディスパッチ。各演算子を `IOperator` 実装として分離 | 2026-01-10 |

**TD-P14-001 詳細**:

```
現状:
- EvaluateBinary() 内に ~40 の if/else 分岐
- 演算子ごとに 2-4 行のインライン実装
- 新演算子追加 = メソッド修正（OCP違反）

推奨リファクタリング:
1. IOperator インターフェース定義
   - Result<object> Evaluate(object left, object right)
   - string Symbol { get; }
   - int Precedence { get; }

2. 各演算子を IOperator 実装クラス化
   - AdditionOperator, SubtractionOperator, etc.

3. OperatorRegistry を Dictionary<string, IOperator> ベースに変更
   - Register(IOperator operator)
   - GetOperator(string symbol)

4. EvaluateBinary() をシンプルなディスパッチに
   - var op = GetOperator(symbol);
   - return op.Evaluate(left, right);

影響範囲:
- src/Era.Core/Expressions/Operators.cs (内部構造のみ)
- IOperatorRegistry インターフェースは変更不要
- 既存テストは全て PASS を維持すること

優先度: LOW（演算子追加予定がなければ実害なし）
```

---

### Phase 16: C# 14 Style Migration (NEW)

**Phase Status**: DONE

**Goal**: 既存コードへの C# 14 パターン適用によるコード簡潔化

**Background**: Phase 10 で .NET 10 / C# 14 を有効化したが、既存コードは旧スタイルのまま。Primary Constructor 等の新機能を適用することで ~400行のボイラープレートを削減。

**Scope**:
- src/Era.Core/ ディレクトリ内の C# ファイル
- Primary Constructor 変換対象: 50ファイル、160フィールド
- Collection Expression 適用: 18箇所
- Null-conditional assignment: 該当箇所

**Tasks**:
1. Training/ ディレクトリ (5ファイル)
   - TrainingProcessor.cs (5 readonly fields)
   - MarkSystem.cs (9 readonly fields)
   - AbilityGrowthProcessor.cs, EquipmentProcessor.cs, OrgasmProcessor.cs
2. Character/ ディレクトリ (4ファイル)
   - ExperienceGrowthCalculator.cs (5 readonly fields)
   - CharacterStateTracker.cs, VirginityManager.cs, PainStateChecker.cs
3. Commands/Flow/ ディレクトリ (10ファイル)
   - CallHandler.cs, IfHandler.cs, ForHandler.cs, etc.
4. Commands/Special/ ディレクトリ (16ファイル)
   - Scomf1Handler.cs - Scomf16Handler.cs
5. Commands/System/ ディレクトリ (3ファイル)
   - AddCharaHandler.cs, SetColorHandler.cs, SaveGameHandler.cs
6. その他 (12ファイル)
   - Common/, Variables/, Functions/
7. Collection Expression 適用
   - `new List<T>()` → `[]`
   - `new List<T> { ... }` → `[...]`
8. **Create Phase 16 Post-Phase Review feature** (type: infra)
9. **Create Phase 17 Planning feature** (type: research)

**Pattern Reference**: `.claude/skills/csharp-14/SKILL.md`

**Conversion Example**:

```csharp
// Before (C# 12 style)
public class MarkSystem : IMarkSystem
{
    private readonly IVariableStore _variableStore;
    private readonly SubmissionMarkCalculator _submissionCalculator;
    // ... 7 more fields ...

    public MarkSystem(
        IVariableStore variableStore,
        SubmissionMarkCalculator submissionCalculator,
        // ... 7 more params ...
    {
        _variableStore = variableStore ?? throw new ArgumentNullException(...);
        // ... 9 assignments ...
    }
}

// After (C# 14 Primary Constructor)
public class MarkSystem(
    IVariableStore variableStore,
    SubmissionMarkCalculator submissionCalculator,
    // ... 7 more params ...
) : IMarkSystem
{
    // Field declarations and assignments eliminated
}
```

**Success Criteria**:
- [x] All 50 target files converted to Primary Constructor
- [x] All 18 `new List<T>()` converted to Collection Expression
- [x] All tests pass
- [x] No functional changes (refactoring only)
- [x] ~400 lines of boilerplate removed

**Actual Results** (F509-F514):
- Primary Constructor: 50 files, 73 classes, 104 fields migrated
  - F509 Training: 10 files, 14 classes, 29 fields
  - F510 Character: 4 files, 4 classes, 11 fields
  - F511 Commands/Flow: 11 files, 18 classes, 24 fields
  - F512 Commands/Special: 16 files, 16 classes, 16 fields
  - F513 Commands/System+Other: 9 files, 21 classes, 24 fields
- Collection Expression: 18 conversions across 4 files (F514)
- Boilerplate reduction: ~300-350 lines (field declarations + constructor assignments + null checks)

**Risk**: LOW - コンパイラが型チェックを行うため、リファクタリングの安全性が高い

---

### Phase 17: Data Migration (was Phase 16)

**Phase Status**: DONE

**Goal**: Migrate genuinely moddable content and configuration data to YAML while keeping engine-dependent definitions in C# for type safety and handler enforcement.

**Boundary Principle**: C# Engine (logic, effect handlers, condition evaluators) vs YAML Content (COM definitions, character data, kojo dialogue). Community moddability requires preserving ERA-style text file editing workflow - "edit YAML → reload game → works" for Tier 1 content.

**CRITICAL**: `VariableSize.csv` を最初に移行（配列サイズ定義、全変数の前提条件）

### Data Placement Strategy (Revised 2026-01-19)

| Data Category | Format | Rationale | Moddability Tier |
|--------------|--------|-----------|:----------------:|
| **Kojo Dialogue** | YAML | Pure data-driven, no logic required | ✅ Tier 1 |
| **Character Definitions** | YAML | TALENT combinations, genuinely moddable | ✅ Tier 1 |
| **COM Definitions** | YAML | Effect combinations (80% YAML-only) | ✅ Tier 1 |
| **Configuration Values** | YAML | Costs, thresholds, multipliers | ⚠️ Tier 2 |
| **Effect Handlers** | C# | Application logic with YAML parameters | C# Engine |
| **Condition Evaluators** | C# | Evaluation logic with YAML thresholds | C# Engine |
| **TALENT/Ability Enums** | C# ENUM | Require C# handlers, not moddable | ❌ Tier 3 |
| **FLAG/CFLAG Names** | YAML | Name mappings, no logic | ⚠️ Tier 2 |

**Moddability Tier Legend**:
- **Tier 1 (✅)**: Fully moddable - No C# compilation required for additions
- **Tier 2 (⚠️)**: Parameter moddable - Can adjust values, cannot add mechanics
- **Tier 3 (❌)**: NOT moddable - C# handler required for additions

**Phantom Moddability Prevention**: Files in Tier 3 include schema warnings and startup validation to prevent non-functional YAML additions.

**Statistics**: 43 CSV files total (19 character + 24 system/config)

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **データローダーのインターフェース**:
> ```csharp
> public interface IDataLoader<T>
> {
>     Result<T> Load(string path);
> }
>
> public interface ICharacterDataLoader : IDataLoader<CharacterData> { }
> public interface IConfigLoader : IDataLoader<GameConfig> { }
> ```
>
> **DI登録**:
> - `ICharacterDataLoader` → `YamlCharacterDataLoader`
> - `IConfigLoader` → `JsonConfigLoader`
>
> **Strongly Typed データモデル**:
> - CSV行 → 型付きクラス（`CharacterDefinition`, `FlagDefinition`, 等）
> - マジックナンバー禁止 → enum または Strongly Typed ID

**Tasks**:
1. **Critical Config Files** (Phase 4 依存)
2. Variable Definition CSVs (Phase 4 依存)
3. Character Data CSVs (19 files)
4. Content Definition CSVs
5. Config Files
6. **Create Phase 14 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 14 セクションとの整合確認必須
7. **Create Phase 15 Planning feature** (type: research, include transition feature tasks)

**Migration Order** (依存関係順):

| Order | File | Purpose | Priority |
|:-----:|------|---------|:--------:|
| 1 | `VariableSize.csv` | 配列サイズ定義 | **CRITICAL** |
| 2 | `GameBase.csv` | ゲーム基本設定 | **CRITICAL** |
| 3 | `_default.config` | デフォルト設定 | HIGH |
| 4 | `_fixed.config` | 固定設定 | HIGH |
| 5 | `emuera.config` | エンジン設定 | HIGH |
| 6 | `FLAG.CSV` | グローバルフラグ (66) | HIGH |
| 7 | `CFLAG.CSV` | キャラクターフラグ (489) | HIGH |
| 8 | `TFLAG.CSV` | ターンフラグ (80) | HIGH |
| 9 | `Talent.csv` | 素質名定義 (name mappings only) | HIGH |
| 10 | `Abl.csv` | 能力名定義 (name mappings only) | HIGH |
| 11 | `Palam.csv` | パラメータ定義 | HIGH |
| 12 | `exp.csv` | 経験値定義 | Medium |
| 13 | `ex.csv` | 拡張経験値 | Medium |
| 14 | `Train.csv` | 訓練定義 | Medium |
| 15 | `Item.csv` | アイテム定義 | Medium |
| 16 | `Juel.csv` | 宝石定義 | Medium |
| 17 | `Mark.csv` | マーク定義 | Medium |
| 18 | `Stain.csv` | 汚れ定義 | Medium |
| 19 | `source.csv` | ソース定義 | Medium |
| 20 | `Tequip.csv` | 装備定義 | Medium |
| 21 | `TCVAR.csv` | 変数マッピング | Medium |
| 22 | `TSTR.csv` | 文字列テーブル | Medium |
| 23 | `CSTR.csv` | キャラ文字列 | Medium |
| 24 | `Str.csv` | 汎用文字列 | Medium |
| 25 | `_Rename.csv` | リネームマッピング | Low |
| 26 | `_Replace.csv` | 置換ルール | Low |
| 26+ | `Chara*.csv` (26) | キャラクターデータ | Medium |

**Note on Talent/Ability Migration**: Talent.csv and Abl.csv migrate NAME DEFINITIONS only. TalentIndex and AbilityIndex remain C# enums - new talents/abilities require C# handler implementation (not genuinely moddable).

**Character Data Files** (19 files total):

| File | Purpose |
|------|---------|
| `Chara0.csv` | プレイヤー「あなた」 |
| `Chara1.csv` - `Chara13.csv` | メインキャラ (美鈴〜) |
| `Chara28.csv`, `Chara29.csv` | サブキャラ |
| `Chara99.csv` | NPCテンプレート |
| `Chara148.csv`, `Chara149.csv` | 追加キャラ |

**System Definition Files** (24 files total):

| Category | Files | Purpose |
|----------|-------|---------|
| Config | `VariableSize.csv`, `GameBase.csv` | エンジン設定 |
| Transform | `_Rename.csv`, `_Replace.csv` | 前処理ルール |
| Variables | `Talent.csv`, `CFLAG.csv`, `TFLAG.csv`, `Abl.csv`, `Palam.csv`, `exp.csv`, `Base.csv`, `source.csv` | 変数名定義 |
| Strings | `Str.csv`, `CSTR.csv`, `TSTR.csv`, `TCVAR.csv` | 文字列変数 |
| Items | `Train.csv`, `Item.csv`, `Equip.csv`, `Tequip.csv` | アイテム/装備 |
| Effects | `ex.csv`, `Mark.csv`, `Juel.csv`, `Stain.csv` | 効果/状態 |

**Note**: `Juel.csv` と `Palam.csv` は同一内容（レガシー互換）

**Deliverables**:

| Current | Target | Schema |
|---------|--------|--------|
| `VariableSize.csv` | `Game/config/variable_sizes.yaml` | Required |
| `GameBase.csv` | `Game/config/game_base.yaml` | Required |
| `FLAG.CSV` | `Game/config/flags.yaml` | Required |
| `CFLAG.CSV` | `Game/config/character_flags.yaml` | Required |
| `Talent.csv` | `Game/config/talents.yaml` | Required |
| `Abl.csv` | `Game/config/abilities.yaml` | Required |
| `Chara*.csv` | `Game/characters/*.yaml` | Per-character |
| `Train.csv` | `Game/content/training.yaml` | Required |
| `Item.csv` | `Game/content/items.yaml` | Required |

### Community Moddability Scope (Added 2026-01-19)

**Genuinely Moddable Content** (Tier 1 - No C# Required):

**Examples**:
1. **Kojo Dialogue Addition**:
   ```yaml
   # Game/kojo/K1_Meiling/com101_kiss.yaml
   entries:
     - conditions:
         talent_affection: gte:5
       master_lines:
         - "美鈴、キスしよう"
   ```

2. **Character Addition**:
   ```yaml
   # Game/characters/CharaYukari.yaml
   id: 200
   name: "八雲紫"
   talents: [1, 5]  # Existing TALENTs
   ```

3. **COM Variant**:
   ```yaml
   # Game/data/coms/com101_gentle_kiss.yaml
   id: 101
   name: "優しいキス"
   effects:
     - type: source
       affection: 50
   ```

**NOT Moddable Content** (Tier 3 - C# Handler Required):

**Why These Require C#**:
- **New TALENTs**: Gameplay effects, dialogue branching, stat modifications
- **New Abilities**: Growth calculations, training modifiers
- **New Effect Types**: Effect application logic, system interactions

**Phantom Moddability Prevention**: See docs/modding-guide.md for proper modding workflow and tier explanations.

**Tools Created in This Phase**:
| Tool | Purpose |
|------|---------|
| `tools/CsvToYaml/` | CSV→YAML batch converter for 43 CSV files |
| `tools/SchemaValidator/` | YAML schema validation CLI (extends F348) |

**Verification**:
```bash
# CSV→YAML conversion
dotnet run --project tools/CsvToYaml -- Game/CSV/ --output Game/
# Schema validation for all YAML files
dotnet run --project tools/SchemaValidator -- Game/config/ Game/content/ Game/characters/
# Variable count verification
dotnet test Era.Core.Tests --filter "Category=DataMigration"
```

**Success Criteria**:
- [ ] 43 CSV ファイル移行完了
- [ ] YAML スキーマ検証 PASS
- [ ] データ等価性確認

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 17: Data Migration」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **Tasks: 等価性検証** | CSV→YAML 等価性テストを含む | AC にテスト存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 17 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 18: KojoEngine SRP分割 (was Phase 17)

**Phase Status**: DONE

**Goal**: KojoEngine (391行単一クラス)のSingle Responsibility分割

**Background**: KojoEngineは現在4つの責務を持つ:
1. YAML読み込み（Loading）
2. スキーマ検証（Validation）
3. 条件評価（Evaluation）
4. テンプレート展開（Rendering）

これらを分離し、テスト容易性と拡張性を向上させる。

> **⚠️ SRP原則（このPhaseの本質）**
>
> **責務分離**:
> - 各責務は独立したインターフェース + 実装クラス
> - KojoEngine は Facade として統合
> - 各コンポーネントは個別にテスト可能
>
> **Specification Pattern導入**:
> - 複雑な条件判定を再利用可能なオブジェクトに
> - TALENT/ABL/EXP分岐ロジックをカプセル化

**Tasks**:
1. IDialogueLoader インターフェース抽出
2. YamlDialogueLoader 実装
3. IConditionEvaluator インターフェース抽出
4. ConditionEvaluator 実装
5. Specification Pattern基盤実装
6. TalentSpecification, AblSpecification等の実装
7. IDialogueRenderer インターフェース抽出
8. TemplateDialogueRenderer 実装
9. IDialogueSelector インターフェース抽出
10. PriorityDialogueSelector 実装
11. KojoEngine Facade再構成
12. **Create Phase 18 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 18 セクションとの整合確認必須
13. **Create Phase 19 Planning feature** (type: research, include transition feature tasks)

**Core Interfaces**:

```csharp
// src/Era.Core/Dialogue/Loading/IDialogueLoader.cs
public interface IDialogueLoader
{
    Result<DialogueFile> Load(string path);
    Result<IReadOnlyList<DialogueFile>> LoadAll(string directory);
}

// src/Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs
public interface IConditionEvaluator
{
    bool Evaluate(DialogueCondition condition, IEvaluationContext context);
}

// src/Era.Core/Dialogue/Specifications/ISpecification.cs
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
    ISpecification<T> And(ISpecification<T> other);
    ISpecification<T> Or(ISpecification<T> other);
    ISpecification<T> Not();
}

// src/Era.Core/Dialogue/Rendering/IDialogueRenderer.cs
public interface IDialogueRenderer
{
    Result<string> Render(string template, IEvaluationContext context);
}

// src/Era.Core/Dialogue/Selection/IDialogueSelector.cs
public interface IDialogueSelector
{
    Result<DialogueEntry> Select(IReadOnlyList<DialogueEntry> entries, IEvaluationContext context);
}
```

**Specification Pattern Examples**:

```csharp
// src/Era.Core/Dialogue/Specifications/TalentSpecification.cs
public class TalentSpecification : ISpecification<IEvaluationContext>
{
    private readonly TalentType _talent;

    public TalentSpecification(TalentType talent) => _talent = talent;

    public bool IsSatisfiedBy(IEvaluationContext ctx) =>
        ctx.Talents.Contains(_talent);

    public ISpecification<IEvaluationContext> And(ISpecification<IEvaluationContext> other) =>
        new AndSpecification<IEvaluationContext>(this, other);

    public ISpecification<IEvaluationContext> Or(ISpecification<IEvaluationContext> other) =>
        new OrSpecification<IEvaluationContext>(this, other);

    public ISpecification<IEvaluationContext> Not() =>
        new NotSpecification<IEvaluationContext>(this);
}

// Usage example
var condition = new TalentSpecification(TalentType.恋慕)
    .And(new AblSpecification(AblType.V感覚, threshold: 3));

if (condition.IsSatisfiedBy(context))
    return highSensitivityDialogue;
```

**KojoEngine Facade**:

```csharp
// src/Era.Core/Dialogue/KojoEngine.cs (Refactored)
public class KojoEngine : IKojoEngine
{
    private readonly IDialogueLoader _loader;
    private readonly IDialogueSelector _selector;
    private readonly IDialogueRenderer _renderer;

    public KojoEngine(
        IDialogueLoader loader,
        IDialogueSelector selector,
        IDialogueRenderer renderer)
    {
        _loader = loader;
        _selector = selector;
        _renderer = renderer;
    }

    public Result<string> GetDialogue(CharacterId character, ComId com, IEvaluationContext ctx)
    {
        var fileResult = _loader.Load(GetPath(character, com));
        if (fileResult is Result<DialogueFile>.Failure f1)
            return Result<string>.Fail(f1.Error);

        var file = ((Result<DialogueFile>.Success)fileResult).Value;
        var entryResult = _selector.Select(file.Entries, ctx);
        if (entryResult is Result<DialogueEntry>.Failure f2)
            return Result<string>.Fail(f2.Error);

        var entry = ((Result<DialogueEntry>.Success)entryResult).Value;
        return _renderer.Render(entry.Template, ctx);
    }
}
```

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Dialogue/Loading/IDialogueLoader.cs` | 読み込みインターフェース |
| `src/Era.Core/Dialogue/Loading/YamlDialogueLoader.cs` | YAML読み込み実装 |
| `src/Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs` | 条件評価インターフェース |
| `src/Era.Core/Dialogue/Evaluation/ConditionEvaluator.cs` | 条件評価実装 |
| `src/Era.Core/Dialogue/Specifications/*.cs` | Specification Pattern実装 |
| `src/Era.Core/Dialogue/Rendering/IDialogueRenderer.cs` | 描画インターフェース |
| `src/Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs` | テンプレート展開実装 |
| `src/Era.Core/Dialogue/Selection/IDialogueSelector.cs` | 選択インターフェース |
| `src/Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs` | 優先度選択実装 |
| `src/Era.Core/Dialogue/KojoEngine.cs` | Facade（リファクタ後） |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=Dialogue"
# Each component tested independently
# Specification combinations tested
# KojoEngine facade integration tested
```

**Success Criteria**:
- [x] KojoEngine SRP 分割完了
- [x] 各コンポーネント独立テスト PASS
- [x] Specification Pattern 確立

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 18: KojoEngine SRP」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 18 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 19: Kojo Conversion (was Phase 18)

**Phase Status**: DONE

**Goal**: All ERB kojo converted to YAML

**Related Features**: F344 (Codebase Analysis), F345 (Phase 1 Breakdown), F346-F353 (Phase 1 Tools), F352 (Phase 2 Planning)

> **✅ F352 Planning Complete (2026-01-09)**: Phase 16 計画は本セクションに統合済み。
> F352 は [DONE]。Phase 16 開始時は本セクションを参照し、Initialization Tasks を実行すること。

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **KojoEngineインターフェース（Phase 12で定義済み）を使用**:
> - YAMLファイルは `IKojoEngine.GetDialogue()` 経由でアクセス
> - 変換後のYAMLはPhase 4のStrongly Typed IDsを参照
>
> **YAMLスキーマでの型安全**:
> ```yaml
> # 型参照はPhase 4のIDを使用
> conditions:
>   - type: talent
>     character: !CharacterId 1  # 人物_美鈴
>     talent: !TalentId 100      # TALENT番号
> ```
>
> **バッチ変換ツールのDI対応**:
> - `IErbParser`, `IYamlGenerator` をDI注入
> - Result型でエラーハンドリング

> **Cross-Reference Note**: F344's "Phase 2: Core Migration" corresponds to this Phase 12.
> F352 creates F354-F357 as concrete feature specifications for this phase.

**Strategy**: Automated conversion with manual review workflow
- Automatic: ~70-80% (clean DATALIST patterns)
- Manual: ~20-30% (edge cases, complex expressions)

**Scope Policy**:
- **Migration対象**: 既存ERB 117ファイルのみ
- **空スタブ**: 作成しない（存在しないCOMの口上は不要）
- **新規コンテンツ**: Phase 16完了後はYAML直接作成（ERB経由しない）

---

#### Phase 1 Pilot Results (F351)

**Pilot Target**: `Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB` → `COM_0.yaml`

**Known Limitations**:
| Issue | Impact | Workaround | Phase 12 Solution |
|-------|--------|------------|-------------------|
| ErbParser: `PRINTDATA...ENDDATA` unsupported | Cannot parse DATALIST blocks | Regex extraction in F351 test | **F354: PRINTDATA Parser Extension** |

**Pilot Deliverables**:
| Tool | Feature | Status |
|------|---------|:------:|
| F347 | TALENT Branching Extractor | ✅ |
| F348 | YAML Schema Generator | ✅ |
| F349 | DATALIST→YAML Converter | ✅ |
| F350 | YAML Dialogue Renderer | ✅ |
| F351 | Pilot Conversion (美鈴 COM_0) | ✅ |
| F353 | CFLAG/Function Condition Extractor | ✅ |

---

#### Phase 12 Sub-Features (Created by F352)

| Feature | Type | Scope | Dependency |
|---------|------|-------|------------|
| F354 | engine | PRINTDATA Parser Extension - add `PRINTDATA...ENDDATA` parsing to ErbParser | Blocks F355 |
| F355 | engine | Batch Conversion Tool - automate ErbToYaml for directory processing | Requires F354 |
| F356 | infra | Character Migration Framework - per-character workflow and tracking | - |
| F357 | erb | NTR System Integration - integrate YAML dialogues with NTR system | - |

**Migration Status Tracking**: `pm/migration-status.md`

---

#### Phase 16 Initialization Tasks (from F352)

> **Phase 16 開始時に実行**（Phase 15 Post-Phase Review 完了後）

| Task# | Type | Description | Output |
|:-----:|:----:|-------------|--------|
| 0 | infra | Create migration-status.md tracking file | `pm/migration-status.md` |
| 0.5 | infra | **COM Number ↔ Command Name Mapping SSOT** - `com_file_map.json` 拡張 | `tools/kojo-mapper/com_file_map.json` |
| | | - COM番号 → セマンティック名（英語: `Kiss`, `Caress`, 日本語: `キス`, `愛撫`） | |
| | | - eraTW 参照は COM 番号で継続（外部リポジトリ変更なし） | |
| | | - kojo-init Feature 命名をセマンティック名に移行可能にする | |
| | | - C# クラス名との対応を明確化（`[ComId(N)]` 属性と併用） | |
| 1 | research | Create F354 PRINTDATA Parser Extension feature | `pm/features/feature-354.md` |
| 2 | research | Create F355 Batch Conversion Tool feature | `pm/features/feature-355.md` |
| 3 | research | Create F356 Character Migration Framework feature | `pm/features/feature-356.md` |
| 4 | research | Create F357 NTR System Integration feature (optional) | `pm/features/feature-357.md` |
| 5 | infra | Update index-features.md with Phase 16 features | - |

**migration-status.md Template**:
```markdown
# Migration Status

## Summary
| Status | Count |
|--------|------:|
| Auto | 0 |
| Manual | 0 |
| Failed | 0 |
| Pending | 117 |

## Per-Character Status

| Character | COM | Status | Notes |
|-----------|-----|:------:|-------|
| K1 | COM_0 | pending | - |
| K1 | COM_1 | pending | - |
...
```

---

**Kojo Utility Files** (Root level - migrate before character kojo):

| File | Lines | Purpose |
|------|------:|---------|
| `LUNA_KOJO.ERB` | 162 | 汎用「ルナ」スタイル口上 |
| `汎用口上ランダム生成.ERB` | 451 | ランダム口上ジェネレータ |
| `客口上.ERB` | ~100 | ゲスト/客向け口上 |
| `客汎用地の文.ERB` | ~100 | ゲスト汎用テキスト |
| `口上/U_汎用/KOJO_MODIFIER_COMMON.ERB` | ~200 | 口上修飾子共通処理 |

**Scope**:

| Character | Directory | Files | Complexity |
|-----------|-----------|:-----:|:----------:|
| 美鈴 (Meiling) | 1_美鈴/ | 11 | High |
| 小悪魔 (Koakuma) | 2_小悪魔/ | 11 | High |
| パチュリー (Patchouli) | 3_パチュリー/ | 11 | High |
| 咲夜 (Sakuya) | 4_咲夜/ | 15 | High |
| レミリア (Remilia) | 5_レミリア/ | 10 | High |
| フラン (Flandre) | 6_フラン/ | 10 | High |
| 子悪魔 (ChldAkuma) | 7_子悪魔/ | 7 | High |
| チルノ (Cirno) | 8_チルノ/ | 10 | High |
| 大妖精 (Great Fairy) | 9_大妖精/ | 11 | High |
| 魔理沙 (Marisa) | 10_魔理沙/ | 9 | High |
| 汎用 (Generic) | U_汎用/ | 12 | Medium |
| **Total** | | **117** | |

**Tasks**:
1. Extend `tools/ErbToYaml/` with batch conversion mode (Phase 1 creates single-function mode)
2. Convert all K1-K10 kojo files (105 files)
3. Convert generic kojo (U_汎用)
4. Validate YAML against schema
5. Verify dialogue output matches ERB using KojoComparer (Phase 2)
6. **Implement Kojo Quality Validator** (`tools/KojoQualityValidator/`) - コンテンツ品質ルールの機械的検証
   - 品質ルール検証: 4分岐 × 4種類 × 4行の最低要件チェック
   - 差分検証モード: `--diff HEAD~1` で変更ファイルのみ検証
   - ファイル指定モード: `--files tea/*.yaml` で特定ファイルのみ検証
   - CI統合: `git diff --name-only | xargs dotnet run --project tools/KojoQualityValidator`
7. Manual review and correction of edge cases
   - COM 94 K3 (パチュリー): アナル参照の明示化
   - COM 94 K8 (チルノ): コメント方向修正 + アナル参照の明示化
8. **Create Phase 16 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 16 セクションとの整合確認必須
9. **Create Phase 17 Planning feature** (type: research, include transition feature tasks)

**Tool Extensions in This Phase**:
| Tool | Extension |
|------|-----------|
| `tools/ErbToYaml/` | Batch mode: `--batch Game/ERB/口上/` for 117 files |
| `tools/KojoComparer/` | Batch verification: `--all` for full coverage |
| `tools/KojoQualityValidator/` | Quality rules + incremental validation: `--diff`, `--files` |

**Batch Converter**:
```csharp
// tools/ErbToYaml/Program.cs (batch mode extension)
public class ErbToYamlConverter
{
    public void ConvertBatch(string directory, string outputDir)
    {
        foreach (var erbFile in Directory.GetFiles(directory, "*.ERB", SearchOption.AllDirectories))
        {
            var erb = ErbParser.Parse(erbFile);
            var yaml = TransformToYaml(erb);
            var outputPath = GetOutputPath(erbFile, outputDir);
            File.WriteAllText(outputPath, yaml);
        }
    }
}
```

**Verification**:
```bash
# Batch conversion
dotnet run --project tools/ErbToYaml -- --batch "Game/ERB/口上/" --output "Game/content/kojo/"
# Full equivalence check using KojoComparer (Phase 2)
dotnet run --project tools/KojoComparer -- --all
# Output: 117/117 MATCH (or diff details for failures)
```

**Kojo Quality Validator** (Task 6):

品質ルール定義とインクリメンタル検証を行うツール。

| Quality Rule | Description | Schema Enforcement |
|--------------|-------------|:------------------:|
| 4+ branches | 4種類以上の分岐（TALENT/Route等） | `branches: minItems: 4` |
| 4+ variations per branch | 分岐毎に4種類以上のバリエーション | `variations: minItems: 4` |
| 4+ lines per variation | バリエーション毎に4行以上の台詞 | `lines: minItems: 4` |

```csharp
// tools/KojoQualityValidator/QualityValidator.cs
public class KojoQualityValidator
{
    public record QualityRule(int MinBranches = 4, int MinVariations = 4, int MinLines = 4);

    public ValidationResult Validate(KojoFile kojo, QualityRule rule)
    {
        var errors = new List<string>();

        if (kojo.Dialogue.Count < rule.MinBranches)
            errors.Add($"Branch count {kojo.Dialogue.Count} < {rule.MinBranches}");

        foreach (var (branch, i) in kojo.Dialogue.Select((b, i) => (b, i)))
        {
            if (branch.Variations.Count < rule.MinVariations)
                errors.Add($"Branch[{i}]: Variations {branch.Variations.Count} < {rule.MinVariations}");

            foreach (var (variation, j) in branch.Variations.Select((v, j) => (v, j)))
            {
                if (variation.Lines.Count < rule.MinLines)
                    errors.Add($"Branch[{i}].Variation[{j}]: Lines {variation.Lines.Count} < {rule.MinLines}");
            }
        }

        return new ValidationResult(kojo.Path, errors);
    }
}
```

**Usage Examples**:
```bash
# 差分検証: 直近コミットで変更されたファイルのみ
dotnet run --project tools/KojoQualityValidator -- --diff HEAD~1

# ファイル指定: 特定コマンドの口上のみ（例: お茶コマンド10キャラ）
dotnet run --project tools/KojoQualityValidator -- --files "Game/content/kojo/tea/*.yaml"

# CI統合: PR で変更されたファイルのみ検証
git diff --name-only origin/main -- '*.yaml' | xargs dotnet run --project tools/KojoQualityValidator

# カスタムルール: 最低要件を変更
dotnet run --project tools/KojoQualityValidator -- --min-branches 3 --min-variations 2 --min-lines 3
```

**Output Example**:
```
Validating 10 files (changed since HEAD~1)...

✓ tea/meiling.yaml     4 branches × 4+ variations × 4+ lines
✓ tea/sakuya.yaml      4 branches × 4+ variations × 4+ lines
✗ tea/flandre.yaml     Branch[2]: Variations 3 < 4
✗ tea/patchouli.yaml   Branch[1].Variation[0]: Lines 3 < 4

Result: 8/10 PASS, 2/10 FAIL
```

**Success Criteria**:
- [x] 117 ERB 口上ファイル変換完了
- [x] KojoComparer 全ファイル MATCH
- [x] YAML スキーマ検証 PASS
- [x] KojoQualityValidator 実装完了（差分検証・ファイル指定モード）
- [x] 品質ルール（4分岐×4種類×4行）の機械的検証可能

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 19: Kojo Conversion」を継承 | Grep |
| **Tasks: 等価性検証** | ERB→YAML 等価性テストを含む | KojoComparer MATCH |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 19 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

---

#### Test Infrastructure Transition (Phase 19 Completion Trigger)

**Trigger**: Phase 19 (Kojo Conversion) 完了時に実行

**Rationale**: ERB口上 → YAML 変換完了後、テスト基盤も ERB ベースから C#/YAML ベースに移行が必要。

**変更対象と内容**:

| 対象 | 現在 (ERB) | 移行後 (C#/YAML) | 変更タイミング |
|------|------------|------------------|----------------|
| `.githooks/pre-commit` | `--flow tests/regression/` | `dotnet test src/Era.Core.Tests/` | Phase 19 完了時 |
| `.githooks/pre-commit` | ErbLinter | (削除) | Phase 20 完了時 |
| `.githooks/pre-commit` | `verify_com_map.py` | YAML schema validation | Phase 20 完了時 |
| `.claude/commands/do.md` | Phase 3: `--unit` / `--flow` | Phase 3: `dotnet test` | Phase 20 完了時 |
| `.claude/commands/do.md` | Phase 6: `kojo_test_gen.py` | Phase 6: YAML test runner | Phase 20 完了時 |
| `.claude/skills/testing/SKILL.md` | `--unit`, `--flow` コマンド | `dotnet test` コマンド | Phase 20 完了時 |
| `.claude/skills/testing/SKILL.md` | ErbLinter セクション | (削除) | Phase 20 完了時 |
| `.claude/skills/testing/KOJO.md` | `kojo_test_gen.py` | YAML equivalence test | Phase 20 完了時 |

**実行手順** (Phase 20 Post-Phase Review Feature の Tasks として):

1. **pre-commit hook 更新**:
   ```bash
   # 旧
   [1/4] regression tests (--flow)
   [2/4] ErbLinter
   [3/4] COM_FILE_MAP verification
   [4/4] verify-logs.py

   # 新
   [1/3] C# unit tests (dotnet test src/Era.Core.Tests/)
   [2/3] YAML schema validation
   [3/3] verify-logs.py
   ```

2. **do.md Phase 3 (TDD) 更新**:
   - `--unit` → `dotnet test --filter` (C# unit tests)
   - `--flow` → `dotnet test` (integration tests)
   - kojo type: YAML schema validation

3. **do.md Phase 6 (Verification) 更新**:
   - ErbLinter 削除
   - BOM check 削除 (YAML は UTF-8)
   - kojo_test_gen.py → KojoComparer batch verification

4. **testing SKILL 更新**:
   - Quick Reference: `--unit`/`--flow` → `dotnet test`
   - AC Types: Method 列を C# テストに変更
   - ERB 固有セクション削除 (ErbLinter, BOM, etc.)

5. **testing/KOJO.md 更新**:
   - kojo_test_gen.py → YAML equivalence tests
   - DATALIST 形式 → YAML dialogue 形式

6. **Legacy test infrastructure 削除/アーカイブ**:
   - `test/regression/` → `test/_archived/regression/` (24 JSON scenarios)
   - `_out/logs/prod/regression/` → 削除 (result files)
   - `tools/verify-logs.py` → regression 関連コード削除 (`verify_regression_logs` 関数、`scope="regression"` 分岐)
     - **維持**: `verify_engine_logs` (TRX/C#テスト結果)、`verify_ac_logs` (AC検証結果)

7. **C# テスト結果の TRX 出力修正** (Phase 14 からの引き継ぎ):
   - **問題**: `dotnet test --filter` が TRX をログディレクトリに出力していない
   - **現状**: `Feature-{ID}: OK:0/0` (verify-logs.py が空振り)
   - **修正**: do.md Phase 6 で TRX 出力パス指定を追加
     ```bash
     dotnet test --filter {pattern} \
       --logger "trx;LogFileName=ac{N}-{name}.trx" \
       --results-directory _out/logs/prod/ac/engine/feature-{ID}/
     ```
   - **対象ファイル**: `.claude/commands/do.md` Phase 6 Verification セクション

**AC for Test Infrastructure Transition** (Phase 20 Post-Phase Review に追加):

| AC# | Description | Type | Matcher | Expected |
|:---:|-------------|------|---------|----------|
| N+1 | pre-commit uses dotnet test | code | contains | `dotnet test Era.Core.Tests` |
| N+2 | pre-commit removes ErbLinter | code | not_contains | `ErbLinter` |
| N+3 | do.md Phase 3 uses dotnet test | code | contains | `dotnet test` |
| N+4 | testing SKILL removes --unit/--flow | code | not_contains | `--unit path/` |
| N+5 | verify-logs.py removes regression | code | not_contains | `verify_regression_logs` |
| N+6 | regression tests archived | file | not_exists | `test/regression/` |
| N+7 | do.md outputs TRX to logs | code | contains | `--results-directory _out/logs/prod/ac/engine` |

### Obligation Status (as of F786 completion)

| AC | Description | Status | Destination/Reference |
|:--:|-------------|--------|----------------------|
| N+1 | pre-commit scope expansion | DONE | F784 AC#6 |
| N+2 | pre-commit removes ErbLinter | DONE | Already satisfied (F786 AC#1 verification) |
| N+3 | do.md Phase 3 uses dotnet test | OBSOLETE | do.md replaced by /run command |
| N+4 | testing SKILL removes --unit/--flow | DEFERRED | F782 (--unit actively used; expanded scope: 5+ files) |
| N+5 | verify-logs.py removes regression | DONE | F786 AC#3-5, AC#8-9 |
| N+6 | regression tests archived | DONE | Already satisfied (_archived/regression/ exists, regression/ absent) |
| N+7 | do.md outputs TRX to logs | OBSOLETE | do.md replaced by /run command (Phase 7 TRX output) |

**CRITICAL**: Phase 20 完了前にこれらの変更を行ってはいけない。ERB テスト基盤は Phase 20 完了まで必要。

---
