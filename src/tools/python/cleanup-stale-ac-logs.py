#!/usr/bin/env python3
"""
One-time bulk cleanup of stale AC logs from [DONE] features.
Deletes feature-scoped log directories and files for non-active features.

Usage:
    python tools/cleanup-stale-ac-logs.py [--dry-run]
"""

import argparse
import re
import shutil
import sys
from pathlib import Path


def get_active_features(repo_root: Path) -> set[str]:
    """Extract active feature IDs from index-features.md.

    Returns set of feature IDs (numeric strings) that are NOT [DONE] or [CANCELLED].
    Raises ValueError if zero features parsed (safety check for bulk deletion).
    """
    index_path = repo_root / "dev" / "planning" / "index-features.md"
    active_features = set()
    in_active_section = False

    with open(index_path, 'r', encoding='utf-8') as f:
        for line in f:
            line = line.strip()
            if line.startswith("## Active Features"):
                in_active_section = True
                continue
            if in_active_section and line.startswith("## ") and "Active Features" not in line:
                break

            if not in_active_section:
                continue

            # Parse table rows: "| {ID} | [{STATUS}] | ..."
            if line.startswith("|") and not line.startswith("|:") and not line.startswith("| ID"):
                parts = [p.strip() for p in line.split("|")]
                if len(parts) >= 3:
                    id_str = parts[1]
                    status = parts[2]
                    if id_str.isdigit() and "[DONE]" not in status and "[CANCELLED]" not in status:
                        active_features.add(id_str)

    if not active_features:
        raise ValueError("No active features parsed from index file — refusing to delete")

    return active_features


def cleanup_feature_logs(ac_dir: Path, active_features: set[str], dry_run: bool) -> dict:
    """Delete ac/{type}/feature-{ID}/ directories where ID is NOT in active set."""
    stats = {"dirs_deleted": 0, "dirs_skipped": 0}

    # Standard AC type directories
    for type_dir in sorted(ac_dir.iterdir()):
        if not type_dir.is_dir():
            continue
        # Skip non-standard directories (handled by cleanup_legacy_dirs)
        if type_dir.name in ("f266", "f267", "feature-727", "test"):
            continue

        for feature_dir in sorted(type_dir.iterdir()):
            if not feature_dir.is_dir():
                continue
            if not feature_dir.name.startswith("feature-"):
                continue

            feature_id = feature_dir.name.replace("feature-", "")
            if feature_id not in active_features:
                if dry_run:
                    print(f"  [DRY-RUN] Would delete non-active feature dir: {feature_dir.relative_to(ac_dir.parent.parent.parent)}")
                else:
                    shutil.rmtree(feature_dir)
                    print(f"  Deleted: {feature_dir.relative_to(ac_dir.parent.parent.parent)}")
                stats["dirs_deleted"] += 1
            else:
                stats["dirs_skipped"] += 1

    return stats


def cleanup_engine_trx(ac_dir: Path, active_features: set[str], dry_run: bool) -> dict:
    """Delete root-level engine TRX files for non-active features and orphaned generic TRX."""
    stats = {"files_deleted": 0, "files_skipped": 0}
    engine_dir = ac_dir / "engine"
    if not engine_dir.exists():
        return stats

    pattern = re.compile(r"^feature-(\d+)")

    for trx_file in sorted(engine_dir.glob("*.trx")):
        # Check if it's a feature-associated TRX
        match = pattern.match(trx_file.stem)
        if match:
            feature_id = match.group(1)
            if feature_id not in active_features:
                if dry_run:
                    print(f"  [DRY-RUN] Would delete non-active feature TRX: {trx_file.relative_to(ac_dir.parent.parent.parent)}")
                else:
                    trx_file.unlink()
                    print(f"  Deleted: {trx_file.relative_to(ac_dir.parent.parent.parent)}")
                stats["files_deleted"] += 1
            else:
                stats["files_skipped"] += 1
        else:
            # Generic TRX files (displaymode-tests.trx, KojoComparer-*.trx, test-result.trx)
            # These are project-wide test results, not feature-scoped
            # KojoComparer TRX files dispositioned as stale (Task#1) — safe to delete
            if dry_run:
                print(f"  [DRY-RUN] Would delete orphaned generic TRX: {trx_file.relative_to(ac_dir.parent.parent.parent)}")
            else:
                trx_file.unlink()
                print(f"  Deleted orphaned: {trx_file.relative_to(ac_dir.parent.parent.parent)}")
            stats["files_deleted"] += 1

    return stats


def cleanup_legacy_dirs(ac_dir: Path, dry_run: bool) -> dict:
    """Delete non-standard naming directories."""
    stats = {"dirs_deleted": 0}
    legacy_dirs = ["f266", "f267", "feature-727", "test"]

    for dirname in legacy_dirs:
        target = ac_dir / dirname
        if target.exists():
            if dry_run:
                print(f"  [DRY-RUN] Would delete legacy dir: {target.relative_to(ac_dir.parent.parent.parent)}")
            else:
                shutil.rmtree(target)
                print(f"  Deleted legacy: {target.relative_to(ac_dir.parent.parent.parent)}")
            stats["dirs_deleted"] += 1

    return stats


def main():
    parser = argparse.ArgumentParser(description="Cleanup stale AC logs from [DONE] features")
    parser.add_argument("--dry-run", action="store_true", help="Preview deletions without executing")
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parent.parent.parent.parent
    ac_dir = repo_root / "Game" / "logs" / "prod" / "ac"

    if not ac_dir.exists():
        print(f"Error: AC directory not found: {ac_dir}", file=sys.stderr)
        sys.exit(1)

    print("=== AC Log Cleanup ===")
    if args.dry_run:
        print("Mode: DRY-RUN (no deletions)")
    else:
        print("Mode: LIVE (deleting files)")

    # Get active features (inverse approach)
    active_features = get_active_features(repo_root)
    print(f"Active features ({len(active_features)}): {', '.join(sorted(active_features, key=int))}")
    print(f"Deleting logs for non-active feature directories")
    print()

    # Execute cleanup
    print("--- Feature log directories ---")
    feature_stats = cleanup_feature_logs(ac_dir, active_features, args.dry_run)

    print("\n--- Engine TRX files ---")
    engine_stats = cleanup_engine_trx(ac_dir, active_features, args.dry_run)

    print("\n--- Legacy directories ---")
    legacy_stats = cleanup_legacy_dirs(ac_dir, args.dry_run)

    # Report
    print("\n=== Summary ===")
    total_dirs = feature_stats["dirs_deleted"] + legacy_stats["dirs_deleted"]
    total_files = engine_stats["files_deleted"]
    print(f"Directories {'would be ' if args.dry_run else ''}deleted: {total_dirs}")
    print(f"TRX files {'would be ' if args.dry_run else ''}deleted: {total_files}")
    print(f"Feature dirs skipped (active): {feature_stats['dirs_skipped']}")
    print(f"TRX files skipped (active): {engine_stats['files_skipped']}")


if __name__ == "__main__":
    main()
