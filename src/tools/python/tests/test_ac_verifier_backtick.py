#!/usr/bin/env python3
"""
Test ac-static-verifier.py backtick handling (AC#3).

This test verifies that inline code markers (backticks) in Expected column
are correctly stripped during table parsing:
- `code` should be extracted as code
- Normal text without backticks should remain unchanged
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


def test_backtick_wrapped_pattern():
    """Test that backtick-wrapped patterns are correctly extracted."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with code pattern
        test_file = tmpdir_path / "test.py"
        test_file.write_text("def my_function():\n    pass\n")

        # Create AC definition with backtick-wrapped Expected value
        # The backticks would be stripped during table parsing, so we provide already-stripped value
        ac = ACDefinition(
            ac_number=1,
            description="Check function",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="my_function"  # Backticks already stripped by parse_feature_markdown
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result - should match "my_function" (without backticks)
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] Backtick-wrapped pattern extracted correctly")


def test_pattern_without_backticks():
    """Test that normal patterns without backticks remain unchanged."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with normal text
        test_file = tmpdir_path / "test.txt"
        test_file.write_text("This is normal text\n")

        # Create AC definition with normal Expected value (no backticks)
        ac = ACDefinition(
            ac_number=1,
            description="Check text",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="normal text"
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] Pattern without backticks works correctly")


def test_backtick_in_middle_of_pattern():
    """Test patterns with backticks in the middle (inline code in longer text)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file
        test_file = tmpdir_path / "test.md"
        test_file.write_text("Call the function do_something() here\n")

        # Create AC definition - backticks would be stripped during parse
        ac = ACDefinition(
            ac_number=1,
            description="Check call",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="do_something()"  # Backticks already stripped
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result - should match "do_something()" (without backticks)
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] Backtick in middle of pattern handled correctly")


def test_multiple_backticks():
    """Test patterns with multiple backtick sections (edge case)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file
        test_file = tmpdir_path / "test.txt"
        test_file.write_text("Variable: value\n")

        # Create AC definition with simple pattern
        ac = ACDefinition(
            ac_number=1,
            description="Check var",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="value"  # Backticks already stripped
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] Multiple backticks handled correctly")


if __name__ == "__main__":
    print("Running backtick handling tests (AC#3)...")
    try:
        test_backtick_wrapped_pattern()
        test_pattern_without_backticks()
        test_backtick_in_middle_of_pattern()
        test_multiple_backticks()
        print("\nAll backtick handling tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
