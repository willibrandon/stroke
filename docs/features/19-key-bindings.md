# Feature 19: Key Bindings System

## Overview

Implement the key bindings registry for associating key sequences with handlers, including support for filters, eager matching, and binding composition.

## Python Prompt Toolkit Reference

**Sources:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/key_bindings.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/__init__.py`

## Public API

### Binding Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Key binding: (key sequence + handler + filter).
/// Immutable binding class.
/// </summary>
public sealed class Binding
{
    /// <summary>
    /// Creates a key binding.
    /// </summary>
    /// <param name="keys">Tuple of keys in the sequence.</param>
    /// <param name="handler">The handler to call when the key sequence is pressed.</param>
    /// <param name="filter">Filter to determine when this binding is active.</param>
    /// <param name="eager">When true, ignore potential longer matches.</param>
    /// <param name="isGlobal">When true, this is a global (always active) binding.</param>
    /// <param name="saveBefore">Callable that returns true if we should save the buffer before handling.</param>
    /// <param name="recordInMacro">When true, record this binding when a macro is recorded.</param>
    public Binding(
        IReadOnlyList<object> keys, // Keys enum or string
        Func<KeyPressEvent, object?> handler,
        IFilter? filter = null,
        IFilter? eager = null,
        IFilter? isGlobal = null,
        Func<KeyPressEvent, bool>? saveBefore = null,
        IFilter? recordInMacro = null);

    /// <summary>
    /// The keys in the sequence.
    /// </summary>
    public IReadOnlyList<object> Keys { get; }

    /// <summary>
    /// The handler function.
    /// </summary>
    public Func<KeyPressEvent, object?> Handler { get; }

    /// <summary>
    /// Filter for when this binding is active.
    /// </summary>
    public IFilter Filter { get; }

    /// <summary>
    /// Filter for eager matching.
    /// </summary>
    public IFilter Eager { get; }

    /// <summary>
    /// Filter for global binding.
    /// </summary>
    public IFilter IsGlobal { get; }

    /// <summary>
    /// Callable that returns true if we should save before handling.
    /// </summary>
    public Func<KeyPressEvent, bool> SaveBefore { get; }

    /// <summary>
    /// Filter for recording in macro.
    /// </summary>
    public IFilter RecordInMacro { get; }

    /// <summary>
    /// Call the handler.
    /// </summary>
    public void Call(KeyPressEvent @event);
}
```

### IKeyBindingsBase Interface

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Interface for a KeyBindings.
/// </summary>
public interface IKeyBindingsBase
{
    /// <summary>
    /// For cache invalidation. This should increase every time something changes.
    /// </summary>
    object Version { get; }

    /// <summary>
    /// Return a list of key bindings that can handle these keys.
    /// This returns inactive bindings too; the filter still has to be called.
    /// </summary>
    IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<object> keys);

    /// <summary>
    /// Return a list of key bindings that handle a key sequence starting with keys.
    /// Returns only bindings with sequences longer than keys.
    /// </summary>
    IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<object> keys);

    /// <summary>
    /// List of all Binding objects.
    /// </summary>
    IReadOnlyList<Binding> Bindings { get; }
}
```

### KeyBindings Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// A container for a set of key bindings.
/// </summary>
public sealed class KeyBindings : IKeyBindingsBase
{
    public KeyBindings();

    public object Version { get; }
    public IReadOnlyList<Binding> Bindings { get; }

    /// <summary>
    /// Add a key binding.
    /// Returns a decorator that can be used to add the binding.
    /// </summary>
    /// <param name="keys">The key sequence.</param>
    /// <param name="filter">Filter to determine when this binding is active.</param>
    /// <param name="eager">When true, ignore potential longer matches.</param>
    /// <param name="isGlobal">When true, this is a global binding.</param>
    /// <param name="saveBefore">Callable that returns true if we should save before handling.</param>
    /// <param name="recordInMacro">When true, record this binding when a macro is recorded.</param>
    public Action<Func<KeyPressEvent, object?>> Add(
        Keys[] keys,
        IFilter? filter = null,
        IFilter? eager = null,
        IFilter? isGlobal = null,
        Func<KeyPressEvent, bool>? saveBefore = null,
        IFilter? recordInMacro = null);

    /// <summary>
    /// Add a key binding with string keys.
    /// </summary>
    public Action<Func<KeyPressEvent, object?>> Add(
        string[] keys,
        IFilter? filter = null,
        IFilter? eager = null,
        IFilter? isGlobal = null,
        Func<KeyPressEvent, bool>? saveBefore = null,
        IFilter? recordInMacro = null);

    /// <summary>
    /// Add a Binding directly.
    /// </summary>
    public void Add(Binding binding);

    /// <summary>
    /// Remove a key binding by handler.
    /// </summary>
    public void Remove(Func<KeyPressEvent, object?> handler);

    /// <summary>
    /// Remove a key binding by key sequence.
    /// </summary>
    public void Remove(params object[] keys);

    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<object> keys);
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<object> keys);
}
```

### ConditionalKeyBindings Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Wraps around a KeyBindings. Disable/enable all key bindings according to
/// the given (additional) filter.
/// </summary>
public sealed class ConditionalKeyBindings : IKeyBindingsBase
{
    /// <summary>
    /// Creates conditional key bindings.
    /// </summary>
    /// <param name="keyBindings">The key bindings to wrap.</param>
    /// <param name="filter">The filter to apply.</param>
    public ConditionalKeyBindings(IKeyBindingsBase keyBindings, IFilter? filter = null);

    /// <summary>
    /// The wrapped key bindings.
    /// </summary>
    public IKeyBindingsBase KeyBindings { get; }

    /// <summary>
    /// The filter applied to all bindings.
    /// </summary>
    public IFilter Filter { get; }

    public object Version { get; }
    public IReadOnlyList<Binding> Bindings { get; }
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<object> keys);
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<object> keys);
}
```

### DynamicKeyBindings Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// KeyBindings class that can dynamically return any KeyBindings.
/// </summary>
public sealed class DynamicKeyBindings : IKeyBindingsBase
{
    /// <summary>
    /// Creates dynamic key bindings.
    /// </summary>
    /// <param name="getKeyBindings">Callable that returns a KeyBindings instance.</param>
    public DynamicKeyBindings(Func<IKeyBindingsBase?> getKeyBindings);

    /// <summary>
    /// The callable that returns key bindings.
    /// </summary>
    public Func<IKeyBindingsBase?> GetKeyBindings { get; }

    public object Version { get; }
    public IReadOnlyList<Binding> Bindings { get; }
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<object> keys);
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<object> keys);
}
```

### GlobalOnlyKeyBindings Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Wrapper around a KeyBindings object that only exposes the global key bindings.
/// </summary>
public sealed class GlobalOnlyKeyBindings : IKeyBindingsBase
{
    public GlobalOnlyKeyBindings(IKeyBindingsBase keyBindings);

    public IKeyBindingsBase KeyBindings { get; }

    public object Version { get; }
    public IReadOnlyList<Binding> Bindings { get; }
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<object> keys);
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<object> keys);
}
```

### MergeKeyBindings Function

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Key binding utilities.
/// </summary>
public static class KeyBindingUtils
{
    /// <summary>
    /// Merge multiple KeyBindings objects together.
    /// </summary>
    public static IKeyBindingsBase MergeKeyBindings(IEnumerable<IKeyBindingsBase> bindings);

    /// <summary>
    /// Parse a key string and return the Keys enum or character.
    /// </summary>
    public static object ParseKey(string key);
}
```

### KeyBindingAttribute

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Attribute that turns a method into a Binding object.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class KeyBindingAttribute : Attribute
{
    public KeyBindingAttribute();

    public bool Filter { get; set; }
    public bool Eager { get; set; }
    public bool IsGlobal { get; set; }
    public bool RecordInMacro { get; set; }
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    ├── Binding.cs
    ├── IKeyBindingsBase.cs
    ├── KeyBindings.cs
    ├── ConditionalKeyBindings.cs
    ├── DynamicKeyBindings.cs
    ├── GlobalOnlyKeyBindings.cs
    ├── MergedKeyBindings.cs (internal)
    ├── KeyBindingUtils.cs
    └── KeyBindingAttribute.cs
tests/Stroke.Tests/
└── KeyBinding/
    ├── BindingTests.cs
    ├── KeyBindingsTests.cs
    ├── ConditionalKeyBindingsTests.cs
    ├── DynamicKeyBindingsTests.cs
    ├── GlobalOnlyKeyBindingsTests.cs
    ├── MergeKeyBindingsTests.cs
    └── KeyBindingUtilsTests.cs
```

## Implementation Notes

### Key Matching Algorithm

The key processor uses a prefix-matching algorithm:
1. Collect key presses into a buffer
2. Check if buffer matches any binding exactly
3. Check if buffer is a prefix of any longer binding
4. If exact match and no longer matches (or eager), call handler
5. If no match and no prefix, shift buffer and retry

### Any Key Matching

The `Keys.Any` key matches any single key press. Bindings with fewer `Any` occurrences have higher priority.

### Binding Caching

The `KeyBindings` class caches:
- `GetBindingsForKeys` results (up to 10,000 entries)
- `GetBindingsStartingWithKeys` results (up to 1,000 entries)

Caches are invalidated when bindings change (version increments).

### Filter Composition

When adding bindings through `ConditionalKeyBindings`, the filter is composed with AND:
```csharp
newBinding.Filter = conditionalFilter & originalBinding.Filter
```

### Eager and Global Composition

When merging bindings, eager and global flags are composed with OR:
```csharp
newBinding.Eager = eager | originalBinding.Eager
newBinding.IsGlobal = isGlobal | originalBinding.IsGlobal
```

### Async Handlers

Handlers can return `Task` or be async. The result is awaited in a background task, and the UI is invalidated after completion (unless `NotImplemented` is returned).

## Dependencies

- `Stroke.Input.Keys` (Feature 10) - Key enum
- `Stroke.Filters` (Feature 12) - Filter system
- `Stroke.Core.Cache` (Feature 05) - Simple cache

## Implementation Tasks

1. Implement `Binding` class
2. Implement `IKeyBindingsBase` interface
3. Implement `KeyBindings` class with caching
4. Implement `ConditionalKeyBindings` class
5. Implement `DynamicKeyBindings` class
6. Implement `GlobalOnlyKeyBindings` class
7. Implement `MergedKeyBindings` internal class
8. Implement `KeyBindingUtils` static class
9. Implement `KeyBindingAttribute`
10. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All key binding types match Python Prompt Toolkit semantics
- [ ] Key matching algorithm works correctly
- [ ] Binding caching is efficient
- [ ] Filter composition works correctly
- [ ] Unit tests achieve 80% coverage
