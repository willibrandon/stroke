# Feature 88: Core Enums and Constants

## Overview

Implement core enumerations and constants used throughout the library, including editing modes and buffer names.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/enums.py`

## Public API

### EditingMode Enum

```csharp
namespace Stroke;

/// <summary>
/// The set of key bindings that is active.
/// </summary>
public enum EditingMode
{
    /// <summary>
    /// Vi editing mode with modal editing.
    /// </summary>
    Vi,

    /// <summary>
    /// Emacs editing mode.
    /// </summary>
    Emacs
}
```

### Buffer Names

```csharp
namespace Stroke;

/// <summary>
/// Well-known buffer names used throughout the library.
/// </summary>
public static class BufferNames
{
    /// <summary>
    /// Name of the search buffer.
    /// </summary>
    public const string Search = "SEARCH_BUFFER";

    /// <summary>
    /// Name of the default/main buffer.
    /// </summary>
    public const string Default = "DEFAULT_BUFFER";

    /// <summary>
    /// Name of the system command buffer.
    /// </summary>
    public const string System = "SYSTEM_BUFFER";
}
```

## Project Structure

```
src/Stroke/
├── EditingMode.cs
└── BufferNames.cs
tests/Stroke.Tests/
└── EnumsTests.cs
```

## Implementation Notes

### EditingMode Usage

The editing mode determines which key bindings are active:

```csharp
// In Application
public EditingMode EditingMode { get; set; } = EditingMode.Emacs;

// In key binding processor
public bool Matches(KeyPress keyPress)
{
    // Check mode-specific bindings
    var app = Application.Current;
    if (RequiresMode == EditingMode.Vi && app.EditingMode != EditingMode.Vi)
        return false;
    if (RequiresMode == EditingMode.Emacs && app.EditingMode != EditingMode.Emacs)
        return false;

    return MatchesKey(keyPress);
}
```

### Buffer Name Usage

Buffer names are used to identify specific buffers in the application:

```csharp
// In Application
public Buffer GetBuffer(string name)
{
    return name switch
    {
        BufferNames.Default => _defaultBuffer,
        BufferNames.Search => _searchBuffer,
        BufferNames.System => _systemBuffer,
        _ => _namedBuffers.GetValueOrDefault(name)
            ?? throw new KeyNotFoundException($"Buffer '{name}' not found")
    };
}

// In Layout
public void FocusBuffer(string name)
{
    var bufferControl = FindBufferControl(name);
    if (bufferControl != null)
        Focus(bufferControl);
}
```

### Filter Integration

Editing mode is commonly used in filters for conditional key bindings:

```csharp
// Built-in filters
public static class Filters
{
    public static IFilter ViMode =>
        new Condition(() => Application.Current?.EditingMode == EditingMode.Vi);

    public static IFilter EmacsMode =>
        new Condition(() => Application.Current?.EditingMode == EditingMode.Emacs);

    public static IFilter ViInsertMode => ViMode & new Condition(
        () => Application.Current?.ViState?.InputMode == InputMode.Insert);

    public static IFilter ViNavigationMode => ViMode & new Condition(
        () => Application.Current?.ViState?.InputMode == InputMode.Navigation);
}
```

### Mode Switching

```csharp
// Key binding for mode switching
KeyBindings.Add(
    Keys.Escape, Keys.Escape,
    handler: (e) =>
    {
        var app = Application.Current;
        app.EditingMode = app.EditingMode == EditingMode.Vi
            ? EditingMode.Emacs
            : EditingMode.Vi;
    });
```

## Dependencies

None (core types).

## Implementation Tasks

1. Implement `EditingMode` enum
2. Implement `BufferNames` static class
3. Integrate with Application
4. Integrate with Filter system
5. Write unit tests

## Acceptance Criteria

- [ ] EditingMode has Vi and Emacs values
- [ ] BufferNames has Search, Default, System constants
- [ ] Application respects EditingMode setting
- [ ] Filters can check current EditingMode
- [ ] Buffer lookup by name works correctly
- [ ] Unit tests achieve 80% coverage
