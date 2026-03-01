# Feature 270: 口上主客修正 - F269 監査結果に基づく主客逆転修正

## Status: [CANCELLED]

**Reason**: v0.7 設計 (v0.7-audit-fix.md) に統合。COM 43 は Feature Group B2 (器具系) として対応予定。

## Type: kojo

## Background

### Philosophy (Mid-term Vision)

口上は COMF/COMABLE の定義に従い、正しい主客関係で記述されるべき。
「誰が誰に何をするか」が COMF の PLAYER/TARGET 定義と一致していなければならない。

> F269 監査結果に基づき、主客逆転 (E1) を優先修正する。

### Problem (Current Issue)

F269 監査で以下の不整合が検出:

| Category | Count | Description |
|----------|------:|-------------|
| E1 | 129 | 主客逆転 (actor/receiver mismatch) |
| E2 | 49 | 条件分岐不足 |
| E3 | 95 | 判定不能 (要調査) |

**E1 例** (K6 COM43):
- COMF43 定義: PLAYER が TARGET の男性器にオナホールを使用
- 問題の口上: TARGET が PLAYER にオナホールを使用
- 不整合: 主客が逆転

### Goal (What to Achieve)

E1 (主客逆転) 129件のうち、優先度の高いものから修正。
初回スコープ: COM 43 (オナホール) 全キャラ分

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K3 COM 43 主客修正 | output | --unit | contains | PLAYER uses onahole | [ ] |
| 2 | K6 COM 43 主客修正 | output | --unit | contains | PLAYER uses onahole | [ ] |
| 3 | K10 COM 43 主客修正 | output | --unit | contains | PLAYER uses onahole | [ ] |
| 4 | ビルド成功 | build | Bash | succeeds | - | [ ] |
| 5 | 回帰テスト PASS | test | --flow | succeeds | 24/24 | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | K3 COM 43 口上修正 (PLAYER→行為者) | [ ] |
| 2 | 2 | K6 COM 43 口上修正 (PLAYER→行為者) | [ ] |
| 3 | 3 | K10 COM 43 口上修正 (PLAYER→行為者) | [ ] |
| 4 | 4 | ビルド確認 | [ ] |
| 5 | 5 | 回帰テスト実行 | [ ] |

---

## Scope Note

本 Feature は E1 修正の初回スコープとして COM 43 のみを対象とする。
残りの E1 件数は Volume に応じて後続 Feature で対応。

**F269 レポート参照**: `.tmp/f269-fix-candidates.md`

---

## Dependencies

| Feature | Relationship |
|---------|-------------|
| F269 | depends_on (監査結果を入力として使用) |
| F265 | related (F265 の一部 Task と重複の可能性あり) |

---

## Links

- [feature-269.md](feature-269.md) - 口上主客関係監査
- [feature-265.md](feature-265.md) - 各種口上品質修正

---

## Execution Log

| Date | Task | Action | Result |
|------|------|--------|--------|
