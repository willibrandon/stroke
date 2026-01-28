using System.Collections.Immutable;
using Stroke.Filters;

namespace Stroke.KeyBinding;

/// <summary>
/// A key bindings wrapper that applies a filter to all bindings.
/// </summary>
/// <remarks>
/// <para>
/// The wrapper filter is composed with each binding's filter using AND composition
/// per FR-026. If the wrapper filter returns false, all bindings are effectively inactive.
/// </para>
/// <para>
/// This type is thread-safe. Cache operations are synchronized using
/// <see cref="System.Threading.Lock"/>.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>ConditionalKeyBindings</c> class.
/// </para>
/// </remarks>
public sealed class ConditionalKeyBindings : KeyBindingsProxy
{
    private readonly IKeyBindingsBase _baseKeyBindings;
    private readonly IFilter _filter;

    /// <summary>
    /// Creates a new ConditionalKeyBindings wrapping the specified registry.
    /// </summary>
    /// <param name="keyBindings">The underlying key bindings registry.</param>
    /// <param name="filter">The filter to apply to all bindings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyBindings"/> or <paramref name="filter"/> is null.</exception>
    public ConditionalKeyBindings(IKeyBindingsBase keyBindings, IFilter filter)
    {
        ArgumentNullException.ThrowIfNull(keyBindings);
        ArgumentNullException.ThrowIfNull(filter);
        _baseKeyBindings = keyBindings;
        _filter = filter;
    }

    /// <summary>
    /// Gets the filter applied to all bindings.
    /// </summary>
    public IFilter Filter => _filter;

    /// <inheritdoc/>
    protected override object GetSourceVersion()
    {
        return _baseKeyBindings.Version;
    }

    /// <inheritdoc/>
    protected override IKeyBindingsBase CreateCachedBindings()
    {
        var baseBindings = _baseKeyBindings.Bindings;
        var result = new KeyBindings();

        foreach (var binding in baseBindings)
        {
            // Compose filter using AND: wrapper filter AND binding filter
            // Per FR-026 and FR-056
            var composedFilter = _filter.And(binding.Filter);

            // Create a new binding with the composed filter
            result.Add<KeyHandlerCallable>(
                [.. binding.Keys],
                filter: new FilterOrBool(composedFilter),
                eager: new FilterOrBool(binding.Eager),
                isGlobal: new FilterOrBool(binding.IsGlobal),
                saveBefore: binding.SaveBefore,
                recordInMacro: new FilterOrBool(binding.RecordInMacro)
            )(binding.Handler);
        }

        return result;
    }
}
