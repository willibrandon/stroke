using Stroke.Core;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application.Bindings;

/// <summary>
/// Search-related key binding handler functions and binding loaders.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.search</c> module
/// (handler functions) and the search-related portions of
/// <c>prompt_toolkit.key_binding.bindings.emacs.load_emacs_search_bindings</c> and
/// <c>prompt_toolkit.key_binding.bindings.vi.load_vi_search_bindings</c> (binding loaders).
/// </para>
/// <para>
/// All handler functions match the <see cref="KeyHandlerCallable"/> delegate signature.
/// All loader methods create and return new <see cref="IKeyBindingsBase"/> instances.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe.
/// </para>
/// </remarks>
public static class SearchBindings
{
    /// <summary>
    /// True if the previously focused buffer has a return handler.
    /// </summary>
    /// <remarks>
    /// Used as a filter when registering <see cref="AcceptSearchAndAcceptInput"/>:
    /// <c>SearchFilters.IsSearching &amp; SearchBindings.PreviousBufferIsReturnable</c>.
    /// </remarks>
    public static readonly IFilter PreviousBufferIsReturnable = new Condition(() =>
    {
        var prevControl = AppContext.GetApp().Layout.SearchTargetBufferControl;
        return prevControl is not null && prevControl.Buffer.IsReturnable;
    });

    #region Handler Functions

    /// <summary>
    /// Abort an incremental search and restore the original line.
    /// Usually bound to Ctrl+G / Ctrl+C.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.IsSearching"/></remarks>
    public static NotImplementedOrNone? AbortSearch(KeyPressEvent @event)
    {
        SearchOperations.StopSearch();
        return null;
    }

    /// <summary>
    /// Accept current search result. When Enter is pressed in isearch, quit isearch mode.
    /// Usually bound to Enter.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.IsSearching"/></remarks>
    public static NotImplementedOrNone? AcceptSearch(KeyPressEvent @event)
    {
        SearchOperations.AcceptSearch();
        return null;
    }

    /// <summary>
    /// Enter reverse incremental search. Usually bound to Ctrl+R.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.ControlIsSearchable"/></remarks>
    public static NotImplementedOrNone? StartReverseIncrementalSearch(KeyPressEvent @event)
    {
        SearchOperations.StartSearch(direction: SearchDirection.Backward);
        return null;
    }

    /// <summary>
    /// Enter forward incremental search. Usually bound to Ctrl+S.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.ControlIsSearchable"/></remarks>
    public static NotImplementedOrNone? StartForwardIncrementalSearch(KeyPressEvent @event)
    {
        SearchOperations.StartSearch(direction: SearchDirection.Forward);
        return null;
    }

    /// <summary>
    /// Apply reverse incremental search, keeping search buffer focused.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.IsSearching"/></remarks>
    public static NotImplementedOrNone? ReverseIncrementalSearch(KeyPressEvent @event)
    {
        SearchOperations.DoIncrementalSearch(SearchDirection.Backward, @event.Arg);
        return null;
    }

    /// <summary>
    /// Apply forward incremental search, keeping search buffer focused.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="SearchFilters.IsSearching"/></remarks>
    public static NotImplementedOrNone? ForwardIncrementalSearch(KeyPressEvent @event)
    {
        SearchOperations.DoIncrementalSearch(SearchDirection.Forward, @event.Arg);
        return null;
    }

    /// <summary>
    /// Accept the search operation first, then accept the input.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>
    /// Filter: <see cref="SearchFilters.IsSearching"/> AND <see cref="PreviousBufferIsReturnable"/>.
    /// Calls AcceptSearch then ValidateAndHandle on the current buffer.
    /// </remarks>
    public static NotImplementedOrNone? AcceptSearchAndAcceptInput(KeyPressEvent @event)
    {
        SearchOperations.AcceptSearch();
        @event.CurrentBuffer?.ValidateAndHandle();
        return null;
    }

    /// <summary>
    /// Jump to the next search match in the current buffer.
    /// Used in read-only Emacs mode with the 'n' key.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="AppFilters.IsReadOnly"/> (in Emacs read-only mode).</remarks>
    public static NotImplementedOrNone? JumpToNextMatch(KeyPressEvent @event)
    {
        var searchState = AppContext.GetApp().CurrentSearchState;
        @event.CurrentBuffer?.ApplySearch(searchState, includeCurrentPosition: false, count: @event.Arg);
        return null;
    }

    /// <summary>
    /// Jump to the previous search match in the current buffer (reverse direction).
    /// Used in read-only Emacs mode with the 'N' key.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success.</returns>
    /// <remarks>Filter: <see cref="AppFilters.IsReadOnly"/> (in Emacs read-only mode).</remarks>
    public static NotImplementedOrNone? JumpToPreviousMatch(KeyPressEvent @event)
    {
        var searchState = ~AppContext.GetApp().CurrentSearchState;
        @event.CurrentBuffer?.ApplySearch(searchState, includeCurrentPosition: false, count: @event.Arg);
        return null;
    }

    #endregion

    #region Binding Loaders

    /// <summary>
    /// Load Emacs-mode search key bindings.
    /// </summary>
    /// <returns>
    /// A <see cref="ConditionalKeyBindings"/> with Emacs search keys,
    /// filtered by <see cref="EmacsFilters.EmacsMode"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Port of Python Prompt Toolkit's
    /// <c>prompt_toolkit.key_binding.bindings.emacs.load_emacs_search_bindings</c>.
    /// </para>
    /// <para>
    /// Registers handlers with their natural filters (from <c>@key_binding</c> metadata)
    /// composed (AND) with any explicit per-registration filter.
    /// </para>
    /// </remarks>
    public static IKeyBindingsBase LoadEmacsSearchBindings()
    {
        var kb = new KeyBindings();
        var isSearching = SearchFilters.IsSearching;
        var controlIsSearchable = SearchFilters.ControlIsSearchable;

        // Start search (natural filter: ControlIsSearchable)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlR)],
            filter: new FilterOrBool(controlIsSearchable))(StartReverseIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlS)],
            filter: new FilterOrBool(controlIsSearchable))(StartForwardIncrementalSearch);

        // Abort search (natural filter: IsSearching)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlC)],
            filter: new FilterOrBool(isSearching))(AbortSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlG)],
            filter: new FilterOrBool(isSearching))(AbortSearch);

        // Incremental search (natural filter: IsSearching)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlR)],
            filter: new FilterOrBool(isSearching))(ReverseIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlS)],
            filter: new FilterOrBool(isSearching))(ForwardIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Up)],
            filter: new FilterOrBool(isSearching))(ReverseIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Down)],
            filter: new FilterOrBool(isSearching))(ForwardIncrementalSearch);

        // Accept search (natural filter: IsSearching)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlM)],
            filter: new FilterOrBool(isSearching))(AcceptSearch);

        // NOTE: We don't bind 'Escape' to 'abort_search'. The reason is that we
        //       want Alt+Enter to accept input directly in incremental search mode.
        //       Instead, we have eager Escape for accept.
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape)],
            filter: new FilterOrBool(isSearching),
            eager: new FilterOrBool(true))(AcceptSearch);

        // Read-only mode: Vi-style / and ? keys for searching.
        // Composed filter: handler's control_is_searchable AND explicit is_read_only & direction.
        var readOnlyNotReversed = controlIsSearchable
            .And(AppFilters.IsReadOnly)
            .And(ViFilters.ViSearchDirectionReversed.Invert());
        var readOnlyReversed = controlIsSearchable
            .And(AppFilters.IsReadOnly)
            .And(ViFilters.ViSearchDirectionReversed);

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('?')],
            filter: new FilterOrBool(readOnlyNotReversed))(StartReverseIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('/')],
            filter: new FilterOrBool(readOnlyNotReversed))(StartForwardIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('?')],
            filter: new FilterOrBool(readOnlyReversed))(StartForwardIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('/')],
            filter: new FilterOrBool(readOnlyReversed))(StartReverseIncrementalSearch);

        // Read-only: Jump to next/previous match (inline handlers, no @key_binding metadata).
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('n')],
            filter: new FilterOrBool(AppFilters.IsReadOnly))(JumpToNextMatch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('N')],
            filter: new FilterOrBool(AppFilters.IsReadOnly))(JumpToPreviousMatch);

        return new ConditionalKeyBindings(kb, EmacsFilters.EmacsMode);
    }

    /// <summary>
    /// Load Vi-mode search key bindings.
    /// </summary>
    /// <returns>
    /// A <see cref="ConditionalKeyBindings"/> with Vi search keys,
    /// filtered by <see cref="ViFilters.ViMode"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Port of Python Prompt Toolkit's
    /// <c>prompt_toolkit.key_binding.bindings.vi.load_vi_search_bindings</c>.
    /// </para>
    /// <para>
    /// Registers handlers with their natural filters (from <c>@key_binding</c> metadata)
    /// composed (AND) with any explicit per-registration filter.
    /// </para>
    /// </remarks>
    public static IKeyBindingsBase LoadViSearchBindings()
    {
        var kb = new KeyBindings();
        var isSearching = SearchFilters.IsSearching;
        var controlIsSearchable = SearchFilters.ControlIsSearchable;
        var viNavOrSel = ViFilters.ViNavigationMode.Or(ViFilters.ViSelectionMode);
        var notReversed = ViFilters.ViSearchDirectionReversed.Invert();

        // Vi-style forward search.
        // Composed: control_is_searchable AND (vi_nav | vi_sel) & ~reversed
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('/')],
            filter: new FilterOrBool(controlIsSearchable.And(viNavOrSel).And(notReversed))
        )(StartForwardIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('?')],
            filter: new FilterOrBool(controlIsSearchable.And(viNavOrSel).And(ViFilters.ViSearchDirectionReversed))
        )(StartForwardIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlS)],
            filter: new FilterOrBool(controlIsSearchable))(StartForwardIncrementalSearch);

        // Vi-style backward search.
        // Composed: control_is_searchable AND (vi_nav | vi_sel) & ~reversed
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('?')],
            filter: new FilterOrBool(controlIsSearchable.And(viNavOrSel).And(notReversed))
        )(StartReverseIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('/')],
            filter: new FilterOrBool(controlIsSearchable.And(viNavOrSel).And(ViFilters.ViSearchDirectionReversed))
        )(StartReverseIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlR)],
            filter: new FilterOrBool(controlIsSearchable))(StartReverseIncrementalSearch);

        // Apply the search (at the / or ? prompt).
        // Composed: is_searching AND is_searching = is_searching
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlM)],
            filter: new FilterOrBool(isSearching))(AcceptSearch);

        // Incremental search while in search mode.
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlR)],
            filter: new FilterOrBool(isSearching))(ReverseIncrementalSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlS)],
            filter: new FilterOrBool(isSearching))(ForwardIncrementalSearch);

        // Abort search.
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlC)],
            filter: new FilterOrBool(isSearching))(AbortSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlG)],
            filter: new FilterOrBool(isSearching))(AbortSearch);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlH)],
            filter: new FilterOrBool(isSearching.And(SearchFilters.SearchBufferIsEmpty)))(AbortSearch);

        // Handle escape. This should accept the search, just like readline.
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape)],
            filter: new FilterOrBool(isSearching))(AcceptSearch);

        return new ConditionalKeyBindings(kb, ViFilters.ViMode);
    }

    #endregion
}
