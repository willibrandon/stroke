using System.Collections.Concurrent;
using Stroke.Styles;

namespace Stroke.Lexers;

/// <summary>
/// Cache that converts Pygments token types into style class names.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python's internal <c>_TokenCache</c> class.
/// </para>
/// <para>
/// Converts token type paths like <c>["Name", "Exception"]</c> to style strings
/// like <c>"class:pygments.name.exception"</c>.
/// </para>
/// <para>
/// This class is thread-safe. Uses a <see cref="ConcurrentDictionary{TKey, TValue}"/>
/// internally for lock-free concurrent access.
/// </para>
/// </remarks>
internal sealed class TokenCache
{
    private readonly ConcurrentDictionary<string, string> _cache = new();

    /// <summary>
    /// Gets the style class name for the given token type path.
    /// </summary>
    /// <param name="tokenType">The token type path (e.g., ["Name", "Exception"]).</param>
    /// <returns>The style class name (e.g., "class:pygments.name.exception").</returns>
    /// <remarks>
    /// Results are cached. Repeated calls with equivalent token paths return
    /// the cached result without recomputation.
    /// </remarks>
    public string GetStyleClass(IReadOnlyList<string> tokenType)
    {
        // Create a cache key from the token type parts
        var key = string.Join(".", tokenType);

        return _cache.GetOrAdd(key, _ =>
        {
            // Use the existing PygmentsStyleUtils to convert token type to class name
            var className = PygmentsStyleUtils.PygmentsTokenToClassName(tokenType);
            return $"class:{className}";
        });
    }
}
