using Stroke.AutoSuggest;
using Xunit;

namespace Stroke.Tests.AutoSuggest;

/// <summary>
/// Unit tests for <see cref="Suggestion"/> record type.
/// </summary>
public sealed class SuggestionTests
{
    #region Text Property Tests

    [Fact]
    public void Constructor_WithText_SetsTextProperty()
    {
        // Arrange & Act
        var suggestion = new Suggestion("hello world");

        // Assert
        Assert.Equal("hello world", suggestion.Text);
    }

    [Fact]
    public void Constructor_WithEmptyText_SetsEmptyTextProperty()
    {
        // Arrange & Act
        var suggestion = new Suggestion("");

        // Assert
        Assert.Equal("", suggestion.Text);
    }

    [Fact]
    public void Constructor_WithNullText_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        // Record primary constructor parameters are non-nullable by default
        // This test documents the expected behavior
        Assert.Throws<ArgumentNullException>(() => new Suggestion(null!));
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var suggestion = new Suggestion("hello world");

        // Act
        var result = suggestion.ToString();

        // Assert
        Assert.Equal("Suggestion(hello world)", result);
    }

    [Fact]
    public void ToString_WithEmptyText_ReturnsCorrectFormat()
    {
        // Arrange
        var suggestion = new Suggestion("");

        // Act
        var result = suggestion.ToString();

        // Assert
        Assert.Equal("Suggestion()", result);
    }

    [Fact]
    public void ToString_WithSpecialCharacters_ReturnsCorrectFormat()
    {
        // Arrange
        var suggestion = new Suggestion("git commit -m 'test'");

        // Act
        var result = suggestion.ToString();

        // Assert
        Assert.Equal("Suggestion(git commit -m 'test')", result);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameText_ReturnsTrue()
    {
        // Arrange
        var suggestion1 = new Suggestion("hello");
        var suggestion2 = new Suggestion("hello");

        // Act & Assert
        Assert.Equal(suggestion1, suggestion2);
        Assert.True(suggestion1 == suggestion2);
        Assert.False(suggestion1 != suggestion2);
    }

    [Fact]
    public void Equals_WithDifferentText_ReturnsFalse()
    {
        // Arrange
        var suggestion1 = new Suggestion("hello");
        var suggestion2 = new Suggestion("world");

        // Act & Assert
        Assert.NotEqual(suggestion1, suggestion2);
        Assert.False(suggestion1 == suggestion2);
        Assert.True(suggestion1 != suggestion2);
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var suggestion = new Suggestion("hello");

        // Act & Assert
        Assert.False(suggestion.Equals(null));
    }

    [Fact]
    public void GetHashCode_SameText_ReturnsSameHashCode()
    {
        // Arrange
        var suggestion1 = new Suggestion("hello");
        var suggestion2 = new Suggestion("hello");

        // Act & Assert
        Assert.Equal(suggestion1.GetHashCode(), suggestion2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentText_ReturnsDifferentHashCode()
    {
        // Arrange
        var suggestion1 = new Suggestion("hello");
        var suggestion2 = new Suggestion("world");

        // Act & Assert
        // Note: Different hash codes are not guaranteed but highly likely for different strings
        Assert.NotEqual(suggestion1.GetHashCode(), suggestion2.GetHashCode());
    }

    #endregion

}
