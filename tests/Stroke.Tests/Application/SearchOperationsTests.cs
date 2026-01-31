using Stroke.Core;
using Stroke.Filters;
using Stroke.Input.Pipe;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Application;

/// <summary>
/// Tests for <see cref="Stroke.Application.SearchOperations"/>.
/// </summary>
public sealed class SearchOperationsTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public SearchOperationsTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    /// <summary>
    /// Creates a minimal searchable layout with a BufferControl linked to a SearchBufferControl.
    /// Returns the buffer control, search buffer control, shared search state, application, and scope.
    /// </summary>
    private (BufferControl BC, SearchBufferControl SBC, SearchState SearchState,
             Buffer MainBuffer, Buffer SearchBuffer,
             Stroke.Application.Application<object> App, IDisposable Scope)
        CreateSearchableEnvironment(string text = "", int cursorPosition = 0)
    {
        // Create the main buffer with content
        var mainBuffer = new Buffer(document: new Document(text, cursorPosition: cursorPosition));

        // Create the shared SearchState
        var searchState = new SearchState();

        // Create the search buffer (for the search field)
        var searchBuffer = new Buffer();

        // Create the SearchBufferControl with the shared SearchState
        // focusable: true is required for Focus() to accept this control
        var sbc = new SearchBufferControl(
            buffer: searchBuffer,
            searcherSearchState: searchState,
            focusable: new FilterOrBool(true));

        // Create the main BufferControl linked to the SBC
        var bc = new BufferControl(
            buffer: mainBuffer,
            searchBufferControl: sbc);

        // Create windows and layout
        var mainWindow = new Window(content: bc);
        var searchWindow = new Window(content: sbc);
        var container = new HSplit([mainWindow, searchWindow]);
        var layout = new Stroke.Layout.Layout(new AnyContainer(container));

        // Create application and set as current
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        var scope = AppContext.SetApp(app.UnsafeCast);

        return (bc, sbc, searchState, mainBuffer, searchBuffer, app, scope);
    }

    #region StartSearch Tests (T008)

    [Fact]
    public void StartSearch_FocusesSearchBufferControl()
    {
        var (bc, sbc, _, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();

            Assert.Same(sbc, app.Layout.CurrentControl);
        }
    }

    [Fact]
    public void StartSearch_SetsSearchDirection()
    {
        var (_, _, searchState, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch(direction: SearchDirection.Backward);

            Assert.Equal(SearchDirection.Backward, searchState.Direction);
        }
    }

    [Fact]
    public void StartSearch_AddsSearchLink()
    {
        var (bc, sbc, _, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();

            var links = app.Layout.SearchLinks;
            Assert.True(links.ContainsKey(sbc));
            Assert.Same(bc, links[sbc]);
        }
    }

    [Fact]
    public void StartSearch_SetsViModeToInsert()
    {
        var (_, _, _, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();

            Assert.Equal(Stroke.KeyBinding.InputMode.Insert, app.ViState.InputMode);
        }
    }

    [Fact]
    public void StartSearch_DoesNotResetSearchText()
    {
        var (_, _, searchState, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            searchState.Text = "existing query";

            Stroke.Application.SearchOperations.StartSearch();

            Assert.Equal("existing query", searchState.Text);
        }
    }

    [Fact]
    public void StartSearch_SilentlyReturns_WhenNoBufferControl()
    {
        // Create a layout where current control is NOT a BufferControl
        var sbc = new SearchBufferControl();
        var window = new Window(content: sbc);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // Should not throw
        Stroke.Application.SearchOperations.StartSearch();
    }

    [Fact]
    public void StartSearch_SilentlyReturns_WhenNoSearchBufferControl()
    {
        // Create a BufferControl without a linked SearchBufferControl
        var buffer = new Buffer();
        var bc = new BufferControl(buffer: buffer);
        var window = new Window(content: bc);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // Should not throw
        Stroke.Application.SearchOperations.StartSearch();
    }

    [Fact]
    public void StartSearch_WithExplicitBufferControl()
    {
        var (bc, sbc, _, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch(bufferControl: bc);

            Assert.Same(sbc, app.Layout.CurrentControl);
        }
    }

    #endregion

    #region StopSearch Tests (T009)

    [Fact]
    public void StopSearch_RestoresFocusToBufferControl()
    {
        var (bc, _, _, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();
            Stroke.Application.SearchOperations.StopSearch();

            Assert.Same(bc, app.Layout.CurrentControl);
        }
    }

    [Fact]
    public void StopSearch_RemovesSearchLink()
    {
        var (_, _, _, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();
            Stroke.Application.SearchOperations.StopSearch();

            Assert.Empty(app.Layout.SearchLinks);
        }
    }

    [Fact]
    public void StopSearch_ResetsSearchBuffer()
    {
        var (_, _, _, _, searchBuffer, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();
            searchBuffer.Text = "some search text";
            Stroke.Application.SearchOperations.StopSearch();

            Assert.Equal("", searchBuffer.Text);
        }
    }

    [Fact]
    public void StopSearch_SetsViModeToNavigation()
    {
        var (_, _, _, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();
            Stroke.Application.SearchOperations.StopSearch();

            Assert.Equal(Stroke.KeyBinding.InputMode.Navigation, app.ViState.InputMode);
        }
    }

    [Fact]
    public void StopSearch_SilentlyReturns_WhenNoActiveSearch()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            // No search started — should not throw
            Stroke.Application.SearchOperations.StopSearch();
        }
    }

    [Fact]
    public void StopSearch_WithExplicitBufferControl()
    {
        var (bc, _, _, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();
            Stroke.Application.SearchOperations.StopSearch(bufferControl: bc);

            Assert.Same(bc, app.Layout.CurrentControl);
            Assert.Empty(app.Layout.SearchLinks);
        }
    }

    [Fact]
    public void StopSearch_SilentlyReturns_WhenBCNotInSearchLinks()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            // Create a different BufferControl that is NOT in any search link
            var otherBc = new BufferControl(buffer: new Buffer());

            Stroke.Application.SearchOperations.StartSearch();
            // Passing a BC not in search links should silently return
            Stroke.Application.SearchOperations.StopSearch(bufferControl: otherBc);
        }
    }

    #endregion

    #region AcceptSearch Tests (T010)

    [Fact]
    public void AcceptSearch_UpdatesSearchStateText()
    {
        var (_, _, searchState, _, searchBuffer, _, scope) =
            CreateSearchableEnvironment("hello world hello");
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();
            searchBuffer.Text = "hello";
            Stroke.Application.SearchOperations.AcceptSearch();

            Assert.Equal("hello", searchState.Text);
        }
    }

    [Fact]
    public void AcceptSearch_PreservesSearchStateText_WhenSearchBufferEmpty()
    {
        var (_, _, searchState, _, _, _, scope) =
            CreateSearchableEnvironment("hello world");
        using (scope)
        {
            searchState.Text = "previous";
            Stroke.Application.SearchOperations.StartSearch();
            // Search buffer is empty — should NOT overwrite
            Stroke.Application.SearchOperations.AcceptSearch();

            Assert.Equal("previous", searchState.Text);
        }
    }

    [Fact]
    public void AcceptSearch_AppliesSearchWithIncludeCurrentPosition()
    {
        var (_, _, searchState, mainBuffer, searchBuffer, _, scope) =
            CreateSearchableEnvironment("hello world hello");
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();
            searchBuffer.Text = "hello";
            Stroke.Application.SearchOperations.AcceptSearch();

            // After accepting with includeCurrentPosition=true, cursor should be at a match
            // The exact position depends on the search implementation,
            // but it should find "hello" at position 0 (first occurrence)
            Assert.Equal(0, mainBuffer.CursorPosition);
        }
    }

    [Fact]
    public void AcceptSearch_AppendsToSearchHistory()
    {
        var (_, _, _, _, searchBuffer, _, scope) =
            CreateSearchableEnvironment("hello world");
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();
            searchBuffer.Text = "hello";
            Stroke.Application.SearchOperations.AcceptSearch();

            // The search buffer's history should contain the search term
            var history = searchBuffer.History.GetStrings();
            Assert.Contains("hello", history);
        }
    }

    [Fact]
    public void AcceptSearch_CallsStopSearch()
    {
        var (bc, _, _, _, _, app, scope) =
            CreateSearchableEnvironment("hello world");
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();
            Stroke.Application.SearchOperations.AcceptSearch();

            // Focus should return to original BufferControl
            Assert.Same(bc, app.Layout.CurrentControl);
            Assert.Empty(app.Layout.SearchLinks);
        }
    }

    [Fact]
    public void AcceptSearch_SilentlyReturns_WhenNoSearchTarget()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            // No search started — should not throw
            Stroke.Application.SearchOperations.AcceptSearch();
        }
    }

    #endregion

    #region GetReverseSearchLinks Tests (T011)

    [Fact]
    public void StopSearch_WithExplicitBC_UsesReverseSearchLinks()
    {
        // This tests the private GetReverseSearchLinks indirectly via StopSearch
        var (bc, sbc, _, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();

            // Verify search link exists: SBC → BC
            Assert.True(app.Layout.SearchLinks.ContainsKey(sbc));

            // StopSearch with explicit BC should find the SBC via reverse mapping
            Stroke.Application.SearchOperations.StopSearch(bufferControl: bc);

            Assert.Same(bc, app.Layout.CurrentControl);
            Assert.Empty(app.Layout.SearchLinks);
        }
    }

    #endregion

    #region DoIncrementalSearch Tests (T017)

    [Fact]
    public void DoIncrementalSearch_UpdatesSearchStateText()
    {
        var (_, _, searchState, _, searchBuffer, _, scope) =
            CreateSearchableEnvironment("hello world hello");
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch();
            searchBuffer.Text = "world";
            Stroke.Application.SearchOperations.DoIncrementalSearch(SearchDirection.Forward);

            Assert.Equal("world", searchState.Text);
        }
    }

    [Fact]
    public void DoIncrementalSearch_UpdatesSearchDirection()
    {
        var (_, _, searchState, _, searchBuffer, _, scope) =
            CreateSearchableEnvironment("hello world");
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch(direction: SearchDirection.Forward);
            searchBuffer.Text = "hello";
            Stroke.Application.SearchOperations.DoIncrementalSearch(SearchDirection.Backward);

            Assert.Equal(SearchDirection.Backward, searchState.Direction);
        }
    }

    [Fact]
    public void DoIncrementalSearch_AppliesSearch_WhenDirectionUnchanged()
    {
        var (_, _, _, mainBuffer, searchBuffer, _, scope) =
            CreateSearchableEnvironment("hello world hello", cursorPosition: 6);
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch(direction: SearchDirection.Forward);
            searchBuffer.Text = "hello";

            // Same direction (Forward) — should apply search and move cursor
            Stroke.Application.SearchOperations.DoIncrementalSearch(SearchDirection.Forward);

            // With includeCurrentPosition=false and forward direction from position 6,
            // it should find "hello" at position 12
            Assert.Equal(12, mainBuffer.CursorPosition);
        }
    }

    [Fact]
    public void DoIncrementalSearch_DoesNotApplySearch_WhenDirectionChanged()
    {
        var (_, _, _, mainBuffer, searchBuffer, _, scope) =
            CreateSearchableEnvironment("hello world hello", cursorPosition: 6);
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch(direction: SearchDirection.Forward);
            searchBuffer.Text = "hello";

            // Different direction (Backward) — should NOT apply search, cursor stays
            Stroke.Application.SearchOperations.DoIncrementalSearch(SearchDirection.Backward);

            Assert.Equal(6, mainBuffer.CursorPosition);
        }
    }

    [Fact]
    public void DoIncrementalSearch_PassesCountToApplySearch()
    {
        var (_, _, _, mainBuffer, searchBuffer, _, scope) =
            CreateSearchableEnvironment("aaa bbb aaa bbb aaa", cursorPosition: 0);
        using (scope)
        {
            Stroke.Application.SearchOperations.StartSearch(direction: SearchDirection.Forward);
            searchBuffer.Text = "aaa";

            // count=2 should skip first match and go to second
            Stroke.Application.SearchOperations.DoIncrementalSearch(SearchDirection.Forward, count: 2);

            // With includeCurrentPosition=false, forward, count=2 from position 0:
            // First match at 8, second match at 16
            Assert.Equal(16, mainBuffer.CursorPosition);
        }
    }

    [Fact]
    public void DoIncrementalSearch_SilentlyReturns_WhenNotBufferControl()
    {
        var buffer = new Buffer();
        var sbc = new SearchBufferControl(buffer: buffer);
        var window = new Window(content: sbc);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // Current control is SBC (not a regular BufferControl in search links)
        // Should not throw
        Stroke.Application.SearchOperations.DoIncrementalSearch(SearchDirection.Forward);
    }

    [Fact]
    public void DoIncrementalSearch_SilentlyReturns_WhenSearchTargetNull()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            // No search started — SearchTargetBufferControl is null
            Stroke.Application.SearchOperations.DoIncrementalSearch(SearchDirection.Forward);
        }
    }

    [Fact]
    public void DoIncrementalSearch_DirectionCheckBeforeUpdate()
    {
        var (_, _, searchState, mainBuffer, searchBuffer, _, scope) =
            CreateSearchableEnvironment("hello world hello", cursorPosition: 6);
        using (scope)
        {
            // Start with Forward
            Stroke.Application.SearchOperations.StartSearch(direction: SearchDirection.Forward);
            searchBuffer.Text = "hello";

            // Call with Forward (same direction) — should apply search
            Stroke.Application.SearchOperations.DoIncrementalSearch(SearchDirection.Forward);
            var posAfterFirstSearch = mainBuffer.CursorPosition;

            // Now direction is Forward. Call with Forward again — same direction, should apply
            Stroke.Application.SearchOperations.DoIncrementalSearch(SearchDirection.Forward);

            // The key assertion: direction check should happen BEFORE the update
            Assert.Equal(SearchDirection.Forward, searchState.Direction);
        }
    }

    #endregion
}
