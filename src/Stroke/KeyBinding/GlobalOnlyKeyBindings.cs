using Stroke.Filters;

namespace Stroke.KeyBinding;

/// <summary>
/// A key bindings wrapper that exposes only global bindings.
/// </summary>
/// <remarks>
/// <para>
/// Only bindings where IsGlobal.Invoke() returns true are included.
/// The IsGlobal filter is evaluated at cache update time.
/// </para>
/// <para>
/// This type is thread-safe. Cache operations are synchronized using
/// <see cref="System.Threading.Lock"/>.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>_GlobalOnlyKeyBindings</c> class.
/// </para>
/// </remarks>
public sealed class GlobalOnlyKeyBindings : KeyBindingsProxy
{
    private readonly IKeyBindingsBase _baseKeyBindings;

    /// <summary>
    /// Creates a new GlobalOnlyKeyBindings wrapping the specified registry.
    /// </summary>
    /// <param name="keyBindings">The underlying key bindings registry.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyBindings"/> is null.</exception>
    public GlobalOnlyKeyBindings(IKeyBindingsBase keyBindings)
    {
        ArgumentNullException.ThrowIfNull(keyBindings);
        _baseKeyBindings = keyBindings;
    }

    /// <inheritdoc/>
    protected override object GetSourceVersion()
    {
        return _baseKeyBindings.Version;
    }

    /// <inheritdoc/>
    protected override IKeyBindingsBase CreateCachedBindings()
    {
        var result = new KeyBindings();

        foreach (var binding in _baseKeyBindings.Bindings)
        {
            // Only include bindings where IsGlobal returns true
            if (binding.IsGlobal.Invoke())
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
}
