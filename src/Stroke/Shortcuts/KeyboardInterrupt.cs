namespace Stroke.Shortcuts;

/// <summary>
/// Exception raised when the user presses Ctrl+C during a prompt.
/// </summary>
/// <remarks>
/// <para>
/// This exception mirrors Python's <c>KeyboardInterrupt</c> behavior.
/// It is the default exception type for <see cref="ChoiceInput{T}"/> interrupt handling.
/// </para>
/// <para>
/// Thread safety: This class is inherently thread-safe as exceptions are immutable after construction.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's usage of <c>KeyboardInterrupt</c> for interrupt handling
/// in <c>prompt_toolkit.shortcuts.choice_input</c>.
/// </para>
/// </remarks>
public class KeyboardInterrupt : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardInterrupt"/> class.
    /// </summary>
    public KeyboardInterrupt()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardInterrupt"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public KeyboardInterrupt(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardInterrupt"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or a null reference
    /// if no inner exception is specified.
    /// </param>
    public KeyboardInterrupt(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
