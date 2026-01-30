using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;

namespace Stroke.Layout.Containers;

/// <summary>
/// Container with background content and floating overlays.
/// </summary>
/// <remarks>
/// <para>
/// FloatContainer renders a background container with one or more Float elements
/// positioned on top. Floats can be positioned absolutely or relative to the
/// cursor position.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>FloatContainer</c> class from <c>layout/containers.py</c>.
/// </para>
/// </remarks>
public sealed class FloatContainer : IContainer
{
    /// <summary>
    /// Gets the background content container.
    /// </summary>
    public IContainer Content { get; }

    /// <summary>
    /// Gets the list of floating elements.
    /// </summary>
    public IReadOnlyList<Float> Floats { get; }

    /// <summary>
    /// Gets whether this container is modal.
    /// </summary>
    public bool Modal { get; }

    /// <summary>
    /// Gets the key bindings for this container.
    /// </summary>
    public IKeyBindingsBase? KeyBindings { get; }

    /// <summary>
    /// Gets the style function for this container.
    /// </summary>
    public Func<string>? StyleGetter { get; }

    /// <summary>
    /// Gets the z-index for the entire container (for nesting).
    /// </summary>
    public int? ZIndex { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FloatContainer"/> class.
    /// </summary>
    /// <param name="content">The background content container.</param>
    /// <param name="floats">The floating elements.</param>
    /// <param name="modal">Whether this container is modal.</param>
    /// <param name="keyBindings">Key bindings for this container.</param>
    /// <param name="style">Style string for this container.</param>
    /// <param name="styleGetter">Function returning style string.</param>
    /// <param name="zIndex">Z-index for the entire container.</param>
    public FloatContainer(
        AnyContainer content,
        IReadOnlyList<Float>? floats = null,
        bool modal = false,
        IKeyBindingsBase? keyBindings = null,
        string? style = null,
        Func<string>? styleGetter = null,
        int? zIndex = null)
    {
        Content = content.HasValue ? content.ToContainer() : new Window(content: new DummyControl());
        Floats = floats ?? Array.Empty<Float>();
        Modal = modal;
        KeyBindings = keyBindings;
        ZIndex = zIndex;

        if (style != null)
            StyleGetter = () => style;
        else
            StyleGetter = styleGetter;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        Content.Reset();
        foreach (var floatElement in Floats)
        {
            floatElement.Content.ToContainer()?.Reset();
        }
    }

    /// <inheritdoc/>
    public Dimension PreferredWidth(int maxAvailableWidth)
    {
        return Content.PreferredWidth(maxAvailableWidth);
    }

    /// <inheritdoc/>
    public Dimension PreferredHeight(int width, int maxAvailableHeight)
    {
        return Content.PreferredHeight(width, maxAvailableHeight);
    }

    /// <inheritdoc/>
    public void WriteToScreen(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        bool eraseBg,
        int? zIndex)
    {
        var style = StyleGetter?.Invoke() ?? "";
        var fullStyle = string.IsNullOrEmpty(parentStyle)
            ? style
            : string.IsNullOrEmpty(style)
                ? parentStyle
                : $"{parentStyle} {style}";

        // If this container has a z-index, use it (Python unconditionally overrides
        // with container's own z-index when set).
        if (ZIndex.HasValue)
        {
            zIndex = ZIndex.Value;
        }

        // Draw background content first
        Content.WriteToScreen(screen, mouseHandlers, writePosition, fullStyle, eraseBg, zIndex);

        // Draw floats on top (in order, so later floats appear on top of earlier ones within same z-index)
        foreach (var floatElement in Floats)
        {
            DrawFloat(screen, mouseHandlers, writePosition, fullStyle, floatElement, zIndex);
        }
    }

    /// <summary>
    /// Draws a single float element.
    /// </summary>
    private void DrawFloat(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        Float floatElement,
        int? zIndex)
    {
        var floatContent = floatElement.Content.ToContainer();
        if (floatContent == null)
            return;

        // Calculate available space
        var availableWidth = writePosition.Width;
        var availableHeight = writePosition.Height;

        // Get float dimensions
        var floatWidth = CalculateFloatWidth(floatElement, floatContent, availableWidth);
        var floatHeight = CalculateFloatHeight(floatElement, floatContent, floatWidth, availableHeight);

        if (floatWidth <= 0 || floatHeight <= 0)
            return;

        // Calculate position (AllowCoverCursor affects Y offset for ycursor floats)
        var (floatX, floatY) = CalculateFloatPosition(
            floatElement, screen, writePosition, floatWidth, floatHeight);

        // Clamp to available area (xpos/ypos can be negative: partially visible)
        floatWidth = Math.Min(floatWidth, availableWidth - floatX);
        floatHeight = Math.Min(floatHeight, availableHeight - floatY);

        if (floatWidth <= 0 || floatHeight <= 0)
            return;

        // Create write position for the float
        var floatWritePosition = new WritePosition(
            writePosition.XPos + floatX,
            writePosition.YPos + floatY,
            floatWidth,
            floatHeight);

        // Check hide when covering content — checks screen cells for non-space chars
        // (Python's _area_is_empty checks character content, not cursor positions)
        if (floatElement.HideWhenCoveringContent && !AreaIsEmpty(screen, floatWritePosition))
            return;

        // Compute effective z-index: parent z-index + float z-index (per Python)
        var effectiveZIndex = (zIndex ?? 0) + floatElement.ZIndex;

        screen.DrawWithZIndex(
            zIndex: effectiveZIndex,
            () =>
            {
                // Erase background if not transparent
                var shouldErase = !floatElement.Transparent;

                floatContent.WriteToScreen(
                    screen,
                    mouseHandlers,
                    floatWritePosition,
                    parentStyle,
                    shouldErase,
                    effectiveZIndex);
            });
    }

    /// <summary>
    /// Calculates the width of a float element.
    /// </summary>
    private static int CalculateFloatWidth(Float floatElement, IContainer content, int availableWidth)
    {
        // Explicit width takes precedence
        var explicitWidth = floatElement.GetWidth();
        if (explicitWidth.HasValue)
            return Math.Min(explicitWidth.Value, availableWidth);

        // If left and right both specified, calculate width
        if (floatElement.Left.HasValue && floatElement.Right.HasValue)
        {
            var calculatedWidth = availableWidth - floatElement.Left.Value - floatElement.Right.Value;
            return Math.Max(0, calculatedWidth);
        }

        // Use content's preferred width
        var preferred = content.PreferredWidth(availableWidth);
        var preferredValue = preferred.PreferredSpecified ? preferred.Preferred : availableWidth;
        return Math.Min(preferredValue, availableWidth);
    }

    /// <summary>
    /// Calculates the height of a float element.
    /// </summary>
    private static int CalculateFloatHeight(Float floatElement, IContainer content, int width, int availableHeight)
    {
        // Explicit height takes precedence
        var explicitHeight = floatElement.GetHeight();
        if (explicitHeight.HasValue)
            return Math.Min(explicitHeight.Value, availableHeight);

        // If top and bottom both specified, calculate height
        if (floatElement.Top.HasValue && floatElement.Bottom.HasValue)
        {
            var calculatedHeight = availableHeight - floatElement.Top.Value - floatElement.Bottom.Value;
            return Math.Max(0, calculatedHeight);
        }

        // Use content's preferred height
        var preferred = content.PreferredHeight(width, availableHeight);
        var preferredValue = preferred.PreferredSpecified ? preferred.Preferred : availableHeight;
        return Math.Min(preferredValue, availableHeight);
    }

    /// <summary>
    /// Calculates the position of a float element.
    /// </summary>
    private (int x, int y) CalculateFloatPosition(
        Float floatElement,
        Screen screen,
        WritePosition writePosition,
        int floatWidth,
        int floatHeight)
    {
        int x, y;
        var availableWidth = writePosition.Width;
        var availableHeight = writePosition.Height;

        // Calculate X position
        if (floatElement.XCursor)
        {
            // Position relative to cursor
            var cursorPos = GetCursorPosition(screen, floatElement.AttachToWindow);
            x = cursorPos?.X ?? 0;
        }
        else if (floatElement.Left.HasValue)
        {
            x = floatElement.Left.Value;
        }
        else if (floatElement.Right.HasValue)
        {
            x = availableWidth - floatElement.Right.Value - floatWidth;
        }
        else
        {
            // Center horizontally
            x = (availableWidth - floatWidth) / 2;
        }

        // Calculate Y position
        if (floatElement.YCursor)
        {
            // Position relative to cursor. When AllowCoverCursor is false,
            // offset by +1 to place the float below the cursor line (per Python PTK).
            var cursorPos = GetCursorPosition(screen, floatElement.AttachToWindow);
            var cursorY = cursorPos?.Y ?? 0;
            y = cursorY + (floatElement.AllowCoverCursor ? 0 : 1);

            // If not enough space below cursor, try fitting above
            if (y + floatHeight > availableHeight)
            {
                if (availableHeight - y + 1 >= y)
                {
                    // More space below — just reduce height (handled by caller clamping)
                }
                else
                {
                    // Fit above the cursor
                    var aboveHeight = Math.Min(floatHeight, cursorY);
                    y = cursorY - aboveHeight;
                }
            }
        }
        else if (floatElement.Top.HasValue)
        {
            y = floatElement.Top.Value;
        }
        else if (floatElement.Bottom.HasValue)
        {
            y = availableHeight - floatElement.Bottom.Value - floatHeight;
        }
        else
        {
            // Center vertically
            y = (availableHeight - floatHeight) / 2;
        }

        return (x, y);
    }

    /// <summary>
    /// Gets the cursor position from a window or the screen.
    /// </summary>
    private static Point? GetCursorPosition(Screen screen, Window? window)
    {
        if (window != null)
        {
            var pos = screen.GetCursorPosition(window);
            // Return null if position is zero (not set)
            if (pos == Point.Zero)
                return null;
            return pos;
        }

        // Find the first cursor position from any visible window
        foreach (var visibleWindow in screen.VisibleWindows)
        {
            if (visibleWindow is Window w)
            {
                var pos = screen.GetCursorPosition(w);
                if (pos != Point.Zero)
                    return pos;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns true when the area below the write position is still empty
    /// (all characters are spaces). Used for floats with HideWhenCoveringContent.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_area_is_empty</c> method.
    /// </remarks>
    private static bool AreaIsEmpty(Screen screen, WritePosition wp)
    {
        for (int y = wp.YPos; y < wp.YPos + wp.Height; y++)
        {
            for (int x = wp.XPos; x < wp.XPos + wp.Width; x++)
            {
                var c = screen[y, x];
                if (c.Character != " ")
                    return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public bool IsModal => Modal;

    /// <inheritdoc/>
    public IKeyBindingsBase? GetKeyBindings() => KeyBindings;

    /// <inheritdoc/>
    public IReadOnlyList<IContainer> GetChildren()
    {
        var children = new List<IContainer> { Content };
        foreach (var floatElement in Floats)
        {
            var container = floatElement.Content.ToContainer();
            if (container != null)
                children.Add(container);
        }
        return children;
    }

    /// <summary>
    /// Returns a string representation of this container.
    /// </summary>
    public override string ToString()
    {
        return $"FloatContainer(floats={Floats.Count})";
    }
}
