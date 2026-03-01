#!/usr/bin/env python3
"""
Test ac-static-verifier.py handling of $ character in patterns.

This test verifies that the tool correctly handles $ characters in patterns
without interpreting them as regex anchors. The fix uses Python native search
instead of subprocess to avoid Windows command-line escaping issues.
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


def test_dollar_at_end_of_pattern():
    """Test pattern with $ at end (e.g., $"string")."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with C# interpolated string pattern
        test_file = tmpdir_path / "test.cs"
        test_file.write_text('var message = $"Hello World";\n')

        # Create AC definition with $ pattern at end
        ac = ACDefinition(
            ac_number=1,
            description="Test $ at end of pattern",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected='$"Hello World"'
        )

        # Create verifier instance
        verifier = ACVerifier("630", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS for pattern with $ at end, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Pattern with $ at end works correctly")


def test_dollar_in_middle_of_pattern():
    """Test pattern with $ in middle (e.g., $"var{id}")."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with C# interpolated string pattern with variable
        test_file = tmpdir_path / "test.cs"
        test_file.write_text('var result = $"Character{characterId.Value}";\n')

        # Create AC definition with $ pattern in middle
        ac = ACDefinition(
            ac_number=2,
            description="Test $ in middle of pattern",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected='$"Character{characterId.Value}"'
        )

        # Create verifier instance
        verifier = ACVerifier("630", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS for pattern with $ in middle, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Pattern with $ in middle works correctly")


def test_multiple_dollar_patterns():
    """Test multiple $ patterns in same file."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with multiple C# interpolated strings
        test_file = tmpdir_path / "test.cs"
        test_file.write_text('''
var first = $"First {value1}";
var second = $"Second {value2}";
var third = $"Third {value3}";
''')

        # Create AC definitions for each pattern
        test_cases = [
            ('$"First {value1}"', "first pattern"),
            ('$"Second {value2}"', "second pattern"),
            ('$"Third {value3}"', "third pattern"),
        ]

        verifier = ACVerifier("630", "code", tmpdir_path)

        for expected_pattern, description in test_cases:
            ac = ACDefinition(
                ac_number=3,
                description=f"Test {description}",
                ac_type="code",
                method=f"Grep({test_file})",
                matcher="contains",
                expected=expected_pattern
            )

            result = verifier.verify_code_ac(ac)

            assert result["result"] == "PASS", (
                f"Expected PASS for {description}, got {result['result']}: "
                f"{result.get('details', {})}"
            )

        print(f"[PASS] Multiple $ patterns in same file work correctly")


def test_dollar_pattern_not_found():
    """Test that $ pattern correctly returns FAIL when not found."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file without the pattern
        test_file = tmpdir_path / "test.cs"
        test_file.write_text('var message = "Regular string";\n')

        # Create AC definition with $ pattern that doesn't exist
        ac = ACDefinition(
            ac_number=4,
            description="Test $ pattern not found",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected='$"Interpolated"'
        )

        # Create verifier instance
        verifier = ACVerifier("630", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "FAIL", (
            f"Expected FAIL when $ pattern not found, got {result['result']}"
        )
        print(f"[PASS] $ pattern correctly returns FAIL when not found")


if __name__ == "__main__":
    print("Running $ character handling tests...")
    try:
        test_dollar_at_end_of_pattern()
        test_dollar_in_middle_of_pattern()
        test_multiple_dollar_patterns()
        test_dollar_pattern_not_found()
        print("\nAll $ character tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
