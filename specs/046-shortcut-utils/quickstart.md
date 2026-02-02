# Quickstart: Shortcut Utilities

**Feature**: 046-shortcut-utils
**Date**: 2026-02-01

## Prerequisites

Before implementing, verify these exist and compile:

```bash
# All of these should already exist
ls src/Stroke/Rendering/RendererUtils.cs        # PrintFormattedText static method
ls src/Stroke/Application/AppContext.cs          # GetAppOrNull, GetAppSession
ls src/Stroke/Application/RunInTerminal.cs       # RunAsync for running-app dispatch
ls src/Stroke/Styles/StyleMerger.cs              # MergeStyles
ls src/Stroke/Styles/DefaultStyles.cs            # DefaultUiStyle, DefaultPygmentsStyle
ls src/Stroke/Output/OutputFactory.cs            # Create(TextWriter)
ls src/Stroke/Input/DummyInput.cs                # For PrintContainer
ls src/Stroke/FormattedText/FormattedTextUtils.cs # ToFormattedText
```

## Implementation Order

### Step 1: Create directory structure

```bash
mkdir -p src/Stroke/Shortcuts
mkdir -p tests/Stroke.Tests/Shortcuts
```

### Step 2: Implement TerminalUtils (simplest — no dependencies beyond IOutput)

File: `src/Stroke/Shortcuts/TerminalUtils.cs`

Three methods, all one-liners delegating to `AppContext.GetAppSession().Output`:
1. `Clear()` → `output.EraseScreen()` + `output.CursorGoto(0, 0)` + `output.Flush()`
2. `SetTitle(string text)` → `output.SetTitle(text)`
3. `ClearTitle()` → `SetTitle("")`

### Step 3: Implement FormattedTextOutput.CreateMergedStyle (private helper)

File: `src/Stroke/Shortcuts/FormattedTextOutput.cs`

Direct port of `_create_merged_style`:
1. Start with `[DefaultStyles.DefaultUiStyle]`
2. Conditionally add `DefaultStyles.DefaultPygmentsStyle`
3. Conditionally add user `style`
4. Return `StyleMerger.MergeStyles(styles)`

### Step 4: Implement FormattedTextOutput.Print (multi-value overload — core logic)

The main implementation with the `object[] values` parameter:

1. **Validate**: Throw `ArgumentException` if both `output` and `file` are non-null
2. **Resolve output**: `file` → `OutputFactory.Create(stdout: file)`, else `AppContext.GetAppSession().Output`
3. **Resolve color depth**: `colorDepth ?? output.GetDefaultColorDepth()`
4. **Build fragments**: Convert each value via `ToText` helper, join with `sep`, append `end`
5. **Define render action**: Call `RendererUtils.PrintFormattedText` with merged style, then conditionally flush
6. **Dispatch**: If `AppContext.GetAppOrNull()` is running → `RunInTerminal.RunAsync(render)`, else `render()`

### Step 5: Implement FormattedTextOutput.Print (single-value overload)

Wraps the single `AnyFormattedText` in an array and delegates to the multi-value overload.

### Step 6: Implement FormattedTextOutput.PrintContainer

1. **Resolve output**: `file` → `OutputFactory.Create(stdout: file)`, else `AppContext.GetAppSession().Output`
2. **Create Application**: `new Application<object?>(layout: new Layout(container), output: output, input: new DummyInput(), style: CreateMergedStyle(style, includeDefaultPygmentsStyle))`
3. **Run and catch**: `app.Run(inThread: true)` wrapped in try/catch `EndOfStreamException`

### Step 7: Write tests

Two test files covering all acceptance scenarios from the spec:

- `FormattedTextOutputTests.cs`: Tests for Print (plain string, HTML, multi-value, sep, end, file, flush, style, output+file conflict, zero values, plain list conversion) and PrintContainer
- `TerminalUtilsTests.cs`: Tests for Clear, SetTitle, ClearTitle

### Testing Strategy (No Mocks)

Tests use **real infrastructure**:
- Capture output via `StringWriter` + `OutputFactory.Create(stdout: writer)` → `PlainTextOutput`
- For escape sequence verification, use `Vt100Output.FromPty(writer)` and inspect raw output
- For Application-running scenarios, create a real `Application` with `DummyInput` and verify dispatch
- For style verification, use real `Style.FromDict()` and verify attributes in output

## Key Python → C# Translations

| Python | C# |
|--------|----|
| `*values: Any` | `object[] values` |
| `assert not (output and file)` | `if (output is not null && file is not null) throw new ArgumentException(...)` |
| `create_output(stdout=file)` | `OutputFactory.Create(stdout: file)` |
| `get_app_session().output` | `AppContext.GetAppSession().Output` |
| `get_app_or_none()` | `AppContext.GetAppOrNull()` |
| `app.loop` | `app.IsRunning` (no explicit loop in C#) |
| `loop.call_soon_threadsafe(lambda: run_in_terminal(render))` | `RunInTerminal.RunAsync(render)` |
| `to_formatted_text(val, auto_convert=True)` | `FormattedTextUtils.ToFormattedText(new AnyFormattedText(val), autoConvert: true)` |
| `isinstance(val, list) and not isinstance(val, FormattedText)` | `val is IList && val is not FormattedText` |
| `merge_styles([...])` | `StyleMerger.MergeStyles([...])` |
| `default_ui_style()` | `DefaultStyles.DefaultUiStyle` |
| `default_pygments_style()` | `DefaultStyles.DefaultPygmentsStyle` |
| `DummyInput()` | `new DummyInput()` |
| `app.run(in_thread=True)` | `app.Run(inThread: true)` |
| `except EOFError: pass` | `catch (EndOfStreamException) { }` |
| `output.erase_screen()` | `output.EraseScreen()` |
| `output.cursor_goto(0, 0)` | `output.CursorGoto(0, 0)` |
| `output.set_title(text)` | `output.SetTitle(text)` |
| `output.flush()` | `output.Flush()` |
