using Stroke.Core;
using Stroke.Filters;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

namespace Stroke.Application;

/// <summary>
/// Filter functions that query the current application state.
/// Used by key bindings, containers, and other components.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's application-aware filter functions from
/// <c>prompt_toolkit.filters.app</c>.
/// </para>
/// </remarks>
public static class AppFilters
{
    private static readonly SimpleCache<EditingMode, IFilter> _editingModeCache = new(2);

    /// <summary>True when the current buffer has a selection.</summary>
    public static IFilter HasSelection { get; } = new Condition(() =>
        AppContext.GetApp().CurrentBuffer.SelectionState is not null);

    /// <summary>True when the current buffer has a non-empty suggestion.</summary>
    public static IFilter HasSuggestion { get; } = new Condition(() =>
    {
        var suggestion = AppContext.GetApp().CurrentBuffer.Suggestion;
        return suggestion is not null && suggestion.Text != "";
    });

    /// <summary>True when the current buffer has active completions.</summary>
    public static IFilter HasCompletions { get; } = new Condition(() =>
    {
        var state = AppContext.GetApp().CurrentBuffer.CompleteState;
        return state is not null && state.Completions.Count > 0;
    });

    /// <summary>True when the user has selected a completion.</summary>
    public static IFilter CompletionIsSelected { get; } = new Condition(() =>
    {
        var state = AppContext.GetApp().CurrentBuffer.CompleteState;
        return state is not null && state.CurrentCompletion is not null;
    });

    /// <summary>True when the current buffer is read-only.</summary>
    public static IFilter IsReadOnly { get; } = new Condition(() =>
        AppContext.GetApp().CurrentBuffer.ReadOnly);

    /// <summary>True when the current buffer is a multiline buffer.</summary>
    public static IFilter IsMultiline { get; } = new Condition(() =>
        AppContext.GetApp().CurrentBuffer.Multiline);

    /// <summary>True when there is a validation error on the current buffer.</summary>
    public static IFilter HasValidationError { get; } = new Condition(() =>
        AppContext.GetApp().CurrentBuffer.ValidationError is not null);

    /// <summary>True when the key processor has an 'arg' (numeric prefix).</summary>
    public static IFilter HasArg { get; } = new Condition(() =>
        AppContext.GetApp().KeyProcessor.Arg is not null);

    /// <summary>True when the application is done (returning/aborting).</summary>
    public static IFilter IsDone { get; } = new Condition(() =>
        AppContext.GetApp().IsDone);

    /// <summary>True when the renderer knows its real terminal height.</summary>
    public static IFilter RendererHeightIsKnown { get; } = new Condition(() =>
        AppContext.GetApp().Renderer.HeightIsKnown);

    /// <summary>True when paste mode is active.</summary>
    public static IFilter InPasteMode { get; } = new Condition(() =>
        AppContext.GetApp().PasteMode.Invoke());

    /// <summary>True when a BufferControl has focus in the current application.</summary>
    public static IFilter BufferHasFocus { get; } = new Condition(() =>
        AppContext.GetApp().Layout.BufferHasFocus);

    /// <summary>
    /// Create a filter that checks if a specific buffer has focus by name.
    /// Each call returns a new instance (no memoization).
    /// </summary>
    /// <param name="bufferName">The buffer name to check.</param>
    /// <returns>A filter that returns true when the named buffer has focus.</returns>
    public static IFilter HasFocus(string bufferName)
    {
        return new Condition(() =>
            AppContext.GetApp().CurrentBuffer.Name == bufferName);
    }

    /// <summary>
    /// Create a filter that checks if a specific Buffer instance has focus.
    /// Each call returns a new instance (no memoization).
    /// </summary>
    /// <param name="buffer">The Buffer instance to check.</param>
    /// <returns>A filter that returns true when the given buffer has focus.</returns>
    public static IFilter HasFocus(Core.Buffer buffer)
    {
        return new Condition(() =>
            ReferenceEquals(AppContext.GetApp().CurrentBuffer, buffer));
    }

    /// <summary>
    /// Create a filter that checks if a specific UIControl has focus.
    /// Each call returns a new instance (no memoization).
    /// </summary>
    /// <param name="control">The UIControl instance to check.</param>
    /// <returns>A filter that returns true when the given control has focus.</returns>
    public static IFilter HasFocus(IUIControl control)
    {
        return new Condition(() =>
            AppContext.GetApp().Layout.CurrentControl == control);
    }

    /// <summary>
    /// Create a filter that checks if a container (or any of its descendant windows) has focus.
    /// For Window instances, checks direct equality. For other containers, walks descendants.
    /// Each call returns a new instance (no memoization).
    /// </summary>
    /// <param name="container">The container to check.</param>
    /// <returns>A filter that returns true when the container or a descendant window has focus.</returns>
    public static IFilter HasFocus(IContainer container)
    {
        return new Condition(() =>
        {
            var currentWindow = AppContext.GetApp().Layout.CurrentWindow;

            if (container is Window w)
                return w == currentWindow;

            foreach (var c in LayoutUtils.Walk(container))
            {
                if (c is Window descendantWindow && descendantWindow == currentWindow)
                    return true;
            }
            return false;
        });
    }

    /// <summary>
    /// Create a cached filter for a given editing mode.
    /// Returns the same instance for the same EditingMode value (memoized).
    /// </summary>
    /// <param name="editingMode">The editing mode to check.</param>
    /// <returns>A filter that returns true when the application is in the given editing mode.</returns>
    public static IFilter InEditingMode(EditingMode editingMode)
    {
        return _editingModeCache.Get(editingMode, () =>
            new Condition(() => AppContext.GetApp().EditingMode == editingMode));
    }
}
