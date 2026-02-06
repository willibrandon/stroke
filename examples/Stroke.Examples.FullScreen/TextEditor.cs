using Stroke.Application;
using Stroke.Completion;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Menus;
using Stroke.Lexers;
using Stroke.Shortcuts;
using Stroke.Styles;
using Stroke.Widgets.Base;
using Stroke.Widgets.Dialogs;
using Stroke.Widgets.Menus;
using Stroke.Widgets.Toolbars;
using AppContext = Stroke.Application.AppContext;

namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// Text editor demo — a complete notepad-like editor with menus, search, and syntax highlighting.
/// Port of Python Prompt Toolkit's text-editor.py example.
/// </summary>
internal static class TextEditor
{
    private static bool _showStatusBar = true;
    private static string? _currentPath;
    private static MenuContainer _rootContainer = null!;

    /// <summary>
    /// Runs the example application.
    /// </summary>
    public static void Run()
    {
        try
        {
            _showStatusBar = true;
            _currentPath = null;

            var searchToolbar = new SearchToolbar();

            var textField = new TextArea(
                lexer: new DynamicLexer(() =>
                    PygmentsLexer.FromFilename(_currentPath ?? ".txt", syncFromStart: false)),
                scrollbar: true,
                lineNumbers: true,
                searchField: searchToolbar);

            string GetStatusbarText() => " Press Ctrl-C to open menu. ";
            string GetStatusbarRightText() =>
                $" {textField.Document.CursorPositionRow + 1}:" +
                $"{textField.Document.CursorPositionCol + 1}  ";

            var body = new HSplit(
            [
                textField.PtContainer(),
                searchToolbar.PtContainer(),
                new ConditionalContainer(
                    content: new AnyContainer(new VSplit(
                    [
                        new Window(
                            content: new FormattedTextControl(
                                (Func<IReadOnlyList<StyleAndTextTuple>>)(() =>
                                    [new StyleAndTextTuple("class:status", GetStatusbarText())])),
                            style: "class:status"),
                        new Window(
                            content: new FormattedTextControl(
                                (Func<IReadOnlyList<StyleAndTextTuple>>)(() =>
                                    [new StyleAndTextTuple("class:status.right", GetStatusbarRightText())])),
                            style: "class:status.right",
                            width: Dimension.Exact(9),
                            align: WindowAlign.Right),
                    ], height: 1)),
                    filter: new FilterOrBool(new Condition(() => _showStatusBar))),
            ]);

            // Menu handlers.
            void DoNewFile() => textField.Text = "";

            void DoOpenFile() => _ = DoOpenFileAsync();

            async Task DoOpenFileAsync()
            {
                var tcs = new TaskCompletionSource<string?>();

                Button okButton = null!;

                var dialogTextArea = new TextArea(
                    completer: new PathCompleter(),
                    multiline: false,
                    width: new Dimension(preferred: 40))
                {
                    AcceptHandler = (buf) =>
                    {
                        AppContext.GetApp().Layout.Focus(
                            new FocusableElement(new AnyContainer(okButton.PtContainer())));
                        return ValueTask.FromResult(true);
                    }
                };

                okButton = new Button(text: "OK", handler: () =>
                    tcs.TrySetResult(dialogTextArea.Text));
                var cancelButton = new Button(text: "Cancel", handler: () =>
                    tcs.TrySetResult(null));

                var dialog = new Dialog(
                    title: "Open file",
                    body: new AnyContainer(new HSplit(
                    [
                        new Label("Enter the path of a file:").PtContainer(),
                        dialogTextArea.PtContainer(),
                    ])),
                    buttons: [okButton, cancelButton],
                    width: new Dimension(preferred: 80),
                    modal: true);

                var path = await ShowDialogAsFloat(dialog, tcs);
                _currentPath = path;

                if (path != null)
                {
                    try
                    {
                        textField.Text = File.ReadAllText(path);
                    }
                    catch (IOException e)
                    {
                        ShowMessage("Error", e.Message);
                    }
                }
            }

            void DoExit() => AppContext.GetApp().Exit();

            void DoUndo() => textField.Buffer.Undo();

            void DoCut()
            {
                var data = textField.Buffer.CutSelection();
                AppContext.GetApp().Clipboard.SetData(data);
            }

            void DoCopy()
            {
                var data = textField.Buffer.CopySelection();
                AppContext.GetApp().Clipboard.SetData(data);
            }

            void DoDelete() => textField.Buffer.CutSelection();

            void DoFind() => SearchOperations.StartSearch(textField.Control);

            void DoFindNext()
            {
                var searchState = AppContext.GetApp().CurrentSearchState;
                var pos = textField.Buffer.GetSearchPosition(searchState, includeCurrentPosition: false);
                textField.Buffer.CursorPosition = pos;
            }

            void DoPaste() =>
                textField.Buffer.PasteClipboardData(AppContext.GetApp().Clipboard.GetData());

            void DoSelectAll()
            {
                textField.Buffer.CursorPosition = 0;
                textField.Buffer.StartSelection();
                textField.Buffer.CursorPosition = textField.Buffer.Text.Length;
            }

            void DoTimeDate()
            {
                var text = DateTime.Now.ToString("o");
                textField.Buffer.InsertText(text);
            }

            void DoStatusBar() => _showStatusBar = !_showStatusBar;

            void DoAbout() =>
                ShowMessage("About", "Text editor demo.\nCreated by Jonathan Slenders.");

            void DoGoTo() => _ = DoGoToAsync();

            async Task DoGoToAsync()
            {
                var tcs = new TaskCompletionSource<string?>();

                Button okButton = null!;

                var dialogTextArea = new TextArea(
                    multiline: false,
                    width: new Dimension(preferred: 40))
                {
                    AcceptHandler = (buf) =>
                    {
                        AppContext.GetApp().Layout.Focus(
                            new FocusableElement(new AnyContainer(okButton.PtContainer())));
                        return ValueTask.FromResult(true);
                    }
                };

                okButton = new Button(text: "OK", handler: () =>
                    tcs.TrySetResult(dialogTextArea.Text));
                var cancelButton = new Button(text: "Cancel", handler: () =>
                    tcs.TrySetResult(null));

                var dialog = new Dialog(
                    title: "Go to line",
                    body: new AnyContainer(new HSplit(
                    [
                        new Label("Line number:").PtContainer(),
                        dialogTextArea.PtContainer(),
                    ])),
                    buttons: [okButton, cancelButton],
                    width: new Dimension(preferred: 80),
                    modal: true);

                var lineNumberStr = await ShowDialogAsFloat(dialog, tcs);

                if (int.TryParse(lineNumberStr, out var lineNumber))
                {
                    textField.Buffer.CursorPosition =
                        textField.Buffer.Document.TranslateRowColToIndex(lineNumber - 1, 0);
                }
                else if (lineNumberStr != null)
                {
                    ShowMessage("Invalid line number", $"\"{lineNumberStr}\" is not a valid number.");
                }
            }

            // --- Dialog helpers (port of Python's show_dialog_as_float / show_message) ---

            async Task<T?> ShowDialogAsFloat<T>(Dialog dialog, TaskCompletionSource<T?> tcs)
            {
                var floatElement = new Float(content: new AnyContainer(dialog.PtContainer()));
                _rootContainer.Floats.Insert(0, floatElement);

                var app = AppContext.GetApp();
                var focusedBefore = app.Layout.CurrentWindow;
                app.Layout.Focus(new FocusableElement(new AnyContainer(dialog.PtContainer())));

                var result = await tcs.Task;

                app.Layout.Focus(new FocusableElement(focusedBefore));

                if (_rootContainer.Floats.Contains(floatElement))
                    _rootContainer.Floats.Remove(floatElement);

                return result;
            }

            void ShowMessage(string title, string text)
            {
                _ = ShowMessageAsync(title, text);
            }

            async Task ShowMessageAsync(string title, string text)
            {
                var tcs = new TaskCompletionSource<object?>();

                var okButton = new Button(text: "OK", handler: () =>
                    tcs.TrySetResult(null));

                var dialog = new Dialog(
                    title: title,
                    body: new AnyContainer(new HSplit(
                    [
                        new Label(text).PtContainer(),
                    ])),
                    buttons: [okButton],
                    width: new Dimension(preferred: 80),
                    modal: true);

                await ShowDialogAsFloat(dialog, tcs);
            }

            // Key bindings.
            Application<object> application = null!;

            var bindings = new KeyBindings();
            bindings.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlC)])((e) =>
            {
                application.Layout.Focus(_rootContainer.Window);
                return null;
            });

            // Create MenuContainer.
            _rootContainer = new MenuContainer(
                body: new AnyContainer(body),
                menuItems:
                [
                    new MenuItem("File", children:
                    [
                        new MenuItem("New...", handler: DoNewFile),
                        new MenuItem("Open...", handler: DoOpenFile),
                        new MenuItem("Save"),
                        new MenuItem("Save as..."),
                        new MenuItem("-", disabled: true),
                        new MenuItem("Exit", handler: DoExit),
                    ]),
                    new MenuItem("Edit", children:
                    [
                        new MenuItem("Undo", handler: DoUndo),
                        new MenuItem("Cut", handler: DoCut),
                        new MenuItem("Copy", handler: DoCopy),
                        new MenuItem("Paste", handler: DoPaste),
                        new MenuItem("Delete", handler: DoDelete),
                        new MenuItem("-", disabled: true),
                        new MenuItem("Find", handler: DoFind),
                        new MenuItem("Find next", handler: DoFindNext),
                        new MenuItem("Replace"),
                        new MenuItem("Go To", handler: DoGoTo),
                        new MenuItem("Select All", handler: DoSelectAll),
                        new MenuItem("Time/Date", handler: DoTimeDate),
                    ]),
                    new MenuItem("View", children:
                    [
                        new MenuItem("Status Bar", handler: DoStatusBar),
                    ]),
                    new MenuItem("Info", children:
                    [
                        new MenuItem("About", handler: DoAbout),
                    ]),
                ],
                floats:
                [
                    new Float(
                        xcursor: true,
                        ycursor: true,
                        content: new AnyContainer(
                            new CompletionsMenu(maxHeight: 16, scrollOffset: 1))),
                ],
                keyBindings: bindings);

            var style = Style.FromDict(new Dictionary<string, string>
            {
                ["status"] = "reverse",
                ["shadow"] = "bg:#440044",
            });

            application = new Application<object>(
                layout: new Stroke.Layout.Layout(
                    new AnyContainer(_rootContainer.PtContainer()),
                    focusedElement: new AnyContainer(textField.PtContainer())),
                enablePageNavigationBindings: true,
                style: style,
                mouseSupport: true,
                fullScreen: true);

            application.Run();
        }
        catch (KeyboardInterrupt) { }
        catch (EOFException) { }
    }
}
