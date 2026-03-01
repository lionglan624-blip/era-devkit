#!/usr/bin/env python3
"""
Test ac-static-verifier.py count_equals matcher.

This test verifies that the count_equals matcher correctly counts files
matching a glob pattern and compares to the expected count.
"""

import sys
from pathlib import Path

# Add parent directory to path to import ac-static-verifier
repo_root = Path(__file__).parent.parent.parent.parent.parent
sys.path.insert(0, str(repo_root / "src" / "tools" / "python"))

# Import the ACVerifier class and related types
import importlib.util

# Load ac-static-verifier.py as a module
verifier_path = repo_root / "src" / "tools" / "python" / "ac-static-verifier.py"
spec = importlib.util.spec_from_file_location("ac_static_verifier", verifier_path)
ac_verifier_module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ac_verifier_module)

ACDefinition = ac_verifier_module.ACDefinition
ACVerifier = ac_verifier_module.ACVerifier
PatternType = ac_verifier_module.PatternType


def test_count_equals_positive():
    """Test that count_equals returns PASS when count matches expected.

    This test counts Python files in src/tools/python/tests directory.
    """
    verifier = ACVerifier("999", "file", repo_root)

    # Count actual .py files in src/tools/python/tests to set correct expected value
    test_dir = repo_root / "src" / "tools" / "python" / "tests"
    actual_py_count = len(list(test_dir.glob("*.py")))

    ac = ACDefinition(
        ac_number=1,
        description="Test count_equals positive case",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="count_equals",
        expected=str(actual_py_count)
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "PASS", (
        f"Expected PASS when count matches, got {result['result']}"
    )
    assert result["details"]["actual_count"] == actual_py_count, (
        f"Expected actual_count={actual_py_count}, got {result['details']['actual_count']}"
    )
    assert result["details"]["expected_count"] == actual_py_count, (
        f"Expected expected_count={actual_py_count}, got {result['details']['expected_count']}"
    )
    print(f"[PASS] count_equals returns PASS when count matches (expected={actual_py_count})")


def test_count_equals_negative():
    """Test that count_equals returns FAIL when count differs from expected.

    Uses an intentionally wrong expected value to trigger FAIL.
    """
    verifier = ACVerifier("999", "file", repo_root)

    # Use an impossible count (999) to guarantee FAIL
    ac = ACDefinition(
        ac_number=2,
        description="Test count_equals negative case",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="count_equals",
        expected="999"
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "FAIL", (
        f"Expected FAIL when count differs, got {result['result']}"
    )
    assert result["details"]["expected_count"] == 999, (
        f"Expected expected_count=999, got {result['details']['expected_count']}"
    )
    assert result["details"]["actual_count"] != 999, (
        "Sanity check: actual count should not be 999"
    )
    print(f"[PASS] count_equals returns FAIL when count differs (actual={result['details']['actual_count']}, expected=999)")


def test_count_equals_non_numeric_expected():
    """Test that count_equals returns FAIL with error when Expected is non-numeric."""
    verifier = ACVerifier("999", "file", repo_root)

    ac = ACDefinition(
        ac_number=3,
        description="Test count_equals with non-numeric Expected",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="count_equals",
        expected="not-a-number"
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "FAIL", (
        f"Expected FAIL for non-numeric Expected, got {result['result']}"
    )
    assert "error" in result["details"], "Expected 'error' in details"
    assert "numeric" in result["details"]["error"].lower(), (
        f"Expected error message to mention 'numeric', got: {result['details']['error']}"
    )
    print(f"[PASS] count_equals returns FAIL for non-numeric Expected: {result['details']['error']}")


def test_classify_pattern_count_equals():
    """Test that classify_pattern returns COUNT for count_equals matcher."""
    verifier = ACVerifier("999", "file", repo_root)
    ac = ACDefinition(
        ac_number=4,
        description="Test classify_pattern for count_equals",
        ac_type="file",
        method="Glob(src/tools/python/*.py)",
        matcher="count_equals",
        expected="5"
    )

    pattern_type = verifier.classify_pattern(ac)
    assert pattern_type == PatternType.COUNT, (
        f"Expected PatternType.COUNT for count_equals, got {pattern_type}"
    )
    print("[PASS] classify_pattern returns COUNT for count_equals matcher")


if __name__ == "__main__":
    print("Running count_equals matcher tests...")
    try:
        test_classify_pattern_count_equals()
        test_count_equals_positive()
        test_count_equals_negative()
        test_count_equals_non_numeric_expected()
        print("\nAll count_equals matcher tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        sys.exit(1)
