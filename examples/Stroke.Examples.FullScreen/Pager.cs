using Stroke.Application;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Lexers;
using Stroke.Shortcuts;
using Stroke.Styles;
using Stroke.Widgets.Base;
using Stroke.Widgets.Toolbars;

namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// A simple application that shows a Pager application.
/// Port of Python Prompt Toolkit's pager.py example.
/// </summary>
internal static class Pager
{
    /// <summary>
    /// Runs the example application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Displays its own source code as read-only text with line numbers,
    /// scrollbar, search, and a status bar showing cursor position.
    /// </para>
    /// <para>
    /// Press Ctrl+C or 'q' to exit. Press '/' to start searching.
    /// </para>
    /// </remarks>
    public static void Run()
    {
        try
        {
            // Read this file's own source code.
            var pagerCsPath = Path.Combine(
                System.AppContext.BaseDirectory, "..", "..", "..", "Pager.cs");
            var text = File.Exists(pagerCsPath)
                ? File.ReadAllText(pagerCsPath)
                : "// Source file not found at: " + pagerCsPath +
                  "\n// This pager displays its own source code.\n// Run from the project directory to see the full content.";

            // Create one text buffer for the main content.
            var searchField = new SearchToolbar(
                textIfNotSearching: "Press '/' to start searching.");

            var textArea = new TextArea(
                text: text,
                readOnly: true,
                scrollbar: true,
                lineNumbers: true,
                searchField: searchField,
                lexer: PygmentsLexer.FromFilename("Pager.cs"));

            IReadOnlyList<StyleAndTextTuple> GetStatusbarText()
            {
                return
                [
                    new StyleAndTextTuple("class:status", pagerCsPath + " - "),
                    new StyleAndTextTuple("class:status.position",
                        $"{textArea.Document.CursorPositionRow + 1}:{textArea.Document.CursorPositionCol + 1}"),
                    new StyleAndTextTuple("class:status", " - Press "),
                    new StyleAndTextTuple("class:status.key", "Ctrl-C"),
                    new StyleAndTextTuple("class:status", " to exit, "),
                    new StyleAndTextTuple("class:status.key", "/"),
                    new StyleAndTextTuple("class:status", " for searching."),
                ];
            }

            var rootContainer = new HSplit(
            [
                // The top toolbar.
                new Window(
                    content: new FormattedTextControl(GetStatusbarText),
                    height: Dimension.Exact(1),
                    style: "class:status"),
                // The main content.
                textArea.PtContainer(),
                searchField.PtContainer(),
            ]);

            // Key bindings.
            Application<object> application = null!;

            var kb = new KeyBindings();
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlC)])((e) =>
            {
                application.Exit();
                return null;
            });
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar('q')])((e) =>
            {
                application.Exit();
                return null;
            });

            // Style: merge syntax highlighting theme with status bar styles.
            var style = StyleMerger.MergeStyles(
            [
                PygmentsStyles.DefaultDark,
                new Style(
                [
                    ("status", "reverse"),
                    ("status.position", "#aaaa00"),
                    ("status.key", "#ffaa00"),
                    ("not-searching", "#888888"),
                ]),
            ]);

            // Create application.
            application = new Application<object>(
                layout: new Stroke.Layout.Layout(
                    new AnyContainer(rootContainer),
                    focusedElement: new FocusableElement(new AnyContainer(textArea))),
                keyBindings: kb,
                enablePageNavigationBindings: true,
                mouseSupport: true,
                style: style,
                fullScreen: true);

            application.Run();
        }
        catch (KeyboardInterrupt)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }
}
