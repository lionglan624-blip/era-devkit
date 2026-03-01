package main

import (
	_ "embed"
	"encoding/json"
	"fmt"
	"os"
	"strings"

	"github.com/xeipuuv/gojsonschema"
	"gopkg.in/yaml.v3"
)

//go:embed schemas/com.schema.json
var embeddedSchema string

// Validator handles YAML validation against JSON schema
type Validator struct {
	schema *gojsonschema.Schema
}

// NewValidator creates a new validator with the embedded schema
func NewValidator() (*Validator, error) {
	schemaLoader := gojsonschema.NewStringLoader(embeddedSchema)
	schema, err := gojsonschema.NewSchema(schemaLoader)
	if err != nil {
		return nil, fmt.Errorf("failed to load schema: %w", err)
	}

	return &Validator{schema: schema}, nil
}

// ValidateFile validates a YAML file against the COM schema
func (v *Validator) ValidateFile(filePath string) error {
	// Read YAML file
	yamlData, err := os.ReadFile(filePath)
	if err != nil {
		return &ValidationError{
			MessageType: "file_not_found",
			Details:     err.Error(),
		}
	}

	// Parse YAML to extract line number information
	var yamlNode yaml.Node
	if err := yaml.Unmarshal(yamlData, &yamlNode); err != nil {
		return &ValidationError{
			Line:        extractLineNumber(err),
			Column:      extractColumnNumber(err),
			MessageType: "parse_error",
			Details:     err.Error(),
		}
	}

	// Convert YAML to JSON for schema validation
	var jsonData interface{}
	if err := yaml.Unmarshal(yamlData, &jsonData); err != nil {
		return &ValidationError{
			MessageType: "parse_error",
			Details:     err.Error(),
		}
	}

	// Convert to JSON structure
	jsonBytes, err := json.Marshal(jsonData)
	if err != nil {
		return &ValidationError{
			MessageType: "format_error",
			Details:     err.Error(),
		}
	}

	// Validate against schema
	documentLoader := gojsonschema.NewBytesLoader(jsonBytes)
	result, err := v.schema.Validate(documentLoader)
	if err != nil {
		return &ValidationError{
			MessageType: "schema_validation",
			Details:     err.Error(),
		}
	}

	// Check validation result
	if !result.Valid() {
		// Convert validation errors to Japanese format
		return v.formatValidationErrors(result.Errors(), &yamlNode, string(yamlData))
	}

	return nil
}

// formatValidationErrors converts gojsonschema errors to our localized format
func (v *Validator) formatValidationErrors(errors []gojsonschema.ResultError, yamlNode *yaml.Node, yamlContent string) error {
	if len(errors) == 0 {
		return nil
	}

	// For now, return the first error with localization
	firstError := errors[0]

	// Extract field path
	field := firstError.Field()

	// Determine error type
	messageType := mapErrorType(firstError.Type())

	// Try to find line/column information from YAML node
	line, column := findFieldLocation(field, yamlNode, yamlContent)

	// Build details
	details := buildErrorDetails(firstError)

	return &ValidationError{
		Line:        line,
		Column:      column,
		Field:       field,
		MessageType: messageType,
		Details:     details,
	}
}

// mapErrorType maps gojsonschema error types to our localized message types
func mapErrorType(errorType string) string {
	typeMap := map[string]string{
		"invalid_type":       "type_mismatch",
		"required":           "required_field",
		"additional_property_not_allowed": "unknown_property",
		"enum":               "invalid_enum",
		"number_not":         "invalid_value",
		"string_gte":         "invalid_value",
		"string_lte":         "invalid_value",
		"array_min_items":    "invalid_value",
		"array_max_items":    "invalid_value",
	}

	if mapped, ok := typeMap[errorType]; ok {
		return mapped
	}

	return "schema_validation"
}

// buildErrorDetails builds detailed error information
func buildErrorDetails(err gojsonschema.ResultError) string {
	var details strings.Builder

	details.WriteString(err.Description())

	// Add additional context based on error type
	if err.Type() == "invalid_type" {
		if expected, ok := err.Details()["expected"].(string); ok {
			details.WriteString(fmt.Sprintf(" (期待される型 / Expected type: %s)", expected))
		}
	}

	if err.Type() == "enum" {
		if allowed, ok := err.Details()["allowed"].([]interface{}); ok {
			details.WriteString(fmt.Sprintf(" (許可された値 / Allowed values: %v)", allowed))
		}
	}

	return details.String()
}

// findFieldLocation attempts to find the line and column of a field in the YAML
func findFieldLocation(field string, yamlNode *yaml.Node, yamlContent string) (int, int) {
	// Default to line 1 if we can't find it
	line := 1
	column := 1

	// Simple heuristic: search for the field name in YAML content
	// This is a basic implementation - could be enhanced with proper AST traversal
	if field != "" {
		// Extract the last component of the field path (e.g., "data.items.0.name" -> "name")
		parts := strings.Split(field, ".")
		fieldName := parts[len(parts)-1]

		// Skip array indices
		if _, err := fmt.Sscanf(fieldName, "%d", &line); err == nil {
			// It's an array index, use previous component
			if len(parts) > 1 {
				fieldName = parts[len(parts)-2]
			}
		}

		// Search for field in YAML content
		lines := strings.Split(yamlContent, "\n")
		for i, lineContent := range lines {
			if strings.Contains(lineContent, fieldName+":") {
				line = i + 1
				column = strings.Index(lineContent, fieldName) + 1
				break
			}
		}
	}

	return line, column
}

// extractLineNumber extracts line number from YAML parse error
func extractLineNumber(err error) int {
	if err == nil {
		return 0
	}

	// Try to extract line number from error message
	errStr := err.Error()
	var line int
	if _, err := fmt.Sscanf(errStr, "yaml: line %d:", &line); err == nil {
		return line
	}

	return 1
}

// extractColumnNumber extracts column number from YAML parse error
func extractColumnNumber(err error) int {
	if err == nil {
		return 0
	}

	// Try to extract column number from error message
	errStr := err.Error()
	var line, column int
	if _, err := fmt.Sscanf(errStr, "yaml: line %d: column %d:", &line, &column); err == nil {
		return column
	}

	return 1
}
