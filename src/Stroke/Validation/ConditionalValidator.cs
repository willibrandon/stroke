using Stroke.Core;

namespace Stroke.Validation;

/// <summary>
/// A validator that conditionally applies wrapped validation based on a filter function.
/// </summary>
/// <remarks>
/// <para>
/// When the filter returns <see langword="true"/>, the wrapped validator is invoked.
/// When the filter returns <see langword="false"/>, validation is skipped and input is accepted.
/// </para>
/// <para>
/// Thread safety: This class is immutable after construction. Thread safety depends on
/// whether the provided filter function is thread-safe.
/// </para>
/// </remarks>
public sealed class ConditionalValidator : ValidatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalValidator"/> class.
    /// </summary>
    /// <param name="validator">The wrapped validator to conditionally invoke.</param>
    /// <param name="filter">
    /// A function that returns <see langword="true"/> when validation should run,
    /// or <see langword="false"/> to skip validation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="validator"/> or <paramref name="filter"/> is <see langword="null"/>.
    /// </exception>
    public ConditionalValidator(IValidator validator, Func<bool> filter)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(filter);

        Validator = validator;
        Filter = filter;
    }

    /// <summary>
    /// Gets the wrapped validator that is conditionally invoked.
    /// </summary>
    public IValidator Validator { get; }

    /// <summary>
    /// Gets the filter function that determines whether validation runs.
    /// </summary>
    public Func<bool> Filter { get; }

    /// <summary>
    /// Validates the given document if the filter returns <see langword="true"/>.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ValidationError">
    /// Thrown when the filter returns <see langword="true"/> and the wrapped validator rejects the input.
    /// </exception>
    /// <remarks>
    /// If the filter returns <see langword="false"/>, this method returns without invoking
    /// the wrapped validator, effectively accepting all input.
    /// </remarks>
    public override void Validate(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (Filter())
        {
            Validator.Validate(document);
        }
    }

    /// <summary>
    /// Validates the given document asynchronously if the filter returns <see langword="true"/>.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>A <see cref="ValueTask"/> that completes when validation finishes.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ValidationError">
    /// Thrown when the filter returns <see langword="true"/> and the wrapped validator rejects the input.
    /// </exception>
    public override ValueTask ValidateAsync(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (Filter())
        {
            return Validator.ValidateAsync(document);
        }

        return ValueTask.CompletedTask;
    }
}
