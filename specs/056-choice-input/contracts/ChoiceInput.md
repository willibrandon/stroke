# API Contract: ChoiceInput

**Feature**: 056-choice-input
**Date**: 2026-02-03

## Namespace

```csharp
namespace Stroke.Shortcuts;
```

## ChoiceInput<T> Class

```csharp
/// <summary>
/// Input selection prompt. Ask the user to choose among a set of options.
/// </summary>
/// <typeparam name="T">The type of value returned when an option is selected.</typeparam>
/// <remarks>
/// <para>
/// Example usage:
/// </para>
/// <code>
/// var choiceInput = new ChoiceInput&lt;string&gt;(
///     message: "Please select a dish:",
///     options: new[]
///     {
///         ("pizza", (AnyFormattedText)"Pizza with mushrooms"),
///         ("salad", (AnyFormattedText)"Salad with tomatoes"),
///         ("sushi", (AnyFormattedText)"Sushi"),
///     },
///     defaultValue: "pizza");
///
/// string result = choiceInput.Prompt();
/// </code>
/// <para>
/// Thread safety: This class is thread-safe. All configuration is immutable after construction.
/// The underlying Application handles thread-safe rendering and input processing.
/// </para>
/// </remarks>
public sealed class ChoiceInput<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChoiceInput{T}"/> class.
    /// </summary>
    /// <param name="message">Plain text or formatted text to be shown before the options.</param>
    /// <param name="options">
    /// Sequence of (value, label) tuples. The labels can be formatted text.
    /// </param>
    /// <param name="defaultValue">
    /// Default value. If none is given or the value doesn't match any option,
    /// the first option is considered the default.
    /// </param>
    /// <param name="mouseSupport">Enable mouse support for clicking options.</param>
    /// <param name="style">
    /// <see cref="IStyle"/> instance for the color scheme.
    /// If null, a default style is used with brownish-red frame border and bold selected option.
    /// </param>
    /// <param name="symbol">Symbol to be displayed in front of the selected choice. Default is ">".</param>
    /// <param name="bottomToolbar">
    /// Formatted text or callable that returns formatted text to be displayed at the bottom of the screen.
    /// </param>
    /// <param name="showFrame">
    /// When true, surround the input with a frame.
    /// Can be a bool or an <see cref="IFilter"/> for conditional display.
    /// </param>
    /// <param name="enableSuspend">
    /// When true on Unix platforms, allow Ctrl+Z to suspend the process to background.
    /// Has no effect on Windows.
    /// </param>
    /// <param name="enableInterrupt">
    /// When true (default), raise the <paramref name="interruptException"/> when Ctrl+C is pressed.
    /// </param>
    /// <param name="interruptException">
    /// The exception type that will be raised on Ctrl+C.
    /// Must be assignable to <see cref="Exception"/>. Default is <see cref="KeyboardInterrupt"/>.
    /// </param>
    /// <param name="keyBindings">
    /// Additional key bindings to merge with the default bindings.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="options"/> is empty.</exception>
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
        IKeyBindingsBase? keyBindings = null);

    /// <summary>
    /// Gets the message/prompt text displayed above the options.
    /// </summary>
    public AnyFormattedText Message { get; }

    /// <summary>
    /// Gets the default value that is pre-selected when the prompt displays.
    /// </summary>
    public T? Default { get; }

    /// <summary>
    /// Gets the available options as value-label pairs.
    /// </summary>
    public IReadOnlyList<(T Value, AnyFormattedText Label)> Options { get; }

    /// <summary>
    /// Gets a value indicating whether mouse support is enabled.
    /// </summary>
    public bool MouseSupport { get; }

    /// <summary>
    /// Gets the style used for rendering.
    /// </summary>
    public IStyle Style { get; }

    /// <summary>
    /// Gets the symbol displayed in front of the selected choice.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// Gets the filter or bool controlling frame display.
    /// </summary>
    public FilterOrBool ShowFrame { get; }

    /// <summary>
    /// Gets the filter or bool controlling suspend-to-background capability.
    /// </summary>
    public FilterOrBool EnableSuspend { get; }

    /// <summary>
    /// Gets the exception type raised on interrupt (Ctrl+C).
    /// </summary>
    public Type InterruptException { get; }

    /// <summary>
    /// Gets the filter or bool controlling interrupt handling.
    /// </summary>
    public FilterOrBool EnableInterrupt { get; }

    /// <summary>
    /// Gets the bottom toolbar content, if any.
    /// </summary>
    public AnyFormattedText? BottomToolbar { get; }

    /// <summary>
    /// Gets the additional key bindings, if any.
    /// </summary>
    public IKeyBindingsBase? KeyBindings { get; }

    /// <summary>
    /// Display the prompt and wait for the user to make a selection.
    /// </summary>
    /// <returns>The value associated with the selected option.</returns>
    /// <exception cref="KeyboardInterrupt">
    /// Thrown when user presses Ctrl+C and <see cref="EnableInterrupt"/> is true
    /// (or the type specified by <see cref="InterruptException"/>).
    /// </exception>
    public T Prompt();

    /// <summary>
    /// Display the prompt asynchronously and wait for the user to make a selection.
    /// </summary>
    /// <returns>A task that completes with the value associated with the selected option.</returns>
    /// <exception cref="KeyboardInterrupt">
    /// Thrown when user presses Ctrl+C and <see cref="EnableInterrupt"/> is true
    /// (or the type specified by <see cref="InterruptException"/>).
    /// </exception>
    public Task<T> PromptAsync();
}
```

## Dialogs Class Extension

```csharp
/// <summary>
/// Factory methods for common dialog types.
/// </summary>
public static partial class Dialogs
{
    /// <summary>
    /// Create and display a choice selection prompt.
    /// </summary>
    /// <typeparam name="T">The type of value returned when an option is selected.</typeparam>
    /// <param name="message">Plain text or formatted text to be shown before the options.</param>
    /// <param name="options">Sequence of (value, label) tuples.</param>
    /// <param name="defaultValue">Default value to pre-select.</param>
    /// <param name="mouseSupport">Enable mouse support.</param>
    /// <param name="style">Style instance for colors.</param>
    /// <param name="symbol">Symbol in front of selected choice.</param>
    /// <param name="bottomToolbar">Text at bottom of screen.</param>
    /// <param name="showFrame">Surround with frame when true.</param>
    /// <param name="enableSuspend">Allow Ctrl+Z suspend on Unix.</param>
    /// <param name="enableInterrupt">Raise exception on Ctrl+C.</param>
    /// <param name="interruptException">Exception type for Ctrl+C.</param>
    /// <param name="keyBindings">Additional key bindings.</param>
    /// <returns>The value associated with the selected option.</returns>
    /// <remarks>
    /// <para>
    /// Example usage:
    /// </para>
    /// <code>
    /// string result = Dialogs.Choice(
    ///     message: "Please select a dish:",
    ///     options: new[]
    ///     {
    ///         ("pizza", (AnyFormattedText)"Pizza with mushrooms"),
    ///         ("salad", (AnyFormattedText)"Salad with tomatoes"),
    ///         ("sushi", (AnyFormattedText)"Sushi"),
    ///     },
    ///     defaultValue: "pizza");
    /// </code>
    /// </remarks>
    public static T Choice<T>(
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
        IKeyBindingsBase? keyBindings = null);

    /// <summary>
    /// Create and display a choice selection prompt asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of value returned when an option is selected.</typeparam>
    /// <param name="message">Plain text or formatted text to be shown before the options.</param>
    /// <param name="options">Sequence of (value, label) tuples.</param>
    /// <param name="defaultValue">Default value to pre-select.</param>
    /// <param name="mouseSupport">Enable mouse support.</param>
    /// <param name="style">Style instance for colors.</param>
    /// <param name="symbol">Symbol in front of selected choice.</param>
    /// <param name="bottomToolbar">Text at bottom of screen.</param>
    /// <param name="showFrame">Surround with frame when true.</param>
    /// <param name="enableSuspend">Allow Ctrl+Z suspend on Unix.</param>
    /// <param name="enableInterrupt">Raise exception on Ctrl+C.</param>
    /// <param name="interruptException">Exception type for Ctrl+C.</param>
    /// <param name="keyBindings">Additional key bindings.</param>
    /// <returns>A task that completes with the value associated with the selected option.</returns>
    public static Task<T> ChoiceAsync<T>(
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
        IKeyBindingsBase? keyBindings = null);
}
```

## Supporting Types

### KeyboardInterrupt Exception

```csharp
/// <summary>
/// Exception raised when the user presses Ctrl+C during a prompt.
/// </summary>
/// <remarks>
/// This exception mirrors Python's KeyboardInterrupt behavior.
/// It is the default exception type for <see cref="ChoiceInput{T}"/> interrupt handling.
/// </remarks>
public class KeyboardInterrupt : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardInterrupt"/> class.
    /// </summary>
    public KeyboardInterrupt();

    /// <summary>
    /// Initializes a new instance with a custom message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public KeyboardInterrupt(string message);

    /// <summary>
    /// Initializes a new instance with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public KeyboardInterrupt(string message, Exception innerException);
}
```

## Key Bindings

The following key bindings are registered by ChoiceInput:

| Key | Filter | Action |
|-----|--------|--------|
| `Enter` | None (eager) | Exit with selected value |
| `Ctrl+C` | `enableInterrupt` | Exit with interrupt exception |
| `<sigint>` | `enableInterrupt` | Exit with interrupt exception |
| `Ctrl+Z` | `enableSuspend & platformSupported` | Suspend to background |

RadioList provides additional bindings:

| Key | Action |
|-----|--------|
| `Up` / `k` | Move selection up (wraps) |
| `Down` / `j` | Move selection down (wraps) |
| `PageUp` | Move selection up one page |
| `PageDown` | Move selection down one page |
| `1-9` | Jump to option 1-9 directly |
| `Space` | Toggle selection (same as Enter for RadioList) |

## Style Classes

| Class | Default Value | Description |
|-------|---------------|-------------|
| `frame.border` | `#884444` | Border color for frame |
| `selected-option` | `bold` | Style for selected option text |
| `input-selection` | (none) | Container style |
| `option` | (none) | Default option style |
| `number` | (none) | Number prefix style |
| `bottom-toolbar` | (none) | Toolbar container style |
| `bottom-toolbar.text` | (none) | Toolbar text style |
