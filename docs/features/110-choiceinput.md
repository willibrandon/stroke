# Feature 110: Choice Input

## Overview

Implement the ChoiceInput class and `choice` function - a selection prompt that asks users to choose among a set of options using a RadioList widget.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/shortcuts/choice_input.py`

## Public API

### ChoiceInput

```csharp
namespace Stroke.Shortcuts;

/// <summary>
/// Input selection prompt. Ask the user to choose among a set of options.
/// </summary>
/// <typeparam name="T">Type of the option values.</typeparam>
/// <example>
/// var inputSelection = new ChoiceInput&lt;string&gt;(
///     message: "Please select a dish:",
///     options: new[]
///     {
///         ("pizza", "Pizza with mushrooms"),
///         ("salad", "Salad with tomatoes"),
///         ("sushi", "Sushi"),
///     },
///     defaultValue: "pizza"
/// );
/// var result = inputSelection.Prompt();
/// </example>
public sealed class ChoiceInput<T>
{
    /// <summary>
    /// Create a choice input prompt.
    /// </summary>
    /// <param name="message">Plain text or formatted text shown before options.</param>
    /// <param name="options">Sequence of (value, label) tuples.</param>
    /// <param name="defaultValue">Default value. First option if not specified.</param>
    /// <param name="mouseSupport">Enable mouse support.</param>
    /// <param name="style">Style instance for the color scheme.</param>
    /// <param name="symbol">Symbol displayed in front of selected choice.</param>
    /// <param name="bottomToolbar">Formatted text displayed at screen bottom.</param>
    /// <param name="showFrame">When true, surround input with a frame.</param>
    /// <param name="enableSuspend">Enable Ctrl+Z suspend to background.</param>
    /// <param name="enableInterrupt">Enable Ctrl+C interrupt.</param>
    /// <param name="interruptException">Exception type for keyboard interrupt.</param>
    /// <param name="keyBindings">Additional key bindings.</param>
    public ChoiceInput(
        AnyFormattedText message,
        IReadOnlyList<(T Value, AnyFormattedText Label)> options,
        T? defaultValue = default,
        bool mouseSupport = false,
        IStyle? style = null,
        string symbol = ">",
        AnyFormattedText? bottomToolbar = null,
        FilterOrBool showFrame = default,
        FilterOrBool enableSuspend = default,
        FilterOrBool enableInterrupt = default,
        Type? interruptException = null,
        IKeyBindings? keyBindings = null);

    /// <summary>
    /// Run the prompt synchronously and return the selected value.
    /// </summary>
    /// <returns>The selected option value.</returns>
    public T Prompt();

    /// <summary>
    /// Run the prompt asynchronously and return the selected value.
    /// </summary>
    /// <returns>The selected option value.</returns>
    public Task<T> PromptAsync();
}
```

### choice Function

```csharp
namespace Stroke.Shortcuts;

public static partial class Dialogs
{
    /// <summary>
    /// Choice selection prompt. Ask the user to choose among a set of options.
    /// </summary>
    /// <typeparam name="T">Type of the option values.</typeparam>
    /// <param name="message">Plain text or formatted text shown before options.</param>
    /// <param name="options">Sequence of (value, label) tuples.</param>
    /// <param name="defaultValue">Default value. First option if not specified.</param>
    /// <param name="mouseSupport">Enable mouse support.</param>
    /// <param name="style">Style instance for the color scheme.</param>
    /// <param name="symbol">Symbol displayed in front of selected choice.</param>
    /// <param name="bottomToolbar">Formatted text displayed at screen bottom.</param>
    /// <param name="showFrame">When true, surround input with a frame.</param>
    /// <param name="enableSuspend">Enable Ctrl+Z suspend to background.</param>
    /// <param name="enableInterrupt">Enable Ctrl+C interrupt.</param>
    /// <param name="interruptException">Exception type for keyboard interrupt.</param>
    /// <param name="keyBindings">Additional key bindings.</param>
    /// <returns>The selected option value.</returns>
    /// <example>
    /// var result = Dialogs.Choice(
    ///     message: "Please select a dish:",
    ///     options: new[]
    ///     {
    ///         ("pizza", "Pizza with mushrooms"),
    ///         ("salad", "Salad with tomatoes"),
    ///         ("sushi", "Sushi"),
    ///     },
    ///     defaultValue: "pizza"
    /// );
    /// </example>
    public static T Choice<T>(
        AnyFormattedText message,
        IReadOnlyList<(T Value, AnyFormattedText Label)> options,
        T? defaultValue = default,
        bool mouseSupport = false,
        IStyle? style = null,
        string symbol = ">",
        AnyFormattedText? bottomToolbar = null,
        bool showFrame = false,
        FilterOrBool enableSuspend = default,
        FilterOrBool enableInterrupt = default,
        Type? interruptException = null,
        IKeyBindings? keyBindings = null);
}
```

## Project Structure

```
src/Stroke/
└── Shortcuts/
    └── ChoiceInput.cs
tests/Stroke.Tests/
└── Shortcuts/
    └── ChoiceInputTests.cs
```

## Implementation Notes

### Default Style

```csharp
private static IStyle CreateDefaultChoiceInputStyle()
{
    return Style.FromDict(new Dictionary<string, string>
    {
        ["frame.border"] = "#884444",
        ["selected-option"] = "bold"
    });
}
```

### ChoiceInput Implementation

```csharp
public sealed class ChoiceInput<T>
{
    private readonly AnyFormattedText _message;
    private readonly IReadOnlyList<(T Value, AnyFormattedText Label)> _options;
    private readonly T? _defaultValue;
    private readonly bool _mouseSupport;
    private readonly IStyle _style;
    private readonly string _symbol;
    private readonly AnyFormattedText? _bottomToolbar;
    private readonly Filter _showFrame;
    private readonly Filter _enableSuspend;
    private readonly Filter _enableInterrupt;
    private readonly Type _interruptException;
    private readonly IKeyBindings? _keyBindings;

    public ChoiceInput(
        AnyFormattedText message,
        IReadOnlyList<(T Value, AnyFormattedText Label)> options,
        T? defaultValue = default,
        bool mouseSupport = false,
        IStyle? style = null,
        string symbol = ">",
        AnyFormattedText? bottomToolbar = null,
        FilterOrBool showFrame = default,
        FilterOrBool enableSuspend = default,
        FilterOrBool enableInterrupt = default,
        Type? interruptException = null,
        IKeyBindings? keyBindings = null)
    {
        _message = message;
        _options = options;
        _defaultValue = defaultValue;
        _mouseSupport = mouseSupport;
        _style = style ?? CreateDefaultChoiceInputStyle();
        _symbol = symbol;
        _bottomToolbar = bottomToolbar;
        _showFrame = showFrame.ToFilter();
        _enableSuspend = enableSuspend.ToFilter();
        _enableInterrupt = enableInterrupt.IsDefault
            ? Filters.Always
            : enableInterrupt.ToFilter();
        _interruptException = interruptException ?? typeof(OperationCanceledException);
        _keyBindings = keyBindings;
    }

    private Application<T> CreateApplication()
    {
        var radioList = new RadioList<T>(
            values: _options,
            defaultValue: _defaultValue,
            selectOnFocus: true,
            openCharacter: "",
            selectCharacter: _symbol,
            closeCharacter: "",
            showCursor: false,
            showNumbers: true,
            containerStyle: "class:input-selection",
            defaultStyle: "class:option",
            selectedStyle: "",
            checkedStyle: "class:selected-option",
            numberStyle: "class:number",
            showScrollbar: false
        );

        var container = new HSplit(
            new Box(
                new Label(_message, dontExtendHeight: true),
                paddingTop: 0,
                paddingLeft: 1,
                paddingRight: 1,
                paddingBottom: 0
            ),
            new Box(
                radioList,
                paddingTop: 0,
                paddingLeft: 3,
                paddingRight: 1,
                paddingBottom: 0
            )
        );

        // Add frame conditionally
        IContainer framedContainer = new ConditionalContainer(
            new Frame(container),
            alternativeContent: container,
            filter: _showFrame
        );

        // Add bottom toolbar
        var showBottomToolbar = Filters.Condition(() => _bottomToolbar != null)
            & ~Filters.IsDone
            & Filters.RendererHeightIsKnown;

        var bottomToolbar = new ConditionalContainer(
            new Window(
                new FormattedTextControl(() => _bottomToolbar!,
                    style: "class:bottom-toolbar.text"),
                style: "class:bottom-toolbar",
                dontExtendHeight: true,
                height: Dimension.Min(1)
            ),
            filter: showBottomToolbar
        );

        var layout = new Layout(
            new HSplit(
                framedContainer,
                new ConditionalContainer(new Window(), filter: showBottomToolbar),
                bottomToolbar
            ),
            focusedElement: radioList
        );

        // Key bindings
        var kb = new KeyBindings();

        kb.Add(Keys.Enter, eager: true, handler: e =>
        {
            e.App.Exit(result: radioList.CurrentValue, style: "class:accepted");
        });

        kb.Add(Keys.ControlC, filter: _enableInterrupt, handler: e =>
        {
            var ex = (Exception)Activator.CreateInstance(_interruptException)!;
            e.App.Exit(exception: ex, style: "class:aborting");
        });

        if (Platform.SuspendToBackgroundSupported)
        {
            kb.Add(Keys.ControlZ, filter: _enableSuspend, handler: e =>
            {
                e.App.SuspendToBackground();
            });
        }

        return new Application<T>(
            layout: layout,
            fullScreen: false,
            mouseSupport: _mouseSupport,
            keyBindings: KeyBindings.Merge(kb, new DynamicKeyBindings(() => _keyBindings)),
            style: _style
        );
    }

    public T Prompt() => CreateApplication().Run();

    public Task<T> PromptAsync() => CreateApplication().RunAsync();
}
```

### Usage Example

```csharp
// Simple choice
var dish = Dialogs.Choice(
    message: "Please select a dish:",
    options: new[]
    {
        ("pizza", "Pizza with mushrooms"),
        ("salad", "Salad with tomatoes"),
        ("sushi", "Sushi")
    },
    defaultValue: "pizza"
);

Console.WriteLine($"You selected: {dish}");

// With styling and frame
var color = Dialogs.Choice(
    message: "Choose your favorite color:",
    options: new[]
    {
        ("red", new HTML("<ansired>Red</ansired>")),
        ("green", new HTML("<ansigreen>Green</ansigreen>")),
        ("blue", new HTML("<ansiblue>Blue</ansiblue>"))
    },
    showFrame: true,
    bottomToolbar: "Press Enter to select, Ctrl+C to cancel"
);
```

## Dependencies

- Feature 31: Application
- Feature 45: Widgets (RadioList, Box, Frame, Label)
- Feature 25: Containers (HSplit, ConditionalContainer)
- Feature 19: Key Bindings
- Feature 14: Styles

## Implementation Tasks

1. Define ChoiceInput generic class
2. Implement CreateApplication with RadioList
3. Add frame conditional display
4. Add bottom toolbar conditional display
5. Implement key bindings (Enter, Ctrl+C, Ctrl+Z)
6. Implement Prompt and PromptAsync methods
7. Implement static Choice helper function
8. Write unit tests

## Acceptance Criteria

- [ ] ChoiceInput displays message and options
- [ ] Options are selectable with keyboard
- [ ] Numbers can be used to select options
- [ ] Enter accepts the current selection
- [ ] Ctrl+C raises interrupt exception when enabled
- [ ] Ctrl+Z suspends when enabled (Unix)
- [ ] Frame is displayed conditionally
- [ ] Bottom toolbar is displayed conditionally
- [ ] Mouse support works when enabled
- [ ] Unit tests achieve 80% coverage
