# Feature 320: COM_FILE_MAP JSON SSOT 化 + 全参照統一 + 配置修正

## Status: [DONE]

## Type: infra

## Background

### Philosophy
COM→ファイル対応を JSON として一元管理し、全ての subagents・ドキュメント・Python ツールが同一 SSOT を参照する構造を確立する。将来の COM 追加時の修正コストをゼロにする。

### Problem
1. **配置不一致**: COM 65, 92-93 が COM_FILE_MAP の期待と異なるファイルに配置
2. **分散管理**: COM_FILE_MAP が kojo_test_gen.py にハードコード、各ドキュメントに個別記載
3. **将来の保守負担**: 新 COM 追加時に複数箇所を修正する必要
4. **JSON 読み解き方の欠如**: SKILL.md が「JSON を参照せよ」と言うだけで、`ranges` と `character_overrides` の適用方法を説明していない。kojo-writer が K10 例外 (COM 90-93 → _愛撫.ERB) を見落とすリスクがある

### Goal
1. `com_file_map.json` を SSOT として作成（全 COM + 将来 COM 対応）
2. 全ての関連ファイル (5 skills/agents/commands + 2 Python tools) に JSON 参照リンクを統一記載
3. 現在の配置不一致 (COM 65, 90系) を修正

### Context
- 影響範囲: JSON 作成 + 7 ファイル更新 + 14件コード移動
- 発見経緯: F317 Phase 5 → FL 議論で SSOT 化の必要性を認識

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | com_file_map.json 存在 | file | exists | exists | tools/kojo-mapper/com_file_map.json | [x] |
| 2 | kojo_test_gen.py が JSON 読み込み | code | grep | contains | "com_file_map.json" in kojo_test_gen.py | [x] |
| 3 | kojo-writing SKILL が JSON 参照 | code | grep | contains | "com_file_map.json" in SKILL.md | [x] |
| 4 | 全参照ファイルに JSON リンク | exit_code | python verify_json_references.py | succeeds | exit 0 | [x] |
| 5 | COM 65 K4 が _挿入.ERB に配置 | code | grep | contains | "@KOJO_MESSAGE_COM_K4_65" in KOJO_K4_挿入.ERB | [x] |
| 6 | COM 92 K1-K9 が _口挿入.ERB に配置 | code | grep | matches | "@KOJO_MESSAGE_COM_K[1-9]_92" in KOJO_K*_口挿入.ERB | [x] |
| 6b | COM 92 K10 が _愛撫.ERB に配置 | code | grep | contains | "@KOJO_MESSAGE_COM_K10_92" in KOJO_K10_愛撫.ERB | [x] |
| 7 | COM 93 K1-K9 が _口挿入.ERB に配置 | code | grep | matches | "@KOJO_MESSAGE_COM_K[1-9]_93" in KOJO_K*_口挿入.ERB | [x] |
| 7b | COM 93 K10 が _愛撫.ERB に配置 | code | grep | contains | "@KOJO_MESSAGE_COM_K10_93" in KOJO_K10_愛撫.ERB | [x] |
| 8 | ビルド成功 | build | dotnet build | succeeds | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | com_file_map.json 作成 (全 COM + 将来 COM) | [x] |
| 2 | 2 | kojo_test_gen.py を JSON 読み込みに変更 (K10_COM_FILE_OVERRIDE 削除、JSON character_overrides に移行) | [x] |
| 3 | 2 | verify_com_map.py を JSON 読み込みに変更 (SKIP_COMBINATIONS 削除 → JSON skip_combinations に移行) | [x] |
| 4 | 4 | verify_json_references.py 作成 (7ファイル検証: com_file_map.json 参照あり→exit 0, なし→exit 1) | [x] |
| 5 | 3 | kojo-writing SKILL.md: COM 90-99 行 (line 61) 削除 + JSON 参照追加 | [x] |
| 6 | 4 | com-auditor.md: JSON 参照追加 (既存テーブルは audit workflow 用に維持) | [x] |
| 7 | 4 | do.md に JSON 参照追加 | [x] |
| 8 | 4 | kojo-init.md に JSON 参照追加 | [x] |
| 9 | 4 | testing/KOJO.md に JSON 参照追加 | [x] |
| 10 | 5 | COM 65 K4: 現配置確認 + _口挿入.ERB → _挿入.ERB 移動 | [x] |
| 11 | 6,7 | COM 92-93 K1-K9 移動: _挿入.ERB → _口挿入.ERB (11件) | [x] |
| 12 | 6b,7b | COM 92-93 K10 移動: _挿入.ERB → _愛撫.ERB (2件) | [x] |
| 13 | 8 | ビルド確認 | [x] |
| 14 | 3 | kojo-writing SKILL.md に JSON 読み解き方を追記 (ranges → character_overrides 適用順序) | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Task 1-4 (JSON + Python + 検証スクリプト) | SSOT + ツール更新 |
| 2 | implementer | sonnet | Task 5-9 (参照追加) | 5ファイル更新 |
| 3 | implementer | sonnet | Task 10-12 (コード移動) | 14件移動完了 |
| 4 | ac-tester | haiku | AC 1-8 (6b, 7b 含む) | PASS/FAIL |

---

## 設計詳細

### SSOT ファイル配置

```
tools/kojo-mapper/
├── com_file_map.json    ← SSOT (新規作成)
├── kojo_test_gen.py     ← JSON 読み込みに変更
└── verify_com_map.py    ← JSON 読み込みに変更
```

### com_file_map.json 構造 (全 COM 対応)

```json
{
  "version": "1.1",
  "description": "COM→ERBファイル対応マッピング (SSOT)",
  "ssot_note": "このファイルが唯一の定義元。他ファイルは参照のみ。",

  "ranges": [
    {"start": 0, "end": 6, "file": "_愛撫.ERB", "note": "愛撫系", "implemented": true},
    {"start": 500, "end": 599, "file": "_イベント.ERB", "note": "イベント系", "implemented": false},
    {"start": 600, "end": 699, "file": "_特殊.ERB", "note": "特殊系", "implemented": false}
  ],

  "character_overrides": { ... },
  "skip_combinations": [ ... ]
}
```

### implemented フラグ

| 値 | 意味 | verify_com_map.py の動作 |
|----|------|-------------------------|
| `true` | 実装済み範囲 | ファイル存在を検証（なければエラー） |
| `false` | 将来実装予定 | スキップ（ファイルがあればエラー） |

**pre-commit での強制**: ファイルが存在するのに `implemented: false` → コミット拒否

### 既存定義との不整合解消

現在の `kojo-writing/SKILL.md` は COM 90-99 で per-character split を記載しているが、
実際の `kojo_test_gen.py` および本 JSON 設計では:

| 定義元 | COM 90-93 デフォルト | K10 例外 |
|--------|---------------------|----------|
| SKILL.md (現状) | K1,K2,K3,K5,K8,K9,K10: `_挿入.ERB` / K4,K6,K7: `_口挿入.ERB` | なし |
| kojo_test_gen.py | `_口挿入.ERB` | `_愛撫.ERB` (90-93全て) |
| **本 JSON 設計** | `_口挿入.ERB` | `_愛撫.ERB` (90-93全て) |

Task 4 で SKILL.md の個別定義を削除し、JSON 参照に置換することで不整合を解消。

### 参照リンク統一フォーマット

全ての参照ファイルに以下を記載：

```markdown
## COM→ファイル対応

**SSOT**: [`tools/kojo-mapper/com_file_map.json`](../../../tools/kojo-mapper/com_file_map.json)

COM 番号からファイル名への対応は上記 JSON を参照。このファイルに個別定義を記載しない。
```

### 参照追加対象ファイル (7件)

| # | ファイル | 現状 | 変更内容 |
|:-:|----------|------|----------|
| 1 | `.claude/skills/kojo-writing/SKILL.md` | 個別定義あり | JSON 参照に置換 |
| 2 | `.claude/agents/com-auditor.md` | audit 用テーブルあり | JSON 参照追加 (テーブルは audit workflow 用に維持) |
| 3 | `.claude/commands/do.md` | kojo_test_gen 言及 | JSON 参照追加 |
| 4 | `.claude/commands/kojo-init.md` | ファイル配置言及 | JSON 参照追加 |
| 5 | `.claude/skills/testing/KOJO.md` | ファイル配置言及 | JSON 参照追加 |
| 6 | `tools/kojo-mapper/kojo_test_gen.py` | ハードコード | JSON 読み込み |
| 7 | `tools/kojo-mapper/verify_com_map.py` | ハードコード | JSON 読み込み |

---

## 現状分析

Grep で実際の配置を確認した結果:

### COM 92 現状
| キャラ | 現在のファイル | 期待ファイル | 状態 |
|:------:|----------------|--------------|:----:|
| K1 | _挿入.ERB | _口挿入.ERB | 要移動 |
| K2 | _挿入.ERB | _口挿入.ERB | 要移動 |
| K3 | _口挿入.ERB | _口挿入.ERB | ✓ |
| K4 | _口挿入.ERB | _口挿入.ERB | ✓ |
| K5 | _挿入.ERB | _口挿入.ERB | 要移動 |
| K6 | _口挿入.ERB | _口挿入.ERB | ✓ |
| K7 | _口挿入.ERB | _口挿入.ERB | ✓ |
| K8 | _挿入.ERB | _口挿入.ERB | 要移動 |
| K9 | _挿入.ERB | _口挿入.ERB | 要移動 |
| K10 | _挿入.ERB | _愛撫.ERB | 要移動 (※) |

### COM 93 現状
| キャラ | 現在のファイル | 期待ファイル | 状態 |
|:------:|----------------|--------------|:----:|
| K1 | _挿入.ERB | _口挿入.ERB | 要移動 |
| K2 | _挿入.ERB | _口挿入.ERB | 要移動 |
| K3 | _挿入.ERB | _口挿入.ERB | 要移動 |
| K4 | _口挿入.ERB | _口挿入.ERB | ✓ |
| K5 | _挿入.ERB | _口挿入.ERB | 要移動 |
| K6 | _口挿入.ERB | _口挿入.ERB | ✓ |
| K7 | _口挿入.ERB | _口挿入.ERB | ✓ |
| K8 | _挿入.ERB | _口挿入.ERB | 要移動 |
| K9 | _挿入.ERB | _口挿入.ERB | 要移動 |
| K10 | _挿入.ERB | _愛撫.ERB | 要移動 (※) |

**※ K10 は character_overrides により _愛撫.ERB が期待ファイル**

---

## 移動対象詳細

### COM 65 (1件)
移動: `_口挿入.ERB` → `_挿入.ERB`

| COM | 対象キャラ | 件数 |
|:---:|------------|:----:|
| 65 | K4 | 1 |

### COM 92 (6件)
| 対象キャラ | 移動元 | 移動先 | 件数 |
|------------|--------|--------|:----:|
| K1,K2,K5,K8,K9 | _挿入.ERB | _口挿入.ERB | 5 |
| K10 | _挿入.ERB | _愛撫.ERB | 1 |

### COM 93 (7件)
| 対象キャラ | 移動元 | 移動先 | 件数 |
|------------|--------|--------|:----:|
| K1,K2,K3,K5,K8,K9 | _挿入.ERB | _口挿入.ERB | 6 |
| K10 | _挿入.ERB | _愛撫.ERB | 1 |

**移動合計**: 14件 (COM 65: 1件 + COM 92: 6件 + COM 93: 7件)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-03 08:11 | START | implementer | Task 1-4 | - |
| 2026-01-03 08:11 | END | implementer | Task 1 | SUCCESS |
| 2026-01-03 08:11 | END | implementer | Task 2 | SUCCESS |
| 2026-01-03 08:11 | END | implementer | Task 3 | SUCCESS |
| 2026-01-03 08:11 | END | implementer | Task 4 | SUCCESS |
| 2026-01-03 08:13 | START | implementer | Task 5-9 | - |
| 2026-01-03 08:13 | END | implementer | Task 5 | SUCCESS |
| 2026-01-03 08:13 | END | implementer | Task 6 | SUCCESS |
| 2026-01-03 08:13 | END | implementer | Task 7 | SUCCESS |
| 2026-01-03 08:13 | END | implementer | Task 8 | SUCCESS |
| 2026-01-03 08:13 | END | implementer | Task 9 | SUCCESS |
| 2026-01-03 08:22 | START | implementer | Task 10-12 | - |
| 2026-01-03 08:22 | END | implementer | Task 10 | SUCCESS (COM 65 K4: 1件移動) |
| 2026-01-03 08:22 | END | implementer | Task 11 | SUCCESS (COM 92-93 K1-K9: 11件移動) |
| 2026-01-03 08:22 | END | implementer | Task 12 | SUCCESS (COM 92-93 K10: 2件移動) |
| 2026-01-03 09:30 | REVIEW | opus | AC 検証 | 全 AC PASS 確認 |
| 2026-01-03 09:35 | ADD | opus | Task 14 追加 | レビューで SKILL.md の JSON 読み解き方欠如を発見 |
| 2026-01-03 09:35 | END | opus | Task 14 | SUCCESS (SKILL.md に JSON 読み解き方追記) |
| 2026-01-03 | FIX | opus | Procedure 明確化 | SKILL.md Procedure Step 5 に JSON Read 手順を明示 |

---

## Links

- [index-features.md](index-features.md)
- 関連: [feature-317.md](feature-317.md) (発見元)
- SSOT: [com_file_map.json](../../tools/kojo-mapper/com_file_map.json)
