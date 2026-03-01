package main

import (
	"strings"
	"testing"
)

func TestValidationErrorFormat(t *testing.T) {
	err := &ValidationError{
		Line:        10,
		Column:      5,
		Field:       "character_id",
		MessageType: "type_mismatch",
		Details:     "Expected number, got string",
	}

	output := err.FormatError()

	// Verify Japanese error message is present
	if !strings.Contains(output, "エラー") {
		t.Errorf("Expected Japanese error message (エラー) in output")
	}

	// Verify line and column are shown
	if !strings.Contains(output, "10行") || !strings.Contains(output, "5列") {
		t.Errorf("Expected Japanese line/column format in output")
	}

	// Verify English fallback is present
	if !strings.Contains(output, "Line 10, Column 5") {
		t.Errorf("Expected English line/column format in output")
	}
}

func TestSuggestionForTypo(t *testing.T) {
	testCases := []struct {
		input    string
		expected string
	}{
		{"charcter", "character"},
		{"charcter_id", "character_id"},
		{"discription", "description"},
		{"talant", "talent"},
		{"dialoge", "dialogue"},
		{"affectoin", "affection"},
		{"", ""}, // Empty input should return empty suggestion
		{"correct_field", ""}, // Correct spelling should return no suggestion
	}

	for _, tc := range testCases {
		result := getSuggestion(tc.input)
		if result != tc.expected {
			t.Errorf("getSuggestion(%q) = %q, expected %q", tc.input, result, tc.expected)
		}
	}
}

func TestErrorMessageLocalization(t *testing.T) {
	testCases := []struct {
		messageType string
		expectJP    string
		expectEN    string
	}{
		{
			messageType: "type_mismatch",
			expectJP:    "型が一致しません",
			expectEN:    "Type mismatch",
		},
		{
			messageType: "required_field",
			expectJP:    "必須フィールドが不足",
			expectEN:    "Required field is missing",
		},
		{
			messageType: "format_error",
			expectJP:    "フォーマットエラー",
			expectEN:    "Format error",
		},
		{
			messageType: "unknown_error_type",
			expectJP:    "不明なエラー",
			expectEN:    "unknown error",
		},
	}

	for _, tc := range testCases {
		msg := getErrorMessage(tc.messageType)
		if !strings.Contains(msg.Japanese, tc.expectJP) {
			t.Errorf("Expected Japanese message to contain %q, got %q", tc.expectJP, msg.Japanese)
		}
		if !strings.Contains(msg.English, tc.expectEN) {
			t.Errorf("Expected English message to contain %q, got %q", tc.expectEN, msg.English)
		}
	}
}

func TestPrintUsageExamples(t *testing.T) {
	// This is a simple smoke test to ensure the function doesn't panic
	// Actual output verification would require capturing stdout
	defer func() {
		if r := recover(); r != nil {
			t.Errorf("PrintUsageExamples() panicked: %v", r)
		}
	}()

	PrintUsageExamples()
}
