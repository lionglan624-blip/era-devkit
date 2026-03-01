package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"

	"github.com/xeipuuv/gojsonschema"
	"gopkg.in/yaml.v3"
)

func TestMapErrorType(t *testing.T) {
	testCases := []struct {
		errorType string
		expected  string
	}{
		{"invalid_type", "type_mismatch"},
		{"required", "required_field"},
		{"additional_property_not_allowed", "unknown_property"},
		{"enum", "invalid_enum"},
		{"number_not", "invalid_value"},
		{"string_gte", "invalid_value"},
		{"string_lte", "invalid_value"},
		{"array_min_items", "invalid_value"},
		{"array_max_items", "invalid_value"},
		{"unknown_error_type", "schema_validation"}, // Default case
		{"", "schema_validation"},                   // Empty input
	}

	for _, tc := range testCases {
		result := mapErrorType(tc.errorType)
		if result != tc.expected {
			t.Errorf("mapErrorType(%q) = %q, expected %q", tc.errorType, result, tc.expected)
		}
	}
}

func TestBuildErrorDetails(t *testing.T) {
	testCases := []struct {
		name         string
		errorType    string
		description  string
		details      gojsonschema.ErrorDetails
		expectSubstr string
	}{
		{
			name:         "invalid_type with expected field",
			errorType:    "invalid_type",
			description:  "Invalid type",
			details:      gojsonschema.ErrorDetails{"expected": "string"},
			expectSubstr: "期待される型",
		},
		{
			name:         "enum with allowed values",
			errorType:    "enum",
			description:  "Must be one of the allowed values",
			details:      gojsonschema.ErrorDetails{"allowed": []interface{}{"value1", "value2"}},
			expectSubstr: "許可された値",
		},
		{
			name:         "basic error without details",
			errorType:    "required",
			description:  "Field is required",
			details:      gojsonschema.ErrorDetails{},
			expectSubstr: "Field is required",
		},
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			// Create a mock ResultError
			mockError := &mockResultError{
				errorType:   tc.errorType,
				description: tc.description,
				details:     tc.details,
			}

			result := buildErrorDetails(mockError)
			if !strings.Contains(result, tc.expectSubstr) {
				t.Errorf("buildErrorDetails() = %q, expected to contain %q", result, tc.expectSubstr)
			}
		})
	}
}

func TestFindFieldLocation(t *testing.T) {
	yamlContent := `character_id: 1
description: Test description
conditions:
  - type: talent
    value: 100
`

	var yamlNode yaml.Node
	if err := yaml.Unmarshal([]byte(yamlContent), &yamlNode); err != nil {
		t.Fatalf("Failed to parse YAML: %v", err)
	}

	testCases := []struct {
		field        string
		expectedLine int
		minLine      int // Expected line should be >= this
	}{
		{"character_id", 1, 1},
		{"description", 2, 2},
		{"conditions", 3, 3},
		{"conditions.0.type", 4, 3},     // Array element, should find "type"
		{"conditions.0.value", 5, 3},    // Array element, should find "value"
		{"", 1, 1},                      // Empty field returns default
		{"nonexistent_field", 1, 1},     // Non-existent field returns default
	}

	for _, tc := range testCases {
		line, _ := findFieldLocation(tc.field, &yamlNode, yamlContent)
		if line < tc.minLine {
			t.Errorf("findFieldLocation(%q) returned line %d, expected >= %d", tc.field, line, tc.minLine)
		}
	}
}

func TestNewValidator(t *testing.T) {
	validator, err := NewValidator()
	if err != nil {
		t.Fatalf("NewValidator() failed: %v", err)
	}

	if validator == nil {
		t.Error("NewValidator() returned nil validator")
	}

	if validator.schema == nil {
		t.Error("NewValidator() returned validator with nil schema")
	}
}

func TestValidateFile_ValidYAML(t *testing.T) {
	// Create a temporary valid YAML file
	validYAML := `id: 1
name: Test Command
category: Training/Touch
description: Test command
conditions:
  - type: talent
    target: TALENT_LOVE
    operator: gte
    value: 100
effects:
  - type: exp
    target: EXP_TOUCH
    value: 5
`

	tmpDir := t.TempDir()
	tmpFile := filepath.Join(tmpDir, "test_valid.yaml")
	if err := os.WriteFile(tmpFile, []byte(validYAML), 0644); err != nil {
		t.Fatalf("Failed to create temp file: %v", err)
	}

	validator, err := NewValidator()
	if err != nil {
		t.Fatalf("NewValidator() failed: %v", err)
	}

	err = validator.ValidateFile(tmpFile)
	if err != nil {
		t.Errorf("ValidateFile() with valid YAML returned error: %v", err)
	}
}

func TestValidateFile_InvalidYAML(t *testing.T) {
	testCases := []struct {
		name        string
		yamlContent string
		expectError string
	}{
		{
			name: "missing required field",
			yamlContent: `name: Test
category: Training/Touch
`,
			expectError: "required_field",
		},
		{
			name: "invalid type",
			yamlContent: `id: "not_a_number"
name: Test
category: Training/Touch
`,
			expectError: "type_mismatch",
		},
		{
			name:        "malformed YAML",
			yamlContent: `invalid: yaml: syntax: error:`,
			expectError: "parse_error",
		},
		{
			name: "unknown property",
			yamlContent: `id: 1
name: Test
category: Training/Touch
unknown_field: invalid
`,
			expectError: "unknown_property",
		},
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			tmpDir := t.TempDir()
			tmpFile := filepath.Join(tmpDir, "test_invalid.yaml")
			if err := os.WriteFile(tmpFile, []byte(tc.yamlContent), 0644); err != nil {
				t.Fatalf("Failed to create temp file: %v", err)
			}

			validator, err := NewValidator()
			if err != nil {
				t.Fatalf("NewValidator() failed: %v", err)
			}

			err = validator.ValidateFile(tmpFile)
			if err == nil {
				t.Errorf("ValidateFile() expected error for %s, but got nil", tc.name)
				return
			}

			validationErr, ok := err.(*ValidationError)
			if !ok {
				t.Errorf("ValidateFile() returned non-ValidationError: %T", err)
				return
			}

			if validationErr.MessageType != tc.expectError {
				t.Errorf("ValidateFile() MessageType = %q, expected %q", validationErr.MessageType, tc.expectError)
			}
		})
	}
}

func TestValidateFile_FileNotFound(t *testing.T) {
	validator, err := NewValidator()
	if err != nil {
		t.Fatalf("NewValidator() failed: %v", err)
	}

	err = validator.ValidateFile("/nonexistent/path/to/file.yaml")
	if err == nil {
		t.Error("ValidateFile() with nonexistent file should return error")
		return
	}

	validationErr, ok := err.(*ValidationError)
	if !ok {
		t.Errorf("ValidateFile() returned non-ValidationError: %T", err)
		return
	}

	if validationErr.MessageType != "file_not_found" {
		t.Errorf("ValidateFile() MessageType = %q, expected %q", validationErr.MessageType, "file_not_found")
	}
}

// mockResultError implements gojsonschema.ResultError for testing
type mockResultError struct {
	errorType   string
	description string
	details     gojsonschema.ErrorDetails
}

func (m *mockResultError) Type() string                                { return m.errorType }
func (m *mockResultError) Field() string                               { return "" }
func (m *mockResultError) SetType(string)                              {}
func (m *mockResultError) Context() *gojsonschema.JsonContext          { return nil }
func (m *mockResultError) SetContext(*gojsonschema.JsonContext)        {}
func (m *mockResultError) Description() string                         { return m.description }
func (m *mockResultError) SetDescription(string)                       {}
func (m *mockResultError) Value() interface{}                          { return nil }
func (m *mockResultError) SetValue(interface{})                        {}
func (m *mockResultError) Details() gojsonschema.ErrorDetails          { return m.details }
func (m *mockResultError) SetDetails(gojsonschema.ErrorDetails)        {}
func (m *mockResultError) DescriptionFormat() string                   { return "" }
func (m *mockResultError) SetDescriptionFormat(string)                 {}
func (m *mockResultError) String() string                              { return m.description }
