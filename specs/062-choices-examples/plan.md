# Implementation Plan: Choices Examples (Complete Set)

**Branch**: `062-choices-examples` | **Date**: 2026-02-04 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/062-choices-examples/spec.md`

## Summary

Implement all 8 Python Prompt Toolkit choices examples in the `Stroke.Examples.Choices` project, demonstrating `Dialogs.Choice<T>()` capabilities: basic selection, default values, custom styling, frames, toolbars, accept-state styling, scrollable lists, and mouse support. This is a straightforward port of example code following the established Stroke.Examples.Prompts pattern.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke library (Stroke.Shortcuts, Stroke.FormattedText, Stroke.Styles, Stroke.Filters, Stroke.Application)
**Storage**: N/A (examples only)
**Testing**: TUI Driver for end-to-end verification; no unit tests (examples project)
**Target Platform**: Cross-platform (Linux, macOS, Windows 10+)
**Project Type**: Single console application with multiple runnable examples
**Performance Goals**: Each example completes selection and exits within 30 seconds
**Constraints**: Must faithfully port Python Prompt Toolkit examples 1:1
**Scale/Scope**: 8 example files + Program.cs + .csproj = 10 files total

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Each example is a 1:1 port from Python Prompt Toolkit |
| II. Immutability by Default | ✅ N/A | Examples only - no new data structures |
| III. Layered Architecture | ✅ PASS | Examples use Stroke.Shortcuts (top layer) |
| IV. Cross-Platform Terminal | ✅ PASS | Uses cross-platform Dialogs.Choice<T>() API |
| V. Editing Mode Parity | ✅ N/A | Not applicable to choice selection |
| VI. Performance-Conscious | ✅ PASS | Selection UI already optimized in Stroke |
| VII. Full Scope Commitment | ✅ PASS | All 8 examples will be implemented |
| VIII. Real-World Testing | ✅ PASS | TUI Driver verification, no mocks |
| IX. Adherence to Planning Docs | ✅ PASS | Follows examples-mapping.md structure |
| X. File Size Limits | ✅ PASS | Each example is <50 LOC |
| XI. Thread Safety | ✅ N/A | Examples use existing thread-safe APIs |

**Gate Status**: ✅ PASSED - All applicable principles satisfied

## Project Structure

### Documentation (this feature)

```text
specs/062-choices-examples/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output (minimal - well-defined scope)
├── quickstart.md        # Phase 1 output
└── checklists/
    └── requirements.md  # Validation checklist
```

### Source Code (repository root)

```text
examples/
├── Stroke.Examples.sln                    # Existing solution (add new project)
├── Stroke.Examples.Prompts/               # Existing reference
└── Stroke.Examples.Choices/               # NEW: This feature
    ├── Stroke.Examples.Choices.csproj     # Project file
    ├── Program.cs                         # Entry point with routing
    ├── SimpleSelection.cs                 # Example 1: Basic selection
    ├── Default.cs                         # Example 2: Pre-selected default
    ├── Color.cs                           # Example 3: Custom styling
    ├── WithFrame.cs                       # Example 4: Frame border
    ├── FrameAndBottomToolbar.cs           # Example 5: Frame + toolbar
    ├── GrayFrameOnAccept.cs               # Example 6: Accept-state styling
    ├── ManyChoices.cs                     # Example 7: Scrollable 99 options
    └── MouseSupport.cs                    # Example 8: Mouse click selection
```

**Structure Decision**: Single console project following `Stroke.Examples.Prompts` pattern with dictionary-based command-line routing.

## Complexity Tracking

> No violations - all principles satisfied.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |

## File Inventory

### Files to Create

| File | Purpose | LOC Est. |
|------|---------|----------|
| `Stroke.Examples.Choices.csproj` | Project configuration | 15 |
| `Program.cs` | Entry point with example routing | 45 |
| `SimpleSelection.cs` | Basic Dialogs.Choice<T>() demo | 25 |
| `Default.cs` | Pre-selected default value demo | 30 |
| `Color.cs` | Custom Style.FromDict() demo | 45 |
| `WithFrame.cs` | showFrame: ~AppFilters.IsDone demo | 35 |
| `FrameAndBottomToolbar.cs` | bottomToolbar parameter demo | 45 |
| `GrayFrameOnAccept.cs` | "accepted" style prefix demo | 35 |
| `ManyChoices.cs` | Scrollable 99-option list demo | 25 |
| `MouseSupport.cs` | mouseSupport: true demo | 30 |

### Files to Modify

| File | Change | Reason |
|------|--------|--------|
| `examples/Stroke.Examples.sln` | Add Stroke.Examples.Choices project | Solution must include new project |

## API Corrections

The spec contained minor API naming discrepancies. Corrected mappings:

| Spec Used | Actual Stroke API | Notes |
|-----------|-------------------|-------|
| `Html.Parse("<u>...</u>:")` | `new Html("<u>...</u>:")` | Html uses constructor |
| `defaultValue:` | `defaultValue:` | ✅ Correct |
| `mouseSupport:` | `mouseSupport:` | ✅ Correct |
| `showFrame:` | `showFrame:` | ✅ Correct |
| `bottomToolbar:` | `bottomToolbar:` | ✅ Correct |
| `Style.FromDict()` | `Style.FromDict()` | ✅ Correct |
| `AppFilters.IsDone` | `AppFilters.IsDone` | ✅ Correct |

## Implementation Approach

### Phase 1: Project Setup

1. Create `Stroke.Examples.Choices.csproj` (copy from Prompts, update namespace)
2. Create `Program.cs` with dictionary-based routing
3. Add project reference to `Stroke.Examples.sln`
4. Verify empty project builds

### Phase 2: Core Examples (P1 Priority)

1. `SimpleSelection.cs` - Foundation example
2. `Default.cs` - Default value demonstration
3. Verify both examples run with TUI Driver

### Phase 3: Styling Examples (P2 Priority)

1. `Color.cs` - Style.FromDict() with HTML labels
2. `WithFrame.cs` - Filter-controlled frame visibility
3. `FrameAndBottomToolbar.cs` - Combined frame + toolbar
4. `GrayFrameOnAccept.cs` - Accept-state styling

### Phase 4: Advanced Examples

1. `ManyChoices.cs` - Scrollable list (99 items)
2. `MouseSupport.cs` - Mouse click selection

### Phase 5: Verification

1. Build entire solution: `dotnet build examples/Stroke.Examples.sln`
2. Run each example with TUI Driver
3. Verify Ctrl+C handling in each example

## Key Implementation Details

### Program.cs Pattern (from Prompts)

```csharp
private static readonly Dictionary<string, Action> Examples = new(StringComparer.OrdinalIgnoreCase)
{
    ["SimpleSelection"] = SimpleSelection.Run,
    ["Default"] = Default.Run,
    // ... all 8 examples
};
```

### Html Constructor (not Parse)

```csharp
// Correct usage:
message: new Html("<u>Please select a dish</u>:")

// NOT:
message: Html.Parse("<u>Please select a dish</u>:")  // Wrong!
```

### Filter Negation for showFrame

```csharp
// Show frame during editing, hide on accept:
showFrame: ~AppFilters.IsDone

// Always show frame:
showFrame: true  // or FilterOrBool with true
```

### Style Classes for Accept State

```csharp
// "accepted" prefix applies when IsDone is true:
["frame.border"] = "#ff4444",           // During editing
["accepted frame.border"] = "#888888",  // After Enter pressed
```

## Dependencies

All dependencies are already implemented in Stroke:

- ✅ `Dialogs.Choice<T>()` - Feature 048/056
- ✅ `Style.FromDict()` - Feature 018
- ✅ `Html` class - Feature 015
- ✅ `AppFilters.IsDone` - Feature 032
- ✅ `FilterOrBool` - Feature 017
- ✅ `RadioList<T>` widget - Feature 045

## Verification Strategy

Each example will be verified using TUI Driver:

1. Launch example with `tui_launch`
2. Wait for prompt text with `tui_wait_for_text`
3. Interact with `tui_press_key` / `tui_click_at`
4. Verify output with `tui_text`
5. Close session with `tui_close`

Example verification for Default:
```javascript
await tui_launch({ command: "dotnet", args: ["run", "--", "Default"] });
await tui_wait_for_text({ text: "Please select a dish" });
await tui_press_key({ key: "Enter" });
await tui_wait_for_text({ text: "salad" });
await tui_close();
```
