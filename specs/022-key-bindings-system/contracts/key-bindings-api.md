# API Contracts: Key Bindings System

**Feature**: 022-key-bindings-system
**Date**: 2026-01-27
**Namespace**: `Stroke.KeyBinding`

---

## Delegates

### KeyHandlerCallable

```csharp
/// <summary>
/// Delegate for key binding handlers.
/// </summary>
/// <param name="event">The key press event data.</param>
/// <returns>
/// <see cref="NotImplementedOrNone.None"/> to indicate the event was handled,
/// <see cref="NotImplementedOrNone.NotImplemented"/> to indicate it was not handled,
/// or null (treated as None).
/// </returns>
public delegate NotImplementedOrNone? KeyHandlerCallable(KeyPressEvent @event);
```

### AsyncKeyHandlerCallable

```csharp
/// <summary>
/// Delegate for asynchronous key binding handlers.
/// </summary>
/// <param name="event">The key press event data.</param>
/// <returns>A task that completes with the handler result.</returns>
public delegate Task<NotImplementedOrNone?> AsyncKeyHandlerCallable(KeyPressEvent @event);
```

---

## KeyOrChar

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Union type representing either a <see cref="Keys"/> enum value or a single character.
/// </summary>
/// <remarks>
/// <para>
/// This type is thread-safe as it is immutable.
/// </para>
/// <para>
/// Equivalent to Python's <c>Keys | str</c> union type where str is a single character.
/// </para>
/// </remarks>
public readonly record struct KeyOrChar : IEquatable<KeyOrChar>
{
    /// <summary>Gets whether this represents a <see cref="Keys"/> enum value.</summary>
    public bool IsKey { get; }

    /// <summary>Gets whether this represents a single character.</summary>
    public bool IsChar { get; }

    /// <summary>Gets the key value. Throws if <see cref="IsKey"/> is false.</summary>
    /// <exception cref="InvalidOperationException">This is not a key.</exception>
    public Keys Key { get; }

    /// <summary>Gets the character value. Throws if <see cref="IsChar"/> is false.</summary>
    /// <exception cref="InvalidOperationException">This is not a character.</exception>
    public char Char { get; }

    /// <summary>Creates a KeyOrChar from a Keys enum value.</summary>
    public KeyOrChar(Keys key);

    /// <summary>Creates a KeyOrChar from a single character.</summary>
    public KeyOrChar(char c);

    /// <summary>Implicit conversion from Keys.</summary>
    public static implicit operator KeyOrChar(Keys key);

    /// <summary>Implicit conversion from char.</summary>
    public static implicit operator KeyOrChar(char c);

    /// <summary>Implicit conversion from single-character string.</summary>
    /// <exception cref="ArgumentException">String is not exactly one character.</exception>
    public static implicit operator KeyOrChar(string s);
}
```

---

## KeyPress

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Represents a single key press with the key value and optional raw terminal data.
/// </summary>
/// <remarks>
/// <para>
/// This type is thread-safe as it is immutable.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>KeyPress</c> class from <c>key_processor.py</c>.
/// </para>
/// </remarks>
public readonly record struct KeyPress : IEquatable<KeyPress>
{
    /// <summary>Gets the key that was pressed.</summary>
    public KeyOrChar Key { get; }

    /// <summary>
    /// Gets the raw terminal data (escape sequence or character).
    /// If not provided, defaults to the key's string representation.
    /// </summary>
    public string Data { get; }

    /// <summary>
    /// Creates a new KeyPress with the specified key and optional raw data.
    /// </summary>
    /// <param name="key">The key or character pressed.</param>
    /// <param name="data">Raw terminal data. If null, defaults based on key type.</param>
    public KeyPress(KeyOrChar key, string? data = null);

    /// <summary>Creates a KeyPress from a Keys enum value.</summary>
    public KeyPress(Keys key, string? data = null);

    /// <summary>Creates a KeyPress from a character.</summary>
    public KeyPress(char c, string? data = null);
}
```

---

## Binding

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Immutable binding associating a key sequence with a handler and conditions.
/// </summary>
/// <remarks>
/// <para>
/// This class is thread-safe as it is immutable after construction.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>Binding</c> class from <c>key_bindings.py</c>.
/// </para>
/// </remarks>
public sealed class Binding
{
    /// <summary>Gets the key sequence that triggers this binding.</summary>
    public IReadOnlyList<KeyOrChar> Keys { get; }

    /// <summary>Gets the handler function.</summary>
    public KeyHandlerCallable Handler { get; }

    /// <summary>Gets the filter that determines when this binding is active.</summary>
    public IFilter Filter { get; }

    /// <summary>Gets the filter that determines eager matching behavior.</summary>
    public IFilter Eager { get; }

    /// <summary>Gets the filter that determines global binding behavior.</summary>
    public IFilter IsGlobal { get; }

    /// <summary>
    /// Gets the callback that determines whether to save buffer state before handler execution.
    /// </summary>
    public Func<KeyPressEvent, bool> SaveBefore { get; }

    /// <summary>Gets the filter that determines whether to record in macro.</summary>
    public IFilter RecordInMacro { get; }

    /// <summary>
    /// Creates a new Binding instance.
    /// </summary>
    /// <param name="keys">Key sequence (must not be empty).</param>
    /// <param name="handler">Handler function (must not be null).</param>
    /// <param name="filter">Activation filter (default: Always).</param>
    /// <param name="eager">Eager matching filter (default: Never).</param>
    /// <param name="isGlobal">Global binding filter (default: Never).</param>
    /// <param name="saveBefore">Save-before callback (default: always true).</param>
    /// <param name="recordInMacro">Macro recording filter (default: Always).</param>
    /// <exception cref="ArgumentException">Keys is empty.</exception>
    /// <exception cref="ArgumentNullException">Handler is null.</exception>
    /// <remarks>
    /// <para>
    /// <b>Implementation note:</b> The <see cref="FilterOrBool"/> parameters use C# struct defaults
    /// (equivalent to <c>false</c>), but this constructor MUST apply semantic defaults per FR-055:
    /// filter defaults to <c>Always</c>, eager defaults to <c>Never</c>, isGlobal defaults to <c>Never</c>,
    /// and recordInMacro defaults to <c>Always</c>. Implementation should check <c>!param.HasValue</c>
    /// and substitute the appropriate default filter.
    /// </para>
    /// </remarks>
    public Binding(
        IReadOnlyList<KeyOrChar> keys,
        KeyHandlerCallable handler,
        FilterOrBool filter = default,
        FilterOrBool eager = default,
        FilterOrBool isGlobal = default,
        Func<KeyPressEvent, bool>? saveBefore = null,
        FilterOrBool recordInMacro = default);

    /// <summary>
    /// Invokes the handler with the given event.
    /// If handler returns an awaitable, creates a background task.
    /// </summary>
    /// <param name="event">The key press event.</param>
    public void Call(KeyPressEvent @event);
}
```

---

## IKeyBindingsBase

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Interface defining the contract for key binding registries.
/// </summary>
/// <remarks>
/// <para>
/// Implementations must be thread-safe for concurrent read access.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>KeyBindingsBase</c> abstract class.
/// </para>
/// </remarks>
public interface IKeyBindingsBase
{
    /// <summary>
    /// Gets the version for cache invalidation.
    /// This value changes whenever bindings are added or removed.
    /// </summary>
    object Version { get; }

    /// <summary>
    /// Gets all bindings in this registry.
    /// </summary>
    IReadOnlyList<Binding> Bindings { get; }

    /// <summary>
    /// Returns bindings that exactly match the given key sequence.
    /// Results are sorted by Keys.Any count (fewer wildcards = higher priority).
    /// </summary>
    /// <param name="keys">The key sequence to match.</param>
    /// <returns>
    /// Matching bindings including inactive ones. Caller must check filters.
    /// </returns>
    IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyOrChar> keys);

    /// <summary>
    /// Returns bindings with sequences longer than and starting with the given prefix.
    /// </summary>
    /// <param name="keys">The prefix to match.</param>
    /// <returns>
    /// Bindings that could complete the sequence. Caller must check filters.
    /// </returns>
    IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyOrChar> keys);
}
```

---

## KeyBindings

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Concrete mutable key binding registry with add/remove capabilities and caching.
/// </summary>
/// <remarks>
/// <para>
/// This class is thread-safe. All operations are protected by an internal lock.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>KeyBindings</c> class.
/// </para>
/// </remarks>
public sealed class KeyBindings : IKeyBindingsBase
{
    /// <summary>Creates an empty KeyBindings registry.</summary>
    public KeyBindings();

    /// <inheritdoc/>
    public object Version { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> Bindings { get; }

    /// <summary>
    /// Adds a key binding. Returns a decorator function for method chaining.
    /// </summary>
    /// <typeparam name="T">Handler type (KeyHandlerCallable or Binding).</typeparam>
    /// <param name="keys">Key sequence to bind.</param>
    /// <param name="filter">Activation filter (default: true).</param>
    /// <param name="eager">Eager matching filter (default: false).</param>
    /// <param name="isGlobal">Global binding filter (default: false).</param>
    /// <param name="saveBefore">Save-before callback (default: true).</param>
    /// <param name="recordInMacro">Macro recording filter (default: true).</param>
    /// <returns>Decorator function that returns the input unchanged.</returns>
    /// <exception cref="ArgumentException">Keys is empty.</exception>
    public Func<T, T> Add<T>(
        KeyOrChar[] keys,
        FilterOrBool filter = default,
        FilterOrBool eager = default,
        FilterOrBool isGlobal = default,
        Func<KeyPressEvent, bool>? saveBefore = null,
        FilterOrBool recordInMacro = default) where T : class;

    /// <summary>
    /// Removes bindings by handler reference.
    /// </summary>
    /// <param name="handler">The handler to remove.</param>
    /// <exception cref="InvalidOperationException">No binding found for handler.</exception>
    public void Remove(KeyHandlerCallable handler);

    /// <summary>
    /// Removes bindings by key sequence.
    /// </summary>
    /// <param name="keys">The key sequence to remove.</param>
    /// <exception cref="InvalidOperationException">No binding found for keys.</exception>
    public void Remove(params KeyOrChar[] keys);

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyOrChar> keys);

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyOrChar> keys);

    // Backwards compatibility aliases
    /// <summary>Alias for <see cref="Add{T}"/>.</summary>
    public Func<T, T> AddBinding<T>(
        KeyOrChar[] keys,
        FilterOrBool filter = default,
        FilterOrBool eager = default,
        FilterOrBool isGlobal = default,
        Func<KeyPressEvent, bool>? saveBefore = null,
        FilterOrBool recordInMacro = default) where T : class;

    /// <summary>Alias for <see cref="Remove(KeyHandlerCallable)"/>.</summary>
    public void RemoveBinding(KeyHandlerCallable handler);

    /// <summary>Alias for <see cref="Remove(KeyOrChar[])"/>.</summary>
    public void RemoveBinding(params KeyOrChar[] keys);
}
```

---

## ConditionalKeyBindings

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Wraps a KeyBindings registry and applies an additional filter to all bindings.
/// </summary>
/// <remarks>
/// <para>
/// This class is thread-safe. Cache updates are protected by an internal lock.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>ConditionalKeyBindings</c> class.
/// </para>
/// </remarks>
public sealed class ConditionalKeyBindings : IKeyBindingsBase
{
    /// <summary>Gets the wrapped key bindings.</summary>
    public IKeyBindingsBase KeyBindings { get; }

    /// <summary>Gets the additional filter applied to all bindings.</summary>
    public IFilter Filter { get; }

    /// <summary>
    /// Creates a ConditionalKeyBindings wrapper.
    /// </summary>
    /// <param name="keyBindings">The registry to wrap.</param>
    /// <param name="filter">The additional filter (default: Always).</param>
    public ConditionalKeyBindings(IKeyBindingsBase keyBindings, FilterOrBool filter = default);

    /// <inheritdoc/>
    public object Version { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> Bindings { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyOrChar> keys);

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyOrChar> keys);
}
```

---

## MergedKeyBindings

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Combines multiple key binding registries into a single view.
/// </summary>
/// <remarks>
/// <para>
/// This class is thread-safe. Cache updates are protected by an internal lock.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>_MergedKeyBindings</c> class.
/// </para>
/// </remarks>
public sealed class MergedKeyBindings : IKeyBindingsBase
{
    /// <summary>Gets the child registries being merged.</summary>
    public IReadOnlyList<IKeyBindingsBase> Registries { get; }

    /// <summary>
    /// Creates a MergedKeyBindings from multiple registries.
    /// </summary>
    /// <param name="registries">The registries to merge.</param>
    public MergedKeyBindings(IEnumerable<IKeyBindingsBase> registries);

    /// <inheritdoc/>
    public object Version { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> Bindings { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyOrChar> keys);

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyOrChar> keys);
}
```

---

## DynamicKeyBindings

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Key bindings that delegate to a callable returning any KeyBindings instance.
/// </summary>
/// <remarks>
/// <para>
/// This class is thread-safe. Cache updates are protected by an internal lock.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>DynamicKeyBindings</c> class.
/// </para>
/// </remarks>
public sealed class DynamicKeyBindings : IKeyBindingsBase
{
    /// <summary>Gets the provider callable.</summary>
    public Func<IKeyBindingsBase?> GetKeyBindings { get; }

    /// <summary>
    /// Creates a DynamicKeyBindings with the specified provider.
    /// </summary>
    /// <param name="getKeyBindings">
    /// Callable that returns a KeyBindings instance, or null for empty bindings.
    /// </param>
    public DynamicKeyBindings(Func<IKeyBindingsBase?> getKeyBindings);

    /// <inheritdoc/>
    public object Version { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> Bindings { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyOrChar> keys);

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyOrChar> keys);
}
```

---

## GlobalOnlyKeyBindings

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Wrapper that exposes only global bindings from a wrapped registry.
/// </summary>
/// <remarks>
/// <para>
/// This class is thread-safe. Cache updates are protected by an internal lock.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>GlobalOnlyKeyBindings</c> class.
/// </para>
/// </remarks>
public sealed class GlobalOnlyKeyBindings : IKeyBindingsBase
{
    /// <summary>Gets the wrapped key bindings.</summary>
    public IKeyBindingsBase KeyBindings { get; }

    /// <summary>
    /// Creates a GlobalOnlyKeyBindings wrapper.
    /// </summary>
    /// <param name="keyBindings">The registry to filter.</param>
    public GlobalOnlyKeyBindings(IKeyBindingsBase keyBindings);

    /// <inheritdoc/>
    public object Version { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> Bindings { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyOrChar> keys);

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyOrChar> keys);
}
```

---

## KeyBindingUtils

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Utility methods for working with key bindings.
/// </summary>
public static class KeyBindingUtils
{
    /// <summary>
    /// Merges multiple key binding registries into one.
    /// </summary>
    /// <param name="bindings">The registries to merge.</param>
    /// <returns>A merged registry containing all bindings.</returns>
    public static MergedKeyBindings Merge(IEnumerable<IKeyBindingsBase> bindings);

    /// <summary>
    /// Merges multiple key binding registries into one.
    /// </summary>
    /// <param name="bindings">The registries to merge.</param>
    /// <returns>A merged registry containing all bindings.</returns>
    public static MergedKeyBindings Merge(params IKeyBindingsBase[] bindings);

    /// <summary>
    /// Parses a key string into a KeyOrChar value.
    /// Handles aliases (c-a → ControlA) and special names (space → ' ').
    /// </summary>
    /// <param name="key">The key string to parse.</param>
    /// <returns>The parsed key value.</returns>
    /// <exception cref="ArgumentException">Invalid key string.</exception>
    public static KeyOrChar ParseKey(string key);
}
```

---

## KeyBindingDecorator

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Factory for creating Binding objects with pre-configured settings.
/// Equivalent to Python's @key_binding decorator.
/// </summary>
public static class KeyBindingDecorator
{
    /// <summary>
    /// Creates a decorator function that turns a handler into a Binding.
    /// </summary>
    /// <param name="filter">Activation filter (default: true).</param>
    /// <param name="eager">Eager matching filter (default: false).</param>
    /// <param name="isGlobal">Global binding filter (default: false).</param>
    /// <param name="saveBefore">Save-before callback (default: true).</param>
    /// <param name="recordInMacro">Macro recording filter (default: true).</param>
    /// <returns>
    /// A function that takes a handler and returns a Binding with empty keys.
    /// The binding can then be added to a KeyBindings registry.
    /// </returns>
    public static Func<KeyHandlerCallable, Binding> Create(
        FilterOrBool filter = default,
        FilterOrBool eager = default,
        FilterOrBool isGlobal = default,
        Func<KeyPressEvent, bool>? saveBefore = null,
        FilterOrBool recordInMacro = default);
}
```

---

## KeyPressEvent

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Event data passed to key binding handlers.
/// </summary>
/// <remarks>
/// <para>
/// Property access is thread-safe. Buffer operations require external synchronization.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>KeyPressEvent</c> class from <c>key_processor.py</c>.
/// </para>
/// </remarks>
public class KeyPressEvent
{
    /// <summary>Gets the key processor that created this event.</summary>
    /// <exception cref="InvalidOperationException">KeyProcessor was garbage collected.</exception>
    public KeyProcessor KeyProcessor { get; }

    /// <summary>Gets the key sequence that triggered this event.</summary>
    public IReadOnlyList<KeyPress> KeySequence { get; }

    /// <summary>Gets the previous key sequence (before this event).</summary>
    public IReadOnlyList<KeyPress> PreviousKeySequence { get; }

    /// <summary>Gets whether this is a repeat of the previous handler.</summary>
    public bool IsRepeat { get; }

    /// <summary>
    /// Gets the repetition argument. Defaults to 1.
    /// Special value: -1 when arg is "-".
    /// Clamped to avoid exceeding 1,000,000.
    /// </summary>
    public int Arg { get; }

    /// <summary>Gets whether a repetition argument was explicitly provided.</summary>
    public bool ArgPresent { get; }

    /// <summary>Gets the current application.</summary>
    public IApplication App { get; }

    /// <summary>Gets the current buffer (shortcut for App.CurrentBuffer).</summary>
    public Buffer CurrentBuffer { get; }

    /// <summary>Gets the raw data of the last key in the sequence.</summary>
    public string Data { get; }

    /// <summary>
    /// Creates a new KeyPressEvent.
    /// </summary>
    public KeyPressEvent(
        WeakReference<KeyProcessor> keyProcessorRef,
        string? arg,
        IReadOnlyList<KeyPress> keySequence,
        IReadOnlyList<KeyPress> previousKeySequence,
        bool isRepeat);

    /// <summary>
    /// Appends a digit to the repetition argument.
    /// </summary>
    /// <param name="data">Digit character ('0'-'9' or '-').</param>
    /// <exception cref="ArgumentException">Invalid digit.</exception>
    public void AppendToArgCount(string data);

    /// <summary>Backwards compatibility alias for App.</summary>
    [Obsolete("Use App property instead")]
    public IApplication Cli { get; }
}
```

---

## Extension Methods

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Extension methods for key bindings.
/// </summary>
public static class KeyBindingsExtensions
{
    /// <summary>
    /// Merges this registry with others.
    /// </summary>
    public static MergedKeyBindings Merge(
        this IKeyBindingsBase first,
        params IKeyBindingsBase[] others);

    /// <summary>
    /// Wraps this registry with a conditional filter.
    /// </summary>
    public static ConditionalKeyBindings WithFilter(
        this IKeyBindingsBase bindings,
        FilterOrBool filter);

    /// <summary>
    /// Wraps this registry to expose only global bindings.
    /// </summary>
    public static GlobalOnlyKeyBindings GlobalOnly(this IKeyBindingsBase bindings);
}
```
