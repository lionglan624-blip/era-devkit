#!/usr/bin/env python3
"""
Test ac-static-verifier.py cross-repo path handling (AC#11-AC#17, AC#20).

This test verifies that the verifier handles absolute cross-repo paths correctly:
- Absolute path detection via is_absolute() (AC#11)
- _safe_relative_path returns full path string for cross-repo paths (AC#12)
- not_matches matcher with cross-repo absolute path (AC#13)
- file-type AC with cross-repo absolute path (AC#14)
- _expand_glob_path returns absolute path directly WITHOUT prepending repo_root (AC#15)
- matches matcher with cross-repo absolute path (AC#16)
- count matcher with cross-repo absolute path (AC#17)
- _expand_glob_path still prepends repo_root for relative paths (AC#20)
"""

import sys
import tempfile
from pathlib import Path
import importlib.util

# Load module dynamically (required because filename has hyphen)
repo_root = Path(__file__).parent.parent.parent.parent.parent
spec = importlib.util.spec_from_file_location(
    "ac_static_verifier",
    repo_root / "src" / "tools" / "python" / "ac-static-verifier.py"
)
ac_verifier_module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ac_verifier_module)
ACVerifier = ac_verifier_module.ACVerifier
ACDefinition = ac_verifier_module.ACDefinition


def test_is_absolute_detection():
    """Test that _expand_glob_path detects absolute paths via is_absolute() (AC#11).

    Verifies that when an absolute path is given to _expand_glob_path, it resolves
    the path directly without prepending repo_root. This confirms the is_absolute()
    detection logic is in place and working.
    """
    with tempfile.TemporaryDirectory() as repo_dir, tempfile.TemporaryDirectory() as cross_dir:
        repo_root_path = Path(repo_dir)
        cross_root = Path(cross_dir)

        # Create a file in the cross-repo directory
        cross_file = cross_root / "src" / "SomeClass.cs"
        cross_file.parent.mkdir(parents=True, exist_ok=True)
        cross_file.write_text("public class SomeClass {}\n")

        verifier = ACVerifier("999", "code", repo_root_path)

        # Pass an absolute path to _expand_glob_path
        success, error_msg, matched_files = verifier._expand_glob_path(str(cross_file))

        # Should succeed - absolute path found directly, no repo_root prepend
        assert success, f"Expected success for absolute cross-repo path, got error: {error_msg}"
        assert len(matched_files) == 1, f"Expected 1 matched file, got {len(matched_files)}"
        assert matched_files[0] == cross_file, (
            f"Expected matched file to be {cross_file}, got {matched_files[0]}"
        )
        print(f"[PASS] test_is_absolute_detection: absolute path detected and resolved directly")


def test_safe_relative_path_cross_repo():
    """Test that _safe_relative_path returns full path string for cross-repo paths (AC#12).

    Verifies that when _safe_relative_path receives a path outside repo_root,
    it returns the full absolute path string instead of raising ValueError.
    """
    with tempfile.TemporaryDirectory() as repo_dir, tempfile.TemporaryDirectory() as cross_dir:
        repo_root_path = Path(repo_dir)
        cross_root = Path(cross_dir)

        # Create a file in cross-repo
        cross_file = cross_root / "src" / "Era.Core" / "Counter.cs"
        cross_file.parent.mkdir(parents=True, exist_ok=True)
        cross_file.write_text("public class Counter {}\n")

        verifier = ACVerifier("999", "code", repo_root_path)

        # _safe_relative_path should return full path for cross-repo paths
        result = verifier._safe_relative_path(cross_file)

        # Should be the full path string, not raise ValueError
        assert result == str(cross_file), (
            f"Expected full path string '{cross_file}', got '{result}'"
        )
        # Confirm it is NOT a relative path (would start with ".." or be short)
        assert not result.startswith(".."), (
            f"Expected full path, not relative path starting with '..': {result}"
        )
        print(f"[PASS] test_safe_relative_path_cross_repo: returns full path for cross-repo path")


def test_cross_repo_not_matches():
    """Test not_matches matcher with cross-repo absolute path (AC#13).

    Verifies that the not_matches matcher handles absolute cross-repo paths
    without crashing at the _safe_relative_path call sites.
    """
    with tempfile.TemporaryDirectory() as repo_dir, tempfile.TemporaryDirectory() as cross_dir:
        repo_root_path = Path(repo_dir)
        cross_root = Path(cross_dir)

        # Create a file in cross-repo WITHOUT the pattern we're checking
        cross_file = cross_root / "src" / "CleanCode.cs"
        cross_file.parent.mkdir(parents=True, exist_ok=True)
        cross_file.write_text("public class CleanCode\n{\n    public void Run() {}\n}\n")

        # Create ACDefinition with absolute path to cross-repo file
        ac = ACDefinition(
            ac_number=1,
            description="No TODO markers in cross-repo file",
            ac_type="code",
            method=f"Grep({cross_file})",
            matcher="not_matches",
            expected="TODO|FIXME"
        )

        verifier = ACVerifier("999", "code", repo_root_path)
        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", (
            f"Expected PASS for not_matches with no TODO pattern, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] test_cross_repo_not_matches: not_matches works with cross-repo absolute path")


def test_cross_repo_file():
    """Test file-type AC with cross-repo absolute path (AC#14).

    Verifies that file-type ACs handle absolute cross-repo paths correctly
    through _safe_relative_path without crashing.
    """
    with tempfile.TemporaryDirectory() as repo_dir, tempfile.TemporaryDirectory() as cross_dir:
        repo_root_path = Path(repo_dir)
        cross_root = Path(cross_dir)

        # Create a file in cross-repo
        cross_file = cross_root / "src" / "Era.Core" / "IService.cs"
        cross_file.parent.mkdir(parents=True, exist_ok=True)
        cross_file.write_text("public interface IService {}\n")

        # Create ACDefinition with absolute path to cross-repo file using Glob
        ac = ACDefinition(
            ac_number=1,
            description="IService file exists in cross-repo",
            ac_type="file",
            method=f"Glob({cross_file})",
            matcher="exists",
            expected="-"
        )

        verifier = ACVerifier("999", "file", repo_root_path)
        result = verifier.verify_file_ac(ac)

        assert result["result"] == "PASS", (
            f"Expected PASS for exists with cross-repo file, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] test_cross_repo_file: file-type AC works with cross-repo absolute path")


def test_expand_glob_path_absolute():
    """Test _expand_glob_path returns absolute path directly WITHOUT prepending repo_root (AC#15).

    Verifies the behavioral correctness of the is_absolute() guard:
    when given an absolute path, _expand_glob_path must NOT prepend repo_root.
    """
    with tempfile.TemporaryDirectory() as repo_dir, tempfile.TemporaryDirectory() as cross_dir:
        repo_root_path = Path(repo_dir)
        cross_root = Path(cross_dir)

        # Create a file in cross-repo
        cross_file = cross_root / "src" / "Counter" / "CounterMessage.cs"
        cross_file.parent.mkdir(parents=True, exist_ok=True)
        cross_file.write_text("namespace Counter { public class CounterMessage {} }\n")

        verifier = ACVerifier("999", "code", repo_root_path)

        # Verify that _expand_glob_path does NOT prepend repo_root for absolute paths
        success, error_msg, matched_files = verifier._expand_glob_path(str(cross_file))

        assert success, f"Expected success for existing absolute path, got error: {error_msg}"
        assert len(matched_files) == 1, f"Expected 1 matched file, got {len(matched_files)}"

        # Critical: the matched file should be exactly the cross_file path,
        # NOT repo_root / cross_file (which would be wrong for absolute paths)
        matched = matched_files[0]
        assert matched == cross_file, (
            f"Expected matched file == cross_file ({cross_file}), got {matched}. "
            f"repo_root was NOT prepended (correct behavior)."
        )

        # Also verify that the file is NOT under repo_root (confirming no incorrect join)
        try:
            matched.relative_to(repo_root_path)
            assert False, (
                f"Matched file {matched} should NOT be under repo_root {repo_root_path}. "
                f"If relative_to succeeds, repo_root was incorrectly prepended."
            )
        except ValueError:
            pass  # Expected: cross-repo file is not relative to repo_root

        print(f"[PASS] test_expand_glob_path_absolute: absolute path returned directly without repo_root prepend")


def test_cross_repo_matches():
    """Test matches matcher with cross-repo absolute path (AC#16).

    Verifies that the matches matcher handles absolute cross-repo paths
    through _safe_relative_path without crashing.
    """
    with tempfile.TemporaryDirectory() as repo_dir, tempfile.TemporaryDirectory() as cross_dir:
        repo_root_path = Path(repo_dir)
        cross_root = Path(cross_dir)

        # Create a file in cross-repo with known content
        cross_file = cross_root / "src" / "file.py"
        cross_file.parent.mkdir(parents=True, exist_ok=True)
        cross_file.write_text("def hello_world():\n    pass\n")

        # Create ACDefinition with absolute path to cross-repo file
        ac = ACDefinition(
            ac_number=1,
            description="Test matches in cross-repo file",
            ac_type="code",
            method=f"Grep({cross_file})",
            matcher="matches",
            expected="def hello"
        )

        # Run verifier
        verifier = ACVerifier("999", "code", repo_root_path)
        result = verifier.verify_code_ac(ac)

        # Assert no crash and PASS result
        assert result["result"] == "PASS", (
            f"Expected PASS for matches with cross-repo path, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] test_cross_repo_matches: matches matcher works with cross-repo absolute path")


def test_cross_repo_count():
    """Test count matcher with cross-repo absolute path (AC#17).

    Verifies that the count_equals matcher handles absolute cross-repo paths
    through _safe_relative_path without crashing.
    """
    with tempfile.TemporaryDirectory() as repo_dir, tempfile.TemporaryDirectory() as cross_dir:
        repo_root_path = Path(repo_dir)
        cross_root = Path(cross_dir)

        # Create a file in cross-repo with known content (2 occurrences of "def func")
        cross_file = cross_root / "src" / "module.py"
        cross_file.parent.mkdir(parents=True, exist_ok=True)
        cross_file.write_text("def func_one():\n    pass\n\ndef func_two():\n    pass\n")

        # Create ACDefinition with absolute path to cross-repo file
        # Pattern "def func (2)" means count literal "def func" occurrences, expect 2
        ac = ACDefinition(
            ac_number=1,
            description="Two def functions in cross-repo file",
            ac_type="code",
            method=f"Grep({cross_file})",
            matcher="count_equals",
            expected="def func (2)"
        )

        # Run verifier
        verifier = ACVerifier("999", "code", repo_root_path)
        result = verifier.verify_code_ac(ac)

        # Assert no crash and PASS result
        assert result["result"] == "PASS", (
            f"Expected PASS for count_equals with cross-repo path, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] test_cross_repo_count: count matcher works with cross-repo absolute path")


def test_expand_glob_path_relative():
    """Test _expand_glob_path still prepends repo_root for relative paths (AC#20).

    Verifies that the is_absolute() guard does not break existing behavior:
    relative paths must still be joined with repo_root as before.
    """
    with tempfile.TemporaryDirectory() as repo_dir:
        repo_root_path = Path(repo_dir)

        # Create a file at a relative path within the simulated repo
        subdir = repo_root_path / "src" / "tools" / "python"
        subdir.mkdir(parents=True, exist_ok=True)
        test_file = subdir / "my_tool.py"
        test_file.write_text("def main():\n    pass\n")

        verifier = ACVerifier("999", "code", repo_root_path)

        # Pass a relative path - should prepend repo_root
        relative_path = "src/tools/python/my_tool.py"
        success, error_msg, matched_files = verifier._expand_glob_path(relative_path)

        assert success, f"Expected success for existing relative path, got error: {error_msg}"
        assert len(matched_files) == 1, f"Expected 1 matched file, got {len(matched_files)}"

        # The matched file should resolve to repo_root / relative_path
        expected_absolute = repo_root_path / relative_path
        matched = matched_files[0]
        assert matched == expected_absolute, (
            f"Expected matched file == {expected_absolute}, got {matched}. "
            f"Relative path must be joined with repo_root."
        )

        # Confirm the matched file IS under repo_root
        try:
            matched.relative_to(repo_root_path)
        except ValueError:
            assert False, (
                f"Matched file {matched} should be under repo_root {repo_root_path}. "
                f"Relative path behavior must be preserved."
            )

        print(f"[PASS] test_expand_glob_path_relative: relative paths still prepend repo_root")


def test_convert_to_wsl_path():
    """Test _convert_to_wsl_path conversion for Windows paths (AC#18).

    Verifies correct conversion for both forward-slash and backslash Windows paths,
    as well as already-WSL paths that should not be modified.
    """
    with tempfile.TemporaryDirectory() as tmpdir:
        verifier = ACVerifier("999", "code", Path(tmpdir))
        # Forward slash
        assert verifier._convert_to_wsl_path("C:/Era/devkit") == "/mnt/c/Era/devkit"
        # Backslash
        assert verifier._convert_to_wsl_path("C:\\Era\\devkit") == "/mnt/c/Era/devkit"
        # Already WSL path (no conversion)
        assert verifier._convert_to_wsl_path("/mnt/c/Era") == "/mnt/c/Era"
    print("[PASS] test_convert_to_wsl_path: Windows paths converted to WSL mount paths correctly")


def test_verify_build_ac_uses_wsl():
    """Verify verify_build_ac uses WSL subprocess for dotnet commands (AC#21)."""
    import unittest.mock
    with tempfile.TemporaryDirectory() as tmpdir:
        repo_root = Path(tmpdir)
        # Create feature file with build AC
        pm_dir = repo_root / "pm" / "features"
        pm_dir.mkdir(parents=True, exist_ok=True)
        feature_file = pm_dir / "feature-999.md"
        feature_file.write_text(
            "| 1 | Test build | build | dotnet build | succeeds | - | [ ] |\n"
        )

        verifier = ACVerifier("999", "build", repo_root)

        # Mock subprocess.run to capture the command
        with unittest.mock.patch("subprocess.run") as mock_run:
            mock_run.return_value = unittest.mock.Mock(
                returncode=0, stdout="Build succeeded", stderr=""
            )

            # Create ACDefinition for build type
            ac = ACDefinition(
                ac_number=1,
                description="Test",
                ac_type="build",
                method="dotnet build",
                matcher="succeeds",
                expected="-"
            )
            verifier.verify_build_ac(ac)

            # Verify WSL invocation
            call_args = mock_run.call_args
            cmd = call_args[0][0]  # First positional arg
            assert cmd[0] == "wsl", f"Expected cmd[0]='wsl', got '{cmd[0]}'"
            assert cmd[1] == "--", f"Expected cmd[1]='--', got '{cmd[1]}'"
            assert cmd[2] == "bash", f"Expected cmd[2]='bash', got '{cmd[2]}'"
            assert cmd[3] == "-c", f"Expected cmd[3]='-c', got '{cmd[3]}'"
            assert "/mnt/" in cmd[4], f"Expected '/mnt/' in WSL bash command, got '{cmd[4]}'"
    print("[PASS] test_verify_build_ac_uses_wsl: verify_build_ac uses WSL subprocess for dotnet commands")


def test_verify_build_ac_cross_repo_path():
    """Verify verify_build_ac converts repo_root to WSL mount path in cd command (AC#22)."""
    import unittest.mock
    with tempfile.TemporaryDirectory() as tmpdir:
        # Use a path that simulates a Windows drive path
        repo_root = Path(tmpdir)
        verifier = ACVerifier("999", "build", repo_root)

        with unittest.mock.patch("subprocess.run") as mock_run:
            mock_run.return_value = unittest.mock.Mock(
                returncode=0, stdout="Build succeeded", stderr=""
            )

            ac = ACDefinition(
                ac_number=1,
                description="Test",
                ac_type="build",
                method="dotnet build",
                matcher="succeeds",
                expected="-"
            )
            verifier.verify_build_ac(ac)

            call_args = mock_run.call_args
            cmd = call_args[0][0]
            bash_cmd = cmd[4]  # The bash -c argument
            # Verify cd command contains converted path
            assert "cd " in bash_cmd, f"Expected 'cd ' in bash command, got '{bash_cmd}'"
            assert ".dotnet/dotnet" in bash_cmd, (
                f"Expected '.dotnet/dotnet' in bash command, got '{bash_cmd}'"
            )
    print("[PASS] test_verify_build_ac_cross_repo_path: repo_root converted to WSL path in cd command")


if __name__ == "__main__":
    print("Running cross-repo path tests (AC#11-AC#17, AC#20, AC#18, AC#21, AC#22)...")
    try:
        test_is_absolute_detection()
        test_safe_relative_path_cross_repo()
        test_cross_repo_not_matches()
        test_cross_repo_file()
        test_expand_glob_path_absolute()
        test_cross_repo_matches()
        test_cross_repo_count()
        test_expand_glob_path_relative()
        test_convert_to_wsl_path()
        test_verify_build_ac_uses_wsl()
        test_verify_build_ac_cross_repo_path()
        print("\nAll cross-repo path tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
