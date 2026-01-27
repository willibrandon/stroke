using Stroke.Core;

namespace Stroke.Styles;

/// <summary>
/// Style merging utilities.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>merge_styles</c> function
/// from <c>prompt_toolkit.styles.style</c>.
/// </para>
/// </remarks>
public static class StyleMerger
{
    /// <summary>
    /// Merge multiple Style objects into one.
    /// </summary>
    /// <param name="styles">The styles to merge. Null entries are filtered out.</param>
    /// <returns>A merged style. Later styles override earlier ones.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="styles"/> is null.</exception>
    public static IStyle MergeStyles(IEnumerable<IStyle?> styles)
    {
        ArgumentNullException.ThrowIfNull(styles);

        var nonNullStyles = styles.Where(s => s is not null).Cast<IStyle>().ToList();

        if (nonNullStyles.Count == 0)
        {
            return DummyStyle.Instance;
        }

        if (nonNullStyles.Count == 1)
        {
            return nonNullStyles[0];
        }

        return new MergedStyle(nonNullStyles);
    }
}

/// <summary>
/// Merge multiple Style objects into one.
/// This is supposed to ensure consistency: if any of the given styles changes,
/// then this style will be updated.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>_MergedStyle</c> class
/// from <c>prompt_toolkit.styles.style</c>.
/// </para>
/// <para>
/// This type is thread-safe. The style cache uses thread-safe SimpleCache.
/// </para>
/// </remarks>
internal sealed class MergedStyle : IStyle
{
    private readonly IReadOnlyList<IStyle> _styles;
    private readonly SimpleCache<object, Style> _styleCache = new(maxSize: 1);

    public MergedStyle(IReadOnlyList<IStyle> styles)
    {
        _styles = styles;
    }

    /// <summary>
    /// The Style object that has the other styles merged together.
    /// </summary>
    private Style GetMergedStyle()
    {
        return _styleCache.Get(InvalidationHash, () => new Style(StyleRules));
    }

    /// <inheritdoc/>
    public IReadOnlyList<(string ClassNames, string StyleDef)> StyleRules
    {
        get
        {
            var rules = new List<(string, string)>();
            foreach (var style in _styles)
            {
                rules.AddRange(style.StyleRules);
            }
            return rules.AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null)
    {
        return GetMergedStyle().GetAttrsForStyleStr(styleStr, @default);
    }

    /// <inheritdoc/>
    public object InvalidationHash
    {
        get
        {
            // Create a tuple of all underlying invalidation hashes
            var hashes = _styles.Select(s => s.InvalidationHash).ToArray();
            return new InvalidationHashTuple(hashes);
        }
    }

    /// <summary>
    /// A tuple of invalidation hashes that implements proper equality.
    /// </summary>
    private sealed class InvalidationHashTuple : IEquatable<InvalidationHashTuple>
    {
        private readonly object[] _hashes;
        private readonly int _cachedHashCode;

        public InvalidationHashTuple(object[] hashes)
        {
            _hashes = hashes;
            _cachedHashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            var hash = new HashCode();
            foreach (var h in _hashes)
            {
                hash.Add(h);
            }
            return hash.ToHashCode();
        }

        public bool Equals(InvalidationHashTuple? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (_hashes.Length != other._hashes.Length) return false;

            for (int i = 0; i < _hashes.Length; i++)
            {
                if (!Equals(_hashes[i], other._hashes[i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object? obj) => Equals(obj as InvalidationHashTuple);
        public override int GetHashCode() => _cachedHashCode;
    }
}
