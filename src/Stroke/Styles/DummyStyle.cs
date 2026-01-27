namespace Stroke.Styles;

/// <summary>
/// A style that doesn't style anything.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DummyStyle</c> class
/// from <c>prompt_toolkit.styles.base</c>.
/// </para>
/// <para>
/// This type is thread-safe. It is stateless and immutable.
/// </para>
/// </remarks>
public sealed class DummyStyle : IStyle
{
    /// <summary>
    /// Singleton instance of the dummy style.
    /// </summary>
    public static readonly DummyStyle Instance = new();

    private static readonly IReadOnlyList<(string, string)> EmptyRules = Array.Empty<(string, string)>();

    private DummyStyle()
    {
    }

    /// <inheritdoc/>
    public Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null)
    {
        return @default ?? DefaultAttrs.Default;
    }

    /// <inheritdoc/>
    public IReadOnlyList<(string, string)> StyleRules => EmptyRules;

    /// <inheritdoc/>
    public object InvalidationHash => "DummyStyle";
}
