# 堕落度連動衣装変化システム

## Status: DRAFT

**Target Version**: v2.5 (S2+)

訪問者への堕落が進行すると、キャラクターの衣装が自動的に露出度の高いものへ変化するシステム。

---

## Motivation

- NTR進行の視覚的フィードバック
- 堕落度の可視化によるゲーム体験の向上
- 既存の衣装システム・堕落システムの有効活用

---

## Design

### 堕落段階（5段階）

| Level | 状態 | 露出度 | 衣装例 |
|:-----:|------|--------|--------|
| 0 | 通常 | 標準 | キャラ固有プリセット (101-113) |
| 1 | 軽度堕落 | やや露出 | 肩出し・スカート短め |
| 2 | 中度堕落 | 明確な露出 | ミニスカ・胸元開き |
| 3 | 重度堕落 | 扇情的 | 下着見え・際どい衣装 |
| 4 | 完全堕落 | 最小限/全裸 | パターン0 (全裸) |

### トリガー条件

優先度順に判定:

```erb
@GET_CORRUPTION_OUTFIT_LEVEL(ARG)
  ;ARG = キャラID
  LOCAL = 0

  ;Level 4: 公衆便所
  IF TALENT:ARG:公衆便所 == 1
    LOCAL = 4

  ;Level 3: NTR不可逆 or 淫乱2(娼婦級)
  ELSEIF TALENT:ARG:NTR == 2 || TALENT:ARG:淫乱 == 2
    LOCAL = 3

  ;Level 2: NTR中 or 淫乱1 or 浮気公認3+
  ELSEIF TALENT:ARG:NTR == 1 || TALENT:ARG:淫乱 == 1 || TALENT:ARG:浮気公認 >= 3
    LOCAL = 2

  ;Level 1: 屈服度50+ or 浮気公認1+
  ELSEIF CFLAG:ARG:屈服度 >= 50 || TALENT:ARG:浮気公認 >= 1
    LOCAL = 1

  ENDIF

  RETURNF LOCAL
```

### 使用するTALENT/CFLAG

| ID | 変数 | 値域 | 用途 |
|----|------|------|------|
| TALENT:4 | 淫乱 | 0-2 | 主トリガー |
| TALENT:6 | NTR | 0-2 | 訪問者専用 |
| TALENT:7 | 公衆便所 | 0/1 | 最終段階 |
| TALENT:159 | 浮気公認 | 0-4 | 段階的判定 |
| CFLAG:21 | 屈服度 | 0-100+ | 細かい閾値 |

### 実行タイミング

1. **訪問者来訪時** - `VISITER_APPEARANCE()` (NTR.ERB)
2. **日次更新時** - `EVENTTURNEND()` or similar
3. **行為終了後** - 状態変化イベント

### 対象キャラ

全女性キャラ（10キャラ）:
- レミリア、フラン、咲夜、パチュリー、小悪魔、美鈴
- その他女性キャラ

### 衣装パターン設計（要検討）

各キャラ×5段階 = 50パターン必要

| キャラ | Lv0 | Lv1 | Lv2 | Lv3 | Lv4 |
|--------|:---:|:---:|:---:|:---:|:---:|
| 美鈴 | 101 | TBD | TBD | TBD | 0 |
| 咲夜 | 104 | TBD | TBD | TBD | 0 |
| ... | ... | ... | ... | ... | ... |

**選択肢**:
- A) 既存パターン流用（工数少）
- B) 新規パターン定義（表現力高）
- C) ハイブリッド（段階1-2流用、3-4新規）

---

## Implementation Tasks

| # | Task | Status |
|:-:|------|:------:|
| 1 | 衣装パターン設計（各キャラ×5段階） | [ ] |
| 2 | `GET_CORRUPTION_OUTFIT_LEVEL()` 実装 | [ ] |
| 3 | `NTR_UPDATE_OUTFIT()` 実装 | [ ] |
| 4 | トリガーポイントへの組み込み | [ ] |
| 5 | テスト・調整 | [ ] |

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | 公衆便所キャラが全裸 | variable | equals | CFLAG:服装パターン=0 | [ ] |
| 2 | NTR=2で露出衣装 | output | contains | (露出衣装表示) | [ ] |
| 3 | 通常時は標準衣装 | variable | equals | CFLAG:服装パターン=キャラ固有 | [ ] |
| 4 | 訪問者来訪で衣装更新 | build | succeeds | - | [ ] |

---

## References

- [CLOTHES.ERB](../../ERB/CLOTHES.ERB) - 衣装システム
- [NTR.ERB](../../ERB/NTR/NTR.ERB) - NTRメインロジック
- [Talent.csv](../../CSV/Talent.csv) - TALENT定義

---

## Notes

- 既存の衣装システム・堕落システムは完備済み
- 接続レイヤーの実装のみ必要
- 衣装パターン設計が最大の検討事項
