# ERB Testing (flow + ErbLinter)

ERBロジックテスト専用リファレンス。共通情報は [SKILL.md](SKILL.md) 参照。

---

## flow Mode

### Basic Execution

```bash
cd Game
dotnet run --project ../engine/uEmuera.Headless.csproj -- . \
  --flow tests/regression/train/scenario-train-basic.json
```

### Branch Tracing (F084)

| Option | Description |
|--------|-------------|
| `--trace` | Basic branch tracing |
| `--branch-map <file>` | Load semantic labels from kojo-mapper |

**Generate**: `cd src/tools/kojo-mapper && python kojo_mapper.py "../../Game/ERB/口上/1_美鈴" --branch-map`

### Parallel Execution

**CRITICAL**: Pre-build required.

```bash
dotnet build engine/uEmuera.Headless.csproj
cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --parallel
```

**Behavior**: `--parallel N` sets max worker count (≤20). All tasks always processed.

---

## ErbLinter

```bash
dotnet run --project tools/ErbLinter -- Game/ERB/口上/4_咲夜/*.ERB
```

**Checks**: Undefined function calls, missing RETURN, syntax errors.

---

## Test Scenario JSON Format

```json
{
  "name": "Test name",
  "description": "What this test verifies",
  "variables": {
    "TARGET:0": 4,
    "FLAG:26": 128
  },
  "characters": {
    "Sakuya": {
      "CFLAG:2": 5000,
      "TEQUIP:50": 1
    }
  }
}
```

**Input File**: Start with `0` (new game), `9` (Quick Start). End with empty line.

---

## Test Workflow

```
Run flow (baseline) → Refactor → Run flow (verify) → Regression tests
```

---

## Positive/Negative Testing

erb はポジネガ両方必須。詳細は [SKILL.md](SKILL.md#positivenegative-testing) 参照。

| テスト対象 | ポジ例 | ネガ例 |
|------------|--------|--------|
| 分岐条件 | 条件成立 → A処理 | 条件不成立 → B処理 |
| 閾値判定 | 値≥閾値 → 発動 | 値<閾値 → 非発動 |
| フラグ処理 | フラグON → 有効 | フラグOFF → 無効 |
