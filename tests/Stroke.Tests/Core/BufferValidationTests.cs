using Stroke.Validation;
using Xunit;

// Use alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;
using Document = Stroke.Core.Document;
using ValidationState = Stroke.Core.ValidationState;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Buffer validation operations (T105-T112).
/// </summary>
public class BufferValidationTests
{
    #region Test Helpers

    /// <summary>
    /// A simple validator that rejects text containing "invalid".
    /// </summary>
    private sealed class SimpleValidator : IValidator
    {
        public void Validate(Document document)
        {
            var text = document.Text;
            var index = text.IndexOf("invalid", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                throw new ValidationError(index, "Text contains 'invalid'");
            }
        }

        public ValueTask ValidateAsync(Document document)
        {
            Validate(document);
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// A validator that rejects empty text.
    /// </summary>
    private sealed class NotEmptyValidator : IValidator
    {
        public void Validate(Document document)
        {
            if (string.IsNullOrEmpty(document.Text))
            {
                throw new ValidationError(0, "Text cannot be empty");
            }
        }

        public ValueTask ValidateAsync(Document document)
        {
            Validate(document);
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// An async validator with delay for testing async behavior.
    /// </summary>
    private sealed class DelayedValidator : IValidator
    {
        private readonly int _delayMs;

        public DelayedValidator(int delayMs = 50)
        {
            _delayMs = delayMs;
        }

        public void Validate(Document document)
        {
            if (document.Text.Contains("bad"))
            {
                throw new ValidationError(document.Text.IndexOf("bad"), "Contains 'bad'");
            }
        }

        public async ValueTask ValidateAsync(Document document)
        {
            await Task.Delay(_delayMs);
            Validate(document);
        }
    }

    #endregion

    #region Validate Tests (Synchronous)

    [Fact]
    public void Validate_ValidText_ReturnsTrue()
    {
        // Arrange
        var validator = new SimpleValidator();
        var buffer = new Buffer(validator: validator, document: new Document("hello world"));

        // Act
        var result = buffer.Validate();

        // Assert
        Assert.True(result);
        Assert.Equal(ValidationState.Valid, buffer.ValidationState);
        Assert.Null(buffer.ValidationError);
    }

    [Fact]
    public void Validate_InvalidText_ReturnsFalse()
    {
        // Arrange
        var validator = new SimpleValidator();
        var buffer = new Buffer(validator: validator, document: new Document("this is invalid text"));

        // Act
        var result = buffer.Validate();

        // Assert
        Assert.False(result);
        Assert.Equal(ValidationState.Invalid, buffer.ValidationState);
        Assert.NotNull(buffer.ValidationError);
        Assert.Equal("Text contains 'invalid'", buffer.ValidationError.Message);
    }

    [Fact]
    public void Validate_NoValidator_ReturnsTrue()
    {
        // Arrange - no validator
        var buffer = new Buffer(document: new Document("any text"));

        // Act
        var result = buffer.Validate();

        // Assert
        Assert.True(result);
        Assert.Equal(ValidationState.Valid, buffer.ValidationState);
    }

    [Fact]
    public void Validate_AlreadyValidated_ReturnsCachedResult()
    {
        // Arrange
        var validator = new SimpleValidator();
        var buffer = new Buffer(validator: validator, document: new Document("hello"));

        // First validation
        buffer.Validate();
        Assert.Equal(ValidationState.Valid, buffer.ValidationState);

        // Act - validate again (should use cached result)
        var result = buffer.Validate();

        // Assert
        Assert.True(result);
        Assert.Equal(ValidationState.Valid, buffer.ValidationState);
    }

    [Fact]
    public void Validate_SetCursorTrue_MovesCursorToError()
    {
        // Arrange
        var validator = new SimpleValidator();
        var buffer = new Buffer(validator: validator, document: new Document("test invalid here", cursorPosition: 0));

        // Act
        buffer.Validate(setCursor: true);

        // Assert - cursor should be at position of "invalid"
        Assert.Equal(5, buffer.CursorPosition); // "test " is 5 chars
    }

    [Fact]
    public void Validate_SetCursorFalse_DoesNotMoveCursor()
    {
        // Arrange
        var validator = new SimpleValidator();
        var buffer = new Buffer(validator: validator, document: new Document("test invalid here", cursorPosition: 0));

        // Act
        buffer.Validate(setCursor: false);

        // Assert - cursor should not have moved
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void Validate_ErrorCursorBeyondText_ClampsToBounds()
    {
        // Arrange
        var validatorWithBadCursor = new BoundaryValidator();
        var buffer = new Buffer(validator: validatorWithBadCursor, document: new Document("short", cursorPosition: 0));

        // Act
        buffer.Validate(setCursor: true);

        // Assert - cursor should be clamped to text length
        Assert.True(buffer.CursorPosition >= 0 && buffer.CursorPosition <= buffer.Text.Length);
    }

    /// <summary>
    /// Validator that reports cursor position beyond text bounds.
    /// </summary>
    private sealed class BoundaryValidator : IValidator
    {
        public void Validate(Document document)
        {
            throw new ValidationError(1000, "Error at position 1000"); // Beyond any reasonable text
        }

        public ValueTask ValidateAsync(Document document)
        {
            Validate(document);
            return ValueTask.CompletedTask;
        }
    }

    #endregion

    #region ValidationState Transition Tests

    [Fact]
    public void ValidationState_InitiallyUnknown()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test"));

        // Assert
        Assert.Equal(ValidationState.Unknown, buffer.ValidationState);
    }

    [Fact]
    public void ValidationState_AfterTextChange_ResetsToUnknown()
    {
        // Arrange
        var validator = new SimpleValidator();
        var buffer = new Buffer(validator: validator, document: new Document("hello"));
        buffer.Validate();
        Assert.Equal(ValidationState.Valid, buffer.ValidationState);

        // Act - change text
        buffer.InsertText(" world", fireEvent: false);

        // Assert
        Assert.Equal(ValidationState.Unknown, buffer.ValidationState);
    }

    [Fact]
    public void ValidationState_AfterReset_ResetsToUnknown()
    {
        // Arrange
        var validator = new SimpleValidator();
        var buffer = new Buffer(validator: validator, document: new Document("hello"));
        buffer.Validate();
        Assert.Equal(ValidationState.Valid, buffer.ValidationState);

        // Act
        buffer.Reset();

        // Assert
        Assert.Equal(ValidationState.Unknown, buffer.ValidationState);
    }

    #endregion

    #region ValidateAndHandle Tests

    [Fact]
    public void ValidateAndHandle_Valid_ResetsBuffer()
    {
        // Arrange
        var validator = new SimpleValidator();
        var buffer = new Buffer(validator: validator, document: new Document("hello"));

        // Act
        buffer.ValidateAndHandle();

        // Assert - buffer should be reset
        Assert.Equal("", buffer.Text);
    }

    [Fact]
    public void ValidateAndHandle_Invalid_DoesNotReset()
    {
        // Arrange
        var validator = new SimpleValidator();
        var buffer = new Buffer(validator: validator, document: new Document("test invalid"));

        // Act
        buffer.ValidateAndHandle();

        // Assert - buffer should not be reset
        Assert.Equal("test invalid", buffer.Text);
    }

    [Fact]
    public void ValidateAndHandle_WithAcceptHandler_CallsHandler()
    {
        // Arrange
        var handlerCalled = false;
        var validator = new SimpleValidator();
        var buffer = new Buffer(
            validator: validator,
            acceptHandler: b => { handlerCalled = true; return false; },
            document: new Document("hello"));

        // Act
        buffer.ValidateAndHandle();

        // Assert
        Assert.True(handlerCalled);
    }

    [Fact]
    public void ValidateAndHandle_AcceptHandlerReturnsTrue_KeepsText()
    {
        // Arrange
        var validator = new SimpleValidator();
        var buffer = new Buffer(
            validator: validator,
            acceptHandler: _ => true, // Keep text
            document: new Document("hello"));

        // Act
        buffer.ValidateAndHandle();

        // Assert - text should be kept
        Assert.Equal("hello", buffer.Text);
    }

    #endregion

    #region Async Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidText_SetsValidState()
    {
        // Arrange
        var validator = new DelayedValidator(10);
        var buffer = new Buffer(validator: validator, document: new Document("hello"));

        // Act
        await buffer.ValidateAsync();

        // Assert
        Assert.Equal(ValidationState.Valid, buffer.ValidationState);
    }

    [Fact]
    public async Task ValidateAsync_InvalidText_SetsInvalidState()
    {
        // Arrange
        var validator = new DelayedValidator(10);
        var buffer = new Buffer(validator: validator, document: new Document("this is bad"));

        // Act
        await buffer.ValidateAsync();

        // Assert
        Assert.Equal(ValidationState.Invalid, buffer.ValidationState);
        Assert.NotNull(buffer.ValidationError);
    }

    [Fact]
    public async Task ValidateAsync_AlreadyValidated_SkipsRevalidation()
    {
        // Arrange
        var validator = new DelayedValidator(10);
        var buffer = new Buffer(validator: validator, document: new Document("hello"));
        await buffer.ValidateAsync();
        Assert.Equal(ValidationState.Valid, buffer.ValidationState);

        // Act - validate again
        await buffer.ValidateAsync();

        // Assert - still valid (no re-validation)
        Assert.Equal(ValidationState.Valid, buffer.ValidationState);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentValidation_ThreadSafe()
    {
        // Arrange
        var validator = new DelayedValidator(5);
        var buffer = new Buffer(validator: validator, document: new Document("hello"));
        var iterations = 20;
        var barrier = new Barrier(3);

        // Act - concurrent validation and text changes
        var validateTask = Task.Run(async () =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                await buffer.ValidateAsync();
            }
        });

        var syncValidateTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.Validate();
            }
        });

        var textChangeTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.InsertText("x", fireEvent: false);
            }
        });

        // Assert - no exceptions
        await Task.WhenAll(validateTask, syncValidateTask, textChangeTask);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Validate_EmptyTextWithNotEmptyValidator_Fails()
    {
        // Arrange
        var validator = new NotEmptyValidator();
        var buffer = new Buffer(validator: validator, document: new Document(""));

        // Act
        var result = buffer.Validate();

        // Assert
        Assert.False(result);
        Assert.Equal(ValidationState.Invalid, buffer.ValidationState);
        Assert.Equal("Text cannot be empty", buffer.ValidationError!.Message);
    }

    [Fact]
    public void Validate_ThenInsertInvalidText_RequiresRevalidation()
    {
        // Arrange
        var validator = new SimpleValidator();
        var buffer = new Buffer(validator: validator, document: new Document("hello"));

        // First validation - valid
        Assert.True(buffer.Validate());
        Assert.Equal(ValidationState.Valid, buffer.ValidationState);

        // Insert invalid text
        buffer.InsertText(" invalid", fireEvent: false);
        Assert.Equal(ValidationState.Unknown, buffer.ValidationState);

        // Revalidate - should fail
        Assert.False(buffer.Validate());
        Assert.Equal(ValidationState.Invalid, buffer.ValidationState);
    }

    #endregion
}
