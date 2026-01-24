using System.Diagnostics;
using Stroke.AutoSuggest;
using Stroke.Core;
using Stroke.Tests.AutoSuggest.Helpers;
using Xunit;

namespace Stroke.Tests.AutoSuggest;

/// <summary>
/// Unit tests for <see cref="ThreadedAutoSuggest"/>.
/// </summary>
public sealed class ThreadedAutoSuggestTests
{
    #region Test Setup

    private static (TestBuffer buffer, TestHistory history) CreateTestBuffer()
    {
        var history = new TestHistory();
        var buffer = new TestBuffer(history);
        return (buffer, history);
    }

    #endregion

    #region Async Thread Execution Tests

    [Fact]
    public async Task GetSuggestionAsync_ExecutesOnDifferentThread()
    {
        // Arrange - Acceptance scenario 1
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var callingThreadId = Environment.CurrentManagedThreadId;
        int? executionThreadId = null;

        // Callback captures thread ID then sleeps to keep work running
        var inner = new ThreadCapturingAutoSuggest(id =>
        {
            executionThreadId = id;
            Thread.Sleep(100); // Keep work running while calling thread is busy
        });
        var threaded = new ThreadedAutoSuggest(inner);

        // Act - Start async work without awaiting immediately
        var task = threaded.GetSuggestionAsync(buffer, buffer.Document);

        // Keep calling thread busy so ThreadPool must use a different thread
        Thread.Sleep(50);

        await task;

        // Assert - Execution thread should be different from calling thread
        Assert.NotNull(executionThreadId);
        Assert.NotEqual(callingThreadId, executionThreadId.Value);
    }

    #endregion

    #region Sync Thread Execution Tests

    [Fact]
    public void GetSuggestion_ExecutesOnCurrentThread()
    {
        // Arrange - Acceptance scenario 2
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var callingThreadId = Environment.CurrentManagedThreadId;
        int? executionThreadId = null;

        var inner = new ThreadCapturingAutoSuggest(id => executionThreadId = id);
        var threaded = new ThreadedAutoSuggest(inner);

        // Act
        threaded.GetSuggestion(buffer, buffer.Document);

        // Assert - Execution thread should be same as calling thread
        Assert.NotNull(executionThreadId);
        Assert.Equal(callingThreadId, executionThreadId.Value);
    }

    #endregion

    #region Async Immediate Return Tests

    [Fact]
    public async Task GetSuggestionAsync_ReturnsImmediatelyForSlowProvider()
    {
        // Arrange - Acceptance scenario 3: 100ms provider, returns within 10ms
        var (buffer, history) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var inner = new SlowAutoSuggest(TimeSpan.FromMilliseconds(100));
        var threaded = new ThreadedAutoSuggest(inner);

        // Act - Start the task and measure time to return
        var sw = Stopwatch.StartNew();
        var task = threaded.GetSuggestionAsync(buffer, buffer.Document);
        var returnTime = sw.ElapsedMilliseconds;

        // Wait for actual completion
        await task;
        var completionTime = sw.ElapsedMilliseconds;

        // Assert - Task should return immediately (well under 100ms)
        Assert.True(returnTime < 50, $"Expected return time < 50ms, but was {returnTime}ms");
        // And completion should happen after ~100ms
        Assert.True(completionTime >= 90, $"Expected completion >= 90ms, but was {completionTime}ms");
    }

    #endregion

    #region Null Parameter Tests

    [Fact]
    public void Constructor_NullAutoSuggest_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert - Per FR-029
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ThreadedAutoSuggest(null!));
        Assert.Equal("autoSuggest", exception.ParamName);
    }

    #endregion

    #region Exception Propagation Tests

    [Fact]
    public async Task GetSuggestionAsync_BackgroundException_PropagatesWhenAwaited()
    {
        // Arrange - Per Edge Cases
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var inner = new ThrowingAutoSuggest();
        var threaded = new ThreadedAutoSuggest(inner);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await threaded.GetSuggestionAsync(buffer, buffer.Document));
        Assert.Equal("Background error", exception.Message);
    }

    [Fact]
    public void GetSuggestion_Exception_PropagatesImmediately()
    {
        // Arrange
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("test");
        var inner = new ThrowingAutoSuggest();
        var threaded = new ThreadedAutoSuggest(inner);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => threaded.GetSuggestion(buffer, buffer.Document));
        Assert.Equal("Background error", exception.Message);
    }

    #endregion

    #region Delegation Tests

    [Fact]
    public void GetSuggestion_DelegatesToWrappedProvider()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var inner = new AutoSuggestFromHistory();
        var threaded = new ThreadedAutoSuggest(inner);

        // Act
        var suggestion = threaded.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("ommit", suggestion.Text);
    }

    [Fact]
    public async Task GetSuggestionAsync_DelegatesToWrappedProvider()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("git c");
        var inner = new AutoSuggestFromHistory();
        var threaded = new ThreadedAutoSuggest(inner);

        // Act
        var suggestion = await threaded.GetSuggestionAsync(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("ommit", suggestion.Text);
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Auto-suggest that captures the executing thread ID.
    /// </summary>
    private sealed class ThreadCapturingAutoSuggest : IAutoSuggest
    {
        private readonly Action<int> _captureThreadId;

        public ThreadCapturingAutoSuggest(Action<int> captureThreadId)
        {
            _captureThreadId = captureThreadId;
        }

        public Suggestion? GetSuggestion(IBuffer buffer, Document document)
        {
            _captureThreadId(Environment.CurrentManagedThreadId);
            return new Suggestion("test");
        }

        public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
        {
            _captureThreadId(Environment.CurrentManagedThreadId);
            return ValueTask.FromResult<Suggestion?>(new Suggestion("test"));
        }
    }

    /// <summary>
    /// Auto-suggest that takes a long time to execute.
    /// </summary>
    private sealed class SlowAutoSuggest : IAutoSuggest
    {
        private readonly TimeSpan _delay;

        public SlowAutoSuggest(TimeSpan delay)
        {
            _delay = delay;
        }

        public Suggestion? GetSuggestion(IBuffer buffer, Document document)
        {
            Thread.Sleep(_delay);
            return new Suggestion("slow");
        }

        public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
        {
            Thread.Sleep(_delay);
            return ValueTask.FromResult<Suggestion?>(new Suggestion("slow"));
        }
    }

    /// <summary>
    /// Auto-suggest that throws an exception.
    /// </summary>
    private sealed class ThrowingAutoSuggest : IAutoSuggest
    {
        public Suggestion? GetSuggestion(IBuffer buffer, Document document)
            => throw new InvalidOperationException("Background error");

        public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
            => throw new InvalidOperationException("Background error");
    }

    #endregion
}
