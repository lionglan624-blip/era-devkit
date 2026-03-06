# Feature 177: /plan Command + Planning Subagents

## Status: [DONE]

## Type: infra

## Background

### Problem

現在の`/roadmap`コマンドは6オプションがあるが曖昧で実用的でない。Version計画から Feature作成までの流れが不明確。

### Goal

v0.1単位でVersion計画を実行する`/plan`コマンドと、各ステップを担当するsubagentを作成する。

### Key Decisions (from discussion)

1. **計画単位**: v0.1単位（v2.0, v2.1, ...）
2. **Feature粒度**: AC 8-15件/Feature（中間粒度）
3. **3ステップ構造**: goal-setter → dependency-analyzer → spec-writer
4. **Step 4はnextに委譲**: Feature分割は`/next`で実行
5. **口上もdesignが必要**: スコープ（キャラ/行数/パターン）を事前に決める
6. **既存designsは維持**: 検討資料として`reference/designs-archive/`に移動せず活用

### Workflow

```
/plan (引数なし)
    ↓
途中状態チェック:
├── APPROVEDなdesign → /nextでFeature分割へ案内
├── DRAFTなdesign → Step 3: spec-writer
├── 未着手Version → Step 1-2: goal-setter + dependency-analyzer
└── 全完了 → 「/nextで実装へ」

/plan v{X.Y}
    ↓
Step 1: goal-setter
- Version達成目標を具体化
- 機能リストをFeature粒度で分解
- 口上: COM単位 + キャラ/行数/分岐スコープ
- システム: 論理機能単位（AC 8-15件）
    ↓
Step 2: dependency-analyzer
- 実際のコード調査（ERB/C#/CSV）
- 依存関係特定
- 技術的リスク洗い出し
    ↓
Step 3: spec-writer
- designs/{version-feature}.md 作成
- Feature分割案明記
- 口上量予想
    ↓
→ /next でFeature分割
```

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | plan.md作成 | file | exists | ".claude/commands/plan.md" | [x] |
| 2 | goal-setter.md作成 | file | exists | ".claude/agents/goal-setter.md" | [x] |
| 3 | dependency-analyzer.md作成 | file | exists | ".claude/agents/dependency-analyzer.md" | [x] |
| 4 | spec-writer.md作成 | file | exists | ".claude/agents/spec-writer.md" | [x] |
| 5 | 口上design用テンプレート作成 | file | exists | "Game/agents/reference/design-template-kojo.md" | [x] |
| 6 | システムdesign用テンプレート作成 | file | exists | "Game/agents/reference/design-template-system.md" | [x] |
| 7 | /roadmap削除 | file | not_exists | ".claude/commands/roadmap.md" | [x] |
| 8 | CLAUDE.md更新（/planコマンド追加） | file | contains | "/plan" | [x] |
| 9 | designs/README.md更新（ワークフロー反映） | file | contains | "/plan" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | `/plan`コマンド作成 (.claude/commands/plan.md) | [x] |
| 2 | 2 | goal-setter subagent作成 (.claude/agents/goal-setter.md) | [x] |
| 3 | 3 | dependency-analyzer subagent作成 (.claude/agents/dependency-analyzer.md) | [x] |
| 4 | 4 | spec-writer subagent作成 (.claude/agents/spec-writer.md) | [x] |
| 5 | 5 | 口上design用テンプレート作成 | [x] |
| 6 | 6 | システムdesign用テンプレート作成 | [x] |
| 7 | 7 | /roadmap削除 | [x] |
| 8 | 8 | CLAUDE.md更新 | [x] |
| 9 | 9 | designs/README.md更新 | [x] |

---

## Design Details

### /plan コマンド引数

| 引数 | 動作 |
|------|------|
| (なし) | 途中状態から最短ステップ実行 |
| `v{X.Y}` | 指定Version計画を開始/継続 |
| `design {name}` | 指定designのStep 3へ |

### Subagent仕様

#### goal-setter (haiku)

```
入力: Version番号
処理:
1. content-roadmap.md読み込み
2. 該当Versionの現状確認
3. 目標具体化（口上: COM/キャラ/行数、システム: 機能リスト）
4. Feature粒度で分解案作成
出力: 目標リスト + Feature分割案
```

#### dependency-analyzer (sonnet)

```
入力: Version目標リスト
処理:
1. ERB/C#/CSVコード調査
2. 依存関係グラフ作成
3. 技術的リスク特定
4. 口上の場合: 既存口上パターン分析
出力: 依存分析結果 + リスクリスト
```

#### spec-writer (sonnet)

```
入力: 目標 + 依存分析
処理:
1. designs/{name}.md 作成/更新
2. Feature分割案明記
3. 口上量/実装量予想
4. 未解決事項リスト
出力: design.md (DRAFT)
```

### Feature粒度ガイドライン

| Type | 粒度 | AC目安 |
|------|------|:------:|
| kojo | 1 COM × 10キャラ | 10-12 |
| erb/engine | 1論理機能 | 8-15 |
| infra | 1改修テーマ | 8-15 |

### AC粒度ルール

**ACはタスク可能な最小単位で定義する**:

- 1 AC = 1 Task = 1回のsubagent dispatch
- 1 ACは1つのファイル作成/変更、または1つの検証可能な動作
- 曖昧な「〜が動作する」ではなく、具体的な成果物（file exists, output contains）

**良い例**:
```
| 1 | CFLAG:OPP追加 | file | contains | "OPP" in CFLAG.csv |
| 2 | OPP計算関数作成 | file | exists | "NTR/NTR_OPP.ERB" |
| 3 | OPP表示UI | output | contains | "OPP:" |
```

**悪い例**:
```
| 1 | OPPシステム実装 | output | contains | "OPP動作" |  ← 曖昧、複数タスク混在
```

### 口上Versionのdesign例

```markdown
# v0.7 60-90番台COM口上設計

## Status: DRAFT

## スコープ

| 番台 | COM数 | 優先度 | キャラ | 分岐 | 行数 |
|------|:-----:|:------:|:------:|:----:|:----:|
| 60 | 13 | ★★★ | 10全員 | TALENT×4 | 4-8 |
| 80 | 6 | ★★ | 10全員 | TALENT×4 | 4-8 |
| 90 | 10 | ★★ | 10全員 | TALENT×4 | 4-8 |

## 口上量予想

29 COM × 10キャラ × 4分岐 × 6行 = 約6,960行

## Feature分割案

| ID | Type | Name | COM数 |
|:---|:----:|------|:-----:|
| F-XXX | kojo | COM60-61 正常位系 | 2 |
| F-XXX | kojo | COM62-63 アナル挿入系 | 2 |
| ... | ... | ... | ... |
```

---

## Links

- [content-roadmap.md](content-roadmap.md)
- [designs/](designs/)
- [.claude/commands/next.md](../../.claude/commands/next.md)
