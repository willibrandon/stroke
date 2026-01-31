# Contract: BasicBindings

**Feature**: 037-basic-key-bindings
**Date**: 2026-01-30
**Python Source**: `prompt_toolkit/key_binding/bindings/basic.py`

## Public API

```csharp
namespace Stroke.Application.Bindings;

/// <summary>
/// Key binding loader for basic key bindings shared between Emacs and Vi modes.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.basic</c> module.
/// Provides a single factory method that creates and returns a <see cref="KeyBindings"/>
/// instance containing all basic key bindings: ignored keys, readline movement/editing,
/// self-insert, tab completion, history navigation, auto up/down, selection delete,
/// Ctrl+D, Enter multiline, Ctrl+J re-dispatch, Ctrl+Z literal, bracketed paste,
/// and quoted insert.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. The factory method creates a
/// new <see cref="KeyBindings"/> instance on each call.
/// </para>
/// </remarks>
public static class BasicBindings
{
    /// <summary>
    /// Load basic key bindings shared by Emacs and Vi modes.
    /// </summary>
    /// <returns>
    /// A new <see cref="KeyBindings"/> instance containing all basic key bindings.
    /// </returns>
    public static KeyBindings LoadBasicBindings();
}
```

## Binding Registration Order

The method registers bindings in the following order, matching the Python source exactly:

### 1. Ignored Keys (90 bindings)

No-op handlers that prevent keys from falling through to the `Any` self-insert handler.

```csharp
// All 26 control keys: Ctrl+A through Ctrl+Z
// 24 function keys: F1 through F24
// 5 control-punctuation keys: Ctrl+@, Ctrl+\, Ctrl+], Ctrl+^, Ctrl+_
// Navigation keys: Backspace, Up, Down, Right, Left
// Shift+navigation: Shift+Up/Down/Right/Left, Shift+Home/End, Shift+Delete
// Home, End, Delete, PageUp, PageDown, Tab, Shift+Tab
// Ctrl+navigation: Ctrl+Left/Right/Up/Down/Home/End/Delete
// Ctrl+Shift+navigation: Ctrl+Shift+Left/Right/Home/End
// Insert, Shift+Insert, Ctrl+Insert
// SIGINT, Ignore
```

### 2. Readline Movement Bindings (7 bindings, no filter)

```csharp
// Home → beginning-of-line
// End → end-of-line
// Left → backward-char
// Right → forward-char
// Ctrl+Up → previous-history
// Ctrl+Down → next-history
// Ctrl+L → clear-screen
```

### 3. Readline Editing Bindings (7 bindings, filter: InsertMode)

```csharp
// Ctrl+K → kill-line
// Ctrl+U → unix-line-discard
// Backspace → backward-delete-char (saveBefore: IfNoRepeat)
// Delete → delete-char (saveBefore: IfNoRepeat)
// Ctrl+Delete → delete-char (saveBefore: IfNoRepeat)
// Ctrl+T → transpose-chars
// Ctrl+W → unix-word-rubout
```

### 4. Self-Insert (1 binding, filter: InsertMode, saveBefore: IfNoRepeat)

```csharp
// Keys.Any → self-insert
```

### 5. Tab Completion (2 bindings, filter: InsertMode)

```csharp
// Ctrl+I (Tab) → menu-complete
// Shift+Tab → menu-complete-backward
```

### 6. History Navigation (2 bindings, filter: ~HasSelection)

```csharp
// PageUp → previous-history
// PageDown → next-history
```

### 7. Ctrl+D (1 binding, filter: HasTextBeforeCursor & InsertMode)

```csharp
// Ctrl+D → delete-char
```

### 8. Enter Multiline (1 binding, filter: InsertMode & IsMultiline)

```csharp
// Enter (Ctrl+M) → inline handler: buffer.Newline(copyMargin: !InPasteMode)
```

### 9. Ctrl+J Re-dispatch (1 binding, no filter)

```csharp
// Ctrl+J → inline handler: keyProcessor.Feed(KeyPress(ControlM, "\r"), first: true)
```

### 10. Auto Up/Down (2 bindings, no filter)

```csharp
// Up → inline handler: buffer.AutoUp(count: event.Arg)
// Down → inline handler: buffer.AutoDown(count: event.Arg)
```

### 11. Delete Selection (1 binding, filter: HasSelection)

```csharp
// Delete → inline handler: buffer.CutSelection() → clipboard.SetData()
```

### 12. Ctrl+Z (1 binding, no filter)

```csharp
// Ctrl+Z → inline handler: buffer.InsertText(event.Data)
```

### 13. Bracketed Paste (1 binding, no filter)

```csharp
// Keys.BracketedPaste → inline handler: normalize \r\n/\r to \n, then InsertText
```

### 14. Quoted Insert (1 binding, filter: InQuotedInsert, eager: true)

```csharp
// Keys.Any → inline handler: buffer.InsertText(event.Data, overwrite: false),
//            then app.QuotedInsert = false
```

## Private Members

```csharp
// Composite filter: Vi insert mode OR Emacs insert mode
private static readonly IFilter InsertMode;

// Dynamic condition: current buffer has text
private static readonly IFilter HasTextBeforeCursor;

// Dynamic condition: quoted insert mode is active
private static readonly IFilter InQuotedInsert;

// Save-before callback: returns false for repeated events
private static bool IfNoRepeat(KeyPressEvent @event);

// Shared no-op handler for ignored keys
private static NotImplementedOrNone? Ignore(KeyPressEvent @event);
```

## Filter Compositions Used

| Filter Expression | Used By | Python Equivalent |
|-------------------|---------|-------------------|
| `InsertMode` (Vi insert OR Emacs insert) | Editing, self-insert, tab, Ctrl+D, Enter | `insert_mode` |
| `~HasSelection` | PageUp/PageDown history nav | `~has_selection` |
| `HasTextBeforeCursor & InsertMode` | Ctrl+D | `has_text_before_cursor & insert_mode` |
| `InsertMode & IsMultiline` | Enter multiline | `insert_mode & is_multiline` |
| `HasSelection` | Delete selection | `has_selection` |
| `InQuotedInsert` (eager) | Quoted insert | `in_quoted_insert` |
