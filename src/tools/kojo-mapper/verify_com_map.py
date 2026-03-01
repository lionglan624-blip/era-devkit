#!/usr/bin/env python3
"""
COM_FILE_MAP → ERB File Existence Verification

Verifies that all ERB files referenced in COM_FILE_MAP actually exist
for all characters K1-K10 (with documented exceptions).

Only checks ranges with "implemented": true in com_file_map.json.
Ranges with "implemented": false are skipped (future expansion).

Exit codes:
  0: All implemented COM mappings verified successfully
  1: Missing ERB files detected
"""

import sys
import json
from pathlib import Path

# SSOT: tools/kojo-mapper/com_file_map.json


def load_json_config():
    """Load configuration from JSON SSOT"""
    json_path = Path(__file__).parent / "com_file_map.json"
    with open(json_path, 'r', encoding='utf-8') as f:
        return json.load(f)


def load_com_file_map(data):
    """Build COM_FILE_MAP from JSON ranges (implemented: true only)"""
    com_file_map = {}
    for range_entry in data["ranges"]:
        if not range_entry.get("implemented", True):
            continue  # Skip unimplemented ranges
        for com in range(range_entry["start"], range_entry["end"] + 1):
            com_file_map[com] = range_entry["file"]
    return com_file_map


def load_skip_combinations(data):
    """Load skip combinations from JSON"""
    skip_set = set()
    for entry in data["skip_combinations"]:
        skip_set.add((entry["character"], entry["file"]))
    return skip_set


# Character mapping (same as kojo_test_gen.py)
CHAR_MAP = {
    "K1": ("1", "美鈴", "Game/ERB/口上/1_美鈴/KOJO_K1"),
    "K2": ("2", "小悪魔", "Game/ERB/口上/2_小悪魔/KOJO_K2"),
    "K3": ("3", "パチュリー", "Game/ERB/口上/3_パチュリー/KOJO_K3"),
    "K4": ("4", "咲夜", "Game/ERB/口上/4_咲夜/KOJO_K4"),
    "K5": ("5", "レミリア", "Game/ERB/口上/5_レミリア/KOJO_K5"),
    "K6": ("6", "フラン", "Game/ERB/口上/6_フラン/KOJO_K6"),
    "K7": ("7", "子悪魔", "Game/ERB/口上/7_子悪魔/KOJO_K7"),
    "K8": ("8", "チルノ", "Game/ERB/口上/8_チルノ/KOJO_K8"),
    "K9": ("9", "大妖精", "Game/ERB/口上/9_大妖精/KOJO_K9"),
    "K10": ("10", "魔理沙", "Game/ERB/口上/10_魔理沙/KOJO_K10"),
}


def get_exclusively_unimplemented_files(data):
    """Get ERB file suffixes that are ONLY in unimplemented ranges

    Files shared between implemented and unimplemented ranges are excluded,
    since they legitimately exist for the implemented ranges.
    """
    implemented_files = set()
    unimplemented_files = set()

    for range_entry in data["ranges"]:
        if range_entry.get("implemented", True):
            implemented_files.add(range_entry["file"])
        else:
            unimplemented_files.add(range_entry["file"])

    # Only return files that are exclusively unimplemented
    return unimplemented_files - implemented_files


def verify_com_file_map():
    """Verify COM_FILE_MAP ERB file existence for all characters

    Returns:
        int: 0 on success, 1 on error
    """
    # Load JSON configuration
    data = load_json_config()
    com_file_map = load_com_file_map(data)
    skip_combinations = load_skip_combinations(data)
    unimplemented_files = get_exclusively_unimplemented_files(data)

    # Get unique ERB file suffixes from implemented ranges
    erb_suffixes = set(com_file_map.values())

    errors = []
    verified_count = 0

    # Check 1: Verify implemented ranges have files
    for erb_suffix in sorted(erb_suffixes):
        for k_id in sorted(CHAR_MAP.keys()):
            # Skip known missing combinations
            if (k_id, erb_suffix) in skip_combinations:
                continue

            # Build full ERB file path
            _, _, erb_file_prefix = CHAR_MAP[k_id]
            erb_path = Path(erb_file_prefix + erb_suffix)

            # Check file existence
            if not erb_path.exists():
                # Find all COM numbers that map to this ERB suffix
                com_nums = [com for com, suffix in com_file_map.items() if suffix == erb_suffix]
                for com_num in com_nums:
                    errors.append(f"ERROR: COM {com_num} {k_id} → {erb_path} (missing)")
            else:
                verified_count += 1

    # Check 2: Detect files that exist but are marked as unimplemented
    for erb_suffix in sorted(unimplemented_files):
        for k_id in sorted(CHAR_MAP.keys()):
            _, _, erb_file_prefix = CHAR_MAP[k_id]
            erb_path = Path(erb_file_prefix + erb_suffix)

            if erb_path.exists():
                errors.append(
                    f"ERROR: {erb_path} exists but range is marked implemented=false. "
                    f"Update com_file_map.json to set implemented=true."
                )

    # Report results
    if errors:
        for error in errors:
            print(error)
        return 1
    else:
        print(f"OK: All {verified_count} COM mappings verified")
        return 0


if __name__ == "__main__":
    sys.exit(verify_com_file_map())
