using Stroke.Core;

namespace Stroke.Validation;

/// <summary>
/// Abstract base class for validators, providing default async implementation
/// and the FromCallable factory methods.
/// </summary>
/// <remarks>
/// <para>
/// Subclasses must implement <see cref="Validate"/> to perform synchronous validation.
/// The default <see cref="ValidateAsync"/> implementation calls <see cref="Validate"/>
/// synchronously and returns a completed <see cref="ValueTask"/>.
/// </para>
/// <para>
/// Thread safety: This base class is stateless. Subclasses should be stateless or
/// immutable to ensure thread safety.
/// </para>
/// </remarks>
public abstract class ValidatorBase : IValidator
{
    /// <summary>
    /// Validate the given document synchronously.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ValidationError">Thrown when validation fails.</exception>
    public abstract void Validate(Document document);

    /// <summary>
    /// Validate the given document asynchronously.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>A completed <see cref="ValueTask"/> after validation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ValidationError">Thrown when validation fails.</exception>
    /// <remarks>
    /// The default implementation calls <see cref="Validate"/> synchronously.
    /// Override this method for true asynchronous validation (e.g., in ThreadedValidator).
    /// </remarks>
    public virtual ValueTask ValidateAsync(Document document)
    {
        Validate(document);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Create a validator from a boolean validation function.
    /// </summary>
    /// <param name="validateFunc">
    /// A function that takes the document text and returns <see langword="true"/>
    /// if the text is valid, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="errorMessage">
    /// The error message to include in <see cref="ValidationError"/> when validation fails.
    /// Defaults to "Invalid input".
    /// </param>
    /// <param name="moveCursorToEnd">
    /// If <see langword="true"/>, the cursor position in <see cref="ValidationError"/>
    /// will be set to the text length. If <see langword="false"/>, cursor position will be 0.
    /// Defaults to <see langword="false"/>.
    /// </param>
    /// <returns>A validator that uses the provided function for validation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="validateFunc"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This factory method provides a convenient way to create validators from simple
    /// functions without implementing a full validator class.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a validator that requires non-empty input
    /// var validator = ValidatorBase.FromCallable(
    ///     text => text.Length > 0,
    ///     errorMessage: "Input cannot be empty"
    /// );
    /// </code>
    /// </example>
    public static IValidator FromCallable(
        Func<string, bool> validateFunc,
        string errorMessage = "Invalid input",
        bool moveCursorToEnd = false)
    {
        ArgumentNullException.ThrowIfNull(validateFunc);
        return new FromCallableValidator(validateFunc, errorMessage, moveCursorToEnd);
    }

    /// <summary>
    /// Create a validator from a throwing validation function.
    /// </summary>
    /// <param name="validateFunc">
    /// An action that validates the document and throws <see cref="ValidationError"/>
    /// directly if validation fails. If the action completes without throwing,
    /// the document is considered valid.
    /// </param>
    /// <returns>A validator that uses the provided action for validation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="validateFunc"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This overload allows full control over the <see cref="ValidationError"/>,
    /// including cursor position and error message.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a validator with custom cursor positioning
    /// var validator = ValidatorBase.FromCallable(doc =>
    /// {
    ///     if (doc.Text.Contains("bad"))
    ///     {
    ///         int pos = doc.Text.IndexOf("bad");
    ///         throw new ValidationError(pos, "Found 'bad' at this position");
    ///     }
    /// });
    /// </code>
    /// </example>
    public static IValidator FromCallable(Action<Document> validateFunc)
    {
        ArgumentNullException.ThrowIfNull(validateFunc);
        return new FromCallableActionValidator(validateFunc);
    }
}

/// <summary>
/// Internal validator created by <see cref="ValidatorBase.FromCallable(Func{string, bool}, string, bool)"/>.
/// </summary>
internal sealed class FromCallableValidator : ValidatorBase
{
    private readonly Func<string, bool> _validateFunc;
    private readonly string _errorMessage;
    private readonly bool _moveCursorToEnd;

    /// <summary>
    /// Initializes a new instance of the <see cref="FromCallableValidator"/> class.
    /// </summary>
    /// <param name="validateFunc">The validation function.</param>
    /// <param name="errorMessage">The error message for validation failures.</param>
    /// <param name="moveCursorToEnd">Whether to position cursor at end on error.</param>
    internal FromCallableValidator(Func<string, bool> validateFunc, string errorMessage, bool moveCursorToEnd)
    {
        _validateFunc = validateFunc;
        _errorMessage = errorMessage;
        _moveCursorToEnd = moveCursorToEnd;
    }

    /// <inheritdoc/>
    public override void Validate(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!_validateFunc(document.Text))
        {
            int cursorPosition = _moveCursorToEnd ? document.Text.Length : 0;
            throw new ValidationError(cursorPosition, _errorMessage);
        }
    }
}

/// <summary>
/// Internal validator created by <see cref="ValidatorBase.FromCallable(Action{Document})"/>.
/// </summary>
internal sealed class FromCallableActionValidator : ValidatorBase
{
    private readonly Action<Document> _validateFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="FromCallableActionValidator"/> class.
    /// </summary>
    /// <param name="validateFunc">The validation action that throws ValidationError on failure.</param>
    internal FromCallableActionValidator(Action<Document> validateFunc)
    {
        _validateFunc = validateFunc;
    }

    /// <inheritdoc/>
    public override void Validate(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _validateFunc(document);
    }
}
