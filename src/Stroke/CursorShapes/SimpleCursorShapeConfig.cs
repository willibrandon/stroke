namespace Stroke.CursorShapes;

/// <summary>
/// Simple cursor shape configuration with a fixed cursor shape.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SimpleCursorShapeConfig</c> pattern.
/// </para>
/// <para>
/// This class is inherently thread-safe as it maintains no mutable state.
/// </para>
/// </remarks>
public sealed class SimpleCursorShapeConfig : ICursorShapeConfig
{
    private readonly CursorShape _cursorShape;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleCursorShapeConfig"/> class.
    /// </summary>
    /// <param name="cursorShape">The fixed cursor shape to use.</param>
    public SimpleCursorShapeConfig(CursorShape cursorShape = CursorShape.NeverChange)
    {
        _cursorShape = cursorShape;
    }

    /// <inheritdoc/>
    public CursorShape GetCursorShape() => _cursorShape;
}
