using System.Collections.Immutable;
using Stroke.Filters;
using Stroke.Input;

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
    /// <exception cref="ArgumentNullException">Handler is null or keys is null.</exception>
    /// <remarks>
    /// <para>
    /// The <see cref="FilterOrBool"/> parameters use C# struct defaults (equivalent to <c>false</c>),
    /// but this constructor applies semantic defaults per FR-055: filter defaults to <c>Always</c>,
    /// eager defaults to <c>Never</c>, isGlobal defaults to <c>Never</c>,
    /// and recordInMacro defaults to <c>Always</c>.
    /// </para>
    /// </remarks>
    public Binding(
        IReadOnlyList<KeyOrChar> keys,
        KeyHandlerCallable handler,
        FilterOrBool filter = default,
        FilterOrBool eager = default,
        FilterOrBool isGlobal = default,
        Func<KeyPressEvent, bool>? saveBefore = null,
        FilterOrBool recordInMacro = default)
    {
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(handler);

        if (keys.Count == 0)
        {
            throw new ArgumentException("Keys must not be empty.", nameof(keys));
        }

        // Store an immutable copy of the keys
        Keys = keys as ImmutableArray<KeyOrChar>? ?? [.. keys];
        Handler = handler;

        // Apply semantic defaults per FR-055:
        // filter: default → Always, explicit false → Never
        // eager: default → Never, explicit true → Always
        // isGlobal: default → Never, explicit true → Always
        // recordInMacro: default → Always, explicit false → Never
        Filter = ApplyDefaultFilter(filter, defaultToAlways: true);
        Eager = ApplyDefaultFilter(eager, defaultToAlways: false);
        IsGlobal = ApplyDefaultFilter(isGlobal, defaultToAlways: false);
        RecordInMacro = ApplyDefaultFilter(recordInMacro, defaultToAlways: true);
        SaveBefore = saveBefore ?? (_ => true);
    }

    /// <summary>
    /// Applies default filter logic for FilterOrBool parameters.
    /// </summary>
    /// <param name="value">The filter or bool value.</param>
    /// <param name="defaultToAlways">If true, struct default maps to Always; otherwise to Never.</param>
    /// <returns>The resolved IFilter instance.</returns>
    private static IFilter ApplyDefaultFilter(FilterOrBool value, bool defaultToAlways)
    {
        // Check if this is the struct default (no value explicitly set)
        if (!value.HasValue)
        {
            // Apply semantic default per FR-055
            return defaultToAlways ? Always.Instance : Never.Instance;
        }

        // Explicit filter value
        if (value.IsFilter)
        {
            return value.FilterValue;
        }

        // Explicit boolean value
        // true → Always, false → Never
        return value.BoolValue ? Always.Instance : Never.Instance;
    }

    /// <summary>
    /// Invokes the handler with the given event.
    /// If handler returns an awaitable, creates a background task.
    /// </summary>
    /// <param name="event">The key press event.</param>
    public void Call(KeyPressEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // First, check and invoke saveBefore callback
        // If it throws, the exception propagates and handler is NOT executed
        bool shouldSave = SaveBefore(@event);
        if (shouldSave)
        {
            // In Python, this would call save_before to checkpoint undo state
            // The actual save logic is handled by the caller/KeyProcessor
        }

        // Invoke the handler
        var result = Handler(@event);

        // Note: Async handler support (background task creation) would be handled
        // by checking if the handler is async and using App.CreateBackgroundTask.
        // Since Application doesn't exist yet, this is a placeholder.
        // The synchronous case just returns the result directly.
    }

    /// <summary>
    /// Gets the number of <see cref="Input.Keys.Any"/> wildcards in the key sequence.
    /// Used for priority sorting (fewer wildcards = higher priority).
    /// </summary>
    internal int AnyCount
    {
        get
        {
            int count = 0;
            foreach (var key in Keys)
            {
                if (key.IsKey && key.Key == Input.Keys.Any)
                {
                    count++;
                }
            }
            return count;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Binding(Keys=[{string.Join(", ", Keys)}], Handler={Handler.Method.Name})";
    }
}
