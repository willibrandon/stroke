namespace Stroke.Core;

/// <summary>
/// Fast, lightweight cache which keeps at most <see cref="Size"/> items.
/// It will discard the oldest items in the cache first (FIFO eviction).
/// </summary>
/// <remarks>
/// <para>
/// The cache provides dictionary-style indexer access. Accessing a missing key
/// automatically invokes the factory function to create and cache the value.
/// </para>
/// <para>
/// This type is thread-safe. Individual operations are atomic.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>FastDictCache</c> class from <c>cache.py</c>.
/// </para>
/// </remarks>
/// <typeparam name="TKey">The type of cache keys. Must be non-null and implement proper equality.</typeparam>
/// <typeparam name="TValue">The type of cached values.</typeparam>
public sealed class FastDictCache<TKey, TValue> where TKey : notnull
{
    private readonly Lock _lock = new();
    private readonly Dictionary<TKey, TValue> _data;
    private readonly Queue<TKey> _keys;
    private readonly Func<TKey, TValue> _getValue;

    /// <summary>
    /// Gets the maximum number of entries the cache can hold.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets the current number of entries in the cache.
    /// </summary>
    public int Count
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _data.Count;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FastDictCache{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="getValue">Factory function called when accessing a missing key.</param>
    /// <param name="size">Maximum number of entries. Default is 1,000,000.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getValue"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="size"/> is less than or equal to 0.</exception>
    public FastDictCache(Func<TKey, TValue> getValue, int size = 1_000_000)
    {
        ArgumentNullException.ThrowIfNull(getValue);

        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be greater than 0.");
        }

        _getValue = getValue;
        Size = size;
        _data = new Dictionary<TKey, TValue>();
        _keys = new Queue<TKey>();
    }

    /// <summary>
    /// Gets the cached value for the specified key. If the key is not in the cache,
    /// the factory function is invoked to create and cache the value.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The cached or newly computed value.</returns>
    public TValue this[TKey key]
    {
        get
        {
            using (_lock.EnterScope())
            {
                // Look in cache first.
                if (_data.TryGetValue(key, out var cachedValue))
                {
                    return cachedValue;
                }

                // Remove the oldest key when the size is exceeded (BEFORE adding new entry).
                // Python uses > not >=, so we check if current count already exceeds size.
                if (_data.Count > Size)
                {
                    var keyToRemove = _keys.Dequeue();
                    if (_data.ContainsKey(keyToRemove))
                    {
                        _data.Remove(keyToRemove);
                    }
                }

                // Not found? Get it.
                TValue value;
                try
                {
                    value = _getValue(key);
                }
                catch
                {
                    // If factory throws, don't modify cache state
                    throw;
                }

                _data[key] = value;
                _keys.Enqueue(key);

                return value;
            }
        }
    }

    /// <summary>
    /// Determines whether the cache contains the specified key.
    /// This method does NOT invoke the factory function.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the cache contains the key; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(TKey key)
    {
        using (_lock.EnterScope())
        {
            return _data.ContainsKey(key);
        }
    }

    /// <summary>
    /// Gets the value associated with the specified key if it exists in the cache.
    /// This method does NOT invoke the factory function.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">When this method returns, contains the value associated with the key,
    /// or the default value if the key was not found.</param>
    /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
        using (_lock.EnterScope())
        {
            return _data.TryGetValue(key, out value!);
        }
    }
}
