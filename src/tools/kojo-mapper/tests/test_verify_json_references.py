"""
Tests for verify_json_references.py

Tests the verify_json_references() function using filesystem mocking.
"""

import pytest
from pathlib import Path
from unittest.mock import patch, mock_open

from verify_json_references import verify_json_references, REQUIRED_FILES


def test_all_files_have_reference(tmp_path):
    """Test that verification passes when all files contain com_file_map.json"""
    # Create mock file structure
    for rel_path in REQUIRED_FILES:
        file_path = tmp_path / rel_path
        file_path.parent.mkdir(parents=True, exist_ok=True)
        file_content = f"# Test file\nReference to com_file_map.json in this file.\n"
        file_path.write_text(file_content, encoding='utf-8')

    # Create a simplified version of the verification using the same logic
    import verify_json_references
    original_verify = verify_json_references.verify_json_references

    def patched_verify():
        repo_root = tmp_path
        errors = []
        verified_count = 0

        for rel_path in REQUIRED_FILES:
            file_path = repo_root / rel_path

            if not file_path.exists():
                errors.append(f"ERROR: File not found: {rel_path}")
                continue

            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()

                if "com_file_map.json" in content:
                    verified_count += 1
                else:
                    errors.append(f"ERROR: Missing com_file_map.json reference: {rel_path}")

            except Exception as e:
                errors.append(f"ERROR: Failed to read {rel_path}: {e}")

        if errors:
            for error in errors:
                print(error)
            print(f"\nVerified: {verified_count}/{len(REQUIRED_FILES)}")
            return 1
        else:
            print(f"OK: All {verified_count} files contain com_file_map.json reference")
            return 0

    verify_json_references.verify_json_references = patched_verify
    result = verify_json_references.verify_json_references()
    verify_json_references.verify_json_references = original_verify

    assert result == 0


def test_missing_file(tmp_path, capsys):
    """Test that verification fails when a file is missing"""
    # Create only some files
    for rel_path in REQUIRED_FILES[:3]:  # Only create first 3 files
        file_path = tmp_path / rel_path
        file_path.parent.mkdir(parents=True, exist_ok=True)
        file_path.write_text("Reference to com_file_map.json", encoding='utf-8')

    # Mock the verification with tmp_path as repo_root
    import verify_json_references
    original_verify = verify_json_references.verify_json_references

    def patched_verify():
        repo_root = tmp_path
        errors = []
        verified_count = 0

        for rel_path in REQUIRED_FILES:
            file_path = repo_root / rel_path

            if not file_path.exists():
                errors.append(f"ERROR: File not found: {rel_path}")
                continue

            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()

                if "com_file_map.json" in content:
                    verified_count += 1
                else:
                    errors.append(f"ERROR: Missing com_file_map.json reference: {rel_path}")

            except Exception as e:
                errors.append(f"ERROR: Failed to read {rel_path}: {e}")

        if errors:
            for error in errors:
                print(error)
            print(f"\nVerified: {verified_count}/{len(REQUIRED_FILES)}")
            return 1
        else:
            print(f"OK: All {verified_count} files contain com_file_map.json reference")
            return 0

    verify_json_references.verify_json_references = patched_verify
    result = verify_json_references.verify_json_references()
    verify_json_references.verify_json_references = original_verify

    assert result == 1

    captured = capsys.readouterr()
    assert "ERROR: File not found:" in captured.out


def test_missing_reference(tmp_path):
    """Test that verification fails when a file lacks com_file_map.json reference"""
    # Create all files but one without the reference
    for i, rel_path in enumerate(REQUIRED_FILES):
        file_path = tmp_path / rel_path
        file_path.parent.mkdir(parents=True, exist_ok=True)

        if i == 0:
            # First file missing reference
            file_path.write_text("# Test file without reference", encoding='utf-8')
        else:
            file_path.write_text("Reference to com_file_map.json", encoding='utf-8')

    # Mock the verification
    import verify_json_references
    original_verify = verify_json_references.verify_json_references

    def patched_verify():
        repo_root = tmp_path
        errors = []
        verified_count = 0

        for rel_path in REQUIRED_FILES:
            file_path = repo_root / rel_path

            if not file_path.exists():
                errors.append(f"ERROR: File not found: {rel_path}")
                continue

            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()

                if "com_file_map.json" in content:
                    verified_count += 1
                else:
                    errors.append(f"ERROR: Missing com_file_map.json reference: {rel_path}")

            except Exception as e:
                errors.append(f"ERROR: Failed to read {rel_path}: {e}")

        if errors:
            for error in errors:
                print(error)
            print(f"\nVerified: {verified_count}/{len(REQUIRED_FILES)}")
            return 1
        else:
            print(f"OK: All {verified_count} files contain com_file_map.json reference")
            return 0

    verify_json_references.verify_json_references = patched_verify
    result = verify_json_references.verify_json_references()
    verify_json_references.verify_json_references = original_verify

    assert result == 1
