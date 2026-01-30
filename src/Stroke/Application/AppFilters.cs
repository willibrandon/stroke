using Stroke.Filters;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;

namespace Stroke.Application;

/// <summary>
/// Filter functions that query the current application state.
/// Used by key bindings, containers, and other components.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's application-aware filter functions from
/// <c>prompt_toolkit.key_binding.key_bindings</c> and
/// <c>prompt_toolkit.filters.app</c>.
/// </para>
/// </remarks>
public static class AppFilters
{
    /// <summary>True when a BufferControl has focus in the current application.</summary>
    public static IFilter BufferHasFocus { get; } = new Condition(() =>
        AppContext.GetApp().Layout.CurrentControl is BufferControl);

    /// <summary>True when the current application is in Vi navigation mode.</summary>
    public static IFilter ViNavigationMode { get; } = new Condition(() =>
        AppContext.GetApp().ViState.InputMode == InputMode.Navigation);

    /// <summary>True when the current application is in Vi insert mode.</summary>
    public static IFilter ViInsertMode { get; } = new Condition(() =>
        AppContext.GetApp().ViState.InputMode == InputMode.Insert);

    /// <summary>True when the current application uses Emacs editing mode.</summary>
    public static IFilter EmacsMode { get; } = new Condition(() =>
        AppContext.GetApp().EditingMode == EditingMode.Emacs);

    /// <summary>True when the current application uses Vi editing mode.</summary>
    public static IFilter ViMode { get; } = new Condition(() =>
        AppContext.GetApp().EditingMode == EditingMode.Vi);

    /// <summary>True when the current application is searching.</summary>
    public static IFilter IsSearching { get; } = new Condition(() =>
        AppContext.GetApp().Layout.IsSearching);

    /// <summary>True when the current application has a selection active.</summary>
    public static IFilter HasSelection { get; } = new Condition(() =>
        AppContext.GetApp().CurrentBuffer.SelectionState is not null);

    /// <summary>True when the current buffer has a completer assigned.</summary>
    public static IFilter HasCompletions { get; } = new Condition(() =>
        AppContext.GetApp().CurrentBuffer.Completer is not null);

    /// <summary>True when a completion menu is currently showing.</summary>
    public static IFilter CompletionIsSelected { get; } = new Condition(() =>
    {
        var buffer = AppContext.GetApp().CurrentBuffer;
        return buffer.CompleteState is not null;
    });

    /// <summary>True when there is a validation error on the current buffer.</summary>
    public static IFilter HasValidationError { get; } = new Condition(() =>
        AppContext.GetApp().CurrentBuffer.ValidationError is not null);

    /// <summary>True when the current buffer has an argument typed (e.g., Vi numeric prefix).</summary>
    public static IFilter HasArg { get; } = new Condition(() =>
        AppContext.GetApp().KeyProcessor.Arg is not null);

    /// <summary>True when the current buffer is read-only.</summary>
    public static IFilter IsReadOnly { get; } = new Condition(() =>
        AppContext.GetApp().CurrentBuffer.ReadOnly);

    /// <summary>True when the current buffer is a multiline buffer.</summary>
    public static IFilter IsMultiline { get; } = new Condition(() =>
        AppContext.GetApp().CurrentBuffer.Multiline);

    /// <summary>True when the current application has an active buffer focused.</summary>
    public static IFilter HasFocus { get; } = new Condition(() =>
        AppContext.GetApp().Layout.CurrentBuffer is not null);

    /// <summary>
    /// True when the current application is in Vi insert-multiple mode.
    /// Checks: Vi editing mode, no pending operator, no digraph wait,
    /// no selection, no temporary navigation, not read-only,
    /// and InputMode is InsertMultiple.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>vi_insert_multiple_mode</c> filter
    /// from <c>prompt_toolkit.filters.app</c>.
    /// </remarks>
    public static IFilter ViInsertMultipleMode { get; } = new Condition(() =>
    {
        var app = AppContext.GetApp();
        if (app.EditingMode != EditingMode.Vi
            || app.ViState.OperatorFunc is not null
            || app.ViState.WaitingForDigraph
            || app.CurrentBuffer.SelectionState is not null
            || app.ViState.TemporaryNavigationMode
            || app.CurrentBuffer.ReadOnly)
        {
            return false;
        }
        return app.ViState.InputMode == InputMode.InsertMultiple;
    });

    /// <summary>
    /// Create a filter that checks if a specific buffer has focus.
    /// </summary>
    /// <param name="bufferName">The buffer name to check.</param>
    /// <returns>A filter that returns true when the named buffer has focus.</returns>
    public static IFilter CreateHasFocus(string bufferName)
    {
        return new Condition(() =>
            AppContext.GetApp().Layout.CurrentBuffer?.Name == bufferName);
    }
}
