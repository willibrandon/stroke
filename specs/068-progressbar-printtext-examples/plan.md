# Implementation Plan: Progress Bar and Print Text Examples

**Branch**: `068-progressbar-printtext-examples` | **Date**: 2026-02-07 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/068-progressbar-printtext-examples/spec.md`

## Summary

Implement all 24 remaining non-tutorial Python Prompt Toolkit examples across two new projects: **Stroke.Examples.PrintText** (9 synchronous examples) and **Stroke.Examples.ProgressBar** (15 async examples). PrintText examples use existing infrastructure (FormattedTextOutput, Html, Ansi, Style, ColorDepth, Widgets) and can be built immediately. ProgressBar examples depend on Feature 71 (ProgressBar API) and will compile against its public API surface. Both projects follow the established dictionary-based routing pattern from Stroke.Examples.Prompts/FullScreen/Dialogs.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: Stroke library (all subsystems already implemented: FormattedTextOutput, Html, Ansi, Style, ColorDepth, Frame, TextArea, KeyBindings, PatchStdout); Feature 71 ProgressBar API (not yet implemented — blocks runtime testing of 15 progress bar examples)
**Storage**: N/A (in-memory only; examples are transient console applications)
**Testing**: Visual verification via TUI Driver (tui_launch, tui_text, tui_screenshot); no xUnit tests for example projects (examples are the tests)
**Target Platform**: macOS, Linux, Windows 10+ (cross-platform .NET 10)
**Project Type**: Two executable console projects within existing solution
**Performance Goals**: N/A (examples are demonstrations, not performance-critical)
**Constraints**: Each example file must be <1,000 LOC; each example must be a faithful port matching Python Prompt Toolkit visual output
**Scale/Scope**: 24 example files + 2 Program.cs files + 2 .csproj files + solution file update = ~30 files total

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | PASS | All 24 examples are 1:1 ports of Python Prompt Toolkit examples per docs/examples-mapping.md |
| II. Immutability | N/A | Example code does not define core data structures |
| III. Layered Architecture | PASS | Examples depend on Stroke.Shortcuts (top layer) — no circular deps |
| IV. Cross-Platform | PASS | Both projects target net10.0; no platform-specific code in examples |
| V. Editing Mode Parity | N/A | Examples do not define editing modes |
| VI. Performance-Conscious | N/A | Examples are transient demos, not performance-critical code |
| VII. Full Scope | PASS | All 24 examples will be implemented; none deferred or skipped |
| VIII. Real-World Testing | PASS | Examples exercise real implementations; no mocks |
| IX. Planning Documents | PASS | docs/examples-mapping.md consulted — all 24 examples match mapping rows 82-96 (ProgressBar) and 106-114 (PrintText) |
| X. File Size Limits | PASS | All Python originals are 16-102 lines; C# ports will be well under 1,000 LOC |
| XI. Thread Safety | N/A | Example classes are stateless static classes with Run() methods |
| XII. Contracts in Markdown | PASS | All contracts in this plan are markdown, not .cs files |

**Gate result: PASS** — No violations. Proceeding to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/068-progressbar-printtext-examples/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 research findings
├── data-model.md        # Phase 1 entity model
├── quickstart.md        # Phase 1 developer quickstart
├── contracts/           # Phase 1 API contracts (markdown)
│   ├── print-text-examples.md
│   └── progress-bar-examples.md
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Phase 2 task breakdown (created by /speckit.tasks)
```

### Source Code (repository root)

```text
examples/
├── Stroke.Examples.sln                          # Updated: add 2 new projects
├── Stroke.Examples.PrintText/
│   ├── Stroke.Examples.PrintText.csproj         # New: executable, refs Stroke.csproj
│   ├── Program.cs                               # New: Dictionary<string, Action> routing
│   ├── AnsiColors.cs                            # FR-006: 16 FG + 16 BG ANSI colors
│   ├── Ansi.cs                                  # FR-007: bold/italic/underline/strike/256-color
│   ├── Html.cs                                  # FR-008: <b>/<i>/<ansired>/<style> tags
│   ├── NamedColors.cs                           # FR-009: all named colors at 3 depths
│   ├── PrintFormattedText.cs                    # FR-010: 4 formatting methods
│   ├── PrintFrame.cs                            # FR-011: Frame + TextArea via PrintContainer
│   ├── TrueColorDemo.cs                         # FR-012: 7 RGB gradients × 3 depths
│   ├── PygmentsTokens.cs                        # FR-013: syntax-highlighted tokens
│   └── LogoAnsiArt.cs                           # FR-014: 24-bit true color ANSI art logo
├── Stroke.Examples.ProgressBar/
│   ├── Stroke.Examples.ProgressBar.csproj       # New: executable, refs Stroke.csproj
│   ├── Program.cs                               # New: Dictionary<string, Func<Task>> routing
│   ├── SimpleProgressBar.cs                     # FR-021: 800 items, basic bar
│   ├── TwoTasks.cs                              # FR-022: 2 parallel threaded tasks
│   ├── UnknownLength.cs                         # FR-023: no known total
│   ├── NestedProgressBars.cs                    # FR-024: outer + inner bars, removeWhenDone
│   ├── ColoredTitleLabel.cs                     # FR-025: HTML-colored title and label
│   ├── ScrollingTaskName.cs                     # FR-026: long label, horizontal scroll
│   ├── Styled1.cs                               # FR-027: custom Style with 9 keys
│   ├── Styled2.cs                               # FR-028: custom formatters
│   ├── StyledAptGet.cs                          # FR-029: apt-get install format
│   ├── StyledRainbow.cs                         # FR-030: Rainbow formatter + color depth prompt
│   ├── StyledTqdm1.cs                           # FR-031: tqdm format with iters/sec
│   ├── StyledTqdm2.cs                           # FR-032: tqdm reverse-video bar
│   ├── CustomKeyBindings.cs                     # FR-033: f/q/x keys + PatchStdout
│   ├── ManyParallelTasks.cs                     # FR-034: 8 tasks, HTML title + toolbar
│   └── LotOfParallelTasks.cs                    # FR-035: 160 tasks, random durations
```

**Structure Decision**: Two new executable console projects following the established pattern from Stroke.Examples.Prompts. PrintText uses `Dictionary<string, Action>` (synchronous `void Run()`). ProgressBar uses `Dictionary<string, Func<Task>>` (async `Task Run()`). Both reference `../../src/Stroke/Stroke.csproj`.

## Complexity Tracking

> No constitution violations detected. Table intentionally empty.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| — | — | — |

## Post-Design Constitution Re-Check

*Re-evaluated after Phase 1 design completion.*

| Principle | Status | Post-Design Notes |
|-----------|--------|-------------------|
| I. Faithful Port | PASS | Contracts verified against all 24 Python source files. CLI routing names match Python filenames. Class names follow PascalCase convention. |
| II. Immutability | N/A | No new data structures introduced |
| III. Layered Architecture | PASS | Both projects depend only on Stroke (top-level library package). No new library code introduced. |
| IV. Cross-Platform | PASS | No platform-specific code. Signal handling (CustomKeyBindings 'x' key) uses cross-platform KeyboardInterruptException instead of POSIX signals. |
| V. Editing Mode Parity | N/A | No editing mode changes |
| VI. Performance-Conscious | N/A | Example code only |
| VII. Full Scope | PASS | All 24 examples have contracts. 9 PrintText + 15 ProgressBar = 24 total. None deferred. |
| VIII. Real-World Testing | PASS | Examples are runnable programs exercising real Stroke APIs. TUI Driver verification planned. |
| IX. Planning Documents | PASS | All CLI names and class names verified against docs/examples-mapping.md rows 82-96 and 106-114 |
| X. File Size Limits | PASS | Largest Python original is 102 lines (ansi-colors.py). All C# ports will be well under 1,000 LOC. |
| XI. Thread Safety | N/A | Example classes are stateless. Parallel examples use Thread with IsBackground (no shared mutable state in example code). |
| XII. Contracts in Markdown | PASS | Both contract files are .md. Zero .cs files created during planning. |

**Post-design gate result: PASS** — No new violations introduced by the design.
