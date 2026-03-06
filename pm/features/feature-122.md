# Feature 122: 妖精メイド増員コマンド専用化

Type: erb

## Summary

「メイドを増やしませんか」コマンドを咲夜・レミリア専用コマンドに改修し、成功/失敗分岐を削除してテンポ改善。

## Background

現状の問題:
- 成功/失敗の分岐(4パターン)があり、テンポが悪い
- コマンド名が長い「新しいメイドを増やしませんか？」

現在の実装: [COMF300ex02_妖精メイド増員.ERB](../ERB/会話拡張/COMF300ex02_妖精メイド増員.ERB)

## Design Decision

| 項目 | 決定 |
|------|------|
| コマンド名 | **メイド増員** |
| 対象キャラ | 咲夜(4)・レミリア(5)がTARGETの時のみ |
| 分岐削除方式 | スキル不足時は_ENABLEで非表示（コマンド自体が出ない） |
| コマンド数 | 1コマンド維持（2人共用） |

## Implementation

```erb
@COM300_02_ENABLE
    RESULTS:0 = %"[ 2]-メイド増員"%
    ; 咲夜・レミリアがTARGETの時だけ表示
    IF NO:TARGET != 4 && NO:TARGET != 5
        RETURN 0
    ENDIF
    ; 妖精メイド枠が上限なら非表示
    IF 妖精メイド採用状態:(妖精メイド雇用上限-1) != 妖精メイド採用状態_未許可
        RETURN 0
    ENDIF
    ; スキル不足なら非表示（分岐なし）
    IF ((ABL:MASTER:清掃技能+ABL:MASTER:戦闘能力+ABL:MASTER:料理技能)/3) <= FairyMaids_HiredCount()
        RETURN 0
    ENDIF
    RETURN 1

@COM300_02
    ; 成功時の処理のみ（失敗分岐なし）
    CALLF FairyMaids_addVacancy()
    TCVAR:会話累積値 += 200 / (3 + ABL:MASTER:話術技能)
    TIME += 10
    EXP:MASTER:会話経験 ++
    ; キャラ別口上
    TRYCCALLFORM COM300_02_%TOSTR(TARGET,"00")%
        RETURN RESULT:0
    CATCH
        RETURN 0
    ENDCATCH
```

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | コマンド名変更 | code | contains | "メイド増員" | [x] |
| 2 | 咲夜TARGET時チェック実装 | code | contains | "NO:TARGET != 4" | [x] |
| 3 | レミリアTARGET時チェック実装 | code | contains | "NO:TARGET != 5" | [x] |
| 4 | スキル不足チェック実装 | code | contains | "(ABL:MASTER:清掃技能+ABL:MASTER:戦闘能力+ABL:MASTER:料理技能)/3" | [x] |
| 5 | 成功時処理のみ実装（FairyMaids_addVacancy呼び出し） | code | contains | "CALLF FairyMaids_addVacancy()" | [x] |
| 6 | 失敗分岐削除（成否判定変数なし） | code | not_contains | "成否判定" | [x] |
| 7 | 失敗処理関数削除 | code | not_contains | "@COM300_02_00" | [x] |
| 8 | ビルド成功 | build | succeeds | - | [x] |

## Tasks

| Task# | AC | Description | Status |
|:-----:|:--:|-------------|:------:|
| 1 | 1-4 | COM300_02_ENABLE改修（コマンド名、TARGET/スキルチェック） | [x] |
| 2 | 5-6 | COM300_02本体改修（失敗分岐削除、成功処理のみ） | [x] |
| 3 | 7 | 失敗用関数削除（COM300_02_00_xx） | [x] |
| 4 | 8 | ビルド確認 | [x] |

## Execution State

| Field | Value |
|-------|-------|
| Current Status | DONE |
| Assigned Subagent | finalizer |
| Blocker | None |
| Last Updated | 2025-12-18 21:38 |

---

## Progress Log

### Entry 1 - Investigation Start
- 現状: COM300_02 で TARGET==4(咲夜) or TARGET==5(レミリア) で分岐
- 成否判定: (清掃+戦闘+料理)/3 > メイド数
- 問題: 毎回会話初回に表示、管理系なのにテンポ阻害

### Entry 2 - Design Decision
- TARGETシステム調査完了: 複数キャラ同時存在時、選択したTARGETに対してのみコマンド表示可能
- 方針決定: 1コマンド維持、_ENABLEでスキルチェック、分岐削除
- コマンド名: 「メイド増員」に短縮

### Entry 3 - AC Validation (2025-12-18)
- AC#1-4: Changed from "manual" to "code" type for automated verification
- AC#1: Verify command name changed to "メイド増員"
- AC#2-3: Verify TARGET checks (NO:TARGET != 4 && NO:TARGET != 5)
- AC#4: Verify skill formula implementation
- AC#5: Verify success-only implementation (CALLF FairyMaids_addVacancy())
- AC#6: Verify failure branch removal (成否判定 variable removed)
- AC#7: Verify failure functions deleted (@COM300_02_00, @COM300_02_00_04, @COM300_02_00_05)
- AC#8: Build success verification
- All ACs now testable with "code" type matcher (Grep) and "build" type

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2025-12-18 21:32 | START | implementer | Task 1 | - |
| 2025-12-18 21:32 | END | implementer | Task 1 | SUCCESS (1min) |
| 2025-12-18 21:35 | START | implementer | Task 2 | - |
| 2025-12-18 21:35 | END | implementer | Task 2 | SUCCESS (1min) |
| 2025-12-18 21:37 | START | implementer | Task 3 | - |
| 2025-12-18 21:37 | END | implementer | Task 3 | SUCCESS (1min) |
| 2025-12-18 21:38 | START | finalizer | Feature 122 | - |
| 2025-12-18 21:38 | END | finalizer | Feature 122 | DONE (1min) |

## Notes

- 元ファイル: `Game/ERB/会話拡張/COMF300ex02_妖精メイド増員.ERB`
- 咲夜=4, レミリア=5
- 削除対象: `@COM300_02_00`, `@COM300_02_00_04`, `@COM300_02_00_05`（失敗時処理）
