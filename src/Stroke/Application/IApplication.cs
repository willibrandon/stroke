using Stroke.Clipboard;
using Stroke.Core;
using Stroke.CursorShapes;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Output;
using Stroke.Rendering;
using Stroke.Styles;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Application;

/// <summary>
/// Non-generic interface for <see cref="Application{TResult}"/>.
/// Provides type-safe access to all application members without requiring
/// knowledge of the result type parameter.
/// </summary>
/// <remarks>
/// <para>
/// This interface eliminates the need for <c>Unsafe.As</c> casts that were
/// previously required because C# generics are invariant — an
/// <c>Application&lt;string&gt;</c> is NOT assignable to
/// <c>Application&lt;object?&gt;</c>. With <c>IApplication</c>, any
/// <c>Application&lt;T&gt;</c> is naturally assignable to <c>IApplication</c>.
/// </para>
/// <para>
/// Port rationale: Python Prompt Toolkit's <c>Application</c> is non-generic
/// (Python doesn't have this problem). The generic parameter in C# exists
/// only for the <c>Exit(result)</c> / <c>RunAsync()</c> return type. All
/// other members are independent of <c>TResult</c>.
/// </para>
/// </remarks>
public interface IApplication
{
    // ════════════════════════════════════════════════════════════════════════
    // LAYOUT & RENDERING
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>The root layout for this application.</summary>
    Layout.Layout Layout { get; set; }

    /// <summary>The renderer instance.</summary>
    Renderer Renderer { get; }

    /// <summary>Render counter incremented each time the UI is rendered.</summary>
    int RenderCounter { get; }

    /// <summary>The active color depth.</summary>
    ColorDepth ColorDepth { get; }

    /// <summary>Whether to run in full-screen mode.</summary>
    bool FullScreen { get; }

    /// <summary>Whether to erase output when the application finishes.</summary>
    bool EraseWhenDone { get; }

    /// <summary>Cursor shape configuration.</summary>
    ICursorShapeConfig Cursor { get; }

    // ════════════════════════════════════════════════════════════════════════
    // INPUT & OUTPUT
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>The input device for this application.</summary>
    IInput Input { get; }

    /// <summary>The output device for this application.</summary>
    IOutput Output { get; }

    // ════════════════════════════════════════════════════════════════════════
    // KEY PROCESSING & BINDINGS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>The key processor instance.</summary>
    KeyProcessor KeyProcessor { get; }

    /// <summary>Application-level key bindings.</summary>
    IKeyBindingsBase? KeyBindings { get; set; }

    /// <summary>The default key bindings.</summary>
    IKeyBindingsBase DefaultBindings { get; }

    /// <summary>The page navigation key bindings.</summary>
    IKeyBindingsBase PageNavigationBindings { get; }

    // ════════════════════════════════════════════════════════════════════════
    // EDITING STATE
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Vi editing mode state.</summary>
    ViState ViState { get; }

    /// <summary>Emacs editing mode state.</summary>
    EmacsState EmacsState { get; }

    /// <summary>Current editing mode (Vi or Emacs).</summary>
    EditingMode EditingMode { get; set; }

    /// <summary>Whether quoted insert mode is active.</summary>
    bool QuotedInsert { get; set; }

    /// <summary>Clipboard implementation.</summary>
    IClipboard Clipboard { get; set; }

    // ════════════════════════════════════════════════════════════════════════
    // BUFFER & SEARCH
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>The currently focused Buffer.</summary>
    Buffer CurrentBuffer { get; }

    /// <summary>The SearchState for the currently focused BufferControl.</summary>
    SearchState CurrentSearchState { get; }

    // ════════════════════════════════════════════════════════════════════════
    // STYLE
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>User-provided custom style.</summary>
    IStyle? Style { get; set; }

    /// <summary>Style transformation applied to merged style output.</summary>
    IStyleTransformation StyleTransformation { get; set; }

    /// <summary>Style string applied on exit.</summary>
    string ExitStyle { get; set; }

    // ════════════════════════════════════════════════════════════════════════
    // FILTERS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Filter controlling mouse support.</summary>
    IFilter MouseSupport { get; }

    /// <summary>Filter controlling paste mode.</summary>
    IFilter PasteMode { get; }

    /// <summary>Filter controlling reverse Vi search direction.</summary>
    IFilter ReverseViSearchDirection { get; }

    /// <summary>Filter controlling page navigation bindings.</summary>
    IFilter EnablePageNavigationBindings { get; }

    // ════════════════════════════════════════════════════════════════════════
    // APPLICATION STATE
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>True when the application is currently active/running.</summary>
    bool IsRunning { get; }

    /// <summary>True when the application result has been set.</summary>
    bool IsDone { get; }

    /// <summary>True when a redraw has been scheduled but not yet executed.</summary>
    bool Invalidated { get; }

    /// <summary>Auto-invalidation interval in seconds.</summary>
    double? RefreshInterval { get; set; }

    /// <summary>Key sequence timeout in seconds. Null disables the timeout.</summary>
    double? TimeoutLen { get; set; }

    /// <summary>List of callables executed before each run.</summary>
    List<Action> PreRunCallables { get; }

    // ════════════════════════════════════════════════════════════════════════
    // METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Exit the application with an optional result or exception.
    /// </summary>
    /// <param name="result">Result value (boxed for value types).</param>
    /// <param name="exception">Exception to throw from RunAsync.</param>
    /// <param name="style">Style to apply to content on exit.</param>
    void Exit(object? result = null, Exception? exception = null, string style = "");

    /// <summary>Trigger a UI redraw.</summary>
    void Invalidate();

    /// <summary>Start an incremental search.</summary>
    void StartSearch(SearchDirection direction);

    /// <summary>Suspend the process to background (Unix only).</summary>
    void SuspendToBackground(bool suspendGroup = true);

    /// <summary>Run a system command while the application is suspended.</summary>
    Task RunSystemCommandAsync(
        string command,
        bool waitForEnter = true,
        AnyFormattedText displayBeforeText = default,
        string waitText = "Press ENTER to continue...");

    /// <summary>Print formatted text to the output.</summary>
    void PrintText(AnyFormattedText text, IStyle? style = null);

    /// <summary>Reset the application to a clean state.</summary>
    void Reset();

    /// <summary>Create a tracked background task.</summary>
    Task CreateBackgroundTask(Func<CancellationToken, Task> taskFactory);

    // ════════════════════════════════════════════════════════════════════════
    // INTERNAL: RunInTerminal support
    // ════════════════════════════════════════════════════════════════════════
    // These members use default implementations so that DummyApplication
    // (and any other implementors) don't need to provide them.
    // Only accessed by RunInTerminal within the same assembly.

    /// <summary>Whether the application is currently running in a system command terminal.</summary>
    internal bool RunningInTerminal { get => false; set { } }

    /// <summary>Future for awaiting RunInTerminal completion.</summary>
    internal TaskCompletionSource<object?>? RunningInTerminalFuture { get => null; set { } }
}
