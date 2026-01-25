using Stroke.Core;

namespace Stroke.Validation;

/// <summary>
/// Base interface for input validation.
/// </summary>
/// <remarks>
/// <para>
/// Validators check Document content and throw <see cref="ValidationError"/>
/// if the content is invalid. Valid input passes without exception.
/// </para>
/// <para>
/// Implementations should be stateless or immutable to ensure thread safety.
/// </para>
/// </remarks>
public interface IValidator
{
    /// <summary>
    /// Validate the given document synchronously.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ValidationError">Thrown when validation fails.</exception>
    /// <remarks>
    /// If the document is valid, this method returns without throwing.
    /// Non-ValidationError exceptions from validation logic propagate unchanged.
    /// </remarks>
    void Validate(Document document);

    /// <summary>
    /// Validate the given document asynchronously.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> that completes when validation finishes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ValidationError">Thrown when validation fails.</exception>
    /// <remarks>
    /// <para>
    /// ValueTask is used to avoid heap allocation when validation completes synchronously.
    /// </para>
    /// <para>
    /// For validators that don't override this method, the default implementation
    /// calls <see cref="Validate"/> synchronously and returns a completed ValueTask.
    /// </para>
    /// </remarks>
    ValueTask ValidateAsync(Document document);
}
