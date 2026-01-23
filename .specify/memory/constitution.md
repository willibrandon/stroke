<!--
SYNC IMPACT REPORT
==================
Version change: 1.2.1 → 1.3.0
Modified principles:
  - Principle I (Faithful Port): Strengthened from "API shape parity where idiomatic" to "100% API match"
Added sections: None
Removed sections: None
Templates requiring updates:
  - .specify/templates/plan-template.md ✅ (no changes required - already references Constitution Check)
  - .specify/templates/spec-template.md ✅ (no changes required)
  - .specify/templates/tasks-template.md ✅ (no changes required)
  - .specify/templates/checklist-template.md ✅ (no changes required)
Follow-up TODOs: None
-->

# Stroke Constitution

## Core Principles

### I. Faithful Port (100% API Fidelity)

Stroke MUST be a 100% faithful port of Python Prompt Toolkit. This is NON-NEGOTIABLE:

- **Every public class** in Python Prompt Toolkit MUST have an equivalent class in Stroke
- **Every public method** MUST be ported with matching semantics
- **Every public property** MUST be ported with matching semantics
- **Every public constant/enum** MUST be ported with matching values
- **API names** MUST match the original (adjusted only for C# naming conventions: `snake_case` → `PascalCase`)
- **Module/namespace structure** MUST mirror the original package hierarchy

The Python source at `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/` serves as the authoritative reference. Before implementing ANY feature, the developer MUST:
1. Read the corresponding Python source file(s)
2. Identify all public APIs in that module
3. Port each API faithfully without invention or embellishment

**Forbidden behaviors**:
- Inventing APIs that do not exist in Python Prompt Toolkit
- Omitting APIs that exist in Python Prompt Toolkit
- Renaming APIs beyond case convention adjustments
- Changing method signatures beyond type-system requirements
- Adding "improvements" or "enhancements" not present in the original

Deviations from the original are permitted ONLY when:
1. C# language constraints require adaptation (e.g., `async/await` patterns, generic type constraints)
2. Platform differences necessitate change (e.g., file system APIs)

All such deviations MUST be documented with explicit rationale.

### II. Immutability by Default

Core data structures MUST be immutable unless mutation is explicitly required for state management. The `Document` class MUST remain immutable with flyweight caching and lazy property computation. Mutable wrappers (e.g., `Buffer`) MUST encapsulate immutable cores. Use `sealed` on classes not designed for inheritance. Prefer `record` types for value-semantic data. Use `ImmutableArray<T>` for readonly collections exposed in APIs.

### III. Layered Architecture

The codebase MUST follow a strict bottom-to-top dependency hierarchy:
1. **Stroke.Core** (Document, Buffer, Primitives) - zero external dependencies
2. **Stroke.Rendering** (Screen, Renderer, Output) - depends only on Core
3. **Stroke.Input** (Keys, Parsing, Mouse) - depends only on Core
4. **Stroke.KeyBinding** - depends on Core, Input
5. **Stroke.Layout** - depends on Core, Rendering
6. **Stroke.Completion** - depends on Core
7. **Stroke.Application** - orchestration layer, may depend on all lower layers
8. **Stroke.Shortcuts** - high-level API, depends on Application

Circular dependencies are forbidden. Lower layers MUST NOT reference higher layers.

### IV. Cross-Platform Terminal Compatibility

Stroke MUST support Linux, macOS, and Windows 10+ with ANSI/VT100 as the primary output mode. A `WindowsConsoleOutput` fallback MUST exist for legacy Windows terminals. All implementations MUST handle:
- Wide characters (CJK) via UnicodeWidth calculations
- Mouse tracking (click, drag, scroll)
- Bracketed paste mode
- Alternate screen buffer
- True color (24-bit) with graceful degradation to 256/16 colors

### V. Complete Editing Mode Parity

Both Emacs and Vi editing modes MUST be fully implemented to match Python Prompt Toolkit functionality:
- **Vi**: Navigation, Insert, Replace, Visual, VisualLine, VisualBlock modes; operators (d, c, y); motions; repeat (.)
- **Emacs**: Kill ring, transpose, incremental search forward/backward
- Mode-conditional bindings MUST use the Filter system (composable boolean conditions)

### VI. Performance-Conscious Design

Rendering MUST use differential updates (compare previous/current screen, update only changed regions). Screen storage MUST be sparse (dictionary-based, storing only non-empty cells). Character interning MUST be used for common ASCII characters. Lazy evaluation MUST be used for derived Document properties. No global mutable state; multiple independent Application instances MUST coexist in the same process.

### VII. Full Scope Commitment (NON-NEGOTIABLE)

Claude MUST NOT reduce scope, defer, deprioritize, or skip any requirements and/or tasks. This principle is absolute and admits no exceptions:
- **No scope reduction**: All specified requirements MUST be implemented as defined
- **No deferral**: Tasks MUST NOT be postponed to "future work" or "later phases" unless explicitly approved by the user
- **No deprioritization**: All tasks retain their assigned priority; Claude MUST NOT unilaterally lower priority
- **No skipping**: Every task in a task list MUST be completed; none may be omitted

When blockers arise, Claude MUST surface them immediately and await user direction rather than autonomously reducing scope. Partial implementations are acceptable only when explicitly requested by the user.

### VIII. Real-World Testing

The project MUST target 80% test coverage. Tests MUST exercise real implementations only:
- **No mocks**: Mock objects are forbidden
- **No fakes**: Fake implementations are forbidden
- **No doubles**: Test doubles of any kind are forbidden
- **No simulations**: Simulated dependencies are forbidden

**Forbidden Libraries**:
- Moq MUST NOT be used under any circumstances
- FluentAssertions MUST NOT be used under any circumstances

Tests MUST use xUnit with standard assertions. Integration and unit tests MUST interact with actual implementations, leveraging the layered architecture to test components in isolation through their real interfaces.

## Technical Standards

**Framework**: .NET 10+
**Language**: C# 13
**Naming**: PascalCase for public members, async methods suffixed with `Async`
**Async**: Prefer `async/await` for I/O-bound operations; use `ValueTask` for hot paths
**Nullability**: Nullable reference types enabled; explicit null handling required
**Testing**: Unit tests MUST accompany new public APIs; use xUnit conventions; target 80% coverage
**Documentation**: Triple-slash XML doc comments (`///`) are required as follows:
- All public types (classes, structs, records, interfaces, enums, delegates) MUST have XML docs
- All public members (methods, properties, fields, events, constructors) MUST have XML docs
- Private members that require explanation SHOULD have XML docs for clarity

## Development Workflow

1. **Reference First**: Before implementing any feature, locate the equivalent in Python Prompt Toolkit source and read it completely
2. **API Inventory**: List all public APIs in the Python module before writing any C# code
3. **Faithful Implementation**: Port each API exactly as defined in Python Prompt Toolkit
4. **Design Review**: Complex subsystems require written design in `/docs/` before implementation
5. **Incremental Delivery**: Features MUST be deliverable in independently testable increments
6. **Constitution Check**: Implementation plans MUST verify compliance with all Core Principles before proceeding

## Governance

This Constitution supersedes all other development practices in the Stroke repository. Amendments require:
1. Written proposal documenting the change and rationale
2. Impact analysis on existing code and dependent artifacts
3. Version increment following semantic versioning (MAJOR for breaking changes, MINOR for additions, PATCH for clarifications)

All pull requests MUST verify compliance with Core Principles. Violations require explicit justification in the PR description and approval from the project maintainer. Complexity that violates principles MUST be tracked in the implementation plan's Complexity Tracking table.

Use `CLAUDE.md` for runtime development guidance and architectural reference.

**Version**: 1.3.0 | **Ratified**: 2026-01-23 | **Last Amended**: 2026-01-23
