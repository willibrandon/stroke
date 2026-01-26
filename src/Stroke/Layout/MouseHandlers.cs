using Stroke.Input;
using Stroke.KeyBinding;

namespace Stroke.Layout;

/// <summary>
/// Two dimensional raster of callbacks for mouse events.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>MouseHandlers</c> class in <c>prompt_toolkit.layout.mouse_handlers</c>.
/// </para>
/// <para>
/// This class is used by the renderer during layout to register mouse handlers for UI elements.
/// The lifecycle is: created empty → populated via <see cref="SetMouseHandlerForRange"/> →
/// queried via <see cref="GetHandler"/> → cleared via <see cref="Clear"/> before next layout pass.
/// </para>
/// <para>
/// Uses sparse storage (nested dictionaries) for memory efficiency - only positions with handlers are stored.
/// Achieves O(1) lookup at any position.
/// </para>
/// <para>
/// Thread-safe per Constitution XI. All mutable operations use <see cref="Lock"/> synchronization.
/// Individual operations are atomic; compound operations (e.g., read-modify-write sequences)
/// require external synchronization by the caller.
/// </para>
/// </remarks>
public sealed class MouseHandlers
{
    private readonly Lock _lock = new();

    // Map y (row) to x (column) to handler
    // Matches Python's defaultdict(lambda: defaultdict(...)) pattern
    private readonly Dictionary<int, Dictionary<int, Func<MouseEvent, NotImplementedOrNone>>> _handlers = new();

    /// <summary>
    /// Default handler returned for positions without a registered handler.
    /// Returns <see cref="NotImplementedOrNone.NotImplemented"/> to signal event was not handled.
    /// </summary>
    /// <remarks>
    /// Matches Python's <c>dummy_callback</c> which returns <c>NotImplemented</c>.
    /// </remarks>
    private static NotImplementedOrNone DummyHandler(MouseEvent e) => NotImplementedOrNone.NotImplemented;

    /// <summary>
    /// Set a mouse handler for a rectangular region.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The range is defined with inclusive minimum and exclusive maximum bounds:
    /// xMin &lt;= x &lt; xMax and yMin &lt;= y &lt; yMax.
    /// </para>
    /// <para>
    /// If a handler already exists at a position within the range, the new handler replaces it.
    /// </para>
    /// <para>
    /// If xMin &gt;= xMax or yMin &gt;= yMax, no positions are affected (empty region).
    /// </para>
    /// </remarks>
    /// <param name="xMin">Minimum X coordinate (inclusive).</param>
    /// <param name="xMax">Maximum X coordinate (exclusive).</param>
    /// <param name="yMin">Minimum Y coordinate (inclusive).</param>
    /// <param name="yMax">Maximum Y coordinate (exclusive).</param>
    /// <param name="handler">The mouse handler callback. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> is null.</exception>
    public void SetMouseHandlerForRange(
        int xMin,
        int xMax,
        int yMin,
        int yMax,
        Func<MouseEvent, NotImplementedOrNone> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        // Empty region - nothing to do
        if (xMin >= xMax || yMin >= yMax)
            return;

        using (_lock.EnterScope())
        {
            for (int y = yMin; y < yMax; y++)
            {
                if (!_handlers.TryGetValue(y, out var row))
                {
                    row = new Dictionary<int, Func<MouseEvent, NotImplementedOrNone>>();
                    _handlers[y] = row;
                }

                for (int x = xMin; x < xMax; x++)
                {
                    row[x] = handler;
                }
            }
        }
    }

    /// <summary>
    /// Get the mouse handler at a specific position.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns a handler for any position. If no handler is registered at the specified
    /// position, returns a default handler that returns <see cref="NotImplementedOrNone.NotImplemented"/>.
    /// This matches Python's <c>defaultdict</c> behavior where accessing any position always returns a callable.
    /// </para>
    /// </remarks>
    /// <param name="x">X coordinate (column).</param>
    /// <param name="y">Y coordinate (row).</param>
    /// <returns>The handler at the position, or a default handler if none is registered.</returns>
    public Func<MouseEvent, NotImplementedOrNone> GetHandler(int x, int y)
    {
        using (_lock.EnterScope())
        {
            if (_handlers.TryGetValue(y, out var row) && row.TryGetValue(x, out var handler))
            {
                return handler;
            }

            return DummyHandler;
        }
    }

    /// <summary>
    /// Clear all handlers.
    /// </summary>
    /// <remarks>
    /// Removes all registered handlers. After calling this method,
    /// <see cref="GetHandler"/> will return the default handler for all positions.
    /// </remarks>
    public void Clear()
    {
        using (_lock.EnterScope())
        {
            _handlers.Clear();
        }
    }
}
