# Contract: OpenInEditorBindings

**Module**: `Stroke.Application.Bindings`
**Python Source**: `prompt_toolkit.key_binding.bindings.open_in_editor`
**Date**: 2026-01-31

## Class: OpenInEditorBindings

Static class containing three binding loader functions for opening the current buffer in an external editor.

```csharp
namespace Stroke.Application.Bindings;

/// <summary>
/// Key binding loaders for opening the current buffer content in an external editor.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.open_in_editor</c> module.
/// Provides 3 binding loaders: Emacs (Ctrl-X Ctrl-E), Vi ('v' in navigation mode), and combined.
/// </para>
/// <para>
/// All loaders delegate to the <c>edit-and-execute-command</c> named command, which calls
/// <see cref="Buffer.OpenInEditorAsync"/> with <c>validateAndHandle: true</c>.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. Each loader creates and returns a new
/// key bindings instance.
/// </para>
/// </remarks>
public static class OpenInEditorBindings
{
    /// <summary>
    /// Load both the Vi and Emacs key bindings for handling edit-and-execute-command.
    /// </summary>
    /// <returns>
    /// A <see cref="MergedKeyBindings"/> containing both Emacs and Vi open-in-editor bindings.
    /// </returns>
    public static IKeyBindingsBase LoadOpenInEditorBindings();

    /// <summary>
    /// Load Emacs key binding for opening the buffer in an external editor.
    /// Pressing Ctrl-X followed by Ctrl-E invokes <c>edit-and-execute-command</c>.
    /// </summary>
    /// <returns>
    /// A <see cref="KeyBindings"/> with one binding: Ctrl-X Ctrl-E filtered by
    /// Emacs mode with no active selection.
    /// </returns>
    public static KeyBindings LoadEmacsOpenInEditorBindings();

    /// <summary>
    /// Load Vi key binding for opening the buffer in an external editor.
    /// Pressing 'v' in navigation mode invokes <c>edit-and-execute-command</c>.
    /// </summary>
    /// <returns>
    /// A <see cref="KeyBindings"/> with one binding: 'v' filtered by Vi navigation mode.
    /// </returns>
    public static KeyBindings LoadViOpenInEditorBindings();
}
```

## Binding Details

### Emacs Binding

| Property | Value |
|----------|-------|
| Keys | `[Keys.ControlX, Keys.ControlE]` (two-key sequence) |
| Handler | `NamedCommands.GetByName("edit-and-execute-command")` |
| Filter | `EmacsFilters.EmacsMode & AppFilters.HasSelection.Invert()` |

### Vi Binding

| Property | Value |
|----------|-------|
| Keys | `['v']` (single character) |
| Handler | `NamedCommands.GetByName("edit-and-execute-command")` |
| Filter | `ViFilters.ViNavigationMode` |

### Combined Loader

| Property | Value |
|----------|-------|
| Merges | `LoadEmacsOpenInEditorBindings()` + `LoadViOpenInEditorBindings()` |
| Returns | `MergedKeyBindings` (implements `IKeyBindingsBase`) |
| Total bindings | 2 (1 Emacs + 1 Vi) |

## Python Source Reference

```python
# prompt_toolkit/key_binding/bindings/open_in_editor.py

def load_open_in_editor_bindings() -> KeyBindingsBase:
    return merge_key_bindings([
        load_emacs_open_in_editor_bindings(),
        load_vi_open_in_editor_bindings(),
    ])

def load_emacs_open_in_editor_bindings() -> KeyBindings:
    key_bindings = KeyBindings()
    key_bindings.add("c-x", "c-e", filter=emacs_mode & ~has_selection)(
        get_by_name("edit-and-execute-command"))
    return key_bindings

def load_vi_open_in_editor_bindings() -> KeyBindings:
    key_bindings = KeyBindings()
    key_bindings.add("v", filter=vi_navigation_mode)(
        get_by_name("edit-and-execute-command"))
    return key_bindings
```
