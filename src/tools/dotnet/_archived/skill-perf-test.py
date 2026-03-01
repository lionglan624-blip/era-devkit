#!/usr/bin/env python3
"""
Skill Performance Testing Script

Measures file I/O performance for SKILL.md files from F519 converted skills.
Tests loading times and file sizes for the 8 context:fork skills.

Usage:
    python tools/skill-perf-test.py

Output:
    Performance metrics for each skill (file size, loading time)
    Summary statistics (mean, min, max)
"""

import time
from pathlib import Path
from typing import Dict, List
import statistics


# F519 converted skills with context:fork
SKILLS = [
    "initializer",
    "finalizer",
    "reference-checker",
    "eratw-reader",
    "dependency-analyzer",
    "goal-setter",
    "philosophy-deriver",
    "task-comparator",
]


def measure_skill_load_time(skill_name: str, skill_path: Path, iterations: int = 10) -> Dict:
    """Measure file loading time for a SKILL.md file.

    Args:
        skill_name: Name of the skill
        skill_path: Path to SKILL.md file
        iterations: Number of iterations for timing measurement

    Returns:
        Dictionary with skill metrics (name, size, load_time)
    """
    if not skill_path.exists():
        return {
            "skill": skill_name,
            "exists": False,
            "size_bytes": 0,
            "load_time_ms": 0.0,
        }

    # Get file size
    file_size = skill_path.stat().st_size

    # Measure loading time (average of multiple iterations)
    load_times = []
    for _ in range(iterations):
        start = time.perf_counter()
        with open(skill_path, 'r', encoding='utf-8') as f:
            content = f.read()
        end = time.perf_counter()
        load_times.append((end - start) * 1000)  # Convert to milliseconds

    avg_load_time = statistics.mean(load_times)

    return {
        "skill": skill_name,
        "exists": True,
        "size_bytes": file_size,
        "load_time_ms": avg_load_time,
    }


def format_bytes(size_bytes: int) -> str:
    """Format byte size for human readability."""
    if size_bytes < 1024:
        return f"{size_bytes}B"
    elif size_bytes < 1024 * 1024:
        return f"{size_bytes / 1024:.1f}KB"
    else:
        return f"{size_bytes / (1024 * 1024):.1f}MB"


def main():
    # Base path for skills
    base_path = Path(__file__).parent.parent / ".claude" / "skills"

    print("=== Skill Performance Testing ===")
    print(f"Base path: {base_path}")
    print(f"Testing {len(SKILLS)} skills from F519\n")

    results: List[Dict] = []

    # Measure each skill
    for skill_name in SKILLS:
        skill_path = base_path / skill_name / "SKILL.md"
        print(f"Testing {skill_name}...", end=" ")

        result = measure_skill_load_time(skill_name, skill_path, iterations=10)
        results.append(result)

        if result["exists"]:
            print(f"OK (size: {format_bytes(result['size_bytes'])}, "
                  f"load: {result['load_time_ms']:.3f}ms)")
        else:
            print("NOT FOUND")

    # Calculate summary statistics
    print("\n=== Summary Statistics ===")

    valid_results = [r for r in results if r["exists"]]
    if valid_results:
        sizes = [r["size_bytes"] for r in valid_results]
        times = [r["load_time_ms"] for r in valid_results]

        print(f"Total skills tested: {len(valid_results)}/{len(SKILLS)}")
        print(f"\nFile sizes:")
        print(f"  Min:  {format_bytes(min(sizes))}")
        print(f"  Max:  {format_bytes(max(sizes))}")
        print(f"  Mean: {format_bytes(int(statistics.mean(sizes)))}")

        print(f"\nLoad times:")
        print(f"  Min:  {min(times):.3f}ms")
        print(f"  Max:  {max(times):.3f}ms")
        print(f"  Mean: {statistics.mean(times):.3f}ms")

        # Overhead threshold analysis
        max_time = max(times)
        print(f"\n=== Overhead Analysis ===")
        print(f"Maximum load time: {max_time:.3f}ms")
        if max_time < 10.0:
            print("Status: Acceptable (< 10ms threshold)")
        elif max_time < 50.0:
            print("Status: Moderate (10-50ms)")
        else:
            print("Status: High (> 50ms)")
    else:
        print("No valid results to analyze")

    print("\n=== Results ===")
    for result in results:
        if result["exists"]:
            print(f"{result['skill']:25s} | "
                  f"Size: {format_bytes(result['size_bytes']):8s} | "
                  f"Load: {result['load_time_ms']:7.3f}ms")
        else:
            print(f"{result['skill']:25s} | NOT FOUND")


if __name__ == "__main__":
    main()
