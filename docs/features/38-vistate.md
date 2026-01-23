# Feature 38: Vi State

## Overview

Implement the Vi editing mode state management including input modes, character find state, operator state, named registers, digraph handling, and macro recording.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/vi_state.py`

## Public API

### InputMode Enum

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Vi input mode enumeration.
/// </summary>
public enum InputMode
{
    /// <summary>
    /// Insert mode - regular text input.
    /// </summary>
    Insert,

    /// <summary>
    /// Insert multiple mode - insert on multiple cursors.
    /// </summary>
    InsertMultiple,

    /// <summary>
    /// Navigation mode (also known as normal mode).
    /// </summary>
    Navigation,

    /// <summary>
    /// Replace mode - overwrite characters.
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
/// Represents a character find operation for Vi mode.
/// Used to repeat the last character search with ; or ,.
/// </summary>
public sealed class CharacterFind
{
    /// <summary>
    /// Creates a CharacterFind.
    /// </summary>
    /// <param name="character">The character to find.</param>
    /// <param name="backwards">Search backwards if true.</param>
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
    /// <summary>
    /// Creates a ViState with default INSERT mode.
    /// </summary>
    public ViState();

    /// <summary>
    /// The last character find operation.
    /// Used to repeat the last search with 'n' or 'N' in navigation mode.
    /// </summary>
    public CharacterFind? LastCharacterFind { get; set; }

    /// <summary>
    /// Operator callback when waiting for text object.
    /// For example, after 'd' in 'dw', waiting for 'w' motion.
    /// </summary>
    public Action<KeyPressEvent, TextObject>? OperatorFunc { get; set; }

    /// <summary>
    /// Operator argument (count) for the pending operator.
    /// </summary>
    public int? OperatorArg { get; set; }

    /// <summary>
    /// Named registers mapping register name (e.g., 'a') to ClipboardData.
    /// </summary>
    public IDictionary<string, ClipboardData> NamedRegisters { get; }

    /// <summary>
    /// The current Vi input mode.
    /// Setting to Navigation clears operator state and digraph waiting.
    /// </summary>
    public InputMode InputMode { get; set; }

    /// <summary>
    /// True when waiting for a digraph character.
    /// </summary>
    public bool WaitingForDigraph { get; set; }

    /// <summary>
    /// First symbol of a digraph pair (null if not in digraph mode).
    /// </summary>
    public string? DigraphSymbol1 { get; set; }

    /// <summary>
    /// When true, the ~ key acts as an operator.
    /// </summary>
    public bool TildeOperator { get; set; }

    /// <summary>
    /// Register name currently recording a macro.
    /// Null when not recording.
    /// </summary>
    public string? RecordingRegister { get; set; }

    /// <summary>
    /// The current recording buffer for macro recording.
    /// </summary>
    public string CurrentRecording { get; set; }

    /// <summary>
    /// True when in temporary navigation mode (after Ctrl-O in insert mode).
    /// User can do one navigation action then returns to insert/replace.
    /// </summary>
    public bool TemporaryNavigationMode { get; set; }

    /// <summary>
    /// Reset state to INSERT mode and clear all transient state.
    /// </summary>
    public void Reset();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    ├── InputMode.cs
    ├── CharacterFind.cs
    └── ViState.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── ViStateTests.cs
```

## Implementation Notes

### InputMode Setter Behavior

When setting `InputMode` to `Navigation`:
1. Clear `WaitingForDigraph` to false
2. Clear `OperatorFunc` to null
3. Clear `OperatorArg` to null

This ensures clean state when returning to navigation mode.

### Operator State

The operator/motion pattern in Vi:
1. User presses operator key (d, c, y, etc.)
2. `OperatorFunc` is set to the operator handler
3. `OperatorArg` may store a count prefix
4. User presses motion (w, e, $, etc.)
5. Motion is evaluated to get `TextObject`
6. `OperatorFunc` is called with the `TextObject`
7. State is cleared

### Named Registers

Vi named registers (a-z) store yanked/deleted text:
- `"ay` - Yank into register 'a'
- `"ap` - Paste from register 'a'
- Registers persist across operations
- Stored as `ClipboardData` to preserve selection type

### Macro Recording

Macro recording flow:
1. Press `q` followed by register name (e.g., `qa`)
2. `RecordingRegister` is set to 'a'
3. `CurrentRecording` accumulates key presses
4. Press `q` again to stop recording
5. Recording is stored in `NamedRegisters`
6. `@a` replays the macro

### Temporary Navigation Mode

Ctrl-O in insert mode:
1. Sets `TemporaryNavigationMode` to true
2. User executes one navigation command
3. Mode returns to insert automatically
4. Similar to Vim's `Ctrl-O` behavior

### Digraph Entry

Ctrl-K starts digraph entry:
1. `WaitingForDigraph` set to true
2. First character stored in `DigraphSymbol1`
3. Second character completes the digraph
4. Digraph character is inserted

## Dependencies

- `Stroke.Clipboard.ClipboardData` (Feature 03) - Clipboard data
- `Stroke.KeyBinding.KeyPressEvent` (Feature 20) - Key events
- `Stroke.KeyBinding.TextObject` (Feature 21) - Text objects

## Implementation Tasks

1. Implement `InputMode` enum
2. Implement `CharacterFind` class
3. Implement `ViState` class with all properties
4. Implement `InputMode` setter with state clearing
5. Implement `Reset()` method
6. Implement `NamedRegisters` dictionary
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] InputMode enum matches Python Prompt Toolkit values
- [ ] CharacterFind stores character and direction
- [ ] ViState maintains all state properties
- [ ] InputMode setter clears operator state when entering Navigation
- [ ] Reset() returns to INSERT mode and clears state
- [ ] Named registers store ClipboardData correctly
- [ ] Macro recording state works correctly
- [ ] Unit tests achieve 80% coverage
