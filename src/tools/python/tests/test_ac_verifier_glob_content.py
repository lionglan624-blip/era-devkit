#!/usr/bin/env python3
"""
Test ac-static-verifier.py glob pattern expansion for content search.

This test verifies that the tool correctly expands glob patterns in file paths
for code and file type verification with contains/matches matchers, and searches
all matched files when multiple files match the pattern.
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


def test_single_glob_match_pattern_found():
    """Test case 1: Single glob match with pattern found (AC#2)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create subdirectory and test file
        subdir = tmpdir_path / "Characters"
        subdir.mkdir()
        test_file = subdir / "Character.cs"
        test_file.write_text('public class Character { private string name; }\n')

        # Create AC definition with glob pattern
        ac = ACDefinition(
            ac_number=2,
            description="Test glob pattern expands and finds content",
            ac_type="code",
            method=f"Grep({tmpdir_path}/Characters/*.cs)",
            matcher="contains",
            expected="private string name"
        )

        # Create verifier instance
        verifier = ACVerifier("630", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS when glob matches single file with pattern, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Single glob match with pattern found works correctly")


def test_multiple_glob_matches_pattern_in_one():
    """Test case 2: Multiple glob matches with pattern in one file (AC#3)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create subdirectory with multiple test files
        subdir = tmpdir_path / "Characters"
        subdir.mkdir()

        # Create first file WITH the pattern
        file1 = subdir / "Character1.cs"
        file1.write_text('public class Character1 { private ICharacterDataService service; }\n')

        # Create second file WITHOUT the pattern
        file2 = subdir / "Character2.cs"
        file2.write_text('public class Character2 { private string name; }\n')

        # Create third file WITHOUT the pattern
        file3 = subdir / "Character3.cs"
        file3.write_text('public class Character3 { private int id; }\n')

        # Create AC definition with glob pattern
        ac = ACDefinition(
            ac_number=3,
            description="Test glob matches multiple files, pattern in one",
            ac_type="code",
            method=f"Grep({tmpdir_path}/Characters/*.cs)",
            matcher="contains",
            expected="ICharacterDataService"
        )

        # Create verifier instance
        verifier = ACVerifier("630", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result - should PASS because pattern found in at least one file
        assert result["result"] == "PASS", (
            f"Expected PASS when pattern found in ANY matched file, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Multiple glob matches with pattern in one file works correctly")


def test_literal_path_without_glob():
    """Test case 3: Literal path without glob characters still works (AC#8)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with literal path (no glob characters)
        test_file = tmpdir_path / "test.py"
        test_file.write_text('def test_function():\n    pass\n')

        # Create AC definition with literal path (no glob)
        ac = ACDefinition(
            ac_number=8,
            description="Test literal path without glob chars",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="def test_function"
        )

        # Create verifier instance
        verifier = ACVerifier("630", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS for literal path (backward compatibility), got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Literal path without glob characters works correctly")


def test_glob_no_matches_error():
    """Test case 4: Glob pattern with no matches returns appropriate error."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create AC definition with glob pattern that matches no files
        ac = ACDefinition(
            ac_number=4,
            description="Test glob pattern with no matches",
            ac_type="code",
            method=f"Grep({tmpdir_path}/NonexistentDir/*.cs)",
            matcher="contains",
            expected="some pattern"
        )

        # Create verifier instance
        verifier = ACVerifier("630", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result - should FAIL with appropriate error
        assert result["result"] == "FAIL", (
            f"Expected FAIL when glob matches no files, got {result['result']}"
        )

        # Check error message mentions glob pattern
        error_msg = result.get("details", {}).get("error", "")
        assert "glob" in error_msg.lower() or "match" in error_msg.lower(), (
            f"Error message should mention glob/match, got: {error_msg}"
        )

        print(f"[PASS] Glob pattern with no matches returns appropriate error")


def test_glob_with_subdirectories():
    """Test glob pattern with subdirectory structure."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create nested directory structure
        subdir1 = tmpdir_path / "src" / "Models"
        subdir1.mkdir(parents=True)

        # Create test files in subdirectory
        file1 = subdir1 / "Character.cs"
        file1.write_text('public class Character { }\n')

        # Create AC definition with glob pattern
        ac = ACDefinition(
            ac_number=5,
            description="Test glob with subdirectories",
            ac_type="code",
            method=f"Grep({tmpdir_path}/src/Models/*.cs)",
            matcher="contains",
            expected="public class Character"
        )

        # Create verifier instance
        verifier = ACVerifier("630", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS for glob with subdirectories, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Glob pattern with subdirectories works correctly")


def test_file_type_with_glob():
    """Test file type verification with glob pattern."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create subdirectory and test file
        subdir = tmpdir_path / "config"
        subdir.mkdir()
        test_file = subdir / "settings.yaml"
        test_file.write_text('environment: production\n')

        # Create AC definition for file type with glob
        ac = ACDefinition(
            ac_number=6,
            description="Test file type with glob pattern",
            ac_type="file",
            method=f"Grep({tmpdir_path}/config/*.yaml)",
            matcher="contains",
            expected="environment: production"
        )

        # Create verifier instance
        verifier = ACVerifier("630", "file", tmpdir_path)

        # Verify AC - this uses _verify_file_content internally
        result = verifier.verify_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS for file type with glob, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] File type verification with glob pattern works correctly")


if __name__ == "__main__":
    print("Running glob pattern expansion tests...")
    try:
        test_single_glob_match_pattern_found()
        test_multiple_glob_matches_pattern_in_one()
        test_literal_path_without_glob()
        test_glob_no_matches_error()
        test_glob_with_subdirectories()
        test_file_type_with_glob()
        print("\nAll glob pattern expansion tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
