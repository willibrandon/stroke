namespace Stroke.CursorShapes;

/// <summary>
/// Dynamic cursor shape configuration that delegates to another config.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DynamicCursorShapeConfig</c> pattern.
/// </para>
/// <para>
/// This class wraps a function that returns an <see cref="ICursorShapeConfig"/>,
/// allowing the cursor shape configuration to be changed at runtime.
/// </para>
/// <para>
/// This class is thread-safe. The getter function may be called from any thread.
/// </para>
/// </remarks>
public sealed class DynamicCursorShapeConfig : ICursorShapeConfig
{
    private readonly Func<ICursorShapeConfig?> _getConfigFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicCursorShapeConfig"/> class.
    /// </summary>
    /// <param name="getConfigFunc">
    /// Function that returns the current cursor shape configuration.
    /// May return null, in which case <see cref="CursorShape.NeverChange"/> is used.
    /// </param>
    public DynamicCursorShapeConfig(Func<ICursorShapeConfig?> getConfigFunc)
    {
        _getConfigFunc = getConfigFunc ?? throw new ArgumentNullException(nameof(getConfigFunc));
    }

    /// <inheritdoc/>
    public CursorShape GetCursorShape()
    {
        var config = _getConfigFunc();
        return config?.GetCursorShape() ?? CursorShape.NeverChange;
    }
}
