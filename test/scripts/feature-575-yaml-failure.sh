#!/bin/bash
# Feature 575: YAML Load Failure Test
# Tests AC#5, AC#6, AC#7: YAML load failure causes fatal error with appropriate messages
#
# Expected behavior (after F575 implementation):
# - AC#5: Engine exits with non-zero code when YAML load fails
# - AC#6: Error message contains "Failed to initialize VariableSize from YAML"
# - AC#7: Error message contains "Failed to initialize GameBase from YAML"
#
# Current behavior (RED phase):
# - Engine continues with CSV fallback (graceful degradation from F558)
# - Test will FAIL until F575 changes graceful degradation to fatal error

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
GAME_DIR="$(dirname "$(dirname "$SCRIPT_DIR")")"
REPO_ROOT="$(dirname "$GAME_DIR")"
ENGINE_PROJECT="$REPO_ROOT/engine/uEmuera.Headless.csproj"
CONFIG_DIR="$GAME_DIR/config"
BACKUP_DIR="$GAME_DIR/.tmp/feature-575-backup"

echo "=== Feature 575: YAML Load Failure Test ==="
echo "Game: $GAME_DIR"
echo "Engine: $ENGINE_PROJECT"
echo ""

# Ensure backup directory exists
mkdir -p "$BACKUP_DIR"

# Test AC#5, AC#6: VariableSize YAML load failure
test_variable_size_failure() {
    echo "--- Test AC#5, AC#6: VariableSize YAML Load Failure ---"

    # Backup variable_sizes.yaml
    if [ -f "$CONFIG_DIR/variable_sizes.yaml" ]; then
        cp "$CONFIG_DIR/variable_sizes.yaml" "$BACKUP_DIR/variable_sizes.yaml.bak"
        echo "Backed up variable_sizes.yaml"
    else
        echo "WARNING: variable_sizes.yaml not found at $CONFIG_DIR/variable_sizes.yaml"
    fi

    # Remove variable_sizes.yaml to simulate load failure
    rm -f "$CONFIG_DIR/variable_sizes.yaml"
    echo "Removed variable_sizes.yaml"

    # Run engine and capture output
    local exit_code=0
    local output
    output=$(dotnet run --project "$ENGINE_PROJECT" -- "$GAME_DIR" 2>&1) || exit_code=$?

    # Restore variable_sizes.yaml
    if [ -f "$BACKUP_DIR/variable_sizes.yaml.bak" ]; then
        cp "$BACKUP_DIR/variable_sizes.yaml.bak" "$CONFIG_DIR/variable_sizes.yaml"
        echo "Restored variable_sizes.yaml"
    fi

    # AC#5: Verify non-zero exit code
    if [ $exit_code -eq 0 ]; then
        echo "FAIL AC#5: Engine exited with code 0 (expected non-zero)"
        echo "Output: $output"
        return 1
    fi
    echo "PASS AC#5: Engine exited with non-zero code ($exit_code)"

    # AC#6: Verify error message
    if echo "$output" | grep -q "Failed to initialize VariableSize from YAML"; then
        echo "PASS AC#6: Error message contains 'Failed to initialize VariableSize from YAML'"
    else
        echo "FAIL AC#6: Error message does not contain 'Failed to initialize VariableSize from YAML'"
        echo "Output: $output"
        return 1
    fi

    return 0
}

# Test AC#7: GameBase YAML load failure
test_game_base_failure() {
    echo ""
    echo "--- Test AC#7: GameBase YAML Load Failure ---"

    # Backup game_base.yaml
    if [ -f "$CONFIG_DIR/game_base.yaml" ]; then
        cp "$CONFIG_DIR/game_base.yaml" "$BACKUP_DIR/game_base.yaml.bak"
        echo "Backed up game_base.yaml"
    else
        echo "WARNING: game_base.yaml not found at $CONFIG_DIR/game_base.yaml"
    fi

    # Remove game_base.yaml to simulate load failure
    rm -f "$CONFIG_DIR/game_base.yaml"
    echo "Removed game_base.yaml"

    # Run engine and capture output
    local exit_code=0
    local output
    output=$(dotnet run --project "$ENGINE_PROJECT" -- "$GAME_DIR" 2>&1) || exit_code=$?

    # Restore game_base.yaml
    if [ -f "$BACKUP_DIR/game_base.yaml.bak" ]; then
        cp "$BACKUP_DIR/game_base.yaml.bak" "$CONFIG_DIR/game_base.yaml"
        echo "Restored game_base.yaml"
    fi

    # AC#7: Verify error message
    # Note: Exit code verification is implicit from AC#5 test structure
    # In RED phase, exit_code will be 0 (graceful degradation)
    # In GREEN phase, exit_code will be non-zero (fatal error)
    if echo "$output" | grep -q "Failed to initialize GameBase from YAML"; then
        echo "PASS AC#7: Error message contains 'Failed to initialize GameBase from YAML'"
        # However, in RED phase the engine continues (exit_code 0), so overall this is still FAIL
        if [ $exit_code -eq 0 ]; then
            echo "NOTE: Engine continues with graceful degradation (RED phase behavior)"
            return 1  # FAIL because engine should exit with error in GREEN phase
        fi
    else
        echo "FAIL AC#7: Error message does not contain 'Failed to initialize GameBase from YAML'"
        echo "Output: $output"
        return 1
    fi

    return 0
}

# Run tests
PASSED=0
FAILED=0

if test_variable_size_failure; then
    PASSED=$((PASSED + 2))  # AC#5 and AC#6
else
    FAILED=$((FAILED + 2))
fi

if test_game_base_failure; then
    PASSED=$((PASSED + 1))  # AC#7
else
    FAILED=$((FAILED + 1))
fi

echo ""
echo "=== Results ==="
echo "Passed: $PASSED/3"
echo "Failed: $FAILED/3"

if [ $FAILED -gt 0 ]; then
    echo "Status: FAIL (RED phase - expected until F575 implementation)"
    exit 1
fi

echo "Status: PASS (GREEN phase - F575 implementation complete)"
exit 0
