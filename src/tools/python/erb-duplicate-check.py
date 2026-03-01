#!/usr/bin/env python3
"""
ERB Duplicate Function Checker - Detects duplicate function definitions
Prevents stub function conflicts during kojo implementation (Feature 259)
"""

import re
import sys
import argparse
from pathlib import Path
from typing import Dict, List, Tuple
from collections import defaultdict


def find_erb_files(search_path: Path) -> List[Path]:
    """Find all .ERB files in the given path."""
    if search_path.is_file():
        return [search_path] if search_path.suffix.upper() == '.ERB' else []

    erb_files = []
    for pattern in ['**/*.ERB', '**/*.erb']:
        erb_files.extend(search_path.glob(pattern))
    return sorted(set(erb_files))


def extract_functions(erb_file: Path) -> Dict[str, List[Tuple[str, int]]]:
    """
    Extract function definitions from ERB file.

    Returns:
        Dict[function_name, List[(file_path, line_number)]]
    """
    functions = defaultdict(list)

    # Pattern: @FUNCTION_NAME or @FUNCTION_NAME(args)
    # Matches at start of line
    # Handles mixed-case and Unicode names (e.g., SexHara休憩中, KOJO_MESSAGE_COM)
    # Matches non-whitespace, non-parenthesis characters
    func_pattern = re.compile(r'^@([^\s(]+)(?:\([^)]*\))?', re.MULTILINE)

    try:
        with open(erb_file, 'r', encoding='utf-8-sig') as f:
            content = f.read()

        for match in func_pattern.finditer(content):
            func_name = match.group(1)
            # Calculate line number
            line_num = content[:match.start()].count('\n') + 1
            functions[func_name].append((str(erb_file), line_num))

    except Exception as e:
        print(f"Warning: Failed to read {erb_file}: {e}", file=sys.stderr)

    return functions


def extract_function_scope(file_content: str, function_name: str) -> str:
    """
    Extract parent function + _1 suffix function content.

    Example: function_name = "KOJO_MESSAGE_COM_K2_83"
        -> Extracts from @KOJO_MESSAGE_COM_K2_83 to end of @KOJO_MESSAGE_COM_K2_83_1

    Args:
        file_content: Full file content
        function_name: Function name (without @)

    Returns:
        Extracted function scope, or empty string if not found
    """
    # Patterns to match function boundaries
    # Must handle word boundaries: @FUNC followed by whitespace, (, or end of line
    parent_pattern = rf'^@{re.escape(function_name)}(?:\s|\(|$)'
    suffix_pattern = rf'^@{re.escape(function_name)}_1(?:\s|\(|$)'

    # Step 1: Find parent function start
    parent_match = re.search(parent_pattern, file_content, re.MULTILINE)
    if not parent_match:
        return ""

    # Step 2: Extract from parent start to end of _1 function (or EOF)
    search_start = parent_match.start()
    remaining = file_content[search_start:]

    # Find next @ that doesn't belong to our function or its _1 variant
    # This regex matches @ at line start, NOT followed by our function name
    # Build pattern: ^@ followed by negative lookahead for our function name
    # Note: Use chr(33) for '!' to avoid Python 3.13 parser treating \! as escape
    escaped_name = re.escape(function_name)
    end_pattern = '^@(?' + chr(33) + escaped_name + r'(_1)?(?:\s|\(|$))'
    end_match = re.search(end_pattern, remaining, re.MULTILINE)

    if end_match:
        return remaining[:end_match.start()]
    else:
        return remaining  # EOF


def check_stub_status(func_name: str, file_path: str) -> str:
    """
    Check if a function is a stub or implemented.

    Returns:
        "STUB" if function exists but has no content (empty PRINTFORMW or no text)
        "IMPLEMENTED" if function exists and has DATAFORM or PRINTFORMW with content
    """
    try:
        with open(file_path, 'r', encoding='utf-8-sig') as f:
            content = f.read()

        # Extract only the function scope (parent + _1)
        function_content = extract_function_scope(content, func_name)
        if not function_content:
            return "STUB"

        # Pattern 1: DATAFORM with content
        # DATAFORM\s+\S matches DATAFORM followed by whitespace then non-whitespace
        dataform_pattern = re.compile(r'DATAFORM\s+\S', re.MULTILINE)
        if dataform_pattern.search(function_content):
            return "IMPLEMENTED"

        # Pattern 2: PRINTFORMW with content (Feature 301)
        # Match PRINTFORMW followed by non-whitespace on the SAME LINE
        # Use [ \t]+ for horizontal whitespace only (not newlines)
        # Then require a non-whitespace, non-newline character: [^\s\n\r]
        # Exclude commented lines (lines starting with ;)
        printformw_pattern = re.compile(r'^[^;\n]*PRINTFORMW[ \t]+[^\s\n\r]', re.MULTILINE)
        if printformw_pattern.search(function_content):
            return "IMPLEMENTED"

        return "STUB"
    except Exception as e:
        print(f"Warning: Failed to check stub status for {file_path}: {e}", file=sys.stderr)
        return "STUB"


def check_single_function(func_name: str, search_path: Path, check_stub: bool = False) -> int:
    """
    Check for duplicates of a specific function.

    Returns:
        0 if unique, 1 if duplicates found
    """
    all_functions = defaultdict(list)

    erb_files = find_erb_files(search_path)
    for erb_file in erb_files:
        file_functions = extract_functions(erb_file)
        if func_name in file_functions:
            all_functions[func_name].extend(file_functions[func_name])

    if not all_functions[func_name]:
        print(f"NOT_FOUND: {func_name}")
        return 1

    locations = all_functions[func_name]
    if len(locations) > 1:
        print(f"DUPLICATE: {func_name}")
        for file_path, line_num in sorted(locations):
            print(f"  - {file_path}:{line_num}")
        return 1
    else:
        file_path, line_num = locations[0]

        if check_stub:
            # Check stub status
            status = check_stub_status(func_name, file_path)
            print(f"{status}: {func_name}")
            return 0
        else:
            print(f"OK: {func_name} (1 definition)")
            print(f"  - {file_path}:{line_num}")
            return 0


def scan_all_functions(search_path: Path) -> int:
    """
    Scan all functions in path and check for duplicates.

    Returns:
        0 if all unique, 1 if any duplicates found
    """
    all_functions = defaultdict(list)

    erb_files = find_erb_files(search_path)
    for erb_file in erb_files:
        file_functions = extract_functions(erb_file)
        for func_name, locations in file_functions.items():
            all_functions[func_name].extend(locations)

    if not all_functions:
        print("No ERB functions found")
        return 0

    # Find duplicates
    duplicates = {name: locs for name, locs in all_functions.items() if len(locs) > 1}

    if duplicates:
        for func_name, locations in sorted(duplicates.items()):
            print(f"DUPLICATE: {func_name}")
            for file_path, line_num in sorted(locations):
                print(f"  - {file_path}:{line_num}")
        return 1
    else:
        total_functions = len(all_functions)
        print(f"OK: All {total_functions} functions are unique")
        return 0


def main():
    parser = argparse.ArgumentParser(
        description='ERB Duplicate Function Checker - Detects duplicate function definitions'
    )
    parser.add_argument(
        '--function',
        type=str,
        help='Function name to check (without @). If omitted, scans all functions.'
    )
    parser.add_argument(
        '--path',
        type=str,
        required=True,
        help='Directory or file to search'
    )
    parser.add_argument(
        '--check-stub',
        action='store_true',
        help='Check if function is stub (empty) or implemented (has DATAFORM content). Requires --function.'
    )

    args = parser.parse_args()

    search_path = Path(args.path)
    if not search_path.exists():
        print(f"Error: Path not found: {search_path}", file=sys.stderr)
        return 1

    if args.check_stub and not args.function:
        print("Error: --check-stub requires --function", file=sys.stderr)
        return 1

    if args.function:
        return check_single_function(args.function, search_path, args.check_stub)
    else:
        return scan_all_functions(search_path)


if __name__ == '__main__':
    sys.exit(main())
