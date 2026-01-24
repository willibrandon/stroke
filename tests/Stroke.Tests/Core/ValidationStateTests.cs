using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for the ValidationState enum.
/// </summary>
public class ValidationStateTests
{
    #region Enum Values Tests

    [Fact]
    public void ValidationState_HasValidValue()
    {
        // Arrange & Act
        var valid = ValidationState.Valid;

        // Assert
        Assert.Equal(ValidationState.Valid, valid);
    }

    [Fact]
    public void ValidationState_HasInvalidValue()
    {
        // Arrange & Act
        var invalid = ValidationState.Invalid;

        // Assert
        Assert.Equal(ValidationState.Invalid, invalid);
    }

    [Fact]
    public void ValidationState_HasUnknownValue()
    {
        // Arrange & Act
        var unknown = ValidationState.Unknown;

        // Assert
        Assert.Equal(ValidationState.Unknown, unknown);
    }

    [Fact]
    public void ValidationState_DefaultsToValid()
    {
        // Arrange & Act
        // Default value of enum is the first value (0)
        var defaultState = default(ValidationState);

        // Assert
        Assert.Equal(ValidationState.Valid, defaultState);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ValidationState_Valid_ToStringReturnsValid()
    {
        // Arrange
        var state = ValidationState.Valid;

        // Act
        var result = state.ToString();

        // Assert
        Assert.Equal("Valid", result);
    }

    [Fact]
    public void ValidationState_Invalid_ToStringReturnsInvalid()
    {
        // Arrange
        var state = ValidationState.Invalid;

        // Act
        var result = state.ToString();

        // Assert
        Assert.Equal("Invalid", result);
    }

    [Fact]
    public void ValidationState_Unknown_ToStringReturnsUnknown()
    {
        // Arrange
        var state = ValidationState.Unknown;

        // Act
        var result = state.ToString();

        // Assert
        Assert.Equal("Unknown", result);
    }

    #endregion

    #region Enum Parsing Tests

    [Theory]
    [InlineData("Valid", ValidationState.Valid)]
    [InlineData("Invalid", ValidationState.Invalid)]
    [InlineData("Unknown", ValidationState.Unknown)]
    public void ValidationState_ParseFromString_ReturnsCorrectValue(string input, ValidationState expected)
    {
        // Act
        var result = Enum.Parse<ValidationState>(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("VALID")]
    [InlineData("VaLiD")]
    public void ValidationState_ParseFromString_CaseInsensitive(string input)
    {
        // Act
        var result = Enum.Parse<ValidationState>(input, ignoreCase: true);

        // Assert
        Assert.Equal(ValidationState.Valid, result);
    }

    [Fact]
    public void ValidationState_TryParse_ValidValue_ReturnsTrue()
    {
        // Act
        var success = Enum.TryParse<ValidationState>("Invalid", out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(ValidationState.Invalid, result);
    }

    [Fact]
    public void ValidationState_TryParse_InvalidValue_ReturnsFalse()
    {
        // Act
        var success = Enum.TryParse<ValidationState>("NotAValidState", out var result);

        // Assert
        Assert.False(success);
        Assert.Equal(default(ValidationState), result);
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void ValidationState_Valid_NotEqualsInvalid()
    {
        // Assert
        Assert.NotEqual(ValidationState.Valid, ValidationState.Invalid);
    }

    [Fact]
    public void ValidationState_Valid_NotEqualsUnknown()
    {
        // Assert
        Assert.NotEqual(ValidationState.Valid, ValidationState.Unknown);
    }

    [Fact]
    public void ValidationState_Invalid_NotEqualsUnknown()
    {
        // Assert
        Assert.NotEqual(ValidationState.Invalid, ValidationState.Unknown);
    }

    [Fact]
    public void ValidationState_SameValue_AreEqual()
    {
        // Arrange
        var state1 = ValidationState.Invalid;
        var state2 = ValidationState.Invalid;

        // Assert
        Assert.Equal(state1, state2);
    }

    #endregion

    #region Underlying Value Tests

    [Fact]
    public void ValidationState_Valid_HasExpectedUnderlyingValue()
    {
        // The enum is defined with Valid first (0), Invalid second (1), Unknown third (2)
        Assert.Equal(0, (int)ValidationState.Valid);
    }

    [Fact]
    public void ValidationState_Invalid_HasExpectedUnderlyingValue()
    {
        Assert.Equal(1, (int)ValidationState.Invalid);
    }

    [Fact]
    public void ValidationState_Unknown_HasExpectedUnderlyingValue()
    {
        Assert.Equal(2, (int)ValidationState.Unknown);
    }

    [Fact]
    public void ValidationState_CastFromInt_ReturnsCorrectValue()
    {
        // Act
        var state = (ValidationState)1;

        // Assert
        Assert.Equal(ValidationState.Invalid, state);
    }

    #endregion

    #region Collection/Dictionary Tests

    [Fact]
    public void ValidationState_CanBeUsedAsDictionaryKey()
    {
        // Arrange
        var dict = new Dictionary<ValidationState, string>
        {
            { ValidationState.Valid, "The input is valid" },
            { ValidationState.Invalid, "The input is invalid" },
            { ValidationState.Unknown, "Validation pending" }
        };

        // Act & Assert
        Assert.Equal("The input is valid", dict[ValidationState.Valid]);
        Assert.Equal("The input is invalid", dict[ValidationState.Invalid]);
        Assert.Equal("Validation pending", dict[ValidationState.Unknown]);
    }

    [Fact]
    public void ValidationState_GetValues_ReturnsAllValues()
    {
        // Act
        var values = Enum.GetValues<ValidationState>();

        // Assert
        Assert.Equal(3, values.Length);
        Assert.Contains(ValidationState.Valid, values);
        Assert.Contains(ValidationState.Invalid, values);
        Assert.Contains(ValidationState.Unknown, values);
    }

    #endregion

    #region Switch/Pattern Matching Tests

    [Theory]
    [InlineData(ValidationState.Valid, "valid")]
    [InlineData(ValidationState.Invalid, "invalid")]
    [InlineData(ValidationState.Unknown, "unknown")]
    public void ValidationState_SwitchExpression_MatchesCorrectly(ValidationState state, string expected)
    {
        // Act
        var result = state switch
        {
            ValidationState.Valid => "valid",
            ValidationState.Invalid => "invalid",
            ValidationState.Unknown => "unknown",
            _ => throw new ArgumentOutOfRangeException()
        };

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}
