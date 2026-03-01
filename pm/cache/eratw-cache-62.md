; eraTW霊夢 COM_62 reference
; Extracted: 2025-12-26
; 正常位アナル (Normal Position Anal)

;==================================================
;62,正常位アナル
;==================================================
@M_KOJO_MESSAGE_COM_K1_62
CALL M_KOJO_MESSAGE_COM_K1_62_1
RETURN RESULT

@M_KOJO_MESSAGE_COM_K1_62_1
;-------------------------------------------------
;立位
;記入チェック（=0, 非表示、1, 表示）
LOCAL = 1
;-------------------------------------------------
IF LOCAL && TFLAG:193
	;-------------------------------------------------
	;小分岐
	;-------------------------------------------------
	;初めて
	;記入チェック
	LOCAL:1 = 1
	;-------------------------------------------------
	IF LOCAL:1 && FIRSTTIME(SELECTCOM)
		IF FLAG:時間停止
			PRINTFORML
			PRINTFORMW %CALLNAME:MASTER%は霊夢の片脚を持ち上げ、正面から菊穴へと亀頭をあてがう。
			PRINTFORMW そして、%CALLNAME:MASTER%はぬちゅ、と霊夢の腸内へと陰茎を進入させた。
				SIF TCVAR:Ａ破瓜
					PRINTFORMW 霊夢の菊穴はぎゅう、と強く締め付けてきて、%CALLNAME:MASTER%に丁度良い快楽を与えてくる。
			PRINTFORMW %CALLNAME:MASTER%は半分くらいまで挿入してから、抽送を開始した。
			RETURN 1
		;アナル処女
		ELSEIF TCVAR:Ａ破瓜
			PRINTFORML
			PRINTFORMW %CALLNAME:MASTER%は霊夢に、ある提案をする。
			PRINTFORMW 霊夢、今日は一つ試したいことがあるんだ。
			PRINTFORMW 「……何よ、あんまり痛いのは嫌よ」
			PRINTFORMW 後ろの穴でやってみないか。
			PRINTFORMW 「……後ろ？　……後ろって……違うほうの穴じゃない」
			PRINTFORMW そうだが。
			PRINTFORMW 「そうだが、って……後ろの穴でするなんて、どんな感触かしらね……」
			PRINTFORMW 霊夢は少し黙考している。
			PRINTFORMW 興味があるならやってみよう、そう言って、%CALLNAME:MASTER%は霊夢の片脚を持ち上げる。
			PRINTFORMW 「ちょ、ちょっと！？　……まあ、いいか……」
			PRINTFORMW 霊夢は諦めたかのように抵抗するのをやめた。
			PRINTFORMW %CALLNAME:MASTER%は霊夢の菊穴に亀頭をあてがった。
			PRINTFORMW 「……どきどき、する……」
			PRINTFORMW 霊夢ははじめての体験に胸を高鳴らせているようだ。
			PRINTFORMW %CALLNAME:MASTER%はぬちゅ、と霊夢の腸内へ陰茎を進入させる。
			PRINTFORMW 「ん、くぅ、っ……い、違和感が……半端じゃ、ないっ……」
			PRINTFORMW 霊夢は慣れない感覚に戸惑っている。
			PRINTFORMW %CALLNAME:MASTER%はゆっくりと奥まで押し込んでいき、根元まで挿しこんだ。
			PRINTFORMW そして%CALLNAME:MASTER%は、ゆっくりと抽送を開始した。
			RETURN 1
		ELSE
			PRINTFORML
			PRINTFORMW 霊夢、今日は後ろでしよう。
			PRINTFORMW 「え……良いけど、ここ、寝転がれないわよ？」
			PRINTFORMW 何、立ってすれば良い、そう言って、%CALLNAME:MASTER%は霊夢の片脚を持ち上げる。
			PRINTFORMW 「ひゃっ……とと、ちょっと危ないわね……」
			PRINTFORMW 霊夢は体勢を崩しかけている。
			PRINTFORMW %CALLNAME:MASTER%は霊夢の菊穴にそそり立った一物をあてがい、ずぶ、と挿入した。
			PRINTFORMW 「ん、は……ふぅっ……はっ、はぁっ……」
			PRINTFORMW 霊夢は向かい合った%CALLNAME:MASTER%の顔に荒い息を吹きかけている。
			PRINTFORMW %CALLNAME:MASTER%は陰茎の半分ほどまで挿れて、抽送を開始した。
			RETURN 1
		ENDIF
	ENDIF
	;-------------------------------------------------
	;挿入継続
	;記入チェック
	LOCAL:1 = 1
	;-------------------------------------------------
	IF LOCAL:1 && TCVAR:101 == PLAYER
		IF FLAG:時間停止
			PRINTFORML
			PRINTFORMW %CALLNAME:MASTER%は霊夢の菊穴に挿入したまま、体勢を変えた。
			PRINTFORMW 向かい合うような姿勢となって、%CALLNAME:MASTER%は再度抽送を始めた。
			PRINTFORMW 霊夢は%CALLNAME:MASTER%にされるがままとなっている。
			RETURN 1
		;前回射精した
		ELSEIF TCVAR:104
			PRINTFORML
			PRINTFORMW 「んっ……中に、出てる……」
			PRINTFORMW 霊夢は腸内の精液の感触に甘い息を吐いている。
			PRINTFORMW %CALLNAME:MASTER%は体勢を変え、向かい合うような姿勢となる。
			PRINTFORMW そして%CALLNAME:MASTER%は抽送を再開した。
			RETURN 1
		ELSE
			PRINTFORML
			PRINTFORMW 「……ん、もう止めるの……？」
			PRINTFORMW 霊夢は%CALLNAME:MASTER%が抽送を止めたことを不思議に思っている。
			PRINTFORMW %CALLNAME:MASTER%は無言で体勢を変え、霊夢と向かい合うような体勢となる。
			PRINTFORMW そしてそのまま、抽送を再開した。
			RETURN 1
		ENDIF
	ENDIF
	;基本セット
	;時姦中
	IF FLAG:70
		PRINTFORML
		PRINTFORMW %CALLNAME:MASTER%は霊夢の前に立ち、脚を持ち上げた。
		PRINTFORMW 菊穴にそそり立つ一物を当て、%CALLNAME:MASTER%は一気に奥まで貫く。
		PRINTFORMW %CALLNAME:MASTER%は霊夢が動かないのを良いことに激しく抽送している……
		RETURN 1
	;A絶頂、挿れただけで達しちゃう淫らな巫女さんって言うのも良いかも
	ELSEIF NOWEX:Ａ絶頂
		PRINTFORML
		PRINTFORMW 「あっ……その、今回は、どっちでするの……？」
		PRINTFORMW 霊夢は%CALLNAME:MASTER%の直立する一物を見て訊ねてきた。
		PRINTFORMW 後ろでしたい、と伝えると、霊夢は%CALLNAME:MASTER%の正面に歩み寄ってきた。
		PRINTFORMW どうやら正面向かい合ってしたいらしい。
		PRINTFORMW %CALLNAME:MASTER%は霊夢の片脚を持ち上げ、亀頭を霊夢の菊穴にあてがった。
		PRINTFORMW 少し間を空けてから、ずん、と奥まで一気に挿すと、
		PRINTFORMW 「く、ぁ―――――っ！　……はぁ……ん、っ、はっ、は、んっ、ふっ」
		PRINTFORMW と、一瞬で絶頂まで達した。
		PRINTFORMW %CALLNAME:MASTER%は霊夢が達したばかりにもかかわらず、激しく抽送をしている……
		RETURN 1
	;屈服3
	ELSEIF MARK:不埒刻印 == 3
		PRINTFORML
		PRINTFORMW 「……ぁ……もう、そんなに勃たせて……今日は、お尻でしましょ……？」
		PRINTFORMW 霊夢は%CALLNAME:MASTER%の正面に歩み寄ってきて、妖しい笑みを浮かべながらそう言った。
		PRINTFORMW %CALLNAME:MASTER%は要望通り、霊夢の片脚を持ち上げて菊穴に亀頭をあてがった。
		PRINTFORMW 「んっ……もう、そんなに焦らさないで……んんっ！　……っ、はぁっ……挿入って、きたぁ%UNICODE(0x2665)%……」
		PRINTFORMW %CALLNAME:MASTER%がずん、と勢いよく挿入すると、霊夢は恍惚の表情で喘いだ。
		PRINTFORMW 霊夢の菊穴は一物をきゅう、と締め付けてきて、丁度良い快感を%CALLNAME:MASTER%に与えてきている……
		RETURN 1
	;それ以外
	ELSE
		PRINTFORML
		PRINTFORMW 「……そんなに、大きくして……」
		PRINTFORMW 霊夢は%CALLNAME:MASTER%のそそり立つ一物を見てため息をついている。
		PRINTFORMW 今日は後ろでしたいんだが、どうかな？
		PRINTFORMW 「……別に良いけど……そんなに後ろでするのが良いのかしらね……？」
		PRINTFORMW 霊夢は何かと呟きながら%CALLNAME:MASTER%の正面に歩み寄ってきた。
		PRINTFORMW %CALLNAME:MASTER%は霊夢の片脚を持ち上げ、腸内へ陰茎を挿入した。
		PRINTFORMW きゅう、と締め付けてくる菊穴の感触を楽しみながら、%CALLNAME:MASTER%は抽送を開始した。
		RETURN 1
	ENDIF
ENDIF
;-------------------------------------------------
;正常位
;記入チェック（=0, 非表示、1, 表示）
LOCAL = 1
;-------------------------------------------------
IF LOCAL
	;-------------------------------------------------
	;小分岐
	;-------------------------------------------------
	;初めて
	;記入チェック
	LOCAL:1 = 1
	;-------------------------------------------------
	IF LOCAL:1 && FIRSTTIME(SELECTCOM)
		IF FLAG:時間停止
			PRINTFORML
			PRINTFORMW %CALLNAME:MASTER%は霊夢を床に倒し、上に覆いかぶさった。
			PRINTFORMW そして亀頭を菊穴にあてがい、ぬちゅ、と進入させる。
				SIF TCVAR:Ａ破瓜
					PRINTFORMW 霊夢の菊穴は拒むようにぎゅっ、と%CALLNAME:MASTER%の陰茎を締め付けている。
			PRINTFORMW %CALLNAME:MASTER%は時を止めたまま、激しく抽送を開始した。
			RETURN 1
		;アナル処女
		ELSEIF TCVAR:Ａ破瓜
			PRINTFORML
			PRINTFORMW 霊夢、後ろの穴でしたいんだが。
			PRINTFORMW 「……へ？　後ろ……お尻？」
			PRINTFORMW 応。
			PRINTFORMW 「……%CALLNAME:MASTER%って、なかなか変なところもあるわよね……」
			PRINTFORMW そう呟きながら、霊夢は床に座った。
			PRINTFORMW %CALLNAME:MASTER%は霊夢を床に押し倒し、上に覆いかぶさる。
			PRINTFORMW 「……後ろの穴、かぁ……どんな感じかしらね」
			PRINTFORMW 霊夢は未曽有の体験への興味に顔を染めている。
			PRINTFORMW %CALLNAME:MASTER%は菊穴に亀頭をあてがって、ぬちゅ、と挿入した。
			PRINTFORMW 「が、くっ……い、痛いわね……」
			PRINTFORMW 霊夢は痛みを訴えた。
				SIF TALENT:処女 != 1
					PRINTFORMW まあ、処女を喪失したほどではなさそうだが。
			PRINTFORMW 霊夢の菊穴は異物を押し出すようにぎゅう、と%CALLNAME:MASTER%の陰茎を締め付けてくる。
			PRINTFORMW %CALLNAME:MASTER%はその感触を味わいながら、抽送を開始した。
			RETURN 1
		ELSE
			PRINTFORML
			PRINTFORMW 霊夢、今日は後ろの穴でしたいんだが……
			PRINTFORMW 「……良いわよ。で、どういう体勢でするのかしら？」
			PRINTFORMW 何、安心しなさい。今日は普通の体勢でするつもりだから。
			PRINTFORMW 「普通、ねぇ……じゃあ、私は座れば良いのね？」
			PRINTFORMW うむ。
			PRINTFORMW 霊夢は床に座って、%CALLNAME:MASTER%が動くのを待っている。
			PRINTFORMW %CALLNAME:MASTER%は霊夢を押し倒し、上に覆いかぶさった。
			PRINTFORMW 「……なんか、さっきより大きくなってない……？」
			PRINTFORMW 霊夢は視線を下に落として呟く。
			PRINTFORMW %CALLNAME:MASTER%が菊穴に亀頭を触れさせると、霊夢は小さな悲鳴を上げた。
			PRINTFORMW 「ん……良いわ、挿れて……」
			PRINTFORMW %CALLNAME:MASTER%は霊夢の腸内へと陰茎を挿入する。
			PRINTFORMW 深くまで挿入してから、%CALLNAME:MASTER%はゆっくりと抽送を開始した。
			RETURN 1
		ENDIF
	ENDIF
	;-------------------------------------------------
	;挿入継続
	;記入チェック
	LOCAL:1 = 1
	;-------------------------------------------------
	IF LOCAL:1 && TCVAR:101 == PLAYER
		IF FLAG:時間停止
			PRINTFORML
			PRINTFORMW %CALLNAME:MASTER%は霊夢に挿入したまま体勢を変えた。
			PRINTFORMW 霊夢の上に覆いかぶさり、より深くまで挿入する。
			PRINTFORMW %CALLNAME:MASTER%は再度抽送を始めた。
			RETURN 1
		;前回射精した
		ELSEIF TCVAR:104
			PRINTFORML
			PRINTFORMW 「んっ……どろっ、て、してる……」
			PRINTFORMW 霊夢は腸内の精液の感触に顔をしかめている。
			PRINTFORMW %CALLNAME:MASTER%は先ほど射精したにもかかわらず、体勢を変えた。
			PRINTFORMW 霊夢の上に覆いかぶさり、より深くまで挿入することができる体勢になる。
			PRINTFORMW それから%CALLNAME:MASTER%は抽送を再開した。
			RETURN 1
		ELSE
			PRINTFORML
			PRINTFORMW 「はっ、んっ、ふぅっ……あ、れ？　止める、の……？」
			PRINTFORMW 霊夢は息を荒くしながら聞いてくる。
			PRINTFORMW %CALLNAME:MASTER%は無言で体勢を変え、霊夢の上に覆いかぶさった。
			PRINTFORMW そしてより深くまで挿入し、再度抽送を開始した。
			RETURN 1
		ENDIF
	ENDIF
	;基本セット
	;時姦中
	IF FLAG:70
		PRINTFORML
		PRINTFORMW %CALLNAME:MASTER%は霊夢を床に寝させて、上に覆いかぶさった。
		PRINTFORMW %CALLNAME:MASTER%は膣口ではなく、菊穴に亀頭をあてがう。
		PRINTFORMW そして%CALLNAME:MASTER%は、思いっきり奥まで陰茎を進入させた。
		PRINTFORMW %CALLNAME:MASTER%は小気味良い音を立てながら腰を霊夢の尻に打ち付けている……
		RETURN 1
	;A絶頂、挿れただけでイっちゃう淫乱な霊夢さん（二回目）
	ELSEIF NOWEX:Ａ絶頂
		PRINTFORML
		PRINTFORMW 霊夢、今回は後ろでしようか。
		PRINTFORMW 「ん……良い、わよ……」
		PRINTFORMW 霊夢は自ら床に寝転がり、脚を大きく開いた。
		PRINTFORMW %CALLNAME:MASTER%はその淫靡な姿を見て興奮し、霊夢の上にのしかかった。
		PRINTFORMW 「んっ……早く、挿れて……」
		PRINTFORMW 霊夢は待ちきれないといった感じで、%CALLNAME:MASTER%に懇願してきた。
		PRINTFORMW %CALLNAME:MASTER%はその要望通り、ずん、と、最奥まで勢いよく貫いた。
		PRINTFORMW 「ぁ―――――っ%UNICODE(0x2665)%、んっ、すぐにっ、イっちゃったぁっ、はぁっ、は、んっ、ふっ」
		PRINTFORMW %CALLNAME:MASTER%が最奥まで貫いた瞬間、霊夢は体を反らせて絶頂に達した。
		PRINTFORMW %CALLNAME:MASTER%は絶頂によってぎゅう、と強く締め付けてくる菊穴の感触を味わいながら、激しい抽送を開始した。
		RETURN 1
	;屈服3
	ELSEIF MARK:不埒刻印 == 3
		PRINTFORML
		PRINTFORMW 「あ……したいの？　良いけど……今回は、お尻でしない……？」
		PRINTFORMW 霊夢はおずおずと提案してきた。
		PRINTFORMW %CALLNAME:MASTER%は快く承諾した。
		PRINTFORMW %CALLNAME:MASTER%は少し亀頭を菊穴に擦り付けてから、腸内へと肉棒を進入させていった。
		PRINTFORMW 「んっ、はぁぁっ%UNICODE(0x2665)%……んっ、はぁっ、ふぅっ、はっ、んんっ」
		PRINTFORMW 陰茎を根元まで挿入し、%CALLNAME:MASTER%はゆっくりと抽送を始めた。
		RETURN 1
	;それ以外
	ELSE
		PRINTFORML
		PRINTFORMW 霊夢、今回は後ろの穴でしたいんだが。
		PRINTFORMW 「……そんなにそそり立たせて……良いわよ」
		PRINTFORMW 霊夢は%CALLNAME:MASTER%の直立する一物を見て、ため息を吐いた。
		PRINTFORMW %CALLNAME:MASTER%は霊夢を座らせ、押し倒した。
		PRINTFORMW そしてそそり立つ一物を霊夢の腸内へ、ずぶ、と進入させた。
		PRINTFORMW 「ん、くぅっ……」
		PRINTFORMW 霊夢は腸内の異物感にうめき声を上げている。
		PRINTFORMW %CALLNAME:MASTER%は霊夢の腸内にまだ残っている硬さを堪能しながら腰を動かした。
		RETURN 1
	ENDIF
ENDIF
RETURN 1

;==================================================
; 抽出完了
;==================================================
