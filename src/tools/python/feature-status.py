#!/usr/bin/env python3
"""
Feature Status Management Tool

Automates feature status transitions, index synchronization, and
dependency cascade updates. Replaces manual Edit() calls in
initializer, finalizer, FL workflow, and FC commands.

Usage:
    python tools/feature-status.py set <ID> <STATUS> [--fl-reviewed] [--dry-run]
    python tools/feature-status.py query <ID> [<ID>...]
    python tools/feature-status.py deps <ID> [--sync] [--dry-run]
    python tools/feature-status.py ac-check <ID>
    python tools/feature-status.py ac-renumber <ID> [--dry-run] [--force]
    python tools/feature-status.py ac-insert <ID> <AFTER> [--dry-run] [--force]
    python tools/feature-status.py ac-delete <ID> <AC#> [--dry-run] [--force]
    python tools/feature-status.py ac-fix <ID> <AC#> [--expected VAL] [--description VAL] [--dry-run]

Exit codes:
    0 = Success
    1 = Error
"""

import argparse
import re
import sys
from datetime import datetime, timezone
from pathlib import Path
import ac_ops

REPO_ROOT = Path(__file__).resolve().parent.parent.parent.parent
PLANNING_DIR = REPO_ROOT / "pm"
AGENTS_DIR = PLANNING_DIR / "features"
INDEX_PATH = PLANNING_DIR / "index-features.md"
HISTORY_PATH = PLANNING_DIR / "index-features-history.md"

VALID_STATUSES = {"DRAFT", "PROPOSED", "REVIEWED", "WIP", "BLOCKED", "DONE", "CANCELLED"}
COMPLETION_STATUSES = {"DONE", "CANCELLED"}

STATUS_RE = re.compile(r"^## Status: \[(\w+)\]")
DEP_ROW_RE = re.compile(
    r"^\|\s*(Predecessor|Blocker|Successor|Related)\s*\|\s*F(\d+)\s*\|\s*\[(\w+)\]\s*\|"
)


# ── Feature file operations ─────────────────────────────────────────────────


def feature_path(fid: str) -> Path:
    return AGENTS_DIR / f"feature-{fid}.md"


def read_feature_status(fid: str) -> str | None:
    """Read just the Status from a feature file."""
    path = feature_path(fid)
    if not path.exists():
        return None
    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            m = STATUS_RE.match(line.strip())
            if m:
                return m.group(1)
    return None


def set_feature_status(fid: str, new_status: str, dry_run: bool = False) -> bool:
    """Update the Status line in a feature file."""
    path = feature_path(fid)
    text = path.read_text(encoding="utf-8")
    new_text, count = re.subn(
        r"^## Status: \[\w+\]",
        f"## Status: [{new_status}]",
        text,
        count=1,
        flags=re.MULTILINE,
    )
    if count == 0 or new_text == text:
        return False
    if not dry_run:
        path.write_text(new_text, encoding="utf-8")
    return True


def add_fl_reviewed_marker(fid: str, dry_run: bool = False) -> bool:
    """Insert or update fl-reviewed HTML comment after Status line."""
    path = feature_path(fid)
    lines = path.read_text(encoding="utf-8").splitlines(keepends=True)
    timestamp = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    marker = f"<!-- fl-reviewed: {timestamp} -->\n"

    for i, line in enumerate(lines):
        if STATUS_RE.match(line.strip()):
            if i + 1 < len(lines) and "<!-- fl-reviewed:" in lines[i + 1]:
                lines[i + 1] = marker
            else:
                lines.insert(i + 1, marker)
            if not dry_run:
                path.write_text("".join(lines), encoding="utf-8")
            return True
    return False


def get_feature_dependencies(fid: str) -> list[dict]:
    """Parse Dependencies table from a feature file."""
    path = feature_path(fid)
    if not path.exists():
        return []

    deps = []
    in_deps = False
    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            stripped = line.strip()
            if stripped == "## Dependencies":
                in_deps = True
                continue
            if in_deps and stripped.startswith("## "):
                break
            if in_deps:
                m = DEP_ROW_RE.match(stripped)
                if m:
                    deps.append(
                        {"type": m.group(1), "id": m.group(2), "status": m.group(3)}
                    )
    return deps


def update_dep_status_in_file(
    fid: str, dep_id: str, new_status: str, dry_run: bool = False
) -> bool:
    """Update a specific dependency's status in a feature's Dependencies table."""
    path = feature_path(fid)
    text = path.read_text(encoding="utf-8")

    # Match dep row containing the specific feature ID and replace status
    pattern = re.compile(
        r"^(\|\s*(?:Predecessor|Blocker|Successor|Related)\s*\|\s*F"
        + dep_id
        + r"\s*\|\s*)\[(\w+)\]",
        re.MULTILINE,
    )
    new_text, count = pattern.subn(rf"\g<1>[{new_status}]", text, count=1)
    if count == 0 or new_text == text:
        return False
    if not dry_run:
        path.write_text(new_text, encoding="utf-8")
    return True


# ── Index operations ─────────────────────────────────────────────────────────


def read_index() -> list[str]:
    return INDEX_PATH.read_text(encoding="utf-8").splitlines(keepends=True)


def write_index(lines: list[str], dry_run: bool = False) -> None:
    if not dry_run:
        INDEX_PATH.write_text("".join(lines), encoding="utf-8")


def find_index_row(lines: list[str], fid: str) -> int | None:
    """Find the line index of a feature row in the index."""
    pat = re.compile(r"^\|\s*F?" + fid + r"\s*\|")
    for i, line in enumerate(lines):
        if pat.match(line):
            return i
    return None


def update_index_status(lines: list[str], fid: str, new_status: str) -> bool:
    """Update a feature's status column in an index table row."""
    pat = re.compile(r"^(\|\s*F?" + fid + r"\s*\|\s*)\[\w+\]")
    for i, line in enumerate(lines):
        m = pat.match(line)
        if m:
            lines[i] = pat.sub(rf"\g<1>[{new_status}]", line, count=1)
            return True
    return False


def extract_title_from_index_row(line: str) -> str:
    """Extract title (Name column) from an Active Features row."""
    parts = [p.strip() for p in line.split("|")]
    # | ID | Status | Name | Depends On | Links | → parts[3] = Name
    if len(parts) >= 4:
        return parts[3]
    return ""


def get_feature_title(fid: str) -> str:
    """Extract the title from a feature file's H1 heading."""
    path = feature_path(fid)
    if not path.exists():
        return f"Feature {fid}"
    with open(path, "r", encoding="utf-8") as f:
        first_line = f.readline().strip()
    m = re.match(r"^# Feature \d+:\s*(.+)", first_line)
    return m.group(1) if m else f"Feature {fid}"


def move_to_recently_completed(
    lines: list[str], fid: str, status_emoji: str = "\u2705"
) -> bool:
    """Remove row from Active and add to Recently Completed."""
    row_idx = find_index_row(lines, fid)
    if row_idx is None:
        return False

    removed_line = lines[row_idx]
    title = extract_title_from_index_row(removed_line) or get_feature_title(fid)
    lines.pop(row_idx)

    rc_row = f"| {fid} | {status_emoji} | {title} | [feature-{fid}.md](feature-{fid}.md) |\n"

    # Find insertion point in Recently Completed (before first data row)
    in_rc = False
    for i, line in enumerate(lines):
        if "## Recently Completed" in line:
            in_rc = True
            continue
        if in_rc:
            stripped = line.strip()
            # Skip comments, blanks, table header, separator
            if (
                stripped.startswith("<!--")
                or stripped == ""
                or stripped.startswith("| ID")
                or stripped.startswith("|:")
            ):
                continue
            # Insert before first data row or section boundary
            lines.insert(i, rc_row)
            return True

    return False


def unbold_depends_on(lines: list[str], fid: str) -> bool:
    """Remove bold from **F{ID}** in Depends On columns."""
    pat = re.compile(r"\*\*F" + fid + r"\*\*")
    changed = False
    for i, line in enumerate(lines):
        if pat.search(line):
            lines[i] = pat.sub(f"F{fid}", line)
            changed = True
    return changed


def count_recently_completed(lines: list[str]) -> int:
    count = 0
    in_rc = False
    for line in lines:
        if "## Recently Completed" in line:
            in_rc = True
            continue
        if in_rc and line.strip().startswith("## "):
            break
        if in_rc and re.match(r"^\|\s*\d+\s*\|", line.strip()):
            count += 1
    return count


def rotate_history(lines: list[str], dry_run: bool = False) -> str | None:
    """If Recently Completed > 6, move oldest to history. Returns moved ID."""
    if count_recently_completed(lines) <= 6:
        return None

    # Find last (oldest) data row in Recently Completed
    in_rc = False
    last_row_idx = None
    for i, line in enumerate(lines):
        if "## Recently Completed" in line:
            in_rc = True
            continue
        if in_rc and line.strip().startswith("## "):
            break
        if in_rc and re.match(r"^\|\s*\d+\s*\|", line.strip()):
            last_row_idx = i

    if last_row_idx is None:
        return None

    parts = [p.strip() for p in lines[last_row_idx].split("|")]
    if len(parts) < 4:
        return None

    moved_id = parts[1]
    moved_name = parts[3]
    lines.pop(last_row_idx)

    if not dry_run and HISTORY_PATH.exists():
        hist_lines = HISTORY_PATH.read_text(encoding="utf-8").splitlines(keepends=True)
        for j, hl in enumerate(hist_lines):
            if "## Completed Features" in hl:
                k = j + 1
                while k < len(hist_lines) and (
                    hist_lines[k].strip().startswith("| ID")
                    or hist_lines[k].strip().startswith("|:")
                ):
                    k += 1
                today = datetime.now().strftime("%Y-%m-%d")
                hist_row = f"| **{moved_id}** | [DONE] | {moved_name} | (moved from Recently Completed {today}) |\n"
                hist_lines.insert(k, hist_row)
                HISTORY_PATH.write_text("".join(hist_lines), encoding="utf-8")
                break

    return moved_id


# ── Cascade operations ───────────────────────────────────────────────────────


def find_dependents(fid: str) -> list[str]:
    """Find feature IDs that list F{fid} as Predecessor or Blocker."""
    dependents = []
    for fp in sorted(AGENTS_DIR.glob("feature-*.md")):
        m = re.match(r"feature-(\d+)\.md", fp.name)
        if not m or m.group(1) == fid:
            continue
        deps = get_feature_dependencies(m.group(1))
        if any(d["id"] == fid and d["type"] in ("Predecessor", "Blocker") for d in deps):
            dependents.append(m.group(1))
    return dependents


def check_all_deps_satisfied(fid: str) -> bool:
    """Check if all Predecessor/Blocker deps of a feature are DONE."""
    deps = get_feature_dependencies(fid)
    for d in deps:
        if d["type"] in ("Predecessor", "Blocker"):
            if read_feature_status(d["id"]) != "DONE":
                return False
    return True


def cascade_unblock(
    completed_fid: str, index_lines: list[str], dry_run: bool = False
) -> list[dict]:
    """Update dependents and unblock where possible. Returns actions taken."""
    actions = []
    queue = [completed_fid]
    processed = set()

    while queue:
        fid = queue.pop(0)
        if fid in processed:
            continue
        processed.add(fid)

        for dep_fid in find_dependents(fid):
            if update_dep_status_in_file(dep_fid, fid, "DONE", dry_run):
                actions.append(
                    {"feature": dep_fid, "action": "dep_updated", "dep_id": fid}
                )

            current = read_feature_status(dep_fid)
            if current == "BLOCKED" and check_all_deps_satisfied(dep_fid):
                set_feature_status(dep_fid, "PROPOSED", dry_run)
                update_index_status(index_lines, dep_fid, "PROPOSED")
                actions.append(
                    {
                        "feature": dep_fid,
                        "action": "unblocked",
                        "from": "BLOCKED",
                        "to": "PROPOSED",
                    }
                )
                # Newly PROPOSED ≠ DONE, so no further cascade

    return actions


# ── Commands ─────────────────────────────────────────────────────────────────


def cmd_set(args) -> int:
    fid = args.id
    new_status = args.status.upper()
    dry_run = args.dry_run

    if new_status not in VALID_STATUSES:
        print(
            f"ERROR: Invalid status '{new_status}'. Valid: {', '.join(sorted(VALID_STATUSES))}",
            file=sys.stderr,
        )
        return 1

    current = read_feature_status(fid)
    if current is None:
        print(
            f"ERROR: feature-{fid}.md not found or has no Status line", file=sys.stderr
        )
        return 1

    already_set = current == new_status
    modified = []

    if already_set:
        print(f"F{fid}: already [{current}] (syncing index)")
    else:
        # 1. Update feature file
        if set_feature_status(fid, new_status, dry_run):
            modified.append(str(feature_path(fid).relative_to(REPO_ROOT)))
        print(f"F{fid}: [{current}] -> [{new_status}]")

    # 2. fl-reviewed marker
    if args.fl_reviewed and new_status == "REVIEWED":
        add_fl_reviewed_marker(fid, dry_run)

    # 3. Index operations
    index_lines = read_index()

    if new_status in COMPLETION_STATUSES:
        emoji = "\u2705" if new_status == "DONE" else "\u274c"
        if move_to_recently_completed(index_lines, fid, emoji):
            print("Index: Active -> Recently Completed")

        if unbold_depends_on(index_lines, fid):
            print(f"Index: unbolded F{fid} in Depends On")

        cascade_actions = cascade_unblock(fid, index_lines, dry_run)
        if cascade_actions:
            print("Dependencies updated:")
            for a in cascade_actions:
                if a["action"] == "dep_updated":
                    print(f"  F{a['feature']}: dep F{a['dep_id']} -> [DONE]")
                    p = str(feature_path(a["feature"]).relative_to(REPO_ROOT))
                    if p not in modified:
                        modified.append(p)
                elif a["action"] == "unblocked":
                    print(f"  F{a['feature']}: [{a['from']}] -> [{a['to']}]")

        rotated = rotate_history(index_lines, dry_run)
        if rotated:
            print(f"History: F{rotated} moved to history")
            hp = str(HISTORY_PATH.relative_to(REPO_ROOT))
            if hp not in modified:
                modified.append(hp)
    else:
        update_index_status(index_lines, fid, new_status)

    write_index(index_lines, dry_run)
    ip = str(INDEX_PATH.relative_to(REPO_ROOT))
    if ip not in modified:
        modified.append(ip)

    if dry_run:
        print("[DRY RUN] No files modified")

    print("Modified:")
    for f in modified:
        print(f"  {f}")

    return 0


def cmd_query(args) -> int:
    for fid in args.ids:
        status = read_feature_status(fid)
        if status is None:
            print(f"F{fid}: NOT_FOUND", file=sys.stderr)
        else:
            print(f"F{fid}: [{status}]")
    return 0


def cmd_sync(args) -> int:
    """Repair stale dependency statuses across all feature files."""
    dry_run = args.dry_run
    modified = []
    fixes = 0

    for fp in sorted(AGENTS_DIR.glob("feature-*.md")):
        m = re.match(r"feature-(\d+)\.md", fp.name)
        if not m:
            continue
        fid = m.group(1)
        deps = get_feature_dependencies(fid)
        for d in deps:
            actual = read_feature_status(d["id"])
            if actual and actual != d["status"]:
                if update_dep_status_in_file(fid, d["id"], actual, dry_run):
                    print(f"  F{fid}: dep F{d['id']} [{d['status']}] -> [{actual}]")
                    p = str(fp.relative_to(REPO_ROOT))
                    if p not in modified:
                        modified.append(p)
                    fixes += 1

    # Also sync index bold formatting for DONE features
    index_lines = read_index()
    index_changed = False
    for fp in sorted(AGENTS_DIR.glob("feature-*.md")):
        m_fp = re.match(r"feature-(\d+)\.md", fp.name)
        if not m_fp:
            continue
        fid = m_fp.group(1)
        if read_feature_status(fid) == "DONE":
            if unbold_depends_on(index_lines, fid):
                index_changed = True

    if index_changed:
        write_index(index_lines, dry_run)
        modified.append(str(INDEX_PATH.relative_to(REPO_ROOT)))

    if fixes == 0 and not index_changed:
        print("All dependency statuses are current")
    else:
        if dry_run:
            print(f"[DRY RUN] {fixes} fixes found, no files modified")
        else:
            print(f"Fixed {fixes} stale entries")
        if modified:
            print("Modified:")
            for f in modified:
                print(f"  {f}")
    return 0


def cmd_deps(args) -> int:
    fid = args.id
    deps = get_feature_dependencies(fid)
    if not deps:
        print(f"F{fid}: no dependencies")
        return 0

    sync_mode = getattr(args, "sync", False)
    dry_run = getattr(args, "dry_run", False)
    drift_candidates = []
    synced = []

    if sync_mode:
        index_lines = read_index()

    unsatisfied = 0
    for d in deps:
        actual = read_feature_status(d["id"])
        stale = ""
        if actual and actual != d["status"]:
            if sync_mode:
                old_status = d["status"]
                update_dep_status_in_file(fid, d["id"], actual, dry_run)
                update_index_status(index_lines, d["id"], actual)
                synced.append(f"F{d['id']} [{old_status}] -> [{actual}]")
                if actual == "DONE" and old_status != "DONE":
                    drift_candidates.append((d["id"], d["type"]))
            else:
                stale = f" (stale: file=[{d['status']}] actual=[{actual}])"
        if d["type"] in ("Predecessor", "Blocker") and actual != "DONE":
            unsatisfied += 1
        marker = "+" if actual == "DONE" else "-"
        print(f"  {marker} {d['type']:12s} F{d['id']}: [{actual or '?'}]{stale}")

    total = sum(1 for d in deps if d["type"] in ("Predecessor", "Blocker"))

    if sync_mode:
        if synced:
            for s in synced:
                print(f"Synced: {s}")
            write_index(index_lines, dry_run)
        for did, dtype in drift_candidates:
            print(f"DRIFT: F{did} ({dtype})")
        if dry_run:
            print("[DRY RUN] No files modified")

    print(f"Blocking: {total - unsatisfied}/{total} satisfied")
    return 0


# ── CLI ──────────────────────────────────────────────────────────────────────


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Feature status management tool",
        epilog=(
            "examples:\n"
            "  %(prog)s set 794 DONE            # Status transition + index sync + cascade\n"
            "  %(prog)s set 780 REVIEWED --fl-reviewed  # Add fl-reviewed marker\n"
            "  %(prog)s set 797 PROPOSED --dry-run      # Preview without writing\n"
            "  %(prog)s query 794 780 782        # Quick status lookup\n"
            "  %(prog)s deps 782                 # Show dependencies with stale detection\n"
            "  %(prog)s deps 782 --sync            # Sync stale deps + output drift candidates\n"
            "  %(prog)s deps 782 --sync --dry-run   # Preview sync without writing\n"
            "  %(prog)s sync                     # Repair all stale dependency statuses\n"
            "  %(prog)s sync --dry-run           # Preview stale fixes\n"
            "  %(prog)s ac-check 800              # Check AC consistency\n"
            "  %(prog)s ac-renumber 800 --dry-run # Preview renumbering\n"
            "  %(prog)s ac-insert 800 26          # Insert slot after AC#26\n"
            "  %(prog)s ac-delete 800 10          # Delete AC#10 and renumber\n"
            "  %(prog)s ac-fix 800 6 --expected 8 # Fix AC#6 expected value"
        ),
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    sub = parser.add_subparsers(dest="command", required=True)

    p_set = sub.add_parser("set", help="Set feature status")
    p_set.add_argument("id", help="Feature ID (numeric)")
    p_set.add_argument("status", help="New status")
    p_set.add_argument(
        "--fl-reviewed", action="store_true", help="Add fl-reviewed marker"
    )
    p_set.add_argument(
        "--dry-run", action="store_true", help="Preview without writing"
    )

    p_query = sub.add_parser("query", help="Query feature status")
    p_query.add_argument("ids", nargs="+", help="Feature ID(s)")

    p_deps = sub.add_parser("deps", help="Show dependencies with live status")
    p_deps.add_argument("id", help="Feature ID")
    p_deps.add_argument("--sync", action="store_true", help="Fix stale deps + output drift candidates")
    p_deps.add_argument("--dry-run", action="store_true", help="Preview sync without writing")

    p_sync = sub.add_parser("sync", help="Repair stale dependency statuses")
    p_sync.add_argument(
        "--dry-run", action="store_true", help="Preview without writing"
    )

    # --- AC subcommands ---
    p_ac_check = sub.add_parser("ac-check", help="Check AC consistency")
    p_ac_check.add_argument("id", help="Feature ID (numeric)")

    p_ac_renumber = sub.add_parser("ac-renumber", help="Renumber ACs to close gaps")
    p_ac_renumber.add_argument("id", help="Feature ID (numeric)")
    p_ac_renumber.add_argument("--dry-run", action="store_true", help="Preview without writing")
    p_ac_renumber.add_argument("--force", action="store_true", help="Override sub-numbered AC safety check")

    p_ac_insert = sub.add_parser("ac-insert", help="Insert AC slot after given position")
    p_ac_insert.add_argument("id", help="Feature ID (numeric)")
    p_ac_insert.add_argument("after", type=int, help="Insert after this AC number")
    p_ac_insert.add_argument("--dry-run", action="store_true", help="Preview without writing")
    p_ac_insert.add_argument("--force", action="store_true", help="Override sub-numbered AC safety check")

    p_ac_delete = sub.add_parser("ac-delete", help="Delete an AC and renumber")
    p_ac_delete.add_argument("id", help="Feature ID (numeric)")
    p_ac_delete.add_argument("ac_num", type=int, help="AC number to delete")
    p_ac_delete.add_argument("--dry-run", action="store_true", help="Preview without writing")
    p_ac_delete.add_argument("--force", action="store_true", help="Override sub-numbered AC safety check")

    p_ac_fix = sub.add_parser("ac-fix", help="Fix AC metadata")
    p_ac_fix.add_argument("id", help="Feature ID (numeric)")
    p_ac_fix.add_argument("ac_num", type=int, help="AC number to fix")
    p_ac_fix.add_argument("--expected", default=None, help="New Expected value")
    p_ac_fix.add_argument("--description", default=None, help="New Description value")
    p_ac_fix.add_argument("--dry-run", action="store_true", help="Preview without writing")

    return parser


def main() -> int:
    args = build_parser().parse_args()
    if args.command == "set":
        return cmd_set(args)
    elif args.command == "query":
        return cmd_query(args)
    elif args.command == "deps":
        return cmd_deps(args)
    elif args.command == "sync":
        return cmd_sync(args)
    elif args.command == "ac-check":
        issues = ac_ops.ac_check(args.id)
        return 1 if issues else 0
    elif args.command == "ac-renumber":
        return ac_ops.ac_renumber(args.id, dry_run=args.dry_run, force=args.force)
    elif args.command == "ac-insert":
        return ac_ops.ac_insert(args.id, after=args.after, dry_run=args.dry_run, force=args.force)
    elif args.command == "ac-delete":
        return ac_ops.ac_delete(args.id, ac_num=args.ac_num, dry_run=args.dry_run, force=args.force)
    elif args.command == "ac-fix":
        return ac_ops.ac_fix(
            args.id,
            ac_num=args.ac_num,
            expected=args.expected,
            description=args.description,
            dry_run=args.dry_run,
        )
    return 1


if __name__ == "__main__":
    sys.exit(main())
