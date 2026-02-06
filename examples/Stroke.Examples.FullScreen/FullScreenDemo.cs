using Stroke.Application;
using Stroke.Completion;
using Stroke.Application.Bindings;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Menus;
using Stroke.Lexers;
using Stroke.Shortcuts;
using Stroke.Styles;
using Stroke.Widgets.Base;
using Stroke.Widgets.Dialogs;
using Stroke.Widgets.Lists;
using Stroke.Widgets.Menus;

namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// Full-screen demo — showcases multiple widgets including menus, dialogs, and completion.
/// Port of Python Prompt Toolkit's full-screen-demo.py example.
/// </summary>
internal static class FullScreenDemo
{
    private static readonly WordCompleter AnimalCompleter = new(
    [
        "alligator", "ant", "ape", "bat", "bear", "beaver", "bee", "bison",
        "butterfly", "cat", "chicken", "crocodile", "dinosaur", "dog",
        "dolphin", "dove", "duck", "eagle", "elephant", "fish", "goat",
        "gorilla", "kangaroo", "leopard", "lion", "mouse", "rabbit", "rat",
        "snake", "spider", "turkey", "turtle",
    ],
    ignoreCase: true);

    /// <summary>
    /// Runs the example application.
    /// </summary>
    public static void Run()
    {
        try
        {
            Application<object> application = null!;

            void AcceptYes() => application.Exit(result: true);
            void AcceptNo() => application.Exit(result: false);
            void DoExit() => application.Exit(result: false);

            var yesButton = new Button(text: "Yes", handler: AcceptYes);
            var noButton = new Button(text: "No", handler: AcceptNo);
            var textfield = new TextArea(lexer: PygmentsLexer.FromFilename("example.html"));
            var checkbox1 = new Checkbox(text: "Checkbox");
            var checkbox2 = new Checkbox(text: "Checkbox");

            var radios = new RadioList<string>(
                values:
                [
                    ("Red", "red"),
                    ("Green", "green"),
                    ("Blue", "blue"),
                    ("Orange", "orange"),
                    ("Yellow", "yellow"),
                    ("Purple", "Purple"),
                    ("Brown", "Brown"),
                ]);

            // Build layout.
            IContainer rootContainer = new HSplit(
            [
                // Top row: 3 columns (height=D() in Python — flexible to fill remaining space).
                new VSplit(
                    children:
                    [
                        new Frame(body: new AnyContainer(
                            new Label("Left frame\ncontent").PtContainer())).PtContainer(),
                        new Dialog(
                            title: "The custom window",
                            body: new AnyContainer(
                                new Label("hello\ntest").PtContainer())).PtContainer(),
                        textfield.PtContainer(),
                    ],
                    windowTooSmall: null,
                    align: HorizontalAlign.Justify,
                    padding: null,
                    paddingChar: null,
                    paddingStyle: "",
                    width: null,
                    height: new Dimension(),
                    zIndex: null,
                    modal: false,
                    keyBindings: null,
                    styleGetter: () => ""),
                // Middle row: progress, checkboxes, radios.
                new VSplit(
                [
                    new Frame(
                        body: new AnyContainer(new ProgressBar().PtContainer()),
                        title: "Progress bar").PtContainer(),
                    new Frame(
                        title: "Checkbox list",
                        body: new AnyContainer(new HSplit(
                        [
                            checkbox1.PtContainer(),
                            checkbox2.PtContainer(),
                        ]))).PtContainer(),
                    new Frame(
                        title: "Radio list",
                        body: new AnyContainer(radios.PtContainer())).PtContainer(),
                ], padding: 1),
                // Bottom row: buttons.
                new Box(
                    body: new AnyContainer(new VSplit(
                    [
                        yesButton.PtContainer(),
                        noButton.PtContainer(),
                    ], align: Stroke.Layout.Containers.HorizontalAlign.Center, padding: 3)),
                    height: Dimension.Exact(3),
                    style: "class:button-bar").PtContainer(),
            ]);

            // Wrap in MenuContainer.
            rootContainer = new MenuContainer(
                body: new AnyContainer(rootContainer),
                menuItems:
                [
                    new MenuItem("File", children:
                    [
                        new MenuItem("New"),
                        new MenuItem("Open", children:
                        [
                            new MenuItem("From file..."),
                            new MenuItem("From URL..."),
                            new MenuItem("Something else..", children:
                            [
                                new MenuItem("A"),
                                new MenuItem("B"),
                                new MenuItem("C"),
                                new MenuItem("D"),
                                new MenuItem("E"),
                            ]),
                        ]),
                        new MenuItem("Save"),
                        new MenuItem("Save as..."),
                        new MenuItem("-", disabled: true),
                        new MenuItem("Exit", handler: DoExit),
                    ]),
                    new MenuItem("Edit", children:
                    [
                        new MenuItem("Undo"),
                        new MenuItem("Cut"),
                        new MenuItem("Copy"),
                        new MenuItem("Paste"),
                        new MenuItem("Delete"),
                        new MenuItem("-", disabled: true),
                        new MenuItem("Find"),
                        new MenuItem("Find next"),
                        new MenuItem("Replace"),
                        new MenuItem("Go To"),
                        new MenuItem("Select All"),
                        new MenuItem("Time/Date"),
                    ]),
                    new MenuItem("View", children:
                    [
                        new MenuItem("Status Bar"),
                    ]),
                    new MenuItem("Info", children:
                    [
                        new MenuItem("About"),
                    ]),
                ],
                floats:
                [
                    new Float(
                        xcursor: true,
                        ycursor: true,
                        content: new AnyContainer(
                            new CompletionsMenu(maxHeight: 16, scrollOffset: 1))),
                ]).PtContainer();

            // Key bindings.
            var bindings = new KeyBindings();
            bindings.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.ControlI)])(FocusFunctions.FocusNext);
            bindings.Add<KeyHandlerCallable>(
                [new KeyOrChar(Keys.BackTab)])(FocusFunctions.FocusPrevious);

            var style = Style.FromDict(new Dictionary<string, string>
            {
                ["window.border"] = "#888888",
                ["shadow"] = "bg:#222222",
                ["menu-bar"] = "bg:#aaaaaa #888888",
                ["menu-bar.selected-item"] = "bg:#ffffff #000000",
                ["menu"] = "bg:#888888 #ffffff",
                ["menu.border"] = "#aaaaaa",
                ["window.border shadow"] = "#444444",
                ["focused  button"] = "bg:#880000 #ffffff noinherit",
                ["button-bar"] = "bg:#aaaaff",
            });

            application = new Application<object>(
                layout: new Stroke.Layout.Layout(
                    new AnyContainer(rootContainer),
                    focusedElement: new AnyContainer(yesButton.PtContainer())),
                keyBindings: bindings,
                style: style,
                mouseSupport: true,
                fullScreen: true);

            var result = application.Run();
            Console.WriteLine($"You said: {result}");
        }
        catch (KeyboardInterrupt) { }
        catch (EOFException) { }
    }
}
