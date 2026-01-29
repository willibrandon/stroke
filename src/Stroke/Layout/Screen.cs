using Stroke.Core.Primitives;

namespace Stroke.Layout;

/// <summary>
/// Two-dimensional buffer of <see cref="Char"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// Provides sparse storage for terminal screen content with defaultdict-like access patterns.
/// Accessing unset positions returns a configurable default character.
/// </para>
/// <para>
/// Supports cursor and menu position tracking per window, zero-width escape sequence
/// storage, and deferred float drawing with z-index ordering.
/// </para>
/// <para>
/// This type is thread-safe. All mutable operations are synchronized using
/// <see cref="System.Threading.Lock"/>.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Screen</c> class from <c>layout/screen.py</c>.
/// </para>
/// </remarks>
public sealed class Screen
{
    private readonly Lock _lock = new();
    private readonly int _initialWidth;
    private readonly int _initialHeight;

    // Sparse storage: row -> (col -> Char)
    private readonly Dictionary<int, Dictionary<int, Char>> _dataBuffer = [];

    // Zero-width escape sequences: (row, col) -> concatenated escapes
    private readonly Dictionary<(int Row, int Col), string> _zeroWidthEscapes = [];

    // Per-window cursor and menu positions
    private readonly Dictionary<IWindow, Point> _cursorPositions = [];
    private readonly Dictionary<IWindow, Point> _menuPositions = [];

    // Visible windows tracking
    private readonly Dictionary<IWindow, WritePosition> _visibleWindowsToWritePositions = [];

    // Deferred draw queue: (z-index, insertion order, draw function)
    private readonly List<(int ZIndex, int Order, Action DrawFunc)> _drawFloatFunctions = [];
    private int _drawOrder;

    // Current screen dimensions (auto-expand on write)
    private int _width;
    private int _height;
    private bool _showCursor = true;

    /// <summary>
    /// Gets the default character used for empty cells.
    /// </summary>
    public Char DefaultChar { get; }

    /// <summary>
    /// Gets or sets the current screen width.
    /// </summary>
    /// <remarks>
    /// This value may increase when writing to positions beyond the current width.
    /// </remarks>
    public int Width
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _width;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _width = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the current screen height.
    /// </summary>
    /// <remarks>
    /// This value may increase when writing to positions beyond the current height.
    /// </remarks>
    public int Height
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _height;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _height = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the cursor should be visible.
    /// </summary>
    public bool ShowCursor
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _showCursor;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _showCursor = value;
            }
        }
    }

    /// <summary>
    /// Gets the underlying data buffer for direct access.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exposes the sparse storage dictionary where row indices map to
    /// column dictionaries containing <see cref="Char"/> instances.
    /// </para>
    /// <para>
    /// Equivalent to Python Prompt Toolkit's <c>data_buffer</c> attribute.
    /// Used by renderer and layout components for efficient screen traversal.
    /// </para>
    /// <para>
    /// <b>Warning:</b> Direct modification bypasses dimension tracking and
    /// thread synchronization. Prefer using the indexer for normal operations.
    /// </para>
    /// </remarks>
    public IDictionary<int, Dictionary<int, Char>> DataBuffer => _dataBuffer;

    /// <summary>
    /// Gets the zero-width escape sequence storage for direct access.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Maps (row, column) positions to concatenated escape sequences.
    /// These sequences are invisible but may contain terminal commands
    /// like hyperlinks or title changes.
    /// </para>
    /// <para>
    /// Equivalent to Python Prompt Toolkit's <c>zero_width_escapes</c> attribute.
    /// </para>
    /// <para>
    /// <b>Warning:</b> Direct modification bypasses thread synchronization.
    /// Prefer using <see cref="AddZeroWidthEscape"/> and <see cref="GetZeroWidthEscapes"/>.
    /// </para>
    /// </remarks>
    public IDictionary<(int Row, int Col), string> ZeroWidthEscapes => _zeroWidthEscapes;

    /// <summary>
    /// Gets the list of visible windows.
    /// </summary>
    /// <remarks>
    /// Returns a snapshot of window references from <see cref="VisibleWindowsToWritePositions"/>.
    /// </remarks>
    public IReadOnlyList<IWindow> VisibleWindows
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _visibleWindowsToWritePositions.Keys.ToList();
            }
        }
    }

    /// <summary>
    /// Gets the dictionary mapping windows to their write positions.
    /// </summary>
    /// <remarks>
    /// Windows add themselves to this dictionary when drawn.
    /// Note: For thread-safe modifications, external synchronization may be needed
    /// for compound operations on this dictionary.
    /// </remarks>
    public IDictionary<IWindow, WritePosition> VisibleWindowsToWritePositions
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _visibleWindowsToWritePositions;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Screen"/> class.
    /// </summary>
    /// <param name="defaultChar">
    /// The default character returned for unset positions.
    /// If null, uses a space with <see cref="Char.Transparent"/> style.
    /// </param>
    /// <param name="initialWidth">Initial screen width. Default is 0. Negative values are clamped to 0.</param>
    /// <param name="initialHeight">Initial screen height. Default is 0. Negative values are clamped to 0.</param>
    public Screen(Char? defaultChar = null, int initialWidth = 0, int initialHeight = 0)
    {
        DefaultChar = defaultChar ?? Char.Create(" ", Char.Transparent);
        _initialWidth = Math.Max(0, initialWidth);
        _initialHeight = Math.Max(0, initialHeight);
        _width = _initialWidth;
        _height = _initialHeight;
    }

    /// <summary>
    /// Gets or sets the character at the specified position.
    /// </summary>
    /// <param name="row">The row (y coordinate).</param>
    /// <param name="col">The column (x coordinate).</param>
    /// <returns>
    /// The character at the position, or <see cref="DefaultChar"/> if not set.
    /// </returns>
    /// <remarks>
    /// Setting a value creates the row dictionary if needed. Getting a value
    /// for an unset position does not create entries (sparse storage).
    /// </remarks>
    public Char this[int row, int col]
    {
        get
        {
            using (_lock.EnterScope())
            {
                if (_dataBuffer.TryGetValue(row, out var rowDict) &&
                    rowDict.TryGetValue(col, out var ch))
                {
                    return ch;
                }
                return DefaultChar;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                if (!_dataBuffer.TryGetValue(row, out var rowDict))
                {
                    rowDict = [];
                    _dataBuffer[row] = rowDict;
                }
                rowDict[col] = value;

                // Auto-expand dimensions (only for non-negative coords that exceed current size)
                if (col >= 0 && col + 1 > _width)
                {
                    _width = col + 1;
                }
                if (row >= 0 && row + 1 > _height)
                {
                    _height = row + 1;
                }
            }
        }
    }

    /// <summary>
    /// Sets the cursor position for a given window.
    /// </summary>
    /// <param name="window">The window to set the cursor for.</param>
    /// <param name="position">The cursor position.</param>
    /// <exception cref="ArgumentNullException"><paramref name="window"/> is <c>null</c>.</exception>
    public void SetCursorPosition(IWindow window, Point position)
    {
        ArgumentNullException.ThrowIfNull(window);

        using (_lock.EnterScope())
        {
            _cursorPositions[window] = position;
        }
    }

    /// <summary>
    /// Gets the cursor position for a given window.
    /// </summary>
    /// <param name="window">The window to get the cursor for.</param>
    /// <returns>
    /// The cursor position, or <see cref="Point.Zero"/> if not set.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="window"/> is <c>null</c>.</exception>
    public Point GetCursorPosition(IWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        using (_lock.EnterScope())
        {
            return _cursorPositions.TryGetValue(window, out var pos) ? pos : Point.Zero;
        }
    }

    /// <summary>
    /// Sets the menu position for a given window.
    /// </summary>
    /// <param name="window">The window to set the menu position for.</param>
    /// <param name="position">The menu anchor position.</param>
    /// <exception cref="ArgumentNullException"><paramref name="window"/> is <c>null</c>.</exception>
    public void SetMenuPosition(IWindow window, Point position)
    {
        ArgumentNullException.ThrowIfNull(window);

        using (_lock.EnterScope())
        {
            _menuPositions[window] = position;
        }
    }

    /// <summary>
    /// Gets the menu position for a given window.
    /// </summary>
    /// <param name="window">The window to get the menu position for.</param>
    /// <returns>
    /// The menu position if set; otherwise the cursor position if set;
    /// otherwise <see cref="Point.Zero"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="window"/> is <c>null</c>.</exception>
    public Point GetMenuPosition(IWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        using (_lock.EnterScope())
        {
            // Fallback chain: menu position -> cursor position -> Point.Zero
            if (_menuPositions.TryGetValue(window, out var menuPos))
            {
                return menuPos;
            }
            if (_cursorPositions.TryGetValue(window, out var cursorPos))
            {
                return cursorPos;
            }
            return Point.Zero;
        }
    }

    /// <summary>
    /// Adds a zero-width escape sequence at the specified position.
    /// </summary>
    /// <param name="row">The row (y coordinate).</param>
    /// <param name="col">The column (x coordinate).</param>
    /// <param name="escape">The escape sequence to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="escape"/> is <c>null</c>.</exception>
    /// <remarks>
    /// Multiple escapes at the same position are concatenated.
    /// Empty strings are ignored (no-op).
    /// </remarks>
    public void AddZeroWidthEscape(int row, int col, string escape)
    {
        ArgumentNullException.ThrowIfNull(escape);

        if (string.IsNullOrEmpty(escape))
        {
            return;
        }

        using (_lock.EnterScope())
        {
            var key = (row, col);
            if (_zeroWidthEscapes.TryGetValue(key, out var existing))
            {
                _zeroWidthEscapes[key] = existing + escape;
            }
            else
            {
                _zeroWidthEscapes[key] = escape;
            }
        }
    }

    /// <summary>
    /// Gets the zero-width escape sequences at the specified position.
    /// </summary>
    /// <param name="row">The row (y coordinate).</param>
    /// <param name="col">The column (x coordinate).</param>
    /// <returns>
    /// The concatenated escape sequences, or empty string if none.
    /// </returns>
    public string GetZeroWidthEscapes(int row, int col)
    {
        using (_lock.EnterScope())
        {
            return _zeroWidthEscapes.TryGetValue((row, col), out var escapes)
                ? escapes
                : string.Empty;
        }
    }

    /// <summary>
    /// Queues a draw function to be executed during <see cref="DrawAllFloats"/>.
    /// </summary>
    /// <param name="zIndex">The z-index determining draw order (lower = first).</param>
    /// <param name="drawFunc">The draw function to execute.</param>
    /// <remarks>
    /// Draw functions are executed in ascending z-index order.
    /// </remarks>
    public void DrawWithZIndex(int zIndex, Action drawFunc)
    {
        ArgumentNullException.ThrowIfNull(drawFunc);

        using (_lock.EnterScope())
        {
            _drawFloatFunctions.Add((zIndex, _drawOrder++, drawFunc));
        }
    }

    /// <summary>
    /// Executes all queued float draw functions in z-index order.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Functions are sorted by z-index and executed one at a time.
    /// For equal z-index values, functions execute in FIFO order (the order queued).
    /// If a draw function queues additional functions, they are processed
    /// in the same loop (iterative, not recursive).
    /// </para>
    /// <para>
    /// After completion, the draw function queue is empty.
    /// </para>
    /// <para>
    /// If a draw function throws an exception, the queue is cleared and
    /// the exception is re-thrown. Remaining functions are not executed.
    /// </para>
    /// </remarks>
    public void DrawAllFloats()
    {
        using (_lock.EnterScope())
        {
            try
            {
                // Process iteratively: sort and execute, repeat if new items were added
                while (_drawFloatFunctions.Count > 0)
                {
                    // Sort by z-index, then by insertion order
                    _drawFloatFunctions.Sort((a, b) =>
                    {
                        int cmp = a.ZIndex.CompareTo(b.ZIndex);
                        return cmp != 0 ? cmp : a.Order.CompareTo(b.Order);
                    });

                    // Take the first item (lowest z-index/order)
                    var (_, _, drawFunc) = _drawFloatFunctions[0];
                    _drawFloatFunctions.RemoveAt(0);

                    // Execute (may add more items to the queue)
                    drawFunc();
                }
            }
            catch
            {
                // Clear queue on exception
                _drawFloatFunctions.Clear();
                throw;
            }
        }
    }

    /// <summary>
    /// Fills a rectangular region with a style.
    /// </summary>
    /// <param name="writePosition">The region to fill.</param>
    /// <param name="style">The style to apply.</param>
    /// <param name="after">
    /// If <c>true</c>, appends style after existing style.
    /// If <c>false</c> (default), prepends style before existing style.
    /// </param>
    /// <remarks>
    /// <para>
    /// If <paramref name="style"/> is empty or whitespace-only, no changes are made.
    /// </para>
    /// <para>
    /// For each cell in the region, a new Char is created with the combined style.
    /// This may cause cells that didn't exist to be created with the default character
    /// plus the fill style.
    /// </para>
    /// </remarks>
    public void FillArea(WritePosition writePosition, string style = "", bool after = false)
    {
        if (string.IsNullOrWhiteSpace(style))
        {
            return;
        }

        using (_lock.EnterScope())
        {
            for (int row = writePosition.YPos; row < writePosition.YPos + writePosition.Height; row++)
            {
                for (int col = writePosition.XPos; col < writePosition.XPos + writePosition.Width; col++)
                {
                    var existing = this[row, col];
                    var newStyle = after
                        ? (string.IsNullOrEmpty(existing.Style) ? style : $"{existing.Style} {style}")
                        : (string.IsNullOrEmpty(existing.Style) ? style : $"{style} {existing.Style}");
                    this[row, col] = Char.Create(existing.Character, newStyle);
                }
            }
        }
    }

    /// <summary>
    /// Appends a style string to all characters currently in the screen.
    /// </summary>
    /// <param name="styleStr">The style string to append.</param>
    /// <remarks>
    /// Iterates all stored characters and creates new Char instances with
    /// the appended style. Uses character cache for efficiency.
    /// </remarks>
    public void AppendStyleToContent(string styleStr)
    {
        if (string.IsNullOrWhiteSpace(styleStr))
        {
            return;
        }

        using (_lock.EnterScope())
        {
            foreach (var rowEntry in _dataBuffer)
            {
                var rowDict = rowEntry.Value;
                var cols = rowDict.Keys.ToList();
                foreach (var col in cols)
                {
                    var existing = rowDict[col];
                    var newStyle = string.IsNullOrEmpty(existing.Style)
                        ? styleStr
                        : $"{existing.Style} {styleStr}";
                    rowDict[col] = Char.Create(existing.Character, newStyle);
                }
            }
        }
    }

    /// <summary>
    /// Clears all screen content and resets to initial state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Clears: data buffer, zero-width escapes, cursor positions, menu positions,
    /// draw queue, and visible windows.
    /// </para>
    /// <para>
    /// Resets Width and Height to the constructor's initialWidth and initialHeight values.
    /// </para>
    /// <para>
    /// Preserves: DefaultChar and ShowCursor.
    /// </para>
    /// </remarks>
    public void Clear()
    {
        using (_lock.EnterScope())
        {
            _dataBuffer.Clear();
            _zeroWidthEscapes.Clear();
            _cursorPositions.Clear();
            _menuPositions.Clear();
            _visibleWindowsToWritePositions.Clear();
            _drawFloatFunctions.Clear();
            _drawOrder = 0;
            _width = _initialWidth;
            _height = _initialHeight;
        }
    }
}
