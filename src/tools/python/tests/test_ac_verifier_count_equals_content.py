#!/usr/bin/env python3
"""
Test ac-static-verifier.py count_equals matcher for content-type verification.

This test verifies that count_equals, gt, gte, lt, lte matchers correctly count
pattern occurrences in file content (not file counts) for code-type ACs.
"""

import sys
import tempfile
from pathlib import Path

# Add parent directory to path to import ac-static-verifier
repo_root = Path(__file__).parent.parent.parent.parent.parent
sys.path.insert(0, str(repo_root / "src" / "tools" / "python"))

# Import the ACVerifier class and related types
import importlib.util

# Load ac-static-verifier.py as a module
verifier_path = repo_root / "src" / "tools" / "python" / "ac-static-verifier.py"
spec = importlib.util.spec_from_file_location("ac_static_verifier", verifier_path)
ac_verifier_module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ac_verifier_module)

ACDefinition = ac_verifier_module.ACDefinition
ACVerifier = ac_verifier_module.ACVerifier
PatternType = ac_verifier_module.PatternType


def test_count_equals_content_positive():
    """Test count_equals in _verify_content counts pattern occurrences correctly."""
    # Create a temp file in .tmp/ directory
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_count_equals_positive.txt"

    # Write content with known pattern count
    content = """
    Result<Unit> appeared once
    Another Result<Unit> appeared twice
    And Result<Unit> appeared three times
    """
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)

        # Make path relative to repo_root
        rel_path = temp_path.relative_to(repo_root)

        ac = ACDefinition(
            ac_number=1,
            description="Test count_equals content positive case",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="count_equals",
            expected="Result<Unit> (3)"  # Pattern (N) format
        )

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", (
            f"Expected PASS when count matches, got {result['result']}: {result.get('details', {}).get('error', '')}"
        )
        assert result["details"]["actual_count"] == 3, (
            f"Expected actual_count=3, got {result['details']['actual_count']}"
        )
        assert result["details"]["expected_count"] == 3, (
            f"Expected expected_count=3, got {result['details']['expected_count']}"
        )
        assert result["details"]["pattern"] == "Result<Unit>", (
            f"Expected pattern='Result<Unit>', got {result['details']['pattern']}"
        )
        print("[PASS] count_equals returns PASS when content count matches")
    finally:
        temp_path.unlink()


def test_count_equals_content_negative():
    """Test count_equals returns FAIL when count doesn't match."""
    # Create a temp file in .tmp/ directory
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_count_equals_negative.txt"

    content = "Result<Unit> appears once only"
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)

        # Make path relative to repo_root
        rel_path = temp_path.relative_to(repo_root)

        ac = ACDefinition(
            ac_number=2,
            description="Test count_equals content negative case",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="count_equals",
            expected="Result<Unit> (5)"  # Wrong count
        )

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "FAIL", (
            f"Expected FAIL when count doesn't match, got {result['result']}"
        )
        assert result["details"]["actual_count"] == 1, (
            f"Expected actual_count=1, got {result['details']['actual_count']}"
        )
        assert result["details"]["expected_count"] == 5, (
            f"Expected expected_count=5, got {result['details']['expected_count']}"
        )
        print(f"[PASS] count_equals returns FAIL when count differs (actual=1, expected=5)")
    finally:
        temp_path.unlink()


def test_pattern_n_format_parsing():
    """Test Pattern (N) format parser extracts pattern and count correctly."""
    # Create a temp file in .tmp/ directory
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_pattern_parsing.txt"

    content = "f(x) f(x) f(x)"  # Function name with parentheses, appears 3 times
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)

        # Make path relative to repo_root
        rel_path = temp_path.relative_to(repo_root)

        ac = ACDefinition(
            ac_number=3,
            description="Test Pattern (N) parsing with parenthesized content",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="count_equals",
            expected="f(x) (3)"  # Pattern contains parens, rightmost (N) is the count
        )

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", (
            f"Expected PASS, got {result['result']}: {result.get('details', {}).get('error', '')}"
        )
        assert result["details"]["pattern"] == "f(x)", (
            f"Expected pattern='f(x)', got {result['details']['pattern']}"
        )
        assert result["details"]["expected_count"] == 3, (
            f"Expected expected_count=3, got {result['details']['expected_count']}"
        )
        assert result["details"]["actual_count"] == 3, (
            f"Expected actual_count=3, got {result['details']['actual_count']}"
        )
        print("[PASS] Pattern (N) parser correctly extracts pattern and count for parenthesized content")
    finally:
        temp_path.unlink()


def test_gt_gte_lt_lte_matchers():
    """Test gt/gte/lt/lte matchers return correct PASS/FAIL."""
    # Create a temp file in .tmp/ directory
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_numeric_matchers.txt"

    content = "test test test test test"  # 5 occurrences
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)
        rel_path = temp_path.relative_to(repo_root)

        # Test gt: actual (5) > expected (3) -> PASS
        ac_gt_pass = ACDefinition(
            ac_number=4,
            description="Test gt matcher PASS",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="gt",
            expected="test (3)"
        )
        result_gt_pass = verifier.verify_code_ac(ac_gt_pass)
        assert result_gt_pass["result"] == "PASS", f"gt: 5 > 3 should PASS, got {result_gt_pass['result']}"
        print("[PASS] gt matcher: actual > expected -> PASS")

        # Test gt: actual (5) == expected (5) -> FAIL
        ac_gt_fail = ACDefinition(
            ac_number=5,
            description="Test gt matcher FAIL",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="gt",
            expected="test (5)"
        )
        result_gt_fail = verifier.verify_code_ac(ac_gt_fail)
        assert result_gt_fail["result"] == "FAIL", f"gt: 5 > 5 should FAIL, got {result_gt_fail['result']}"
        print("[PASS] gt matcher: actual == expected -> FAIL")

        # Test gte: actual (5) >= expected (5) -> PASS
        ac_gte_pass = ACDefinition(
            ac_number=6,
            description="Test gte matcher PASS",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="gte",
            expected="test (5)"
        )
        result_gte_pass = verifier.verify_code_ac(ac_gte_pass)
        assert result_gte_pass["result"] == "PASS", f"gte: 5 >= 5 should PASS, got {result_gte_pass['result']}"
        print("[PASS] gte matcher: actual >= expected -> PASS")

        # Test gte: actual (5) < expected (6) -> FAIL
        ac_gte_fail = ACDefinition(
            ac_number=7,
            description="Test gte matcher FAIL",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="gte",
            expected="test (6)"
        )
        result_gte_fail = verifier.verify_code_ac(ac_gte_fail)
        assert result_gte_fail["result"] == "FAIL", f"gte: 5 >= 6 should FAIL, got {result_gte_fail['result']}"
        print("[PASS] gte matcher: actual < expected -> FAIL")

        # Test lt: actual (5) < expected (6) -> PASS
        ac_lt_pass = ACDefinition(
            ac_number=8,
            description="Test lt matcher PASS",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="lt",
            expected="test (6)"
        )
        result_lt_pass = verifier.verify_code_ac(ac_lt_pass)
        assert result_lt_pass["result"] == "PASS", f"lt: 5 < 6 should PASS, got {result_lt_pass['result']}"
        print("[PASS] lt matcher: actual < expected -> PASS")

        # Test lt: actual (5) >= expected (5) -> FAIL
        ac_lt_fail = ACDefinition(
            ac_number=9,
            description="Test lt matcher FAIL",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="lt",
            expected="test (5)"
        )
        result_lt_fail = verifier.verify_code_ac(ac_lt_fail)
        assert result_lt_fail["result"] == "FAIL", f"lt: 5 < 5 should FAIL, got {result_lt_fail['result']}"
        print("[PASS] lt matcher: actual >= expected -> FAIL")

        # Test lte: actual (5) <= expected (5) -> PASS
        ac_lte_pass = ACDefinition(
            ac_number=10,
            description="Test lte matcher PASS",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="lte",
            expected="test (5)"
        )
        result_lte_pass = verifier.verify_code_ac(ac_lte_pass)
        assert result_lte_pass["result"] == "PASS", f"lte: 5 <= 5 should PASS, got {result_lte_pass['result']}"
        print("[PASS] lte matcher: actual <= expected -> PASS")

        # Test lte: actual (5) > expected (4) -> FAIL
        ac_lte_fail = ACDefinition(
            ac_number=11,
            description="Test lte matcher FAIL",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="lte",
            expected="test (4)"
        )
        result_lte_fail = verifier.verify_code_ac(ac_lte_fail)
        assert result_lte_fail["result"] == "FAIL", f"lte: 5 <= 4 should FAIL, got {result_lte_fail['result']}"
        print("[PASS] lte matcher: actual > expected -> FAIL")

    finally:
        temp_path.unlink()


def test_count_equals_format_a_backtick_regex():
    """Test count_equals with Format A: `regex_pattern` = N (backtick-wrapped regex counting)."""
    verifier = ACVerifier("999", "code", repo_root)

    # Create temp file with known content
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    test_file = tmp_dir / "test_format_a.py"

    try:
        # Content with 2 occurrences of "def foo(" pattern
        test_file.write_text(
            "def foo():\n    pass\ndef bar():\n    pass\ndef foo(x):\n    pass\n",
            encoding='utf-8'
        )

        # Make path relative to repo_root
        rel_path = test_file.relative_to(repo_root)

        # Format A: `pattern` = N with regex
        ac = ACDefinition(
            ac_number=13,
            description="Test Format A backtick-wrapped regex counting",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="count_equals",
            expected=r"`def foo\(` = 2"
        )

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", f"Expected PASS, got {result['result']}: {result.get('details', {}).get('error', '')}"
        assert result["details"]["actual_count"] == 2, f"Expected actual_count=2, got {result['details']['actual_count']}"
        assert result["details"]["expected_count"] == 2, f"Expected expected_count=2, got {result['details']['expected_count']}"
        print("[PASS] Format A: backtick-wrapped regex counting works correctly")
    finally:
        if test_file.exists():
            test_file.unlink()


def test_count_equals_file_grep_path():
    """Test that file+Grep+count_equals delegates to _verify_content correctly."""
    # Create a temp file in .tmp/ directory
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_file_grep.txt"

    content = "pattern pattern pattern"  # 3 occurrences
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "file", repo_root)
        rel_path = temp_path.relative_to(repo_root)

        # Use file type with Grep method
        ac = ACDefinition(
            ac_number=12,
            description="Test file+Grep+count_equals delegation",
            ac_type="file",
            method=f"Grep({rel_path})",
            matcher="count_equals",
            expected="pattern (3)"
        )

        result = verifier.verify_file_ac(ac)

        assert result["result"] == "PASS", (
            f"Expected PASS for file+Grep+count_equals, got {result['result']}: {result.get('details', {}).get('error', '')}"
        )
        assert result["details"]["actual_count"] == 3, (
            f"Expected actual_count=3, got {result['details']['actual_count']}"
        )
        assert result["details"]["expected_count"] == 3, (
            f"Expected expected_count=3, got {result['details']['expected_count']}"
        )
        print("[PASS] file+Grep+count_equals correctly delegates to _verify_content")
    finally:
        temp_path.unlink()


def test_count_equals_format_c_bare_number_complex_method():
    """Test count_equals with Format C: bare numeric Expected + pattern from complex Method."""
    verifier = ACVerifier("999", "code", repo_root)

    # Create temp file with known content
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    test_file = tmp_dir / "test_format_c.cs"

    try:
        # Content with 3 occurrences of "int characterIndex"
        test_file.write_text(
            "public void Method1(int characterIndex) {}\n"
            "public void Method2(int characterIndex) {}\n"
            "public void Method3(int characterIndex) {}\n"
            "public void Method4(string name) {}\n",
            encoding='utf-8'
        )

        # Make path relative to repo_root
        rel_path = test_file.relative_to(repo_root)

        # Simulate complex method: pattern from Method, count from Expected
        ac = ACDefinition(
            ac_number=1,
            description="Test Format C",
            ac_type="code",
            method=f'Grep(path={rel_path}, pattern=int characterIndex)',
            matcher="count_equals",
            expected="3"
        )
        ac.pattern_type = verifier.classify_pattern(ac)

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", f"Expected PASS, got {result}"
        assert result["details"]["actual_count"] == 3
        assert result["details"]["expected_count"] == 3
        print("[PASS] Format C: bare numeric Expected + complex method pattern works correctly")
    finally:
        if test_file.exists():
            test_file.unlink()


def test_count_equals_format_c_regex_pattern():
    """Test Format C with regex pattern from complex method."""
    verifier = ACVerifier("999", "code", repo_root)

    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    test_file = tmp_dir / "test_format_c_regex.cs"

    try:
        test_file.write_text(
            "public Result<Unit> BeginTrain() {}\n"
            "public Result<Unit> SaveGameDialog() {}\n"
            "public Result<Unit> LoadGameDialog() {}\n"
            "public Result<string> GetName() {}\n",
            encoding='utf-8'
        )

        # Make path relative to repo_root
        rel_path = test_file.relative_to(repo_root)

        # Pattern with regex from complex method
        ac = ACDefinition(
            ac_number=2,
            description="Test Format C regex",
            ac_type="code",
            method=fr'Grep(path={rel_path}, pattern=public Result<Unit> (BeginTrain|SaveGameDialog|LoadGameDialog)\(\))',
            matcher="count_equals",
            expected="3"
        )
        ac.pattern_type = verifier.classify_pattern(ac)

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", f"Expected PASS, got {result}"
        assert result["details"]["actual_count"] == 3
        print("[PASS] Format C: regex pattern from complex method works correctly")
    finally:
        if test_file.exists():
            test_file.unlink()


if __name__ == "__main__":
    print("Running count_equals content-type matcher tests...")
    try:
        test_count_equals_content_positive()
        test_count_equals_content_negative()
        test_pattern_n_format_parsing()
        test_gt_gte_lt_lte_matchers()
        test_count_equals_format_a_backtick_regex()
        test_count_equals_file_grep_path()
        test_count_equals_format_c_bare_number_complex_method()
        test_count_equals_format_c_regex_pattern()
        print("\nAll count_equals content-type matcher tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
