namespace Stroke.Styles;

/// <summary>
/// Style class that can dynamically return another Style.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DynamicStyle</c> class
/// from <c>prompt_toolkit.styles.base</c>.
/// </para>
/// <para>
/// This type is thread-safe. The underlying callable may be invoked from multiple
/// threads; thread safety of the callable is the caller's responsibility.
/// </para>
/// </remarks>
public sealed class DynamicStyle : IStyle
{
    private readonly Func<IStyle?> _getStyle;

    /// <summary>
    /// Creates a dynamic style.
    /// </summary>
    /// <param name="getStyle">Callable that returns a <see cref="IStyle"/> instance, or null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getStyle"/> is null.</exception>
    public DynamicStyle(Func<IStyle?> getStyle)
    {
        ArgumentNullException.ThrowIfNull(getStyle);
        _getStyle = getStyle;
    }

    /// <summary>
    /// Gets the current underlying style, or DummyStyle if null.
    /// </summary>
    private IStyle CurrentStyle => _getStyle() ?? DummyStyle.Instance;

    /// <inheritdoc/>
    public Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null)
    {
        return CurrentStyle.GetAttrsForStyleStr(styleStr, @default);
    }

    /// <inheritdoc/>
    public IReadOnlyList<(string ClassNames, string StyleDef)> StyleRules => CurrentStyle.StyleRules;

    /// <inheritdoc/>
    public object InvalidationHash => CurrentStyle.InvalidationHash;
}
