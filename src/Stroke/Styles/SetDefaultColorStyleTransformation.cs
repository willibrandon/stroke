namespace Stroke.Styles;

/// <summary>
/// Set default foreground/background color for output that doesn't specify anything.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SetDefaultColorStyleTransformation</c>
/// from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This is useful for overriding the terminal default colors.
/// </para>
/// <para>
/// This type is thread-safe. The color callables may be invoked from multiple threads;
/// thread safety of the callables is the caller's responsibility.
/// </para>
/// </remarks>
public sealed class SetDefaultColorStyleTransformation : IStyleTransformation
{
    private readonly Func<string> _fg;
    private readonly Func<string> _bg;

    /// <summary>
    /// Creates a new instance with static color values.
    /// </summary>
    /// <param name="fg">Foreground color string.</param>
    /// <param name="bg">Background color string.</param>
    public SetDefaultColorStyleTransformation(string fg, string bg)
    {
        _fg = () => fg;
        _bg = () => bg;
    }

    /// <summary>
    /// Creates a new instance with dynamic color callables.
    /// </summary>
    /// <param name="fg">Callable that returns foreground color string.</param>
    /// <param name="bg">Callable that returns background color string.</param>
    /// <exception cref="ArgumentNullException">Thrown when fg or bg is null.</exception>
    public SetDefaultColorStyleTransformation(Func<string> fg, Func<string> bg)
    {
        ArgumentNullException.ThrowIfNull(fg);
        ArgumentNullException.ThrowIfNull(bg);
        _fg = fg;
        _bg = bg;
    }

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs)
    {
        var result = attrs;

        // If background is empty or default, set it
        if (result.BgColor is "" or "default" or null)
        {
            result = result with { BgColor = StyleParser.ParseColor(_bg()) };
        }

        // If foreground is empty or default, set it
        if (result.Color is "" or "default" or null)
        {
            result = result with { Color = StyleParser.ParseColor(_fg()) };
        }

        return result;
    }

    /// <inheritdoc/>
    public object InvalidationHash => ("set-default-color", _fg(), _bg());
}
