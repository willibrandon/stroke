namespace Stroke.Layout;

/// <summary>
/// Exception thrown when a <see cref="Layout"/> is invalid.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown when constructing a <see cref="Layout"/> with a container
/// hierarchy that contains no <see cref="Containers.Window"/> objects. At least one
/// Window is required for the layout to be functional (focus, rendering).
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>InvalidLayoutError</c> from
/// <c>prompt_toolkit.layout.layout</c>.
/// </para>
/// </remarks>
public sealed class InvalidLayoutException : Exception
{
    /// <summary>
    /// Creates a new InvalidLayoutException with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public InvalidLayoutException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new InvalidLayoutException with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public InvalidLayoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
