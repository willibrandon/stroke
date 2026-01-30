using Stroke.Filters;
using Stroke.KeyBinding;

namespace Stroke.Application;

/// <summary>
/// Filter functions for Vi editing mode sub-modes.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's Vi-specific filter functions from
/// <c>prompt_toolkit.filters.app</c>.
/// </para>
/// </remarks>
public static class ViFilters
{
    /// <summary>True when Vi editing mode is active.</summary>
    public static IFilter ViMode { get; } = new Condition(() =>
        AppContext.GetApp().EditingMode == EditingMode.Vi);

    /// <summary>
    /// True when Vi navigation key bindings should be active.
    /// Returns false if: not Vi mode, operator pending, digraph wait, or selection active.
    /// Returns true if: InputMode is Navigation, OR temporary navigation mode, OR read-only buffer.
    /// </summary>
    public static IFilter ViNavigationMode { get; } = new Condition(() =>
    {
        var app = AppContext.GetApp();
        if (app.EditingMode != EditingMode.Vi
            || app.ViState.OperatorFunc is not null
            || app.ViState.WaitingForDigraph
            || app.CurrentBuffer.SelectionState is not null)
        {
            return false;
        }
        return app.ViState.InputMode == InputMode.Navigation
            || app.ViState.TemporaryNavigationMode
            || app.CurrentBuffer.ReadOnly;
    });

    /// <summary>
    /// True when Vi insert mode is active.
    /// Guarded by: not Vi mode, operator pending, digraph wait, selection, temp nav, read-only.
    /// </summary>
    public static IFilter ViInsertMode { get; } = new Condition(() =>
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
        return app.ViState.InputMode == InputMode.Insert;
    });

    /// <summary>
    /// True when Vi insert-multiple mode is active (multiple cursors).
    /// Same guard conditions as ViInsertMode.
    /// </summary>
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
    /// True when Vi replace mode is active (overwrite).
    /// Same guard conditions as ViInsertMode.
    /// </summary>
    public static IFilter ViReplaceMode { get; } = new Condition(() =>
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
        return app.ViState.InputMode == InputMode.Replace;
    });

    /// <summary>
    /// True when Vi replace-single mode is active (single char overwrite).
    /// Same guard conditions as ViInsertMode.
    /// </summary>
    public static IFilter ViReplaceSingleMode { get; } = new Condition(() =>
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
        return app.ViState.InputMode == InputMode.ReplaceSingle;
    });

    /// <summary>True when Vi selection (visual) mode is active.</summary>
    public static IFilter ViSelectionMode { get; } = new Condition(() =>
    {
        var app = AppContext.GetApp();
        if (app.EditingMode != EditingMode.Vi)
            return false;
        return app.CurrentBuffer.SelectionState is not null;
    });

    /// <summary>True when a Vi operator is pending (waiting for text object/motion).</summary>
    public static IFilter ViWaitingForTextObjectMode { get; } = new Condition(() =>
    {
        var app = AppContext.GetApp();
        if (app.EditingMode != EditingMode.Vi)
            return false;
        return app.ViState.OperatorFunc is not null;
    });

    /// <summary>True when Vi digraph input is active (waiting for second character).</summary>
    public static IFilter ViDigraphMode { get; } = new Condition(() =>
    {
        var app = AppContext.GetApp();
        if (app.EditingMode != EditingMode.Vi)
            return false;
        return app.ViState.WaitingForDigraph;
    });

    /// <summary>True when Vi is recording a macro.</summary>
    public static IFilter ViRecordingMacro { get; } = new Condition(() =>
    {
        var app = AppContext.GetApp();
        if (app.EditingMode != EditingMode.Vi)
            return false;
        return app.ViState.RecordingRegister is not null;
    });

    /// <summary>True when Vi search direction is reversed ('/' and '?' swapped).</summary>
    public static IFilter ViSearchDirectionReversed { get; } = new Condition(() =>
        AppContext.GetApp().ReverseViSearchDirection.Invoke());
}
