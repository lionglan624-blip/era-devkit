#!/usr/bin/env python3
"""
Test ac-static-verifier.py pipe escape handling.

This test verifies that backslash-escaped pipes (\|) in AC Expected values
are correctly parsed (not split as column separator) and unescaped to literal pipes.
"""

import sys
import tempfile
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


def test_backslash_pipe_not_split():
    """Test that \\| in Expected is not split as a column separator."""
    # Create a temp file in .tmp/ directory
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_pipe_escape.md"

    feature_content = '''# Feature Test

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Pipe test | code | Grep(.claude/skills/testing/SKILL.md) | contains | "Predecessor \\| F788" | [ ] |
'''
    temp_path.write_text(feature_content, encoding='utf-8')

    try:
        # Create verifier pointing to temp file
        verifier = ACVerifier("999", "code", repo_root)
        # Override feature_file to use temp file
        verifier.feature_file = temp_path

        # Parse the markdown
        acs = verifier.parse_feature_markdown()

        assert len(acs) == 1, f"Expected 1 AC, got {len(acs)}"

        ac = acs[0]
        # After parsing and unescaping, \| should become |
        assert ac.expected == "Predecessor | F788", (
            f"Expected 'Predecessor | F788' after unescape, got '{ac.expected}'"
        )
        print(f"[PASS] Backslash-pipe in Expected is not split as column separator: '{ac.expected}'")
    finally:
        temp_path.unlink()


def test_unescape_pipe():
    """Test that unescape() converts \\| to |."""
    result = ACVerifier.unescape(r'\|')
    assert result == '|', f"Expected '|', got '{result}'"
    print(f"[PASS] unescape(r'\\|') -> '{result}'")

    # Test with surrounding text
    result2 = ACVerifier.unescape(r'Predecessor \| F788')
    assert result2 == 'Predecessor | F788', f"Expected 'Predecessor | F788', got '{result2}'"
    print(f"[PASS] unescape(r'Predecessor \\| F788') -> '{result2}'")


def test_unescape_for_literal_search_pipe():
    """Test that unescape_for_literal_search() converts \\| to |."""
    result = ACVerifier.unescape_for_literal_search(r'\|')
    assert result == '|', f"Expected '|', got '{result}'"
    print(f"[PASS] unescape_for_literal_search(r'\\|') -> '{result}'")

    # Test with surrounding text
    result2 = ACVerifier.unescape_for_literal_search(r'Predecessor \| F788')
    assert result2 == 'Predecessor | F788', f"Expected 'Predecessor | F788', got '{result2}'"
    print(f"[PASS] unescape_for_literal_search(r'Predecessor \\| F788') -> '{result2}'")


if __name__ == "__main__":
    print("Running pipe escape handling tests...")
    try:
        test_unescape_pipe()
        test_unescape_for_literal_search_pipe()
        test_backslash_pipe_not_split()
        print("\nAll pipe escape handling tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
