# Tasks: System Clipboard

**Input**: Design documents from `/specs/066-system-clipboard/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Included — Constitution VIII requires 80% coverage (SC-007), and SC-008 requires concurrent stress tests.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization — create internal abstractions and test infrastructure that all stories depend on

- [x] T001 Add `IsWsl` property to `PlatformUtils` in `src/Stroke/Core/PlatformUtils.cs` — reads `/proc/version` and checks for case-insensitive "microsoft" match (FR-014). Return `false` on non-Linux platforms or if `/proc/version` is unreadable. Cache the result in a `Lazy<bool>` since `/proc/version` won't change at runtime
- [x] T002 Create `IClipboardProvider` internal interface in `src/Stroke/Clipboard/IClipboardProvider.cs` — two methods: `void SetText(string text)` and `string GetText()`. Include XML docs per contract in `contracts/clipboard-provider.md`. Implementations are stateless and thread-safe by design
- [x] T003 [P] Create `StringClipboardProvider` in `src/Stroke/Clipboard/StringClipboardProvider.cs` — real `IClipboardProvider` backed by a `string` field (not a mock per Constitution VIII). Thread-safe via `Lock`. Constructor accepts optional `initialText` parameter (default `""`). Per contract in `contracts/clipboard-provider.md`
- [x] T004 [P] Create `ClipboardProviderNotAvailableException` in `src/Stroke/Clipboard/ClipboardProviderNotAvailableException.cs` — public sealed exception with two constructors: `(string message)` and `(string message, Exception innerException)`. Per contract in `contracts/system-clipboard.md`

**Checkpoint**: Internal abstractions ready — provider implementations and SystemClipboard can now be built

---

## Phase 2: Foundational (Provider Implementations)

**Purpose**: Platform-specific clipboard providers — MUST complete before SystemClipboard can delegate to them

**⚠️ CRITICAL**: All providers must exist before SystemClipboard and ClipboardProviderDetector can be implemented

- [x] T005 [P] Create `WindowsClipboardProvider` in `src/Stroke/Clipboard/WindowsClipboardProvider.cs` — Win32 P/Invoke provider implementing `IClipboardProvider`. Nested `ClipboardApi` class with `[LibraryImport]` declarations for `OpenClipboard`, `CloseClipboard`, `EmptyClipboard`, `GetClipboardData`, `SetClipboardData`, `GlobalAlloc`, `GlobalLock`, `GlobalUnlock`, `GlobalFree`. Uses `CF_UNICODETEXT = 13`. `SetText`: OpenClipboard → EmptyClipboard → GlobalAlloc(GMEM_MOVEABLE) → GlobalLock → copy UTF-16 → GlobalUnlock → SetClipboardData → CloseClipboard in finally. GlobalFree only on failure before SetClipboardData. `GetText`: OpenClipboard → GetClipboardData → GlobalLock → Marshal.PtrToStringUni → CloseClipboard in finally. All failures return empty string (read) or swallow (write) per FR-008. `[SupportedOSPlatform("windows")]`. Per contract and FR-002, FR-008
- [x] T006 [P] Create `MacOsClipboardProvider` in `src/Stroke/Clipboard/MacOsClipboardProvider.cs` — process-based provider using `pbcopy` (stdin write) and `pbpaste` (stdout read). `ProcessStartInfo.ArgumentList` (no arguments needed), `UseShellExecute = false`, `CreateNoWindow = true`, `RedirectStandardInput/Output/Error`. 5-second wall-clock timeout via `CancellationTokenSource`. Kill process on timeout. Return empty string on read failure, swallow write failure. `[SupportedOSPlatform("macos")]`. Per contract and FR-002, FR-008, FR-009, FR-012, FR-016, FR-017, FR-019
- [x] T007 [P] Create `LinuxClipboardProvider` in `src/Stroke/Clipboard/LinuxClipboardProvider.cs` — process-based provider accepting `copyCommand`, `copyArgs`, `pasteCommand`, `pasteArgs` in constructor (all readonly). Same process execution pattern as macOS provider: `ArgumentList`, `UseShellExecute = false`, `CreateNoWindow = true`, 5-second timeout, Kill on timeout. Return empty string on read failure, swallow write failure. Per contract and FR-002, FR-008, FR-009, FR-012, FR-013, FR-016, FR-017, FR-019, FR-023
- [x] T008 [P] Create `WslClipboardProvider` in `src/Stroke/Clipboard/WslClipboardProvider.cs` — process-based provider. Write: `clip.exe` (stdin). Read: `powershell.exe` with `-NoProfile`, `-Command`, `Get-Clipboard` via `ArgumentList`. Strip trailing CRLF from PowerShell output (`TrimEnd('\r', '\n')`). `UseShellExecute = false`, `CreateNoWindow = true`, 5-second timeout. Per contract and FR-002, FR-008, FR-009, FR-012, FR-016, FR-017, FR-018, FR-019

**Checkpoint**: All four platform providers exist (StringClipboardProvider already created in Phase 1) — detector and SystemClipboard can now use them

---

## Phase 3: User Story 2 - Opt-In Clipboard Activation (Priority: P2) 🎯 MVP

**Goal**: Developers can create a `SystemClipboard` instance and pass it to prompt sessions as a drop-in `IClipboard` replacement

**Why US2 first**: US2 delivers the `SystemClipboard` class itself — without it, neither US1 (interop) nor US3 (detection) can function. The detector (US3) is used by the parameterless constructor, but the `SystemClipboard(IClipboardProvider)` constructor enables testing and explicit provider injection immediately.

**Independent Test**: Construct `SystemClipboard` with `StringClipboardProvider`, call `SetData`/`GetData`/`SetText`/`Rotate`, verify `IClipboard` contract is fulfilled

### Tests for User Story 2

- [x] T009 [P] [US2] Create `SystemClipboardTests` in `tests/Stroke.Tests/Clipboard/SystemClipboardTests.cs` — test core behavior using `StringClipboardProvider`: (1) `SetData` then `GetData` returns same `ClipboardData` (text and SelectionType preserved per FR-005), (2) `SetText` then `GetData` returns `Characters` type, (3) `GetData` on empty clipboard returns empty `ClipboardData`, (4) `Rotate` is a no-op (FR-015), (5) null `data` throws `ArgumentNullException`, (6) null `text` throws `ArgumentNullException`, (7) null provider throws `ArgumentNullException`, (8) provider write failure is silently swallowed (FR-008), (9) provider read failure returns empty `ClipboardData` (FR-008), (10) cache-before-write ordering (FR-020): if provider.SetText throws, `_lastData` is still cached, (11) value equality comparison (FR-021): same text from different string instances still returns cached data, (12) timeout simulation (SC-004): create a `StringClipboardProvider` subclass or wrapper that sleeps in `SetText`/`GetText` to verify the operation doesn't hang the caller beyond 5 seconds

### Implementation for User Story 2

- [x] T010 [US2] Create `SystemClipboard` in `src/Stroke/Clipboard/SystemClipboard.cs` — public sealed class implementing `IClipboard`. Fields: `_provider` (IClipboardProvider, readonly), `_lastData` (ClipboardData?, mutable), `_lock` (Lock, readonly). Two constructors: (1) `SystemClipboard() : this(ClipboardProviderDetector.Detect())` — the detector (T012) may not exist yet; if so, implement the parameterless constructor body as `throw new NotImplementedException()` and T013 will replace it with the real call. This makes the temporary state explicit and avoids silent NullReferenceException. (2) `SystemClipboard(IClipboardProvider provider)` with `ArgumentNullException.ThrowIfNull`. `SetData`: validate null → lock → cache `_lastData = data` → try `_provider.SetText(data.Text)` catch swallow (FR-008, FR-010, FR-020). `GetData`: lock → try `_provider.GetText()` catch return `""` → if `_lastData != null && text == _lastData.Text` return `_lastData` → else return `new ClipboardData(text, text.Contains('\n') ? SelectionType.Lines : SelectionType.Characters)` (FR-005, FR-006, FR-010, FR-021). `SetText`: delegates to `SetData(new ClipboardData(text))`. `Rotate`: no-op empty body (FR-015). All methods synchronized via `Lock.EnterScope()`. Full XML docs with thread safety documentation per Constitution XI. Per contract in `contracts/system-clipboard.md`

**Checkpoint**: `SystemClipboard` is functional with explicit provider injection. Can be tested with `StringClipboardProvider`. US2 acceptance scenarios 1-3 are verifiable (modulo auto-detection which comes in US3).

---

## Phase 4: User Story 3 - Cross-Platform Clipboard Detection (Priority: P3)

**Goal**: `SystemClipboard()` parameterless constructor auto-detects platform and selects correct provider

**Independent Test**: On macOS (dev platform), `new SystemClipboard()` succeeds and uses `MacOsClipboardProvider`. Detection logic is unit-testable via extracted helper methods.

### Tests for User Story 3

- [x] T011 [P] [US3] Create `ClipboardProviderDetectorTests` in `tests/Stroke.Tests/Clipboard/ClipboardProviderDetectorTests.cs` — test detection logic: (1) `Detect()` returns a non-null `IClipboardProvider` on the current platform (macOS dev machine), (2) the returned provider type matches the expected platform provider, (3) on the current platform, `SetText`/`GetText` round-trip works on the real OS clipboard (platform-gated integration test). Note: full cross-platform detection can't be unit-tested without environment manipulation; focus on verifiable behavior on the current platform and error paths

### Implementation for User Story 3

- [x] T012 [US3] Create `ClipboardProviderDetector` in `src/Stroke/Clipboard/ClipboardProviderDetector.cs` — internal static class with `public static IClipboardProvider Detect()`. Detection order per FR-013, FR-014 and contract: (1) `OperatingSystem.IsWindows()` → `new WindowsClipboardProvider()`, (2) `OperatingSystem.IsMacOS()` → `new MacOsClipboardProvider()`, (3) `PlatformUtils.IsLinux`: (a) `PlatformUtils.IsWsl` → verify `clip.exe` and `powershell.exe` accessible via tool detection → `new WslClipboardProvider()`, else throw WSL-specific guidance, (b) `WAYLAND_DISPLAY` env var set AND `wl-copy`/`wl-paste` found → `new LinuxClipboardProvider(wayland config: wl-copy, [], wl-paste, ["--no-newline"])`, (c) `xclip` found → `new LinuxClipboardProvider("xclip", ["-selection", "clipboard"], "xclip", ["-selection", "clipboard", "-o"])`, (d) `xsel` found → `new LinuxClipboardProvider("xsel", ["--clipboard", "--input"], "xsel", ["--clipboard", "--output"])`, (4) throw `ClipboardProviderNotAvailableException` with platform-specific message per FR-011. Tool detection: private helper using `Process.Start("which", toolName)` with 2-second timeout; returns bool. Include XML docs
- [x] T013 [US3] Wire `SystemClipboard()` parameterless constructor to `ClipboardProviderDetector.Detect()` in `src/Stroke/Clipboard/SystemClipboard.cs` — if T010 used a placeholder, replace it now: `public SystemClipboard() : this(ClipboardProviderDetector.Detect()) { }`

**Checkpoint**: `new SystemClipboard()` auto-detects platform. US3 acceptance scenarios 1-6 are verifiable. On macOS dev machine, full round-trip works.

---

## Phase 5: User Story 1 - Copy and Paste Between Stroke and External Applications (Priority: P1)

**Goal**: End-to-end clipboard interop — paste from external apps into Stroke, cut from Stroke to paste elsewhere

**Why US1 last**: Although P1 priority, US1 is an integration story that depends on US2 (SystemClipboard exists) and US3 (auto-detection works). The actual P1 value is already delivered once US2+US3 are complete — this phase adds selection type inference tests and integration verification.

**Independent Test**: On macOS, use `pbcopy` to put text on clipboard, construct `SystemClipboard`, call `GetData()`, verify text matches. Then `SetText()`, verify `pbpaste` returns the text.

### Tests for User Story 1

- [x] T014 [P] [US1] Add selection type inference tests to `tests/Stroke.Tests/Clipboard/SystemClipboardTests.cs` — test FR-006 edge cases: (1) external text with `\n` → `SelectionType.Lines`, (2) external text without `\n` → `SelectionType.Characters`, (3) external text with `\r\n` → `SelectionType.Lines` (since `\r\n` contains `\n`), (4) `SelectionType.Block` is NEVER inferred from external text, (5) same text written externally returns cached data (FR-005 indistinguishable case), (6) empty external text returns `Characters` type. All tests use `StringClipboardProvider` to simulate external modifications (write directly to provider, then call `GetData()` on `SystemClipboard`)
- [x] T015 [P] [US1] Add platform integration test to `tests/Stroke.Tests/Clipboard/MacOsClipboardProviderTests.cs` — platform-gated with `[SupportedOSPlatform("macos")]` skip condition: (1) `MacOsClipboardProvider.SetText` then `GetText` round-trip, (2) verify non-empty text survives round-trip, (3) verify empty text round-trip. These test real OS clipboard interaction on the dev machine

### Implementation for User Story 1

- [x] T016 [US1] Verify end-to-end clipboard interop on macOS using TUI driver — automated verification: (1) use `tui_launch` to start a Stroke prompt example configured with `SystemClipboard`, (2) use Bash to run `echo "hello from external" | pbcopy` to put text on OS clipboard, (3) use `tui_press_key` Ctrl+Y to paste, (4) use `tui_text` to verify "hello from external" appears in the prompt, (5) use `tui_send_text` to type text, `tui_press_key` Ctrl+A then Ctrl+W to select-all and kill, (6) use Bash `pbpaste` to verify the killed text is on the OS clipboard. This validates US1 acceptance scenarios 1-3. If TUI driver is unavailable, fall back to manual verification and document results

**Checkpoint**: Full clipboard interop verified. US1 acceptance scenarios 1-3 confirmed on macOS.

---

## Phase 6: User Story 4 - Selection Type Preservation (Priority: P3)

**Goal**: Vi/Emacs selection types (Lines, Characters, Block) are preserved across clipboard round-trips within the same session

**Independent Test**: Write `ClipboardData` with `SelectionType.Lines` via `SetData`, read back via `GetData`, verify type is `Lines` — already partially tested in US2, this phase adds comprehensive edge case coverage.

### Tests for User Story 4

- [x] T017 [P] [US4] Add selection type preservation tests to `tests/Stroke.Tests/Clipboard/SystemClipboardTests.cs` — comprehensive edge cases: (1) `SetData` with `Lines` type, `GetData` returns `Lines`, (2) `SetData` with `Block` type, `GetData` returns `Block`, (3) `SetData` with `Characters` type, `GetData` returns `Characters`, (4) external modification breaks cache — new text gets inferred type, (5) external modification to same text — cache still returned (FR-005), (6) multiple `SetData` calls — only last cached, (7) `SetData` then `SetText` — overwrites cache with `Characters` type

### Implementation for User Story 4

> No additional implementation needed — selection type preservation is already built into `SystemClipboard.GetData()` in T010. This phase only adds test coverage.

**Checkpoint**: Selection type preservation fully tested. US4 acceptance scenarios 1-3 confirmed.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Thread safety verification, coverage validation, and code quality

- [x] T018 [P] Add concurrent stress tests to `tests/Stroke.Tests/Clipboard/SystemClipboardTests.cs` — per SC-008 and Constitution XI: 10+ threads, 1000+ operations, verify no data corruption, deadlocks, or exceptions. Pattern: parallel `SetData`/`GetData`/`SetText` from multiple threads using `StringClipboardProvider`. Use `Task.WhenAll` with `Parallel.ForEachAsync` or explicit `Thread` creation. Verify final state is consistent (no garbled text, no null references)
- [x] T019 [P] Add `ClipboardProviderNotAvailableException` tests to `tests/Stroke.Tests/Clipboard/ClipboardProviderNotAvailableExceptionTests.cs` — test: (1) message constructor stores message, (2) message + inner exception constructor stores both, (3) exception is catchable as `Exception`
- [x] T020 [P] Add `StringClipboardProvider` tests to `tests/Stroke.Tests/Clipboard/StringClipboardProviderTests.cs` — test: (1) default constructor gives empty text, (2) initial text constructor, (3) `SetText`/`GetText` round-trip, (4) concurrent access (thread safety)
- [x] T021 Build and run full test suite — `dotnet build src/Stroke/Stroke.csproj` and `dotnet test tests/Stroke.Tests/ --filter "FullyQualifiedName~Clipboard"`. Verify all tests pass. Verify no existing tests are broken (SC-006). Specifically confirm FR-007: the default `PromptSession` still uses `InMemoryClipboard` (no code paths changed for non-adopters)
- [x] T022 Verify test coverage meets 80% — run coverage analysis on new clipboard files (`SystemClipboard`, providers, detector, exception). Target SC-007: 80% line coverage at the file level
- [x] T023 Run quickstart.md validation — verify the code examples in `specs/066-system-clipboard/quickstart.md` compile and work as documented

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on T002 (IClipboardProvider interface) from Phase 1
- **US2 (Phase 3)**: Depends on T002, T003 (StringClipboardProvider for testing), T004 (exception)
- **US3 (Phase 4)**: Depends on T001 (IsWsl), T005-T008 (all providers), T010 (SystemClipboard exists)
- **US1 (Phase 5)**: Depends on US2 + US3 (SystemClipboard with auto-detection)
- **US4 (Phase 6)**: Depends on US2 (SystemClipboard behavior already implemented)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 2 (P2)**: Can start after Phase 1 setup — only needs `IClipboardProvider` and `StringClipboardProvider`
- **User Story 3 (P3)**: Can start after Phase 2 foundational — needs all provider implementations
- **User Story 1 (P1)**: Can start after US2 + US3 — needs full SystemClipboard with auto-detection
- **User Story 4 (P3)**: Can start after US2 — only needs `SystemClipboard` with `StringClipboardProvider`; can run in parallel with US3

### Within Each User Story

- Tests written alongside implementation (not TDD — tests reference implementation types)
- Core class before integration tests
- Story complete before moving to next priority

### Parallel Opportunities

- **Phase 1**: T003 and T004 can run in parallel (after T002)
- **Phase 2**: T005, T006, T007, T008 can all run in parallel (all depend only on T002)
- **Phase 3**: T009 (tests) can run in parallel with T010 (implementation) since tests reference the type being built
- **Phase 4**: T011 can run in parallel with T012
- **Phase 5**: T014 and T015 can run in parallel
- **Phase 6**: T017 can run in parallel with Phase 5 work (independent test file sections)
- **Phase 7**: T018, T019, T020 can all run in parallel

---

## Parallel Example: Phase 2 (Foundational Providers)

```bash
# All four providers can be implemented simultaneously (different files, no dependencies):
Task: T005 "WindowsClipboardProvider in src/Stroke/Clipboard/WindowsClipboardProvider.cs"
Task: T006 "MacOsClipboardProvider in src/Stroke/Clipboard/MacOsClipboardProvider.cs"
Task: T007 "LinuxClipboardProvider in src/Stroke/Clipboard/LinuxClipboardProvider.cs"
Task: T008 "WslClipboardProvider in src/Stroke/Clipboard/WslClipboardProvider.cs"
```

---

## Implementation Strategy

### MVP First (US2 — Opt-In Clipboard with Explicit Provider)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: Foundational providers (T005-T008)
3. Complete Phase 3: User Story 2 — SystemClipboard with provider injection (T009-T010)
4. **STOP and VALIDATE**: Test `SystemClipboard` with `StringClipboardProvider`
5. At this point, developers can use `new SystemClipboard(provider)` with any provider

### Incremental Delivery

1. Setup + Foundational → Internal abstractions ready
2. Add US2 → SystemClipboard works with explicit providers → Test independently
3. Add US3 → Auto-detection works → `new SystemClipboard()` just works on all platforms
4. Add US1 → End-to-end interop verified on dev machine
5. Add US4 → Selection type edge cases fully tested
6. Polish → Coverage, stress tests, validation

### Suggested MVP Scope

**MVP = Phase 1 + Phase 2 + Phase 3 (US2)**: This delivers a fully functional `SystemClipboard` class that can be constructed with any provider and used as a drop-in `IClipboard` replacement. Auto-detection (US3) is the natural next increment.

---

## Task Summary

| Phase | Tasks | Parallel | Description |
|-------|-------|----------|-------------|
| Phase 1: Setup | T001-T004 | T003, T004 | Interface, test provider, exception, IsWsl |
| Phase 2: Foundational | T005-T008 | T005-T008 | Four platform providers |
| Phase 3: US2 | T009-T010 | T009 ∥ T010 | SystemClipboard + core tests |
| Phase 4: US3 | T011-T013 | T011 ∥ T012 | Detector + auto-detection |
| Phase 5: US1 | T014-T016 | T014, T015 | Selection inference + integration |
| Phase 6: US4 | T017 | — | Selection preservation edge cases |
| Phase 7: Polish | T018-T023 | T018-T020 | Stress tests, coverage, validation |

**Total tasks**: 23
**Parallel opportunities**: 14 tasks marked [P]
**New source files**: 8 (7 in `src/Stroke/Clipboard/`, 1 modified `PlatformUtils.cs`)
**New test files**: 4 (in `tests/Stroke.Tests/Clipboard/`)
**Estimated LOC**: ~600-800 (source) + ~400-500 (tests)

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- `StringClipboardProvider` is the testing backbone — enables all `SystemClipboard` behavior testing without OS clipboard access per Constitution VIII (no mocks)
- Process-based providers (macOS, Linux, WSL) share the same execution pattern — consider extracting a private helper method within each or a shared static helper, but don't over-abstract since each has slight variations (stdin vs stdout, argument lists, CRLF stripping)
- Win32 provider is self-contained with nested P/Invoke — follows `PlatformUtils.Vt100Detection` pattern per research R2
- US1 is P1 priority but implemented last because it's an integration story that depends on US2 + US3
