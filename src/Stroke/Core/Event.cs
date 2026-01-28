namespace Stroke.Core;

/// <summary>
/// Simple event to which event handlers can be attached.
/// </summary>
/// <remarks>
/// <para>
/// This class provides a pub/sub pattern for component communication.
/// Handlers are called in the order they were added when the event fires.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Event</c> class from <c>utils.py</c>.
/// </para>
/// <para>
/// This type is NOT thread-safe, following standard .NET event semantics. Like the
/// built-in C# <c>event</c> keyword, all operations should occur on the same thread
/// (typically the UI thread). Callers requiring cross-thread access should add
/// external synchronization.
/// </para>
/// </remarks>
/// <typeparam name="TSender">The type of the sender object.</typeparam>
/// <example>
/// <code>
/// class MyComponent
/// {
///     public Event&lt;MyComponent&gt; Changed { get; }
///
///     public MyComponent()
///     {
///         Changed = new Event&lt;MyComponent&gt;(this);
///     }
///
///     public void DoSomething()
///     {
///         // ... do work ...
///         Changed.Fire(); // Notify subscribers
///     }
/// }
///
/// var component = new MyComponent();
/// component.Changed += sender => Console.WriteLine("Changed!");
/// component.DoSomething(); // Prints "Changed!"
/// </code>
/// </example>
public sealed class Event<TSender>
{
    private readonly List<Action<TSender>> _handlers = [];

    /// <summary>
    /// Gets the sender object that is passed to handlers when the event fires.
    /// </summary>
    public TSender Sender { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Event{TSender}"/> class.
    /// </summary>
    /// <param name="sender">The sender object to pass to handlers.</param>
    /// <param name="handler">Optional initial handler to add.</param>
    public Event(TSender sender, Action<TSender>? handler = null)
    {
        Sender = sender;
        if (handler is not null)
        {
            _handlers.Add(handler);
        }
    }

    /// <summary>
    /// Fires the event, invoking all registered handlers in order.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Handlers are called synchronously in the order they were added.
    /// If a handler throws an exception, subsequent handlers are NOT called.
    /// </para>
    /// <para>
    /// If no handlers are registered, this method completes successfully with no effect.
    /// </para>
    /// <para>
    /// The method iterates over a snapshot of the handler list, so modifications
    /// made during iteration (adding or removing handlers) do not affect the
    /// current invocation. Changes take effect on subsequent Fire() calls.
    /// </para>
    /// </remarks>
    public void Fire()
    {
        // Take a snapshot to allow modifications during iteration
        var snapshot = _handlers.ToArray();
        foreach (var handler in snapshot)
        {
            handler(Sender);
        }
    }

    /// <summary>
    /// Adds a handler to this event.
    /// </summary>
    /// <param name="handler">The handler to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
    /// <remarks>
    /// The same handler can be added multiple times; it will be called multiple times when fired.
    /// </remarks>
    public void AddHandler(Action<TSender> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handlers.Add(handler);
    }

    /// <summary>
    /// Removes a handler from this event.
    /// </summary>
    /// <param name="handler">The handler to remove.</param>
    /// <remarks>
    /// If the handler was not previously added, this method does nothing.
    /// If the handler was added multiple times, only the first occurrence is removed.
    /// </remarks>
    public void RemoveHandler(Action<TSender> handler)
    {
        if (handler is not null)
        {
            _handlers.Remove(handler);
        }
    }

    /// <summary>
    /// Adds a handler using the += operator.
    /// </summary>
    /// <param name="e">The event.</param>
    /// <param name="handler">The handler to add.</param>
    /// <returns>The same event instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
    public static Event<TSender> operator +(Event<TSender> e, Action<TSender> handler)
    {
        e.AddHandler(handler);
        return e;
    }

    /// <summary>
    /// Removes a handler using the -= operator.
    /// </summary>
    /// <param name="e">The event.</param>
    /// <param name="handler">The handler to remove.</param>
    /// <returns>The same event instance for chaining.</returns>
    public static Event<TSender> operator -(Event<TSender> e, Action<TSender> handler)
    {
        e.RemoveHandler(handler);
        return e;
    }
}
