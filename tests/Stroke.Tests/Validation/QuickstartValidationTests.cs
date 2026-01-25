using System.Text.RegularExpressions;
using Stroke.Core;
using Stroke.Validation;
using Xunit;

namespace Stroke.Tests.Validation;

/// <summary>
/// Integration tests validating the quickstart.md examples work correctly.
/// </summary>
public sealed class QuickstartValidationTests
{
    #region Basic Usage Examples

    [Fact]
    public void FromCallable_NonEmptyValidator_Example()
    {
        // From quickstart.md - Creating a Validator from a Function
        var nonEmptyValidator = ValidatorBase.FromCallable(
            text => text.Length > 0,
            errorMessage: "Input cannot be empty"
        );

        // Valid input
        nonEmptyValidator.Validate(new Document("hello"));

        // Invalid input
        var ex = Assert.Throws<ValidationError>(() =>
            nonEmptyValidator.Validate(new Document("")));
        Assert.Equal("Input cannot be empty", ex.Message);
    }

    [Fact]
    public void FromCallable_NumericValidator_Example()
    {
        // From quickstart.md - Numeric input validator
        var numericValidator = ValidatorBase.FromCallable(
            text => int.TryParse(text, out _),
            errorMessage: "Please enter a number",
            moveCursorToEnd: true
        );

        // Valid
        numericValidator.Validate(new Document("123"));

        // Invalid - cursor at end
        var ex = Assert.Throws<ValidationError>(() =>
            numericValidator.Validate(new Document("abc")));
        Assert.Equal("Please enter a number", ex.Message);
        Assert.Equal(3, ex.CursorPosition); // "abc".Length
    }

    [Fact]
    public void ValidatingInput_Example()
    {
        var nonEmptyValidator = ValidatorBase.FromCallable(
            text => text.Length > 0,
            errorMessage: "Input cannot be empty"
        );

        var document = new Document("hello");

        // From quickstart.md - validating input pattern
        try
        {
            nonEmptyValidator.Validate(document);
            // Valid!
        }
        catch (ValidationError error)
        {
            Assert.Fail($"Unexpected error at position {error.CursorPosition}: {error.Message}");
        }
    }

    [Fact]
    public async Task AsyncValidation_Example()
    {
        var emailValidator = ValidatorBase.FromCallable(
            text => text.Contains('@'),
            errorMessage: "Invalid email"
        );

        var document = new Document("test@example.com");

        // From quickstart.md - async validation pattern
        try
        {
            await emailValidator.ValidateAsync(document);
            // Email is valid!
        }
        catch (ValidationError error)
        {
            Assert.Fail($"Invalid email: {error.Message}");
        }
    }

    #endregion

    #region Validator Types Examples

    [Fact]
    public void DummyValidator_Example()
    {
        // From quickstart.md - DummyValidator
        var validator = new DummyValidator();
        validator.Validate(new Document("anything")); // Never throws
        validator.Validate(new Document("")); // Also never throws
        validator.Validate(new Document("!@#$%")); // Still never throws
    }

    [Fact]
    public async Task ThreadedValidator_Example()
    {
        // From quickstart.md - ThreadedValidator (simplified, no Thread.Sleep)
        var slowValidator = ValidatorBase.FromCallable(
            text => text.Length >= 5,
            "Input must be at least 5 characters"
        );

        var threadedValidator = new ThreadedValidator(slowValidator);

        // Async method runs in background thread
        await threadedValidator.ValidateAsync(new Document("hello")); // Valid

        var ex = await Assert.ThrowsAsync<ValidationError>(async () =>
            await threadedValidator.ValidateAsync(new Document("hi")));
        Assert.Equal("Input must be at least 5 characters", ex.Message);
    }

    [Fact]
    public void ConditionalValidator_Example()
    {
        // From quickstart.md - ConditionalValidator
        bool isStrictMode = true;

        var conditionalValidator = new ConditionalValidator(
            validator: ValidatorBase.FromCallable(
                text => text.All(char.IsLetterOrDigit),
                "Only alphanumeric characters allowed"
            ),
            filter: () => isStrictMode
        );

        // When isStrictMode is true, validation runs
        conditionalValidator.Validate(new Document("hello123")); // OK

        // When isStrictMode is false, validation is skipped
        isStrictMode = false;
        conditionalValidator.Validate(new Document("hello@world")); // Also OK (skipped)
    }

    [Fact]
    public void DynamicValidator_Example()
    {
        // From quickstart.md - DynamicValidator
        int currentStep = 1;

        var stepValidators = new Dictionary<int, IValidator>
        {
            [1] = ValidatorBase.FromCallable(t => t.Length > 0, "Step 1: Enter name"),
            [2] = ValidatorBase.FromCallable(t => t.Contains('@'), "Step 2: Enter email"),
            [3] = ValidatorBase.FromCallable(t => t.All(char.IsDigit), "Step 3: Enter phone")
        };

        var dynamicValidator = new DynamicValidator(
            () => stepValidators.GetValueOrDefault(currentStep)
        );

        // Validates with current step's validator
        currentStep = 1;
        dynamicValidator.Validate(new Document("John Doe")); // Uses step 1 validator

        currentStep = 2;
        dynamicValidator.Validate(new Document("john@example.com")); // Uses step 2 validator

        // Returns null → uses DummyValidator (accepts all)
        currentStep = 99;
        dynamicValidator.Validate(new Document("anything")); // OK
    }

    #endregion

    #region Custom Validator Examples

    [Fact]
    public void CustomEmailValidator_Example()
    {
        // From quickstart.md - Custom Validators
        var emailValidator = new EmailValidator();

        // Valid email
        emailValidator.Validate(new Document("test@example.com"));

        // Invalid email
        var ex = Assert.Throws<ValidationError>(() =>
            emailValidator.Validate(new Document("invalid")));
        Assert.Equal("Please enter a valid email address", ex.Message);
    }

    /// <summary>
    /// Custom email validator from quickstart.md example.
    /// </summary>
    private sealed class EmailValidator : ValidatorBase
    {
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled
        );

        public override void Validate(Document document)
        {
            ArgumentNullException.ThrowIfNull(document);

            if (!EmailRegex.IsMatch(document.Text))
            {
                throw new ValidationError(
                    cursorPosition: document.Text.IndexOf('@') is int i and >= 0 ? i : 0,
                    message: "Please enter a valid email address"
                );
            }
        }
    }

    #endregion

    #region Common Patterns Examples

    [Fact]
    public void RequiredField_Pattern()
    {
        // From quickstart.md - Common Patterns: Required Field
        var required = ValidatorBase.FromCallable(
            text => !string.IsNullOrWhiteSpace(text),
            "This field is required"
        );

        required.Validate(new Document("value"));

        Assert.Throws<ValidationError>(() => required.Validate(new Document("")));
        Assert.Throws<ValidationError>(() => required.Validate(new Document("   ")));
    }

    [Fact]
    public void LengthLimits_Pattern()
    {
        // From quickstart.md - Common Patterns: Length Limits
        var lengthValidator = ValidatorBase.FromCallable(
            text => text.Length >= 3 && text.Length <= 50,
            "Input must be between 3 and 50 characters"
        );

        lengthValidator.Validate(new Document("abc")); // Min
        lengthValidator.Validate(new Document(new string('x', 50))); // Max

        Assert.Throws<ValidationError>(() => lengthValidator.Validate(new Document("ab")));
        Assert.Throws<ValidationError>(() => lengthValidator.Validate(new Document(new string('x', 51))));
    }

    [Fact]
    public void PatternMatching_Pattern()
    {
        // From quickstart.md - Common Patterns: Pattern Matching
        var usernameValidator = ValidatorBase.FromCallable(
            text => Regex.IsMatch(text, @"^[a-z][a-z0-9_]{2,19}$"),
            "Username must start with a letter, 3-20 chars, letters/numbers/underscores only"
        );

        usernameValidator.Validate(new Document("john_doe123"));

        Assert.Throws<ValidationError>(() => usernameValidator.Validate(new Document("1invalid"))); // Starts with number
        Assert.Throws<ValidationError>(() => usernameValidator.Validate(new Document("ab"))); // Too short
    }

    #endregion

    #region Error Handling Example

    [Fact]
    public void ErrorHandling_Example()
    {
        var validator = ValidatorBase.FromCallable(
            text => text.Length >= 5,
            "Input too short",
            moveCursorToEnd: true
        );

        var document = new Document("abc");

        try
        {
            validator.Validate(document);
        }
        catch (ValidationError error)
        {
            // Move cursor to error position by creating new document
            var newDocument = new Document(document.Text, cursorPosition: error.CursorPosition);

            Assert.Equal(3, newDocument.CursorPosition);
            Assert.Equal("Input too short", error.Message);
        }
    }

    #endregion
}
