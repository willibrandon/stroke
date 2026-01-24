namespace Stroke.Core;

/// <summary>
/// Exception thrown when attempting to edit a read-only buffer.
/// </summary>
public sealed class EditReadOnlyBufferException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditReadOnlyBufferException"/> class.
    /// </summary>
    public EditReadOnlyBufferException()
        : base("Attempt editing of read-only Buffer.") { }
}
