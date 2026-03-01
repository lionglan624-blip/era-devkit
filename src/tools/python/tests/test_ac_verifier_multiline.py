#!/usr/bin/env python3
"""
Test ac-static-verifier.py re.MULTILINE and Format A comparison operator support.
"""

import sys
from pathlib import Path

repo_root = Path(__file__).parent.parent.parent.parent.parent
sys.path.insert(0, str(repo_root / "src" / "tools" / "python"))

import importlib.util

verifier_path = repo_root / "src" / "tools" / "python" / "ac-static-verifier.py"
spec = importlib.util.spec_from_file_location("ac_static_verifier", verifier_path)
ac_verifier_module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ac_verifier_module)

ACDefinition = ac_verifier_module.ACDefinition
ACVerifier = ac_verifier_module.ACVerifier
PatternType = ac_verifier_module.PatternType


def test_count_equals_caret_anchor_multiline():
    """AC#1: count_equals with ^-anchored pattern returns correct count (Pos)."""
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_multiline_ac1.txt"

    lines = ["namespace Foo;"]
    for i in range(1, 8):
        lines.append(f"    void Method{i}()")
    content = "\n".join(lines)
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)
        rel_path = temp_path.relative_to(repo_root)

        ac = ACDefinition(
            ac_number=1,
            description="count_equals with caret-anchored pattern",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="count_equals",
            expected="`^\\s+void ` = 7"
        )

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", (
            f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        )
        assert result["details"]["actual_count"] == 7, (
            f"Expected actual_count=7, got {result['details']['actual_count']}"
        )
        print("[PASS] count_equals with ^-anchored pattern returns correct count=7")
    finally:
        temp_path.unlink()


def test_matches_caret_anchor_multiline():
    """AC#2: matches with ^-anchored pattern returns True for mid-file content (Pos)."""
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_multiline_ac2.txt"

    content = "namespace X;\n    public void Method()"
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)
        rel_path = temp_path.relative_to(repo_root)

        ac = ACDefinition(
            ac_number=2,
            description="matches with caret-anchored pattern mid-file",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="matches",
            expected="^\\s+public"
        )

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", (
            f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        )
        print("[PASS] matches with ^-anchored pattern returns PASS for mid-file content")
    finally:
        temp_path.unlink()


def test_not_matches_caret_anchor_multiline():
    """AC#3: not_matches with ^-anchored pattern returns FAIL when pattern exists mid-file (Pos)."""
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_multiline_ac3.txt"

    content = "namespace X;\n    public void Method()"
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)
        rel_path = temp_path.relative_to(repo_root)

        ac = ACDefinition(
            ac_number=3,
            description="not_matches with caret-anchored pattern mid-file",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="not_matches",
            expected="^\\s+public"
        )

        result = verifier.verify_code_ac(ac)

        # Pattern IS found, so not_matches should FAIL
        assert result["result"] == "FAIL", (
            f"Expected FAIL (pattern exists so not_matches should fail), got {result['result']}: {result.get('details', {})}"
        )
        print("[PASS] not_matches with ^-anchored pattern returns FAIL when pattern exists mid-file")
    finally:
        temp_path.unlink()


def test_dollar_anchor_multiline():
    """AC#4: $-anchored pattern works correctly with MULTILINE (Pos)."""
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_multiline_ac4.txt"

    content = "class Foo {\nvoid Bar() {"
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)
        rel_path = temp_path.relative_to(repo_root)

        ac = ACDefinition(
            ac_number=4,
            description="dollar-anchor pattern with MULTILINE",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="count_equals",
            expected="`\\{$` = 2"
        )

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", (
            f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        )
        assert result["details"]["actual_count"] == 2, (
            f"Expected actual_count=2, got {result['details']['actual_count']}"
        )
        print("[PASS] $-anchored pattern works correctly with MULTILINE")
    finally:
        temp_path.unlink()


def test_format_a_gte_operator():
    """AC#5: Format A parser accepts >= N operator (Pos)."""
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_multiline_ac5.txt"

    content = "[Theory]\n[Theory]\n[Theory]\n[Theory]\n[Theory]\n"
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)
        rel_path = temp_path.relative_to(repo_root)

        ac = ACDefinition(
            ac_number=5,
            description="Format A accepts >= operator",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="gte",
            expected="`\\[Theory\\]` >= 4"
        )

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", (
            f"Expected PASS (5 >= 4), got {result['result']}: {result.get('details', {})}"
        )
        print("[PASS] Format A parser accepts >= N operator")
    finally:
        temp_path.unlink()


def test_format_a_all_comparison_operators():
    """AC#6: Format A parser accepts > N, <= N, < N operators (Pos)."""
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_multiline_ac6.txt"

    content = "TestPattern\n" * 5
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)
        rel_path = temp_path.relative_to(repo_root)

        # Test gt: 5 > 3 => PASS
        ac_gt = ACDefinition(
            ac_number=6,
            description="Format A accepts > operator",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="gt",
            expected="`TestPattern` > 3"
        )
        result_gt = verifier.verify_code_ac(ac_gt)
        assert result_gt["result"] == "PASS", (
            f"Expected PASS (5 > 3), got {result_gt['result']}: {result_gt.get('details', {})}"
        )

        # Test lte: 5 <= 6 => PASS
        ac_lte = ACDefinition(
            ac_number=6,
            description="Format A accepts <= operator",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="lte",
            expected="`TestPattern` <= 6"
        )
        result_lte = verifier.verify_code_ac(ac_lte)
        assert result_lte["result"] == "PASS", (
            f"Expected PASS (5 <= 6), got {result_lte['result']}: {result_lte.get('details', {})}"
        )

        # Test lt: 5 < 10 => PASS
        ac_lt = ACDefinition(
            ac_number=6,
            description="Format A accepts < operator",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="lt",
            expected="`TestPattern` < 10"
        )
        result_lt = verifier.verify_code_ac(ac_lt)
        assert result_lt["result"] == "PASS", (
            f"Expected PASS (5 < 10), got {result_lt['result']}: {result_lt.get('details', {})}"
        )

        # Test failing boundary: gt with > 5, actual=5 => FAIL (5 is not > 5)
        ac_gt_fail = ACDefinition(
            ac_number=6,
            description="Format A gt boundary FAIL",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="gt",
            expected="`TestPattern` > 5"
        )
        result_gt_fail = verifier.verify_code_ac(ac_gt_fail)
        assert result_gt_fail["result"] == "FAIL", (
            f"Expected FAIL (5 is not > 5), got {result_gt_fail['result']}: {result_gt_fail.get('details', {})}"
        )

        print("[PASS] Format A parser accepts >, <=, < operators; boundary case correctly FAILs")
    finally:
        temp_path.unlink()


def test_format_a_operator_does_not_override_matcher():
    """AC#7: Format A operator does not override matcher semantics (Neg)."""
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_multiline_ac7.txt"

    content = "CheckMe\n" * 5
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)
        rel_path = temp_path.relative_to(repo_root)

        # Positive equivalence: gte with = 5 notation => PASS (5 >= 5)
        ac_gte_eq = ACDefinition(
            ac_number=7,
            description="gte matcher with = notation",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="gte",
            expected="`CheckMe` = 5"
        )
        result_gte_eq = verifier.verify_code_ac(ac_gte_eq)
        assert result_gte_eq["result"] == "PASS", (
            f"Expected PASS (gte with =5, actual=5), got {result_gte_eq['result']}: {result_gte_eq.get('details', {})}"
        )

        # Positive equivalence: gte with >= 5 notation => PASS (5 >= 5)
        ac_gte_gte = ACDefinition(
            ac_number=7,
            description="gte matcher with >= notation",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="gte",
            expected="`CheckMe` >= 5"
        )
        result_gte_gte = verifier.verify_code_ac(ac_gte_gte)
        assert result_gte_gte["result"] == "PASS", (
            f"Expected PASS (gte with >=5, actual=5), got {result_gte_gte['result']}: {result_gte_gte.get('details', {})}"
        )

        # Both produce identical actual_count
        assert result_gte_eq["details"]["actual_count"] == result_gte_gte["details"]["actual_count"], (
            f"actual_count mismatch: {result_gte_eq['details']['actual_count']} vs {result_gte_gte['details']['actual_count']}"
        )

        # Negative conflict: lt matcher with >= 5 notation, actual=5 => FAIL
        # (5 is not < 5; operator >= is discarded, lt semantics apply)
        ac_lt_conflict = ACDefinition(
            ac_number=7,
            description="lt matcher with >= notation (conflict)",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="lt",
            expected="`CheckMe` >= 5"
        )
        result_lt_conflict = verifier.verify_code_ac(ac_lt_conflict)
        assert result_lt_conflict["result"] == "FAIL", (
            f"Expected FAIL (lt with >=5 notation, actual=5; operator is discarded, lt semantics: 5<5 is False), "
            f"got {result_lt_conflict['result']}: {result_lt_conflict.get('details', {})}"
        )

        print("[PASS] Format A operator does not override matcher semantics; conflict case correctly FAILs")
    finally:
        temp_path.unlink()


def test_format_a_equals_backward_compat():
    """AC#8: Existing Format A = N backward compatibility (Pos)."""
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_multiline_ac8.txt"

    content = "def foo(\n    pass\ndef foo(\n    pass\n"
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)
        rel_path = temp_path.relative_to(repo_root)

        ac = ACDefinition(
            ac_number=8,
            description="Format A = N backward compatibility",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="count_equals",
            expected="`def foo\\(` = 2"
        )

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", (
            f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        )
        assert result["details"]["actual_count"] == 2, (
            f"Expected actual_count=2, got {result['details']['actual_count']}"
        )
        print("[PASS] Existing Format A = N backward compatibility preserved")
    finally:
        temp_path.unlink()


def test_format_b_backward_compat():
    """AC#9: Existing Format B Pattern (N) backward compatibility (Pos)."""
    tmp_dir = repo_root / ".tmp"
    tmp_dir.mkdir(exist_ok=True)
    temp_path = tmp_dir / "test_multiline_ac9.txt"

    content = "Result<Unit>\nResult<Unit>\nResult<Unit>\n"
    temp_path.write_text(content, encoding='utf-8')

    try:
        verifier = ACVerifier("999", "code", repo_root)
        rel_path = temp_path.relative_to(repo_root)

        ac = ACDefinition(
            ac_number=9,
            description="Format B Pattern (N) backward compatibility",
            ac_type="code",
            method=f"Grep({rel_path})",
            matcher="count_equals",
            expected="Result<Unit> (3)"
        )

        result = verifier.verify_code_ac(ac)

        assert result["result"] == "PASS", (
            f"Expected PASS, got {result['result']}: {result.get('details', {})}"
        )
        assert result["details"]["actual_count"] == 3, (
            f"Expected actual_count=3, got {result['details']['actual_count']}"
        )
        print("[PASS] Existing Format B Pattern (N) backward compatibility preserved")
    finally:
        temp_path.unlink()
