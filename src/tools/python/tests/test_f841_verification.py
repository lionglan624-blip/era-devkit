"""End-to-end verification test for F841 code-type ACs (F842 Task 5).

Runs ac-static-verifier.py --feature 841 --ac-type code via subprocess
from the repo root and asserts all 15/15 ACs pass.
"""
import subprocess
import sys
from pathlib import Path

repo_root = Path(__file__).parent.parent.parent.parent.parent


class TestF841Verification:
    """F841 code-type AC end-to-end verification."""

    def test_f841_code_acs_all_pass(self):
        """All 15 F841 code-type ACs pass after F842 fixes (AC#1).

        Runs: python src/tools/python/ac-static-verifier.py --feature 841 --ac-type code
        from the repo root.
        Asserts: exit code 0 and stdout contains '15/15 passed'.
        """
        result = subprocess.run(
            [sys.executable, "src/tools/python/ac-static-verifier.py", "--feature", "841", "--ac-type", "code"],
            cwd=str(repo_root),
            capture_output=True,
            text=True,
        )

        assert result.returncode == 0, (
            f"ac-static-verifier exited with code {result.returncode}\n"
            f"stdout: {result.stdout}\n"
            f"stderr: {result.stderr}"
        )
        assert "15/15 passed" in result.stdout, (
            f"Expected '15/15 passed' in stdout, got:\n{result.stdout}\n"
            f"stderr: {result.stderr}"
        )
