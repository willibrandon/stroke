# Implementation Plan: Telnet Server

**Branch**: `060-telnet-server` | **Date**: 2026-02-03 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/060-telnet-server/spec.md`

## Summary

Implement a Telnet server that enables running Stroke prompt toolkit applications over the Telnet protocol. This is a faithful port of Python Prompt Toolkit's `prompt_toolkit.contrib.telnet` module, providing network-accessible REPLs, command-line interfaces, and interactive shells with full terminal emulation.

The implementation consists of four core classes:
- **TelnetServer**: TCP listener managing concurrent connections
- **TelnetConnection**: Per-client session with isolated input/output
- **TelnetProtocolParser**: State machine for telnet protocol parsing
- **ConnectionStdout**: TextWriter wrapper for NVT-compliant output

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: `Stroke.Application` (AppSession, AppContext), `Stroke.Output` (IOutput, Vt100Output), `Stroke.Input` (IInput), `Stroke.Styles` (BaseStyle), `Stroke.FormattedText`, `System.Net.Sockets`
**Storage**: N/A (in-memory only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Cross-platform (Linux, macOS, Windows 10+)
**Project Type**: Single project (Stroke library extension in `Stroke.Contrib.Telnet` namespace)
**Performance Goals**: 50+ concurrent connections (SC-001), <500ms negotiation (SC-002), <50ms input latency (SC-003)
**Constraints**: Connection cleanup <1s (SC-004), server startup <100ms (SC-005), 80% test coverage (SC-008)
**Scale/Scope**: Single namespace with 4 classes, ~600-800 LOC implementation

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Faithful Port (100% API Fidelity)** | ✅ PASS | Port of `prompt_toolkit.contrib.telnet` - Python source at `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/telnet/` will be followed exactly |
| **II. Immutability by Default** | ✅ PASS | TelnetProtocolParser uses immutable protocol constants; mutable state limited to connection tracking |
| **III. Layered Architecture** | ✅ PASS | `Stroke.Contrib.Telnet` depends only on lower layers (Application, Output, Input, Styles, Core) |
| **IV. Cross-Platform Terminal Compatibility** | ✅ PASS | Uses existing Vt100Output; socket layer is cross-platform via .NET BCL |
| **V. Complete Editing Mode Parity** | ✅ N/A | This feature doesn't add editing modes; uses existing bindings |
| **VI. Performance-Conscious Design** | ✅ PASS | Socket reads are non-blocking; parser uses efficient state machine |
| **VII. Full Scope Commitment** | ✅ PASS | All 20 functional requirements will be implemented |
| **VIII. Real-World Testing** | ✅ PASS | Tests will use actual TCP sockets, real protocol negotiation |
| **IX. Adherence to Planning Documents** | ✅ PASS | Namespace `Stroke.Contrib.Telnet` matches api-mapping.md line 29 |
| **X. Source Code File Size Limits** | ✅ PASS | Implementation split across 4 files, each <500 LOC |
| **XI. Thread Safety by Default** | ✅ PASS | TelnetServer.Connections uses thread-safe set; connection methods are synchronized |

**Gate Status**: ✅ PASS - All principles satisfied or N/A. Proceeding to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/060-telnet-server/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (C# interfaces in markdown)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/Contrib/Telnet/
├── TelnetServer.cs          # Main server class (FR-001, FR-008, FR-009, FR-010, FR-012)
├── TelnetConnection.cs      # Per-client connection (FR-007, FR-011, FR-013-015)
├── TelnetProtocolParser.cs  # Telnet protocol state machine (FR-003, FR-004, FR-005, FR-016)
├── ConnectionStdout.cs      # TextWriter for NVT output (FR-006)
└── TelnetConstants.cs       # Protocol byte constants (FR-002)

tests/Stroke.Tests/Contrib/Telnet/
├── TelnetServerTests.cs
├── TelnetConnectionTests.cs
├── TelnetProtocolParserTests.cs
├── TelnetConstantsTests.cs
└── ConnectionStdoutTests.cs

examples/Stroke.Examples.Telnet/
└── BasicRepl.cs             # Simple telnet REPL example
```

**Structure Decision**: Follows existing `Stroke.Contrib.*` pattern established by `RegularLanguages` and `Completers`. All telnet classes reside in `src/Stroke/Contrib/Telnet/` namespace matching api-mapping.md.

## Complexity Tracking

> **No violations to justify** - All complexity within Constitution bounds.

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| Threading model | async/await with per-connection tasks | Matches Python's asyncio pattern; idiomatic .NET |
| State machine parser | Generator-style coroutine → explicit state enum | C# lacks Python's `yield`-based coroutines; explicit state machine is cleaner |
| Socket management | Raw `Socket` with `NetworkStream` | No higher-level abstraction needed; direct control required for telnet protocol |

## Constitution Re-Check (Post Phase 1 Design)

*GATE: Re-evaluated after contracts and data model are complete.*

| Principle | Status | Post-Design Notes |
|-----------|--------|-------------------|
| **I. Faithful Port** | ✅ PASS | Contracts match Python API exactly. All public methods/properties mapped. |
| **II. Immutability** | ✅ PASS | `TelnetConstants` is static/immutable. Parser state is mutable but encapsulated. |
| **III. Layered Architecture** | ✅ PASS | No new layer dependencies introduced beyond initial design. |
| **IV. Cross-Platform** | ✅ PASS | Using BCL `Socket` class which is cross-platform. |
| **V. Editing Modes** | ✅ N/A | No editing mode changes. |
| **VI. Performance** | ✅ PASS | State machine parser is O(n) where n = bytes. No allocations in hot path. |
| **VII. Full Scope** | ✅ PASS | All 20 FRs have corresponding contract methods. |
| **VIII. Real-World Testing** | ✅ PASS | Test strategy uses actual TCP sockets. |
| **IX. Planning Documents** | ✅ PASS | Namespace matches api-mapping.md. |
| **X. File Size** | ✅ PASS | 5 files × ~150 LOC average = ~750 LOC total, well under limit. |
| **XI. Thread Safety** | ✅ PASS | Contracts document thread-safety guarantees. Lock patterns specified. |

**Final Gate Status**: ✅ PASS - Design complete. Ready for `/speckit.tasks`.

## Generated Artifacts

| Artifact | Path | Purpose |
|----------|------|---------|
| Research | `specs/060-telnet-server/research.md` | Technology decisions, Python analysis |
| Data Model | `specs/060-telnet-server/data-model.md` | Entities, relationships, state machines |
| Contract: TelnetServer | `specs/060-telnet-server/contracts/TelnetServer.md` | Main server API |
| Contract: TelnetConnection | `specs/060-telnet-server/contracts/TelnetConnection.md` | Connection API |
| Contract: TelnetProtocolParser | `specs/060-telnet-server/contracts/TelnetProtocolParser.md` | Parser API |
| Contract: ConnectionStdout | `specs/060-telnet-server/contracts/ConnectionStdout.md` | TextWriter wrapper |
| Contract: TelnetConstants | `specs/060-telnet-server/contracts/TelnetConstants.md` | Protocol constants |
| Quickstart | `specs/060-telnet-server/quickstart.md` | Usage examples and patterns |
