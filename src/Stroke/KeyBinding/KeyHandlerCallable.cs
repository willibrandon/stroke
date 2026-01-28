namespace Stroke.KeyBinding;

/// <summary>
/// Delegate for synchronous key binding handlers.
/// </summary>
/// <param name="event">The key press event data.</param>
/// <returns>
/// <see cref="NotImplementedOrNone.None"/> to indicate the event was handled,
/// <see cref="NotImplementedOrNone.NotImplemented"/> to indicate it was not handled,
/// or null (treated as None).
/// </returns>
public delegate NotImplementedOrNone? KeyHandlerCallable(KeyPressEvent @event);

/// <summary>
/// Delegate for asynchronous key binding handlers.
/// </summary>
/// <param name="event">The key press event data.</param>
/// <returns>A task that completes with the handler result.</returns>
public delegate Task<NotImplementedOrNone?> AsyncKeyHandlerCallable(KeyPressEvent @event);
