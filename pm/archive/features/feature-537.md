# Feature 537: Transform Rules Migration - Phase 17

## Status: [CANCELLED]

> **Cancellation Reason**: Per F562/F563 architecture analysis, Transform Rules migration is unnecessary. _Rename.csv contains zero data (template comments only), _Replace.csv contains only 1 value (汚れの初期値) which should be consolidated to C# ConfigData. YAML migration serves no purpose for empty/minimal files. See F562 (Architecture Analysis) and F563 (Full COM YAML Migration) for details.

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

## Type: engine

## Background

### Philosophy (Mid-term Vision)

Phase 17: Data Migration - Establish YAML as the single source of truth for all configuration data across Era.Core, ensuring consistent data access patterns, type safety, and schema validation while eliminating CSV parsing technical debt from legacy Emuera CSV format throughout the entire codebase.

### Problem (Current Issue)

Transform rule definition files (_Rename.csv and _Replace.csv) remain in legacy CSV format, creating inconsistency with the YAML migration pattern established in F528. These files define critical display transformation logic (money unit formatting, character name mappings, initial state values) but lack type safety, schema validation, and modern data access patterns. Current GlobalStatic direct access prevents unit testing and violates the IDataLoader pattern established by Phase 17 architecture.

### Goal (What to Achieve)

Migrate _Rename.csv and _Replace.csv to YAML format with type-safe data models, implement ITransformRuleLoader interface following F528 precedent, establish DI registration patterns, and achieve 100% behavioral equivalence verification ensuring zero regression in display transformation logic functionality.

**Dependencies**: Requires F528 (Critical Config Files Migration) for IDataLoader pattern and migration toolchain.

**Total Volume**: 2 CSV files (_Rename.csv, _Replace.csv), estimated ~200 lines of migration code.

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | _Rename.yaml exists | file | Glob | exists | "Game/config/_rename.yaml" | [ ] |
| 2 | _Replace.yaml exists | file | Glob | exists | "Game/config/_replace.yaml" | [ ] |
| 3 | ITransformRuleLoader interface | file | Glob | exists | "Era.Core/Data/ITransformRuleLoader.cs" | [ ] |
| 4 | TransformRuleLoader implementation | file | Glob | exists | "Era.Core/Data/TransformRuleLoader.cs" | [ ] |
| 5 | TransformRule data models | code | Grep | contains | "public.*class RenameRule" | [ ] |
| 6 | ReplaceRule data models | code | Grep | contains | "public.*class ReplaceRule" | [ ] |
| 7 | DI registration | file | Grep | contains | "AddSingleton.*ITransformRuleLoader.*TransformRuleLoader" | [ ] |
| 8 | Rename rule loading test | test | Bash | succeeds | "dotnet test --filter TestRenameRuleLoading" | [ ] |
| 9 | Replace rule loading test | test | Bash | succeeds | "dotnet test --filter TestReplaceRuleLoading" | [ ] |
| 10 | Schema validation test | test | Bash | succeeds | "dotnet test --filter TestTransformRuleSchema" | [ ] |
| 11 | Equivalence verification | test | Bash | succeeds | "dotnet test --filter TestTransformRuleEquivalence" | [ ] |
| 12 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [ ] |

### AC Details

**AC#1**: _Rename.yaml file creation
- Test: Glob pattern="Game/config/_rename.yaml"
- Expected: File exists in config directory structure

**AC#2**: _Replace.yaml file creation
- Test: Glob pattern="Game/config/_replace.yaml"
- Expected: File exists in config directory structure

**AC#3**: ITransformRuleLoader interface definition
- Test: Glob pattern="Era.Core/Data/ITransformRuleLoader.cs"
- Expected: Interface file exists following F528 IDataLoader pattern

**AC#4**: TransformRuleLoader implementation
- Test: Glob pattern="Era.Core/Data/TransformRuleLoader.cs"
- Expected: Implementation class exists

**AC#5**: RenameRule data model verification
- Test: Grep pattern="public.*class RenameRule" path="Era.Core/Data/"
- Expected: Contains RenameRule class definition with proper properties

**AC#6**: ReplaceRule data model verification
- Test: Grep pattern="public.*class ReplaceRule" path="Era.Core/Data/"
- Expected: Contains ReplaceRule class definition with proper properties

**AC#7**: DI registration verification
- Test: Grep pattern="AddSingleton.*ITransformRuleLoader.*TransformRuleLoader" path="Era.Core/DependencyInjection/"
- Expected: Contains DI registration following F528 pattern

**AC#8**: Rename rule loading functionality
- Test: dotnet test --filter TestRenameRuleLoading
- Expected: Test passes - empty rule list handling (no active rules in _Rename.csv)
- Minimum: 2 Assert statements validating empty collection handling

**AC#9**: Replace rule loading functionality
- Test: dotnet test --filter TestReplaceRuleLoading
- Expected: Test passes - 汚れの初期値 rule loaded correctly
- Minimum: 3 Assert statements validating single rule parsing

**AC#10**: Schema validation functionality
- Test: dotnet test --filter TestTransformRuleSchema
- Expected: Test passes - YAML structure validation against schema
- Minimum: 2 Assert statements (valid/invalid YAML)

**AC#11**: Behavioral equivalence verification
- Test: dotnet test --filter TestTransformRuleEquivalence
- Expected: Test passes - CSV vs YAML data produces identical transform results
- Minimum: 5 Assert statements covering all transform rule types

**AC#12**: Zero technical debt verification
- Test: Grep pattern="TODO|FIXME|HACK" paths=[Era.Core/Data/ITransformRuleLoader.cs, Era.Core/Data/TransformRuleLoader.cs]
- Expected: 0 matches across all feature implementation files

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Convert CSV files to YAML format manually following F528 pattern | [ ] |
| 2 | 3 | Create ITransformRuleLoader interface following F528 pattern | [ ] |
| 3 | 4,5,6 | Implement TransformRuleLoader with RenameRule and ReplaceRule data models | [ ] |
| 4 | 7 | Register ITransformRuleLoader in DI container | [ ] |
| 5 | 8,9,10 | Create unit tests for rule loading and schema validation | [ ] |
| 6 | 11 | Implement equivalence verification tests comparing CSV vs YAML behavior | [ ] |
| 7 | 12 | Verify zero technical debt across all implementation files | [ ] |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Data Models

Following F528 pattern for strongly typed configuration data:

```csharp
// Era.Core/Data/TransformRules.cs
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>Character/attribute rename mapping rule</summary>
public class RenameRule
{
    public string Source { get; init; }     // Original game string (e.g., "TALENT:110")
    public string Target { get; init; }     // Display name (e.g., "巨乳")
}

/// <summary>Display replacement/formatting rule</summary>
public class ReplaceRule
{
    public string Category { get; init; }   // Rule category (e.g., "汚れの初期値")
    public string Value { get; init; }      // Replacement value or configuration
}
```

### Interface Definition

```csharp
// Era.Core/Data/ITransformRuleLoader.cs
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>Provides access to display transformation rules</summary>
public interface ITransformRuleLoader
{
    /// <summary>Get all rename mapping rules</summary>
    Result<IReadOnlyList<RenameRule>> GetRenameRules();

    /// <summary>Get all replacement formatting rules</summary>
    Result<IReadOnlyList<ReplaceRule>> GetReplaceRules();
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<ITransformRuleLoader, TransformRuleLoader>();
```

### Test Naming Convention

Test methods follow `Test{Functionality}{Type}` format (e.g., `TestRenameRuleLoading`, `TestReplaceRuleEquivalence`). This ensures AC filter patterns match correctly.

### Migration Source Reference

**Legacy Location**: `Game/CSV/_Rename.csv`, `Game/CSV/_Replace.csv`

| File | Content Type | Actual Content |
|------|--------------|----------------|
| _Rename.csv | Character/talent mappings | No active rules (only template comments) |
| _Replace.csv | Display formatting | 1 rule: 汚れの初期値, 0/0/2/1/8/0/1/8 |

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Prerequisite | F528 | [DONE] | Critical Config Files Migration - establishes IDataLoader pattern and migration toolchain |

## Links

- [index-features.md](index-features.md)
- [feature-516.md](feature-516.md) - Phase 17 Planning (parent feature)
- [feature-528.md](feature-528.md) - Critical Config Files Migration (prerequisite)
- [feature-534.md](feature-534.md) - Content Definition CSVs Migration (related)
- [feature-535.md](feature-535.md) - Content Definition CSVs Migration Part 2 (related)
- [feature-562.md](feature-562.md) - Architecture Revision (spawned from this FL review)

## 引継ぎ先指定 (Mandatory Handoffs)

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| Architecture revision | FL Review revealed fundamental C#/YAML separation question | Created new feature | F562 |
| F537 final disposition | Pending architecture decision from F562 | Wait for F562 completion | F562 dependency |

---

## FL Review: Architecture Discussion (2026-01-19)

### 発端: CSVファイルの実態調査

FL Review Phase 1で、移行対象CSVの実態を調査:

| ファイル | 想定 | 実態 |
|----------|------|------|
| _Rename.csv | 複数のリネームルール | **ゼロ** (コメントテンプレートのみ) |
| _Replace.csv | 複数の置換ルール | **1件のみ** (汚れの初期値) |

**結論**: 移行すべき実データがほぼ存在しない。

### 質問1: 「エンジンに直接組み込めば？」

`_Replace.csv`の唯一のルール:
```
汚れの初期値, 0/0/2/1/8/0/1/8
```

これはConfigData.csのハードコード値をオーバーライドしている:
```csharp
// 現在: { 0, 0, 2, 1, 8 } (5個)
// CSV: 0/0/2/1/8/0/1/8 (8個)
```

**提案**: YAMLに移行するのではなく、ConfigData.csを直接修正すれば完了。

### 質問2: 「他のCSVも同様に無駄な変換をしているか？」

Phase 17全体（43ファイル）を調査:

| カテゴリ | ファイル数 | 結果 |
|---------|:---------:|------|
| EMPTY (テンプレートのみ) | 2 | _Rename.csv, _Replace.csv のみ |
| MINIMAL (1-5件) | 8 | 少量だが実データあり |
| NORMAL (6件以上) | 34 | 通常のデータ量 |

**結論**: 「無駄な変換」はF537のみ。

### 質問3: 「静的データはC#に組み込むべきでは？」

ユーザーの指摘:
> 「Moddingはゴリゴリに行うが、その場合もC#をいじることが正しいのでは？」

#### 調査: architecture.mdの方針

architecture.mdは「データとコードの分離」を明示:
- Phase 17: 「CSV/定義データをYAML/JSONに変換」
- IDataLoaderパターン: 静的データをファイルから読み込む設計

#### 調査: C#ゲームのベストプラクティス

外部ファイルを推奨する理由:
- バランス調整のホットスワップ
- 非プログラマによるコンテンツ編集
- Mod対応

#### しかし、ユーザーの反論が正しい

> 「YAMLにデータを追加しても、C#がなければ意味がない」

**例**: Talent.yamlに「新タレント: 超巨乳」を追加
1. ゲームは読み込む ✅
2. C#コードに「超巨乳の処理」がない ❌
3. 何も起きない ❌

**結論**: 「ファントムModdability」- YAMLは偽のMod可能性を与えている。

### Moddabilityの実態

| コンテンツ | YAMLで追加 | C#変更なしで動作 | 実質Moddable |
|-----------|:----------:|:---------------:|:------------:|
| 口上（新台詞） | ✅ | ✅ | **✅ YES** |
| 既存キャラのステータス変更 | ✅ | ✅ | ✅ YES |
| 新キャラ追加（既存Talent組み合わせ） | ✅ | ✅ | **✅ YES** |
| 新Talent追加 | ✅ | ❌ 処理コードなし | **❌ NO** |
| 新Ability追加 | ✅ | ❌ 処理コードなし | **❌ NO** |
| ゲームループ変更 | - | ❌ | **❌ NO** |

### 新しいデータ配置方針（提案）

| カテゴリ | 形式 | 理由 |
|---------|------|------|
| **口上** | YAML | 唯一の真のデータ駆動コンテンツ |
| **キャラ定義** | YAML | 既存要素の組み合わせ、Mod対象 |
| **Talent/Ability定義** | C# enum | 挙動定義にはC#コード必要 |
| **フラグ定義** | C# enum | 型安全性 |
| **数値設定** | C# const | ゲームメカニクス |

### Phase 17再設計案

| Feature | 現計画 | 新計画 |
|---------|--------|--------|
| F528 | VariableSize → YAML | → **C#定数** |
| F529 | FLAG定義 → YAML | → **C# enum** |
| F530 | Talent/Abl定義 → YAML | → **C# enum** |
| F532-533 | Chara*.csv → YAML | → **YAML維持** ✅ |
| F534-536 | Train/Item等 → YAML | → **C#定数 or enum** |
| F537 | Transform Rules → YAML | → **キャンセル、エンジン修正** |

### F537の推奨アクション

**オプションA: キャンセルして最小修正**
1. _Rename.csv → 削除 (データなし)
2. _Replace.csv → ConfigData.csに汚れの初期値を統合
3. F537をキャンセル

**オプションB: F537を「Transform Rules Cleanup」に再定義**
- AC1: ConfigData.csのStainDefault値を8個に更新
- AC2: _Rename.csv削除
- AC3: _Replace.csv削除
- AC4: emuera.configから関連設定削除
- AC5: ビルド成功

### 未決事項

1. **Phase 17全体の方針変更が必要か？**
   - 「YAML移行」→「C#組み込み + 口上/キャラのみYAML」
   - F516 (Phase 17 Planning) の更新が必要

2. **architecture.mdの更新が必要か？**
   - 「データとコードの分離」原則の再定義
   - Moddabilityの現実的な範囲の明記

---

## FL Review: Architecture Discussion Part 2 (2026-01-19)

### 議論の深化: 「拡張性」とは誰のためか？

ユーザーからの問い:
> 「COMなども含めてYAML化すればコミュニティの改良を受けられるということか？この拡張性はMODDERではなく、開発者側の視点だったということだろうか？」

#### 「拡張性」の2つの意味

| 視点 | 拡張性の意味 | 最適な形式 |
|------|------------|-----------|
| **開発者** | 型安全、リファクタリング、テスト容易性 | C# |
| **コミュニティ** | 誰でも参加可能、ビルド不要、PRなしで配布 | YAML/外部ファイル |

**結論**: 以前の「C#推奨」は**開発者の生産性向上**であり、**コミュニティ参加の容易さ**ではなかった。

#### ERA系ゲームの特殊性

```
一般的なゲーム開発:
  コア開発チーム >> コミュニティ貢献

ERA系ゲーム:
  コア開発チーム << コミュニティ貢献
```

C#化の隠れたコスト = **コミュニティ参加の敷居を大幅に上げた**

---

### COMのYAML化可能性調査

ユーザーからの問い:
> 「YAML化することを考える。COMはロジックへの影響が大きいか？単純にYAMLをいじるだけでは全く新しいCOMは作れないか？」

#### COM構造分析（185ファイル）

```
COM = {
  実行判定 (CAN_COM): 条件チェック + 閾値判定
  効果 (COM): SOURCE/DOWNBASE/EXP の変更
  口上: 別ファイル（YAML化済み）
}
```

#### COMのYAML表現例

```yaml
# Game/data/coms/com101_鞭.yaml
id: 101
name: "鞭"
category: "SM"
cost:
  stamina: 80
  energy: 50
effects:
  - type: source
    pain: 1500
    fear: 1200
  - type: source_scale
    target: pain
    formula: "value * (10 + 5 * max(getPalamLv(pain, 5) - 1, 0)) / 10"
```

#### 結論

| 拡張内容 | 必要な作業 |
|---------|-----------|
| 新COM（既存効果の組み合わせ） | **YAMLファイル追加のみ** |
| 新効果タイプ | C#でハンドラー追加 + YAML対応 |
| 新条件タイプ | C#で評価器追加 + YAML対応 |

**80%のCOMはYAMLのみで作成可能。**

---

### C#の恩恵の再確認

ユーザーからの問い:
> 「C#の恩恵はなんだっけ。型安全性？テスト容易性？」

| 恩恵 | 説明 | YAML化後も維持？ |
|------|------|:----------------:|
| 型安全性 | コンパイル時にエラー検出 | ⚠️ エンジン部分のみ |
| テスト容易性 | ユニットテスト、モック | ✅ YAML含めてテスト可能 |
| IDE支援 | リファクタリング、自動補完 | ⚠️ エンジン部分のみ |
| パフォーマンス | コンパイル済みコード | ⚠️ YAML解釈のオーバーヘッド |

**両立案**: C#エンジン（開発者が書く）+ YAMLコンテンツ（コミュニティも書ける）

---

### 根本的な問い: ERB時代と何が変わる？

ユーザーからの問い:
> 「ERB時代と何が本質的に変わる？このC# migrationで得たメリットとは？」

#### ERB時代 vs YAML + C#エンジン

| 観点 | ERB時代 | YAML + C#エンジン |
|------|---------|-------------------|
| コンテンツ形式 | ERB (独自スクリプト) | YAML (標準フォーマット) |
| インタプリタ | Emuera (古い、メンテ困難) | C#エンジン (モダン、テスト可能) |
| ロジックとデータ | **混在** | **分離** |
| スキーマ検証 | なし | JSON Schema |
| コミュニティ参加 | ✅ 容易 | ✅ 容易（維持可能） |

#### C# migrationの本質

```
C# migration の本質 =
  「より良いインタプリタを作った」+
  「コンテンツ形式をより構造化した」

コンテンツ作成のワークフローは ERB時代と本質的に同じ:
  テキストファイルを編集 → ゲームで動作確認
```

#### 本当に得たメリット

| メリット | 説明 |
|---------|------|
| **エンジンの品質向上** | Emueraより保守・拡張しやすい |
| **テスタビリティ** | エンジン + コンテンツ両方テスト可能 |
| **関心の分離** | ロジック(C#) と データ(YAML) の明確な分離 |
| **スキーマによる早期エラー検出** | 実行前にコンテンツエラーを発見 |
| **モダン開発環境** | IDE、デバッガ、プロファイラ |

#### 結論

**変わったこと**: エンジンの品質、開発者体験
**変わらないこと**: コミュニティ参加の容易さ（維持できる）

これは**良いこと**。ERB時代の良さ（コミュニティ参加）を維持しながら、エンジン品質を向上させる。

---

### 次のアクション

ユーザーの決定:
> 「これはmigrationの大規模な変更が必要だ。architecture.mdと既存の実装を深く読み込み、architecture.mdを更新するfeatureが必要だ。」

**必要なFeature**:
- architecture.md の深い分析
- 既存実装との整合性確認
- 新しいアーキテクチャ方針の策定:
  - C#エンジン + YAMLコンテンツの分離
  - コミュニティModdabilityの明確化
  - Phase 17以降の計画修正

→ **F562 (Architecture Revision: C# Engine + YAML Content Separation) 作成**

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | Created Feature 537 from Phase 17 Planning, Low priority, engine type, depends on F528 | PROPOSED |
| 2026-01-19 | fl-review | orchestrator | Phase 0: Reference Check passed | OK |
| 2026-01-19 | fl-review | feature-reviewer | Phase 1: Review - found critical issues (empty CSV, wrong assumptions) | NEEDS_REVISION |
| 2026-01-19 | fl-review | orchestrator | Applied fixes: F528 status, data model pattern, AC patterns, task description | FIXED |
| 2026-01-19 | fl-review | Explore | Analyzed all Phase 17 CSVs - only F537 files are EMPTY | MINIMAL_SCOPE |
| 2026-01-19 | fl-review | orchestrator | User raised fundamental question: Should static data be in C#? | ARCHITECTURE_DISCUSSION |
| 2026-01-19 | fl-review | Explore x3 | Investigated: architecture.md, C# best practices, kojo embedding | COMPLETED |
| 2026-01-19 | fl-review | orchestrator | Concluded: "Phantom Moddability" - YAML gives false sense of mod capability | INSIGHT |
| 2026-01-19 | fl-review | orchestrator | New proposal: Kojo + Chara = YAML, everything else = C# | PROPOSED |
| 2026-01-19 | status | orchestrator | Changed status to BLOCKED pending Phase 17 architecture decision | BLOCKED |
| 2026-01-19 | fl-review | orchestrator | User questioned: Is "extensibility" for developers or community? | DEEP_QUESTION |
| 2026-01-19 | fl-review | orchestrator | Analyzed COM YAML-ization: 80% of COMs can be YAML-only | FEASIBLE |
| 2026-01-19 | fl-review | orchestrator | User asked: What are the real C# benefits? | REFLECTION |
| 2026-01-19 | fl-review | orchestrator | Fundamental question: What changed from ERB era? | CORE_INSIGHT |
| 2026-01-19 | fl-review | orchestrator | Conclusion: C# migration = better interpreter + structured content | UNDERSTANDING |
| 2026-01-19 | decision | user | Requested new Feature for architecture.md revision | ACTION_REQUIRED |
| 2026-01-19 | create | feature-creator | Created F562 Architecture Revision from F537 FL Review | SPAWNED |