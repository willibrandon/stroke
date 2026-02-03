# Quickstart: Event Loop Utilities

**Feature**: 050-event-loop-utils
**Build order**: EventLoopUtils.cs → EventLoopUtilsTests.cs

## Build Sequence

### Step 1: Create EventLoopUtils.cs

Create the static class with all three utility functions:

```
src/Stroke/EventLoop/EventLoopUtils.cs
```

Functions to implement (in order):
1. `GetTracebackFromContext` — simplest, no dependencies
2. `RunInExecutorWithContextAsync<T>` — core context preservation
3. `RunInExecutorWithContextAsync` (void overload) — delegates to generic version
4. `CallSoonThreadSafe` — most complex (deadline scheduling logic)

### Step 2: Create EventLoopUtilsTests.cs

```
tests/Stroke.Tests/EventLoop/EventLoopUtilsTests.cs
```

Test categories:
1. **GetTracebackFromContext tests** — exception with trace, no exception, non-exception value
2. **RunInExecutorWithContext tests** — context preservation, result return, cancellation, exception propagation
3. **CallSoonThreadSafe tests** — no sync context, no deadline, with deadline (idle + busy), zero/negative deadline

### Step 3: Verify

- `dotnet build src/Stroke/` compiles cleanly
- `dotnet test tests/Stroke.Tests/ --filter "FullyQualifiedName~EventLoop"` passes
- All acceptance scenarios from spec are covered

## Verification Commands

```bash
# Build
dotnet build src/Stroke/

# Run feature tests only
dotnet test tests/Stroke.Tests/ --filter "FullyQualifiedName~EventLoop"

# Run all tests (regression check)
dotnet test
```

## Dependencies

- No new NuGet packages required
- No changes to existing files required
- `Stroke.EventLoop` namespace is new — just create the directory
