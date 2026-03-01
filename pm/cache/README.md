# agents/cache/

eraTW リファレンス抽出キャッシュ。gitignored (再生成可能)。

## 再生成方法

`Skill(eratw-reader)` で COM 番号を指定して再抽出:

```
; Claude Code 内で実行
Skill(eratw-reader) COM_1
```

全キャッシュの一括再生成はスキルに未実装。個別抽出で対応。

## ソース

`ERATW_PATH` 環境変数 (default: `C:\Users\siihe\OneDrive\同人ゲーム\eraTW4.920`)

## ファイル命名規則

- `eratw-COM_N.txt` — COM N番のリファレンス抽出
- `eratw-COM48.txt` — COM48 (番号のみ、アンダースコアなし) も存在
