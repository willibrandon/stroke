using Stroke.Core;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;

namespace Stroke.Layout.Containers;

/// <summary>
/// Stacks children vertically (one above the other).
/// </summary>
/// <remarks>
/// <para>
/// Creates a layout where child containers are arranged from top to bottom:
/// </para>
/// <code>
/// +--------------------+
/// |                    |
/// +--------------------+
/// |                    |
/// +--------------------+
/// </code>
/// <para>
/// By default, this doesn't display a horizontal line between the children,
/// but if needed, create an HSplit with padding configuration:
/// </para>
/// <code>
/// new HSplit(children, padding: 1, paddingChar: '-', paddingStyle: "fg:yellow")
/// </code>
/// <para>
/// Port of Python Prompt Toolkit's <c>HSplit</c> class from <c>layout/containers.py</c>.
/// </para>
/// <para>
/// Thread-safe: Internal caches are protected by <see cref="Lock"/>.
/// </para>
/// </remarks>
public sealed class HSplit : IContainer
{
    private readonly Lock _lock = new();
    private readonly IReadOnlyList<IContainer> _children;
    private readonly Func<IContainer>? _windowTooSmallGetter;
    private IContainer? _windowTooSmallCache;
    private readonly Dimension? _width;
    private readonly Dimension? _height;
    private readonly int? _zIndex;
    private readonly bool _modal;
    private readonly IKeyBindingsBase? _keyBindings;
    private readonly Func<string> _styleGetter;

    // Cache for children including padding windows
    private readonly SimpleCache<int, IReadOnlyList<IContainer>> _childrenCache = new(maxSize: 1);

    // Dummy window for filling remaining space
    private IContainer? _remainingSpaceWindow;

    /// <summary>
    /// Gets the list of direct children (without padding).
    /// </summary>
    public IReadOnlyList<IContainer> Children => _children;

    /// <summary>
    /// Gets the vertical alignment.
    /// </summary>
    public VerticalAlign Align { get; }

    /// <summary>
    /// Gets the padding dimension between children.
    /// </summary>
    public Dimension Padding { get; }

    /// <summary>
    /// Gets the character used for padding fill.
    /// </summary>
    public char? PaddingChar { get; }

    /// <summary>
    /// Gets the style for padding.
    /// </summary>
    public string PaddingStyle { get; }

    /// <inheritdoc/>
    public bool IsModal => _modal;

    /// <summary>
    /// Initializes a new instance of the <see cref="HSplit"/> class.
    /// </summary>
    /// <param name="children">Child containers to arrange vertically.</param>
    /// <param name="windowTooSmall">Container to display when there's not enough space.</param>
    /// <param name="align">Vertical alignment of children.</param>
    /// <param name="padding">Size of padding between children.</param>
    /// <param name="paddingChar">Character to fill padding with.</param>
    /// <param name="paddingStyle">Style for padding regions.</param>
    /// <param name="width">Override width dimension.</param>
    /// <param name="height">Override height dimension.</param>
    /// <param name="zIndex">Z-index for float rendering.</param>
    /// <param name="modal">Whether key bindings are modal.</param>
    /// <param name="keyBindings">Key bindings for this container.</param>
    /// <param name="style">Style or style getter function.</param>
    public HSplit(
        IReadOnlyList<IContainer> children,
        IContainer? windowTooSmall = null,
        VerticalAlign align = VerticalAlign.Justify,
        int padding = 0,
        char? paddingChar = null,
        string paddingStyle = "",
        int? width = null,
        int? height = null,
        int? zIndex = null,
        bool modal = false,
        IKeyBindingsBase? keyBindings = null,
        string style = "")
        : this(
            children,
            windowTooSmall,
            align,
            Dimension.Exact(padding),
            paddingChar,
            paddingStyle,
            width.HasValue ? Dimension.Exact(width.Value) : null,
            height.HasValue ? Dimension.Exact(height.Value) : null,
            zIndex,
            modal,
            keyBindings,
            () => style)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HSplit"/> class with full configuration.
    /// </summary>
    public HSplit(
        IReadOnlyList<IContainer> children,
        IContainer? windowTooSmall,
        VerticalAlign align,
        Dimension? padding,
        char? paddingChar,
        string paddingStyle,
        Dimension? width,
        Dimension? height,
        int? zIndex,
        bool modal,
        IKeyBindingsBase? keyBindings,
        Func<string> styleGetter)
    {
        ArgumentNullException.ThrowIfNull(children);

        _children = children.ToList(); // Defensive copy
        _windowTooSmallGetter = windowTooSmall is not null ? () => windowTooSmall : null;
        Align = align;
        Padding = padding ?? Dimension.Exact(0);
        PaddingChar = paddingChar;
        PaddingStyle = paddingStyle ?? "";
        _width = width;
        _height = height;
        _zIndex = zIndex;
        _modal = modal;
        _keyBindings = keyBindings;
        _styleGetter = styleGetter ?? (() => "");
    }

    /// <inheritdoc/>
    public void Reset()
    {
        foreach (var child in _children)
        {
            child.Reset();
        }
    }

    /// <inheritdoc/>
    public Dimension PreferredWidth(int maxAvailableWidth)
    {
        if (_width is not null)
        {
            return _width;
        }

        if (_children.Count == 0)
        {
            return new Dimension();
        }

        var dimensions = _children.Select(c => c.PreferredWidth(maxAvailableWidth)).ToList();
        return DimensionUtils.MaxLayoutDimensions(dimensions);
    }

    /// <inheritdoc/>
    public Dimension PreferredHeight(int width, int maxAvailableHeight)
    {
        if (_height is not null)
        {
            return _height;
        }

        var allChildren = GetAllChildren();
        if (allChildren.Count == 0)
        {
            return new Dimension(min: 0, max: 0, preferred: 0);
        }

        var dimensions = allChildren.Select(c => c.PreferredHeight(width, maxAvailableHeight)).ToList();
        return DimensionUtils.SumLayoutDimensions(dimensions);
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
        var sizes = DivideHeights(writePosition);
        var style = string.IsNullOrEmpty(parentStyle) ? _styleGetter() : $"{parentStyle} {_styleGetter()}";
        var effectiveZIndex = _zIndex ?? zIndex;

        if (sizes is null)
        {
            // Window too small - render the "too small" message
            var tooSmall = GetWindowTooSmall();
            tooSmall.WriteToScreen(screen, mouseHandlers, writePosition, style, eraseBg, effectiveZIndex);
        }
        else
        {
            var ypos = writePosition.YPos;
            var xpos = writePosition.XPos;
            var width = writePosition.Width;
            var allChildren = GetAllChildren();

            // Draw child panes
            for (int i = 0; i < sizes.Count; i++)
            {
                var s = sizes[i];
                var c = allChildren[i];

                c.WriteToScreen(
                    screen,
                    mouseHandlers,
                    new WritePosition(xpos, ypos, width, s),
                    style,
                    eraseBg,
                    effectiveZIndex);

                ypos += s;
            }

            // Fill in remaining space (if children refused to take all space)
            var remainingHeight = writePosition.YPos + writePosition.Height - ypos;
            if (remainingHeight > 0)
            {
                var remaining = GetRemainingSpaceWindow();
                remaining.WriteToScreen(
                    screen,
                    mouseHandlers,
                    new WritePosition(xpos, ypos, width, remainingHeight),
                    style,
                    eraseBg,
                    effectiveZIndex);
            }
        }
    }

    /// <inheritdoc/>
    public IKeyBindingsBase? GetKeyBindings() => _keyBindings;

    /// <inheritdoc/>
    public IReadOnlyList<IContainer> GetChildren() => _children;

    /// <summary>
    /// Gets all children including padding windows.
    /// </summary>
    private IReadOnlyList<IContainer> GetAllChildren()
    {
        // Use a simple hash of children count for cache key
        var cacheKey = _children.GetHashCode();

        return _childrenCache.Get(cacheKey, () =>
        {
            var result = new List<IContainer>();

            // Padding at top for Center/Bottom alignment
            if (Align is VerticalAlign.Center or VerticalAlign.Bottom)
            {
                result.Add(CreateFlexibleWindow());
            }

            // Children with padding between them
            for (int i = 0; i < _children.Count; i++)
            {
                result.Add(_children[i]);

                if (i < _children.Count - 1)
                {
                    result.Add(CreatePaddingWindow());
                }
            }

            // Padding at bottom for Center/Top alignment
            if (Align is VerticalAlign.Center or VerticalAlign.Top)
            {
                result.Add(CreateFlexibleWindow());
            }

            return result;
        });
    }

    /// <summary>
    /// Creates a flexible window for alignment.
    /// </summary>
    private static IContainer CreateFlexibleWindow()
    {
        // A window that takes up flexible space but prefers 0
        return new DummyWindow(new Dimension(preferred: 0));
    }

    /// <summary>
    /// Creates a padding window between children.
    /// </summary>
    private IContainer CreatePaddingWindow()
    {
        return new DummyWindow(
            Padding,
            PaddingChar?.ToString(),
            PaddingStyle);
    }

    /// <summary>
    /// Gets or creates the window-too-small container.
    /// </summary>
    private IContainer GetWindowTooSmall()
    {
        using (_lock.EnterScope())
        {
            if (_windowTooSmallCache is null)
            {
                _windowTooSmallCache = _windowTooSmallGetter?.Invoke() ?? CreateDefaultTooSmall();
            }
            return _windowTooSmallCache;
        }
    }

    /// <summary>
    /// Creates the default "window too small" container.
    /// </summary>
    private static IContainer CreateDefaultTooSmall()
    {
        return new DummyWindow(null, " Window too small... ", "class:window-too-small");
    }

    /// <summary>
    /// Gets or creates the remaining space window.
    /// </summary>
    private IContainer GetRemainingSpaceWindow()
    {
        using (_lock.EnterScope())
        {
            return _remainingSpaceWindow ??= new DummyWindow();
        }
    }

    /// <summary>
    /// Divides the available height among children.
    /// </summary>
    /// <param name="writePosition">The available region.</param>
    /// <returns>Heights for each child, or null if space is insufficient.</returns>
    private IReadOnlyList<int>? DivideHeights(WritePosition writePosition)
    {
        if (_children.Count == 0)
        {
            return [];
        }

        var width = writePosition.Width;
        var height = writePosition.Height;
        var allChildren = GetAllChildren();

        // Calculate height dimensions for all children (including padding)
        var dimensions = allChildren.Select(c => c.PreferredHeight(width, height)).ToList();

        // Sum dimensions
        var sumDimensions = DimensionUtils.SumLayoutDimensions(dimensions);

        // If minimum sizes exceed available space, return null (window too small)
        if (sumDimensions.Min > height)
        {
            return null;
        }

        // Start with minimum sizes
        var sizes = dimensions.Select(d => d.Min).ToList();

        // Get weighted iterator
        var weights = dimensions.Select(d => d.Weight).ToList();

        // Only proceed if there are items with positive weights
        if (!weights.Any(w => w > 0))
        {
            return sizes;
        }

        using var childGenerator = CollectionUtils.TakeUsingWeights(
            Enumerable.Range(0, dimensions.Count).ToList(),
            weights).GetEnumerator();

        childGenerator.MoveNext();
        var i = childGenerator.Current;

        // Phase 1: Increase until we meet the preferred size
        var preferredStop = Math.Min(height, sumDimensions.Preferred);
        var preferredDimensions = dimensions.Select(d => d.Preferred).ToList();
        var lastSum = sizes.Sum();
        var iterationsWithoutProgress = 0;
        var totalWeights = weights.Sum();

        while (sizes.Sum() < preferredStop)
        {
            if (sizes[i] < preferredDimensions[i])
            {
                sizes[i]++;
            }

            childGenerator.MoveNext();
            i = childGenerator.Current;

            // Check for progress periodically (after one "round" of weighted allocation)
            iterationsWithoutProgress++;
            if (iterationsWithoutProgress >= totalWeights)
            {
                var currentSum = sizes.Sum();
                if (currentSum == lastSum)
                {
                    // No progress in a full cycle - all items at their limit
                    break;
                }
                lastSum = currentSum;
                iterationsWithoutProgress = 0;
            }
        }

        // Phase 2: Increase until we use all available space (up to max)
        var maxStop = Math.Min(height, sumDimensions.Max);
        var maxDimensions = dimensions.Select(d => d.Max).ToList();
        lastSum = sizes.Sum();
        iterationsWithoutProgress = 0;

        while (sizes.Sum() < maxStop)
        {
            if (sizes[i] < maxDimensions[i])
            {
                sizes[i]++;
            }

            childGenerator.MoveNext();
            i = childGenerator.Current;

            // Check for progress periodically
            iterationsWithoutProgress++;
            if (iterationsWithoutProgress >= totalWeights)
            {
                var currentSum = sizes.Sum();
                if (currentSum == lastSum)
                {
                    // No progress in a full cycle - all items at their limit
                    break;
                }
                lastSum = currentSum;
                iterationsWithoutProgress = 0;
            }
        }

        return sizes;
    }

    /// <summary>
    /// Simple dummy window for padding and spacing.
    /// </summary>
    private sealed class DummyWindow : IContainer
    {
        private readonly Dimension _height;
        private readonly string? _char;
        private readonly string _style;

        public DummyWindow(Dimension? height = null, string? charText = null, string style = "")
        {
            _height = height ?? new Dimension();
            _char = charText;
            _style = style ?? "";
        }

        public bool IsModal => false;

        public void Reset() { }

        public Dimension PreferredWidth(int maxAvailableWidth) => new Dimension(preferred: 0);

        public Dimension PreferredHeight(int width, int maxAvailableHeight) => _height;

        public void WriteToScreen(
            Screen screen,
            MouseHandlers mouseHandlers,
            WritePosition writePosition,
            string parentStyle,
            bool eraseBg,
            int? zIndex)
        {
            var style = string.IsNullOrEmpty(parentStyle) ? _style : $"{parentStyle} {_style}";
            var charToUse = string.IsNullOrEmpty(_char) ? " " : _char;

            for (int y = writePosition.YPos; y < writePosition.YPos + writePosition.Height; y++)
            {
                for (int x = writePosition.XPos; x < writePosition.XPos + writePosition.Width; x++)
                {
                    screen[y, x] = Char.Create(charToUse, style);
                }
            }
        }

        public IKeyBindingsBase? GetKeyBindings() => null;

        public IReadOnlyList<IContainer> GetChildren() => [];
    }
}
