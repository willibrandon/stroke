using Stroke.Filters;
using Stroke.KeyBinding;

namespace Stroke.Application;

/// <summary>
/// Filter functions for Emacs editing mode.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's Emacs-specific filter functions from
/// <c>prompt_toolkit.filters.app</c>.
/// </para>
/// </remarks>
public static class EmacsFilters
{
    /// <summary>True when Emacs editing mode is active.</summary>
    public static IFilter EmacsMode { get; } = new Condition(() =>
        AppContext.GetApp().EditingMode == EditingMode.Emacs);

    /// <summary>
    /// True when Emacs insert mode is active (Emacs mode, no selection, not read-only).
    /// </summary>
    public static IFilter EmacsInsertMode { get; } = new Condition(() =>
    {
        var app = AppContext.GetApp();
        return app.EditingMode == EditingMode.Emacs
            && app.CurrentBuffer.SelectionState is null
            && !app.CurrentBuffer.ReadOnly;
    });

    /// <summary>
    /// True when Emacs selection mode is active (Emacs mode with selection).
    /// </summary>
    public static IFilter EmacsSelectionMode { get; } = new Condition(() =>
    {
        var app = AppContext.GetApp();
        return app.EditingMode == EditingMode.Emacs
            && app.CurrentBuffer.SelectionState is not null;
    });
}
