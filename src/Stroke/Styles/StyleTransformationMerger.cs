namespace Stroke.Styles;

/// <summary>
/// Style transformation merging utilities.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>merge_style_transformations</c>
/// function from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// </remarks>
public static class StyleTransformationMerger
{
    /// <summary>
    /// Merge multiple style transformations together.
    /// </summary>
    /// <param name="transformations">Transformations to merge. Null entries are filtered out.</param>
    /// <returns>A merged transformation that applies all transformations in sequence.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformations"/> is null.</exception>
    public static IStyleTransformation MergeStyleTransformations(
        IEnumerable<IStyleTransformation?> transformations)
    {
        ArgumentNullException.ThrowIfNull(transformations);

        var nonNullTransformations = transformations
            .Where(t => t is not null)
            .Cast<IStyleTransformation>()
            .ToList();

        if (nonNullTransformations.Count == 0)
        {
            return DummyStyleTransformation.Instance;
        }

        if (nonNullTransformations.Count == 1)
        {
            return nonNullTransformations[0];
        }

        return new MergedStyleTransformation(nonNullTransformations);
    }
}

/// <summary>
/// A merged style transformation that applies multiple transformations in sequence.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>_MergedStyleTransformation</c>
/// internal class from <c>prompt_toolkit.styles.style_transformation</c>.
/// </para>
/// <para>
/// This type is thread-safe. All underlying transformations are applied in sequence.
/// </para>
/// </remarks>
internal sealed class MergedStyleTransformation : IStyleTransformation
{
    private readonly IReadOnlyList<IStyleTransformation> _transformations;

    public MergedStyleTransformation(IReadOnlyList<IStyleTransformation> transformations)
    {
        _transformations = transformations;
    }

    /// <inheritdoc/>
    public Attrs TransformAttrs(Attrs attrs)
    {
        foreach (var transformation in _transformations)
        {
            attrs = transformation.TransformAttrs(attrs);
        }
        return attrs;
    }

    /// <inheritdoc/>
    public object InvalidationHash
    {
        get
        {
            var hashes = _transformations.Select(t => t.InvalidationHash).ToArray();
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
