namespace Stroke.KeyBinding;

/// <summary>
/// Callback signature for pending Vi operator functions.
/// </summary>
/// <param name="e">The key press event that completes the operator.</param>
/// <param name="textObject">The text object describing the range to operate on.</param>
/// <returns><see cref="NotImplementedOrNone"/> indicating if the event was handled.</returns>
/// <remarks>
/// Port of Python Prompt Toolkit's operator function signature:
/// <c>Callable[[KeyPressEvent, TextObject], None]</c>.
/// </remarks>
public delegate NotImplementedOrNone OperatorFuncDelegate(KeyPressEvent e, TextObject textObject);
