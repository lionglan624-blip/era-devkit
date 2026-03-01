#!/bin/bash
# Master Regression Test Runner
# Usage: ./run-all-tests.sh [logfile]
# Examples:
#   ./run-all-tests.sh                    # Console output only
#   ./run-all-tests.sh regression.log     # Save to log file

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
LOG_FILE="$1"

run_with_log() {
    if [ -n "$LOG_FILE" ]; then
        "$@" 2>&1 | tee -a "$LOG_FILE"
    else
        "$@" 2>&1
    fi
}

# Initialize log file
if [ -n "$LOG_FILE" ]; then
    echo "=== ERA Regression Test Log ===" > "$LOG_FILE"
    echo "Date: $(date '+%Y-%m-%d %H:%M:%S')" >> "$LOG_FILE"
    echo "" >> "$LOG_FILE"
fi

run_with_log echo "=========================================="
run_with_log echo "  ERA Regression Test Suite"
run_with_log echo "=========================================="
run_with_log echo ""

# Phase 1: Core Tests
run_with_log echo "Running Phase 1: Core Tests..."
if run_with_log bash "$SCRIPT_DIR/run-core-tests.sh"; then
    run_with_log echo "Phase 1: OK"
else
    run_with_log echo "Phase 1: FAILED"
fi
run_with_log echo ""

# Phase 2: Training Tests
run_with_log echo "Running Phase 2: Training Tests..."
if run_with_log bash "$SCRIPT_DIR/run-train-tests.sh"; then
    run_with_log echo "Phase 2: OK"
else
    run_with_log echo "Phase 2: FAILED"
fi
run_with_log echo ""

# Phase 3: NTR Tests (if exists)
if [ -f "$SCRIPT_DIR/run-ntr-tests.sh" ]; then
    run_with_log echo "Running Phase 3: NTR Tests..."
    if run_with_log bash "$SCRIPT_DIR/run-ntr-tests.sh"; then
        run_with_log echo "Phase 3: OK"
    else
        run_with_log echo "Phase 3: FAILED"
    fi
    run_with_log echo ""
fi

run_with_log echo "=========================================="
run_with_log echo "  All Tests Complete"
run_with_log echo "=========================================="

if [ -n "$LOG_FILE" ]; then
    echo ""
    echo "Log saved to: $LOG_FILE"
fi
