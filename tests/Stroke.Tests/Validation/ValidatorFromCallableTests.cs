using Stroke.Core;
using Stroke.Validation;
using Xunit;

namespace Stroke.Tests.Validation;

/// <summary>
/// Unit tests for <see cref="ValidatorBase.FromCallable"/> factory method.
/// </summary>
public sealed class ValidatorFromCallableTests
{
    #region Boolean Function Overload Tests (T007)

    [Fact]
    public void FromCallable_WithBooleanFunction_ValidInput_DoesNotThrow()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(text => text.Length > 0);
        var document = new Document("hello");

        // Act & Assert - should not throw
        validator.Validate(document);
    }

    [Fact]
    public void FromCallable_WithBooleanFunction_InvalidInput_ThrowsValidationError()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(text => text.Length > 0);
        var document = new Document("");

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal("Invalid input", ex.Message); // Default message
    }

    [Fact]
    public void FromCallable_WithBooleanFunction_CustomMessage_UsesMessage()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(
            text => text.Length > 0,
            errorMessage: "Cannot be empty"
        );
        var document = new Document("");

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal("Cannot be empty", ex.Message);
    }

    [Fact]
    public void FromCallable_WithBooleanFunction_NumericValidation()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(
            text => int.TryParse(text, out _),
            errorMessage: "Must be a number"
        );

        // Act & Assert - valid number
        var validDoc = new Document("123");
        validator.Validate(validDoc); // Should not throw

        // Act & Assert - invalid number
        var invalidDoc = new Document("abc");
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(invalidDoc));
        Assert.Equal("Must be a number", ex.Message);
    }

    [Fact]
    public void FromCallable_WithBooleanFunction_PatternMatching()
    {
        // Arrange - email-like pattern
        var validator = ValidatorBase.FromCallable(
            text => text.Contains('@'),
            errorMessage: "Must contain @"
        );

        // Act & Assert - valid
        var validDoc = new Document("user@example.com");
        validator.Validate(validDoc);

        // Act & Assert - invalid
        var invalidDoc = new Document("userexample.com");
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(invalidDoc));
        Assert.Equal("Must contain @", ex.Message);
    }

    [Fact]
    public void FromCallable_ReturnedValidator_ImplementsIValidator()
    {
        var validator = ValidatorBase.FromCallable(text => true);

        Assert.IsAssignableFrom<IValidator>(validator);
    }

    [Fact]
    public void FromCallable_ReturnedValidator_ImplementsValidatorBase()
    {
        var validator = ValidatorBase.FromCallable(text => true);

        Assert.IsAssignableFrom<ValidatorBase>(validator);
    }

    #endregion

    #region MoveCursorToEnd Parameter Tests (T008)

    [Fact]
    public void FromCallable_MoveCursorToEndFalse_CursorPositionIsZero()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(
            text => false, // Always fails
            moveCursorToEnd: false
        );
        var document = new Document("hello");

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal(0, ex.CursorPosition);
    }

    [Fact]
    public void FromCallable_MoveCursorToEndTrue_CursorPositionIsTextLength()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(
            text => false, // Always fails
            moveCursorToEnd: true
        );
        var document = new Document("hello"); // Length = 5

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal(5, ex.CursorPosition);
    }

    [Fact]
    public void FromCallable_MoveCursorToEnd_WithEmptyText_CursorPositionIsZero()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(
            text => false,
            moveCursorToEnd: true
        );
        var document = new Document(""); // Length = 0

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal(0, ex.CursorPosition);
    }

    [Fact]
    public void FromCallable_MoveCursorToEnd_WithLongText()
    {
        // Arrange
        var longText = new string('a', 1000);
        var validator = ValidatorBase.FromCallable(
            text => false,
            moveCursorToEnd: true
        );
        var document = new Document(longText);

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal(1000, ex.CursorPosition);
    }

    [Fact]
    public void FromCallable_DefaultMoveCursorToEnd_IsFalse()
    {
        // Arrange - don't specify moveCursorToEnd
        var validator = ValidatorBase.FromCallable(text => false);
        var document = new Document("hello");

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal(0, ex.CursorPosition); // Default is false, so position = 0
    }

    #endregion

    #region Null Parameter Handling Tests (T009)

    [Fact]
    public void FromCallable_NullValidateFunc_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ValidatorBase.FromCallable((Func<string, bool>)null!));
    }

    [Fact]
    public void FromCallable_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(text => true);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }

    [Fact]
    public async Task FromCallable_NullDocumentAsync_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(text => true);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await validator.ValidateAsync(null!));
    }

    #endregion

    #region Action<Document> Overload Tests (T013)

    [Fact]
    public void FromCallable_WithAction_ValidInput_DoesNotThrow()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(doc =>
        {
            // Valid - do nothing
        });
        var document = new Document("hello");

        // Act & Assert - should not throw
        validator.Validate(document);
    }

    [Fact]
    public void FromCallable_WithAction_ThrowsValidationError_PropagatesCorrectly()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(doc =>
        {
            throw new ValidationError(3, "Error at position 3");
        });
        var document = new Document("hello");

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal(3, ex.CursorPosition);
        Assert.Equal("Error at position 3", ex.Message);
    }

    [Fact]
    public void FromCallable_WithAction_CustomCursorPosition()
    {
        // Arrange - find "bad" in text and set cursor there
        var validator = ValidatorBase.FromCallable(doc =>
        {
            int pos = doc.Text.IndexOf("bad");
            if (pos >= 0)
            {
                throw new ValidationError(pos, "Found 'bad' at this position");
            }
        });

        // Act & Assert - valid
        var validDoc = new Document("hello world");
        validator.Validate(validDoc);

        // Act & Assert - invalid
        var invalidDoc = new Document("this is bad");
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(invalidDoc));
        Assert.Equal(8, ex.CursorPosition); // "bad" starts at position 8
        Assert.Equal("Found 'bad' at this position", ex.Message);
    }

    [Fact]
    public void FromCallable_WithAction_NullAction_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ValidatorBase.FromCallable((Action<Document>)null!));
    }

    [Fact]
    public void FromCallable_WithAction_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(doc => { });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }

    [Fact]
    public void FromCallable_WithAction_NonValidationErrorException_Propagates()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(doc =>
        {
            throw new InvalidOperationException("Something went wrong");
        });
        var document = new Document("hello");

        // Act & Assert - non-ValidationError exceptions propagate unchanged
        Assert.Throws<InvalidOperationException>(() => validator.Validate(document));
    }

    #endregion

    #region ValidateAsync Tests

    [Fact]
    public async Task ValidateAsync_WithBooleanFunction_ValidInput_Completes()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(text => text.Length > 0);
        var document = new Document("hello");

        // Act & Assert - should complete without exception
        await validator.ValidateAsync(document);
    }

    [Fact]
    public async Task ValidateAsync_WithBooleanFunction_InvalidInput_ThrowsValidationError()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(text => text.Length > 0);
        var document = new Document("");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationError>(async () =>
            await validator.ValidateAsync(document));
        Assert.Equal("Invalid input", ex.Message);
    }

    [Fact]
    public async Task ValidateAsync_WithAction_ValidInput_Completes()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(doc => { });
        var document = new Document("hello");

        // Act & Assert - should complete without exception
        await validator.ValidateAsync(document);
    }

    [Fact]
    public async Task ValidateAsync_WithAction_InvalidInput_ThrowsValidationError()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(doc =>
        {
            throw new ValidationError(5, "Error");
        });
        var document = new Document("hello");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationError>(async () =>
            await validator.ValidateAsync(document));
        Assert.Equal(5, ex.CursorPosition);
    }

    #endregion

    #region Immutability & Statelesness Tests

    [Fact]
    public void FromCallable_MultipleValidations_SameResult()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(text => text.Length > 3);

        // Act & Assert - multiple validations with same input should behave identically
        var shortDoc = new Document("hi");
        var longDoc = new Document("hello");

        Assert.Throws<ValidationError>(() => validator.Validate(shortDoc));
        validator.Validate(longDoc); // Should not throw
        Assert.Throws<ValidationError>(() => validator.Validate(shortDoc)); // Still fails
        validator.Validate(longDoc); // Still passes
    }

    [Fact]
    public void FromCallable_ValidatorIsReusable()
    {
        // Arrange
        var validator = ValidatorBase.FromCallable(text => text.Length > 0);

        // Act & Assert - same validator can validate different documents
        validator.Validate(new Document("a"));
        validator.Validate(new Document("abc"));
        validator.Validate(new Document("hello world"));
    }

    #endregion
}
