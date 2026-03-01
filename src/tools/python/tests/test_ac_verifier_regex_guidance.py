#!/usr/bin/env python3
"""
Test ac-static-verifier.py regex detection with guidance (AC#4).

This test verifies that the tool detects regex metacharacters in 'contains'
matcher and provides clear guidance to use 'matches' instead:
- Patterns like .* should fail with helpful error
- Literal text should pass
- Single metacharacters common in code (like { or }) should not trigger the error
- Using 'matches' matcher for regex should work correctly
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


def test_regex_pattern_in_contains_fails():
    """Test that regex patterns in 'contains' matcher fail with guidance."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file
        test_file = tmpdir_path / "test.md"
        test_file.write_text("DEPRECATED: Use /fc command instead\n")

        # Create AC definition with regex pattern in 'contains' matcher
        ac = ACDefinition(
            ac_number=1,
            description="Check deprecation",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="DEPRECATED.*Use /fc"  # Regex pattern (should fail)
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result shows failure with guidance
        assert result["result"] == "FAIL", f"Expected FAIL, got {result['result']}"

        # Check error message contains guidance
        error_msg = result["details"].get("error", "")
        guidance_msg = result["details"].get("guidance", "")

        # Error should mention regex patterns and suggest 'matches'
        assert "regex" in error_msg.lower() or "matches" in error_msg.lower(), \
            f"Error should mention regex/matches, got: {error_msg}"
        assert "matches" in guidance_msg.lower() or "matches" in error_msg.lower(), \
            f"Should suggest 'matches' matcher, got error: {error_msg}, guidance: {guidance_msg}"

        print(f"[PASS] Regex pattern in 'contains' fails with guidance:")
        print(f"  Error: {error_msg}")
        print(f"  Guidance: {guidance_msg}")


def test_literal_text_in_contains_passes():
    """Test that literal text in 'contains' matcher passes."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file
        test_file = tmpdir_path / "test.txt"
        test_file.write_text("This is literal text\n")

        # Create AC definition with literal pattern in 'contains' matcher
        ac = ACDefinition(
            ac_number=1,
            description="Check text",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="literal text"
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] Literal text in 'contains' passes correctly")


def test_single_metachar_in_contains_passes():
    """Test that single metacharacters common in code don't trigger false positives."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with JSON structure
        test_file = tmpdir_path / "test.json"
        test_file.write_text('{"summary": {\n  "total": 5\n}}\n')

        # Create AC definition checking for literal { in JSON
        ac = ACDefinition(
            ac_number=1,
            description="Check JSON",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected='"summary": {'
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result - should pass (single { is common in JSON, not flagged as regex)
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] Single metacharacter in 'contains' passes (no false positive)")


def test_regex_in_matches_passes():
    """Test that regex patterns work correctly with 'matches' matcher."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file
        test_file = tmpdir_path / "test.md"
        test_file.write_text("DEPRECATED: Use /fc command\n")

        # Create AC definition with regex pattern in 'matches' matcher (correct usage)
        ac = ACDefinition(
            ac_number=1,
            description="Check deprecation",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="matches",
            expected="DEPRECATED.*Use /fc"  # Regex pattern with 'matches'
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result - should pass with 'matches' matcher
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] Regex pattern with 'matches' matcher passes correctly")


def test_character_class_in_contains_fails():
    """Test that character classes in 'contains' fail with guidance."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file
        test_file = tmpdir_path / "test.txt"
        test_file.write_text("value123\n")

        # Create AC definition with character class in 'contains'
        ac = ACDefinition(
            ac_number=1,
            description="Check number",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="value[0-9]+"  # Character class (should fail)
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result shows failure
        assert result["result"] == "FAIL", f"Expected FAIL, got {result['result']}"
        print(f"[PASS] Character class in 'contains' fails with guidance")


if __name__ == "__main__":
    print("Running regex detection with guidance tests (AC#4)...")
    try:
        test_regex_pattern_in_contains_fails()
        test_literal_text_in_contains_passes()
        test_single_metachar_in_contains_passes()
        test_regex_in_matches_passes()
        test_character_class_in_contains_fails()
        print("\nAll regex guidance tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
