# Feature 267: K4 NTR 口上スタブ実装

## Status: [CANCELLED]

**Cancellation Reason (2025-12-31)**: Wrapper パターン誤解により誤作成。実際には `_1` suffix 関数に NTR 口上が実装済み。詳細は [feature-261.md](feature-261.md) 参照。

## Type: kojo

## Background

### Philosophy (Mid-term Vision)

F261 全 ERB 完全調査の結果、K4 (咲夜) の NTR 会話親密口上 4 関数が D (内容品質) NG として検出。
現状は `CALL TRAIN_MESSAGE` のみでスタブ状態。実際の NTR 口上を実装する。

### Problem (Current Issue)

~~F261 Phase ② 監査で以下の D (内容品質) NG が検出~~

**実際**: これらは wrapper 関数であり、実装本体は `_1` suffix 関数に存在。

| COM | Wrapper (D: NG) | Implementation (D: OK) |
|:---:|-----------------|------------------------|
| 302 | @NTR_KOJO_MESSAGE_COM_K4_302 | @NTR_KOJO_MESSAGE_COM_K4_302_1 |
| 311 | @NTR_KOJO_MESSAGE_COM_K4_311 | @NTR_KOJO_MESSAGE_COM_K4_311_1 |
| 312 | @NTR_KOJO_MESSAGE_COM_K4_312 | @NTR_KOJO_MESSAGE_COM_K4_312_1 |
| 314 | @NTR_KOJO_MESSAGE_COM_K4_314 | @NTR_KOJO_MESSAGE_COM_K4_314_1 |

### Goal (What to Achieve)

~~K4 NTR 会話親密口上 4 関数に実際の口上を実装。~~
→ **不要**: 既に実装済み。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | NTR COM 302 口上実装 | output | --unit | contains | "その瞳には、困惑と罪悪感、そして仄暗い情欲が揺らめいている。" | [ ] |
| 2 | NTR COM 311 口上実装 | output | --unit | contains | "怯えたような表情を浮かべていた。" | [ ] |
| 3 | NTR COM 312 口上実装 | output | --unit | contains | "・・だめ・・んっ・・" | [ ] |
| 4 | NTR COM 314 口上実装 | output | --unit | contains | "どこか上の空のようだ。" | [ ] |
| 5 | ビルド成功 | build | Bash | succeeds | dotnet build | [ ] |
| 6 | 回帰テスト PASS | test | --flow | succeeds | 24/24 scenarios | [ ] |

### AC Details

**AC1**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit "tests/ac/f267/302.json"`
**AC2**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit "tests/ac/f267/311.json"`
**AC3**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit "tests/ac/f267/312.json"`
**AC4**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit "tests/ac/f267/314.json"`
**AC5**: `dotnet build`
**AC6**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- . --flow`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | NTR COM 302 口上作成 | [ ] |
| 2 | 2 | NTR COM 311 口上作成 | [ ] |
| 3 | 3 | NTR COM 312 口上作成 | [ ] |
| 4 | 4 | NTR COM 314 口上作成 | [ ] |
| 5 | 5 | ビルド確認 | [ ] |
| 6 | 6 | 回帰テスト実行 | [ ] |

---

## Dependencies

- **F262**: COM 314 のファイル配置修正 ✅ (DONE)

---

## Review Notes

<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Links

- [feature-261.md](feature-261.md) - 全ERB完全調査 (本 Feature の入力)
- [feature-262.md](feature-262.md) - ファイル配置修正 (依存)
