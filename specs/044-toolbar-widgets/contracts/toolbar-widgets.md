# API Contracts: Toolbar Widgets

**Feature**: 044-toolbar-widgets
**Date**: 2026-02-01
**Namespace**: `Stroke.Widgets.Toolbars`

## FormattedTextToolbar

```csharp
namespace Stroke.Widgets.Toolbars;

/// <summary>
/// A toolbar displaying formatted text in a single-line window.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>FormattedTextToolbar</c> from <c>widgets/toolbars.py</c>.
/// </remarks>
public class FormattedTextToolbar : Window
{
    /// <summary>
    /// Initializes a new FormattedTextToolbar.
    /// </summary>
    /// <param name="text">The formatted text to display. Supports string, FormattedText, or Func.</param>
    /// <param name="style">Style string applied to the Window (not the inner control).</param>
    /// <remarks>
    /// Calls base Window constructor with: FormattedTextControl(() => FormattedTextUtils.ToFormattedText(text)),
    /// style, dontExtendHeight: true, height: new Dimension(min: 1).
    /// Python's **kw forwarding is omitted (C# does not support kwargs). See FR-001 deviation note.
    /// </remarks>
    public FormattedTextToolbar(AnyFormattedText text, string style = "");
}
```

## SystemToolbar

```csharp
namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Toolbar for entering and executing system shell commands.
/// </summary>
/// <remarks>
/// <para>
/// Provides a command prompt that appears when focused, with Emacs and Vi
/// mode-specific key bindings for cancel, execute, and global focus shortcuts.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>SystemToolbar</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
public class SystemToolbar : IMagicContainer
{
    /// <summary>
    /// Initializes a new SystemToolbar.
    /// </summary>
    /// <param name="prompt">The prompt text shown before user input. Default: "Shell command: ".</param>
    /// <param name="enableGlobalBindings">Whether to register global key bindings (M-! for Emacs, ! for Vi). Default: true.</param>
    public SystemToolbar(
        AnyFormattedText prompt = default,  // defaults to "Shell command: "
        FilterOrBool enableGlobalBindings = default);  // defaults to true

    /// <summary>Gets the prompt text.</summary>
    public AnyFormattedText Prompt { get; }

    /// <summary>Gets the filter controlling global binding registration.</summary>
    public IFilter EnableGlobalBindings { get; }

    /// <summary>Gets the system command buffer.</summary>
    public Buffer SystemBuffer { get; }

    /// <summary>Gets the buffer control displaying the system buffer.</summary>
    public BufferControl BufferControl { get; }

    /// <summary>Gets the window containing the buffer control (height=1, style="class:system-toolbar").</summary>
    public Window Window { get; }

    /// <summary>Gets the conditional container (visible when system buffer is focused).</summary>
    public ConditionalContainer Container { get; }

    // Private: IKeyBindingsBase _bindings — three-group merged key bindings (not exposed as property)
    // Private: GetDisplayBeforeText() — builds formatted text for RunSystemCommandAsync displayBeforeText

    /// <inheritdoc/>
    public IContainer PtContainer();
}
```

## ArgToolbar

```csharp
namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Toolbar displaying the current numeric prefix argument (e.g., "Repeat: 5").
/// </summary>
/// <remarks>
/// <para>
/// Visible only when a numeric argument is active in the key processor.
/// Displays "-1" when the arg value is "-".
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ArgToolbar</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
public class ArgToolbar : IMagicContainer
{
    /// <summary>
    /// Initializes a new ArgToolbar.
    /// </summary>
    public ArgToolbar();

    /// <summary>Gets the window displaying the arg text (height=1, FormattedTextControl with styled "Repeat: {arg}").</summary>
    public Window Window { get; }

    /// <summary>Gets the conditional container (visible when arg is active).</summary>
    public ConditionalContainer Container { get; }

    /// <inheritdoc/>
    public IContainer PtContainer();
}
```

## SearchToolbar

```csharp
namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Toolbar for incremental search input with direction-aware prompts.
/// </summary>
/// <remarks>
/// <para>
/// Displays "I-search: " / "I-search backward: " in Emacs mode, or "/" / "?" in Vi mode.
/// Visible only when the search control is registered in the layout's search links.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>SearchToolbar</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
public class SearchToolbar : IMagicContainer
{
    /// <summary>
    /// Initializes a new SearchToolbar.
    /// </summary>
    /// <param name="searchBuffer">Buffer for search input. Creates new if null.</param>
    /// <param name="viMode">If true, use "/" and "?" prompts instead of "I-search".</param>
    /// <param name="textIfNotSearching">Text to show when not actively searching.</param>
    /// <param name="forwardSearchPrompt">Prompt for forward search. Default: "I-search: ".</param>
    /// <param name="backwardSearchPrompt">Prompt for backward search. Default: "I-search backward: ".</param>
    /// <param name="ignoreCase">Filter controlling case-insensitive search. Default: false.</param>
    public SearchToolbar(
        Buffer? searchBuffer = null,
        bool viMode = false,
        AnyFormattedText textIfNotSearching = default,  // defaults to ""
        AnyFormattedText forwardSearchPrompt = default,  // defaults to "I-search: "
        AnyFormattedText backwardSearchPrompt = default,  // defaults to "I-search backward: "
        FilterOrBool ignoreCase = default);  // defaults to false

    /// <summary>Gets the search buffer.</summary>
    public Buffer SearchBuffer { get; }

    /// <summary>Gets the search buffer control.</summary>
    public SearchBufferControl Control { get; }

    /// <summary>Gets the conditional container (visible when searching).</summary>
    public ConditionalContainer Container { get; }

    /// <inheritdoc/>
    public IContainer PtContainer();
}
```

## CompletionsToolbarControl (internal)

```csharp
namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Internal UIControl that renders completions horizontally with pagination arrows.
/// </summary>
/// <remarks>
/// <para>
/// Displays completion items in a single line with "&lt;" and "&gt;" arrow indicators
/// when completions extend beyond the visible area. The currently selected completion
/// is highlighted with the current-completion style.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>_CompletionsToolbarControl</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
internal class CompletionsToolbarControl : IUIControl
{
    /// <summary>Gets whether the control is focusable (always false).</summary>
    public bool IsFocusable { get; }  // false

    /// <summary>
    /// Creates the UIContent for the given dimensions.
    /// </summary>
    public UIContent CreateContent(int width, int height);
}
```

## CompletionsToolbar

```csharp
namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Toolbar displaying completions in a horizontal row with pagination.
/// </summary>
/// <remarks>
/// <para>
/// Visible only when completions are active on the current buffer.
/// Wraps <see cref="CompletionsToolbarControl"/> in a conditional container.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>CompletionsToolbar</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
public class CompletionsToolbar : IMagicContainer
{
    /// <summary>
    /// Initializes a new CompletionsToolbar.
    /// </summary>
    public CompletionsToolbar();

    /// <summary>Gets the conditional container (visible when completions active).</summary>
    public ConditionalContainer Container { get; }

    /// <inheritdoc/>
    public IContainer PtContainer();
}
```

## ValidationToolbar

```csharp
namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Toolbar displaying the current buffer's validation error message.
/// </summary>
/// <remarks>
/// <para>
/// Visible only when the current buffer has a validation error.
/// Optionally includes line and column position in the error display.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ValidationToolbar</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
public class ValidationToolbar : IMagicContainer
{
    /// <summary>
    /// Initializes a new ValidationToolbar.
    /// </summary>
    /// <param name="showPosition">Whether to include line/column in error display. Default: false.</param>
    public ValidationToolbar(bool showPosition = false);

    /// <summary>Gets the formatted text control displaying the error.</summary>
    public FormattedTextControl Control { get; }

    /// <summary>Gets the conditional container (visible when validation error exists).</summary>
    public ConditionalContainer Container { get; }

    /// <inheritdoc/>
    public IContainer PtContainer();
}
```

## Style Classes Reference

| Style Class | Used By | Description |
|-------------|---------|-------------|
| `class:system-toolbar` | SystemToolbar | Toolbar background and BeforeInput prompt |
| `class:system-toolbar.text` | SystemToolbar | Input text in system toolbar |
| `class:arg-toolbar` | ArgToolbar | "Repeat: " label |
| `class:arg-toolbar.text` | ArgToolbar | Arg value text |
| `class:search-toolbar` | SearchToolbar | Toolbar window background |
| `class:search-toolbar.prompt` | SearchToolbar | BeforeInput search prompt |
| `class:search-toolbar.text` | SearchToolbar | Search input text |
| `class:completion-toolbar` | CompletionsToolbar | Toolbar window background |
| `class:completion-toolbar.completion` | CompletionsToolbarControl | Normal completion item |
| `class:completion-toolbar.completion.current` | CompletionsToolbarControl | Selected completion item |
| `class:completion-toolbar.arrow` | CompletionsToolbarControl | Left/right pagination arrows |
| `class:validation-toolbar` | ValidationToolbar | Validation error text (applied to text fragments, NOT the Window) |
