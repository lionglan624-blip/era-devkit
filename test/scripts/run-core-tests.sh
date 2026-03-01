#!/bin/bash
# Core Regression Tests Runner
# Usage: ./run-core-tests.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
GAME_DIR="$(dirname "$SCRIPT_DIR")"
HEADLESS_DIR="$GAME_DIR/../uEmuera/bin/Release/net8.0"
HEADLESS="dotnet $HEADLESS_DIR/uEmuera.Headless.dll"

echo "=== Core Regression Tests ==="
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

cd "$SCRIPT_DIR/core"

# Run tests (use ASCII patterns due to encoding)
run_test "core-wakeup" "scenario-wakeup.json" "input-wakeup.txt" "\[400\]"
run_test "core-movement" "scenario-movement.json" "input-movement.txt" "\[400\]"
run_test "core-conversation" "scenario-conversation.json" "input-conversation.txt" ""
run_test "core-sameroom" "scenario-sameroom.json" "input-sameroom.txt" ""
run_test "core-dayend" "scenario-dayend.json" "input-dayend.txt" "\[100\]"

echo ""
echo "=== Results ==="
echo "Passed: $PASSED"
echo "Failed: $FAILED"

if [ $FAILED -gt 0 ]; then
    exit 1
fi
