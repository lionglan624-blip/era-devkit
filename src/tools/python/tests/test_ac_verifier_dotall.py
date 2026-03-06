"""Tests for multiline=true parameter enabling re.DOTALL (F842 Task 3).

Verifies AC#5 (multiline=true enables DOTALL for cross-line matching)
and AC#6 (DOTALL not applied by default without multiline=true).
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
PatternType = ac_verifier_module.PatternType


class TestDotallPropagation:
    """Tests for multiline=true -> re.DOTALL propagation."""

    def test_dotall_with_multiline_param(self, tmp_path):
        """multiline=true enables re.DOTALL so .+ matches across newlines.

        Creates a temp file with 'line1\\nline2', uses pattern '.+line2' with
        multiline=true. Without DOTALL, '.' does not match '\\n', so the pattern
        would NOT match. With DOTALL, '.' matches '\\n' so '.+line2' matches.
        """
        # Create temp file with content spanning two lines
        test_file = tmp_path / "test_content.txt"
        test_file.write_text("line1\nline2", encoding='utf-8')

        verifier = ACVerifier(feature_id=842, ac_type='code', repo_root=repo_root)

        # AC with multiline=true in the Method column
        ac = ACDefinition(
            ac_number=1,
            description='dotall test',
            ac_type='code',
            method=f'Grep(path="{test_file}", multiline=true, pattern=".+line2")',
            matcher='matches',
            expected='-',
        )
        ac.pattern_type = PatternType.REGEX

        result = verifier.verify_code_ac(ac)
        assert result['result'] == 'PASS', (
            f"Expected PASS with multiline=true (re.DOTALL), got {result['result']}: "
            f"{result.get('details', {})}"
        )

    def test_no_dotall_without_multiline_param(self, tmp_path):
        """Without multiline=true, .+ does NOT match across newlines.

        Same content and pattern as test_dotall_with_multiline_param, but
        without multiline=true. The '.' should NOT match '\\n' so the
        cross-line pattern '.+line2' should FAIL.
        """
        # Create temp file with content spanning two lines
        test_file = tmp_path / "test_content.txt"
        test_file.write_text("line1\nline2", encoding='utf-8')

        verifier = ACVerifier(feature_id=842, ac_type='code', repo_root=repo_root)

        # AC WITHOUT multiline=true — default behavior
        ac = ACDefinition(
            ac_number=2,
            description='no dotall test',
            ac_type='code',
            method=f'Grep(path="{test_file}", pattern=".+line2")',
            matcher='matches',
            expected='-',
        )
        ac.pattern_type = PatternType.REGEX

        result = verifier.verify_code_ac(ac)
        assert result['result'] == 'FAIL', (
            f"Expected FAIL without multiline=true (no re.DOTALL), got {result['result']}: "
            f"{result.get('details', {})}"
        )
