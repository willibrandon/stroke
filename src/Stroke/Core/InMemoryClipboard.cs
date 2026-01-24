namespace Stroke.Core;

/// <summary>
/// Default clipboard implementation that stores data in memory with kill ring support.
/// </summary>
/// <remarks>
/// <para>
/// This implementation maintains a kill ring for Emacs-style yank-pop operations.
/// New data is added to the front of the ring, and <see cref="Rotate"/> moves
/// the front item to the back, cycling through clipboard history.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>clipboard.in_memory.InMemoryClipboard</c>.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is thread-safe. All public methods are synchronized
/// using <see cref="System.Threading.Lock"/>. Individual operations are atomic;
/// compound operations (e.g., read-modify-write sequences) require external synchronization.
/// </para>
/// </remarks>
public sealed class InMemoryClipboard : IClipboard
{
    private readonly LinkedList<ClipboardData> _ring = new();
    private readonly Lock _lock = new();

    /// <summary>
    /// Gets the maximum number of items the kill ring can hold.
    /// </summary>
    public int MaxSize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryClipboard"/> class.
    /// </summary>
    /// <param name="data">Optional initial clipboard data.</param>
    /// <param name="maxSize">Maximum number of items in the kill ring. Default is 60.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSize"/> is less than 1.</exception>
    public InMemoryClipboard(ClipboardData? data = null, int maxSize = 60)
    {
        if (maxSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), maxSize, "Must be at least 1.");
        }

        MaxSize = maxSize;

        if (data is not null)
        {
            _ring.AddFirst(data);
        }
    }

    /// <summary>
    /// Set data on the clipboard.
    /// </summary>
    /// <param name="data">The clipboard data to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public void SetData(ClipboardData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        using (_lock.EnterScope())
        {
            _ring.AddFirst(data);

            // Trim oldest items if exceeding max size
            while (_ring.Count > MaxSize)
            {
                _ring.RemoveLast();
            }
        }
    }

    /// <summary>
    /// Return the current clipboard data.
    /// </summary>
    /// <returns>
    /// The most recently stored <see cref="ClipboardData"/>, or an empty instance if the clipboard is empty.
    /// </returns>
    public ClipboardData GetData()
    {
        using (_lock.EnterScope())
        {
            return _ring.First?.Value ?? new ClipboardData();
        }
    }

    /// <summary>
    /// Set plain text on the clipboard with <see cref="SelectionType.Characters"/> type.
    /// </summary>
    /// <param name="text">The text to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    public void SetText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        SetData(new ClipboardData(text));
    }

    /// <summary>
    /// Rotate the kill ring, moving the current item to the back.
    /// </summary>
    /// <remarks>
    /// After rotation, <see cref="GetData"/> returns the next item in the ring.
    /// Rotating a full cycle returns to the original item.
    /// </remarks>
    public void Rotate()
    {
        using (_lock.EnterScope())
        {
            if (_ring.Count <= 1)
            {
                return; // Nothing to rotate
            }

            // Move first item to last
            var first = _ring.First!.Value;
            _ring.RemoveFirst();
            _ring.AddLast(first);
        }
    }
}
