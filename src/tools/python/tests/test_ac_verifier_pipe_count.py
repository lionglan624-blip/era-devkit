r"""Test that bare | in Method-column patterns works as regex alternation with count/gte matchers.

Feature 842: For count/gte matchers, bare `|` inside quoted patterns is regex alternation.
`\|` with count/gte matchers should emit a diagnostic WARNING on stderr to guide AC authors.
"""
import pytest
import sys
import os
import tempfile
from pathlib import Path

sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))
from importlib import import_module
ac_mod = import_module('ac-static-verifier')
ACVerifier = ac_mod.ACVerifier
ACDefinition = ac_mod.ACDefinition
PatternType = ac_mod.PatternType


class TestPipeAlternationWithCountMatcher:
    """Verify that bare | works as regex alternation with count/gte matchers."""

    def test_pipe_alternation_with_gte(self, tmp_path):
        """Bare | in pattern is regex alternation — matches 'foo' or 'bar' lines."""
        # Create temp file with content matching either 'foo' or 'bar'
        test_file = tmp_path / "test_content.txt"
        test_file.write_text("foo\nbar\nbaz\n", encoding="utf-8")

        verifier = ACVerifier(feature_id=0, ac_type='code', repo_root=tmp_path)

        ac = ACDefinition(
            ac_number=99,
            description='test pipe alternation with gte',
            ac_type='code',
            method=f'Grep({test_file}, pattern="foo|bar")',
            matcher='gte',
            expected='2',
        )
        ac.pattern_type = PatternType.COUNT

        result = verifier.verify_code_ac(ac)
        assert result['result'] == 'PASS', (
            f"Expected PASS for bare | alternation with gte matcher, got: {result}"
        )

    def test_pipe_escape_warning_with_gte(self, tmp_path, capsys):
        r"""Pattern with \| and gte matcher emits WARNING on stderr about pipe convention."""
        # Create temp file with simple content
        test_file = tmp_path / "test_content.txt"
        test_file.write_text("foo\nbar\nbaz\n", encoding="utf-8")

        verifier = ACVerifier(feature_id=0, ac_type='code', repo_root=tmp_path)

        # Use \| in pattern with gte matcher — should trigger WARNING
        ac = ACDefinition(
            ac_number=42,
            description='test pipe escape warning with gte',
            ac_type='code',
            method=r'Grep(' + str(test_file) + r', pattern="foo\|bar")',
            matcher='gte',
            expected='1',
        )
        ac.pattern_type = PatternType.COUNT

        verifier.verify_code_ac(ac)

        captured = capsys.readouterr()
        assert 'WARNING' in captured.err, (
            f"Expected WARNING in stderr about \\| with gte matcher, got stderr: {captured.err!r}"
        )
        assert r'\|' in captured.err or 'pipe' in captured.err.lower(), (
            f"Expected WARNING to mention \\| or pipe, got stderr: {captured.err!r}"
        )
