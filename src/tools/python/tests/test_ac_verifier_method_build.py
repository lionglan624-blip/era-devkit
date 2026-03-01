#!/usr/bin/env python3
"""
Test cases for build matcher with command in Method column.

Tests the enhancement where build matcher can extract commands from the Method
column when Expected="-", supporting the evolved AC definition format.
"""

import sys
import importlib.util
from pathlib import Path

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
