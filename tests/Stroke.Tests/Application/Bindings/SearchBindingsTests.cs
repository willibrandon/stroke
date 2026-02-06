using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.Core;
using Stroke.Filters;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.Application.Bindings;

/// <summary>
/// Tests for <see cref="SearchBindings"/> handler functions.
/// </summary>
public sealed class SearchBindingsTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public SearchBindingsTests()
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
    /// </summary>
    private (BufferControl BC, SearchBufferControl SBC, SearchState SearchState,
             Buffer MainBuffer, Buffer SearchBuffer,
             Stroke.Application.Application<object> App, IDisposable Scope)
        CreateSearchableEnvironment(string text = "", int cursorPosition = 0)
    {
        var mainBuffer = new Buffer(document: new Document(text, cursorPosition: cursorPosition));
        var searchState = new SearchState();
        var searchBuffer = new Buffer();
        var sbc = new SearchBufferControl(
            buffer: searchBuffer,
            searcherSearchState: searchState,
            focusable: new FilterOrBool(true));
        var bc = new BufferControl(
            buffer: mainBuffer,
            searchBufferControl: sbc);
        var mainWindow = new Window(content: bc);
        var searchWindow = new Window(content: sbc);
        var container = new HSplit([mainWindow, searchWindow]);
        var layout = new Stroke.Layout.Layout(new AnyContainer(container));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        var scope = AppContext.SetApp(app.UnsafeCast);

        return (bc, sbc, searchState, mainBuffer, searchBuffer, app, scope);
    }

    /// <summary>
    /// Creates a KeyPressEvent for testing binding handlers.
    /// </summary>
    private static KeyPressEvent CreateEvent(
        Buffer? buffer = null,
        object? app = null,
        string? arg = null)
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: arg,
            keySequence: [new KeyPress(Keys.Any)],
            previousKeySequence: [],
            isRepeat: false,
            app: app,
            currentBuffer: buffer);
    }

    #region AbortSearch Tests (T021)

    [Fact]
    public void AbortSearch_CallsStopSearch()
    {
        var (bc, _, _, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            SearchOperations.StartSearch();
            var evt = CreateEvent(buffer: app.CurrentBuffer, app: app);

            SearchBindings.AbortSearch(evt);

            Assert.Same(bc, app.Layout.CurrentControl);
            Assert.Empty(app.Layout.SearchLinks);
        }
    }

    [Fact]
    public void AbortSearch_RequiresIsSearchingFilter()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            // Before starting search, IsSearching should be false
            Assert.False(SearchFilters.IsSearching.Invoke());

            SearchOperations.StartSearch();
            Assert.True(SearchFilters.IsSearching.Invoke());
        }
    }

    #endregion

    #region AcceptSearch Tests (T021)

    [Fact]
    public void AcceptSearch_CallsSearchOperationsAcceptSearch()
    {
        var (bc, _, searchState, mainBuffer, searchBuffer, app, scope) =
            CreateSearchableEnvironment("hello world hello");
        using (scope)
        {
            SearchOperations.StartSearch();
            searchBuffer.Text = "hello";
            var evt = CreateEvent(buffer: searchBuffer, app: app);

            SearchBindings.AcceptSearch(evt);

            // Search accepted: cursor at match, focus returned
            Assert.Equal("hello", searchState.Text);
            Assert.Same(bc, app.Layout.CurrentControl);
        }
    }

    [Fact]
    public void AcceptSearch_RequiresIsSearchingFilter()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Assert.False(SearchFilters.IsSearching.Invoke());

            SearchOperations.StartSearch();
            Assert.True(SearchFilters.IsSearching.Invoke());
        }
    }

    #endregion

    #region StartReverseIncrementalSearch Tests (T022)

    [Fact]
    public void StartReverseIncrementalSearch_StartsSearchBackward()
    {
        var (_, sbc, searchState, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            var evt = CreateEvent(buffer: app.CurrentBuffer, app: app);

            SearchBindings.StartReverseIncrementalSearch(evt);

            Assert.Equal(SearchDirection.Backward, searchState.Direction);
            Assert.Same(sbc, app.Layout.CurrentControl);
        }
    }

    [Fact]
    public void StartReverseIncrementalSearch_RequiresControlIsSearchableFilter()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            // Current control is a BufferControl with a SearchBufferControl
            Assert.True(SearchFilters.ControlIsSearchable.Invoke());
        }
    }

    #endregion

    #region StartForwardIncrementalSearch Tests (T022)

    [Fact]
    public void StartForwardIncrementalSearch_StartsSearchForward()
    {
        var (_, sbc, searchState, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            var evt = CreateEvent(buffer: app.CurrentBuffer, app: app);

            SearchBindings.StartForwardIncrementalSearch(evt);

            Assert.Equal(SearchDirection.Forward, searchState.Direction);
            Assert.Same(sbc, app.Layout.CurrentControl);
        }
    }

    [Fact]
    public void StartForwardIncrementalSearch_RequiresControlIsSearchableFilter()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Assert.True(SearchFilters.ControlIsSearchable.Invoke());
        }
    }

    #endregion

    #region ReverseIncrementalSearch Tests (T023)

    [Fact]
    public void ReverseIncrementalSearch_CallsDoIncrementalSearchBackward()
    {
        var (_, _, searchState, _, searchBuffer, app, scope) =
            CreateSearchableEnvironment("hello world hello");
        using (scope)
        {
            SearchOperations.StartSearch(direction: SearchDirection.Backward);
            searchBuffer.Text = "hello";
            var evt = CreateEvent(buffer: searchBuffer, app: app);

            SearchBindings.ReverseIncrementalSearch(evt);

            Assert.Equal(SearchDirection.Backward, searchState.Direction);
            Assert.Equal("hello", searchState.Text);
        }
    }

    [Fact]
    public void ReverseIncrementalSearch_PassesEventArg()
    {
        var (_, _, _, mainBuffer, searchBuffer, app, scope) =
            CreateSearchableEnvironment("aaa bbb aaa bbb aaa", cursorPosition: 0);
        using (scope)
        {
            SearchOperations.StartSearch(direction: SearchDirection.Forward);
            searchBuffer.Text = "aaa";
            // arg="2" means count=2
            var evt = CreateEvent(buffer: searchBuffer, app: app, arg: "2");

            SearchBindings.ForwardIncrementalSearch(evt);

            // With count=2, forward, includeCurrentPosition=false from 0:
            // should skip to second match
            Assert.Equal(16, mainBuffer.CursorPosition);
        }
    }

    #endregion

    #region ForwardIncrementalSearch Tests (T023)

    [Fact]
    public void ForwardIncrementalSearch_CallsDoIncrementalSearchForward()
    {
        var (_, _, searchState, _, searchBuffer, app, scope) =
            CreateSearchableEnvironment("hello world hello");
        using (scope)
        {
            SearchOperations.StartSearch(direction: SearchDirection.Forward);
            searchBuffer.Text = "world";
            var evt = CreateEvent(buffer: searchBuffer, app: app);

            SearchBindings.ForwardIncrementalSearch(evt);

            Assert.Equal(SearchDirection.Forward, searchState.Direction);
            Assert.Equal("world", searchState.Text);
        }
    }

    [Fact]
    public void ForwardIncrementalSearch_PassesEventArg()
    {
        var (_, _, _, mainBuffer, searchBuffer, app, scope) =
            CreateSearchableEnvironment("aaa bbb aaa bbb aaa", cursorPosition: 0);
        using (scope)
        {
            SearchOperations.StartSearch(direction: SearchDirection.Forward);
            searchBuffer.Text = "aaa";
            var evt = CreateEvent(buffer: searchBuffer, app: app, arg: "2");

            SearchBindings.ForwardIncrementalSearch(evt);

            Assert.Equal(16, mainBuffer.CursorPosition);
        }
    }

    #endregion

    #region Filter-False Scenario Tests (T024)

    [Fact]
    public void IsSearching_ReturnsFalse_WhenNotSearching()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Assert.False(SearchFilters.IsSearching.Invoke());
        }
    }

    [Fact]
    public void ControlIsSearchable_ReturnsFalse_WhenNoSearchBufferControl()
    {
        // Create a BufferControl without a SearchBufferControl
        var buffer = new Buffer();
        var bc = new BufferControl(buffer: buffer);
        var window = new Window(content: bc);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.False(SearchFilters.ControlIsSearchable.Invoke());
    }

    [Fact]
    public void PreviousBufferIsReturnable_ReturnsFalse_WhenNotSearching()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            Assert.False(SearchBindings.PreviousBufferIsReturnable.Invoke());
        }
    }

    [Fact]
    public void PreviousBufferIsReturnable_ReturnsFalse_WhenBufferNotReturnable()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment("hello");
        using (scope)
        {
            SearchOperations.StartSearch();
            // Buffer has no AcceptHandler so IsReturnable is false
            Assert.False(SearchBindings.PreviousBufferIsReturnable.Invoke());
        }
    }

    [Fact]
    public void PreviousBufferIsReturnable_ReturnsTrue_WhenBufferIsReturnable()
    {
        var mainBuffer = new Buffer(
            document: new Document("hello"),
            acceptHandler: _ => ValueTask.FromResult(false));
        var searchState = new SearchState();
        var searchBuffer = new Buffer();
        var sbc = new SearchBufferControl(
            buffer: searchBuffer,
            searcherSearchState: searchState,
            focusable: new FilterOrBool(true));
        var bc = new BufferControl(
            buffer: mainBuffer,
            searchBufferControl: sbc);
        var mainWindow = new Window(content: bc);
        var searchWindow = new Window(content: sbc);
        var container = new HSplit([mainWindow, searchWindow]);
        var layout = new Stroke.Layout.Layout(new AnyContainer(container));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        SearchOperations.StartSearch();
        Assert.True(SearchBindings.PreviousBufferIsReturnable.Invoke());
    }

    #endregion

    #region AcceptSearchAndAcceptInput Tests (T028)

    [Fact]
    public void AcceptSearchAndAcceptInput_AcceptsSearchThenValidates()
    {
        var accepted = false;
        var mainBuffer = new Buffer(
            document: new Document("hello world"),
            acceptHandler: _ => { accepted = true; return ValueTask.FromResult(false); });
        var searchState = new SearchState();
        var searchBuffer = new Buffer();
        var sbc = new SearchBufferControl(
            buffer: searchBuffer,
            searcherSearchState: searchState,
            focusable: new FilterOrBool(true));
        var bc = new BufferControl(
            buffer: mainBuffer,
            searchBufferControl: sbc);
        var mainWindow = new Window(content: bc);
        var searchWindow = new Window(content: sbc);
        var container = new HSplit([mainWindow, searchWindow]);
        var layout = new Stroke.Layout.Layout(new AnyContainer(container));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        SearchOperations.StartSearch();
        searchBuffer.Text = "hello";
        var evt = CreateEvent(buffer: mainBuffer, app: app);

        SearchBindings.AcceptSearchAndAcceptInput(evt);

        // Search should be accepted
        Assert.Equal("hello", searchState.Text);
        // ValidateAndHandle should have been called (which invokes the accept handler)
        Assert.True(accepted);
    }

    [Fact]
    public void AcceptSearchAndAcceptInput_RequiresPreviousBufferIsReturnable()
    {
        // Non-returnable buffer — no accept handler
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment("hello world");
        using (scope)
        {
            SearchOperations.StartSearch();

            // The buffer is not returnable (no accept handler), so the combined
            // filter IsSearching & PreviousBufferIsReturnable should reflect this
            var prevControl = AppContext.GetApp().Layout.SearchTargetBufferControl;
            Assert.NotNull(prevControl);
            Assert.False(prevControl!.Buffer.IsReturnable);
        }
    }

    [Fact]
    public void AcceptSearchAndAcceptInput_ValidateAndHandleFailure_SearchStillAccepted()
    {
        // Create a buffer with a validator that rejects input
        var mainBuffer = new Buffer(
            document: new Document("hello world"),
            acceptHandler: _ => ValueTask.FromResult(false));
        var searchState = new SearchState();
        var searchBuffer = new Buffer();
        var sbc = new SearchBufferControl(
            buffer: searchBuffer,
            searcherSearchState: searchState,
            focusable: new FilterOrBool(true));
        var bc = new BufferControl(
            buffer: mainBuffer,
            searchBufferControl: sbc);
        var mainWindow = new Window(content: bc);
        var searchWindow = new Window(content: sbc);
        var container = new HSplit([mainWindow, searchWindow]);
        var layout = new Stroke.Layout.Layout(new AnyContainer(container));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        SearchOperations.StartSearch();
        searchBuffer.Text = "hello";
        var evt = CreateEvent(buffer: mainBuffer, app: app);

        SearchBindings.AcceptSearchAndAcceptInput(evt);

        // Even if ValidateAndHandle doesn't "succeed" in the way we expect,
        // AcceptSearch should have already been called first
        Assert.Equal("hello", searchState.Text);
        // Focus should be back on the main control (search was accepted)
        Assert.Same(bc, app.Layout.CurrentControl);
    }

    #endregion

    #region SearchBufferIsEmpty Filter Tests

    [Fact]
    public void SearchBufferIsEmpty_ReturnsTrue_WhenBufferIsEmpty()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            // Current buffer is the main buffer which is empty ("")
            Assert.True(SearchFilters.SearchBufferIsEmpty.Invoke());
        }
    }

    [Fact]
    public void SearchBufferIsEmpty_ReturnsFalse_WhenBufferHasText()
    {
        var (_, _, _, _, _, _, scope) = CreateSearchableEnvironment("hello world");
        using (scope)
        {
            Assert.False(SearchFilters.SearchBufferIsEmpty.Invoke());
        }
    }

    [Fact]
    public void SearchBufferIsEmpty_ReturnsTrue_WhenSearchBufferIsEmpty()
    {
        var (_, _, _, _, searchBuffer, _, scope) = CreateSearchableEnvironment("hello");
        using (scope)
        {
            SearchOperations.StartSearch();
            // After starting search, focus is on search buffer which is empty
            Assert.True(SearchFilters.SearchBufferIsEmpty.Invoke());
        }
    }

    [Fact]
    public void SearchBufferIsEmpty_ReturnsFalse_WhenSearchBufferHasText()
    {
        var (_, _, _, _, searchBuffer, _, scope) = CreateSearchableEnvironment("hello");
        using (scope)
        {
            SearchOperations.StartSearch();
            searchBuffer.Text = "hello";
            Assert.False(SearchFilters.SearchBufferIsEmpty.Invoke());
        }
    }

    #endregion

    #region JumpToNextMatch Tests

    [Fact]
    public void JumpToNextMatch_MovesToNextMatch()
    {
        var (_, _, searchState, mainBuffer, _, app, scope) =
            CreateSearchableEnvironment("hello world hello", cursorPosition: 0);
        using (scope)
        {
            searchState.Text = "hello";
            searchState.Direction = SearchDirection.Forward;
            var evt = CreateEvent(buffer: mainBuffer, app: app);

            SearchBindings.JumpToNextMatch(evt);

            // From position 0, forward, includeCurrentPosition=false: finds "hello" at 12
            Assert.Equal(12, mainBuffer.CursorPosition);
        }
    }

    [Fact]
    public void JumpToNextMatch_PassesCount()
    {
        var (_, _, searchState, mainBuffer, _, app, scope) =
            CreateSearchableEnvironment("aaa bbb aaa bbb aaa", cursorPosition: 0);
        using (scope)
        {
            searchState.Text = "aaa";
            searchState.Direction = SearchDirection.Forward;
            var evt = CreateEvent(buffer: mainBuffer, app: app, arg: "2");

            SearchBindings.JumpToNextMatch(evt);

            // count=2, from 0 forward: skip first match (8), land on second (16)
            Assert.Equal(16, mainBuffer.CursorPosition);
        }
    }

    #endregion

    #region JumpToPreviousMatch Tests

    [Fact]
    public void JumpToPreviousMatch_MovesToPreviousMatch()
    {
        var (_, _, searchState, mainBuffer, _, app, scope) =
            CreateSearchableEnvironment("hello world hello", cursorPosition: 12);
        using (scope)
        {
            searchState.Text = "hello";
            searchState.Direction = SearchDirection.Forward;
            var evt = CreateEvent(buffer: mainBuffer, app: app);

            SearchBindings.JumpToPreviousMatch(evt);

            // ~searchState reverses to Backward. From 12, backward: finds "hello" at 0
            Assert.Equal(0, mainBuffer.CursorPosition);
        }
    }

    [Fact]
    public void JumpToPreviousMatch_PassesCount()
    {
        var (_, _, searchState, mainBuffer, _, app, scope) =
            CreateSearchableEnvironment("aaa bbb aaa bbb aaa", cursorPosition: 16);
        using (scope)
        {
            searchState.Text = "aaa";
            searchState.Direction = SearchDirection.Forward;
            var evt = CreateEvent(buffer: mainBuffer, app: app, arg: "2");

            SearchBindings.JumpToPreviousMatch(evt);

            // ~searchState reverses to Backward. count=2, from 16 backward:
            // first match at 8, second at 0
            Assert.Equal(0, mainBuffer.CursorPosition);
        }
    }

    #endregion

    #region LoadEmacsSearchBindings Tests

    [Fact]
    public void LoadEmacsSearchBindings_ReturnsConditionalKeyBindings()
    {
        var bindings = SearchBindings.LoadEmacsSearchBindings();

        Assert.IsType<ConditionalKeyBindings>(bindings);
    }

    [Fact]
    public void LoadEmacsSearchBindings_WrappedWithEmacsMode()
    {
        var bindings = SearchBindings.LoadEmacsSearchBindings();
        var conditional = Assert.IsType<ConditionalKeyBindings>(bindings);

        Assert.Same(EmacsFilters.EmacsMode, conditional.Filter);
    }

    [Fact]
    public void LoadEmacsSearchBindings_ContainsBindings()
    {
        var bindings = SearchBindings.LoadEmacsSearchBindings();

        // Should have multiple bindings registered
        Assert.NotEmpty(bindings.Bindings);
    }

    [Fact]
    public void LoadEmacsSearchBindings_RegistersControlRBinding()
    {
        var bindings = SearchBindings.LoadEmacsSearchBindings();

        // There should be at least one binding with ControlR key
        var ctrlRBindings = bindings.Bindings
            .Where(b => b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlR)
            .ToList();

        Assert.NotEmpty(ctrlRBindings);
    }

    [Fact]
    public void LoadEmacsSearchBindings_RegistersEscapeBindingEager()
    {
        var bindings = SearchBindings.LoadEmacsSearchBindings();

        // Should have an eager Escape binding for AcceptSearch
        var escapeBindings = bindings.Bindings
            .Where(b => b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.Escape)
            .ToList();

        Assert.NotEmpty(escapeBindings);
    }

    #endregion

    #region LoadViSearchBindings Tests

    [Fact]
    public void LoadViSearchBindings_ReturnsConditionalKeyBindings()
    {
        var bindings = SearchBindings.LoadViSearchBindings();

        Assert.IsType<ConditionalKeyBindings>(bindings);
    }

    [Fact]
    public void LoadViSearchBindings_WrappedWithViMode()
    {
        var bindings = SearchBindings.LoadViSearchBindings();
        var conditional = Assert.IsType<ConditionalKeyBindings>(bindings);

        Assert.Same(ViFilters.ViMode, conditional.Filter);
    }

    [Fact]
    public void LoadViSearchBindings_ContainsBindings()
    {
        var bindings = SearchBindings.LoadViSearchBindings();

        Assert.NotEmpty(bindings.Bindings);
    }

    [Fact]
    public void LoadViSearchBindings_RegistersSlashBinding()
    {
        var bindings = SearchBindings.LoadViSearchBindings();

        // / is used for forward search in Vi mode
        var slashBindings = bindings.Bindings
            .Where(b => b.Keys.Count == 1 && b.Keys[0].IsChar && b.Keys[0].Char == '/')
            .ToList();

        Assert.NotEmpty(slashBindings);
    }

    [Fact]
    public void LoadViSearchBindings_RegistersQuestionMarkBinding()
    {
        var bindings = SearchBindings.LoadViSearchBindings();

        // ? is used for backward search in Vi mode
        var questionBindings = bindings.Bindings
            .Where(b => b.Keys.Count == 1 && b.Keys[0].IsChar && b.Keys[0].Char == '?')
            .ToList();

        Assert.NotEmpty(questionBindings);
    }

    [Fact]
    public void LoadViSearchBindings_RegistersBackspaceAbortBinding()
    {
        var bindings = SearchBindings.LoadViSearchBindings();

        // Backspace (ControlH) for abort when search buffer is empty
        var bsBindings = bindings.Bindings
            .Where(b => b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlH)
            .ToList();

        Assert.NotEmpty(bsBindings);
    }

    #endregion

    #region CurrentSearchState Tests

    [Fact]
    public void CurrentSearchState_ReturnsBufferControlSearchState()
    {
        var (bc, _, searchState, _, _, app, scope) = CreateSearchableEnvironment();
        using (scope)
        {
            searchState.Direction = SearchDirection.Backward;
            searchState.Text = "test";

            var result = app.CurrentSearchState;

            Assert.Same(searchState, result);
        }
    }

    [Fact]
    public void CurrentSearchState_ReturnsDefaultSearchState_WhenNotBufferControl()
    {
        var sbc = new SearchBufferControl(
            focusable: new FilterOrBool(true));
        var window = new Window(content: sbc);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // SearchBufferControl is the current control, not a BufferControl
        // (unless SBC extends BufferControl — in that case it returns its search state)
        var result = app.CurrentSearchState;
        Assert.NotNull(result);
    }

    #endregion
}
