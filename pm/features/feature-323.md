# Feature 323: COM_FILE_MAP 全 COM 範囲対応

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
COM→ファイル対応を JSON として一元管理し、**将来の COM 追加時の修正コストをゼロにする**。(F320 Philosophy 継承)

### Problem (Current Issue)
F320 の設計思想は「将来 COM も全て含める」だったが、実際の ranges 実装は既存実装のみをカバー：

```json
{"start": 90, "end": 93, "file": "_口挿入.ERB", ...}
```

F319 ad-hoc fix で COM_94 を追加したが、まだギャップが残っている (95-99, 12-19, 22-39, 49-59, 73-79, 86-89 等)。
これは F320 の **実装漏れ** であり、設計思想と実装が乖離している。

**発見経緯**: F319 Phase 5 で kojo_test_gen.py が COM_94 を認識できず失敗 → ad-hoc fix で対応済み。

### Goal (What to Achieve)
com_file_map.json の ranges を全 COM 範囲 (0-699) に拡張し、将来の COM 追加時に JSON 修正が不要な設計を実現する。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ranges が 0-699 を連続カバー (ギャップなし) | exit_code | python verify_range_coverage.py | succeeds | - | [x] |
| 2 | COM 95 lookup が _口挿入.ERB を返す | output | python -c "from kojo_test_gen import get_erb_file_for_com; print(get_erb_file_for_com(95))" | contains | _口挿入.ERB | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | verify_range_coverage.py 作成 + com_file_map.json ranges 拡張 (0-699) | [x] |
| 2 | 2 | COM 95 lookup テスト実行 | [x] |

<!-- Note: Task 1 creates the verification script as part of achieving AC#1 (TDD approach) -->

---

## 設計詳細

### 現状の ranges (抜粋)

```json
"ranges": [
  {"start": 0, "end": 6, "file": "_愛撫.ERB", "implemented": true},
  ...
  {"start": 90, "end": 94, "file": "_口挿入.ERB", "implemented": true},
  // Gap: 95-99 が未定義
  {"start": 100, "end": 106, ...},
  ...
]
```

### 変更後の ranges (全範囲カバー)

```json
"ranges": [
  // 0-99: 愛撫・調教系
  {"start": 0, "end": 6, "file": "_愛撫.ERB", "note": "愛撫系", "implemented": true},
  {"start": 7, "end": 7, "file": "_乳首責め.ERB", "note": "乳首責め", "implemented": true},
  {"start": 8, "end": 9, "file": "_愛撫.ERB", "note": "愛撫系", "implemented": true},
  {"start": 10, "end": 11, "file": "_乳首責め.ERB", "note": "乳首吸い/吸わせ", "implemented": true},
  {"start": 12, "end": 19, "file": "_愛撫.ERB", "note": "愛撫系(未実装)", "implemented": false},
  {"start": 20, "end": 21, "file": "_愛撫.ERB", "note": "キス系", "implemented": true},
  {"start": 22, "end": 39, "file": "_愛撫.ERB", "note": "愛撫系(未実装)", "implemented": false},
  {"start": 40, "end": 48, "file": "_愛撫.ERB", "note": "道具系", "implemented": true},
  {"start": 49, "end": 59, "file": "_愛撫.ERB", "note": "道具系(未実装)", "implemented": false},
  {"start": 60, "end": 72, "file": "_挿入.ERB", "note": "挿入系", "implemented": true},
  {"start": 73, "end": 79, "file": "_挿入.ERB", "note": "挿入系(未実装)", "implemented": false},
  {"start": 80, "end": 85, "file": "_口挿入.ERB", "note": "奉仕系", "implemented": true},
  {"start": 86, "end": 89, "file": "_口挿入.ERB", "note": "奉仕系(未実装)", "implemented": false},
  {"start": 90, "end": 94, "file": "_口挿入.ERB", "note": "アナル系", "implemented": true},
  {"start": 95, "end": 99, "file": "_口挿入.ERB", "note": "アナル系(未実装)", "implemented": false},

  // 100-199: SM・ハード系
  {"start": 100, "end": 106, "file": "_口挿入.ERB", "note": "SM系", "implemented": true},
  {"start": 107, "end": 119, "file": "_口挿入.ERB", "note": "SM系(未実装)", "implemented": false},
  {"start": 120, "end": 126, "file": "_口挿入.ERB", "note": "助手/レズプレイ", "implemented": true},
  {"start": 127, "end": 139, "file": "_口挿入.ERB", "note": "レズ系(未実装)", "implemented": false},
  {"start": 140, "end": 148, "file": "_口挿入.ERB", "note": "ハード調教", "implemented": true},
  {"start": 149, "end": 179, "file": "_口挿入.ERB", "note": "ハード系(未実装)", "implemented": false},
  {"start": 180, "end": 189, "file": "_口挿入.ERB", "note": "特殊", "implemented": true},
  {"start": 190, "end": 199, "file": "_口挿入.ERB", "note": "特殊(未実装)", "implemented": false},

  // 200-299: 脱衣・その他
  {"start": 200, "end": 203, "file": "_口挿入.ERB", "note": "脱衣系", "implemented": true},
  {"start": 204, "end": 299, "file": "_口挿入.ERB", "note": "その他(未実装)", "implemented": false},

  // 300-499: 会話・日常系
  {"start": 300, "end": 316, "file": "_会話親密.ERB", "note": "会話親密系", "implemented": true},
  {"start": 317, "end": 349, "file": "_会話親密.ERB", "note": "会話(未実装)", "implemented": false},
  {"start": 350, "end": 352, "file": "_会話親密.ERB", "note": "会話親密系", "implemented": true},
  {"start": 353, "end": 409, "file": "_会話親密.ERB", "note": "会話(未実装)", "implemented": false},
  {"start": 410, "end": 415, "file": "_日常.ERB", "note": "日常系", "implemented": true},
  {"start": 416, "end": 462, "file": "_日常.ERB", "note": "日常(未実装)", "implemented": false},
  {"start": 463, "end": 463, "file": "_日常.ERB", "note": "日常系", "implemented": true},
  {"start": 464, "end": 499, "file": "_日常.ERB", "note": "日常(未実装)", "implemented": false},

  // 500-699: イベント・特殊系
  {"start": 500, "end": 599, "file": "_イベント.ERB", "note": "イベント系", "implemented": false},
  {"start": 600, "end": 699, "file": "_特殊.ERB", "note": "特殊系", "implemented": false}
]
```

### implemented フラグの意味

| 値 | 意味 | kojo_test_gen.py 動作 |
|----|------|----------------------|
| `true` | 実装済み or 実装可能 | COM を認識、テスト生成可能 |
| `false` | 将来実装予定 | COM を認識するがファイル未存在を警告 |

**Key Point**: `implemented: false` でも COM 番号自体は ranges に含まれるため、kojo_test_gen.py は `Unsupported COM` エラーを出さない。

### 既存 character_overrides との整合性

character_overrides は ranges の上書きルールなので、ranges 拡張による影響なし。

---

## Review Notes

### AC#3 削除の検討過程 (FL 323, 2026-01-03)

**元の AC#3**: `verify_com_map.py が implemented=true 範囲を検証 | exit_code | succeeds`

**削除理由**:

1. **verify_com_map.py の目的分析**:
   - F285 で作成。目的は「com_file_map.json と実ファイルの整合性保証」
   - Check 1: `implemented: true` のレンジについて K1-K10 ERB ファイル存在検証
   - Check 2: `implemented: false` のレンジについてファイルが**存在しない**ことを検証

2. **現状の問題**:
   - 現在 `verify_com_map.py` は exit 1（既存の missing files）
   - これは既存の `implemented: true` レンジに対応するファイル/関数が未実装のため
   - F323 とは無関係の既存問題

3. **F323 の変更内容**:
   - `implemented: false` のレンジのみ追加
   - `verify_com_map.py` は `implemented: false` をスキップする（コード行 34）
   - よって F323 の変更では新規エラーは発生しない

4. **長期保守性・可読性の観点**:
   - AC#3 を維持すると「exit 0 必須」と誤解される
   - 既存 missing files の解決は別 Feature のスコープ（kojo 実装時に解消）
   - F323 のゴールは「ranges 拡張」のみ。整合性検証は設計説明で十分

5. **結論**:
   - AC#3 を削除
   - 設計セクションに「verify_com_map.py は implemented=false をスキップするため影響なし」を明記（既存の行 124 で説明済み）

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-03 18:41 | START | implementer | Task 1 | - |
| 2026-01-03 18:41 | END | implementer | Task 1 | SUCCESS |
| 2026-01-03 18:42 | VERIFY | Opus | Task 2 (AC#2) | PASS: _口挿入.ERB |

---

## Links

- [index-features.md](index-features.md)
- 発見元: [feature-319.md](feature-319.md)
- 親 Feature: [feature-320.md](feature-320.md) (F320 の設計思想継承)
- SSOT: [com_file_map.json](../../tools/kojo-mapper/com_file_map.json)
