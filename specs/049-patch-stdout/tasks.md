# Tasks: Patch Stdout

**Input**: Design documents from `/specs/049-patch-stdout/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Tests are included per Constitution VIII (80% coverage target, real implementations only, no mocks).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/Stroke/Application/`
- **Tests**: `tests/Stroke.Tests/Application/`

---

## Phase 1: Setup

**Purpose**: Create the FlushItem discriminated union — shared by all user stories.

- [x] T001 [P] Implement `FlushItem` sealed record hierarchy (Text/Done variants) in `src/Stroke/Application/FlushItem.cs` per contract `contracts/stdout-patching.md` (§FlushItem). Include XML doc comments. ~20 LOC.

---

## Phase 2: Foundational — StdoutProxy Core (Blocking Prerequisites)

**Purpose**: Implement the StdoutProxy class with all internal mechanics. This MUST be complete before any user story can be verified end-to-end.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### StdoutProxy Implementation

- [x] T002 Implement `StdoutProxy` class skeleton in `src/Stroke/Application/StdoutProxy.cs` — sealed class extending `TextWriter`, constructor capturing `AppSession` via `AppContext.GetAppSession()` and `IOutput` via `appSession.Output` (FR-013), immutable config properties `SleepBetweenWrites` (TimeSpan, default 200ms, must be non-negative per FR-005) and `Raw` (bool, default false), `Closed` property guarded by `Lock`, `_buffer` as `List<string>`, `_flushQueue` as `BlockingCollection<FlushItem>`, `Encoding` property delegating to `_output.Encoding`. Validate `SleepBetweenWrites` is non-negative in constructor (throw `ArgumentOutOfRangeException`). Wire `OriginalStdout` property to return `_output.Stdout`. ~60 LOC.
- [x] T003 Implement newline-gated buffering in `StdoutProxy.Write(string?)` in `src/Stroke/Application/StdoutProxy.cs` — under `Lock`: silently ignore null/empty (FR-018), silently ignore if `Closed` (FR-019), use `LastIndexOf('\n')` to split (FR-003): everything up to and including last `\n` is joined from `_buffer` and queued as `FlushItem.Text`, remainder stays in buffer. No-newline text appends to buffer. Port of Python's `_write()` method. ~30 LOC.
- [x] T004 Implement `StdoutProxy.Write(char)` override in `src/Stroke/Application/StdoutProxy.cs` — convert char to string via `char.ToString()`, delegate to `Write(string)` (FR-023). ~5 LOC.
- [x] T005 Implement `StdoutProxy.Flush()` override in `src/Stroke/Application/StdoutProxy.cs` — under `Lock`: silently ignore if `Closed` (FR-019), join `_buffer` contents, queue as `FlushItem.Text`, clear buffer (FR-015). Port of Python's `_flush()`. ~10 LOC.
- [x] T006 Implement `StdoutProxy.Close()` and `Dispose(bool)` in `src/Stroke/Application/StdoutProxy.cs` — `Close()` is idempotent (FR-011): if not closed, acquire lock, flush remaining buffer, set `Closed=true`, release lock, queue `FlushItem.Done` sentinel, `_flushThread.Join()`. `Dispose(bool disposing)` calls `Close()`. ~20 LOC.
- [x] T007 Implement background flush thread in `StdoutProxy` in `src/Stroke/Application/StdoutProxy.cs` — `_StartWriteThread()` creates `Thread(IsBackground=true, Name="patch-stdout-flush-thread")` and starts it (RT-002). `_WriteThread()` main loop: `Take()` from queue (blocks), drain remaining via `TryTake()`, skip empty strings (FR-018), detect `Done` sentinel, call `_WriteAndFlush()` with concatenated text. Sleep `SleepBetweenWrites` after each write cycle when app is running (FR-005). Wrap entire write cycle in try/catch to swallow exceptions and continue (FR-022). Port of Python's `_write_thread()`. ~50 LOC.
- [x] T008 Implement `_WriteAndFlush()` in `StdoutProxy` in `src/Stroke/Application/StdoutProxy.cs` — detect whether an app is running by checking the captured `_appSession`'s app (matching Python's `self.app_session.app` pattern, FR-007/FR-008, RT-005). If app running: call `RunInTerminal.RunAsync(writeAction, inExecutor: false)` and block synchronously via `.GetAwaiter().GetResult()` (flush thread is a dedicated Thread, not a thread pool thread, so synchronous blocking is appropriate). The writeAction calls `_output.EnableAutowrap()` (FR-014), then `_output.WriteRaw(text)` if `Raw` else `_output.Write(text)` (FR-006), then `_output.Flush()`. If no app running: call writeAction directly (FR-008). ~30 LOC.
- [x] T009 Implement `Fileno()` and `IsAtty()` methods in `StdoutProxy` in `src/Stroke/Application/StdoutProxy.cs` — `Fileno()` delegates to `_output.Fileno()` (FR-021). `IsAtty()` checks `_output.Stdout` (a `TextWriter?` property on `IOutput`): returns `false` if null, otherwise checks if the underlying stream is a console/TTY (matching Python's `stdout.isatty()` — note: .NET's `TextWriter` has no `IsAtty()`, so use stream-type check or `Console.IsOutputRedirected` equivalent). Both operate on captured `_output`, do not require proxy to be open (FR-021). ~10 LOC.

**Checkpoint**: StdoutProxy is fully implemented. All internal mechanics (buffering, threading, flush coordination) are ready.

---

## Phase 3: User Story 1 — Print Output Above Active Prompt (Priority: P1) 🎯 MVP

**Goal**: `Console.Write`/`Console.WriteLine` calls during an active prompt session appear above the prompt without corruption.

**Independent Test**: Activate PatchStdout, write to Console.Out, verify output routed through StdoutProxy via IOutput, prompt UI undisturbed.

### Tests for User Story 1

- [x] T010 [P] [US1] Write `StdoutPatchingTests` in `tests/Stroke.Tests/Application/StdoutPatchingTests.cs` — test `PatchStdout()` replaces `Console.Out` and `Console.Error` with a `StdoutProxy` instance, test disposal restores original streams (SC-005), test both stdout and stderr redirect to the same proxy (FR-017), test nesting: call PatchStdout while already patched, verify new proxy wraps current output, inner dispose restores outer proxy (FR-020). Use real `AppSession` and `IOutput` (no mocks per Constitution VIII). ~120 LOC.
- [x] T011 [P] [US1] Write core `StdoutProxyTests` in `tests/Stroke.Tests/Application/StdoutProxyTests.cs` — test construction captures AppSession and IOutput, test `SleepBetweenWrites` defaults to 200ms, test `Raw` defaults to false, test `Encoding` property returns output encoding, test `OriginalStdout` property, test constructor rejects negative `SleepBetweenWrites` (throws `ArgumentOutOfRangeException`), test constructor accepts zero `SleepBetweenWrites`. ~80 LOC.

### Implementation for User Story 1

- [x] T012 [US1] Implement `StdoutPatching` static class in `src/Stroke/Application/StdoutPatching.cs` — `PatchStdout(bool raw = false)` method: create `StdoutProxy`, save `Console.Out` and `Console.Error`, call `Console.SetOut(proxy)` and `Console.SetError(proxy)` (FR-017), return `IDisposable` that restores originals first then disposes proxy (FR-001, edge case: disposal ordering). Support nesting per FR-020. Port of Python's `patch_stdout()` context manager. ~50 LOC.

**Checkpoint**: User Story 1 complete — PatchStdout works end-to-end.

---

## Phase 4: User Story 2 — Batched Write Output (Priority: P1)

**Goal**: Rapid small writes are buffered until newline, then batched with configurable delay to reduce terminal repaint frequency.

**Independent Test**: Write multiple partial strings without newlines, then write a newline, verify only one flush occurs containing the full line.

### Tests for User Story 2

- [x] T013 [P] [US2] Write buffering tests in `tests/Stroke.Tests/Application/StdoutProxyBufferingTests.cs` — test partial write (no newline) does not flush, test newline triggers flush of accumulated buffer (US2 scenario 2), test embedded newlines split correctly (`"line1\nline2\nline3"` → `"line1\nline2\n"` flushed, `"line3"` buffered per FR-003), test only-newlines (`"\n\n\n"`) each trigger flush, test `\r` is NOT treated as line terminator (FR-003), test `\r\n` on Windows buffers until `\n` (FR-003), test whitespace-only (no newlines) stays buffered (FR-003 edge case), test `Flush()` forces buffer to queue without newline (FR-015), test `Write(null)` is silently ignored (FR-018), test `Write("")` is silently ignored (FR-018), test `Write(char)` delegates correctly (FR-023), test direct IOutput write path when no Stroke application is running (FR-008), test writing a very large string (1MB+) is processed without error (NFR-001). ~160 LOC.
- [x] T014 [P] [US2] Write batching tests in `tests/Stroke.Tests/Application/StdoutProxyBatchingTests.cs` — test rapid writes (10+ within 50ms) are batched into ≤2 repaints (SC-002), test configurable `SleepBetweenWrites` affects batch timing, test zero `SleepBetweenWrites` disables delay. Use real `Thread` and timing with tolerant assertions. ~80 LOC.

**Checkpoint**: User Story 2 complete — buffering and batching verified.

---

## Phase 5: User Story 3 — Standalone StdoutProxy Usage (Priority: P2)

**Goal**: StdoutProxy can be created directly and used as a TextWriter without PatchStdout.

**Independent Test**: Create StdoutProxy, write to it, flush, verify output delivered. Close and verify background thread terminates.

### Tests for User Story 3

- [x] T015 [P] [US3] Write lifecycle tests in `tests/Stroke.Tests/Application/StdoutProxyLifecycleTests.cs` — test `Close()` flushes remaining buffer and terminates flush thread within 1 second (SC-004), test `Close()` is idempotent (FR-011), test `Write()` after `Close()` is silently ignored (FR-019), test `Flush()` after `Close()` is silently ignored (FR-019), test `Dispose()` calls `Close()`, test that `Write()` returns without blocking on flush thread processing (FR-004 non-blocking verification), test `Fileno()` delegates to output (FR-021), test `IsAtty()` returns false when output has no stdout, test `IsAtty()` delegates when stdout available (FR-021), test `Fileno()` and `IsAtty()` work after `Close()` (FR-021). ~110 LOC.

**Checkpoint**: User Story 3 complete — standalone proxy lifecycle verified.

---

## Phase 6: User Story 4 — Raw VT100 Passthrough (Priority: P3)

**Goal**: When `raw=true`, VT100 escape sequences pass through unmodified; when `raw=false`, they are escaped.

**Independent Test**: Create proxy with `raw=true`, write ANSI text, verify `IOutput.WriteRaw` called. Create with `raw=false`, verify `IOutput.Write` called (which escapes 0x1B → '?').

### Tests for User Story 4

- [x] T016 [P] [US4] Write raw mode tests in `tests/Stroke.Tests/Application/StdoutProxyRawModeTests.cs` — test `raw=true` routes through `IOutput.WriteRaw()` (escape sequences unmodified), test `raw=false` (default) routes through `IOutput.Write()` (escapes 0x1B → '?'), test raw mode toggle at construction time is immutable, test `EnableAutowrap()` called before each write in both modes (FR-014). ~60 LOC.

**Checkpoint**: User Story 4 complete — raw/non-raw routing verified.

---

## Phase 7: User Story Cross-Cutting — Thread Safety (Priority: P1)

**Goal**: Concurrent writes from 4+ threads produce no data loss, corruption, or deadlock.

### Tests for Thread Safety

- [x] T017 [P] [US1] Write thread safety tests in `tests/Stroke.Tests/Application/StdoutProxyConcurrencyTests.cs` — test 4 threads writing unique identifiable strings concurrently, verify all strings appear in output without interleaving corruption (SC-003), test 8 threads for scalability (SC-003 SHOULD), test 16 threads for stress, test concurrent Write + Flush from different threads (edge case), test concurrent Write + Close (thread safety edge case), test no deadlock under contention. Use real `Thread` instances, `CountdownEvent` for synchronization, timeout-based deadlock detection. ~120 LOC.

**Checkpoint**: Thread safety verified across all user stories.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories.

- [x] T018 Run all tests, verify ≥80% code coverage across `StdoutProxy.cs`, `StdoutPatching.cs`, `FlushItem.cs` (SC-006). Fix any failing tests.
- [x] T019 Run `quickstart.md` code examples mentally or via build verification — ensure API usage in quickstart matches actual implementation.
- [x] T020 Verify no source file exceeds 1,000 LOC (Constitution X). If `StdoutProxy.cs` approaches limit (~215 LOC estimated), no split needed.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 (T001) — BLOCKS all user stories
- **Phase 3 (US1)**: Depends on Phase 2 completion
- **Phase 4 (US2)**: Depends on Phase 2 completion — can run in parallel with Phase 3
- **Phase 5 (US3)**: Depends on Phase 2 completion — can run in parallel with Phase 3/4
- **Phase 6 (US4)**: Depends on Phase 2 completion — can run in parallel with Phase 3/4/5
- **Phase 7 (Thread Safety)**: Depends on Phase 2 completion — can run in parallel with Phase 3-6
- **Phase 8 (Polish)**: Depends on all previous phases

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Phase 2 (StdoutProxy core) — needs StdoutPatching (T012)
- **User Story 2 (P1)**: Depends on Phase 2 only — tests buffering mechanics directly on StdoutProxy
- **User Story 3 (P2)**: Depends on Phase 2 only — tests lifecycle directly on StdoutProxy
- **User Story 4 (P3)**: Depends on Phase 2 only — tests raw/non-raw routing on StdoutProxy

### Within Each User Story

- Tests written FIRST, verified to fail before implementation (TDD where tests precede impl)
- For US1: tests (T010, T011) → StdoutPatching implementation (T012)
- For US2-4: tests only (implementation already in Phase 2)
- Thread safety tests (T017) verify cross-cutting concern

### Parallel Opportunities

- **Phase 1**: T001 is a single task — no parallelism needed
- **Phase 2**: T002 must come first (skeleton), then T003-T009 can be built sequentially (same file)
- **Phase 3-7**: All user story test tasks marked [P] can run in parallel across stories:
  - T010, T011, T013, T014, T015, T016, T017 are ALL in different files and can be written in parallel
- **Within Phase 3**: T010 and T011 can run in parallel (different test files), then T012

---

## Parallel Example: All Test Files

```bash
# After Phase 2 completes, launch ALL test files in parallel (different files, no dependencies):
Task T010: "StdoutPatchingTests in tests/Stroke.Tests/Application/StdoutPatchingTests.cs"
Task T011: "StdoutProxyTests in tests/Stroke.Tests/Application/StdoutProxyTests.cs"
Task T013: "StdoutProxyBufferingTests in tests/Stroke.Tests/Application/StdoutProxyBufferingTests.cs"
Task T014: "StdoutProxyBatchingTests in tests/Stroke.Tests/Application/StdoutProxyBatchingTests.cs"
Task T015: "StdoutProxyLifecycleTests in tests/Stroke.Tests/Application/StdoutProxyLifecycleTests.cs"
Task T016: "StdoutProxyRawModeTests in tests/Stroke.Tests/Application/StdoutProxyRawModeTests.cs"
Task T017: "StdoutProxyConcurrencyTests in tests/Stroke.Tests/Application/StdoutProxyConcurrencyTests.cs"

# Then the one implementation file:
Task T012: "StdoutPatching in src/Stroke/Application/StdoutPatching.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: FlushItem (T001)
2. Complete Phase 2: StdoutProxy core (T002-T009)
3. Complete Phase 3: StdoutPatching + tests (T010-T012)
4. **STOP and VALIDATE**: PatchStdout works end-to-end, Console output appears above prompt
5. Run tests, verify passing

### Incremental Delivery

1. Phase 1 + Phase 2 → StdoutProxy core ready
2. Add US1 (Phase 3) → PatchStdout works → **MVP!**
3. Add US2 (Phase 4) → Buffering/batching verified
4. Add US3 (Phase 5) → Standalone lifecycle verified
5. Add US4 (Phase 6) → Raw mode verified
6. Add Thread Safety (Phase 7) → Concurrency verified
7. Polish (Phase 8) → Coverage, quickstart, file size checks

### File Summary

| File | Phase | Est. LOC |
|------|-------|----------|
| `src/Stroke/Application/FlushItem.cs` | 1 | ~20 |
| `src/Stroke/Application/StdoutProxy.cs` | 2 | ~215 |
| `src/Stroke/Application/StdoutPatching.cs` | 3 | ~50 |
| `tests/Stroke.Tests/Application/StdoutPatchingTests.cs` | 3 | ~120 |
| `tests/Stroke.Tests/Application/StdoutProxyTests.cs` | 3 | ~80 |
| `tests/Stroke.Tests/Application/StdoutProxyBufferingTests.cs` | 4 | ~160 |
| `tests/Stroke.Tests/Application/StdoutProxyBatchingTests.cs` | 4 | ~80 |
| `tests/Stroke.Tests/Application/StdoutProxyLifecycleTests.cs` | 5 | ~110 |
| `tests/Stroke.Tests/Application/StdoutProxyRawModeTests.cs` | 6 | ~60 |
| `tests/Stroke.Tests/Application/StdoutProxyConcurrencyTests.cs` | 7 | ~120 |
| **Total** | | **~1,015** |

---

## Notes

- All tasks reference specific FRs, NFRs, SCs, and research decisions (RT-xxx) for traceability
- Constitution VIII: No mocks — tests use real AppSession, IOutput, Thread, BlockingCollection
- Constitution X: No file exceeds 1,000 LOC (StdoutProxy.cs ~215 LOC, largest test file ~150 LOC)
- Constitution XI: Thread safety via System.Threading.Lock + BlockingCollection
- Constitution XII: All contracts remain in markdown (contracts/*.md)
- Python source reference: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/patch_stdout.py` (301 lines)
