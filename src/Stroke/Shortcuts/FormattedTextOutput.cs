using System.Collections;

using Stroke.Application;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.Layout.Containers;
using Stroke.Output;
using Stroke.Rendering;
using Stroke.Styles;

namespace Stroke.Shortcuts;

/// <summary>
/// High-level functions for printing formatted text and rendering containers
/// directly to the terminal output.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>print_formatted_text</c> and
/// <c>print_container</c> functions from <c>prompt_toolkit.shortcuts.utils</c>.
/// </para>
/// <para>
/// This type is thread-safe. All methods are stateless and delegate to
/// thread-safe infrastructure.
/// </para>
/// </remarks>
public static class FormattedTextOutput
{
    /// <summary>
    /// Print formatted text to the terminal output.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If a prompt_toolkit Application is currently running, this will always
    /// print above the application or prompt. The method will erase the current
    /// application, print the text, and render the application again.
    /// </para>
    /// </remarks>
    /// <param name="text">The formatted text to print (string, HTML, ANSI, FormattedText).</param>
    /// <param name="sep">String inserted between values. Default: space.</param>
    /// <param name="end">String appended after the last value. Default: newline.</param>
    /// <param name="file">Optional TextWriter for output redirection. Mutually exclusive with <paramref name="output"/>.</param>
    /// <param name="flush">Whether to flush the output after printing.</param>
    /// <param name="style">Optional style for rendering.</param>
    /// <param name="output">Optional IOutput for direct output control. Mutually exclusive with <paramref name="file"/>.</param>
    /// <param name="colorDepth">Optional color depth override.</param>
    /// <param name="styleTransformation">Optional style transformation pipeline.</param>
    /// <param name="includeDefaultPygmentsStyle">Whether to include the default Pygments style. Default: true.</param>
    /// <exception cref="ArgumentException">Both <paramref name="output"/> and <paramref name="file"/> are specified.</exception>
    public static void Print(
        AnyFormattedText text,
        string sep = " ",
        string end = "\n",
        TextWriter? file = null,
        bool flush = false,
        IStyle? style = null,
        IOutput? output = null,
        ColorDepth? colorDepth = null,
        IStyleTransformation? styleTransformation = null,
        bool includeDefaultPygmentsStyle = true)
    {
        Print(
            [text],
            sep: sep,
            end: end,
            file: file,
            flush: flush,
            style: style,
            output: output,
            colorDepth: colorDepth,
            styleTransformation: styleTransformation,
            includeDefaultPygmentsStyle: includeDefaultPygmentsStyle);
    }

    /// <summary>
    /// Print multiple values to the terminal output, matching Python's print() semantics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Values are converted to formatted text fragments, joined with <paramref name="sep"/>,
    /// and terminated with <paramref name="end"/>. Plain lists that are not FormattedText
    /// are converted to their string representation.
    /// </para>
    /// </remarks>
    /// <param name="values">The values to print.</param>
    /// <param name="sep">String inserted between values. Default: space.</param>
    /// <param name="end">String appended after the last value. Default: newline.</param>
    /// <param name="file">Optional TextWriter for output redirection.</param>
    /// <param name="flush">Whether to flush the output after printing.</param>
    /// <param name="style">Optional style for rendering.</param>
    /// <param name="output">Optional IOutput for direct output control.</param>
    /// <param name="colorDepth">Optional color depth override.</param>
    /// <param name="styleTransformation">Optional style transformation pipeline.</param>
    /// <param name="includeDefaultPygmentsStyle">Whether to include the default Pygments style.</param>
    /// <exception cref="ArgumentException">Both <paramref name="output"/> and <paramref name="file"/> are specified.</exception>
    public static void Print(
        object[] values,
        string sep = " ",
        string end = "\n",
        TextWriter? file = null,
        bool flush = false,
        IStyle? style = null,
        IOutput? output = null,
        ColorDepth? colorDepth = null,
        IStyleTransformation? styleTransformation = null,
        bool includeDefaultPygmentsStyle = true)
    {
        if (output is not null && file is not null)
        {
            throw new ArgumentException(
                "Cannot specify both 'output' and 'file' parameters.");
        }

        // Resolve output.
        if (output is null)
        {
            output = file is not null
                ? OutputFactory.Create(stdout: file)
                : Stroke.Application.AppContext.GetAppSession().Output;
        }

        // Resolve color depth.
        colorDepth ??= output.GetDefaultColorDepth();

        // Build fragments.
        var fragments = new List<StyleAndTextTuple>();

        for (var i = 0; i < values.Length; i++)
        {
            fragments.AddRange(ToText(values[i]));

            if (sep.Length > 0 && i != values.Length - 1)
            {
                fragments.AddRange(ToText(sep));
            }
        }

        fragments.AddRange(ToText(end));

        // Define render action.
        var mergedStyle = CreateMergedStyle(style, includeDefaultPygmentsStyle);

        void Render()
        {
            AnyFormattedText wrappedFragments = new FormattedText.FormattedText(fragments);
            RendererUtils.PrintFormattedText(
                output,
                wrappedFragments,
                mergedStyle,
                colorDepth,
                styleTransformation);

            if (flush)
            {
                output.Flush();
            }
        }

        // Dispatch: if an Application is running, post to the event loop via
        // RunInTerminal and return immediately (fire-and-forget). This matches
        // Python's `loop.call_soon_threadsafe(lambda: run_in_terminal(render))`
        // which posts and returns without blocking. Blocking here would deadlock
        // if Print is called from within a RunInTerminal context.
        var app = Stroke.Application.AppContext.GetAppOrNull();
        if (app is not null)
        {
            _ = RunInTerminal.RunAsync(Render);
        }
        else
        {
            Render();
        }
    }

    /// <summary>
    /// Print any layout container to the output in a non-interactive way.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates a temporary Application with DummyInput, renders the container once,
    /// and terminates. The Application runs on a background thread (inThread=true).
    /// The <see cref="EndOfStreamException"/> from DummyInput is the expected
    /// termination signal and is caught silently.
    /// </para>
    /// </remarks>
    /// <param name="container">The layout container to render.</param>
    /// <param name="file">Optional TextWriter for output redirection.</param>
    /// <param name="style">Optional style for rendering.</param>
    /// <param name="includeDefaultPygmentsStyle">Whether to include the default Pygments style.</param>
    public static void PrintContainer(
        AnyContainer container,
        TextWriter? file = null,
        IStyle? style = null,
        bool includeDefaultPygmentsStyle = true)
    {
        var output = file is not null
            ? OutputFactory.Create(stdout: file)
            : Stroke.Application.AppContext.GetAppSession().Output;

        using var input = new DummyInput();
        var app = new Application<object?>(
            layout: new Stroke.Layout.Layout(container: container),
            output: output,
            input: input,
            style: CreateMergedStyle(style, includeDefaultPygmentsStyle));

        try
        {
            app.Run(inThread: true);
        }
        catch (EndOfStreamException)
        {
            // Expected termination from DummyInput — equivalent to Python's
            // `except EOFError: pass`.
        }
    }

    /// <summary>
    /// Merge user-defined style with built-in defaults.
    /// </summary>
    /// <param name="style">User-provided style (highest precedence).</param>
    /// <param name="includeDefaultPygmentsStyle">Include default Pygments style.</param>
    /// <returns>Merged style with order: default UI (lowest), [Pygments], [user] (highest).</returns>
    private static IStyle CreateMergedStyle(
        IStyle? style,
        bool includeDefaultPygmentsStyle)
    {
        var styles = new List<IStyle?> { DefaultStyles.DefaultUiStyle };

        if (includeDefaultPygmentsStyle)
        {
            styles.Add(DefaultStyles.DefaultPygmentsStyle);
        }

        if (style is not null)
        {
            styles.Add(style);
        }

        return StyleMerger.MergeStyles(styles);
    }

    /// <summary>
    /// Convert a value to formatted text fragments.
    /// </summary>
    /// <remarks>
    /// Plain lists (implementing <see cref="IList"/> but not <see cref="FormattedText.FormattedText"/>)
    /// are converted to their string representation. All other values are converted via
    /// <see cref="FormattedTextUtils.ToFormattedText"/> with auto-conversion enabled.
    /// </remarks>
    private static FormattedText.FormattedText ToText(object val)
    {
        // Normal lists which are not instances of FormattedText are
        // considered plain text.
        if (val is IList && val is not FormattedText.FormattedText)
        {
            AnyFormattedText listText = val.ToString() ?? "";
            return FormattedTextUtils.ToFormattedText(listText);
        }

        var anyText = ToAnyFormattedText(val);
        return FormattedTextUtils.ToFormattedText(anyText, autoConvert: true);
    }

    /// <summary>
    /// Convert an arbitrary object to <see cref="AnyFormattedText"/> via implicit conversions
    /// for known types, falling back to <see cref="object.ToString"/> for others.
    /// </summary>
    private static AnyFormattedText ToAnyFormattedText(object val) => val switch
    {
        AnyFormattedText aft => aft,
        string s => s,
        FormattedText.FormattedText ft => ft,
        Html html => html,
        Ansi ansi => ansi,
        PygmentsTokens tokens => tokens,
        Func<AnyFormattedText> func => func,
        _ => val.ToString() ?? ""
    };
}
