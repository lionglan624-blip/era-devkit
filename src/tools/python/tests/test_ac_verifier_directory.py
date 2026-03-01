#!/usr/bin/env python3
"""
Test ac-static-verifier.py directory path handling.

This test verifies that the tool correctly handles directory paths:
- Directory paths expand to list of files, not the directory itself
- Nested directory structures are traversed properly
- Only files (not directories) are included in results
- Empty directories return empty list without error
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


def test_directory_path_returns_files():
    """Test case 1: Directory path expands to files, not directory itself."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmppath = Path(tmpdir)

        # Create test files in directory
        test_dir = tmppath / "testdir"
        test_dir.mkdir()
        (test_dir / "file1.py").write_text("# test file 1")
        (test_dir / "file2.py").write_text("# test file 2")

        # Create ACVerifier instance with temp directory as repo_root
        verifier = ACVerifier("699", "code", tmppath)

        # Test directory path expansion
        success, error, matched_files = verifier._expand_glob_path("testdir/")

        # Verify success
        assert success, f"Expansion failed: {error}"
        assert error is None, f"Unexpected error: {error}"

        # Verify files were returned (not the directory itself)
        assert len(matched_files) > 0, "Expected files but got empty list"

        # Verify all results are files, not directories
        for path in matched_files:
            assert path.is_file(), f"Expected file but got directory or non-file: {path}"

        # Verify we got the expected files
        file_names = {p.name for p in matched_files}
        assert "file1.py" in file_names, "file1.py not found in results"
        assert "file2.py" in file_names, "file2.py not found in results"


def test_directory_with_nested_subdirs():
    """Test case 2: Nested directory structure finds all files."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmppath = Path(tmpdir)

        # Create nested directory structure
        root = tmppath / "root"
        root.mkdir()
        (root / "file1.py").write_text("# file 1")

        subdir = root / "subdir"
        subdir.mkdir()
        (subdir / "file2.py").write_text("# file 2")

        nested = subdir / "nested"
        nested.mkdir()
        (nested / "file3.py").write_text("# file 3")

        # Create ACVerifier instance
        verifier = ACVerifier("699", "code", tmppath)

        # Test nested directory expansion
        success, error, matched_files = verifier._expand_glob_path("root/")

        # Verify success
        assert success, f"Expansion failed: {error}"
        assert error is None, f"Unexpected error: {error}"

        # Verify all files at all nesting levels are found
        file_names = {p.name for p in matched_files}
        assert "file1.py" in file_names, "file1.py not found (root level)"
        assert "file2.py" in file_names, "file2.py not found (subdir level)"
        assert "file3.py" in file_names, "file3.py not found (nested level)"

        # Verify we got exactly 3 files
        assert len(matched_files) == 3, f"Expected 3 files but got {len(matched_files)}"


def test_files_only_no_directories():
    """Test case 3: Subdirectories are excluded from results."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmppath = Path(tmpdir)

        # Create directory with both files and subdirectories
        test_dir = tmppath / "directory"
        test_dir.mkdir()
        (test_dir / "file1.py").write_text("# file 1")
        (test_dir / "file2.txt").write_text("test")

        subdir1 = test_dir / "subdir1"
        subdir1.mkdir()
        (subdir1 / "file3.py").write_text("# file 3")

        subdir2 = test_dir / "subdir2"
        subdir2.mkdir()

        # Create ACVerifier instance
        verifier = ACVerifier("699", "code", tmppath)

        # Test directory expansion
        success, error, matched_files = verifier._expand_glob_path("directory/")

        # Verify success
        assert success, f"Expansion failed: {error}"
        assert error is None, f"Unexpected error: {error}"

        # Verify only files are returned (no directory paths)
        for path in matched_files:
            assert path.is_file(), f"Expected only files, but got non-file: {path}"
            # Additional check: path should not be a directory
            assert not path.is_dir(), f"Directory path found in results: {path}"

        # Verify files from all levels are included
        assert len(matched_files) >= 3, f"Expected at least 3 files, got {len(matched_files)}"


def test_empty_directory_returns_empty_list():
    """Test case 4: Empty directory returns [] without error."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmppath = Path(tmpdir)

        # Create empty directory
        empty_dir = tmppath / "empty_dir"
        empty_dir.mkdir()

        # Create ACVerifier instance
        verifier = ACVerifier("699", "code", tmppath)

        # Test empty directory expansion
        success, error, matched_files = verifier._expand_glob_path("empty_dir/")

        # Verify success (empty directory is not an error)
        assert success, f"Empty directory should succeed, but got error: {error}"
        assert error is None, f"Empty directory should have no error, but got: {error}"

        # Verify empty list is returned
        assert matched_files == [], f"Expected empty list, got: {matched_files}"
        assert len(matched_files) == 0, "Expected 0 files in empty directory"


if __name__ == "__main__":
    print("Running directory path handling tests...")
    try:
        test_directory_path_returns_files()
        test_directory_with_nested_subdirs()
        test_files_only_no_directories()
        test_empty_directory_returns_empty_list()
        print("\nAll directory path handling tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
