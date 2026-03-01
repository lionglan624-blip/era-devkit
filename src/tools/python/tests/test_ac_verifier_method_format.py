#!/usr/bin/env python3
"""
Test ac-static-verifier.py flexible Method parsing (AC#1).

This test verifies that the tool accepts multiple Method format variations:
- Grep(path) - existing format with parentheses
- Grep path - space-separated format
- Grep( path ) - format with extra whitespace
- Invalid formats should fail with clear error messages
"""

import sys
import tempfile
from pathlib import Path

# Add parent directory to path to import ac-static-verifier
repo_root = Path(__file__).parent.parent.parent.parent.parent
sys.path.insert(0, str(repo_root / "src" / "tools" / "python"))

# Import the ACVerifier and ACDefinition classes
import importlib.util

verifier_path = repo_root / "src" / "tools" / "python" / "ac-static-verifier.py"
spec = importlib.util.spec_from_file_location("ac_static_verifier", verifier_path)
ac_verifier_module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ac_verifier_module)

ACVerifier = ac_verifier_module.ACVerifier
ACDefinition = ac_verifier_module.ACDefinition


def test_method_format_with_parens():
    """Test existing Grep(path) format continues to work."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with target pattern
        test_file = tmpdir_path / "test.py"
        test_file.write_text("def search_pattern():\n    pass\n")

        # Create AC definition with Grep(path) format
        ac = ACDefinition(
            ac_number=1,
            description="Test pattern",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="search_pattern"
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] Grep(path) format works correctly")


def test_method_format_space_separated():
    """Test new Grep path format (space-separated, no parens)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with target pattern
        test_file = tmpdir_path / "test.py"
        test_file.write_text("def search_pattern():\n    pass\n")

        # Create AC definition with Grep path format (no parens)
        ac = ACDefinition(
            ac_number=1,
            description="Test pattern",
            ac_type="code",
            method=f"Grep {test_file}",
            matcher="contains",
            expected="search_pattern"
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] Grep path format (space-separated) works correctly")


def test_method_format_with_whitespace():
    """Test Grep( path ) format with extra whitespace."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with target pattern
        test_file = tmpdir_path / "test.py"
        test_file.write_text("def search_pattern():\n    pass\n")

        # Create AC definition with Grep( path ) format (extra spaces)
        ac = ACDefinition(
            ac_number=1,
            description="Test pattern",
            ac_type="code",
            method=f"Grep( {test_file} )",
            matcher="contains",
            expected="search_pattern"
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] Grep( path ) format with whitespace works correctly")


def test_method_format_invalid():
    """Test that invalid Method format fails with clear error."""
    # Create AC definition with invalid Method format
    ac = ACDefinition(
        ac_number=1,
        description="Test pattern",
        ac_type="code",
        method="GrepInvalid",
        matcher="contains",
        expected="pattern"
    )

    # Create verifier instance
    verifier = ACVerifier("621", "code", repo_root)

    # Verify AC
    result = verifier.verify_code_ac(ac)

    # Verify result shows failure with clear error
    assert result["result"] == "FAIL", f"Expected FAIL, got {result['result']}"

    # Check error message mentions expected formats
    error_msg = result["details"]["error"]
    assert "Invalid Method format" in error_msg or "expected" in error_msg.lower(), \
        f"Error message should mention invalid format, got: {error_msg}"

    print(f"[PASS] Invalid Method format fails with clear error: {error_msg}")


if __name__ == "__main__":
    print("Running Method format parsing tests (AC#1)...")
    try:
        test_method_format_with_parens()
        test_method_format_space_separated()
        test_method_format_with_whitespace()
        test_method_format_invalid()
        print("\nAll Method format tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
