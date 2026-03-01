# Feature 177: /plan Command + Planning Subagents

## Status: [DONE]

## Type: infra

## Background

### Problem

現在の`/roadmap`コマンド�E6オプションがあるが曖昧で実用皁E��なぁE��Version計画から Feature作�Eまでの流れが不�E確、E
### Goal

v0.1単位でVersion計画を実行する`/plan`コマンドと、各スチE��プを拁E��するsubagentを作�Eする、E
### Key Decisions (from discussion)

1. **計画単佁E*: v0.1単位！E2.0, v2.1, ...�E�E2. **Feature粒度**: AC 8-15件/Feature�E�中間粒度�E�E3. **3スチE��プ構造**: goal-setter ↁEdependency-analyzer ↁEspec-writer
4. **Step 4はnextに委譲**: Feature刁E��は`/next`で実衁E5. **口上もdesignが忁E��E*: スコープ（キャラ/行数/パターン�E�を事前に決める
6. **既存designsは維持E*: 検討賁E��として`reference/designs-archive/`に移動せず活用

### Workflow

```
/plan (引数なぁE
    ↁE途中状態チェチE��:
├── APPROVEDなdesign ↁE/nextでFeature刁E��へ案�E
├── DRAFTなdesign ↁEStep 3: spec-writer
├── 未着手Version ↁEStep 1-2: goal-setter + dependency-analyzer
└── 全完亁EↁE、Enextで実裁E��、E
/plan v{X.Y}
    ↁEStep 1: goal-setter
- Version達�E目標を具体化
- 機�EリストをFeature粒度で刁E��
- 口丁E COM単佁E+ キャラ/行数/刁E��スコーチE- シスチE��: 論理機�E単位！EC 8-15件�E�E    ↁEStep 2: dependency-analyzer
- 実際のコード調査�E�ERB/C#/CSV�E�E- 依存関係特宁E- 技術的リスク洗い出ぁE    ↁEStep 3: spec-writer
- designs/{version-feature}.md 作�E
- Feature刁E��案�E訁E- 口上量予想
    ↁEↁE/next でFeature刁E��
```

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | plan.md作�E | file | exists | ".claude/commands/plan.md" | [x] |
| 2 | goal-setter.md作�E | file | exists | ".claude/agents/goal-setter.md" | [x] |
| 3 | dependency-analyzer.md作�E | file | exists | ".claude/agents/dependency-analyzer.md" | [x] |
| 4 | spec-writer.md作�E | file | exists | ".claude/agents/spec-writer.md" | [x] |
| 5 | 口上design用チE��プレート作�E | file | exists | "pm/reference/design-template-kojo.md" | [x] |
| 6 | シスチE��design用チE��プレート作�E | file | exists | "pm/reference/design-template-system.md" | [x] |
| 7 | /roadmap削除 | file | not_exists | ".claude/commands/roadmap.md" | [x] |
| 8 | CLAUDE.md更新�E�Eplanコマンド追加�E�E| file | contains | "/plan" | [x] |
| 9 | designs/README.md更新�E�ワークフロー反映�E�E| file | contains | "/plan" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | `/plan`コマンド作�E (.claude/commands/plan.md) | [x] |
| 2 | 2 | goal-setter subagent作�E (.claude/agents/goal-setter.md) | [x] |
| 3 | 3 | dependency-analyzer subagent作�E (.claude/agents/dependency-analyzer.md) | [x] |
| 4 | 4 | spec-writer subagent作�E (.claude/agents/spec-writer.md) | [x] |
| 5 | 5 | 口上design用チE��プレート作�E | [x] |
| 6 | 6 | シスチE��design用チE��プレート作�E | [x] |
| 7 | 7 | /roadmap削除 | [x] |
| 8 | 8 | CLAUDE.md更新 | [x] |
| 9 | 9 | designs/README.md更新 | [x] |

---

## Design Details

### /plan コマンド引数

| 引数 | 動佁E|
|------|------|
| (なぁE | 途中状態から最短スチE��プ実衁E|
| `v{X.Y}` | 持E��Version計画を開姁E継綁E|
| `design {name}` | 持E��designのStep 3へ |

### Subagent仕槁E
#### goal-setter (haiku)

```
入劁E Version番号
処琁E
1. content-roadmap.md読み込み
2. 該当Versionの現状確誁E3. 目標�E体化�E�口丁E COM/キャラ/行数、シスチE��: 機�Eリスト！E4. Feature粒度で刁E��案作�E
出劁E 目標リスチE+ Feature刁E��桁E```

#### dependency-analyzer (sonnet)

```
入劁E Version目標リスチE処琁E
1. ERB/C#/CSVコード調査
2. 依存関係グラフ作�E
3. 技術的リスク特宁E4. 口上�E場吁E 既存口上パターン刁E��
出劁E 依存�E析結果 + リスクリスチE```

#### spec-writer (sonnet)

```
入劁E 目樁E+ 依存�E极E処琁E
1. designs/{name}.md 作�E/更新
2. Feature刁E��案�E訁E3. 口上量/実裁E��予想
4. 未解決事頁E��スチE出劁E design.md (DRAFT)
```

### Feature粒度ガイドライン

| Type | 粒度 | AC目宁E|
|------|------|:------:|
| kojo | 1 COM ÁE10キャラ | 10-12 |
| erb/engine | 1論理機�E | 8-15 |
| infra | 1改修チE�EチE| 8-15 |

### AC粒度ルール

**ACはタスク可能な最小単位で定義する**:

- 1 AC = 1 Task = 1回�Esubagent dispatch
- 1 ACは1つのファイル作�E/変更、また�E1つの検証可能な動佁E- 曖昧な「〜が動作する」ではなく、�E体的な成果物�E�Eile exists, output contains�E�E
**良ぁE��E*:
```
| 1 | CFLAG:OPP追加 | file | contains | "OPP" in CFLAG.csv |
| 2 | OPP計算関数作�E | file | exists | "NTR/NTR_OPP.ERB" |
| 3 | OPP表示UI | output | contains | "OPP:" |
```

**悪ぁE��E*:
```
| 1 | OPPシスチE��実裁E| output | contains | "OPP動佁E |  ↁE曖昧、褁E��タスク混在
```

### 口上Versionのdesign侁E
```markdown
# v0.7 60-90番台COM口上設訁E
## Status: DRAFT

## スコーチE
| 番台 | COM数 | 優先度 | キャラ | 刁E��E| 行数 |
|------|:-----:|:------:|:------:|:----:|:----:|
| 60 | 13 | ☁E�E☁E| 10全員 | TALENTÁE | 4-8 |
| 80 | 6 | ☁E�E | 10全員 | TALENTÁE | 4-8 |
| 90 | 10 | ☁E�E | 10全員 | TALENTÁE | 4-8 |

## 口上量予想

29 COM ÁE10キャラ ÁE4刁E��EÁE6衁E= 紁E,960衁E
## Feature刁E��桁E
| ID | Type | Name | COM数 |
|:---|:----:|------|:-----:|
| F-XXX | kojo | COM60-61 正常位系 | 2 |
| F-XXX | kojo | COM62-63 アナル挿入系 | 2 |
| ... | ... | ... | ... |
```

---

## Links

- [content-roadmap.md](../content-roadmap.md)
- [designs/](../designs/)
- [.claude/commands/next.md](../../../archive/claude_legacy_20251230/commands/next.md)
