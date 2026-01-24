using Stroke.AutoSuggest;
using Stroke.Core;
using Stroke.Tests.AutoSuggest.Helpers;
using Xunit;

namespace Stroke.Tests.AutoSuggest;

/// <summary>
/// Unit tests for <see cref="DummyAutoSuggest"/>.
/// </summary>
public sealed class DummyAutoSuggestTests
{
    #region Test Setup

    private static (TestBuffer buffer, TestHistory history) CreateTestBuffer()
    {
        var history = new TestHistory();
        var buffer = new TestBuffer(history);
        return (buffer, history);
    }

    #endregion

    #region GetSuggestion Tests

    [Fact]
    public void GetSuggestion_AlwaysReturnsNull()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var autoSuggest = new DummyAutoSuggest();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    [Fact]
    public void GetSuggestion_WithEmptyDocument_ReturnsNull()
    {
        // Arrange
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("");
        var autoSuggest = new DummyAutoSuggest();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    [Fact]
    public void GetSuggestion_WithEmptyHistory_ReturnsNull()
    {
        // Arrange
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var autoSuggest = new DummyAutoSuggest();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    #endregion

    #region GetSuggestionAsync Tests

    [Fact]
    public async Task GetSuggestionAsync_AlwaysReturnsNull()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var autoSuggest = new DummyAutoSuggest();

        // Act
        var suggestion = await autoSuggest.GetSuggestionAsync(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    [Fact]
    public async Task GetSuggestionAsync_WithEmptyDocument_ReturnsNull()
    {
        // Arrange
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("");
        var autoSuggest = new DummyAutoSuggest();

        // Act
        var suggestion = await autoSuggest.GetSuggestionAsync(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task GetSuggestion_ConcurrentAccess_IsThreadSafe()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var autoSuggest = new DummyAutoSuggest();

        // Act - Call from multiple threads concurrently
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => autoSuggest.GetSuggestion(buffer, buffer.Document)))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert - All results should be null
        Assert.All(results, result => Assert.Null(result));
    }

    [Fact]
    public async Task GetSuggestionAsync_ConcurrentAccess_IsThreadSafe()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var autoSuggest = new DummyAutoSuggest();

        // Act - Call from multiple threads concurrently
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => autoSuggest.GetSuggestionAsync(buffer, buffer.Document).AsTask())
            .ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert - All results should be null
        Assert.All(results, result => Assert.Null(result));
    }

    #endregion
}
