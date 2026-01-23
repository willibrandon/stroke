# Feature 39: Emacs State

## Overview

Implement the Emacs editing mode state management including macro recording and playback support.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/emacs_state.py`

## Public API

### EmacsState Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Mutable class to hold Emacs-specific state.
/// </summary>
public sealed class EmacsState
{
    /// <summary>
    /// Creates an EmacsState.
    /// </summary>
    public EmacsState();

    /// <summary>
    /// The stored macro (list of key presses).
    /// This is the macro that was recorded and can be played back.
    /// </summary>
    public IList<KeyPress>? Macro { get; set; }

    /// <summary>
    /// The current recording buffer.
    /// Null when not recording, otherwise accumulates key presses.
    /// </summary>
    public IList<KeyPress>? CurrentRecording { get; set; }

    /// <summary>
    /// True when currently recording a macro.
    /// </summary>
    public bool IsRecording { get; }

    /// <summary>
    /// Start recording a macro.
    /// Initializes CurrentRecording to an empty list.
    /// </summary>
    public void StartMacro();

    /// <summary>
    /// End recording a macro.
    /// Copies CurrentRecording to Macro and clears CurrentRecording.
    /// </summary>
    public void EndMacro();

    /// <summary>
    /// Reset state.
    /// Clears CurrentRecording (stops recording if active).
    /// </summary>
    public void Reset();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── EmacsState.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── EmacsStateTests.cs
```

## Implementation Notes

### Macro Recording Flow

1. **Start Recording** (Ctrl-X `(`):
   - Call `StartMacro()`
   - `CurrentRecording` initialized to empty list
   - `IsRecording` returns true

2. **During Recording**:
   - Key processor adds each `KeyPress` to `CurrentRecording`
   - All key presses are recorded (except macro control keys)

3. **End Recording** (Ctrl-X `)`):
   - Call `EndMacro()`
   - `CurrentRecording` copied to `Macro`
   - `CurrentRecording` set to null
   - `IsRecording` returns false

4. **Playback** (Ctrl-X `e`):
   - Read `Macro` property
   - Feed each `KeyPress` back through key processor
   - Can be repeated with count prefix

### IsRecording Property

```csharp
public bool IsRecording => CurrentRecording != null;
```

Simple null check determines recording state.

### Reset Behavior

`Reset()` only clears `CurrentRecording`:
- Does NOT clear `Macro` (preserved for later playback)
- Stops any in-progress recording
- Called when returning to prompt, etc.

### KeyPress Storage

Stores full `KeyPress` objects:
- Includes `Keys` enum value
- Includes any text data
- Preserves exact sequence for playback

### Comparison with Vi Macros

| Feature | Emacs | Vi |
|---------|-------|-----|
| Registers | Single macro | Named registers (a-z) |
| Start | Ctrl-X ( | q{register} |
| Stop | Ctrl-X ) | q |
| Play | Ctrl-X e | @{register} |
| Storage | `Macro` property | `NamedRegisters` dict |

## Dependencies

- `Stroke.KeyBinding.KeyPress` (Feature 20) - Key press data

## Implementation Tasks

1. Implement `EmacsState` class
2. Implement `Macro` property
3. Implement `CurrentRecording` property
4. Implement `IsRecording` property
5. Implement `StartMacro()` method
6. Implement `EndMacro()` method
7. Implement `Reset()` method
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] EmacsState matches Python Prompt Toolkit semantics
- [ ] StartMacro initializes recording correctly
- [ ] EndMacro saves and clears recording
- [ ] IsRecording reflects recording state
- [ ] Reset stops recording but preserves Macro
- [ ] Macro stores list of KeyPress objects
- [ ] Unit tests achieve 80% coverage
