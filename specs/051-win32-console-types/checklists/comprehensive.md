# Checklist: Comprehensive Requirements Quality

**Feature**: 051-win32-console-types
**Purpose**: Unit tests for requirements — validates completeness, clarity, consistency, and measurability of Win32 Console Types specification
**Created**: 2026-02-02
**Focus Areas**: P/Invoke Correctness, API Fidelity, Cross-Platform Safety, Edge Cases
**Audience**: Author (pre-implementation), Reviewer (PR)

---

## Requirement Completeness

- [x] CHK001 - Are exact byte sizes specified for all 11 struct types? [Completeness, Spec §FR-001 through §FR-009, §FR-014, §FR-018] ✓ All structs now have explicit byte sizes
- [x] CHK002 - Are field offsets explicitly documented for the INPUT_RECORD union struct? [Completeness, Spec §FR-009] ✓ FieldOffset attributes documented
- [x] CHK003 - Are all fields of KEY_EVENT_RECORD specified with their exact C# types and sizes? [Completeness, Spec §FR-004] ✓ All fields with types and sizes
- [x] CHK004 - Are all fields of MOUSE_EVENT_RECORD specified with their exact C# types? [Completeness, Spec §FR-005] ✓ Fields now use typed enums
- [x] CHK005 - Are all ControlKeyState flag values enumerated with their hex constants? [Completeness, Spec §FR-011] ✓ All 10 values with hex
- [x] CHK006 - Are all MouseEventFlags values enumerated with their hex constants? [Completeness, Spec §FR-012] ✓ All 5 values with hex
- [x] CHK007 - Are all MouseButtonState values enumerated with their hex constants? [Completeness, Spec §FR-012] ✓ All 6 values with hex
- [x] CHK008 - Are all ConsoleInputMode flag values enumerated? [Completeness, Spec §FR-013] ✓ All 10 values with hex
- [x] CHK009 - Are all ConsoleOutputMode flag values enumerated? [Completeness, Spec §FR-013] ✓ All 6 values with hex
- [x] CHK010 - Are marshalling attributes specified for each P/Invoke parameter and return type? [Gap, Spec §FR-016] ✓ [In], [Out], [MarshalAs] documented
- [x] CHK011 - Is the calling convention specified for P/Invoke declarations? [Gap, Spec §FR-016] ✓ LibraryImport uses default stdcall
- [x] CHK012 - Are entry point names specified for Unicode variants (W-suffix functions)? [Gap, Spec §FR-016] ✓ W-suffix entry points documented
- [x] CHK013 - Is SetLastError behavior documented for error-returning P/Invoke methods? [Gap, Spec §FR-016] ✓ SetLastError = true specified
- [x] CHK014 - Are Width and Height computed properties specified for SMALL_RECT? [Gap, Spec §FR-002] ✓ Width/Height formulas added
- [x] CHK015 - Is an IsKeyDown convenience property specified for KEY_EVENT_RECORD? [Gap, Spec §FR-004] ✓ IsKeyDown property added
- [x] CHK016 - Is a HasFocus convenience property specified for FOCUS_EVENT_RECORD? [Gap, Spec §FR-008] ✓ HasFocus property added

---

## Requirement Clarity

- [x] CHK017 - Is "sequential memory layout" defined with the specific StructLayout attribute to use? [Clarity, Spec §FR-001] ✓ `[StructLayout(LayoutKind.Sequential)]` specified
- [x] CHK018 - Is "explicit memory layout" defined with LayoutKind.Explicit and FieldOffset attributes? [Clarity, Spec §FR-009] ✓ Explicit layout with FieldOffset documented
- [x] CHK019 - Is the union overlay mechanism for INPUT_RECORD clearly explained (all event records at offset 4)? [Clarity, Spec §FR-009] ✓ All fields at offset 4 documented
- [x] CHK020 - Are the EventType enum values documented as powers of 2 (bitmask pattern) or sequential? [Clarity, Spec §FR-010] ✓ Hex values show power-of-2 pattern
- [x] CHK021 - Is the relationship between EventType value and valid union field clearly specified? [Clarity, Spec §FR-009, §FR-010] ✓ Mapping table added to FR-009
- [x] CHK022 - Is "correct Windows API values" quantified with specific hex constants for each flag? [Clarity, Spec §FR-011, §FR-012, §FR-013] ✓ All hex constants enumerated
- [x] CHK023 - Is "platform-specific attributes" specified as [SupportedOSPlatform("windows")]? [Clarity, Spec §FR-017] ✓ Attribute and namespace specified
- [x] CHK024 - Is the term "native integer types" clarified as nint (IntPtr) for handles? [Clarity, Spec Assumptions] ✓ nint used consistently in FR-016
- [x] CHK025 - Is "correctly marshal data" defined with specific marshalling attributes and behaviors? [Clarity, Spec §SC-004] ✓ [In], [Out], [MarshalAs] documented in FR-016
- [x] CHK026 - Are field types specified consistently (e.g., "int" vs "Int32", "uint" vs "UInt32")? [Clarity] ✓ Consistent use of C# aliases (int, uint, short, char, nint)

---

## Requirement Consistency

- [x] CHK027 - Is ControlKeyState used consistently as the type for modifier state across KEY_EVENT_RECORD and MOUSE_EVENT_RECORD? [Consistency, Spec §FR-004, §FR-005] ✓ ControlKeyState enum used in both
- [x] CHK028 - Are COORD field names consistent (X/Y) across all structures that contain coordinates? [Consistency, Spec §FR-001, §FR-003, §FR-005, §FR-006] ✓ Coord type with X/Y used consistently
- [x] CHK029 - Is the naming convention consistent between C# (PascalCase) and Windows API (original names)? [Consistency] ✓ NFR-001 documents full naming mapping
- [x] CHK030 - Are all flags enums consistently marked with [Flags] attribute requirement? [Consistency, Spec §FR-011, §FR-012, §FR-013] ✓ [Flags] specified for all flags enums
- [x] CHK031 - Is readonly/immutable specified consistently for all struct types? [Consistency] ✓ NFR-002 specifies readonly struct requirement
- [x] CHK032 - Are handle types consistently specified as nint across all P/Invoke declarations? [Consistency, Spec §FR-016] ✓ nint used for all handle parameters
- [x] CHK033 - Is IEquatable<T> implementation specified consistently for value-semantic structs? [Consistency, Gap] ✓ NFR-003 specifies IEquatable requirements

---

## Acceptance Criteria Quality

- [x] CHK034 - Can "struct size equals 4 bytes" (COORD) be objectively measured? [Measurability, Spec §US-1] ✓ Marshal.SizeOf<T>() verification in SC-001
- [x] CHK035 - Can "all fields populated correctly" be objectively verified for CONSOLE_SCREEN_BUFFER_INFO? [Measurability, Spec §US-1] ✓ SC-004 specifies non-default value verification
- [x] CHK036 - Can "correct offset" for INPUT_RECORD union fields be objectively verified? [Measurability, Spec §US-1] ✓ SC-002 specifies Marshal.OffsetOf verification
- [x] CHK037 - Can "value equals 0x0018" for combined flags be objectively verified? [Measurability, Spec §US-2] ✓ Direct bitwise comparison
- [x] CHK038 - Can "non-null handle is returned" be objectively verified? [Measurability, Spec §US-3] ✓ SC-004 specifies handle != IntPtr.Zero
- [x] CHK039 - Can "clear indication that Windows is required" be objectively measured? [Ambiguity, Spec §US-3] ✓ SC-008 specifies CA1416 analyzer warning
- [x] CHK040 - Are expected struct sizes documented for all 11 struct types to enable verification? [Gap, Spec §SC-001] ✓ Full size table added to SC-001
- [x] CHK041 - Is the source of truth for "correct Windows API values" specified (Microsoft docs URL)? [Traceability, Spec §SC-003] ✓ MS Learn URLs in SC-003

---

## Scenario Coverage

- [x] CHK042 - Are requirements defined for creating struct instances with constructors? [Coverage, Gap] ✓ FR-001, FR-002, FR-018 specify constructors
- [x] CHK043 - Are requirements defined for struct equality comparison? [Coverage, Gap] ✓ NFR-003 specifies IEquatable requirements
- [x] CHK044 - Are requirements defined for struct ToString() representations? [Coverage, Gap] ✓ NFR-003 specifies ToString() override
- [x] CHK045 - Are requirements defined for reading INPUT_RECORD from native memory? [Coverage, Spec §FR-009] ✓ FR-016 ReadConsoleInput + Edge Cases
- [x] CHK046 - Are requirements defined for writing CHAR_INFO arrays to console buffer? [Coverage, Spec §FR-018] ✓ FR-016 WriteConsoleOutput specified
- [x] CHK047 - Are requirements defined for all 12 P/Invoke functions listed in FR-016? [Coverage, Spec §FR-016] ✓ All 12 methods with signatures
- [x] CHK048 - Are SecurityAttributes.Create() factory method requirements specified? [Coverage, Spec §FR-014] ✓ FR-014 specifies Create() factory
- [x] CHK049 - Are requirements for combining multiple ConsoleInputMode flags specified? [Coverage, Spec §FR-013] ✓ Usage example added to FR-013

---

## Edge Case Coverage

- [x] CHK050 - Are requirements defined for handling unknown EventType values from future Windows versions? [Edge Case, Spec Edge Cases] ✓ Edge Cases section with handling guidance
- [x] CHK051 - Are requirements defined for struct behavior when marshalled on non-Windows platforms? [Edge Case, Spec Edge Cases] ✓ Cross-Platform Marshalling edge case added
- [x] CHK052 - Are requirements defined for reading mismatched union fields (e.g., MouseEvent when EventType is KeyEvent)? [Edge Case, Spec Edge Cases] ✓ Mismatched Union Field Access edge case
- [x] CHK053 - Is the UnicodeChar/AsciiChar union simplification documented with rationale? [Edge Case, Spec Assumptions] ✓ UnicodeChar Simplification edge case
- [x] CHK054 - Are requirements defined for GetStdHandle with invalid handle values? [Edge Case, Gap] ✓ Invalid Handle Values edge case added
- [x] CHK055 - Are requirements defined for P/Invoke calls when console is not attached? [Edge Case, Gap] ✓ Console Not Attached edge case added
- [x] CHK056 - Are requirements defined for INPUT_RECORD array bounds and buffer sizing? [Edge Case, Gap] ✓ INPUT_RECORD Array Sizing edge case
- [x] CHK057 - Are requirements defined for SMALL_RECT with negative or zero dimensions? [Edge Case, Gap] ✓ SMALL_RECT Edge Values edge case
- [x] CHK058 - Are requirements defined for COORD with negative coordinate values? [Edge Case, Gap] ✓ COORD Negative Values edge case

---

## Non-Functional Requirements

- [x] CHK059 - Are thread safety requirements specified for struct types? [NFR, Gap] ✓ NFR-004 specifies inherent thread safety
- [x] CHK060 - Are memory allocation requirements specified (stack vs heap for structs)? [NFR, Gap] ✓ NFR-009 specifies stack vs heap allocation
- [x] CHK061 - Are performance requirements for P/Invoke marshalling specified? [NFR, Gap] ✓ NFR-010 specifies blittable types for zero-copy
- [x] CHK062 - Is the XML documentation requirement specified for all public types? [NFR, Gap] ✓ NFR-007 specifies XML doc requirements
- [x] CHK063 - Are namespace placement requirements specified (Stroke.Input.Windows.Win32Types)? [NFR, Gap] ✓ NFR-005 specifies namespace structure
- [x] CHK064 - Are file organization requirements specified (one type per file, <1000 LOC)? [NFR, Gap] ✓ NFR-006 specifies file organization

---

## Dependencies & Assumptions

- [x] CHK065 - Is the dependency on existing ConsoleApi.cs documented? [Dependency, Gap] ✓ Dependencies section documents ConsoleApi.cs
- [x] CHK066 - Is the relationship to existing Win32Input.cs documented? [Dependency, Gap] ✓ Dependencies section documents Win32Input.cs
- [x] CHK067 - Is the assumption that P/Invoke declarations are internal validated? [Assumption, Spec Assumptions] ✓ Documented in Assumptions
- [x] CHK068 - Is the assumption about collapsing UNICODE_OR_ASCII to char validated against Python PTK behavior? [Assumption, Spec Assumptions] ✓ Documented in Assumptions + Edge Cases
- [x] CHK069 - Is the rationale for including CHAR_INFO (not in Python PTK) documented? [Assumption, Spec Assumptions] ✓ Documented in Assumptions
- [x] CHK070 - Is the rationale for including ConsoleInputMode/ConsoleOutputMode (not in Python PTK) documented? [Assumption, Spec Assumptions] ✓ Documented in Assumptions
- [x] CHK071 - Are the source Python Prompt Toolkit files (win32_types.py) referenced for traceability? [Traceability, Gap] ✓ External References in Dependencies

---

## API Fidelity (Python Prompt Toolkit)

- [x] CHK072 - Are all 9 struct types from win32_types.py mapped to C# equivalents? [Fidelity, Spec §SC-006] ✓ SC-006 with source file reference
- [x] CHK073 - Is MENU_EVENT_RECORD explicitly included despite being "reserved by Windows"? [Fidelity, Spec §FR-007] ✓ FR-007 with "reserved" note
- [x] CHK074 - Is FOCUS_EVENT_RECORD explicitly included despite being "reserved by Windows"? [Fidelity, Spec §FR-008] ✓ FR-008 with "reserved" note
- [x] CHK075 - Are deviations from Python PTK (CHAR_INFO, ConsoleInputMode, ConsoleOutputMode additions) documented with rationale? [Fidelity, Spec Assumptions] ✓ Documented in Assumptions
- [x] CHK076 - Is the mapping between Python field names (snake_case) and C# field names (PascalCase) documented? [Fidelity, Gap] ✓ NFR-001 with full mapping table

---

## P/Invoke Correctness

- [x] CHK077 - Is LibraryImport (source generator) vs DllImport (legacy) approach specified? [Correctness, Gap] ✓ FR-016 specifies LibraryImport
- [x] CHK078 - Are [In] and [Out] attributes specified for array parameters? [Correctness, Gap] ✓ FR-016 specifies [In]/[Out] usage
- [x] CHK079 - Is [MarshalAs(UnmanagedType.Bool)] specified for BOOL return types? [Correctness, Gap] ✓ FR-016 specifies MarshalAs for BOOL
- [x] CHK080 - Are ref/out parameter modifiers specified for output structs? [Correctness, Gap] ✓ FR-016 specifies ref/out modifiers
- [x] CHK081 - Is the kernel32.dll library name constant specified? [Correctness, Gap] ✓ FR-016 specifies "kernel32.dll" constant
- [x] CHK082 - Are partial class/method modifiers specified for LibraryImport usage? [Correctness, Gap] ✓ FR-016 specifies partial class requirement

---

## Cross-Platform Safety

- [x] CHK083 - Is compilation behavior on non-Windows platforms specified (types compile, P/Invoke not callable)? [Cross-Platform, Spec §SC-005] ✓ SC-005 and NFR-008
- [x] CHK084 - Is OperatingSystem.IsWindows() guard pattern documented for runtime checks? [Cross-Platform, Gap] ✓ NFR-008 with code example
- [x] CHK085 - Are CA1416 (platform compatibility) analyzer warnings addressed in requirements? [Cross-Platform, Gap] ✓ SC-008 specifies CA1416 behavior
- [x] CHK086 - Is the distinction between compile-time (types) and runtime (P/Invoke) platform requirements clear? [Cross-Platform, Spec §SC-005] ✓ NFR-008 and Edge Cases

---

**Total Items**: 86
**Completed**: 86/86 (100%)
**Traceability Coverage**: 86/86 items (100%) have spec references
