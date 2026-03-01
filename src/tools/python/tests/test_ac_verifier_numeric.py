#!/usr/bin/env python3
"""
Test ac-static-verifier.py numeric comparison matchers (gt, gte, lt, lte).

This test verifies that numeric comparison matchers correctly count files
and perform comparisons against expected thresholds.
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


def test_gt_positive():
    """Test that gt returns PASS when actual > expected."""
    verifier = ACVerifier("999", "file", repo_root)

    ac = ACDefinition(
        ac_number=1,
        description="Test gt positive case",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="gt",
        expected="0"  # Should be > 0 since test files exist
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "PASS", (
        f"Expected PASS when actual > expected, got {result['result']}"
    )
    assert result["details"]["actual_count"] > 0, (
        "Expected actual_count > 0"
    )
    print(f"[PASS] gt returns PASS when actual > expected (actual={result['details']['actual_count']}, expected=0)")


def test_gt_negative():
    """Test that gt returns FAIL when actual <= expected."""
    verifier = ACVerifier("999", "file", repo_root)

    ac = ACDefinition(
        ac_number=2,
        description="Test gt negative case",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="gt",
        expected="999"  # Impossible threshold
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "FAIL", (
        f"Expected FAIL when actual <= expected, got {result['result']}"
    )
    assert result["details"]["actual_count"] <= 999, (
        "Expected actual_count <= 999"
    )
    print(f"[PASS] gt returns FAIL when actual <= expected (actual={result['details']['actual_count']}, expected=999)")


def test_gte_positive():
    """Test that gte returns PASS when actual >= expected."""
    verifier = ACVerifier("999", "file", repo_root)

    # Count actual files to set threshold at exact count (test equality edge case)
    test_dir = repo_root / "src" / "tools" / "python" / "tests"
    actual_count = len(list(test_dir.glob("*.py")))

    ac = ACDefinition(
        ac_number=3,
        description="Test gte positive case",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="gte",
        expected=str(actual_count)  # Should be >= actual_count (equality)
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "PASS", (
        f"Expected PASS when actual >= expected, got {result['result']}"
    )
    assert result["details"]["actual_count"] >= actual_count, (
        f"Expected actual_count >= {actual_count}"
    )
    print(f"[PASS] gte returns PASS when actual >= expected (actual={result['details']['actual_count']}, expected={actual_count})")


def test_gte_negative():
    """Test that gte returns FAIL when actual < expected."""
    verifier = ACVerifier("999", "file", repo_root)

    ac = ACDefinition(
        ac_number=4,
        description="Test gte negative case",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="gte",
        expected="999"  # Impossible threshold
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "FAIL", (
        f"Expected FAIL when actual < expected, got {result['result']}"
    )
    assert result["details"]["actual_count"] < 999, (
        "Expected actual_count < 999"
    )
    print(f"[PASS] gte returns FAIL when actual < expected (actual={result['details']['actual_count']}, expected=999)")


def test_lt_positive():
    """Test that lt returns PASS when actual < expected."""
    verifier = ACVerifier("999", "file", repo_root)

    ac = ACDefinition(
        ac_number=5,
        description="Test lt positive case",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="lt",
        expected="999"  # Should be < 999
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "PASS", (
        f"Expected PASS when actual < expected, got {result['result']}"
    )
    assert result["details"]["actual_count"] < 999, (
        "Expected actual_count < 999"
    )
    print(f"[PASS] lt returns PASS when actual < expected (actual={result['details']['actual_count']}, expected=999)")


def test_lt_negative():
    """Test that lt returns FAIL when actual >= expected."""
    verifier = ACVerifier("999", "file", repo_root)

    ac = ACDefinition(
        ac_number=6,
        description="Test lt negative case",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="lt",
        expected="0"  # Files exist, so actual >= 0
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "FAIL", (
        f"Expected FAIL when actual >= expected, got {result['result']}"
    )
    assert result["details"]["actual_count"] >= 0, (
        "Expected actual_count >= 0"
    )
    print(f"[PASS] lt returns FAIL when actual >= expected (actual={result['details']['actual_count']}, expected=0)")


def test_lte_positive():
    """Test that lte returns PASS when actual <= expected."""
    verifier = ACVerifier("999", "file", repo_root)

    # Count actual files to set threshold at exact count (test equality edge case)
    test_dir = repo_root / "src" / "tools" / "python" / "tests"
    actual_count = len(list(test_dir.glob("*.py")))

    ac = ACDefinition(
        ac_number=7,
        description="Test lte positive case",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="lte",
        expected=str(actual_count)  # Should be <= actual_count (equality)
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "PASS", (
        f"Expected PASS when actual <= expected, got {result['result']}"
    )
    assert result["details"]["actual_count"] <= actual_count, (
        f"Expected actual_count <= {actual_count}"
    )
    print(f"[PASS] lte returns PASS when actual <= expected (actual={result['details']['actual_count']}, expected={actual_count})")


def test_lte_negative():
    """Test that lte returns FAIL when actual > expected."""
    verifier = ACVerifier("999", "file", repo_root)

    ac = ACDefinition(
        ac_number=8,
        description="Test lte negative case",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="lte",
        expected="0"  # Files exist, so actual > 0
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "FAIL", (
        f"Expected FAIL when actual > expected, got {result['result']}"
    )
    assert result["details"]["actual_count"] > 0, (
        "Expected actual_count > 0"
    )
    print(f"[PASS] lte returns FAIL when actual > expected (actual={result['details']['actual_count']}, expected=0)")


def test_numeric_non_numeric_expected():
    """Test that numeric matchers return FAIL with error when Expected is non-numeric."""
    verifier = ACVerifier("999", "file", repo_root)

    # Test with gt matcher (same logic applies to all numeric matchers)
    ac = ACDefinition(
        ac_number=9,
        description="Test numeric matcher with non-numeric Expected",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="gt",
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
    print(f"[PASS] Numeric matcher returns FAIL for non-numeric Expected: {result['details']['error']}")


def test_classify_pattern_numeric_matchers():
    """Test that classify_pattern returns COUNT for all numeric matchers."""
    verifier = ACVerifier("999", "file", repo_root)

    for matcher in ["gt", "gte", "lt", "lte"]:
        ac = ACDefinition(
            ac_number=10,
            description=f"Test classify_pattern for {matcher}",
            ac_type="file",
            method="Glob(src/tools/python/*.py)",
            matcher=matcher,
            expected="5"
        )

        pattern_type = verifier.classify_pattern(ac)
        assert pattern_type == PatternType.COUNT, (
            f"Expected PatternType.COUNT for {matcher}, got {pattern_type}"
        )
        print(f"[PASS] classify_pattern returns COUNT for {matcher} matcher")


if __name__ == "__main__":
    print("Running numeric comparison matcher tests...")
    try:
        test_classify_pattern_numeric_matchers()
        test_gt_positive()
        test_gt_negative()
        test_gte_positive()
        test_gte_negative()
        test_lt_positive()
        test_lt_negative()
        test_lte_positive()
        test_lte_negative()
        test_numeric_non_numeric_expected()
        print("\nAll numeric comparison matcher tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        sys.exit(1)
