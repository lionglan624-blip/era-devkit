"""Tests for escape-aware quote parsing in _parse_complex_method (F842 Task 1).

Verifies that AC#2 (escaped quote parsing) works correctly:
- pattern="\"cd \" in build_command" extracts full pattern `"cd " in build_command`
- Regression: pattern="foo" still works
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
ACDefinition = ac_verifier_module.ACDefinition


class TestEscapeAwareQuoteParsing:
    """Tests for escape-aware quote parsing in _parse_complex_method."""

    def test_escaped_quotes_in_pattern_extracts_full_pattern(self):
        """Pattern with escaped internal quotes extracts the full pattern correctly.

        Input:  Grep(file.py, pattern="\"cd \" in build_command")
        Expected pattern: "cd " in build_command  (not just backslash)
        """
        verifier = ACVerifier(feature_id=842, ac_type='code', repo_root=repo_root)
        parsed = verifier._parse_complex_method(
            'Grep(file.py, pattern="\\"cd \\" in build_command")'
        )
        assert parsed is not None, "Parsing should succeed"
        pattern = parsed.get('pattern')
        assert pattern is not None, "Pattern should be extracted"
        # The full pattern should be present, not just a backslash
        assert '"cd " in build_command' == pattern, (
            f'Expected \'"cd " in build_command\', got: {pattern!r}'
        )

    def test_simple_pattern_regression(self):
        """Regression: simple pattern="foo" still parses correctly."""
        verifier = ACVerifier(feature_id=842, ac_type='code', repo_root=repo_root)
        parsed = verifier._parse_complex_method(
            'Grep(src/file.py, pattern="foo")'
        )
        assert parsed is not None, "Parsing should succeed"
        pattern = parsed.get('pattern')
        assert pattern == 'foo', f'Expected "foo", got: {pattern!r}'

    def test_escaped_quote_not_partial(self):
        """Ensure the naive loop (stop at first quote) is not in effect.

        The naive loop would stop at the first quote inside the pattern,
        extracting only '\\' as the pattern value.
        """
        verifier = ACVerifier(feature_id=842, ac_type='code', repo_root=repo_root)
        parsed = verifier._parse_complex_method(
            'Grep(file.py, pattern="\\"cd \\" in build_command")'
        )
        assert parsed is not None, "Parsing should succeed"
        pattern = parsed.get('pattern')
        # The naive loop would return just backslash
        assert pattern != '\\', (
            f'Naive loop detected: pattern is just backslash. Got: {pattern!r}'
        )

    def test_multiple_escaped_quotes(self):
        """Pattern with multiple escaped quotes extracts correctly."""
        verifier = ACVerifier(feature_id=842, ac_type='code', repo_root=repo_root)
        parsed = verifier._parse_complex_method(
            'Grep(path="src/foo.py", pattern="\\"hello\\" and \\"world\\"")'
        )
        assert parsed is not None, "Parsing should succeed"
        pattern = parsed.get('pattern')
        assert '"hello" and "world"' == pattern, (
            f'Expected \'"hello" and "world"\', got: {pattern!r}'
        )
