# Feature 157: output_equals プレースホルダ展開対応

## Status: [DONE]

## Type: engine

## Background

### Problem
kojo-writer が生成するテストで `output_equals` を使用しているが、テストが失敗する。

**原因**: KojoExpectValidator の `CheckOutputEquals` は単純な文字列比較のみ。
- テストJSON: `"output_equals": "%CALLNAME%、今日も綺麗だね"`
- 実際の出力: `"美鈴、今日も綺麗だね"` (%CALLNAME% がキャラ名に展開済み)
- 結果: 不一致でテスト失敗

### Goal
エンジン側で expected 文字列内の `%CALLNAME%` 等を実行時に展開し、`output_equals` が正しく動作するようにする。

### Context
- Feature 136 で発覚（K5, K7, K10 で 33件の誤検出）
- 現状の回避策: `output_contains` で部分一致検証 → 厳密さが失われる
- 根本解決: エンジン側でプレースホルダ展開

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | %CALLNAME% を含む output_equals テストが成功する | exit_code | equals | "0" | [x] |
| 2 | %CALLNAME:MASTER% を含むテストも成功する | exit_code | equals | "0" | [x] |
| 3 | ビルド成功 | build | succeeds | - | [x] |
| 4 | 既存テスト回帰なし | exit_code | equals | "0" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | %CALLNAME% プレースホルダ展開テスト実装 | [x] |
| 2 | 2 | %CALLNAME:MASTER% プレースホルダ展開テスト実装 | [x] |
| 3 | 3 | ビルド確認 | [x] |
| 4 | 4 | 既存テスト実行 | [x] |

---

## Technical Notes

### 対象プレースホルダ
- `%CALLNAME%` → TARGET キャラの呼び名
- `%CALLNAME:MASTER%` → MASTER (プレイヤー) の呼び名

### 修正箇所
- `engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs`
  - Line 178: `CheckOutputEquals` メソッド
  - expected 文字列を比較前にプレースホルダ展開

### 参考実装
- `VariableResolver.cs` - 既存の変数解決ロジック
- テスト実行時には TARGET/MASTER が設定済み

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-20 | orchestrator | Investigation | READY |
| 2025-12-20 | implementer | ExpandPlaceholders実装 | SUCCESS |
| 2025-12-20 | orchestrator | AC検証 | BLOCKED (K8 State Error) |
| 2025-12-20 | orchestrator | Feature 158作成 | Created |
| 2025-12-20 | debugger | VariableData caching fix | SUCCESS |
| 2025-12-20 | orchestrator | AC全件検証 | All PASS |

---

## Links

- [index-features.md](index-features.md)
- [KojoExpectValidator.cs](../../engine/Assets/Scripts/Emuera/Headless/KojoExpectValidator.cs)
