using System.Collections.Immutable;
using Stroke.Filters;

namespace Stroke.KeyBinding;

/// <summary>
/// A key bindings wrapper that merges multiple registries.
/// </summary>
/// <remarks>
/// <para>
/// Bindings from all registries are combined in registration order.
/// Changes to any underlying registry are reflected in the merged view.
/// </para>
/// <para>
/// This type is thread-safe. Cache operations are synchronized using
/// <see cref="System.Threading.Lock"/>.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>_MergedKeyBindings</c> class.
/// </para>
/// </remarks>
public sealed class MergedKeyBindings : KeyBindingsProxy
{
    private readonly IReadOnlyList<IKeyBindingsBase> _registries;

    /// <summary>
    /// Creates a new MergedKeyBindings from the specified registries.
    /// </summary>
    /// <param name="registries">The registries to merge.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registries"/> is null.</exception>
    public MergedKeyBindings(IEnumerable<IKeyBindingsBase> registries)
    {
        ArgumentNullException.ThrowIfNull(registries);
        _registries = registries.ToImmutableArray();
    }

    /// <summary>
    /// Creates a new MergedKeyBindings from the specified registries.
    /// </summary>
    /// <param name="registries">The registries to merge.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registries"/> is null.</exception>
    public MergedKeyBindings(params IKeyBindingsBase[] registries)
        : this((IEnumerable<IKeyBindingsBase>)registries)
    {
    }

    /// <summary>
    /// Gets the underlying registries.
    /// </summary>
    public IReadOnlyList<IKeyBindingsBase> Registries => _registries;

    /// <inheritdoc/>
    protected override object GetSourceVersion()
    {
        // Version is a tuple of all child versions
        var versions = new object[_registries.Count];
        for (int i = 0; i < _registries.Count; i++)
        {
            versions[i] = _registries[i].Version;
        }
        return new CompositeVersion(versions);
    }

    /// <inheritdoc/>
    protected override IKeyBindingsBase CreateCachedBindings()
    {
        var result = new KeyBindings();

        foreach (var registry in _registries)
        {
            foreach (var binding in registry.Bindings)
            {
                result.Add<KeyHandlerCallable>(
                    [.. binding.Keys],
                    filter: new FilterOrBool(binding.Filter),
                    eager: new FilterOrBool(binding.Eager),
                    isGlobal: new FilterOrBool(binding.IsGlobal),
                    saveBefore: binding.SaveBefore,
                    recordInMacro: new FilterOrBool(binding.RecordInMacro)
                )(binding.Handler);
            }
        }

        return result;
    }

    /// <summary>
    /// Represents a composite version for tracking changes across multiple registries.
    /// </summary>
    private sealed class CompositeVersion : IEquatable<CompositeVersion>
    {
        private readonly object[] _versions;

        public CompositeVersion(object[] versions)
        {
            _versions = versions;
        }

        public override bool Equals(object? obj)
        {
            return obj is CompositeVersion other && Equals(other);
        }

        public bool Equals(CompositeVersion? other)
        {
            if (other is null) return false;
            if (_versions.Length != other._versions.Length) return false;

            for (int i = 0; i < _versions.Length; i++)
            {
                if (!Equals(_versions[i], other._versions[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var v in _versions)
            {
                hash.Add(v);
            }
            return hash.ToHashCode();
        }
    }
}
