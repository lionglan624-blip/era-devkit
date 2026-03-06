# Feature 295: content-roadmap.md への F292 分析結果反映

## Status: [DONE]

## Type: infra

## Background

### Philosophy
eraTW の分岐構造を理解し、紅魔館 protoNTR の kojo 品質を eraTW レベルに引き上げる

### Problem
F292 で eraTW 分岐構造の詳細分析と roadmap 更新提案が完成したが、content-roadmap.md への実際の反映が未実施。

### Goal
F292 の分析結果を content-roadmap.md に反映し、新規 Phase 追加を含む長期計画を確定する。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 8j セクション追加 | code | Grep(content-roadmap.md) | contains | "Phase 8j" | [x] |
| 2 | 射精状態分岐の説明存在 | code | Grep(content-roadmap.md) | contains | "射精状態" or "Ejaculation" | [x] |
| 3 | Phase 8l セクション追加 | code | Grep(content-roadmap.md) | contains | "Phase 8l" or "FIRSTTIME" | [x] |
| 4 | 日常 COM への FIRSTTIME 適用記載 | code | Grep(content-roadmap.md) | contains | "日常" or "300系" | [x] |
| 5 | 精液中毒分岐を Phase 8e に追記 | code | Grep(content-roadmap.md) | contains | "精液中毒" or "ABL:31" | [x] |
| 6 | Version Roadmap 更新 | code | Grep(content-roadmap.md) | contains | version adjustment | [x] |
| 7 | kojo-phases.md に Phase 8j 追加 | code | Grep(kojo-phases.md) | contains | "Phase 8j" | [x] |
| 8 | kojo-phases.md に Phase 8l 追加 | code | Grep(kojo-phases.md) | contains | "Phase 8l" | [x] |

### AC Details

**AC1**: `Grep("Phase 8j", content-roadmap.md)` → matches (射精状態分岐)
**AC2**: `Grep("射精状態|Ejaculation", content-roadmap.md)` → matches
**AC3**: `Grep("Phase 8l|FIRSTTIME", content-roadmap.md)` → matches (初回実行分岐)
**AC4**: `Grep("日常|300系", content-roadmap.md)` → matches
**AC5**: `Grep("精液中毒|ABL:31", content-roadmap.md)` → matches
**AC6**: Version Roadmap に新 Phase 対応バージョンが追加されている
**AC7**: `Grep("Phase 8j", reference/kojo-phases.md)` → matches
**AC8**: `Grep("Phase 8l", reference/kojo-phases.md)` → matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Phase 8j "Ejaculation-state kojo" セクションを content-roadmap.md に追加 | [x] |
| 2 | 3,4 | Phase 8l "FIRSTTIME Guard" セクションを content-roadmap.md に追加（日常 COM 含む） | [x] |
| 3 | 5 | Phase 8e に精液中毒 (ABL:31) 分岐を content-roadmap.md に追記 | [x] |
| 4 | 6 | Version Roadmap を調整 | [x] |
| 5 | 7,8 | kojo-phases.md に Phase 8j, 8l の詳細仕様を追加 | [x] |

---

## Implementation Notes

### Phase 構成変更案

**現行 Phase 8f の分離**:

| Phase | 名前 | 対象 | Lines | 概要 |
|:-----:|------|------|:-----:|------|
| 8f | First Experience (初体験) | 特定 COM | 15-30 | 処女喪失等の重要イベント |
| **8l** | **FIRSTTIME Guard (初回実行)** | **全 COM** | 1-3 | 各コマンド初回時の短文 |

**理由**:
- 8f (初体験) = 人生で一度の重要イベント（15-30 lines）
- 8l (初回実行) = 各コマンドを初めて実行した時（1-3 lines、88 COM で使用）
- 概念が異なるため別 Phase として管理

### 日常 COM への FIRSTTIME 適用

**対象 COM 例**:
- COM_300系: 会話、お茶を淹れる、散歩など
- COM_400系: 食事、休憩など

**効果**:
- 関係構築の自然さ向上
- キャラクター表現（緊張、照れ、期待）
- 実装コスト低（1-3 lines）

**例（COM_303 お茶を淹れる）**:
```erb
IF FIRSTTIME()
    PRINTFORMW 「……%CALLNAME:MASTER%にお茶を淹れるのは、初めてね。美味しいといいけど……」
    RETURN 1
ENDIF
; 以降通常口上
```

### 精液中毒分岐 (Phase 8e 追記)

**ABL:31 精液中毒** を Phase 8e の分岐対象に追加:

```erb
; GET_ABL_BRANCH(TARGET, ABL:精液中毒) → 0:低 / 1:中 / 2:高
IF GET_ABL_BRANCH(TARGET, 31) == 2
    ; 精液中毒:高 → 精液を求める口上
    PRINTFORMW 「……もっと、欲しい……♥」
ENDIF
```

**適用 COM**:
- COM_81 (フェラチオ) - 精飲時
- COM_80-85 (射精系) - 射精後反応

### F292 からの主な提案（統合版）

| 優先度 | 提案 | Phase | 対応 |
|:------:|------|:-----:|------|
| **高** | 射精状態分岐 (NOWEX/TCVAR) | **8j (新規)** | 新セクション追加 |
| **高** | FIRSTTIME 初回実行 | **8l (新規)** | 8f から分離、日常 COM 含む |
| **高** | 精液中毒 (ABL:31) 分岐 | 8e | 既存 8e に追記 |
| **中** | CFLAG 状況分岐 | 8k | 既存 8k に統合 |
| **低** | MARK 刻印 | - | v2.x 以降検討 |

### 検討事項

1. **Version 番号の調整**: 新 Phase 追加に伴うバージョン再編成
2. **Phase 8l の配置**: 8k の後に追加
3. **工数見積もり**: F292 の見積もり（560 DATALIST）を roadmap に記載するか

---

## Session Analysis (2026-01-01)

### 調査目的
F295 と roadmap、designs、reference の重複確認および品質基準の明確化

### 調査結果

#### 1. 重複確認: なし
designs フォルダの全ファイルを確認:
- v0.7-audit-fix.md: F253+F269 監査修正 → 独立
- v0.8-kojo-80-90.md: 80-90系 Phase 8d → 独立
- new-commands.md: v1.7 +37 COM → 独立（COM追加、F295は口上品質）
- m-orgasm-system.md: v1.8 M絶頂 → 独立

Version Roadmap ↔ designs 整合: ✅ 問題なし

#### 2. 未反映箇所
| 項目 | content-roadmap.md | kojo-phases.md | F295 |
|------|:------------------:|:--------------:|:----:|
| Phase 8j (射精状態) | ❌ 未記載 | ❌ 未記載 | ✅ 追加予定 |
| Phase 8l (初回実行) | ❌ 未記載 | ❌ 未記載 | ✅ 追加予定 |
| ABL:31 精液中毒 → 8e | ❌ 未記載 | ❌ 未記載 | ✅ 追記予定 |

**課題**: kojo-phases.md への Phase 8j, 8l 追加が AC/Tasks に含まれていない

#### 3. 品質基準の明確化

**v0.6 = Phase 8d = C2 品質**:
- 4パターン (DATALIST × 4)
- 4-8行
- 地の文 + セリフ
- eraTW 霊夢相当
- キャラ間被りなし

**F295 で追加する Phase の品質基準（確定）**:

| Phase | 行数 | パターン | 分岐 | v0.6相当 | 備考 |
|:-----:|:----:|:--------:|------|:--------:|------|
| **8e 追記 (精液中毒)** | 4-8行以上 | 分岐ごとに4 | TALENT×ABL | ✅ YES | eraTW参考、キャラ被りなし |
| **8j (射精状態)** | 4-8行以上 | 分岐ごとに4 | TALENT×NOWEX×TCVAR | ✅ YES | eraTW参考、状況描写重視 |
| **8l (初回実行)** | **8-15行以上** | 1 | FIRSTTIME のみ | ✅ YES | **長文可（25行も可）** |

**結論**: 全 Phase が v0.6 相当の品質基準を満たす。

#### Phase 8l eraTW 参照方針

**実装時の参照ルール**:
1. **合致する COM がある場合**: eraTW の該当 COM の FIRSTTIME 口上を参照
2. **合致する COM がない場合**: 下記参考資料（COM_愛撫）をベースに作成

**参考資料（COM_愛撫 FIRSTTIME、25行）**:
```erb
; eraTW 霊夢 M_KOJO_K1_コマンド.ERB 3772-3797行
PRINTFORML
PRINTFORMW 「――っ！」
PRINTFORMW %CALLNAME:MASTER%が霊夢の肩に触れると、霊夢はぴくり、と体を跳ねさせた。
PRINTFORMW %CALLNAME:MASTER%はそのまま霊夢の肩に触れ続ける。
PRINTFORMW 「……ん……」
PRINTFORMW %CALLNAME:MASTER%は霊夢の肩から腕へと、手を滑らせる。
PRINTFORMW 霊夢の肌はきめ細やかで、なめらかな曲線美を描いている。
PRINTFORMW 「んっ……ひゃっ……」
PRINTFORMW %CALLNAME:MASTER%は霊夢の乳房に触れた。
PRINTFORMW 霊夢は小さな悲鳴を上げたが、拒絶はしない。
PRINTFORMW そのまま、%CALLNAME:MASTER%は霊夢の乳房を、ゆっくりと撫で始めた。
PRINTFORMW 揉むのではなく、撫でる。
PRINTFORMW ゆっくりと、じっくりと。
PRINTFORMW 霊夢が%CALLNAME:MASTER%の手の感触に慣れるのを待ってから、%CALLNAME:MASTER%は霊夢の乳房を揉み始めた。
PRINTFORMW 「あっ……やぁ……」
PRINTFORMW 霊夢は顔を真っ赤にしてじっとしている。
PRINTFORMW %CALLNAME:MASTER%は少しの間、霊夢の胸の感触を味わっていた。
PRINTFORMW やがて%CALLNAME:MASTER%は霊夢の陰部へと手を伸ばした。
PRINTFORMW 「――っ……」
PRINTFORMW 霊夢は少し脚を閉じるも、すぐに力を抜く。
PRINTFORMW それから%CALLNAME:MASTER%は、霊夢の陰部をまた『撫でる』。
PRINTFORMW 霊夢は真っ赤な顔を手で押さえて、それでもじっとしている。
PRINTFORMW %CALLNAME:MASTER%は霊夢の陰部を撫でるのをやめ、さきほどまで霊夢の陰部を愛撫していた指を見た。
PRINTFORMW その指には少しだけ、透明な液体がついている。
PRINTFORMW 指をすり合わせ、その液体が作る銀色の橋を見ると、%CALLNAME:MASTER%は満足して愛撫を再開した。
```

**キャッシュ運用**: eratw-reader が COM 別に eraTW 参照をキャッシュするため、kojo-writer は実装時にキャッシュを参照して効率的に作成できる。

#### 4. 完全分岐 vs モジュール式の整理

**2つの方式が共存**（混乱防止のため明記）:

| 方式 | Phase | 説明 | 状態 |
|:----:|:-----:|------|:----:|
| **完全分岐** | 8e (ABL/EXP) | TALENT×ABLネスト、条件別口上で即RETURN | 未実装 |
| **モジュール式** | 8e-mod | PRE/POST修飾、基本口上と共存 | F154 実装済み |

**完全分岐（ネスト分岐）**: 条件マッチで専用口上を出力し即RETURN
```erb
IF FIRSTTIME(SELECTCOM)
    PRINTFORMW ...（長文）
    RETURN 1    ; ← 基本口上には到達しない
ENDIF
; 基本口上
```

**モジュール式（PRE/POST修飾）**: 基本口上の前後に修飾を追加、基本口上は常に出力
```erb
CALL KOJO_MODIFIER_PRE_{COM}    ; PRE修飾
CALL KOJO_MESSAGE_COM_K{N}_{COM}_1  ; 基本口上（常に出力）
CALL KOJO_MODIFIER_POST_{COM}   ; POST修飾
```

**content-roadmap.md の記述との整合性**:
- 「モジュール式ではない」は Phase 8e（ABL/EXP分岐）の話
- Phase 8e-mod（F154）とは別概念で、両者は共存する設計
- **整合性問題なし**

#### 5. F295 で追加する Phase の実装方式

**全て完全分岐（即RETURN）で実装**:

| Phase | 方式 | 理由 |
|:-----:|:----:|------|
| 8e追記（精液中毒） | **完全分岐** | TALENT×ABLネスト、条件別口上 |
| 8j（射精状態） | **完全分岐** | 射精中/直後は専用口上で即RETURN |
| 8l（初回実行） | **完全分岐** | FIRSTTIMEは専用口上で即RETURN |

**eraTW 実装パターン準拠**:
```erb
; Phase 8l: FIRSTTIME（最優先判定）
IF FIRSTTIME(SELECTCOM)
    PRINTFORMW ...（8-15行以上）
    RETURN 1
ENDIF

; Phase 8j: 射精状態
IF NOWEX:MASTER:11  ; 射精中
    PRINTFORMW ...
    RETURN 1
ENDIF

; Phase 8e: ABL分岐（TALENT内ネスト）
IF TALENT:恋人
    IF ABL:精液中毒 >= 高
        PRINTFORMW ...
        RETURN 1
    ENDIF
    ; 基本口上
    PRINTDATA ...
ENDIF
```

#### 6. 精液中毒 (ABL:31) の根拠
- F189 (Kojo Ability/Talent Branch Systematic Review) で ABL:31 精液中毒を分岐対象として明記済み
- F292 分析ではなく、F189 のコンテンツ充足から追加

### 判断待ち事項

| # | 項目 | 選択肢 | 推奨 | 決定 |
|:-:|------|--------|:----:|:----:|
| 1 | kojo-phases.md 更新を AC に追加するか | Yes / No / 別Feature | Yes | **Yes** |
| 2 | 8j/8l の品質基準を Implementation Notes に追記するか | Yes / No | Yes | **Yes（上記に記載済み）** |
| 3 | content-roadmap の「モジュール式ではない」記述を Phase 8e-mod (F154) と整合させるか | 修正 / 注釈追加 / 別Issue | 注釈追加 | **問題なし（§4で整理済み）** |

### 次セッションでの作業

1. ~~**判断待ち事項の決定**: ユーザーに確認~~ → **全て決定済み**
2. ~~**AC/Tasks 更新**: kojo-phases.md 更新を AC に追加~~ → **AC7-8, Task5 追加済み**
3. ~~**品質基準追記**: Implementation Notes に Phase 別品質基準を追加~~ → **§3, §5 に記載済み**
4. **実装**: content-roadmap.md / kojo-phases.md への反映

---

### Session Analysis (2026-01-01 続き): ワークフロー統合検討

#### 7. ワークフロー統合の課題

**現在のハードコード箇所**:

| ファイル | 内容 | 問題 |
|----------|------|------|
| kojo_mapper.py:127 | `CURRENT_PHASE = "C2"` | Phase 固定 |
| kojo_mapper.py:123 | `PHASE_REQUIREMENTS` | Phase 8j/8l 未定義 |
| kojo-init.md:73 | `(Phase 8d)` | Phase 8d 固定テンプレート |
| kojo-writing SKILL | 品質基準 | Phase 8d のみ |

**課題**: F295 で Phase 8j/8l を定義しても、実装フローが対応していない

#### 8. SKILL 拡張アプローチ（採用）

**方針**: kojo-writing SKILL を Phase 定義の SSOT にする

```
kojo-writing SKILL (SSOT)
├── ## Phase Definitions
│   ├── Phase 8d: 基本口上 (TALENT 4分岐, 4-8行)
│   ├── Phase 8j: 射精状態 (完全分岐, 4-8行以上)
│   └── Phase 8l: 初回実行 (完全分岐, 8-15行以上)
├── ## Quality Standards (Phase別)
├── ## ERB Templates (Phase別)
└── ## COM Scope (Phase別対象COM)

各ツール/コマンド → SKILL を参照
├── kojo_mapper.py
├── kojo-init.md
├── kojo-writer.md
└── eratw-reader.md
```

#### 9. Feature 分割決定

**F295**: roadmap 定義のみ（本 Feature）
- content-roadmap.md に Phase 8j/8l 追加
- kojo-phases.md に Phase 8j/8l 詳細仕様追加
- ワークフロー変更は**しない**

**F296**: kojo-writing SKILL 拡張
- Phase Definitions セクション追加
- Phase 別品質基準追加
- Phase 別 ERB Template 追加
- 依存: F295（Phase 定義が確定してから）

**F297**: ツール連携更新
- kojo_mapper.py: SKILL 参照 or PHASE_REQUIREMENTS 更新
- kojo-init.md: Phase 判定ロジック追加
- eratw-reader.md: Phase 別抽出方針追加
- 依存: F296（SKILL が SSOT として確立してから）

#### 10. 依存関係

```
F295 (roadmap 定義)
  ↓ Phase 8j/8l が content-roadmap.md / kojo-phases.md に定義される
F296 (SKILL 拡張)
  ↓ SKILL が Phase 定義の SSOT になる
F297 (ツール連携)
  ↓ 各ツールが SKILL を参照するようになる
Phase 8j/8l Feature 作成可能
```

**F295 の結果で F296/F297 が変わるか？**
- **Phase 定義が変われば**: F296 の SKILL 内容が変わる
- **品質基準が変われば**: F296 の Quality Standards が変わる
- **対象 COM が変われば**: F297 の kojo-init ロジックが変わる

→ **F295 を先に完了し、確定した定義を F296/F297 に反映する**

### 参照ファイル一覧

| ファイル | 内容 |
|----------|------|
| content-roadmap.md | 長期計画、Phase定義、Version Roadmap |
| reference/kojo-phases.md | Phase 8f-8k 詳細仕様 |
| reference/eratw-branch-analysis.md | F292 成果物、差分表、提案 |
| feature-189.md | ABL/TALENT分岐レビュー、精液中毒根拠 |
| feature-154.md | Phase 8e-mod モジュラー修飾（完了済） |
| designs/v0.8-kojo-80-90.md | 80-90系計画 |

---

## Reference

- [eratw-branch-analysis.md](reference/eratw-branch-analysis.md) - F292 成果物
- [feature-292.md](feature-292.md) - 親 Feature
- [feature-189.md](feature-189.md) - ABL/TALENT分岐レビュー（精液中毒根拠）
- [feature-154.md](feature-154.md) - Phase 8e-mod モジュラー修飾
- Abl.csv: ABL:31 = 精液中毒
- [feature-296.md](feature-296.md) - 子 Feature: kojo-writing SKILL 拡張
- [feature-297.md](feature-297.md) - 子 Feature: ツール連携更新

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 16:17 | START | implementer | Task 1 | - |
| 2026-01-01 16:17 | END | implementer | Task 1 | SUCCESS |
| 2026-01-01 16:17 | START | implementer | Task 2 | - |
| 2026-01-01 16:17 | END | implementer | Task 2 | SUCCESS |
| 2026-01-01 16:17 | START | implementer | Task 3 | - |
| 2026-01-01 16:17 | END | implementer | Task 3 | SUCCESS |
| 2026-01-01 16:17 | START | implementer | Task 4 | - |
| 2026-01-01 16:17 | END | implementer | Task 4 | SUCCESS |
| 2026-01-01 16:17 | START | implementer | Task 5 | - |
| 2026-01-01 16:17 | END | implementer | Task 5 | SUCCESS |

---

## Links
- [index-features.md](index-features.md)
- [content-roadmap.md](content-roadmap.md)
- 親Feature: [feature-292.md](feature-292.md)
- 子Feature: [feature-296.md](feature-296.md) (SKILL 拡張)
- 子Feature: [feature-297.md](feature-297.md) (ツール連携)
