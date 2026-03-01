#!/usr/bin/env python3
"""
Test ac-static-verifier.py complex method parsing with named parameters.

This test verifies that the tool can parse and process complex Method formats:
- Grep(path="...", pattern="...", type=cs) - named parameters
- Grep(path="...") - path-only named parameter
- Pattern override behavior (pattern= takes precedence over Expected column)
- Type parameter parsing (type=cs, type=py, etc.)
- Edge cases: quotes, whitespace variations, missing optional params
"""

import sys
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


def test_complex_method_basic():
    """Test basic named parameters: Grep(path="...", pattern="...") - pattern should override Expected."""
    # Use actual file from repository (ac-static-verifier.py contains "class PatternType")
    test_file = "src/tools/python/ac-static-verifier.py"

    # Create AC definition with complex method format
    # pattern= should override Expected column for matching
    ac = ACDefinition(
        ac_number=1,
        description="Test pattern override",
        ac_type="code",
        method=f'Grep(path="{test_file}", pattern="class PatternType")',
        matcher="contains",
        expected="IGNORED_VALUE"  # This should be overridden by pattern= parameter
    )

    # Create verifier instance
    verifier = ACVerifier("632", "code", repo_root)

    # Verify AC
    result = verifier.verify_code_ac(ac)

    # Verify result
    assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
    print(f"[PASS] Complex method with named parameters works correctly")


def test_complex_method_path_only():
    """Test path-only format: Grep(path="...") - should use Expected column for pattern."""
    # Use actual file from repository (ac-static-verifier.py contains "def parse_feature_markdown")
    test_file = "src/tools/python/ac-static-verifier.py"

    # Create AC definition with path-only named parameter
    # No pattern= parameter, so Expected column should be used
    ac = ACDefinition(
        ac_number=2,
        description="Test path-only",
        ac_type="code",
        method=f'Grep(path="{test_file}")',
        matcher="contains",
        expected="def parse_feature_markdown"
    )

    # Create verifier instance
    verifier = ACVerifier("632", "code", repo_root)

    # Verify AC
    result = verifier.verify_code_ac(ac)

    # Verify result
    assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
    print(f"[PASS] Path-only named parameter works correctly")


def test_complex_method_with_type():
    """Test type parameter: Grep(path="...", pattern="...", type=py) - type should be parseable."""
    # Use actual Python file from repository
    test_file = "src/tools/python/ac-static-verifier.py"

    # Create AC definition with type parameter
    ac = ACDefinition(
        ac_number=3,
        description="Test type parameter",
        ac_type="code",
        method=f'Grep(path="{test_file}", pattern="class ACVerifier", type=py)',
        matcher="contains",
        expected="IGNORED"
    )

    # Create verifier instance
    verifier = ACVerifier("632", "code", repo_root)

    # Verify AC
    result = verifier.verify_code_ac(ac)

    # Verify result - should parse without error (even if type filter not yet implemented)
    assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
    print(f"[PASS] Type parameter parsed correctly")


def test_complex_method_pattern_override():
    """Test that Method pattern= parameter overrides Expected column."""
    # Use actual file from repository
    test_file = "src/tools/python/ac-static-verifier.py"

    # Create AC with pattern in Method (should take precedence over Expected)
    ac = ACDefinition(
        ac_number=4,
        description="Test pattern override",
        ac_type="code",
        method=f'Grep(path="{test_file}", pattern="class ACVerifier")',
        matcher="contains",
        expected="wrong_pattern_that_does_not_exist"  # This should NOT be used
    )

    # Create verifier instance
    verifier = ACVerifier("632", "code", repo_root)

    # Verify AC
    result = verifier.verify_code_ac(ac)

    # Should PASS because pattern= is used, not Expected
    assert result["result"] == "PASS", f"Expected PASS (pattern override), got {result['result']}: {result.get('details', {})}"
    print(f"[PASS] Pattern override works correctly")


def test_complex_method_whitespace_variations():
    """Test extra spaces in named parameters are handled correctly."""
    # Use actual file from repository
    test_file = "src/tools/python/ac-static-verifier.py"

    # Create AC with extra whitespace in Method
    ac = ACDefinition(
        ac_number=5,
        description="Test whitespace",
        ac_type="code",
        method=f'Grep(  path = "{test_file}" ,  pattern = "class ACDefinition"  )',
        matcher="contains",
        expected="IGNORED"
    )

    # Create verifier instance
    verifier = ACVerifier("632", "code", repo_root)

    # Verify AC
    result = verifier.verify_code_ac(ac)

    # Should handle whitespace variations
    assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
    print(f"[PASS] Whitespace variations handled correctly")


def test_complex_method_quoted_values():
    """Test single and double quotes around parameter values."""
    # Use actual file from repository
    test_file = "src/tools/python/ac-static-verifier.py"

    # Test with single quotes (if supported)
    ac_single = ACDefinition(
        ac_number=6,
        description="Test single quotes",
        ac_type="code",
        method=f"Grep(path='{test_file}', pattern='class PatternType')",
        matcher="contains",
        expected="IGNORED"
    )

    # Create verifier instance
    verifier = ACVerifier("632", "code", repo_root)

    # Verify AC with single quotes
    result = verifier.verify_code_ac(ac_single)

    # Should handle single quotes
    assert result["result"] == "PASS", f"Expected PASS with single quotes, got {result['result']}: {result.get('details', {})}"
    print(f"[PASS] Single quotes handled correctly")

    # Test with double quotes
    ac_double = ACDefinition(
        ac_number=7,
        description="Test double quotes",
        ac_type="code",
        method=f'Grep(path="{test_file}", pattern="class PatternType")',
        matcher="contains",
        expected="IGNORED"
    )

    result = verifier.verify_code_ac(ac_double)

    # Should handle double quotes
    assert result["result"] == "PASS", f"Expected PASS with double quotes, got {result['result']}: {result.get('details', {})}"
    print(f"[PASS] Double quotes handled correctly")


def test_complex_method_missing_optional_params():
    """Test that missing optional parameters (type) don't cause errors."""
    # Use actual file from repository
    test_file = "src/tools/python/ac-static-verifier.py"

    # Create AC without optional type parameter
    ac = ACDefinition(
        ac_number=8,
        description="Test missing optional params",
        ac_type="code",
        method=f'Grep(path="{test_file}", pattern="def verify_code_ac")',
        matcher="contains",
        expected="IGNORED"
    )

    # Create verifier instance
    verifier = ACVerifier("632", "code", repo_root)

    # Verify AC
    result = verifier.verify_code_ac(ac)

    # Should work without optional parameters
    assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {})}"
    print(f"[PASS] Missing optional parameters handled correctly")


if __name__ == "__main__":
    print("Running complex method parsing tests...")
    try:
        test_complex_method_basic()
        test_complex_method_path_only()
        test_complex_method_with_type()
        test_complex_method_pattern_override()
        test_complex_method_whitespace_variations()
        test_complex_method_quoted_values()
        test_complex_method_missing_optional_params()
        print("\nAll complex method tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
