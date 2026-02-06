# Implementation Plan: Prompt Examples (Complete Set)

**Branch**: `065-prompt-examples` | **Date**: 2026-02-06 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/065-prompt-examples/spec.md`

## Summary

Implement all 56 Python Prompt Toolkit prompt examples as C# classes in the existing `Stroke.Examples.Prompts` project. Each example is a faithful port using the Stroke public API only. Four examples already exist (GetInput, AutoSuggestion, Autocompletion, FuzzyWordCompleter); the remaining 52 are new. Implementation follows a bottom-up approach: basic prompts first, then styling, key bindings, completions, history, validation, advanced features, and frames. Program.cs routing is updated to 58 entries (56 primary + 2 backward-compatibility aliases).

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke library (`../../src/Stroke/Stroke.csproj`) — all APIs already implemented
**Storage**: N/A (examples only; PersistentHistory uses temp file)
**Testing**: TUI Driver MCP tools for representative example verification; no xUnit tests (FR: examples verified via TUI Driver or manual testing)
**Target Platform**: Cross-platform (Linux, macOS, Windows 10+)
**Project Type**: Single console application project (existing)
**Performance Goals**: Each example launches and displays initial prompt within 5 seconds (SC-002)
**Constraints**: ≤ 200 LOC per example file (FR-020); public API only (FR-008)
**Scale/Scope**: 52 new example files + 3 new subdirectory files + Program.cs update = 56 files total

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Faithful Port** | PASS | Each example faithfully ports its Python equivalent per `docs/examples-mapping.md` |
| **II. Immutability** | N/A | Examples consume existing APIs; no new mutable data structures |
| **III. Layered Architecture** | PASS | Examples only depend on `Stroke.Shortcuts` (highest layer); no circular deps |
| **IV. Cross-Platform** | PASS | Examples use Stroke's cross-platform abstractions |
| **V. Editing Mode Parity** | PASS | Vi and Emacs examples (#3, #24, #26) exercise both modes |
| **VI. Performance** | PASS | No new rendering code; examples use existing differential renderer |
| **VII. Full Scope** | PASS | All 56 examples implemented — no scope reduction |
| **VIII. Real-World Testing** | PASS | TUI Driver verification for representative examples; no mocks |
| **IX. Planning Documents** | PASS | Examples follow `docs/examples-mapping.md` exactly — names, structure, order |
| **X. File Size** | PASS | FR-020 limits examples to 200 LOC; well under 1,000 LOC limit |
| **XI. Thread Safety** | N/A | Examples consume thread-safe APIs; no new mutable classes |
| **XII. Contracts in Markdown** | PASS | Contracts defined in `contracts/example-contract.md` and `contracts/routing-manifest.md` |

**Post-Design Re-check**: All principles still PASS. No violations detected.

## Project Structure

### Documentation (this feature)

```text
specs/065-prompt-examples/
├── spec.md                          # Feature specification
├── plan.md                          # This file
├── research.md                      # Phase 0 research output
├── data-model.md                    # Phase 1 entity model
├── quickstart.md                    # Phase 1 build guide
├── contracts/
│   ├── example-contract.md          # Class/routing contracts
│   └── routing-manifest.md          # Authoritative 56-entry routing table
├── checklists/
│   └── requirements.md              # Spec quality checklist
└── tasks.md                         # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
examples/Stroke.Examples.Prompts/
├── Program.cs                       # Routing (58 entries: 56 + 2 aliases)
├── Stroke.Examples.Prompts.csproj   # Project config (existing, unchanged)
│
│   # Basic Prompts (13 files — 1 existing, 12 new)
├── GetInput.cs                      # EXISTING (#1)
├── GetInputWithDefault.cs           # NEW (#2)
├── GetInputViMode.cs                # NEW (#3)
├── GetPassword.cs                   # NEW (#4)
├── GetMultilineInput.cs             # NEW (#5)
├── AcceptDefault.cs                 # NEW (#6)
├── ConfirmationPrompt.cs            # NEW (#7)
├── PlaceholderText.cs               # NEW (#8)
├── MouseSupport.cs                  # NEW (#9)
├── NoWrapping.cs                    # NEW (#10)
├── MultilinePrompt.cs               # NEW (#11)
├── OperateAndGetNext.cs             # NEW (#12)
├── EnforceTtyInputOutput.cs         # NEW (#13)
│
│   # Password & Security (1 new)
├── GetPasswordWithToggle.cs         # NEW (#14)
│
│   # Styling & Formatting (8 new)
├── ColoredPrompt.cs                 # NEW (#15)
├── BottomToolbar.cs                 # NEW (#16)
├── RightPrompt.cs                   # NEW (#17)
├── ClockInput.cs                    # NEW (#18)
├── FancyZshPrompt.cs                # NEW (#19)
├── TerminalTitle.cs                 # NEW (#20)
├── SwapLightDarkColors.cs           # NEW (#21)
├── CursorShapes.cs                  # NEW (#22)
│
│   # Key Bindings & Input Handling (5 new)
├── CustomKeyBinding.cs              # NEW (#23)
├── CustomViOperator.cs              # NEW (#24)
├── SystemPrompt.cs                  # NEW (#25)
├── SwitchViEmacs.cs                 # NEW (#26)
├── Autocorrection.cs                # NEW (#27)
│
│   # Auto-Suggestion & History (1 existing, 2 new)
├── AutoSuggestion.cs                # EXISTING (#28)
├── MultilineAutosuggest.cs          # NEW (#29)
├── UpArrowPartialMatch.cs           # NEW (#30)
│
│   # Validation & Lexing (4 new)
├── InputValidation.cs               # NEW (#45)
├── RegularLanguage.cs               # NEW (#46)
├── HtmlInput.cs                     # NEW (#47)
├── CustomLexer.cs                   # NEW (#48)
│
│   # Advanced Features (5 new)
├── AsyncPrompt.cs                   # NEW (#49)
├── PatchStdout.cs                   # NEW (#50)
├── InputHook.cs                     # NEW (#51)
├── ShellIntegration.cs              # NEW (#52)
├── SystemClipboard.cs               # NEW (#53)
│
│   # Auto-Completion subdirectory (2 existing, 10 new)
├── AutoCompletion/
│   ├── Autocompletion.cs            # EXISTING (#31)
│   ├── ControlSpaceTrigger.cs       # NEW (#32)
│   ├── ReadlineStyle.cs             # NEW (#33)
│   ├── ColoredCompletions.cs        # NEW (#34)
│   ├── FormattedCompletions.cs      # NEW (#35)
│   ├── MergedCompleters.cs          # NEW (#36)
│   ├── FuzzyWordCompleter.cs        # EXISTING (#37)
│   ├── FuzzyCustomCompleter.cs      # NEW (#38)
│   ├── MultiColumn.cs              # NEW (#39)
│   ├── MultiColumnWithMeta.cs       # NEW (#40)
│   ├── NestedCompletion.cs          # NEW (#41)
│   └── SlowCompletions.cs           # NEW (#42)
│
│   # History subdirectory (2 new)
├── History/
│   ├── PersistentHistory.cs         # NEW (#43)
│   └── SlowHistory.cs              # NEW (#44)
│
│   # WithFrames subdirectory (3 new)
└── WithFrames/
    ├── BasicFrame.cs                # NEW (#54)
    ├── GrayFrameOnAccept.cs         # NEW (#55)
    └── FrameWithCompletion.cs       # NEW (#56)
```

**Structure Decision**: Extends the existing `Stroke.Examples.Prompts` project. No new projects needed. Creates 3 new subdirectories (`History/`, `WithFrames/`; `AutoCompletion/` already exists). The `.csproj` requires no changes — it already references the Stroke library.

## Complexity Tracking

> No Constitution violations detected. No complexity justifications required.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |
