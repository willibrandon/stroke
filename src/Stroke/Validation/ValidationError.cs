namespace Stroke.Validation;

/// <summary>
/// Exception thrown when input validation fails.
/// </summary>
/// <remarks>
/// <para>
/// This exception is raised by validators when the input document
/// does not meet validation requirements.
/// </para>
/// <para>
/// The <see cref="CursorPosition"/> property indicates where in the text
/// the error was detected, using 0-based indexing (0 = before first character).
/// </para>
/// <para>
/// Thread safety: This class is immutable and inherently thread-safe.
/// </para>
/// </remarks>
public sealed class ValidationError : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class.
    /// </summary>
    /// <param name="cursorPosition">
    /// Position to move cursor to (0-based indexing). Defaults to 0.
    /// Negative values and values exceeding document length are stored as-is;
    /// consumers are responsible for clamping if needed.
    /// </param>
    /// <param name="message">Error message to display. Defaults to empty string.</param>
    public ValidationError(int cursorPosition = 0, string message = "")
        : base(message)
    {
        CursorPosition = cursorPosition;
    }

    /// <summary>
    /// Gets the cursor position where the error occurred.
    /// </summary>
    /// <remarks>
    /// Uses 0-based indexing (0 = before first character, N = after last character for N-length text).
    /// </remarks>
    public int CursorPosition { get; }

    /// <summary>
    /// Returns a string representation of this validation error.
    /// </summary>
    /// <returns>
    /// A string in the format: <c>ValidationError(CursorPosition={0}, Message="{1}")</c>.
    /// </returns>
    public override string ToString()
    {
        return $"ValidationError(CursorPosition={CursorPosition}, Message=\"{Message}\")";
    }
}
