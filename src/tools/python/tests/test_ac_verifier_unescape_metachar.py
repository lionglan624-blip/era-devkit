"""Tests for unescape() metacharacter handling (F817)."""
import sys
import importlib.util
from pathlib import Path

repo_root = Path(__file__).parent.parent.parent.parent.parent

# Load ac-static-verifier.py as module (hyphenated filename)
verifier_path = repo_root / "src" / "tools" / "python" / "ac-static-verifier.py"
spec = importlib.util.spec_from_file_location("ac_static_verifier", verifier_path)
ac_verifier_module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ac_verifier_module)

ACVerifier = ac_verifier_module.ACVerifier
ACDefinition = ac_verifier_module.ACDefinition


def test_unescape_paren():
    """Verify unescape handles escaped parentheses."""
    assert ACVerifier.unescape(r'\\(') == r'\('
    assert ACVerifier.unescape(r'\\)') == r'\)'
    # Combined
    assert ACVerifier.unescape(r'def _extract.*\\(') == r'def _extract.*\('


def test_unescape_dot():
    """Verify unescape handles escaped dot."""
    assert ACVerifier.unescape(r'\\.') == r'\.'
    assert ACVerifier.unescape(r'foo\\.bar') == r'foo\.bar'


def test_unescape_word():
    """Verify unescape handles escaped word character class."""
    assert ACVerifier.unescape(r'\\w') == r'\w'
    assert ACVerifier.unescape(r'\\w+\\.py') == r'\w+\.py'


def test_unescape_question():
    """Verify unescape handles escaped question mark."""
    assert ACVerifier.unescape(r'\\?') == r'\?'
    assert ACVerifier.unescape(r'foo\\?bar') == r'foo\?bar'


def test_complex_method_unescape():
    """End-to-end: complex method pattern with escaped metachar."""
    verifier = ACVerifier(feature_id=817, ac_type="code", repo_root=repo_root)
    ac = ACDefinition(
        ac_number=99,
        description="test",
        ac_type="code",
        method='Grep(path="src/tools/python/ac-static-verifier.py", pattern="def _extract.*\\\\(")',
        matcher="matches",
        expected="-"
    )
    result = verifier.verify_code_ac(ac)
    assert result["result"] == "PASS", f"Expected PASS but got {result['result']}: {result.get('details', {})}"


def test_glob_filtering():
    """Verify glob parameter restricts file search to matching files."""
    verifier = ACVerifier(feature_id=817, ac_type="code", repo_root=repo_root)
    ac = ACDefinition(
        ac_number=98,
        description="test glob",
        ac_type="code",
        method='Grep(path="src/tools/python/", glob="*.py", pattern="import")',
        matcher="matches",
        expected="-"
    )
    result = verifier.verify_code_ac(ac)
    assert result["result"] == "PASS", f"Expected PASS but got {result['result']}: {result.get('details', {})}"
    # Verify only .py files were matched
    matched_files = result.get("details", {}).get("matched_files", [])
    assert len(matched_files) > 0, "Expected at least one matched file"
    for f in matched_files:
        assert f.endswith('.py'), f"Expected .py file but got: {f}"
