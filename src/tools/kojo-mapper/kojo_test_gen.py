#!/usr/bin/env python3
"""
Kojo Test Generator - Mechanical DATALIST extraction for 100% accurate test JSON generation
Eliminates LLM hallucination errors in test expectations
"""

import re
import os
import sys
import json
import argparse
from pathlib import Path
from typing import Dict, List, Optional, Tuple
from kojo_mapper import parse_erb_file, KojoFunction, detect_relationship_branches

# CALLNAME placeholder mappings
CALLNAME_MAP = {
    "%CALLNAME:MASTER%": "あなた",
    "%CALLNAME:人物_美鈴%": "美鈴",
    "%CALLNAME:人物_小悪魔%": "小悪魔",
    "%CALLNAME:人物_パチュリー%": "パチュリー",
    "%CALLNAME:人物_咲夜%": "咲夜",
    "%CALLNAME:人物_レミリア%": "レミリア",
    "%CALLNAME:人物_フラン%": "フラン",
    "%CALLNAME:人物_子悪魔%": "子悪魔",
    "%CALLNAME:人物_チルノ%": "チルノ",
    "%CALLNAME:人物_大妖精%": "大妖精",
    "%CALLNAME:人物_魔理沙%": "魔理沙",
}

# TALENT → state mapping
TALENT_STATE_MAP = {
    "恋人": {"TALENT:TARGET:16": 1},
    "恋慕": {"TALENT:TARGET:3": 1},
    "思慕": {"TALENT:TARGET:17": 1},
    "なし": {},
}

# Load COM→ERBファイルマッピング from JSON SSOT
# SSOT: tools/kojo-mapper/com_file_map.json
def load_com_file_map():
    """Load COM file mapping from JSON SSOT"""
    json_path = Path(__file__).parent / "com_file_map.json"
    with open(json_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    # Build COM_FILE_MAP from ranges
    com_map = {}
    for range_entry in data["ranges"]:
        for com_num in range(range_entry["start"], range_entry["end"] + 1):
            com_map[com_num] = range_entry["file"]

    return com_map, data["character_overrides"]

# Load at module import
COM_FILE_MAP, CHARACTER_OVERRIDES = load_com_file_map()

# Character K-number mapping (for Feature 182)
# File suffix is now dynamic based on COM_FILE_MAP
CHAR_MAP = {
    "K1": ("美鈴", "1", "Game/ERB/口上/1_美鈴/KOJO_K1"),
    "K2": ("小悪魔", "2", "Game/ERB/口上/2_小悪魔/KOJO_K2"),
    "K3": ("パチュリー", "3", "Game/ERB/口上/3_パチュリー/KOJO_K3"),
    "K4": ("咲夜", "4", "Game/ERB/口上/4_咲夜/KOJO_K4"),
    "K5": ("レミリア", "5", "Game/ERB/口上/5_レミリア/KOJO_K5"),
    "K6": ("フラン", "6", "Game/ERB/口上/6_フラン/KOJO_K6"),
    "K7": ("子悪魔", "7", "Game/ERB/口上/7_子悪魔/KOJO_K7"),
    "K8": ("チルノ", "8", "Game/ERB/口上/8_チルノ/KOJO_K8"),
    "K9": ("大妖精", "9", "Game/ERB/口上/9_大妖精/KOJO_K9"),
    "K10": ("魔理沙", "10", "Game/ERB/口上/10_魔理沙/KOJO_K10"),
}


def get_erb_file_for_com(com_num: int) -> str:
    """Get ERB file suffix for a given COM number

    Args:
        com_num: COM number

    Returns:
        ERB file suffix (e.g., "_愛撫.ERB", "_挿入.ERB")

    Raises:
        ValueError: If COM number is not supported
    """
    if com_num in COM_FILE_MAP:
        return COM_FILE_MAP[com_num]
    raise ValueError(f"Unsupported COM: {com_num} (no kojo file exists)")


def expand_placeholders(text: str, target_name: Optional[str] = None, warn_unknown: bool = False) -> str:
    """Expand CALLNAME placeholders in text

    Args:
        text: Text containing placeholders
        target_name: Character name for TARGET placeholder (e.g., "美鈴")
        warn_unknown: If True, warn about unknown placeholders

    Returns:
        Text with placeholders expanded
    """
    result = text

    # Expand TARGET placeholder if target_name provided
    if target_name:
        result = result.replace("%CALLNAME:TARGET%", target_name)

    # Expand other CALLNAME placeholders
    for placeholder, value in CALLNAME_MAP.items():
        result = result.replace(placeholder, value)

    # Warn about unknown placeholders
    if warn_unknown:
        unknown = re.findall(r'%CALLNAME:[^%]+%', result)
        for unknown_placeholder in unknown:
            print(f"Warning: Unknown placeholder '{unknown_placeholder}'", file=sys.stderr)

    return result


def extract_datalist_blocks(func: KojoFunction, all_functions: Dict[str, KojoFunction], target_name: Optional[str] = None) -> List[Tuple[str, List[str]]]:
    """Extract DATALIST blocks from a function, resolving CALL chains

    Args:
        func: Target function to extract from
        all_functions: Dict of all functions (for CALL resolution)
        target_name: Character name for TARGET placeholder expansion (e.g., "美鈴")

    Returns:
        List of (branch_label, datalist_patterns) tuples
        branch_label: "恋人", "恋慕", "思慕", "なし"
        datalist_patterns: List of DATAFORM text lists (one per DATALIST block)
    """
    # Read function content from file
    try:
        with open(func.file, 'r', encoding='utf-8-sig') as f:
            lines = f.readlines()
    except Exception as e:
        print(f"Error reading {func.file}: {e}", file=sys.stderr)
        return []

    # Find function start and extract content
    func_content = []
    in_function = False
    for i, line in enumerate(lines):
        if line.strip().startswith(f'@{func.name}'):
            in_function = True
            continue
        elif in_function:
            # Check for next function definition
            if line.strip().startswith('@') and not line.strip().startswith('@;'):
                break
            func_content.append(line)

    # Parse TALENT branches and extract DATALIST blocks
    return parse_talent_branches(func_content, func.name, all_functions, target_name)


def parse_talent_branches(lines: List[str], func_name: str, all_functions: Dict[str, KojoFunction], target_name: Optional[str] = None) -> List[Tuple[str, List[List[str]]]]:
    """Parse TALENT branches and extract DATALIST blocks from each branch

    Args:
        lines: Function content lines
        func_name: Function name (for error reporting)
        all_functions: Dict of all functions (for CALL resolution)
        target_name: Character name for TARGET placeholder expansion (e.g., "美鈴")

    Returns:
        List of (branch_label, datalist_patterns) tuples
    """
    results = []
    current_branch = None
    in_printdata = False
    in_datalist = False
    current_datalist = []
    branch_datalists = []

    for line in lines:
        stripped = line.strip()
        upper_stripped = stripped.upper()

        # Detect TALENT branches
        if re.match(r'^IF\s+TALENT:恋人', upper_stripped):
            current_branch = "恋人"
            branch_datalists = []
        elif re.match(r'^ELSEIF\s+TALENT:恋慕', upper_stripped):
            # Save previous branch
            if current_branch and branch_datalists:
                results.append((current_branch, branch_datalists))
            current_branch = "恋慕"
            branch_datalists = []
        elif re.match(r'^ELSEIF\s+TALENT:思慕', upper_stripped):
            # Save previous branch
            if current_branch and branch_datalists:
                results.append((current_branch, branch_datalists))
            current_branch = "思慕"
            branch_datalists = []
        elif upper_stripped == 'ELSE':
            # Save previous branch
            if current_branch and branch_datalists:
                results.append((current_branch, branch_datalists))
            current_branch = "なし"
            branch_datalists = []
        elif upper_stripped == 'ENDIF':
            # Save final branch
            if current_branch and branch_datalists:
                results.append((current_branch, branch_datalists))
            current_branch = None
            branch_datalists = []

        # Track PRINTDATA blocks
        if re.match(r'^PRINTDATA[LW]?\b', upper_stripped):
            in_printdata = True
        elif re.match(r'^ENDDATA\b', upper_stripped):
            in_printdata = False

        # Track DATALIST blocks
        if in_printdata:
            if upper_stripped == 'DATALIST':
                in_datalist = True
                current_datalist = []
            elif upper_stripped == 'ENDLIST':
                in_datalist = False
                if current_datalist:
                    branch_datalists.append(current_datalist)
                current_datalist = []
            elif in_datalist:
                # Extract DATAFORM content
                dataform_match = re.match(r'^DATAFORM\s*(.*)', stripped, re.IGNORECASE)
                if dataform_match:
                    content = dataform_match.group(1).strip()
                    # Expand placeholders
                    content = expand_placeholders(content, target_name=target_name)
                    # Only add non-empty lines
                    if content:
                        current_datalist.append(content)

    return results


def generate_test_json(func_name: str, branch_data: List[Tuple[str, List[List[str]]]], character: str, com: str, feature_id: str) -> Dict:
    """Generate test JSON in Feature 180 format

    Args:
        func_name: Function name (e.g., "KOJO_MESSAGE_COM_K1_48")
        branch_data: List of (branch_label, datalist_patterns) tuples
        character: Character number (e.g., "1")
        com: COM number (e.g., "48")
        feature_id: Feature ID (e.g., "182")

    Returns:
        Test JSON dict
    """
    tests = []

    for branch_label, datalist_patterns in branch_data:
        state = TALENT_STATE_MAP.get(branch_label, {})

        for pattern_idx, dataform_list in enumerate(datalist_patterns):
            test = {
                "name": f"{branch_label}_pattern{pattern_idx}",
                "call": func_name,
                "mock_rand": [pattern_idx],
                "state": state,
                "expect": {
                    "output_contains": dataform_list
                }
            }
            tests.append(test)

    # Determine K-number from function name
    k_match = re.search(r'_K(\d+)_', func_name)
    k_num = k_match.group(1) if k_match else character

    return {
        "name": f"Feature {feature_id}: K{k_num} COM_{com}",
        "defaults": {"character": character},
        "tests": tests
    }


def find_function_by_name(erb_file: Path, func_name: str) -> Optional[KojoFunction]:
    """Find a function by name in an ERB file

    Args:
        erb_file: Path to ERB file
        func_name: Function name to find

    Returns:
        KojoFunction if found, None otherwise
    """
    kojo_file = parse_erb_file(erb_file)
    for func in kojo_file.functions:
        if func.name == func_name:
            return func
    return None


def main():
    parser = argparse.ArgumentParser(
        description='Kojo Test Generator - Mechanical DATALIST extraction',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Single function
  python kojo_test_gen.py --function KOJO_MESSAGE_COM_K1_48 --output test.json Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB

  # Verbose mode
  python kojo_test_gen.py --function KOJO_MESSAGE_COM_K1_48 --verbose Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB

  # Feature batch generation
  python kojo_test_gen.py --feature 182 --com 48 --output-dir tests/ac/kojo/feature-182/

  # Test unknown placeholder warning
  python kojo_test_gen.py --test-unknown-placeholder
        """
    )
    parser.add_argument('erb_file', type=Path, nargs='?',
                        help='Path to ERB file')
    parser.add_argument('--function', type=str,
                        help='Function name to extract (e.g., KOJO_MESSAGE_COM_K1_48)')
    parser.add_argument('--output', type=Path,
                        help='Output JSON file path')
    parser.add_argument('--verbose', action='store_true',
                        help='Verbose output (show branch info)')
    parser.add_argument('--feature', type=str,
                        help='Feature ID for batch generation (requires --com)')
    parser.add_argument('--com', type=str,
                        help='COM number for batch generation (requires --feature)')
    parser.add_argument('--output-dir', type=Path,
                        help='Output directory for batch generation')
    parser.add_argument('--test-unknown-placeholder', action='store_true',
                        help='Test unknown placeholder warning')

    args = parser.parse_args()

    # Test mode for unknown placeholder
    if args.test_unknown_placeholder:
        test_text = "Hello %CALLNAME:UNKNOWN_CHAR% world"
        print(f"Testing unknown placeholder warning:")
        expand_placeholders(test_text, warn_unknown=True)
        return

    # Feature batch mode
    if args.feature and args.com:
        if not args.output_dir:
            print("Error: --output-dir required for batch generation", file=sys.stderr)
            sys.exit(1)

        # Get ERB file suffix for this COM
        com_num = int(args.com)
        try:
            erb_file_suffix = get_erb_file_for_com(com_num)
        except ValueError as e:
            print(f"Error: {e}", file=sys.stderr)
            sys.exit(1)

        # Generate tests for all 10 characters
        args.output_dir.mkdir(parents=True, exist_ok=True)

        for k_num in range(1, 11):
            k_id = f"K{k_num}"
            char_name, char_id, erb_file_prefix = CHAR_MAP[k_id]
            func_name = f"KOJO_MESSAGE_COM_{k_id}_{args.com}"
            # Inner function name (contains DATALIST, avoids TRAIN_MESSAGE timeout)
            inner_func_name = func_name + "_1"

            # Build full ERB file path using COM_FILE_MAP (with character overrides)
            if k_id in CHARACTER_OVERRIDES and str(com_num) in CHARACTER_OVERRIDES[k_id]:
                char_erb_suffix = CHARACTER_OVERRIDES[k_id][str(com_num)]
            else:
                char_erb_suffix = erb_file_suffix
            erb_file = erb_file_prefix + char_erb_suffix

            erb_path = Path(erb_file)
            if not erb_path.exists():
                print(f"Warning: {erb_file} not found, skipping {k_id}", file=sys.stderr)
                continue

            # Find inner function (contains DATALIST blocks)
            func = find_function_by_name(erb_path, inner_func_name)
            if not func:
                print(f"Error: Function '{inner_func_name}' not found in {erb_file}", file=sys.stderr)
                continue

            # Build all_functions dict (for CALL resolution)
            kojo_file = parse_erb_file(erb_path)
            all_functions = {f.name: f for f in kojo_file.functions}

            # Extract DATALIST blocks (with character name for TARGET placeholder expansion)
            branch_data = extract_datalist_blocks(func, all_functions, target_name=char_name)

            if not branch_data:
                print(f"Warning: Found 0 DATALIST blocks in {inner_func_name}", file=sys.stderr)
                continue

            # Generate test JSON (use inner function for testing to avoid TRAIN_MESSAGE)
            test_json = generate_test_json(inner_func_name, branch_data, char_id, args.com, args.feature)

            # Write output
            output_file = args.output_dir / f"test-{args.feature}-{k_id}.json"
            with open(output_file, 'w', encoding='utf-8') as f:
                json.dump(test_json, f, ensure_ascii=False, indent=2)

            print(f"Generated: {output_file} ({len(test_json['tests'])} tests)")

        return

    # Single function mode
    if not args.erb_file:
        print("Error: ERB file required (or use --feature --com for batch mode)", file=sys.stderr)
        sys.exit(1)

    if not args.erb_file.exists():
        print(f"Error: File not found: {args.erb_file}", file=sys.stderr)
        sys.exit(1)

    if not args.function:
        print("Error: --function required for single function mode", file=sys.stderr)
        sys.exit(1)

    # Find function in ERB file
    func = find_function_by_name(args.erb_file, args.function)
    if not func:
        print(f"Error: Function '{args.function}' not found in {args.erb_file}", file=sys.stderr)
        sys.exit(1)

    # Build all_functions dict (for CALL resolution)
    kojo_file = parse_erb_file(args.erb_file)
    all_functions = {f.name: f for f in kojo_file.functions}

    # Extract character name from function name for TARGET placeholder expansion
    # e.g., KOJO_MESSAGE_COM_K1_48 -> K1 -> "美鈴"
    k_match = re.search(r'_K(\d+)_', args.function)
    target_name = None
    if k_match:
        k_id = f"K{k_match.group(1)}"
        if k_id in CHAR_MAP:
            target_name = CHAR_MAP[k_id][0]

    # Extract DATALIST blocks (with character name for TARGET placeholder expansion)
    branch_data = extract_datalist_blocks(func, all_functions, target_name=target_name)

    # Count total DATALIST blocks
    total_datalists = sum(len(patterns) for _, patterns in branch_data)

    print(f"Found {total_datalists} DATALIST blocks")

    if args.verbose:
        # Show branch info
        branch_info = detect_relationship_branches('\n'.join([line for line in open(func.file, 'r', encoding='utf-8-sig').readlines()]))
        branch_labels = [label for label, _ in branch_data]
        print(f"TALENT branches: {', '.join(branch_labels)}")

    if total_datalists == 0:
        # Exit 0 for no DATALIST (AC13)
        sys.exit(0)

    # Generate test JSON if output specified
    if args.output:
        # Extract COM number and character from function name
        # e.g., KOJO_MESSAGE_COM_K1_48 -> com=48, char=1
        match = re.match(r'KOJO_MESSAGE_COM_K(\d+)_(\d+)', args.function)
        if not match:
            print(f"Error: Cannot parse COM/character from function name '{args.function}'", file=sys.stderr)
            sys.exit(1)

        char_num = match.group(1)
        com_num = match.group(2)
        feature_id = "XXX"  # Default feature ID

        test_json = generate_test_json(args.function, branch_data, char_num, com_num, feature_id)

        # Ensure output directory exists
        args.output.parent.mkdir(parents=True, exist_ok=True)

        with open(args.output, 'w', encoding='utf-8') as f:
            json.dump(test_json, f, ensure_ascii=False, indent=2)

        print(f"Generated: {args.output}")


if __name__ == '__main__':
    main()
