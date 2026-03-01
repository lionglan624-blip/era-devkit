#!/usr/bin/env python3
"""
Test ac-static-verifier.py Unicode emoji and multi-byte character handling (AC#7-8).

This test verifies that the tool correctly handles Unicode emoji and multi-byte
characters in Expected values and file content.
"""

import sys
import tempfile
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


def test_emoji_contains():
    """Test single emoji in Expected value matches file content (AC#7)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with emoji
        test_file = tmpdir_path / "status.md"
        test_file.write_text("Status: ✅ PASS\n", encoding='utf-8')

        # Create AC definition with emoji in Expected
        ac = ACDefinition(
            ac_number=1,
            description="Test emoji contains",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="✅"
        )

        # Create verifier instance
        verifier = ACVerifier("631", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS for emoji pattern, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Single emoji in Expected works correctly")


def test_emoji_with_text():
    """Test emoji with surrounding text (AC#7 edge case)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with emoji and text
        test_file = tmpdir_path / "status.md"
        test_file.write_text("Test result: ✅ PASS 🎉\n", encoding='utf-8')

        # Create AC definition with emoji and text
        ac = ACDefinition(
            ac_number=2,
            description="Test emoji with text",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="✅ PASS 🎉"
        )

        # Create verifier instance
        verifier = ACVerifier("631", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS for emoji with text, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Emoji with surrounding text works correctly")


def test_multibyte_unicode():
    """Test multi-byte Unicode characters (Japanese, math symbols) (AC#8)."""
    test_cases = [
        ("Japanese hiragana", "こんにちは世界"),
        ("Japanese kanji", "漢字テスト"),
        ("Mathematical symbols", "∑∫∂√∞"),
        ("Greek letters", "αβγδε"),
        ("Box drawing", "┌─┐│└┘"),
        ("CJK mixed", "日本語テスト123"),
    ]

    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        for description, unicode_text in test_cases:
            # Create test file with multi-byte Unicode
            test_file = tmpdir_path / f"test_{description.replace(' ', '_')}.txt"
            test_file.write_text(f"Content: {unicode_text}\n", encoding='utf-8')

            # Create AC definition with multi-byte Unicode
            ac = ACDefinition(
                ac_number=3,
                description=f"Test {description}",
                ac_type="code",
                method=f"Grep({test_file})",
                matcher="contains",
                expected=unicode_text
            )

            # Create verifier instance
            verifier = ACVerifier("631", "code", tmpdir_path)

            # Verify AC
            result = verifier.verify_code_ac(ac)

            # Verify result
            assert result["result"] == "PASS", (
                f"Expected PASS for {description}, got {result['result']}: "
                f"{result.get('details', {})} (text: {unicode_text})"
            )

        print(f"[PASS] Multi-byte Unicode characters work correctly ({len(test_cases)} cases)")


def test_emoji_in_method_column():
    """Test emoji in AC description (edge case - not in verification path)."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file
        test_file = tmpdir_path / "test.txt"
        test_file.write_text("Status: PASS\n", encoding='utf-8')

        # Create AC definition with emoji in description
        ac = ACDefinition(
            ac_number=4,
            description="Test ✅ emoji in description",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="PASS"
        )

        # Create verifier instance
        verifier = ACVerifier("631", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS when emoji in description, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Emoji in AC description works correctly")


def test_emoji_not_found():
    """Test that emoji correctly returns FAIL when not found."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file without emoji
        test_file = tmpdir_path / "status.md"
        test_file.write_text("Status: PASS\n", encoding='utf-8')

        # Create AC definition with emoji that doesn't exist
        ac = ACDefinition(
            ac_number=5,
            description="Test emoji not found",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected="✅"
        )

        # Create verifier instance
        verifier = ACVerifier("631", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "FAIL", (
            f"Expected FAIL when emoji not found, got {result['result']}"
        )
        print(f"[PASS] Emoji correctly returns FAIL when not found")


def test_combined_emoji_multibyte():
    """Test combination of emoji and multi-byte characters."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir_path = Path(tmpdir)

        # Create test file with mixed Unicode
        test_file = tmpdir_path / "mixed.md"
        combined_text = "テスト結果: ✅ 成功 🎉"
        test_file.write_text(f"{combined_text}\n", encoding='utf-8')

        # Create AC definition with combined pattern
        ac = ACDefinition(
            ac_number=6,
            description="Test combined emoji and multibyte",
            ac_type="code",
            method=f"Grep({test_file})",
            matcher="contains",
            expected=combined_text
        )

        # Create verifier instance
        verifier = ACVerifier("631", "code", tmpdir_path)

        # Verify AC
        result = verifier.verify_code_ac(ac)

        # Verify result
        assert result["result"] == "PASS", (
            f"Expected PASS for combined Unicode, got {result['result']}: "
            f"{result.get('details', {})}"
        )
        print(f"[PASS] Combined emoji and multi-byte characters work correctly")


if __name__ == "__main__":
    print("Running Unicode emoji and multi-byte character tests (AC#7-8)...")
    try:
        test_emoji_contains()
        test_emoji_with_text()
        test_multibyte_unicode()
        test_emoji_in_method_column()
        test_emoji_not_found()
        test_combined_emoji_multibyte()
        print("\nAll emoji and Unicode tests passed!")
        sys.exit(0)
    except AssertionError as e:
        print(f"\n[FAIL] Test failed: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"\n[ERROR] Unexpected error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        sys.exit(1)
