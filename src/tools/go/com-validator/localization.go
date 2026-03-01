package main

import (
	"fmt"
	"strings"
)

// ErrorMessage represents a localized error message
type ErrorMessage struct {
	Japanese string
	English  string
}

// ValidationError represents a validation error with location information
type ValidationError struct {
	Line        int
	Column      int
	Field       string
	MessageType string
	Details     string
}

// Error implements the error interface
func (e *ValidationError) Error() string {
	return e.FormatError()
}

// FormatError formats a validation error with Japanese and English messages
func (e *ValidationError) FormatError() string {
	var builder strings.Builder

	// エラーヘッダー (Error header)
	builder.WriteString("==================== 検証エラー / Validation Error ====================\n")

	// 位置情報 (Location)
	if e.Line > 0 {
		builder.WriteString(fmt.Sprintf("位置 / Location: %d行 %d列 (Line %d, Column %d)\n",
			e.Line, e.Column, e.Line, e.Column))
	}

	// フィールド情報 (Field information)
	if e.Field != "" {
		builder.WriteString(fmt.Sprintf("フィールド / Field: %s\n", e.Field))
	}

	// エラーメッセージ (Error message)
	msg := getErrorMessage(e.MessageType)
	builder.WriteString(fmt.Sprintf("\nエラー / Error:\n  [JP] %s\n  [EN] %s\n",
		msg.Japanese, msg.English))

	// 詳細情報 (Details)
	if e.Details != "" {
		builder.WriteString(fmt.Sprintf("\n詳細 / Details: %s\n", e.Details))
	}

	// もしかして (Did you mean) suggestions
	if suggestion := getSuggestion(e.Field); suggestion != "" {
		builder.WriteString(fmt.Sprintf("\nもしかして / Did you mean: %s\n", suggestion))
	}

	builder.WriteString("======================================================================\n")

	return builder.String()
}

// getErrorMessage returns the localized error message for a given type
func getErrorMessage(messageType string) ErrorMessage {
	messages := map[string]ErrorMessage{
		"type_mismatch": {
			Japanese: "型が一致しません。期待される型と異なる値が指定されています。",
			English:  "Type mismatch. The value does not match the expected type.",
		},
		"required_field": {
			Japanese: "必須フィールドが不足しています。",
			English:  "Required field is missing.",
		},
		"invalid_value": {
			Japanese: "無効な値が指定されています。",
			English:  "Invalid value specified.",
		},
		"format_error": {
			Japanese: "フォーマットエラー。YAML形式が正しくありません。",
			English:  "Format error. Invalid YAML format.",
		},
		"schema_validation": {
			Japanese: "スキーマ検証エラー。スキーマの制約に違反しています。",
			English:  "Schema validation error. The data violates schema constraints.",
		},
		"unknown_property": {
			Japanese: "未知のプロパティが含まれています。",
			English:  "Unknown property detected.",
		},
		"invalid_enum": {
			Japanese: "列挙型の値が無効です。許可された値のいずれかを指定してください。",
			English:  "Invalid enum value. Please specify one of the allowed values.",
		},
		"file_not_found": {
			Japanese: "ファイルが見つかりません。",
			English:  "File not found.",
		},
		"parse_error": {
			Japanese: "YAMLの解析に失敗しました。",
			English:  "Failed to parse YAML.",
		},
	}

	if msg, ok := messages[messageType]; ok {
		return msg
	}

	// デフォルトメッセージ (Default message)
	return ErrorMessage{
		Japanese: "不明なエラーが発生しました。",
		English:  "An unknown error occurred.",
	}
}

// getSuggestion returns a suggestion for a potentially misspelled field name
func getSuggestion(field string) string {
	if field == "" {
		return ""
	}

	// 一般的なタイポマッピング (Common typo mapping)
	typoMap := map[string]string{
		"charcter":     "character",
		"charactor":    "character",
		"discription":  "description",
		"descripion":   "description",
		"conditon":     "condition",
		"condiiton":    "condition",
		"experiance":   "experience",
		"expereince":   "experience",
		"requred":      "required",
		"requried":     "required",
		"valiue":       "value",
		"valeu":        "value",
		"expresion":    "expression",
		"expresssion":  "expression",
		"positon":      "position",
		"postion":      "position",
		"targett":      "target",
		"taget":        "target",
		"mesage":       "message",
		"messege":      "message",
		"seperator":    "separator",
		"seperater":    "separator",
		"lenght":       "length",
		"lenth":        "length",
		"indx":         "index",
		"indexx":       "index",
		"reponse":      "response",
		"responce":     "response",
		"reciver":      "receiver",
		"reciever":     "receiver",
		"performace":   "performance",
		"performence":  "performance",
		"occured":      "occurred",
		"occurd":       "occurred",
	}

	// フィールド名を小文字に変換してチェック (Check with lowercase)
	lowerField := strings.ToLower(field)
	if suggestion, ok := typoMap[lowerField]; ok {
		return suggestion
	}

	// COM固有のフィールド名タイポ (COM-specific field typos)
	comTypoMap := map[string]string{
		"charcter_id":        "character_id",
		"charactor_id":       "character_id",
		"discription":        "description",
		"conditons":          "conditions",
		"experiance_gain":    "experience_gain",
		"requred_talent":     "required_talent",
		"dialoge":            "dialogue",
		"dialouge":           "dialogue",
		"affectoin":          "affection",
		"afection":           "affection",
		"favoriability":      "favorability",
		"favorablity":        "favorability",
		"intimicy":           "intimacy",
		"intimacy_value":     "intimacy",
		"talant":             "talent",
		"tallent":            "talent",
		"paramater":          "parameter",
		"parametar":          "parameter",
		"variabl":            "variable",
		"variabel":           "variable",
	}

	if suggestion, ok := comTypoMap[lowerField]; ok {
		return suggestion
	}

	return ""
}

// PrintUsageExamples prints usage examples in Japanese and English
func PrintUsageExamples() {
	fmt.Println("\n==================== 凡例 / Usage Examples ====================")
	fmt.Println()
	fmt.Println("基本的な使用方法 / Basic Usage:")
	fmt.Println("  com-validator <YAMLファイルのパス>")
	fmt.Println("  com-validator <path-to-yaml-file>")
	fmt.Println()
	fmt.Println("例 / Examples:")
	fmt.Println("  1. 単一ファイルの検証 / Validate a single file:")
	fmt.Println("     com-validator Game/data/coms/training/touch/caress.yaml")
	fmt.Println()
	fmt.Println("  2. ドラッグ&ドロップ / Drag and drop:")
	fmt.Println("     YAMLファイルをvalidate.batにドラッグ&ドロップしてください")
	fmt.Println("     Drag and drop a YAML file onto validate.bat")
	fmt.Println()
	fmt.Println("  3. バージョン確認 / Check version:")
	fmt.Println("     com-validator --version")
	fmt.Println()
	fmt.Println("  4. ヘルプ表示 / Show help:")
	fmt.Println("     com-validator --help")
	fmt.Println()
	fmt.Println("エラーメッセージの見方 / How to read error messages:")
	fmt.Println("  - 位置 (Location): エラーが発生した行と列")
	fmt.Println("                     Line and column where the error occurred")
	fmt.Println("  - フィールド (Field): 問題のあるYAMLフィールド名")
	fmt.Println("                        Name of the problematic YAML field")
	fmt.Println("  - エラー (Error): エラーの説明 (日本語と英語)")
	fmt.Println("                    Error description (Japanese and English)")
	fmt.Println("  - もしかして (Did you mean): タイポの可能性がある場合の修正候補")
	fmt.Println("                               Correction suggestion for possible typos")
	fmt.Println()
	fmt.Println("よくあるエラーと対処法 / Common errors and solutions:")
	fmt.Println()
	fmt.Println("  1. 型エラー / Type error:")
	fmt.Println("     問題: 数値が必要な場所に文字列を指定")
	fmt.Println("     Problem: String specified where number is required")
	fmt.Println("     解決: 値を正しい型に修正")
	fmt.Println("     Solution: Correct the value to the right type")
	fmt.Println()
	fmt.Println("  2. 必須フィールド不足 / Missing required field:")
	fmt.Println("     問題: スキーマで必須とされるフィールドがない")
	fmt.Println("     Problem: Required field according to schema is missing")
	fmt.Println("     解決: 必須フィールドを追加")
	fmt.Println("     Solution: Add the required field")
	fmt.Println()
	fmt.Println("  3. フォーマットエラー / Format error:")
	fmt.Println("     問題: YAML構文が間違っている (インデント、コロンなど)")
	fmt.Println("     Problem: Incorrect YAML syntax (indentation, colons, etc.)")
	fmt.Println("     解決: YAML構文を修正")
	fmt.Println("     Solution: Fix YAML syntax")
	fmt.Println()
	fmt.Println("===============================================================")
}
