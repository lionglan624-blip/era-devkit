; eraTW霊夢 COM_63 reference
; Extracted: 2025-12-26
; 後背位アナル (Back Position Anal)

;==================================================
;63,後背位アナル
;==================================================
@M_KOJO_MESSAGE_COM_K1_63
CALL M_KOJO_MESSAGE_COM_K1_63_1
RETURN RESULT

@M_KOJO_MESSAGE_COM_K1_63_1
;-------------------------------------------------
;立位
;記入チェック（=0, 非表示、1, 表示）
LOCAL = 0
;-------------------------------------------------
IF LOCAL && TFLAG:193
	;-------------------------------------------------
	;小分岐
	;-------------------------------------------------
	;初めて
	;記入チェック
	LOCAL:1 = 0
	;-------------------------------------------------
	IF LOCAL:1 && FIRSTTIME(SELECTCOM)
		IF FLAG:時間停止
			PRINTFORML
			PRINTFORMW
			RETURN 1
		;アナル処女
		ELSEIF TCVAR:Ａ破瓜
			PRINTFORMW
			RETURN 1
		ELSE
			PRINTFORMW
			RETURN 1
		ENDIF
	ENDIF
	;-------------------------------------------------
	;挿入継続
	;記入チェック
	LOCAL:1 = 0
	;-------------------------------------------------
	IF LOCAL:1 && TCVAR:101 == PLAYER
		IF FLAG:時間停止
			PRINTFORML
			PRINTFORMW
			RETURN 1
		;前回射精した
		ELSEIF TCVAR:104
			PRINTFORMW
			RETURN 1
		ELSE
			PRINTFORMW
			RETURN 1
		ENDIF
	ENDIF
	;基本セット
	;時姦中
	IF FLAG:70
		PRINTFORMW
		RETURN 1
	;屈服3
	ELSEIF MARK:不埒刻印 == 3
		PRINTFORMW
		RETURN 1
	;それ以外
	ELSE
		PRINTFORMW
		RETURN 1
	ENDIF
ENDIF
;-------------------------------------------------
;後背位
;記入チェック（=0, 非表示、1, 表示）
LOCAL = 0
;-------------------------------------------------
IF LOCAL
	;-------------------------------------------------
	;小分岐
	;-------------------------------------------------
	;初めて
	;記入チェック
	LOCAL:1 = 0
	;-------------------------------------------------
	IF LOCAL:1 && FIRSTTIME(SELECTCOM)
		IF FLAG:時間停止
			PRINTFORML
			PRINTFORMW
			RETURN 1
		;アナル処女
		ELSEIF TCVAR:Ａ破瓜
			PRINTFORML
			PRINTFORMW
			RETURN 1
		ELSE
			PRINTFORML
			PRINTFORMW
			RETURN 1
		ENDIF
	ENDIF
	;-------------------------------------------------
	;挿入継続
	;記入チェック
	LOCAL:1 = 0
	;-------------------------------------------------
	IF LOCAL:1 && TCVAR:101 == PLAYER
		IF FLAG:時間停止
			PRINTFORML
			PRINTFORMW
			RETURN 1
		;前回射精した
		ELSEIF TCVAR:104
			PRINTFORMW
			RETURN 1
		ELSE
			PRINTFORMW
			RETURN 1
		ENDIF
	ENDIF
	;基本セット
	;時姦中
	IF FLAG:70
		PRINTFORMW
		RETURN 1
	;屈服3
	ELSEIF MARK:不埒刻印 == 3
		PRINTFORMW
		RETURN 1
	;それ以外
	ELSE
		PRINTFORMW
		RETURN 1
	ENDIF
ENDIF
RETURN 1

;==================================================
; 抽出完了
;==================================================

; NOTE: All dialogue content (PRINTFORMW lines) is empty in eraTW4.920
; Both position sections (立位 and 後背位) are disabled with LOCAL = 0
; Structure mirrors COM_62 but lacks implemented content
