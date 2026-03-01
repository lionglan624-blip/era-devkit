#!/usr/bin/env python3
"""
Test ac-static-verifier.py comma-separated glob patterns (AC#1-3).

This test verifies that comma-separated glob patterns are correctly split
and expanded to match files from multiple patterns.
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


def test_comma_glob_split():
    """Test that comma-separated patterns are correctly split (AC#1 unit test)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test files matching different patterns
        for i in range(542, 550):
            (tmpdir_path / f"feature-{i}.md").write_text(f"Feature {i}\n")
        for i in range(550, 554):
            (tmpdir_path / f"feature-{i}.md").write_text(f"Feature {i}\n")

        # Create verifier instance
        verifier = ACVerifier("631", "file", tmpdir_path)

        # Test comma-separated pattern expansion
        # Pattern: "feature-54[2-9].md,feature-55[0-3].md"
        success, error_msg, matches = verifier._expand_glob_path("feature-54[2-9].md,feature-55[0-3].md")

        # Verify successful expansion
        assert success, f"Expected success, got error: {error_msg}"
        assert error_msg is None, f"Expected no error, got: {error_msg}"

        # Verify we got files from both patterns
        match_names = sorted([m.name for m in matches])
        expected_first = [f"feature-{i}.md" for i in range(542, 550)]
        expected_second = [f"feature-{i}.md" for i in range(550, 554)]
        expected_all = sorted(expected_first + expected_second)

        assert match_names == expected_all, (
            f"Expected {len(expected_all)} files from both patterns, "
            f"got {len(match_names)}: {match_names}"
        )
        print(f"[PASS] Comma glob split correctly: {len(match_names)} files from 2 patterns")


def test_comma_glob_first_pattern():
    """Test first pattern in comma glob matches correctly (AC#2)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test files matching first pattern only
        for i in range(542, 550):
            (tmpdir_path / f"feature-{i}.md").write_text(f"Feature {i}\n")

        # Create AC definition with comma-separated glob in Expected
        ac = ACDefinition(
            ac_number=1,
            description="Test first pattern matches",
            ac_type="file",
            method="",
            matcher="exists",
            expected="feature-54[2-9].md,feature-55[0-3].md"
        )

        # Create verifier instance
        verifier = ACVerifier("631", "file", tmpdir_path)

        # Verify AC
        result = verifier.verify_file_ac(ac)

        # Verify result - should PASS because first pattern matches
        assert result["result"] == "PASS", (
            f"Expected PASS when first pattern matches, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] First pattern in comma glob matches correctly")


def test_comma_glob_second_pattern():
    """Test second pattern in comma glob matches correctly (AC#3)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test files matching second pattern only
        for i in range(550, 554):
            (tmpdir_path / f"feature-{i}.md").write_text(f"Feature {i}\n")

        # Create AC definition with comma-separated glob in Expected
        ac = ACDefinition(
            ac_number=2,
            description="Test second pattern matches",
            ac_type="file",
            method="",
            matcher="exists",
            expected="feature-54[2-9].md,feature-55[0-3].md"
        )

        # Create verifier instance
        verifier = ACVerifier("631", "file", tmpdir_path)

        # Verify AC
        result = verifier.verify_file_ac(ac)

        # Verify result - should PASS because second pattern matches
        assert result["result"] == "PASS", (
            f"Expected PASS when second pattern matches, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Second pattern in comma glob matches correctly")


def test_comma_glob_both_patterns():
    """Test both patterns in comma glob match correctly (integration)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test files matching both patterns
        for i in range(542, 550):
            (tmpdir_path / f"feature-{i}.md").write_text(f"Feature {i}\n")
        for i in range(550, 554):
            (tmpdir_path / f"feature-{i}.md").write_text(f"Feature {i}\n")

        # Create AC definition with comma-separated glob in Expected
        ac = ACDefinition(
            ac_number=3,
            description="Test both patterns match",
            ac_type="file",
            method="",
            matcher="exists",
            expected="feature-54[2-9].md,feature-55[0-3].md"
        )

        # Create verifier instance
        verifier = ACVerifier("631", "file", tmpdir_path)

        # Verify AC
        result = verifier.verify_file_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS when both patterns match, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Both patterns in comma glob match correctly")


def test_single_pattern_without_comma():
    """Test backward compatibility - single pattern without comma (AC#1 edge case)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test files matching single pattern
        for i in range(542, 550):
            (tmpdir_path / f"feature-{i}.md").write_text(f"Feature {i}\n")

        # Create AC definition with single pattern (no comma)
        ac = ACDefinition(
            ac_number=4,
            description="Test single pattern without comma",
            ac_type="file",
            method="",
            matcher="exists",
            expected="feature-54[2-9].md"
        )

        # Create verifier instance
        verifier = ACVerifier("631", "file", tmpdir_path)

        # Verify AC
        result = verifier.verify_file_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS for single pattern (backward compat), got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Single pattern without comma works (backward compatibility)")


def test_comma_glob_no_match():
    """Test comma glob returns FAIL when no patterns match."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Don't create any matching files

        # Create AC definition with comma-separated glob that won't match
        ac = ACDefinition(
            ac_number=5,
            description="Test no patterns match",
            ac_type="file",
            method="",
            matcher="exists",
            expected="feature-54[2-9].md,feature-55[0-3].md"
        )

        # Create verifier instance
        verifier = ACVerifier("631", "file", tmpdir_path)

        # Verify AC
        result = verifier.verify_file_ac(ac)

        # Verify result - should FAIL when no patterns match
        assert result["result"] == "FAIL", (
            f"Expected FAIL when no patterns match, got {result['result']}"
        )
        print(f"[PASS] Comma glob correctly returns FAIL when no patterns match")


if __name__ == "__main__":
    print("Running comma-separated glob pattern tests (AC#1-3)...")
    try:
        test_comma_glob_split()
        test_comma_glob_first_pattern()
        test_comma_glob_second_pattern()
        test_comma_glob_both_patterns()
        test_single_pattern_without_comma()
        test_comma_glob_no_match()
        print("\nAll comma glob tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
