using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application.Bindings;

/// <summary>
/// Key binding loaders for opening the current buffer content in an external editor.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.open_in_editor</c> module.
/// Provides 3 binding loaders: Emacs (Ctrl-X Ctrl-E), Vi ('v' in navigation mode), and combined.
/// </para>
/// <para>
/// All loaders delegate to the <c>edit-and-execute-command</c> named command, which calls
/// <see cref="Stroke.Core.Buffer.OpenInEditorAsync"/> with <c>validateAndHandle: true</c>.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. Each loader creates and returns a new
/// key bindings instance.
/// </para>
/// </remarks>
public static class OpenInEditorBindings
{
    /// <summary>
    /// Load both the Vi and Emacs key bindings for handling edit-and-execute-command.
    /// </summary>
    /// <returns>
    /// A <see cref="MergedKeyBindings"/> containing both Emacs and Vi open-in-editor bindings.
    /// </returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>load_open_in_editor_bindings()</c>.
    /// This type is stateless and inherently thread-safe.
    /// </remarks>
    public static IKeyBindingsBase LoadOpenInEditorBindings()
    {
        return new MergedKeyBindings(
            LoadEmacsOpenInEditorBindings(),
            LoadViOpenInEditorBindings());
    }

    /// <summary>
    /// Load Emacs key binding for opening the buffer in an external editor.
    /// Pressing Ctrl-X followed by Ctrl-E invokes <c>edit-and-execute-command</c>.
    /// </summary>
    /// <returns>
    /// A <see cref="KeyBindings"/> with one binding: Ctrl-X Ctrl-E filtered by
    /// Emacs mode with no active selection.
    /// </returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>load_emacs_open_in_editor_bindings()</c>.
    /// This type is stateless and inherently thread-safe.
    /// </remarks>
    public static KeyBindings LoadEmacsOpenInEditorBindings()
    {
        var kb = new KeyBindings();

        var filter = new FilterOrBool(
            ((Filter)EmacsFilters.EmacsMode).And(AppFilters.HasSelection.Invert()));

        kb.Add<Binding>(
            [new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlE)],
            filter: filter)(
            NamedCommands.GetByName("edit-and-execute-command"));

        return kb;
    }

    /// <summary>
    /// Load Vi key binding for opening the buffer in an external editor.
    /// Pressing 'v' in navigation mode invokes <c>edit-and-execute-command</c>.
    /// </summary>
    /// <returns>
    /// A <see cref="KeyBindings"/> with one binding: 'v' filtered by Vi navigation mode.
    /// </returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>load_vi_open_in_editor_bindings()</c>.
    /// This type is stateless and inherently thread-safe.
    /// </remarks>
    public static KeyBindings LoadViOpenInEditorBindings()
    {
        var kb = new KeyBindings();

        kb.Add<Binding>(
            [new KeyOrChar('v')],
            filter: new FilterOrBool(ViFilters.ViNavigationMode))(
            NamedCommands.GetByName("edit-and-execute-command"));

        return kb;
    }
}
