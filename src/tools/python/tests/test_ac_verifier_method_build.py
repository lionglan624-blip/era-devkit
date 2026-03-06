#!/usr/bin/env python3
"""
Test cases for build matcher with command in Method column.

Tests the enhancement where build matcher can extract commands from the Method
column when Expected="-", supporting the evolved AC definition format.
"""

import sys
import os
import importlib.util
from pathlib import Path
from unittest.mock import patch, MagicMock
import subprocess

# Get the repository root
repo_root = Path(__file__).resolve().parent.parent.parent.parent.parent

# Import ac-static-verifier dynamically
verifier_path = repo_root / "src" / "tools" / "python" / "ac-static-verifier.py"
spec = importlib.util.spec_from_file_location("ac_static_verifier", verifier_path)
ac_verifier_module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ac_verifier_module)

ACDefinition = ac_verifier_module.ACDefinition
ACVerifier = ac_verifier_module.ACVerifier


def test_method_command_positive():
    """Test build matcher with command in Method - exit 0 → PASS"""
    # AC: Method="cmd /c exit 0", Expected="-", matcher="succeeds", Type="build"
    # Using "exit 0" instead of "echo test" to avoid quoting issues with split()
    ac = ACDefinition(
        ac_number=1,
        description="Test positive case with Method command",
        ac_type="build",
        method="cmd /c exit 0",
        matcher="succeeds",
        expected="-"
    )

    verifier = ACVerifier("626", "build", repo_root)
    result = verifier.verify_build_ac(ac)

    # Exit code 0 with succeeds matcher → PASS
    assert result["result"] == "PASS", (
        f"Expected PASS for exit code 0 with succeeds matcher, got {result['result']}: {result.get('details', '')}"
    )
    print(f"[PASS] test_method_command_positive")


def test_method_command_negative():
    """Test build matcher with command in Method - exit 1 → FAIL"""
    # AC: Method="cmd /c exit 1", Expected="-", matcher="succeeds", Type="build"
    ac = ACDefinition(
        ac_number=2,
        description="Test negative case with Method command",
        ac_type="build",
        method="cmd /c exit 1",
        matcher="succeeds",
        expected="-"
    )

    verifier = ACVerifier("626", "build", repo_root)
    result = verifier.verify_build_ac(ac)

    # Exit code 1 with succeeds matcher → FAIL
    assert result["result"] == "FAIL", (
        f"Expected FAIL for exit code 1 with succeeds matcher, got {result['result']}"
    )
    print(f"[PASS] test_method_command_negative")


def test_backward_compat_expected_column():
    """Test backward compatibility with Expected column"""
    # AC: Method="", Expected="cmd /c exit 0", matcher="succeeds", Type="build"
    # Traditional format - command in Expected column
    # Using "exit 0" to avoid quoting issues with split()
    ac = ACDefinition(
        ac_number=3,
        description="Test backward compatibility",
        ac_type="build",
        method="",
        matcher="succeeds",
        expected="cmd /c exit 0"
    )

    verifier = ACVerifier("626", "build", repo_root)
    result = verifier.verify_build_ac(ac)

    # Traditional format should still work → PASS
    assert result["result"] == "PASS", (
        f"Expected PASS for traditional format (Expected column), got {result['result']}: {result.get('details', '')}"
    )
    print(f"[PASS] test_backward_compat_expected_column")


def _make_build_ac(method, matcher="succeeds", expected="-", ac_number=99):
    """Helper: create a build ACDefinition with the given method string."""
    return ACDefinition(
        ac_number=ac_number,
        description="cross-repo build test",
        ac_type="build",
        method=method,
        matcher=matcher,
        expected=expected,
    )


def _mock_completed_process(returncode=0, stdout="Build succeeded.", stderr=""):
    """Helper: create a CompletedProcess mock."""
    mock_result = MagicMock()
    mock_result.returncode = returncode
    mock_result.stdout = stdout
    mock_result.stderr = stderr
    return mock_result


def test_wsl_cross_repo_cwd():
    """WSL dotnet path: engine/ prefix resolves CWD to engine repo WSL path (AC#6, AC#12)."""
    ac = _make_build_ac("dotnet build engine/uEmuera.Headless.csproj")

    with patch("subprocess.run", return_value=_mock_completed_process()) as mock_subprocess:
        verifier = ACVerifier("841", "build", repo_root)
        result = verifier.verify_build_ac(ac)

    assert mock_subprocess.called, "subprocess.run should have been called"
    call_args = mock_subprocess.call_args

    # The WSL bash-c command should be the first positional argument (a list)
    cmd_list = call_args[0][0]
    assert cmd_list[0] == "wsl", "Command should start with 'wsl'"
    assert cmd_list[2] == "bash", "Command should use bash"

    # The bash -c string should contain the engine repo WSL path
    bash_c_string = cmd_list[4]
    assert "/mnt/c/Era/engine" in bash_c_string, (
        f"bash-c string should contain '/mnt/c/Era/engine' but got: {bash_c_string}"
    )

    # The prefix 'engine/' should be stripped from the project path argument
    assert "engine/uEmuera.Headless.csproj" not in bash_c_string, (
        f"Prefix 'engine/' should be stripped; bash-c string: {bash_c_string}"
    )
    assert "uEmuera.Headless.csproj" in bash_c_string, (
        f"Project filename should remain in bash-c string: {bash_c_string}"
    )

    print(f"[PASS] test_wsl_cross_repo_cwd")


def test_non_dotnet_cross_repo_cwd():
    """Non-dotnet path: cross-repo prefix resolves cwd kwarg to correct repo root (AC#7, AC#12)."""
    # Use a non-dotnet command with core/ prefix; Expected column not '-' so use Method via expected="-"
    ac = _make_build_ac("some-tool core/SomeTool.csproj")

    with patch("subprocess.run", return_value=_mock_completed_process()) as mock_subprocess:
        verifier = ACVerifier("841", "build", repo_root)
        result = verifier.verify_build_ac(ac)

    assert mock_subprocess.called, "subprocess.run should have been called"
    call_kwargs = mock_subprocess.call_args[1]

    assert "cwd" in call_kwargs, "subprocess.run should receive a cwd kwarg"
    # Path("C:/Era/core") normalizes to OS path separators; compare via Path equality
    assert Path(call_kwargs["cwd"]) == Path("C:/Era/core"), (
        f"cwd should be the resolved core repo root 'C:/Era/core', got: {call_kwargs['cwd']}"
    )

    print(f"[PASS] test_non_dotnet_cross_repo_cwd")


def test_explicit_cd_skip():
    """Explicit 'cd' in command prevents CWD auto-resolution (AC#8, AC#12)."""
    # This is the F839 workaround pattern — should NOT be modified
    ac = _make_build_ac("wsl -- bash -c 'cd /mnt/c/Era/core && dotnet build'")

    with patch("subprocess.run", return_value=_mock_completed_process()) as mock_subprocess:
        verifier = ACVerifier("841", "build", repo_root)
        result = verifier.verify_build_ac(ac)

    assert mock_subprocess.called, "subprocess.run should have been called"
    call_kwargs = mock_subprocess.call_args[1]

    # Non-dotnet path: cwd should fall back to self.repo_root (not a cross-repo root)
    assert "cwd" in call_kwargs, "subprocess.run should receive a cwd kwarg"
    assert call_kwargs["cwd"] == str(repo_root), (
        f"With explicit 'cd', cwd should remain repo_root='{repo_root}', got: {call_kwargs['cwd']}"
    )

    print(f"[PASS] test_explicit_cd_skip")


def test_env_var_override_cwd():
    """ENGINE_PATH env var overrides default engine repo root in CWD resolution (AC#9, AC#12)."""
    custom_engine_path = "C:/custom/engine"
    ac = _make_build_ac("dotnet build engine/uEmuera.Headless.csproj")

    with patch("subprocess.run", return_value=_mock_completed_process()) as mock_subprocess:
        with patch.dict(os.environ, {"ENGINE_PATH": custom_engine_path}):
            verifier = ACVerifier("841", "build", repo_root)
            result = verifier.verify_build_ac(ac)

    assert mock_subprocess.called, "subprocess.run should have been called"
    cmd_list = mock_subprocess.call_args[0][0]
    bash_c_string = cmd_list[4]

    # WSL path for custom engine: C:/custom/engine → /mnt/c/custom/engine
    expected_wsl_path = "/mnt/c/custom/engine"
    assert expected_wsl_path in bash_c_string, (
        f"bash-c string should contain '{expected_wsl_path}' (from ENGINE_PATH override), "
        f"got: {bash_c_string}"
    )
    # Default path should NOT be used
    assert "/mnt/c/Era/engine" not in bash_c_string, (
        f"Default engine path should NOT appear when ENGINE_PATH is overridden, got: {bash_c_string}"
    )

    print(f"[PASS] test_env_var_override_cwd")


def test_no_prefix_backward_compat():
    """Devkit-local build command (no cross-repo prefix) keeps CWD at repo_root (AC#10, AC#12, AC#13)."""
    ac = _make_build_ac("dotnet build devkit.sln")

    with patch("subprocess.run", return_value=_mock_completed_process()) as mock_subprocess:
        verifier = ACVerifier("841", "build", repo_root)
        result = verifier.verify_build_ac(ac)

    assert mock_subprocess.called, "subprocess.run should have been called"
    cmd_list = mock_subprocess.call_args[0][0]
    bash_c_string = cmd_list[4]

    # CWD should be the devkit repo root (WSL-converted)
    expected_wsl_devkit = verifier._convert_to_wsl_path(str(repo_root))
    assert expected_wsl_devkit in bash_c_string, (
        f"bash-c string should contain devkit WSL root '{expected_wsl_devkit}', got: {bash_c_string}"
    )
    # No cross-repo paths should appear
    for cross_repo in ["/mnt/c/Era/engine", "/mnt/c/Era/core", "/mnt/c/Era/game", "/mnt/c/Era/dashboard"]:
        assert cross_repo not in bash_c_string, (
            f"Cross-repo path '{cross_repo}' should NOT appear for devkit-local command, got: {bash_c_string}"
        )

    print(f"[PASS] test_no_prefix_backward_compat")


import pytest

@pytest.mark.parametrize("prefix,env_var,default_path,expected_wsl", [
    ("engine/", "ENGINE_PATH", "C:/Era/engine", "/mnt/c/Era/engine"),
    ("core/",   "CORE_PATH",   "C:/Era/core",   "/mnt/c/Era/core"),
    ("game/",   "GAME_PATH",   "C:/Era/game",   "/mnt/c/Era/game"),
    ("dashboard/", "DASHBOARD_PATH", "C:/Era/dashboard", "/mnt/c/Era/dashboard"),
])
def test_all_cross_repo_prefixes(prefix, env_var, default_path, expected_wsl):
    """All 4 cross-repo prefixes (engine/, core/, game/, dashboard/) resolve to correct CWD (AC#14, AC#12, AC#13)."""
    ac = _make_build_ac(f"dotnet build {prefix}SomeProject.csproj")

    with patch("subprocess.run", return_value=_mock_completed_process()) as mock_subprocess:
        # Ensure env var is not set so defaults apply
        env_without_override = {k: v for k, v in os.environ.items() if k != env_var}
        with patch.dict(os.environ, env_without_override, clear=True):
            verifier = ACVerifier("841", "build", repo_root)
            result = verifier.verify_build_ac(ac)

    assert mock_subprocess.called, f"subprocess.run should have been called for prefix '{prefix}'"
    cmd_list = mock_subprocess.call_args[0][0]
    bash_c_string = cmd_list[4]

    assert expected_wsl in bash_c_string, (
        f"bash-c string should contain '{expected_wsl}' for prefix '{prefix}', got: {bash_c_string}"
    )
    # Prefix should be stripped from the project path
    assert f"{prefix}SomeProject.csproj" not in bash_c_string, (
        f"Prefix '{prefix}' should be stripped from args in bash-c string: {bash_c_string}"
    )
    assert "SomeProject.csproj" in bash_c_string, (
        f"'SomeProject.csproj' should remain in bash-c string for prefix '{prefix}': {bash_c_string}"
    )

    print(f"[PASS] test_all_cross_repo_prefixes[{prefix}]")


if __name__ == "__main__":
    print("Running build matcher Method column tests...")
    try:
        test_method_command_positive()
        test_method_command_negative()
        test_backward_compat_expected_column()
        print("\nAll tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        sys.exit(1)
