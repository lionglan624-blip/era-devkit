#!/usr/bin/env python3
"""
Test ac-static-verifier.py UNKNOWN pattern type fallback behavior.

This test ensures that the UNKNOWN pattern type is properly returned for
unsupported matchers (e.g., equals, gt) and that verification methods
handle unknown matchers gracefully by returning FAIL with error messages.
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


def test_pattern_type_unknown_enum_exists():
    """Test that PatternType.UNKNOWN enum value exists."""
    assert hasattr(PatternType, 'UNKNOWN'), "PatternType.UNKNOWN enum value not found"
    print("[PASS] PatternType.UNKNOWN enum value exists")


def test_classify_pattern_unknown_equals():
    """Test that classify_pattern returns UNKNOWN for 'equals' matcher."""
    verifier = ACVerifier("999", "code", repo_root)
    ac = ACDefinition(
        ac_number=1,
        description="Test AC with equals matcher",
        ac_type="code",
        method="Grep(test.py)",
        matcher="equals",
        expected="test_value"
    )

    pattern_type = verifier.classify_pattern(ac)
    assert pattern_type == PatternType.UNKNOWN, (
        f"Expected PatternType.UNKNOWN for 'equals' matcher, got {pattern_type}"
    )
    print("[PASS] classify_pattern returns UNKNOWN for 'equals' matcher")


def test_classify_pattern_unknown_gt():
    """Test that classify_pattern returns COUNT for 'gt' matcher (now supported)."""
    verifier = ACVerifier("999", "code", repo_root)
    ac = ACDefinition(
        ac_number=2,
        description="Test AC with gt matcher",
        ac_type="code",
        method="Grep(test.py)",
        matcher="gt",
        expected="5"
    )

    pattern_type = verifier.classify_pattern(ac)
    assert pattern_type == PatternType.COUNT, (
        f"Expected PatternType.COUNT for 'gt' matcher, got {pattern_type}"
    )
    print("[PASS] classify_pattern returns COUNT for 'gt' matcher")


def test_classify_pattern_unknown_gte():
    """Test that classify_pattern returns COUNT for 'gte' matcher (now supported)."""
    verifier = ACVerifier("999", "code", repo_root)
    ac = ACDefinition(
        ac_number=3,
        description="Test AC with gte matcher",
        ac_type="code",
        method="Grep(test.py)",
        matcher="gte",
        expected="10"
    )

    pattern_type = verifier.classify_pattern(ac)
    assert pattern_type == PatternType.COUNT, (
        f"Expected PatternType.COUNT for 'gte' matcher, got {pattern_type}"
    )
    print("[PASS] classify_pattern returns COUNT for 'gte' matcher")


def test_verify_code_ac_unknown_matcher():
    """Test that verify_code_ac returns FAIL for unknown matcher."""
    verifier = ACVerifier("999", "code", repo_root)
    ac = ACDefinition(
        ac_number=4,
        description="Test AC with unknown matcher",
        ac_type="code",
        method="Grep(src/tools/python/ac-static-verifier.py)",
        matcher="equals",
        expected="test_value"
    )

    result = verifier.verify_code_ac(ac)

    assert result["result"] == "FAIL", (
        f"Expected result=FAIL for unknown matcher, got {result['result']}"
    )
    assert "error" in result["details"], "Expected 'error' in details"
    assert "Unknown matcher" in result["details"]["error"], (
        f"Expected 'Unknown matcher' error message, got {result['details']['error']}"
    )
    print(f"[PASS] verify_code_ac returns FAIL for unknown matcher: {result['details']['error']}")


def test_verify_build_ac_unknown_matcher():
    """Test that verify_build_ac returns FAIL for unknown matcher.

    Note: Uses a simple command that will execute successfully (echo)
    to ensure we reach the matcher validation logic.
    """
    verifier = ACVerifier("999", "build", repo_root)
    ac = ACDefinition(
        ac_number=5,
        description="Test AC with unknown matcher",
        ac_type="build",
        method="echo test",
        matcher="equals",
        expected="-"  # Use "-" to force Method column as command
    )

    result = verifier.verify_build_ac(ac)

    assert result["result"] == "FAIL", (
        f"Expected result=FAIL for unknown matcher, got {result['result']}"
    )
    assert "error" in result["details"], "Expected 'error' in details"
    assert "Unknown matcher" in result["details"]["error"], (
        f"Expected 'Unknown matcher' error message, got {result['details']['error']}"
    )
    print(f"[PASS] verify_build_ac returns FAIL for unknown matcher: {result['details']['error']}")


def test_verify_file_ac_unknown_matcher():
    """Test that verify_file_ac returns FAIL for truly unknown matcher (not count_equals which is now supported)."""
    verifier = ACVerifier("999", "file", repo_root)
    ac = ACDefinition(
        ac_number=6,
        description="Test AC with unknown matcher",
        ac_type="file",
        method="Glob(src/tools/python/*.py)",
        matcher="unsupported_matcher",  # Changed from count_equals to truly unknown matcher
        expected="5"
    )

    result = verifier.verify_file_ac(ac)

    assert result["result"] == "FAIL", (
        f"Expected result=FAIL for unknown matcher, got {result['result']}"
    )
    assert "error" in result["details"], "Expected 'error' in details"
    assert "Unknown matcher" in result["details"]["error"], (
        f"Expected 'Unknown matcher' error message, got {result['details']['error']}"
    )
    print(f"[PASS] verify_file_ac returns FAIL for unknown matcher: {result['details']['error']}")


def test_classify_pattern_known_matchers_not_unknown():
    """Test that classify_pattern does NOT return UNKNOWN for supported matchers."""
    verifier = ACVerifier("999", "code", repo_root)

    known_cases = [
        ("contains", PatternType.LITERAL),
        ("not_contains", PatternType.LITERAL),
        ("matches", PatternType.REGEX),
        ("exists", PatternType.GLOB),
        ("not_exists", PatternType.GLOB),
    ]

    for matcher, expected_type in known_cases:
        ac = ACDefinition(
            ac_number=99,
            description=f"Test {matcher}",
            ac_type="code",
            method="Grep(test.py)",
            matcher=matcher,
            expected="pattern"
        )

        pattern_type = verifier.classify_pattern(ac)
        assert pattern_type == expected_type, (
            f"Expected {expected_type} for '{matcher}', got {pattern_type}"
        )
        print(f"[PASS] classify_pattern returns {expected_type.name} for '{matcher}' matcher")


if __name__ == "__main__":
    print("Running UNKNOWN pattern type fallback tests...")
    try:
        test_pattern_type_unknown_enum_exists()
        test_classify_pattern_unknown_equals()
        test_classify_pattern_unknown_gt()
        test_classify_pattern_unknown_gte()
        test_verify_code_ac_unknown_matcher()
        test_verify_build_ac_unknown_matcher()
        test_verify_file_ac_unknown_matcher()
        test_classify_pattern_known_matchers_not_unknown()
        print("\nAll UNKNOWN pattern type fallback tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        sys.exit(1)
