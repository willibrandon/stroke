namespace Stroke.Validation;

/// <summary>
/// Exception thrown when input validation fails.
/// </summary>
/// <remarks>
/// This is a stub class for Feature 07 (Buffer).
/// Full implementation will be provided in Feature 09 (Validation System).
/// </remarks>
public sealed class ValidationError : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class.
    /// </summary>
    /// <param name="cursorPosition">Position to move cursor to (optional).</param>
    /// <param name="message">Error message to display.</param>
    public ValidationError(int cursorPosition, string message)
        : base(message)
    {
        CursorPosition = cursorPosition;
    }

    /// <summary>
    /// Gets the cursor position where the error occurred.
    /// </summary>
    public int CursorPosition { get; }
}
