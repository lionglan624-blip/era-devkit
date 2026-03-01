#!/bin/bash
# Phase 3: NTR Tests Runner
# Usage: ./run-ntr-tests.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
GAME_DIR="$(dirname "$SCRIPT_DIR")"
HEADLESS_DIR="$GAME_DIR/../uEmuera/bin/Release/net8.0"
HEADLESS="dotnet $HEADLESS_DIR/uEmuera.Headless.dll"

echo "=== Phase 3: NTR Tests ==="
echo "Game: $GAME_DIR"
echo "Mode: Quick Start (no save file)"
echo ""

PASSED=0
FAILED=0

run_test() {
    local name=$1
    local scenario=$2
    local input=$3
    local expect=$4

    echo -n "[$name] "

    local output
    output=$($HEADLESS --inject "$scenario" "$GAME_DIR" < "$input" 2>&1)

    if echo "$output" | grep -i "error\|exception" > /dev/null; then
        echo "FAIL (error detected)"
        FAILED=$((FAILED + 1))
        return 1
    fi

    if [ -n "$expect" ]; then
        if echo "$output" | grep -q "$expect"; then
            echo "PASS"
            PASSED=$((PASSED + 1))
        else
            echo "FAIL (expected: $expect)"
            FAILED=$((FAILED + 1))
            return 1
        fi
    else
        echo "PASS (no error)"
        PASSED=$((PASSED + 1))
    fi
}

cd "$SCRIPT_DIR/ntr"

# Run NTR tests (use ASCII patterns due to encoding)
run_test "ntr-visitor" "scenario-ntr-visitor.json" "input-ntr-visitor.txt" "\[100\]"
run_test "ntr-movement" "scenario-ntr-movement.json" "input-ntr-movement.txt" "\[100\]"

echo ""
echo "=== Results ==="
echo "Passed: $PASSED"
echo "Failed: $FAILED"

if [ $FAILED -gt 0 ]; then
    exit 1
fi
