# Feature 253: COM-Kojo Semantic Audit

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
口上�E COM の行為冁E��と意味皁E��整合すべき。キーワード�E有無ではなく、描写が行為を正しく表現してぁE��かを判定する、E

侁E
- COM60�E�正常位）�E口上が「キスしてぁE��」描写�Eみなら矛盾
- COM69�E�対面座位アナル�E��E口上がアナル行為を描写してぁE��ば整合（婉曲表現可�E�E

### Problem (Current Issue)
- F252 のキーワード�Eース監査は誤検知が多い�E�婉曲表現を矛盾と判定！E
- 意味皁E��合性は人間また�E LLM による判断が忁E��E
- 152 COMF ÁE10 キャラ = 最大 1,520 パターンを網羁E��に検証する仕絁E��がなぁE

### Goal (What to Achieve)
1. 全 COM の全キャラ・全刁E��を意味皁E��監査
2. 監査結果めEJSON ファイルで管琁E
3. 矛盾箁E��の特定と報呁E

### Session Context
- **前提**: F252 (CANCELLED) からの引継ぎ
- **設訁E*: バッチ並刁Edispatch + スチE�Eタスポ�Eリング�E�オーケストレーターのコンチE��スト最小化�E�E

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 監査 agent 定義作�E | file | Glob | exists | .claude/agents/com-auditor.md | [ ] |
| 2 | 全 COMF の監査完亁E| file | Glob | gte | 152 | [ ] |
| 3 | サマリーレポ�Eト生戁E| file | Glob | exists | _out/logs/audit/summary.json | [ ] |

### AC Details

**AC1**: `.claude/agents/com-auditor.md` を作�E
- 1 COM の全キャラ・全刁E��を監査する agent 定義
- 結果めE`_out/logs/audit/com-{NUM}.json` に出劁E
- ※ com-auditor は本監査専用の一時的 agent�E�ELAUDE.md 登録対象外！E
- Agent 構�E要素:
  - 入劁E COM 番号
  - COMF パ�Eス: `Game/ERB/COMF{NUM}.ERB` から SOURCE/EXP 抽出
  - 口上検索: kojo-writing SKILL 参�E�E�パス解決ロジチE��はSKILLに従う�E�E
  - 判宁E PASS/INCONSISTENT/NOT_IMPLEMENTED
  - 出劁E JSON (Output JSON Schema 参�E)

**AC2**: 全 152 COMF に対して監査を実行！Eame/ERB/COMF*.ERB ファイル数に基づく！E
- 前提: `_out/logs/audit/` チE��レクトリを事前作�E
- バッチサイズ: 不均一�E�Eatch Configuration 参�E�E�E
- バッチ数: 14
- 吁E��チE��は前バチE��完亁E��に開姁E
- 出劁E `_out/logs/audit/com-{NUM}.json`
- 検証方況E `Glob("_out/logs/audit/com-*.json")` のファイル数をカウントし >= 152 を確誁E

**AC3**: 全監査完亁E��にサマリー生�E
- summary agent が�E JSON を集訁E
- `_out/logs/audit/summary.json` に出劁E
- ※ summary agent も本監査専用の一時的 agent�E�ELAUDE.md 登録対象外！E

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | com-auditor.md agent 定義作�E | [ ] |
| 2 | 2 | 全バッチ監査実行（結果: 全COMF対忁EJSON 生�E�E�E| [ ] |
| 3 | 3 | summary.json 生�E | [ ] |

---

## Technical Notes

### Architecture

```
Orchestrator (Opus) - コンチE��スト最小化
  ━E
  ▼ (頁E�� 14 バッチE バッチ�E並刁E
Auditor Agent (Sonnet)
  ━E・1 dispatch = 1 COM の全キャラ (10吁E ・全刁E��監査
  ━E・結果めEJSON ファイルに出劁E
  ▼
出劁E _out/logs/audit/com-{NUM}.json
  ━E
  ▼ (全バッチ完亁E��E
Summary Agent (Haiku)
  ━E・全 JSON を集訁E
  ▼
_out/logs/audit/summary.json
```

### Output JSON Schema

```json
{
  "com": 60,
  "comf_name": "正常佁E,
  "comf_sources": ["快�E�", "惁E�E", "苦痁E],
  "comf_exps": ["�E�性交経騁E],
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
      "char_name": "小悪魁E,
      "functions_checked": 8,
      "status": "INCONSISTENT",
      "issues": [
        {
          "function": "@KOJO_MESSAGE_COM_K2_60_1",
          "reason": "口上がキス描�Eのみで挿入行為を描写してぁE��ぁE
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

※ 監査対象: Game/ERB/COMF*.ERB ファイル�E�E52 個、E025-12-27 時点�E�E
※ Batch Configuration は参老E��。実行時は Glob で COMF ファイルを動皁E�E持E

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

※ 未実裁E��上�E COM は NOT_IMPLEMENTED を返す見込み

### Polling Parameters

- バッチE��隁E 前バチE��の全 JSON 出力確認征E
- ポ�Eリング間隔: 60 秒（調整可能な初期値�E�E
- タイムアウチE 30 刁EバッチE��調整可能な初期値�E�E
- スチE�Eタス確誁E `Glob("_out/logs/audit/com-*.json")` の count
- ※ 実行時間�E COM 褁E��度により変動。�E回実行後にパラメータ調整を想宁E

### Judgment Criteria

**PASS**: 口上が COM の行為冁E��を正しく描�EしてぁE��
- 婉曲表現でも行為が伝われ�EOK
- SOURCE/EXP と矛盾しなければOK

**INCONSISTENT**: 口上が COM の行為と矛盾してぁE��
- 別の行為を描写してぁE��
- 行為に関連する描�Eが�EくなぁE

**NOT_IMPLEMENTED**: 該彁ECOM の口上関数が存在しなぁE

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 05:30 | START | opus | F253 implementation | - |
| 2025-12-28 05:32 | AC1 | opus | Created com-auditor.md | DONE |
| 2025-12-28 05:35 | AC2 | com-auditorÁE52 | Batch audit execution (14 batches) | 152 JSONs |
| 2025-12-28 06:10 | AC3 | haiku | Summary generation | summary.json |
| 2025-12-28 06:15 | VERIFY | opus | All 3 ACs verified | PASS |
| 2025-12-28 06:16 | END | opus | Feature complete | SUCCESS |

---

## Links

- [index-features.md](../index-features.md)
- [feature-252.md](feature-252.md) - CANCELLED (キーワード監査)
- [kojo-writing SKILL](../../../archive/claude_legacy_20251230/skills/kojo-writing/SKILL.md)
