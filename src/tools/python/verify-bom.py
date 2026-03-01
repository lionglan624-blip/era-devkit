#!/usr/bin/env python3
"""
BOM Verification Script for ERB Files
Detects missing UTF-8 BOM in ERB files and optionally fixes them.

Usage:
    python tools/verify-bom.py <path>              # Verify only
    python tools/verify-bom.py --fix <path>        # Verify and fix

Exit codes:
    0 = All files have BOM (or all fixed)
    1 = One or more files missing BOM
"""

import argparse
import sys
from pathlib import Path
from typing import List, Tuple


def find_erb_files(search_path: Path) -> List[Path]:
    """Find all .ERB files in the given path."""
    if search_path.is_file():
        return [search_path] if search_path.suffix.upper() == '.ERB' else []

    erb_files = []
    for pattern in ['**/*.ERB', '**/*.erb']:
        erb_files.extend(search_path.glob(pattern))
    return sorted(set(erb_files))


def has_bom(file_path: Path) -> bool:
    """Check if file has UTF-8 BOM (EF BB BF)."""
    try:
        with open(file_path, 'rb') as f:
            first_bytes = f.read(3)
            return first_bytes == b'\xef\xbb\xbf'
    except OSError as e:
        print(f"Warning: Failed to read {file_path}: {e}", file=sys.stderr)
        return False


def add_bom(file_path: Path) -> bool:
    """Add UTF-8 BOM to file. Returns True on success."""
    try:
        # Read existing content
        with open(file_path, 'rb') as f:
            content = f.read()

        # Write BOM + content
        with open(file_path, 'wb') as f:
            f.write(b'\xef\xbb\xbf')
            f.write(content)

        return True
    except OSError as e:
        print(f"Error: Failed to fix {file_path}: {e}", file=sys.stderr)
        return False


def verify_bom(search_path: Path, fix: bool = False) -> Tuple[List[Path], List[Path], int]:
    """
    Verify BOM in ERB files.

    Args:
        search_path: Directory or file to search
        fix: If True, add BOM to files missing it

    Returns:
        (files_with_bom, files_without_bom, fixed_count)
    """
    erb_files = find_erb_files(search_path)

    if not erb_files:
        print(f"No ERB files found in {search_path}", file=sys.stderr)
        return [], [], 0

    files_with_bom = []
    files_without_bom = []

    for erb_file in erb_files:
        if has_bom(erb_file):
            files_with_bom.append(erb_file)
        else:
            files_without_bom.append(erb_file)

    # Fix if requested
    fixed_count = 0
    if fix and files_without_bom:
        fixed = []
        failed = []

        for erb_file in files_without_bom:
            if add_bom(erb_file):
                fixed.append(erb_file)
                fixed_count += 1
            else:
                failed.append(erb_file)

        # Update lists after fixing
        files_with_bom.extend(fixed)
        files_without_bom = failed

    return files_with_bom, files_without_bom, fixed_count


def main():
    parser = argparse.ArgumentParser(
        description='BOM Verification Script for ERB Files'
    )
    parser.add_argument(
        'path',
        type=str,
        help='Directory or file to verify'
    )
    parser.add_argument(
        '--fix',
        action='store_true',
        help='Automatically add BOM to files missing it'
    )

    args = parser.parse_args()

    search_path = Path(args.path)
    if not search_path.exists():
        print(f"Error: Path not found: {search_path}", file=sys.stderr)
        sys.exit(1)

    # Verify BOM
    files_with_bom, files_without_bom, fixed_count = verify_bom(search_path, args.fix)

    # Report results
    total = len(files_with_bom) + len(files_without_bom)

    if not total:
        sys.exit(0)

    if files_without_bom:
        print(f"BOM Missing: {len(files_without_bom)}/{total} files")
        for file_path in files_without_bom:
            print(f"  - {file_path}")
        sys.exit(1)
    else:
        if args.fix and fixed_count > 0:
            print(f"BOM Fixed: {fixed_count}/{total} files")
        else:
            print(f"BOM OK: {len(files_with_bom)}/{total} files")
        sys.exit(0)


if __name__ == '__main__':
    main()
