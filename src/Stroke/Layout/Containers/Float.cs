using Stroke.Filters;

namespace Stroke.Layout.Containers;

/// <summary>
/// A floating element with positioning configuration.
/// </summary>
/// <remarks>
/// <para>
/// Float defines a UI element that can be positioned over a background container.
/// Supports both absolute positioning (top, left, right, bottom) and cursor-relative
/// positioning (xcursor, ycursor).
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Float</c> class from <c>layout/containers.py</c>.
/// </para>
/// </remarks>
public sealed class Float
{
    /// <summary>
    /// Gets the content to display in this float.
    /// </summary>
    public AnyContainer Content { get; }

    /// <summary>
    /// Gets the absolute top position in rows, or null for no constraint.
    /// </summary>
    public int? Top { get; }

    /// <summary>
    /// Gets the absolute right position in columns from the right edge, or null for no constraint.
    /// </summary>
    public int? Right { get; }

    /// <summary>
    /// Gets the absolute bottom position in rows from the bottom edge, or null for no constraint.
    /// </summary>
    public int? Bottom { get; }

    /// <summary>
    /// Gets the absolute left position in columns, or null for no constraint.
    /// </summary>
    public int? Left { get; }

    /// <summary>
    /// Gets the function returning the width, or null for automatic sizing.
    /// </summary>
    public Func<int>? WidthGetter { get; }

    /// <summary>
    /// Gets the function returning the height, or null for automatic sizing.
    /// </summary>
    public Func<int>? HeightGetter { get; }

    /// <summary>
    /// Gets whether the float should position horizontally relative to the cursor.
    /// </summary>
    public bool XCursor { get; }

    /// <summary>
    /// Gets whether the float should position vertically relative to the cursor.
    /// </summary>
    public bool YCursor { get; }

    /// <summary>
    /// Gets the window to attach to for cursor-relative positioning, or null for the focused window.
    /// </summary>
    public Window? AttachToWindow { get; }

    /// <summary>
    /// Gets whether to hide this float when it would cover the content cursor.
    /// </summary>
    public bool HideWhenCoveringContent { get; }

    /// <summary>
    /// Gets whether this float is allowed to cover the cursor.
    /// </summary>
    public bool AllowCoverCursor { get; }

    /// <summary>
    /// Gets the z-index for layering (higher values on top). Minimum is 1.
    /// </summary>
    public int ZIndex { get; }

    /// <summary>
    /// Gets whether background content should show through this float.
    /// </summary>
    public bool Transparent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Float"/> class.
    /// </summary>
    /// <param name="content">The container content for this float.</param>
    /// <param name="top">Absolute top position in rows.</param>
    /// <param name="right">Absolute right position in columns from right edge.</param>
    /// <param name="bottom">Absolute bottom position in rows from bottom edge.</param>
    /// <param name="left">Absolute left position in columns.</param>
    /// <param name="width">Fixed width in columns, or null for automatic.</param>
    /// <param name="height">Fixed height in rows, or null for automatic.</param>
    /// <param name="widthGetter">Function returning width, or null for automatic.</param>
    /// <param name="heightGetter">Function returning height, or null for automatic.</param>
    /// <param name="xcursor">Position horizontally relative to cursor.</param>
    /// <param name="ycursor">Position vertically relative to cursor.</param>
    /// <param name="attachToWindow">Window for cursor position, or null for focused window.</param>
    /// <param name="hideWhenCoveringContent">Hide when covering the content cursor.</param>
    /// <param name="allowCoverCursor">Allow covering the cursor.</param>
    /// <param name="zIndex">Z-index for layering (minimum 1).</param>
    /// <param name="transparent">Allow background to show through.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when xcursor is true and left is specified, or when ycursor is true and top is specified.
    /// </exception>
    public Float(
        AnyContainer content,
        int? top = null,
        int? right = null,
        int? bottom = null,
        int? left = null,
        int? width = null,
        int? height = null,
        Func<int>? widthGetter = null,
        Func<int>? heightGetter = null,
        bool xcursor = false,
        bool ycursor = false,
        Window? attachToWindow = null,
        bool hideWhenCoveringContent = false,
        bool allowCoverCursor = false,
        int zIndex = 1,
        bool transparent = false)
    {
        // Validation: cursor-relative and absolute positioning are mutually exclusive
        if (xcursor && left.HasValue)
            throw new ArgumentException("Cannot use xcursor with absolute left position.", nameof(xcursor));
        if (ycursor && top.HasValue)
            throw new ArgumentException("Cannot use ycursor with absolute top position.", nameof(ycursor));

        Content = content;
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
        XCursor = xcursor;
        YCursor = ycursor;
        AttachToWindow = attachToWindow;
        HideWhenCoveringContent = hideWhenCoveringContent;
        AllowCoverCursor = allowCoverCursor;
        Transparent = transparent;

        // Z-index must be at least 1 (FR-009)
        ZIndex = Math.Max(1, zIndex);

        // Width and height getters (fixed value or dynamic)
        if (width.HasValue)
            WidthGetter = () => width.Value;
        else
            WidthGetter = widthGetter;

        if (height.HasValue)
            HeightGetter = () => height.Value;
        else
            HeightGetter = heightGetter;
    }

    /// <summary>
    /// Gets the current width, or null if automatic sizing should be used.
    /// </summary>
    /// <returns>The width in columns, or null.</returns>
    public int? GetWidth() => WidthGetter?.Invoke();

    /// <summary>
    /// Gets the current height, or null if automatic sizing should be used.
    /// </summary>
    /// <returns>The height in rows, or null.</returns>
    public int? GetHeight() => HeightGetter?.Invoke();

    /// <summary>
    /// Returns a string representation of this float.
    /// </summary>
    public override string ToString()
    {
        return $"Float(top={Top}, left={Left}, zIndex={ZIndex})";
    }
}
