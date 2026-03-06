"""Test that backslash-pipe in Method-column regex patterns is preserved.

Feature 834: unescape_for_regex_pattern() must NOT strip \\| from regex patterns
extracted via _extract_grep_params, because \\| means literal pipe in regex context.
"""
import pytest
import sys
import os
from pathlib import Path

sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))
from importlib import import_module
ac_mod = import_module('ac-static-verifier')
ACVerifier = ac_mod.ACVerifier


class TestPipeInRegexPattern:
    """Verify that pipe escape is preserved in Method-column regex patterns."""

    def test_unescape_for_regex_pattern_preserves_pipe(self):
        """unescape_for_regex_pattern keeps \\| intact for regex literal pipe."""
        result = ACVerifier.unescape_for_regex_pattern(r'(foo\|bar)')
        assert result == r'(foo\|bar)', f"Expected pipe preserved, got: {result}"

    def test_unescape_still_converts_pipe_for_expected(self):
        """unescape() still converts \\| to | for Expected-column markdown text."""
        result = ACVerifier.unescape(r'Predecessor \| F788')
        assert result == 'Predecessor | F788', f"Expected pipe unescaped, got: {result}"

    def test_unescape_for_regex_pattern_applies_other_rules(self):
        """unescape_for_regex_pattern applies bracket and other markdown rules."""
        result = ACVerifier.unescape_for_regex_pattern(r'\\[DRAFT\\]')
        assert result == r'\[DRAFT\]', f"Expected brackets unescaped, got: {result}"

    def test_pipe_in_complex_method_pattern_preserved(self):
        """Integration: pipe in complex Grep method pattern is preserved through _extract_grep_params."""
        verifier = ACVerifier(feature_id=0, ac_type='code', repo_root=Path('.'))

        # Create a mock AC with pipe in pattern
        ac = ac_mod.ACDefinition(
            ac_number=99,
            description='test pipe',
            ac_type='code',
            method=r'Grep(src/test.py, pattern="(foo\|bar)")',
            matcher='matches',
            expected=r'(foo\|bar)',
        )
        ac.pattern_type = ac_mod.PatternType.REGEX

        file_path, pattern, error = verifier._extract_grep_params(ac)
        # The pipe should be preserved in the extracted pattern
        assert r'\|' in pattern, f"Expected \\| preserved in pattern, got: {pattern}"
