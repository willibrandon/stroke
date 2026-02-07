# Implementation Plan: System Clipboard

**Branch**: `066-system-clipboard` | **Date**: 2026-02-07 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/066-system-clipboard/spec.md`

## Summary

Port Python Prompt Toolkit's `PyperclipClipboard` class to Stroke as `SystemClipboard` — a cross-platform `IClipboard` implementation that synchronizes with the operating system clipboard. Platform-specific I/O is abstracted behind an internal `IClipboardProvider` interface with four concrete providers (Windows via Win32 P/Invoke, macOS via pbcopy/pbpaste, Linux via wl-copy/xclip/xsel, WSL via clip.exe/powershell.exe). A `ClipboardProviderDetector` selects the correct provider at construction time. Selection type preservation follows the Python original: cache last-written `ClipboardData`, return it when OS clipboard text matches, otherwise infer Lines/Characters from newline presence.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: .NET BCL only — `System.Diagnostics.Process` (subprocess providers), `System.Runtime.InteropServices` (Win32 P/Invoke), `System.Threading.Lock` (thread safety)
**Storage**: N/A (in-memory caching of last-written ClipboardData only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Windows 10+, macOS, Linux (Wayland, X11, WSL)
**Project Type**: Single project — adds files to existing `src/Stroke/Clipboard/` directory
**Performance Goals**: All clipboard operations complete within 5 seconds (FR-009); typical operations under 100ms
**Constraints**: Process-based providers MUST use `ArgumentList` (not `Arguments`) to prevent command injection (FR-012); thread-safe via `System.Threading.Lock` (FR-010)
**Scale/Scope**: 8 new source files (7 in `src/Stroke/Clipboard/`, 1 modified `PlatformUtils.cs`), 5 new test files, ~600-800 LOC source + ~400-500 LOC tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Faithful Port | **PASS** | Ports `PyperclipClipboard` from `prompt_toolkit.clipboard.pyperclip`. Python delegates to `pyperclip` library; Stroke inlines the platform detection and process execution (pyperclip has no .NET equivalent). This is a documented deviation: replacing a third-party library with inline platform providers. |
| II | Immutability by Default | **PASS** | `ClipboardData` is already immutable. `IClipboardProvider` implementations are stateless (no mutable fields). Only `SystemClipboard` has mutable state (`_lastData` cache), protected by Lock. |
| III | Layered Architecture | **PASS** | `SystemClipboard` lives in `Stroke.Clipboard` (same layer as existing clipboard types, within `Stroke.Core` project). Win32 P/Invoke is declared locally (nested class pattern matching `PlatformUtils.Vt100Detection`) to avoid depending on `Stroke.Input.Windows`. |
| IV | Cross-Platform Compatibility | **PASS** | Explicit support for Windows, macOS, Linux (Wayland, X11), and WSL per FR-002. |
| V | Editing Mode Parity | **N/A** | No editing mode changes. |
| VI | Performance-Conscious Design | **PASS** | No rendering changes. Subprocess providers use 5-second timeout (FR-009). |
| VII | Full Scope Commitment | **PASS** | All 23 functional requirements implemented. No deferrals. |
| VIII | Real-World Testing | **PASS** | Uses `StringClipboardProvider` — a real `IClipboardProvider` implementation backed by a `string` field. Not a mock. Platform integration tests are platform-gated with `[SupportedOSPlatform]`. |
| IX | Adherence to Planning Documents | **PASS** | `api-mapping.md` does not map `PyperclipClipboard` (it's optional/not exported in Python). New API follows existing mapping conventions. |
| X | File Size Limits | **PASS** | Each provider is a separate file (~50-100 LOC). Largest file (`SystemClipboard`) estimated ~150 LOC. |
| XI | Thread Safety | **PASS** | `SystemClipboard` uses `System.Threading.Lock` with `EnterScope()`. Provider implementations are stateless (thread-safe by design). |
| XII | Contracts in Markdown Only | **PASS** | All contracts in `contracts/` as `.md` files. |

**Deviation Documentation**:
- **Principle I**: Python's `PyperclipClipboard` delegates to the `pyperclip` third-party library. No .NET equivalent of `pyperclip` exists. Stroke inlines the platform detection and clipboard I/O logic that `pyperclip` provides, using the same platform mechanisms (pbcopy/pbpaste, xclip/xsel, wl-copy/wl-paste, Win32 API). The public API (`SystemClipboard` implementing `IClipboard`) matches `PyperclipClipboard`'s behavior exactly.
- **Principle XI**: Python's `PyperclipClipboard` is not thread-safe. Stroke adds `Lock` synchronization per Constitution XI requirements.

## Project Structure

### Documentation (this feature)

```text
specs/066-system-clipboard/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: research findings
├── data-model.md        # Phase 1: entity model
├── quickstart.md        # Phase 1: integration guide
├── contracts/           # Phase 1: API contracts
│   ├── system-clipboard.md
│   └── clipboard-provider.md
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/Clipboard/
├── IClipboard.cs                    # (existing) Interface
├── ClipboardData.cs                 # (existing) Immutable data
├── DummyClipboard.cs                # (existing) No-op implementation
├── InMemoryClipboard.cs             # (existing) Kill ring implementation
├── DynamicClipboard.cs              # (existing) Runtime delegation
├── SystemClipboard.cs               # (NEW) OS clipboard synchronization
├── IClipboardProvider.cs            # (NEW) Internal provider interface
├── ClipboardProviderDetector.cs     # (NEW) Platform detection + instantiation
├── WindowsClipboardProvider.cs      # (NEW) Win32 P/Invoke provider
├── MacOsClipboardProvider.cs        # (NEW) pbcopy/pbpaste provider
├── LinuxClipboardProvider.cs        # (NEW) wl-copy/xclip/xsel provider
├── WslClipboardProvider.cs          # (NEW) clip.exe/powershell.exe provider
└── StringClipboardProvider.cs       # (NEW) Test provider (real impl, not mock)

src/Stroke/Core/
└── PlatformUtils.cs                 # (MODIFIED) Add IsWsl property

tests/Stroke.Tests/Clipboard/
├── ClipboardDataTests.cs            # (existing)
├── DummyClipboardTests.cs           # (existing)
├── InMemoryClipboardTests.cs        # (existing)
├── DynamicClipboardTests.cs         # (existing)
├── SystemClipboardTests.cs          # (NEW) Core behavior tests
├── ClipboardProviderDetectorTests.cs # (NEW) Detection logic tests
├── MacOsClipboardProviderTests.cs   # (NEW) Platform-gated integration tests
├── ClipboardProviderNotAvailableExceptionTests.cs # (NEW) Exception tests
└── StringClipboardProviderTests.cs  # (NEW) Test provider tests
```

**Structure Decision**: Extends the existing `src/Stroke/Clipboard/` directory within the single `Stroke` project. All new files are siblings of the existing clipboard implementations. No new projects or directories needed beyond the existing structure.

## Complexity Tracking

> No violations — all gates pass. No complexity tracking needed.
