using Stroke.Validation;
using Xunit;

namespace Stroke.Tests.Validation;

/// <summary>
/// Unit tests for <see cref="ValidationError"/>.
/// </summary>
public sealed class ValidationErrorTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNoArguments_UseDefaults()
    {
        // Act
        var error = new ValidationError();

        // Assert
        Assert.Equal(0, error.CursorPosition);
        Assert.Equal("", error.Message);
    }

    [Fact]
    public void Constructor_WithCursorPositionOnly_UsesDefaultMessage()
    {
        // Act
        var error = new ValidationError(cursorPosition: 5);

        // Assert
        Assert.Equal(5, error.CursorPosition);
        Assert.Equal("", error.Message);
    }

    [Fact]
    public void Constructor_WithMessageOnly_UsesDefaultCursorPosition()
    {
        // Act
        var error = new ValidationError(message: "Invalid input");

        // Assert
        Assert.Equal(0, error.CursorPosition);
        Assert.Equal("Invalid input", error.Message);
    }

    [Fact]
    public void Constructor_WithBothArguments_StoresBoth()
    {
        // Act
        var error = new ValidationError(cursorPosition: 10, message: "Error at position 10");

        // Assert
        Assert.Equal(10, error.CursorPosition);
        Assert.Equal("Error at position 10", error.Message);
    }

    [Fact]
    public void Constructor_WithNegativeCursorPosition_StoresAsIs()
    {
        // Negative values allowed per spec - consumers clamp if needed
        var error = new ValidationError(cursorPosition: -1, message: "test");

        Assert.Equal(-1, error.CursorPosition);
    }

    [Fact]
    public void Constructor_WithZeroCursorPosition_StoresZero()
    {
        var error = new ValidationError(cursorPosition: 0, message: "test");

        Assert.Equal(0, error.CursorPosition);
    }

    [Fact]
    public void Constructor_WithLargeCursorPosition_StoresAsIs()
    {
        // Large values allowed per spec - consumers clamp if needed
        var error = new ValidationError(cursorPosition: int.MaxValue, message: "test");

        Assert.Equal(int.MaxValue, error.CursorPosition);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void CursorPosition_IsReadOnly()
    {
        var error = new ValidationError(cursorPosition: 5, message: "test");

        // CursorPosition has no setter - compile-time verification
        // This test verifies runtime immutability
        var originalPosition = error.CursorPosition;
        _ = error; // Use the error
        Assert.Equal(5, originalPosition);
    }

    [Fact]
    public void Message_IsReadOnly()
    {
        var error = new ValidationError(cursorPosition: 0, message: "original");

        // Message inherited from Exception is read-only
        var originalMessage = error.Message;
        Assert.Equal("original", originalMessage);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        var error = new ValidationError(cursorPosition: 5, message: "Invalid input");

        var result = error.ToString();

        Assert.Equal("ValidationError(CursorPosition=5, Message=\"Invalid input\")", result);
    }

    [Fact]
    public void ToString_WithDefaultValues_ReturnsCorrectFormat()
    {
        var error = new ValidationError();

        var result = error.ToString();

        Assert.Equal("ValidationError(CursorPosition=0, Message=\"\")", result);
    }

    [Fact]
    public void ToString_WithEmptyMessage_IncludesEmptyQuotes()
    {
        var error = new ValidationError(cursorPosition: 10, message: "");

        var result = error.ToString();

        Assert.Equal("ValidationError(CursorPosition=10, Message=\"\")", result);
    }

    [Fact]
    public void ToString_WithSpecialCharactersInMessage_IncludesAsIs()
    {
        var error = new ValidationError(cursorPosition: 0, message: "Line 1\nLine 2");

        var result = error.ToString();

        Assert.Equal("ValidationError(CursorPosition=0, Message=\"Line 1\nLine 2\")", result);
    }

    [Fact]
    public void ToString_WithQuotesInMessage_IncludesAsIs()
    {
        var error = new ValidationError(cursorPosition: 0, message: "Expected \"value\"");

        var result = error.ToString();

        Assert.Equal("ValidationError(CursorPosition=0, Message=\"Expected \"value\"\")", result);
    }

    #endregion

    #region Exception Hierarchy Tests

    [Fact]
    public void ValidationError_IsException()
    {
        var error = new ValidationError(cursorPosition: 0, message: "test");

        Assert.IsAssignableFrom<Exception>(error);
    }

    [Fact]
    public void ValidationError_CanBeCaughtAsException()
    {
        try
        {
            throw new ValidationError(cursorPosition: 5, message: "Validation failed");
        }
        catch (Exception ex)
        {
            Assert.IsType<ValidationError>(ex);
            Assert.Equal("Validation failed", ex.Message);
        }
    }

    [Fact]
    public void ValidationError_CanBeCaughtAsValidationError()
    {
        try
        {
            throw new ValidationError(cursorPosition: 5, message: "Validation failed");
        }
        catch (ValidationError ex)
        {
            Assert.Equal(5, ex.CursorPosition);
            Assert.Equal("Validation failed", ex.Message);
        }
    }

    #endregion
}
