namespace Stroke.Core;

/// <summary>
/// Very simple cache that discards the oldest item when the cache size is exceeded.
/// </summary>
/// <remarks>
/// <para>
/// This type is thread-safe. Individual operations are atomic.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>SimpleCache</c> class from <c>cache.py</c>.
/// </para>
/// </remarks>
/// <typeparam name="TKey">The type of cache keys. Must be non-null and implement proper equality.</typeparam>
/// <typeparam name="TValue">The type of cached values.</typeparam>
public sealed class SimpleCache<TKey, TValue> where TKey : notnull
{
    private readonly Lock _lock = new();
    private Dictionary<TKey, TValue> _data;
    private Queue<TKey> _keys;

    /// <summary>
    /// Gets the maximum number of entries the cache can hold.
    /// </summary>
    public int MaxSize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleCache{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="maxSize">Maximum number of entries. Default is 8.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSize"/> is less than or equal to 0.</exception>
    public SimpleCache(int maxSize = 8)
    {
        if (maxSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), maxSize, "Maximum size must be greater than 0.");
        }

        MaxSize = maxSize;
        _data = new Dictionary<TKey, TValue>();
        _keys = new Queue<TKey>();
    }

    /// <summary>
    /// Gets an object from the cache. If not found, calls <paramref name="getter"/> to resolve it
    /// and caches the result.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="getter">Function to invoke if the key is not in the cache.</param>
    /// <returns>The cached or newly computed value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getter"/> is null.</exception>
    public TValue Get(TKey key, Func<TValue> getter)
    {
        ArgumentNullException.ThrowIfNull(getter);

        using (_lock.EnterScope())
        {
            // Look in cache first.
            if (_data.TryGetValue(key, out var cachedValue))
            {
                return cachedValue;
            }

            // Not found? Get it.
            // Important: Call getter outside the cache mutation to match Python behavior
            // and prevent issues if getter throws.
            TValue value;
            try
            {
                value = getter();
            }
            catch
            {
                // If getter throws, don't modify cache state
                throw;
            }

            _data[key] = value;
            _keys.Enqueue(key);

            // Remove the oldest key when the size is exceeded.
            // Python uses > not >=, so we evict when count exceeds maxSize
            if (_data.Count > MaxSize)
            {
                var keyToRemove = _keys.Dequeue();
                if (_data.ContainsKey(keyToRemove))
                {
                    _data.Remove(keyToRemove);
                }
            }

            return value;
        }
    }

    /// <summary>
    /// Clears all entries from the cache.
    /// </summary>
    public void Clear()
    {
        using (_lock.EnterScope())
        {
            _data = new Dictionary<TKey, TValue>();
            _keys = new Queue<TKey>();
        }
    }
}
