namespace Stroke.KeyBinding;

/// <summary>
/// Return value from key binding and mouse handlers.
/// Used to indicate whether the event was handled or should bubble up.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>NotImplementedOrNone</c> in <c>prompt_toolkit.key_binding.key_bindings</c>.
/// </para>
/// <para>
/// Handlers return <see cref="NotImplemented"/> to signal the event was not handled and should bubble up
/// to parent containers. Handlers return <see cref="None"/> to signal the event was consumed.
/// Use reference equality (<c>is NotImplementedOrNone.NotImplemented</c>) for comparison.
/// </para>
/// </remarks>
public abstract class NotImplementedOrNone
{
    // Private constructor prevents external inheritance
    private NotImplementedOrNone() { }

    /// <summary>
    /// Event was not handled, should bubble up to parent handlers.
    /// </summary>
    public static readonly NotImplementedOrNone NotImplemented = new NotImplementedValue();

    /// <summary>
    /// Event was handled and consumed.
    /// </summary>
    public static readonly NotImplementedOrNone None = new NoneValue();

    private sealed class NotImplementedValue : NotImplementedOrNone { }
    private sealed class NoneValue : NotImplementedOrNone { }
}
