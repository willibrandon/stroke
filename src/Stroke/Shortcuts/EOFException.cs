namespace Stroke.Shortcuts;

/// <summary>
/// Exception thrown when the user presses Ctrl-D on an empty buffer during a prompt.
/// Default EOF exception type for <c>PromptSession&lt;TResult&gt;</c>.
/// </summary>
/// <remarks>
/// Port of Python's <c>EOFError</c> built-in exception.
/// Users can configure a custom exception type via the
/// <c>eofException</c> parameter of <c>PromptSession&lt;TResult&gt;</c>.
/// </remarks>
public sealed class EOFException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EOFException"/> class.
    /// </summary>
    public EOFException() : base("End of input.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EOFException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public EOFException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EOFException"/> class
    /// with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public EOFException(string message, Exception innerException)
        : base(message, innerException) { }
}
