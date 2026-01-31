# Quickstart: Named Commands

**Feature**: 034-named-commands
**Date**: 2026-01-30

## Build Order

### Step 1: Core Registry (NamedCommands.cs)

Create `src/Stroke/KeyBinding/Bindings/NamedCommands.cs` with:
- `ConcurrentDictionary<string, Binding>` field
- `GetByName(string name)` — lookup with `KeyNotFoundException`
- `Register(string name, KeyHandlerCallable handler, bool recordInMacro)` — add/replace
- `RegisterInternal(string name, KeyHandlerCallable handler, bool recordInMacro)` — internal helper
- Static constructor calling `RegisterXxxCommands()` methods (empty initially)

**Verify**: Write `NamedCommandsRegistryTests` — test `GetByName` for known command, unknown command, null input, custom registration, override.

### Step 2: Movement Commands (NamedCommands.Movement.cs)

Implement 10 movement handlers:
- `beginning-of-buffer`, `end-of-buffer`, `beginning-of-line`, `end-of-line`
- `forward-char`, `backward-char`, `forward-word`, `backward-word`
- `clear-screen`, `redraw-current-line`

**Verify**: Write `NamedCommandsMovementTests` — test each command with known buffer state.

### Step 3: Text Modification Commands (NamedCommands.TextEdit.cs)

Implement 9 text editing handlers:
- `end-of-file`, `delete-char`, `backward-delete-char`, `self-insert`
- `transpose-chars`, `uppercase-word`, `downcase-word`, `capitalize-word`
- `quoted-insert`

**Verify**: Write `NamedCommandsTextEditTests` — test each with prepared buffer, including boundary conditions for `transpose-chars`.

### Step 4: Kill and Yank Commands (NamedCommands.KillYank.cs)

Implement 10 kill/yank handlers:
- `kill-line`, `kill-word`, `unix-word-rubout`, `backward-kill-word`
- `delete-horizontal-space`, `unix-line-discard`
- `yank`, `yank-nth-arg`, `yank-last-arg`, `yank-pop`

**Verify**: Write `NamedCommandsKillYankTests` — test kill/clipboard interaction, consecutive kill concatenation, yank-pop rotation.

### Step 5: History Commands (NamedCommands.History.cs)

Implement 6 history handlers:
- `accept-line`, `previous-history`, `next-history`
- `beginning-of-history`, `end-of-history`
- `reverse-search-history`

**Verify**: Write `NamedCommandsHistoryTests` — test with prepared history entries.

### Step 6: Completion Commands (NamedCommands.Completion.cs + CompletionBindings.cs)

Implement `CompletionBindings` static class with `GenerateCompletions` and `DisplayCompletionsLikeReadline`.
Implement 3 completion handlers:
- `complete`, `menu-complete`, `menu-complete-backward`

**Verify**: Write `NamedCommandsCompletionTests` — test completion delegation.

### Step 7: Macro Commands (NamedCommands.Macro.cs)

Implement 4 macro handlers:
- `start-kbd-macro`, `end-kbd-macro`, `call-last-kbd-macro`, `print-last-kbd-macro`

**Verify**: Write `NamedCommandsMacroTests` — test macro recording, replay, print.

### Step 8: Miscellaneous Commands (NamedCommands.Misc.cs)

Implement 7 miscellaneous handlers:
- `undo`, `insert-comment`, `vi-editing-mode`, `emacs-editing-mode`
- `prefix-meta`, `operate-and-get-next`, `edit-and-execute-command`

**Verify**: Write `NamedCommandsMiscTests` — test undo, comment/uncomment, mode switching.

### Step 9: Edge Case Tests (NamedCommandsEdgeCaseTests.cs)

Comprehensive boundary condition tests:
- Empty string/null lookups
- Movement at buffer boundaries (position 0, position = length)
- Kill on empty buffer
- Transpose at position 0
- Yank-pop without preceding yank
- Negative repeat counts
- Word case commands at end of buffer

## Dependencies Between Steps

```
Step 1 (Registry) ──→ Steps 2-8 (all command categories)
Steps 2-8 ──→ Step 9 (edge cases)
```

Steps 2 through 8 are independent of each other and can be implemented in any order after Step 1. Step 9 depends on all commands being registered.

## Key Implementation Patterns

### Creating a Binding from a handler

```csharp
private static void RegisterInternal(string name, KeyHandlerCallable handler, bool recordInMacro = true)
{
    var binding = new Binding(
        keys: [new KeyOrChar(Keys.Any)],
        handler: handler,
        recordInMacro: new FilterOrBool(recordInMacro));
    _commands[name] = binding;
}
```

### Accessing typed Application from handler

```csharp
private static NotImplementedOrNone? SomeHandler(KeyPressEvent @event)
{
    var app = @event.GetApp();  // Extension method → Application<object>
    var buff = @event.CurrentBuffer!;
    // Use buff.Document, app.Clipboard, etc.
    return null;
}
```

### Test pattern

```csharp
[Fact]
public void ForwardChar_MovesCursorRight()
{
    // Arrange
    var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
    var event = CreateEvent(buffer, arg: 1);

    // Act
    var binding = NamedCommands.GetByName("forward-char");
    binding.Call(event);

    // Assert
    Assert.Equal(1, buffer.CursorPosition);
}
```
