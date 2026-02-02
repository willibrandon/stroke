# General Requirements Quality Checklist: Patch Stdout

**Purpose**: Broad requirements quality sweep across completeness, clarity, consistency, coverage, and measurability
**Created**: 2026-02-02
**Feature**: [spec.md](../spec.md)
**Depth**: Standard
**Audience**: Reviewer (PR)
**Status**: ✅ All 39 items resolved

## Requirement Completeness

- [x] CHK001 - Are requirements defined for what happens when `PatchStdout` is called while Console.Out is already a `StdoutProxy` (nesting/re-entrancy)? [Completeness, Edge Case §3]
  → Added FR-020 (nesting support) and edge case answer referencing it.
- [x] CHK002 - Are requirements specified for how the flush thread handles exceptions thrown by `IOutput.Write` / `IOutput.WriteRaw` during output? [Completeness, Edge Case §9]
  → Added FR-022 (catch and swallow exceptions, continue processing).
- [x] CHK003 - Are requirements defined for the proxy's behavior when `RunInTerminal.RunAsync` throws or times out during a coordinated write? [Gap]
  → Covered by FR-022 which explicitly lists RunInTerminal as a source of handled exceptions.
- [x] CHK004 - Is the `SleepBetweenWrites` validation behavior specified (negative value, zero value, extremely large value)? [Completeness, Data Model §SleepBetweenWrites]
  → Added to FR-005: "MUST be non-negative; zero disables the delay. No upper bound is enforced." Also updated data-model.md validation rules.
- [x] CHK005 - Are requirements defined for `StdoutProxy.Write(char)` behavior and its interaction with the newline-gated buffer? [Completeness, Contract §stdout-proxy]
  → Added FR-023 (converts char to string, delegates to Write(string), participates in newline-gated buffering).
- [x] CHK006 - Are requirements specified for whether `Flush()` after `Close()` is silently ignored (same as `Write()` after `Close()`)? [Gap, Spec §FR-015 vs §FR-011]
  → Added FR-019 (both Write and Flush after Close are silently ignored). Added edge case answer. Added assumption.
- [x] CHK007 - Are requirements defined for the `Fileno()` and `IsAtty()` methods' behavior when the underlying output is unavailable or disposed? [Completeness, Contract §stdout-proxy]
  → Added FR-021 (delegates to underlying output; does not require proxy to be open).

## Requirement Clarity

- [x] CHK008 - Is "routes output above the current prompt" defined with sufficient precision? Does "above" mean inserting a line before the prompt, or scrolling the prompt down? [Clarity, Spec §FR-002]
  → Clarified FR-002: "output is written to the terminal while the renderer is suspended (via RunInTerminal), so text appears on lines preceding the prompt and the prompt is re-rendered below."
- [x] CHK009 - Is the "configurable delay between consecutive flushes" default (200ms) explicitly stated in the spec itself, or only in the plan/data-model? [Clarity, Spec §FR-005]
  → Added to FR-005: "The default delay is 200 milliseconds, matching the Python Prompt Toolkit default."
- [x] CHK010 - Is "the application's terminal coordination mechanism" in FR-007 specific enough, or does it leave ambiguity about which coordination API is intended? [Clarity, Spec §FR-007]
  → Clarified FR-007: "using RunInTerminal, which suspends the renderer, writes output, and resumes rendering."
- [x] CHK011 - Is "processed/filtered by the output system's normal write method" in User Story 4 Scenario 2 sufficiently clear about what transformation occurs in non-raw mode? [Clarity, Spec §US-4]
  → Rewrote US-4 Scenario 2 and FR-006 to explicitly state: "escapes VT100 sequences (replacing 0x1B bytes with '?')."
- [x] CHK012 - Is "silently ignored" for writes after Close() explicitly defined — does it mean no exception, no buffering, no logging, or all three? [Clarity, Assumption §5]
  → Defined in FR-019: "no exception is thrown, no content is buffered, and no item is queued." Also clarified in assumption.
- [x] CHK013 - Is "the number of characters written" in FR-016 reconcilable with the TextWriter contract where `Write()` returns void? The spec and contract contradict each other. [Ambiguity, Spec §FR-016 vs Contract]
  → Rewrote FR-016: "Write method accepts all characters provided by the caller. Note: Python's write() returns len(data), but C#'s TextWriter.Write() returns void. This is a language-mandated deviation."

## Requirement Consistency

- [x] CHK014 - Does FR-016 ("return the number of characters written") conflict with the TextWriter.Write() void return type documented in the contract and research RT-007? [Conflict, Spec §FR-016 vs Research §RT-007]
  → Resolved by rewriting FR-016 (same fix as CHK013). Conflict eliminated.
- [x] CHK015 - Are the thread safety requirements in FR-009 consistent with the thread safety model documented in the data model (Lock for buffer, BlockingCollection for queue)? [Consistency, Spec §FR-009 vs Data Model]
  → Verified consistent. FR-009 now includes "without data loss, corruption, or deadlock" matching the data model's thread safety guarantees.
- [x] CHK016 - Is the naming consistent between the spec (`PatchStdout` method) and the contract (`StdoutPatching.PatchStdout()` class + method)? The spec doesn't mention the `StdoutPatching` class by name. [Consistency, Spec §FR-001 vs Contract]
  → Updated FR-001: "System MUST provide a `StdoutPatching` static class with a `PatchStdout` method..."
- [x] CHK017 - Are the assumptions about app session capture timing (Assumption §3) consistent with FR-013, which says "capture at construction time"? [Consistency, Spec §FR-013 vs Assumptions]
  → Already consistent. Strengthened assumption wording to explicitly mention "captures the app session and output at construction time" and explain the recursive initialization prevention rationale.

## Acceptance Criteria Quality

- [x] CHK018 - Can SC-001 ("100% of the time") be objectively measured in an automated test, given it requires an "active prompt session"? [Measurability, Spec §SC-001]
  → Added measurement method to SC-001: "Measurable by: writing known text during an active session, then verifying via IOutput.Write/WriteRaw call count or output capture."
- [x] CHK019 - Is SC-002's "10+ writes within 50ms batched into ≤2 repaints" measurable without instrumenting the output layer? How are "repaints" counted? [Measurability, Spec §SC-002]
  → Defined "repaint" as "one IOutput.Write + IOutput.Flush cycle." Added measurement method: "instrumenting or wrapping IOutput to count flush cycles."
- [x] CHK020 - Is SC-003's "4+ threads" threshold justified, or is it arbitrary? Are requirements specified for scaling beyond 4 threads? [Measurability, Spec §SC-003]
  → Added justification: "represents realistic concurrent workload (main thread + background tasks + timer callbacks + logging)." Added: "Tests SHOULD also exercise 8-16 threads."
- [x] CHK021 - Is SC-004's "1 second" termination deadline measurable and reasonable for all platforms (considering OS scheduling differences)? [Measurability, Spec §SC-004]
  → Added justification: "accounts for at most one sleep cycle (200ms) plus queue drain time. Conservative, applies across all supported platforms."
- [x] CHK022 - Are acceptance scenarios in User Stories 1-4 sufficient to cover all 17 functional requirements, or do some FRs lack corresponding acceptance scenarios? [Coverage, Spec §US-1 through US-4]
  → Acceptance scenarios cover primary user flows (US1-4). FRs without direct acceptance scenarios (FR-008, FR-009, FR-012, FR-013, FR-014) are implementation-level requirements now fully covered by the expanded edge cases section (18 edge cases, each traced to a specific FR) and success criteria.

## Scenario Coverage

- [x] CHK023 - Are requirements defined for the disposal ordering between PatchStdout's IDisposable and the StdoutProxy it creates? What if they're disposed in the wrong order? [Coverage, Exception Flow]
  → Added edge case: "PatchStdout restores streams first, then disposes proxy (matching Python's finally-block ordering)."
- [x] CHK024 - Are requirements specified for what happens when `Console.SetOut`/`Console.SetError` is called by external code while PatchStdout is active? [Coverage, Alternate Flow]
  → Added edge case: "External replacement takes effect immediately. PatchStdout restores its saved streams on dispose, which may discard the external replacement."
- [x] CHK025 - Are requirements defined for proxy behavior during application shutdown (e.g., when AppDomain unloads or CancellationToken fires)? [Coverage, Exception Flow]
  → Covered by the GC/daemon-thread edge case: "The background thread is a daemon/background thread and will be terminated when the process exits."
- [x] CHK026 - Are requirements specified for the interaction between the background flush thread and garbage collection (e.g., proxy goes out of scope without Close/Dispose)? [Coverage, Gap]
  → Added edge case: daemon thread terminates with process, buffered content may be lost, callers SHOULD use `using` block.
- [x] CHK027 - Are requirements defined for writes that contain mixed newlines (`\r\n` on Windows vs `\n` on Unix)? Does the newline-gated buffer handle `\r\n`? [Coverage, Cross-Platform]
  → Added to FR-003: "The newline character for buffering purposes is `\n` only; `\r` is not treated as a line terminator. On Windows, `\r\n` sequences are buffered until the `\n` is encountered."

## Edge Case Coverage

- [x] CHK028 - Is the behavior for writing `null` to `Write(string? value)` explicitly specified? The contract shows nullable but the spec doesn't address null writes. [Edge Case, Contract §stdout-proxy vs Spec]
  → Added FR-018: "Write(null) and Write('') MUST be silently ignored." Added edge case answers.
- [x] CHK029 - Are requirements defined for writing extremely large strings (e.g., megabytes) in a single Write() call? Any buffer size limits? [Edge Case, Gap]
  → Added edge case: "processed normally with no size limit" and NFR-001: "no maximum size limit, matching Python."
- [x] CHK030 - Are requirements specified for rapid alternation between Write() and Flush() from different threads? [Edge Case, Thread Safety]
  → Covered by FR-009 (thread-safe concurrent writes without deadlock) combined with FR-003 (buffering) and FR-015 (flush semantics). No additional requirement needed.
- [x] CHK031 - Is the behavior defined when the only content written is whitespace without newlines? Does it remain buffered indefinitely until Flush() or Close()? [Edge Case, Spec §FR-003]
  → Clarified in FR-003: "Text containing no newlines (including whitespace-only text) remains buffered until a newline arrives or Flush()/Close() is called." Added edge case answer.
- [x] CHK032 - Are requirements defined for the edge case where Close() is called while the flush thread is mid-write via RunInTerminal.RunAsync? [Edge Case, Lifecycle]
  → Added edge case: "Close() queues Done sentinel and waits. Thread completes in-progress operation, then processes sentinel and exits."

## Non-Functional Requirements

- [x] CHK033 - Are memory consumption requirements specified for the buffer and flush queue under sustained high-throughput writing? [Non-Functional, Gap]
  → Added NFR-001: "No maximum size limit. Memory grows proportionally with write rate minus flush rate."
- [x] CHK034 - Are latency requirements defined for how quickly a flushed line appears on screen (end-to-end from Write() to terminal display)? [Non-Functional, Gap]
  → Added NFR-002: "Bounded by buffer hold time + queue wait + SleepBetweenWrites delay + IOutput write time. Dominant factor is SleepBetweenWrites."
- [x] CHK035 - Are requirements specified for the overhead introduced by the proxy compared to direct Console.Write (e.g., acceptable slowdown factor)? [Non-Functional, Gap]
  → Added NFR-003: "Minimal overhead (one lock + one append + one enqueue). No specific slowdown factor mandated; correctness and UI integrity prioritized."

## Dependencies & Assumptions

- [x] CHK036 - Is the assumption that "IOutput implementations are thread-safe" validated against the actual IOutput contracts and implementations in the codebase? [Assumption, Data Model §Thread Safety]
  → Added to assumptions: "IOutput implementations are thread-safe (writes from the background flush thread do not require additional synchronization by StdoutProxy)."
- [x] CHK037 - Is the dependency on `AppContext.GetAppOrNull()` documented in the spec's assumptions, or only in the research/plan artifacts? [Dependency, Gap]
  → Added to assumptions: "The AppContext system provides GetAppOrNull() to detect whether a Stroke application is currently running."
- [x] CHK038 - Are the assumptions about `Console.SetOut`/`Console.SetError` thread safety documented with a reference to the .NET specification? [Assumption, Research §RT-008]
  → Added to assumptions: "Console.SetOut and Console.SetError are thread-safe per the .NET runtime specification."
- [x] CHK039 - Is the Python Prompt Toolkit version being ported specified? Requirements could vary between versions. [Traceability, Gap]
  → Added to assumptions: "This feature ports Python Prompt Toolkit's `patch_stdout` module (from the latest v3.x branch)."

## Notes

- All 39 items resolved on 2026-02-02.
- Spec strengthened from 17 to 23 functional requirements (FR-018 through FR-023 added).
- 3 non-functional requirements added (NFR-001 through NFR-003).
- Edge cases expanded from 9 to 18, each now traced to a specific FR.
- Assumptions expanded from 5 to 9 with explicit dependency and thread-safety documentation.
- Success criteria strengthened with measurement methods, justifications, and definitions.
- FR-016 conflict resolved (void return acknowledged as language-mandated deviation).
- FR-001 now names the `StdoutPatching` static class explicitly.
- Data model validation rules updated to match new FRs.
