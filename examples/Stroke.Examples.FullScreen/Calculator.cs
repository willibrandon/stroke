using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Stroke.Application;
using Stroke.Core;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Shortcuts;
using Stroke.Styles;
using Stroke.Widgets.Base;
using Stroke.Widgets.Toolbars;

namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// A simple example of a calculator program.
/// This could be used as inspiration for a REPL.
/// Port of Python Prompt Toolkit's calculator.py example.
/// </summary>
internal static class Calculator
{
    private const string HelpText = """

        Type any expression (e.g. "4 + 4") followed by enter to execute.
        Press Control-C to exit.
        """;

    /// <summary>
    /// Runs the example application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Demonstrates the accept_handler REPL pattern with TextArea input,
    /// expression evaluation, and styled output.
    /// </para>
    /// <para>
    /// Press Ctrl+C or Ctrl+Q to exit.
    /// </para>
    /// </remarks>
    public static void Run()
    {
        try
        {
            // The layout.
            var searchField = new SearchToolbar(); // For reverse search.

            var outputField = new TextArea(style: "class:output-field", text: HelpText);
            var inputField = new TextArea(
                height: Dimension.Exact(1),
                prompt: ">>> ",
                style: "class:input-field",
                multiline: false,
                wrapLines: false,
                searchField: searchField)
            {
                // Attach accept handler to the input field.
                // NOTE: It's better to assign an accept_handler, rather than adding a
                //       custom ENTER key binding. This will automatically reset the input
                //       field and add the strings to the history.
                // AcceptHandler returns ValueTask<bool>, enabling async/await natively.
                AcceptHandler = async (buff) =>
                    {
                        // Evaluate "calculator" expression.
                        string output;
                        try
                        {
                            var options = ScriptOptions.Default.AddImports("System");
                            var result = await CSharpScript.EvaluateAsync(buff.Text, options);
                            output = $"\n\nIn:  {buff.Text}\nOut: {result}";
                        }
                        catch (Exception ex)
                        {
                            output = $"\n\n{ex.Message}";
                        }

                        string newText = outputField.Text + output;

                        // Add text to output buffer.
                        outputField.Buffer.Document = new Document(text: newText, cursorPosition: newText.Length);
                        return false; // Don't keep text — clear input for next expression.
                    }
            };

            var container = new HSplit(
            [
                outputField.PtContainer(),
                new Window(height: Dimension.Exact(1), @char: "-", style: "class:line"),
                inputField.PtContainer(),
                searchField.PtContainer(),
            ]);

            // Build application reference so exit handler can reference it.
            Application<object> application = null!;

            // The key bindings.
            var kb = new KeyBindings();
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlC)])((e) =>
            {
                application.Exit();
                return null;
            });
            kb.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlQ)])((e) =>
            {
                application.Exit();
                return null;
            });

            // Style.
            var style = new Style(
            [
                ("output-field", "bg:#000044 #ffffff"),
                ("input-field", "bg:#000000 #ffffff"),
                ("line", "#004400"),
            ]);

            // Run application.
            application = new Application<object>(
                layout: new Stroke.Layout.Layout(
                    new AnyContainer(container),
                    focusedElement: new FocusableElement(new AnyContainer(inputField))),
                keyBindings: kb,
                style: style,
                mouseSupport: true,
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
