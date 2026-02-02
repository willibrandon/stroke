using Stroke.AutoSuggest;
using Stroke.Completion;
using Stroke.Core;
using Stroke.Filters;
using Stroke.KeyBinding;
using Stroke.Validation;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Shortcuts;

public partial class PromptSession<TResult>
{
    /// <summary>
    /// Creates the default input buffer with dynamic completer, validator, auto-suggest,
    /// and accept handler.
    /// </summary>
    /// <returns>The configured default buffer.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_create_default_buffer</c>.
    /// The accept handler exits the Application with the buffer text as the result.
    /// </remarks>
    private Buffer CreateDefaultBuffer()
    {
        // Complete-while-typing condition:
        // True when completeWhileTyping AND NOT enableHistorySearch AND NOT ReadlineLike
        var completeWhileTypingCondition = new Condition(() =>
            FilterUtils.ToFilter(CompleteWhileTyping).Invoke()
            && !FilterUtils.ToFilter(EnableHistorySearch).Invoke()
            && CompleteStyle != CompleteStyle.ReadlineLike);

        return new Buffer(
            name: BufferNames.Default,
            completeWhileTyping: () => completeWhileTypingCondition.Invoke(),
            validateWhileTyping: () => DynCond(() => ValidateWhileTyping).Invoke(),
            enableHistorySearch: () => DynCond(() => EnableHistorySearch).Invoke(),
            validator: new DynamicValidator(() => Validator),
            completer: new DynamicCompleter(() =>
            {
                var c = Completer;
                return CompleteInThread && c is not null
                    ? new ThreadedCompleter(c)
                    : c;
            }),
            history: History,
            autoSuggest: new DynamicAutoSuggest(() => AutoSuggest),
            acceptHandler: AcceptInput,
            tempfileSuffix: ResolveTempfileSuffix(),
            tempfile: ResolveTempfile()
        );
    }

    /// <summary>
    /// Accept handler for the default buffer. Exits the application with the buffer text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Python's accept handler does <c>cast(Application[str], get_app()).exit(result=buff.document.text)</c>,
    /// relying on dynamic typing. In C#, <typeparamref name="TResult"/> may not be <c>string</c>
    /// (e.g., <c>PromptSession&lt;bool&gt;</c> for confirm dialogs). For non-string sessions,
    /// the confirm key bindings call <c>App.Exit(result: true/false)</c> directly; this handler
    /// only executes via Enter key on the default buffer.
    /// </para>
    /// </remarks>
    private bool AcceptInput(Buffer buffer)
    {
        var app = Application.AppContext.GetApp();
        var text = buffer.Document.Text;

        // For PromptSession<string>, cast directly. For other TResult types,
        // attempt conversion — this mirrors Python's dynamic cast behavior.
        if (text is TResult result)
        {
            app.Exit(result: result, style: "class:accepted");
        }
        else
        {
            try
            {
                app.Exit(result: (TResult)Convert.ChangeType(text, typeof(TResult)), style: "class:accepted");
            }
            catch (InvalidCastException)
            {
                app.Exit(result: default!, style: "class:accepted");
            }
            catch (FormatException)
            {
                app.Exit(result: default!, style: "class:accepted");
            }
        }

        return true; // Keep text, we call Reset later
    }

    /// <summary>
    /// Creates the search buffer for incremental search.
    /// </summary>
    /// <returns>A simple buffer for search input.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_create_search_buffer</c>.
    /// </remarks>
    private Buffer CreateSearchBuffer()
    {
        return new Buffer(name: BufferNames.Search);
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEMPFILE HELPERS
    // ═══════════════════════════════════════════════════════════════════

    private string ResolveTempfileSuffix()
    {
        var val = TempfileSuffix;
        return val is Func<string> f ? f() : (val as string ?? ".txt");
    }

    private string ResolveTempfile()
    {
        var val = Tempfile;
        return val is Func<string> f ? f() : (val as string ?? "");
    }
}
