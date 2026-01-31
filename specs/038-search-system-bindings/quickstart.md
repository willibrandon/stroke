# Quickstart: Search System & Search Bindings

**Feature**: 038-search-system-bindings
**Date**: 2026-01-31

## Build & Verify

### 1. Build the solution

```bash
cd /Users/brandon/src/stroke
dotnet build src/Stroke/Stroke.csproj
```

### 2. Run all tests

```bash
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj
```

### 3. Run only search-related tests

```bash
# SearchOperations tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~SearchOperations"

# SearchBindings tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~SearchBindings"

# SearchState tests (including new ~ operator test)
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~SearchState"
```

## Implementation Order

1. **SearchState `~` operator** — Add operator to existing class (no dependencies)
2. **SearchOperations relocation** — Move from Core to Application, delete old stubs
3. **SearchOperations implementation** — Implement all 5 methods
4. **SearchBindings creation** — Create 7 handler functions
5. **Tests** — Replace old stub tests, create new integration tests

## Files Changed

| File | Action | Description |
|------|--------|-------------|
| `src/Stroke/Core/SearchState.cs` | MODIFY | Add `operator ~` |
| `src/Stroke/Core/SearchOperations.cs` | DELETE | Remove stubs |
| `src/Stroke/Application/SearchOperations.cs` | CREATE | Full implementation |
| `src/Stroke/Application/Bindings/SearchBindings.cs` | CREATE | 7 binding handlers |
| `tests/Stroke.Tests/Core/SearchOperationsTests.cs` | DELETE | Remove stub tests |
| `tests/Stroke.Tests/Application/SearchOperationsTests.cs` | CREATE | Integration tests |
| `tests/Stroke.Tests/Application/Bindings/SearchBindingsTests.cs` | CREATE | Binding handler tests |
| `tests/Stroke.Tests/Core/SearchStateTests.cs` | MODIFY | Add `~` operator test |

## Verification Checklist

- [ ] `dotnet build` succeeds with no errors
- [ ] All existing tests pass (no regressions)
- [ ] SearchOperations no longer throws NotImplementedException
- [ ] StartSearch focuses SearchBufferControl and sets Vi mode to Insert
- [ ] StopSearch restores focus and sets Vi mode to Navigation
- [ ] DoIncrementalSearch moves cursor to match positions
- [ ] AcceptSearch keeps cursor at result and appends to history
- [ ] All 7 SearchBindings functions delegate correctly
- [ ] Filter conditions gate binding execution
- [ ] SearchState `~` operator returns inverted instance
- [ ] No source file exceeds 1000 LOC
