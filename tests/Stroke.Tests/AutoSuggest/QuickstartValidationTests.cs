using Stroke.AutoSuggest;
using Stroke.Core;
using Stroke.Tests.AutoSuggest.Helpers;
using Xunit;

namespace Stroke.Tests.AutoSuggest;

/// <summary>
/// Tests validating all quickstart.md examples compile and run correctly.
/// </summary>
public sealed class QuickstartValidationTests
{
    #region Basic Usage - History-Based Suggestions

    [Fact]
    public void Quickstart_HistoryBasedSuggestions_Works()
    {
        // From quickstart.md: History-Based Suggestions example
        // Note: Using TestHistory/TestBuffer as InMemoryHistory/Buffer are not yet implemented

        // Create buffer with history
        var history = new TestHistory();
        history.AppendString("git commit -m 'initial'");
        history.AppendString("git push origin main");

        var buffer = new TestBuffer(history);

        // Create auto-suggest
        var autoSuggest = new AutoSuggestFromHistory();

        // Get suggestion for current input
        buffer.Document = new Document("git c");
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Validate: suggestion.Text == "ommit -m 'initial'"
        Assert.NotNull(suggestion);
        Assert.Equal("ommit -m 'initial'", suggestion.Text);
    }

    #endregion

    #region Basic Usage - Conditional Suggestions

    [Fact]
    public void Quickstart_ConditionalSuggestions_EnabledWhenTrue()
    {
        // From quickstart.md: Conditional Suggestions example

        var history = new TestHistory();
        history.AppendString("git commit -m 'initial'");
        var buffer = new TestBuffer(history);
        buffer.Document = new Document("git c");

        // Only suggest when not in Vi command mode
        var isInsertMode = true;
        var conditional = new ConditionalAutoSuggest(
            new AutoSuggestFromHistory(),
            () => isInsertMode
        );

        // Suggestions work when condition is true
        var suggestion = conditional.GetSuggestion(buffer, buffer.Document);
        Assert.NotNull(suggestion);
    }

    [Fact]
    public void Quickstart_ConditionalSuggestions_DisabledWhenFalse()
    {
        // From quickstart.md: Conditional Suggestions example

        var history = new TestHistory();
        history.AppendString("git commit -m 'initial'");
        var buffer = new TestBuffer(history);
        buffer.Document = new Document("git c");

        // No suggestions when condition is false
        var isInsertMode = false;
        var conditional = new ConditionalAutoSuggest(
            new AutoSuggestFromHistory(),
            () => isInsertMode
        );

        var suggestion = conditional.GetSuggestion(buffer, buffer.Document);
        Assert.Null(suggestion);
    }

    #endregion

    #region Basic Usage - Dynamic Provider Selection

    [Fact]
    public void Quickstart_DynamicProviderSelection_HistoryProvider()
    {
        // From quickstart.md: Dynamic Provider Selection example

        var history = new TestHistory();
        history.AppendString("git commit -m 'initial'");
        var buffer = new TestBuffer(history);
        buffer.Document = new Document("git c");

        IAutoSuggest? currentProvider = new AutoSuggestFromHistory();
        var dynamic = new DynamicAutoSuggest(() => currentProvider);

        // Uses history-based suggestions
        var suggestion = dynamic.GetSuggestion(buffer, buffer.Document);
        Assert.NotNull(suggestion);
        Assert.Equal("ommit -m 'initial'", suggestion.Text);
    }

    [Fact]
    public void Quickstart_DynamicProviderSelection_NullFallback()
    {
        // From quickstart.md: Dynamic Provider Selection example

        var history = new TestHistory();
        var buffer = new TestBuffer(history);
        buffer.Document = new Document("git c");

        // Disable suggestions
        IAutoSuggest? currentProvider = null;
        var dynamic = new DynamicAutoSuggest(() => currentProvider);

        var suggestion = dynamic.GetSuggestion(buffer, buffer.Document);
        // Falls back to DummyAutoSuggest (returns null)
        Assert.Null(suggestion);
    }

    #endregion

    #region Basic Usage - Background Suggestion Generation

    [Fact]
    public void Quickstart_ThreadedAutoSuggest_SyncExecution()
    {
        // From quickstart.md: Background Suggestion Generation example

        var history = new TestHistory();
        history.AppendString("git commit -m 'test'");
        var buffer = new TestBuffer(history);
        buffer.Document = new Document("git c");

        // Wrap provider for background execution
        var threaded = new ThreadedAutoSuggest(new AutoSuggestFromHistory());

        // Sync call still works (runs on current thread)
        var suggestion = threaded.GetSuggestion(buffer, buffer.Document);
        Assert.NotNull(suggestion);
    }

    [Fact]
    public async Task Quickstart_ThreadedAutoSuggest_AsyncExecution()
    {
        // From quickstart.md: Background Suggestion Generation example

        var history = new TestHistory();
        history.AppendString("git commit -m 'test'");
        var buffer = new TestBuffer(history);
        buffer.Document = new Document("git c");

        // Wrap provider for background execution
        var threaded = new ThreadedAutoSuggest(new AutoSuggestFromHistory());

        // Async call runs on thread pool
        var suggestion = await threaded.GetSuggestionAsync(buffer, buffer.Document);
        Assert.NotNull(suggestion);
    }

    #endregion

    #region API Reference - Suggestion

    [Fact]
    public void Quickstart_SuggestionApi_Creation()
    {
        // From quickstart.md: API Reference - Suggestion

        // Create suggestion
        var suggestion = new Suggestion("text to append");

        // Access text
        var text = suggestion.Text;
        Assert.Equal("text to append", text);
    }

    [Fact]
    public void Quickstart_SuggestionApi_ValueEquality()
    {
        // From quickstart.md: API Reference - Suggestion

        var a = new Suggestion("hello");
        var b = new Suggestion("hello");
        var equal = a == b; // true
        Assert.True(equal);
    }

    [Fact]
    public void Quickstart_SuggestionApi_ToString()
    {
        // From quickstart.md: API Reference - Suggestion

        var suggestion = new Suggestion("text to append");
        var debug = suggestion.ToString();
        Assert.Equal("Suggestion(text to append)", debug);
    }

    #endregion

    #region Common Patterns - Combining Wrappers

    [Fact]
    public void Quickstart_CombiningWrappers_ConditionalThreadedHistory()
    {
        // From quickstart.md: Common Patterns - Combining Wrappers

        var history = new TestHistory();
        history.AppendString("git commit -m 'test'");
        var buffer = new TestBuffer(history);
        buffer.Document = new Document("git c");

        var isCompletionMenuVisible = false;

        // Conditional + Threaded + History
        var autoSuggest = new ConditionalAutoSuggest(
            new ThreadedAutoSuggest(
                new AutoSuggestFromHistory()
            ),
            () => !isCompletionMenuVisible
        );

        // When completion menu is not visible, suggestions work
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);
        Assert.NotNull(suggestion);

        // When completion menu is visible, no suggestions
        isCompletionMenuVisible = true;
        suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);
        Assert.Null(suggestion);
    }

    #endregion

    #region Common Patterns - Null-Safe Fallback

    [Fact]
    public void Quickstart_NullSafeFallback_Works()
    {
        // From quickstart.md: Common Patterns - Null-Safe Fallback

        IAutoSuggest? GetCurrentProvider() => null;

        var buffer = new TestBuffer(new TestHistory());
        buffer.Document = new Document("test");

        var dynamic = new DynamicAutoSuggest(GetCurrentProvider);

        // If GetCurrentProvider() returns null, DummyAutoSuggest is used
        // No null reference exceptions
        var suggestion = dynamic.GetSuggestion(buffer, buffer.Document);
        Assert.Null(suggestion); // DummyAutoSuggest returns null, not an exception
    }

    #endregion

    #region Thread Safety

    [Fact]
    public async Task Quickstart_ThreadSafety_ConcurrentAccess()
    {
        // From quickstart.md: Thread Safety section
        // All auto-suggest types are thread-safe

        var history = new TestHistory();
        history.AppendString("git commit -m 'test'");
        var buffer = new TestBuffer(history);
        buffer.Document = new Document("git c");

        var autoSuggest = new AutoSuggestFromHistory();

        // Concurrent access from multiple threads
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);
                    Assert.NotNull(suggestion);
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    #endregion
}
