namespace Stroke.Core;

/// <summary>
/// A search 'query', associated with a search field (like a SearchToolbar).
/// </summary>
/// <remarks>
/// <para>
/// Every searchable <c>BufferControl</c> points to a <c>search_buffer_control</c>
/// (another <c>BufferControl</c>) which represents the search field. The
/// <see cref="SearchState"/> attached to that search field is used for storing the current
/// search query.
/// </para>
/// <para>
/// It is possible to have one search field for multiple <c>BufferControl</c>s. In
/// that case, they'll share the same <see cref="SearchState"/>.
/// If there are multiple <c>BufferControl</c>s that display the same <see cref="Buffer"/>,
/// then they can have a different <see cref="SearchState"/> each (if they have a different
/// search control).
/// </para>
/// <para>
/// This class is thread-safe. All property access is synchronized via internal locking.
/// Individual property get/set operations are atomic. Compound operations (e.g., read Text
/// then read Direction) are NOT guaranteed atomic; callers requiring atomicity for compound
/// operations must use external synchronization.
/// </para>
/// </remarks>
public sealed class SearchState
{
    private readonly Lock _lock = new();
    private string _text;
    private SearchDirection _direction;
    private Func<bool>? _ignoreCaseFilter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchState"/> class.
    /// </summary>
    /// <param name="text">The search text. Null values are converted to empty string.</param>
    /// <param name="direction">The search direction.</param>
    /// <param name="ignoreCase">
    /// The filter function for case-insensitive search. When <c>null</c>,
    /// <see cref="IgnoreCase"/> returns <c>false</c> (case-sensitive).
    /// </param>
    public SearchState(
        string text = "",
        SearchDirection direction = SearchDirection.Forward,
        Func<bool>? ignoreCase = null)
    {
        _text = text ?? "";
        _direction = direction;
        _ignoreCaseFilter = ignoreCase;
    }

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    /// <remarks>
    /// Setting to <c>null</c> is converted to empty string.
    /// This property is thread-safe; individual get/set operations are atomic.
    /// </remarks>
    public string Text
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _text;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _text = value ?? "";
            }
        }
    }

    /// <summary>
    /// Gets or sets the search direction.
    /// </summary>
    /// <remarks>
    /// This property is thread-safe; individual get/set operations are atomic.
    /// </remarks>
    public SearchDirection Direction
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _direction;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _direction = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the filter function for case-insensitive search.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When set to a non-null value, the <see cref="IgnoreCase"/> method will
    /// invoke this delegate to determine case sensitivity at runtime.
    /// </para>
    /// <para>
    /// This property is thread-safe; individual get/set operations are atomic.
    /// </para>
    /// </remarks>
    public Func<bool>? IgnoreCaseFilter
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _ignoreCaseFilter;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _ignoreCaseFilter = value;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether to ignore case during search.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the search should ignore case (case-insensitive);
    /// <c>false</c> if the search should be case-sensitive.
    /// Returns <c>false</c> when <see cref="IgnoreCaseFilter"/> is <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method is thread-safe. The filter delegate is captured atomically,
    /// then invoked outside the lock to avoid potential deadlocks.
    /// </remarks>
    public bool IgnoreCase()
    {
        Func<bool>? filter;
        using (_lock.EnterScope())
        {
            filter = _ignoreCaseFilter;
        }
        return filter?.Invoke() ?? false;
    }

    /// <summary>
    /// Creates a new <see cref="SearchState"/> where backwards becomes forwards
    /// and vice versa.
    /// </summary>
    /// <returns>
    /// A new <see cref="SearchState"/> instance with the direction reversed,
    /// preserving the <see cref="Text"/> and <see cref="IgnoreCaseFilter"/> values.
    /// </returns>
    /// <remarks>
    /// This method is thread-safe. A consistent snapshot of the current state
    /// is captured atomically and used to create the new instance.
    /// </remarks>
    public SearchState Invert()
    {
        using (_lock.EnterScope())
        {
            var newDirection = _direction == SearchDirection.Backward
                ? SearchDirection.Forward
                : SearchDirection.Backward;

            return new SearchState(_text, newDirection, _ignoreCaseFilter);
        }
    }

    /// <summary>
    /// Returns a string representation of the search state for debugging.
    /// </summary>
    /// <returns>
    /// A string in the format: <c>SearchState("{Text}", direction={Direction}, ignoreCase={IgnoreCase()})</c>
    /// </returns>
    /// <remarks>
    /// This method is thread-safe. The state is captured atomically before formatting.
    /// </remarks>
    public override string ToString()
    {
        using (_lock.EnterScope())
        {
            var ignoreCase = _ignoreCaseFilter?.Invoke() ?? false;
            return $"SearchState(\"{_text}\", direction={_direction}, ignoreCase={ignoreCase})";
        }
    }
}
