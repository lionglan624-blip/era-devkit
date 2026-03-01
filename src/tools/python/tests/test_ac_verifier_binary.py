#!/usr/bin/env python3
"""
Test ac-static-verifier.py binary file handling.

This test verifies that the tool correctly handles binary files:
- Binary files are filtered out during directory enumeration
- Binary files don't crash the verifier with UnicodeDecodeError
- Skipped files are logged to stderr with warnings
- Integration test with actual Era.Core/ directory
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
AcStaticVerifier = ac_verifier_module.ACVerifier  # Alias for consistency


def test_eracore_directory_no_crash():
    """Test case 1: Integration test with actual Era.Core/ directory.

    Verifies that the verifier can scan Era.Core/ directory without crashing,
    even though it contains obj/ directories with binary files (.dll, .cache, .pdb).
    """
    # Verify Era.Core directory exists
    eracore_dir = repo_root / "Era.Core"
    if not eracore_dir.exists():
        print("SKIP: Era.Core/ directory not found, skipping integration test", file=sys.stderr)
        return

    # Create ACVerifier instance
    verifier = ACVerifier("702", "code", repo_root)

    # Test directory expansion (this is the operation that failed in F683)
    success, error, matched_files = verifier._expand_glob_path("Era.Core/")

    # Verify success (no crash)
    assert success, f"Era.Core/ directory scan failed: {error}"
    assert error is None, f"Unexpected error during Era.Core/ scan: {error}"

    # Verify we got some files (directory is not empty)
    # Note: Some files may be skipped as binaries, but there should be .cs files
    assert len(matched_files) > 0, "Era.Core/ directory should contain at least some text files"

    # Verify all results are files (not directories)
    for path in matched_files:
        assert path.is_file(), f"Expected file but got non-file: {path}"


def test_obj_directory_binaries_skipped():
    """Test case 2: Temp directory with obj/ containing binaries is handled gracefully.

    Creates a realistic build output directory structure with obj/ subdirectory
    containing .dll, .cache, and .pdb files. Verifies that verifier processes
    directory without crash and skips binary files.
    """
    with tempfile.TemporaryDirectory() as tmpdir:
        tmppath = Path(tmpdir)

        # Create realistic project directory structure
        project_dir = tmppath / "TestProject"
        project_dir.mkdir()

        # Add some source files
        (project_dir / "Program.cs").write_text("// C# source file\nusing System;")
        (project_dir / "Helper.cs").write_text("// Helper class")

        # Create obj/ directory with binary files
        obj_dir = project_dir / "obj" / "Release" / "net10.0"
        obj_dir.mkdir(parents=True)

        # Create binary files with actual binary content
        (obj_dir / "TestProject.dll").write_bytes(b'\x4d\x5a\x90\x00')  # MZ header (PE executable)
        (obj_dir / "TestProject.pdb").write_bytes(b'\x4d\x53\x46\x00')  # MSF header (PDB file)
        (obj_dir / "TestProject.assets.cache").write_bytes(b'PKGA\x00\x00\x00\x00')  # PKGA header

        # Create ACVerifier instance
        verifier = ACVerifier("702", "code", tmppath)

        # Test directory expansion
        success, error, matched_files = verifier._expand_glob_path("TestProject/")

        # Verify success (no crash on binary files)
        assert success, f"Directory scan with binaries failed: {error}"
        assert error is None, f"Unexpected error: {error}"

        # Verify only text files are returned (binary files filtered out)
        file_names = {p.name for p in matched_files}
        assert "Program.cs" in file_names, "Program.cs should be included"
        assert "Helper.cs" in file_names, "Helper.cs should be included"
        assert "TestProject.dll" not in file_names, "DLL should be filtered out"
        assert "TestProject.pdb" not in file_names, "PDB should be filtered out"
        assert "TestProject.assets.cache" not in file_names, "Cache file should be filtered out"

        # Verify file count (should only have 2 .cs files)
        assert len(matched_files) == 2, f"Expected 2 text files, got {len(matched_files)}"


def test_binary_files_filtered_by_extension():
    """Test case 3: Direct unit test of _expand_glob_path with mixed files.

    Verifies that _expand_glob_path correctly filters out binary files by extension
    before they reach the file reading stage.
    """
    with tempfile.TemporaryDirectory() as tmpdir:
        tmppath = Path(tmpdir)

        # Create directory with mixed file types
        test_dir = tmppath / "mixed"
        test_dir.mkdir()

        # Text files (should be included)
        (test_dir / "script.py").write_text("# Python script")
        (test_dir / "readme.txt").write_text("README")
        (test_dir / "config.json").write_text('{"key": "value"}')

        # Binary files (should be excluded)
        (test_dir / "library.dll").write_bytes(b'\x4d\x5a\x90\x00')
        (test_dir / "program.exe").write_bytes(b'\x4d\x5a\x90\x00')
        (test_dir / "debug.pdb").write_bytes(b'\x4d\x53\x46\x00')
        (test_dir / "build.cache").write_bytes(b'CACHE\x00\x00')
        (test_dir / "data.bin").write_bytes(b'\x00\x01\x02\x03')

        # Create ACVerifier instance
        verifier = ACVerifier("702", "code", tmppath)

        # Test directory expansion
        success, error, matched_files = verifier._expand_glob_path("mixed/")

        # Verify success
        assert success, f"Expansion failed: {error}"
        assert error is None, f"Unexpected error: {error}"

        # Verify only text files are returned
        file_names = {p.name for p in matched_files}

        # Text files should be included
        assert "script.py" in file_names, "Python file should be included"
        assert "readme.txt" in file_names, "Text file should be included"
        assert "config.json" in file_names, "JSON file should be included"

        # Binary files should be excluded
        assert "library.dll" not in file_names, "DLL should be filtered out"
        assert "program.exe" not in file_names, "EXE should be filtered out"
        assert "debug.pdb" not in file_names, "PDB should be filtered out"
        assert "build.cache" not in file_names, "Cache file should be filtered out"
        assert "data.bin" not in file_names, "Binary file should be filtered out"

        # Verify count (only 3 text files)
        assert len(matched_files) == 3, f"Expected 3 text files, got {len(matched_files)}"


def test_skipped_files_warning_output(capsys):
    """Test case 4: Verify stderr warning messages for skipped files.

    Verifies that when binary files are skipped during directory enumeration,
    an informational message is logged to stderr indicating how many files
    were skipped.
    """
    with tempfile.TemporaryDirectory() as tmpdir:
        tmppath = Path(tmpdir)

        # Create directory with binary files
        test_dir = tmppath / "binaries"
        test_dir.mkdir()

        # Add binary files
        (test_dir / "file1.dll").write_bytes(b'\x4d\x5a\x90\x00')
        (test_dir / "file2.exe").write_bytes(b'\x4d\x5a\x90\x00')
        (test_dir / "file3.cache").write_bytes(b'CACHE\x00')

        # Add one text file
        (test_dir / "readme.txt").write_text("README")

        # Create ACVerifier instance
        verifier = ACVerifier("702", "code", tmppath, verbose=True)

        # Test directory expansion (should log warnings to stderr)
        success, error, matched_files = verifier._expand_glob_path("binaries/")

        # Verify success
        assert success, f"Expansion failed: {error}"

        # Capture stderr output
        captured = capsys.readouterr()

        # Verify warning message contains count of skipped files
        assert "INFO: Skipped 3 binary file(s) in binaries/" in captured.err, \
            f"Expected skipped file warning in stderr, got: {captured.err}"


def test_unicode_error_fallback():
    """Test case 5: Force UnicodeDecodeError with unusual extension.

    Tests the defense-in-depth exception handling layer. Creates a binary file
    with an unusual extension (.zzz) that won't be in BINARY_EXTENSIONS,
    verifying that the UnicodeDecodeError is caught gracefully.
    """
    with tempfile.TemporaryDirectory() as tmpdir:
        tmppath = Path(tmpdir)

        # Create directory
        test_dir = tmppath / "unusual"
        test_dir.mkdir()

        # Create binary file with unusual extension (not in BINARY_EXTENSIONS)
        binary_file = test_dir / "data.zzz"
        # Write binary content that will definitely cause UnicodeDecodeError
        binary_file.write_bytes(b'\x00\x01\x02\x03\x04\x05\xff\xfe\xfd')

        # Create one valid text file so we have something to find
        (test_dir / "valid.txt").write_text("Valid content")

        # Create ACVerifier instance
        verifier = ACVerifier("702", "code", tmppath)

        # Create AC definition for pattern search
        # This will trigger _search_pattern_native which should catch UnicodeDecodeError
        ac = ACDefinition(
            ac_number=1,
            description="Test unicode error handling",
            ac_type="code",
            method="Grep(unusual/)",
            matcher="contains",
            expected="Valid content"
        )
        ac.pattern_type = verifier.classify_pattern(ac)

        # Verify the AC (should succeed by finding pattern in valid.txt, skip data.zzz with warning)
        result = verifier.verify_code_ac(ac)

        # Verify PASS (found in valid.txt, binary file skipped)
        assert result["result"] == "PASS", \
            f"Expected PASS (pattern found in valid.txt), got: {result['result']}"

        # Verify matched files contains only valid.txt
        matched_files = result["details"]["matched_files"]
        assert len(matched_files) == 1, f"Expected 1 matched file, got {len(matched_files)}"
        assert matched_files[0].endswith("valid.txt"), \
            f"Expected valid.txt in matched files, got: {matched_files}"


if __name__ == "__main__":
    print("Running binary file handling tests...")
    try:
        # Note: test_skipped_files_warning_output requires pytest's capsys fixture
        # When run directly, skip that test
        test_eracore_directory_no_crash()
        test_obj_directory_binaries_skipped()
        test_binary_files_filtered_by_extension()
        test_unicode_error_fallback()
        print("\nAll binary file handling tests passed!")
        print("Note: test_skipped_files_warning_output requires pytest (run with: pytest tools/tests/test_ac_verifier_binary.py)")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
