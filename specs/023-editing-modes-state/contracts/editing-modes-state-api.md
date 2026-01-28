# API Contracts: Editing Modes and State

**Feature**: 023-editing-modes-state
**Date**: 2026-01-27

## Overview

This document defines the public API contracts for the editing modes and state feature. All types are in the `Stroke.KeyBinding` namespace.

---

## EditingMode Enum

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Represents the active key binding set.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>EditingMode</c> enum from <c>prompt_toolkit.enums</c>.
/// </para>
/// </remarks>
public enum EditingMode
{
    /// <summary>Vi-style modal editing.</summary>
    Vi,

    /// <summary>Emacs-style chord-based editing.</summary>
    Emacs
}
```

---

## BufferNames Static Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Standard names for well-known buffers.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's buffer name constants from <c>prompt_toolkit.enums</c>.
/// </para>
/// </remarks>
public static class BufferNames
{
    /// <summary>Name of the search buffer.</summary>
    public const string SearchBuffer = "SEARCH_BUFFER";

    /// <summary>Name of the default buffer.</summary>
    public const string DefaultBuffer = "DEFAULT_BUFFER";

    /// <summary>Name of the system buffer.</summary>
    public const string SystemBuffer = "SYSTEM_BUFFER";
}
```

---

## InputMode Enum

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Represents Vi input mode states.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>InputMode</c> enum from <c>prompt_toolkit.key_binding.vi_state</c>.
/// </para>
/// </remarks>
public enum InputMode
{
    /// <summary>Normal text insertion mode.</summary>
    Insert,

    /// <summary>Insert mode for multiple cursors.</summary>
    InsertMultiple,

    /// <summary>Vi normal mode for navigation and commands.</summary>
    Navigation,

    /// <summary>Overwrite mode (like Vi 'R' command).</summary>
    Replace,

    /// <summary>Replace single character (like Vi 'r' command).</summary>
    ReplaceSingle
}
```

---

## CharacterFind Record

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Stores the target character and direction for Vi character find operations (f/F/t/T commands).
/// </summary>
/// <remarks>
/// <para>
/// This type is immutable and inherently thread-safe.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>CharacterFind</c> class from <c>prompt_toolkit.key_binding.vi_state</c>.
/// </para>
/// </remarks>
/// <param name="Character">The target character to find.</param>
/// <param name="Backwards">True for backwards search (F/T), false for forwards search (f/t).</param>
public sealed record CharacterFind(string Character, bool Backwards = false);
```

---

## OperatorFuncDelegate

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Callback signature for pending Vi operator functions.
/// </summary>
/// <param name="e">The key press event that completes the operator.</param>
/// <param name="textObject">The text object (placeholder type until ITextObject is defined).</param>
/// <returns><see cref="NotImplementedOrNone"/> indicating if the event was handled.</returns>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's operator function signature: <c>Callable[[KeyPressEvent, TextObject], None]</c>.
/// </para>
/// </remarks>
public delegate NotImplementedOrNone OperatorFuncDelegate(KeyPressEvent e, object? textObject);
```

---

## ViState Class

```csharp
namespace Stroke.KeyBinding;

using Stroke.Clipboard;

/// <summary>
/// Mutable class to hold Vi navigation state.
/// </summary>
/// <remarks>
/// <para>
/// Thread safety: All property access is thread-safe. Individual operations are atomic.
/// Compound operations (read-modify-write sequences) require external synchronization.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>ViState</c> class from <c>prompt_toolkit.key_binding.vi_state</c>.
/// </para>
/// </remarks>
public sealed class ViState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViState"/> class with default values.
    /// </summary>
    public ViState();

    /// <summary>
    /// Gets or sets the current Vi input mode.
    /// </summary>
    /// <remarks>
    /// When set to <see cref="InputMode.Navigation"/>, automatically clears:
    /// <see cref="WaitingForDigraph"/>, <see cref="OperatorFunc"/>, and <see cref="OperatorArg"/>.
    /// </remarks>
    public InputMode InputMode { get; set; }

    /// <summary>
    /// Gets or sets the last character find operation for repeat (;) and reverse (,) commands.
    /// </summary>
    public CharacterFind? LastCharacterFind { get; set; }

    /// <summary>
    /// Gets or sets the pending operator function callback.
    /// </summary>
    /// <remarks>
    /// Set when waiting for a text object after an operator command (e.g., after 'd' in 'dw').
    /// </remarks>
    public OperatorFuncDelegate? OperatorFunc { get; set; }

    /// <summary>
    /// Gets or sets the count argument for the pending operator.
    /// </summary>
    public int? OperatorArg { get; set; }

    /// <summary>
    /// Gets or sets whether the editor is waiting for the second digraph character.
    /// </summary>
    public bool WaitingForDigraph { get; set; }

    /// <summary>
    /// Gets or sets the first digraph symbol when <see cref="WaitingForDigraph"/> is true.
    /// </summary>
    public string? DigraphSymbol1 { get; set; }

    /// <summary>
    /// Gets or sets whether tilde (~) acts as an operator.
    /// </summary>
    public bool TildeOperator { get; set; }

    /// <summary>
    /// Gets or sets the register being recorded to, or null if not recording.
    /// </summary>
    public string? RecordingRegister { get; set; }

    /// <summary>
    /// Gets or sets the accumulated macro content during recording.
    /// </summary>
    public string CurrentRecording { get; set; }

    /// <summary>
    /// Gets or sets whether temporary navigation mode is active (Ctrl+O in insert mode).
    /// </summary>
    public bool TemporaryNavigationMode { get; set; }

    /// <summary>
    /// Gets the value of a named register.
    /// </summary>
    /// <param name="name">The register name (typically a single character a-z).</param>
    /// <returns>The clipboard data stored in the register, or null if not set.</returns>
    public ClipboardData? GetNamedRegister(string name);

    /// <summary>
    /// Sets the value of a named register.
    /// </summary>
    /// <param name="name">The register name (typically a single character a-z).</param>
    /// <param name="data">The clipboard data to store.</param>
    public void SetNamedRegister(string name, ClipboardData data);

    /// <summary>
    /// Clears a named register.
    /// </summary>
    /// <param name="name">The register name to clear.</param>
    /// <returns>True if the register was present and removed; otherwise, false.</returns>
    public bool ClearNamedRegister(string name);

    /// <summary>
    /// Gets all named register names currently set.
    /// </summary>
    /// <returns>A collection of register names.</returns>
    public IReadOnlyCollection<string> GetNamedRegisterNames();

    /// <summary>
    /// Resets the Vi state to initial values.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="InputMode"/> to <see cref="InputMode.Insert"/> and clears
    /// <see cref="WaitingForDigraph"/>, <see cref="OperatorFunc"/>, <see cref="OperatorArg"/>,
    /// <see cref="RecordingRegister"/>, and <see cref="CurrentRecording"/>.
    /// Does not clear <see cref="NamedRegisters"/> or <see cref="LastCharacterFind"/>.
    /// </remarks>
    public void Reset();
}
```

---

## EmacsState Class

```csharp
namespace Stroke.KeyBinding;

using Stroke.Input;

/// <summary>
/// Mutable class to hold Emacs-specific state.
/// </summary>
/// <remarks>
/// <para>
/// Thread safety: All property access is thread-safe. Individual operations are atomic.
/// Compound operations (read-modify-write sequences) require external synchronization.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>EmacsState</c> class from <c>prompt_toolkit.key_binding.emacs_state</c>.
/// </para>
/// </remarks>
public sealed class EmacsState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmacsState"/> class with default values.
    /// </summary>
    public EmacsState();

    /// <summary>
    /// Gets the last recorded macro, or an empty list if none.
    /// </summary>
    /// <remarks>
    /// Returns a copy of the internal list to ensure thread safety.
    /// </remarks>
    public IReadOnlyList<KeyPress> Macro { get; }

    /// <summary>
    /// Gets the in-progress macro recording, or null if not recording.
    /// </summary>
    /// <remarks>
    /// Returns a copy of the internal list to ensure thread safety.
    /// </remarks>
    public IReadOnlyList<KeyPress>? CurrentRecording { get; }

    /// <summary>
    /// Gets whether a macro is currently being recorded.
    /// </summary>
    public bool IsRecording { get; }

    /// <summary>
    /// Starts recording a new macro.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="CurrentRecording"/> to a new empty list.
    /// </remarks>
    public void StartMacro();

    /// <summary>
    /// Ends macro recording.
    /// </summary>
    /// <remarks>
    /// Copies <see cref="CurrentRecording"/> to <see cref="Macro"/> and sets <see cref="CurrentRecording"/> to null.
    /// If not recording, sets <see cref="Macro"/> to null.
    /// </remarks>
    public void EndMacro();

    /// <summary>
    /// Appends a key press to the current recording.
    /// </summary>
    /// <param name="keyPress">The key press to append.</param>
    /// <remarks>
    /// Does nothing if not currently recording.
    /// </remarks>
    public void AppendToRecording(KeyPress keyPress);

    /// <summary>
    /// Resets the Emacs state.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="CurrentRecording"/> to null. Does not clear <see cref="Macro"/>.
    /// </remarks>
    public void Reset();
}
```

---

## Usage Examples

### Checking Editing Mode

```csharp
var mode = EditingMode.Vi;
if (mode == EditingMode.Vi)
{
    // Apply Vi key bindings
}
```

### Using Buffer Names

```csharp
var searchBuffer = application.GetBuffer(BufferNames.SearchBuffer);
var defaultBuffer = application.GetBuffer(BufferNames.DefaultBuffer);
```

### Vi State Management

```csharp
var viState = new ViState();

// Check initial mode
Debug.Assert(viState.InputMode == InputMode.Insert);

// Transition to Navigation mode (clears operator state)
viState.OperatorFunc = (e, obj) => NotImplementedOrNone.None;
viState.InputMode = InputMode.Navigation;
Debug.Assert(viState.OperatorFunc == null); // Automatically cleared

// Use named registers
viState.SetNamedRegister("a", new ClipboardData("yanked text"));
var data = viState.GetNamedRegister("a");

// Reset state
viState.Reset();
Debug.Assert(viState.InputMode == InputMode.Insert);
```

### Emacs Macro Recording

```csharp
var emacsState = new EmacsState();

// Start recording
emacsState.StartMacro();
Debug.Assert(emacsState.IsRecording);

// Record key presses
emacsState.AppendToRecording(new KeyPress(Keys.ControlA));
emacsState.AppendToRecording(new KeyPress(Keys.ControlE));

// End recording
emacsState.EndMacro();
Debug.Assert(!emacsState.IsRecording);
Debug.Assert(emacsState.Macro.Count == 2);
```

### Character Find Operations

```csharp
var viState = new ViState();

// Record a forward find for 'x'
viState.LastCharacterFind = new CharacterFind("x", Backwards: false);

// Later, repeat the find
var find = viState.LastCharacterFind;
if (find != null)
{
    // Use find.Character and find.Backwards to repeat the search
}
```
