# Feature 091: MockRand Engine Integration

## Status: [DONE]

## Type: engine

## Background

### Problem

`--mock-rand` オプションは KojoTestConfig.MockRandQueue に値を設定するが、実際の RAND 関数（`VariableEvaluator.GetNextRand`）は MTRandom クラスを直接使用しており、MockRandQueue を参照していない。

つまり **Mock Rand 機能は設定だけされて、実際のRANDとは接続されていない**。

### Goal

RAND 関数呼び出し時に MockRandQueue から順番に値を取得できるようにし、口上テストの分岐を完全に制御可能にする。

### Context

Feature 060 で `--mock-rand` CLI オプションが追加されたが、実際の乱数生成への接続が未実装だった。

現状の実装:
```
CLI: --mock-rand 0,1,2
  ↓
HeadlessRunner: options.MockRand.Add(value)
  ↓
KojoTestConfig: MockRandQueue.AddRange(values)
  ↓
(接続なし)
  ↓
VariableEvaluator.GetNextRand: return rand.NextInt64(max) ← MTRandomを使用
```

期待する実装:
```
CLI: --mock-rand 0,1,2
  ↓
HeadlessRunner: options.MockRand.Add(value)
  ↓
KojoTestConfig: MockRandQueue.AddRange(values)
  ↓
GlobalStatic or InjectedQueue
  ↓
VariableEvaluator.GetNextRand: MockRandQueue から取得、なければMTRandom
```

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | ビルド成功 | build | succeeds | - | [x] VERIFIED |
| 2 | MockRand値がRANDに反映される | output | contains | "RAND結果: 42" | [x] VERIFIED |
| 3 | MockRand枯渇時にRAND2が出力される | output | contains | "RAND2:" | [x] VERIFIED |

### AC Details

#### AC1: ビルド成功

**Test Command**:
```bash
dotnet build uEmuera/uEmuera.Headless.csproj
```

**Expected Output**: Exit code 0

---

#### AC2: MockRand値がRANDに反映される

**Test Command**:
```bash
dotnet run --project uEmuera/uEmuera.Headless.csproj -- Game/ \
  --mock-rand 42 \
  --unit TEST_MOCK_RAND \
  --char 1
```

**Test ERB** (Game/ERB/TEST_MOCK_RAND.ERB):
```erb
@TEST_MOCK_RAND
#FUNCTION
PRINTL RAND結果: %RAND(100)%
RETURNF 1
```

**Expected Output**: "RAND結果: 42"

---

#### AC3: MockRand枯渇時にRAND2が出力される

**Test Command**:
```bash
dotnet run --project uEmuera/uEmuera.Headless.csproj -- Game/ \
  --mock-rand 42 \
  --unit TEST_MOCK_RAND_EXHAUST \
  --char 1
```

**Test ERB** (Game/ERB/TEST_MOCK_RAND_EXHAUST.ERB):
```erb
@TEST_MOCK_RAND_EXHAUST
LOCAL = RAND(100)
PRINTFORML RAND1: {LOCAL}
LOCAL = RAND(100)
PRINTFORML RAND2: {LOCAL}
RETURN
```

**Expected Output**: "RAND2:" appears (confirms second RAND call succeeds after mock queue exhaustion)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | VariableEvaluator.GetNextRand に MockRandQueue 機構を実装 | [x] TESTED |
| 2 | 2 | TEST_MOCK_RAND.ERB テスト関数作成 | [x] TESTED |
| 3 | 3 | TEST_MOCK_RAND_EXHAUST.ERB テスト関数作成 | [x] TESTED |

---

## Implementation Approach

### Option A: GlobalStatic経由 (Simple)

```csharp
// VariableEvaluator.cs
public Int64 GetNextRand(Int64 max)
{
    if (GlobalStatic.KojoTestConfig?.MockRandQueue?.Count > 0)
    {
        var queue = GlobalStatic.KojoTestConfig.MockRandQueue;
        var value = queue[0];
        queue.RemoveAt(0);
        return value % max; // Ensure within range
    }
    return rand.NextInt64(max);
}
```

### Option B: IVariableEvaluator拡張 (Clean)

```csharp
// IVariableEvaluator.cs - add method
void SetMockRandQueue(List<int> queue);

// VariableEvaluator.cs
private List<int> mockRandQueue;

public void SetMockRandQueue(List<int> queue)
{
    mockRandQueue = queue;
}

public Int64 GetNextRand(Int64 max)
{
    if (mockRandQueue?.Count > 0)
    {
        var value = mockRandQueue[0];
        mockRandQueue.RemoveAt(0);
        return value % max;
    }
    return rand.NextInt64(max);
}
```

**Recommendation**: Option A is simpler and consistent with existing GlobalStatic pattern.

---

## Execution State

**Current Phase**: Complete
**Assigned To**: finalizer (haiku)
**Completion**: 2025-12-17 23:45:00 UTC

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-17 | initializer | Initialize feature-091 for [WIP] | READY |
| 2025-12-17 | implementer | Task 1: Implement MockRandQueue in GetNextRand | SUCCESS |
| 2025-12-17 | unit-tester | Task 1: Verify build succeeds and code compiles (AC1) | PASS |
| 2025-12-17 | implementer | Task 2: Create TEST_MOCK_RAND.ERB test function | SUCCESS |
| 2025-12-17 | unit-tester | Task 2: Test --unit with TEST_MOCK_RAND (AC2) | FAIL (incompatible #FUNCTION) |
| 2025-12-17 | debugger | Task 2 (Attempt 1): Fix TEST_MOCK_RAND.ERB #FUNCTION issue | FIXED |
| 2025-12-17 | unit-tester | Task 2 (RETRY): Verify MockRand value 42 in RAND output | PASS |
| 2025-12-17 | implementer | Task 3: Create TEST_MOCK_RAND_EXHAUST.ERB test function | SUCCESS |
| 2025-12-17 | unit-tester | Task 3: Verify MockRand exhaustion fallback (AC3) | PASS |
| 2025-12-17 | ac-tester | AC3 Verification: output contains "RAND2:" | PASS |
| 2025-12-17 | debugger | AC2 (Attempt 1): Fix UTF-8 BOM encoding in TEST_MOCK_RAND.ERB | FIXED |
| 2025-12-17 | ac-tester | AC2 Verification (RETRY): output contains "RAND結果: 42" with UTF-8 fix | PASS |

---

## Discovered Issues

| Issue | Type | Priority |
|-------|------|----------|
| Feature 060 未完成 | Bug | High |
| `#FUNCTION` directive incompatible with `--unit` | Limitation | Medium |
| ERB files created by implementer lack UTF-8 BOM | Process | Low |

---

## Links

- [feature-060.md](feature-060.md) - MockRand CLI option (original)
- [KojoTestConfig.cs](../../uEmuera/Assets/Scripts/Emuera/Headless/KojoTestConfig.cs)
- [VariableEvaluator.cs](../../uEmuera/Assets/Scripts/Emuera/GameData/Variable/VariableEvaluator.cs)
- [GlobalStatic.cs](../../uEmuera/Assets/Scripts/Emuera/GlobalStatic.cs) - Added KojoTestConfig property
- [KojoTestRunner.cs](../../uEmuera/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs) - Sets GlobalStatic.KojoTestConfig
