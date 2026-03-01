# Feature 267: K4 NTR 口上スタブ実装

## Status: [COMPLETED]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)

F261 全 ERB 完全調査の結果、K4 (咲夜) の NTR 会話親密・愛撫関数が D (内容品質) NG として検出。
現状は `CALL TRAIN_MESSAGE` のみでスタブ状態。実際の NTR 口上を実装する。

### Problem (Current Issue)

F261 Phase ② 監査で以下の D (内容品質) NG が検出:

| COM | Function | Issue |
|:---:|----------|-------|
| 302 | @NTR_KOJO_MESSAGE_COM_K4_302 | No dialogue (CALL TRAIN_MESSAGE only) |
| 311 | @NTR_KOJO_MESSAGE_COM_K4_311 | No dialogue (CALL TRAIN_MESSAGE only) |
| 312 | @NTR_KOJO_MESSAGE_COM_K4_312 | No dialogue (CALL TRAIN_MESSAGE only) |
| 314 | @NTR_KOJO_MESSAGE_COM_K4_314 | No dialogue + A overlap (配置 NG) |

**Note**: COM 314 は F262 でファイル配置修正後に本 Feature で内容実装。

### Goal (What to Achieve)

K4 NTR 会話親密・愛撫関数に実際の口上を実装。
eraTW 参照 + キャラ設定に基づいた NTR 描写。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | NTR COM 302 口上実装 | output | --unit | contains | NTR 描写 | [x] |
| 2 | NTR COM 311 口上実装 | output | --unit | contains | NTR 描写 | [x] |
| 3 | NTR COM 312 口上実装 | output | --unit | contains | NTR 描写 | [x] |
| 4 | NTR COM 314 口上実装 | output | --unit | contains | NTR 描写 | [x] |
| 5 | ビルド成功 | build | Bash | succeeds | dotnet build | [x] |
| 6 | 回帰テスト PASS | test | --flow | succeeds | 24/24 scenarios | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | NTR COM 302 口上作成 | [x] |
| 2 | 2 | NTR COM 311 口上作成 | [x] |
| 3 | 3 | NTR COM 312 口上作成 | [x] |
| 4 | 4 | NTR COM 314 口上作成 | [x] |
| 5 | 5 | ビルド確認 | [x] |
| 6 | 6 | 回帰テスト実行 | [x] |

---

## Dependencies

- **F262**: COM 314 のファイル配置修正が既に完了。

---

## Links

- [feature-261.md](feature-261.md) - 全ERB完全調査 (本 Feature の入力)
- [feature-262.md](feature-262.md) - ファイル配置修正 (依存)