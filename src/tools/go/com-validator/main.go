package main

import (
	"fmt"
	"os"
)

const version = "0.1.0"

func main() {
	// Parse command line arguments
	if len(os.Args) < 2 {
		printUsage()
		os.Exit(1)
	}

	yamlPath := os.Args[1]

	// Check for special flags
	if yamlPath == "--version" || yamlPath == "-v" {
		fmt.Printf("com-validator version %s\n", version)
		os.Exit(0)
	}

	if yamlPath == "--help" || yamlPath == "-h" {
		printUsage()
		os.Exit(0)
	}

	if yamlPath == "--examples" {
		PrintUsageExamples()
		os.Exit(0)
	}

	// Verify file exists
	if _, err := os.Stat(yamlPath); os.IsNotExist(err) {
		fmt.Fprintf(os.Stderr, "Error: File not found: %s\n", yamlPath)
		os.Exit(1)
	}

	// Create validator with embedded schema
	validator, err := NewValidator()
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to initialize validator: %v\n", err)
		os.Exit(1)
	}

	// Validate the YAML file
	fmt.Printf("Validating: %s\n", yamlPath)
	if err := validator.ValidateFile(yamlPath); err != nil {
		// Check if it's a ValidationError and format it
		if valErr, ok := err.(*ValidationError); ok {
			fmt.Fprint(os.Stderr, valErr.FormatError())
		} else {
			fmt.Fprintf(os.Stderr, "Validation failed: %v\n", err)
		}
		os.Exit(1)
	}

	fmt.Println("✓ Validation successful! / 検証成功!")
	os.Exit(0)
}

func printUsage() {
	fmt.Println("COM YAML Validator - コマンド (COM) YAMLファイル検証ツール")
	fmt.Println()
	fmt.Println("Usage:")
	fmt.Println("  com-validator <yaml-file>")
	fmt.Println("  com-validator --version")
	fmt.Println("  com-validator --help")
	fmt.Println("  com-validator --examples")
	fmt.Println()
	fmt.Println("Options:")
	fmt.Println("  <yaml-file>  Path to YAML file to validate")
	fmt.Println("  --version    Show version information")
	fmt.Println("  --help       Show this help message")
	fmt.Println("  --examples   Show usage examples (凡例)")
	fmt.Println()
	fmt.Println("Example:")
	fmt.Println("  com-validator Game/data/coms/training/touch/caress.yaml")
}
