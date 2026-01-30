namespace Stroke.Layout.Windows;

/// <summary>
/// Configuration for scroll behavior (cursor margin requirements).
/// </summary>
/// <remarks>
/// <para>
/// Defines the minimum number of visible lines/columns to keep around the cursor
/// when scrolling. The scroll algorithm will adjust the scroll position to
/// maintain these margins when possible.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ScrollOffsets</c> class from <c>layout/containers.py</c>.
/// </para>
/// </remarks>
public sealed class ScrollOffsets
{
    private static readonly Func<int> _zeroFunc = () => 0;

    /// <summary>
    /// Gets the function returning the top scroll offset.
    /// </summary>
    public Func<int> TopGetter { get; }

    /// <summary>
    /// Gets the function returning the bottom scroll offset.
    /// </summary>
    public Func<int> BottomGetter { get; }

    /// <summary>
    /// Gets the function returning the left scroll offset.
    /// </summary>
    public Func<int> LeftGetter { get; }

    /// <summary>
    /// Gets the function returning the right scroll offset.
    /// </summary>
    public Func<int> RightGetter { get; }

    /// <summary>
    /// Gets the current top scroll offset.
    /// </summary>
    public int Top => TopGetter();

    /// <summary>
    /// Gets the current bottom scroll offset.
    /// </summary>
    public int Bottom => BottomGetter();

    /// <summary>
    /// Gets the current left scroll offset.
    /// </summary>
    public int Left => LeftGetter();

    /// <summary>
    /// Gets the current right scroll offset.
    /// </summary>
    public int Right => RightGetter();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollOffsets"/> class with constant values.
    /// </summary>
    /// <param name="top">Minimum lines visible above cursor. Default is 0.</param>
    /// <param name="bottom">Minimum lines visible below cursor. Default is 0.</param>
    /// <param name="left">Minimum columns visible left of cursor. Default is 0.</param>
    /// <param name="right">Minimum columns visible right of cursor. Default is 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">Any parameter is negative.</exception>
    public ScrollOffsets(int top = 0, int bottom = 0, int left = 0, int right = 0)
    {
        if (top < 0)
            throw new ArgumentOutOfRangeException(nameof(top), top, "Top offset must be non-negative.");
        if (bottom < 0)
            throw new ArgumentOutOfRangeException(nameof(bottom), bottom, "Bottom offset must be non-negative.");
        if (left < 0)
            throw new ArgumentOutOfRangeException(nameof(left), left, "Left offset must be non-negative.");
        if (right < 0)
            throw new ArgumentOutOfRangeException(nameof(right), right, "Right offset must be non-negative.");

        TopGetter = () => top;
        BottomGetter = () => bottom;
        LeftGetter = () => left;
        RightGetter = () => right;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollOffsets"/> class with dynamic values.
    /// </summary>
    /// <param name="topGetter">Function returning top offset. Null defaults to 0.</param>
    /// <param name="bottomGetter">Function returning bottom offset. Null defaults to 0.</param>
    /// <param name="leftGetter">Function returning left offset. Null defaults to 0.</param>
    /// <param name="rightGetter">Function returning right offset. Null defaults to 0.</param>
    public ScrollOffsets(
        Func<int>? topGetter = null,
        Func<int>? bottomGetter = null,
        Func<int>? leftGetter = null,
        Func<int>? rightGetter = null)
    {
        TopGetter = topGetter ?? _zeroFunc;
        BottomGetter = bottomGetter ?? _zeroFunc;
        LeftGetter = leftGetter ?? _zeroFunc;
        RightGetter = rightGetter ?? _zeroFunc;
    }

    /// <summary>
    /// Returns a string representation of the current scroll offsets.
    /// </summary>
    public override string ToString()
    {
        return $"ScrollOffsets(top={Top}, bottom={Bottom}, left={Left}, right={Right})";
    }
}
