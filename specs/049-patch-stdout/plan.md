# Implementation Plan: Patch Stdout

**Branch**: `049-patch-stdout` | **Date**: 2026-02-02 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/049-patch-stdout/spec.md`

## Summary

Implement the `StdoutProxy` class (a `TextWriter` subclass) and `StdoutPatching` static class that allow `Console.Write`/`Console.WriteLine` calls within a Stroke application to be displayed above the current prompt without corrupting the UI. The implementation uses a producer-consumer pattern with a dedicated background flush thread, newline-gated buffering, and `RunInTerminal` integration for safe output coordination with the renderer.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: `Stroke.Application` (AppSession, AppContext, RunInTerminal), `Stroke.Output` (IOutput)
**Storage**: N/A (in-memory buffering only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Single project (library)
**Performance Goals**: Batch 10+ rapid writes within 50ms into ≤2 terminal repaints; flush thread terminates within 1s of Close()
**Constraints**: Thread-safe writes from 4+ concurrent threads without deadlock; zero prompt corruption
**Scale/Scope**: 2 public classes + 1 internal record, ~285 LOC implementation, ~710 LOC tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | 1:1 port of `patch_stdout.py`. All public APIs mapped per `api-mapping.md`: `StdoutProxy` class, `StdoutPatching.PatchStdout()` static method |
| II. Immutability | ✅ PASS | `StdoutProxy` configuration (SleepBetweenWrites, Raw) is immutable. Mutable state (_buffer, Closed) is synchronized. `FlushItem` is an immutable record hierarchy |
| III. Layered Architecture | ✅ PASS | Lives in `Stroke.Application` (layer 7), depends on `Stroke.Output` (layer 2) and `Stroke.Application` internals. No circular dependencies |
| IV. Cross-Platform | ✅ PASS | Uses IOutput abstraction. EnableAutowrap handles Windows VT processing quirk (FR-014) |
| V. Editing Mode Parity | ✅ N/A | Not related to editing modes |
| VI. Performance | ✅ PASS | Batched writes via configurable delay, newline-gated buffering reduces repaints, background thread avoids blocking callers |
| VII. Full Scope | ✅ PASS | All 23 functional requirements (FR-001 through FR-023) addressed in design. No deferral |
| VIII. Real-World Testing | ✅ PASS | Tests use real BlockingCollection, real Thread, real IOutput implementations. No mocks |
| IX. Planning Documents | ✅ PASS | Consulted `api-mapping.md` (lines 1931-1943): `StdoutProxy` → `StdoutProxy`, `patch_stdout(raw)` → `StdoutPatching.PatchStdout(raw)` returning `IDisposable`. No test mapping exists for this module (new feature) |
| X. File Size | ✅ PASS | Estimated ~300 LOC for StdoutProxy, ~50 LOC for StdoutPatching, ~30 LOC for FlushItem. All well under 1,000 LOC |
| XI. Thread Safety | ✅ PASS | `System.Threading.Lock` for buffer access, `BlockingCollection<T>` for queue, dedicated background thread. Concurrent stress tests planned |
| XII. Contracts in Markdown | ✅ PASS | All contracts in `contracts/*.md` files. No `.cs` contract files |

**Post-Phase 1 Re-check**: All gates continue to pass. Design artifacts (data-model.md, contracts/) maintain full constitutional compliance.

## Project Structure

### Documentation (this feature)

```text
specs/049-patch-stdout/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: technical research decisions
├── data-model.md        # Phase 1: entity model and state transitions
├── quickstart.md        # Phase 1: usage examples
├── contracts/           # Phase 1: API contracts
│   ├── stdout-proxy.md  # StdoutProxy TextWriter subclass
│   └── stdout-patching.md # StdoutPatching static class + FlushItem
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2: implementation tasks (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/Application/
├── StdoutProxy.cs           # StdoutProxy : TextWriter (FR-002 through FR-023)
├── StdoutPatching.cs        # StdoutPatching.PatchStdout() static method (FR-001, FR-017, FR-020)
└── FlushItem.cs             # Internal discriminated union for queue items

tests/Stroke.Tests/Application/
├── StdoutProxyTests.cs              # Core construction and property tests
├── StdoutProxyBufferingTests.cs     # Newline-gated buffering, Write(null), Write(char)
├── StdoutProxyBatchingTests.cs      # Rapid write batching, SleepBetweenWrites
├── StdoutProxyLifecycleTests.cs     # Close, Dispose, Fileno, IsAtty
├── StdoutProxyRawModeTests.cs       # Raw vs non-raw VT100 routing
├── StdoutProxyConcurrencyTests.cs   # Thread safety stress tests
└── StdoutPatchingTests.cs           # Console.Out/Error redirection, nesting, restore
```

**Structure Decision**: Files are placed in the existing `Stroke.Application` namespace directory (`src/Stroke/Application/`), consistent with the api-mapping (`prompt_toolkit.patch_stdout` → `Stroke.Application`). Tests go in the existing test project under `Application/` subfolder.

## Complexity Tracking

> No constitutional violations. Table intentionally empty.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |

## Key Design Decisions

### 1. RunInTerminal Integration (Research RT-005)

When an application is running, the flush thread writes output by calling `RunInTerminal.RunAsync(writeAction, inExecutor: false)`. This suspends the renderer, writes output, and resumes rendering — preventing race conditions between stdout output and the prompt renderer.

When no application is running, the flush thread writes directly to `IOutput.Write()` / `IOutput.WriteRaw()`.

**Adaptation from Python**: Python uses `loop.call_soon_threadsafe()` to marshal into the asyncio event loop, then calls `run_in_terminal()`. Stroke calls `RunInTerminal.RunAsync()` directly from the flush thread, which internally coordinates with the application's async context via `InTerminalContext`. This is simpler because `RunInTerminal.RunAsync()` handles the "is app running?" check internally.

### 2. BlockingCollection vs Channel (Research RT-003)

Chose `BlockingCollection<FlushItem>` over `Channel<FlushItem>` because the consumer (flush thread) is synchronous. `BlockingCollection.Take()` naturally blocks the thread, while `Channel` requires async reads. The flush thread is a dedicated `Thread` (not a thread pool task), so blocking is appropriate.

### 3. Newline-Gated Buffering (Research RT-006)

Buffer accumulates text in a `List<string>`. On `Write()`:
- If data contains `\n`: Find last newline via `LastIndexOf`, flush everything up to and including the last newline, keep remainder in buffer
- If no `\n`: Append to buffer

On `Flush()`: Join buffer contents, put on queue, clear buffer. This matches Python's `_write()` / `_flush()` exactly.

### 4. TextWriter.Write Returns Void (Research RT-007)

Python's `write()` returns `len(data)` (int). C#'s `TextWriter.Write()` returns void. This is a language-mandated deviation. The spec's FR-016 ("return the number of characters written") is satisfied by the TextWriter contract where void return implies all characters were accepted.
