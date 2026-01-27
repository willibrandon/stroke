namespace Stroke.Filters;

/// <summary>
/// Filter that wraps a <see cref="Func{TResult}"/> callable for dynamic evaluation.
/// </summary>
/// <remarks>
/// <para>
/// This filter evaluates the provided callable each time <see cref="Invoke"/> is called,
/// allowing the filter result to change based on runtime state.
/// </para>
/// <para>
/// Can be used as a factory for creating filters from lambda expressions:
/// <code>
/// var isActive = new Condition(() => someState.IsActive);
/// </code>
/// </para>
/// <para>
/// The <see cref="Condition"/> instance itself is thread-safe (immutable after construction).
/// Thread safety of the wrapped <see cref="Func{TResult}"/> is the caller's responsibility.
/// If the callable accesses shared mutable state, the caller must ensure thread safety.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Condition</c> class
/// from <c>prompt_toolkit.filters.base</c>.
/// </para>
/// </remarks>
public sealed class Condition : Filter
{
    private readonly Func<bool> _func;

    /// <summary>
    /// Initializes a new instance of the <see cref="Condition"/> class.
    /// </summary>
    /// <param name="func">The callable function to evaluate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is <c>null</c>.</exception>
    public Condition(Func<bool> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        _func = func;
    }

    /// <inheritdoc/>
    /// <returns>The result of evaluating the wrapped function.</returns>
    /// <remarks>
    /// Any exception thrown by the wrapped function will propagate to the caller.
    /// </remarks>
    public override bool Invoke()
    {
        return _func();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Condition";
    }
}
