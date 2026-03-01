#!/usr/bin/env python3
"""
Verify com_file_map.json References in Documentation

Checks that all 7 files reference com_file_map.json as the SSOT.

Exit codes:
  0: All 7 files contain com_file_map.json reference
  1: One or more files missing reference
"""

import sys
from pathlib import Path

# Files that must reference com_file_map.json
REQUIRED_FILES = [
    ".claude/skills/kojo-writing/SKILL.md",
    ".claude/agents/com-auditor.md",
    ".claude/commands/do.md",
    ".claude/commands/kojo-init.md",
    ".claude/skills/testing/KOJO.md",
    "tools/kojo-mapper/kojo_test_gen.py",
    "tools/kojo-mapper/verify_com_map.py",
]

def verify_json_references():
    """Verify that all required files reference com_file_map.json

    Returns:
        int: 0 on success, 1 on error
    """
    # Get repository root (3 levels up from this script)
    repo_root = Path(__file__).parent.parent.parent.parent

    errors = []
    verified_count = 0

    for rel_path in REQUIRED_FILES:
        file_path = repo_root / rel_path

        # Check file exists
        if not file_path.exists():
            errors.append(f"ERROR: File not found: {rel_path}")
            continue

        # Check for com_file_map.json reference
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()

            if "com_file_map.json" in content:
                verified_count += 1
            else:
                errors.append(f"ERROR: Missing com_file_map.json reference: {rel_path}")

        except Exception as e:
            errors.append(f"ERROR: Failed to read {rel_path}: {e}")

    # Report results
    if errors:
        for error in errors:
            print(error)
        print(f"\nVerified: {verified_count}/{len(REQUIRED_FILES)}")
        return 1
    else:
        print(f"OK: All {verified_count} files contain com_file_map.json reference")
        return 0


if __name__ == "__main__":
    sys.exit(verify_json_references())
