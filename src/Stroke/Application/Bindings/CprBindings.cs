using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application.Bindings;

/// <summary>
/// Key binding loader for Cursor Position Report (CPR) response handling.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.cpr</c> module.
/// Provides a single factory method that creates and returns a <see cref="KeyBindings"/>
/// instance containing a binding for <see cref="Keys.CPRResponse"/> events.
/// </para>
/// <para>
/// When the terminal responds to a cursor position request (DSR), it sends an escape
/// sequence of the form <c>\x1b[row;colR</c>. The handler parses this response and
/// reports the absolute cursor row to the renderer for accurate screen positioning.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. The factory method creates a
/// new <see cref="KeyBindings"/> instance on each call.
/// </para>
/// </remarks>
public static class CprBindings
{
    /// <summary>
    /// Load key bindings for handling CPR (Cursor Position Report) responses.
    /// </summary>
    /// <returns>
    /// A new <see cref="KeyBindings"/> instance containing a single binding for
    /// <see cref="Keys.CPRResponse"/> with saveBefore disabled.
    /// </returns>
    public static KeyBindings LoadCprBindings()
    {
        var kb = new KeyBindings();

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.CPRResponse)],
            saveBefore: _ => false)(CprHandler);

        return kb;
    }

    private static NotImplementedOrNone? CprHandler(KeyPressEvent @event)
    {
        // The incoming data looks like "\x1b[35;1R"
        // Parse row/col information.
        var parts = @event.Data[2..^1].Split(';');
        var row = int.Parse(parts[0]);
        var col = int.Parse(parts[1]);

        // Report absolute cursor position to the renderer.
        @event.GetApp().Renderer.ReportAbsoluteCursorRow(row);

        return null;
    }
}
