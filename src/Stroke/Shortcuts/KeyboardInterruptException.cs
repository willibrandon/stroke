namespace Stroke.Shortcuts;

/// <summary>
/// Exception thrown when the user presses Ctrl-C during a prompt.
/// Default interrupt exception type for <c>PromptSession&lt;TResult&gt;</c>.
/// </summary>
/// <remarks>
/// Port of Python's <c>KeyboardInterrupt</c> built-in exception.
/// Users can configure a custom exception type via the
/// <c>interruptException</c> parameter of <c>PromptSession&lt;TResult&gt;</c>.
/// </remarks>
public sealed class KeyboardInterruptException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardInterruptException"/> class.
    /// </summary>
    public KeyboardInterruptException() : base("Keyboard interrupt.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardInterruptException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public KeyboardInterruptException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardInterruptException"/> class
    /// with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public KeyboardInterruptException(string message, Exception innerException)
        : base(message, innerException) { }
}
