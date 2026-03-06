r"""Tests for 9 new unescape rules in _UNESCAPE_RULES (F842 Task 2).

Verifies AC#3 (9 new rules present) and AC#4 (>= 17 total rules):
- \\s -> \s, \\S -> \S, \\d -> \d, \\D -> \D, \\W -> \W
- \\b -> \b, \\B -> \B, \\A -> \A, \\Z -> \Z
"""
import sys
import importlib.util
from pathlib import Path

repo_root = Path(__file__).parent.parent.parent.parent.parent

verifier_path = repo_root / "src" / "tools" / "python" / "ac-static-verifier.py"
spec = importlib.util.spec_from_file_location("ac_static_verifier", verifier_path)
ac_verifier_module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ac_verifier_module)

ACVerifier = ac_verifier_module.ACVerifier


class TestUnescapeCharclassRules:
    """Tests for new character class unescape rules."""

    def test_unescape_charclass_combination(self):
        r"""unescape_for_regex_pattern('[\\s\\S]*') returns '[\s\S]*'."""
        result = ACVerifier.unescape_for_regex_pattern(r'[\\s\\S]*')
        assert result == r'[\s\S]*', (
            f"Expected r'[\\s\\S]*', got: {result!r}"
        )

    def test_unescape_rule_slash_s(self):
        r"""\\s -> \s."""
        result = ACVerifier.unescape_for_regex_pattern(r'\\s')
        assert result == r'\s', f"Expected r'\\s', got: {result!r}"

    def test_unescape_rule_slash_S(self):
        r"""\\S -> \S."""
        result = ACVerifier.unescape_for_regex_pattern(r'\\S')
        assert result == r'\S', f"Expected r'\\S', got: {result!r}"

    def test_unescape_rule_slash_d(self):
        r"""\\d -> \d."""
        result = ACVerifier.unescape_for_regex_pattern(r'\\d')
        assert result == r'\d', f"Expected r'\\d', got: {result!r}"

    def test_unescape_rule_slash_D(self):
        r"""\\D -> \D."""
        result = ACVerifier.unescape_for_regex_pattern(r'\\D')
        assert result == r'\D', f"Expected r'\\D', got: {result!r}"

    def test_unescape_rule_slash_W(self):
        r"""\\W -> \W (counterpart of existing \\w -> \w)."""
        result = ACVerifier.unescape_for_regex_pattern(r'\\W')
        assert result == r'\W', f"Expected r'\\W', got: {result!r}"

    def test_unescape_rule_slash_b(self):
        r"""\\b -> \b (word boundary assertion)."""
        result = ACVerifier.unescape_for_regex_pattern(r'\\b')
        assert result == r'\b', f"Expected r'\\b', got: {result!r}"

    def test_unescape_rule_slash_B(self):
        r"""\\B -> \B (non-word boundary)."""
        result = ACVerifier.unescape_for_regex_pattern(r'\\B')
        assert result == r'\B', f"Expected r'\\B', got: {result!r}"

    def test_unescape_rule_slash_A(self):
        r"""\\A -> \A (start-of-string anchor)."""
        result = ACVerifier.unescape_for_regex_pattern(r'\\A')
        assert result == r'\A', f"Expected r'\\A', got: {result!r}"

    def test_unescape_rule_slash_Z(self):
        r"""\\Z -> \Z (end-of-string anchor)."""
        result = ACVerifier.unescape_for_regex_pattern(r'\\Z')
        assert result == r'\Z', f"Expected r'\\Z', got: {result!r}"

    def test_unescape_backslash_b_raw_string(self):
        r"""unescape(r'\\b') produces two-char string \b (backslash + b), not backspace \x08."""
        result = ACVerifier.unescape(r'\\b')
        # The result should be the two-character string: backslash followed by 'b'
        assert result == '\\b', (
            f"Expected two-char string backslash+b, got: {result!r} (ord={ord(result[0]) if result else 'empty'})"
        )
        # Explicitly confirm it is NOT the backspace character
        assert result != '\x08', "Result must not be the backspace character \\x08"

    def test_unescape_rules_count(self):
        """_UNESCAPE_RULES must contain at least 17 entries (8 existing + 9 new)."""
        count = len(ACVerifier._UNESCAPE_RULES)
        assert count >= 17, (
            f"Expected at least 17 unescape rules, found {count}. "
            f"Need 8 existing + 9 new (\\s, \\S, \\d, \\D, \\W, \\b, \\B, \\A, \\Z)."
        )
