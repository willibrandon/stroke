# Requirements Review Checklist: Win32 Event Loop Utilities

**Purpose**: Validate requirements quality, completeness, and clarity for PR review
**Created**: 2026-02-03
**Feature**: [spec.md](../spec.md)
**Actor**: Reviewer (PR Review)
**Focus**: Full Coverage (API Contract, Error Handling, Async Patterns, Cross-Platform, PTK Fidelity)
**Status**: ✅ All items complete (49/49)

## API Contract Quality

- [x] CHK001 - Are method signatures explicitly defined with parameter types and return types? [Completeness, Spec §API Contract]
  → Finding: Added "API Contract" section with full method table (6 methods with parameters and return types)

- [x] CHK002 - Is the `timeout` parameter type (int vs uint vs TimeSpan) explicitly specified? [Clarity, Spec §API Contract]
  → Finding: Added "Parameter Types" section specifying `int` for timeout with DWORD semantics

- [x] CHK003 - Is the return type for "signaled handle" unambiguously defined as nullable (`nint?`)? [Clarity, Spec §API Contract]
  → Finding: Added "Return Value Semantics" section specifying `nint?` with null conditions

- [x] CHK004 - Are the `WaitTimeout` and `Infinite` constant values explicitly documented? [Completeness, Spec §API Contract]
  → Finding: Added "Public Constants" table with exact values (0x00000102 and -1)

- [x] CHK005 - Is the `IReadOnlyList<nint>` vs `nint[]` parameter type decision documented? [Clarity, Spec §API Contract]
  → Finding: Added "Parameter Types" section explaining IReadOnlyList choice and internal conversion

- [x] CHK006 - Are all six public methods (WaitForHandles, WaitForHandlesAsync, CreateWin32Event, SetWin32Event, ResetWin32Event, CloseWin32Event) explicitly enumerated? [Completeness, Spec §API Contract]
  → Finding: Added "Public Methods" table listing all 6 methods with full signatures

## Error Handling Completeness

- [x] CHK007 - Are exception types specified for all Win32 API failure modes? [Completeness, Spec §Exception Handling]
  → Finding: Added "Exception Handling" section with full exception type table

- [x] CHK008 - Is the exception type for invalid handles explicitly named (`Win32Exception`)? [Clarity, Spec §Exception Handling]
  → Finding: Table row specifies Win32Exception for invalid handles with Marshal.GetLastWin32Error() source

- [x] CHK009 - Is the exception type for exceeding 64 handles specified (`ArgumentOutOfRangeException`)? [Clarity, Spec §Exception Handling]
  → Finding: Table row specifies ArgumentOutOfRangeException for parameter validation; FR-014 added

- [x] CHK010 - Is the behavior for `WAIT_FAILED` return value explicitly defined? [Completeness, Spec §Exception Handling]
  → Finding: Table row specifies Win32Exception thrown when WAIT_FAILED returned

- [x] CHK011 - Are error details (Win32 error codes, messages) documented as part of exception behavior? [Clarity, Spec §Exception Handling]
  → Finding: Added "Error Code Examples" table and "Exception Message Format" section

- [x] CHK012 - Is the behavior for double-close (closing already-closed handle) specified? [Coverage, Spec §Edge Cases]
  → Finding: Edge case updated to specify Win32Exception with ERROR_INVALID_HANDLE (0x6)

- [x] CHK013 - Is the behavior for resource exhaustion during `CreateEvent` specified? [Coverage, Spec §Edge Cases]
  → Finding: Edge case updated to specify Win32Exception with ERROR_NO_SYSTEM_RESOURCES example

## Async Pattern Correctness

- [x] CHK014 - Is the 100ms polling interval for infinite timeout explicitly documented in requirements? [Clarity, Spec §Async Behavior]
  → Finding: Added "Async Behavior" section with "Polling Strategy" subsection

- [x] CHK015 - Is the cancellation token behavior (return null vs throw) unambiguously specified? [Clarity, Spec §Async Behavior]
  → Finding: Added "Cancellation Semantics" section: returns null, does NOT throw OperationCanceledException

- [x] CHK016 - Are async timeout semantics defined separately from sync timeout semantics? [Consistency, Spec §Async Behavior]
  → Finding: Async behavior section separate from sync; polling wraps sync timeout

- [x] CHK017 - Is the async return type (`Task<nint?>`) explicitly specified? [Completeness, Spec §API Contract]
  → Finding: Method table shows `Task<nint?>` return type for WaitForHandlesAsync

- [x] CHK018 - Is the interaction between cancellation token and finite timeout defined? [Coverage, Spec §Async Behavior]
  → Finding: Added "Timeout + Cancellation Interaction" section with priority rules

- [x] CHK019 - Are deadlock prevention requirements documented for async operations? [Coverage, Spec §Async Behavior]
  → Finding: Added "Deadlock Prevention" section: Task.Run, does not block sync context

## Cross-Platform Safety

- [x] CHK020 - Is the `[SupportedOSPlatform("windows")]` attribute requirement documented? [Completeness, Spec §Platform Requirements]
  → Finding: Added "Platform Requirements" section with explicit attribute requirement

- [x] CHK021 - Is the behavior on non-Windows platforms specified (PlatformNotSupportedException)? [Clarity, Spec §Platform Requirements]
  → Finding: Platform Gating subsection specifies PlatformNotSupportedException at runtime

- [x] CHK022 - Is the 8-byte HANDLE requirement on 64-bit systems explicitly stated? [Clarity, Spec §FR-011]
  → Finding: FR-011 retained; Architecture Support table shows 8 bytes for x64/ARM64

- [x] CHK023 - Is the 4-byte HANDLE behavior on 32-bit systems documented? [Coverage, Spec §Platform Requirements]
  → Finding: Added Architecture Support table with x86 row showing 4 bytes

- [x] CHK024 - Is `nint` explicitly named as the handle type (vs IntPtr vs long)? [Clarity, Spec §FR-015]
  → Finding: Added FR-015 specifying nint; updated Assumptions to use nint not IntPtr

- [x] CHK025 - Is the minimum Windows version requirement (Windows 10+) documented? [Completeness, Spec §Platform Requirements]
  → Finding: Added "Minimum Version" subsection: Windows 10+ primary, Windows 7+ supported

## Python Prompt Toolkit Fidelity

- [x] CHK026 - Does `WaitForHandles` match Python PTK's `wait_for_handles` signature semantically? [Fidelity, Spec §PTK Fidelity]
  → Finding: Added "Python Prompt Toolkit Fidelity" section with API mapping table

- [x] CHK027 - Does `CreateWin32Event` match Python PTK's `create_win32_event` behavior (manual-reset, non-signaled)? [Fidelity, Spec §FR-005, §FR-006]
  → Finding: API mapping table confirms; FR-005/FR-006 specify manual-reset + non-signaled

- [x] CHK028 - Are the constant values (`WAIT_TIMEOUT = 0x102`, `INFINITE = -1`) consistent with Python PTK? [Fidelity, Spec §PTK Fidelity]
  → Finding: API mapping table shows matching values

- [x] CHK029 - Is the "returns handle from input list" behavior (reference equality) documented per PTK comment? [Fidelity, Spec §PTK Fidelity]
  → Finding: Added "Reference Equality" section quoting PTK comment and C# equivalent

- [x] CHK030 - Is the `__all__` export list (`wait_for_handles`, `create_win32_event`) reflected in public API? [Fidelity, Spec §PTK Fidelity]
  → Finding: Added "PTK Source Reference" section listing __all__ exports

- [x] CHK031 - Are SetEvent/ResetEvent/CloseHandle additions documented as C# extensions beyond PTK? [Deviation, Spec §PTK Fidelity]
  → Finding: Added "C# Extensions Beyond PTK" table with rationale for each

## Acceptance Criteria Measurability

- [x] CHK032 - Can "100% accuracy across 1000 test iterations" (SC-001) be objectively measured? [Measurability, Spec §SC-001]
  → Finding: Added measurement method: "automated test loop comparing returned handle to signaled handle"

- [x] CHK033 - Can "within 10% of specified timeout" (SC-002) be objectively measured? [Measurability, Spec §SC-002]
  → Finding: Added measurement method: "stopwatch timing of 100ms, 500ms, 1000ms timeouts"

- [x] CHK034 - Can "no resource leaks in 10,000 iterations" (SC-003) be objectively measured? [Measurability, Spec §SC-003]
  → Finding: Added measurement method: "handle count via Process.HandleCount before/after"

- [x] CHK035 - Is "integrate cleanly with async/await" (SC-004) defined with measurable criteria? [Clarity, Spec §SC-004]
  → Finding: Updated SC-004 with measurement: "test calling from SynchronizationContext, completing within 5 seconds"

- [x] CHK036 - Is "works correctly on 32-bit and 64-bit" (SC-005) defined with specific test scenarios? [Clarity, Spec §SC-005]
  → Finding: Updated SC-005 with measurement: "CI matrix testing x86 and x64 configurations"

- [x] CHK037 - Is "properly reports failures via exceptions" (SC-006) quantified with specific error codes? [Clarity, Spec §SC-006]
  → Finding: Updated SC-006 with measurement: "tests verifying Win32Exception.NativeErrorCode matches expected error"

## Edge Case & Scenario Coverage

- [x] CHK038 - Are all three acceptance scenarios for User Story 1 testable as written? [Coverage, Spec §US1]
  → Finding: Added 4th scenario for already-signaled handle; all scenarios use Given/When/Then format

- [x] CHK039 - Are all four acceptance scenarios for User Story 2 testable as written? [Coverage, Spec §US2]
  → Finding: All 4 scenarios verified testable with Given/When/Then format

- [x] CHK040 - Are all three acceptance scenarios for User Story 3 testable as written? [Coverage, Spec §US3]
  → Finding: Added 4th scenario for cancellation + timeout interaction; updated scenario 2 wording

- [x] CHK041 - Is the "empty handle list" behavior (return null immediately) explicitly in functional requirements? [Traceability, Spec §FR-004]
  → Finding: FR-004 confirmed: "System MUST handle empty handle lists gracefully by returning null immediately"

- [x] CHK042 - Are concurrent access scenarios (multiple threads waiting on same handles) addressed? [Coverage, Spec §Thread Safety]
  → Finding: Added "Thread Safety" section with "Concurrent Wait Behavior" subsection

- [x] CHK043 - Is the behavior when a handle is signaled before wait starts specified? [Coverage, Spec §Edge Cases]
  → Finding: Added edge case "Already-signaled handle" and US1 scenario 4

- [x] CHK044 - Are partial failure scenarios (some handles valid, some invalid) addressed? [Coverage, Spec §Edge Cases]
  → Finding: Added edge case "Partial invalid handles": Win32 API determines behavior, Win32Exception on WAIT_FAILED

## Dependencies & Assumptions

- [x] CHK045 - Is the dependency on Feature 051 (Win32Types) explicitly documented? [Traceability, Spec §Dependencies]
  → Finding: Dependencies section restructured with "Internal Dependencies" subsection

- [x] CHK046 - Is the SECURITY_ATTRIBUTES dependency validated as already ported? [Traceability, Spec §Dependencies]
  → Finding: Added "validated as complete" notation

- [x] CHK047 - Is the assumption "applications will close handles" a documented contract? [Assumption, Spec §Assumptions]
  → Finding: Updated assumption with "(caller responsibility)" clarification

- [x] CHK048 - Is the 64-handle limit assumption documented with reference to MAXIMUM_WAIT_OBJECTS? [Assumption, Spec §Assumptions]
  → Finding: Assumption explicitly references "(MAXIMUM_WAIT_OBJECTS)"

- [x] CHK049 - Are all kernel32.dll P/Invoke dependencies enumerated? [Completeness, Spec §Dependencies]
  → Finding: Added "External Dependencies (kernel32.dll)" table with all 5 functions

## Summary

| Category | Items | Status |
|----------|-------|--------|
| API Contract Quality | CHK001-CHK006 | ✅ 6/6 |
| Error Handling Completeness | CHK007-CHK013 | ✅ 7/7 |
| Async Pattern Correctness | CHK014-CHK019 | ✅ 6/6 |
| Cross-Platform Safety | CHK020-CHK025 | ✅ 6/6 |
| Python Prompt Toolkit Fidelity | CHK026-CHK031 | ✅ 6/6 |
| Acceptance Criteria Measurability | CHK032-CHK037 | ✅ 6/6 |
| Edge Case & Scenario Coverage | CHK038-CHK044 | ✅ 7/7 |
| Dependencies & Assumptions | CHK045-CHK049 | ✅ 5/5 |
| **Total** | **49 items** | **✅ 49/49** |

## Spec Enhancements Made

New sections added to spec:
1. **API Contract** — Method table, constants, parameter types, return semantics
2. **Exception Handling** — Exception types table, error codes, message format
3. **Platform Requirements** — Platform gating, architecture support, minimum version
4. **Async Behavior** — Polling strategy, cancellation semantics, timeout interaction, deadlock prevention
5. **Thread Safety** — Concurrency guarantees, concurrent wait behavior
6. **Python Prompt Toolkit Fidelity** — API mapping, C# extensions, reference equality, PTK source reference

New functional requirements:
- **FR-014**: Handle count validation (≤64)
- **FR-015**: `nint` type requirement

New acceptance scenarios:
- US1 #4: Already-signaled handle
- US3 #4: Cancellation + timeout interaction

Enhanced edge cases:
- Specific exception types and error codes
- Partial invalid handles scenario
- Already-signaled handle scenario
- Concurrent waiting scenario
