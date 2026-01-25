using Stroke.Core;

namespace Stroke.Validation;

/// <summary>
/// A validator that runs wrapped validation in a background thread.
/// </summary>
/// <remarks>
/// <para>
/// This validator is useful for expensive validation operations that should not
/// block the calling thread (e.g., network validation, complex computations).
/// </para>
/// <para>
/// The <see cref="Validate"/> method runs synchronously on the calling thread
/// for compatibility. Use <see cref="ValidateAsync"/> for background execution.
/// </para>
/// <para>
/// Thread safety: This class is immutable after construction. The
/// <see cref="ValidateAsync"/> method uses Task.Run with
/// ConfigureAwait(false) for background execution. Concurrent calls execute independently.
/// </para>
/// </remarks>
public sealed class ThreadedValidator : ValidatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadedValidator"/> class.
    /// </summary>
    /// <param name="validator">The wrapped validator to run in background.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="validator"/> is <see langword="null"/>.
    /// </exception>
    public ThreadedValidator(IValidator validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        Validator = validator;
    }

    /// <summary>
    /// Gets the wrapped validator.
    /// </summary>
    public IValidator Validator { get; }

    /// <summary>
    /// Validates the given document synchronously on the calling thread.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ValidationError">
    /// Thrown when the wrapped validator rejects the input.
    /// </exception>
    /// <remarks>
    /// This method runs synchronously for compatibility with callers that
    /// don't support async. For background execution, use <see cref="ValidateAsync"/>.
    /// </remarks>
    public override void Validate(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        Validator.Validate(document);
    }

    /// <summary>
    /// Validates the given document asynchronously on a background thread.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>A <see cref="ValueTask"/> that completes when validation finishes.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ValidationError">
    /// Thrown when the wrapped validator rejects the input.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Validation runs on a dedicated background thread via TaskCreationOptions.LongRunning.
    /// This ensures the calling thread is not blocked during validation and guarantees
    /// execution on a separate thread (unlike Task.Run which may reuse the calling thread).
    /// </para>
    /// <para>
    /// Exceptions from the wrapped validator (including <see cref="ValidationError"/>)
    /// are propagated to the caller.
    /// </para>
    /// </remarks>
    public override async ValueTask ValidateAsync(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        // Use LongRunning to create a dedicated thread rather than using the thread pool.
        // Task.Run() may reuse the calling thread under certain conditions (e.g., when
        // called from a thread pool thread on a system with limited cores).
        await Task.Factory.StartNew(
            () => Validator.Validate(document),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).ConfigureAwait(false);
    }
}
