# Quickstart: Key Bindings System

**Feature**: 022-key-bindings-system
**Namespace**: `Stroke.KeyBinding`

## Overview

The Key Bindings System provides a registry for associating keyboard input sequences with handler functions. It supports:

- Single and multi-key sequences
- Conditional activation via filters
- Eager matching for immediate execution
- Global bindings for application-wide shortcuts
- Registry composition and merging

## Basic Usage

### Creating a KeyBindings Registry

```csharp
using Stroke.KeyBinding;
using Stroke.Input;

var kb = new KeyBindings();

// Add a single key binding
kb.Add(new[] { (KeyOrChar)Keys.ControlC })(HandleControlC);

// Add a multi-key sequence
kb.Add(new[] { (KeyOrChar)Keys.ControlX, (KeyOrChar)Keys.ControlC })(HandleExit);

// Handler function
NotImplementedOrNone? HandleControlC(KeyPressEvent e)
{
    Console.WriteLine("Ctrl+C pressed!");
    return NotImplementedOrNone.None; // Event was handled
}

NotImplementedOrNone? HandleExit(KeyPressEvent e)
{
    e.App.Exit();
    return NotImplementedOrNone.None;
}
```

### Using Character Keys

```csharp
// Bind to a specific character
kb.Add(new[] { (KeyOrChar)'a' })(HandleLetterA);

// Using KeyBindingUtils.ParseKey for string notation
kb.Add(new[] { KeyBindingUtils.ParseKey("c-x") })(HandleControlX);
```

## Conditional Bindings with Filters

### Simple Filter

```csharp
using Stroke.Filters;

// Binding only active when filter returns true
var isEditMode = new Condition(() => _editMode);

kb.Add(
    new[] { (KeyOrChar)Keys.Escape },
    filter: isEditMode
)(ExitEditMode);
```

### Conditional Wrapper

```csharp
// Apply filter to all bindings in a registry
var editBindings = new KeyBindings();
editBindings.Add(new[] { (KeyOrChar)Keys.ControlS })(Save);
editBindings.Add(new[] { (KeyOrChar)Keys.ControlZ })(Undo);

var conditional = new ConditionalKeyBindings(editBindings, isEditMode);
```

## Eager Matching

Use eager matching when a key should execute immediately, even if it's a prefix of longer sequences:

```csharp
// Without eager: Pressing Escape waits to see if more keys follow
// With eager: Escape executes immediately

kb.Add(
    new[] { (KeyOrChar)Keys.Escape },
    eager: true  // Execute immediately
)(HandleEscape);

// This longer binding still works when user types Escape followed by 'j'
kb.Add(
    new[] { (KeyOrChar)Keys.Escape, (KeyOrChar)'j' }
)(HandleEscapeJ);
```

## Global Bindings

Global bindings remain active regardless of UI focus:

```csharp
kb.Add(
    new[] { (KeyOrChar)Keys.ControlQ },
    isGlobal: true
)(Quit);

// Filter to get only global bindings
var globalOnly = new GlobalOnlyKeyBindings(kb);
```

## Merging Registries

Combine bindings from multiple sources:

```csharp
var emacsBindings = CreateEmacsBindings();
var customBindings = CreateCustomBindings();

// Using static method
var merged = KeyBindingUtils.Merge(emacsBindings, customBindings);

// Using extension method
var merged2 = emacsBindings.Merge(customBindings);
```

## Dynamic Bindings

Switch binding sets at runtime:

```csharp
IKeyBindingsBase? GetCurrentBindings() =>
    _viMode ? _viBindings : _emacsBindings;

var dynamic = new DynamicKeyBindings(GetCurrentBindings);
// Bindings change when _viMode changes
```

## Save Before Handler

Control undo stack behavior:

```csharp
kb.Add(
    new[] { (KeyOrChar)Keys.Delete },
    saveBefore: e => true  // Save buffer state before executing
)(HandleDelete);

kb.Add(
    new[] { (KeyOrChar)Keys.ControlG },
    saveBefore: e => false  // Don't save state (e.g., for cancel)
)(CancelOperation);
```

## Macro Recording

Control whether keystrokes are recorded:

```csharp
kb.Add(
    new[] { (KeyOrChar)Keys.ControlX, (KeyOrChar)'(' },
    recordInMacro: false  // Don't record "start recording" command
)(StartMacro);
```

## Removing Bindings

```csharp
// Remove by handler reference
kb.Remove(HandleControlC);

// Remove by key sequence
kb.Remove(Keys.ControlX, Keys.ControlC);
```

## Querying Bindings

```csharp
// Get exact matches for a key sequence
var matches = kb.GetBindingsForKeys(new[] { (KeyOrChar)Keys.ControlX });

// Check each binding's filter to see which is active
foreach (var binding in matches)
{
    if (binding.Filter.Invoke())
    {
        // This binding is currently active
    }
}

// Get bindings that start with this prefix (for timeout detection)
var prefixMatches = kb.GetBindingsStartingWithKeys(
    new[] { (KeyOrChar)Keys.ControlX }
);
```

## Using the Key Binding Decorator

Pre-configure binding options:

```csharp
var decorateAsGlobal = KeyBindingDecorator.Create(isGlobal: true);

var binding = decorateAsGlobal(HandleQuit);
kb.Add(new[] { (KeyOrChar)Keys.ControlQ })(binding);
```

## Complete Example

```csharp
using Stroke.KeyBinding;
using Stroke.Input;
using Stroke.Filters;

public class MyApplication
{
    private readonly KeyBindings _bindings = new();
    private bool _insertMode = false;

    public MyApplication()
    {
        // Global quit command
        _bindings.Add(
            new[] { (KeyOrChar)Keys.ControlQ },
            isGlobal: true
        )(Quit);

        // Mode-specific bindings
        var insertFilter = new Condition(() => _insertMode);
        var normalFilter = new Condition(() => !_insertMode);

        // Insert mode: Escape exits
        _bindings.Add(
            new[] { (KeyOrChar)Keys.Escape },
            filter: insertFilter,
            eager: true
        )(ExitInsertMode);

        // Normal mode: i enters insert mode
        _bindings.Add(
            new[] { (KeyOrChar)'i' },
            filter: normalFilter
        )(EnterInsertMode);

        // Normal mode: dd deletes line
        _bindings.Add(
            new[] { (KeyOrChar)'d', (KeyOrChar)'d' },
            filter: normalFilter,
            saveBefore: _ => true
        )(DeleteLine);
    }

    private NotImplementedOrNone? Quit(KeyPressEvent e)
    {
        e.App.Exit();
        return NotImplementedOrNone.None;
    }

    private NotImplementedOrNone? ExitInsertMode(KeyPressEvent e)
    {
        _insertMode = false;
        return NotImplementedOrNone.None;
    }

    private NotImplementedOrNone? EnterInsertMode(KeyPressEvent e)
    {
        _insertMode = true;
        return NotImplementedOrNone.None;
    }

    private NotImplementedOrNone? DeleteLine(KeyPressEvent e)
    {
        // Delete current line with arg repetition
        for (int i = 0; i < e.Arg; i++)
        {
            e.CurrentBuffer.DeleteCurrentLine();
        }
        return NotImplementedOrNone.None;
    }
}
```

## Thread Safety

All key binding classes are thread-safe:

- `KeyBindings`: Uses internal lock for add/remove operations
- `ConditionalKeyBindings`, `MergedKeyBindings`, `DynamicKeyBindings`, `GlobalOnlyKeyBindings`: Use internal lock for cache updates
- `Binding`, `KeyPress`, `KeyOrChar`: Immutable and inherently thread-safe

Concurrent reads are always safe. Writes (add/remove) are serialized per registry.

## Performance Tips

1. **Use filters wisely**: Filters are evaluated on every key press. Keep filter logic fast.

2. **Prefer merged registries**: Use `MergedKeyBindings` instead of adding bindings one-by-one from multiple sources.

3. **Cache is automatic**: The system caches binding lookups. No manual cache management needed.

4. **Avoid frequent add/remove**: Each modification invalidates the cache. Batch changes when possible.
