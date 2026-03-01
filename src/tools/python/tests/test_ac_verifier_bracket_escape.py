#!/usr/bin/env python3
"""
Test ac-static-verifier.py bracket escape normalization (AC#2).

This test verifies that markdown bracket escapes are correctly normalized:
- \\[DRAFT\\] should match [DRAFT] in files
- \\] should match ] in files
- Existing \" escape should continue to work
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


def test_unescape_bracket_escape():
    """Test unescape function handles bracket escapes."""
    test_cases = [
        (r"\\[DRAFT\\]", r"\[DRAFT\]"),  # Bracket escape
        (r"\\]", r"\]"),                   # Single bracket escape
        (r"\\[", r"\["),                   # Opening bracket only
        (r"Status: \\[PROPOSED\\]", r"Status: \[PROPOSED\]"),  # In context
    ]

    for input_str, expected in test_cases:
        result = ACVerifier.unescape(input_str)
        assert result == expected, (
            f"Input: {input_str!r}, Expected: {expected!r}, Got: {result!r}"
        )
        print(f"[PASS] unescape({input_str!r}) = {result!r}")


def test_escaped_bracket_not_regex():
    """Test that escaped brackets are not flagged as regex character class (AC#4 unit test).

    This tests the fix for F623/F631: _contains_regex_metacharacters should NOT flag
    escaped brackets like \[x\] as regex character classes.
    """
    # Test cases: (input_pattern, expected_is_regex)
    test_cases = [
        (r"\[DRAFT\]", False),  # Escaped brackets - NOT regex
        (r"\[x\]", False),      # Escaped single char - NOT regex
        (r"\[", False),         # Escaped opening bracket - NOT regex
        (r"\]", False),         # Escaped closing bracket - NOT regex
    ]

    for pattern, expected_is_regex in test_cases:
        has_regex = ACVerifier._contains_regex_metacharacters(pattern)
        assert has_regex == expected_is_regex, (
            f"Pattern {pattern!r}: Expected is_regex={expected_is_regex}, "
            f"got {has_regex}"
        )
        print(f"[PASS] _contains_regex_metacharacters({pattern!r}) = {has_regex}")


def test_real_character_class_flagged():
    """Test that real regex character classes are still flagged (AC#6 negative case).

    This ensures the fix for escaped brackets doesn't break detection of
    legitimate regex character classes.
    """
    # Test cases: (input_pattern, expected_is_regex)
    test_cases = [
        ("[a-z]", True),        # Character class - IS regex
        ("[0-9]", True),        # Digit class - IS regex
        ("[A-Z]+", True),       # Class with quantifier - IS regex
        ("test[abc]", True),    # Class in middle - IS regex
    ]

    for pattern, expected_is_regex in test_cases:
        has_regex = ACVerifier._contains_regex_metacharacters(pattern)
        assert has_regex == expected_is_regex, (
            f"Pattern {pattern!r}: Expected is_regex={expected_is_regex}, "
            f"got {has_regex}"
        )
        print(f"[PASS] _contains_regex_metacharacters({pattern!r}) = {has_regex}")


def test_bracket_escape_in_file_verification():
    """Test bracket escapes work in actual file verification (AC#5 integration test).

    NOTE (F631): This test expects PASS after the bracket escape fix.
    The fix adds negative lookbehind to _contains_regex_metacharacters
    to avoid false positives on escaped brackets like \[DRAFT\].
    """
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file containing [DRAFT] status
        test_file = tmpdir_path / "test.md"
        test_file.write_text("Status: [DRAFT]\n")

        # Create AC definition with literal bracket pattern
        # In real usage, markdown "Status: \\[DRAFT\\]" -> unescape() -> "Status: \[DRAFT\]"
        # When testing directly with ACDefinition, we pass the post-unescape value
        ac = ACDefinition(
            ac_number=1,
            description="Check DRAFT status",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected=r"Status: \[DRAFT\]"  # Escaped brackets (post-unescape from markdown)
        )

        # Create verifier instance
        verifier = ACVerifier("631", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # F631 FIX: Should now PASS after negative lookbehind fix
        # Pattern \[DRAFT\] should NOT be flagged as regex character class
        assert result["result"] == "PASS", (
            f"Expected PASS after F631 bracket escape fix, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Bracket escape pattern works correctly after F631 fix")


def test_closing_bracket_escape():
    """Test single closing bracket works in literal search.

    This tests a simpler case without opening bracket, which avoids F623 limitation.
    """
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file containing closing bracket
        test_file = tmpdir_path / "test.txt"
        test_file.write_text("Array index: arr]\n")

        # Create AC definition with literal bracket pattern
        # In real usage, markdown "arr\\]" -> unescape() -> "arr]"
        # When testing directly with ACDefinition, we pass the post-unescape value
        ac = ACDefinition(
            ac_number=1,
            description="Check bracket",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="arr]"  # Literal closing bracket (post-unescape)
        )

        # Create verifier instance
        verifier = ACVerifier("621", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        print(f"[PASS] Single bracket escape works correctly")


def test_quote_escape_still_works():
    """Test that existing \" escape continues to work (regression check)."""
    test_cases = [
        (r'\"quoted\"', '"quoted"'),
        (r'say \"hello\"', 'say "hello"'),
        (r'\"', '"'),
    ]

    for input_str, expected in test_cases:
        result = ACVerifier.unescape(input_str)
        assert result == expected, (
            f"Input: {input_str!r}, Expected: {expected!r}, Got: {result!r}"
        )
        print(f"[PASS] Quote escape still works: unescape({input_str!r}) = {result!r}")


if __name__ == "__main__":
    print("Running bracket escape normalization tests (AC#2, AC#4-6)...")
    try:
        test_unescape_bracket_escape()
        test_escaped_bracket_not_regex()
        test_real_character_class_flagged()
        test_bracket_escape_in_file_verification()
        test_closing_bracket_escape()
        test_quote_escape_still_works()
        print("\nAll bracket escape tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
