#!/usr/bin/env python3
"""
Test ac-static-verifier.py manual verification functionality for slash commands.

This test ensures that slash commands (e.g., /audit, /commit) are properly
detected and marked as requiring manual verification instead of attempting
subprocess execution, which is impossible per Testing SKILL line 79.
"""

import sys
from pathlib import Path

# Add parent directory to path to import ac-static-verifier
repo_root = Path(__file__).parent.parent.parent.parent.parent
sys.path.insert(0, str(repo_root / "src" / "tools" / "python"))

# Import the ACVerifier class
import importlib.util

# Load ac-static-verifier.py as a module
verifier_path = repo_root / "src" / "tools" / "python" / "ac-static-verifier.py"
spec = importlib.util.spec_from_file_location("ac_static_verifier", verifier_path)
ac_verifier_module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ac_verifier_module)

ACVerifier = ac_verifier_module.ACVerifier
ACDefinition = ac_verifier_module.ACDefinition


def test_slash_command_detection_positive():
    """Test that slash commands are correctly detected."""
    slash_commands = [
        "/audit",
        "/commit",
        "/reference-checker",
        "/fl",
        "/next",
        "/run",
        "/plan",
        "/complete-feature",
    ]

    for cmd in slash_commands:
        # Create ACDefinition with slash command
        ac = ACDefinition(
            ac_number=1,
            description=f"Test {cmd}",
            ac_type="file",
            method=cmd,
            matcher="succeeds",
            expected="-"
        )

        # Create verifier instance
        verifier = ACVerifier("999", "file", repo_root)

        # Verify slash command handling
        result = verifier.verify_file_ac(ac)

        assert result["result"] == "MANUAL", (
            f"Slash command {cmd} should return MANUAL status, got {result['result']}"
        )
        assert "slash_command" in result["details"], (
            f"Result should contain slash_command in details for {cmd}"
        )
        assert result["details"]["slash_command"] == cmd, (
            f"Slash command in details should be {cmd}, got {result['details']['slash_command']}"
        )
        print(f"[PASS] Slash command detected: {cmd}")


def test_slash_command_detection_negative():
    """Test that non-slash commands are not detected as slash commands."""
    non_slash_commands = [
        "Glob(Game/agents/*.md)",
        "Grep(src/tools/python/ac-static-verifier.py)",
        "dotnet build",
        "python script.py",
        "audit",  # without slash
        "commit",  # without slash
    ]

    for method in non_slash_commands:
        # Create ACDefinition with non-slash method
        ac = ACDefinition(
            ac_number=1,
            description=f"Test {method}",
            ac_type="file",
            method=method,
            matcher="exists",
            expected="some/path"
        )

        # Create verifier instance
        verifier = ACVerifier("999", "file", repo_root)

        # Verify non-slash command handling
        result = verifier.verify_file_ac(ac)

        # Should NOT be MANUAL status (may be PASS or FAIL depending on method)
        assert result["result"] != "MANUAL", (
            f"Non-slash method {method} should not return MANUAL status, got {result['result']}"
        )
        print(f"[PASS] Non-slash method not detected as slash command: {method}")


def test_manual_status_json_format():
    """Test that MANUAL status has consistent JSON output format."""
    ac = ACDefinition(
        ac_number=13,
        description="Test manual verification",
        ac_type="file",
        method="/audit",
        matcher="succeeds",
        expected="-"
    )

    verifier = ACVerifier("590", "file", repo_root)
    result = verifier.verify_file_ac(ac)

    # Check JSON structure
    assert "ac_number" in result, "Result should contain ac_number"
    assert "result" in result, "Result should contain result"
    assert "details" in result, "Result should contain details"
    assert result["result"] == "MANUAL", "Result should be MANUAL"
    assert result["ac_number"] == 13, "AC number should match"

    # Check details structure
    details = result["details"]
    assert "slash_command" in details, "Details should contain slash_command"
    assert "matcher" in details, "Details should contain matcher"
    assert "expected" in details, "Details should contain expected"
    assert "manual_verification" in details, "Details should contain manual_verification guidance"

    # Check values
    assert details["slash_command"] == "/audit", "Slash command should match"
    assert details["matcher"] == "succeeds", "Matcher should match"
    assert details["expected"] == "-", "Expected should match"
    assert isinstance(details["manual_verification"], str), "Manual verification should be string"
    assert len(details["manual_verification"]) > 0, "Manual verification should not be empty"

    print("[PASS] MANUAL status JSON format is consistent")


def test_no_subprocess_execution():
    """Test that slash commands do not attempt subprocess execution."""
    # This test verifies the implementation doesn't attempt subprocess execution
    # by checking that _handle_slash_command_ac is called instead
    ac = ACDefinition(
        ac_number=1,
        description="Test no subprocess",
        ac_type="file",
        method="/nonexistent-command",
        matcher="succeeds",
        expected="-"
    )

    verifier = ACVerifier("999", "file", repo_root)

    # Should return MANUAL without attempting execution
    # If subprocess was attempted, this would raise an exception or error
    result = verifier.verify_file_ac(ac)

    assert result["result"] == "MANUAL", (
        "Non-existent slash command should return MANUAL without attempting execution"
    )
    print("[PASS] No subprocess execution attempted for slash commands")


def test_various_slash_command_formats():
    """Test various slash command formats and variations."""
    test_cases = [
        ("/audit", "succeeds", "-"),
        ("/commit", "succeeds", "-"),
        ("/reference-checker", "succeeds", "-"),
        ("/fl", "succeeds", "590"),
        ("/run", "succeeds", "600"),
        ("/next", "succeeds", "-"),
    ]

    for slash_cmd, matcher, expected in test_cases:
        ac = ACDefinition(
            ac_number=1,
            description=f"Test {slash_cmd}",
            ac_type="file",
            method=slash_cmd,
            matcher=matcher,
            expected=expected
        )

        verifier = ACVerifier("999", "file", repo_root)
        result = verifier.verify_file_ac(ac)

        assert result["result"] == "MANUAL", (
            f"Slash command {slash_cmd} should return MANUAL status"
        )
        assert result["details"]["slash_command"] == slash_cmd, (
            f"Slash command in details should match {slash_cmd}"
        )
        assert result["details"]["matcher"] == matcher, (
            f"Matcher in details should match {matcher}"
        )
        assert result["details"]["expected"] == expected, (
            f"Expected in details should match {expected}"
        )
        print(f"[PASS] Slash command format handled: {slash_cmd} with matcher={matcher}")


def test_manual_verification_guidance():
    """Test that MANUAL status includes user guidance."""
    ac = ACDefinition(
        ac_number=1,
        description="Test guidance",
        ac_type="file",
        method="/audit",
        matcher="succeeds",
        expected="-"
    )

    verifier = ACVerifier("999", "file", repo_root)
    result = verifier.verify_file_ac(ac)

    guidance = result["details"]["manual_verification"]
    assert isinstance(guidance, str), "Guidance should be a string"
    assert len(guidance) > 0, "Guidance should not be empty"
    assert "manual" in guidance.lower(), "Guidance should mention manual verification"
    assert "slash command" in guidance.lower() or "command" in guidance.lower(), (
        "Guidance should reference commands"
    )

    print("[PASS] MANUAL status includes user guidance")


def test_literal_bracket_not_rejected_in_contains():
    r"""Test that literal bracket patterns [x], [DRAFT], [B] are NOT rejected as regex.

    These single-character or long-literal (5+ chars) patterns should pass through
    contains matcher without triggering regex metacharacter detection.

    The current regex pattern in _contains_regex_metacharacters checks for:
    - Character classes with ranges (e.g., [a-z])
    - Escape sequences (e.g., [\d])
    - Negation patterns (e.g., [^abc])
    - Short sequences 2-4 chars (e.g., [abc], [WIP]) are treated as character classes

    Single-char brackets [x] and long sequences 5+ chars [DRAFT] pass through.
    """
    literal_bracket_patterns = [
        "[x]",      # Single char in brackets (status marker)
        "[DRAFT]",  # 5+ chars - known literal status
        "[B]",      # Single char blocker marker
        "[ ]",      # Single char (space) - empty checkbox
        "[BLOCKED]",  # 5+ chars - blocker status
    ]

    verifier = ACVerifier("999", "code", repo_root)

    for pattern in literal_bracket_patterns:
        contains_regex = verifier._contains_regex_metacharacters(pattern)
        assert not contains_regex, (
            f"Literal bracket pattern '{pattern}' should NOT be detected as regex, "
            f"but _contains_regex_metacharacters returned True"
        )
        print(f"[PASS] Literal bracket pattern not rejected: {pattern}")


def test_genuine_regex_still_detected():
    """Test that genuine regex patterns with brackets ARE still detected and rejected.

    Character classes with ranges [a-z], escape sequences [\\d], and negation [^abc]
    should be properly detected as regex patterns unsuitable for contains matcher.
    """
    regex_patterns = [
        "[a-z]",        # Character class with range
        "[0-9]",        # Numeric range
        "[A-Z]",        # Uppercase range
        r"[\d]",        # Escape sequence (digit)
        r"[\w]",        # Escape sequence (word char)
        r"[\s]",        # Escape sequence (whitespace)
        "[^abc]",       # Negation pattern
        "[a-zA-Z0-9]",  # Multiple ranges
        "[abc]",        # Multiple literal chars (2+ chars treated as character class)
    ]

    verifier = ACVerifier("999", "code", repo_root)

    for pattern in regex_patterns:
        contains_regex = verifier._contains_regex_metacharacters(pattern)
        assert contains_regex, (
            f"Regex pattern '{pattern}' SHOULD be detected as regex, "
            f"but _contains_regex_metacharacters returned False"
        )
        print(f"[PASS] Regex pattern correctly detected: {pattern}")


if __name__ == "__main__":
    print("Running manual verification tests for ac-static-verifier...")
    try:
        test_slash_command_detection_positive()
        test_slash_command_detection_negative()
        test_manual_status_json_format()
        test_no_subprocess_execution()
        test_various_slash_command_formats()
        test_manual_verification_guidance()
        test_literal_bracket_not_rejected_in_contains()
        test_genuine_regex_still_detected()
        print("\nAll manual verification tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        sys.exit(1)
