using System.Text.RegularExpressions;
using Stroke.AutoSuggest;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;

namespace Stroke.Application.Bindings;

/// <summary>
/// Key binding loader for auto suggestion acceptance bindings.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.auto_suggest</c> module.
/// Provides a single factory method that creates and returns a <see cref="KeyBindings"/>
/// instance containing all auto suggest key bindings: full suggestion acceptance
/// (Ctrl-F, Ctrl-E, Right arrow) and partial word-segment acceptance (Escape+F, Emacs only).
/// </para>
/// <para>
/// These bindings must be loaded after Vi bindings so that suggestion acceptance
/// takes priority over Vi cursor movement when a suggestion is available.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. The factory method creates a
/// new <see cref="KeyBindings"/> instance on each call.
/// </para>
/// </remarks>
public static class AutoSuggestBindings
{
    /// <summary>
    /// True when a suggestion is available for acceptance: the current buffer has
    /// a non-null suggestion with non-empty text and the cursor is at the end of the document.
    /// </summary>
    private static readonly IFilter SuggestionAvailable = new Condition(() =>
    {
        var app = AppContext.GetApp();
        var buffer = app.CurrentBuffer;
        return buffer.Suggestion is not null
            && buffer.Suggestion.Text.Length > 0
            && buffer.Document.IsCursorAtTheEnd;
    });

    /// <summary>
    /// Accept the full suggestion text. Reads <c>Buffer.Suggestion</c>, guards against
    /// null (race condition between filter evaluation and handler execution), then
    /// inserts the entire <c>Suggestion.Text</c> into the buffer via <c>Buffer.InsertText()</c>.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success; guard returns <c>null</c> if suggestion is null.</returns>
    /// <remarks>
    /// Maps to Python's <c>_accept</c> handler (private). Made public in C# for testability
    /// and consistency with the <c>kb.Add&lt;KeyHandlerCallable&gt;</c> registration pattern
    /// used throughout <c>Stroke.Application.Bindings</c>.
    /// </remarks>
    public static NotImplementedOrNone? AcceptSuggestion(KeyPressEvent @event)
    {
        var buffer = @event.CurrentBuffer;
        var suggestion = buffer?.Suggestion;

        if (suggestion is not null)
        {
            buffer!.InsertText(suggestion.Text);
        }

        return null;
    }

    /// <summary>
    /// Accept the next word segment of the suggestion. Splits the suggestion text
    /// using <c>Regex.Split(@"([^\s/]+(?:\s+|/))")</c> and inserts the first non-empty
    /// segment into the buffer. This respects both whitespace boundaries and path
    /// separator (/) boundaries, matching Python's <c>next(x for x in t if x)</c> logic.
    /// Active only in Emacs editing mode.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns><c>null</c> on success; guard returns <c>null</c> if suggestion is null.</returns>
    /// <remarks>
    /// Maps to Python's <c>_fill</c> handler (private). Made public in C# for testability
    /// and consistency with the <c>kb.Add&lt;KeyHandlerCallable&gt;</c> registration pattern
    /// used throughout <c>Stroke.Application.Bindings</c>.
    /// </remarks>
    public static NotImplementedOrNone? AcceptPartialSuggestion(KeyPressEvent @event)
    {
        var buffer = @event.CurrentBuffer;
        var suggestion = buffer?.Suggestion;

        if (suggestion is not null)
        {
            var segments = Regex.Split(suggestion.Text, @"([^\s/]+(?:\s+|/))");
            var firstSegment = segments.First(x => !string.IsNullOrEmpty(x));
            buffer!.InsertText(firstSegment);
        }

        return null;
    }

    /// <summary>
    /// Load key bindings for accepting auto suggestion text.
    /// </summary>
    /// <returns>
    /// A new <see cref="KeyBindings"/> instance containing all 4 auto suggest key bindings.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Registers the following bindings:
    /// </para>
    /// <list type="bullet">
    ///   <item>Ctrl-F → <see cref="AcceptSuggestion"/> (filter: SuggestionAvailable)</item>
    ///   <item>Ctrl-E → <see cref="AcceptSuggestion"/> (filter: SuggestionAvailable)</item>
    ///   <item>Right arrow → <see cref="AcceptSuggestion"/> (filter: SuggestionAvailable)</item>
    ///   <item>Escape+F → <see cref="AcceptPartialSuggestion"/> (filter: SuggestionAvailable AND EmacsMode)</item>
    /// </list>
    /// <para>
    /// These bindings must be loaded after Vi bindings so that suggestion acceptance
    /// takes priority over Vi cursor movement when a suggestion is available.
    /// When no suggestion is available, the filter evaluates to false and the key event
    /// falls through to bindings loaded earlier (e.g., Vi right arrow movement).
    /// </para>
    /// </remarks>
    public static KeyBindings LoadAutoSuggestBindings()
    {
        var kb = new KeyBindings();
        var filter = new FilterOrBool(SuggestionAvailable);

        // Full suggestion acceptance: Ctrl-F, Ctrl-E, Right arrow
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlF)],
            filter: filter)(AcceptSuggestion);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlE)],
            filter: filter)(AcceptSuggestion);
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Right)],
            filter: filter)(AcceptSuggestion);

        // Partial suggestion acceptance: Escape+F (Emacs mode only)
        var emacsFilter = new FilterOrBool(
            ((Filter)SuggestionAvailable).And(EmacsFilters.EmacsMode));
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Escape), new KeyOrChar('f')],
            filter: emacsFilter)(AcceptPartialSuggestion);

        return kb;
    }
}
