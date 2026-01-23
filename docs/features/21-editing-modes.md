# Feature 21: Editing Modes and State

## Overview

Implement the editing mode enums and state classes for Vi and Emacs modes.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/enums.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/vi_state.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/emacs_state.py`

## Public API

### EditingMode Enum

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// The set of key bindings that is active.
/// </summary>
public enum EditingMode
{
    /// <summary>
    /// Vi key bindings.
    /// </summary>
    Vi,

    /// <summary>
    /// Emacs key bindings.
    /// </summary>
    Emacs
}
```

### Buffer Name Constants

```csharp
namespace Stroke.Core;

/// <summary>
/// Standard buffer names.
/// </summary>
public static class BufferNames
{
    /// <summary>
    /// Name of the search buffer.
    /// </summary>
    public const string SearchBuffer = "SEARCH_BUFFER";

    /// <summary>
    /// Name of the default buffer.
    /// </summary>
    public const string DefaultBuffer = "DEFAULT_BUFFER";

    /// <summary>
    /// Name of the system buffer.
    /// </summary>
    public const string SystemBuffer = "SYSTEM_BUFFER";
}
```

### Vi InputMode Enum

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Vi input mode.
/// </summary>
public enum InputMode
{
    /// <summary>
    /// Insert mode.
    /// </summary>
    Insert,

    /// <summary>
    /// Insert multiple mode (for visual block insert).
    /// </summary>
    InsertMultiple,

    /// <summary>
    /// Navigation mode (normal mode).
    /// </summary>
    Navigation,

    /// <summary>
    /// Replace mode.
    /// </summary>
    Replace,

    /// <summary>
    /// Replace single character mode.
    /// </summary>
    ReplaceSingle
}
```

### CharacterFind Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Stores the last character find operation for Vi.
/// </summary>
public sealed class CharacterFind
{
    /// <summary>
    /// Creates a character find.
    /// </summary>
    /// <param name="character">The character to find.</param>
    /// <param name="backwards">True if searching backwards.</param>
    public CharacterFind(string character, bool backwards = false);

    /// <summary>
    /// The character to find.
    /// </summary>
    public string Character { get; }

    /// <summary>
    /// True if searching backwards.
    /// </summary>
    public bool Backwards { get; }
}
```

### ViState Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Mutable class to hold the state of Vi navigation.
/// </summary>
public sealed class ViState
{
    public ViState();

    /// <summary>
    /// The last character find operation (for repeating with ; and ,).
    /// </summary>
    public CharacterFind? LastCharacterFind { get; set; }

    /// <summary>
    /// When an operator is given and we are waiting for a text object
    /// (e.g., after 'd' in 'dw'), this callback is set.
    /// </summary>
    public Func<KeyPressEvent, ITextObject, object?>? OperatorFunc { get; set; }

    /// <summary>
    /// The count argument for the operator.
    /// </summary>
    public int? OperatorArg { get; set; }

    /// <summary>
    /// Named registers. Maps register name (e.g., 'a') to ClipboardData.
    /// </summary>
    public IDictionary<string, ClipboardData> NamedRegisters { get; }

    /// <summary>
    /// The current Vi input mode.
    /// </summary>
    public InputMode InputMode { get; set; }

    /// <summary>
    /// True when waiting for a digraph character.
    /// </summary>
    public bool WaitingForDigraph { get; set; }

    /// <summary>
    /// The first symbol of a digraph (if any).
    /// </summary>
    public string? DigraphSymbol1 { get; set; }

    /// <summary>
    /// When true, make ~ act as an operator.
    /// </summary>
    public bool TildeOperator { get; set; }

    /// <summary>
    /// Register in which we are recording a macro.
    /// Null when not recording.
    /// </summary>
    public string? RecordingRegister { get; set; }

    /// <summary>
    /// The current macro recording content.
    /// </summary>
    public string CurrentRecording { get; set; }

    /// <summary>
    /// Temporary navigation (normal) mode.
    /// Active after Ctrl+O in insert/replace mode.
    /// </summary>
    public bool TemporaryNavigationMode { get; set; }

    /// <summary>
    /// Reset state, go back to insert mode.
    /// </summary>
    public void Reset();
}
```

### EmacsState Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Mutable class to hold Emacs specific state.
/// </summary>
public sealed class EmacsState
{
    public EmacsState();

    /// <summary>
    /// The recorded macro (list of key presses).
    /// </summary>
    public IList<KeyPress>? Macro { get; set; }

    /// <summary>
    /// The current recording (null when not recording).
    /// </summary>
    public IList<KeyPress>? CurrentRecording { get; set; }

    /// <summary>
    /// True when we are recording a macro.
    /// </summary>
    public bool IsRecording { get; }

    /// <summary>
    /// Start recording a macro.
    /// </summary>
    public void StartMacro();

    /// <summary>
    /// End recording a macro.
    /// </summary>
    public void EndMacro();

    /// <summary>
    /// Reset state.
    /// </summary>
    public void Reset();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    ├── EditingMode.cs
    ├── InputMode.cs
    ├── CharacterFind.cs
    ├── ViState.cs
    └── EmacsState.cs
src/Stroke/
└── Core/
    └── BufferNames.cs
tests/Stroke.Tests/
└── KeyBinding/
    ├── ViStateTests.cs
    └── EmacsStateTests.cs
```

## Implementation Notes

### Vi Mode State Machine

The `ViState.InputMode` property setter resets certain state when entering navigation mode:
- `WaitingForDigraph` is set to false
- `OperatorFunc` is set to null
- `OperatorArg` is set to null

### Macro Recording

Vi and Emacs have different macro recording mechanisms:
- **Vi**: Records key data as strings, stored in named registers
- **Emacs**: Records key presses as `KeyPress` objects

### Named Registers

Vi named registers (a-z) store clipboard data that can be accessed with `"x` prefix.

### Digraph Input

Vi supports digraph input (e.g., Ctrl+K followed by two characters) for entering special characters.

### Temporary Navigation Mode

Pressing Ctrl+O in Vi insert or replace mode temporarily switches to navigation mode for one command.

## Dependencies

- `Stroke.Input.KeyPress` (Feature 16) - Key press class
- `Stroke.Clipboard.ClipboardData` (Feature 08) - Clipboard data

## Implementation Tasks

1. Implement `EditingMode` enum
2. Implement `BufferNames` constants
3. Implement `InputMode` enum
4. Implement `CharacterFind` class
5. Implement `ViState` class
6. Implement `EmacsState` class
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All editing modes match Python Prompt Toolkit semantics
- [ ] Vi state management works correctly
- [ ] Emacs state management works correctly
- [ ] Macro recording works correctly
- [ ] Unit tests achieve 80% coverage
