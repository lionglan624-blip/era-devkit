#!/usr/bin/env python3
"""
Log Verification Script for ERA Game Testing
Scans logs/prod/ directory and verifies all test results.

Usage:
    python tools/verify-logs.py [--dir _out/logs/prod]

Exit codes:
    0 = All tests passed
    1 = One or more tests failed
"""

import argparse
import json
import re
import sys
from pathlib import Path
import xml.etree.ElementTree as ET


def get_active_features(repo_root: Path) -> set[str] | None:
    """Extract active feature IDs from index-features.md.

    Returns set of feature IDs (numeric strings) that are NOT [DONE] or [CANCELLED].
    Returns None if index file is unreadable or zero features parsed (sentinel for WARN).
    """
    index_path = repo_root / "pm" / "index-features.md"
    if not index_path.exists():
        print(f"WARN: Index file not found: {index_path}", file=sys.stderr)
        return None

    active_features = set()
    in_active_section = False

    try:
        with open(index_path, 'r', encoding='utf-8') as f:
            for line in f:
                line = line.strip()
                if line.startswith("## Active Features"):
                    in_active_section = True
                    continue
                if in_active_section and line.startswith("## ") and "Active Features" not in line:
                    break  # Past Active Features section

                if not in_active_section:
                    continue

                # Parse table rows: "| {ID} | [{STATUS}] | ..."
                if line.startswith("|") and not line.startswith("|:") and not line.startswith("| ID"):
                    parts = [p.strip() for p in line.split("|")]
                    # parts[0] is empty (before first |), parts[1] is ID, parts[2] is Status
                    if len(parts) >= 3:
                        id_str = parts[1]
                        status = parts[2]
                        if id_str.isdigit() and "[DONE]" not in status and "[CANCELLED]" not in status:
                            active_features.add(id_str)
    except OSError as e:
        print(f"WARN: Cannot read index file: {e}", file=sys.stderr)
        return None

    if not active_features:
        print("WARN: No active features parsed from index file", file=sys.stderr)
        return None

    return active_features


def verify_ac_logs(prod_dir: Path, scope: str = "all"):
    """Verify AC test logs (JSON format).

    Args:
        prod_dir: Production logs directory
        scope: Verification scope ("all", "feature:{ID}")
    """
    ac_dir = prod_dir / "ac"
    if not ac_dir.exists():
        return {"passed": 0, "total": 0, "failed_files": []}

    # Derive repo_root from script location (not prod_dir.parent which is user-configurable via --dir)
    repo_root = Path(__file__).resolve().parent.parent.parent.parent

    active_features = None
    if scope == "all":
        active_features = get_active_features(repo_root)
        if active_features is None:
            # Fallback: WARN status, do not revert to scanning all logs
            return {"status": "WARN", "passed": 0, "total": 0, "failed_files": []}
        print(f"Filtering by active features: {len(active_features)} active", file=sys.stderr)

    # Glob all *-result.json files
    if scope == "all":
        result_files = list(ac_dir.glob("**/*-result.json"))
    elif scope.startswith("feature:"):
        feature_id = scope.split(":", 1)[1]
        result_files = list(ac_dir.glob(f"**/feature-{feature_id}/*-result.json"))
    else:
        result_files = list(ac_dir.glob(f"**/{scope}/*-result.json"))

    # POST-FILTER: When scope == "all", filter by active features
    if scope == "all" and active_features is not None:
        filtered_files = []
        for result_file in result_files:
            # Extract feature ID from path: ac/{type}/feature-{ID}/{type}-result.json
            parts = result_file.parts
            for part in parts:
                if part.startswith("feature-"):
                    feature_id = part.replace("feature-", "")
                    if feature_id in active_features:
                        filtered_files.append(result_file)
                    break
        result_files = filtered_files

    passed = 0
    total = 0
    failed_files = []

    for result_file in result_files:
        try:
            with open(result_file, 'r', encoding='utf-8-sig') as f:
                data = json.load(f)
                if "summary" in data:
                    # Count total tests
                    file_total = data["summary"].get("total", 0)
                    file_failed = data["summary"].get("failed", 0)
                    total += file_total
                    passed += (file_total - file_failed)

                    if file_failed > 0:
                        failed_files.append(str(result_file.relative_to(prod_dir.parent)))
        except (json.JSONDecodeError, KeyError, OSError) as e:
            # Treat malformed files as failures
            failed_files.append(str(result_file.relative_to(prod_dir.parent)))

    return {"passed": passed, "total": total, "failed_files": failed_files}


def extract_feature_id(trx_file: Path) -> str | None:
    """Extract feature ID from TRX file path or filename.

    Args:
        trx_file: Path to TRX file

    Returns:
        Feature ID (numeric string) or None if not feature-associated
    """
    # Check if file is in a feature-{ID}/ subdirectory
    for part in trx_file.parts:
        if part.startswith("feature-"):
            return part.replace("feature-", "")

    # Check root-level filename pattern: feature-{ID}-*.trx
    match = re.match(r"^feature-(\d+)", trx_file.stem)
    if match:
        return match.group(1)

    return None


def verify_engine_logs(prod_dir: Path, scope: str = "all"):
    """Verify Engine test logs (TRX/XML format).

    Args:
        prod_dir: Production logs directory
        scope: Verification scope ("all", "feature:{ID}", "engine")
    """
    engine_dir = prod_dir / "ac" / "engine"
    if not engine_dir.exists():
        return {"passed": 0, "total": 0, "failed_files": []}

    # Derive repo_root from script location (not prod_dir.parent which is user-configurable via --dir)
    repo_root = Path(__file__).resolve().parent.parent.parent.parent

    active_features = None
    if scope == "all":
        active_features = get_active_features(repo_root)
        if active_features is None:
            # Fallback: WARN status, do not revert to scanning all logs
            return {"status": "WARN", "passed": 0, "total": 0, "failed_files": []}
        print(f"Filtering engine logs by active features: {len(active_features)} active", file=sys.stderr)

    # Glob all TRX files (both root-level and subdirectories)
    trx_files = list(engine_dir.glob("**/*.trx"))

    # POST-FILTER: When scope filtering is active, filter TRX files by feature ID
    if scope == "all" and active_features is not None:
        filtered_files = []
        for trx_file in trx_files:
            feature_id = extract_feature_id(trx_file)
            if feature_id and feature_id in active_features:
                filtered_files.append(trx_file)
            # else: non-feature-associated TRX (e.g., displaymode-tests.trx, test-result.trx)
            # Skip these for scope="all" (only include in scope="engine")
        trx_files = filtered_files
    elif scope.startswith("feature:"):
        # Feature-scoped: only include files matching feature ID
        target_feature_id = scope.split(":", 1)[1]
        filtered_files = []
        for trx_file in trx_files:
            feature_id = extract_feature_id(trx_file)
            if feature_id == target_feature_id:
                filtered_files.append(trx_file)
        trx_files = filtered_files
    # else: scope == "engine", scan all TRX files (no filter)
    passed = 0
    total = 0
    failed_files = []

    for trx_file in trx_files:
        try:
            tree = ET.parse(trx_file)
            root = tree.getroot()

            # TRX namespace handling
            ns = {'trx': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}

            # Find all UnitTestResult elements
            results = root.findall('.//trx:UnitTestResult', ns)
            for result in results:
                total += 1
                outcome = result.get('outcome', '')
                if outcome == 'Passed':
                    passed += 1
                else:
                    # Record file if any test failed
                    if str(trx_file.relative_to(prod_dir.parent)) not in failed_files:
                        failed_files.append(str(trx_file.relative_to(prod_dir.parent)))
        except (ET.ParseError, OSError) as e:
            failed_files.append(str(trx_file.relative_to(prod_dir.parent)))

    return {"passed": passed, "total": total, "failed_files": failed_files}


def format_result(label: str, result: dict):
    """Format result in ac-tester compatible format."""
    passed = result["passed"]
    total = result["total"]
    failed_files = result["failed_files"]

    if len(failed_files) == 0:
        return f"{label}:         OK:{passed}/{total}", []
    else:
        failed_count = len(failed_files)
        lines = [f"{label}:         ERR:{failed_count}|{total}"]
        for file in failed_files:
            lines.append(f"  FAIL: {file}")
        return lines[0], lines[1:]


def main():
    parser = argparse.ArgumentParser(description="Verify ERA game test logs")
    parser.add_argument(
        "--dir",
        default="_out/logs/prod",
        help="Path to prod logs directory (default: _out/logs/prod)"
    )
    parser.add_argument(
        "--scope",
        default="all",
        help="Verification scope: feature:ID, engine, all"
    )
    args = parser.parse_args()

    prod_dir = Path(args.dir)
    if not prod_dir.exists():
        print(f"Error: Directory not found: {prod_dir}", file=sys.stderr)
        sys.exit(1)

    # Parse scope
    scope = args.scope
    check_ac = False
    check_engine = False
    ac_label = "AC"

    if scope == "all":
        check_ac = True
        check_engine = True
        print("Scope: active features only")
    elif scope == "engine":
        check_engine = True
    elif scope.startswith("feature:"):
        check_ac = True
        check_engine = True
        feature_id = scope.split(":", 1)[1]
        ac_label = f"Feature-{feature_id}"

    # Verify log types based on scope
    ac_result = {"passed": 0, "total": 0, "failed_files": []}
    engine_result = {"passed": 0, "total": 0, "failed_files": []}

    if check_ac:
        ac_result = verify_ac_logs(prod_dir, scope)
        if ac_result.get("status") == "WARN":
            print("WARN: Cannot determine active features - lifecycle filtering unavailable")
    if check_engine:
        engine_result = verify_engine_logs(prod_dir, scope)
        if engine_result.get("status") == "WARN":
            print("WARN: Cannot determine active features for engine logs")

    # Format output
    print("=== Log Verification ===")

    if check_ac:
        ac_line, ac_details = format_result(ac_label, ac_result)
        print(ac_line)
        for detail in ac_details:
            print(detail)

    if check_engine:
        engine_line, engine_details = format_result("Engine", engine_result)
        print(engine_line)
        for detail in engine_details:
            print(detail)

    print("-------------------------")

    # Calculate total
    total_passed = ac_result["passed"] + engine_result["passed"]
    total_tests = ac_result["total"] + engine_result["total"]
    total_failed = len(ac_result["failed_files"]) + len(engine_result["failed_files"])

    if total_failed == 0:
        print(f"Result:     OK:{total_passed}/{total_tests}")
        sys.exit(0)
    else:
        print(f"Result:     ERR:{total_failed}|{total_tests}")
        sys.exit(1)


if __name__ == "__main__":
    main()
