using System.Collections.Immutable;
using Stroke.Core;
using Stroke.Filters;
using Stroke.Input;

namespace Stroke.KeyBinding;

/// <summary>
/// Concrete mutable key binding registry with add/remove capabilities and caching.
/// </summary>
/// <remarks>
/// <para>
/// This class is thread-safe. All operations are protected by an internal lock.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>KeyBindings</c> class.
/// </para>
/// </remarks>
public sealed class KeyBindings : IKeyBindingsBase
{
    private readonly Lock _lock = new();
    private readonly List<Binding> _bindings = [];
    private int _version;

    // Caches use ImmutableArray<KeyOrChar> as key for structural equality
    private readonly SimpleCache<ImmutableArray<KeyOrChar>, IReadOnlyList<Binding>> _forKeysCache;
    private readonly SimpleCache<ImmutableArray<KeyOrChar>, IReadOnlyList<Binding>> _startingCache;

    /// <summary>
    /// Creates an empty KeyBindings registry.
    /// </summary>
    public KeyBindings()
    {
        _forKeysCache = new SimpleCache<ImmutableArray<KeyOrChar>, IReadOnlyList<Binding>>(10_000);
        _startingCache = new SimpleCache<ImmutableArray<KeyOrChar>, IReadOnlyList<Binding>>(1_000);
    }

    /// <inheritdoc/>
    public object Version
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _version;
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> Bindings
    {
        get
        {
            using (_lock.EnterScope())
            {
                // Return a snapshot
                return [.. _bindings];
            }
        }
    }

    /// <summary>
    /// Adds a key binding. Returns a decorator function for method chaining.
    /// </summary>
    /// <typeparam name="T">Handler type (KeyHandlerCallable or Binding).</typeparam>
    /// <param name="keys">Key sequence to bind.</param>
    /// <param name="filter">Activation filter (default: true).</param>
    /// <param name="eager">Eager matching filter (default: false).</param>
    /// <param name="isGlobal">Global binding filter (default: false).</param>
    /// <param name="saveBefore">Save-before callback (default: always returns true).</param>
    /// <param name="recordInMacro">Macro recording filter (default: true).</param>
    /// <returns>Decorator function that returns the input unchanged after adding the binding.</returns>
    /// <exception cref="ArgumentException">Keys is empty.</exception>
    /// <exception cref="ArgumentNullException">Keys is null.</exception>
    public Func<T, T> Add<T>(
        KeyOrChar[] keys,
        FilterOrBool filter = default,
        FilterOrBool eager = default,
        FilterOrBool isGlobal = default,
        Func<KeyPressEvent, bool>? saveBefore = null,
        FilterOrBool recordInMacro = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(keys);
        if (keys.Length == 0)
        {
            throw new ArgumentException("Keys must not be empty.", nameof(keys));
        }

        // Check for Never filter optimization (FR-025)
        // If filter is explicitly Never, don't store the binding
        if (filter.IsFilter && filter.FilterValue is Never)
        {
            return input => input;
        }

        return input =>
        {
            Binding binding;

            if (input is Binding existingBinding)
            {
                // Compose filters with the existing binding per FR-026:
                // filter: AND composition
                // eager: OR composition
                // isGlobal: OR composition
                var composedFilter = ComposeFilterAnd(existingBinding.Filter, filter);
                var composedEager = ComposeFilterOr(existingBinding.Eager, eager);
                var composedIsGlobal = ComposeFilterOr(existingBinding.IsGlobal, isGlobal);

                binding = new Binding(
                    keys,
                    existingBinding.Handler,
                    new FilterOrBool(composedFilter),
                    new FilterOrBool(composedEager),
                    new FilterOrBool(composedIsGlobal),
                    saveBefore ?? existingBinding.SaveBefore,
                    recordInMacro.HasValue
                        ? recordInMacro
                        : new FilterOrBool(existingBinding.RecordInMacro));
            }
            else if (input is KeyHandlerCallable handler)
            {
                binding = new Binding(
                    keys,
                    handler,
                    filter,
                    eager,
                    isGlobal,
                    saveBefore,
                    recordInMacro);
            }
            else
            {
                throw new ArgumentException(
                    $"Input must be a KeyHandlerCallable or Binding, got {input?.GetType().Name ?? "null"}",
                    nameof(input));
            }

            using (_lock.EnterScope())
            {
                _bindings.Add(binding);
                _version++;
                ClearCaches();
            }

            return input;
        };
    }

    /// <summary>
    /// Removes bindings by handler reference.
    /// </summary>
    /// <param name="handler">The handler to remove.</param>
    /// <exception cref="ArgumentNullException">Handler is null.</exception>
    /// <exception cref="InvalidOperationException">No binding found for handler.</exception>
    public void Remove(KeyHandlerCallable handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        using (_lock.EnterScope())
        {
            int removed = _bindings.RemoveAll(b => ReferenceEquals(b.Handler, handler));

            if (removed == 0)
            {
                throw new InvalidOperationException("No binding found for the specified handler.");
            }

            _version++;
            ClearCaches();
        }
    }

    /// <summary>
    /// Removes bindings by key sequence.
    /// </summary>
    /// <param name="keys">The key sequence to remove.</param>
    /// <exception cref="ArgumentNullException">Keys is null.</exception>
    /// <exception cref="ArgumentException">Keys is empty.</exception>
    /// <exception cref="InvalidOperationException">No binding found for keys.</exception>
    public void Remove(params KeyOrChar[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        if (keys.Length == 0)
        {
            throw new ArgumentException("Keys must not be empty.", nameof(keys));
        }

        using (_lock.EnterScope())
        {
            int removed = _bindings.RemoveAll(b => KeysEqual(b.Keys, keys));

            if (removed == 0)
            {
                throw new InvalidOperationException("No binding found for the specified key sequence.");
            }

            _version++;
            ClearCaches();
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyOrChar> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        if (keys.Count == 0)
        {
            return [];
        }

        var cacheKey = keys as ImmutableArray<KeyOrChar>? ?? [.. keys];

        // Take a snapshot of bindings BEFORE entering cache lock to avoid deadlock.
        // The cache callback must not acquire _lock while cache lock is held.
        List<Binding> snapshot;
        using (_lock.EnterScope())
        {
            snapshot = [.. _bindings];
        }

        return _forKeysCache.Get(cacheKey, () =>
        {
            List<Binding> matches = [];

            foreach (var binding in snapshot)
            {
                if (MatchesKeys(binding.Keys, keys))
                {
                    matches.Add(binding);
                }
            }

            // Sort by Any count (fewer wildcards = higher priority)
            matches.Sort((a, b) => a.AnyCount.CompareTo(b.AnyCount));

            return matches.AsReadOnly();
        });
    }

    /// <inheritdoc/>
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyOrChar> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        if (keys.Count == 0)
        {
            // Return all bindings when prefix is empty
            using (_lock.EnterScope())
            {
                return [.. _bindings];
            }
        }

        var cacheKey = keys as ImmutableArray<KeyOrChar>? ?? [.. keys];

        // Take a snapshot of bindings BEFORE entering cache lock to avoid deadlock.
        // The cache callback must not acquire _lock while cache lock is held.
        List<Binding> snapshot;
        using (_lock.EnterScope())
        {
            snapshot = [.. _bindings];
        }

        return _startingCache.Get(cacheKey, () =>
        {
            List<Binding> matches = [];

            foreach (var binding in snapshot)
            {
                // Binding must be longer than the prefix
                if (binding.Keys.Count > keys.Count && StartsWithPrefix(binding.Keys, keys))
                {
                    matches.Add(binding);
                }
            }

            return matches.AsReadOnly();
        });
    }

    // Backwards compatibility aliases

    /// <summary>Alias for <see cref="Add{T}"/>.</summary>
    public Func<T, T> AddBinding<T>(
        KeyOrChar[] keys,
        FilterOrBool filter = default,
        FilterOrBool eager = default,
        FilterOrBool isGlobal = default,
        Func<KeyPressEvent, bool>? saveBefore = null,
        FilterOrBool recordInMacro = default) where T : class
        => Add<T>(keys, filter, eager, isGlobal, saveBefore, recordInMacro);

    /// <summary>Alias for <see cref="Remove(KeyHandlerCallable)"/>.</summary>
    public void RemoveBinding(KeyHandlerCallable handler) => Remove(handler);

    /// <summary>Alias for <see cref="Remove(KeyOrChar[])"/>.</summary>
    public void RemoveBinding(params KeyOrChar[] keys) => Remove(keys);

    /// <summary>
    /// Clears the lookup caches. Called when bindings are modified.
    /// Must be called within a lock.
    /// </summary>
    private void ClearCaches()
    {
        _forKeysCache.Clear();
        _startingCache.Clear();
    }

    /// <summary>
    /// Checks if a binding's key sequence matches the query keys (including wildcards).
    /// </summary>
    private static bool MatchesKeys(IReadOnlyList<KeyOrChar> bindingKeys, IReadOnlyList<KeyOrChar> queryKeys)
    {
        if (bindingKeys.Count != queryKeys.Count)
        {
            return false;
        }

        for (int i = 0; i < bindingKeys.Count; i++)
        {
            var bindingKey = bindingKeys[i];
            var queryKey = queryKeys[i];

            // Keys.Any matches anything
            if (bindingKey.IsKey && bindingKey.Key == Keys.Any)
            {
                continue;
            }

            // Exact match required
            if (!bindingKey.Equals(queryKey))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if a binding's key sequence starts with the given prefix.
    /// </summary>
    private static bool StartsWithPrefix(IReadOnlyList<KeyOrChar> bindingKeys, IReadOnlyList<KeyOrChar> prefix)
    {
        if (bindingKeys.Count < prefix.Count)
        {
            return false;
        }

        for (int i = 0; i < prefix.Count; i++)
        {
            var bindingKey = bindingKeys[i];
            var prefixKey = prefix[i];

            // Keys.Any matches anything
            if (bindingKey.IsKey && bindingKey.Key == Keys.Any)
            {
                continue;
            }

            // Exact match required
            if (!bindingKey.Equals(prefixKey))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if two key sequences are equal.
    /// </summary>
    private static bool KeysEqual(IReadOnlyList<KeyOrChar> a, IReadOnlyList<KeyOrChar> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        for (int i = 0; i < a.Count; i++)
        {
            if (!a[i].Equals(b[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Composes filters with AND logic.
    /// </summary>
    private static IFilter ComposeFilterAnd(IFilter existing, FilterOrBool added)
    {
        // If no value was explicitly provided, use existing filter unchanged
        if (!added.HasValue)
        {
            return existing;
        }

        var addedFilter = FilterUtils.ToFilter(added);
        return existing.And(addedFilter);
    }

    /// <summary>
    /// Composes filters with OR logic.
    /// </summary>
    private static IFilter ComposeFilterOr(IFilter existing, FilterOrBool added)
    {
        // If no value was explicitly provided, use existing filter unchanged
        if (!added.HasValue)
        {
            return existing;
        }

        var addedFilter = FilterUtils.ToFilter(added);
        return existing.Or(addedFilter);
    }
}
