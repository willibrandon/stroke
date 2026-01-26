# Implementation Plan: Input System

**Branch**: `014-input-system` | **Date**: 2026-01-25 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/014-input-system/spec.md`

## Summary

Implement the input abstraction layer for reading keyboard and mouse input from terminals with VT100 parsing, raw/cooked modes, and platform-specific backends. The system provides a unified `IInput` interface with implementations for POSIX (`Vt100Input`), Windows (`Win32Input`), testing (`PipeInput`), and fallback (`DummyInput`) scenarios.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.Input layer - zero external dependencies per Constitution III)
**Storage**: N/A (in-memory state only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Windows 10+, macOS, Linux
**Project Type**: Single library with namespace organization
**Performance Goals**: Raw mode entry/exit <10ms, escape key detection within 100ms timeout
**Constraints**: Single-threaded reader access (thread safety at distribution layer)
**Scale/Scope**: ~15 types, ~150 methods/properties

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | All APIs mapped from Python Prompt Toolkit `input/` module |
| II. Immutability by Default | ✅ PASS | `KeyPress` is immutable record struct; parser state is internal |
| III. Layered Architecture | ✅ PASS | Stroke.Input depends only on Stroke.Core (Keys enum already exists) |
| IV. Cross-Platform | ✅ PASS | Vt100Input (POSIX), Win32Input (Windows), platform factory |
| V. Complete Editing Mode Parity | N/A | Input layer is below editing modes |
| VI. Performance-Conscious | ✅ PASS | Lazy parsing, cached sequence lookups, buffer reuse |
| VII. Full Scope | ✅ PASS | All 8 user stories and 20 functional requirements covered |
| VIII. Real-World Testing | ✅ PASS | PipeInput enables real integration tests; no mocks |
| IX. Planning Documents | ✅ PASS | Follows api-mapping.md Section "prompt_toolkit.input" |
| X. File Size Limits | ✅ PASS | Design splits into multiple focused files |
| XI. Thread Safety | ✅ PASS | ReadKeys/FlushKeys single-threaded; clarified in spec |

## Project Structure

### Documentation (this feature)

```text
specs/014-input-system/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (interface definitions)
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/
├── Input/
│   ├── IInput.cs                    # Abstract input interface
│   ├── InputBase.cs                 # Optional shared base implementation
│   ├── DummyInput.cs                # No-op input for non-terminal scenarios
│   ├── KeyPress.cs                  # Key press event data (record struct)
│   ├── Keys.cs                      # (EXISTING) Key enum with 151 values
│   ├── KeysExtensions.cs            # (EXISTING) Extension methods
│   ├── AllKeys.cs                   # (EXISTING) ALL_KEYS constant
│   ├── KeyAliases.cs                # (EXISTING) Key aliases
│   ├── KeyAliasMap.cs               # (EXISTING) Alias mapping
│   ├── MouseEvent.cs                # (EXISTING) Mouse event record
│   ├── MouseEventType.cs            # (EXISTING) Mouse event types
│   ├── MouseButton.cs               # (EXISTING) Mouse button enum
│   ├── MouseModifiers.cs            # (EXISTING) Mouse modifier flags
│   ├── Vt100/
│   │   ├── Vt100Parser.cs           # VT100 escape sequence parser
│   │   ├── Vt100Input.cs            # POSIX VT100 input implementation
│   │   ├── AnsiSequences.cs         # ANSI escape sequence mappings
│   │   ├── RawModeContext.cs        # Raw terminal mode context manager
│   │   └── CookedModeContext.cs     # Cooked terminal mode context manager
│   ├── Posix/
│   │   ├── PosixStdinReader.cs      # Non-blocking POSIX stdin reader
│   │   ├── PosixPipeInput.cs        # POSIX pipe input for testing
│   │   └── Termios.cs               # P/Invoke for termios APIs
│   ├── Windows/
│   │   ├── Win32Input.cs            # Windows console input
│   │   ├── ConsoleInputReader.cs    # Legacy Windows input reader
│   │   ├── Vt100ConsoleInputReader.cs # Win10+ VT100 input reader
│   │   ├── Win32PipeInput.cs        # Windows pipe input for testing
│   │   ├── Win32RawMode.cs          # Windows raw mode context
│   │   └── ConsoleApi.cs            # P/Invoke for Console APIs
│   ├── Pipe/
│   │   ├── IPipeInput.cs            # Pipe input interface
│   │   └── PipeInputBase.cs         # Shared pipe input logic
│   ├── Typeahead/
│   │   └── TypeaheadBuffer.cs       # Typeahead key storage
│   └── InputFactory.cs              # Platform-appropriate input factory
│
tests/Stroke.Tests/
├── Input/
│   ├── KeyPressTests.cs
│   ├── DummyInputTests.cs
│   ├── Vt100ParserTests.cs
│   ├── AnsiSequencesTests.cs
│   ├── Vt100InputTests.cs
│   ├── RawModeContextTests.cs
│   ├── PosixStdinReaderTests.cs
│   ├── PosixPipeInputTests.cs
│   ├── Win32InputTests.cs
│   ├── Win32PipeInputTests.cs
│   ├── InputFactoryTests.cs
│   └── TypeaheadBufferTests.cs
```

**Structure Decision**: Single project `Stroke` with organized namespaces under `Stroke.Input`. Platform-specific implementations separated into `Posix/` and `Windows/` subdirectories. Core VT100 parsing in `Vt100/` subdirectory shared between POSIX and Windows VT100 mode.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Platform-specific P/Invoke | Required for terminal mode control (termios on POSIX, Console APIs on Windows) | Managed APIs don't expose raw terminal control |
| Conditional compilation | Windows vs POSIX implementations are mutually exclusive | Runtime detection insufficient for compile-time API availability |
