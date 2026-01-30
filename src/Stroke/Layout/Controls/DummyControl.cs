using Stroke.Core;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Windows;

namespace Stroke.Layout.Controls;

/// <summary>
/// Empty placeholder control that renders nothing.
/// </summary>
/// <remarks>
/// <para>
/// Used as a default content when a Window has no explicit control,
/// or for layout spacing purposes.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>DummyControl</c> class from <c>layout/controls.py</c>.
/// </para>
/// </remarks>
public sealed class DummyControl : IUIControl
{
    private static readonly UIContent _emptyContent = new(lineCount: 1);

    /// <inheritdoc/>
    public bool IsFocusable => false;

    /// <inheritdoc/>
    public UIContent CreateContent(int width, int height)
    {
        return _emptyContent;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        // No state to reset
    }

    /// <inheritdoc/>
    public int? PreferredWidth(int maxAvailableWidth) => null;

    /// <inheritdoc/>
    public int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix) => null;

    /// <inheritdoc/>
    public NotImplementedOrNone MouseHandler(MouseEvent mouseEvent)
        => NotImplementedOrNone.NotImplemented;

    /// <inheritdoc/>
    public void MoveCursorDown()
    {
        // No cursor to move
    }

    /// <inheritdoc/>
    public void MoveCursorUp()
    {
        // No cursor to move
    }

    /// <inheritdoc/>
    public IKeyBindingsBase? GetKeyBindings() => null;

    /// <inheritdoc/>
    public IEnumerable<Event<object>> GetInvalidateEvents() => [];
}
