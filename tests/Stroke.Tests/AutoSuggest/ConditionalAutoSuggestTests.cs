using Stroke.AutoSuggest;
using Stroke.Core;
using Stroke.Tests.AutoSuggest.Helpers;
using Xunit;

namespace Stroke.Tests.AutoSuggest;

/// <summary>
/// Unit tests for <see cref="ConditionalAutoSuggest"/>.
/// </summary>
public sealed class ConditionalAutoSuggestTests
{
    #region Test Setup

    private static (TestBuffer buffer, TestHistory history) CreateTestBuffer()
    {
        var history = new TestHistory();
        var buffer = new TestBuffer(history);
        return (buffer, history);
    }

    #endregion

    #region True Condition Tests

    [Fact]
    public void GetSuggestion_TrueCondition_AllowsSuggestions()
    {
        // Arrange - Acceptance scenario 1
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var inner = new AutoSuggestFromHistory();
        var conditional = new ConditionalAutoSuggest(inner, () => true);

        // Act
        var suggestion = conditional.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("ommit", suggestion.Text);
    }

    [Fact]
    public async Task GetSuggestionAsync_TrueCondition_AllowsSuggestions()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var inner = new AutoSuggestFromHistory();
        var conditional = new ConditionalAutoSuggest(inner, () => true);

        // Act
        var suggestion = await conditional.GetSuggestionAsync(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("ommit", suggestion.Text);
    }

    #endregion

    #region False Condition Tests

    [Fact]
    public void GetSuggestion_FalseCondition_ReturnsNullWithoutCallingWrapped()
    {
        // Arrange - Acceptance scenario 2
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var callCount = 0;
        var inner = new CallTrackingAutoSuggest(() => callCount++);
        var conditional = new ConditionalAutoSuggest(inner, () => false);

        // Act
        var suggestion = conditional.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
        Assert.Equal(0, callCount); // Inner was never called
    }

    [Fact]
    public async Task GetSuggestionAsync_FalseCondition_ReturnsNullWithoutCallingWrapped()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var callCount = 0;
        var inner = new CallTrackingAutoSuggest(() => callCount++);
        var conditional = new ConditionalAutoSuggest(inner, () => false);

        // Act
        var suggestion = await conditional.GetSuggestionAsync(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
        Assert.Equal(0, callCount);
    }

    #endregion

    #region Dynamic Condition Tests

    [Fact]
    public void GetSuggestion_DynamicCondition_ChangesAffectResults()
    {
        // Arrange - Acceptance scenario 3
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var inner = new AutoSuggestFromHistory();
        var condition = true;
        var conditional = new ConditionalAutoSuggest(inner, () => condition);

        // Act & Assert - condition is true
        var suggestion1 = conditional.GetSuggestion(buffer, buffer.Document);
        Assert.NotNull(suggestion1);

        // Change condition
        condition = false;

        // Act & Assert - condition is now false
        var suggestion2 = conditional.GetSuggestion(buffer, buffer.Document);
        Assert.Null(suggestion2);

        // Change condition back
        condition = true;

        // Act & Assert - condition is true again
        var suggestion3 = conditional.GetSuggestion(buffer, buffer.Document);
        Assert.NotNull(suggestion3);
    }

    #endregion

    #region Null Parameter Tests

    [Fact]
    public void Constructor_NullAutoSuggest_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert - Per FR-029
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ConditionalAutoSuggest(null!, () => true));
        Assert.Equal("autoSuggest", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullFilter_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert - Per FR-029
        var inner = new DummyAutoSuggest();
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ConditionalAutoSuggest(inner, null!));
        Assert.Equal("filter", exception.ParamName);
    }

    #endregion

    #region Exception Propagation Tests

    [Fact]
    public void GetSuggestion_FilterThrows_ExceptionPropagates()
    {
        // Arrange - Per Edge Cases
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var inner = new DummyAutoSuggest();
        var conditional = new ConditionalAutoSuggest(inner, () => throw new InvalidOperationException("Filter error"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => conditional.GetSuggestion(buffer, buffer.Document));
        Assert.Equal("Filter error", exception.Message);
    }

    [Fact]
    public async Task GetSuggestionAsync_FilterThrows_ExceptionPropagates()
    {
        // Arrange
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var inner = new DummyAutoSuggest();
        var conditional = new ConditionalAutoSuggest(inner, () => throw new InvalidOperationException("Filter error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await conditional.GetSuggestionAsync(buffer, buffer.Document));
        Assert.Equal("Filter error", exception.Message);
    }

    [Fact]
    public void GetSuggestion_InnerThrows_ExceptionPropagates()
    {
        // Arrange
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var inner = new ThrowingAutoSuggest();
        var conditional = new ConditionalAutoSuggest(inner, () => true);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => conditional.GetSuggestion(buffer, buffer.Document));
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Auto-suggest that tracks calls for verification.
    /// </summary>
    private sealed class CallTrackingAutoSuggest : IAutoSuggest
    {
        private readonly Action _onCall;

        public CallTrackingAutoSuggest(Action onCall)
        {
            _onCall = onCall;
        }

        public Suggestion? GetSuggestion(IBuffer buffer, Document document)
        {
            _onCall();
            return new Suggestion("test");
        }

        public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
        {
            _onCall();
            return ValueTask.FromResult<Suggestion?>(new Suggestion("test"));
        }
    }

    /// <summary>
    /// Auto-suggest that throws an exception.
    /// </summary>
    private sealed class ThrowingAutoSuggest : IAutoSuggest
    {
        public Suggestion? GetSuggestion(IBuffer buffer, Document document)
            => throw new InvalidOperationException("Inner error");

        public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
            => throw new InvalidOperationException("Inner error");
    }

    #endregion
}
