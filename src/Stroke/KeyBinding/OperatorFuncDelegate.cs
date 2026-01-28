namespace Stroke.KeyBinding;

/// <summary>
/// Callback signature for pending Vi operator functions.
/// </summary>
/// <param name="e">The key press event that completes the operator.</param>
/// <param name="textObject">The text object (placeholder type until ITextObject is defined).</param>
/// <returns><see cref="NotImplementedOrNone"/> indicating if the event was handled.</returns>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's operator function signature: <c>Callable[[KeyPressEvent, TextObject], None]</c>.
/// </para>
/// </remarks>
public delegate NotImplementedOrNone OperatorFuncDelegate(KeyPressEvent e, object? textObject);
