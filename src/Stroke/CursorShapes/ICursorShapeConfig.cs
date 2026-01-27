namespace Stroke.CursorShapes;

/// <summary>
/// Configuration interface for determining cursor shape based on application state.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>CursorShapeConfig</c> protocol
/// from <c>prompt_toolkit.cursor_shapes</c>.
/// </para>
/// <para>
/// Implementations of this interface determine the cursor shape to display based on
/// the current application state (e.g., Vi navigation vs. insert mode).
/// </para>
/// </remarks>
public interface ICursorShapeConfig
{
    /// <summary>
    /// Gets the cursor shape for the current application state.
    /// </summary>
    /// <returns>The cursor shape to display.</returns>
    /// <remarks>
    /// <para>
    /// The application parameter is intentionally omitted in this simplified interface.
    /// Implementations that need application state should capture it via constructor or closure.
    /// </para>
    /// </remarks>
    CursorShape GetCursorShape();
}
