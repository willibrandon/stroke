using Stroke.Output.Internal;
using Stroke.Styles;

namespace Stroke.Output;

/// <summary>
/// Partial class containing color and attribute methods.
/// </summary>
public sealed partial class Vt100Output
{
    private readonly Dictionary<ColorDepth, EscapeCodeCache> _escapeCodeCaches = [];

    /// <inheritdoc/>
    public void SetAttributes(Attrs attrs, ColorDepth colorDepth)
    {
        var cache = GetOrCreateCache(colorDepth);
        var sequence = cache.GetEscapeSequence(attrs);
        WriteRaw(sequence);
    }

    /// <inheritdoc/>
    public void ResetAttributes()
    {
        WriteRaw("\x1b[0m");
    }

    private EscapeCodeCache GetOrCreateCache(ColorDepth colorDepth)
    {
        using (_lock.EnterScope())
        {
            if (!_escapeCodeCaches.TryGetValue(colorDepth, out var cache))
            {
                cache = new EscapeCodeCache(colorDepth);
                _escapeCodeCaches[colorDepth] = cache;
            }

            return cache;
        }
    }
}
