#!/usr/bin/env python3
"""
Test ac-static-verifier.py unescape function with normal quotes (regression test).

This test ensures that the unescape function does not break normal quote handling
and preserves existing functionality when processing strings without escape sequences.
"""

import sys
from pathlib import Path

# Add parent directory to path to import ac-static-verifier
repo_root = Path(__file__).parent.parent.parent.parent.parent
sys.path.insert(0, str(repo_root / "src" / "tools" / "python"))

# Import the ACVerifier class to access unescape method
from pathlib import Path as VerifierPath
import importlib.util

# Load ac-static-verifier.py as a module
verifier_path = repo_root / "src" / "tools" / "python" / "ac-static-verifier.py"
spec = importlib.util.spec_from_file_location("ac_static_verifier", verifier_path)
ac_verifier_module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ac_verifier_module)

ACVerifier = ac_verifier_module.ACVerifier


def test_unescape_preserves_normal_text():
    """Test that normal text without escapes is unchanged."""
    test_cases = [
        "simple text",
        "text with spaces",
        "text_with_underscores",
        "text-with-hyphens",
        "UPPERCASE TEXT",
        "MixedCase123",
        "text with numbers 123",
        "special chars !@#$%^&*()",
    ]

    for input_str in test_cases:
        result = ACVerifier.unescape(input_str)
        assert result == input_str, (
            f"Input: {input_str!r}, Expected: {input_str!r}, Got: {result!r}"
        )
        print(f"[PASS] Test passed: {input_str!r}")


def test_unescape_preserves_single_quotes():
    """Test that single quotes are not affected by unescape."""
    test_cases = [
        "text with 'single quotes'",
        "'single' and 'multiple'",
        "apostrophe's test",
    ]

    for input_str in test_cases:
        result = ACVerifier.unescape(input_str)
        assert result == input_str, (
            f"Input: {input_str!r}, Expected: {input_str!r}, Got: {result!r}"
        )
        print(f"[PASS] Test passed: {input_str!r}")


def test_unescape_preserves_other_special_chars():
    """Test that other special characters are preserved."""
    test_cases = [
        "path/to/file",
        "C:\\Windows\\Path",
        "email@example.com",
        "url: https://example.com",
        "brackets [test]",
        "braces {test}",
        "parentheses (test)",
    ]

    for input_str in test_cases:
        result = ACVerifier.unescape(input_str)
        assert result == input_str, (
            f"Input: {input_str!r}, Expected: {input_str!r}, Got: {result!r}"
        )
        print(f"[PASS] Test passed: {input_str!r}")


def test_unescape_empty_and_whitespace():
    """Test that empty strings and whitespace are handled correctly."""
    test_cases = [
        "",
        " ",
        "  ",
        "\t",
        "\n",
        " leading space",
        "trailing space ",
        " both spaces ",
    ]

    for input_str in test_cases:
        result = ACVerifier.unescape(input_str)
        assert result == input_str, (
            f"Input: {input_str!r}, Expected: {input_str!r}, Got: {result!r}"
        )
        print(f"[PASS] Test passed: {input_str!r}")


if __name__ == "__main__":
    print("Running normal quote regression tests...")
    try:
        test_unescape_preserves_normal_text()
        test_unescape_preserves_single_quotes()
        test_unescape_preserves_other_special_chars()
        test_unescape_empty_and_whitespace()
        print("\nAll regression tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        sys.exit(1)
