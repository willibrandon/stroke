namespace Stroke.CursorShapes;

/// <summary>
/// Modal cursor shape configuration based on editing mode state.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ModalCursorShapeConfig</c> pattern.
/// </para>
/// <para>
/// This class changes cursor shape based on the current editing mode:
/// <list type="bullet">
///   <item><description>Vi navigation mode: Block cursor</description></item>
///   <item><description>Vi insert mode: Beam cursor</description></item>
///   <item><description>Vi replace mode: Underline cursor</description></item>
///   <item><description>Emacs mode: Beam cursor (default)</description></item>
/// </list>
/// </para>
/// <para>
/// This class is thread-safe. The mode getter function may be called from any thread.
/// </para>
/// </remarks>
public sealed class ModalCursorShapeConfig : ICursorShapeConfig
{
    private readonly Func<EditingMode> _getModeFunc;
    private readonly CursorShape _navigationShape;
    private readonly CursorShape _insertShape;
    private readonly CursorShape _replaceShape;

    /// <summary>
    /// Represents the editing mode of the application.
    /// </summary>
    public enum EditingMode
    {
        /// <summary>
        /// Vi navigation mode (normal mode).
        /// </summary>
        ViNavigation,

        /// <summary>
        /// Vi insert mode.
        /// </summary>
        ViInsert,

        /// <summary>
        /// Vi replace mode.
        /// </summary>
        ViReplace,

        /// <summary>
        /// Emacs editing mode.
        /// </summary>
        Emacs
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalCursorShapeConfig"/> class.
    /// </summary>
    /// <param name="getModeFunc">Function that returns the current editing mode.</param>
    /// <param name="navigationShape">Cursor shape for Vi navigation mode (default: Block).</param>
    /// <param name="insertShape">Cursor shape for Vi insert and Emacs modes (default: Beam).</param>
    /// <param name="replaceShape">Cursor shape for Vi replace mode (default: Underline).</param>
    public ModalCursorShapeConfig(
        Func<EditingMode> getModeFunc,
        CursorShape navigationShape = CursorShape.Block,
        CursorShape insertShape = CursorShape.Beam,
        CursorShape replaceShape = CursorShape.Underline)
    {
        _getModeFunc = getModeFunc ?? throw new ArgumentNullException(nameof(getModeFunc));
        _navigationShape = navigationShape;
        _insertShape = insertShape;
        _replaceShape = replaceShape;
    }

    /// <inheritdoc/>
    public CursorShape GetCursorShape()
    {
        var mode = _getModeFunc();

        return mode switch
        {
            EditingMode.ViNavigation => _navigationShape,
            EditingMode.ViInsert => _insertShape,
            EditingMode.ViReplace => _replaceShape,
            EditingMode.Emacs => _insertShape,
            _ => CursorShape.NeverChange
        };
    }
}
