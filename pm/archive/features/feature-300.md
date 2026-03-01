# Feature 300: 既存口上品質監査

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
全ての口上が Phase 8d 品質基準を満たし、品質のばらつきをなくす

### Problem (Current Issue)
- 過去に実装された口上の品質が不明
- Phase 8c (4行) と Phase 8d (4-8行+描写) が混在している可能性
- どの COM/キャラの組み合わせが品質不足か把握できていない

### Goal (What to Achieve)
1. 全ての実装済み口上を品質基準でスキャン
2. 品質不足の口上をリスト化
3. 再作成が必要な Feature を特定

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | --quality オプションが存在する | code | Grep(tools/kojo-mapper/kojo_mapper.py) | contains | "--quality" | [x] |
| 2 | 全 COM × 全キャラの品質レポートが出力される | exit_code | python tools/kojo-mapper/kojo_mapper.py Game/ERB/口上 --quality | succeeds | - | [x] |
| 3 | 品質不足リストが生成される | output | python tools/kojo-mapper/kojo_mapper.py Game/ERB/口上 --quality | contains | "LOW_QUALITY" | [x] |
| 4 | 監査結果が agents/ に保存される | file | Glob(Game/agents/audit/kojo-quality-*.md) | exists | - | [x] |

※ AC4 は任意のマッチングファイル存在で PASS。新規生成確認は Execution Log のタイムスタンプで担保。

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo_mapper.py に --quality モード追加 | [x] |
| 2 | 2 | 品質レポート出力ロジック実装 (既存metrics活用、per-branch = dialogue_text_lines / kojo_block_count) | [x] |
| 3 | 3 | LOW_QUALITY 判定ロジック実装 | [x] |
| 4 | 4 | Game/agents/audit/ ディレクトリ作成 + Markdownレポート出力 | [x] |
| 5 | - | 品質統合レポート Feature 作成 (残課題セクション参照) | [skip] |

---

## Design Notes

### Phase Scope

本監査は Phase 8d (基本口上) を品質基準とする。Phase 8j (射精状態分岐) および Phase 8l (初回実行) は将来拡張として別途対応予定。

※ kojo_mapper.py PHASE_REQUIREMENTS に 8j/8l エントリは存在するが、実際の検出ロジック (NOWEX/FIRSTTIME パターンマッチング) は未実装。

### 監査対象範囲

**本 Feature は構造的品質のみを対象とする (Python判定可能)**:
- TALENT 分岐構造
- 行数・パターン数
- スタブ/実装状態

**意味的品質 (LLM判定必須) は対象外**:
- 口上が COM の行為を正しく描写しているか
- キャラクター性格との一致
- 読者理解の可否

→ 意味的品質は com-auditor (F253) で別途監査済み。`Game/logs/audit/com-*.json` 参照。

### 監査項目

Phase 8d 基準に基づく品質判定 (kojo-writing SKILL.md に準拠):

| 項目 | チェック内容 | 品質判定 | kojo_mapper.py 既存機能 | 新規実装 |
|------|-------------|----------|------------------------|---------|
| 存在確認 | 関数定義の有無 | MISSING / EXISTS | - | ○ |
| スタブ判定 | DATAFORM有無 (kojo_mapper.py に DATAFORM 検出統合、erb-duplicate-check.py 準拠ロジック) | STUB / IMPLEMENTED | - | ○ |
| TALENT分岐 | TALENT_4 (恋人/恋慕/思慕/なし) | NONE / TALENT_3 / TALENT_4 | branch_type (既存) | - |
| 行数 | 1分岐あたりの行数 (4-8行) | LOW (<4) / OK (4-8) / EXCESS (>8) | dialogue_text_lines (既存、関数単位) | per-branch = dialogue_text_lines / kojo_block_count |
| パターン数 | 1分岐あたりのバリエーション (1+) | LOW (0) / OK (1+) | kojo_block_count (既存) | 品質判定追加 |

**Phase 8d 合格条件**: TALENT_4 AND 行数 4-8 per branch AND パターン数 1+ per branch

※ content-roadmap.md C2/Phase 8d 基準に準拠 (patterns=1)。将来の品質向上で patterns=4 への引き上げを検討。

**安定出力キーワード**: `LOW_QUALITY` は F299 等の下流 Feature で依存される出力形式。変更時は依存 Feature の Expected 更新が必要。

**テスト前提条件**: AC3 検証時、監査対象に LOW_QUALITY 該当アイテムが存在することが前提 (現状の Phase 8c/8d 混在状態では該当アイテムが存在する見込み)。全口上が Phase 8d 準拠になった場合、AC3 は代替手段で検証。

**初回監査結果 (2026-01-02)**: Phase 8d PASS: 0%, LOW_QUALITY: 405件。これは想定通りの結果であり、歴史的な Phase 8c 実装 (4行のみ、TALENT 分岐なし) が大半を占めることを確認。この結果は品質改善作業の必要性を裏付けるものである。

### 出力形式案

```markdown
# Kojo Quality Audit Report

## Summary
- Scope: kojo_mapper.py COM_RANGES を使用 (52 COM × 11 = 572)
  - K1-K10: 52 COM × 10 K = 520 (キャラ固有)
  - KU: 52 COM × 1 = 52 (汎用フォールバック)
  - 参考: content-roadmap.md v0.6=36 COM, v1.0=150 COM
- Implemented: XXX
- Stub: XXX
- Missing: XXX

## Quality Distribution
- GOOD (Phase 8d): 400
- OK (Phase 8c): 350
- LOW: 100

## LOW_QUALITY List
| COM | Char | Issues |
|-----|------|--------|
| COM_83 | K1 | 2 lines only |
| COM_84 | K5 | No TALENT branch |
```

### スキャン対象

```
Game/ERB/口上/
├── 1_美鈴/KOJO_K1_*.ERB
├── 2_小悪魔/KOJO_K2_*.ERB
├── ...
├── 10_魔理沙/KOJO_K10_*.ERB
└── U_汎用/KOJO_KU_*.ERB
```

### 出力先

`Game/agents/audit/` ディレクトリを新規作成。監査レポートは日付付きで保存: `kojo-quality-YYYY-MM-DD.md`

**ディレクトリ区分**:
- `Game/logs/audit/` = 既存、per-COM semantic audit (JSON、com-*.json)
- `Game/agents/audit/` = 新規、batch quality report (Markdown、kojo-quality-*.md)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 08:35 | START | implementer | Task 1-4 | - |
| 2026-01-02 08:36 | CODE | implementer | Add --quality argparse option | SUCCESS |
| 2026-01-02 08:38 | CODE | implementer | Add stub detection functions (extract_function_scope, check_stub_status) | SUCCESS |
| 2026-01-02 08:40 | CODE | implementer | Add quality audit functions (calculate_quality_audit, generate_quality_report) | SUCCESS |
| 2026-01-02 08:41 | CODE | implementer | Add --quality mode handler in main() | SUCCESS |
| 2026-01-02 08:42 | TEST | implementer | AC1: Verify --quality option | PASS |
| 2026-01-02 08:42 | TEST | implementer | AC2: Test quality report generation | PASS |
| 2026-01-02 08:43 | TEST | implementer | AC3: Verify LOW_QUALITY keyword | PASS |
| 2026-01-02 08:43 | TEST | implementer | AC4: Verify audit file creation | PASS |
| 2026-01-02 08:44 | END | implementer | Task 1-4 | SUCCESS |

---

## Dependencies

F299 (do.md 品質チェック手順追加) は本 Feature に依存。F300 を先に完了する必要あり。

- F300: kojo_mapper.py --quality (バッチ監査)
- F299: erb-duplicate-check.py --check-quality (個別チェック、do.md workflow 用)

## Links

- [index-features.md](index-features.md)
- [kojo-mapper](../../tools/kojo-mapper/) - 既存カバレッジ分析ツール
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md) - Phase 8d 品質基準
- [content-roadmap.md](content-roadmap.md) - COM スコープ定義
- Related: [feature-299.md](feature-299.md) - do.md 品質チェック手順追加 (本 Feature 依存)

---

## 残課題

| 課題 | 説明 | 対応 |
|------|------|------|
| 品質統合レポート | 構造的品質 (F300) と意味的品質 (com-auditor) を統合した全体像把握 | Phase 28 Task 7 |

Philosophy「品質のばらつきをなくす」の完全達成には、両軸の統合が必要。本 Feature は構造的品質監査として価値を持つ。

**→ Phase 28 Task 7 として追跡**: [full-csharp-architecture.md](designs/full-csharp-architecture.md) Phase 28: Documentation
