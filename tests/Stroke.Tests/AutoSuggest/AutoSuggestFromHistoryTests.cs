using Stroke.AutoSuggest;
using Stroke.Core;
using Stroke.Tests.AutoSuggest.Helpers;
using Xunit;

namespace Stroke.Tests.AutoSuggest;

/// <summary>
/// Unit tests for <see cref="AutoSuggestFromHistory"/>.
/// </summary>
public sealed class AutoSuggestFromHistoryTests
{
    #region Test Setup

    private static (TestBuffer buffer, TestHistory history) CreateTestBuffer()
    {
        var history = new TestHistory();
        var buffer = new TestBuffer(history);
        return (buffer, history);
    }

    #endregion

    #region Prefix Match Tests

    [Fact]
    public void GetSuggestion_ExactPrefixMatch_ReturnsSuffixAsSuggestion()
    {
        // Arrange - Acceptance scenario 1
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit -m 'initial'");
        buffer.Document = new Document("git c");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("ommit -m 'initial'", suggestion.Text);
    }

    [Fact]
    public void GetSuggestion_MultipleMatchingEntries_ReturnsMostRecent()
    {
        // Arrange - Acceptance scenario 2
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit -m 'first'");
        history.AppendString("git commit -m 'second'");
        history.AppendString("git commit -m 'third'");
        buffer.Document = new Document("git c");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("ommit -m 'third'", suggestion.Text);
    }

    [Fact]
    public void GetSuggestion_FullMatchAtEndOfHistory_ReturnsEmptySuggestion()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git status");
        buffer.Document = new Document("git status");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("", suggestion.Text);
    }

    #endregion

    #region Empty/Whitespace Input Tests

    [Fact]
    public void GetSuggestion_EmptyInput_ReturnsNull()
    {
        // Arrange - Acceptance scenario 3
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("   ")]
    [InlineData(" \t ")]
    public void GetSuggestion_WhitespaceOnlyInput_ReturnsNull(string whitespace)
    {
        // Arrange - Per Edge Cases
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document(whitespace);
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    #endregion

    #region No Match Tests

    [Fact]
    public void GetSuggestion_NoMatchingHistory_ReturnsNull()
    {
        // Arrange - Acceptance scenario 4
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("npm install");
        history.AppendString("npm run build");
        buffer.Document = new Document("git c");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    [Fact]
    public void GetSuggestion_EmptyHistory_ReturnsNull()
    {
        // Arrange - Per Edge Cases
        var (buffer, _) = CreateTestBuffer();
        buffer.Document = new Document("git c");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    #endregion

    #region Multiline Document Tests

    [Fact]
    public void GetSuggestion_MultilineDocument_UsesOnlyCurrentLine()
    {
        // Arrange - Per Edge Cases
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit -m 'test'");
        buffer.Document = new Document("line1\nline2\ngit c");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("ommit -m 'test'", suggestion.Text);
    }

    [Fact]
    public void GetSuggestion_MultilineDocumentWithEmptyLastLine_ReturnsNull()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("line1\nline2\n");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void GetSuggestion_CaseSensitiveMatch_DoesNotMatchDifferentCase()
    {
        // Arrange - Per FR-012
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("Git c");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    [Fact]
    public void GetSuggestion_CaseSensitiveMatch_MatchesExactCase()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("Git Commit");
        buffer.Document = new Document("Git C");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("ommit", suggestion.Text);
    }

    #endregion

    #region Multi-line History Entry Tests

    [Fact]
    public void GetSuggestion_MultilineHistoryEntry_SearchesAllLinesInReverse()
    {
        // Arrange - Per FR-011
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("first line\nsecond line\nthird line");
        buffer.Document = new Document("second");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal(" line", suggestion.Text);
    }

    [Fact]
    public void GetSuggestion_MultilineHistoryEntry_LastLineMatchedFirst()
    {
        // Arrange - Per FR-011: search lines from last to first
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git status\ngit commit\ngit push");
        buffer.Document = new Document("git");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal(" push", suggestion.Text); // Last line "git push" matches first
    }

    #endregion

    #region Null Parameter Tests

    [Fact]
    public void GetSuggestion_NullBuffer_ThrowsArgumentNullException()
    {
        // Arrange - Per FR-028
        var autoSuggest = new AutoSuggestFromHistory();
        var document = new Document("test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => autoSuggest.GetSuggestion(null!, document));
        Assert.Equal("buffer", exception.ParamName);
    }

    [Fact]
    public void GetSuggestion_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange - Per FR-028
        var (buffer, _) = CreateTestBuffer();
        var autoSuggest = new AutoSuggestFromHistory();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => autoSuggest.GetSuggestion(buffer, null!));
        Assert.Equal("document", exception.ParamName);
    }

    #endregion

    #region Async Method Tests

    [Fact]
    public async Task GetSuggestionAsync_ReturnsWrappedSyncResult()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit -m 'test'");
        buffer.Document = new Document("git c");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = await autoSuggest.GetSuggestionAsync(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("ommit -m 'test'", suggestion.Text);
    }

    [Fact]
    public async Task GetSuggestionAsync_NoMatch_ReturnsNull()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("npm install");
        buffer.Document = new Document("git c");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = await autoSuggest.GetSuggestionAsync(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetSuggestion_PrefixNotAtStart_DoesNotMatch()
    {
        // Arrange - "commit" does NOT match "git commit" (partial line match)
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git commit");
        buffer.Document = new Document("commit");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.Null(suggestion);
    }

    [Fact]
    public void GetSuggestion_SingleCharacterMatch_ReturnsSuggestion()
    {
        // Arrange
        var (buffer, history) = CreateTestBuffer();
        history.AppendString("git status");
        buffer.Document = new Document("g");
        var autoSuggest = new AutoSuggestFromHistory();

        // Act
        var suggestion = autoSuggest.GetSuggestion(buffer, buffer.Document);

        // Assert
        Assert.NotNull(suggestion);
        Assert.Equal("it status", suggestion.Text);
    }

    #endregion
}
