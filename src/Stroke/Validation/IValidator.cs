using Stroke.Core;

namespace Stroke.Validation;

/// <summary>
/// Base interface for input validation.
/// </summary>
/// <remarks>
/// This is a stub interface for Feature 07 (Buffer).
/// Full implementation will be provided in Feature 09 (Validation System).
/// </remarks>
public interface IValidator
{
    /// <summary>
    /// Validate the given document.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <exception cref="ValidationError">Thrown when validation fails.</exception>
    void Validate(Document document);

    /// <summary>
    /// Validate the given document asynchronously.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <exception cref="ValidationError">Thrown when validation fails.</exception>
    ValueTask ValidateAsync(Document document);
}
