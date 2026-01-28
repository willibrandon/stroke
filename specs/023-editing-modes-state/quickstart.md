# Quickstart: Editing Modes and State

**Feature**: 023-editing-modes-state
**Date**: 2026-01-27

## Overview

This feature provides the foundational state management for Vi and Emacs editing modes in Stroke. It ports Python Prompt Toolkit's editing mode enums and state classes to C#, enabling modal editing support.

## Quick Reference

| Type | Purpose | Thread-Safe |
|------|---------|-------------|
| `EditingMode` | Enum: Vi or Emacs | N/A (enum) |
| `BufferNames` | String constants for buffer names | N/A (constants) |
| `InputMode` | Enum: Vi input states | N/A (enum) |
| `CharacterFind` | Immutable f/F/t/T search target | Inherent |
| `ViState` | Mutable Vi navigation state | Yes (Lock) |
| `EmacsState` | Mutable Emacs macro state | Yes (Lock) |

## Getting Started

### 1. Add Using Directive

```csharp
using Stroke.KeyBinding;
using Stroke.Input;      // For KeyPress (Emacs macros)
using Stroke.Clipboard;  // For ClipboardData (Vi registers)
```

### 2. Basic Vi Mode Usage

```csharp
// Create Vi state
var viState = new ViState();

// Check/set input mode
if (viState.InputMode == InputMode.Insert)
{
    // Handle insert mode key presses
}

// Transition to navigation mode (auto-clears operator state)
viState.InputMode = InputMode.Navigation;

// Use named registers for yanking/pasting
viState.SetNamedRegister("a", new ClipboardData("some text"));
var data = viState.GetNamedRegister("a");

// Reset to defaults
viState.Reset();
```

### 3. Basic Emacs Mode Usage

```csharp
// Create Emacs state
var emacsState = new EmacsState();

// Record a macro
emacsState.StartMacro();
emacsState.AppendToRecording(new KeyPress(Keys.ControlA));
emacsState.AppendToRecording(new KeyPress(Keys.ControlK));
emacsState.EndMacro();

// Check recording status
if (emacsState.IsRecording)
{
    // Currently recording...
}

// Retrieve recorded macro
foreach (var keyPress in emacsState.Macro)
{
    // Replay key presses
}
```

### 4. Character Find Operations (Vi)

```csharp
var viState = new ViState();

// Store a forward find for character 'x'
viState.LastCharacterFind = new CharacterFind("x");

// Store a backward find for character 'y'
viState.LastCharacterFind = new CharacterFind("y", Backwards: true);

// Use for repeat (;) command
var find = viState.LastCharacterFind;
if (find != null)
{
    SearchForCharacter(find.Character, find.Backwards);
}
```

## Common Patterns

### Mode-Conditional Key Bindings

```csharp
// In key binding handler
public NotImplementedOrNone HandleKey(KeyPressEvent e, ViState viState)
{
    return viState.InputMode switch
    {
        InputMode.Insert => HandleInsertMode(e),
        InputMode.Navigation => HandleNavigationMode(e),
        InputMode.Replace => HandleReplaceMode(e),
        _ => NotImplementedOrNone.NotImplemented
    };
}
```

### Vi Operator-Pending Mode

```csharp
var viState = new ViState();

// Set pending operator (e.g., after pressing 'd')
viState.OperatorFunc = (e, textObject) =>
{
    // Delete the text object
    return NotImplementedOrNone.None;
};
viState.OperatorArg = 2; // e.g., "2dw" deletes 2 words

// When transitioning to Navigation mode, operator state is cleared
viState.InputMode = InputMode.Navigation;
// viState.OperatorFunc is now null
```

### Editing Mode Selection

```csharp
public void ConfigureApplication(EditingMode mode)
{
    if (mode == EditingMode.Vi)
    {
        // Load Vi key bindings
    }
    else if (mode == EditingMode.Emacs)
    {
        // Load Emacs key bindings
    }
}
```

## Thread Safety Notes

1. **ViState and EmacsState**: All property getters/setters are thread-safe. Individual operations are atomic.

2. **Compound operations** require external synchronization:
   ```csharp
   // UNSAFE - race condition possible
   if (viState.OperatorFunc != null)
   {
       viState.OperatorFunc(...); // May be null by now
   }

   // SAFE - use local copy
   var op = viState.OperatorFunc;
   if (op != null)
   {
       op(...);
   }
   ```

3. **CharacterFind**: Immutable record - inherently thread-safe.

4. **Enums and constants**: Inherently thread-safe.

## Dependencies

- `Stroke.Input.KeyPress` - Used by EmacsState for macro storage
- `Stroke.Clipboard.ClipboardData` - Used by ViState for named registers
- `Stroke.KeyBinding.KeyPressEvent` - Used by OperatorFuncDelegate
- `Stroke.KeyBinding.NotImplementedOrNone` - Used by OperatorFuncDelegate

## Related Features

| Feature | Relationship |
|---------|--------------|
| Key Bindings (022) | Uses state to determine active bindings |
| Input System (014) | Provides KeyPress for macros |
| Clipboard (004) | Provides ClipboardData for registers |
| Filter System (017) | Will use state for mode filters (InViMode, etc.) |

## Python Prompt Toolkit Reference

| C# Type | Python Source |
|---------|---------------|
| `EditingMode` | `prompt_toolkit/enums.py:EditingMode` |
| `BufferNames` | `prompt_toolkit/enums.py` (constants) |
| `InputMode` | `prompt_toolkit/key_binding/vi_state.py:InputMode` |
| `CharacterFind` | `prompt_toolkit/key_binding/vi_state.py:CharacterFind` |
| `ViState` | `prompt_toolkit/key_binding/vi_state.py:ViState` |
| `EmacsState` | `prompt_toolkit/key_binding/emacs_state.py:EmacsState` |
