namespace Stroke.Core;

using Stroke.Validation;

/// <summary>
/// Buffer partial class containing validation operations.
/// </summary>
public sealed partial class Buffer
{
    // Note: _validationState, _validationError, _validationLock, and Validator
    // are declared in Buffer.cs

    // ════════════════════════════════════════════════════════════════════════
    // SYNCHRONOUS VALIDATION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validate the current buffer content.
    /// </summary>
    /// <param name="setCursor">If true, set cursor position to error location if validation fails.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public bool Validate(bool setCursor = false)
    {
        using (_lock.EnterScope())
        {
            // Don't call the validator again if it was already called for the current input
            if (_validationState != ValidationState.Unknown)
            {
                return _validationState == ValidationState.Valid;
            }

            // Call validator
            if (Validator != null)
            {
                try
                {
                    Validator.Validate(Document);
                }
                catch (ValidationError e)
                {
                    // Set cursor position (don't allow invalid values)
                    if (setCursor)
                    {
                        _cursorPosition = Math.Min(Math.Max(0, e.CursorPosition), _workingLines[_workingIndex].Length);
                    }

                    _validationState = ValidationState.Invalid;
                    _validationError = e;
                    return false;
                }
            }

            // Handle validation result
            _validationState = ValidationState.Valid;
            _validationError = null;
            return true;
        }
    }

    /// <summary>
    /// Validate buffer and handle the accept action.
    /// </summary>
    public void ValidateAndHandle()
    {
        var valid = Validate(setCursor: true);

        // When the validation succeeded, accept the input
        if (valid)
        {
            bool keepText;
            if (AcceptHandler != null)
            {
                keepText = AcceptHandler(this);
            }
            else
            {
                keepText = false;
            }

            AppendToHistory();

            if (!keepText)
            {
                Reset();
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // ASYNCHRONOUS VALIDATION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Asynchronous version of <see cref="Validate"/>.
    /// This one doesn't set the cursor position.
    /// </summary>
    /// <remarks>
    /// We have both variants, because a synchronous version is required.
    /// Handling the ENTER key needs to be completely synchronous, otherwise
    /// stuff like type-ahead is going to give very weird results.
    /// An asynchronous version is required if we have validate_while_typing enabled.
    /// </remarks>
    public async Task ValidateAsync()
    {
        await _validationLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await ValidateAsyncInternal().ConfigureAwait(false);
        }
        finally
        {
            _validationLock.Release();
        }
    }

    /// <summary>
    /// Internal async validation implementation.
    /// </summary>
    private async Task ValidateAsyncInternal()
    {
        while (true)
        {
            Document document;
            using (_lock.EnterScope())
            {
                // Don't call the validator again if it was already called for the current input
                if (_validationState != ValidationState.Unknown)
                {
                    return;
                }

                document = Document;
            }

            // Call validator
            ValidationError? error = null;

            if (Validator != null)
            {
                try
                {
                    await Validator.ValidateAsync(document);
                }
                catch (ValidationError e)
                {
                    error = e;
                }
            }

            using (_lock.EnterScope())
            {
                // If the document changed during validation, try again
                if (_workingLines[_workingIndex] != document.Text ||
                    _cursorPosition != document.CursorPosition)
                {
                    continue;
                }

                if (error != null)
                {
                    _validationState = ValidationState.Invalid;
                    _validationError = error;
                }
                else
                {
                    _validationState = ValidationState.Valid;
                    _validationError = null;
                }

                return;
            }
        }
    }

    /// <summary>
    /// Dismiss the current validation error without re-validating.
    /// Equivalent to Python's <c>b.validation_error = None</c>.
    /// </summary>
    public void DismissValidation()
    {
        using (_lock.EnterScope())
        {
            _validationError = null;
            _validationState = ValidationState.Unknown;
        }
    }

    /// <summary>
    /// Start async validation when validate_while_typing is enabled.
    /// </summary>
    internal void StartAsyncValidation()
    {
        if (ValidateWhileTyping)
        {
            _ = ValidateAsync();
        }
    }
}
