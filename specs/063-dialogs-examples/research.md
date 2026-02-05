# Research: Dialogs Examples

**Feature**: 063-dialogs-examples
**Date**: 2026-02-04

## Overview

This research documents the findings for implementing all 9 Python Prompt Toolkit dialog examples in Stroke.Examples.Dialogs.

## Python Source Analysis

### Dialog Examples Inventory

All 9 Python dialog examples have been analyzed from `/Users/brandon/src/python-prompt-toolkit/examples/dialogs/`:

| Python File | C# Equivalent | Stroke API Used | LOC (Python) |
|-------------|---------------|-----------------|--------------|
| `messagebox.py` | `MessageBox.cs` | `Dialogs.MessageDialog()` | 17 |
| `yes_no_dialog.py` | `YesNoDialog.cs` | `Dialogs.YesNoDialog()` | 18 |
| `button_dialog.py` | `ButtonDialog.cs` | `Dialogs.ButtonDialog<T>()` | 20 |
| `input_dialog.py` | `InputDialog.cs` | `Dialogs.InputDialog()` | 18 |
| `password_dialog.py` | `PasswordDialog.cs` | `Dialogs.InputDialog(password: true)` | 20 |
| `radio_dialog.py` | `RadioDialog.cs` | `Dialogs.RadioListDialog<T>()` | 40 |
| `checkbox_dialog.py` | `CheckboxDialog.cs` | `Dialogs.CheckboxListDialog<T>()` | 37 |
| `progress_dialog.py` | `ProgressDialog.cs` | `Dialogs.ProgressDialog()` | 47 |
| `styled_messagebox.py` | `StyledMessageBox.cs` | `Dialogs.MessageDialog()` + `Style.FromDict()` | 38 |

### API Availability Verification

All required Stroke APIs are already implemented in `Stroke.Shortcuts.Dialogs`:

- ✅ `Dialogs.MessageDialog()` — returns `Application<object?>`
- ✅ `Dialogs.YesNoDialog()` — returns `Application<bool>`
- ✅ `Dialogs.ButtonDialog<T>()` — returns `Application<T>`
- ✅ `Dialogs.InputDialog()` — returns `Application<string?>`, supports `password` parameter
- ✅ `Dialogs.RadioListDialog<T>()` — returns `Application<T?>`
- ✅ `Dialogs.CheckboxListDialog<T>()` — returns `Application<IReadOnlyList<T>?>`
- ✅ `Dialogs.ProgressDialog()` — returns `Application<object?>`

Supporting types verified:
- ✅ `Style.FromDict()` — in `Stroke.Styles`
- ✅ `Html` formatted text — in `Stroke.FormattedText`
- ✅ `AnyFormattedText` — implicit conversion from strings

## Design Decisions

### Decision 1: Project Structure

**Chosen**: Single project `Stroke.Examples.Dialogs` with dictionary-based routing in `Program.cs`

**Rationale**: Matches established pattern from `Stroke.Examples.Choices` and `Stroke.Examples.Prompts`. Each example is a static class with a `Run()` method.

**Alternatives Considered**:
- Separate executable per example — rejected (violates established pattern, bloats solution)

### Decision 2: Example Naming Convention

**Chosen**: PascalCase class names matching Python file names: `messagebox.py` → `MessageBox.cs`

**Rationale**: Per `docs/examples-mapping.md` naming conventions.

### Decision 3: Ctrl+C/Ctrl+D Handling

**Chosen**: Wrap each example in try-catch for `KeyboardInterruptException` and `EOFException`

**Rationale**: Spec FR-005 and FR-006 require graceful handling without stack traces.

**Implementation**:
```csharp
try
{
    RunExample();
}
catch (KeyboardInterruptException)
{
    // Graceful exit on Ctrl+C
}
catch (EOFException)
{
    // Graceful exit on Ctrl+D
}
```

### Decision 4: ProgressDialog File Enumeration

**Chosen**: Walk through `../../` (examples/../..) directory, matching Python behavior

**Rationale**: Python example uses `os.walk("../..")`. C# equivalent is `Directory.EnumerateFileSystemEntries()`.

**Edge Case**: Catch `UnauthorizedAccessException` and continue (spec edge case requirement).

## Dependencies

### Existing Dependencies (No Changes)

| Dependency | Location | Status |
|------------|----------|--------|
| Stroke library | `src/Stroke/Stroke.csproj` | ✅ Exists |
| Examples solution | `examples/Stroke.Examples.sln` | ✅ Exists |
| Dialogs API | `Stroke.Shortcuts.Dialogs` | ✅ Feature 48 complete |
| Style.FromDict | `Stroke.Styles.Style` | ✅ Feature 18 complete |
| Html class | `Stroke.FormattedText.Html` | ✅ Feature 15 complete |

### New Files to Create

| File | Purpose |
|------|---------|
| `examples/Stroke.Examples.Dialogs/Stroke.Examples.Dialogs.csproj` | Project configuration |
| `examples/Stroke.Examples.Dialogs/Program.cs` | Entry point with routing |
| `examples/Stroke.Examples.Dialogs/MessageBox.cs` | Example 1 |
| `examples/Stroke.Examples.Dialogs/YesNoDialog.cs` | Example 2 |
| `examples/Stroke.Examples.Dialogs/ButtonDialog.cs` | Example 3 |
| `examples/Stroke.Examples.Dialogs/InputDialog.cs` | Example 4 |
| `examples/Stroke.Examples.Dialogs/PasswordDialog.cs` | Example 5 |
| `examples/Stroke.Examples.Dialogs/RadioDialog.cs` | Example 6 |
| `examples/Stroke.Examples.Dialogs/CheckboxDialog.cs` | Example 7 |
| `examples/Stroke.Examples.Dialogs/ProgressDialog.cs` | Example 8 |
| `examples/Stroke.Examples.Dialogs/StyledMessageBox.cs` | Example 9 |

## Risks and Mitigations

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| ProgressDialog file enumeration hangs | Low | Cap at 100% progress, add timeout fallback |
| Style.FromDict CSS selectors differ | Low | Verified selectors match Python implementation |
| TUI Driver testing limitations | Medium | Manual verification as fallback |

## References

- Python source: `/Users/brandon/src/python-prompt-toolkit/examples/dialogs/`
- Stroke Dialogs API: `/Users/brandon/src/stroke/src/Stroke/Shortcuts/Dialogs.cs`
- Examples mapping: `/Users/brandon/src/stroke/docs/examples-mapping.md`
- Existing pattern: `/Users/brandon/src/stroke/examples/Stroke.Examples.Choices/`
