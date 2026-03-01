#!/usr/bin/env python3
"""
Verify that com_file_map.json ranges cover 0-699 without gaps.

Exit codes:
  0: All ranges are covered without gaps
  1: Gaps or overlaps detected
"""

import json
import sys
from pathlib import Path


def verify_range_coverage(json_path: Path) -> tuple[bool, list[str]]:
    """
    Verify that ranges cover 0-699 continuously without gaps.

    Returns:
        (success, errors): success is True if coverage is complete, errors list describes issues
    """
    with open(json_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    ranges = data.get('ranges', [])
    if not ranges:
        return False, ["No ranges defined"]

    # Sort ranges by start position
    sorted_ranges = sorted(ranges, key=lambda r: r['start'])

    errors = []
    expected_next = 0

    for r in sorted_ranges:
        start = r['start']
        end = r['end']

        # Check for gap
        if start > expected_next:
            errors.append(f"Gap detected: {expected_next}-{start-1} not covered")

        # Check for overlap
        if start < expected_next:
            errors.append(f"Overlap detected: {start} already covered (expected {expected_next})")

        # Update expected next
        expected_next = end + 1

    # Check if we reached 699
    if expected_next <= 699:
        errors.append(f"Incomplete coverage: ends at {expected_next-1}, expected 699")

    # Check if we exceeded 699
    if expected_next > 700:
        errors.append(f"Coverage exceeds limit: ends at {expected_next-1}, expected 699")

    return len(errors) == 0, errors


def main():
    script_dir = Path(__file__).parent
    json_path = script_dir / 'com_file_map.json'

    if not json_path.exists():
        print(f"ERROR: {json_path} not found", file=sys.stderr)
        sys.exit(1)

    success, errors = verify_range_coverage(json_path)

    if success:
        print("OK: Range coverage verification passed: 0-699 fully covered")
        sys.exit(0)
    else:
        print("FAIL: Range coverage verification failed:", file=sys.stderr)
        for error in errors:
            print(f"  - {error}", file=sys.stderr)
        sys.exit(1)


if __name__ == '__main__':
    main()
