using Stroke.Core;

namespace Stroke.Validation;

/// <summary>
/// A null-object validator that accepts all input without throwing.
/// </summary>
/// <remarks>
/// <para>
/// This validator is useful as a placeholder when validation should be disabled,
/// or as a fallback when no specific validation is needed.
/// </para>
/// <para>
/// Thread safety: This class is stateless and inherently thread-safe.
/// </para>
/// </remarks>
public sealed class DummyValidator : ValidatorBase
{
    /// <summary>
    /// Validates the given document. This implementation accepts all input.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method never throws <see cref="ValidationError"/> - all input is accepted.
    /// </remarks>
    public override void Validate(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        // Accept all input - do nothing
    }
}
