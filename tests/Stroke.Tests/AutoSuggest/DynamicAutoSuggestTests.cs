using Stroke.AutoSuggest;
using Stroke.Core;
using Stroke.Tests.AutoSuggest.Helpers;
using Xunit;

namespace Stroke.Tests.AutoSuggest;

/// <summary>
/// Unit tests for <see cref="DynamicAutoSuggest"/>.
/// </summary>
public sealed class DynamicAutoSuggestTests
{
    #region Test Setup

    private static (TestBuffer buffer, TestHistory history) CreateTestBuffer()
    {
        var history = new TestHistory();
        var buffer = new TestBuffer(history);
        return (buffer, history);
    }

    #endregion

    #region Delegation Tests

    [Fact]
    public void GetSuggestion_DelegatesToReturnedProvider()
    {
        // Arrange - Acceptance scenario 1
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var inner = new AutoSuggestFromHistory();
        var dynamic = new DynamicAutoSuggest(() => inner);

        // Act
        var suggestion = dynamic.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("ommit", suggestion.Text);
    }

    [Fact]
    public async Task GetSuggestionAsync_DelegatesToReturnedProvider()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var inner = new AutoSuggestFromHistory();
        var dynamic = new DynamicAutoSuggest(() => inner);

        // Act
        var suggestion = await dynamic.GetSuggestionAsync(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("ommit", suggestion.Text);
    }

    #endregion

    #region Provider Switch Tests

    [Fact]
    public void GetSuggestion_ProviderSwitch_AffectsSubsequentSuggestions()
    {
        // Arrange - Acceptance scenario 2
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        IAutoSuggest? currentProvider = new AutoSuggestFromHistory();
        var dynamic = new DynamicAutoSuggest(() => currentProvider);

        // Act & Assert - Using history provider
        var suggestion1 = dynamic.GetSuggestion(buffer, buffer.Document);
        Assert.NotNull(suggestion1);
        Assert.Equal("ommit", suggestion1.Text);

        // Switch to dummy provider
        currentProvider = new DummyAutoSuggest();

        // Act & Assert - Now using dummy provider
        var suggestion2 = dynamic.GetSuggestion(buffer, buffer.Document);
        Assert.Null(suggestion2);

        // Switch back to history provider
        currentProvider = new AutoSuggestFromHistory();

        // Act & Assert - Back to history provider
        var suggestion3 = dynamic.GetSuggestion(buffer, buffer.Document);
        Assert.NotNull(suggestion3);
        Assert.Equal("ommit", suggestion3.Text);
    }

    #endregion

    #region Null Provider Fallback Tests

    [Fact]
    public void GetSuggestion_NullProvider_FallsBackToDummyAutoSuggest()
    {
        // Arrange - Acceptance scenario 3
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var dynamic = new DynamicAutoSuggest(() => null);

        // Act
        var suggestion = dynamic.GetSuggestion(buffer, buffer.Document);

        // Assert - DummyAutoSuggest always returns null
        Assert.Null(suggestion);
    }

    [Fact]
    public async Task GetSuggestionAsync_NullProvider_FallsBackToDummyAutoSuggest()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var dynamic = new DynamicAutoSuggest(() => null);

        // Act
        var suggestion = await dynamic.GetSuggestionAsync(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    #endregion

    #region Callback Evaluation Tests

    [Fact]
    public void GetSuggestion_CallbackEvaluatedOnEveryCall()
    {
        // Arrange - Per FR-022
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var callCount = 0;
        var dynamic = new DynamicAutoSuggest(() =>
        {
            callCount++;
            return new DummyAutoSuggest();
        });

        // Act
        dynamic.GetSuggestion(buffer, buffer.Document);
        dynamic.GetSuggestion(buffer, buffer.Document);
        dynamic.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task GetSuggestionAsync_CallbackEvaluatedOnEveryCall()
    {
        // Arrange - Per FR-022
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var callCount = 0;
        var dynamic = new DynamicAutoSuggest(() =>
        {
            callCount++;
            return new DummyAutoSuggest();
        });

        // Act
        await dynamic.GetSuggestionAsync(buffer, buffer.Document);
        await dynamic.GetSuggestionAsync(buffer, buffer.Document);
        await dynamic.GetSuggestionAsync(buffer, buffer.Document);

        // Assert
        Assert.Equal(3, callCount);
    }

    #endregion

    #region Null Parameter Tests

    [Fact]
    public void Constructor_NullGetAutoSuggest_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert - Per FR-029
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DynamicAutoSuggest(null!));
        Assert.Equal("getAutoSuggest", exception.ParamName);
    }

    #endregion

    #region Exception Propagation Tests

    [Fact]
    public void GetSuggestion_CallbackThrows_ExceptionPropagates()
    {
        // Arrange - Per Edge Cases
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var dynamic = new DynamicAutoSuggest(() => throw new InvalidOperationException("Callback error"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => dynamic.GetSuggestion(buffer, buffer.Document));
        Assert.Equal("Callback error", exception.Message);
    }

    [Fact]
    public async Task GetSuggestionAsync_CallbackThrows_ExceptionPropagates()
    {
        // Arrange
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var dynamic = new DynamicAutoSuggest(() => throw new InvalidOperationException("Callback error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await dynamic.GetSuggestionAsync(buffer, buffer.Document));
        Assert.Equal("Callback error", exception.Message);
    }

    [Fact]
    public void GetSuggestion_ProviderThrows_ExceptionPropagates()
    {
        // Arrange
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var dynamic = new DynamicAutoSuggest(() => new ThrowingAutoSuggest());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => dynamic.GetSuggestion(buffer, buffer.Document));
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Auto-suggest that throws an exception.
    /// </summary>
    private sealed class ThrowingAutoSuggest : IAutoSuggest
    {
        public Suggestion? GetSuggestion(IBuffer buffer, Document document)
            => throw new InvalidOperationException("Provider error");

        public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
            => throw new InvalidOperationException("Provider error");
    }

    #endregion
}
