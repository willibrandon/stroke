# Implementation Plan: Keys Enum

**Branch**: `011-keys-enum` | **Date**: 2026-01-25 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/011-keys-enum/spec.md`

## Summary

Implement the `Keys` enum that defines all 151 key press types for key bindings, ported 1:1 from Python Prompt Toolkit's `keys.py` module. This includes extension methods for string conversion, alias resolution, and an enumerable collection of all valid keys. The enum enables compile-time safety for key binding registration and replaces magic strings with strongly-typed values.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.Input layer - zero external dependencies per Constitution III)
**Storage**: N/A (enum values and static readonly data only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single project (Stroke library)
**Performance Goals**: O(1) enum-to-string and string-to-enum lookups via cached dictionaries
**Constraints**: Thread-safe (immutable enum + readonly static data)
**Scale/Scope**: 151 enum values, 8 aliases, ~500 LOC

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | Exact 1:1 mapping of 151 Keys values from Python Prompt Toolkit keys.py; all string values match exactly |
| II. Immutability by Default | ✅ PASS | Enum values are inherently immutable; static dictionaries are readonly |
| III. Layered Architecture | ✅ PASS | Keys belongs to Stroke.Input per api-mapping.md; depends only on Stroke.Core (zero external deps) |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | Key string representations use portable ANSI/VT100 conventions |
| V. Complete Editing Mode Parity | ✅ PASS | Keys enum is prerequisite for Emacs/Vi bindings |
| VI. Performance-Conscious Design | ✅ PASS | Dictionary-based lookups for O(1) string↔enum conversion |
| VII. Full Scope Commitment | ✅ PASS | All 151 keys + 8 aliases + ALL_KEYS collection implemented |
| VIII. Real-World Testing | ✅ PASS | Tests use xUnit without mocks; target 80% coverage |
| IX. Adherence to Planning Documents | ✅ PASS | Follows api-mapping.md namespace mapping (prompt_toolkit.keys → Stroke.Input) |
| X. Source Code File Size Limits | ✅ PASS | Keys.cs ~300 LOC, KeysExtensions.cs ~150 LOC, tests split by concern |
| XI. Thread Safety by Default | ✅ PASS | Enum + static readonly = inherently thread-safe |

## Project Structure

### Documentation (this feature)

```text
specs/011-keys-enum/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (N/A - no API contracts)
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/
├── Input/
│   ├── Keys.cs              # Keys enum (151 values)
│   ├── KeysExtensions.cs    # ToKeyString(), ParseKey() extension methods
│   ├── KeyAliases.cs        # Static class with Tab, Enter, Backspace, ControlSpace aliases
│   ├── KeyAliasMap.cs       # Static class with Aliases dictionary and GetCanonical()
│   └── AllKeys.cs           # Static class with Values collection
└── Stroke.csproj

tests/Stroke.Tests/
├── Input/
│   ├── KeysTests.cs              # Enum value tests
│   ├── KeysExtensionsTests.cs    # String conversion tests
│   ├── KeyAliasesTests.cs        # Alias constant tests
│   ├── KeyAliasMapTests.cs       # Alias resolution tests
│   └── AllKeysTests.cs           # Collection enumeration tests
└── Stroke.Tests.csproj
```

**Structure Decision**: Single project structure matches existing Stroke codebase. Input/ subdirectory created under src/Stroke/ for Stroke.Input namespace. Tests mirror source structure under tests/Stroke.Tests/Input/.

## Complexity Tracking

> No violations - all Constitution principles pass without justification needed.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| *None* | *N/A* | *N/A* |
