using Stroke.Core;

namespace Stroke.Validation;

/// <summary>
/// A validator that retrieves the actual validator dynamically at validation time.
/// </summary>
/// <remarks>
/// <para>
/// This validator calls the getter function on each validation to retrieve the current
/// validator. If the getter returns <see langword="null"/>, validation is skipped
/// (equivalent to <see cref="DummyValidator"/> behavior).
/// </para>
/// <para>
/// Thread safety: This class is immutable after construction. Thread safety depends on
/// whether the provided getter function is thread-safe.
/// </para>
/// </remarks>
public sealed class DynamicValidator : ValidatorBase
{
    private static readonly DummyValidator s_fallbackValidator = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValidator"/> class.
    /// </summary>
    /// <param name="getValidator">
    /// A function that returns the current validator to use.
    /// If the function returns <see langword="null"/>, all input is accepted.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="getValidator"/> is <see langword="null"/>.
    /// </exception>
    public DynamicValidator(Func<IValidator?> getValidator)
    {
        ArgumentNullException.ThrowIfNull(getValidator);
        GetValidator = getValidator;
    }

    /// <summary>
    /// Gets the function that retrieves the current validator.
    /// </summary>
    public Func<IValidator?> GetValidator { get; }

    /// <summary>
    /// Validates the given document using the dynamically-retrieved validator.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ValidationError">
    /// Thrown when the dynamically-retrieved validator rejects the input.
    /// </exception>
    /// <remarks>
    /// If <see cref="GetValidator"/> returns <see langword="null"/>, this method
    /// returns without throwing (DummyValidator behavior).
    /// </remarks>
    public override void Validate(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var validator = GetValidator() ?? s_fallbackValidator;
        validator.Validate(document);
    }

    /// <summary>
    /// Validates the given document asynchronously using the dynamically-retrieved validator.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>A <see cref="ValueTask"/> that completes when validation finishes.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ValidationError">
    /// Thrown when the dynamically-retrieved validator rejects the input.
    /// </exception>
    public override ValueTask ValidateAsync(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var validator = GetValidator() ?? s_fallbackValidator;
        return validator.ValidateAsync(document);
    }
}
