#!/usr/bin/env python3
"""
Test cases for exists matcher with Glob(pattern) in Method column

This test ensures that the ac-static-verifier correctly extracts and processes
glob patterns from the Method column when Expected is "-", while maintaining
backward compatibility with the traditional Expected column format.
"""

import sys
import importlib.util
from pathlib import Path
import tempfile
import os

# Get the repository root
repo_root = Path(__file__).resolve().parent.parent.parent.parent.parent

# Import ac-static-verifier dynamically
verifier_path = repo_root / "src" / "tools" / "python" / "ac-static-verifier.py"
spec = importlib.util.spec_from_file_location("ac_static_verifier", verifier_path)
ac_verifier_module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ac_verifier_module)

ACDefinition = ac_verifier_module.ACDefinition
ACVerifier = ac_verifier_module.ACVerifier


def test_glob_pattern_in_method_positive():
    """Test exists matcher with Glob(pattern) in Method - files exist → PASS"""
    # Test with actual repo files
    ac = ACDefinition(
        ac_number=1,
        description="Test glob pattern matches files",
        ac_type="file",
        method="Glob(src/tools/python/*.py)",
        matcher="exists",
        expected="-"
    )

    verifier = ACVerifier("626", "file", repo_root)
    result = verifier.verify_file_ac(ac)

    assert result["result"] == "PASS", (
        f"Expected PASS when glob pattern matches files. Got: {result['result']}, "
        f"Details: {result.get('details', 'N/A')}"
    )
    print(f"[PASS] test_glob_pattern_in_method_positive")


def test_glob_pattern_in_method_negative():
    """Test exists matcher with Glob(pattern) in Method - no files → FAIL"""
    # Use a pattern that definitely won't match any files
    ac = ACDefinition(
        ac_number=2,
        description="Test glob pattern matches no files",
        ac_type="file",
        method="Glob(nonexistent_dir_xyz123/*.foo)",
        matcher="exists",
        expected="-"
    )

    verifier = ACVerifier("626", "file", repo_root)
    result = verifier.verify_file_ac(ac)

    assert result["result"] == "FAIL", (
        f"Expected FAIL when glob pattern matches no files. Got: {result['result']}, "
        f"Details: {result.get('details', 'N/A')}"
    )
    print(f"[PASS] test_glob_pattern_in_method_negative")


def test_backward_compat_expected_column():
    """Test backward compatibility with Expected column"""
    # Traditional format: file path in Expected column, Method empty
    ac = ACDefinition(
        ac_number=3,
        description="Test traditional Expected column format",
        ac_type="file",
        method="",
        matcher="exists",
        expected="src/tools/python/ac-static-verifier.py"
    )

    verifier = ACVerifier("626", "file", repo_root)
    result = verifier.verify_file_ac(ac)

    assert result["result"] == "PASS", (
        f"Expected PASS for traditional Expected column format. Got: {result['result']}, "
        f"Details: {result.get('details', 'N/A')}"
    )
    print(f"[PASS] test_backward_compat_expected_column")


def test_glob_pattern_with_subdirectories():
    """Test glob pattern with subdirectory matching"""
    # Test with actual repo structure
    ac = ACDefinition(
        ac_number=4,
        description="Test glob pattern with subdirectories",
        ac_type="file",
        method="Glob(src/tools/python/tests/*.py)",
        matcher="exists",
        expected="-"
    )

    verifier = ACVerifier("626", "file", repo_root)
    result = verifier.verify_file_ac(ac)

    assert result["result"] == "PASS", (
        f"Expected PASS when glob pattern matches subdirectory files. Got: {result['result']}, "
        f"Details: {result.get('details', 'N/A')}"
    )
    print(f"[PASS] test_glob_pattern_with_subdirectories")


def test_glob_pattern_specific_file():
    """Test glob pattern matching a specific file"""
    # Test with specific file pattern
    ac = ACDefinition(
        ac_number=5,
        description="Test glob pattern for specific file",
        ac_type="file",
        method="Glob(src/tools/python/ac-static-verifier.py)",
        matcher="exists",
        expected="-"
    )

    verifier = ACVerifier("626", "file", repo_root)
    result = verifier.verify_file_ac(ac)

    assert result["result"] == "PASS", (
        f"Expected PASS when glob pattern matches specific file. Got: {result['result']}, "
        f"Details: {result.get('details', 'N/A')}"
    )
    print(f"[PASS] test_glob_pattern_specific_file")


if __name__ == "__main__":
    print("Running exists matcher Glob(pattern) tests...")
    try:
        test_glob_pattern_in_method_positive()
        print("  [PASS] test_glob_pattern_in_method_positive")
        test_glob_pattern_in_method_negative()
        print("  [PASS] test_glob_pattern_in_method_negative")
        test_backward_compat_expected_column()
        print("  [PASS] test_backward_compat_expected_column")
        test_glob_pattern_with_subdirectories()
        print("  [PASS] test_glob_pattern_with_subdirectories")
        test_glob_pattern_specific_file()
        print("  [PASS] test_glob_pattern_specific_file")
        print("All tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"[ERROR] Unexpected error: {e}", file=sys.stderr)
        sys.exit(1)
