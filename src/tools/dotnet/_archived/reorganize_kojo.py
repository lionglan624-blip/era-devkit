#!/usr/bin/env python3
"""
Kojo File Reorganization Script
Feature 065: Generalized from reorganize_k4.py

Reorganizes kojo files for any character into COM category-based structure.
Pattern established in Feature 057 (K4).
"""
import argparse
import os
import re
from pathlib import Path

# Character configuration
CHARACTERS = {
    'K1': {'name': '美鈴', 'dir': '1_美鈴', 'main_kojo': 'KOJO_K1.ERB', 'tai_anata': '対あなた口上.ERB'},
    'K2': {'name': '小悪魔', 'dir': '2_小悪魔', 'main_kojo': 'KOJO_K2.ERB', 'tai_anata': '対あなた口上.ERB'},
    'K3': {'name': 'パチュリー', 'dir': '3_パチュリー', 'main_kojo': None, 'tai_anata': '対あなた口上.ERB'},
    'K4': {'name': '咲夜', 'dir': '4_咲夜', 'main_kojo': 'KOJO_K4.ERB', 'tai_anata': '対あなた口上.ERB'},
    'K5': {'name': 'レミリア', 'dir': '5_レミリア', 'main_kojo': None, 'tai_anata': '対あなた口上.ERB'},
    'K6': {'name': 'フラン', 'dir': '6_フラン', 'main_kojo': None, 'tai_anata': '対あなた口上.ERB'},
    'K7': {'name': '子悪魔', 'dir': '7_子悪魔', 'main_kojo': None, 'tai_anata': '対あなた口上.ERB'},
    'K8': {'name': 'チルノ', 'dir': '8_チルノ', 'main_kojo': 'KOJO_K8.ERB', 'tai_anata': '対あなた口上.ERB'},
    'K9': {'name': '大妖精', 'dir': '9_大妖精', 'main_kojo': 'KOJO_K9.ERB', 'tai_anata': '対あなた口上.ERB'},
    'K10': {'name': '魔理沙', 'dir': '10_魔理沙', 'main_kojo': 'KOJO_K10.ERB', 'tai_anata': None},
    'KU': {'name': '汎用', 'dir': 'U_汎用', 'main_kojo': None, 'tai_anata': '対あなた口上.ERB'},
}

# Base directory
BASE_DIR = Path(r"c:\Era\era紅魔館NTR\Game\ERB\口上")


def read_file(filepath):
    """Read file content as list of lines."""
    with open(filepath, 'r', encoding='utf-8-sig') as f:
        return f.readlines()


def write_file(filepath, lines):
    """Write lines to file."""
    with open(filepath, 'w', encoding='utf-8-sig') as f:
        f.writelines(lines)


def find_function_ranges(lines):
    """Find line ranges for each function."""
    functions = {}
    current_func = None
    current_start = None

    for i, line in enumerate(lines, 1):
        if line.startswith('@'):
            if current_func:
                functions[current_func] = (current_start, i - 1)
            match = re.match(r'^@(\w+)', line)
            if match:
                current_func = match.group(1)
                current_start = i

    if current_func:
        functions[current_func] = (current_start, len(lines))

    return functions


def categorize_function(func_name, char_id):
    """Categorize function by COM type for a specific character."""
    # Extract character number for pattern matching
    char_num = char_id[1:]  # K1 -> 1, K10 -> 10, KU -> U

    # EVENT functions
    if f'EVENT_{char_id}' in func_name or f'EVENT_K{char_num}' in func_name:
        return 'EVENT'
    if 'COUNTER' in func_name:
        return 'EVENT'
    if 'PALAMCNG' in func_name:
        return 'EVENT'
    if 'MARKCNG' in func_name:
        return 'EVENT'
    if 'SeeYou' in func_name:
        return 'EVENT'
    if f'CALLNAME_{char_id}' in func_name or f'CALLNAME_K{char_num}' in func_name:
        return 'EVENT'
    if func_name == f'KOJO_{char_id}' or func_name == f'KOJO_K{char_num}':
        return 'EVENT'

    # Extract COM number from various patterns
    com_patterns = [
        rf'COM_{char_id}_(\d+)',
        rf'COM_K{char_num}_(\d+)',
        rf'MESSAGE_COM_{char_id}_(\d+)',
        rf'MESSAGE_COM_K{char_num}_(\d+)',
    ]

    com = None
    for pattern in com_patterns:
        com_match = re.search(pattern, func_name)
        if com_match:
            com = int(com_match.group(1))
            break

    if com is not None:
        # 会話親密: COM 300-315, 350-352
        if 300 <= com <= 315 or 350 <= com <= 352:
            return '会話親密'
        # 日常: COM 410-415, 463
        if 410 <= com <= 415 or com == 463:
            return '日常'
        # 愛撫: COM 0-9, 20-21, 40-48
        if 0 <= com <= 9 or 20 <= com <= 21 or 40 <= com <= 48:
            return '愛撫'
        # 口挿入: COM 60-71, 80-148, 180-203
        if 60 <= com <= 71 or 80 <= com <= 148 or 180 <= com <= 203:
            return '口挿入'

    # SCOM -> 口挿入
    if f'SCOM_{char_id}' in func_name or f'SCOM_K{char_num}' in func_name:
        return '口挿入'

    # NTR wrapper functions - categorize by COM number
    ntr_patterns = [
        rf'NTR_KOJO_MESSAGE_COM_{char_id}_(\d+)',
        rf'NTR_KOJO_MESSAGE_COM_K{char_num}_(\d+)',
        rf'NTR_MESSAGE_COM_{char_id}_(\d+)',
        rf'NTR_MESSAGE_COM_K{char_num}_(\d+)',
    ]

    for pattern in ntr_patterns:
        ntr_match = re.search(pattern, func_name)
        if ntr_match:
            com = int(ntr_match.group(1))
            if 300 <= com <= 315 or 350 <= com <= 352:
                return '会話親密'
            if 60 <= com <= 71:
                return '口挿入'

    # CHK_CANCEL -> 口挿入
    if 'CHK_CANCEL' in func_name:
        return '口挿入'

    return None


def check_and_fix_return(lines, func_name):
    """
    Check if function block has RETURN statement, add if missing.
    Returns (fixed_lines, was_fixed).
    """
    # Check if RETURN or RETURNF exists in the function block
    has_return = False
    for line in lines:
        stripped = line.strip().upper()
        if stripped.startswith('RETURN') or stripped.startswith('RETURNF'):
            has_return = True
            break

    if has_return:
        return lines, False

    # Add RETURN 0 at the end
    fixed_lines = list(lines)
    # Ensure there's a newline before RETURN
    if fixed_lines and not fixed_lines[-1].endswith('\n'):
        fixed_lines[-1] += '\n'
    fixed_lines.append('RETURN 0\n')

    print(f"  WARNING: Function @{func_name} has no RETURN statement")
    print(f"     -> Auto-added: RETURN 0")

    return fixed_lines, True


def make_header(char_name, category, original_files, feature_id='065'):
    """Create file header."""
    files_str = ' + '.join(original_files)
    return f""";-------------------------------------------------
; KOJO_{category} - {char_name}口上 ({category}系)
;
; 元ファイル: {files_str}
; Feature {feature_id}: COMカテゴリベース分割
;-------------------------------------------------

"""


def reorganize_character(char_id, dry_run=False, verbose=False):
    """Reorganize kojo files for a specific character."""
    if char_id not in CHARACTERS:
        print(f"Error: Unknown character {char_id}")
        print(f"Available characters: {', '.join(CHARACTERS.keys())}")
        return False

    config = CHARACTERS[char_id]
    char_dir = BASE_DIR / config['dir']

    if not char_dir.exists():
        print(f"Error: Directory not found: {char_dir}")
        return False

    print(f"\n{'='*60}")
    print(f"Reorganizing {char_id} ({config['name']})")
    print(f"Directory: {char_dir}")
    print(f"{'='*60}")

    # Collect source files
    source_files = {}
    original_files = []

    # Main kojo file
    if config['main_kojo']:
        main_path = char_dir / config['main_kojo']
        if main_path.exists():
            source_files['main'] = read_file(main_path)
            original_files.append(config['main_kojo'])
            print(f"  Found: {config['main_kojo']} ({len(source_files['main'])} lines)")
        else:
            print(f"  Not found: {config['main_kojo']}")

    # 対あなた file
    if config['tai_anata']:
        tai_path = char_dir / config['tai_anata']
        if tai_path.exists():
            source_files['tai_anata'] = read_file(tai_path)
            original_files.append(config['tai_anata'])
            print(f"  Found: {config['tai_anata']} ({len(source_files['tai_anata'])} lines)")
        else:
            print(f"  Not found: {config['tai_anata']}")

    # NTR拡張 file (optional)
    ntr_ext_name = f"KOJO_{char_id}_NTR拡張.ERB"
    ntr_ext_path = char_dir / ntr_ext_name
    if ntr_ext_path.exists():
        source_files['ntr_ext'] = read_file(ntr_ext_path)
        original_files.append(ntr_ext_name)
        print(f"  Found: {ntr_ext_name} ({len(source_files['ntr_ext'])} lines)")

    if not source_files:
        print(f"No source files found for {char_id}")
        return False

    # Find function ranges in each file
    print("\nFinding function ranges...")
    all_functions = {}

    for src_name, lines in source_files.items():
        funcs = find_function_ranges(lines)
        for func, (start, end) in funcs.items():
            all_functions[func] = {
                'source': src_name,
                'start': start,
                'end': end,
                'lines': lines
            }
        print(f"  {src_name}: {len(funcs)} functions")

    # Categorize functions
    categories = {'EVENT': [], '会話親密': [], '愛撫': [], '口挿入': [], '日常': []}
    uncategorized = []

    for func, info in all_functions.items():
        cat = categorize_function(func, char_id)
        if cat:
            categories[cat].append((func, info))
        else:
            uncategorized.append(func)
            if verbose:
                print(f"  Warning: Uncategorized function: {func}")

    print("\nFunction counts by category:")
    for cat, funcs in categories.items():
        if funcs:
            print(f"  {cat}: {len(funcs)} functions")
    if uncategorized:
        print(f"  Uncategorized: {len(uncategorized)} functions")

    # Generate output files
    print("\nGenerating output files...")
    char_prefix = f"KOJO_{char_id}"

    generated_files = []
    total_fixes = 0

    for cat in ['会話親密', '愛撫', '口挿入', '日常', 'EVENT']:
        if not categories[cat]:
            continue

        output = [make_header(config['name'], cat, original_files)]

        # Sort by source (main first) then by line number
        sorted_funcs = sorted(categories[cat], key=lambda x: (
            x[1]['source'] != 'main',
            x[1]['source'],
            x[1]['start']
        ))

        current_source = None
        for func, info in sorted_funcs:
            if info['source'] != current_source and info['source'] != 'main':
                output.append(f";--- from {info['source']} ---\n")
                current_source = info['source']

            func_lines = info['lines'][info['start']-1:info['end']]
            # Check and fix missing RETURN
            fixed_lines, was_fixed = check_and_fix_return(func_lines, func)
            if was_fixed:
                total_fixes += 1
            output.extend(fixed_lines)
            output.append("\n")

        output_name = f"{char_prefix}_{cat}.ERB"
        output_path = char_dir / output_name

        if dry_run:
            print(f"  [DRY-RUN] Would create: {output_name} ({len(output)} lines)")
        else:
            write_file(output_path, output)
            print(f"  Created: {output_name} ({len(output)} lines)")

        generated_files.append(output_name)

    if dry_run:
        print("\n[DRY-RUN] No files were actually modified.")
        print("\nNext steps (when run without --dry-run):")
    else:
        print("\nDone! Files created in:", char_dir)
        if total_fixes > 0:
            print(f"\nWARNING: Auto-fixed {total_fixes} missing RETURN statement(s)")
        print("\nNext steps:")

    print("  1. Review generated files")
    print("  2. Delete original files:")
    for f in original_files:
        print(f"     - {f}")
    print("  3. Run ErbLinter")
    print("  4. Run kojo-mapper")
    print("  5. Test with headless")

    return True


def verify_headless(char_id):
    """
    Run headless kojo test and check for errors in stdout.
    Returns (success, error_output).
    - success=True, error_output=None: No errors detected
    - success=False, error_output=str: Error found, returns problematic output
    """
    import subprocess

    config = CHARACTERS.get(char_id)
    if not config:
        return False, f"Unknown character: {char_id}"

    char_num = char_id[1:]  # K5 -> 5, K10 -> 10

    print(f"\nRunning headless verification for {char_id}...")

    try:
        result = subprocess.run(
            ['dotnet', 'run', '--project', 'uEmuera/uEmuera.Headless.csproj',
             '--runtime', 'win-x64', '--',
             'Game', '--kojo-test', f'KOJO_MESSAGE_COM_{char_id}_300', '--char', char_num],
            capture_output=True, text=True, timeout=120, encoding='utf-8', errors='replace',
            cwd=str(BASE_DIR.parent.parent)  # Project root
        )
    except subprocess.TimeoutExpired:
        return False, "Headless test timed out"
    except Exception as e:
        return False, f"Failed to run headless: {e}"

    output = (result.stdout or "") + (result.stderr or "")

    # Check for error patterns
    error_patterns = [
        "予期しないスクリプト終端",
        "エラーが発生しました",
        "関数の終端でエラー",
        "例外が発生しました",
    ]

    for pattern in error_patterns:
        if pattern in output:
            # Extract relevant lines around the error
            lines = output.split('\n')
            error_context = []
            for i, line in enumerate(lines):
                if pattern in line:
                    start = max(0, i - 5)
                    end = min(len(lines), i + 10)
                    error_context = lines[start:end]
                    break
            return False, '\n'.join(error_context) if error_context else output[-2000:]

    return True, None


def scan_preserved_files(char_id, fix=False):
    """
    Scan preserved files (NTR口上, SexHara, WC系) for missing RETURN statements.
    Returns list of issues found.
    """
    if char_id not in CHARACTERS:
        print(f"Error: Unknown character {char_id}")
        return []

    config = CHARACTERS[char_id]
    char_dir = BASE_DIR / config['dir']

    preserved_patterns = ['NTR口上*.ERB', 'SexHara*.ERB', 'WC系*.ERB']
    issues = []

    print(f"\nScanning preserved files for missing RETURN statements...")

    for pattern in preserved_patterns:
        for filepath in char_dir.glob(pattern):
            lines = read_file(filepath)
            funcs = find_function_ranges(lines)

            file_issues = []
            for func_name, (start, end) in funcs.items():
                func_lines = lines[start-1:end]
                has_return = False
                for line in func_lines:
                    stripped = line.strip().upper()
                    if stripped.startswith('RETURN') or stripped.startswith('RETURNF'):
                        has_return = True
                        break

                if not has_return:
                    file_issues.append((func_name, start, end))

            if file_issues:
                print(f"\n  {filepath.name}: {len(file_issues)} missing RETURN(s)")
                for func_name, start, end in file_issues:
                    print(f"    WARNING: @{func_name} (line {start}-{end})")
                    issues.append((filepath, func_name, start, end))

                if fix:
                    # Fix the file
                    fixed_lines = list(lines)
                    offset = 0
                    for func_name, start, end in sorted(file_issues, key=lambda x: x[1]):
                        # Insert RETURN 0 after the function (at end position + offset)
                        insert_pos = end + offset
                        fixed_lines.insert(insert_pos, 'RETURN 0\n')
                        offset += 1
                        print(f"      -> Added RETURN 0 after @{func_name}")

                    write_file(filepath, fixed_lines)
                    print(f"    FIXED: {filepath.name}")

    if not issues:
        print("  OK: No missing RETURN statements found in preserved files")

    return issues


def main():
    parser = argparse.ArgumentParser(
        description='Reorganize kojo files into COM category-based structure'
    )
    parser.add_argument(
        '--char', '-c',
        required=True,
        help='Character ID (K1, K2, ..., K10, KU)'
    )
    parser.add_argument(
        '--dry-run', '-n',
        action='store_true',
        help='Preview changes without modifying files'
    )
    parser.add_argument(
        '--verbose', '-v',
        action='store_true',
        help='Show verbose output including uncategorized functions'
    )
    parser.add_argument(
        '--list', '-l',
        action='store_true',
        help='List available characters and exit'
    )
    parser.add_argument(
        '--scan-preserved',
        action='store_true',
        help='Scan preserved files (NTR口上, SexHara, WC系) for missing RETURNs'
    )
    parser.add_argument(
        '--fix-preserved',
        action='store_true',
        help='Scan AND fix preserved files for missing RETURNs'
    )
    parser.add_argument(
        '--verify',
        action='store_true',
        help='Run headless test and check for errors (minimal output on success)'
    )

    args = parser.parse_args()

    if args.list:
        print("Available characters:")
        for cid, config in CHARACTERS.items():
            print(f"  {cid}: {config['name']} ({config['dir']})")
        return

    char_id = args.char.upper()

    # Verify mode - run headless and check for errors
    if args.verify:
        success, error_output = verify_headless(char_id)
        if success:
            print(f"PASS: {char_id} headless test OK")
            exit(0)
        else:
            print(f"FAIL: {char_id} headless test failed")
            print("--- Error Context ---")
            print(error_output)
            print("--- End ---")
            exit(1)

    # Scan/fix preserved files if requested
    if args.scan_preserved or args.fix_preserved:
        issues = scan_preserved_files(char_id, fix=args.fix_preserved)
        if issues and not args.fix_preserved:
            print(f"\nFound {len(issues)} missing RETURN(s). Use --fix-preserved to auto-fix.")
        return

    success = reorganize_character(char_id, dry_run=args.dry_run, verbose=args.verbose)

    if not success:
        exit(1)


if __name__ == '__main__':
    main()
