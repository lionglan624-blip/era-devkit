#!/usr/bin/env python3
"""
Test ac-static-verifier.py not_matches matcher (AC#1, AC#2, AC#11).

This test verifies that the tool correctly handles the not_matches matcher:
- PASS when regex pattern is NOT found in target file (AC#1)
- FAIL when regex pattern IS found in target file (AC#2)
- FAIL with error message when invalid regex pattern (AC#11)
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


def test_not_matches_pass_when_pattern_absent():
    """Test that not_matches returns PASS when pattern is NOT found (AC#1)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file WITHOUT TODO/FIXME/HACK
        test_file = tmpdir_path / "clean_code.py"
        test_file.write_text("def calculate_sum(a, b):\n    return a + b\n")

        # Create AC definition with not_matches matcher
        ac = ACDefinition(
            ac_number=1,
            description="Verify no technical debt markers",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="not_matches",
            expected="TODO|FIXME|HACK"  # Pattern NOT in file
        )

        # Create verifier instance
        verifier = ACVerifier("704", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] not_matches returns PASS when pattern absent")


def test_not_matches_fail_when_pattern_present():
    """Test that not_matches returns FAIL when pattern IS found (AC#2)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file WITH TODO marker
        test_file = tmpdir_path / "dirty_code.py"
        test_file.write_text("def calculate_sum(a, b):\n    # TODO: fix later\n    return a + b\n")

        # Create AC definition with not_matches matcher
        ac = ACDefinition(
            ac_number=2,
            description="Verify no technical debt markers",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="not_matches",
            expected="TODO|FIXME|HACK"  # Pattern IS in file
        )

        # Create verifier instance
        verifier = ACVerifier("704", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result shows failure
        assert result["result"] == "FAIL", f"Expected FAIL, got {result['result']}"
        print(f"[PASS] not_matches returns FAIL when pattern present")


def test_not_matches_invalid_regex_returns_fail():
    """Test that invalid regex in not_matches returns FAIL with error message (AC#11)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file
        test_file = tmpdir_path / "test.py"
        test_file.write_text("def example():\n    pass\n")

        # Create AC definition with INVALID regex pattern
        ac = ACDefinition(
            ac_number=11,
            description="Test invalid regex handling",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="not_matches",
            expected="[unclosed"  # Invalid regex (unclosed bracket)
        )

        # Create verifier instance
        verifier = ACVerifier("704", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result shows failure with error message
        assert result["result"] == "FAIL", f"Expected FAIL for invalid regex, got {result['result']}"

        # Check error message contains indication of invalid regex
        error_msg = result["details"].get("error", "")
        assert "regex" in error_msg.lower() or "pattern" in error_msg.lower(), \
            f"Error should mention regex/pattern issue, got: {error_msg}"

        print(f"[PASS] not_matches returns FAIL with error message for invalid regex:")
        print(f"  Error: {error_msg}")


if __name__ == "__main__":
    print("Running not_matches matcher tests (AC#1, AC#2, AC#11)...")
    try:
        test_not_matches_pass_when_pattern_absent()
        test_not_matches_fail_when_pattern_present()
        test_not_matches_invalid_regex_returns_fail()
        print("\nAll not_matches tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
