"""
Tests for verify_com_map.py

Tests the helper functions using inline data (no filesystem dependencies).
"""

import pytest

from verify_com_map import (
    load_com_file_map,
    load_skip_combinations,
    get_exclusively_unimplemented_files
)


def test_load_com_file_map_all_implemented():
    """Test loading COM file map with all ranges implemented"""
    data = {
        "ranges": [
            {"start": 0, "end": 9, "file": "_A.ERB", "implemented": True},
            {"start": 10, "end": 19, "file": "_B.ERB", "implemented": True}
        ]
    }

    result = load_com_file_map(data)

    # Should map COM 0-9 to _A.ERB and 10-19 to _B.ERB
    assert result[0] == "_A.ERB"
    assert result[9] == "_A.ERB"
    assert result[10] == "_B.ERB"
    assert result[19] == "_B.ERB"
    assert len(result) == 20


def test_load_com_file_map_skip_unimplemented():
    """Test that unimplemented ranges are skipped"""
    data = {
        "ranges": [
            {"start": 0, "end": 9, "file": "_A.ERB", "implemented": True},
            {"start": 10, "end": 19, "file": "_B.ERB", "implemented": False},
            {"start": 20, "end": 29, "file": "_C.ERB", "implemented": True}
        ]
    }

    result = load_com_file_map(data)

    # Should only include implemented ranges
    assert 0 in result
    assert 9 in result
    assert 10 not in result  # Unimplemented
    assert 19 not in result  # Unimplemented
    assert 20 in result
    assert 29 in result
    assert len(result) == 20  # 10 from first range + 10 from third range


def test_load_com_file_map_default_implemented():
    """Test that missing 'implemented' field defaults to True"""
    data = {
        "ranges": [
            {"start": 0, "end": 9, "file": "_A.ERB"}  # No 'implemented' field
        ]
    }

    result = load_com_file_map(data)

    # Should include range (defaults to implemented=True)
    assert len(result) == 10
    assert result[0] == "_A.ERB"


def test_load_skip_combinations():
    """Test loading skip combinations"""
    data = {
        "skip_combinations": [
            {"character": "K1", "file": "_A.ERB"},
            {"character": "K2", "file": "_B.ERB"},
            {"character": "K3", "file": "_A.ERB"}
        ]
    }

    result = load_skip_combinations(data)

    assert len(result) == 3
    assert ("K1", "_A.ERB") in result
    assert ("K2", "_B.ERB") in result
    assert ("K3", "_A.ERB") in result
    assert ("K1", "_B.ERB") not in result


def test_load_skip_combinations_empty():
    """Test loading empty skip combinations"""
    data = {
        "skip_combinations": []
    }

    result = load_skip_combinations(data)

    assert len(result) == 0
    assert isinstance(result, set)


def test_get_exclusively_unimplemented_files():
    """Test identifying files that are ONLY in unimplemented ranges"""
    data = {
        "ranges": [
            {"start": 0, "end": 9, "file": "_A.ERB", "implemented": True},
            {"start": 10, "end": 19, "file": "_B.ERB", "implemented": False},
            {"start": 20, "end": 29, "file": "_C.ERB", "implemented": False}
        ]
    }

    result = get_exclusively_unimplemented_files(data)

    # _B.ERB and _C.ERB are only in unimplemented ranges
    assert "_B.ERB" in result
    assert "_C.ERB" in result
    assert "_A.ERB" not in result  # This is implemented
    assert len(result) == 2


def test_get_exclusively_unimplemented_files_shared():
    """Test that files shared between implemented and unimplemented are excluded"""
    data = {
        "ranges": [
            {"start": 0, "end": 9, "file": "_A.ERB", "implemented": True},
            {"start": 10, "end": 19, "file": "_A.ERB", "implemented": False},  # Same file
            {"start": 20, "end": 29, "file": "_B.ERB", "implemented": False}
        ]
    }

    result = get_exclusively_unimplemented_files(data)

    # _A.ERB is in both implemented and unimplemented, so should be excluded
    assert "_A.ERB" not in result
    assert "_B.ERB" in result  # Only in unimplemented
    assert len(result) == 1


def test_get_exclusively_unimplemented_files_all_implemented():
    """Test when all ranges are implemented"""
    data = {
        "ranges": [
            {"start": 0, "end": 9, "file": "_A.ERB", "implemented": True},
            {"start": 10, "end": 19, "file": "_B.ERB", "implemented": True}
        ]
    }

    result = get_exclusively_unimplemented_files(data)

    assert len(result) == 0


def test_get_exclusively_unimplemented_files_default_implemented():
    """Test that missing 'implemented' field defaults to True"""
    data = {
        "ranges": [
            {"start": 0, "end": 9, "file": "_A.ERB"},  # Defaults to implemented=True
            {"start": 10, "end": 19, "file": "_B.ERB", "implemented": False}
        ]
    }

    result = get_exclusively_unimplemented_files(data)

    assert "_A.ERB" not in result  # Defaults to implemented
    assert "_B.ERB" in result  # Explicitly unimplemented
    assert len(result) == 1
