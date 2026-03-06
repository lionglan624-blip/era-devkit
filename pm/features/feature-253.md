# Feature 253: COM-Kojo Semantic Audit

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
口上は COM の行為内容と意味的に整合すべき。キーワードの有無ではなく、描写が行為を正しく表現しているかを判定する。

例:
- COM60（正常位）の口上が「キスしている」描写のみなら矛盾
- COM69（対面座位アナル）の口上がアナル行為を描写していれば整合（婉曲表現可）

### Problem (Current Issue)
- F252 のキーワードベース監査は誤検知が多い（婉曲表現を矛盾と判定）
- 意味的整合性は人間または LLM による判断が必要
- 152 COMF × 10 キャラ = 最大 1,520 パターンを網羅的に検証する仕組みがない

### Goal (What to Achieve)
1. 全 COM の全キャラ・全分岐を意味的に監査
2. 監査結果を JSON ファイルで管理
3. 矛盾箇所の特定と報告

### Session Context
- **前提**: F252 (CANCELLED) からの引継ぎ
- **設計**: バッチ並列 dispatch + ステータスポーリング（オーケストレーターのコンテキスト最小化）

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 監査 agent 定義作成 | file | Glob | exists | .claude/agents/com-auditor.md | [ ] |
| 2 | 全 COMF の監査完了 | file | Glob | gte | 152 | [ ] |
| 3 | サマリーレポート生成 | file | Glob | exists | Game/logs/audit/summary.json | [ ] |

### AC Details

**AC1**: `.claude/agents/com-auditor.md` を作成
- 1 COM の全キャラ・全分岐を監査する agent 定義
- 結果を `Game/logs/audit/com-{NUM}.json` に出力
- ※ com-auditor は本監査専用の一時的 agent（CLAUDE.md 登録対象外）
- Agent 構成要素:
  - 入力: COM 番号
  - COMF パース: `Game/ERB/COMF{NUM}.ERB` から SOURCE/EXP 抽出
  - 口上検索: kojo-writing SKILL 参照（パス解決ロジックはSKILLに従う）
  - 判定: PASS/INCONSISTENT/NOT_IMPLEMENTED
  - 出力: JSON (Output JSON Schema 参照)

**AC2**: 全 152 COMF に対して監査を実行（Game/ERB/COMF*.ERB ファイル数に基づく）
- 前提: `Game/logs/audit/` ディレクトリを事前作成
- バッチサイズ: 不均一（Batch Configuration 参照）
- バッチ数: 14
- 各バッチは前バッチ完了後に開始
- 出力: `Game/logs/audit/com-{NUM}.json`
- 検証方法: `Glob("Game/logs/audit/com-*.json")` のファイル数をカウントし >= 152 を確認

**AC3**: 全監査完了後にサマリー生成
- summary agent が全 JSON を集計
- `Game/logs/audit/summary.json` に出力
- ※ summary agent も本監査専用の一時的 agent（CLAUDE.md 登録対象外）

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | com-auditor.md agent 定義作成 | [ ] |
| 2 | 2 | 全バッチ監査実行（結果: 全COMF対応 JSON 生成） | [ ] |
| 3 | 3 | summary.json 生成 | [ ] |

---

## Technical Notes

### Architecture

```
Orchestrator (Opus) - コンテキスト最小化
  │
  ▼ (順次 14 バッチ, バッチ内並列)
Auditor Agent (Sonnet)
  │ ・1 dispatch = 1 COM の全キャラ (10名) ・全分岐監査
  │ ・結果を JSON ファイルに出力
  ▼
出力: Game/logs/audit/com-{NUM}.json
  │
  ▼ (全バッチ完了後)
Summary Agent (Haiku)
  │ ・全 JSON を集計
  ▼
Game/logs/audit/summary.json
```

### Output JSON Schema

```json
{
  "com": 60,
  "comf_name": "正常位",
  "comf_sources": ["快Ｖ", "情愛", "苦痛"],
  "comf_exps": ["Ｖ性交経験"],
  "results": [
    {
      "char": 1,
      "char_name": "美鈴",
      "functions_checked": 8,
      "status": "PASS",
      "issues": []
    },
    {
      "char": 2,
      "char_name": "小悪魔",
      "functions_checked": 8,
      "status": "INCONSISTENT",
      "issues": [
        {
          "function": "@KOJO_MESSAGE_COM_K2_60_1",
          "reason": "口上がキス描写のみで挿入行為を描写していない"
        }
      ]
    }
  ],
  "summary": {
    "total_chars": 10,
    "passed": 8,
    "inconsistent": 2,
    "not_implemented": 0
  }
}
```

### Batch Configuration

※ 監査対象: Game/ERB/COMF*.ERB ファイル（152 個、2025-12-27 時点）
※ Batch Configuration は参考値。実行時は Glob で COMF ファイルを動的列挙

| Batch | COM Range | Count |
|:-----:|-----------|:-----:|
| 1 | 0-series (Caress) | 12 |
| 2 | 20-series (2) + 40-series (9) | 11 |
| 3 | 60-series (Penetration) | 13 |
| 4 | 80-series (6) + 90-series (10) | 16 |
| 5 | 100-series (7) + 120-series (5) | 12 |
| 6 | 140-series (Hardcore) | 10 |
| 7 | 160-series (1) + 180-series (10) | 11 |
| 8 | 200-series (Undressing) | 4 |
| 9 | 300-series (Anata-oriented) | 9 |
| 10 | 350-series (Actions) | 8 |
| 11 | 400-series (Movement/Daily) | 22 |
| 12 | 460-series (Visitors) | 4 |
| 13 | 500-series (Special systems) | 1 |
| 14 | 600-series (Self) | 17 |
| **Total** | | **152** |

※ 未実装口上の COM は NOT_IMPLEMENTED を返す見込み

### Polling Parameters

- バッチ間隔: 前バッチの全 JSON 出力確認後
- ポーリング間隔: 60 秒（調整可能な初期値）
- タイムアウト: 30 分/バッチ（調整可能な初期値）
- ステータス確認: `Glob("Game/logs/audit/com-*.json")` の count
- ※ 実行時間は COM 複雑度により変動。初回実行後にパラメータ調整を想定

### Judgment Criteria

**PASS**: 口上が COM の行為内容を正しく描写している
- 婉曲表現でも行為が伝わればOK
- SOURCE/EXP と矛盾しなければOK

**INCONSISTENT**: 口上が COM の行為と矛盾している
- 別の行為を描写している
- 行為に関連する描写が全くない

**NOT_IMPLEMENTED**: 該当 COM の口上関数が存在しない

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 05:30 | START | opus | F253 implementation | - |
| 2025-12-28 05:32 | AC1 | opus | Created com-auditor.md | DONE |
| 2025-12-28 05:35 | AC2 | com-auditor×152 | Batch audit execution (14 batches) | 152 JSONs |
| 2025-12-28 06:10 | AC3 | haiku | Summary generation | summary.json |
| 2025-12-28 06:15 | VERIFY | opus | All 3 ACs verified | PASS |
| 2025-12-28 06:16 | END | opus | Feature complete | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- [feature-252.md](feature-252.md) - CANCELLED (キーワード監査)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
