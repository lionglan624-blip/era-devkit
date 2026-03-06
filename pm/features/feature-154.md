# Feature 154: Modular Kojo Modifier System

## Status: [DONE]

## Type: erb

## Background

現在の口上分岐はTALENT 4段階 × 4パターン = 16パターン/キャラ/COM。
NTR進行(FAV 9段階)や調教度(ABL:欲望/従順)を加えると組み合わせ爆発が発生：
- 全組み合わせ: 208パターン/キャラ/COM
- 10キャラ × 150COM = 管理不能

**解決策**: モジュラー修飾システム
- 基本口上(16パターン)は既存のまま
- PRE/POST修飾ブロックを条件に応じて追加
- 208 → 27ブロックに削減(87%減)

---

## Design

### Architecture

```erb
@KOJO_MESSAGE_COM_K{N}_{COM}
    ;=== PRE修飾（状態による前置き） ===
    CALL KOJO_MODIFIER_PRE_{COM}

    ;=== 基本口上（既存TALENT分岐） ===
    CALL KOJO_MESSAGE_COM_K{N}_{COM}_1

    ;=== POST修飾（状態による後付け） ===
    CALL KOJO_MODIFIER_POST_{COM}
RETURN
```

### Modifier Conditions

**PRE修飾**:
| 条件 | 追加内容 |
|------|----------|
| FAV_寝取られ | 訪問者を思い出す描写 |
| FAV_寝取られそう | 心の揺れ描写 |
| ABL:欲望 >= 7 | 期待に体が反応 |
| TALENT:処女 | 緊張/初々しさ |

**POST修飾**:
| 条件 | 追加内容 |
|------|----------|
| ABL:欲望 >= 5 | もっとねだる |
| CHK_NTR_SATISFACTORY | 訪問者との比較 |
| MARK:快楽刻印 | 快楽の記憶 |

### COM適用範囲

| 適用 | COM種別 | COM数 |
|:----:|---------|:-----:|
| ◎必須 | 愛撫系/コミュ系/挿入系/奉仕系/される系 | 43 |
| ○推奨 | 道具/SM系/ハード系/脱衣系 | 30 |
| △任意 | 特殊/行動系/セルフ系 | 30 |
| ×不要 | 観察/補助系/移動生活系/訪問者系 | 47 |

### Shared vs Character-Specific

```
共通修飾: KOJO_MODIFIER_PRE_COMMON, KOJO_MODIFIER_POST_COMMON
固有修飾: KOJO_MODIFIER_PRE_K{N}_{COM} (必要な場合のみ)
```

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | PRE修飾関数が存在 | code | matches | `/@KOJO_MODIFIER_PRE_COMMON[\s\S]{100,}/` | [x] |
| 2 | POST修飾関数が存在 | code | matches | `/@KOJO_MODIFIER_POST_COMMON[\s\S]{100,}/` | [x] |
| 3 | NTR時PRE修飾出力 | output | contains | 脳裏をよぎ | [x] |
| 4 | 欲望高POST修飾出力 | output | contains | もっと | [x] |
| 5 | 基本口上との結合 | output | contains | 気持ちいい | [x] |
| 6 | ビルド成功 | build | succeeds | - | [x] |

### AC Details

**AC#1-2**: Code existence verification using regex
- **Type**: `code` - Searches ERB files for function definitions
- **Matcher**: `matches` - Regex pattern ensures minimum 100 chars of implementation
- **Pattern**: `/@KOJO_MODIFIER_{PRE|POST}_COMMON[\s\S]{100,}/`
- **Rationale**: Ensures modifier functions exist with substantive content (not just stubs)
- **Note**: ERB uses `@` prefix for function definitions, not `FUNCTION #DIMS`

**AC#3**: NTR状態PRE修飾出力確認
- **Test scenario**: FAV_寝取られそう状態でCOM_0実行
- **Expected output**: NTR訪問者を思い出す描写 (例: "イメージが脳裏をよぎる")
- **Verification**: Test framework with state: `{"FAV:TARGET:7": 1}` (FAV_寝取られそう = 7)
- **Rationale**: "脳裏をよぎ" is the core pattern used in existing NTR dialogue
- **Note**: FAV constants: 寝取られ=9, 寝取られ寸前=8, 寝取られそう=7

**AC#4**: 欲望高POST修飾出力確認
- **Test scenario**: ABL:欲望 >= 5でCOM_0実行
- **Expected output**: もっとねだる描写 (例: "もっと")
- **Verification**: Test framework with state: `{"ABL:TARGET:欲望": 5}`

**AC#5**: PRE+BASE+POST統合確認
- **Test scenario**: 基本口上が正常に出力されることを確認
- **Expected output**: 基本口上の一部 (例: "気持ちいい")
- **Rationale**: 修飾システム導入後も基本口上が破壊されていないことを確認

**AC#6**: ビルド成功
- **Command**: `dotnet build`
- **Expected**: Exit code 0

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | KOJO_MODIFIER_PRE_COMMON関数作成 | [O] |
| 2 | 2 | KOJO_MODIFIER_POST_COMMON関数作成 | [O] |
| 3 | 3 | NTR用PRE修飾実装 | [O] |
| 4 | 4 | 欲望用POST修飾実装 | [O] |
| 5 | 5 | COM_0(愛撫)でパイロット統合 | [O] |
| 6 | 6 | ビルド確認 | [O] |

---

## Phase Integration

This feature introduces **Phase 8e-mod** (Modular Modifier Layer):

```
Phase 8d: 基本口上(TALENT分岐) ← 現在
Phase 8e-mod: モジュラー修飾導入 ← この機能
Phase 8h: NTR修飾拡充
```

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-20 | Initialization | initializer | Status → [IN_PROGRESS] | READY |
| 2025-12-20 | COMPLETE | opus/implementer | All ACs verified, user approved | SUCCESS |

## Links
- [index-features.md](index-features.md)
- [content-roadmap.md](content-roadmap.md)
- [kojo-phases.md](reference/kojo-phases.md)
