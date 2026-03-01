"""
Tests for verify_range_coverage.py

Tests the verify_range_coverage() function with various JSON data scenarios.
"""

import json
import pytest
from pathlib import Path

from verify_range_coverage import verify_range_coverage


def test_complete_coverage(tmp_path):
    """Test that complete 0-699 coverage passes"""
    json_data = {
        "ranges": [
            {"start": 0, "end": 99, "file": "_A.ERB"},
            {"start": 100, "end": 699, "file": "_B.ERB"}
        ]
    }

    json_file = tmp_path / "test.json"
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(json_data, f)

    success, errors = verify_range_coverage(json_file)

    assert success is True
    assert len(errors) == 0


def test_gap_between_ranges(tmp_path):
    """Test that gaps are detected"""
    json_data = {
        "ranges": [
            {"start": 0, "end": 99, "file": "_A.ERB"},
            {"start": 150, "end": 699, "file": "_B.ERB"}  # Gap: 100-149
        ]
    }

    json_file = tmp_path / "test.json"
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(json_data, f)

    success, errors = verify_range_coverage(json_file)

    assert success is False
    assert len(errors) == 1
    assert "Gap detected: 100-149" in errors[0]


def test_overlapping_ranges(tmp_path):
    """Test that overlaps are detected"""
    json_data = {
        "ranges": [
            {"start": 0, "end": 150, "file": "_A.ERB"},
            {"start": 100, "end": 699, "file": "_B.ERB"}  # Overlap: 100-150
        ]
    }

    json_file = tmp_path / "test.json"
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(json_data, f)

    success, errors = verify_range_coverage(json_file)

    assert success is False
    assert len(errors) >= 1
    assert any("Overlap detected" in error for error in errors)


def test_incomplete_coverage(tmp_path):
    """Test that incomplete coverage (not reaching 699) is detected"""
    json_data = {
        "ranges": [
            {"start": 0, "end": 499, "file": "_A.ERB"}  # Ends at 499, should reach 699
        ]
    }

    json_file = tmp_path / "test.json"
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(json_data, f)

    success, errors = verify_range_coverage(json_file)

    assert success is False
    assert len(errors) == 1
    assert "Incomplete coverage" in errors[0]
    assert "ends at 499" in errors[0]


def test_exceeds_limit(tmp_path):
    """Test that coverage exceeding 699 is detected"""
    json_data = {
        "ranges": [
            {"start": 0, "end": 750, "file": "_A.ERB"}  # Exceeds 699
        ]
    }

    json_file = tmp_path / "test.json"
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(json_data, f)

    success, errors = verify_range_coverage(json_file)

    assert success is False
    assert len(errors) == 1
    assert "Coverage exceeds limit" in errors[0]


def test_empty_ranges(tmp_path):
    """Test that empty ranges list is handled"""
    json_data = {
        "ranges": []
    }

    json_file = tmp_path / "test.json"
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(json_data, f)

    success, errors = verify_range_coverage(json_file)

    assert success is False
    assert len(errors) == 1
    assert "No ranges defined" in errors[0]


def test_unsorted_ranges(tmp_path):
    """Test that unsorted ranges are handled correctly"""
    json_data = {
        "ranges": [
            {"start": 400, "end": 699, "file": "_C.ERB"},
            {"start": 0, "end": 199, "file": "_A.ERB"},
            {"start": 200, "end": 399, "file": "_B.ERB"}
        ]
    }

    json_file = tmp_path / "test.json"
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(json_data, f)

    success, errors = verify_range_coverage(json_file)

    assert success is True
    assert len(errors) == 0


def test_multiple_gaps(tmp_path):
    """Test that multiple gaps are all detected"""
    json_data = {
        "ranges": [
            {"start": 0, "end": 99, "file": "_A.ERB"},
            {"start": 150, "end": 249, "file": "_B.ERB"},  # Gap: 100-149
            {"start": 300, "end": 699, "file": "_C.ERB"}   # Gap: 250-299
        ]
    }

    json_file = tmp_path / "test.json"
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(json_data, f)

    success, errors = verify_range_coverage(json_file)

    assert success is False
    assert len(errors) == 2
    assert any("100-149" in error for error in errors)
    assert any("250-299" in error for error in errors)
