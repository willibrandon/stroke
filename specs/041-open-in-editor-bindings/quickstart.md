# Quickstart: Open in Editor Bindings

**Feature**: 041-open-in-editor-bindings
**Date**: 2026-01-31

## Implementation Order

### Step 1: Create OpenInEditorBindings.cs

**File**: `src/Stroke/Application/Bindings/OpenInEditorBindings.cs`

Create a static class with three methods:

1. `LoadEmacsOpenInEditorBindings()` — Creates a `KeyBindings` with one binding:
   - Keys: `[Keys.ControlX, Keys.ControlE]`
   - Handler: `NamedCommands.GetByName("edit-and-execute-command")`
   - Filter: `EmacsFilters.EmacsMode` AND NOT `AppFilters.HasSelection`

2. `LoadViOpenInEditorBindings()` — Creates a `KeyBindings` with one binding:
   - Keys: `['v']`
   - Handler: `NamedCommands.GetByName("edit-and-execute-command")`
   - Filter: `ViFilters.ViNavigationMode`

3. `LoadOpenInEditorBindings()` — Returns `new MergedKeyBindings(emacs, vi)`

### Step 2: Create OpenInEditorBindingsTests.cs

**File**: `tests/Stroke.Tests/Application/Bindings/OpenInEditorBindingsTests.cs`

**Test environment setup** (follows `AutoSuggestBindingsTests` convention):

```csharp
public sealed class OpenInEditorBindingsTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public OpenInEditorBindingsTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose() => _input.Dispose();

    private (Buffer Buffer, Application<object> App, IDisposable Scope)
        CreateEnvironment(EditingMode editingMode = EditingMode.Emacs)
    {
        var buffer = new Buffer();
        var bc = new BufferControl(buffer: buffer);
        var window = new Window(content: bc);
        var container = new HSplit([window]);
        var layout = new Stroke.Layout.Layout(new AnyContainer(container));
        var app = new Application<object>(
            input: _input, output: _output, layout: layout, editingMode: editingMode);
        var scope = AppContext.SetApp(app.UnsafeCast);
        return (buffer, app, scope);
    }
}
```

**Test categories**:
- Loader returns correct binding count (1 Emacs, 1 Vi, 2 combined)
- Emacs binding has correct key sequence (ControlX, ControlE)
- Vi binding has correct key ('v')
- Emacs binding filter activates in Emacs mode without selection
- Emacs binding filter deactivates when selection is active
- Emacs binding filter deactivates in Vi mode
- Vi binding filter activates in Vi navigation mode
- Vi binding filter deactivates in Vi insert mode
- Combined loader contains both bindings
- Handler resolves to edit-and-execute-command

### Step 3: Verify

```bash
dotnet test tests/Stroke.Tests --filter "FullyQualifiedName~OpenInEditorBindings"
```

## Prerequisites (Already Implemented)

- `Buffer.OpenInEditorAsync` — `src/Stroke/Core/Buffer.ExternalEditor.cs`
- `edit-and-execute-command` named command — `src/Stroke/KeyBinding/Bindings/NamedCommands.Misc.cs`
- `EmacsFilters.EmacsMode` — `src/Stroke/Application/EmacsFilters.cs`
- `ViFilters.ViNavigationMode` — `src/Stroke/Application/ViFilters.cs`
- `AppFilters.HasSelection` — `src/Stroke/Application/AppFilters.cs`
- `MergedKeyBindings` — `src/Stroke/KeyBinding/MergedKeyBindings.cs`
