# Quickstart: Toolbar Widgets

**Feature**: 044-toolbar-widgets
**Date**: 2026-02-01

## Implementation Order

Implementation follows user story priority and dependency chain:

1. **FormattedTextToolbar** (P1, FR-001) — simplest toolbar, Window subclass, no dependencies on other toolbars
2. **SystemToolbar** (P1, FR-002 through FR-006) — most complex, exercises key bindings + Buffer + Container
3. **ArgToolbar** (P2, FR-007, FR-008) — simple conditional display
4. **SearchToolbar** (P2, FR-009 through FR-012) — SearchBufferControl integration
5. **CompletionsToolbarControl** (P2, FR-013 through FR-015) — internal UIControl with pagination
6. **CompletionsToolbar** (P2, FR-016) — thin wrapper over CompletionsToolbarControl
7. **ValidationToolbar** (P3, FR-017, FR-018) — simple conditional display

## Build Sequence

```text
Step 1: Create directory structure
  └── src/Stroke/Widgets/Toolbars/
  └── tests/Stroke.Tests/Widgets/Toolbars/

Step 2: Implement FormattedTextToolbar + tests
  └── FormattedTextToolbar.cs
  └── FormattedTextToolbarTests.cs
  └── Verify: construction, style, height, dynamic text

Step 3: Implement SystemToolbar + tests
  └── SystemToolbar.cs
  └── SystemToolbarTests.cs
  └── Verify: buffer creation, key binding registration, conditional visibility,
              IMagicContainer protocol, Emacs/Vi/global binding groups

Step 4: Implement ArgToolbar + tests
  └── ArgToolbar.cs
  └── ArgToolbarTests.cs
  └── Verify: conditional visibility, "-" → "-1" conversion, format

Step 5: Implement SearchToolbar + tests
  └── SearchToolbar.cs
  └── SearchToolbarTests.cs
  └── Verify: SearchBufferControl creation, is_searching condition,
              prompt selection, vi_mode, custom prompts, buffer auto-creation

Step 6: Implement CompletionsToolbarControl + CompletionsToolbar + tests
  └── CompletionsToolbarControl.cs
  └── CompletionsToolbar.cs
  └── CompletionsToolbarControlTests.cs
  └── CompletionsToolbarTests.cs
  └── Verify: pagination at various widths, arrow indicators,
              current item highlighting, edge cases (width < 7, CJK)

Step 7: Implement ValidationToolbar + tests
  └── ValidationToolbar.cs
  └── ValidationToolbarTests.cs
  └── Verify: error display, show_position formatting, conditional visibility

Step 8: Verify coverage and constitution compliance
  └── Run tests: dotnet test
  └── Check LOC limits per file
  └── Verify 80% coverage target
```

## Key Implementation Notes

### FormattedTextToolbar
- Extends `Window`, does NOT implement `IMagicContainer`
- Wraps `AnyFormattedText` → `Func<IReadOnlyList<StyleAndTextTuple>>` → `FormattedTextControl`
- Sets `dontExtendHeight: true`, `height: new Dimension(min: 1)`
- Style goes to Window, not FormattedTextControl

### SystemToolbar
- Creates Buffer with `name: BufferNames.System`
- Three key binding groups (emacs, vi, global) merged via `MergedKeyBindings`
- Enter handler is async (calls `Application.RunSystemCommandAsync`)
- Global binding: M-! uses `Keys.Escape` + `"!"` (two-key sequence) with `isGlobal: true`
- Uses `~focused & emacs_mode` filter for M-! binding (Python uses `~focused & emacs_mode`)
- Uses `~focused & vi_mode & vi_navigation_mode` filter for ! binding

### CompletionsToolbarControl
- Content width = `width - 6` (3 chars margin each side: space + arrow + space)
- Python's `fragments[:content_width]` is a safety trim — translates to limiting fragment count
- `Completion.DisplayText` is the plain text property for display
- Must use `FormattedTextUtils.ToFormattedText()` for applying style to display text
- `FormattedTextUtils.FragmentListLen()` for accumulated width measurement

### SearchToolbar
- `SearcherSearchState.Direction` on the `SearchBufferControl` determines forward/backward
- The `is_searching` condition checks `Layout.SearchLinks.ContainsKey(control)`
- Default `textIfNotSearching` is empty string `""`

## Reference Files

| File | Purpose |
|------|---------|
| `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/widgets/toolbars.py` | Python source (371 lines) |
| `src/Stroke/Layout/Containers/Window.cs` | Window base class |
| `src/Stroke/Layout/Containers/ConditionalContainer.cs` | Conditional visibility |
| `src/Stroke/Layout/Containers/IMagicContainer.cs` | Container protocol interface |
| `src/Stroke/Layout/Controls/FormattedTextControl.cs` | Text display control |
| `src/Stroke/Layout/Controls/BufferControl.cs` | Buffer display control |
| `src/Stroke/Layout/Controls/SearchBufferControl.cs` | Search-specific buffer control |
| `src/Stroke/Layout/Controls/UIContent.cs` | Content model for UIControl |
| `src/Stroke/Layout/Processors/BeforeInput.cs` | Input prefix processor |
| `src/Stroke/Application/AppFilters.cs` | Application state filters |
| `src/Stroke/Core/Buffer.cs` | Mutable buffer |
| `src/Stroke/Core/CompletionState.cs` | Completion tracking |
| `src/Stroke/KeyBinding/KeyBindings.cs` | Key binding registry |
| `src/Stroke/KeyBinding/ConditionalKeyBindings.cs` | Filter-gated bindings |
| `src/Stroke/KeyBinding/MergedKeyBindings.cs` | Merged binding groups |
