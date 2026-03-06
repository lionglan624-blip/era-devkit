# Feature 835: IEngineVariables Abstract Method Stubs — Real VariableData Delegation

## Status: [DRAFT]

## Type: engine

## Background

### Philosophy (Mid-term Vision)
Era.Core interfaces are the SSOT for cross-repo contracts between the core library and the engine runtime. Every interface method must have a concrete runtime implementation that delegates to actual engine data.

### Problem (Current Issue)
After F833 (DIM stubs implementation), ~20 abstract IEngineVariables methods in EngineVariablesImpl still return 0/no-op instead of delegating to actual VariableData arrays. These include GetResult, GetMoney, SetMoney, GetMaster, GetAssi, GetCount, GetCharaNum, GetRandom, GetName, GetCallName, GetIsAssi, GetCharacterNo, SetName, SetCallName, SetMaster, GetTarget(), SetTarget(int), GetPlayer, SetPlayer, SetCharacterNo, GetTarget(int), SetTarget(int,int), GetSelectCom, etc.

### Goal (What to Achieve)
Implement real VariableData delegation for all remaining abstract IEngineVariables methods in EngineVariablesImpl, eliminating the final set of no-op stubs.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F833 | [WIP] | Creates EngineVariablesImpl with stub bodies for abstract methods |

---

## Links
- [Predecessor: F833](feature-833.md) - IEngineVariables DIM Stubs Engine Adapter Implementation
